$cs = "Server=localhost;Database=Focus80G0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
function Dump($sql, $title) {
    Write-Output ""
    Write-Output "========== $title =========="
    $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 90; $cmd.CommandText = $sql
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
                    if ($v.Length -gt 120) { $v = $v.Substring(0,120) + "..." }
                    $parts += $v
                }
            }
            Write-Output ($parts -join " | ")
            if ($c -ge 30) { Write-Output "(truncated)"; break }
        }
        $r.Close()
        if ($c -eq 0) { Write-Output "(no rows)" }
    } catch {
        Write-Output ("ERROR: " + $_.Exception.Message)
        try { $r.Close() } catch {}
    }
}

Dump @"
SELECT iVoucherType, sName, sAbbr FROM dbo.cCore_vouchers_0
WHERE iVoucherType BETWEEN 2560 AND 2570 OR iVoucherType IN (2560,2561,2562,2563,2564)
ORDER BY iVoucherType
"@ "PO voucher types"

Dump @"
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME LIKE N'tCore_HeaderData256%' OR TABLE_NAME LIKE N'tCore_Data256%'
ORDER BY TABLE_NAME
"@ "PO extra tables"

Dump @"
SELECT TOP 8 h.sVoucherNo, h.iVoucherType, h.fNet, hd.sNarration, hd.ItemQty, hd.GlassQty, hd.GlassClassification, hd.DeliveryDate, hd.OrderInsatllationDate
FROM dbo.tCore_Header_0 h
LEFT JOIN dbo.tCore_HeaderData2562_0 hd ON hd.iHeaderId = h.iHeaderId
WHERE h.iVoucherClass = 2560 AND ISNULL(h.bCancelled,0)=0 AND ISNULL(h.bVersion,0)=0
  AND ISNULL(hd.sNarration,N'') <> N''
"@ "sample narration"

Dump @"
SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = N'tCore_Header_0' AND COLUMN_NAME LIKE N'%Narr%'
"@ "header narration"

Dump @"
SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME LIKE N'tCore_HeaderData256%' AND COLUMN_NAME LIKE N'%Narr%'
"@ "PO header narr columns"
