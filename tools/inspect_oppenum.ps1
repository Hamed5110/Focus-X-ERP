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
        if ($c -ge 40) { break }
    }
    $r.Close()
    Write-Output ("-- shown: $c --")
}

Write-Output "=== pick/enum/combo tables ==="
Dump @"
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME LIKE '%Pick%' OR TABLE_NAME LIKE '%Enum%' OR TABLE_NAME LIKE '%Combo%'
   OR TABLE_NAME LIKE '%DropDown%' OR TABLE_NAME LIKE '%OptionList%' OR TABLE_NAME LIKE '%FieldValue%'
ORDER BY TABLE_NAME
"@

Write-Output "`n=== rows mentioning OpportunityType or New Business ==="
Dump @"
SELECT TABLE_NAME, COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME LIKE 'cCore_%' AND DATA_TYPE IN ('nvarchar','varchar','text')
ORDER BY TABLE_NAME
"@

Write-Output "`n=== extra values for field 306676 ==="
Dump @"
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME LIKE '%ExtraValue%' OR TABLE_NAME LIKE '%FieldOption%' OR TABLE_NAME LIKE '%VoucherField%'
ORDER BY TABLE_NAME
"@

$conn.Close()
