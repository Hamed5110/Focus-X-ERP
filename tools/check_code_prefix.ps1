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

Run "Sample account codes under TR and outside" @"
SELECT TOP 12 a.iMasterId, a.sCode, a.sName, a.iParentId
FROM dbo.vaCore_Account a
WHERE ISNULL(a.bGroup,0)=0 AND a.iTreeId=0
ORDER BY a.sCode
"@

Run "Code-prefix 180% vs tree-0 membership for active accounts" @"
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
Acc AS (
    SELECT iMasterId, MAX(ReportStatus) AS ReportStatus, MAX(sName) AS sName, MAX(sCode) AS sCode
    FROM dbo.vaCore_Account
    WHERE ReportStatus IN (1, 2, 3) AND ISNULL(bGroup, 0) = 0
    GROUP BY iMasterId
),
Tree0 AS (
    SELECT iMasterId FROM dbo.mCore_AccountTreeDetails WHERE iTreeId = 0 AND iMasterId = 280
    UNION ALL
    SELECT t.iMasterId FROM dbo.mCore_AccountTreeDetails t INNER JOIN Tree0 x ON t.iParentId = x.iMasterId AND t.iTreeId = 0
)
SELECT a.ReportStatus,
    COUNT(*) AS total,
    SUM(CASE WHEN t0.iMasterId IS NOT NULL THEN 1 ELSE 0 END) AS inTree,
    SUM(CASE WHEN a.sCode LIKE '180%' THEN 1 ELSE 0 END) AS codePrefix,
    SUM(CASE WHEN t0.iMasterId IS NOT NULL AND a.sCode LIKE '180%' THEN 1 ELSE 0 END) AS both,
    SUM(CASE WHEN t0.iMasterId IS NULL AND a.sCode LIKE '180%' THEN 1 ELSE 0 END) AS prefixNotTree,
    SUM(CASE WHEN t0.iMasterId IS NOT NULL AND a.sCode NOT LIKE '180%' THEN 1 ELSE 0 END) AS treeNotPrefix
FROM Acc a
INNER JOIN Act ON Act.iMasterId = a.iMasterId
LEFT JOIN Tree0 t0 ON t0.iMasterId = a.iMasterId
GROUP BY a.ReportStatus
ORDER BY a.ReportStatus
"@

$conn.Close()
