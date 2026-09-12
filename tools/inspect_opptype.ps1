$cs = "Server=localhost;Database=Focus80E0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
function Dump($sql) {
    $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 180; $cmd.CommandText = $sql
    $r = $cmd.ExecuteReader()
    $n = $r.FieldCount
    $hdr = @(); for ($i=0;$i -lt $n;$i++) { $hdr += $r.GetName($i) }
    Write-Output ("COLS: " + ($hdr -join " | "))
    $c = 0
    while ($r.Read()) {
        $parts = @()
        for ($i = 0; $i -lt $n; $i++) {
            if ($r.IsDBNull($i)) { $parts += "" } else { $parts += [string]$r.GetValue($i) }
        }
        Write-Output ($parts -join " | ")
        $c++
        if ($c -ge 60) { break }
    }
    $r.Close()
    Write-Output ("-- shown: $c --")
}

Write-Output "=== voucher fields named Opportunity ==="
Dump @"
SELECT TOP 20 * FROM dbo.cCore_VoucherFields_0
WHERE sFieldName LIKE '%Opport%' OR sCaption LIKE '%Opport%' OR sAlias LIKE '%Opport%'
"@

Write-Output "`n=== all mCore tables with opportunity in name (any case) ==="
Dump @"
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES
WHERE LOWER(TABLE_NAME) LIKE '%opport%' OR LOWER(TABLE_NAME) LIKE '%newbusiness%'
   OR LOWER(TABLE_NAME) LIKE '%additionalbusiness%'
ORDER BY TABLE_NAME
"@

Write-Output "`n=== search New Business in text masters ==="
Dump @"
SELECT TOP 20 TABLE_NAME FROM INFORMATION_SCHEMA.COLUMNS
WHERE COLUMN_NAME='sName' AND TABLE_NAME LIKE 'mCore_%'
  AND TABLE_NAME NOT LIKE '%Language%' AND TABLE_NAME NOT LIKE '%Tree%'
"@

Write-Output "`n=== cCore_Fields opportunity ==="
Dump @"
SELECT TOP 20 * FROM dbo.cCore_Fields
WHERE sName LIKE '%Opport%' OR sCaption LIKE '%Opport%' OR sFieldName LIKE '%Opport%'
"@

$conn.Close()
