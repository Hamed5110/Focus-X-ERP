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

Run "fCore_IntToDateTime definition" @"
SELECT OBJECT_DEFINITION(OBJECT_ID('dbo.fCore_IntToDateTime')) AS Def
"@

Run "Twin activity documents with dates" @"
SELECT h.iHeaderId, h.iVoucherType, h.sVoucherNo, h.iDate, d.iCode, d.iBookNo, d.mAmount1, d.mAmount2
FROM dbo.tCore_Header_0 h
INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId = h.iHeaderId
WHERE d.iFaTag = 2040 AND (d.iCode IN (14589, 9936) OR (d.iBookNo IN (14589, 9936) AND h.iVoucherType IN (5634, 5635, 6145)))
  AND ISNULL(h.bCancelled, 0) = 0 AND ISNULL(h.bVersion, 0) = 0 AND ISNULL(h.bSuspended, 0) = 0 AND ISNULL(d.bVoid, 0) = 0
ORDER BY h.iDate
"@

$conn.Close()
