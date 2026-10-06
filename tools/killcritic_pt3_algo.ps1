$cs = "Server=localhost;Database=Focus80G0;User Id=sa;Password=$env:FOCUS_SQL_PASSWORD;Encrypt=False;TrustServerCertificate=True;"
$q = [System.IO.File]::ReadAllText("C:\Users\Hamed Ali Khan\Focus-X-ERP\reports\sql\Project Tracking III Report Atlas Ledger.sql")
$start = 2021*65536 + 256 + 1
$end = [int](Get-Date).Year*65536 + [int](Get-Date).Month*256 + [int](Get-Date).Day
$run = $q.Replace("@CustomerName","0").Replace("@iStartDate","$start").Replace("@iEndDate","$end")
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
function Dump($sql, $title, $timeout=180) {
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
                    if ($v.Length -gt 120) { $v = $v.Substring(0,120) }
                    $parts += $v
                }
            }
            Write-Output ($parts -join " | ")
            if ($c -ge 25) { Write-Output "..."; break }
        }
        $r.Close()
        if ($c -eq 0) { Write-Output "(no rows)" }
    } catch { Write-Output $_.Exception.Message }
}

Dump @"
SELECT OBJECT_DEFINITION(OBJECT_ID(N'dbo.vtCode_DataFA_0')) Def
"@ "vtCode_DataFA_0 definition" 30

Dump @"
SELECT COUNT(*) Status3Masters
FROM dbo.mCore_Account m
JOIN dbo.muCore_Account u ON u.iMasterId = m.iMasterId
WHERE ISNULL(m.bGroup,0)=0 AND u.ReportStatus=3
"@ "status3 grain"

Dump @"
SELECT COUNT(*) Cnt, COUNT(DISTINCT m.sName) DistinctNames,
       SUM(CASE WHEN x.n>1 THEN 1 ELSE 0 END) DupNameCodes
FROM dbo.mCore_Account m
JOIN dbo.muCore_Account u ON u.iMasterId = m.iMasterId
LEFT JOIN (
    SELECT sName, COUNT(*) n
    FROM dbo.mCore_Account
    WHERE ISNULL(bGroup,0)=0
    GROUP BY sName
    HAVING COUNT(*)>1
) x ON x.sName = m.sName
WHERE ISNULL(m.bGroup,0)=0 AND u.ReportStatus=3
"@ "duplicate names among status3"

Dump @"
SELECT COUNT(*) vmRows, COUNT(DISTINCT iMasterId) vmIds
FROM dbo.vmCore_Account
WHERE ISNULL(bGroup,0)=0 AND ReportStatus=3
"@ "vmCore duplicate"

Dump @"
SELECT COUNT(*) Cnt,
       SUM(CASE WHEN Code=N'AC-847' THEN [Total Contract Amount] ELSE 0 END) C847,
       SUM(CASE WHEN Code=N'AC-847' THEN [Adv. Rct Amount] ELSE 0 END) A847,
       SUM(CASE WHEN Code=N'AC-847' THEN [Balance Amount] ELSE 0 END) B847,
       SUM([Total Contract Amount]) SumC,
       SUM([Adv. Rct Amount]) SumA,
       SUM([Balance Amount]) SumB,
       SUM([Total Contract Amount])-SUM([Adv. Rct Amount]) CubeBalStyle
FROM ($run) r
"@ "query V&V"

Dump @"
SELECT
  (SELECT ROUND(SUM(fNet)*-1,0) FROM (
      SELECT h.iHeaderId, MAX(h.fNet) fNet
      FROM dbo.tCore_Header_0 h
      JOIN dbo.tCore_Data_0 d ON d.iHeaderId=h.iHeaderId
      WHERE h.iVoucherType=5634 AND d.iFaTag=2040 AND d.iBookNo=5247
        AND ISNULL(d.iType,0)=0 AND ISNULL(h.bCancelled,0)=0
        AND ISNULL(h.bVersion,0)=0 AND ISNULL(h.bSuspended,0)=0 AND ISNULL(d.bVoid,0)=0
        AND ISNULL(h.iAuth,1)=1 AND h.iDate BETWEEN $start AND $end
      GROUP BY h.iHeaderId
  ) x) AuthContract,
  (SELECT ROUND(SUM(fNet)*-1,0) FROM (
      SELECT h.iHeaderId, MAX(h.fNet) fNet
      FROM dbo.tCore_Header_0 h
      JOIN dbo.tCore_Data_0 d ON d.iHeaderId=h.iHeaderId
      WHERE h.iVoucherType=5634 AND d.iFaTag=2040 AND d.iBookNo=5247
        AND ISNULL(d.iType,0)=0 AND ISNULL(h.bCancelled,0)=0
        AND ISNULL(h.bVersion,0)=0 AND ISNULL(h.bSuspended,0)=0 AND ISNULL(d.bVoid,0)=0
        AND h.iDate BETWEEN $start AND $end
      GROUP BY h.iHeaderId
  ) y) AuthPlusUnauthContract
"@ "AC-847 auth vs DocOption62"

Dump @"
SELECT
  SUM(CASE WHEN ISNULL(v.Debit,0)<0 THEN -v.Debit WHEN ISNULL(v.Debit,0)>0 THEN v.Debit ELSE 0 END - ISNULL(v.Credit,0)) LedFlip,
  SUM(ISNULL(v.Debit,0)-ISNULL(v.Credit,0)) LedRaw
FROM dbo.vtCode_DataFA_0 v
WHERE v.iMasterId=5247 AND v.iFaTag=2040 AND ISNULL(v.bUpdateFA,0)=1
  AND ISNULL(v.iAuth,1)=1 AND ISNULL(v.bCancelled,0)=0 AND ISNULL(v.bVersion,0)=0
  AND ISNULL(v.bSuspended,0)=0 AND ISNULL(v.bVoid,0)=0 AND v.iDate>0 AND v.iDate<=$end
"@ "AC-847 ledger flip vs raw"

Dump @"
SELECT COUNT(*) Acc2SameCode
FROM dbo.tCore_Header_0 h
JOIN dbo.tCore_Data_0 d ON d.iHeaderId=h.iHeaderId
WHERE h.iVoucherType IN (256,4096,4608,4609,4610,8707)
  AND d.bUpdateFA=1 AND d.iFaTag=2040 AND d.iBookNo=d.iCode AND d.iBookNo>0
  AND d.mAmount2>0 AND ISNULL(d.iType,0)=0 AND ISNULL(h.iAuth,1)=1
  AND h.iDate BETWEEN $start AND $end
"@ "Acc2 same as Acc would double-count if not excluded"

Dump @"
SELECT TOP 8 m.sCode, m.sName, n.Cnt
FROM dbo.mCore_Account m
JOIN dbo.muCore_Account u ON u.iMasterId=m.iMasterId
JOIN (
    SELECT sName, COUNT(*) Cnt FROM dbo.mCore_Account WHERE ISNULL(bGroup,0)=0 GROUP BY sName HAVING COUNT(*)>1
) n ON n.sName=m.sName
WHERE u.ReportStatus=3 AND ISNULL(m.bGroup,0)=0
ORDER BY n.Cnt DESC, m.sName
"@ "status3 codes that share a name"

$conn.Close()
