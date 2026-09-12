$cs = "Server=localhost;Database=Focus80E0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 180
$cmd.CommandText = @"
DECLARE @JulS INT = dbo.DateToInt(CONVERT(datetime,'20260701',112));
DECLARE @JulE INT = dbo.DateToInt(CONVERT(datetime,'20260731',112));
DECLARE @AugS INT = dbo.DateToInt(CONVERT(datetime,'20260801',112));
DECLARE @AugE INT = dbo.DateToInt(CONVERT(datetime,'20260831',112));

;WITH rec AS (
    SELECT
        h.iDate,
        h.iHeaderId,
        d.iCode AS CustId,
        acct.sName AS CustomerName,
        CASE WHEN ISNULL(sm.sName, N'') = N'' THEN N'(blank)' ELSE sm.sName END AS Salesman,
        COALESCE(NULLIF(hd4610.PaymentCode, 0), NULLIF(hd4609.PaymentCode, 0), NULLIF(hd4608.PaymentCode, 0), 0) AS Pay,
        CAST(SUM(CASE WHEN d.mAmount1 > 0 THEN d.mAmount1 ELSE 0 END) AS decimal(18,4)) AS Coll,
        CAST(
            COALESCE(
                NULLIF(hd4610.TotalContractAmt, 0),
                NULLIF(TRY_CONVERT(decimal(18,4), NULLIF(LTRIM(RTRIM(hd4609.ContractAmount)), N'')), 0),
                0
            )
        AS decimal(18,4)) AS ContractVal
    FROM dbo.tCore_Header_0 h
    INNER JOIN dbo.tCore_Data_0 d
        ON d.iHeaderId = h.iHeaderId
       AND h.iVoucherType IN (4608, 4609, 4610)
       AND ISNULL(h.iAuth, 1) = 1
       AND ISNULL(h.bCancelled, 0) = 0
       AND ISNULL(h.bVersion, 0) = 0
       AND ISNULL(h.bSuspended, 0) = 0
       AND h.iDate > 0
       AND d.bUpdateFA = 1
       AND d.iCode > 0
       AND d.iFaTag = 2040
       AND ISNULL(d.iType, 0) = 0
       AND ISNULL(d.bVoid, 0) = 0
       AND ISNULL(d.iAuthStatus, 0) < 2
    INNER JOIN dbo.mCore_Account acct ON acct.iMasterId = d.iCode AND acct.iAccountType IN (5, 7)
    LEFT JOIN dbo.tCore_HeaderData4610_0 hd4610 ON hd4610.iHeaderId = h.iHeaderId
    LEFT JOIN dbo.tCore_HeaderData4609_0 hd4609 ON hd4609.iHeaderId = h.iHeaderId
    LEFT JOIN dbo.tCore_HeaderData4608_0 hd4608 ON hd4608.iHeaderId = h.iHeaderId
    LEFT JOIN (
        SELECT vac0.iMasterId, MAX(vac0.Salesmanname) AS Salesmanname
        FROM dbo.vaCore_Account vac0
        GROUP BY vac0.iMasterId, vac0.iTreeId
        HAVING vac0.iTreeId = 0
    ) vac ON vac.iMasterId = d.iCode
    LEFT JOIN dbo.mCore_Salesman sm ON sm.iMasterId = vac.Salesmanname
    GROUP BY
        h.iDate, h.iHeaderId, d.iCode, acct.sName,
        CASE WHEN ISNULL(sm.sName, N'') = N'' THEN N'(blank)' ELSE sm.sName END,
        hd4610.PaymentCode, hd4609.PaymentCode, hd4608.PaymentCode,
        hd4610.TotalContractAmt, hd4609.ContractAmount
),
life_all AS (
    SELECT CustId, ContractVal,
           SUM(CASE WHEN iDate <= @JulE THEN Coll ELSE 0 END) AS LifeJul,
           SUM(CASE WHEN iDate <= @AugE THEN Coll ELSE 0 END) AS LifeAug
    FROM rec
    WHERE ContractVal > 0
    GROUP BY CustId, ContractVal
),
life_first AS (
    SELECT CustId, ContractVal,
           SUM(CASE WHEN iDate <= @JulE THEN Coll ELSE 0 END) AS LifeJul,
           SUM(CASE WHEN iDate <= @AugE THEN Coll ELSE 0 END) AS LifeAug
    FROM rec
    WHERE ContractVal > 0 AND Pay IN (1, 4)
    GROUP BY CustId, ContractVal
)
SELECT
    Period,
    Variant,
    CAST(SUM(Coll) AS decimal(18,2)) AS FirstPayColl,
    CAST(SUM(Elig) AS decimal(18,2)) AS Elig,
    COUNT(*) AS Rows,
    SUM(CASE WHEN Elig > 0 THEN 1 ELSE 0 END) AS EligRows
