$cs = "Server=localhost;Database=Focus8080;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()

function Run($title, $sql) {
    $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 180; $cmd.CommandText = $sql
    $a = New-Object System.Data.SqlClient.SqlDataAdapter $cmd; $dt = New-Object System.Data.DataTable
    [void]$a.Fill($dt)
    Write-Host "`n== $title =="
    $dt | Format-Table -AutoSize | Out-String -Width 250 | Write-Host
}

Run "Depth distribution of TR(280) descendants in tree 0" @"
WITH Tree0 AS (
    SELECT iMasterId, 0 AS depth FROM dbo.mCore_AccountTreeDetails WHERE iTreeId = 0 AND iMasterId = 280
    UNION ALL
    SELECT t.iMasterId, x.depth + 1 FROM dbo.mCore_AccountTreeDetails t INNER JOIN Tree0 x ON t.iParentId = x.iMasterId AND t.iTreeId = 0
)
SELECT depth, COUNT(*) AS nodes, SUM(CASE WHEN d.iMasterId IS NULL THEN 1 ELSE 0 END) AS notInDetails
FROM Tree0 t
LEFT JOIN dbo.mCore_AccountTreeDetails d ON d.iTreeId = 0 AND d.iMasterId = t.iMasterId
GROUP BY depth ORDER BY depth
"@

Run "Active status-3 accounts by depth under TR" @"
WITH Act AS (
    SELECT DISTINCT d.iBookNo AS iMasterId
    FROM dbo.tCore_Header_0 h
    INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId = h.iHeaderId AND h.iVoucherType = 5634 AND d.iFaTag = 2040 AND d.iBookNo > 0 AND ISNULL(d.iType, 0) = 0 AND ISNULL(h.iAuth, 1) = 1 AND ISNULL(h.bCancelled, 0) = 0 AND ISNULL(h.bVersion, 0) = 0 AND ISNULL(h.bSuspended, 0) = 0 AND ISNULL(d.bVoid, 0) = 0
    UNION
    SELECT d.iCode
    FROM dbo.tCore_Header_0 h
    INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId = h.iHeaderId AND h.iVoucherType IN (256, 4096, 4608, 4609, 4610, 8707) AND d.bUpdateFA = 1 AND d.iFaTag = 2040 AND d.iCode > 0 AND ISNULL(d.iType, 0) = 0 AND ISNULL(h.iAuth, 1) = 1 AND ISNULL(d.iAuthStatus, 0) < 2 AND ISNULL(h.bCancelled, 0) = 0 AND ISNULL(h.bVersion, 0) = 0 AND ISNULL(h.bSuspended, 0) = 0 AND ISNULL(d.bVoid, 0) = 0
    UNION
    SELECT d.iBookNo
    FROM dbo.tCore_Header_0 h
    INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId = h.iHeaderId AND h.iVoucherType IN (256, 4096, 4608, 4609, 4610, 8707) AND d.bUpdateFA = 1 AND d.iFaTag = 2040 AND d.iBookNo > 0 AND d.iBookNo <> d.iCode AND ISNULL(d.iType, 0) = 0 AND ISNULL(h.iAuth, 1) = 1 AND ISNULL(d.iAuthStatus, 0) < 2 AND ISNULL(h.bCancelled, 0) = 0 AND ISNULL(h.bVersion, 0) = 0 AND ISNULL(h.bSuspended, 0) = 0 AND ISNULL(d.bVoid, 0) = 0
),
Tree0 AS (
    SELECT iMasterId, 0 AS depth FROM dbo.mCore_AccountTreeDetails WHERE iTreeId = 0 AND iMasterId = 280
    UNION ALL
    SELECT t.iMasterId, x.depth + 1 FROM dbo.mCore_AccountTreeDetails t INNER JOIN Tree0 x ON t.iParentId = x.iMasterId AND t.iTreeId = 0
)
SELECT t.depth, COUNT(DISTINCT a.iMasterId) AS activeStatus3
FROM Tree0 t
INNER JOIN (SELECT iMasterId FROM dbo.vaCore_Account WHERE ReportStatus = 3 AND ISNULL(bGroup,0) = 0) a ON a.iMasterId = t.iMasterId
INNER JOIN Act ON Act.iMasterId = a.iMasterId
GROUP BY t.depth ORDER BY t.depth
"@

$conn.Close()
