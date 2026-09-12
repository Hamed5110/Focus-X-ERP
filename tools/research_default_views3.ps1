$cs = "Server=localhost;Database=Focus80E0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 60
$cmd.CommandText = "SELECT OBJECT_DEFINITION(OBJECT_ID('dbo.vtCode_DataFA_0'))"
$d = [string]$cmd.ExecuteScalar()
[System.IO.File]::WriteAllText("C:\Users\Hamed Ali Khan\Focus-X-ERP\tools\vtCode_DataFA_0.sql", $d)
Write-Output ("saved len=" + $d.Length)

$cmd.CommandText = @"
SELECT COLUMN_NAME, DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME='vtCode_DataFA_0'
ORDER BY ORDINAL_POSITION
"@
$r = $cmd.ExecuteReader()
while ($r.Read()) { Write-Output ("{0} | {1}" -f $r.GetValue(0), $r.GetValue(1)) }
$r.Close()

Write-Output ""
Write-Output "========== 70028 TS + 4610 examples =========="
$cmd.CommandText = @"
SELECT r.iReportId, r.sReportName, t.iTranSetId, t.iVoucherType, t.iDocumentOption
FROM dbo.cCore_Reports_0 r
INNER JOIN dbo.cCore_ReportTransactionSet_0 t ON t.iReportId = r.iReportId
WHERE t.iVoucherType IN (4608,4609,4610)
ORDER BY r.iReportId, t.iVoucherType
"@
$r = $cmd.ExecuteReader()
while ($r.Read()) { Write-Output ("{0} | {1} | set={2} | vt={3} | doc={4}" -f $r.GetValue(0),$r.GetValue(1),$r.GetValue(2),$r.GetValue(3),$r.GetValue(4)) }
$r.Close()
$conn.Close()
