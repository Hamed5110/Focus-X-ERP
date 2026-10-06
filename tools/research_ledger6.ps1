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
SELECT l.iLayoutId, l.iReportId, r.sReportName, l.sLayoutName, l.iFlag
FROM dbo.cCore_ReportLayouts_0 l
JOIN dbo.cCore_Reports_0 r ON r.iReportId = l.iReportId
WHERE r.iReportId IN (500, 501, 560, 658)
ORDER BY r.iReportId, l.iLayoutId
"@ "layout to report"

Dump @"
SELECT c.iFieldId, c.sColumn, c.sAliasName, c.iType, c.iMiscOption, c.iAlignment, c.iDecimalInColumn
FROM dbo.cCore_ReportColumns_0 c
WHERE c.iLayoutId = 6643
ORDER BY c.iFieldId
"@ "layout 6643 columns"

Dump @"
SELECT c.iFieldId, c.sColumn, c.sAliasName, c.iType, c.iMiscOption, c.iAlignment, c.iDecimalInColumn
FROM dbo.cCore_ReportColumns_0 c
WHERE c.iLayoutId = 6644
ORDER BY c.iFieldId
"@ "layout 6644 columns"

Dump @"
SELECT CHARINDEX('Debit', m.definition) P1, CHARINDEX('Credit', m.definition) P2, LEN(m.definition) L
FROM sys.sql_modules m
WHERE m.object_id = OBJECT_ID('dbo.vtCode_DataFA_0')
"@ "FA def debit positions"

Dump @"
SELECT SUBSTRING(m.definition, CHARINDEX('Debit', m.definition) - 80, 400) AS DebitSlice
FROM sys.sql_modules m
WHERE m.object_id = OBJECT_ID('dbo.vtCode_DataFA_0')
"@ "FA Debit formula slice"
