$cs = "Server=localhost;Database=Focus8080;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()

function Run($title, $sql) {
    $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 300; $cmd.CommandText = $sql
    $a = New-Object System.Data.SqlClient.SqlDataAdapter $cmd; $dt = New-Object System.Data.DataTable
    [void]$a.Fill($dt)
    Write-Host "`n== $title =="
    $dt | Format-Table -AutoSize | Out-String -Width 400 | Write-Host
}

Run "SO headers for the 4 sign-flip accounts" @"
SELECT a.sName, h.iHeaderId, h.sVoucherNo, h.iDate, h.fNet, h.fGross, h.fDiscount, h.iAuth, h.bCancelled, h.bSuspended, h.bVersion
FROM dbo.tCore_Header_0 h
INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId = h.iHeaderId AND h.iVoucherType = 5634 AND d.iFaTag = 2040 AND ISNULL(d.iType,0)=0 AND ISNULL(d.bVoid,0)=0
INNER JOIN dbo.mCore_Account a ON a.iMasterId = d.iBookNo
WHERE d.iBookNo IN (13890, 10707, 7057, 7064)
GROUP BY a.sName, h.iHeaderId, h.sVoucherNo, h.iDate, h.fNet, h.fGross, h.fDiscount, h.iAuth, h.bCancelled, h.bSuspended, h.bVersion
ORDER BY a.sName, h.iDate
"@

Run "Line-level amount fields for those SO headers" @"
SELECT h.iHeaderId, h.sVoucherNo, d.iBookNo, COUNT(*) AS Lines,
       SUM(d.fNet) AS SumLineFNet, SUM(d.fGross) AS SumLineFGross,
       SUM(d.mAmount1) AS SumM1, SUM(d.mAmount2) AS SumM2,
       SUM(d.fQuantity) AS SumQty
FROM dbo.tCore_Header_0 h
INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId = h.iHeaderId
WHERE h.iVoucherType = 5634 AND d.iFaTag = 2040 AND d.iBookNo IN (13890, 10707, 7057, 7064) AND ISNULL(d.iType,0)=0 AND ISNULL(d.bVoid,0)=0
GROUP BY h.iHeaderId, h.sVoucherNo, d.iBookNo
ORDER BY d.iBookNo, h.iHeaderId
"@

$conn.Close()
