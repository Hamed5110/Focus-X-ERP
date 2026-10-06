$cs = "Server=localhost;Database=Focus8080;User Id=sa;Password=$env:FOCUS_SQL_PASSWORD;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
function Dump($sql, $title) {
    Write-Output ""
    Write-Output "========== $title =========="
    $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 60; $cmd.CommandText = $sql
    $r = $cmd.ExecuteReader()
    $n = $r.FieldCount
    $hdr = @(); for ($i=0; $i -lt $n; $i++) { $hdr += $r.GetName($i) }
    Write-Output ($hdr -join " | ")
    $c = 0
    while ($r.Read()) {
        $c++
        $parts = @(); for ($i=0; $i -lt $n; $i++) {
            if ($r.IsDBNull($i)) { $parts += "" } else {
                $v = [string]$r.GetValue($i)
                $v = $v -replace "`r|`n"," "
                if ($v.Length -gt 160) { $v = $v.Substring(0,160) + "..." }
                $parts += $v
            }
        }
        Write-Output ($parts -join " | ")
        if ($c -ge 40) { break }
    }
    $r.Close()
}

Dump @"
SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.TABLES t
JOIN INFORMATION_SCHEMA.COLUMNS c ON c.TABLE_NAME=t.TABLE_NAME
WHERE t.TABLE_NAME LIKE N'%RDQuery%' OR t.TABLE_NAME LIKE N'%ReportQuery%'
"@ "query tables"

Dump @"
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME LIKE N'%RDQuery%' OR TABLE_NAME LIKE N'%Query%' AND TABLE_NAME LIKE N'%Report%'
"@ "query table names"

Dump @"
SELECT TOP 25 r.iReportId, r.sReportName, r.iReportType, r.iSourceType, r.iModifiedDate
FROM dbo.cCore_Reports_0 r
WHERE r.iReportType = 1
ORDER BY r.iReportId DESC
"@ "recent query reports"

Dump @"
SELECT TOP 20 p.iReportId, r.sReportName, r.iReportType, p.sFieldName, p.sFieldVariable, p.iControlType, p.iFieldType, p.sDefault
FROM dbo.cCore_ReportParameter_0 p
JOIN dbo.cCore_Reports_0 r ON r.iReportId = p.iReportId
WHERE r.iReportType = 1
ORDER BY p.iReportId DESC
"@ "query report params"
