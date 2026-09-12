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

Write-Host "=== 1) bBrs distribution on document-set lines (dept 2040, live) ==="
(Q "
SELECT h.iVoucherType, ISNULL(CAST(d.bBrs AS int), -1) AS Brs, COUNT(*) AS Lines
FROM dbo.tCore_Header_0 h
INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId = h.iHeaderId
WHERE h.iVoucherType IN (5634, 5635, 6145) AND d.iFaTag = 2040
  AND ISNULL(h.iAuth,1)=1 AND ISNULL(h.bCancelled,0)=0 AND ISNULL(h.bVersion,0)=0
  AND ISNULL(h.bSuspended,0)=0 AND ISNULL(d.bVoid,0)=0
GROUP BY h.iVoucherType, ISNULL(CAST(d.bBrs AS int), -1)
ORDER BY h.iVoucherType, Brs
").Rows | Format-Table -AutoSize | Out-String -Width 120 | Write-Host

Write-Host "=== 2) Unreconciled (bBrs=0) credit lines by account ReportStatus (cube types, dept 2040, my Adv filters) ==="
(Q "
SELECT v.ReportStatus,
       COUNT(*) AS Lines,
       SUM(CASE WHEN d.mAmount1 > 0 THEN d.mAmount1 ELSE 0 END) AS SumM1,
       SUM(CASE WHEN d.mAmount2 > 0 AND d.iBookNo <> d.iCode THEN d.mAmount2 ELSE 0 END) AS SumM2
FROM dbo.tCore_Header_0 h
INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId = h.iHeaderId
INNER JOIN dbo.vaCore_Account v ON v.iMasterId = d.iCode AND v.iTreeId = 0
WHERE h.iVoucherType IN (256, 4096, 4608, 4609, 4610, 8707)
  AND d.bUpdateFA = 1 AND d.iFaTag = 2040 AND d.iCode > 0
  AND ISNULL(d.iType, 0) = 0 AND ISNULL(h.iAuth, 1) = 1 AND ISNULL(d.iAuthStatus, 0) < 2
  AND ISNULL(h.bCancelled, 0) = 0 AND ISNULL(h.bVersion, 0) = 0 AND ISNULL(h.bSuspended, 0) = 0
  AND ISNULL(d.bVoid, 0) = 0
  AND ISNULL(d.bBrs, 0) = 0
GROUP BY v.ReportStatus
ORDER BY v.ReportStatus
").Rows | Format-Table -AutoSize | Out-String -Width 120 | Write-Host

Write-Host "=== 3) Unreconciled credit lines detail for status 2/3 accounts (iCode branch) ==="
(Q "
SELECT v.ReportStatus, a.sName, h.iVoucherType, h.sVoucherNo, d.mAmount1, d.mAmount2, d.iCode, d.iBookNo
FROM dbo.tCore_Header_0 h
INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId = h.iHeaderId
INNER JOIN dbo.vaCore_Account v ON v.iMasterId = d.iCode AND v.iTreeId = 0
INNER JOIN dbo.mCore_Account a ON a.iMasterId = d.iCode
WHERE h.iVoucherType IN (256, 4096, 4608, 4609, 4610, 8707)
  AND d.bUpdateFA = 1 AND d.iFaTag = 2040 AND d.iCode > 0
  AND ISNULL(d.iType, 0) = 0 AND ISNULL(h.iAuth, 1) = 1 AND ISNULL(d.iAuthStatus, 0) < 2
  AND ISNULL(h.bCancelled, 0) = 0 AND ISNULL(h.bVersion, 0) = 0 AND ISNULL(h.bSuspended, 0) = 0
  AND ISNULL(d.bVoid, 0) = 0
  AND ISNULL(d.bBrs, 0) = 0
  AND v.ReportStatus IN (1, 2, 3)
ORDER BY v.ReportStatus, a.sName
").Rows | Format-Table -AutoSize | Out-String -Width 180 | Write-Host

Write-Host "=== 4) Unreconciled iBookNo-branch lines (mAmount2>0, iBookNo<>iCode) by status ==="
(Q "
SELECT v.ReportStatus,
       COUNT(*) AS Lines,
       SUM(CASE WHEN d.mAmount2 > 0 THEN d.mAmount2 ELSE 0 END) AS SumM2
FROM dbo.tCore_Header_0 h
INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId = h.iHeaderId
INNER JOIN dbo.vaCore_Account v ON v.iMasterId = d.iBookNo AND v.iTreeId = 0
WHERE h.iVoucherType IN (256, 4096, 4608, 4609, 4610, 8707)
  AND d.bUpdateFA = 1 AND d.iFaTag = 2040 AND d.iBookNo > 0 AND d.iBookNo <> d.iCode
  AND ISNULL(d.iType, 0) = 0 AND ISNULL(h.iAuth, 1) = 1 AND ISNULL(d.iAuthStatus, 0) < 2
  AND ISNULL(h.bCancelled, 0) = 0 AND ISNULL(h.bVersion, 0) = 0 AND ISNULL(h.bSuspended, 0) = 0
  AND ISNULL(d.bVoid, 0) = 0
  AND ISNULL(d.bBrs, 0) = 0
GROUP BY v.ReportStatus
ORDER BY v.ReportStatus
").Rows | Format-Table -AutoSize | Out-String -Width 120 | Write-Host

$conn.Close()
