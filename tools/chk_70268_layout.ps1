$cs = "Server=localhost;Database=Focus80E0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$cmd = $conn.CreateCommand()
$cmd.CommandText = @"
SELECT c.iFieldId, c.iType, c.sAliasName, c.iDecimalInColumn, c.iMiscOption
FROM dbo.cCore_ReportLayouts_0 l
JOIN dbo.cCore_ReportColumns_0 c ON c.iLayoutId = l.iLayoutId
WHERE l.iReportId = 70268
ORDER BY c.iFieldId
"@
$r = $cmd.ExecuteReader()
while ($r.Read()) {
    Write-Output ("{0} type={1} alias={2} dec={3} misc={4}" -f $r[0], $r[1], $r[2], $r[3], $r[4])
}
$r.Close()
$conn.Close()
