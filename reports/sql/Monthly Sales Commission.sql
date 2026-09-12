/* ============================================================================
   Monthly Salesman Commission  (Focus Query, Focus80E0)

   Collection = receipt lines on the customer (account type 5/7), not VAT.
   Combined report: Atlas Aluminum 2040 and Aknan Showroom 2057.
   Spare reports (one department each):
     reports/sql/Monthly Sales Commission - Atlas Aluminum.sql
     reports/sql/Monthly Sales Commission - Aknan Showroom.sql
   Includes Customer Name (account sName).

   PaymentCode on 4608/4609/4610:
     1 CI-001 First Payment Bahrain
     2 CI-002 Second Payment Bahrain   -> auto eligible
     4 CI-004 First Payment KSA
     5 CI-005 Second Payment KSA       -> auto eligible
   First payment (1,4): eligible when ALL receipts on that contract
   (customer + contract value) through @iEndDate are >= 50% of contract
   (1 BHD tolerance). Second payment (2,5): always eligible.
   Rate is on the department's eligible collection for that month.
   Aknan >= 80000 is 1.5% (covers 80k-100k and above 100k).

   iDate stays packed decimal (layout Fraction). Hide it.
   Period: h.iDate BETWEEN @iStartDate AND @iEndDate
   Do not DECLARE. Do not convert result iDate.
   ============================================================================ */
SELECT
    x.Department,
    x.MonthYear AS [Month Year],
    x.Salesman,
    x.CustomerName AS [Customer Name],
    x.PayName AS [Payment Code],
    CAST(x.CollectionAmt AS decimal(18, 2)) AS [Collection Amount],
    CAST(x.ContractVal AS decimal(18, 2)) AS [Contract Value],
    CAST(x.EligibleAmt AS decimal(18, 2)) AS [Eligible Collection],
    CAST(ISNULL(x.TeamEligible, 0) AS decimal(18, 2)) AS [Team Eligible Collection],
    CAST(ISNULL(x.TeamRatePct, 0) AS decimal(18, 2)) AS [Team Rate %],
    CAST(ISNULL(x.TeamCommission, 0) AS decimal(18, 2)) AS [Team Commission],
    CAST(ISNULL(x.TeamMembers, 0) AS decimal(18, 2)) AS [Team Members],
    CAST(
        CASE
            WHEN ISNULL(x.TeamMembers, 0) = 0 THEN 0
            ELSE ROUND(x.TeamCommission / x.TeamMembers, 2)
        END
    AS decimal(18, 2)) AS [Share Each],
    CAST(x.iDate AS decimal(18, 0)) AS iDate
