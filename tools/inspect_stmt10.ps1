$cs = "Server=localhost;Database=Focus80G0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 30
$cmd.CommandText = @"
SELECT name FROM sys.views
WHERE name LIKE '%5634%' OR name LIKE '%HeaderData%' OR name LIKE '%Opportunity%'
ORDER BY name
"@
$r = $cmd.ExecuteReader()
while ($r.Read()) { Write-Output $r[0] }
$r.Close()

$cmd.CommandText = @"
SELECT TOP 5 c.name
FROM sys.columns c
WHERE c.name LIKE '%OpportunityType%'
"@
$r = $cmd.ExecuteReader()
while ($r.Read()) { Write-Output ("col=" + $r[0]) }
$r.Close()

# extra field list often in cCore_DropDownInfo or mCore with iMasterType
$cmd.CommandText = @"
SELECT name FROM sys.tables WHERE name LIKE 'cCore_%Field%' OR name LIKE 'cCore_%Extra%' OR name LIKE 'cCore_%List%'
ORDER BY name
"@
$r = $cmd.ExecuteReader()
while ($r.Read()) { Write-Output $r[0] }
$r.Close()
$conn.Close()
