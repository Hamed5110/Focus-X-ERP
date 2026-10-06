$cs = "Server=localhost;Database=Focus8080;User Id=sa;Password=$env:FOCUS_SQL_PASSWORD;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
function Dump($sql, $title) {
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
            if ($c -ge 80) { break }
        }
        $r.Close()
        if ($c -eq 0) { Write-Output "(no rows)" }
    } catch { Write-Output $_.Exception.Message }
}

Dump @"
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME LIKE N'%Filter%' OR TABLE_NAME LIKE N'%Default%' OR TABLE_NAME LIKE N'%ReportParam%'
ORDER BY TABLE_NAME
"@ "filter tables"

Dump @"
SELECT iColumnId, iLayoutId, iFieldId, iType, sColumn, sAliasName, fColumnWidth, iMiscOption, iAlignment, iSubParentId, iDecimalInColumn
FROM dbo.cCore_ReportColumns_0
WHERE iLayoutId IN (6894,6899,6903)
ORDER BY iLayoutId, iColumnId
"@ "glass layout columns"

Dump @"
SELECT iFieldId, sCaption, iDataTypeId FROM dbo.cCore_Fields WHERE iFieldId IN (300006,306832,1,2,3,4,5002,305081,307309)
"@ "known field ids"

Dump @"
SELECT iFilterId, iFilterGroupId, iFieldId, iOperator, sValue, sCompareText, bGroup, iConjunction, iCompareWith, iType, iSubParentId, iDataType
FROM dbo.cCore_ReportFilter_0
WHERE iFilterGroupId IN (
  SELECT iFilterGroupId FROM dbo.cCore_Reports_0 WHERE iReportId IN (70252,70248,70255,70125)
)
"@ "filters via group"

Dump @"
SELECT TOP 15 iReportId, sReportName, iFilterGroupId FROM dbo.cCore_Reports_0
WHERE iFilterGroupId > 0 AND iReportType = 2
ORDER BY iReportId DESC
"@ "cubes with filter groups"
