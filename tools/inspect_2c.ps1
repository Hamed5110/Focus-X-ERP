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

Run "What are accounts 8048 and 8054?" @"
SELECT iMasterId, sName, sCode, bGroup, ReportStatus FROM dbo.vaCore_Account WHERE iMasterId IN (8048, 8054)
"@

Run "iBookNo distribution on 4609/4610 receipts for cube-INCLUDED sample accounts" @"
SELECT TOP 20 d.iCode, a.sName AS Customer, d.iBookNo, b.sName AS BookAccount, COUNT(*) AS Lines
FROM dbo.tCore_Header_0 h
INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId = h.iHeaderId
INNER JOIN dbo.mCore_Account a ON a.iMasterId = d.iCode
LEFT JOIN dbo.mCore_Account b ON b.iMasterId = d.iBookNo
WHERE h.iVoucherType IN (4609, 4610) AND d.iFaTag = 2040 AND d.iCode > 0 AND d.bUpdateFA = 1
GROUP BY d.iCode, a.sName, d.iBookNo, b.sName
ORDER BY Lines DESC
"@

Run "SO headers of the 2 excluded accounts: check bSuspended/bVersion/iAuth across ALL lines and header flags" @"
SELECT h.iHeaderId, h.sVoucherNo, h.iDate, dbo.fCore_IntToDateTime(h.iDate) AS RealDate, h.fNet, h.iAuth, h.bCancelled, h.bSuspended, h.bVersion
FROM dbo.tCore_Header_0 h
WHERE h.iVoucherType = 5634 AND h.iHeaderId IN (74576, 74579, 83955, 106518, 111451, 139879, 156191, 109184, 134081, 134082, 137716)
ORDER BY h.iDate
"@

$conn.Close()
