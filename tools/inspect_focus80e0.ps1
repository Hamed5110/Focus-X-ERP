$cs = "Server=localhost;Database=Focus80E0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
function Dump($sql) {
    $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 180; $cmd.CommandText = $sql
    $r = $cmd.ExecuteReader()
    $n = $r.FieldCount
    while ($r.Read()) {
        $parts = @()
        for ($i = 0; $i -lt $n; $i++) {
            if ($r.IsDBNull($i)) { $parts += "" } else { $parts += [string]$r.GetValue($i) }
        }
        Write-Output ($parts -join " | ")
    }
    $r.Close()
}

Write-Output "=== departments ==="
Dump "SELECT iMasterId, sCode, sName FROM dbo.mCore_Department ORDER BY iMasterId"

Write-Output "`n=== salesman-like tables ==="
Dump "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME LIKE '%salesman%' OR TABLE_NAME LIKE '%Salesman%' OR TABLE_NAME LIKE '%employee%' OR TABLE_NAME LIKE '%Employee%' OR TABLE_NAME LIKE '%designer%' OR TABLE_NAME LIKE '%Designer%' OR TABLE_NAME LIKE '%attendance%' ORDER BY TABLE_NAME"

Write-Output "`n=== voucher types ==="
Dump @"
SELECT TOP 30 h.iVoucherType, COUNT(*) AS Cnt, MIN(h.sVoucherNo) AS SampleNo
FROM dbo.tCore_Header_0 h
WHERE ISNULL(h.bCancelled,0)=0 AND ISNULL(h.bVersion,0)=0
GROUP BY h.iVoucherType
ORDER BY COUNT(*) DESC
"@

Write-Output "`n=== vaCore_Account sample ==="
Dump @"
SELECT TOP 8 iMasterId, sCode, sName, SalesmannameName, DesignernameName, ReportStatus
FROM dbo.vaCore_Account
WHERE iTreeId = 0 AND ISNULL(bGroup,0) = 0 AND SalesmannameName IS NOT NULL AND SalesmannameName <> ''
"@

$conn.Close()
