$cs = "Server=localhost;Database=Focus8080;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()

function Q($sql) {
    $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 280; $cmd.CommandText = $sql
    $a = New-Object System.Data.SqlClient.SqlDataAdapter $cmd
    $dt = New-Object System.Data.DataTable
    [void]$a.Fill($dt)
    , $dt
}

Write-Host "=== Sayed Noaman accounts ==="
(Q "SELECT iMasterId, sName, sCode, ReportStatus FROM dbo.vaCore_Account WHERE sName LIKE '%Sayed%Noaman%' OR sName LIKE '%Noaman%'").Rows |
    Format-Table -AutoSize | Out-String -Width 150 | Write-Host

Write-Host "=== His credit lines (cube types, dept 2040) with packed dates ==="
(Q "
SELECT h.iVoucherType, h.sVoucherNo, h.iDate,
       (h.iDate / 65536) AS Y, ((h.iDate % 65536) / 256) AS M, (h.iDate % 256) AS D,
       d.iCode, d.iBookNo, d.mAmount1, d.mAmount2, d.bUpdateFA, d.iAuthStatus, d.iType
FROM dbo.tCore_Header_0 h
INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId = h.iHeaderId
INNER JOIN dbo.mCore_Account a ON a.iMasterId IN (d.iCode, d.iBookNo)
WHERE a.sName LIKE '%Noaman%'
  AND h.iVoucherType IN (256, 4096, 4608, 4609, 4610, 8707)
  AND d.iFaTag = 2040
ORDER BY h.iDate
").Rows | Format-Table -AutoSize | Out-String -Width 200 | Write-Host

Write-Host "=== Any status-3-name credit lines dated 29/08 or 30/08 (the 947 window) ==="
(Q "
SELECT a.sName, h.iVoucherType, h.sVoucherNo, h.iDate,
       (h.iDate / 65536) AS Y, ((h.iDate % 65536) / 256) AS M, (h.iDate % 256) AS D,
       d.mAmount1
FROM dbo.tCore_Header_0 h
INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId = h.iHeaderId
INNER JOIN dbo.mCore_Account a ON a.iMasterId = d.iCode
INNER JOIN dbo.vaCore_Account v ON v.iMasterId = d.iCode AND v.iTreeId = 0
WHERE h.iVoucherType IN (256, 4096, 4608, 4609, 4610, 8707)
  AND d.bUpdateFA = 1 AND d.iFaTag = 2040 AND d.iCode > 0 AND d.mAmount1 > 0
  AND ISNULL(d.iType, 0) = 0 AND ISNULL(h.iAuth, 1) = 1 AND ISNULL(d.iAuthStatus, 0) < 2
  AND ISNULL(h.bCancelled, 0) = 0 AND ISNULL(h.bVersion, 0) = 0 AND ISNULL(h.bSuspended, 0) = 0
  AND ISNULL(d.bVoid, 0) = 0
  AND v.ReportStatus = 3
  AND h.iDate >= 132778000
ORDER BY h.iDate, a.sName
").Rows | Format-Table -AutoSize | Out-String -Width 180 | Write-Host

$conn.Close()
