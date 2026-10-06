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
                    if ($v.Length -gt 350) { $v = $v.Substring(0,350) + "..." }
                    $parts += $v
                }
            }
            Write-Output ($parts -join " | ")
            if ($c -ge 90) { Write-Output "(truncated)"; break }
        }
        $r.Close()
        if ($c -eq 0) { Write-Output "(no rows)" }
    } catch {
        Write-Output ("ERROR: " + $_.Exception.Message)
        try { $r.Close() } catch {}
    }
}

Dump @"
SELECT r.iReportId, r.sReportName, r.iModule, r.iReportType, r.iSourceType, r.iGroupStyle, r.bAdvance, r.sRemarks
FROM dbo.cCore_Reports_0 r
WHERE r.iReportId IN (500, 501, 513, 542, 560, 658)
"@ "header 500/560"

Dump @"
SELECT c.iColumnId, c.iFieldId, c.sColumn, c.sAliasName, c.iType, c.iMiscOption, c.iAlignment, c.iDecimalInColumn, c.iParentId, c.iSubParentId
FROM dbo.cCore_ReportLayouts_0 l
JOIN dbo.cCore_ReportColumns_0 c ON c.iLayoutId = l.iLayoutId
WHERE l.iReportId = 500
ORDER BY c.iFieldId, c.iColumnId
"@ "500 Ledger ALL columns"

Dump @"
SELECT c.iColumnId, c.iFieldId, c.sColumn, c.sAliasName, c.iType, c.iMiscOption, c.iAlignment, c.iDecimalInColumn
FROM dbo.cCore_ReportLayouts_0 l
JOIN dbo.cCore_ReportColumns_0 c ON c.iLayoutId = l.iLayoutId
WHERE l.iReportId = 658
ORDER BY c.iFieldId
"@ "658 Ledger detail columns"

Dump @"
SELECT c.iColumnId, c.iFieldId, c.sColumn, c.sAliasName, c.iType, c.iMiscOption, c.iAlignment
FROM dbo.cCore_ReportLayouts_0 l
JOIN dbo.cCore_ReportColumns_0 c ON c.iLayoutId = l.iLayoutId
WHERE l.iReportId = 501
ORDER BY c.iFieldId
"@ "501 Sub ledger columns"

Dump @"
SELECT f.iColumnId, f.iFunctionId, f.iType, f.sValue, c.sAliasName, c.iFieldId, l.iReportId
FROM dbo.cCore_ReportColumnsFunction_0 f
JOIN dbo.cCore_ReportColumns_0 c ON c.iColumnId = f.iColumnId
JOIN dbo.cCore_ReportLayouts_0 l ON l.iLayoutId = c.iLayoutId
WHERE l.iReportId IN (500, 501, 560, 658)
ORDER BY l.iReportId, c.iFieldId
"@ "column functions 500/560"

Dump @"
SELECT t.iReportId, t.iVoucherType, v.sName, t.iTranSetId, t.iDocumentOption
FROM dbo.cCore_ReportTransactionSet_0 t
LEFT JOIN dbo.cCore_vouchers_0 v ON v.iVoucherType = t.iVoucherType
WHERE t.iReportId IN (500, 501, 560, 658)
ORDER BY t.iReportId, t.iVoucherType
"@ "transaction sets"