FROM (
    SELECT
        CASE
            WHEN r.iDate BETWEEN @JulS AND @JulE THEN N'Jul-2026'
            WHEN r.iDate BETWEEN @AugS AND @AugE THEN N'Aug-2026'
        END AS Period,
        N'LifeALL' AS Variant,
        r.Coll,
        CASE
            WHEN r.ContractVal > 0 AND ISNULL(
                CASE WHEN r.iDate BETWEEN @JulS AND @JulE THEN la.LifeJul ELSE la.LifeAug END, 0
            ) * 2 + 1 >= r.ContractVal
            THEN r.Coll ELSE 0
        END AS Elig
    FROM rec r
    LEFT JOIN life_all la ON la.CustId = r.CustId AND la.ContractVal = r.ContractVal
    WHERE r.Pay IN (1, 4)
      AND r.iDate BETWEEN @JulS AND @AugE
    UNION ALL
    SELECT
        CASE
            WHEN r.iDate BETWEEN @JulS AND @JulE THEN N'Jul-2026'
            WHEN r.iDate BETWEEN @AugS AND @AugE THEN N'Aug-2026'
        END,
        N'LifeFIRST',
        r.Coll,
        CASE
            WHEN r.ContractVal > 0 AND ISNULL(
                CASE WHEN r.iDate BETWEEN @JulS AND @JulE THEN lf.LifeJul ELSE lf.LifeAug END, 0
            ) * 2 + 1 >= r.ContractVal
            THEN r.Coll ELSE 0
        END
    FROM rec r
    LEFT JOIN life_first lf ON lf.CustId = r.CustId AND lf.ContractVal = r.ContractVal
    WHERE r.Pay IN (1, 4)
      AND r.iDate BETWEEN @JulS AND @AugE
    UNION ALL
    SELECT
        CASE
            WHEN r.iDate BETWEEN @JulS AND @JulE THEN N'Jul-2026'
            WHEN r.iDate BETWEEN @AugS AND @AugE THEN N'Aug-2026'
        END,
        N'ThisVoucher50',
        r.Coll,
        CASE
            WHEN r.ContractVal > 0 AND r.Coll * 2 + 1 >= r.ContractVal
            THEN r.Coll ELSE 0
        END
    FROM rec r
    WHERE r.Pay IN (1, 4)
      AND r.iDate BETWEEN @JulS AND @AugE
) x
WHERE Period IS NOT NULL
GROUP BY Period, Variant
ORDER BY Period, Variant;

SELECT
    CASE
        WHEN r.iDate BETWEEN @JulS AND @JulE THEN N'Jul-2026'
        WHEN r.iDate BETWEEN @AugS AND @AugE THEN N'Aug-2026'
    END AS Period,
    r.Salesman,
    CAST(SUM(r.Coll) AS decimal(18,2)) AS FirstPayColl,
    CAST(SUM(CASE WHEN r.ContractVal > 0 AND ISNULL(CASE WHEN r.iDate BETWEEN @JulS AND @JulE THEN la.LifeJul ELSE la.LifeAug END,0)*2+1 >= r.ContractVal THEN r.Coll ELSE 0 END) AS decimal(18,2)) AS EligLifeAll,
    CAST(SUM(CASE WHEN r.ContractVal > 0 AND ISNULL(CASE WHEN r.iDate BETWEEN @JulS AND @JulE THEN lf.LifeJul ELSE lf.LifeAug END,0)*2+1 >= r.ContractVal THEN r.Coll ELSE 0 END) AS decimal(18,2)) AS EligLifeFirst,
    CAST(SUM(CASE WHEN r.ContractVal > 0 AND r.Coll*2+1 >= r.ContractVal THEN r.Coll ELSE 0 END) AS decimal(18,2)) AS EligThisVoucher
