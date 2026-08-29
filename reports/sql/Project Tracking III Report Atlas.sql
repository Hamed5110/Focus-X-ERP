SELECT
    acc.sName AS Name,
    CAST(ISNULL(doc.ContractAmt, 0) AS decimal(18, 2)) AS [Total Contract Amount],
    CAST(ISNULL(fa.AdvRctAmt, 0) AS decimal(18, 2)) AS [Adv. Rct Amount],
    /* Balance = Total Contract Amount - Adv. Rct Amount (signed contract) */
    CAST(ISNULL(doc.ContractAmt, 0) - ISNULL(fa.AdvRctAmt, 0) AS decimal(18, 2)) AS [Balance Amount],
    CAST(ISNULL(acc.PlanValue, 0) AS decimal(18, 2)) AS [Plan Value]
FROM (
    SELECT
        iMasterId,
        MAX(sName) AS sName,
        MAX(PlanValue) AS PlanValue
    FROM dbo.vaCore_Account
    WHERE ReportStatus = 3
      AND ISNULL(bGroup, 0) = 0
    GROUP BY iMasterId
) acc
LEFT JOIN (
    /* Signed contract net (Focus "Reverse Sign" convention): minus-valued SO
       vouchers net against normal ones inside SUM; if they dominate, the
       account total shows with a minus (e.g. Abdulrahman Abdullah -205). */
    SELECT
        AccName,
        ROUND(SUM(VoucherAmt) * -1, 0) AS ContractAmt
    FROM (
        SELECT
            a.sName AS AccName,
            h.iHeaderId,
            MAX(h.fNet) AS VoucherAmt
        FROM dbo.tCore_Header_0 h
        INNER JOIN dbo.tCore_Data_0 d
            ON d.iHeaderId = h.iHeaderId
           AND h.iVoucherType = 5634
           AND d.iFaTag = 2040
           AND d.iBookNo > 0
           AND ISNULL(d.iType, 0) = 0
           AND ISNULL(h.iAuth, 1) = 1
           AND ISNULL(h.bCancelled, 0) = 0
           AND ISNULL(h.bVersion, 0) = 0
           AND ISNULL(h.bSuspended, 0) = 0
           AND ISNULL(d.bVoid, 0) = 0
        INNER JOIN dbo.mCore_Account a
            ON a.iMasterId = d.iBookNo
        GROUP BY a.sName, h.iHeaderId
    ) DocHdr
    GROUP BY AccName
) doc ON doc.AccName = acc.sName
LEFT JOIN (
    /* Cube Adv = sum of 6 Credit columns then DecimalInColumn=0.
       Per XML: type 4610 (CRM Adv) DecimalInColumn=0; other credits = 2. */
    SELECT
        AccName,
        ROUND(SUM(
            CASE
                WHEN iVoucherType = 4610 THEN ROUND(TypeAmt, 0)
                ELSE ROUND(TypeAmt, 2)
            END
        ), 0) AS AdvRctAmt
    FROM (
        SELECT
            AccName,
            iVoucherType,
            SUM(CreditAmt) AS TypeAmt
        FROM (
            SELECT
                a.sName AS AccName,
                h.iVoucherType,
                CASE WHEN d.mAmount1 > 0 THEN d.mAmount1 ELSE 0 END AS CreditAmt
            FROM dbo.tCore_Header_0 h
            INNER JOIN dbo.tCore_Data_0 d
                ON d.iHeaderId = h.iHeaderId
               AND h.iVoucherType IN (256, 4096, 4608, 4609, 4610, 8707)
               AND d.bUpdateFA = 1
               AND d.iFaTag = 2040
               AND d.iCode > 0
               AND ISNULL(d.iType, 0) = 0
               AND ISNULL(h.iAuth, 1) = 1
               AND ISNULL(d.iAuthStatus, 0) < 2
               AND ISNULL(h.bCancelled, 0) = 0
               AND ISNULL(h.bVersion, 0) = 0
               AND ISNULL(h.bSuspended, 0) = 0
               AND ISNULL(d.bVoid, 0) = 0
            INNER JOIN dbo.mCore_Account a
                ON a.iMasterId = d.iCode
            UNION ALL
            SELECT
                a.sName AS AccName,
                h.iVoucherType,
                CASE WHEN d.mAmount2 > 0 THEN d.mAmount2 ELSE 0 END AS CreditAmt
            FROM dbo.tCore_Header_0 h
            INNER JOIN dbo.tCore_Data_0 d
                ON d.iHeaderId = h.iHeaderId
               AND h.iVoucherType IN (256, 4096, 4608, 4609, 4610, 8707)
               AND d.bUpdateFA = 1
               AND d.iFaTag = 2040
               AND d.iBookNo > 0
               AND d.iBookNo <> d.iCode
               AND ISNULL(d.iType, 0) = 0
               AND ISNULL(h.iAuth, 1) = 1
               AND ISNULL(d.iAuthStatus, 0) < 2
               AND ISNULL(h.bCancelled, 0) = 0
               AND ISNULL(h.bVersion, 0) = 0
               AND ISNULL(h.bSuspended, 0) = 0
               AND ISNULL(d.bVoid, 0) = 0
            INNER JOIN dbo.mCore_Account a
                ON a.iMasterId = d.iBookNo
        ) Cr
        GROUP BY AccName, iVoucherType
    ) ByType
    GROUP BY AccName
) fa ON fa.AccName = acc.sName
WHERE acc.iMasterId > 0
