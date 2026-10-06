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
        try { if ($r) { $r.Close() } } catch {}
    }
}

Run "mCore_Catagory all" @"
SELECT iMasterId, sCode, sName FROM dbo.mCore_Catagory WHERE iMasterId > 0 ORDER BY sName
"@

Run "product tree parents used on PO items" @"
SELECT TOP 20 par.sName AS ItemGroup, COUNT(*) Cnt
FROM dbo.tCore_Header_0 h
INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId = h.iHeaderId
INNER JOIN dbo.tCore_Indta_0 i ON i.iBodyId = d.iBodyId
INNER JOIN dbo.mCore_ProductTreeDetails td ON td.iMasterId = i.iProduct AND td.iTreeId = 0
INNER JOIN dbo.mCore_Product par ON par.iMasterId = td.iParentId
WHERE h.iVoucherType IN (2562, 2563)
  AND ISNULL(h.bCancelled,0)=0
GROUP BY par.sName
ORDER BY Cnt DESC
"@

Run "CatagoryName on PO items" @"
SELECT TOP 20 ISNULL(cat.sName, N'(blank)') AS Cat, COUNT(*) Cnt
FROM dbo.tCore_Header_0 h
INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId = h.iHeaderId
INNER JOIN dbo.tCore_Indta_0 i ON i.iBodyId = d.iBodyId
LEFT JOIN dbo.muCore_Product u ON u.iMasterId = i.iProduct
LEFT JOIN dbo.mCore_Catagory cat ON cat.iMasterId = u.Catagory
WHERE h.iVoucherType IN (2562, 2563)
  AND ISNULL(h.bCancelled,0)=0
GROUP BY ISNULL(cat.sName, N'(blank)')
ORDER BY Cnt DESC
"@

Run "dept on PO" @"
SELECT TOP 10 dep.sName, COUNT(*) Cnt
FROM dbo.tCore_Header_0 h
INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId = h.iHeaderId
INNER JOIN dbo.mCore_Department dep ON dep.iMasterId = d.iFaTag
WHERE h.iVoucherType IN (2562, 2563)
  AND ISNULL(h.bCancelled,0)=0
GROUP BY dep.sName
ORDER BY Cnt DESC
"@

$conn.Close()
