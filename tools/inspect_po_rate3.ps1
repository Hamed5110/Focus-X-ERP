$cs = "Server=localhost;Database=Focus80G0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
function Run($title, $sql) {
    Write-Output "==== $title ===="
    $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 90; $cmd.CommandText = $sql
    try {
        $r = $cmd.ExecuteReader()
        $n = 0
        while ($r.Read()) {
            $n++
            $parts = @()
            for ($i=0; $i -lt $r.FieldCount; $i++) {
                $v = $r.GetValue($i)
                if ($v -is [DBNull]) { $v = "NULL" }
                $parts += ("{0}={1}" -f $r.GetName($i), $v)
            }
            Write-Output ($parts -join " | ")
            if ($n -ge 40) { Write-Output "... truncated"; break }
        }
        $r.Close()
        Write-Output ("rows_shown=$n")
    } catch {
        Write-Output ("FAIL: " + $_.Exception.Message)
        try { if ($r) { $r.Close() } } catch {}
    }
}

Run "PO class voucher types" @"
SELECT v.iVoucherType, v.sName, v.sAbbr, v.bUpdateInv
FROM dbo.cCore_Vouchers_0 v
WHERE v.iVoucherType BETWEEN 2560 AND 2575
ORDER BY v.iVoucherType
"@

Run "Header class 2560 types in G0" @"
SELECT h.iVoucherType, MAX(v.sName) AS VName, COUNT(*) AS Cnt,
       MIN(h.iDate) AS MinDate, MAX(h.iDate) AS MaxDate
FROM dbo.tCore_Header_0 h
LEFT JOIN dbo.cCore_Vouchers_0 v ON v.iVoucherType = h.iVoucherType
WHERE h.iVoucherClass = 2560
  AND ISNULL(h.bCancelled,0)=0 AND ISNULL(h.bVersion,0)=0
GROUP BY h.iVoucherType
ORDER BY h.iVoucherType
"@

Run "Indta columns" @"
SELECT c.name, t.name AS typ
FROM sys.columns c
JOIN sys.types t ON t.user_type_id = c.user_type_id
WHERE c.object_id = OBJECT_ID('dbo.tCore_Indta_0')
ORDER BY c.column_id
"@

Run "sample PO local/import line" @"
SELECT TOP 8
    h.iVoucherType, v.sName, h.sVoucherNo, h.iDate,
    d.iBookNo, acc.sName AS Vendor,
    i.iProduct, p.sName AS Item, i.mRate, i.fQuantity, d.mAmount1, d.iInvTag
FROM dbo.tCore_Header_0 h
INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId = h.iHeaderId
INNER JOIN dbo.tCore_Indta_0 i ON i.iBodyId = d.iBodyId
LEFT JOIN dbo.cCore_Vouchers_0 v ON v.iVoucherType = h.iVoucherType
LEFT JOIN dbo.mCore_Account acc ON acc.iMasterId = d.iBookNo
LEFT JOIN dbo.mCore_Product p ON p.iMasterId = i.iProduct
WHERE h.iVoucherType IN (2562, 2563)
  AND ISNULL(h.bCancelled,0)=0 AND ISNULL(h.bVersion,0)=0
  AND ISNULL(h.bSuspended,0)=0
ORDER BY h.iDate DESC
"@

$conn.Close()
