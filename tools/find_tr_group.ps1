$cs = "Server=localhost;Database=Focus8080;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()

function Run($title, $sql) {
    $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 120; $cmd.CommandText = $sql
    $a = New-Object System.Data.SqlClient.SqlDataAdapter $cmd; $dt = New-Object System.Data.DataTable
    [void]$a.Fill($dt)
    Write-Host "`n== $title =="
    $dt | Format-Table -AutoSize | Out-String -Width 250 | Write-Host
    return $dt
}

Run "Trade Receivables group nodes in vaCore_Account" @"
SELECT iMasterId, sName, sCode, iParentId, iTreeId, bGroup
FROM dbo.vaCore_Account
WHERE sName LIKE '%Trade Receivable%' OR sName LIKE '%Receivable%'
ORDER BY iTreeId, iMasterId
"@

Run "Account trees list" @"
SELECT DISTINCT iTreeId FROM dbo.vaCore_Account ORDER BY iTreeId
"@

Run "mCore_AccountTreeDetails columns" @"
SELECT COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'mCore_AccountTreeDetails' ORDER BY ORDINAL_POSITION
"@

$conn.Close()
