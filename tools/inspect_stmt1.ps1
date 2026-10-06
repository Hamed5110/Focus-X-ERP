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
            if ($n -ge 30) { Write-Output "... truncated"; break }
        }
        $r.Close()
        Write-Output ("rows_shown=$n")
    } catch {
        Write-Output ("FAIL: " + $_.Exception.Message)
        try { if ($r) { $r.Close() } } catch {}
    }
}

Run "5634 header extra cols" @"
SELECT c.name, t.name AS typ
FROM sys.columns c JOIN sys.types t ON t.user_type_id = c.user_type_id
WHERE c.object_id = OBJECT_ID('dbo.tCore_HeaderData5634_0')
ORDER BY c.column_id
"@

Run "opportunity tables" @"
SELECT name FROM sys.tables
WHERE name LIKE '%Opport%' OR name LIKE '%opport%' OR name LIKE '%Opportunity%'
ORDER BY name
"@

Run "cols with Opportunity" @"
SELECT t.name AS tbl, c.name AS col
FROM sys.tables t
JOIN sys.columns c ON c.object_id = t.object_id
WHERE c.name LIKE '%Opport%' OR c.name LIKE '%Opportunity%'
ORDER BY t.name, c.name
"@

Run "bill adj / ledger tables" @"
SELECT name FROM sys.tables
WHERE name LIKE '%Bill%' OR name LIKE '%Ledger%' OR name LIKE '%Adj%' OR name LIKE '%Refrn%'
   OR name LIKE 'tCore_AR%' OR name LIKE 'tCore_Outstanding%'
ORDER BY name
"@

Run "tCore_Refrn_0 cols" @"
SELECT c.name, ty.name AS typ
FROM sys.columns c JOIN sys.types ty ON ty.user_type_id = c.user_type_id
WHERE c.object_id = OBJECT_ID('dbo.tCore_Refrn_0')
ORDER BY c.column_id
"@

$conn.Close()
