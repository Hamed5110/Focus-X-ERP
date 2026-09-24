/* ============================================================================
   Monthly Salesman Commission — Atlas Aluminum (iFaTag 2040)

   Gate + team cliff (NetSuite / SalesCookie style):
     Payment codes CI-001 / CI-004 only (mandatory).
     A contract is eligible only if collection >= 45% of that contract.
     Eligible contracts ADD their contract amount to Overall Sales.
     Below 45% is Not Eligible — excluded from Overall Sales and from payout.
     Collection Eligible Amount = period CI-001/004 receipts on those same
     contracts (cash collected after the gate; not the payout base).
     Rate R is the Atlas cliff on Overall Sales (team, not per salesman).
     C_s = salesman Eligible Amount × R.

   Period: h.iDate BETWEEN @iStartDate AND @iEndDate
   Do not DECLARE. Do not convert result iDate.
   ============================================================================ */
SELECT
    x.Department,
    x.MonthYear AS [Month Year],
    x.Salesman,
    CAST(x.ContractAmt AS decimal(18, 2)) AS [Total Contract Amount],
    CAST(x.CollectionAmt AS decimal(18, 2)) AS [Total Collection],
    CAST(x.CollEligibleAmt AS decimal(18, 2)) AS [Collection Eligible Amount],
    CAST(x.EligibleAmt AS decimal(18, 2)) AS [Eligible Amount],
    CAST(x.NotEligibleAmt AS decimal(18, 2)) AS [Not Eligible Amount],
    CAST(CASE WHEN x.Rn = 1 THEN x.OverallSales ELSE 0 END AS decimal(18, 2)) AS [Overall Sales],
    CAST(CASE WHEN x.Rn = 1 THEN x.RatePct ELSE 0 END AS decimal(18, 2)) AS [Commission Ratio],
    CAST(x.CommissionAmt AS decimal(18, 2)) AS [Total Commission Amount],
    CAST(x.iDate AS decimal(18, 0)) AS iDate
