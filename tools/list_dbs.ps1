$cs = "Server=localhost;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT name FROM sys.databases ORDER BY name"
$r = $cmd.ExecuteReader()
while ($r.Read()) { Write-Host $r.GetString(0) }
$r.Close()
$conn.Close()
