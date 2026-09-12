$cs = "Server=localhost;Database=Focus8080;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 280
$cmd.CommandText = @"
WITH base AS (
    SELECT
        acc.iMasterId,
        acc.ReportStatus,
        acc.sName AS Name,
        CAST(ISNULL(doc.ContractAmt, 0) AS decimal(18, 2)) AS ContractAmt,
        CAST(ISNULL(fa.AdvRctAmt, 0) AS decimal(18, 2)) AS AdvRctAmt,
        CAST(ISNULL(acc.PlanValue, 0) AS decimal(18, 2)) AS PlanValue
    FROM (
        SELECT iMasterId, MAX(ReportStatus) AS ReportStatus, MAX(sName) AS sName, MAX(PlanValue) AS PlanValue
        FROM dbo.vaCore_Account
        WHERE ReportStatus IN (1, 2, 3) AND ISNULL(bGroup, 0) = 0
        GROUP BY iMasterId
    ) acc
    INNER JOIN (
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
    ) act ON act.iMasterId = acc.iMasterId
    LEFT JOIN (
        SELECT AccName, ROUND(SUM(VoucherAmt) * -1, 0) AS ContractAmt
        FROM (
            SELECT a.sName AS AccName, h.iHeaderId, MAX(h.fNet) AS VoucherAmt
            FROM dbo.tCore_Header_0 h
            INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId = h.iHeaderId AND h.iVoucherType = 5634 AND d.iFaTag = 2040 AND d.iBookNo > 0 AND ISNULL(d.iType, 0) = 0 AND ISNULL(h.iAuth, 1) = 1 AND ISNULL(h.bCancelled, 0) = 0 AND ISNULL(h.bVersion, 0) = 0 AND ISNULL(h.bSuspended, 0) = 0 AND ISNULL(d.bVoid, 0) = 0
            INNER JOIN dbo.mCore_Account a ON a.iMasterId = d.iBookNo
            GROUP BY a.sName, h.iHeaderId
        ) DocHdr
        GROUP BY AccName
    ) doc ON doc.AccName = acc.sName
    LEFT JOIN (
        SELECT AccName, ROUND(SUM(CASE WHEN iVoucherType = 4610 THEN ROUND(TypeAmt, 0) ELSE ROUND(TypeAmt, 2) END), 0) AS AdvRctAmt
        FROM (
            SELECT AccName, iVoucherType, SUM(CreditAmt) AS TypeAmt
            FROM (
                SELECT a.sName AS AccName, h.iVoucherType, CASE WHEN d.mAmount1 > 0 THEN d.mAmount1 ELSE 0 END AS CreditAmt
                FROM dbo.tCore_Header_0 h
                INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId = h.iHeaderId AND h.iVoucherType IN (256, 4096, 4608, 4609, 4610, 8707) AND d.bUpdateFA = 1 AND d.iFaTag = 2040 AND d.iCode > 0 AND ISNULL(d.iType, 0) = 0 AND ISNULL(h.iAuth, 1) = 1 AND ISNULL(d.iAuthStatus, 0) < 2 AND ISNULL(h.bCancelled, 0) = 0 AND ISNULL(h.bVersion, 0) = 0 AND ISNULL(h.bSuspended, 0) = 0 AND ISNULL(d.bVoid, 0) = 0
                INNER JOIN dbo.mCore_Account a ON a.iMasterId = d.iCode
                UNION ALL
                SELECT a.sName AS AccName, h.iVoucherType, CASE WHEN d.mAmount2 > 0 THEN d.mAmount2 ELSE 0 END AS CreditAmt
                FROM dbo.tCore_Header_0 h
                INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId = h.iHeaderId AND h.iVoucherType IN (256, 4096, 4608, 4609, 4610, 8707) AND d.bUpdateFA = 1 AND d.iFaTag = 2040 AND d.iBookNo > 0 AND d.iBookNo <> d.iCode AND ISNULL(d.iType, 0) = 0 AND ISNULL(h.iAuth, 1) = 1 AND ISNULL(d.iAuthStatus, 0) < 2 AND ISNULL(h.bCancelled, 0) = 0 AND ISNULL(h.bVersion, 0) = 0 AND ISNULL(h.bSuspended, 0) = 0 AND ISNULL(d.bVoid, 0) = 0
                INNER JOIN dbo.mCore_Account a ON a.iMasterId = d.iBookNo
            ) Cr
            GROUP BY AccName, iVoucherType
        ) ByType
        GROUP BY AccName
    ) fa ON fa.AccName = acc.sName
    WHERE acc.iMasterId > 0
      AND acc.iMasterId IN (
            SELECT tr.iMasterId FROM dbo.mCore_AccountTreeDetails tr
            WHERE tr.iTreeId = 0
              AND tr.iParentId IN (SELECT iMasterId FROM dbo.mCore_Account WHERE sName = 'Trade Receivables' AND ISNULL(bGroup, 0) = 1)
          )
)
SELECT ReportStatus,
    COUNT(*) AS total,
    SUM(CASE WHEN ContractAmt = 0 AND AdvRctAmt = 0 THEN 1 ELSE 0 END) AS zeroContractAndAdv,
    SUM(CASE WHEN ContractAmt = 0 AND AdvRctAmt = 0 AND PlanValue = 0 THEN 1 ELSE 0 END) AS allZero
FROM base
GROUP BY ReportStatus
ORDER BY ReportStatus
"@
$a = New-Object System.Data.SqlClient.SqlDataAdapter $cmd
$dt = New-Object System.Data.DataTable
[void]$a.Fill($dt)
$conn.Close()
$dt | Format-Table -AutoSize | Out-String -Width 200 | Write-Host
