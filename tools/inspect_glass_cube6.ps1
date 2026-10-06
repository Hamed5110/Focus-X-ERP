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
            if ($c -ge 40) { break }
        }
        $r.Close()
        if ($c -eq 0) { Write-Output "(no rows)" }
    } catch { Write-Output $_.Exception.Message }
}

Dump @"
SELECT TOP 40 p.iParameterId, p.iReportId, r.sReportName, r.iReportType, p.sFieldName, p.sFieldVariable, p.iControlType, p.iFieldType, p.iSelectionMode, p.sValue, p.iFieldId, p.iSubParentId, p.iType, p.sDefault
FROM dbo.cCore_ReportParameter_0 p
JOIN dbo.cCore_Reports_0 r ON r.iReportId = p.iReportId
WHERE p.iControlType IN (2,4,3010,3054,3080) OR p.sFieldName LIKE N'%Vendor%' OR p.sFieldName LIKE N'%Job%' OR p.sFieldVariable LIKE N'%Vendor%' OR p.sFieldName LIKE N'%Account%'
ORDER BY p.iReportId
"@ "cube-like params"

Dump @"
SELECT iColumnId, iLayoutId, iFieldId, iType, sColumn, sAliasName, fColumnWidth, iMiscOption, iAlignment, iSubParentId
FROM dbo.cCore_ReportColumns_0 WHERE iColumnId IN (2085,107890)
"@ "narration column sample"

Dump @"
SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME=N'cCore_ReportGrouping_0' ORDER BY ORDINAL_POSITION
"@ "grouping cols"

Dump @"
SELECT * FROM dbo.cCore_ReportGrouping_0 WHERE iReportId IN (70252,70248)
"@ "glass grouping"
