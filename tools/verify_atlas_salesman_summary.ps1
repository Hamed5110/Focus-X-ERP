$cs = "Server=localhost;Database=Focus80E0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$body = Get-Content -Raw "C:\Users\Hamed Ali Khan\Focus-X-ERP\reports\sql\Monthly Sales Commission - Atlas Aluminum.sql"
function Run-Month($label, $s, $e) {
    $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 180
    $cmd.CommandText = @"
DECLARE @iStartDate INT = dbo.DateToInt(CONVERT(datetime,'$s',112))
DECLARE @iEndDate INT = dbo.DateToInt(CONVERT(datetime,'$e',112))
$body
"@
    $r = $cmd.ExecuteReader()
    $cols = @(); for ($i=0; $i -lt $r.FieldCount; $i++) { $cols += $r.GetName($i) }
    Write-Output ""
    Write-Output "=== $label COLS: $($cols -join ' | ') ==="
    $rows = @()
    while ($r.Read()) {
        $o = [ordered]@{}; foreach ($c in $cols) { $o[$c] = $r[$c] }
        $rows += [pscustomobject]$o
        Write-Output ("  {0,-32} contract={1,12:N2} coll={2,10:N2} elig={3,12:N2} notElig={4,12:N2} overall={5,12:N2} ratio={6,4} comm={7,10:N2}" -f `
            $r["Salesman"], $r["Total Contract Amount"], $r["Total Collection"], $r["Eligible Amount"], $r["Not Eligible Amount"], $r["Overall Sales"], $r["Commission Ratio"], $r["Total Commission Amount"])
    }
    $r.Close()
    if ($rows.Count -gt 0) {
        Write-Output ("  TOTAL contract={0:N2} coll={1:N2} elig={2:N2} comm={3:N2} rows={4}" -f `
            ($rows | Measure-Object "Total Contract Amount" -Sum).Sum,
            ($rows | Measure-Object "Total Collection" -Sum).Sum,
            ($rows | Measure-Object "Eligible Amount" -Sum).Sum,
            ($rows | Measure-Object "Total Commission Amount" -Sum).Sum,
            $rows.Count)
    }
}
Run-Month "Aug 2026" "20260801" "20260831"
Run-Month "Jul 2026" "20260701" "20260731"
$conn.Close()
