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

Run "Per-SO: header fNet vs line sums for the 4 flip accounts" @"
SELECT a.sName, h.iHeaderId, h.sVoucherNo, h.fNet, h.fOrigNet,
       SUM(d.mAmount1) AS SumM1, SUM(d.mAmount2) AS SumM2, SUM(d.mAmount3) AS SumM3, SUM(d.mOriginalAmount) AS SumOrig,
       COUNT(*) AS Lines
FROM dbo.tCore_Header_0 h
INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId = h.iHeaderId AND h.iVoucherType = 5634 AND d.iFaTag = 2040 AND ISNULL(d.iType,0)=0 AND ISNULL(d.bVoid,0)=0
INNER JOIN dbo.mCore_Account a ON a.iMasterId = d.iBookNo
WHERE d.iBookNo IN (13890, 10707, 7057, 7064)
GROUP BY a.sName, h.iHeaderId, h.sVoucherNo, h.fNet, h.fOrigNet
ORDER BY a.sName, h.iHeaderId
"@

Run "Account-level totals: -SUM(fNet) vs line sums" @"
SELECT a.sName,
       ROUND(SUM(hdr.fNet) * -1, 0) AS NegSumFNet,
       ROUND(SUM(hdr.fNet), 0) AS SumFNet,
       ROUND(SUM(hdr.SumM1), 0) AS SumM1,
       ROUND(SUM(hdr.SumM2), 0) AS SumM2
FROM (
    SELECT h.iHeaderId, h.fNet, d.iBookNo,
           SUM(d.mAmount1) AS SumM1, SUM(d.mAmount2) AS SumM2
    FROM dbo.tCore_Header_0 h
    INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId = h.iHeaderId AND h.iVoucherType = 5634 AND d.iFaTag = 2040 AND ISNULL(d.iType,0)=0 AND ISNULL(d.bVoid,0)=0
    WHERE d.iBookNo IN (13890, 10707, 7057, 7064)
    GROUP BY h.iHeaderId, h.fNet, d.iBookNo
) hdr
INNER JOIN dbo.mCore_Account a ON a.iMasterId = hdr.iBookNo
GROUP BY a.sName
ORDER BY a.sName
"@

$conn.Close()
