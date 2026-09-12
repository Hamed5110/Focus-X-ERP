$cs = "Server=localhost;Database=Focus80E0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 90
function Dump($sql, $title) {
    Write-Output ""
    Write-Output "========== $title =========="
    $cmd.CommandText = $sql
    try {
        $r = $cmd.ExecuteReader()
        $n = $r.FieldCount
        $hdr = @(); for ($i=0; $i -lt $n; $i++) { $hdr += $r.GetName($i) }
        Write-Output ($hdr -join " | ")
        while ($r.Read()) {
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
        }
        $r.Close()
    } catch {
        Write-Output ("ERROR: " + $_.Exception.Message)
    }
}

Dump @"
SELECT r.iReportId, r.sReportName, r.iReportType, r.iSourceType,
       t.iTranSetId, t.iVoucherType, t.iDocumentOption, t.iBRS
FROM dbo.cCore_Reports_0 r
INNER JOIN dbo.cCore_ReportTransactionSet_0 t ON t.iReportId = r.iReportId
WHERE r.iSourceType = 1 AND r.iReportType = 1
ORDER BY r.iReportId
"@ "hybrid SQL+TS query reports"

Dump @"
SELECT DISTINCT iTranSetId, iVoucherType, COUNT(*) Cnt
FROM dbo.cCore_ReportTransactionSet_0
WHERE iVoucherType IN (0, 4608, 4609, 4610, 5634)
GROUP BY iTranSetId, iVoucherType
ORDER BY Cnt DESC
"@ "common TS ids for receipts"

Dump @"
SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME='cCore_RDQuery_0'
"@ "RDQuery cols"

Dump @"
SELECT iReportId, iSecurityType, LEFT(sConnection,80) Conn
FROM dbo.cCore_RDQuery_0
WHERE iReportId IN (70266,70256,70259)
"@ "RDQuery extras"
