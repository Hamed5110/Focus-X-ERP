$cs = "Server=localhost;Database=Focus80G0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
function Run($title, $sql) {
    Write-Output "==== $title ===="
    $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 60; $cmd.CommandText = $sql
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
        try { $r.Close() } catch {}
    }
}

Run "PO voucher types" @"
SELECT v.iVoucherType, v.sName, v.iVoucherClass
FROM dbo.cCore_Vouchers_0 v
WHERE v.sName LIKE N'%Purchase Order%'
   OR v.sName LIKE N'%PO %'
   OR v.iVoucherClass = 2560
   OR v.iVoucherType IN (2560, 2561, 2562, 2563, 2564)
ORDER BY v.iVoucherType
"@

Run "tCore_Data_0 rate-like columns" @"
SELECT c.name, t.name AS typ
FROM sys.columns c
JOIN sys.types t ON t.user_type_id = c.user_type_id
WHERE c.object_id = OBJECT_ID('dbo.tCore_Data_0')
  AND (c.name LIKE '%Rate%' OR c.name LIKE '%Qty%' OR c.name LIKE '%Product%'
       OR c.name IN ('iProduct','fQuantity','fRate','mRate','iInvTag','iCode','iBookNo','mAmount1'))
ORDER BY c.column_id
"@

Run "item master tables" @"
SELECT name FROM sys.tables
WHERE name LIKE '%Product%' OR name LIKE '%Item%' OR name LIKE '%InvItem%'
ORDER BY name
"@

$conn.Close()
