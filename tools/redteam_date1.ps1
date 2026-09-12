$cs = "Server=localhost;Database=Focus80E0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 180
function Dump($sql, $title) {
    Write-Output ""
    Write-Output "========== $title =========="
    $cmd.CommandText = $sql
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
            if ($c -ge 50) { break }
        }
        $r.Close()
    } catch {
        Write-Output ("ERROR: " + $_.Exception.Message)
        try { $conn.Close(); $conn.Open(); $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 180 } catch {}
    }
}

Dump @"
SELECT r.iReportId, r.sReportName, r.iReportType, r.iSourceType, r.iFilterGroupId, r.iModule, r.bAdvance,
       CASE WHEN q.iReportId IS NULL THEN 0 ELSE 1 END AS HasSql,
       LEN(q.sSqlQuery) AS SqlLen
FROM dbo.cCore_Reports_0 r
LEFT JOIN dbo.cCore_RDQuery_0 q ON q.iReportId = r.iReportId
WHERE r.iReportType = 1
ORDER BY r.iReportId
"@ "all query reports"

Dump @"
SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME='cCore_Reports_0' ORDER BY ORDINAL_POSITION
"@ "report cols"

Dump @"
SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME LIKE 'cCore_Report%' ORDER BY TABLE_NAME, ORDINAL_POSITION
"@ "all cCore_Report* cols"
