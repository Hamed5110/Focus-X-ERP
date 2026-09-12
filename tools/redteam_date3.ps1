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
                    if ($v.Length -gt 450) { $v = $v.Substring(0,450) + "..." }
                    $parts += $v
                }
            }
            Write-Output ($parts -join " | ")
            if ($c -ge 30) { break }
        }
        $r.Close()
    } catch {
        Write-Output ("ERROR: " + $_.Exception.Message)
        try { $conn.Close(); $conn.Open(); $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 180 } catch {}
    }
}

Dump @"
SELECT r.iReportId, r.sReportName, r.iReportType, r.iSourceType,
       COUNT(t.iTranId) AS TranSets
FROM dbo.cCore_Reports_0 r
LEFT JOIN dbo.cCore_ReportTransactionSet_0 t ON t.iReportId = r.iReportId
WHERE r.iReportId IN (70028,70064,70074,70219,70220,70221,70230,70256,70259,70266)
GROUP BY r.iReportId, r.sReportName, r.iReportType, r.iSourceType
"@ "TS count vs source type"

Dump @"
SELECT r.iSourceType, r.iReportType,
       SUM(CASE WHEN t.iTranId IS NULL THEN 0 ELSE 1 END) AS WithTS,
       SUM(CASE WHEN t.iTranId IS NULL THEN 1 ELSE 0 END) AS NoTS
FROM dbo.cCore_Reports_0 r
LEFT JOIN dbo.cCore_ReportTransactionSet_0 t ON t.iReportId = r.iReportId
GROUP BY r.iSourceType, r.iReportType
"@ "TS by source/type"

Dump @"
SELECT TOP 8 iParameterId, iReportId, sFieldName, sFieldVariable, iControlType, iFieldType, iSelectionMode
FROM dbo.cCore_ReportParameter_0
WHERE iReportId IN (70028,70220,70256,70266,70259) OR sFieldVariable LIKE N'%Date%'
"@ "params"

Dump @"
SELECT TOP 1 LEFT(sSqlQuery, 400) FROM dbo.cCore_RDQuery_0 WHERE iReportId=70259
"@ "70259 SQL start"

Dump @"
SELECT TOP 1
  CASE WHEN sSqlQuery LIKE N'%DateToInt%' THEN 1 ELSE 0 END AS UsesDateToInt,
  CASE WHEN sSqlQuery LIKE N'%GETDATE%' THEN 1 ELSE 0 END AS UsesGetDate,
  CASE WHEN sSqlQuery LIKE N'%WHERE%' THEN 1 ELSE 0 END AS HasWhere
FROM dbo.cCore_RDQuery_0 WHERE iReportId=70259
"@ "70259 date style"

Dump @"
SELECT iReportId,
  CASE WHEN sSqlQuery LIKE N'%DateToInt%' THEN 1 ELSE 0 END AS UsesDateToInt,
  CASE WHEN sSqlQuery LIKE N'%GETDATE%' THEN 1 ELSE 0 END AS UsesGetDate,
  CASE WHEN sSqlQuery LIKE N'%IntToDate%' THEN 1 ELSE 0 END AS UsesIntToDate
FROM dbo.cCore_RDQuery_0
"@ "all custom SQL date helpers"

Dump @"
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME LIKE '%Report%Run%' OR TABLE_NAME LIKE '%Session%'
   OR TABLE_NAME LIKE '%FilterDate%' OR TABLE_NAME LIKE '%AsOn%'
   OR TABLE_NAME LIKE 'cCore_ReportExtra%'
ORDER BY TABLE_NAME
"@ "session/run tables"
