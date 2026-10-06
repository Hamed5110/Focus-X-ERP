$cs = "Server=localhost;Database=Focus80G0;User Id=sa;Password=$env:FOCUS_SQL_PASSWORD;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
function Q($sql, $timeout=60) {
    $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = $timeout; $cmd.CommandText = $sql
    $da = New-Object System.Data.SqlClient.SqlDataAdapter $cmd
    $dt = New-Object System.Data.DataTable
    [void]$da.Fill($dt)
    return $dt
}
function Dump($dt, $title, $max=12) {
    Write-Output ""
    Write-Output "========== $title =========="
    if ($dt.Rows.Count -eq 0) { Write-Output "(no rows)"; return }
    $cols = ($dt.Columns | ForEach-Object { $_.ColumnName }) -join " | "
    Write-Output $cols
    $n = 0
    foreach ($r in $dt.Rows) {
        $n++
        $parts = @()
        foreach ($c in $dt.Columns) {
            $v = [string]$r[$c.ColumnName]
            $v = $v -replace "`r|`n"," "
            if ($v.Length -gt 50) { $v = $v.Substring(0,50) + "..." }
            $parts += $v
        }
        Write-Output ($parts -join " | ")
        if ($n -ge $max) { break }
    }
}

Dump (Q "SELECT iMasterId, sName FROM dbo.mCore_Department WHERE iMasterId=2040 OR sName LIKE N'%Atlas%'") "dept Atlas"

Dump (Q @"
DECLARE @iEndDate INT = dbo.DateToInt(CONVERT(date, GETDATE()));
SELECT acc.sCode,
    CAST(SUM(CASE WHEN ISNULL(v.Debit,0)<0 THEN -v.Debit WHEN ISNULL(v.Debit,0)>0 THEN v.Debit ELSE 0 END - ISNULL(v.Credit,0)) AS decimal(18,2)) AS LedgerBal,
    CAST(SUM(ISNULL(v.Credit,0)) AS decimal(18,2)) AS FaCredit,
    CAST(ROUND(SUM(CASE WHEN v.iVoucherType IN (256,4096,4608,4609,4610,8707) THEN CASE WHEN v.iVoucherType=4610 THEN ROUND(ISNULL(v.Credit,0),0) ELSE ROUND(ISNULL(v.Credit,0),2) END ELSE 0 END),0) AS decimal(18,2)) AS FaAdv6
FROM dbo.vtCode_DataFA_0 v
JOIN dbo.mCore_Account acc ON acc.iMasterId=v.iMasterId
WHERE v.iMasterId=5247 AND v.iFaTag=2040 AND ISNULL(v.bUpdateFA,0)=1 AND ISNULL(v.iAuth,1)=1
  AND ISNULL(v.bCancelled,0)=0 AND ISNULL(v.bVersion,0)=0 AND ISNULL(v.bSuspended,0)=0 AND ISNULL(v.bVoid,0)=0
  AND v.iDate>0 AND v.iDate<=@iEndDate
GROUP BY acc.sCode
"@) "AC-847 ledger + FA-adv"

Dump (Q @"
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
              AND d.iCode=5247 AND ISNULL(d.iType,0)=0 AND ISNULL(h.iAuth,1)=1 AND ISNULL(d.iAuthStatus,0)<2
              AND ISNULL(h.bCancelled,0)=0 AND ISNULL(h.bVersion,0)=0 AND ISNULL(h.bSuspended,0)=0 AND ISNULL(d.bVoid,0)=0
            UNION ALL
            SELECT h.iVoucherType, CASE WHEN d.mAmount2>0 THEN d.mAmount2 ELSE 0 END
            FROM tCore_Header_0 h JOIN tCore_Data_0 d ON d.iHeaderId=h.iHeaderId
            WHERE h.iVoucherType IN (256,4096,4608,4609,4610,8707) AND d.bUpdateFA=1 AND d.iFaTag=2040
              AND d.iBookNo=5247 AND d.iBookNo<>d.iCode AND ISNULL(d.iType,0)=0 AND ISNULL(h.iAuth,1)=1 AND ISNULL(d.iAuthStatus,0)<2
              AND ISNULL(h.bCancelled,0)=0 AND ISNULL(h.bVersion,0)=0 AND ISNULL(h.bSuspended,0)=0 AND ISNULL(d.bVoid,0)=0
        ) z GROUP BY iVoucherType
     ) y) AS CubeAdv
"@) "AC-847 cube contract/adv"

Dump (Q @"
SELECT COUNT(*) AS Status3Leaves
FROM (
    SELECT iMasterId FROM dbo.vaCore_Account
    WHERE ReportStatus=3 AND ISNULL(bGroup,0)=0
    GROUP BY iMasterId
) a
"@) "status3 count"

$conn.Close()
