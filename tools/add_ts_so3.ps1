$cs = "Server=localhost;Database=Focus80E0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 60

$cmd.CommandText = "SELECT COUNT(*) FROM dbo.cCore_ReportTransactionSet_0 WHERE iReportId=70266 AND iVoucherType=5634"
$exists = [int]$cmd.ExecuteScalar()
Write-Output ("existing 5634 on 70266 = {0}" -f $exists)

if ($exists -eq 0) {
    $cmd.CommandText = @"
INSERT INTO dbo.cCore_ReportTransactionSet_0
    (iReportId, iTranSetId, iVoucherType, iBRS, iDocumentOption)
VALUES
    (70266, 14, 5634, 3, 30)
"@
    $n = $cmd.ExecuteNonQuery()
    Write-Output ("inserted rows={0}" -f $n)
}

$cmd.CommandText = @"
SELECT t.iTranId, t.iReportId, t.iTranSetId, t.iVoucherType, t.iBRS, t.iDocumentOption, v.sName
FROM dbo.cCore_ReportTransactionSet_0 t
LEFT JOIN dbo.cCore_Vouchers_0 v ON v.iVoucherType = t.iVoucherType
WHERE t.iReportId = 70266
"@
$r = $cmd.ExecuteReader()
$n = $r.FieldCount
$hdr = @(); for ($i=0;$i -lt $n;$i++) { $hdr += $r.GetName($i) }
Write-Output ($hdr -join " | ")
while ($r.Read()) {
    $parts = @(); for ($i=0;$i -lt $n;$i++) {
        if ($r.IsDBNull($i)) { $parts += "" } else { $parts += [string]$r.GetValue($i) }
    }
    Write-Output ($parts -join " | ")
}
$r.Close()
$conn.Close()
