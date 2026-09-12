$cs = "Server=localhost;Database=Focus80E0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$base = [System.IO.File]::ReadAllText("C:\Users\Hamed Ali Khan\Focus-X-ERP\reports\sql\Monthly Sales Commission.sql")
# Exact Focus append seen on this project: AND iDate >= S AND iDate <= E OR iDate = 0
$sql = $base.Trim() + "`r`n  AND iDate >= (2026*65536 + 8*256 + 1) AND iDate <= (2026*65536 + 8*256 + 31) OR iDate = 0"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 180
$cmd.CommandText = @"
SELECT COUNT(*) Cnt, COUNT(DISTINCT [Month Year]) Months, MIN([Month Year]) MinM, MAX([Month Year]) MaxM
FROM (
$sql
) r
"@
try {
    $r = $cmd.ExecuteReader()
    while ($r.Read()) {
        Write-Output ("OR-append Aug: cnt={0} months={1} {2} .. {3}" -f $r.GetValue(0), $r.GetValue(1), $r.GetValue(2), $r.GetValue(3))
    }
    $r.Close()
} catch {
    Write-Output ("SQL ERROR: " + $_.Exception.Message)
}
$conn.Close()
