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
            if ($n -ge 25) { Write-Output "... truncated"; break }
        }
        $r.Close()
        Write-Output ("rows_shown=$n")
    } catch {
        Write-Output ("FAIL: " + $_.Exception.Message)
        try { if ($r) { $r.Close() } } catch {}
    }
}

Run "vaCore_Product group-like cols" @"
SELECT c.name FROM sys.columns c
WHERE c.object_id = OBJECT_ID('dbo.vaCore_Product')
  AND (c.name LIKE '%Group%' OR c.name LIKE '%Cat%' OR c.name LIKE '%Type%' OR c.name LIKE '%Make%' OR c.name LIKE '%Dept%' OR c.name LIKE '%Parent%')
ORDER BY c.name
"@

Run "Wacker extras" @"
SELECT p.iMasterId, p.sName, u.iCategory, u.Catagory, u.MainGroup, u.SubGroup, u.Department, u.iProductMake, u.ProductMake
FROM dbo.mCore_Product p
LEFT JOIN dbo.muCore_Product u ON u.iMasterId = p.iMasterId
WHERE p.iMasterId IN (4646, 9714, 456, 2428, 4251)
"@

Run "category master tables" @"
SELECT name FROM sys.tables WHERE name LIKE '%categor%' OR name LIKE '%Catagor%' OR name LIKE 'mCore_iCategory%' OR name LIKE '%productmake%'
ORDER BY name
"@

Run "MainGroup distinct on products" @"
SELECT TOP 20 u.MainGroup, ISNULL(mg.sName, CAST(u.MainGroup AS varchar(20))) AS Nm, COUNT(*) Cnt
FROM dbo.muCore_Product u
LEFT JOIN dbo.mCore_maingroup mg ON mg.iMasterId = u.MainGroup
GROUP BY u.MainGroup, mg.sName
ORDER BY Cnt DESC
"@

Run "Catagory distinct" @"
SELECT TOP 20 u.Catagory, COUNT(*) Cnt
FROM dbo.muCore_Product u
GROUP BY u.Catagory
ORDER BY Cnt DESC
"@

Run "iCategory distinct" @"
SELECT TOP 20 u.iCategory, COUNT(*) Cnt
FROM dbo.muCore_Product u
GROUP BY u.iCategory
ORDER BY Cnt DESC
"@

$conn.Close()
