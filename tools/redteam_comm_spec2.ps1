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
            if ($c -ge 50) { Write-Output "(truncated)"; break }
        }
        $r.Close()
        if ($c -eq 0) { Write-Output "(no rows)" }
    } catch {
        Write-Output ("ERROR: " + $_.Exception.Message)
    }
}

Dump @"
SELECT
  h.iVoucherType,
  COALESCE(NULLIF(hd4610.PaymentCode,0), NULLIF(hd4609.PaymentCode,0), NULLIF(hd4608.PaymentCode,0), 0) AS Pay,
  COUNT(DISTINCT h.iHeaderId) AS Hdrs,
  SUM(CASE WHEN ISNULL(hd4610.TotalContractAmt,0)>0 THEN 1 ELSE 0 END) AS LinesWith4610Contract,
  SUM(CASE WHEN ISNULL(hd4609.ContractAmount,0)>0 THEN 1 ELSE 0 END) AS LinesWith4609Contract,
  CAST(SUM(CASE WHEN d.mAmount1>0 THEN d.mAmount1 ELSE 0 END) AS decimal(18,2)) AS Coll
FROM dbo.tCore_Header_0 h
JOIN dbo.tCore_Data_0 d ON d.iHeaderId=h.iHeaderId
LEFT JOIN dbo.tCore_HeaderData4610_0 hd4610 ON hd4610.iHeaderId=h.iHeaderId
LEFT JOIN dbo.tCore_HeaderData4609_0 hd4609 ON hd4609.iHeaderId=h.iHeaderId
LEFT JOIN dbo.tCore_HeaderData4608_0 hd4608 ON hd4608.iHeaderId=h.iHeaderId
WHERE h.iVoucherType IN (4608,4609,4610)
  AND ISNULL(h.iAuth,1)=1 AND ISNULL(h.bCancelled,0)=0 AND ISNULL(h.bVersion,0)=0 AND ISNULL(h.bSuspended,0)=0
  AND h.iDate>0 AND d.bUpdateFA=1 AND d.iCode>0 AND d.iFaTag IN (2040,2057)
  AND ISNULL(d.iType,0)=0 AND ISNULL(d.bVoid,0)=0 AND ISNULL(d.iAuthStatus,0)<2
  AND COALESCE(NULLIF(hd4610.PaymentCode,0), NULLIF(hd4609.PaymentCode,0), NULLIF(hd4608.PaymentCode,0), 0) IN (1,4)
GROUP BY h.iVoucherType,
  COALESCE(NULLIF(hd4610.PaymentCode,0), NULLIF(hd4609.PaymentCode,0), NULLIF(hd4608.PaymentCode,0), 0)
ORDER BY 1,2
"@ "T5 first-pay by voucher + which contract field is filled"

Dump @"
SELECT
  dep.sName AS Dept,
  (h.iDate & 0xfff0000)/65536 AS Y,
  (h.iDate & 0xff00)/256 AS M,
  CAST(SUM(CASE WHEN d.mAmount1>0 THEN d.mAmount1 ELSE 0 END) AS decimal(18,2)) AS CollectionAmt,
  CAST(SUM(CASE
    WHEN COALESCE(NULLIF(hd4610.PaymentCode,0),NULLIF(hd4609.PaymentCode,0),NULLIF(hd4608.PaymentCode,0),0) IN (2,5)
         AND SUM(CASE WHEN d.mAmount1>0 THEN d.mAmount1 ELSE 0 END) OVER (PARTITION BY h.iHeaderId) IS NULL
    THEN 0 ELSE 0 END) AS decimal(18,2)) AS Dummy
FROM dbo.tCore_Header_0 h
JOIN dbo.tCore_Data_0 d ON d.iHeaderId=h.iHeaderId
JOIN dbo.mCore_Department dep ON dep.iMasterId=d.iFaTag
LEFT JOIN dbo.tCore_HeaderData4610_0 hd4610 ON hd4610.iHeaderId=h.iHeaderId
LEFT JOIN dbo.tCore_HeaderData4609_0 hd4609 ON hd4609.iHeaderId=h.iHeaderId
LEFT JOIN dbo.tCore_HeaderData4608_0 hd4608 ON hd4608.iHeaderId=h.iHeaderId
WHERE 1=0
GROUP BY dep.sName, (h.iDate & 0xfff0000)/65536, (h.iDate & 0xff00)/256
"@ "skip"

