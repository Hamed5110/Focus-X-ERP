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

Run "SO lines with iType<>0 for the 4 flip accounts" @"
SELECT a.sName, h.iHeaderId, h.sVoucherNo, h.fNet, d.iType, d.bVoid, COUNT(*) AS Lines, SUM(d.mAmount1) AS SumM1, SUM(d.mAmount2) AS SumM2
FROM dbo.tCore_Header_0 h
INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId = h.iHeaderId AND h.iVoucherType = 5634 AND d.iFaTag = 2040
INNER JOIN dbo.mCore_Account a ON a.iMasterId = d.iBookNo
WHERE d.iBookNo IN (13890, 10707, 7057, 7064) AND ISNULL(d.iType, 0) <> 0
GROUP BY a.sName, h.iHeaderId, h.sVoucherNo, h.fNet, d.iType, d.bVoid
ORDER BY a.sName, h.iHeaderId
"@

Run "ALL SO lines (any iType, any bVoid) totals for the 4 accounts vs filtered" @"
SELECT a.sName,
       ROUND(SUM(CASE WHEN ISNULL(d.iType,0)=0 AND ISNULL(d.bVoid,0)=0 THEN hdr.fNet ELSE 0 END), 2) AS SumFiltered,
       ROUND(SUM(hdr.fNet), 2) AS SumAll
FROM (
    SELECT DISTINCT h.iHeaderId, h.fNet, d.iBookNo, d.iType, d.bVoid
    FROM dbo.tCore_Header_0 h
    INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId = h.iHeaderId AND h.iVoucherType = 5634 AND d.iFaTag = 2040
    WHERE d.iBookNo IN (13890, 10707, 7057, 7064)
) hdr
INNER JOIN dbo.mCore_Account a ON a.iMasterId = hdr.iBookNo
GROUP BY a.sName
ORDER BY a.sName
"@

Run "Any SO headers for these accounts where ALL lines are iType<>0 or void" @"
SELECT a.sName, h.iHeaderId, h.sVoucherNo, h.fNet,
       SUM(CASE WHEN ISNULL(d.iType,0)=0 AND ISNULL(d.bVoid,0)=0 THEN 1 ELSE 0 END) AS GoodLines,
       COUNT(*) AS TotalLines
FROM dbo.tCore_Header_0 h
INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId = h.iHeaderId AND h.iVoucherType = 5634 AND d.iFaTag = 2040
INNER JOIN dbo.mCore_Account a ON a.iMasterId = d.iBookNo
WHERE d.iBookNo IN (13890, 10707, 7057, 7064)
GROUP BY a.sName, h.iHeaderId, h.sVoucherNo, h.fNet
HAVING SUM(CASE WHEN ISNULL(d.iType,0)=0 AND ISNULL(d.bVoid,0)=0 THEN 1 ELSE 0 END) = 0
ORDER BY a.sName
"@

$conn.Close()
