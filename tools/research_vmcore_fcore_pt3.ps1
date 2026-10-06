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
                    if ($v.Length -gt 220) { $v = $v.Substring(0,220) + "..." }
                    $parts += $v
                }
            }
            Write-Output ($parts -join " | ")
            if ($c -ge 40) { Write-Output "... truncated"; break }
        }
        $r.Close()
        if ($c -eq 0) { Write-Output "(no rows)" }
    } catch { Write-Output $_.Exception.Message }
}

Dump @"
SELECT name, type_desc FROM sys.objects
WHERE name LIKE N'vmCore_Account%' OR name LIKE N'fCore_GetAccount%'
   OR name LIKE N'vaCore_Account%' OR name LIKE N'vrCore_Account%'
ORDER BY type_desc, name
"@ "account objects"

Dump @"
SELECT c.name, t.name AS typ, c.max_length
FROM sys.columns c
JOIN sys.types t ON t.user_type_id = c.user_type_id
WHERE c.object_id = OBJECT_ID(N'dbo.vmCore_Account')
ORDER BY c.column_id
"@ "vmCore_Account columns"

Dump @"
SELECT c.name, t.name AS typ
FROM sys.columns c
JOIN sys.types t ON t.user_type_id = c.user_type_id
WHERE c.object_id = OBJECT_ID(N'dbo.vmCore_Account_0')
ORDER BY c.column_id
"@ "vmCore_Account_0 columns"

Dump @"
SELECT p.name, t.name AS typ
FROM sys.parameters p
JOIN sys.types t ON t.user_type_id = p.user_type_id
WHERE p.object_id = OBJECT_ID(N'dbo.fCore_GetAccountByLevel')
ORDER BY p.parameter_id
"@ "fCore_GetAccountByLevel params"

Dump @"
SELECT name FROM sys.parameters
WHERE object_id IN (
  OBJECT_ID(N'dbo.fCore_GetAccountHierarchy'),
  OBJECT_ID(N'dbo.fCore_GetAccountGroupCodes'),
  OBJECT_ID(N'dbo.fCore_GetAccountGroupNames'),
  OBJECT_ID(N'dbo.fCore_GetAccountClubBy'),
  OBJECT_ID(N'dbo.fCore_GetAccountTreeSequence')
)
"@ "other fCore params"

Dump @"
SELECT COUNT(*) AS vmCnt FROM dbo.vmCore_Account
"@ "vm count"

Dump @"
SELECT COUNT(*) AS vm0Cnt FROM dbo.vmCore_Account_0
"@ "vm0 count"

Dump @"
SELECT COUNT(*) AS mCnt FROM dbo.mCore_Account
"@ "m count"

Dump @"
SELECT COUNT(*) AS vaCnt FROM dbo.vaCore_Account
"@ "va count"

Dump @"
SELECT COUNT(*) AS status3
FROM dbo.vmCore_Account a
WHERE a.ReportStatus = 3 AND CAST(ISNULL(a.bGroup,0) AS int) = 0
"@ "vm status3"

Dump @"
SELECT a.iMasterId, a.sCode, a.sName, a.iAccountType, a.ReportStatus, a.PlanValue, a.bGroup
FROM dbo.vmCore_Account a
WHERE a.sCode = N'AC-847'
"@ "vm AC-847"

Dump @"
SELECT iReportId, sFieldName, sFieldVariable, iControlType, iFieldType, iFieldId
FROM dbo.cCore_ReportParameter_0
WHERE iReportId = 70268
"@ "70268 params"

Dump @"
SELECT iReportId, sReportName, iReportType, iSourceType
FROM dbo.cCore_Reports_0 WHERE iReportId IN (70268,70266,70198,70223)
"@ "report types"

Dump @"
SELECT TOP 8 iReportId, LEFT(sSqlQuery, 180) q
FROM dbo.cCore_RDQuery_0
WHERE sSqlQuery LIKE N'%vmCore_Account%'
ORDER BY iReportId DESC
"@ "queries using vmCore_Account"
