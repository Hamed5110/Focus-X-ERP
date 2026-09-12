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
        $dept = @{}
        $n = 0
        while ($r.Read()) {
            $n++
            $d = [string]$r["Department"]
            $my = [string]$r["Month Year"]
            $key = "$d | $my"
            if (-not $dept.ContainsKey($key)) {
                $dept[$key] = @{ Coll = 0.0; Elig = 0.0; Rate = [double]$r["Team Rate %"]; TeamElig = [double]$r["Team Eligible Collection"] }
            }
            $dept[$key].Coll += [double]$r["Collection Amount"]
            $dept[$key].Elig += [double]$r["Eligible Collection"]
        }
        $r.Close()
        Write-Output ("rows={0}" -f $n)
        $dept.GetEnumerator() | Sort-Object Name | ForEach-Object {
            Write-Output ("  {0}  coll={1:N2}  elig={2:N2}  teamElig={3:N2}  rate={4}" -f $_.Name, $_.Value.Coll, $_.Value.Elig, $_.Value.TeamElig, $_.Value.Rate)
        }
        if ($n -eq 0) { Write-Output "  (no rows)" }
    } catch {
        Write-Output ("ERROR: " + $_.Exception.Message)
    }
}

Run-Case "Aug 2026 packed" @"
DECLARE @iStartDate INT = dbo.DateToInt(CONVERT(datetime,'20260801',112))
DECLARE @iEndDate INT = dbo.DateToInt(CONVERT(datetime,'20260831',112))
"@

Run-Case "Sep 2026 packed" @"
DECLARE @iStartDate INT = dbo.DateToInt(CONVERT(datetime,'20260901',112))
DECLARE @iEndDate INT = dbo.DateToInt(CONVERT(datetime,'20260930',112))
"@

Run-Case "Jul 2026 packed" @"
DECLARE @iStartDate INT = dbo.DateToInt(CONVERT(datetime,'20260701',112))
DECLARE @iEndDate INT = dbo.DateToInt(CONVERT(datetime,'20260731',112))
"@

$conn.Close()
