$cs = "Server=localhost;Database=Focus80G0;User Id=sa;Password=$env:FOCUS_SQL_PASSWORD;Encrypt=False;TrustServerCertificate=True;"
$q = [System.IO.File]::ReadAllText("C:\Users\Hamed Ali Khan\Focus-X-ERP\reports\sql\Project Tracking III Report Atlas Ledger.sql")
$start = 2021*65536 + 256 + 1
$end = [int](Get-Date).Year*65536 + [int](Get-Date).Month*256 + [int](Get-Date).Day

# Exact RDBackend.replace_inputvariables behaviour (FocusX Focus.RD.BL.dll):
#   @iStartDate / @iEndDate -> packed ints
#   parameter value 0       -> "'' OR 1=1"
#   parameter value > 0     -> the master id
function FocusReplace([string]$sql, [string]$custToken) {
    $s = $sql.Replace("@iStartDate", "$start").Replace("@iEndDate", "$end")
    return $s.Replace("@CustomerName", $custToken)
}

$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()

function Run([string]$title, [string]$sql) {
    $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 180
    $cmd.CommandText = @"
SELECT COUNT(*) Cnt,
       SUM(CASE WHEN Code = N'AC-847' THEN [Total Contract Amount] ELSE 0 END) C847,
       SUM(CASE WHEN Code = N'AC-847' THEN [Adv. Rct Amount] ELSE 0 END) A847,
       SUM(CASE WHEN Code = N'AC-847' THEN [Balance Amount] ELSE 0 END) B847
FROM (
$sql
) r
"@
    $sw = [Diagnostics.Stopwatch]::StartNew()
    try {
        $r = $cmd.ExecuteReader()
        [void]$r.Read()
        Write-Output ("PASS  $title rows=$($r[0]) AC-847 C=$($r[1]) Adv=$($r[2]) Bal=$($r[3]) ms=$($sw.ElapsedMilliseconds)")
        $r.Close()
    } catch {
        Write-Output ("FAIL  $title :: " + ($_.Exception.Message -replace "`r|`n"," "))
    }
}

Write-Output "===== old CASE line under Focus empty-picker replace ====="
$old = $q.Replace("AND (m.iMasterId = @CustomerName)", "AND m.iMasterId = CASE @CustomerName WHEN 0 THEN m.iMasterId ELSE @CustomerName END")
Run "OLD empty picker" (FocusReplace $old "'' OR 1=1")

Write-Output ""
Write-Output "===== new line ====="
Run "NEW empty picker ('' OR 1=1)" (FocusReplace $q "'' OR 1=1")
Run "NEW AC-847 selected (5247)" (FocusReplace $q "5247")

Write-Output ""
Write-Output "===== 70268 report filters / params on Focus80G0 ====="
$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT (SELECT COUNT(*) FROM dbo.cCore_ReportFilter_0 f JOIN dbo.cCore_ReportLayouts_0 l ON 1=0) Dummy"
$cmd.CommandText = "SELECT sFieldName, sFieldVariable, iControlType, iFieldType, iSelectionMode, iFieldId, bGroup FROM dbo.cCore_ReportParameter_0 WHERE iReportId = 70268"
$r = $cmd.ExecuteReader()
while ($r.Read()) { Write-Output ("param $($r[0]) var=$($r[1]) ctl=$($r[2]) type=$($r[3]) sel=$($r[4]) field=$($r[5]) group=$($r[6])") }
$r.Close()
$conn.Close()
