/* ============================================================================
   Monthly Commission Atlas — Detail (contract + customer sandwich)

   Bottom: CON-Atl (5634) — contract no, date, ABS(fNet), customer iBookNo
   Filling: CI-001/004 receipts — cash in the period / lifetime
   Top: customer + salesman presentation

   Contract amount is CON net, not the typed receipt extra.
   Link a receipt to a CON, first match:
     customer and rounded typed extra equals rounded CON net;
     else the only CON-Atl on that receipt date for the customer;
     else the only CON-Atl for the customer through @iEndDate.
   No CON and failed 45 percent gate are excluded. This list is passed
   contracts only, so Gate Status is always Yes. Not Eligible is omitted.

   One row per first-pay receipt. Contract / Lifetime / % /
   Eligible / Commission print on the first receipt of that contract
   only so Focus Sum cannot double them.

   Period: h.iDate BETWEEN @iStartDate AND @iEndDate
   Do not DECLARE. Do not convert result iDate.
   ============================================================================ */
SELECT
    x.Department,
    x.MonthYear AS [Month Year],
    x.Salesman,
    x.CustomerName AS [Customer Name],
    CAST(NULLIF(x.SONo, N'') AS nvarchar(40)) AS [Contract / Sales Order No.],
    CAST(NULLIF(x.SODateText, N'') AS nvarchar(10)) AS [Contract Date],
    x.PayName AS [Payment Code],
    CAST(CASE WHEN x.ReceiptRn = 1 THEN x.ContractAmt ELSE 0 END AS decimal(18, 2)) AS [Total Contract Amount],
    CAST(x.CollectionAmt AS decimal(18, 2)) AS [Total Collection],
    CAST(x.ReceiptNo AS nvarchar(40)) AS [Receipt No.],
    CAST(NULLIF(x.ReceiptDateText, N'') AS nvarchar(10)) AS [Receipt Date],
    CAST(CASE WHEN x.ReceiptRn = 1 THEN x.LifeColl ELSE 0 END AS decimal(18, 2)) AS [Lifetime Collection],
    CAST(CASE WHEN x.ReceiptRn = 1 THEN x.CollectPct ELSE 0 END AS decimal(18, 2)) AS [Collection %],
    CAST(N'Yes' AS nvarchar(3)) AS [Gate Status],
    CAST(x.CollEligibleAmt AS decimal(18, 2)) AS [Collection Eligible Amount],
    CAST(CASE WHEN x.ReceiptRn = 1 THEN x.EligibleAmt ELSE 0 END AS decimal(18, 2)) AS [Eligible Amount],
    CAST(CASE WHEN x.Rn = 1 THEN x.OverallSales ELSE 0 END AS decimal(18, 2)) AS [Overall Sales],
    CAST(CASE WHEN x.Rn = 1 THEN x.RatePct ELSE 0 END AS decimal(18, 2)) AS [Commission Ratio],
    CAST(CASE WHEN x.ReceiptRn = 1 THEN x.CommissionAmt ELSE 0 END AS decimal(18, 2)) AS [Total Commission Amount],
    CAST(x.iDate AS decimal(18, 0)) AS iDate
