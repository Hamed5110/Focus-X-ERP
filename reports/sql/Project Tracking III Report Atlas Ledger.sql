SELECT
    CAST(ISNULL(m.sName, N'') AS nvarchar(120)) AS [Name],
    CAST(ISNULL(bank.CPRCRNumber, N'') AS nvarchar(40)) AS [CPR/CR Number],
    CAST(ISNULL(m.sCode, N'') AS nvarchar(40)) AS [Code],
    CAST(ISNULL(city.sName, N'') AS nvarchar(80)) AS [City],
    CAST(ISNULL(det.sTelNo, N'') AS nvarchar(40)) AS [Phone No.],
    CAST(ISNULL(sm.sName, N'') AS nvarchar(80)) AS [Salesman name],
    CAST(ISNULL(des.sName, N'') AS nvarchar(80)) AS [Designer name],
    CAST(ISNULL(pipe.sName, N'') AS nvarchar(80)) AS [Pipeline],
    CAST(ISNULL(site.sName, N'') AS nvarchar(80)) AS [Site Status],
    CAST(ISNULL((
        SELECT ROUND(SUM(h.fNet) * -1, 0)
        FROM dbo.tCore_Header_0 h
        WHERE h.iVoucherType = 5634
          AND ISNULL(h.iAuth, 1) = 1
          AND ISNULL(h.bCancelled, 0) = 0
          AND ISNULL(h.bVersion, 0) = 0
          AND ISNULL(h.bSuspended, 0) = 0
          AND h.iDate BETWEEN @iStartDate AND @iEndDate
          AND EXISTS (
                SELECT 1
                FROM dbo.tCore_Data_0 d
                WHERE d.iHeaderId = h.iHeaderId
                  AND d.iFaTag = 2040
                  AND d.iBookNo = m.iMasterId
                  AND ISNULL(d.iType, 0) = 0
                  AND ISNULL(d.bVoid, 0) = 0
          )
    ), 0) AS decimal(18, 2)) AS [Total Contract Amount],
    CAST(ISNULL((
        SELECT ROUND(
            ISNULL((
                SELECT ROUND(SUM(CASE WHEN d.mAmount1 > 0 THEN d.mAmount1 ELSE 0 END), 2)
                FROM dbo.tCore_Header_0 h
                   , dbo.tCore_Data_0 d
                WHERE d.iHeaderId = h.iHeaderId
                  AND h.iVoucherType IN (256, 4096, 4608, 4609, 8707)
                  AND d.bUpdateFA = 1
                  AND d.iFaTag = 2040
                  AND d.iCode = m.iMasterId
                  AND ISNULL(d.iType, 0) = 0
                  AND ISNULL(h.iAuth, 1) = 1
                  AND ISNULL(d.iAuthStatus, 0) < 2
                  AND ISNULL(h.bCancelled, 0) = 0
                  AND ISNULL(h.bVersion, 0) = 0
                  AND ISNULL(h.bSuspended, 0) = 0
                  AND ISNULL(d.bVoid, 0) = 0
                  AND h.iDate BETWEEN @iStartDate AND @iEndDate
            ), 0)
            + ISNULL((
                SELECT ROUND(SUM(CASE WHEN d.mAmount1 > 0 THEN d.mAmount1 ELSE 0 END), 0)
                FROM dbo.tCore_Header_0 h
                   , dbo.tCore_Data_0 d
                WHERE d.iHeaderId = h.iHeaderId
                  AND h.iVoucherType = 4610
                  AND d.bUpdateFA = 1
                  AND d.iFaTag = 2040
                  AND d.iCode = m.iMasterId
                  AND ISNULL(d.iType, 0) = 0
                  AND ISNULL(h.iAuth, 1) = 1
                  AND ISNULL(d.iAuthStatus, 0) < 2
                  AND ISNULL(h.bCancelled, 0) = 0
                  AND ISNULL(h.bVersion, 0) = 0
                  AND ISNULL(h.bSuspended, 0) = 0
                  AND ISNULL(d.bVoid, 0) = 0
                  AND h.iDate BETWEEN @iStartDate AND @iEndDate
            ), 0)
            + ISNULL((
                SELECT ROUND(SUM(CASE WHEN d.mAmount2 > 0 THEN d.mAmount2 ELSE 0 END), 2)
                FROM dbo.tCore_Header_0 h
                   , dbo.tCore_Data_0 d
                WHERE d.iHeaderId = h.iHeaderId
                  AND h.iVoucherType IN (256, 4096, 4608, 4609, 8707)
                  AND d.bUpdateFA = 1
                  AND d.iFaTag = 2040
                  AND d.iBookNo = m.iMasterId
                  AND d.iBookNo <> d.iCode
                  AND ISNULL(d.iType, 0) = 0
                  AND ISNULL(h.iAuth, 1) = 1
                  AND ISNULL(d.iAuthStatus, 0) < 2
                  AND ISNULL(h.bCancelled, 0) = 0
                  AND ISNULL(h.bVersion, 0) = 0
                  AND ISNULL(h.bSuspended, 0) = 0
                  AND ISNULL(d.bVoid, 0) = 0
                  AND h.iDate BETWEEN @iStartDate AND @iEndDate
            ), 0)
            + ISNULL((
                SELECT ROUND(SUM(CASE WHEN d.mAmount2 > 0 THEN d.mAmount2 ELSE 0 END), 0)
                FROM dbo.tCore_Header_0 h
                   , dbo.tCore_Data_0 d
                WHERE d.iHeaderId = h.iHeaderId
                  AND h.iVoucherType = 4610
                  AND d.bUpdateFA = 1
                  AND d.iFaTag = 2040
                  AND d.iBookNo = m.iMasterId
                  AND d.iBookNo <> d.iCode
                  AND ISNULL(d.iType, 0) = 0
                  AND ISNULL(h.iAuth, 1) = 1
                  AND ISNULL(d.iAuthStatus, 0) < 2
                  AND ISNULL(h.bCancelled, 0) = 0
                  AND ISNULL(h.bVersion, 0) = 0
                  AND ISNULL(h.bSuspended, 0) = 0
                  AND ISNULL(d.bVoid, 0) = 0
                  AND h.iDate BETWEEN @iStartDate AND @iEndDate
            ), 0)
        , 0)
    ), 0) AS decimal(18, 2)) AS [Adv. Rct Amount],
    CAST(ISNULL((
        SELECT SUM(
            CASE
                WHEN ISNULL(v.Debit, 0) < 0 THEN -v.Debit
                WHEN ISNULL(v.Debit, 0) > 0 THEN v.Debit
                ELSE 0
            END
            - ISNULL(v.Credit, 0)
        )
        FROM dbo.vtCode_DataFA_0 v
        WHERE v.iMasterId = m.iMasterId
          AND v.iFaTag = 2040
          AND ISNULL(v.bUpdateFA, 0) = 1
          AND ISNULL(v.iAuth, 1) = 1
          AND ISNULL(v.bCancelled, 0) = 0
          AND ISNULL(v.bVersion, 0) = 0
          AND ISNULL(v.bSuspended, 0) = 0
          AND ISNULL(v.bVoid, 0) = 0
          AND v.iDate > 0
          AND v.iDate <= @iEndDate
    ), 0) AS decimal(18, 2)) AS [Balance Amount],
    CAST(ISNULL((
        SELECT ROUND(ABS(SUM(h.fNet)), 0)
        FROM dbo.tCore_Header_0 h
        WHERE h.iVoucherType = 5635
          AND ISNULL(h.iAuth, 1) = 1
          AND ISNULL(h.bCancelled, 0) = 0
          AND ISNULL(h.bVersion, 0) = 0
          AND ISNULL(h.bSuspended, 0) = 0
          AND h.iDate BETWEEN @iStartDate AND @iEndDate
          AND EXISTS (
                SELECT 1
                FROM dbo.tCore_Data_0 d
                WHERE d.iHeaderId = h.iHeaderId
                  AND d.iFaTag = 2040
                  AND d.iBookNo = m.iMasterId
                  AND ISNULL(d.iType, 0) = 0
                  AND ISNULL(d.bVoid, 0) = 0
          )
    ), 0) AS decimal(18, 2)) AS [Sales Job Order],
    CAST(ISNULL((
        SELECT ROUND(ABS(SUM(h.fNet)), 0)
        FROM dbo.tCore_Header_0 h
        WHERE h.iVoucherType = 6145
          AND ISNULL(h.iAuth, 1) = 1
          AND ISNULL(h.bCancelled, 0) = 0
          AND ISNULL(h.bVersion, 0) = 0
          AND ISNULL(h.bSuspended, 0) = 0
          AND h.iDate BETWEEN @iStartDate AND @iEndDate
          AND EXISTS (
                SELECT 1
                FROM dbo.tCore_Data_0 d
                WHERE d.iHeaderId = h.iHeaderId
                  AND d.iFaTag = 2040
                  AND d.iBookNo = m.iMasterId
                  AND ISNULL(d.iType, 0) = 0
                  AND ISNULL(d.bVoid, 0) = 0
          )
    ), 0) AS decimal(18, 2)) AS [Production Note Amount],
    CAST(ISNULL(smod.sName, N'') AS nvarchar(80)) AS [Sales Module],
    CAST(
        ISNULL((
            SELECT ROUND(SUM(h.fNet) * -1, 0)
            FROM dbo.tCore_Header_0 h
            WHERE h.iVoucherType = 5634
              AND ISNULL(h.iAuth, 1) = 1
              AND ISNULL(h.bCancelled, 0) = 0
              AND ISNULL(h.bVersion, 0) = 0
              AND ISNULL(h.bSuspended, 0) = 0
              AND h.iDate BETWEEN @iStartDate AND @iEndDate
              AND EXISTS (
                    SELECT 1
                    FROM dbo.tCore_Data_0 d
                    WHERE d.iHeaderId = h.iHeaderId
                      AND d.iFaTag = 2040
                      AND d.iBookNo = m.iMasterId
                      AND ISNULL(d.iType, 0) = 0
                      AND ISNULL(d.bVoid, 0) = 0
              )
        ), 0)
        - ISNULL((
            SELECT ROUND(ABS(SUM(h.fNet)), 0)
            FROM dbo.tCore_Header_0 h
            WHERE h.iVoucherType = 5635
              AND ISNULL(h.iAuth, 1) = 1
              AND ISNULL(h.bCancelled, 0) = 0
              AND ISNULL(h.bVersion, 0) = 0
              AND ISNULL(h.bSuspended, 0) = 0
              AND h.iDate BETWEEN @iStartDate AND @iEndDate
              AND EXISTS (
                    SELECT 1
                    FROM dbo.tCore_Data_0 d
                    WHERE d.iHeaderId = h.iHeaderId
                      AND d.iFaTag = 2040
                      AND d.iBookNo = m.iMasterId
                      AND ISNULL(d.iType, 0) = 0
                      AND ISNULL(d.bVoid, 0) = 0
              )
        ), 0)
    AS decimal(18, 2)) AS [Sales Job Order Balance],
    CAST(ISNULL(u.PlanValue, 0) AS decimal(18, 2)) AS [Plan Value],
    CAST(ISNULL(u.Planningmonth, N'') AS nvarchar(40)) AS [Planning month],
    CAST(ISNULL(acEmp.sName, N'') AS nvarchar(80)) AS [Account  Controller],
    CAST(ISNULL(rs.sName, N'') AS nvarchar(60)) AS [Account2.Report Status]
