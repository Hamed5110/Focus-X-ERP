$cs = "Server=localhost;Database=Focus80G0;User Id=sa;Password=$env:FOCUS_SQL_PASSWORD;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
function Dump($sql, $title, $max=80, $timeout=60) {
    Write-Output ""
    Write-Output "========== $title =========="
    $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = $timeout; $cmd.CommandText = $sql
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
SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME IN (N'cCore_Reports_0', N'cCore_RDQuery_0', N'cCore_ReportLayouts_0', N'cCore_ReportColumns_0', N'cCore_ReportParameter_0', N'cCore_ReportExtraValues_0')
ORDER BY TABLE_NAME, ORDINAL_POSITION
"@ "report table columns" 200

Dump @"
SELECT r.iReportId, r.sReportName, r.iReportType, r.iSourceType,
       ISNULL(ts.Cnt,0) TranCnt
FROM dbo.cCore_Reports_0 r
LEFT JOIN (
    SELECT iReportId, COUNT(*) Cnt FROM dbo.cCore_ReportTransactionSet_0 GROUP BY iReportId
) ts ON ts.iReportId = r.iReportId
WHERE r.sReportName LIKE N'%Tracking III%' OR r.iReportId IN (70153,70241,70245,70258,70267,70268)
ORDER BY r.iReportId
"@ "existing PT3 / query reports"

Dump @"
SELECT TOP 1 * FROM dbo.cCore_Reports_0 WHERE iReportId = 70241
"@ "70241 report"

Dump @"
SELECT TOP 1 * FROM dbo.cCore_RDQuery_0 WHERE iReportId = 70241
"@ "70241 query row"

Dump @"
SELECT TOP 1 * FROM dbo.cCore_ReportLayouts_0 WHERE iReportId = 70241
"@ "70241 layout"

Dump @"
SELECT TOP 3 * FROM dbo.cCore_ReportParameter_0
WHERE iControlType = 1
ORDER BY iParameterId DESC
"@ "account params"

Dump @"
SELECT iParameterId, iReportId, sParameterName, sFieldVariable, iControlType, iFieldType, iMasterTypeId
FROM dbo.cCore_ReportParameter_0
WHERE iReportId IN (70241,70245,70258,70197)
"@ "params of known queries"

Dump @"
SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = N'cCore_ReportParameter_0' ORDER BY ORDINAL_POSITION
"@ "param cols"

Dump @"
SELECT (SELECT MAX(iReportId) FROM dbo.cCore_Reports_0) MaxRep,
       (SELECT MAX(iLayoutId) FROM dbo.cCore_ReportLayouts_0) MaxLay,
       (SELECT MAX(iColumnId) FROM dbo.cCore_ReportColumns_0) MaxCol,
       (SELECT MAX(iParameterId) FROM dbo.cCore_ReportParameter_0) MaxPar
"@ "idents"

$conn.Close()
