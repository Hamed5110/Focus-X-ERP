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
;WITH f AS (
  SELECT
    dep.sName AS Department,
    (h.iDate & 0xfff0000)/65536 AS Y,
    (h.iDate & 0xff00)/256 AS M,
    CAST(COALESCE(NULLIF(hd4610.PaymentCode,0),NULLIF(hd4609.PaymentCode,0),NULLIF(hd4608.PaymentCode,0),0) AS int) AS Pay,
    CAST(SUM(CASE WHEN d.mAmount1>0 THEN d.mAmount1 ELSE 0 END) AS decimal(18,4)) AS Coll,
    CAST(ISNULL(hd4610.TotalContractAmt,0) AS decimal(18,4)) AS C4610,
    CAST(ISNULL(TRY_CONVERT(decimal(18,4), NULLIF(hd4609.ContractAmount,N'')),0) AS decimal(18,4)) AS C4609
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
                ELSE 0 END) AS decimal(18,2)) AS Elig_now,
  CAST(SUM(CASE WHEN Pay IN (2,5) AND Coll>0 THEN Coll
                WHEN Pay IN (1,4) AND COALESCE(NULLIF(C4610,0),NULLIF(C4609,0),0)>0
                     AND Coll*2+1 >= COALESCE(NULLIF(C4610,0),NULLIF(C4609,0),0) THEN Coll
                ELSE 0 END) AS decimal(18,2)) AS Elig_plus_4609,
  CAST(SUM(CASE WHEN Pay IN (2,5) AND Coll>0 THEN Coll
                WHEN Pay IN (1,4) AND C4610>0 AND Coll*2 >= C4610 THEN Coll
                ELSE 0 END) AS decimal(18,2)) AS Elig_exact50,
  SUM(CASE WHEN Pay IN (1,4) THEN 1 ELSE 0 END) AS FirstRows,
  SUM(CASE WHEN Pay IN (1,4) AND C4610=0 THEN 1 ELSE 0 END) AS FirstNo4610,
  SUM(CASE WHEN Pay IN (1,4) AND C4610=0 AND C4609>0 THEN 1 ELSE 0 END) AS FirstOnly4609,
  SUM(CASE WHEN Pay IN (2,5) THEN 1 ELSE 0 END) AS SecondRows,
  CASE
    WHEN Department=N'Atlas Aluminum' AND SUM(CASE WHEN Pay IN (2,5) AND Coll>0 THEN Coll WHEN Pay IN (1,4) AND C4610>0 AND Coll*2+1>=C4610 THEN Coll ELSE 0 END)>=300000 THEN 1.70
    WHEN Department=N'Atlas Aluminum' AND SUM(CASE WHEN Pay IN (2,5) AND Coll>0 THEN Coll WHEN Pay IN (1,4) AND C4610>0 AND Coll*2+1>=C4610 THEN Coll ELSE 0 END)>=250000 THEN 1.50
    WHEN Department=N'Atlas Aluminum' AND SUM(CASE WHEN Pay IN (2,5) AND Coll>0 THEN Coll WHEN Pay IN (1,4) AND C4610>0 AND Coll*2+1>=C4610 THEN Coll ELSE 0 END)>=200000 THEN 1.20
    WHEN Department=N'Atlas Aluminum' AND SUM(CASE WHEN Pay IN (2,5) AND Coll>0 THEN Coll WHEN Pay IN (1,4) AND C4610>0 AND Coll*2+1>=C4610 THEN Coll ELSE 0 END)>=150000 THEN 1.00
    WHEN Department=N'Aknan Showroom' AND SUM(CASE WHEN Pay IN (2,5) AND Coll>0 THEN Coll WHEN Pay IN (1,4) AND C4610>0 AND Coll*2+1>=C4610 THEN Coll ELSE 0 END)>=80000 THEN 1.50
    WHEN Department=N'Aknan Showroom' AND SUM(CASE WHEN Pay IN (2,5) AND Coll>0 THEN Coll WHEN Pay IN (1,4) AND C4610>0 AND Coll*2+1>=C4610 THEN Coll ELSE 0 END)>=70000 THEN 1.20
    WHEN Department=N'Aknan Showroom' AND SUM(CASE WHEN Pay IN (2,5) AND Coll>0 THEN Coll WHEN Pay IN (1,4) AND C4610>0 AND Coll*2+1>=C4610 THEN Coll ELSE 0 END)>=50000 THEN 1.00
    ELSE 0
  END AS RateNow
