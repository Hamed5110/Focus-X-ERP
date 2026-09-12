$cs = "Server=localhost;Database=Focus80E0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 180
$cmd.CommandText = @"
DECLARE @S INT = dbo.DateToInt(CONVERT(datetime,'20260801',112));
DECLARE @E INT = dbo.DateToInt(CONVERT(datetime,'20260831',112));

-- 1) Pay-code mix in period (must be 1,4 only in report)
SELECT
    COALESCE(NULLIF(hd4610.PaymentCode,0), NULLIF(hd4609.PaymentCode,0), NULLIF(hd4608.PaymentCode,0), 0) AS Pay,
    COUNT(DISTINCT h.iHeaderId) Docs,
    CAST(SUM(CASE WHEN d.mAmount1>0 THEN d.mAmount1 ELSE 0 END) AS decimal(18,2)) AS Coll
FROM dbo.tCore_Header_0 h
INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId=h.iHeaderId
   AND h.iVoucherType IN (4608,4609,4610)
   AND ISNULL(h.iAuth,1)=1 AND ISNULL(h.bCancelled,0)=0 AND ISNULL(h.bVersion,0)=0 AND ISNULL(h.bSuspended,0)=0
   AND h.iDate BETWEEN @S AND @E AND d.bUpdateFA=1 AND d.iCode>0 AND d.iFaTag=2040
   AND ISNULL(d.iType,0)=0 AND ISNULL(d.bVoid,0)=0 AND ISNULL(d.iAuthStatus,0)<2
INNER JOIN dbo.mCore_Account a ON a.iMasterId=d.iCode AND a.iAccountType IN (5,7)
LEFT JOIN dbo.tCore_HeaderData4610_0 hd4610 ON hd4610.iHeaderId=h.iHeaderId
LEFT JOIN dbo.tCore_HeaderData4609_0 hd4609 ON hd4609.iHeaderId=h.iHeaderId
LEFT JOIN dbo.tCore_HeaderData4608_0 hd4608 ON hd4608.iHeaderId=h.iHeaderId
GROUP BY COALESCE(NULLIF(hd4610.PaymentCode,0), NULLIF(hd4609.PaymentCode,0), NULLIF(hd4608.PaymentCode,0), 0)
ORDER BY Pay;

-- 2) Unique contracts 001/004: gate, no double count
;WITH rec AS (
    SELECT h.iDate, d.iCode AS CustId, a.sName AS Cust,
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
    GROUP BY h.iDate, d.iCode, a.sName, CASE WHEN ISNULL(sm.sName,N'')=N'' THEN N'(blank)' ELSE sm.sName END,
             hd4610.PaymentCode, hd4609.PaymentCode, hd4608.PaymentCode, hd4610.TotalContractAmt, hd4609.ContractAmount
),
life AS (
    SELECT CustId, CVal, SUM(CASE WHEN Pay IN (1,4) AND iDate<=@E THEN Coll ELSE 0 END) AS LifeColl
    FROM rec WHERE CVal>0 GROUP BY CustId, CVal
),
per AS (
    SELECT CustId, MAX(Cust) Cust, MAX(Salesman) Salesman, CVal, SUM(Coll) PeriodColl
    FROM rec
    WHERE Pay IN (1,4) AND iDate BETWEEN @S AND @E
    GROUP BY CustId, CVal
)
SELECT
    p.Salesman, p.Cust, p.CVal, p.PeriodColl, ISNULL(l.LifeColl,0) AS LifeColl,
    CASE WHEN p.CVal>0 THEN CAST(ISNULL(l.LifeColl,0)/p.CVal*100 AS decimal(18,2)) ELSE NULL END AS LifePct,
    CASE WHEN p.CVal>0 AND ISNULL(l.LifeColl,0)*2+1 >= p.CVal THEN 1 ELSE 0 END AS Qual,
    CASE WHEN p.CVal>0 AND ISNULL(l.LifeColl,0)*2+1 >= p.CVal THEN p.CVal ELSE 0 END AS EligC,
    CASE WHEN p.CVal>0 AND ISNULL(l.LifeColl,0)*2+1 >= p.CVal THEN 0 ELSE p.CVal END AS NotEligC
