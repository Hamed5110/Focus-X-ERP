$cs = "Server=localhost;Database=Focus80G0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
function Dump($sql, $title) {
    Write-Output ""
    Write-Output "========== $title =========="
    $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 120; $cmd.CommandText = $sql
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
                if ($r.IsDBNull($i)) { $parts += "NULL" } else {
                    $v = [string]$r.GetValue($i)
                    $v = $v -replace "`r|`n"," "
                    if ($v.Length -gt 280) { $v = $v.Substring(0,280) + "..." }
                    $parts += $v
                }
            }
            Write-Output ($parts -join " | ")
            if ($c -ge 60) { Write-Output "(truncated)"; break }
        }
        $r.Close()
        if ($c -eq 0) { Write-Output "(no rows)" }
    } catch {
        Write-Output ("ERROR: " + $_.Exception.Message)
        try { $r.Close() } catch {}
    }
}

Dump @"
SELECT COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'cCore_Fields' ORDER BY ORDINAL_POSITION
"@ "cCore_Fields cols"

Dump @"
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME LIKE '%Function%' OR TABLE_NAME LIKE '%Field%'
ORDER BY TABLE_NAME
"@ "function/field tables"

Dump @"
SELECT f.iFunctionId, f.iType, f.sValue, COUNT(*) Cnt,
  MIN(c.sAliasName) SampleAlias, MIN(c.iFieldId) SampleField
FROM dbo.cCore_ReportColumnsFunction_0 f
JOIN dbo.cCore_ReportColumns_0 c ON c.iColumnId = f.iColumnId
WHERE f.iFunctionId IN (1370, 1371, 1372, 1650)
GROUP BY f.iFunctionId, f.iType, f.sValue
"@ "function 1370-1372 1650 usage"

Dump @"
SELECT r.iReportId, r.sReportName, c.iFieldId, c.sAliasName, c.iMiscOption, fn.iFunctionId, fn.sValue
FROM dbo.cCore_ReportColumnsFunction_0 fn
JOIN dbo.cCore_ReportColumns_0 c ON c.iColumnId = fn.iColumnId
JOIN dbo.cCore_ReportLayouts_0 l ON l.iLayoutId = c.iLayoutId
JOIN dbo.cCore_Reports_0 r ON r.iReportId = l.iReportId
WHERE fn.iFunctionId IN (1370, 1371, 1372)
ORDER BY r.iReportId, c.iFieldId
"@ "all reports using 1370-1372"

Dump @"
SELECT r.iReportId, r.sReportName, c.iFieldId, c.sAliasName, fn.iFunctionId
FROM dbo.cCore_ReportColumnsFunction_0 fn
JOIN dbo.cCore_ReportColumns_0 c ON c.iColumnId = fn.iColumnId
JOIN dbo.cCore_ReportLayouts_0 l ON l.iLayoutId = c.iLayoutId
JOIN dbo.cCore_Reports_0 r ON r.iReportId = l.iReportId
WHERE c.sAliasName LIKE '%Balance%'
ORDER BY r.iReportId
"@ "Balance columns with function row"
