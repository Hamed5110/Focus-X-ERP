$cs = "Server=localhost;Database=Focus80E0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 120
$cmd.CommandText = @"
DECLARE @SepS INT = dbo.DateToInt(CONVERT(datetime,'20260901',112));
DECLARE @SepE INT = dbo.DateToInt(CONVERT(datetime,'20260912',112));
DECLARE @AugS INT = dbo.DateToInt(CONVERT(datetime,'20260801',112));
DECLARE @AugE INT = dbo.DateToInt(CONVERT(datetime,'20260831',112));

SELECT
    CASE WHEN h.iDate BETWEEN @AugS AND @AugE THEN N'Aug' ELSE N'Sep' END AS Period,
    COALESCE(NULLIF(hd4610.PaymentCode,0), NULLIF(hd4609.PaymentCode,0), NULLIF(hd4608.PaymentCode,0), 0) AS Pay,
    COUNT(DISTINCT h.iHeaderId) Docs,
    CAST(SUM(CASE WHEN d.mAmount1>0 THEN d.mAmount1 ELSE 0 END) AS decimal(18,2)) Coll
FROM dbo.tCore_Header_0 h
INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId=h.iHeaderId
   AND h.iVoucherType IN (4608,4609,4610)
   AND ISNULL(h.iAuth,1)=1 AND ISNULL(h.bCancelled,0)=0 AND ISNULL(h.bVersion,0)=0 AND ISNULL(h.bSuspended,0)=0
   AND ((h.iDate BETWEEN @AugS AND @AugE) OR (h.iDate BETWEEN @SepS AND @SepE))
   AND d.bUpdateFA=1 AND d.iCode>0 AND d.iFaTag=2040
   AND ISNULL(d.iType,0)=0 AND ISNULL(d.bVoid,0)=0 AND ISNULL(d.iAuthStatus,0)<2
INNER JOIN dbo.mCore_Account a ON a.iMasterId=d.iCode AND a.iAccountType IN (5,7)
LEFT JOIN dbo.tCore_HeaderData4610_0 hd4610 ON hd4610.iHeaderId=h.iHeaderId
LEFT JOIN dbo.tCore_HeaderData4609_0 hd4609 ON hd4609.iHeaderId=h.iHeaderId
LEFT JOIN dbo.tCore_HeaderData4608_0 hd4608 ON hd4608.iHeaderId=h.iHeaderId
GROUP BY CASE WHEN h.iDate BETWEEN @AugS AND @AugE THEN N'Aug' ELSE N'Sep' END,
         COALESCE(NULLIF(hd4610.PaymentCode,0), NULLIF(hd4609.PaymentCode,0), NULLIF(hd4608.PaymentCode,0), 0)
ORDER BY Period, Pay;

-- Aug collection eligible independent
;WITH rec AS (
    SELECT h.iDate, d.iCode AS CustId,
        CASE WHEN ISNULL(sm.sName,N'')=N'' THEN N'(blank)' ELSE sm.sName END AS Salesman,
        COALESCE(NULLIF(hd4610.PaymentCode,0), NULLIF(hd4609.PaymentCode,0), NULLIF(hd4608.PaymentCode,0), 0) AS Pay,
        CAST(SUM(CASE WHEN d.mAmount1>0 THEN d.mAmount1 ELSE 0 END) AS decimal(18,4)) AS Coll,
        CAST(COALESCE(NULLIF(hd4610.TotalContractAmt,0), NULLIF(TRY_CONVERT(decimal(18,4), NULLIF(LTRIM(RTRIM(hd4609.ContractAmount)),N'')),0), 0) AS decimal(18,4)) AS CVal
    FROM dbo.tCore_Header_0 h
    INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId=h.iHeaderId
       AND h.iVoucherType IN (4608,4609,4610)
       AND ISNULL(h.iAuth,1)=1 AND ISNULL(h.bCancelled,0)=0 AND ISNULL(h.bVersion,0)=0 AND ISNULL(h.bSuspended,0)=0
       AND h.iDate>0 AND d.bUpdateFA=1 AND d.iCode>0 AND d.iFaTag=2040
       AND ISNULL(d.iType,0)=0 AND ISNULL(d.bVoid,0)=0 AND ISNULL(d.iAuthStatus,0)<2
    INNER JOIN dbo.mCore_Account a ON a.iMasterId=d.iCode AND a.iAccountType IN (5,7)
    LEFT JOIN dbo.tCore_HeaderData4610_0 hd4610 ON hd4610.iHeaderId=h.iHeaderId
    LEFT JOIN dbo.tCore_HeaderData4609_0 hd4609 ON hd4609.iHeaderId=h.iHeaderId
    LEFT JOIN dbo.tCore_HeaderData4608_0 hd4608 ON hd4608.iHeaderId=h.iHeaderId
    LEFT JOIN (
        SELECT vac0.iMasterId, MAX(vac0.Salesmanname) AS Salesmanname
        FROM dbo.vaCore_Account vac0 GROUP BY vac0.iMasterId, vac0.iTreeId HAVING vac0.iTreeId=0
    ) vac ON vac.iMasterId=d.iCode
    LEFT JOIN dbo.mCore_Salesman sm ON sm.iMasterId=vac.Salesmanname
    GROUP BY h.iDate, d.iCode, CASE WHEN ISNULL(sm.sName,N'')=N'' THEN N'(blank)' ELSE sm.sName END,
             hd4610.PaymentCode, hd4609.PaymentCode, hd4608.PaymentCode, hd4610.TotalContractAmt, hd4609.ContractAmount
),
life AS (
    SELECT CustId, CVal, SUM(CASE WHEN Pay IN (1,4) AND iDate<=@AugE THEN Coll ELSE 0 END) Life
    FROM rec WHERE CVal>0 GROUP BY CustId, CVal
),
per AS (
    SELECT Salesman, CustId, CVal, SUM(Coll) PeriodColl
    FROM rec WHERE Pay IN (1,4) AND iDate BETWEEN @AugS AND @AugE
    GROUP BY Salesman, CustId, CVal
)
SELECT p.Salesman,
    CAST(SUM(p.PeriodColl) AS decimal(18,2)) Coll,
    CAST(SUM(CASE WHEN p.CVal>0 AND ISNULL(l.Life,0)*2+1>=p.CVal THEN p.PeriodColl ELSE 0 END) AS decimal(18,2)) CollElig
FROM per p
LEFT JOIN life l ON l.CustId=p.CustId AND l.CVal=p.CVal
GROUP BY p.Salesman
ORDER BY p.Salesman;
"@
$r = $cmd.ExecuteReader()
Write-Output "=== PAY BY MONTH ==="
while ($r.Read()) { Write-Output ("{0} pay={1} docs={2} coll={3:N2}" -f $r["Period"], $r["Pay"], $r["Docs"], $r["Coll"]) }
$r.NextResult() | Out-Null
Write-Output "=== AUG COLL ELIG ==="
$t=0; $e=0
while ($r.Read()) {
    $t += [double]$r["Coll"]; $e += [double]$r["CollElig"]
    Write-Output ("{0,-32} coll={1,10:N2} collElig={2,10:N2}" -f $r["Salesman"], $r["Coll"], $r["CollElig"])
}
Write-Output ("TOTAL coll={0:N2} collElig={1:N2}" -f $t, $e)
$r.Close(); $conn.Close()
