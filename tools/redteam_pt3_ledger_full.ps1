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
                    if ($v.Length -gt 60) { $v = $v.Substring(0,60) + "..." }
                    $parts += $v
                }
            }
            Write-Output ($parts -join " | ")
            if ($c -ge 12) { break }
        }
        $r.Close()
        if ($c -eq 0) { Write-Output "(no rows)" }
    } catch {
        if ($r -and -not $r.IsClosed) { $r.Close() }
        Write-Output $_.Exception.Message
    }
}

Dump @"
SELECT TOP 8 acc.sCode, acc.sName,
    CAST(SUM(CASE WHEN ISNULL(v.Debit,0)<0 THEN -v.Debit WHEN ISNULL(v.Debit,0)>0 THEN v.Debit ELSE 0 END - ISNULL(v.Credit,0)) AS decimal(18,2)) AS Ledger
FROM dbo.vtCode_DataFA_0 v
JOIN dbo.mCore_Account acc ON acc.iMasterId=v.iMasterId
JOIN (
    SELECT iMasterId FROM dbo.vaCore_Account WHERE ReportStatus=3 AND ISNULL(bGroup,0)=0 GROUP BY iMasterId
) s3 ON s3.iMasterId=v.iMasterId
WHERE v.iFaTag=2040 AND ISNULL(v.bUpdateFA,0)=1 AND ISNULL(v.iAuth,1)=1
  AND ISNULL(v.bCancelled,0)=0 AND ISNULL(v.bVersion,0)=0 AND ISNULL(v.bSuspended,0)=0 AND ISNULL(v.bVoid,0)=0
  AND v.iDate>0 AND v.iDate<=dbo.DateToInt(CONVERT(date, GETDATE()))
GROUP BY acc.sCode, acc.sName
HAVING ABS(SUM(CASE WHEN ISNULL(v.Debit,0)<0 THEN -v.Debit WHEN ISNULL(v.Debit,0)>0 THEN v.Debit ELSE 0 END - ISNULL(v.Credit,0)))>0
ORDER BY acc.sCode
"@ "sample independent ledger status3"

$sqlFile = Get-Content -Raw "C:\Users\Hamed Ali Khan\Focus-X-ERP\reports\sql\Project Tracking III Report Atlas Ledger.sql"
$sqlFile = [regex]::Replace($sqlFile, '(?s)/\*.*?\*/', '')
$decl = @"
DECLARE @iStartDate INT = dbo.DateToInt(CONVERT(datetime,'20210101',112));
DECLARE @iEndDate INT = dbo.DateToInt(CONVERT(date, GETDATE()));
"@
Write-Output ""
Write-Output "========== FULL QUERY 2021..today =========="
$cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 240
$cmd.CommandText = $decl + $sqlFile
$adapter = New-Object System.Data.SqlClient.SqlDataAdapter $cmd
$dt = New-Object System.Data.DataTable
try {
    [void]$adapter.Fill($dt)
    Write-Output ("ROWS {0}" -f $dt.Rows.Count)
    $sumC=0; $sumA=0; $sumL=0; $sumCubeBal=0; $diffN=0
    foreach ($row in $dt.Rows) {
        $c = [decimal]$row['Total Contract Amount']
        $a = [decimal]$row['Adv. Rct Amount']
        $l = [decimal]$row['Balance Amount']
        $sumC += $c; $sumA += $a; $sumL += $l
        if ([math]::Abs($l - ($c-$a)) -gt 0.5) { $diffN++ }
    }
    Write-Output ("SUM Contract {0:n2}" -f $sumC)
    Write-Output ("SUM Adv {0:n2}" -f $sumA)
    Write-Output ("SUM LedgerBal {0:n2}" -f $sumL)
    Write-Output ("SUM Contract-Adv {0:n2}" -f ($sumC-$sumA))
    Write-Output ("Accounts where ledger <> contract-adv {0}" -f $diffN)

    $hit = @($dt.Select("Code = 'AC-847'"))
    if ($hit.Length -gt 0) {
        Write-Output ("REPORT AC-847 C={0} A={1} L={2} cubeBal={3}" -f $hit[0]['Total Contract Amount'], $hit[0]['Adv. Rct Amount'], $hit[0]['Balance Amount'], ([decimal]$hit[0]['Total Contract Amount']-[decimal]$hit[0]['Adv. Rct Amount']))
        $l = [decimal]$hit[0]['Balance Amount']
        $c = [decimal]$hit[0]['Total Contract Amount']
        $a = [decimal]$hit[0]['Adv. Rct Amount']
        if ([math]::Abs($l - (-860.26)) -lt 0.02) { Write-Output "PASS ledger -860.26" } else { Write-Output "FAIL ledger" }
        if ([math]::Abs($c - 7533) -lt 1) { Write-Output "PASS contract ~7533" } else { Write-Output ("NOTE contract {0} vs all-time 7533" -f $c) }
        if ([math]::Abs($a - 8213) -lt 1) { Write-Output "PASS adv ~8213" } else { Write-Output ("NOTE adv {0} vs all-time 8213" -f $a) }
        if ([math]::Abs($l - ($c-$a)) -gt 0.5) { Write-Output "PASS balance is ledger not c11-c12" } else { Write-Output "FAIL balance still equals contract-adv" }
    } else { Write-Output "FAIL AC-847 missing on 2021..today" }

    # cross-check 5 report rows vs independent FA
    Write-Output ""
    Write-Output "========== TRUE vs REPORT 5 codes =========="
    $codes = @($dt.Rows | Select-Object -First 5 | ForEach-Object { $_.Code })
    $inList = ($codes | ForEach-Object { "N'$_'" }) -join ","
    $cmd.CommandText = @"
DECLARE @iEndDate INT = dbo.DateToInt(CONVERT(date, GETDATE()));
SELECT acc.sCode,
    CAST(SUM(CASE WHEN ISNULL(v.Debit,0)<0 THEN -v.Debit WHEN ISNULL(v.Debit,0)>0 THEN v.Debit ELSE 0 END - ISNULL(v.Credit,0)) AS decimal(18,2)) AS Indep
FROM dbo.vtCode_DataFA_0 v
JOIN dbo.mCore_Account acc ON acc.iMasterId=v.iMasterId
WHERE acc.sCode IN ($inList) AND v.iFaTag=2040 AND ISNULL(v.bUpdateFA,0)=1 AND ISNULL(v.iAuth,1)=1
  AND ISNULL(v.bCancelled,0)=0 AND ISNULL(v.bVersion,0)=0 AND ISNULL(v.bSuspended,0)=0 AND ISNULL(v.bVoid,0)=0
  AND v.iDate>0 AND v.iDate<=@iEndDate
GROUP BY acc.sCode
"@
    $r = $cmd.ExecuteReader()
    $indep = @{}
    while ($r.Read()) { $indep[[string]$r['sCode']] = [decimal]$r['Indep'] }
    $r.Close()
    $pass=0; $fail=0
    foreach ($code in $codes) {
        $rep = [decimal](@($dt.Select("Code = '$code'"))[0]['Balance Amount'])
        $ind = 0
        if ($indep.ContainsKey($code)) { $ind = $indep[$code] }
        $ok = [math]::Abs($rep - $ind) -lt 0.02
        if ($ok) { $pass++ } else { $fail++ }
        Write-Output ("{0} report={1} indep={2} {3}" -f $code, $rep, $ind, $(if($ok){'PASS'}else{'FAIL'}))
    }
    Write-Output ("CROSSCHECK $pass PASS / $fail FAIL")
} catch {
    Write-Output $_.Exception.Message
}
$conn.Close()
