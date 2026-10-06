$cs = "Server=localhost;Database=Focus80G0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
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
                    if ($v.Length -gt 100) { $v = $v.Substring(0,100) + "..." }
                    $parts += $v
                }
            }
            Write-Output ($parts -join " | ")
            if ($c -ge 40) { Write-Output "(truncated)"; break }
        }
        $r.Close()
        if ($c -eq 0) { Write-Output "(no rows)" }
    } catch {
        Write-Output ("ERROR: " + $_.Exception.Message)
        try { $r.Close() } catch {}
    }
}

Dump @"
SELECT iReportId, sReportName, iReportType, iSourceType, iGroupStyle, bAdvance
FROM dbo.cCore_Reports_0 WHERE iReportId IN (70125, 560, 500)
"@ "70125/560/500 header"

Dump @"
SELECT l.iLayoutId, l.sLayoutName, c.iFieldId, c.sColumn, c.sAliasName, c.iType, c.iMiscOption, c.iAlignment, c.sFormula
FROM dbo.cCore_ReportLayouts_0 l
JOIN dbo.cCore_ReportColumns_0 c ON c.iLayoutId = l.iLayoutId
WHERE l.iReportId = 70125
ORDER BY c.iFieldId, c.iColumnId
"@ "70125 columns"

Dump @"
SELECT iTransactionSetId, iVoucherType, iTranSetId, iDocumentOption
FROM dbo.cCore_ReportTransactionSet_0 WHERE iReportId = 70125
"@ "70125 transets"

Dump @"
SELECT TOP 5 LEFT(ISNULL(sSqlQuery,N''), 80) AS q
FROM dbo.cCore_RDQuery_0 WHERE iReportId = 70125
"@ "70125 query sql?"
