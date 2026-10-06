$cs = "Server=localhost;Database=Focus80G0;User Id=sa;Password=$env:FOCUS_SQL_PASSWORD;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
function Dump($sql, $title) {
    Write-Output ""
    Write-Output "========== $title =========="
    $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 60; $cmd.CommandText = $sql
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
                    if ($v.Length -gt 100) { $v = $v.Substring(0,100) + "..." }
                    $parts += $v
                }
            }
            Write-Output ($parts -join " | ")
            if ($c -ge 25) { break }
        }
        $r.Close()
        if ($c -eq 0) { Write-Output "(no rows)" }
    } catch { Write-Output $_.Exception.Message }
}

Dump @"
SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = N'cCore_Fields' ORDER BY ORDINAL_POSITION
"@ "cCore_Fields cols"

Dump @"
SELECT LEFT(sSqlQuery, 800)
FROM dbo.cCore_RDQuery_0
WHERE iReportId IN (70203, 70206)
"@ "comp trail sql uses @Status"

Dump @"
SELECT p.iReportId, r.sReportName, p.sFieldName, p.sFieldVariable, p.iControlType, p.iFieldType, p.iFieldId,
       LEFT(ISNULL(p.sValue,N''),80) Val
FROM Focus80E0.dbo.cCore_ReportParameter_0 p
JOIN Focus80E0.dbo.cCore_Reports_0 r ON r.iReportId = p.iReportId
WHERE p.iControlType IN (0,4,5,6,7,8,9,10,11,12)
   OR p.sFieldName LIKE N'%Customer%'
   OR p.sFieldVariable LIKE N'%Customer%'
ORDER BY p.iControlType, p.iReportId
"@ "E0 extra param types"

Dump @"
SELECT iMasterId, sCode, sName FROM dbo.mCore_Account
WHERE iMasterId = 280 OR sCode = N'180'
"@ "TR group"
