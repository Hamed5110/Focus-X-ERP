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
                    if ($v.Length -gt 220) { $v = $v.Substring(0,220) + "..." }
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
SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = N'cCore_Reports_0' ORDER BY ORDINAL_POSITION
"@ "report cols"

Dump @"
SELECT * FROM dbo.cCore_Reports_0 WHERE iReportId IN (70266, 70268, 70198, 70223)
"@ "report rows"

Dump @"
SELECT iReportId, iParameterId, sParameterName, sFieldVariable, iControlType, iFieldType, iMasterTypeId
FROM dbo.cCore_ReportParameter_0
WHERE iReportId IN (70266, 70268, 70198, 70223)
ORDER BY iReportId, iParameterId
"@ "params"

Dump @"
SELECT iReportId, COUNT(*) Cnt FROM dbo.cCore_ReportTransactionSet_0
WHERE iReportId IN (70266, 70268, 70198, 70223)
GROUP BY iReportId
"@ "transet"

Dump @"
SELECT iReportId, LEFT(sSqlQuery, 400) Head
FROM dbo.cCore_RDQuery_0
WHERE iReportId IN (70266, 70198, 70223)
"@ "working sql heads"

Dump @"
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME LIKE N'%Report%'
ORDER BY TABLE_NAME
"@ "report tables"

$conn.Close()
