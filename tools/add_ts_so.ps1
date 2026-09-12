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
                    if ($v.Length -gt 200) { $v = $v.Substring(0,200) + "..." }
                    $parts += $v
                }
            }
            Write-Output ($parts -join " | ")
        }
        $r.Close()
    } catch { Write-Output ("ERROR: " + $_.Exception.Message) }
}

Dump @"
SELECT iVoucherType, sName, iVoucherClass
FROM dbo.cCore_vouchers_0
WHERE iVoucherType IN (5634, 5632, 4608, 4609, 4610)
   OR sName LIKE N'%Sales Order%' OR sName LIKE N'%Contract%'
ORDER BY iVoucherType
"@ "voucher names"

Dump @"
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME LIKE '%Voucher%' AND TABLE_NAME LIKE 'cCore%'
ORDER BY TABLE_NAME
"@ "voucher config tables"

Dump @"
SELECT t.iTranId, t.iReportId, r.sReportName, t.iTranSetId, t.iVoucherType, t.iBRS, t.iDocumentOption
FROM dbo.cCore_ReportTransactionSet_0 t
INNER JOIN dbo.cCore_Reports_0 r ON r.iReportId = t.iReportId
WHERE t.iVoucherType = 5634
ORDER BY t.iReportId
"@ "reports with SO 5634 TS"

Dump @"
SELECT * FROM dbo.cCore_ReportTransactionSet_0 WHERE iReportId = 70266
"@ "70266 current TS"

Dump @"
SELECT MAX(iTranId) MaxId FROM dbo.cCore_ReportTransactionSet_0
"@ "max tran id"
