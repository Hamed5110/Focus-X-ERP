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

# packed date for 2026-08-30 = 2026*65536 + 8*256 + 30 = 132778014
$asOn = 132778014

Write-Host "=== 1) FUTURE-DATED (> 30/08/2026) credit lines by status - iCode branch (my Adv filters) ==="
(Q "
SELECT v.ReportStatus, COUNT(*) AS Lines, SUM(d.mAmount1) AS SumM1
FROM dbo.tCore_Header_0 h
INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId = h.iHeaderId
INNER JOIN dbo.vaCore_Account v ON v.iMasterId = d.iCode AND v.iTreeId = 0
WHERE h.iVoucherType IN (256, 4096, 4608, 4609, 4610, 8707)
  AND d.bUpdateFA = 1 AND d.iFaTag = 2040 AND d.iCode > 0
  AND ISNULL(d.iType, 0) = 0 AND ISNULL(h.iAuth, 1) = 1 AND ISNULL(d.iAuthStatus, 0) < 2
  AND ISNULL(h.bCancelled, 0) = 0 AND ISNULL(h.bVersion, 0) = 0 AND ISNULL(h.bSuspended, 0) = 0
  AND ISNULL(d.bVoid, 0) = 0 AND d.mAmount1 > 0
  AND h.iDate > $asOn
GROUP BY v.ReportStatus ORDER BY v.ReportStatus
").Rows | Format-Table -AutoSize | Out-String -Width 120 | Write-Host

Write-Host "=== 2) FUTURE-DATED credit lines - iBookNo branch ==="
(Q "
SELECT v.ReportStatus, COUNT(*) AS Lines, SUM(d.mAmount2) AS SumM2
FROM dbo.tCore_Header_0 h
INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId = h.iHeaderId
INNER JOIN dbo.vaCore_Account v ON v.iMasterId = d.iBookNo AND v.iTreeId = 0
WHERE h.iVoucherType IN (256, 4096, 4608, 4609, 4610, 8707)
  AND d.bUpdateFA = 1 AND d.iFaTag = 2040 AND d.iBookNo > 0 AND d.iBookNo <> d.iCode
  AND ISNULL(d.iType, 0) = 0 AND ISNULL(h.iAuth, 1) = 1 AND ISNULL(d.iAuthStatus, 0) < 2
  AND ISNULL(h.bCancelled, 0) = 0 AND ISNULL(h.bVersion, 0) = 0 AND ISNULL(h.bSuspended, 0) = 0
  AND ISNULL(d.bVoid, 0) = 0 AND d.mAmount2 > 0
  AND h.iDate > $asOn
GROUP BY v.ReportStatus ORDER BY v.ReportStatus
").Rows | Format-Table -AutoSize | Out-String -Width 120 | Write-Host

Write-Host "=== 3) FUTURE-DATED credit line detail (status 1/2/3) ==="
(Q "
SELECT v.ReportStatus, a.sName, h.iVoucherType, h.sVoucherNo, h.iDate, d.mAmount1
FROM dbo.tCore_Header_0 h
INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId = h.iHeaderId
INNER JOIN dbo.vaCore_Account v ON v.iMasterId = d.iCode AND v.iTreeId = 0
INNER JOIN dbo.mCore_Account a ON a.iMasterId = d.iCode
WHERE h.iVoucherType IN (256, 4096, 4608, 4609, 4610, 8707)
  AND d.bUpdateFA = 1 AND d.iFaTag = 2040 AND d.iCode > 0
  AND ISNULL(d.iType, 0) = 0 AND ISNULL(h.iAuth, 1) = 1 AND ISNULL(d.iAuthStatus, 0) < 2
  AND ISNULL(h.bCancelled, 0) = 0 AND ISNULL(h.bVersion, 0) = 0 AND ISNULL(h.bSuspended, 0) = 0
  AND ISNULL(d.bVoid, 0) = 0 AND d.mAmount1 > 0
  AND h.iDate > $asOn AND v.ReportStatus IN (1,2,3)
ORDER BY v.ReportStatus, a.sName
").Rows | Format-Table -AutoSize | Out-String -Width 160 | Write-Host

Write-Host "=== 4) FUTURE-DATED SO (5634) vouchers by status (name-level, fNet) ==="
(Q "
SELECT v.ReportStatus, a.sName, h.sVoucherNo, h.iDate, h.fNet
FROM dbo.tCore_Header_0 h
INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId = h.iHeaderId
INNER JOIN dbo.mCore_Account a ON a.iMasterId = d.iBookNo
INNER JOIN dbo.vaCore_Account v ON v.iMasterId = d.iBookNo AND v.iTreeId = 0
WHERE h.iVoucherType = 5634 AND d.iFaTag = 2040 AND d.iBookNo > 0
  AND ISNULL(d.iType, 0) = 0 AND ISNULL(h.iAuth, 1) = 1
  AND ISNULL(h.bCancelled, 0) = 0 AND ISNULL(h.bVersion, 0) = 0 AND ISNULL(h.bSuspended, 0) = 0
  AND ISNULL(d.bVoid, 0) = 0
  AND h.iDate > $asOn AND v.ReportStatus IN (1,2,3)
ORDER BY v.ReportStatus, a.sName
").Rows | Format-Table -AutoSize | Out-String -Width 160 | Write-Host

$conn.Close()
