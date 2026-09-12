$cs = "Server=localhost;Database=Focus80E0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 180
$cmd.CommandText = @"
DECLARE @JulS INT = dbo.DateToInt(CONVERT(datetime,'20260701',112));
DECLARE @JulE INT = dbo.DateToInt(CONVERT(datetime,'20260731',112));
DECLARE @AugS INT = dbo.DateToInt(CONVERT(datetime,'20260801',112));
DECLARE @AugE INT = dbo.DateToInt(CONVERT(datetime,'20260831',112));

-- Sales Orders (overall sales / bookings)
SELECT
    CASE WHEN h.iDate BETWEEN @JulS AND @JulE THEN N'Jul' ELSE N'Aug' END AS Period,
    COUNT(DISTINCT h.iHeaderId) AS SOCnt,
    CAST(ABS(SUM(h.fNet)) AS decimal(18,2)) AS SOAbsSumNet,
    CAST(SUM(-h.fNet) AS decimal(18,2)) AS SORevSign
FROM dbo.tCore_Header_0 h
INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId = h.iHeaderId
WHERE h.iVoucherType = 5634
  AND ISNULL(h.iAuth,1)=1 AND ISNULL(h.bCancelled,0)=0 AND ISNULL(h.bVersion,0)=0 AND ISNULL(h.bSuspended,0)=0
  AND d.iFaTag = 2040 AND d.bUpdateFA = 1
  AND ((h.iDate BETWEEN @JulS AND @JulE) OR (h.iDate BETWEEN @AugS AND @AugE))
GROUP BY CASE WHEN h.iDate BETWEEN @JulS AND @JulE THEN N'Jul' ELSE N'Aug' END;

-- Unique receipt contracts (001/004) all vs qualified
;WITH rec AS (
    SELECT
        h.iDate, d.iCode AS CustId,
        COALESCE(NULLIF(hd4610.PaymentCode,0), NULLIF(hd4609.PaymentCode,0), NULLIF(hd4608.PaymentCode,0), 0) AS Pay,
        CAST(SUM(CASE WHEN d.mAmount1>0 THEN d.mAmount1 ELSE 0 END) AS decimal(18,4)) AS Coll,
        CAST(COALESCE(NULLIF(hd4610.TotalContractAmt,0), NULLIF(TRY_CONVERT(decimal(18,4), NULLIF(LTRIM(RTRIM(hd4609.ContractAmount)),N'')),0), 0) AS decimal(18,4)) AS CVal
    FROM dbo.tCore_Header_0 h
    INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId=h.iHeaderId
       AND h.iVoucherType IN (4608,4609,4610)
       AND ISNULL(h.iAuth,1)=1 AND ISNULL(h.bCancelled,0)=0 AND ISNULL(h.bVersion,0)=0 AND ISNULL(h.bSuspended,0)=0
       AND d.bUpdateFA=1 AND d.iCode>0 AND d.iFaTag=2040 AND ISNULL(d.iType,0)=0 AND ISNULL(d.bVoid,0)=0 AND ISNULL(d.iAuthStatus,0)<2
    INNER JOIN dbo.mCore_Account a ON a.iMasterId=d.iCode AND a.iAccountType IN (5,7)
    LEFT JOIN dbo.tCore_HeaderData4610_0 hd4610 ON hd4610.iHeaderId=h.iHeaderId
    LEFT JOIN dbo.tCore_HeaderData4609_0 hd4609 ON hd4609.iHeaderId=h.iHeaderId
    LEFT JOIN dbo.tCore_HeaderData4608_0 hd4608 ON hd4608.iHeaderId=h.iHeaderId
    GROUP BY h.iDate, d.iCode, hd4610.PaymentCode, hd4609.PaymentCode, hd4608.PaymentCode, hd4610.TotalContractAmt, hd4609.ContractAmount
),
life AS (
    SELECT CustId, CVal,
           SUM(CASE WHEN Pay IN (1,4) AND iDate<=@JulE THEN Coll ELSE 0 END) AS LifeJul,
           SUM(CASE WHEN Pay IN (1,4) AND iDate<=@AugE THEN Coll ELSE 0 END) AS LifeAug
    FROM rec WHERE CVal>0
    GROUP BY CustId, CVal
),
per AS (
    SELECT
        CASE WHEN r.iDate BETWEEN @JulS AND @JulE THEN N'Jul' ELSE N'Aug' END AS Period,
        r.CustId, r.CVal,
        SUM(r.Coll) AS PeriodColl
    FROM rec r
    WHERE r.Pay IN (1,4)
      AND ((r.iDate BETWEEN @JulS AND @JulE) OR (r.iDate BETWEEN @AugS AND @AugE))
    GROUP BY CASE WHEN r.iDate BETWEEN @JulS AND @JulE THEN N'Jul' ELSE N'Aug' END, r.CustId, r.CVal
)
SELECT
    p.Period,
    COUNT(*) AS Contracts,
    CAST(SUM(p.CVal) AS decimal(18,2)) AS AllContract,
    CAST(SUM(CASE WHEN p.CVal>0 AND ISNULL(CASE WHEN p.Period=N'Jul' THEN l.LifeJul ELSE l.LifeAug END,0)*2+1 >= p.CVal THEN p.CVal ELSE 0 END) AS decimal(18,2)) AS QualContract,
    CAST(SUM(p.PeriodColl) AS decimal(18,2)) AS AllColl,
    CAST(SUM(CASE WHEN p.CVal>0 AND ISNULL(CASE WHEN p.Period=N'Jul' THEN l.LifeJul ELSE l.LifeAug END,0)*2+1 >= p.CVal THEN p.PeriodColl ELSE 0 END) AS decimal(18,2)) AS QualColl
FROM per p
LEFT JOIN life l ON l.CustId=p.CustId AND l.CVal=p.CVal
GROUP BY p.Period
ORDER BY p.Period;
"@
$r = $cmd.ExecuteReader()
Write-Output "=== SO 5634 Atlas ==="
while ($r.Read()) { Write-Output ("{0} so={1} absNet={2:N2} revSign={3:N2}" -f $r["Period"], $r["SOCnt"], $r["SOAbsSumNet"], $r["SORevSign"]) }
$r.NextResult() | Out-Null
Write-Output "=== Receipt unique contracts 001/004 ==="
while ($r.Read()) {
    Write-Output ("{0} n={1} allC={2:N2} qualC={3:N2} allColl={4:N2} qualColl={5:N2}" -f $r["Period"], $r["Contracts"], $r["AllContract"], $r["QualContract"], $r["AllColl"], $r["QualColl"])
}
$r.Close(); $conn.Close()
