$cs = "Server=localhost;Database=Focus8080;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()

function Run($title, $sql) {
    $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 180; $cmd.CommandText = $sql
    $a = New-Object System.Data.SqlClient.SqlDataAdapter $cmd; $dt = New-Object System.Data.DataTable
    [void]$a.Fill($dt)
    Write-Host "`n== $title =="
    $dt | Format-Table -AutoSize | Out-String -Width 300 | Write-Host
}

Run "Tree definitions" @"
SELECT * FROM dbo.mCore_AccountTree
"@

Run "Accounts whose ReportStatus differs across trees" @"
SELECT iMasterId, COUNT(DISTINCT ReportStatus) AS Statuses, COUNT(*) AS TreeRows
FROM dbo.vaCore_Account
GROUP BY iMasterId
HAVING COUNT(DISTINCT ReportStatus) > 1
ORDER BY iMasterId
"@

Run "Status per tree for the 2 excluded accounts + a few included ones" @"
SELECT iMasterId, sName, iTreeId, ReportStatus
FROM dbo.vaCore_Account
WHERE iMasterId IN (12606, 15268, 374, 17060, 17179, 14412, 16677)
ORDER BY iMasterId, iTreeId
"@

$conn.Close()