FROM (
    SELECT
        g.Department,
        g.MonthYear,
        g.Salesman,
        g.ContractAmt,
        g.CollectionAmt,
        g.CollEligibleAmt,
        g.EligibleAmt,
        g.NotEligibleAmt,
        t.OverallSales,
        t.RatePct,
        ROW_NUMBER() OVER (
            PARTITION BY g.Department, g.MonthYear
            ORDER BY g.Salesman
        ) AS Rn,
        CAST(g.EligibleAmt *
            CASE
                WHEN t.OverallSales >= 300000 THEN 0.017
                WHEN t.OverallSales >= 250000 THEN 0.015
                WHEN t.OverallSales >= 200000 THEN 0.012
                WHEN t.OverallSales >= 150000 THEN 0.010
                ELSE 0
            END
        AS decimal(18, 4)) AS CommissionAmt,
        g.iDate
    FROM (
        SELECT
            p.Department,
            p.MonthYear,
            p.Salesman,
            CAST(SUM(p.ContractVal / n.SplitN) AS decimal(18, 4)) AS ContractAmt,
            CAST(SUM(p.PeriodColl / n.SplitN) AS decimal(18, 4)) AS CollectionAmt,
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
            CAST(SUM(
                CASE
                    WHEN p.ContractVal > 0
                         AND ISNULL(c.LifeColl, 0) * (100.0 / 45.0) + 1 >= p.ContractVal
                    THEN 0
                    ELSE p.ContractVal
                END / n.SplitN
            ) AS decimal(18, 4)) AS NotEligibleAmt,
            MIN(p.Packed) AS iDate
        FROM (
            SELECT
                dep.sName AS Department,
                dbo.GetDateName(N'm', h.iDate)
                    + N'-'
                    + CAST((h.iDate & 0xfff0000) / 65536 AS varchar(4)) AS MonthYear,
                CASE WHEN ISNULL(sm.sName, N'') = N'' THEN N'(blank)' ELSE sm.sName END AS Salesman,
                d.iCode AS CustId,
                CAST(
                    COALESCE(
                        NULLIF(hd4610.TotalContractAmt, 0),
                        NULLIF(TRY_CONVERT(decimal(18, 4), NULLIF(LTRIM(RTRIM(hd4609.ContractAmount)), N'')), 0),
                        0
                    )
                AS decimal(18, 4)) AS ContractVal,
                CAST(SUM(CASE WHEN d.mAmount1 > 0 THEN d.mAmount1 ELSE 0 END) AS decimal(18, 4)) AS PeriodColl,
                MIN(h.iDate) AS Packed
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
                CASE WHEN ISNULL(sm.sName, N'') = N'' THEN N'(blank)' ELSE sm.sName END,
                CAST(
                    COALESCE(
                        NULLIF(hd4610.TotalContractAmt, 0),
                        NULLIF(TRY_CONVERT(decimal(18, 4), NULLIF(LTRIM(RTRIM(hd4609.ContractAmount)), N'')), 0),
                        0
                    )
                AS decimal(18, 4))
        ) p
        LEFT JOIN (
            SELECT
                d.iCode AS CustId,
                CAST(
                    COALESCE(
                        NULLIF(hd4610.TotalContractAmt, 0),
                        NULLIF(TRY_CONVERT(decimal(18, 4), NULLIF(LTRIM(RTRIM(hd4609.ContractAmount)), N'')), 0),
                        0
                    )
                AS decimal(18, 4)) AS ContractVal,
                CAST(SUM(CASE WHEN d.mAmount1 > 0 THEN d.mAmount1 ELSE 0 END) AS decimal(18, 4)) AS LifeColl
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
                CAST(
                    COALESCE(
                        NULLIF(hd4610.TotalContractAmt, 0),
                        NULLIF(TRY_CONVERT(decimal(18, 4), NULLIF(LTRIM(RTRIM(hd4609.ContractAmount)), N'')), 0),
                        0
                    )
                AS decimal(18, 4))
        ) c ON c.CustId = p.CustId
           AND c.ContractVal = p.ContractVal
           AND p.ContractVal > 0
        INNER JOIN (
            SELECT
                p2.CustId,
                p2.ContractVal,
                CAST(COUNT(DISTINCT p2.Salesman) AS decimal(18, 4)) AS SplitN
            FROM (
                SELECT
                    CASE WHEN ISNULL(sm.sName, N'') = N'' THEN N'(blank)' ELSE sm.sName END AS Salesman,
                    d.iCode AS CustId,
                    CAST(
                        COALESCE(
                            NULLIF(hd4610.TotalContractAmt, 0),
                            NULLIF(TRY_CONVERT(decimal(18, 4), NULLIF(LTRIM(RTRIM(hd4609.ContractAmount)), N'')), 0),
                            0
                        )
                    AS decimal(18, 4)) AS ContractVal
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
            ) p2
            GROUP BY p2.CustId, p2.ContractVal
        ) n ON n.CustId = p.CustId
           AND n.ContractVal = p.ContractVal
        GROUP BY
            p.Department,
            p.MonthYear,
            p.Salesman
    ) g
    INNER JOIN (
        SELECT
            s.Department,
            s.MonthYear,
            CAST(SUM(s.EligibleAmt) AS decimal(18, 4)) AS OverallSales,
            CASE
                WHEN SUM(s.EligibleAmt) >= 300000 THEN 1.70
                WHEN SUM(s.EligibleAmt) >= 250000 THEN 1.50
                WHEN SUM(s.EligibleAmt) >= 200000 THEN 1.20
                WHEN SUM(s.EligibleAmt) >= 150000 THEN 1.00
                ELSE 0
            END AS RatePct
        FROM (
            SELECT
                p.Department,
                p.MonthYear,
                CAST(SUM(
                    CASE
                        WHEN p.ContractVal > 0
                             AND ISNULL(c.LifeColl, 0) * (100.0 / 45.0) + 1 >= p.ContractVal
                        THEN p.ContractVal
                        ELSE 0
                    END / n.SplitN
                ) AS decimal(18, 4)) AS EligibleAmt
            FROM (
                SELECT
                    dep.sName AS Department,
                    dbo.GetDateName(N'm', h.iDate)
                        + N'-'
                        + CAST((h.iDate & 0xfff0000) / 65536 AS varchar(4)) AS MonthYear,
                    CASE WHEN ISNULL(sm.sName, N'') = N'' THEN N'(blank)' ELSE sm.sName END AS Salesman,
                    d.iCode AS CustId,
                    CAST(
                        COALESCE(
                            NULLIF(hd4610.TotalContractAmt, 0),
                            NULLIF(TRY_CONVERT(decimal(18, 4), NULLIF(LTRIM(RTRIM(hd4609.ContractAmount)), N'')), 0),
                            0
                        )
                    AS decimal(18, 4)) AS ContractVal
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
                    CASE WHEN ISNULL(sm.sName, N'') = N'' THEN N'(blank)' ELSE sm.sName END,
                    CAST(
                        COALESCE(
                            NULLIF(hd4610.TotalContractAmt, 0),
                            NULLIF(TRY_CONVERT(decimal(18, 4), NULLIF(LTRIM(RTRIM(hd4609.ContractAmount)), N'')), 0),
                            0
                        )
                    AS decimal(18, 4))
            ) p
            LEFT JOIN (
                SELECT
                    d.iCode AS CustId,
                    CAST(
                        COALESCE(
                            NULLIF(hd4610.TotalContractAmt, 0),
                            NULLIF(TRY_CONVERT(decimal(18, 4), NULLIF(LTRIM(RTRIM(hd4609.ContractAmount)), N'')), 0),
                            0
                        )
                    AS decimal(18, 4)) AS ContractVal,
                    CAST(SUM(CASE WHEN d.mAmount1 > 0 THEN d.mAmount1 ELSE 0 END) AS decimal(18, 4)) AS LifeColl
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
                    CAST(
                        COALESCE(
                            NULLIF(hd4610.TotalContractAmt, 0),
                            NULLIF(TRY_CONVERT(decimal(18, 4), NULLIF(LTRIM(RTRIM(hd4609.ContractAmount)), N'')), 0),
                            0
                        )
                    AS decimal(18, 4))
            ) c ON c.CustId = p.CustId
               AND c.ContractVal = p.ContractVal
               AND p.ContractVal > 0
            INNER JOIN (
                SELECT
                    p2.CustId,
                    p2.ContractVal,
                    CAST(COUNT(DISTINCT p2.Salesman) AS decimal(18, 4)) AS SplitN
                FROM (
                    SELECT
                        CASE WHEN ISNULL(sm.sName, N'') = N'' THEN N'(blank)' ELSE sm.sName END AS Salesman,
                        d.iCode AS CustId,
                        CAST(
                            COALESCE(
                                NULLIF(hd4610.TotalContractAmt, 0),
                                NULLIF(TRY_CONVERT(decimal(18, 4), NULLIF(LTRIM(RTRIM(hd4609.ContractAmount)), N'')), 0),
                                0
                            )
                        AS decimal(18, 4)) AS ContractVal
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
                ) p2
                GROUP BY p2.CustId, p2.ContractVal
            ) n ON n.CustId = p.CustId
               AND n.ContractVal = p.ContractVal
            GROUP BY
                p.Department,
                p.MonthYear,
                p.Salesman
        ) s
        GROUP BY s.Department, s.MonthYear
    ) t ON t.Department = g.Department
       AND t.MonthYear = g.MonthYear
) x
WHERE x.iDate > 0
