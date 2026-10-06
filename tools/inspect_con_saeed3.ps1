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
                    if ($v.Length -gt 140) { $v = $v.Substring(0,140) + "..." }
                    $parts += $v
                }
            }
            Write-Output ($parts -join " | ")
            if ($c -ge 20) { Write-Output "(truncated)"; break }
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
WHERE c.object_id = OBJECT_ID(N'dbo.tCore_Data5634_0')
ORDER BY c.column_id
"@ "data5634 cols"

Dump @"
SELECT TOP 3 * FROM dbo.tCore_Data5634_0 WHERE iBodyId IN (643452,643453)
"@ "data5634 sample"

Dump @"
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME LIKE N'%5634%' OR TABLE_NAME LIKE N'%Salesman%'
ORDER BY TABLE_NAME
"@ "5634 / salesman tables"

Dump @"
SELECT TOP 15 t.name AS tbl, c.name AS col
FROM sys.columns c
JOIN sys.tables t ON t.object_id = c.object_id
WHERE t.name LIKE N'tCore%' AND (c.name LIKE N'%Salesman%' OR c.name LIKE N'%iEmployee%')
ORDER BY t.name, c.name
"@ "tCore salesman cols"

Dump @"
SELECT hd4610.QuoteNo, hd4610.TotalContractAmt, h.sVoucherNo
FROM dbo.tCore_Header_0 h
JOIN dbo.tCore_HeaderData4610_0 hd4610 ON hd4610.iHeaderId = h.iHeaderId
WHERE h.sVoucherNo IN (N'ATIC-26-1383', N'ATIC-26-1384')
"@ "receipt QuoteNo"

Dump @"
SELECT r.iBodyId, r.iRef, r.iCode, r.iRefType, r.mAmount, r.iAccount
FROM dbo.tCore_Refrn_0 r
WHERE r.iBodyId IN (643452,648079,648078)
   OR r.iRef IN (164378,165821,165822)
"@ "refrn around CON/receipts"
