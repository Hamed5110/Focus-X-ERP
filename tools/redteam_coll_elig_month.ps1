$cs = "Server=localhost;Database=Focus80E0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 180
$cmd.CommandText = @"
DECLARE @AugS INT = dbo.DateToInt(CONVERT(datetime,'20260801',112));
DECLARE @AugE INT = dbo.DateToInt(CONVERT(datetime,'20260831',112));
DECLARE @SepS INT = dbo.DateToInt(CONVERT(datetime,'20260901',112));
DECLARE @SepE INT = dbo.DateToInt(CONVERT(datetime,'20260912',112));

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
)
SELECT N'Aug all-pay' AS Bucket, COUNT(*) Rows, CAST(SUM(Coll) AS decimal(18,2)) Coll
FROM rec WHERE iDate BETWEEN @AugS AND @AugE
UNION ALL
SELECT N'Aug 001/004', COUNT(*), CAST(SUM(Coll) AS decimal(18,2))
FROM rec WHERE iDate BETWEEN @AugS AND @AugE AND Pay IN (1,4)
UNION ALL
SELECT N'Sep 1-12 all-pay', COUNT(*), CAST(SUM(Coll) AS decimal(18,2))
FROM rec WHERE iDate BETWEEN @SepS AND @SepE
UNION ALL
SELECT N'Sep 1-12 001/004', COUNT(*), CAST(SUM(Coll) AS decimal(18,2))
FROM rec WHERE iDate BETWEEN @SepS AND @SepE AND Pay IN (1,4)
UNION ALL
SELECT N'Sep 1-30 001/004', COUNT(*), CAST(SUM(Coll) AS decimal(18,2))
FROM rec WHERE iDate BETWEEN @SepS AND dbo.DateToInt(CONVERT(datetime,'20260930',112)) AND Pay IN (1,4);

SELECT
    CASE WHEN r.iDate BETWEEN @AugS AND @AugE THEN N'Aug' ELSE N'Sep' END AS Period,
    r.Salesman,
    CAST(SUM(r.Coll) AS decimal(18,2)) AS PeriodColl,
    CAST(SUM(CASE WHEN r.CVal>0 AND ISNULL(l.Life,0)*2+1 >= r.CVal THEN r.Coll ELSE 0 END) AS decimal(18,2)) AS CollElig,
    CAST(SUM(CASE WHEN r.CVal>0 AND ISNULL(l.Life,0)*2+1 >= r.CVal THEN r.CVal ELSE 0 END) AS decimal(18,2)) AS ContrElig
FROM rec r
OUTER APPLY (
    SELECT SUM(r2.Coll) AS Life
    FROM rec r2
    WHERE r2.CustId=r.CustId AND r2.CVal=r.CVal AND r2.Pay IN (1,4)
      AND r2.iDate <= CASE WHEN r.iDate BETWEEN @AugS AND @AugE THEN @AugE ELSE @SepE END
) l
WHERE r.Pay IN (1,4)
  AND ((r.iDate BETWEEN @AugS AND @AugE) OR (r.iDate BETWEEN @SepS AND @SepE))
GROUP BY CASE WHEN r.iDate BETWEEN @AugS AND @AugE THEN N'Aug' ELSE N'Sep' END, r.Salesman
ORDER BY Period, r.Salesman;
"@
$r = $cmd.ExecuteReader()
Write-Output "=== MONTH COVERAGE ==="
while ($r.Read()) { Write-Output ("{0,-20} rows={1} coll={2:N2}" -f $r["Bucket"], $r["Rows"], $r["Coll"]) }
$r.NextResult() | Out-Null
Write-Output ""
Write-Output "=== SALESMAN COLL ELIG ==="
while ($r.Read()) {
    Write-Output ("{0} | {1,-32} coll={2,10:N2} collElig={3,10:N2} contrElig={4,10:N2}" -f $r["Period"], $r["Salesman"], $r["PeriodColl"], $r["CollElig"], $r["ContrElig"])
}
$r.Close(); $conn.Close()
