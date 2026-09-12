$cs = "Server=localhost;Database=Focus80E0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 180
function Dump($sql, $title) {
    Write-Output ""
    Write-Output "========== $title =========="
    $cmd.CommandText = $sql
    try {
        $r = $cmd.ExecuteReader()
        $n = $r.FieldCount
        $hdr = @(); for ($i=0; $i -lt $n; $i++) { $hdr += $r.GetName($i) }
        Write-Output ($hdr -join " | ")
        $c = 0
        while ($r.Read()) {
            $c++
            $parts = @()
            for ($i=0; $i -lt $n; $i++) {
                if ($r.IsDBNull($i)) { $parts += "" } else {
                    $v = [string]$r.GetValue($i)
                    $v = $v -replace "`r|`n"," "
                    if ($v.Length -gt 500) { $v = $v.Substring(0,500) + "..." }
                    $parts += $v
                }
            }
            Write-Output ($parts -join " | ")
            if ($c -ge 40) { break }
        }
        $r.Close()
    } catch {
        Write-Output ("ERROR: " + $_.Exception.Message)
        try { $conn.Close(); $conn.Open(); $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 180 } catch {}
    }
}

Dump @"
SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME='cCore_ReportTransactionSet_0' ORDER BY ORDINAL_POSITION
"@ "TranSet cols"

Dump @"
SELECT t.*
FROM dbo.cCore_ReportTransactionSet_0 t
WHERE t.iReportId IN (70028, 70064, 70074, 70220, 70221, 70266, 70256, 70259)
ORDER BY t.iReportId
"@ "tran sets for key reports"

Dump @"
SELECT r.iReportId, r.sReportName, r.iReportType, r.iSourceType,
       CASE WHEN q.iReportId IS NULL THEN 0 ELSE 1 END HasSql,
       CASE WHEN q.sSqlQuery LIKE N'%iDate%' THEN 1 ELSE 0 END HasIDateCol
FROM dbo.cCore_Reports_0 r
LEFT JOIN dbo.cCore_RDQuery_0 q ON q.iReportId = r.iReportId
WHERE r.iSourceType = 1 OR (r.iReportType = 1 AND q.iReportId IS NOT NULL)
ORDER BY r.iReportId
"@ "custom SQL reports"

Dump @"
SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME='cCore_ReportFilter_0' ORDER BY ORDINAL_POSITION
"@ "filter cols"

Dump @"
SELECT TOP 20 f.*, r.sReportName
FROM dbo.cCore_ReportFilter_0 f
LEFT JOIN dbo.cCore_Reports_0 r ON r.iFilterGroupId = f.iFilterGroupId
WHERE r.iReportId IN (70028, 70266, 70220, 70256)
"@ "filters"

Dump @"
SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME='cCore_ReportParameter_0' ORDER BY ORDINAL_POSITION
"@ "param cols"
