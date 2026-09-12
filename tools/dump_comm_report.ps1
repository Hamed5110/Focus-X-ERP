$cs = "Server=localhost;Database=Focus80E0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$cmd = $conn.CreateCommand()
$cmd.CommandTimeout = 60
$cmd.CommandText = @"
SELECT r.iReportId, r.sName, r.iType
FROM dbo.cCore_Reports_0 r
WHERE r.sName LIKE N'%Commission%' OR r.sName LIKE N'%commission%'
ORDER BY r.iReportId
"@
try {
    $r = $cmd.ExecuteReader()
    while ($r.Read()) { Write-Output ("{0} | {1} | type={2}" -f $r.GetValue(0), $r.GetValue(1), $r.GetValue(2)) }
    $r.Close()
} catch {
    Write-Output ("reports err: " + $_.Exception.Message)
}

Write-Output "`n=== tCore_Data_0 date-like columns ==="
$cmd.CommandText = @"
SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'tCore_Data_0' AND COLUMN_NAME LIKE '%Date%'
ORDER BY COLUMN_NAME
"@
try {
    $r = $cmd.ExecuteReader()
    while ($r.Read()) { Write-Output $r.GetValue(0) }
    $r.Close()
} catch {
    Write-Output $_.Exception.Message
}
$conn.Close()
