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

Run "Local state of the 5 cube-only accounts (by code)" @"
SELECT iMasterId, sName, sCode, ReportStatus, bGroup, iParentId, iTreeId
FROM dbo.vaCore_Account
WHERE sCode IN ('AC-6277', 'AC-8140', 'AC-7287', 'AC-2071', 'AC-7074')
ORDER BY sCode, iTreeId
"@

Run "Their local Atlas activity (SO 5634)" @"
SELECT d.iBookNo, a.sCode, COUNT(DISTINCT h.iHeaderId) AS SOCount, SUM(h.fNet) AS SumFNet
FROM dbo.tCore_Header_0 h
INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId = h.iHeaderId AND h.iVoucherType = 5634 AND d.iFaTag = 2040 AND ISNULL(d.iType,0)=0 AND ISNULL(h.iAuth,1)=1 AND ISNULL(h.bCancelled,0)=0 AND ISNULL(h.bVersion,0)=0 AND ISNULL(h.bSuspended,0)=0 AND ISNULL(d.bVoid,0)=0
INNER JOIN dbo.mCore_Account a ON a.iMasterId = d.iBookNo
WHERE a.sCode IN ('AC-6277', 'AC-8140', 'AC-7287', 'AC-2071', 'AC-7074')
GROUP BY d.iBookNo, a.sCode
"@

Run "Their local Atlas credit activity" @"
SELECT d.iCode, a.sCode, COUNT(DISTINCT h.iHeaderId) AS RctCount, SUM(CASE WHEN d.mAmount1 > 0 THEN d.mAmount1 ELSE 0 END) AS SumCr
FROM dbo.tCore_Header_0 h
INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId = h.iHeaderId AND h.iVoucherType IN (256, 4096, 4608, 4609, 4610, 8707) AND d.bUpdateFA = 1 AND d.iFaTag = 2040 AND ISNULL(d.iType,0)=0 AND ISNULL(h.iAuth,1)=1 AND ISNULL(d.iAuthStatus,0)<2 AND ISNULL(h.bCancelled,0)=0 AND ISNULL(h.bVersion,0)=0 AND ISNULL(h.bSuspended,0)=0 AND ISNULL(d.bVoid,0)=0
INNER JOIN dbo.mCore_Account a ON a.iMasterId = d.iCode
WHERE a.sCode IN ('AC-6277', 'AC-8140', 'AC-7287', 'AC-2071', 'AC-7074')
GROUP BY d.iCode, a.sCode
"@

$conn.Close()
