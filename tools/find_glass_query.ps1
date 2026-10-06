$cs = "Server=localhost;Database=Focus8080;User Id=sa;Password=$env:FOCUS_SQL_PASSWORD;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$cmd = $conn.CreateCommand()
$cmd.CommandText = @"
SELECT r.iReportId, r.sReportName, r.iReportType, r.iSourceType, r.iModifiedDate
FROM dbo.cCore_Reports_0 r
WHERE r.sReportName LIKE N'%Glass%' OR r.sReportName LIKE N'%Status All%'
ORDER BY r.iReportId DESC
"@
$r = $cmd.ExecuteReader()
while ($r.Read()) { Write-Output ("{0} | {1} | type={2} src={3}" -f $r[0], $r[1], $r[2], $r[3]) }
$r.Close()

Write-Output ""
Write-Output "========== params on glass-like =========="
$cmd.CommandText = @"
SELECT p.iReportId, r.sReportName, p.sFieldName, p.sFieldVariable, p.iControlType, p.sDefault
FROM dbo.cCore_ReportParameter_0 p
JOIN dbo.cCore_Reports_0 r ON r.iReportId = p.iReportId
WHERE r.sReportName LIKE N'%Glass%' OR p.sFieldVariable IN (N'@VendorAC', N'@JobOrder', N'@DeliveryStatus')
ORDER BY p.iReportId
"@
$r = $cmd.ExecuteReader()
while ($r.Read()) { Write-Output ("{0} | {1} | {2} | {3} | ctrl={4} def={5}" -f $r[0], $r[1], $r[2], $r[3], $r[4], $r[5]) }
$r.Close()
$conn.Close()