FROM rec r
LEFT JOIN life_all la ON la.CustId = r.CustId AND la.ContractVal = r.ContractVal
LEFT JOIN life_first lf ON lf.CustId = r.CustId AND lf.ContractVal = r.ContractVal
WHERE r.Pay IN (1, 4)
  AND r.iDate BETWEEN @JulS AND @AugE
GROUP BY
    CASE
        WHEN r.iDate BETWEEN @JulS AND @JulE THEN N'Jul-2026'
        WHEN r.iDate BETWEEN @AugS AND @AugE THEN N'Aug-2026'
    END,
    r.Salesman
ORDER BY Period, Salesman;

SELECT
    CASE
        WHEN r.iDate BETWEEN @JulS AND @JulE THEN N'Jul-2026'
        WHEN r.iDate BETWEEN @AugS AND @AugE THEN N'Aug-2026'
    END AS Period,
    CAST(SUM(CASE WHEN r.Pay IN (1,4) THEN r.Coll ELSE 0 END) AS decimal(18,2)) AS FirstColl,
    CAST(SUM(CASE WHEN r.Pay IN (2,5) THEN r.Coll ELSE 0 END) AS decimal(18,2)) AS SecondColl,
    CAST(SUM(CASE WHEN r.Pay NOT IN (1,2,4,5) THEN r.Coll ELSE 0 END) AS decimal(18,2)) AS OtherColl,
    CAST(SUM(r.Coll) AS decimal(18,2)) AS AllColl
FROM rec r
WHERE r.iDate BETWEEN @JulS AND @AugE
GROUP BY
    CASE
        WHEN r.iDate BETWEEN @JulS AND @JulE THEN N'Jul-2026'
        WHEN r.iDate BETWEEN @AugS AND @AugE THEN N'Aug-2026'
    END
ORDER BY Period;
"@
$r = $cmd.ExecuteReader()
Write-Output "=== PERIOD / VARIANT ==="
while ($r.Read()) {
    Write-Output ("{0} {1,-14} first={2,12:N2} elig={3,12:N2} rows={4} eligRows={5}" -f $r["Period"], $r["Variant"], $r["FirstPayColl"], $r["Elig"], $r["Rows"], $r["EligRows"])
}
$r.NextResult() | Out-Null
Write-Output ""
Write-Output "=== SALESMAN ==="
while ($r.Read()) {
    $ea = [double]$r["EligLifeAll"]; $ef = [double]$r["EligLifeFirst"]; $ev = [double]$r["EligThisVoucher"]
    function Rate($amt) {
        if ($amt -ge 300000) { return 1.7 }
        elseif ($amt -ge 250000) { return 1.5 }
        elseif ($amt -ge 200000) { return 1.2 }
        elseif ($amt -ge 150000) { return 1.0 }
        else { return 0.0 }
    }
    Write-Output ("{0} | {1,-28} first={2,10:N2} lifeAll={3,10:N2} r={4:N1} | life1={5,10:N2} r={6:N1} | voucher={7,10:N2} r={8:N1}" -f `
        $r["Period"], $r["Salesman"], $r["FirstPayColl"], $ea, (Rate $ea), $ef, (Rate $ef), $ev, (Rate $ev))
}
$r.NextResult() | Out-Null
Write-Output ""
Write-Output "=== PAY MIX ==="
while ($r.Read()) {
    Write-Output ("{0} first={1:N2} second={2:N2} other={3:N2} all={4:N2}" -f $r["Period"], $r["FirstColl"], $r["SecondColl"], $r["OtherColl"], $r["AllColl"])
}
$r.Close(); $conn.Close()
