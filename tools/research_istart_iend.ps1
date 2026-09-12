$cs = "Server=localhost;Database=Focus80E0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
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
                    if ($v.Length -gt 500) { $v = $v.Substring(0,500) + "..." }
                    $parts += $v
                }
            }
            Write-Output ($parts -join " | ")
            if ($c -ge 50) { Write-Output "(truncated)"; break }
        }
        $r.Close()
        if ($c -eq 0) { Write-Output "(no rows)" }
    } catch {
        Write-Output ("ERROR: " + $_.Exception.Message)
    }
}

Dump @"
SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'cCore_ReportParameter_0'
ORDER BY ORDINAL_POSITION
"@ "cCore_ReportParameter_0 columns"

Dump @"
SELECT TOP 30 *
FROM dbo.cCore_ReportParameter_0
WHERE iReportId IN (70266, 70028, 70064, 70165, 70220, 70074, 70259)
ORDER BY iReportId, iParameterId
"@ "params on known reports"

Dump @"
SELECT TOP 40 p.iReportId, r.sName, p.iParameterId
FROM dbo.cCore_ReportParameter_0 p
LEFT JOIN dbo.cCore_Reports_0 r ON r.iReportId = p.iReportId
ORDER BY p.iReportId
"@ "any report parameters sample"

Dump @"
SELECT iReportId, sName
FROM dbo.cCore_Reports_0
WHERE sSqlQuery LIKE N'%@iStartDate%'
   OR sName LIKE N'%iStart%'
"@ "reports named start (wrong col maybe)"

Dump @"
SELECT q.iReportId, r.sName, r.iReportType, r.iSourceType
FROM dbo.cCore_RDQuery_0 q
JOIN dbo.cCore_Reports_0 r ON r.iReportId = q.iReportId
WHERE q.sSqlQuery LIKE N'%@iStartDate%'
   OR q.sSqlQuery LIKE N'%@iEndDate%'
   OR q.sSqlQuery LIKE N'%@FromDate%'
   OR q.sSqlQuery LIKE N'%@ToDate%'
   OR q.sSqlQuery LIKE N'%@StartDate%'
   OR q.sSqlQuery LIKE N'%@EndDate%'
   OR q.sSqlQuery LIKE N'%BETWEEN @%'
ORDER BY q.iReportId
"@ "query reports using @ date params"

Dump @"
SELECT OBJECT_NAME(object_id) N, type
FROM sys.objects
WHERE OBJECT_DEFINITION(object_id) LIKE N'%@iStartDate%'
   OR OBJECT_DEFINITION(object_id) LIKE N'%iStartDate%'
   OR OBJECT_DEFINITION(object_id) LIKE N'%@iEndDate%'
ORDER BY N
"@ "procs/fns mentioning iStartDate"

Dump @"
SELECT iReportId, iVoucherType, iTranSetId, iBRS, iDocumentOption
FROM dbo.cCore_ReportTransactionSet_0
WHERE iReportId = 70266
"@ "70266 transaction set"

Dump @"
SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'cCore_Reports_0'
  AND COLUMN_NAME LIKE '%Param%' OR COLUMN_NAME LIKE '%Date%' OR COLUMN_NAME LIKE '%Sql%'
"@ "report date/param-ish cols"

$conn.Close()
