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
                    if ($v.Length -gt 500) { $v = $v.Substring(0,500) + "..." }
                    $parts += $v
                }
            }
            Write-Output ($parts -join " | ")
            if ($c -ge 80) { Write-Output "(truncated)"; break }
        }
        $r.Close()
        if ($c -eq 0) { Write-Output "(no rows)" }
    } catch {
        Write-Output ("ERROR: " + $_.Exception.Message)
        try { $r.Close() } catch {}
    }
}

Dump @"
SELECT iFieldId, sCaption, sFieldName, iDataType, iControlType
FROM dbo.cCore_Fields
WHERE iFieldId IN (-4, 2, 17, 18, 19, 24, 171) OR sCaption LIKE '%Balance%' OR sCaption IN ('Debit','Credit','Date')
ORDER BY iFieldId
"@ "cCore_Fields balance/debit"

Dump @"
SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'cCore_ReportGrouping_0'
ORDER BY ORDINAL_POSITION
"@ "grouping cols"

Dump @"
SELECT * FROM dbo.cCore_ReportGrouping_0 WHERE iReportId IN (500, 501, 560, 658)
"@ "grouping 500/560"

Dump @"
SELECT c.iFieldId, c.sAliasName, c.iType, c.iMiscOption
FROM dbo.cCore_ReportLayouts_0 l
JOIN dbo.cCore_ReportColumns_0 c ON c.iLayoutId = l.iLayoutId
WHERE l.iReportId = 70036
ORDER BY c.iFieldId
"@ "70036 Vendor Ledger layout"

Dump @"
SELECT LEN(sSqlQuery) SqlLen, LEFT(sSqlQuery, 1500) Head
FROM dbo.cCore_RDQuery_0
WHERE iReportId = 70036
"@ "70036 Vendor Ledger SQL head"

Dump @"
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME LIKE '%Report%Formula%'
   OR TABLE_NAME LIKE '%Report%Field%'
   OR TABLE_NAME LIKE '%Cube%'
   OR TABLE_NAME LIKE '%RDQuery%'
   OR TABLE_NAME LIKE '%ReportXML%'
   OR TABLE_NAME LIKE '%ReportData%'
ORDER BY TABLE_NAME
"@ "report formula/xml tables"