FROM dbo.mCore_Account m
INNER JOIN dbo.muCore_Account u
    ON u.iMasterId = m.iMasterId
LEFT JOIN dbo.muCore_Account_BankDetails bank
    ON bank.iMasterId = m.iMasterId
LEFT JOIN dbo.muCore_Account_Details det
    ON det.iMasterId = m.iMasterId
LEFT JOIN dbo.mCore_City city
    ON city.iMasterId = det.iCity
LEFT JOIN dbo.mCore_Salesman sm
    ON sm.iMasterId = u.Salesmanname
LEFT JOIN dbo.mPay_Employee des
    ON des.iMasterId = u.Designername
LEFT JOIN dbo.mCore_pipeline pipe
    ON pipe.iMasterId = u.Pipeline
LEFT JOIN dbo.mCore_sitestatus site
    ON site.iMasterId = u.SiteStatus
LEFT JOIN dbo.mCore_salesmodule smod
    ON smod.iMasterId = u.SalesModule
LEFT JOIN dbo.mPay_Employee acEmp
    ON acEmp.iMasterId = u.AccountController
LEFT JOIN dbo.mCore_reportstatus rs
    ON rs.iMasterId = u.ReportStatus
WHERE ISNULL(m.bGroup, 0) = 0
  AND u.ReportStatus = 3
  AND (m.iMasterId = @CustomerName)
