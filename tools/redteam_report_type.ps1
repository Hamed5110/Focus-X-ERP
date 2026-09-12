$cs = "Server=localhost;Database=Focus80E0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
function Dump($sql, $title) {
    Write-Output ""
    Write-Output "=== $title ==="
    $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 120; $cmd.CommandText = $sql
    $r = $cmd.ExecuteReader()
    $hdr = @(); for ($i=0;$i -lt $r.FieldCount;$i++) { $hdr += $r.GetName($i) }
    Write-Output ("COLS: " + ($hdr -join " | "))
    $c = 0
    while ($r.Read()) {
        $parts = @(); for ($i=0;$i -lt $r.FieldCount;$i++) {
            if ($r.IsDBNull($i)) { $parts += "" } else { $parts += [string]$r.GetValue($i) }
        }
        Write-Output ($parts -join " | ")
        $c++; if ($c -ge 40) { break }
    }
    $r.Close(); Write-Output "-- $c --"
}

Dump @"
SELECT iReportType, COUNT(*) Cnt
FROM dbo.cCore_Reports_0
GROUP BY iReportType
ORDER BY iReportType
"@ "iReportType counts"

Dump @"
SELECT iSourceType, COUNT(*) Cnt
FROM dbo.cCore_Reports_0
GROUP BY iSourceType
ORDER BY iSourceType
"@ "iSourceType counts"

Dump @"
SELECT r.iReportType, r.iSourceType,
       CASE WHEN q.iReportId IS NULL THEN 0 ELSE 1 END AS HasSql,
       COUNT(*) Cnt
FROM dbo.cCore_Reports_0 r
LEFT JOIN dbo.cCore_RDQuery_0 q ON q.iReportId = r.iReportId
GROUP BY r.iReportType, r.iSourceType, CASE WHEN q.iReportId IS NULL THEN 0 ELSE 1 END
ORDER BY r.iReportType, r.iSourceType
"@ "type x source x hasSQL"

Dump @"
SELECT TOP 15 r.iReportId, r.sReportName, r.iReportType, r.iSourceType,
       CASE WHEN q.iReportId IS NULL THEN 0 ELSE 1 END HasSql,
       CASE WHEN t.iReportId IS NULL THEN 0 ELSE 1 END HasTS
FROM dbo.cCore_Reports_0 r
LEFT JOIN dbo.cCore_RDQuery_0 q ON q.iReportId = r.iReportId
LEFT JOIN (
    SELECT DISTINCT iReportId FROM dbo.cCore_ReportTransactionSet_0
) t ON t.iReportId = r.iReportId
WHERE r.iReportId IN (70198,70223,70256,70259,70266,70267,70028,70064,70220,70219)
   OR r.sReportName LIKE N'%Commission%'
ORDER BY r.iReportId
"@ "commission and known working reports"

Dump @"
SELECT TABLE_NAME, COLUMN_NAME
FROM INFORMATION_SCHEMA.COLUMNS
WHERE COLUMN_NAME LIKE '%ReportType%' OR COLUMN_NAME LIKE '%ReportTypeName%'
ORDER BY TABLE_NAME
"@ "report type lookup tables"

$conn.Close()
