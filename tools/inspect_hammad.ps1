$cs = "Server=localhost;Database=Focus8080;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()

function Q($sql) {
    $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 280; $cmd.CommandText = $sql
    $a = New-Object System.Data.SqlClient.SqlDataAdapter $cmd
    $dt = New-Object System.Data.DataTable
    [void]$a.Fill($dt)
    return $dt
}

Write-Host "=== Ahmed Hammad accounts ==="
Q "SELECT iMasterId, sName, sCode FROM dbo.mCore_Account WHERE sName LIKE '%Ahmed Hammad%'" | Format-Table -AutoSize | Out-String -Width 150 | Write-Host

Write-Host "=== His credit lines (cube types, dept 2040) - ALL lines incl. negatives/flags ==="
Q "
SELECT h.iVoucherType, h.sVoucherNo, h.iDate, d.iCode, d.iBookNo, d.mAmount1, d.mAmount2,
       d.bUpdateFA, d.iAuthStatus, d.iType, d.bVoid, h.iAuth, h.bCancelled, h.bSuspended, h.bVersion
FROM dbo.tCore_Header_0 h
INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId = h.iHeaderId
INNER JOIN dbo.mCore_Account a ON a.iMasterId IN (d.iCode, d.iBookNo)
WHERE a.sName LIKE '%Ahmed Hammad%'
  AND h.iVoucherType IN (256, 4096, 4608, 4609, 4610, 8707)
  AND d.iFaTag = 2040
ORDER BY h.iVoucherType, h.sVoucherNo
" | Format-Table -AutoSize | Out-String -Width 220 | Write-Host

$conn.Close()
