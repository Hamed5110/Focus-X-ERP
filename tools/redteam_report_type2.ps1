$cs = "Server=localhost;Database=Focus80E0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 60
$cmd.CommandText = @"
SELECT r.iReportType,
       MIN(r.sReportName) AS Sample1,
       MAX(r.sReportName) AS Sample2,
       COUNT(*) Cnt
FROM dbo.cCore_Reports_0 r
GROUP BY r.iReportType
ORDER BY r.iReportType;

SELECT r.iReportId, r.sReportName, r.iReportType, r.iSourceType
FROM dbo.cCore_Reports_0 r
WHERE r.iReportId = 70267 OR r.sReportName LIKE N'%Commission Atlas%' OR r.sReportName LIKE N'%Monthly Commission%';

SELECT r.iReportId, r.sReportName, r.iReportType, r.iSourceType
FROM dbo.cCore_Reports_0 r
INNER JOIN dbo.cCore_RDQuery_0 q ON q.iReportId = r.iReportId
WHERE r.iReportType = 1 AND r.iSourceType = 1
ORDER BY r.iReportId;
"@
$r = $cmd.ExecuteReader()
Write-Output "=== TYPE SAMPLES ==="
while ($r.Read()) { Write-Output ("type={0} n={1} e.g. {2} | {3}" -f $r["iReportType"], $r["Cnt"], $r["Sample1"], $r["Sample2"]) }
$r.NextResult() | Out-Null
Write-Output "=== ATLAS COMMISSION REPORTS ==="
while ($r.Read()) { Write-Output ("{0} [{1}] type={2} source={3}" -f $r["iReportId"], $r["sReportName"], $r["iReportType"], $r["iSourceType"]) }
$r.NextResult() | Out-Null
Write-Output "=== ALL WORKING CUSTOM SQL (type=1 source=1) ==="
while ($r.Read()) { Write-Output ("{0} {1}" -f $r["iReportId"], $r["sReportName"]) }
$r.Close(); $conn.Close()
