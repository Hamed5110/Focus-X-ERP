/* ============================================================================
   III Summary Report  (report 70257 / layout 6906)
   Totals per Report Status for the "Project Tracking III Report Atlas"
   detail query (report 70256). Same engine, grouped by Report Status.

   Columns: Report Status, Total Contract Amount, Adv. Rct Amount,
            Balance Amount, Plan Value, No. of Accounts

   Report Statuses: 1 = Pending - I, 2 = In Progress - II,
                    3 = Partial Consumed - III  (mCore_reportstatus).
   Department: Atlas Aluminum only (tCore_Data_0.iFaTag = 2040).
   Balance = Total Contract Amount - Adv. Rct Amount (signed contract),
   so each row satisfies Balance = Contract - Adv exactly.
   No. of Accounts = number of accounts (rows) per status in the detail report.

   Row inclusion mirrors the cube: the cube builds rows from its transaction
   sets, so an account appears only when its OWN master has at least one
   direct Atlas (2040) authorized live document (SO 5634 or credit types
   256/4096/4608/4609/4610/8707). Accounts with no direct activity (or amounts
   only via a same-name twin) get no row - exactly like the cube export.

   Duplicate-name rule mirrors the cube: the cube groups rows by account NAME
   (Account2.Name), so several Trade-Receivables accounts sharing one name form
   ONE cube row. That row's Report Status is the MINIMUM Report Status among
   the name members that actually contribute transactions to the cube's sets
   (dept 2040: any voucher type linked via iCode, or 5634/5635/6145 via
   iBookNo). Example: "Mr. Ahmed Abdulla Jaffar" = status-3 account 12606 plus
   status-0 twin 14589 that has receipts -> group status 0 -> the cube hides
   the name from the III report. Verified against cube exports: names whose
   contributing members are {3,0} vanish (Ahmed Abdulla Jaffar, Hussain Ali),
   {4,3} stays in III (Dr. Muneer Mahdi), and twins with no cube-set activity
   never affect the row (Mr. Hassan Ali).

   Account group filter mirrors the cube run from "Trade Receivables"
   (group sCode 180): only accounts that sit directly under the
   "Trade Receivables" group in the main account tree
   (mCore_AccountTreeDetails iTreeId = 0, iParentId = group id) are counted.
   The group id is resolved by name at run time, so no hard-coded id and no
   separate setup script are needed on the cloud server.
   ============================================================================ */
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
        /* Balance = Total Contract Amount - Adv. Rct Amount (signed contract) */
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
        /* Cube row-inclusion rule: account's OWN master has >= 1 direct
           Atlas (2040) authorized live document (SO or any credit type). */
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
    LEFT JOIN (
        /* Cube duplicate-name rule: a name shared by several Trade-Receivables
           accounts is ONE cube row whose Report Status = MIN(ReportStatus)
           over the members that contribute transactions to the cube's sets
           (dept 2040: any voucher type via iCode, or 5634/5635/6145 via
           iBookNo). Keep this account only when its own status equals the
           name-group status; otherwise the cube files the row under another
           status report (or hides it when the group status is 0). */
        SELECT n.sName, MIN(n.ReportStatus) AS GroupStatus
        FROM (
            SELECT v.iMasterId, v.sName, v.ReportStatus
            FROM dbo.vaCore_Account v
            WHERE v.iTreeId = 0 AND ISNULL(v.bGroup, 0) = 0
              AND v.iMasterId IN (
                    SELECT tr.iMasterId
                    FROM dbo.mCore_AccountTreeDetails tr
                    WHERE tr.iTreeId = 0
                      AND tr.iParentId IN (
                            SELECT iMasterId
                            FROM dbo.mCore_Account
                            WHERE sName = 'Trade Receivables'
                              AND ISNULL(bGroup, 0) = 1
                          )
                  )
              AND v.sName IN (
                    SELECT v2.sName
                    FROM dbo.vaCore_Account v2
                    WHERE v2.iTreeId = 0 AND ISNULL(v2.bGroup, 0) = 0
                      AND v2.iMasterId IN (
                            SELECT tr2.iMasterId
                            FROM dbo.mCore_AccountTreeDetails tr2
                            WHERE tr2.iTreeId = 0
                              AND tr2.iParentId IN (
                                    SELECT iMasterId
                                    FROM dbo.mCore_Account
                                    WHERE sName = 'Trade Receivables'
                                      AND ISNULL(bGroup, 0) = 1
                                  )
                          )
                    GROUP BY v2.sName
                    HAVING COUNT(DISTINCT v2.iMasterId) > 1
                  )
        ) n
        INNER JOIN (
            SELECT DISTINCT d.iCode AS iMasterId
            FROM dbo.tCore_Header_0 h
            INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId = h.iHeaderId
            WHERE d.iFaTag = 2040 AND d.iCode > 0
              AND ISNULL(h.iAuth, 1) = 1 AND ISNULL(h.bCancelled, 0) = 0
              AND ISNULL(h.bVersion, 0) = 0 AND ISNULL(h.bSuspended, 0) = 0
              AND ISNULL(d.bVoid, 0) = 0
            UNION
            SELECT DISTINCT d.iBookNo
            FROM dbo.tCore_Header_0 h
            INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId = h.iHeaderId
            WHERE h.iVoucherType IN (5634, 5635, 6145)
              AND d.iFaTag = 2040 AND d.iBookNo > 0
              AND ISNULL(h.iAuth, 1) = 1 AND ISNULL(h.bCancelled, 0) = 0
              AND ISNULL(h.bVersion, 0) = 0 AND ISNULL(h.bSuspended, 0) = 0
              AND ISNULL(d.bVoid, 0) = 0
        ) c ON c.iMasterId = n.iMasterId
        GROUP BY n.sName
    ) grp ON grp.sName = acc.sName
    WHERE acc.iMasterId > 0
      /* Cube duplicate-name rule: the account row survives only when its own
         Report Status equals the name-group status (MIN over contributing
         members). Single-name accounts have no grp row and always pass. */
      AND (grp.sName IS NULL OR grp.GroupStatus = acc.ReportStatus)
      /* Cube account-group filter: only accounts under "Trade Receivables"
         (main account tree, iTreeId = 0). Group id resolved by name. */
      AND acc.iMasterId IN (
            SELECT tr.iMasterId
            FROM dbo.mCore_AccountTreeDetails tr
            WHERE tr.iTreeId = 0
              AND tr.iParentId IN (
                    SELECT iMasterId
                    FROM dbo.mCore_Account
                    WHERE sName = 'Trade Receivables'
                      AND ISNULL(bGroup, 0) = 1
                  )
          )
      /* Cube zero-row suppression (IsPrintZeroValue = false): the cube hides
         rows whose value columns are all zero, so all-zero accounts (e.g.
         documents netting to 0) must not be counted either. */
      AND (
            ISNULL(doc.ContractAmt, 0) <> 0
         OR ISNULL(fa.AdvRctAmt, 0) <> 0
         OR ISNULL(acc.PlanValue, 0) <> 0
          )
) x
GROUP BY x.ReportStatus
ORDER BY x.ReportStatus
