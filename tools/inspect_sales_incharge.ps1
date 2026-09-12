$cs = "Server=localhost;Database=Focus80E0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
function Dump($sql, $title) {
    Write-Output "`n=== $title ==="
    $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 120; $cmd.CommandText = $sql
    try {
        $r = $cmd.ExecuteReader()
        $hdr = @(); for ($i=0;$i -lt $r.FieldCount;$i++) { $hdr += $r.GetName($i) }
        Write-Output ("COLS: " + ($hdr -join " | "))
        $c = 0
        while ($r.Read()) {
            $parts = @(); for ($i=0;$i -lt $r.FieldCount;$i++) {
                if ($r.IsDBNull($i)) { $parts += "" } else { $parts += [string]$r.GetValue($i) }
            }
            Write-Output ($parts -join " | ")
            $c++; if ($c -ge 25) { break }
        }
        $r.Close(); Write-Output "-- $c --"
    } catch { Write-Output $_.Exception.Message; if ($r) { try { $r.Close() } catch {} } }
}
Dump "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='tCore_HeaderData5634_0' AND COLUMN_NAME LIKE '%Sales%' OR (TABLE_NAME='tCore_HeaderData5634_0' AND COLUMN_NAME LIKE '%Charge%') ORDER BY COLUMN_NAME" "5634 sales cols"
Dump "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='tCore_HeaderData4610_0' ORDER BY COLUMN_NAME" "4610 all cols"
Dump @"
SELECT TOP 10 h.iHeaderId, hd.SalesInCharge, sm.sName
FROM dbo.tCore_HeaderData5634_0 hd
JOIN dbo.tCore_Header_0 h ON h.iHeaderId = hd.iHeaderId
LEFT JOIN dbo.mCore_Salesman sm ON sm.iMasterId = hd.SalesInCharge
WHERE ISNULL(hd.SalesInCharge,0) > 0
"@ "5634 SalesInCharge sample"
$conn.Close()
