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
        if ($c -ge 50) { break }
    }
    $r.Close()
    Write-Output ("-- shown: $c --")
}

Write-Output "=== opportunity type master tables ==="
Dump "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME LIKE '%opport%' OR TABLE_NAME LIKE '%Opportunity%' ORDER BY TABLE_NAME"

Write-Output "`n=== mCore tables matching type ==="
Dump "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME LIKE 'mCore_%type%' OR TABLE_NAME LIKE 'mCore_%Type%' OR TABLE_NAME LIKE 'mCore_opportunity%' ORDER BY TABLE_NAME"

Write-Output "`n=== link tables ==="
Dump "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME LIKE '%Link%' AND TABLE_TYPE='BASE TABLE' ORDER BY TABLE_NAME"

Write-Output "`n=== salesman on 6882 tags (non-zero) ==="
Dump @"
SELECT t.*
FROM dbo.tCore_Header_0 h
INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId = h.iHeaderId
INNER JOIN dbo.tCore_Data_Tags_0 t ON t.iBodyId = d.iBodyId
WHERE h.sVoucherNo = 'CON-Atl-6882'
"@

$conn.Close()
