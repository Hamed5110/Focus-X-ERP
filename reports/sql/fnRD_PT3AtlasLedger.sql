CREATE OR ALTER FUNCTION dbo.fnRD_PT3AtlasLedger (
    @CustomerName int,
    @iStartDate int,
    @iEndDate int
)
RETURNS TABLE
AS
RETURN
(
SELECT
    acc.iMasterId,
    CAST(ISNULL(acc.sName, N'') AS nvarchar(120)) AS [Name],
    CAST(ISNULL(acc.CPRCRNumber, N'') AS nvarchar(40)) AS [CPR/CR Number],
    CAST(ISNULL(acc.sCode, N'') AS nvarchar(40)) AS [Code],
    CAST(ISNULL(acc.iCityName, N'') AS nvarchar(80)) AS [City],
    CAST(ISNULL(acc.sTelNo, N'') AS nvarchar(40)) AS [Phone No.],
    CAST(ISNULL(acc.SalesmannameName, N'') AS nvarchar(80)) AS [Salesman name],
    CAST(ISNULL(acc.DesignernameName, N'') AS nvarchar(80)) AS [Designer name],
    CAST(ISNULL(acc.PipelineName, N'') AS nvarchar(80)) AS [Pipeline],
    CAST(ISNULL(acc.SiteStatusName, N'') AS nvarchar(80)) AS [Site Status],
    CAST(ISNULL((
        SELECT ROUND(SUM(soHdr.fNet) * -1, 0)
        FROM (
            SELECT DISTINCT h.iHeaderId, h.fNet
            FROM dbo.tCore_Header_0 h
            INNER JOIN dbo.tCore_Data_0 d
                ON d.iHeaderId = h.iHeaderId
               AND h.iVoucherType = 5634
               AND d.iFaTag = 2040
               AND d.iBookNo = acc.iMasterId
               AND ISNULL(d.iType, 0) = 0
               AND ISNULL(h.iAuth, 1) = 1
               AND ISNULL(h.bCancelled, 0) = 0
               AND ISNULL(h.bVersion, 0) = 0
               AND ISNULL(h.bSuspended, 0) = 0
               AND ISNULL(d.bVoid, 0) = 0
               AND h.iDate BETWEEN @iStartDate AND @iEndDate
        ) soHdr
    ), 0) AS decimal(18, 2)) AS [Total Contract Amount],
    CAST(ISNULL((
        SELECT ROUND(SUM(
            CASE
                WHEN advType.iVoucherType = 4610 THEN ROUND(advType.TypeAmt, 0)
                ELSE ROUND(advType.TypeAmt, 2)
            END
        ), 0)
        FROM (
            SELECT
                advLine.iVoucherType,
                SUM(advLine.CreditAmt) AS TypeAmt
            FROM (
                SELECT
                    h.iVoucherType,
                    CASE WHEN d.mAmount1 > 0 THEN d.mAmount1 ELSE 0 END AS CreditAmt
                FROM dbo.tCore_Header_0 h
                INNER JOIN dbo.tCore_Data_0 d
                    ON d.iHeaderId = h.iHeaderId
                   AND h.iVoucherType IN (256, 4096, 4608, 4609, 4610, 8707)
                   AND d.bUpdateFA = 1
                   AND d.iFaTag = 2040
                   AND d.iCode = acc.iMasterId
                   AND ISNULL(d.iType, 0) = 0
                   AND ISNULL(h.iAuth, 1) = 1
                   AND ISNULL(d.iAuthStatus, 0) < 2
                   AND ISNULL(h.bCancelled, 0) = 0
                   AND ISNULL(h.bVersion, 0) = 0
                   AND ISNULL(h.bSuspended, 0) = 0
                   AND ISNULL(d.bVoid, 0) = 0
                   AND h.iDate BETWEEN @iStartDate AND @iEndDate
                UNION ALL
                SELECT
                    h.iVoucherType,
                    CASE WHEN d.mAmount2 > 0 THEN d.mAmount2 ELSE 0 END AS CreditAmt
                FROM dbo.tCore_Header_0 h
                INNER JOIN dbo.tCore_Data_0 d
                    ON d.iHeaderId = h.iHeaderId
                   AND h.iVoucherType IN (256, 4096, 4608, 4609, 4610, 8707)
                   AND d.bUpdateFA = 1
                   AND d.iFaTag = 2040
                   AND d.iBookNo = acc.iMasterId
                   AND d.iBookNo <> d.iCode
                   AND ISNULL(d.iType, 0) = 0
                   AND ISNULL(h.iAuth, 1) = 1
                   AND ISNULL(d.iAuthStatus, 0) < 2
                   AND ISNULL(h.bCancelled, 0) = 0
                   AND ISNULL(h.bVersion, 0) = 0
                   AND ISNULL(h.bSuspended, 0) = 0
                   AND ISNULL(d.bVoid, 0) = 0
                   AND h.iDate BETWEEN @iStartDate AND @iEndDate
            ) advLine
            GROUP BY advLine.iVoucherType
        ) advType
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
        INNER JOIN dbo.mCore_Account mfa
            ON mfa.iMasterId = v.iMasterId
           AND v.iMasterId = acc.iMasterId
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
        SELECT ROUND(ABS(SUM(sjoHdr.fNet)), 0)
        FROM (
            SELECT DISTINCT h.iHeaderId, h.fNet
            FROM dbo.tCore_Header_0 h
            INNER JOIN dbo.tCore_Data_0 d
                ON d.iHeaderId = h.iHeaderId
               AND h.iVoucherType = 5635
               AND d.iFaTag = 2040
               AND d.iBookNo = acc.iMasterId
               AND ISNULL(d.iType, 0) = 0
               AND ISNULL(h.iAuth, 1) = 1
               AND ISNULL(h.bCancelled, 0) = 0
               AND ISNULL(h.bVersion, 0) = 0
               AND ISNULL(h.bSuspended, 0) = 0
               AND ISNULL(d.bVoid, 0) = 0
               AND h.iDate BETWEEN @iStartDate AND @iEndDate
        ) sjoHdr
    ), 0) AS decimal(18, 2)) AS [Sales Job Order],
    CAST(ISNULL((
        SELECT ROUND(ABS(SUM(pnHdr.fNet)), 0)
        FROM (
            SELECT DISTINCT h.iHeaderId, h.fNet
            FROM dbo.tCore_Header_0 h
            INNER JOIN dbo.tCore_Data_0 d
                ON d.iHeaderId = h.iHeaderId
               AND h.iVoucherType = 6145
               AND d.iFaTag = 2040
               AND d.iBookNo = acc.iMasterId
               AND ISNULL(d.iType, 0) = 0
               AND ISNULL(h.iAuth, 1) = 1
               AND ISNULL(h.bCancelled, 0) = 0
               AND ISNULL(h.bVersion, 0) = 0
               AND ISNULL(h.bSuspended, 0) = 0
               AND ISNULL(d.bVoid, 0) = 0
               AND h.iDate BETWEEN @iStartDate AND @iEndDate
        ) pnHdr
    ), 0) AS decimal(18, 2)) AS [Production Note Amount],
    CAST(ISNULL(acc.SalesModuleName, N'') AS nvarchar(80)) AS [Sales Module],
    CAST(
        ISNULL((
            SELECT ROUND(SUM(soBal.fNet) * -1, 0)
            FROM (
                SELECT DISTINCT h.iHeaderId, h.fNet
                FROM dbo.tCore_Header_0 h
                INNER JOIN dbo.tCore_Data_0 d
                    ON d.iHeaderId = h.iHeaderId
                   AND h.iVoucherType = 5634
                   AND d.iFaTag = 2040
                   AND d.iBookNo = acc.iMasterId
                   AND ISNULL(d.iType, 0) = 0
                   AND ISNULL(h.iAuth, 1) = 1
                   AND ISNULL(h.bCancelled, 0) = 0
                   AND ISNULL(h.bVersion, 0) = 0
                   AND ISNULL(h.bSuspended, 0) = 0
                   AND ISNULL(d.bVoid, 0) = 0
                   AND h.iDate BETWEEN @iStartDate AND @iEndDate
            ) soBal
        ), 0)
        - ISNULL((
            SELECT ROUND(ABS(SUM(sjoBal.fNet)), 0)
            FROM (
                SELECT DISTINCT h.iHeaderId, h.fNet
                FROM dbo.tCore_Header_0 h
                INNER JOIN dbo.tCore_Data_0 d
                    ON d.iHeaderId = h.iHeaderId
                   AND h.iVoucherType = 5635
                   AND d.iFaTag = 2040
                   AND d.iBookNo = acc.iMasterId
                   AND ISNULL(d.iType, 0) = 0
                   AND ISNULL(h.iAuth, 1) = 1
                   AND ISNULL(h.bCancelled, 0) = 0
                   AND ISNULL(h.bVersion, 0) = 0
                   AND ISNULL(h.bSuspended, 0) = 0
                   AND ISNULL(d.bVoid, 0) = 0
                   AND h.iDate BETWEEN @iStartDate AND @iEndDate
            ) sjoBal
        ), 0)
    AS decimal(18, 2)) AS [Sales Job Order Balance],
    CAST(ISNULL(acc.PlanValue, 0) AS decimal(18, 2)) AS [Plan Value],
    CAST(ISNULL(acc.Planningmonth, N'') AS nvarchar(40)) AS [Planning month],
    CAST(ISNULL(acc.AccountControllerName, N'') AS nvarchar(80)) AS [Account  Controller],
    CAST(ISNULL(acc.ReportStatusName, N'') AS nvarchar(60)) AS [Account2.Report Status],
    CAST(acc.iDate AS decimal(18, 0)) AS iDate
FROM (
    SELECT
        m.iMasterId,
        m.sName,
        m.sCode,
        ISNULL(bank.CPRCRNumber, N'') AS CPRCRNumber,
        ISNULL(city.sName, N'') AS iCityName,
        ISNULL(det.sTelNo, N'') AS sTelNo,
        ISNULL(sm.sName, N'') AS SalesmannameName,
        ISNULL(des.sName, N'') AS DesignernameName,
        ISNULL(pipe.sName, N'') AS PipelineName,
        ISNULL(site.sName, N'') AS SiteStatusName,
        ISNULL(smod.sName, N'') AS SalesModuleName,
        ISNULL(u.PlanValue, 0) AS PlanValue,
        ISNULL(u.Planningmonth, N'') AS Planningmonth,
        ISNULL(acEmp.sName, N'') AS AccountControllerName,
        ISNULL(rs.sName, N'') AS ReportStatusName,
        CAST(0 AS decimal(18, 0)) AS iDate
    FROM dbo.mCore_Account m
    INNER JOIN dbo.muCore_Account u
        ON u.iMasterId = m.iMasterId
       AND CAST(ISNULL(m.bGroup, 0) AS int) = 0
       AND u.ReportStatus = 3
       AND m.iMasterId = CASE @CustomerName WHEN 0 THEN m.iMasterId ELSE @CustomerName END
    LEFT JOIN dbo.muCore_Account_Details det
        ON det.iMasterId = m.iMasterId
    LEFT JOIN dbo.muCore_Account_BankDetails bank
        ON bank.iMasterId = m.iMasterId
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
) acc
)