# Proper month totals with same eligibility as report
Dump @"
;WITH f AS (
  SELECT
    dep.sName AS Department,
    (h.iDate & 0xfff0000)/65536 AS Y,
    (h.iDate & 0xff00)/256 AS M,
    h.iHeaderId,
    COALESCE(NULLIF(hd4610.PaymentCode,0),NULLIF(hd4609.PaymentCode,0),NULLIF(hd4608.PaymentCode,0),0) AS Pay,
    CAST(SUM(CASE WHEN d.mAmount1>0 THEN d.mAmount1 ELSE 0 END) AS decimal(18,4)) AS Coll,
    CAST(ISNULL(hd4610.TotalContractAmt,0) AS decimal(18,4)) AS C4610,
    CAST(ISNULL(hd4609.ContractAmount,0) AS decimal(18,4)) AS C4609
  FROM dbo.tCore_Header_0 h
  JOIN dbo.tCore_Data_0 d ON d.iHeaderId=h.iHeaderId
    AND h.iVoucherType IN (4608,4609,4610)
    AND ISNULL(h.iAuth,1)=1 AND ISNULL(h.bCancelled,0)=0 AND ISNULL(h.bVersion,0)=0 AND ISNULL(h.bSuspended,0)=0
    AND h.iDate>0 AND d.bUpdateFA=1 AND d.iCode>0 AND d.iFaTag IN (2040,2057)
    AND ISNULL(d.iType,0)=0 AND ISNULL(d.bVoid,0)=0 AND ISNULL(d.iAuthStatus,0)<2
  JOIN dbo.mCore_Department dep ON dep.iMasterId=d.iFaTag
  LEFT JOIN dbo.tCore_HeaderData4610_0 hd4610 ON hd4610.iHeaderId=h.iHeaderId
  LEFT JOIN dbo.tCore_HeaderData4609_0 hd4609 ON hd4609.iHeaderId=h.iHeaderId
  LEFT JOIN dbo.tCore_HeaderData4608_0 hd4608 ON hd4608.iHeaderId=h.iHeaderId
  WHERE (h.iDate & 0xfff0000)/65536 = 2026
    AND (h.iDate & 0xff00)/256 IN (7,8,9)
  GROUP BY dep.sName, h.iDate, h.iHeaderId, hd4610.PaymentCode, hd4609.PaymentCode, hd4608.PaymentCode,
           hd4610.TotalContractAmt, hd4609.ContractAmount
)
SELECT Department, Y, M,
  CAST(SUM(Coll) AS decimal(18,2)) AS Collection,
  CAST(SUM(CASE WHEN Pay IN (2,5) AND Coll>0 THEN Coll
                WHEN Pay IN (1,4) AND C4610>0 AND Coll*2+1 >= C4610 THEN Coll
                ELSE 0 END) AS decimal(18,2)) AS Elig_SQL_now,
  CAST(SUM(CASE WHEN Pay IN (2,5) AND Coll>0 THEN Coll
                WHEN Pay IN (1,4) AND COALESCE(NULLIF(C4610,0),NULLIF(C4609,0),0)>0
                     AND Coll*2+1 >= COALESCE(NULLIF(C4610,0),NULLIF(C4609,0),0) THEN Coll
                ELSE 0 END) AS decimal(18,2)) AS Elig_if_4609_too,
  CAST(SUM(CASE WHEN Pay IN (2,5) AND Coll>0 THEN Coll
                WHEN Pay IN (1,4) AND C4610>0 AND Coll*2 >= C4610 THEN Coll
                ELSE 0 END) AS decimal(18,2)) AS Elig_exact_50_no_plus1,
  SUM(CASE WHEN Pay IN (1,4) THEN 1 ELSE 0 END) AS FirstPayRows,
  SUM(CASE WHEN Pay IN (1,4) AND C4610=0 THEN 1 ELSE 0 END) AS FirstPayNo4610Contract,
  SUM(CASE WHEN Pay IN (1,4) AND C4610=0 AND C4609>0 THEN 1 ELSE 0 END) AS FirstPayOnly4609Contract,
  SUM(CASE WHEN Pay IN (2,5) THEN 1 ELSE 0 END) AS SecondPayRows
FROM f
GROUP BY Department, Y, M
ORDER BY Department, Y, M
"@ "T6 Jul-Sep 2026 eligibility variants"

