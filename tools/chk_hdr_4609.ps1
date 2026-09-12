$cs = "Server=localhost;Database=Focus80E0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME IN ('tCore_HeaderData4609_0','tCore_HeaderData4608_0') ORDER BY TABLE_NAME, ORDINAL_POSITION"
$r = $cmd.ExecuteReader()
while ($r.Read()) { Write-Output $r.GetString(0) }
$r.Close()
$conn.Close()
