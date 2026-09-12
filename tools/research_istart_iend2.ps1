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
                    if ($v.Length -gt 600) { $v = $v.Substring(0,600) + "..." }
                    $parts += $v
                }
            }
            Write-Output ($parts -join " | ")
            if ($c -ge 60) { Write-Output "(truncated)"; break }
        }
        $r.Close()
        if ($c -eq 0) { Write-Output "(no rows)" }
    } catch {
        Write-Output ("ERROR: " + $_.Exception.Message)
    }
}

Dump @"
SELECT COLUMN_NAME, DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'cCore_Reports_0'
ORDER BY ORDINAL_POSITION
"@ "cCore_Reports_0 all columns"

Dump @"
SELECT iReportId, iStartDate, iEndDate, iReportType, iSourceType
FROM dbo.cCore_Reports_0
WHERE iReportId IN (70266, 70028, 70064, 70165, 70220, 70074, 70259, 70219, 70221)
"@ "start/end on key reports"

Dump @"
SELECT TOP 20 iReportId, iStartDate, iEndDate,
  iStartDate / 65536 AS SY, (iStartDate / 256) % 256 AS SM, iStartDate % 256 AS SD,
  iEndDate / 65536 AS EY, (iEndDate / 256) % 256 AS EM, iEndDate % 256 AS ED
FROM dbo.cCore_Reports_0
WHERE iStartDate > 0
ORDER BY iReportId DESC
"@ "unpack report iStartDate/iEndDate"

Dump @"
SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'cCore_Reports_0' AND COLUMN_NAME LIKE '%Name%'
"@ "report name columns"

Dump @"
SELECT q.iReportId, LEFT(q.sSqlQuery, 80) AS Head
FROM dbo.cCore_RDQuery_0 q
WHERE q.sSqlQuery LIKE N'%@%'
ORDER BY q.iReportId
"@ "query SQL containing @"

Dump @"
SELECT p.iParameterId, p.iReportId, p.sFieldName, p.sFieldVariable, p.iControlType, p.iFieldType, p.iSelectionMode, p.sValue, p.iFieldId, p.iType, p.sDefault
FROM dbo.cCore_ReportParameter_0 p
WHERE p.sFieldVariable LIKE N'%Date%'
   OR p.sFieldName LIKE N'%Date%'
   OR p.sFieldVariable LIKE N'%Start%'
   OR p.sFieldVariable LIKE N'%End%'
ORDER BY p.iReportId
"@ "date-named report parameters"

Dump @"
SELECT TOP 25 p.iParameterId, p.iReportId, p.sFieldName, p.sFieldVariable, p.iControlType, p.iFieldType, p.iSelectionMode, p.sDefault
FROM dbo.cCore_ReportParameter_0 p
ORDER BY p.iReportId
"@ "first report parameters"

Dump @"
SELECT OBJECT_NAME(m.object_id) N
FROM sys.sql_modules m
WHERE m.definition LIKE N'%@iStartDate%'
   OR m.definition LIKE N'%BETWEEN @iStart%'
   OR m.definition LIKE N'%ht.iDate BETWEEN%'
ORDER BY 1
"@ "modules with @iStartDate / ht.iDate BETWEEN"