Dump @"
;WITH f AS (
  SELECT
    h.sVoucherNo,
    dep.sName AS Department,
    COALESCE(NULLIF(hd4610.PaymentCode,0),NULLIF(hd4609.PaymentCode,0),NULLIF(hd4608.PaymentCode,0),0) AS Pay,
    CAST(SUM(CASE WHEN d.mAmount1>0 THEN d.mAmount1 ELSE 0 END) AS decimal(18,4)) AS Coll,
    CAST(ISNULL(hd4610.TotalContractAmt,0) AS decimal(18,4)) AS C4610,
    CAST(ISNULL(hd4609.ContractAmount,0) AS decimal(18,4)) AS C4609
  FROM dbo.tCore_Header_0 h
  JOIN dbo.tCore_Data_0 d ON d.iHeaderId=h.iHeaderId
    AND h.iVoucherType IN (4608,4609,4610)
    AND ISNULL(h.iAuth,1)=1 AND ISNULL(h.bCancelled,0)=0 AND ISNULL(h.bVersion,0)=0 AND ISNULL(h.bSuspended,0)=0
    AND h.iDate>0 AND d.bUpdateFA=1 AND d.iCode>0 AND d.iFaTag IN (2040,2057)
    AND ISNULL(d.iType,0)=0 AND ISNULL(d.bVoid,0)=0 AND ISNULL(d.iAuthStatus,0)<2
  JOIN dbo.mCore_Department dep ON dep.iMasterId=d.iFaTag
  LEFT JOIN dbo.tCore_HeaderData4610_0 hd4610 ON hd4610.iHeaderId=h.iHeaderId
  LEFT JOIN dbo.tCore_HeaderData4609_0 hd4609 ON hd4609.iHeaderId=h.iHeaderId
  LEFT JOIN dbo.tCore_HeaderData4608_0 hd4608 ON hd4608.iHeaderId=h.iHeaderId
  WHERE COALESCE(NULLIF(hd4610.PaymentCode,0),NULLIF(hd4609.PaymentCode,0),NULLIF(hd4608.PaymentCode,0),0) IN (1,4)
  GROUP BY h.sVoucherNo, dep.sName, hd4610.PaymentCode, hd4609.PaymentCode, hd4608.PaymentCode,
           hd4610.TotalContractAmt, hd4609.ContractAmount
)
SELECT
  SUM(CASE WHEN C4610>0 AND Coll*2+1 >= C4610 AND Coll*2 < C4610 THEN 1 ELSE 0 END) AS Flip_only_because_plus1,
  SUM(CASE WHEN C4610>0 AND Coll*2 >= C4610 THEN 1 ELSE 0 END) AS Exact_or_over_50,
  SUM(CASE WHEN C4610>0 AND Coll*2+1 < C4610 THEN 1 ELSE 0 END) AS Below50_even_with_plus1,
  SUM(CASE WHEN C4610=0 THEN 1 ELSE 0 END) AS FirstPay_zero_4610_contract,
  SUM(CASE WHEN C4610=0 AND C4609>0 THEN 1 ELSE 0 END) AS FirstPay_4609_has_contract,
  SUM(CASE WHEN C4610=0 AND Coll>0 THEN 1 ELSE 0 END) AS FirstPay_zero_contract_has_coll
FROM f
"@ "T7 first-pay 50% boundary and missing contract"

Dump @"
SELECT TOP 12
  h.sVoucherNo, h.iVoucherType, dep.sName,
  COALESCE(NULLIF(hd4610.PaymentCode,0),NULLIF(hd4609.PaymentCode,0),NULLIF(hd4608.PaymentCode,0),0) AS Pay,
  CAST(SUM(CASE WHEN d.mAmount1>0 THEN d.mAmount1 ELSE 0 END) AS decimal(18,2)) AS Coll,
  CAST(ISNULL(hd4610.TotalContractAmt,0) AS decimal(18,2)) AS C4610,
  CAST(ISNULL(hd4609.ContractAmount,0) AS decimal(18,2)) AS C4609
FROM dbo.tCore_Header_0 h
JOIN dbo.tCore_Data_0 d ON d.iHeaderId=h.iHeaderId
JOIN dbo.mCore_Department dep ON dep.iMasterId=d.iFaTag
LEFT JOIN dbo.tCore_HeaderData4610_0 hd4610 ON hd4610.iHeaderId=h.iHeaderId
LEFT JOIN dbo.tCore_HeaderData4609_0 hd4609 ON hd4609.iHeaderId=h.iHeaderId
LEFT JOIN dbo.tCore_HeaderData4608_0 hd4608 ON hd4608.iHeaderId=h.iHeaderId
WHERE h.iVoucherType IN (4608,4609,4610)
  AND ISNULL(h.iAuth,1)=1 AND ISNULL(h.bCancelled,0)=0 AND ISNULL(h.bVersion,0)=0
  AND h.iDate>0 AND d.bUpdateFA=1 AND d.iCode>0 AND d.iFaTag IN (2040,2057)
  AND COALESCE(NULLIF(hd4610.PaymentCode,0),NULLIF(hd4609.PaymentCode,0),NULLIF(hd4608.PaymentCode,0),0) IN (1,4)
  AND ISNULL(hd4610.TotalContractAmt,0)=0
  AND ISNULL(hd4609.ContractAmount,0)>0
GROUP BY h.sVoucherNo, h.iVoucherType, dep.sName, hd4610.PaymentCode, hd4609.PaymentCode, hd4608.PaymentCode,
         hd4610.TotalContractAmt, hd4609.ContractAmount
"@ "T8 first-pay samples with only 4609 ContractAmount"

$conn.Close()