FROM per p
LEFT JOIN life l ON l.CustId=p.CustId AND l.CVal=p.CVal
ORDER BY p.Salesman, p.Cust;

-- 3) report 70267 type
SELECT r.iReportId, r.sReportName, r.iReportType, r.iSourceType
FROM dbo.cCore_Reports_0 r
WHERE r.sReportName LIKE N'%Commission%Atlas%' OR r.iReportId IN (70266,70267);
"@
$r = $cmd.ExecuteReader()
Write-Output "=== PAY MIX Atlas Aug ==="
while ($r.Read()) { Write-Output ("pay={0} docs={1} coll={2:N2}" -f $r["Pay"], $r["Docs"], $r["Coll"]) }
$r.NextResult() | Out-Null
Write-Output ""
Write-Output "=== CONTRACT GATE (independent) ==="
$elig=0.0; $not=0.0; $coll=0.0; $cval=0.0; $n=0; $nq=0
$bySm = @{}
while ($r.Read()) {
    $n++
    $q=[int]$r["Qual"]; if ($q -eq 1) { $nq++ }
    $e=[double]$r["EligC"]; $ne=[double]$r["NotEligC"]; $cv=[double]$r["CVal"]; $pc=[double]$r["PeriodColl"]
    $elig += $e; $not += $ne; $cval += $cv; $coll += $pc
    $sm=[string]$r["Salesman"]
    if (-not $bySm.ContainsKey($sm)) { $bySm[$sm] = @{C=0;P=0;E=0;N=0} }
    $bySm[$sm].C += $cv; $bySm[$sm].P += $pc; $bySm[$sm].E += $e; $bySm[$sm].N += $ne
    $flag = if ($q -eq 1) { "ELIG" } else { "NOT " }
    Write-Output ("{0} | {1,-36} cval={2,10:N2} period={3,9:N2} life={4,9:N2} pct={5,6} {6}" -f $flag, $r["Cust"], $cv, $pc, $r["LifeColl"], $r["LifePct"], $r["Salesman"])
}
Write-Output ""
Write-Output ("CONTRACTS n={0} qual={1} fail={2}" -f $n, $nq, ($n-$nq))
Write-Output ("SUM cval={0:N2} coll={1:N2} elig={2:N2} notElig={3:N2} elig+not={4:N2}" -f $cval, $coll, $elig, $not, ($elig+$not))
Write-Output ("GRID header Overall if SUM repeated 6x = {0:N2}" -f ($elig*6))
Write-Output ""
Write-Output "=== BY SALESMAN vs GRID ==="
$grid = @{
    "(blank)" = @(26096.97,13096.00,23511.24,2585.73)
    "Hassan Ali Ahmed" = @(12981.00,4615.61,4250.40,8730.61)
    "Husain Ali Alsankis" = @(21272.85,10816.85,21272.85,0)
    "Mohamed Hussanin Moustafa" = @(48.80,48.80,48.80,0)
    "Mohamed Khalid" = @(12152.91,6101.91,8151.99,4000.92)
    "Mohammed Jawad" = @(41458.81,20802.00,36408.52,5050.29)
}
foreach ($k in ($bySm.Keys | Sort-Object)) {
    $v = $bySm[$k]
    $g = $grid[$k]
    $ok = $null
    if ($g) {
        $ok = ([math]::Abs($v.C-$g[0]) -lt 0.02) -and ([math]::Abs($v.P-$g[1]) -lt 0.02) -and ([math]::Abs($v.E-$g[2]) -lt 0.02) -and ([math]::Abs($v.N-$g[3]) -lt 0.02)
    }
    Write-Output ("{0,-32} c={1,10:N2} p={2,10:N2} e={3,10:N2} n={4,10:N2} matchGrid={5}" -f $k, $v.C, $v.P, $v.E, $v.N, $ok)
}
$r.NextResult() | Out-Null
Write-Output ""
Write-Output "=== LOCAL REPORT TYPE ==="
while ($r.Read()) { Write-Output ("{0} {1} type={2} source={3}" -f $r["iReportId"], $r["sReportName"], $r["iReportType"], $r["iSourceType"]) }
$r.Close(); $conn.Close()
