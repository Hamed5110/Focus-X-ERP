$cs = "Server=localhost;Database=Focus80G0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
function Dump($sql, $title) {
    Write-Output ""
    Write-Output "========== $title =========="
    $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 180; $cmd.CommandText = $sql
    try {
        $r = $cmd.ExecuteReader()
        $n = $r.FieldCount
        $hdr = @(); for ($i=0; $i -lt $n; $i++) { $hdr += $r.GetName($i) }
        Write-Output ($hdr -join " | ")
        $c = 0
        while ($r.Read()) {
            $c++
            $parts = @()
            for ($i=0; $i -lt $n; $i++) {
                if ($r.IsDBNull($i)) { $parts += "" } else {
                    $v = [string]$r.GetValue($i)
                    $v = $v -replace "`r|`n"," "
                    if ($v.Length -gt 160) { $v = $v.Substring(0,160) + "..." }
                    $parts += $v
                }
            }
            Write-Output ($parts -join " | ")
            if ($c -ge 40) { Write-Output "(truncated)"; break }
        }
        $r.Close()
        if ($c -eq 0) { Write-Output "(no rows)" }
    } catch {
        Write-Output ("ERROR: " + $_.Exception.Message)
        try { if ($r) { $r.Close() } } catch {}
    }
}

Dump @"
SELECT c.name FROM sys.columns c
WHERE c.object_id = OBJECT_ID(N'dbo.tCore_Header_0')
  AND (c.name LIKE N'%Sales%' OR c.name LIKE N'%Emp%' OR c.name LIKE N'%User%'
       OR c.name LIKE N'%iInv%' OR c.name LIKE N'%iOther%')
ORDER BY c.name
"@ "header salesman-like"

Dump @"
SELECT h.iHeaderId, h.sVoucherNo, h.fNet, h.iFaTag
FROM dbo.tCore_Header_0 h
WHERE h.sVoucherNo = N'CON-Atl-6955'
"@ "CON header"

Dump @"
SELECT c.name FROM sys.columns c
WHERE c.object_id = OBJECT_ID(N'dbo.tCore_Header_0')
ORDER BY c.column_id
"@ "all header cols"

Dump @"
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME LIKE N'%Refrn%' OR TABLE_NAME LIKE N'%Ref%' AND TABLE_NAME LIKE N'tCore%'
ORDER BY TABLE_NAME
"@ "ref tables"
