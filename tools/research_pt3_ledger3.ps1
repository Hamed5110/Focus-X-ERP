$cs = "Server=localhost;Database=Focus80G0;User Id=sa;Password=$env:FOCUS_SQL_PASSWORD;Encrypt=False;TrustServerCertificate=True;"
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
                    if ($v.Length -gt 60) { $v = $v.Substring(0,60) + "..." }
                    $parts += $v
                }
            }
            Write-Output ($parts -join " | ")
            if ($c -ge 20) { break }
        }
        $r.Close()
        if ($c -eq 0) { Write-Output "(no rows)" }
    } catch { Write-Output $_.Exception.Message }
}

Dump @"
SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = N'vaCore_Account'
  AND (COLUMN_NAME LIKE N'%Tel%' OR COLUMN_NAME LIKE N'%Phone%' OR COLUMN_NAME LIKE N'%Sales%'
    OR COLUMN_NAME LIKE N'%Site%' OR COLUMN_NAME LIKE N'%Module%')
ORDER BY COLUMN_NAME
"@ "tel/sales/site cols"

Dump @"
SELECT TOP 12
    v.iVoucherType, v.sVoucherNo, v.Debit, v.Credit, v.mAmount1, v.mAmount2, v.iCode, v.iBookNo, v.iMasterId
FROM dbo.vtCode_DataFA_0 v
WHERE v.iMasterId = 5247 AND v.iFaTag = 2040
  AND ISNULL(v.bCancelled,0)=0 AND ISNULL(v.bVersion,0)=0 AND ISNULL(v.bSuspended,0)=0
  AND ISNULL(v.bVoid,0)=0 AND ISNULL(v.bUpdateFA,0)=1
ORDER BY v.iDate, v.iHeaderId
"@ "AC-847 FA lines"

Dump @"
SELECT
    SUM(ISNULL(Debit,0)) AS SumDebit,
    SUM(ISNULL(Credit,0)) AS SumCredit,
    SUM(CASE WHEN Debit < 0 THEN -Debit ELSE ISNULL(Debit,0) END) AS AbsNegDebit,
    SUM(CASE WHEN mAmount2 < 0 THEN -mAmount2 ELSE 0 END) AS DrFromAmt2,
    SUM(CASE WHEN mAmount2 > 0 THEN mAmount2 ELSE 0 END) AS CrFromAmt2,
    SUM(-ISNULL(mAmount2,0)) AS NegAmt2,
    SUM(ISNULL(Debit,0) - ISNULL(Credit,0)) AS DebitMinusCredit
FROM dbo.vtCode_DataFA_0 v
WHERE v.iMasterId = 5247 AND v.iFaTag = 2040
  AND ISNULL(v.bCancelled,0)=0 AND ISNULL(v.bVersion,0)=0 AND ISNULL(v.bSuspended,0)=0
  AND ISNULL(v.bVoid,0)=0 AND ISNULL(v.bUpdateFA,0)=1 AND ISNULL(v.iAuth,1)=1
"@ "AC-847 ledger formulas"

Dump @"
-- cube-style contract - adv for AC-847
SELECT
    (SELECT ROUND(SUM(x.fNet)*-1,0) FROM (
        SELECT MAX(h.fNet) fNet
        FROM tCore_Header_0 h
        JOIN tCore_Data_0 d ON d.iHeaderId=h.iHeaderId AND h.iVoucherType=5634
          AND d.iFaTag=2040 AND d.iBookNo=5247 AND ISNULL(d.iType,0)=0 AND ISNULL(h.iAuth,1)=1
          AND ISNULL(h.bCancelled,0)=0 AND ISNULL(h.bVersion,0)=0 AND ISNULL(h.bSuspended,0)=0 AND ISNULL(d.bVoid,0)=0
        GROUP BY h.iHeaderId
    ) x) AS Contract,
    (SELECT ROUND(SUM(CASE WHEN iVoucherType=4610 THEN ROUND(TypeAmt,0) ELSE ROUND(TypeAmt,2) END),0)
     FROM (
        SELECT iVoucherType, SUM(CreditAmt) TypeAmt FROM (
            SELECT h.iVoucherType, CASE WHEN d.mAmount1>0 THEN d.mAmount1 ELSE 0 END CreditAmt
            FROM tCore_Header_0 h JOIN tCore_Data_0 d ON d.iHeaderId=h.iHeaderId
            WHERE h.iVoucherType IN (256,4096,4608,4609,4610,8707) AND d.bUpdateFA=1 AND d.iFaTag=2040
              AND d.iCode=5247 AND ISNULL(h.iAuth,1)=1 AND ISNULL(h.bCancelled,0)=0 AND ISNULL(h.bVersion,0)=0
            UNION ALL
            SELECT h.iVoucherType, CASE WHEN d.mAmount2>0 THEN d.mAmount2 ELSE 0 END
            FROM tCore_Header_0 h JOIN tCore_Data_0 d ON d.iHeaderId=h.iHeaderId
            WHERE h.iVoucherType IN (256,4096,4608,4609,4610,8707) AND d.bUpdateFA=1 AND d.iFaTag=2040
              AND d.iBookNo=5247 AND d.iBookNo<>d.iCode AND ISNULL(h.iAuth,1)=1 AND ISNULL(h.bCancelled,0)=0
        ) z GROUP BY iVoucherType
     ) y) AS Adv
"@ "AC-847 cube contract adv"