FROM (
    SELECT
        q.Department,
        q.MonthYear,
        q.Salesman,
        q.CustomerName,
        q.PayName,
        q.CollectionAmt,
        q.ContractVal,
        q.EligibleAmt,
        t.TeamEligible,
        t.TeamRatePct,
        t.TeamCommission,
        t.TeamMembers,
        CAST(q.Packed AS decimal(18, 0)) AS iDate
    FROM (
        SELECT
            f.Department,
            f.MonthYear,
            f.Salesman,
            f.CustomerName,
            f.PayName,
            f.CollectionAmt,
            f.ContractVal,
            CAST(
                CASE
                    WHEN f.Pay IN (2, 5) AND f.CollectionAmt > 0
                    THEN f.CollectionAmt
                    WHEN f.Pay IN (1, 4)
                         AND f.ContractVal > 0
                         AND ISNULL(c.LifeColl, 0) * 2 + 1 >= f.ContractVal
                    THEN f.CollectionAmt
                    ELSE 0
                END
            AS decimal(18, 4)) AS EligibleAmt,
            f.Packed,
            f.Y,
            f.M
        FROM (
            SELECT
                dep.sName AS Department,
                dbo.GetDateName(N'm', h.iDate)
                    + N'-'
                    + CAST((h.iDate & 0xfff0000) / 65536 AS varchar(4)) AS MonthYear,
                CASE WHEN ISNULL(sm.sName, N'') = N'' THEN N'(blank)' ELSE sm.sName END AS Salesman,
                ISNULL(acct.sName, N'') AS CustomerName,
                CASE COALESCE(NULLIF(hd4610.PaymentCode, 0), NULLIF(hd4609.PaymentCode, 0), NULLIF(hd4608.PaymentCode, 0), 0)
                    WHEN 1 THEN N'CI - 001 First Payment Bahrain'
                    WHEN 2 THEN N'CI - 002 Second Payment Bahrain'
                    WHEN 3 THEN N'CI - 003 Additional Contract Payment Bahrain'
                    WHEN 4 THEN N'CI - 004 First Payment KSA'
                    WHEN 5 THEN N'CI - 005 Second Payment KSA'
                    WHEN 6 THEN N'CI - 006 Additional Contract Payment KSA'
                    WHEN 7 THEN N'CI - 007 Scrap Payment'
                    WHEN 8 THEN N'CI - 008 Maintenances Payment'
                    ELSE N''
                END AS PayName,
                COALESCE(NULLIF(hd4610.PaymentCode, 0), NULLIF(hd4609.PaymentCode, 0), NULLIF(hd4608.PaymentCode, 0), 0) AS Pay,
                d.iCode AS CustId,
                CAST(SUM(CASE WHEN d.mAmount1 > 0 THEN d.mAmount1 ELSE 0 END) AS decimal(18, 4)) AS CollectionAmt,
                CAST(
                    COALESCE(
                        NULLIF(hd4610.TotalContractAmt, 0),
                        NULLIF(TRY_CONVERT(decimal(18, 4), NULLIF(LTRIM(RTRIM(hd4609.ContractAmount)), N'')), 0),
                        0
                    )
                AS decimal(18, 4)) AS ContractVal,
                h.iDate AS Packed,
                (h.iDate & 0xfff0000) / 65536 AS Y,
                (h.iDate & 0xff00) / 256 AS M
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
               AND d.iFaTag IN (2040, 2057)
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
            GROUP BY
                dep.sName,
                h.iDate,
                h.iHeaderId,
                d.iCode,
                hd4610.PaymentCode,
                hd4609.PaymentCode,
                hd4608.PaymentCode,
                hd4610.TotalContractAmt,
                hd4609.ContractAmount,
                CASE WHEN ISNULL(sm.sName, N'') = N'' THEN N'(blank)' ELSE sm.sName END,
                ISNULL(acct.sName, N'')
        ) f
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
               AND d.iFaTag IN (2040, 2057)
               AND ISNULL(d.iType, 0) = 0
               AND ISNULL(d.bVoid, 0) = 0
               AND ISNULL(d.iAuthStatus, 0) < 2
            INNER JOIN dbo.mCore_Account acct
                ON acct.iMasterId = d.iCode
               AND acct.iAccountType IN (5, 7)
            LEFT JOIN dbo.tCore_HeaderData4610_0 hd4610 ON hd4610.iHeaderId = h.iHeaderId
            LEFT JOIN dbo.tCore_HeaderData4609_0 hd4609 ON hd4609.iHeaderId = h.iHeaderId
            GROUP BY
                d.iCode,
                CAST(
                    COALESCE(
                        NULLIF(hd4610.TotalContractAmt, 0),
                        NULLIF(TRY_CONVERT(decimal(18, 4), NULLIF(LTRIM(RTRIM(hd4609.ContractAmount)), N'')), 0),
                        0
                    )
                AS decimal(18, 4))
        ) c ON c.CustId = f.CustId
           AND c.ContractVal = f.ContractVal
           AND f.ContractVal > 0
    ) q
    LEFT JOIN (
        SELECT
            s2.Department,
            s2.Y,
            s2.M,
            CAST(SUM(s2.EligibleAmt) AS decimal(18, 4)) AS TeamEligible,
            COUNT(DISTINCT CASE
                WHEN s2.EligibleAmt > 0 AND s2.Salesman <> N'(blank)' THEN s2.Salesman
            END) AS TeamMembers,
            CASE
                WHEN s2.Department = N'Atlas Aluminum' AND SUM(s2.EligibleAmt) >= 300000 THEN 1.70
                WHEN s2.Department = N'Atlas Aluminum' AND SUM(s2.EligibleAmt) >= 250000 THEN 1.50
                WHEN s2.Department = N'Atlas Aluminum' AND SUM(s2.EligibleAmt) >= 200000 THEN 1.20
                WHEN s2.Department = N'Atlas Aluminum' AND SUM(s2.EligibleAmt) >= 150000 THEN 1.00
                WHEN s2.Department = N'Aknan Showroom' AND SUM(s2.EligibleAmt) >= 80000 THEN 1.50
                WHEN s2.Department = N'Aknan Showroom' AND SUM(s2.EligibleAmt) >= 70000 THEN 1.20
                WHEN s2.Department = N'Aknan Showroom' AND SUM(s2.EligibleAmt) >= 50000 THEN 1.00
                ELSE 0
            END AS TeamRatePct,
            CAST(SUM(s2.EligibleAmt) *
                CASE
                    WHEN s2.Department = N'Atlas Aluminum' AND SUM(s2.EligibleAmt) >= 300000 THEN 0.017
                    WHEN s2.Department = N'Atlas Aluminum' AND SUM(s2.EligibleAmt) >= 250000 THEN 0.015
                    WHEN s2.Department = N'Atlas Aluminum' AND SUM(s2.EligibleAmt) >= 200000 THEN 0.012
                    WHEN s2.Department = N'Atlas Aluminum' AND SUM(s2.EligibleAmt) >= 150000 THEN 0.010
                    WHEN s2.Department = N'Aknan Showroom' AND SUM(s2.EligibleAmt) >= 80000 THEN 0.015
                    WHEN s2.Department = N'Aknan Showroom' AND SUM(s2.EligibleAmt) >= 70000 THEN 0.012
                    WHEN s2.Department = N'Aknan Showroom' AND SUM(s2.EligibleAmt) >= 50000 THEN 0.010
                    ELSE 0
                END
            AS decimal(18, 4)) AS TeamCommission
        FROM (
            SELECT
                f.Department,
                f.Y,
                f.M,
                f.Salesman,
                CAST(
                    CASE
                        WHEN f.Pay IN (2, 5) AND f.CollectionAmt > 0
                        THEN f.CollectionAmt
                        WHEN f.Pay IN (1, 4)
                             AND f.ContractVal > 0
                             AND ISNULL(c.LifeColl, 0) * 2 + 1 >= f.ContractVal
                        THEN f.CollectionAmt
                        ELSE 0
                    END
                AS decimal(18, 4)) AS EligibleAmt
            FROM (
                SELECT
                    dep.sName AS Department,
                    CASE WHEN ISNULL(sm.sName, N'') = N'' THEN N'(blank)' ELSE sm.sName END AS Salesman,
                    COALESCE(NULLIF(hd4610.PaymentCode, 0), NULLIF(hd4609.PaymentCode, 0), NULLIF(hd4608.PaymentCode, 0), 0) AS Pay,
                    d.iCode AS CustId,
                    CAST(SUM(CASE WHEN d.mAmount1 > 0 THEN d.mAmount1 ELSE 0 END) AS decimal(18, 4)) AS CollectionAmt,
                    CAST(
                        COALESCE(
                            NULLIF(hd4610.TotalContractAmt, 0),
                            NULLIF(TRY_CONVERT(decimal(18, 4), NULLIF(LTRIM(RTRIM(hd4609.ContractAmount)), N'')), 0),
                            0
                        )
                    AS decimal(18, 4)) AS ContractVal,
                    (h.iDate & 0xfff0000) / 65536 AS Y,
                    (h.iDate & 0xff00) / 256 AS M
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
                   AND d.iFaTag IN (2040, 2057)
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
                GROUP BY
                    dep.sName,
                    h.iDate,
                    h.iHeaderId,
                    d.iCode,
                    hd4610.PaymentCode,
                    hd4609.PaymentCode,
                    hd4608.PaymentCode,
                    hd4610.TotalContractAmt,
                    hd4609.ContractAmount,
                    CASE WHEN ISNULL(sm.sName, N'') = N'' THEN N'(blank)' ELSE sm.sName END
            ) f
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
                   AND d.iFaTag IN (2040, 2057)
                   AND ISNULL(d.iType, 0) = 0
                   AND ISNULL(d.bVoid, 0) = 0
                   AND ISNULL(d.iAuthStatus, 0) < 2
                INNER JOIN dbo.mCore_Account acct
                    ON acct.iMasterId = d.iCode
                   AND acct.iAccountType IN (5, 7)
                LEFT JOIN dbo.tCore_HeaderData4610_0 hd4610 ON hd4610.iHeaderId = h.iHeaderId
                LEFT JOIN dbo.tCore_HeaderData4609_0 hd4609 ON hd4609.iHeaderId = h.iHeaderId
                GROUP BY
                    d.iCode,
                    CAST(
                        COALESCE(
                            NULLIF(hd4610.TotalContractAmt, 0),
                            NULLIF(TRY_CONVERT(decimal(18, 4), NULLIF(LTRIM(RTRIM(hd4609.ContractAmount)), N'')), 0),
                            0
                        )
                    AS decimal(18, 4))
            ) c ON c.CustId = f.CustId
               AND c.ContractVal = f.ContractVal
               AND f.ContractVal > 0
        ) s2
        GROUP BY s2.Department, s2.Y, s2.M
    ) t ON t.Department = q.Department
       AND t.Y = q.Y
       AND t.M = q.M
) x
WHERE x.iDate > 0
