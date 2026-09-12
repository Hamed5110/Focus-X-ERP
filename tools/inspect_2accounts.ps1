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

Run "Account master state for 12606 and 15268" @"
SELECT iMasterId, sName, sCode, ReportStatus, bGroup, iParentId, iTreeId
FROM dbo.vaCore_Account
WHERE iMasterId IN (12606, 15268)
"@

Run "Their tree-0 membership (parent chain)" @"
SELECT t.iMasterId, t.iParentId, t.iLevel, p.sName AS ParentName
FROM dbo.mCore_AccountTreeDetails t
LEFT JOIN dbo.mCore_Account p ON p.iMasterId = t.iParentId
WHERE t.iTreeId = 0 AND t.iMasterId IN (12606, 15268)
"@

Run "Their Atlas SO documents (5634)" @"
SELECT h.iHeaderId, h.sVoucherNo, h.iDate, h.fNet, h.iAuth, h.bCancelled, h.bSuspended, h.bVersion, d.iBookNo, d.iFaTag, d.bVoid, d.iType
FROM dbo.tCore_Header_0 h
INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId = h.iHeaderId
WHERE h.iVoucherType = 5634 AND d.iFaTag = 2040 AND d.iBookNo IN (12606, 15268)
ORDER BY d.iBookNo, h.iDate
"@

Run "Their Atlas credit documents" @"
SELECT h.iHeaderId, h.iVoucherType, h.sVoucherNo, h.iDate, h.iAuth, h.bCancelled, h.bSuspended, h.bVersion, d.iCode, d.iBookNo, d.mAmount1, d.mAmount2, d.bUpdateFA, d.iAuthStatus, d.bVoid, d.iFaTag
FROM dbo.tCore_Header_0 h
INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId = h.iHeaderId
WHERE h.iVoucherType IN (256, 4096, 4608, 4609, 4610, 8707) AND d.iFaTag = 2040 AND (d.iCode IN (12606, 15268) OR d.iBookNo IN (12606, 15268))
ORDER BY d.iCode, h.iDate
"@

$conn.Close()
