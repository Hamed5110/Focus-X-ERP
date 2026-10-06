$cs = "Server=localhost;Database=Focus80G0;User Id=sa;Password=$env:FOCUS_SQL_PASSWORD;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
function Dump($sql, $title) {
    Write-Output ""
    Write-Output "========== $title =========="
    $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 30; $cmd.CommandText = $sql
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
                    if ($v.Length -gt 200) { $v = $v.Substring(0,200) + "..." }
                    $parts += $v
                }
            }
            Write-Output ($parts -join " | ")
        }
        $r.Close()
        if ($c -eq 0) { Write-Output "(no rows)" }
    } catch { Write-Output $_.Exception.Message }
}

Dump @"
SELECT r.iReportId, r.sReportName, r.iReportType, r.iSourceType,
       ISNULL(ts.Cnt,0) TranCnt, LEN(q.sSqlQuery) SqlLen
FROM dbo.cCore_Reports_0 r
LEFT JOIN dbo.cCore_RDQuery_0 q ON q.iReportId = r.iReportId
LEFT JOIN (
    SELECT iReportId, COUNT(*) Cnt FROM dbo.cCore_ReportTransactionSet_0 GROUP BY iReportId
) ts ON ts.iReportId = r.iReportId
WHERE r.iReportId = 70268
"@ "70268 meta"

Dump @"
SELECT TOP 5 iId FROM dbo.cCore_ReportTransactionSet_0 WHERE iReportId = 70268
"@ "70268 transet"

Dump @"
SELECT sSqlQuery FROM dbo.cCore_RDQuery_0 WHERE iReportId = 70268
"@ "70268 sql"

Dump @"
SELECT COUNT(*) Cnt FROM dbo.tCore_Header_0 WHERE iDate = 0
"@ "headers iDate=0"

Dump @"
SELECT TOP 3 iHeaderId, iDate, iVoucherType FROM dbo.tCore_Header_0 WHERE iDate = 0
"@ "sample iDate=0"

Dump @"
SELECT TOP 1 iHeaderId, iDate FROM dbo.tCore_Header_0 ORDER BY iHeaderId
"@ "min header"

$conn.Close()
