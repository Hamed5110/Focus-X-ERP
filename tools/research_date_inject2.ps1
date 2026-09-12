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
            if ($c -ge 100) { Write-Output "... truncated ..."; break }
        }
        $r.Close()
        if ($c -eq 0) { Write-Output "(no rows)" }
    } catch {
        Write-Output ("ERROR: " + $_.Exception.Message)
    }
}

Dump @"
SELECT iReportId, sReportName, iReportType, iSourceType, iFilterGroupId, iModule, bAdvance
FROM dbo.cCore_Reports_0
WHERE sReportName LIKE N'%Commission%' OR sReportName LIKE N'%commission%'
ORDER BY iReportId
"@ "commission reports"

Dump @"
SELECT COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME='cCore_RDQuery_0' ORDER BY ORDINAL_POSITION
"@ "RDQuery columns"

Dump @"
SELECT r.iReportId, r.sReportName, r.iReportType, r.iSourceType,
       LEN(q.sSqlQuery) AS SqlLen,
       CASE WHEN q.sSqlQuery LIKE N'%iDate%' THEN 1 ELSE 0 END AS HasIDate,
       CASE WHEN q.sSqlQuery LIKE N'%WHERE%' THEN 1 ELSE 0 END AS HasWhere
FROM dbo.cCore_Reports_0 r
INNER JOIN dbo.cCore_RDQuery_0 q ON q.iReportId = r.iReportId
WHERE r.iReportType = 1
ORDER BY r.iReportId
"@ "all query reports"

Dump @"
SELECT r.iReportId, r.sReportName, q.sSqlQuery
FROM dbo.cCore_Reports_0 r
INNER JOIN dbo.cCore_RDQuery_0 q ON q.iReportId = r.iReportId
WHERE r.sReportName LIKE N'%Commission%'
   OR r.sReportName LIKE N'%Monthly Commission%'
"@ "commission saved SQL"

$conn.Close()
