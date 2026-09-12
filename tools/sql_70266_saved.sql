SELECT
    x.Department,
    x.MonthYear AS [Month Year],
    x.Salesman,
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
    x.iDate
FROM (
    SELECT
        q.Department,
        q.MonthYear,
        q.Salesman,
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
        f.PayName,
        f.CollectionAmt,
        f.ContractVal,
        f.EligibleAmt,
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
            CAST(SUM(CASE WHEN d.mAmount1 > 0 THEN d.mAmount1 ELSE 0 END) AS decimal(18, 4)) AS CollectionAmt,
            CAST(ISNULL(hd4610.TotalContractAmt, 0) AS decimal(18, 4)) AS ContractVal,
            CAST(
                CASE
                    WHEN COALESCE(NULLIF(hd4610.PaymentCode, 0), NULLIF(hd4609.PaymentCode, 0), NULLIF(hd4608.PaymentCode, 0), 0) IN (2, 5)
                         AND SUM(CASE WHEN d.mAmount1 > 0 THEN d.mAmount1 ELSE 0 END) > 0
                    THEN SUM(CASE WHEN d.mAmount1 > 0 THEN d.mAmount1 ELSE 0 END)
                    WHEN COALESCE(NULLIF(hd4610.PaymentCode, 0), NULLIF(hd4609.PaymentCode, 0), NULLIF(hd4608.PaymentCode, 0), 0) IN (1, 4)
                         AND ISNULL(hd4610.TotalContractAmt, 0) > 0
                         AND SUM(CASE WHEN d.mAmount1 > 0 THEN d.mAmount1 ELSE 0 END) * 2 + 1
                             >= ISNULL(hd4610.TotalContractAmt, 0)
                    THEN SUM(CASE WHEN d.mAmount1 > 0 THEN d.mAmount1 ELSE 0 END)
                    ELSE 0
                END
            AS decimal(18, 4)) AS EligibleAmt,
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
           AND d.bUpdateFA = 1
           AND d.iCode > 0
           AND d.iFaTag IN (2040, 2057)
           AND ISNULL(d.iType, 0) = 0
           AND ISNULL(d.bVoid, 0) = 0
           AND ISNULL(d.iAuthStatus, 0) < 2
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
            hd4610.PaymentCode,
            hd4609.PaymentCode,
            hd4608.PaymentCode,
            hd4610.TotalContractAmt,
            CASE WHEN ISNULL(sm.sName, N'') = N'' THEN N'(blank)' ELSE sm.sName END
    ) f
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
            dep.sName AS Department,
            (h.iDate & 0xfff0000) / 65536 AS Y,
            (h.iDate & 0xff00) / 256 AS M,
            CASE WHEN ISNULL(sm.sName, N'') = N'' THEN N'(blank)' ELSE sm.sName END AS Salesman,
            CAST(
                CASE
                    WHEN COALESCE(NULLIF(hd4610.PaymentCode, 0), NULLIF(hd4609.PaymentCode, 0), NULLIF(hd4608.PaymentCode, 0), 0) IN (2, 5)
                         AND SUM(CASE WHEN d.mAmount1 > 0 THEN d.mAmount1 ELSE 0 END) > 0
                    THEN SUM(CASE WHEN d.mAmount1 > 0 THEN d.mAmount1 ELSE 0 END)
                    WHEN COALESCE(NULLIF(hd4610.PaymentCode, 0), NULLIF(hd4609.PaymentCode, 0), NULLIF(hd4608.PaymentCode, 0), 0) IN (1, 4)
                         AND ISNULL(hd4610.TotalContractAmt, 0) > 0
                         AND SUM(CASE WHEN d.mAmount1 > 0 THEN d.mAmount1 ELSE 0 END) * 2 + 1
                             >= ISNULL(hd4610.TotalContractAmt, 0)
                    THEN SUM(CASE WHEN d.mAmount1 > 0 THEN d.mAmount1 ELSE 0 END)
                    ELSE 0
                END
            AS decimal(18, 4)) AS EligibleAmt
        FROM dbo.tCore_Header_0 h
        INNER JOIN dbo.tCore_Data_0 d
            ON d.iHeaderId = h.iHeaderId
           AND h.iVoucherType IN (4608, 4609, 4610)
           AND ISNULL(h.iAuth, 1) = 1
           AND ISNULL(h.bCancelled, 0) = 0
           AND ISNULL(h.bVersion, 0) = 0
           AND ISNULL(h.bSuspended, 0) = 0
           AND h.iDate > 0
           AND d.bUpdateFA = 1
           AND d.iCode > 0
           AND d.iFaTag IN (2040, 2057)
           AND ISNULL(d.iType, 0) = 0
           AND ISNULL(d.bVoid, 0) = 0
           AND ISNULL(d.iAuthStatus, 0) < 2
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
            hd4610.PaymentCode,
            hd4609.PaymentCode,
            hd4608.PaymentCode,
            hd4610.TotalContractAmt,
            CASE WHEN ISNULL(sm.sName, N'') = N'' THEN N'(blank)' ELSE sm.sName END
    ) s2
    GROUP BY s2.Department, s2.Y, s2.M
) t ON t.Department = q.Department
   AND t.Y = q.Y
   AND t.M = q.M
) x
WHERE x.iDate > 0