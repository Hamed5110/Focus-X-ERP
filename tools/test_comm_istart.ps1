$cs = "Server=localhost;Database=Focus80E0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$body = Get-Content -Raw "C:\Users\Hamed Ali Khan\Focus-X-ERP\reports\sql\Monthly Sales Commission.sql"

function Run-Case($title, $prefix) {
    Write-Output ""
    Write-Output "========== $title =========="
    $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 180
    $cmd.CommandText = $prefix + "`r`n" + $body
    try {
        $r = $cmd.ExecuteReader()
        $months = @{}
        $n = 0
        $coll = 0.0
        $elig = 0.0
        while ($r.Read()) {
            $n++
            $my = [string]$r["Month Year"]
            $months[$my] = 1 + $(if ($months.ContainsKey($my)) { $months[$my] } else { 0 })
            $coll += [double]$r["Collection Amount"]
            $elig += [double]$r["Eligible Collection"]
        }
        $r.Close()
        Write-Output ("rows={0} collection={1:N2} eligible={2:N2}" -f $n, $coll, $elig)
        $months.GetEnumerator() | Sort-Object Name | ForEach-Object { Write-Output ("  {0} = {1}" -f $_.Name, $_.Value) }
        if ($n -eq 0) { Write-Output "  (no rows)" }
    } catch {
        Write-Output ("ERROR: " + $_.Exception.Message)
    }
}

$pSep = "dbo.DateToInt(CONVERT(datetime,'20260901',112))"
$pEnd = "dbo.DateToInt(CONVERT(datetime,'20260930',112))"

Run-Case "packed DateToInt Sep 2026" @"
DECLARE @iStartDate INT = $pSep
DECLARE @iEndDate INT = $pEnd
"@

Run-Case "YYYYMMDD 20260901-20260930" @"
DECLARE @iStartDate INT = 20260901
DECLARE @iEndDate INT = 20260930
"@

Run-Case "datetime Sep 2026" @"
DECLARE @iStartDate datetime = CONVERT(datetime,'20260901',112)
DECLARE @iEndDate datetime = CONVERT(datetime,'20260930',112)
"@

Run-Case "YYYYMMDD 20240101-20241231 (70223 comment style)" @"
DECLARE @iStartDate INT = 20240101
DECLARE @iEndDate INT = 20241231
"@

Write-Output ""
Write-Output "========== bound values =========="
$cmd = $conn.CreateCommand()
$cmd.CommandText = @"
SELECT
  dbo.DateToInt(CONVERT(datetime,'20260901',112)) AS PackedStart,
  dbo.DateToInt(CONVERT(datetime,'20260930',112)) AS PackedEnd
"@
$r = $cmd.ExecuteReader()
$r.Read() | Out-Null
Write-Output ("PackedStart={0} PackedEnd={1}" -f $r.GetValue(0), $r.GetValue(1))
$r.Close()
$conn.Close()
