$cs = "Server=localhost;Database=Focus8080;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()

function Run($title, $sql) {
    $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 300; $cmd.CommandText = $sql
    $a = New-Object System.Data.SqlClient.SqlDataAdapter $cmd; $dt = New-Object System.Data.DataTable
    [void]$a.Fill($dt)
    Write-Host "`n== $title =="
    $dt | Format-Table -AutoSize | Out-String -Width 300 | Write-Host
}

Run "Voucher types of twin activity: 14589 (Ahmed twin), 9936 (Hussain twin), 16895 (Hassan Ali twin)" @"
SELECT d.iCode, d.iBookNo, h.iVoucherType, COUNT(*) AS Lines, SUM(d.mAmount1) AS SumM1, SUM(d.mAmount2) AS SumM2
FROM dbo.tCore_Header_0 h
INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId = h.iHeaderId
WHERE d.iFaTag = 2040 AND (d.iCode IN (14589, 9936, 16895) OR d.iBookNo IN (14589, 9936, 16895))
  AND ISNULL(h.bCancelled, 0) = 0 AND ISNULL(h.bVersion, 0) = 0 AND ISNULL(h.bSuspended, 0) = 0 AND ISNULL(d.bVoid, 0) = 0
GROUP BY d.iCode, d.iBookNo, h.iVoucherType
ORDER BY d.iCode, d.iBookNo, h.iVoucherType
"@

Run "Name-groups where cube-contributing members have MIXED statuses" @"
WITH TR AS (
    SELECT v.iMasterId, v.sName, v.ReportStatus
    FROM dbo.vaCore_Account v
    WHERE v.iTreeId = 0 AND ISNULL(v.bGroup, 0) = 0
      AND v.iMasterId IN (
          SELECT tr.iMasterId FROM dbo.mCore_AccountTreeDetails tr
          WHERE tr.iTreeId = 0 AND tr.iParentId IN (SELECT iMasterId FROM dbo.mCore_Account WHERE sName = 'Trade Receivables' AND ISNULL(bGroup,0)=1)
      )
),
Contrib AS (
    SELECT DISTINCT d.iCode AS iMasterId
    FROM dbo.tCore_Header_0 h
    INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId = h.iHeaderId
    WHERE d.iFaTag = 2040 AND d.iCode > 0
      AND ISNULL(h.bCancelled, 0) = 0 AND ISNULL(h.bVersion, 0) = 0 AND ISNULL(h.bSuspended, 0) = 0 AND ISNULL(d.bVoid, 0) = 0
    UNION
    SELECT DISTINCT d.iBookNo
    FROM dbo.tCore_Header_0 h
    INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId = h.iHeaderId
    WHERE d.iFaTag = 2040 AND d.iBookNo > 0 AND h.iVoucherType IN (5634, 5635, 6145)
      AND ISNULL(h.bCancelled, 0) = 0 AND ISNULL(h.bVersion, 0) = 0 AND ISNULL(h.bSuspended, 0) = 0 AND ISNULL(d.bVoid, 0) = 0
)
SELECT tr.sName,
       COUNT(DISTINCT tr.iMasterId) AS Members,
       COUNT(DISTINCT CASE WHEN c.iMasterId IS NOT NULL THEN tr.iMasterId END) AS ContribMembers,
       STRING_AGG(CAST(CASE WHEN c.iMasterId IS NOT NULL THEN tr.ReportStatus END AS varchar), ',') AS ContribStatuses
FROM TR
LEFT JOIN Contrib c ON c.iMasterId = tr.iMasterId
GROUP BY tr.sName
HAVING COUNT(DISTINCT CASE WHEN c.iMasterId IS NOT NULL THEN tr.ReportStatus END) > 1
ORDER BY tr.sName
"@

$conn.Close()
