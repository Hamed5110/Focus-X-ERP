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
                if ($r.IsDBNull($i)) { $parts += "" } else { $parts += ([string]$r.GetValue($i) -replace "`r|`n"," ") }
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
SELECT t.name AS tbl, c.name AS col
FROM sys.columns c
JOIN sys.tables t ON t.object_id = c.object_id
WHERE c.name LIKE N'%Salesman%' OR c.name LIKE N'Salesman%'
ORDER BY t.name
"@ "all salesman columns"

Dump @"
SELECT TOP 5 * FROM dbo.vaCore_Salesman
"@ "vaCore_Salesman sample"

Dump @"
SELECT c.name FROM sys.columns c WHERE c.object_id = OBJECT_ID(N'dbo.tCore_Docs5634_0')
"@ "docs5634 cols"

Dump @"
SELECT iRef, iRefType, COUNT(*) n, SUM(mAmount) amt
FROM dbo.tCore_Refrn_0
WHERE iBodyId IN (648078,648079)
GROUP BY iRef, iRefType
"@ "receipt refrn summary"

Dump @"
SELECT h.iHeaderId, h.sVoucherNo, h.iVoucherType
FROM dbo.tCore_Header_0 h
WHERE h.iHeaderId IN (72175,72176)
   OR EXISTS (SELECT 1 FROM dbo.tCore_Data_0 d WHERE d.iHeaderId=h.iHeaderId AND d.iBodyId IN (72175,72176))
"@ "what is iRef 72175"
