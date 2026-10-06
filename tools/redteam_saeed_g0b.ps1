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
                    if ($v.Length -gt 180) { $v = $v.Substring(0,180) + "..." }
                    $parts += $v
                }
            }
            Write-Output ($parts -join " | ")
        }
        $r.Close()
        if ($c -eq 0) { Write-Output "(no rows)" }
    } catch {
        Write-Output ("ERROR: " + $_.Exception.Message)
        try { if ($r) { $r.Close() } } catch {}
    }
}

Dump @"
SELECT
  h.iHeaderId, h.iVoucherType, h.sVoucherNo,
  h.iDate AS Packed,
  CAST((h.iDate & 0xfff0000)/65536 AS varchar(4)) + N'-'
    + RIGHT(N'0'+CAST((h.iDate & 0xff00)/256 AS varchar(2)),2) + N'-'
    + RIGHT(N'0'+CAST((h.iDate & 0xff) AS varchar(2)),2) AS VoucherDate,
  h.iCreatedDate, h.iModifiedDate,
  ISNULL(h.iAuth,1) iAuth, ISNULL(h.bCancelled,0) Canc,
  ISNULL(h.bVersion,0) Ver, ISNULL(h.bSuspended,0) Susp,
  h.fNet,
  d.iBodyId, d.iCode, d.iBookNo, d.iFaTag, d.bUpdateFA, d.iType, d.bVoid, d.iAuthStatus,
  d.iInvTag, d.mAmount1, d.mAmount2,
  COALESCE(NULLIF(hd4610.PaymentCode,0), NULLIF(hd4609.PaymentCode,0), NULLIF(hd4608.PaymentCode,0), 0) AS Pay,
  hd4610.TotalContractAmt AS C4610,
  hd4609.ContractAmount AS C4609,
  hd4608.PaymentCode AS P4608
FROM dbo.tCore_Header_0 h
INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId = h.iHeaderId
LEFT JOIN dbo.tCore_HeaderData4610_0 hd4610 ON hd4610.iHeaderId = h.iHeaderId
LEFT JOIN dbo.tCore_HeaderData4609_0 hd4609 ON hd4609.iHeaderId = h.iHeaderId
LEFT JOIN dbo.tCore_HeaderData4608_0 hd4608 ON hd4608.iHeaderId = h.iHeaderId
WHERE h.iVoucherType IN (4608,4609,4610)
  AND (d.iCode = 21061 OR h.sVoucherNo LIKE N'%ATIC-26-1383%' OR h.sVoucherNo LIKE N'%1383%')
ORDER BY h.iDate, h.iHeaderId, d.iBodyId
"@ "all receipts 4608/09/10 for cust 21061 or ATIC-26-1383"

Dump @"
SELECT
  h.iHeaderId, h.iVoucherType, h.sVoucherNo, h.iDate,
  CAST((h.iDate & 0xfff0000)/65536 AS varchar(4)) + N'-'
    + RIGHT(N'0'+CAST((h.iDate & 0xff00)/256 AS varchar(2)),2) + N'-'
    + RIGHT(N'0'+CAST((h.iDate & 0xff) AS varchar(2)),2) AS VoucherDate,
  ISNULL(h.iAuth,1) iAuth, ISNULL(h.bCancelled,0) Canc, ISNULL(h.bVersion,0) Ver,
  h.fNet,
  d.iCode, d.iBookNo, d.iFaTag, d.mAmount1, acct.sName, acct.sCode
FROM dbo.tCore_Header_0 h
INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId = h.iHeaderId
LEFT JOIN dbo.mCore_Account acct ON acct.iMasterId = d.iCode
WHERE h.sVoucherNo LIKE N'%ATIC-26-1383%' OR h.sVoucherNo LIKE N'%NDT97%'
ORDER BY h.iVoucherType, h.iHeaderId, d.iBodyId
"@ "ATIC-26-1383 / NDT97 any type"

Dump @"
SELECT
  h.iHeaderId, h.iVoucherType, h.sVoucherNo, h.iDate,
  CAST((h.iDate & 0xfff0000)/65536 AS varchar(4)) + N'-'
    + RIGHT(N'0'+CAST((h.iDate & 0xff00)/256 AS varchar(2)),2) + N'-'
    + RIGHT(N'0'+CAST((h.iDate & 0xff) AS varchar(2)),2) AS VoucherDate,
  ISNULL(h.iAuth,1) iAuth, ISNULL(h.bCancelled,0) Canc, ISNULL(h.bVersion,0) Ver, ISNULL(h.bSuspended,0) Susp,
  h.fNet,
  d.iCode, d.iBookNo, d.iFaTag, d.mAmount1,
  hd.ContractValue, hd.RefNo
FROM dbo.tCore_Header_0 h
INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId = h.iHeaderId
LEFT JOIN dbo.tCore_HeaderData5634_0 hd ON hd.iHeaderId = h.iHeaderId
WHERE h.iVoucherType = 5634
  AND (h.sVoucherNo LIKE N'CON-Atl-69555%' OR d.iBookNo = 21061 OR d.iCode = 21061)
ORDER BY h.iDate, h.iHeaderId, d.iBodyId
"@ "CON-Atl for this customer"
