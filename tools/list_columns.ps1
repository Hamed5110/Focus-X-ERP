$cs = "Server=localhost;Database=Focus8080;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()

function Run($title, $sql) {
    $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 300; $cmd.CommandText = $sql
    $a = New-Object System.Data.SqlClient.SqlDataAdapter $cmd; $dt = New-Object System.Data.DataTable
    [void]$a.Fill($dt)
    Write-Host "`n== $title =="
    $dt | Format-Table -AutoSize | Out-String -Width 300 | Write-Host
}

Run "tCore_Data_0 amount-ish columns" @"
SELECT COLUMN_NAME, DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'tCore_Data_0'
  AND (COLUMN_NAME LIKE '%mount%' OR COLUMN_NAME LIKE '%Net%' OR COLUMN_NAME LIKE '%ross%' OR COLUMN_NAME LIKE '%Qty%' OR COLUMN_NAME LIKE '%uant%' OR COLUMN_NAME LIKE '%Price%' OR COLUMN_NAME LIKE '%Value%')
ORDER BY ORDINAL_POSITION
"@

Run "tCore_Header_0 amount-ish columns" @"
SELECT COLUMN_NAME, DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'tCore_Header_0'
  AND (COLUMN_NAME LIKE '%mount%' OR COLUMN_NAME LIKE '%Net%' OR COLUMN_NAME LIKE '%ross%' OR COLUMN_NAME LIKE '%isc%')
ORDER BY ORDINAL_POSITION
"@

$conn.Close()
