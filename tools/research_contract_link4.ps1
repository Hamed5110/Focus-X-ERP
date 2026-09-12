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
                    if ($v.Length -gt 200) { $v = $v.Substring(0,200) + "..." }
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
SELECT h.iHeaderId, h.sVoucherNo, d.iBodyId, d.iCode, d.mAmount1, hd.PaymentCode, hd.TotalContractAmt
FROM dbo.tCore_Header_0 h
JOIN dbo.tCore_Data_0 d ON d.iHeaderId=h.iHeaderId AND d.bUpdateFA=1 AND d.iCode>0
LEFT JOIN dbo.tCore_HeaderData4610_0 hd ON hd.iHeaderId=h.iHeaderId
WHERE h.sVoucherNo IN (N'ATIC-26-1372', N'ATIC-26-1264', N'RV-26-00156')
"@ "sample receipt bodies"

Dump @"
SELECT r.*, h.sVoucherNo, h.iVoucherType
FROM dbo.tCore_Refrn_0 r
JOIN dbo.tCore_Data_0 d ON d.iBodyId=r.iBodyId
JOIN dbo.tCore_Header_0 h ON h.iHeaderId=d.iHeaderId
WHERE h.sVoucherNo IN (N'ATIC-26-1372', N'ATIC-26-1264', N'RV-26-00156')
"@ "refrn on those receipts"

Dump @"
SELECT
  SUM(CASE WHEN r.iBodyId IS NULL THEN 0 ELSE 1 END) AS ReceiptLinesWithRef,
  COUNT(*) AS ReceiptLines
FROM dbo.tCore_Header_0 h
JOIN dbo.tCore_Data_0 d ON d.iHeaderId=h.iHeaderId AND d.bUpdateFA=1 AND d.iCode>0 AND d.iFaTag IN (2040,2057)
LEFT JOIN dbo.tCore_Refrn_0 r ON r.iBodyId=d.iBodyId
WHERE h.iVoucherType IN (4608,4609,4610)
  AND ISNULL(h.bCancelled,0)=0 AND ISNULL(h.bVersion,0)=0
"@ "how many receipt lines have refrn"

Dump @"
SELECT TOP 8
  hr.sVoucherNo AS RctNo, hr.iVoucherType AS RctType,
  hs.sVoucherNo AS RefDoc, hs.iVoucherType AS RefType,
  r.iRef, r.iRefType, r.mAmount
FROM dbo.tCore_Header_0 hr
JOIN dbo.tCore_Data_0 dr ON dr.iHeaderId=hr.iHeaderId AND dr.bUpdateFA=1
JOIN dbo.tCore_Refrn_0 r ON r.iBodyId=dr.iBodyId
LEFT JOIN dbo.tCore_Data_0 ds ON ds.iBodyId = r.iRef
LEFT JOIN dbo.tCore_Header_0 hs ON hs.iHeaderId=ds.iHeaderId
WHERE hr.iVoucherType IN (4608,4609,4610) AND dr.iFaTag IN (2040,2057)
"@ "refrn iRef as bodyId?"

Dump @"
SELECT TOP 8
  hr.sVoucherNo AS RctNo,
  hs.sVoucherNo AS RefHdr, hs.iVoucherType AS RefType,
  r.iRef
FROM dbo.tCore_Header_0 hr
JOIN dbo.tCore_Data_0 dr ON dr.iHeaderId=hr.iHeaderId AND dr.bUpdateFA=1
JOIN dbo.tCore_Refrn_0 r ON r.iBodyId=dr.iBodyId
LEFT JOIN dbo.tCore_Header_0 hs ON hs.iHeaderId = r.iRef
WHERE hr.iVoucherType IN (4608,4609,4610) AND dr.iFaTag IN (2040,2057)
"@ "refrn iRef as headerId?"
