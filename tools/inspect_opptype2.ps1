$cs = "Server=localhost;Database=Focus80E0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
function Dump($sql) {
    $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 240; $cmd.CommandText = $sql
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

Write-Output "=== cCore_VoucherFields_0 columns ==="
Dump "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='cCore_VoucherFields_0' ORDER BY ORDINAL_POSITION"

Write-Output "`n=== cCore_Fields columns ==="
Dump "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='cCore_Fields' ORDER BY ORDINAL_POSITION"

Write-Output "`n=== find New Business in any mCore sName ==="
Dump @"
SELECT t.name AS Tbl
FROM sys.tables t
INNER JOIN sys.columns c ON c.object_id=t.object_id
WHERE t.name LIKE 'mCore_%' AND c.name='sName'
  AND t.name NOT LIKE '%Language%' AND t.name NOT LIKE '%Tree%'
  AND EXISTS (
        SELECT 1 FROM sys.columns c2
        WHERE c2.object_id=t.object_id AND c2.name='iMasterId'
      )
ORDER BY t.name
"@

$conn.Close()
