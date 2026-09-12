$cs = "Server=localhost;Database=Focus80E0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
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
                    if ($v.Length -gt 160) { $v = $v.Substring(0,160) + "..." }
                    $parts += $v
                }
            }
            Write-Output ($parts -join " | ")
            if ($c -ge 35) { Write-Output "(truncated)"; break }
        }
        $r.Close()
        if ($c -eq 0) { Write-Output "(no rows)" }
    } catch {
        Write-Output ("ERROR: " + $_.Exception.Message)
        try { if ($r) { $r.Close() } } catch {}
    }
}

Dump @"
SELECT COLUMN_NAME, DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME='tCore_HeaderData4610_0'
ORDER BY ORDINAL_POSITION
"@ "4610 extra columns"

Dump @"
SELECT COLUMN_NAME, DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME IN ('tCore_HeaderData4609_0','tCore_HeaderData4608_0','tCore_HeaderData5634_0')
  AND (COLUMN_NAME LIKE '%Contract%' OR COLUMN_NAME LIKE '%Sales%' OR COLUMN_NAME LIKE '%Order%'
       OR COLUMN_NAME LIKE '%Ref%' OR COLUMN_NAME LIKE '%Link%' OR COLUMN_NAME LIKE '%Doc%'
       OR COLUMN_NAME LIKE '%CON%' OR COLUMN_NAME LIKE '%SO%')
ORDER BY TABLE_NAME, COLUMN_NAME
"@ "link-like extra fields"

Dump @"
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME LIKE '%Link%' OR TABLE_NAME LIKE '%Ref%' OR TABLE_NAME LIKE 'tCore_Inv%'
ORDER BY TABLE_NAME
"@ "link/ref tables"

Dump @"
SELECT COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME='tCore_Data_0'
  AND (COLUMN_NAME LIKE '%Ref%' OR COLUMN_NAME LIKE '%Link%' OR COLUMN_NAME LIKE '%Inv%'
       OR COLUMN_NAME LIKE '%Parent%' OR COLUMN_NAME LIKE '%Against%')
ORDER BY ORDINAL_POSITION
"@ "tCore_Data ref cols"

Dump @"
SELECT TOP 1 * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME LIKE 'tCore_Links%'
"@ "tCore_Links exists"

$conn.Close()
