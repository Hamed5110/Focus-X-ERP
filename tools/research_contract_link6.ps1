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
                    if ($v.Length -gt 220) { $v = $v.Substring(0,220) + "..." }
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
SELECT d.iBodyId, d.iHeaderId, h.sVoucherNo, h.iVoucherType
FROM dbo.tCore_Data_0 d
LEFT JOIN dbo.tCore_Header_0 h ON h.iHeaderId=d.iHeaderId
WHERE d.iBodyId=71110
"@ "ATIC-26-1264 iRef 71110"

Dump @"
SELECT TOP 15
  acc.sName,
  CAST(hd.TotalContractAmt AS decimal(18,2)) AS Contract,
  COUNT(DISTINCT h.iHeaderId) AS RctCnt,
  CAST(SUM(CASE WHEN d.mAmount1>0 THEN d.mAmount1 ELSE 0 END) AS decimal(18,2)) AS AllRct,
  SUM(CASE WHEN COALESCE(NULLIF(hd.PaymentCode,0),NULLIF(hd9.PaymentCode,0),NULLIF(hd8.PaymentCode,0),0) IN (1,4) THEN 1 ELSE 0 END) AS FirstRows
FROM dbo.tCore_Header_0 h
JOIN dbo.tCore_Data_0 d ON d.iHeaderId=h.iHeaderId AND d.bUpdateFA=1 AND d.iCode>0 AND d.iFaTag IN (2040,2057)
JOIN dbo.mCore_Account acc ON acc.iMasterId=d.iCode
LEFT JOIN dbo.tCore_HeaderData4610_0 hd ON hd.iHeaderId=h.iHeaderId
LEFT JOIN dbo.tCore_HeaderData4609_0 hd9 ON hd9.iHeaderId=h.iHeaderId
LEFT JOIN dbo.tCore_HeaderData4608_0 hd8 ON hd8.iHeaderId=h.iHeaderId
WHERE h.iVoucherType IN (4608,4609,4610)
  AND ISNULL(h.iAuth,1)=1 AND ISNULL(h.bCancelled,0)=0 AND ISNULL(h.bVersion,0)=0
  AND h.iDate>0 AND ISNULL(hd.TotalContractAmt,0)>0
GROUP BY acc.sName, hd.TotalContractAmt
HAVING COUNT(DISTINCT h.iHeaderId) > 1
ORDER BY COUNT(DISTINCT h.iHeaderId) DESC
"@ "contracts with multiple receipts (account + TotalContractAmt)"

Dump @"
SELECT
  SUM(CASE WHEN RctCnt=1 THEN 1 ELSE 0 END) AS SingleRctContracts,
  SUM(CASE WHEN RctCnt>1 THEN 1 ELSE 0 END) AS MultiRctContracts,
  SUM(CASE WHEN RctCnt>1 THEN FirstColl ELSE 0 END) AS FirstCollOnMulti
FROM (
  SELECT d.iCode, hd.TotalContractAmt,
    COUNT(DISTINCT h.iHeaderId) AS RctCnt,
    CAST(SUM(CASE WHEN COALESCE(NULLIF(hd.PaymentCode,0),0) IN (1,4) AND d.mAmount1>0 THEN d.mAmount1 ELSE 0 END) AS decimal(18,2)) AS FirstColl
  FROM dbo.tCore_Header_0 h
  JOIN dbo.tCore_Data_0 d ON d.iHeaderId=h.iHeaderId AND d.bUpdateFA=1 AND d.iCode>0 AND d.iFaTag IN (2040,2057)
  JOIN dbo.tCore_HeaderData4610_0 hd ON hd.iHeaderId=h.iHeaderId
  WHERE h.iVoucherType IN (4608,4609,4610)
    AND ISNULL(h.iAuth,1)=1 AND ISNULL(h.bCancelled,0)=0 AND ISNULL(h.bVersion,0)=0
    AND h.iDate>0 AND ISNULL(hd.TotalContractAmt,0)>0
  GROUP BY d.iCode, hd.TotalContractAmt
) x
"@ "how often one contract has several receipts"

