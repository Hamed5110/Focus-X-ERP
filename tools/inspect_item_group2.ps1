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

Run "muCore_Product columns" @"
SELECT c.name, t.name AS typ
FROM sys.columns c JOIN sys.types t ON t.user_type_id = c.user_type_id
WHERE c.object_id = OBJECT_ID('dbo.muCore_Product')
ORDER BY c.column_id
"@

Run "vaCore_Product exists" @"
SELECT OBJECT_ID('dbo.vaCore_Product') AS vid, OBJECT_ID('dbo.vCore_Product') AS v2
"@

Run "product tree details cols" @"
SELECT c.name FROM sys.columns c WHERE c.object_id = OBJECT_ID('dbo.mCore_ProductTreeDetails') ORDER BY c.column_id
"@

Run "product groups in default tree" @"
SELECT TOP 20 p.iMasterId, p.sCode, p.sName, p.bGroup
FROM dbo.mCore_Product p
WHERE p.bGroup = 1
ORDER BY p.sName
"@

Run "parent of Wacker items" @"
SELECT p.iMasterId, p.sCode, p.sName, td.iParentId, par.sName AS ParentName, par.bGroup
FROM dbo.mCore_Product p
LEFT JOIN dbo.mCore_ProductTreeDetails td ON td.iMasterId = p.iMasterId AND td.iTreeId = 0
LEFT JOIN dbo.mCore_Product par ON par.iMasterId = td.iParentId
WHERE p.iMasterId IN (4646, 9714, 456, 2428, 4251)
"@

Run "PO body tags columns" @"
SELECT c.name FROM sys.columns c
WHERE c.object_id = OBJECT_ID('dbo.tCore_Data_Tags_0')
  AND (c.name LIKE 'iTag%' OR c.name LIKE '%Group%' OR c.name LIKE '%Product%')
ORDER BY c.column_id
"@

Run "maingroup sample" @"
SELECT TOP 15 iMasterId, sCode, sName FROM dbo.mCore_maingroup ORDER BY sName
"@

$conn.Close()
