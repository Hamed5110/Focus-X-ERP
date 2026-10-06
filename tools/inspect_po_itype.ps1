$cs = "Server=localhost;Database=Focus80G0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 30
$cmd.CommandText = @"
SELECT ISNULL(d.iType,0) iType, COUNT(*) Cnt
FROM dbo.tCore_Header_0 h
INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId = h.iHeaderId
INNER JOIN dbo.tCore_Indta_0 i ON i.iBodyId = d.iBodyId
WHERE h.iVoucherType IN (2562, 2563)
GROUP BY ISNULL(d.iType,0)
ORDER BY Cnt DESC
"@
$r = $cmd.ExecuteReader()
while ($r.Read()) { Write-Output ("iType={0} Cnt={1}" -f $r["iType"], $r["Cnt"]) }
$r.Close(); $conn.Close()
