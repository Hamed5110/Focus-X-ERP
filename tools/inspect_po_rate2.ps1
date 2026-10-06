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
            if ($n -ge 50) { Write-Output "... truncated"; break }
        }
        $r.Close()
        Write-Output ("rows_shown=$n")
    } catch {
        Write-Output ("FAIL: " + $_.Exception.Message)
        try { if ($r) { $r.Close() } } catch {}
    }
}

Run "cCore_Vouchers_0 columns" @"
SELECT c.name, t.name AS typ
FROM sys.columns c
JOIN sys.types t ON t.user_type_id = c.user_type_id
WHERE c.object_id = OBJECT_ID('dbo.cCore_Vouchers_0')
ORDER BY c.column_id
"@

Run "PO voucher names" @"
SELECT TOP 80 *
FROM dbo.cCore_Vouchers_0
WHERE sName LIKE N'%Purchase%'
   OR sName LIKE N'%PO %'
   OR sName LIKE N'%Import%'
   OR sName LIKE N'%Local%'
"@

Run "tCore tables with Rate" @"
SELECT t.name AS tbl, c.name AS col
FROM sys.tables t
JOIN sys.columns c ON c.object_id = t.object_id
WHERE t.name LIKE 'tCore_%'
  AND (c.name LIKE '%Rate%' OR c.name LIKE '%fQty%' OR c.name = 'iProduct' OR c.name = 'iInvTag')
ORDER BY t.name, c.column_id
"@

Run "header columns class" @"
SELECT c.name
FROM sys.columns c
WHERE c.object_id = OBJECT_ID('dbo.tCore_Header_0')
  AND (c.name LIKE '%Class%' OR c.name LIKE '%Voucher%' OR c.name LIKE '%Type%')
ORDER BY c.column_id
"@

$conn.Close()
