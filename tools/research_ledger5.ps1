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
                    if ($v.Length -gt 400) { $v = $v.Substring(0,400) + "..." }
                    $parts += $v
                }
            }
            Write-Output ($parts -join " | ")
            if ($c -ge 50) { Write-Output "(truncated)"; break }
        }
        $r.Close()
        if ($c -eq 0) { Write-Output "(no rows)" }
    } catch {
        Write-Output ("ERROR: " + $_.Exception.Message)
        try { $r.Close() } catch {}
    }
}

Dump @"
SELECT iFieldId, sCaption, iDataTypeId, iModuleType
FROM dbo.cCore_Fields
WHERE iFieldId IN (-4,-2,1,2,3,4,5,11,12,13,17,18,19,24)
ORDER BY iFieldId
"@ "field captions"

Dump @"
SELECT iLayoutId, sLayoutName, iFlag, bDefault
FROM dbo.cCore_ReportLayouts_0
WHERE iReportId IN (500, 501, 560, 658)
ORDER BY iReportId, iLayoutId
"@ "layouts"

Dump @"
SELECT l.iLayoutId, l.sLayoutName, l.bDefault, c.iFieldId, c.sAliasName, c.iMiscOption, c.iAlignment, c.iType
FROM dbo.cCore_ReportLayouts_0 l
JOIN dbo.cCore_ReportColumns_0 c ON c.iLayoutId = l.iLayoutId
WHERE l.iReportId = 500 AND l.bDefault = 1
ORDER BY c.iFieldId
"@ "500 default layout columns"

Dump @"
SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'cCore_ReportColumnsFunction_0'
ORDER BY ORDINAL_POSITION
"@ "function table cols"

Dump @"
SELECT LEFT(OBJECT_DEFINITION(OBJECT_ID('dbo.vtCode_DataFA_0')), 2500) AS Def
"@ "FA view more"
