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
            if ($n -ge 25) { Write-Output "... truncated"; break }
        }
        $r.Close()
        Write-Output ("rows_shown=$n")
    } catch {
        Write-Output ("FAIL: " + $_.Exception.Message)
        try { if ($r) { $r.Close() } } catch {}
    }
}

Run "product columns group-like" @"
SELECT c.name
FROM sys.columns c
WHERE c.object_id = OBJECT_ID('dbo.mCore_Product')
  AND (c.name LIKE '%Group%' OR c.name LIKE '%Cat%' OR c.name LIKE '%Type%' OR c.name LIKE '%Class%' OR c.name LIKE '%iParent%')
ORDER BY c.column_id
"@

Run "product extra tables" @"
SELECT name FROM sys.tables
WHERE name LIKE '%Product%' AND (name LIKE 'mu%' OR name LIKE 'va%' OR name LIKE '%Group%' OR name LIKE '%Tree%')
ORDER BY name
"@

Run "item group masters" @"
SELECT name FROM sys.tables
WHERE name LIKE '%itemgroup%' OR name LIKE '%ItemGroup%' OR name LIKE '%maingroup%'
   OR name LIKE '%productgroup%' OR name LIKE 'mCore_%group%'
ORDER BY name
"@

Run "Wacker product" @"
SELECT TOP 8 p.iMasterId, p.sCode, p.sName
FROM dbo.mCore_Product p
WHERE p.sName LIKE N'%Wacker%' OR p.sCode LIKE N'%Wacker%'
   OR p.sName LIKE N'%GP Silicone%' OR p.sName LIKE N'%Silglaze%'
"@

Run "product tree sample" @"
SELECT TOP 8 * FROM dbo.mCore_ProductTree
"@

$conn.Close()
