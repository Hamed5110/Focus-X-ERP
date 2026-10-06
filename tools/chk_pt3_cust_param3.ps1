$cs = "Server=localhost;Database=Focus80G0;User Id=sa;Password=$env:FOCUS_SQL_PASSWORD;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
function Dump($sql, $title) {
    Write-Output ""
    Write-Output "========== $title =========="
    $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 60; $cmd.CommandText = $sql
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
                    if ($v.Length -gt 100) { $v = $v.Substring(0,100) + "..." }
                    $parts += $v
                }
            }
            Write-Output ($parts -join " | ")
            if ($c -ge 30) { break }
        }
        $r.Close()
        if ($c -eq 0) { Write-Output "(no rows)" }
    } catch { Write-Output $_.Exception.Message }
}

Dump "SELECT TOP 1 * FROM dbo.cCore_ReportFilter_0" "filter sample"
Dump @"
SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = N'cCore_ReportFilter_0' ORDER BY ORDINAL_POSITION
"@ "filter cols"

Dump @"
SELECT TOP 15 iFilterId, iReportId, iFieldId, iOperator, sCompareValue, sCompareText, iParentId, iSubParentId
FROM dbo.cCore_ReportFilter_0
WHERE iReportId IN (70125, 70153, 70245) OR sCompareText LIKE N'%Receiv%' OR sCompareValue IN (N'280', N'180')
"@ "receivable filters"

Dump @"
SELECT TOP 12 iMasterId, sCode, sName, bGroup
FROM dbo.mCore_Account
WHERE sName LIKE N'%Ebrahim Saad%' OR sName LIKE N'%Hussain%' AND ISNULL(bGroup,0)=0
ORDER BY sName
"@ "sample customers"

Dump @"
SELECT p.iReportId, r.sReportName, p.sFieldName, p.sFieldVariable, p.iControlType, p.iFieldId, p.bGroup,
       LEFT(ISNULL(p.sDefault,N''),80) Def
FROM dbo.cCore_ReportParameter_0 p
JOIN dbo.cCore_Reports_0 r ON r.iReportId = p.iReportId
WHERE p.iControlType = 1
ORDER BY p.iReportId
"@ "all Account-type params G0"
