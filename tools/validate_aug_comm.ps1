$cs = "Server=localhost;Database=Focus80E0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
function Dump($sql) {
    $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 180; $cmd.CommandText = $sql
    $r = $cmd.ExecuteReader()
    $n = $r.FieldCount
    $hdr = @(); for ($i=0;$i -lt $n;$i++) { $hdr += $r.GetName($i) }
    Write-Output ("COLS: " + ($hdr -join " | "))
    $c = 0
    while ($r.Read()) {
        $parts = @()
        for ($i = 0; $i -lt $n; $i++) {
            if ($r.IsDBNull($i)) { $parts += "" } else { $parts += [string]$r.GetValue($i) }
        }
        Write-Output ($parts -join " | ")
        $c++
        if ($c -ge 80) { break }
    }
    $r.Close()
    Write-Output ("-- shown: $c --")
}

Write-Output "=== Aug Atlas header-level SO ==="
Dump @"
SELECT COUNT(*) Cnt, CAST(SUM(NetAbs) AS decimal(18,2)) SumAbs
FROM (
    SELECT h.iHeaderId, MAX(ABS(h.fNet)) AS NetAbs
    FROM dbo.tCore_Header_0 h
    INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId=h.iHeaderId
    WHERE h.iVoucherType=5634 AND d.iFaTag=2040
      AND ISNULL(h.bCancelled,0)=0 AND ISNULL(h.bVersion,0)=0 AND ISNULL(h.bSuspended,0)=0
      AND ISNULL(d.iType,0)=0 AND ISNULL(d.bVoid,0)=0 AND ISNULL(h.iAuth,1)=1
      AND (h.iDate/65536)=2026 AND ((h.iDate/256)%256)=8
      AND h.sVoucherNo LIKE 'CON-Atl-%'
    GROUP BY h.iHeaderId
) x
"@

Write-Output "`n=== Aknan SO prefixes and departments ==="
Dump @"
SELECT LEFT(h.sVoucherNo,8) Pfx, d.iFaTag, dep.sName, COUNT(DISTINCT h.iHeaderId) Cnt
FROM dbo.tCore_Header_0 h
INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId=h.iHeaderId AND ISNULL(d.iType,0)=0
LEFT JOIN dbo.mCore_Department dep ON dep.iMasterId=d.iFaTag
WHERE h.iVoucherType=5634 AND h.sVoucherNo LIKE 'CON-%'
  AND ISNULL(h.bCancelled,0)=0 AND ISNULL(h.bVersion,0)=0
GROUP BY LEFT(h.sVoucherNo,8), d.iFaTag, dep.sName
ORDER BY Cnt DESC
"@

Write-Output "`n=== Aug Atlas with CRM adv match ==="
Dump @"
;WITH so AS (
    SELECT h.iHeaderId, h.sVoucherNo, h.iDate, MAX(d.iBookNo) AccId, MAX(ABS(h.fNet)) ContractAmt,
           MAX(ISNULL(hd.OpportunityType,0)) Opp
    FROM dbo.tCore_Header_0 h
    INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId=h.iHeaderId AND d.iFaTag=2040 AND ISNULL(d.iType,0)=0 AND ISNULL(d.bVoid,0)=0
    LEFT JOIN dbo.tCore_HeaderData5634_0 hd ON hd.iHeaderId=h.iHeaderId
    WHERE h.iVoucherType=5634 AND ISNULL(h.bCancelled,0)=0 AND ISNULL(h.bVersion,0)=0 AND ISNULL(h.bSuspended,0)=0
      AND ISNULL(h.iAuth,1)=1
      AND (h.iDate/65536)=2026 AND ((h.iDate/256)%256)=8
      AND h.sVoucherNo LIKE 'CON-Atl-%'
    GROUP BY h.iHeaderId, h.sVoucherNo, h.iDate
)
SELECT
    COUNT(*) SoCnt,
    SUM(CASE WHEN adv.AdvAmt IS NOT NULL THEN 1 ELSE 0 END) MatchedAdv,
    CAST(SUM(so.ContractAmt) AS decimal(18,2)) AllContract,
    CAST(SUM(CASE WHEN so.Opp IN (1,2) AND so.ContractAmt>0 THEN so.ContractAmt ELSE 0 END) AS decimal(18,2)) SalesTypes,
    CAST(SUM(ISNULL(adv.AdvAmt,0)) AS decimal(18,2)) AllAdv,
    CAST(SUM(CASE WHEN so.Opp IN (1,2) AND so.ContractAmt>0 AND ISNULL(adv.AdvAmt,0)*2+1 >= so.ContractAmt THEN so.ContractAmt ELSE 0 END) AS decimal(18,2)) Qualified
FROM so
OUTER APPLY (
    SELECT SUM(CASE WHEN d.mAmount1>0 THEN d.mAmount1 ELSE 0 END) AdvAmt
    FROM dbo.tCore_Header_0 rh
    INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId=rh.iHeaderId
    INNER JOIN dbo.tCore_HeaderData4610_0 rhd ON rhd.iHeaderId=rh.iHeaderId
    WHERE rh.iVoucherType=4610 AND d.iCode=so.AccId AND d.iFaTag=2040
      AND ISNULL(rh.bCancelled,0)=0 AND ISNULL(rh.bVersion,0)=0 AND ISNULL(rh.iAuth,1)=1
      AND ABS(ISNULL(rhd.TotalContractAmt,0) - so.ContractAmt) < 0.05
) adv
"@

Write-Output "`n=== payroll attendance tables sample cols ==="
Dump @"
SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME='vPay_PayrollAttendance'
ORDER BY ORDINAL_POSITION
"@

$conn.Close()
