/* ============================================================================
   DEPLOY SCRIPT - "III Summary" Focus Query report
   Run against the LIVE Focus X company database (SSMS), then refresh the
   report list in Focus X. Safe to re-run (idempotent).

   What it does:
     1) Registers report "III Summary" (cCore_Reports_0)
     2) Stores its query (cCore_RDQuery_0)
     3) Creates the Standard layout (cCore_ReportLayouts_0, identity id)
     4) Creates the 6 layout columns positionally (cCore_ReportColumns_0)

   NOTE: if report id 70257 already exists on the live server for a DIFFERENT
   report, change @ReportId below to a free id, e.g.:
     SELECT MAX(iReportId) + 1 FROM cCore_Reports_0;
   ============================================================================ */
SET XACT_ABORT ON;
BEGIN TRAN;

DECLARE @ReportId int = 70257;
DECLARE @LayoutId int;

/* 1) Report header -------------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM cCore_Reports_0 WHERE iReportId = @ReportId)
BEGIN
    INSERT INTO cCore_Reports_0
        (iReportId, sReportName, iModule, iReportType, iSourceType,
         iCreatedBy, iModifiedBy, iCreatedDate, iModifiedDate, bDelayFetch)
    VALUES
        (@ReportId, 'III Summary', 2, 1, 1, 1, 1, 132778008, 132778008, 0);
    PRINT 'cCore_Reports_0: inserted report ' + CAST(@ReportId AS varchar);
END
ELSE
    PRINT 'cCore_Reports_0: report ' + CAST(@ReportId AS varchar) + ' already exists - kept';

/* 2) Query text ------------------------------------------------------------ */
DECLARE @q nvarchar(max) = N'SELECT
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
        SELECT
            iMasterId,
            MAX(ReportStatus) AS ReportStatus,
            MAX(sName) AS sName,
            MAX(PlanValue) AS PlanValue
        FROM dbo.vaCore_Account
        WHERE ReportStatus IN (1, 2, 3)
          AND ISNULL(bGroup, 0) = 0
        GROUP BY iMasterId
    ) acc
    INNER JOIN (
        SELECT DISTINCT d.iBookNo AS iMasterId
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
        UNION
        SELECT d.iCode
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
        UNION
        SELECT d.iBookNo
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
    ) act ON act.iMasterId = acc.iMasterId
    LEFT JOIN (
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
) x
GROUP BY x.ReportStatus
ORDER BY x.ReportStatus';

IF EXISTS (SELECT 1 FROM cCore_RDQuery_0 WHERE iReportId = @ReportId)
BEGIN
    UPDATE cCore_RDQuery_0 SET sSqlQuery = @q WHERE iReportId = @ReportId;
    PRINT 'cCore_RDQuery_0: query updated';
END
ELSE
BEGIN
    INSERT INTO cCore_RDQuery_0 (iReportId, iSecurityType, sConnection, sSqlQuery)
    VALUES (@ReportId, 0, '', @q);
    PRINT 'cCore_RDQuery_0: query inserted';
END

/* 3) Layout (iLayoutId is IDENTITY - auto-generated) ---------------------- */
IF NOT EXISTS (SELECT 1 FROM cCore_ReportLayouts_0 WHERE iReportId = @ReportId)
BEGIN
    INSERT INTO cCore_ReportLayouts_0 (iReportId, sLayoutName, iFlag, iSubReportId, bPrintZero)
    VALUES (@ReportId, 'Standard', 0, 0, 0);
    SET @LayoutId = CAST(SCOPE_IDENTITY() AS int);
    PRINT 'cCore_ReportLayouts_0: layout ' + CAST(@LayoutId AS varchar) + ' created';
END
ELSE
BEGIN
    SET @LayoutId = (SELECT TOP 1 iLayoutId FROM cCore_ReportLayouts_0 WHERE iReportId = @ReportId);
    PRINT 'cCore_ReportLayouts_0: layout ' + CAST(@LayoutId AS varchar) + ' already exists - kept';
END

/* 4) Layout columns, positional FieldId 1..6 ------------------------------ */
IF NOT EXISTS (SELECT 1 FROM cCore_ReportColumns_0 WHERE iLayoutId = @LayoutId)
BEGIN
    INSERT INTO cCore_ReportColumns_0
        (iLayoutId, iFieldId, sColumn, iType, iAlignment, fColumnWidth, iDecimalInColumn, iMiscOption, iParentId, iSubParentId, sAliasName)
    VALUES
        (@LayoutId, 1, 'Report Status',          0, 0,  80, 0, 64, 0, 0, 'Report Status'),
        (@LayoutId, 2, 'Total Contract Amount',  6, 18, 80, 0, 64, 0, 0, 'Total Contract Amount'),
        (@LayoutId, 3, 'Adv. Rct Amount',        6, 18, 80, 0, 64, 0, 0, 'Adv. Rct Amount'),
        (@LayoutId, 4, 'Balance Amount',         6, 18, 80, 0, 64, 0, 0, 'Balance Amount'),
        (@LayoutId, 5, 'Plan Value',             6, 18, 80, 0, 64, 0, 0, 'Plan Value'),
        (@LayoutId, 6, 'No. of Accounts',        6, 18, 80, 0, 64, 0, 0, 'No. of Accounts');
    PRINT 'cCore_ReportColumns_0: 6 columns created';
END
ELSE
    PRINT 'cCore_ReportColumns_0: layout already has columns - kept (delete them first if you want them recreated)';

COMMIT;
PRINT 'DONE. Refresh the report list in Focus X and run "III Summary".';

/* Verification (optional):
SELECT r.iReportId, r.sReportName, l.iLayoutId, LEN(q.sSqlQuery) AS SqlLen
FROM cCore_Reports_0 r
JOIN cCore_ReportLayouts_0 l ON l.iReportId = r.iReportId
JOIN cCore_RDQuery_0 q ON q.iReportId = r.iReportId
WHERE r.iReportId = 70257;
SELECT iFieldId, sColumn, iType, iAlignment FROM cCore_ReportColumns_0
WHERE iLayoutId = (SELECT TOP 1 iLayoutId FROM cCore_ReportLayouts_0 WHERE iReportId = 70257)
ORDER BY iFieldId;
*/
