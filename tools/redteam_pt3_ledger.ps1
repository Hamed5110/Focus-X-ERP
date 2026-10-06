$cs = "Server=localhost;Database=Focus80G0;User Id=sa;Password=$env:FOCUS_SQL_PASSWORD;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
function Dump($sql, $title, $timeout=90) {
    Write-Output ""
    Write-Output "========== $title =========="
    $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = $timeout; $cmd.CommandText = $sql
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
                    if ($v.Length -gt 55) { $v = $v.Substring(0,55) + "..." }
                    $parts += $v
                }
            }
            Write-Output ($parts -join " | ")
            if ($c -ge 15) { break }
        }
        $r.Close()
        if ($c -eq 0) { Write-Output "(no rows)" }
    } catch {
        if ($r -and -not $r.IsClosed) { $r.Close() }
        Write-Output $_.Exception.Message
    }
}

Dump "SELECT iMasterId, sName FROM dbo.mCore_Department WHERE iMasterId=2040 OR sName LIKE N'%Atlas%'" "dept Atlas Aluminum"

Dump @"
DECLARE @iEndDate INT = dbo.DateToInt(CONVERT(date, GETDATE()));
SELECT acc.sCode, acc.sName,
    CAST(SUM(CASE WHEN ISNULL(v.Debit,0)<0 THEN -v.Debit WHEN ISNULL(v.Debit,0)>0 THEN v.Debit ELSE 0 END - ISNULL(v.Credit,0)) AS decimal(18,2)) AS LedgerBal,
    CAST(SUM(CASE WHEN ISNULL(v.Debit,0)<0 THEN -v.Debit WHEN ISNULL(v.Debit,0)>0 THEN v.Debit ELSE 0 END) AS decimal(18,2)) AS DispDr,
    CAST(SUM(ISNULL(v.Credit,0)) AS decimal(18,2)) AS FaCredit
FROM dbo.vtCode_DataFA_0 v
JOIN dbo.mCore_Account acc ON acc.iMasterId=v.iMasterId
WHERE v.iMasterId=5247 AND v.iFaTag=2040 AND ISNULL(v.bUpdateFA,0)=1 AND ISNULL(v.iAuth,1)=1
  AND ISNULL(v.bCancelled,0)=0 AND ISNULL(v.bVersion,0)=0 AND ISNULL(v.bSuspended,0)=0 AND ISNULL(v.bVoid,0)=0
  AND v.iDate>0 AND v.iDate<=@iEndDate
GROUP BY acc.sCode, acc.sName
"@ "TRUE AC-847 ledger as-on"

Dump @"
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
"@ "TRUE AC-847 cube contract-adv"

Dump @"
SELECT COUNT(*) AS Status3Leaves
FROM (
    SELECT iMasterId FROM dbo.vaCore_Account
    WHERE ReportStatus=3 AND ISNULL(bGroup,0)=0
    GROUP BY iMasterId
) a
"@ "status3 leaf count"

$sqlFile = Get-Content -Raw "C:\Users\Hamed Ali Khan\Focus-X-ERP\reports\sql\Project Tracking III Report Atlas Ledger.sql"
$sqlFile = [regex]::Replace($sqlFile, '(?s)/\*.*?\*/', '')
$decl = @"
DECLARE @iStartDate INT = dbo.DateToInt(CONVERT(datetime,'20260101',112));
DECLARE @iEndDate INT = dbo.DateToInt(CONVERT(date, GETDATE()));
"@
Write-Output ""
Write-Output "========== FULL QUERY 2026..today =========="
$cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 240
$cmd.CommandText = $decl + $sqlFile
$adapter = New-Object System.Data.SqlClient.SqlDataAdapter $cmd
$dt = New-Object System.Data.DataTable
try {
    [void]$adapter.Fill($dt)
    Write-Output ("ROWS {0}" -f $dt.Rows.Count)
    $sumC=0; $sumA=0; $sumL=0; $sumCubeBal=0; $diffN=0; $dup=0
    $seen = @{}
    foreach ($row in $dt.Rows) {
        $c = [decimal]$row['Total Contract Amount']
        $a = [decimal]$row['Adv. Rct Amount']
        $l = [decimal]$row['Balance Amount']
        $sumC += $c; $sumA += $a; $sumL += $l
        $cubeBal = $c - $a
        $sumCubeBal += $cubeBal
        if ([math]::Abs($l - $cubeBal) -gt 0.5) { $diffN++ }
        $code = [string]$row['Code']
        if ($seen.ContainsKey($code)) { $dup++ } else { $seen[$code] = 1 }
    }
    Write-Output ("SUM Contract {0:n2}" -f $sumC)
    Write-Output ("SUM Adv {0:n2}" -f $sumA)
    Write-Output ("SUM LedgerBal {0:n2}" -f $sumL)
    Write-Output ("SUM Contract-Adv {0:n2}" -f $sumCubeBal)
    Write-Output ("Accounts where ledger <> contract-adv {0}" -f $diffN)
    Write-Output ("Duplicate codes {0}" -f $dup)

    $hit = @($dt.Select("Code = 'AC-847'"))
    if ($hit.Length -gt 0) {
        $rep = [decimal]$hit[0]['Balance Amount']
        Write-Output ("REPORT AC-847 ledger {0} contract {1} adv {2}" -f $rep, $hit[0]['Total Contract Amount'], $hit[0]['Adv. Rct Amount'])
        if ([math]::Abs($rep - (-860.26)) -lt 0.02) { Write-Output "PASS AC-847 ledger matches independent FA -860.26" }
        else { Write-Output "FAIL AC-847 ledger mismatch vs -860.26" }
    } else { Write-Output "NOTE AC-847 not in 2026 activity set (check as-on / period)" }

    Write-Output ""
    Write-Output "========== sample 8 =========="
    $n=0
    foreach ($row in $dt.Rows) {
        $n++
        if ($n -gt 8) { break }
        $cubeBal = [decimal]$row['Total Contract Amount']-[decimal]$row['Adv. Rct Amount']
        Write-Output ("{0} | {1} | C={2} A={3} L={4} cubeBal={5}" -f $row['Code'], $row['Name'], $row['Total Contract Amount'], $row['Adv. Rct Amount'], $row['Balance Amount'], $cubeBal)
    }
} catch {
    Write-Output $_.Exception.Message
}

$conn.Close()
