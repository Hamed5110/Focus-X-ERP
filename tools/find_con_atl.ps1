$cs = "Server=localhost;Database=Focus80E0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
function Dump($sql) {
    $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 180; $cmd.CommandText = $sql
    $r = $cmd.ExecuteReader()
    $n = $r.FieldCount
    $c = 0
    while ($r.Read()) {
        $parts = @()
        for ($i = 0; $i -lt $n; $i++) {
            if ($r.IsDBNull($i)) { $parts += "" } else { $parts += [string]$r.GetValue($i) }
        }
        Write-Output ($parts -join " | ")
        $c++
    }
    $r.Close()
    Write-Output ("-- rows: $c --")
}

Write-Output "=== CON-Atl-6882 in header ==="
Dump @"
SELECT h.iHeaderId, h.iVoucherType, h.sVoucherNo, h.iDate, h.fNet, h.iAuth, h.bCancelled
FROM dbo.tCore_Header_0 h
WHERE h.sVoucherNo LIKE 'CON-Atl-6882%' OR h.sVoucherNo LIKE 'CON-Atl-6882'
"@

Write-Output "`n=== voucher nos like CON- ==="
Dump @"
SELECT TOP 15 h.iVoucherType, COUNT(*) Cnt, MIN(h.sVoucherNo) SampleNo
FROM dbo.tCore_Header_0 h
WHERE h.sVoucherNo LIKE 'CON-%'
GROUP BY h.iVoucherType
ORDER BY COUNT(*) DESC
"@

Write-Output "`n=== tables with Opportunity / Contract / CRM ==="
Dump @"
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME LIKE '%Opport%'
   OR TABLE_NAME LIKE '%CRM%'
   OR TABLE_NAME LIKE '%Lead%'
   OR TABLE_NAME LIKE '%SalesOrder%'
   OR TABLE_NAME LIKE '%Contract%'
ORDER BY TABLE_NAME
"@

Write-Output "`n=== voucher type master ==="
Dump @"
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME LIKE '%VoucherType%' OR TABLE_NAME LIKE '%vouchertype%'
ORDER BY TABLE_NAME
"@

$conn.Close()
