$cs = "Server=localhost;Database=Focus80E0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$base = [System.IO.File]::ReadAllText("C:\Users\Hamed Ali Khan\Focus-X-ERP\reports\sql\Monthly Sales Commission.sql")
# strip the leading comment block? keep it, SQL Server allows it
$q = @"
SELECT Company,
       COUNT(*) Cnt,
       SUM(CASE WHEN Qualified=1 THEN 1 ELSE 0 END) QualCnt,
       CAST(SUM([Contract Amount]) AS decimal(18,2)) AllContract,
       CAST(SUM(CASE WHEN Qualified=1 THEN [Contract Amount] ELSE 0 END) AS decimal(18,2)) QualAmt,
       CAST(SUM([Advance Amount]) AS decimal(18,2)) AllAdv,
       MAX([Team Qualified Total]) TeamQual,
       MAX([Team Rate %]) Rate,
       MAX([Team Commission]) Comm,
       MAX([Team Members]) Members,
       MAX([Share Each]) Share
FROM (
$base
) r
WHERE r.Company = N'Atlas Aluminum'
  AND (r.iDate/65536)=2026 AND ((r.iDate/256)%256)=8
GROUP BY Company
"@
$cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 180; $cmd.CommandText = $q
try {
    $r = $cmd.ExecuteReader()
    $n = $r.FieldCount
    $hdr = @(); for ($i=0;$i -lt $n;$i++) { $hdr += $r.GetName($i) }
    Write-Output ($hdr -join " | ")
    while ($r.Read()) {
        $parts = @()
        for ($i=0;$i -lt $n;$i++) {
            if ($r.IsDBNull($i)) { $parts += "" } else { $parts += [string]$r.GetValue($i) }
        }
        Write-Output ($parts -join " | ")
    }
    $r.Close()
} catch {
    Write-Output ("SQL ERROR: " + $_.Exception.Message)
}
$conn.Close()
