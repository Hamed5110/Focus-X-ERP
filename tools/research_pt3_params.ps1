$cs = "Server=localhost;Database=Focus80G0;User Id=sa;Password=$env:FOCUS_SQL_PASSWORD;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
function Dump($sql, $title, $max=80) {
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
                    if ($v.Length -gt 140) { $v = $v.Substring(0,140) + "..." }
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
SELECT name FROM sys.databases
WHERE name LIKE N'Focus80%'
ORDER BY name
"@ "company dbs"

Dump @"
SELECT c.TABLE_NAME, c.COLUMN_NAME, c.DATA_TYPE, c.CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS c
WHERE c.TABLE_NAME IN (
    N'cCore_ReportParameter_0', N'cCore_ReportTransactionSet_0',
    N'cCore_ReportFilter_0', N'cCore_RDQuery_0', N'cCore_Reports_0',
    N'cCore_ReportExtraValues_0'
)
ORDER BY c.TABLE_NAME, c.ORDINAL_POSITION
"@ "param-related columns" 200

Dump @"
SELECT r.iReportId, r.sReportName, r.iReportType, r.iSourceType, r.iModule, r.bDelayFetch
FROM dbo.cCore_Reports_0 r
WHERE r.sReportName LIKE N'%Tracking%'
   OR r.sReportName LIKE N'%III%'
   OR r.sReportName LIKE N'%Atlas Ledger%'
   OR r.sReportName LIKE N'%Commission%Atlas%'
ORDER BY r.iReportId
"@ "candidate reports"

Dump @"
SELECT p.iParameterId, p.iReportId, r.sReportName, r.iReportType, r.iSourceType,
       p.sFieldName, p.sFieldVariable, p.iControlType, p.iFieldType, p.iSelectionMode,
       p.iFieldId, p.iSubParentId, p.iType, p.bGroup, p.bDefault,
       LEFT(ISNULL(p.sDefault,N''),80) Def, LEFT(ISNULL(p.sValue,N''),80) Val
FROM dbo.cCore_ReportParameter_0 p
JOIN dbo.cCore_Reports_0 r ON r.iReportId = p.iReportId
WHERE r.sReportName LIKE N'%Tracking%'
   OR r.sReportName LIKE N'%III%'
   OR p.sFieldVariable LIKE N'%Customer%'
   OR p.sFieldVariable LIKE N'%iStart%'
   OR p.sFieldVariable LIKE N'%Date%'
ORDER BY p.iReportId, p.iParameterId
"@ "PT3 and date/customer params"

Dump @"
SELECT ts.*
FROM dbo.cCore_ReportTransactionSet_0 ts
JOIN dbo.cCore_Reports_0 r ON r.iReportId = ts.iReportId
WHERE r.sReportName LIKE N'%Tracking%'
   OR r.sReportName LIKE N'%III%'
   OR r.iReportId IN (70266, 70153, 70256, 70257)
"@ "transaction sets"

Dump @"
SELECT r.iReportId, r.sReportName, LEN(q.sSqlQuery) SqlLen,
       CASE WHEN q.sSqlQuery LIKE N'%LEFT JOIN%' THEN 1 ELSE 0 END HasLeft,
       CASE WHEN q.sSqlQuery LIKE N'%iDate%' THEN 1 ELSE 0 END HasIDate,
       CASE WHEN q.sSqlQuery LIKE N'%@iStartDate%' THEN 1 ELSE 0 END HasStart,
       CASE WHEN q.sSqlQuery LIKE N'%@CustomerName%' THEN 1 ELSE 0 END HasCust,
       LEFT(q.sSqlQuery, 180) Head
FROM dbo.cCore_RDQuery_0 q
JOIN dbo.cCore_Reports_0 r ON r.iReportId = q.iReportId
WHERE r.sReportName LIKE N'%Tracking%'
   OR r.sReportName LIKE N'%III%'
   OR r.iReportId IN (70266, 70267)
ORDER BY r.iReportId
"@ "saved SQL flags"

Dump @"
SELECT p.iControlType, COUNT(*) Cnt,
       MIN(p.sFieldName) ExName, MIN(p.sFieldVariable) ExVar
FROM dbo.cCore_ReportParameter_0 p
GROUP BY p.iControlType
ORDER BY p.iControlType
"@ "all control types"

Dump @"
SELECT TOP 20 p.iReportId, r.sReportName, r.iReportType, r.iSourceType,
       p.sFieldName, p.sFieldVariable, p.iControlType, p.iFieldType
FROM dbo.cCore_ReportParameter_0 p
JOIN dbo.cCore_Reports_0 r ON r.iReportId = p.iReportId
WHERE r.iReportId = 70266
ORDER BY p.iParameterId
"@ "70266 params"

Dump @"
SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = N'cCore_ReportTransactionSet_0'
ORDER BY ORDINAL_POSITION
"@ "tran set cols"
