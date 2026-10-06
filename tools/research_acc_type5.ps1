$cs = "Server=localhost;Database=Focus80G0;User Id=sa;Password=$env:FOCUS_SQL_PASSWORD;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 60
$cmd.CommandText = @"
SELECT iAccountType, COUNT(*) Cnt
FROM (
    SELECT iMasterId, MAX(iAccountType) iAccountType
    FROM dbo.vaCore_Account
    WHERE ReportStatus = 3 AND ISNULL(bGroup,0)=0
    GROUP BY iMasterId
) a
GROUP BY iAccountType
ORDER BY COUNT(*) DESC
"@
$r = $cmd.ExecuteReader()
Write-Output "iAccountType | Cnt"
while ($r.Read()) { Write-Output ("{0} | {1}" -f $r[0], $r[1]) }
$r.Close()
$conn.Close()
