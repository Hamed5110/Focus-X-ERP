$cs = "Server=localhost;Database=Focus80G0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT iMasterId, sCode, sName FROM dbo.mCore_marketingchannel ORDER BY iMasterId"
$r = $cmd.ExecuteReader()
Write-Output "==== marketingchannel ===="
while ($r.Read()) { Write-Output ("{0} {1} {2}" -f $r[0], $r[1], $r[2]) }
$r.Close()
$cmd.CommandText = "SELECT name FROM sys.tables WHERE name LIKE 'mCore_%' AND name LIKE '%lead%' OR name LIKE 'mCore_%deal%' OR name LIKE 'mCore_%crm%'"
$r = $cmd.ExecuteReader()
Write-Output "==== crm tables ===="
while ($r.Read()) { Write-Output $r[0] }
$r.Close()
$conn.Close()
