$cs = "Server=localhost;Database=Focus80G0;User Id=sa;Password=$env:FOCUS_SQL_PASSWORD;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
function Dump($sql, $title, $max=60) {
    Write-Output ""
    Write-Output "========== $title =========="
    $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 90; $cmd.CommandText = $sql
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
                    if ($v.Length -gt 160) { $v = $v.Substring(0,160) + "..." }
                    $parts += $v
                }
            }
            Write-Output ($parts -join " | ")
            if ($c -ge $max) { break }
        }
        $r.Close()
        if ($c -eq 0) { Write-Output "(no rows)" }
    } catch { Write-Output $_.Exception.Message }
}

Dump @"
SELECT r.iReportId, r.sReportName, r.iReportType, r.iSourceType,
       r.iModifiedDate, r.iModifiedBy
FROM dbo.cCore_Reports_0 r
WHERE r.iReportType = 1
ORDER BY r.iModifiedDate DESC
"@ "recent Query reports by modified"

Dump @"
SELECT q.iReportId, r.sReportName, r.iReportType, r.iSourceType,
       CASE WHEN q.sSqlQuery LIKE N'%@CustomerName%' THEN 1 ELSE 0 END HasCust,
       CASE WHEN q.sSqlQuery LIKE N'%LEFT JOIN%' THEN 1 ELSE 0 END HasLeft,
       CASE WHEN q.sSqlQuery LIKE N'% AS iDate%' THEN 1 ELSE 0 END HasIDateCol,
       CASE WHEN q.sSqlQuery LIKE N'%h.iDate%' THEN 1 ELSE 0 END HasHIDate,
       CASE WHEN EXISTS (
           SELECT 1 FROM dbo.cCore_ReportTransactionSet_0 ts WHERE ts.iReportId = r.iReportId
       ) THEN 1 ELSE 0 END HasTranSet,
       (SELECT COUNT(*) FROM dbo.cCore_ReportParameter_0 p WHERE p.iReportId = r.iReportId) ParamCnt
FROM dbo.cCore_RDQuery_0 q
JOIN dbo.cCore_Reports_0 r ON r.iReportId = q.iReportId
WHERE q.sSqlQuery LIKE N'%@CustomerName%'
   OR q.sSqlQuery LIKE N'%Balance Amount%'
   OR r.sReportName LIKE N'%simple Test%'
   OR r.sReportName LIKE N'%70245%'
   OR r.iReportId IN (70245,70241,70244,70258,70197,70198)
ORDER BY r.iReportId
"@ "query flags vs transet"

Dump @"
SELECT p.*
FROM dbo.cCore_ReportParameter_0 p
WHERE p.iReportId IN (70245,70241,70244,70258,70197)
"@ "params of PT3 query reports"

Dump @"
SELECT ts.iReportId, r.sReportName, r.iReportType, r.iSourceType,
       COUNT(*) TranRows,
       MIN(ts.iDocumentOption) MinDocOpt, MAX(ts.iDocumentOption) MaxDocOpt,
       MIN(ts.iTranSetId) MinSet, MAX(ts.iTranSetId) MaxSet
FROM dbo.cCore_ReportTransactionSet_0 ts
JOIN dbo.cCore_Reports_0 r ON r.iReportId = ts.iReportId
WHERE r.iReportType = 1
GROUP BY ts.iReportId, r.sReportName, r.iReportType, r.iSourceType
ORDER BY ts.iReportId
"@ "Query reports that still have Transaction Set"

Dump @"
SELECT r.iReportId, r.sReportName, r.iReportType, r.iSourceType,
       ISNULL(ts.Cnt,0) TranCnt, ISNULL(p.Cnt,0) ParamCnt
FROM dbo.cCore_Reports_0 r
LEFT JOIN (
    SELECT iReportId, COUNT(*) Cnt FROM dbo.cCore_ReportTransactionSet_0 GROUP BY iReportId
) ts ON ts.iReportId = r.iReportId
LEFT JOIN (
    SELECT iReportId, COUNT(*) Cnt FROM dbo.cCore_ReportParameter_0 GROUP BY iReportId
) p ON p.iReportId = r.iReportId
WHERE r.iReportType = 1 AND r.iSourceType = 1
ORDER BY r.iReportId DESC
"@ "all Query+SQL reports transet/params"

Dump @"
SELECT TOP 5 q.iReportId, r.sReportName,
       SUBSTRING(q.sSqlQuery, 1, 400) Head,
       CASE WHEN q.sSqlQuery LIKE N'%LEFT JOIN%' THEN 1 ELSE 0 END HasLeft
FROM dbo.cCore_RDQuery_0 q
JOIN dbo.cCore_Reports_0 r ON r.iReportId = q.iReportId
WHERE r.iReportId = 70245
"@ "70245 SQL head"

Dump @"
SELECT f.iFilterId, f.iFilterGroupId, f.iFieldId, f.iOperator, f.sValue, f.sCompareText,
       f.bGroup, f.iConjunction, f.iCompareWith, f.iType, f.iSubParentId, f.iDataType
FROM dbo.cCore_ReportFilter_0 f
JOIN dbo.cCore_Reports_0 r ON r.iFilterGroupId = f.iFilterGroupId
WHERE r.iReportId IN (70245,70153,70244)
"@ "filters via report group"

Dump @"
SELECT name, TYPE_NAME(system_type_id) Typ
FROM sys.columns
WHERE object_id = OBJECT_ID(N'dbo.cCore_Reports_0')
ORDER BY column_id
"@ "cCore_Reports all cols"