Dump @"
-- Jul-Aug first-pay: this-receipt 50% vs contract-cumulative (account+TotalContractAmt)
;WITH rct AS (
  SELECT
    h.iHeaderId,
    d.iCode,
    CAST(ISNULL(hd.TotalContractAmt,0) AS decimal(18,4)) AS CVal,
    CAST(SUM(CASE WHEN d.mAmount1>0 THEN d.mAmount1 ELSE 0 END) AS decimal(18,4)) AS ThisColl,
    COALESCE(NULLIF(hd.PaymentCode,0),NULLIF(hd9.PaymentCode,0),NULLIF(hd8.PaymentCode,0),0) AS Pay,
    (h.iDate & 0xfff0000)/65536 AS Y,
    (h.iDate & 0xff00)/256 AS M,
    dep.sName AS Dept
  FROM dbo.tCore_Header_0 h
  JOIN dbo.tCore_Data_0 d ON d.iHeaderId=h.iHeaderId AND d.bUpdateFA=1 AND d.iCode>0 AND d.iFaTag IN (2040,2057)
    AND ISNULL(d.iType,0)=0 AND ISNULL(d.bVoid,0)=0
  JOIN dbo.mCore_Department dep ON dep.iMasterId=d.iFaTag
  LEFT JOIN dbo.tCore_HeaderData4610_0 hd ON hd.iHeaderId=h.iHeaderId
  LEFT JOIN dbo.tCore_HeaderData4609_0 hd9 ON hd9.iHeaderId=h.iHeaderId
  LEFT JOIN dbo.tCore_HeaderData4608_0 hd8 ON hd8.iHeaderId=h.iHeaderId
  WHERE h.iVoucherType IN (4608,4609,4610)
    AND ISNULL(h.iAuth,1)=1 AND ISNULL(h.bCancelled,0)=0 AND ISNULL(h.bVersion,0)=0 AND ISNULL(h.bSuspended,0)=0
    AND h.iDate>0
  GROUP BY h.iHeaderId, d.iCode, hd.TotalContractAmt, hd.PaymentCode, hd9.PaymentCode, hd8.PaymentCode,
           h.iDate, dep.sName
),
cum AS (
  SELECT iCode, CVal, CAST(SUM(ThisColl) AS decimal(18,4)) AS LifeColl
  FROM rct
  WHERE CVal>0
  GROUP BY iCode, CVal
)
SELECT
  r.Dept, r.Y, r.M,
  CAST(SUM(r.ThisColl) AS decimal(18,2)) AS PeriodColl,
  CAST(SUM(CASE WHEN r.Pay IN (2,5) AND r.ThisColl>0 THEN r.ThisColl
                WHEN r.Pay IN (1,4) AND r.CVal>0 AND r.ThisColl*2+1>=r.CVal THEN r.ThisColl
                ELSE 0 END) AS decimal(18,2)) AS Elig_this_rct,
  CAST(SUM(CASE WHEN r.Pay IN (2,5) AND r.ThisColl>0 THEN r.ThisColl
                WHEN r.Pay IN (1,4) AND r.CVal>0 AND c.LifeColl*2+1>=r.CVal THEN r.ThisColl
                ELSE 0 END) AS decimal(18,2)) AS Elig_contract_life,
  SUM(CASE WHEN r.Pay IN (1,4) AND r.CVal>0 AND r.ThisColl*2+1<r.CVal AND c.LifeColl*2+1>=r.CVal THEN 1 ELSE 0 END) AS FirstFlippedByCum
FROM rct r
LEFT JOIN cum c ON c.iCode=r.iCode AND c.CVal=r.CVal
WHERE r.Y=2026 AND r.M IN (7,8)
GROUP BY r.Dept, r.Y, r.M
ORDER BY r.Dept, r.Y, r.M
"@ "this-receipt vs contract-cumulative Jul-Aug"
