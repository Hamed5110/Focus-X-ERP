$cs = "Server=localhost;Database=Focus80G0;User Id=sa;Password=$env:FOCUS_SQL_PASSWORD;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$cmd = $conn.CreateCommand()
$cmd.CommandTimeout = 60
$cmd.CommandText = @"
SELECT o.type_desc, o.name
FROM sys.objects o
WHERE o.name LIKE N'%Date%' AND o.type IN ('V','TF','IF','FN','U')
ORDER BY o.type, o.name
"@
$rd = $cmd.ExecuteReader()
$c=0
while($rd.Read() -and $c -lt 40){
    Write-Output ("{0} | {1}" -f $rd[0], $rd[1])
    $c++
}
$rd.Close()

$cmd.CommandText = @"
SELECT c.name, o.name
FROM sys.columns c
JOIN sys.objects o ON o.object_id = c.object_id
WHERE c.name = N'iDate' AND o.type IN ('V','U')
  AND o.name LIKE N'v%'
ORDER BY o.name
"@
$rd = $cmd.ExecuteReader()
$c=0
Write-Output "===== views with iDate ====="
while($rd.Read() -and $c -lt 30){
    Write-Output ("{0} | {1}" -f $rd[1], $rd[0])
    $c++
}
$rd.Close()
$conn.Close()
