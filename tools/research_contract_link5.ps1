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
            if ($c -ge 45) { Write-Output "(truncated)"; break }
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
  ISNULL(hs.iVoucherType, -1) AS RefVoucher,
  COUNT(DISTINCT hr.iHeaderId) AS RctHdrs,
  CAST(SUM(CASE WHEN dr.mAmount1>0 THEN dr.mAmount1 ELSE 0 END) AS decimal(18,2)) AS Coll
FROM dbo.tCore_Header_0 hr
JOIN dbo.tCore_Data_0 dr ON dr.iHeaderId=hr.iHeaderId AND dr.bUpdateFA=1 AND dr.iCode>0 AND dr.iFaTag IN (2040,2057)
  AND ISNULL(dr.iType,0)=0 AND ISNULL(dr.bVoid,0)=0
LEFT JOIN dbo.tCore_Refrn_0 r ON r.iBodyId=dr.iBodyId
LEFT JOIN dbo.tCore_Data_0 ds ON ds.iBodyId=r.iRef
LEFT JOIN dbo.tCore_Header_0 hs ON hs.iHeaderId=ds.iHeaderId
WHERE hr.iVoucherType IN (4608,4609,4610)
  AND ISNULL(hr.iAuth,1)=1 AND ISNULL(hr.bCancelled,0)=0 AND ISNULL(hr.bVersion,0)=0 AND ISNULL(hr.bSuspended,0)=0
  AND hr.iDate>0
GROUP BY ISNULL(hs.iVoucherType, -1)
ORDER BY 2 DESC
"@ "receipt refs by target voucher type"

Dump @"
SELECT COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME='tCore_HeaderData5634_0'
  AND COLUMN_NAME IN ('ContractValue','ContractStatus','iHeaderId')
"@ "5634 contract value col"

Dump @"
;WITH lnk AS (
  SELECT
    hr.iHeaderId AS RctId,
    hr.sVoucherNo,
    MAX(hs.iHeaderId) AS SOId,
    MAX(hs.sVoucherNo) AS SONo,
    MAX(CAST(ISNULL(so.ContractValue,0) AS decimal(18,4))) AS SOVal,
    MAX(CAST(ISNULL(hd.TotalContractAmt,0) AS decimal(18,4))) AS RctVal,
    CAST(SUM(CASE WHEN dr.mAmount1>0 THEN dr.mAmount1 ELSE 0 END) AS decimal(18,4)) AS ThisRct
  FROM dbo.tCore_Header_0 hr
  JOIN dbo.tCore_Data_0 dr ON dr.iHeaderId=hr.iHeaderId AND dr.bUpdateFA=1 AND dr.iCode>0 AND dr.iFaTag IN (2040,2057)
  LEFT JOIN dbo.tCore_HeaderData4610_0 hd ON hd.iHeaderId=hr.iHeaderId
  LEFT JOIN dbo.tCore_Refrn_0 r ON r.iBodyId=dr.iBodyId
  LEFT JOIN dbo.tCore_Data_0 ds ON ds.iBodyId=r.iRef
  LEFT JOIN dbo.tCore_Header_0 hs ON hs.iHeaderId=ds.iHeaderId AND hs.iVoucherType=5634
  LEFT JOIN dbo.tCore_HeaderData5634_0 so ON so.iHeaderId=hs.iHeaderId
  WHERE hr.iVoucherType IN (4608,4609,4610)
    AND ISNULL(hr.iAuth,1)=1 AND ISNULL(hr.bCancelled,0)=0 AND ISNULL(hr.bVersion,0)=0
    AND hr.iDate>0 AND COALESCE(NULLIF(hd.PaymentCode,0),0) IN (1,4)
    AND (hr.iDate & 0xfff0000)/65536=2026 AND (hr.iDate & 0xff00)/256 IN (7,8)
  GROUP BY hr.iHeaderId, hr.sVoucherNo
)
SELECT
  SUM(CASE WHEN SOId IS NOT NULL THEN 1 ELSE 0 END) AS FirstPay_with_SO,
  SUM(CASE WHEN SOId IS NULL THEN 1 ELSE 0 END) AS FirstPay_no_SO,
  SUM(CASE WHEN SOVal>0 THEN 1 ELSE 0 END) AS HasSOVal,
  SUM(CASE WHEN RctVal>0 THEN 1 ELSE 0 END) AS HasRctVal,
  CAST(SUM(ThisRct) AS decimal(18,2)) AS FirstPayColl
FROM lnk
"@ "Jul-Aug 2026 first-pay SO link rate"

Dump @"
SELECT TOP 10
  hr.sVoucherNo AS Rct, hs.sVoucherNo AS SO, hd.PaymentCode,
  CAST(hd.TotalContractAmt AS decimal(18,2)) AS RctContract,
  CAST(so.ContractValue AS decimal(18,2)) AS SOContract,
  CAST(SUM(CASE WHEN dr.mAmount1>0 THEN dr.mAmount1 ELSE 0 END) AS decimal(18,2)) AS ThisRct
FROM dbo.tCore_Header_0 hr
JOIN dbo.tCore_Data_0 dr ON dr.iHeaderId=hr.iHeaderId AND dr.bUpdateFA=1 AND dr.iCode>0
JOIN dbo.tCore_HeaderData4610_0 hd ON hd.iHeaderId=hr.iHeaderId
JOIN dbo.tCore_Refrn_0 r ON r.iBodyId=dr.iBodyId
JOIN dbo.tCore_Data_0 ds ON ds.iBodyId=r.iRef
JOIN dbo.tCore_Header_0 hs ON hs.iHeaderId=ds.iHeaderId AND hs.iVoucherType=5634
JOIN dbo.tCore_HeaderData5634_0 so ON so.iHeaderId=hs.iHeaderId
WHERE hr.sVoucherNo=N'ATIC-26-1264'
GROUP BY hr.sVoucherNo, hs.sVoucherNo, hd.PaymentCode, hd.TotalContractAmt, so.ContractValue
"@ "ATIC-26-1264 linked SO"