FROM (
    SELECT
        w.Department,
        w.MonthYear,
        w.Salesman,
        w.CustomerName,
        w.SONo,
        CASE
            WHEN w.SODate > 0 THEN
                CAST((w.SODate & 0xfff0000) / 65536 AS varchar(4))
                + N'-'
                + RIGHT(N'0' + CAST((w.SODate & 0xff00) / 256 AS varchar(2)), 2)
                + N'-'
                + RIGHT(N'0' + CAST((w.SODate & 0xff) AS varchar(2)), 2)
            ELSE N''
        END AS SODateText,
        w.ReceiptNo,
        CASE
            WHEN w.ReceiptPacked > 0 THEN
                CAST((w.ReceiptPacked & 0xfff0000) / 65536 AS varchar(4))
                + N'-'
                + RIGHT(N'0' + CAST((w.ReceiptPacked & 0xff00) / 256 AS varchar(2)), 2)
                + N'-'
                + RIGHT(N'0' + CAST((w.ReceiptPacked & 0xff) AS varchar(2)), 2)
            ELSE N''
        END AS ReceiptDateText,
        w.PayName,
        w.ContractAmt,
        w.CollectionAmt,
        w.LifeColl,
        CASE
            WHEN w.ContractVal > 0 THEN w.LifeColl * 100.0 / w.ContractVal
            ELSE 0
        END AS CollectPct,
        w.CollEligibleAmt,
        w.EligibleAmt,
        SUM(CASE WHEN w.ReceiptRn = 1 THEN w.EligibleAmt ELSE 0 END) OVER (
            PARTITION BY w.Department, w.MonthYear
        ) AS OverallSales,
        CASE
            WHEN SUM(CASE WHEN w.ReceiptRn = 1 THEN w.EligibleAmt ELSE 0 END) OVER (PARTITION BY w.Department, w.MonthYear) >= 300000 THEN 1.70
            WHEN SUM(CASE WHEN w.ReceiptRn = 1 THEN w.EligibleAmt ELSE 0 END) OVER (PARTITION BY w.Department, w.MonthYear) >= 250000 THEN 1.50
            WHEN SUM(CASE WHEN w.ReceiptRn = 1 THEN w.EligibleAmt ELSE 0 END) OVER (PARTITION BY w.Department, w.MonthYear) >= 200000 THEN 1.20
            WHEN SUM(CASE WHEN w.ReceiptRn = 1 THEN w.EligibleAmt ELSE 0 END) OVER (PARTITION BY w.Department, w.MonthYear) >= 150000 THEN 1.00
            ELSE 0
        END AS RatePct,
        w.Rn,
        w.ReceiptRn,
        CAST(w.EligibleAmt *
            CASE
                WHEN SUM(CASE WHEN w.ReceiptRn = 1 THEN w.EligibleAmt ELSE 0 END) OVER (PARTITION BY w.Department, w.MonthYear) >= 300000 THEN 0.017
                WHEN SUM(CASE WHEN w.ReceiptRn = 1 THEN w.EligibleAmt ELSE 0 END) OVER (PARTITION BY w.Department, w.MonthYear) >= 250000 THEN 0.015
                WHEN SUM(CASE WHEN w.ReceiptRn = 1 THEN w.EligibleAmt ELSE 0 END) OVER (PARTITION BY w.Department, w.MonthYear) >= 200000 THEN 0.012
                WHEN SUM(CASE WHEN w.ReceiptRn = 1 THEN w.EligibleAmt ELSE 0 END) OVER (PARTITION BY w.Department, w.MonthYear) >= 150000 THEN 0.010
                ELSE 0
            END
        AS decimal(18, 4)) AS CommissionAmt,
        w.iDate
    FROM (
    SELECT
        g.Department,
        g.MonthYear,
        g.Salesman,
        g.CustomerName,
        g.SONo,
        g.SODate,
        g.ReceiptNo,
        g.ReceiptPacked,
        g.PayName,
        g.ContractAmt,
        g.CollectionAmt,
        g.LifeColl,
        g.ContractVal,
        g.CollEligibleAmt,
        g.EligibleAmt,
        g.iDate,
        ROW_NUMBER() OVER (
            PARTITION BY g.Department, g.MonthYear
            ORDER BY g.Salesman, g.CustomerName, g.ReceiptPacked, g.ReceiptNo
        ) AS Rn,
        ROW_NUMBER() OVER (
            PARTITION BY g.Department, g.MonthYear, g.CustId,
                         CASE WHEN g.SONo <> N'' THEN g.SONo ELSE CAST(g.ContractVal AS varchar(40)) END
            ORDER BY g.ReceiptPacked, g.ReceiptNo
        ) AS ReceiptRn
    FROM (
        SELECT
            p.Department,
            p.MonthYear,
            p.Salesman,
            p.CustomerName,
            p.CustId,
            p.SONo,
            p.SODate,
            p.ReceiptNo,
            p.ReceiptPacked,
            p.PayName,
            CAST(p.ContractVal / n.SplitN AS decimal(18, 4)) AS ContractAmt,
            CAST(p.PeriodColl / n.SplitN AS decimal(18, 4)) AS CollectionAmt,
            CAST(ISNULL(c.LifeColl, 0) AS decimal(18, 4)) AS LifeColl,
            CAST(p.ContractVal AS decimal(18, 4)) AS ContractVal,
            CAST(SUM(
                CASE
                    WHEN p.ContractVal > 0
                         AND ISNULL(c.LifeColl, 0) * (100.0 / 45.0) + 1 >= p.ContractVal
                    THEN p.PeriodColl
                    ELSE 0
                END / n.SplitN
            ) AS decimal(18, 4)) AS CollEligibleAmt,
            CAST(SUM(
                CASE
                    WHEN p.ContractVal > 0
                         AND ISNULL(c.LifeColl, 0) * (100.0 / 45.0) + 1 >= p.ContractVal
                    THEN p.ContractVal
                    ELSE 0
                END / n.SplitN
            ) AS decimal(18, 4)) AS EligibleAmt,
            p.ReceiptPacked AS iDate
        FROM (
            SELECT
                r.Department,
                r.MonthYear,
                r.Salesman,
                r.CustomerName,
                r.CustId,
                r.ReceiptNo,
                r.ReceiptPacked,
                r.PayName,
                r.PeriodColl,
                ISNULL(COALESCE(NULLIF(so.SONo, N''), NULLIF(sod.SONo, N''), so1.SONo), N'') AS SONo,
                ISNULL(COALESCE(NULLIF(so.SODate, 0), NULLIF(sod.SODate, 0), so1.SODate), 0) AS SODate,
                CAST(COALESCE(so.ContractNet, sod.ContractNet, so1.ContractNet, r.TypedContract) AS decimal(18, 4)) AS ContractVal
            FROM (
                SELECT
                    dep.sName AS Department,
                    dbo.GetDateName(N'm', h.iDate)
                        + N'-'
                        + CAST((h.iDate & 0xfff0000) / 65536 AS varchar(4)) AS MonthYear,
                    CASE WHEN ISNULL(sm.sName, N'') = N'' THEN N'(blank)' ELSE sm.sName END AS Salesman,
                    ISNULL(acct.sName, N'') AS CustomerName,
                    d.iCode AS CustId,
                    h.sVoucherNo AS ReceiptNo,
                    h.iDate AS ReceiptPacked,
                    CASE
                        WHEN COALESCE(NULLIF(hd4610.PaymentCode, 0), NULLIF(hd4609.PaymentCode, 0), NULLIF(hd4608.PaymentCode, 0), 0) = 1
                        THEN N'CI - 001 First Payment Bahrain'
                        WHEN COALESCE(NULLIF(hd4610.PaymentCode, 0), NULLIF(hd4609.PaymentCode, 0), NULLIF(hd4608.PaymentCode, 0), 0) = 4
                        THEN N'CI - 004 First Payment KSA'
                        ELSE N'CI - 001 + CI - 004'
                    END AS PayName,
                    CAST(
                        COALESCE(
                            NULLIF(hd4610.TotalContractAmt, 0),
                            NULLIF(TRY_CONVERT(decimal(18, 4), NULLIF(LTRIM(RTRIM(hd4609.ContractAmount)), N'')), 0),
                            0
                        )
                    AS decimal(18, 4)) AS TypedContract,
                    CAST(SUM(CASE WHEN d.mAmount1 > 0 THEN d.mAmount1 ELSE 0 END) AS decimal(18, 4)) AS PeriodColl
                FROM dbo.tCore_Header_0 h
                INNER JOIN dbo.tCore_Data_0 d
                    ON d.iHeaderId = h.iHeaderId
                   AND h.iVoucherType IN (4608, 4609, 4610)
                   AND ISNULL(h.iAuth, 1) = 1
                   AND ISNULL(h.bCancelled, 0) = 0
                   AND ISNULL(h.bVersion, 0) = 0
                   AND ISNULL(h.bSuspended, 0) = 0
                   AND h.iDate > 0
                   AND h.iDate BETWEEN @iStartDate AND @iEndDate
                   AND d.bUpdateFA = 1
                   AND d.iCode > 0
                   AND d.iFaTag = 2040
                   AND ISNULL(d.iType, 0) = 0
                   AND ISNULL(d.bVoid, 0) = 0
                   AND ISNULL(d.iAuthStatus, 0) < 2
                INNER JOIN dbo.mCore_Account acct
                    ON acct.iMasterId = d.iCode
                   AND acct.iAccountType IN (5, 7)
                INNER JOIN dbo.mCore_Department dep ON dep.iMasterId = d.iFaTag
                LEFT JOIN dbo.tCore_HeaderData4610_0 hd4610 ON hd4610.iHeaderId = h.iHeaderId
                LEFT JOIN dbo.tCore_HeaderData4609_0 hd4609 ON hd4609.iHeaderId = h.iHeaderId
                LEFT JOIN dbo.tCore_HeaderData4608_0 hd4608 ON hd4608.iHeaderId = h.iHeaderId
                LEFT JOIN (
                    SELECT vac0.iMasterId, MAX(vac0.Salesmanname) AS Salesmanname
                    FROM dbo.vaCore_Account vac0
                    GROUP BY vac0.iMasterId, vac0.iTreeId
                    HAVING vac0.iTreeId = 0
                ) vac ON vac.iMasterId = d.iCode
                LEFT JOIN dbo.mCore_Salesman sm ON sm.iMasterId = vac.Salesmanname
                WHERE COALESCE(NULLIF(hd4610.PaymentCode, 0), NULLIF(hd4609.PaymentCode, 0), NULLIF(hd4608.PaymentCode, 0), 0) IN (1, 4)
                GROUP BY
                    dep.sName,
                    (h.iDate & 0xfff0000) / 65536,
                    (h.iDate & 0xff00) / 256,
                    dbo.GetDateName(N'm', h.iDate)
                        + N'-'
                        + CAST((h.iDate & 0xfff0000) / 65536 AS varchar(4)),
                    d.iCode,
                    ISNULL(acct.sName, N''),
                    CASE WHEN ISNULL(sm.sName, N'') = N'' THEN N'(blank)' ELSE sm.sName END,
                    h.sVoucherNo,
                    h.iDate,
                    COALESCE(NULLIF(hd4610.PaymentCode, 0), NULLIF(hd4609.PaymentCode, 0), NULLIF(hd4608.PaymentCode, 0), 0),
                    CAST(
                        COALESCE(
                            NULLIF(hd4610.TotalContractAmt, 0),
                            NULLIF(TRY_CONVERT(decimal(18, 4), NULLIF(LTRIM(RTRIM(hd4609.ContractAmount)), N'')), 0),
                            0
                        )
                    AS decimal(18, 4))
            ) r
            LEFT JOIN (
                SELECT
                    d.iBookNo AS CustId,
                    CAST(ROUND(ABS(h.fNet), 0) AS decimal(18, 0)) AS NetRnd,
                    MAX(h.sVoucherNo) AS SONo,
                    MAX(h.iDate) AS SODate,
                    CAST(ABS(MAX(h.fNet)) AS decimal(18, 4)) AS ContractNet
                FROM dbo.tCore_Header_0 h
                INNER JOIN dbo.tCore_Data_0 d
                    ON d.iHeaderId = h.iHeaderId
                   AND d.iFaTag = 2040
                   AND d.iBookNo > 0
                WHERE h.iVoucherType = 5634
                  AND ISNULL(h.iAuth, 1) = 1
                  AND ISNULL(h.bCancelled, 0) = 0
                  AND ISNULL(h.bVersion, 0) = 0
                  AND ISNULL(h.bSuspended, 0) = 0
                  AND h.sVoucherNo LIKE N'CON-Atl-%'
                  AND h.iDate <= @iEndDate
                GROUP BY
                    d.iBookNo,
                    CAST(ROUND(ABS(h.fNet), 0) AS decimal(18, 0))
            ) so ON so.CustId = r.CustId
               AND r.TypedContract > 0
               AND so.NetRnd = CAST(ROUND(r.TypedContract, 0) AS decimal(18, 0))
            LEFT JOIN (
                SELECT
                    d.iBookNo AS CustId,
                    h.iDate AS SODate,
                    MAX(h.sVoucherNo) AS SONo,
                    CAST(ABS(MAX(h.fNet)) AS decimal(18, 4)) AS ContractNet
                FROM dbo.tCore_Header_0 h
                INNER JOIN dbo.tCore_Data_0 d
                    ON d.iHeaderId = h.iHeaderId
                   AND d.iFaTag = 2040
                   AND d.iBookNo > 0
                WHERE h.iVoucherType = 5634
                  AND ISNULL(h.iAuth, 1) = 1
                  AND ISNULL(h.bCancelled, 0) = 0
                  AND ISNULL(h.bVersion, 0) = 0
                  AND ISNULL(h.bSuspended, 0) = 0
                  AND h.sVoucherNo LIKE N'CON-Atl-%'
                  AND h.iDate <= @iEndDate
                GROUP BY
                    d.iBookNo,
                    h.iDate
                HAVING COUNT(DISTINCT h.sVoucherNo) = 1
            ) sod ON sod.CustId = r.CustId
               AND r.TypedContract > 0
               AND sod.SODate = r.ReceiptPacked
            LEFT JOIN (
                SELECT
                    d.iBookNo AS CustId,
                    MAX(h.sVoucherNo) AS SONo,
                    MAX(h.iDate) AS SODate,
                    CAST(ABS(MAX(h.fNet)) AS decimal(18, 4)) AS ContractNet
                FROM dbo.tCore_Header_0 h
                INNER JOIN dbo.tCore_Data_0 d
                    ON d.iHeaderId = h.iHeaderId
                   AND d.iFaTag = 2040
                   AND d.iBookNo > 0
                WHERE h.iVoucherType = 5634
                  AND ISNULL(h.iAuth, 1) = 1
                  AND ISNULL(h.bCancelled, 0) = 0
                  AND ISNULL(h.bVersion, 0) = 0
                  AND ISNULL(h.bSuspended, 0) = 0
                  AND h.sVoucherNo LIKE N'CON-Atl-%'
                  AND h.iDate <= @iEndDate
                GROUP BY d.iBookNo
                HAVING COUNT(DISTINCT h.sVoucherNo) = 1
            ) so1 ON so1.CustId = r.CustId
               AND r.TypedContract > 0
        ) p
        LEFT JOIN (
            SELECT
                z.CustId,
                z.SONo,
                z.ContractVal,
                CAST(SUM(z.PeriodColl) AS decimal(18, 4)) AS LifeColl
            FROM (
                SELECT
                    r.CustId,
                    r.PeriodColl,
                    ISNULL(COALESCE(NULLIF(so.SONo, N''), NULLIF(sod.SONo, N''), so1.SONo), N'') AS SONo,
                    CAST(COALESCE(so.ContractNet, sod.ContractNet, so1.ContractNet, r.TypedContract) AS decimal(18, 4)) AS ContractVal
                FROM (
                    SELECT
                        d.iCode AS CustId,
                        h.sVoucherNo AS ReceiptNo,
                        h.iDate AS ReceiptPacked,
                        CAST(
                            COALESCE(
                                NULLIF(hd4610.TotalContractAmt, 0),
                                NULLIF(TRY_CONVERT(decimal(18, 4), NULLIF(LTRIM(RTRIM(hd4609.ContractAmount)), N'')), 0),
                                0
                            )
                        AS decimal(18, 4)) AS TypedContract,
                        CAST(SUM(CASE WHEN d.mAmount1 > 0 THEN d.mAmount1 ELSE 0 END) AS decimal(18, 4)) AS PeriodColl
                    FROM dbo.tCore_Header_0 h
                    INNER JOIN dbo.tCore_Data_0 d
                        ON d.iHeaderId = h.iHeaderId
                       AND h.iVoucherType IN (4608, 4609, 4610)
                       AND ISNULL(h.iAuth, 1) = 1
                       AND ISNULL(h.bCancelled, 0) = 0
                       AND ISNULL(h.bVersion, 0) = 0
                       AND ISNULL(h.bSuspended, 0) = 0
                       AND h.iDate > 0
                       AND h.iDate <= @iEndDate
                       AND d.bUpdateFA = 1
                       AND d.iCode > 0
                       AND d.iFaTag = 2040
                       AND ISNULL(d.iType, 0) = 0
                       AND ISNULL(d.bVoid, 0) = 0
                       AND ISNULL(d.iAuthStatus, 0) < 2
                    INNER JOIN dbo.mCore_Account acct
                        ON acct.iMasterId = d.iCode
                       AND acct.iAccountType IN (5, 7)
                    LEFT JOIN dbo.tCore_HeaderData4610_0 hd4610 ON hd4610.iHeaderId = h.iHeaderId
                    LEFT JOIN dbo.tCore_HeaderData4609_0 hd4609 ON hd4609.iHeaderId = h.iHeaderId
                    LEFT JOIN dbo.tCore_HeaderData4608_0 hd4608 ON hd4608.iHeaderId = h.iHeaderId
                    WHERE COALESCE(NULLIF(hd4610.PaymentCode, 0), NULLIF(hd4609.PaymentCode, 0), NULLIF(hd4608.PaymentCode, 0), 0) IN (1, 4)
                    GROUP BY
                        d.iCode,
                        h.sVoucherNo,
                        h.iDate,
                        CAST(
                            COALESCE(
                                NULLIF(hd4610.TotalContractAmt, 0),
                                NULLIF(TRY_CONVERT(decimal(18, 4), NULLIF(LTRIM(RTRIM(hd4609.ContractAmount)), N'')), 0),
                                0
                            )
                        AS decimal(18, 4))
                ) r
                LEFT JOIN (
                    SELECT
                        d.iBookNo AS CustId,
                        CAST(ROUND(ABS(h.fNet), 0) AS decimal(18, 0)) AS NetRnd,
                        MAX(h.sVoucherNo) AS SONo,
                        MAX(h.iDate) AS SODate,
                        CAST(ABS(MAX(h.fNet)) AS decimal(18, 4)) AS ContractNet
                    FROM dbo.tCore_Header_0 h
                    INNER JOIN dbo.tCore_Data_0 d
                        ON d.iHeaderId = h.iHeaderId
                       AND d.iFaTag = 2040
                       AND d.iBookNo > 0
                    WHERE h.iVoucherType = 5634
                      AND ISNULL(h.iAuth, 1) = 1
                      AND ISNULL(h.bCancelled, 0) = 0
                      AND ISNULL(h.bVersion, 0) = 0
                      AND ISNULL(h.bSuspended, 0) = 0
                      AND h.sVoucherNo LIKE N'CON-Atl-%'
                      AND h.iDate <= @iEndDate
                    GROUP BY
                        d.iBookNo,
                        CAST(ROUND(ABS(h.fNet), 0) AS decimal(18, 0))
                ) so ON so.CustId = r.CustId
                   AND r.TypedContract > 0
                   AND so.NetRnd = CAST(ROUND(r.TypedContract, 0) AS decimal(18, 0))
                LEFT JOIN (
                    SELECT
                        d.iBookNo AS CustId,
                        h.iDate AS SODate,
                        MAX(h.sVoucherNo) AS SONo,
                        CAST(ABS(MAX(h.fNet)) AS decimal(18, 4)) AS ContractNet
                    FROM dbo.tCore_Header_0 h
                    INNER JOIN dbo.tCore_Data_0 d
                        ON d.iHeaderId = h.iHeaderId
                       AND d.iFaTag = 2040
                       AND d.iBookNo > 0
                    WHERE h.iVoucherType = 5634
                      AND ISNULL(h.iAuth, 1) = 1
                      AND ISNULL(h.bCancelled, 0) = 0
                      AND ISNULL(h.bVersion, 0) = 0
                      AND ISNULL(h.bSuspended, 0) = 0
                      AND h.sVoucherNo LIKE N'CON-Atl-%'
                      AND h.iDate <= @iEndDate
                    GROUP BY
                        d.iBookNo,
                        h.iDate
                    HAVING COUNT(DISTINCT h.sVoucherNo) = 1
                ) sod ON sod.CustId = r.CustId
                   AND r.TypedContract > 0
                   AND sod.SODate = r.ReceiptPacked
                LEFT JOIN (
                    SELECT
                        d.iBookNo AS CustId,
                        MAX(h.sVoucherNo) AS SONo,
                        MAX(h.iDate) AS SODate,
                        CAST(ABS(MAX(h.fNet)) AS decimal(18, 4)) AS ContractNet
                    FROM dbo.tCore_Header_0 h
                    INNER JOIN dbo.tCore_Data_0 d
                        ON d.iHeaderId = h.iHeaderId
                       AND d.iFaTag = 2040
                       AND d.iBookNo > 0
                    WHERE h.iVoucherType = 5634
                      AND ISNULL(h.iAuth, 1) = 1
                      AND ISNULL(h.bCancelled, 0) = 0
                      AND ISNULL(h.bVersion, 0) = 0
                      AND ISNULL(h.bSuspended, 0) = 0
                      AND h.sVoucherNo LIKE N'CON-Atl-%'
                      AND h.iDate <= @iEndDate
                    GROUP BY d.iBookNo
                    HAVING COUNT(DISTINCT h.sVoucherNo) = 1
                ) so1 ON so1.CustId = r.CustId
                   AND r.TypedContract > 0
            ) z
            GROUP BY z.CustId, z.SONo, z.ContractVal
        ) c ON c.CustId = p.CustId
           AND c.SONo = p.SONo
           AND c.ContractVal = p.ContractVal
        INNER JOIN (
            SELECT
                p2.CustId,
                p2.SONo,
                p2.ContractVal,
                CAST(COUNT(DISTINCT p2.Salesman) AS decimal(18, 4)) AS SplitN
            FROM (
                SELECT
                    r.CustId,
                    r.Salesman,
                    ISNULL(COALESCE(NULLIF(so.SONo, N''), NULLIF(sod.SONo, N''), so1.SONo), N'') AS SONo,
                    CAST(COALESCE(so.ContractNet, sod.ContractNet, so1.ContractNet, r.TypedContract) AS decimal(18, 4)) AS ContractVal
                FROM (
                    SELECT
                        CASE WHEN ISNULL(sm.sName, N'') = N'' THEN N'(blank)' ELSE sm.sName END AS Salesman,
                        d.iCode AS CustId,
                        h.sVoucherNo AS ReceiptNo,
                        h.iDate AS ReceiptPacked,
                        CAST(
                            COALESCE(
                                NULLIF(hd4610.TotalContractAmt, 0),
                                NULLIF(TRY_CONVERT(decimal(18, 4), NULLIF(LTRIM(RTRIM(hd4609.ContractAmount)), N'')), 0),
                                0
                            )
                        AS decimal(18, 4)) AS TypedContract
                    FROM dbo.tCore_Header_0 h
                    INNER JOIN dbo.tCore_Data_0 d
                        ON d.iHeaderId = h.iHeaderId
                       AND h.iVoucherType IN (4608, 4609, 4610)
                       AND ISNULL(h.iAuth, 1) = 1
                       AND ISNULL(h.bCancelled, 0) = 0
                       AND ISNULL(h.bVersion, 0) = 0
                       AND ISNULL(h.bSuspended, 0) = 0
                       AND h.iDate > 0
                       AND h.iDate BETWEEN @iStartDate AND @iEndDate
                       AND d.bUpdateFA = 1
                       AND d.iCode > 0
                       AND d.iFaTag = 2040
                       AND ISNULL(d.iType, 0) = 0
                       AND ISNULL(d.bVoid, 0) = 0
                       AND ISNULL(d.iAuthStatus, 0) < 2
                    INNER JOIN dbo.mCore_Account acct
                        ON acct.iMasterId = d.iCode
                       AND acct.iAccountType IN (5, 7)
                    LEFT JOIN dbo.tCore_HeaderData4610_0 hd4610 ON hd4610.iHeaderId = h.iHeaderId
                    LEFT JOIN dbo.tCore_HeaderData4609_0 hd4609 ON hd4609.iHeaderId = h.iHeaderId
                    LEFT JOIN dbo.tCore_HeaderData4608_0 hd4608 ON hd4608.iHeaderId = h.iHeaderId
                    LEFT JOIN (
                        SELECT vac0.iMasterId, MAX(vac0.Salesmanname) AS Salesmanname
                        FROM dbo.vaCore_Account vac0
                        GROUP BY vac0.iMasterId, vac0.iTreeId
                        HAVING vac0.iTreeId = 0
                    ) vac ON vac.iMasterId = d.iCode
                    LEFT JOIN dbo.mCore_Salesman sm ON sm.iMasterId = vac.Salesmanname
                    WHERE COALESCE(NULLIF(hd4610.PaymentCode, 0), NULLIF(hd4609.PaymentCode, 0), NULLIF(hd4608.PaymentCode, 0), 0) IN (1, 4)
                    GROUP BY
                        d.iCode,
                        CASE WHEN ISNULL(sm.sName, N'') = N'' THEN N'(blank)' ELSE sm.sName END,
                        h.sVoucherNo,
                        h.iDate,
                        CAST(
                            COALESCE(
                                NULLIF(hd4610.TotalContractAmt, 0),
                                NULLIF(TRY_CONVERT(decimal(18, 4), NULLIF(LTRIM(RTRIM(hd4609.ContractAmount)), N'')), 0),
                                0
                            )
                        AS decimal(18, 4))
                ) r
                LEFT JOIN (
                    SELECT
                        d.iBookNo AS CustId,
                        CAST(ROUND(ABS(h.fNet), 0) AS decimal(18, 0)) AS NetRnd,
                        MAX(h.sVoucherNo) AS SONo,
                        MAX(h.iDate) AS SODate,
                        CAST(ABS(MAX(h.fNet)) AS decimal(18, 4)) AS ContractNet
                    FROM dbo.tCore_Header_0 h
                    INNER JOIN dbo.tCore_Data_0 d
                        ON d.iHeaderId = h.iHeaderId
                       AND d.iFaTag = 2040
                       AND d.iBookNo > 0
                    WHERE h.iVoucherType = 5634
                      AND ISNULL(h.iAuth, 1) = 1
                      AND ISNULL(h.bCancelled, 0) = 0
                      AND ISNULL(h.bVersion, 0) = 0
                      AND ISNULL(h.bSuspended, 0) = 0
                      AND h.sVoucherNo LIKE N'CON-Atl-%'
                      AND h.iDate <= @iEndDate
                    GROUP BY
                        d.iBookNo,
                        CAST(ROUND(ABS(h.fNet), 0) AS decimal(18, 0))
                ) so ON so.CustId = r.CustId
                   AND r.TypedContract > 0
                   AND so.NetRnd = CAST(ROUND(r.TypedContract, 0) AS decimal(18, 0))
                LEFT JOIN (
                    SELECT
                        d.iBookNo AS CustId,
                        h.iDate AS SODate,
                        MAX(h.sVoucherNo) AS SONo,
                        CAST(ABS(MAX(h.fNet)) AS decimal(18, 4)) AS ContractNet
                    FROM dbo.tCore_Header_0 h
                    INNER JOIN dbo.tCore_Data_0 d
                        ON d.iHeaderId = h.iHeaderId
                       AND d.iFaTag = 2040
                       AND d.iBookNo > 0
                    WHERE h.iVoucherType = 5634
                      AND ISNULL(h.iAuth, 1) = 1
                      AND ISNULL(h.bCancelled, 0) = 0
                      AND ISNULL(h.bVersion, 0) = 0
                      AND ISNULL(h.bSuspended, 0) = 0
                      AND h.sVoucherNo LIKE N'CON-Atl-%'
                      AND h.iDate <= @iEndDate
                    GROUP BY
                        d.iBookNo,
                        h.iDate
                    HAVING COUNT(DISTINCT h.sVoucherNo) = 1
                ) sod ON sod.CustId = r.CustId
                   AND r.TypedContract > 0
                   AND sod.SODate = r.ReceiptPacked
                LEFT JOIN (
                    SELECT
                        d.iBookNo AS CustId,
                        MAX(h.sVoucherNo) AS SONo,
                        MAX(h.iDate) AS SODate,
                        CAST(ABS(MAX(h.fNet)) AS decimal(18, 4)) AS ContractNet
                    FROM dbo.tCore_Header_0 h
                    INNER JOIN dbo.tCore_Data_0 d
                        ON d.iHeaderId = h.iHeaderId
                       AND d.iFaTag = 2040
                       AND d.iBookNo > 0
                    WHERE h.iVoucherType = 5634
                      AND ISNULL(h.iAuth, 1) = 1
                      AND ISNULL(h.bCancelled, 0) = 0
                      AND ISNULL(h.bVersion, 0) = 0
                      AND ISNULL(h.bSuspended, 0) = 0
                      AND h.sVoucherNo LIKE N'CON-Atl-%'
                      AND h.iDate <= @iEndDate
                    GROUP BY d.iBookNo
                    HAVING COUNT(DISTINCT h.sVoucherNo) = 1
                ) so1 ON so1.CustId = r.CustId
                   AND r.TypedContract > 0
            ) p2
            GROUP BY p2.CustId, p2.SONo, p2.ContractVal
        ) n ON n.CustId = p.CustId
           AND n.SONo = p.SONo
           AND n.ContractVal = p.ContractVal
        GROUP BY
            p.Department,
            p.MonthYear,
            p.Salesman,
            p.CustomerName,
            p.CustId,
            p.SONo,
            p.SODate,
            p.ReceiptNo,
            p.ReceiptPacked,
            p.PayName,
            p.ContractVal,
            p.PeriodColl,
            c.LifeColl,
            n.SplitN
    ) g
    WHERE ISNULL(g.SONo, N'') <> N''
      AND g.ContractVal > 0
      AND g.LifeColl * (100.0 / 45.0) + 1 >= g.ContractVal
    ) w
) x
WHERE x.iDate > 0
