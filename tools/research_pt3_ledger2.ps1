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
                    if ($v.Length -gt 70) { $v = $v.Substring(0,70) + "..." }
                    $parts += $v
                }
            }
            Write-Output ($parts -join " | ")
            if ($c -ge 25) { break }
        }
        $r.Close()
        if ($c -eq 0) { Write-Output "(no rows)" }
    } catch { Write-Output $_.Exception.Message }
}

Dump @"
SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = N'vaCore_Account' AND (
    COLUMN_NAME LIKE N'%CPR%' OR COLUMN_NAME LIKE N'%City%' OR COLUMN_NAME LIKE N'%Tel%'
    OR COLUMN_NAME LIKE N'%Salesman%' OR COLUMN_NAME LIKE N'%Designer%' OR COLUMN_NAME LIKE N'%Pipeline%'
    OR COLUMN_NAME LIKE N'%Site%' OR COLUMN_NAME LIKE N'%Plan%' OR COLUMN_NAME LIKE N'%Sales Module%'
    OR COLUMN_NAME LIKE N'%Controller%' OR COLUMN_NAME LIKE N'%ReportStatus%' OR COLUMN_NAME LIKE N'%Phone%'
)
ORDER BY COLUMN_NAME
"@ "va extra fields"

Dump @"
SELECT iMasterId, sCode, sName, ReportStatus, PlanValue, CPRCRNumber, SalesmannameName, DesignernameName
FROM dbo.vaCore_Account
WHERE ReportStatus = 3 AND ISNULL(bGroup,0)=0
GROUP BY iMasterId, sCode, sName, ReportStatus, PlanValue, CPRCRNumber, SalesmannameName, DesignernameName
"@ "status3 sample fail if dups"

Dump @"
SELECT TOP 8
    v.iMasterId, acc.sCode, acc.sName,
    SUM(ISNULL(v.Debit,0)) AS Debit,
    SUM(ISNULL(v.Credit,0)) AS Credit,
    SUM(ISNULL(v.Debit,0) - ISNULL(v.Credit,0)) AS LedBal
FROM dbo.vtCode_DataFA_0 v
INNER JOIN dbo.mCore_Account acc ON acc.iMasterId = v.iMasterId
INNER JOIN dbo.vaCore_Account va ON va.iMasterId = v.iMasterId AND va.ReportStatus = 3
WHERE v.iFaTag = 2040
  AND ISNULL(v.bCancelled,0)=0 AND ISNULL(v.bVersion,0)=0 AND ISNULL(v.bSuspended,0)=0
  AND ISNULL(v.bVoid,0)=0 AND ISNULL(v.bUpdateFA,0)=1 AND ISNULL(v.iAuth,1)=1
GROUP BY v.iMasterId, acc.sCode, acc.sName
ORDER BY acc.sCode
"@ "ledger bal status3 atlas"