FROM f
GROUP BY Department, Y, M
ORDER BY Department, Y, M
"@ "T6 Jul-Sep 2026"

Dump @"
;WITH f AS (
  SELECT
    CAST(SUM(CASE WHEN d.mAmount1>0 THEN d.mAmount1 ELSE 0 END) AS decimal(18,4)) AS Coll,
    CAST(ISNULL(hd4610.TotalContractAmt,0) AS decimal(18,4)) AS C4610,
    CAST(ISNULL(TRY_CONVERT(decimal(18,4), NULLIF(hd4609.ContractAmount,N'')),0) AS decimal(18,4)) AS C4609
  FROM dbo.tCore_Header_0 h
  JOIN dbo.tCore_Data_0 d ON d.iHeaderId=h.iHeaderId
    AND h.iVoucherType IN (4608,4609,4610)
    AND ISNULL(h.iAuth,1)=1 AND ISNULL(h.bCancelled,0)=0 AND ISNULL(h.bVersion,0)=0 AND ISNULL(h.bSuspended,0)=0
    AND h.iDate>0 AND d.bUpdateFA=1 AND d.iCode>0 AND d.iFaTag IN (2040,2057)
    AND ISNULL(d.iType,0)=0 AND ISNULL(d.bVoid,0)=0 AND ISNULL(d.iAuthStatus,0)<2
  LEFT JOIN dbo.tCore_HeaderData4610_0 hd4610 ON hd4610.iHeaderId=h.iHeaderId
  LEFT JOIN dbo.tCore_HeaderData4609_0 hd4609 ON hd4609.iHeaderId=h.iHeaderId
  LEFT JOIN dbo.tCore_HeaderData4608_0 hd4608 ON hd4608.iHeaderId=h.iHeaderId
  WHERE COALESCE(NULLIF(hd4610.PaymentCode,0),NULLIF(hd4609.PaymentCode,0),NULLIF(hd4608.PaymentCode,0),0) IN (1,4)
  GROUP BY h.iHeaderId, hd4610.TotalContractAmt, hd4609.ContractAmount
)
SELECT
  SUM(CASE WHEN C4610>0 AND Coll*2+1 >= C4610 AND Coll*2 < C4610 THEN 1 ELSE 0 END) AS Flip_only_plus1,
  SUM(CASE WHEN C4610>0 AND Coll*2 >= C4610 THEN 1 ELSE 0 END) AS Exact_or_over_50,
  SUM(CASE WHEN C4610>0 AND Coll*2+1 < C4610 THEN 1 ELSE 0 END) AS Below50_with_plus1,
  SUM(CASE WHEN C4610=0 THEN 1 ELSE 0 END) AS First_zero_4610,
  SUM(CASE WHEN C4610=0 AND C4609>0 THEN 1 ELSE 0 END) AS First_only_4609,
  CAST(SUM(CASE WHEN C4610=0 THEN Coll ELSE 0 END) AS decimal(18,2)) AS Coll_first_no_4610
FROM f
"@ "T7 first-pay 50% boundary"

Dump @"
SELECT
  CASE
    WHEN 149999.99 >= 150000 THEN 1 ELSE 0 END AS Atlas_just_below,
    CASE WHEN 150000 >= 150000 AND 150000 < 200000 THEN 1 ELSE 0 END AS Atlas_at_150,
    CASE WHEN 199999.99 < 200000 AND 199999.99 >= 150000 THEN 1 ELSE 0 END AS Atlas_just_below_200,
    CASE WHEN 200000 >= 200000 THEN 1 ELSE 0 END AS Atlas_at_200,
    CASE WHEN 300000 >= 300000 THEN 1 ELSE 0 END AS Atlas_at_300,
    CASE WHEN 79999.99 >= 80000 THEN 1 ELSE 0 END AS Aknan_just_below_80,
    CASE WHEN 80000 >= 80000 THEN 1 ELSE 0 END AS Aknan_at_80,
    CASE WHEN 100000.01 >= 80000 THEN 1.5 ELSE 0 END AS Aknan_above_100
"@ "T9 band boundaries"

Dump @"
SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME LIKE 'tCore_HeaderData5634%'
  AND (COLUMN_NAME LIKE '%Contract%' OR COLUMN_NAME LIKE '%Total%' OR COLUMN_NAME LIKE '%Amount%')
ORDER BY COLUMN_NAME
"@ "SO 5634 contract-like fields"

$conn.Close()
