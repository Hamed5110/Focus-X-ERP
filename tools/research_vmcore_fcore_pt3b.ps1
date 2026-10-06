$cs = "Server=localhost;Database=Focus80G0;User Id=sa;Password=$env:FOCUS_SQL_PASSWORD;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
function Dump($sql, $title) {
    Write-Output ""
    Write-Output "========== $title =========="
    $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 90; $cmd.CommandText = $sql
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
                    if ($v.Length -gt 400) { $v = $v.Substring(0,400) + "..." }
                    $parts += $v
                }
            }
            Write-Output ($parts -join " | ")
            if ($c -ge 80) { Write-Output "... truncated"; break }
        }
        $r.Close()
        if ($c -eq 0) { Write-Output "(no rows)" }
    } catch { Write-Output $_.Exception.Message }
}

Dump @"
SELECT c.name
FROM sys.columns c
WHERE c.object_id = OBJECT_ID(N'dbo.vmCore_Account')
  AND (
       c.name LIKE N'%CPR%' OR c.name LIKE N'%City%' OR c.name LIKE N'%Tel%'
    OR c.name LIKE N'%Salesman%' OR c.name LIKE N'%Designer%' OR c.name LIKE N'%Pipeline%'
    OR c.name LIKE N'%Site%' OR c.name LIKE N'%SalesModule%' OR c.name LIKE N'%Plan%'
    OR c.name LIKE N'%Controller%' OR c.name LIKE N'%ReportStatus%' OR c.name LIKE N'%Phone%'
    OR c.name LIKE N'%Lang%' OR c.name LIKE N'%Location%'
  )
ORDER BY c.column_id
"@ "vm extra cols"

Dump @"
SELECT iLocationId, iEditingLocation, COUNT(*) cnt
FROM dbo.vmCore_Account
GROUP BY iLocationId, iEditingLocation
"@ "vm location split"

Dump @"
SELECT COUNT(*) FROM (
  SELECT iMasterId FROM dbo.vmCore_Account
  WHERE ReportStatus = 3 AND CAST(ISNULL(bGroup,0) AS int) = 0
  GROUP BY iMasterId
) x
"@ "vm status3 distinct"

Dump @"
SELECT LEFT(OBJECT_DEFINITION(OBJECT_ID(N'dbo.vmCore_Account')), 800)
"@ "vmCore_Account def head"

Dump @"
SELECT LEFT(OBJECT_DEFINITION(OBJECT_ID(N'dbo.vmCore_Account_0')), 800)
"@ "vmCore_Account_0 def head"

Dump @"
SELECT OBJECT_DEFINITION(OBJECT_ID(N'dbo.fCore_GetAccountByLevel'))
"@ "fCore_GetAccountByLevel"

Dump @"
SELECT LEFT(sSqlQuery, 2500)
FROM dbo.cCore_RDQuery_0 WHERE iReportId = 70244
"@ "70244 sql"

Dump @"
SELECT LEFT(sSqlQuery, 400)
FROM dbo.cCore_RDQuery_0 WHERE iReportId = 70268
"@ "70268 sql head"

Dump @"
SELECT p.iReportId, r.sReportName, p.sFieldVariable, LEFT(q.sSqlQuery, 120)
FROM dbo.cCore_ReportParameter_0 p
JOIN dbo.cCore_Reports_0 r ON r.iReportId = p.iReportId
JOIN dbo.cCore_RDQuery_0 q ON q.iReportId = p.iReportId
WHERE p.sFieldVariable = N'@CustomerName'
"@ "all CustomerName queries"
