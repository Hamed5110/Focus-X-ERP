$cs = "Server=localhost;Database=Focus80G0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$cmd = $conn.CreateCommand()
$cmd.CommandText = @"
SELECT 132776219 AS Packed,
 CONVERT(varchar(20), TRY_CONVERT(date, CAST(132776219 AS varchar(8)), 112), 23) AS Direct112,
 CONVERT(varchar(20), CONVERT(date, CAST((132776219 / 65536)*10000 + ((132776219 / 256) % 256)*100 + (132776219 % 256) AS char(8)), 112), 23) AS Unpack112,
 CONVERT(varchar(20), dbo.IntToDate(132776219), 23) AS IntToDate,
 CONVERT(varchar(20), CONVERT(date, CAST(20260930 AS varchar(8)), 112), 23) AS Example20260930,
 SQL_VARIANT_PROPERTY(CONVERT(date, CAST((132776219 / 65536)*10000 + ((132776219 / 256) % 256)*100 + (132776219 % 256) AS char(8)), 112), 'BaseType') AS UnpackType
"@
$r = $cmd.ExecuteReader()
$r.Read()
for ($i=0; $i -lt $r.FieldCount; $i++) {
    $v = if ($r.IsDBNull($i)) { "NULL" } else { [string]$r.GetValue($i) }
    Write-Output ("{0}={1}" -f $r.GetName($i), $v)
}
$r.Close()
$conn.Close()
