$cs = "Server=localhost;Database=Focus80G0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
function Dump($sql, $title) {
    Write-Output ""
    Write-Output "========== $title =========="
    $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 180; $cmd.CommandText = $sql
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
                    if ($v.Length -gt 500) { $v = $v.Substring(0,500) + "..." }
                    $parts += $v
                }
            }
            Write-Output ($parts -join " | ")
            if ($c -ge 100) { Write-Output "(truncated)"; break }
        }
        $r.Close()
        if ($c -eq 0) { Write-Output "(no rows)" }
    } catch {
        Write-Output ("ERROR: " + $_.Exception.Message)
        try { $r.Close() } catch {}
    }
}

Dump @"
SELECT OBJECT_NAME(object_id) AS obj, o.type_desc
FROM sys.sql_modules m
JOIN sys.objects o ON o.object_id = m.object_id
WHERE m.definition LIKE N'%Customer Statement%'
   OR m.definition LIKE N'%customer statement%'
   OR m.definition LIKE N'%CustomerStatement%'
   OR m.definition LIKE N'%CustStatement%'
ORDER BY o.type_desc, obj
"@ "modules mentioning customer statement"

Dump @"
SELECT name FROM sys.views ORDER BY name
"@ "all views"

Dump @"
SELECT l.iLayoutId, c.iFieldId, c.iType, c.sColumn, c.sAliasName, c.iMiscOption, c.iAlignment, c.iDecimalInColumn
FROM dbo.cCore_ReportLayouts_0 l
JOIN dbo.cCore_ReportColumns_0 c ON c.iLayoutId = l.iLayoutId
WHERE l.iReportId IN (560, 70115, 70125, 500, 658)
ORDER BY l.iReportId, c.iFieldId
"@ "statement report layouts"

Dump @"
SELECT iReportId, LEFT(sSqlQuery, 400) AS Head
FROM dbo.cCore_RDQuery_0
WHERE iReportId IN (560, 70115, 70125, 70036)
   OR sSqlQuery LIKE N'%Statement%'
"@ "query SQL for statement reports"
