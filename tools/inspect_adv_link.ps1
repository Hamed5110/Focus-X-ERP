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

Write-Output "=== HeaderData4610 columns ==="
Dump "SELECT COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='tCore_HeaderData4610_0' ORDER BY ORDINAL_POSITION"

Write-Output "`n=== ATIC-26-1264 header extra ==="
Dump @"
SELECT h.iHeaderId, h.sVoucherNo, h.iDate, h.iVoucherType, hd.*
FROM dbo.tCore_Header_0 h
INNER JOIN dbo.tCore_HeaderData4610_0 hd ON hd.iHeaderId=h.iHeaderId
WHERE h.sVoucherNo='ATIC-26-1264'
"@

Write-Output "`n=== ATIC body ==="
Dump @"
SELECT d.iBodyId, d.iCode, d.iBookNo, d.iFaTag, d.mAmount1, d.mAmount2, d.bUpdateFA
FROM dbo.tCore_Header_0 h
INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId=h.iHeaderId
WHERE h.sVoucherNo='ATIC-26-1264'
"@

Write-Output "`n=== opportunity type distinct values on SO ==="
Dump @"
SELECT hd.OpportunityType, COUNT(*) Cnt
FROM dbo.tCore_HeaderData5634_0 hd
GROUP BY hd.OpportunityType
ORDER BY hd.OpportunityType
"@

Write-Output "`n=== extra field definition tables ==="
Dump @"
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME LIKE '%Field%' AND (TABLE_NAME LIKE 'cCore%' OR TABLE_NAME LIKE 'mCore%')
ORDER BY TABLE_NAME
"@

Write-Output "`n=== mCore names containing business/adjust/free ==="
Dump @"
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME LIKE 'mCore_%' AND (
    TABLE_NAME LIKE '%business%' OR TABLE_NAME LIKE '%adjust%' OR TABLE_NAME LIKE '%free%'
    OR TABLE_NAME LIKE '%size%' OR TABLE_NAME LIKE '%variation%' OR TABLE_NAME LIKE '%opport%'
    OR TABLE_NAME LIKE '%salesorder%' OR TABLE_NAME LIKE '%SalesOrder%'
)
ORDER BY TABLE_NAME
"@

$conn.Close()
