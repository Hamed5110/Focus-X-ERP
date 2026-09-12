$cs = "Server=localhost;Database=Focus80E0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$base = [System.IO.File]::ReadAllText("C:\Users\Hamed Ali Khan\Focus-X-ERP\reports\sql\Monthly Sales Commission.sql")
$whereHits = [regex]::Matches($base, '(?i)\bwhere\b')
Write-Output ("WHERE count = {0}" -f $whereHits.Count)
foreach ($m in $whereHits) {
    $line = ($base.Substring(0, $m.Index) -split "`n").Count
    Write-Output ("  line {0}" -f $line)
}
$sql = $base.Trim() + "`r`n  AND iDate >= CONVERT(datetime,'2026-08-01') AND iDate <= CONVERT(datetime,'2026-08-31') OR iDate = 0"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 180
$cmd.CommandText = @"
SELECT Department, COUNT(*) Rows, COUNT(DISTINCT [Month Year]) Months, MIN([Month Year]) MinM, MAX([Month Year]) MaxM,
       CAST(SUM([Eligible Collection]) AS decimal(18,2)) Elig
FROM (
$sql
) r
GROUP BY Department
ORDER BY Department
"@
try {
    $r = $cmd.ExecuteReader()
    $n = $r.FieldCount
    $hdr = @(); for ($i=0; $i -lt $n; $i++) { $hdr += $r.GetName($i) }
    Write-Output ($hdr -join " | ")
    while ($r.Read()) {
        $parts = @()
        for ($i=0; $i -lt $n; $i++) {
            if ($r.IsDBNull($i)) { $parts += "" } else { $parts += [string]$r.GetValue($i) }
        }
        Write-Output ($parts -join " | ")
    }
    $r.Close()
} catch {
    Write-Output ("SQL ERROR: " + $_.Exception.Message)
}
$conn.Close()
