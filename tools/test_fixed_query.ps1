$cs = "Server=localhost;Database=Focus8080;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()

function Run($title, $sql) {
    $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 280; $cmd.CommandText = $sql
    $a = New-Object System.Data.SqlClient.SqlDataAdapter $cmd; $dt = New-Object System.Data.DataTable
    [void]$a.Fill($dt)
    Write-Host "`n== $title =="
    $dt | Format-Table -AutoSize | Out-String -Width 250 | Write-Host
}

Run "Name lookup for Trade Receivables group id" @"
SELECT iMasterId, sName, bGroup FROM dbo.mCore_Account WHERE sName = 'Trade Receivables'
"@

Run "Fixed summary query (with TR group filter)" @"
SELECT
    (SELECT sName FROM dbo.mCore_reportstatus WHERE iMasterId = x.ReportStatus) AS [Report Status],
    CAST(ISNULL(SUM(x.[Total Contract Amount]), 0) AS decimal(18, 2)) AS [Total Contract Amount],
    CAST(ISNULL(SUM(x.[Adv. Rct Amount]), 0) AS decimal(18, 2)) AS [Adv. Rct Amount],
    CAST(ISNULL(SUM(x.[Balance Amount]), 0) AS decimal(18, 2)) AS [Balance Amount],
    CAST(ISNULL(SUM(x.[Plan Value]), 0) AS decimal(18, 2)) AS [Plan Value],
    COUNT(*) AS [No. of Accounts]
FROM (
    SELECT
        acc.ReportStatus,
        acc.sName AS Name,
        CAST(ISNULL(doc.ContractAmt, 0) AS decimal(18, 2)) AS [Total Contract Amount],
        CAST(ISNULL(fa.AdvRctAmt, 0) AS decimal(18, 2)) AS [Adv. Rct Amount],
        CAST(ISNULL(doc.ContractAmt, 0) - ISNULL(fa.AdvRctAmt, 0) AS decimal(18, 2)) AS [Balance Amount],
        CAST(ISNULL(acc.PlanValue, 0) AS decimal(18, 2)) AS [Plan Value]
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
) x
GROUP BY x.ReportStatus
ORDER BY x.ReportStatus
"@

$conn.Close()
