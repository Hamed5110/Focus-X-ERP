$cs = "Server=localhost;Database=Focus80E0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 120
function Dump($sql, $title) {
    Write-Output ""
    Write-Output "========== $title =========="
    $cmd.CommandText = $sql
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
                    if ($v.Length -gt 2500) { $v = $v.Substring(0,2500) + "...TRUNC" }
                    $parts += $v
                }
            }
            Write-Output ($parts -join " | ")
            if ($c -ge 40) { break }
        }
        $r.Close()
    } catch {
        Write-Output ("ERROR: " + $_.Exception.Message)
        try { $conn.Close(); $conn.Open(); $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 120 } catch {}
    }
}

Dump "SELECT OBJECT_DEFINITION(OBJECT_ID('dbo.fCore_DateTimeToInt')) AS Def" "fCore_DateTimeToInt"
Dump "SELECT OBJECT_DEFINITION(OBJECT_ID('dbo.fCore_IntToDateTime')) AS Def" "fCore_IntToDateTime"
Dump "SELECT OBJECT_DEFINITION(OBJECT_ID('dbo.IntToGregDateTime')) AS Def" "IntToGregDateTime"
Dump "SELECT OBJECT_DEFINITION(OBJECT_ID('dbo.fCore_GetDefaultValuesForDate')) AS Def" "fCore_GetDefaultValuesForDate"
Dump "SELECT OBJECT_DEFINITION(OBJECT_ID('dbo.fCore_GetDefaultValuesForDateTime')) AS Def" "fCore_GetDefaultValuesForDateTime"
Dump "SELECT OBJECT_DEFINITION(OBJECT_ID('dbo.IntToDate')) AS Def" "IntToDate"

Dump @"
SELECT TABLE_NAME
FROM INFORMATION_SCHEMA.VIEWS
WHERE TABLE_NAME LIKE '%Header%' OR TABLE_NAME LIKE 'v%Core%' AND TABLE_NAME LIKE '%Tran%'
   OR TABLE_NAME LIKE 'vtCore%' OR TABLE_NAME LIKE 'viCore%' OR TABLE_NAME LIKE 'vCore_Header%'
   OR TABLE_NAME LIKE '%voucher%' OR TABLE_NAME LIKE '%Voucher%'
ORDER BY TABLE_NAME
"@ "header/voucher views"

Dump @"
SELECT name FROM sys.views
WHERE name LIKE 'v%Header%' OR name LIKE 'vt%' OR name LIKE 'viCore%'
   OR name LIKE '%tCore_Header%' OR name LIKE 'vCore_%Tran%'
ORDER BY name
"@ "sys header-like views"
