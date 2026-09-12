$cs = "Server=localhost;Database=Focus80E0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
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
                    if ($v.Length -gt 180) { $v = $v.Substring(0,180) + "..." }
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
SELECT COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME='tCore_Links_0' ORDER BY ORDINAL_POSITION
"@ "tCore_Links_0 columns"

Dump @"
SELECT TOP 15 l.*
FROM dbo.tCore_Links_0 l
JOIN dbo.tCore_Header_0 h ON h.iHeaderId = l.iHeaderId
WHERE h.iVoucherType IN (4608,4609,4610)
"@ "sample links from receipts (may fail col names)"

Dump @"
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME LIKE 'tCore_Link%' OR TABLE_NAME LIKE 'tCore_Ref%'
ORDER BY TABLE_NAME
"@ "tCore link/ref tables"

Dump @"
SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME='tCore_Header_0'
  AND (COLUMN_NAME LIKE '%Ref%' OR COLUMN_NAME LIKE '%Link%' OR COLUMN_NAME LIKE '%Against%'
       OR COLUMN_NAME LIKE '%Inv%' OR COLUMN_NAME LIKE '%Parent%')
"@ "header ref cols"

Dump @"
SELECT TOP 8
  h.sVoucherNo, h.iVoucherType, hd.QuoteNo, hd.AdvanceReceiptNo, hd.TotalContractAmt, hd.PaymentCode
FROM dbo.tCore_Header_0 h
JOIN dbo.tCore_HeaderData4610_0 hd ON hd.iHeaderId=h.iHeaderId
WHERE ISNULL(hd.QuoteNo,N'') <> N''
  AND h.iVoucherType=4610
ORDER BY h.iHeaderId DESC
"@ "4610 with QuoteNo"

Dump @"
SELECT
  SUM(CASE WHEN ISNULL(hd.QuoteNo,N'')<>N'' THEN 1 ELSE 0 END) AS HasQuote,
  SUM(CASE WHEN ISNULL(hd.AdvanceReceiptNo,N'')<>N'' THEN 1 ELSE 0 END) AS HasAdvNo,
  COUNT(*) AS Hdrs
FROM dbo.tCore_Header_0 h
JOIN dbo.tCore_HeaderData4610_0 hd ON hd.iHeaderId=h.iHeaderId
WHERE h.iVoucherType=4610 AND ISNULL(h.bCancelled,0)=0
"@ "4610 quote/adv fill rate"
