$cs = "Server=localhost;Database=Focus8080;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='vaCore_Account' ORDER BY ORDINAL_POSITION"
$r = $cmd.ExecuteReader()
$cols = @()
while ($r.Read()) { $cols += $r.GetString(0) }
$r.Close()
Write-Host ($cols -join ", ")
$conn.Close()
