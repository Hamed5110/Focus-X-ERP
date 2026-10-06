$cs = "Server=localhost;Database=Focus8080;User Id=sa;Password=$env:FOCUS_SQL_PASSWORD;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$cmd = $conn.CreateCommand()
$cmd.CommandText = @"
SELECT r.iReportId, r.sReportName, r.iReportType, r.iSourceType
FROM dbo.cCore_Reports_0 r
WHERE r.sReportName LIKE N'%Glass%ALL%' OR r.sReportName LIKE N'%Glass Status ALL%'
   OR r.sReportName LIKE N'%ALL new%'
ORDER BY r.iReportId DESC
"@
$r = $cmd.ExecuteReader()
while ($r.Read()) { Write-Output ("{0} | {1} | type={2} src={3}" -f $r[0], $r[1], $r[2], $r[3]) }
$r.Close()

Write-Output '--- params ---'
$cmd.CommandText = @"
SELECT p.iReportId, r.sReportName, p.sFieldName, p.sFieldVariable, p.iControlType, p.iFieldType, p.sDefault
FROM dbo.cCore_ReportParameter_0 p
JOIN dbo.cCore_Reports_0 r ON r.iReportId = p.iReportId
WHERE r.sReportName LIKE N'%Glass%'
"@
$r = $cmd.ExecuteReader()
$n = 0
while ($r.Read()) { $n++; Write-Output ("{0} | {1} | {2} | {3} | ctrl={4} ft={5}" -f $r[0], $r[1], $r[2], $r[3], $r[4], $r[5]) }
$r.Close()
if ($n -eq 0) { Write-Output '(none)' }

Write-Output '--- sql snippet ---'
$cmd.CommandText = @"
SELECT r.iReportId, r.sReportName, LEFT(q.sSqlQuery, 400)
FROM dbo.cCore_RDQuery_0 q
JOIN dbo.cCore_Reports_0 r ON r.iReportId = q.iReportId
WHERE r.sReportName LIKE N'%Glass%'
"@
$r = $cmd.ExecuteReader()
while ($r.Read()) { Write-Output ("{0} {1}" -f $r[0], $r[1]); Write-Output ([string]$r[2]) }
$r.Close()
$conn.Close()
