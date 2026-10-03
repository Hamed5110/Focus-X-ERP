/* ============================================================================
   Customer Statement — cube 70125 grain + Excel columns

   Cube XML Customer Statement Report (1).xml  ReportId 70125  type Cubes
   Row groupings (this is why the cube prints every voucher):
     1 Department.Name 5042
     2 Account2.Name   5002
     3 Account2.Code   5003
     4 Transaction Fields.DocNo 1   ← unique per CON / receipt
   Without DocNo as a group, cube/Query SUM to one customer row (Hussain screenshot).
   Transaction sets: FA AccountingTransactionsOfanAccount + 5634.
   Types: 4609, 4610, 4096, 8707, 5634. Contract Amount field 171 Sign -/+ (50).
   Rct Amount field 16 where type <> 5634. Cube Balance formula c8-c9 (line, not running).
   Query keeps running B_i (Excel). DocNo is prefixed yyyy-MM-dd + row type so
   Focus group/sort matches the window ORDER BY (date, contract then receipt).
   Without that prefix, ATI* sorts before CON* and Balance looks random.
   Negative Balance is nvarchar with '-'. Layout Type Text (Sign None abs’s it).

   Columns: Department, Customer Name, Customer Code, DocNo, Particulars, ...
   Layout: Query (not Details+Query). Group through DocNo if grouping.
   Contract Date / Receipt Date / Balance / DocNo = Text.
   Contract Amount Sign -/+. Credit Sign None. Hide iDate.
   Do not DECLARE. Do not ORDER BY. Trailing WHERE iDate > 0.
   ============================================================================ */
SELECT
    CAST(ISNULL(x.Department, N'') AS nvarchar(80)) AS [Department],
    CAST(ISNULL(x.CustomerName, N'') AS nvarchar(120)) AS [Customer Name],
    CAST(ISNULL(x.CustomerCode, N'') AS nvarchar(40)) AS [Customer Code],
    CAST(
        CASE WHEN ISNULL(x.iDate, 0) > 0 THEN
            CAST((x.iDate & 0xfff0000) / 65536 AS varchar(4))
            + N'-'
            + RIGHT(N'0' + CAST((x.iDate & 0xff00) / 256 AS varchar(2)), 2)
            + N'-'
            + RIGHT(N'0' + CAST((x.iDate & 0xff) AS varchar(2)), 2)
        ELSE N'0000-00-00'
        END
        + N' '
        + CASE ISNULL(x.RowType, 0)
            WHEN 0 THEN N'O '
            WHEN 1 THEN N'C '
            ELSE N'R '
          END
        + CASE WHEN ISNULL(x.VoucherNo, N'') <> N'' THEN x.VoucherNo ELSE N'Opening' END
    AS nvarchar(60)) AS [DocNo],
    CAST(ISNULL(x.Particulars, N'') AS nvarchar(80)) AS [Particulars],
    CAST(ISNULL(x.OpportunityType, N'') AS nvarchar(40)) AS [Opportunity Type],
    CAST(ISNULL(x.ContractNo, N'') AS nvarchar(40)) AS [Contract / Sales Order No.],
    CAST(NULLIF(x.ContractDate, N'') AS nvarchar(10)) AS [Contract Date],
    CAST(NULLIF(x.ContractAmt, 0) AS decimal(18, 2)) AS [Contract Amount],
    CAST(ISNULL(x.ReceiptNo, N'') AS nvarchar(40)) AS [Receipt No.],
    CAST(NULLIF(x.ReceiptDate, N'') AS nvarchar(10)) AS [Receipt Date],
    CAST(NULLIF(x.ReceiptAmt, 0) AS decimal(18, 2)) AS [Amount Received],
    CAST(NULLIF(x.DebitAmt, 0) AS decimal(18, 2)) AS [Debit],
    CAST(NULLIF(x.CreditAmt, 0) AS decimal(18, 2)) AS [Credit],
    CASE
        WHEN ISNULL(x.BalanceAmt, 0) < 0 THEN
            N'-' + CONVERT(nvarchar(23), CAST(ABS(x.BalanceAmt) AS decimal(18, 2)))
        ELSE
            CONVERT(nvarchar(24), CAST(ISNULL(x.BalanceAmt, 0) AS decimal(18, 2)))
    END AS [Balance],
    CAST(ISNULL(x.iDate, 0) AS decimal(18, 0)) AS iDate
FROM (
    SELECT
        u.CustomerId,
        u.CustomerCode,
        u.CustomerName,
        u.Department,
        u.Particulars,
        u.OpportunityType,
        u.ContractNo,
        u.ContractDate,
        u.ContractAmt,
        u.ReceiptNo,
        u.ReceiptDate,
        u.ReceiptAmt,
        u.DebitAmt,
        u.CreditAmt,
        u.VoucherNo,
        u.RowType,
        SUM(u.DebitAmt - u.CreditAmt) OVER (
            PARTITION BY u.CustomerId
            ORDER BY u.SortKey, u.iDate, u.RowType, u.VoucherNo, u.LineId
            ROWS UNBOUNDED PRECEDING
        ) AS BalanceAmt,
        u.iDate
    FROM (
        SELECT
            o.CustomerId,
            o.CustomerCode,
            o.CustomerName,
            o.Department,
            N'Opening Balance' AS Particulars,
            N'' AS OpportunityType,
            N'' AS ContractNo,
            N'' AS ContractDate,
            CAST(0 AS decimal(18, 4)) AS ContractAmt,
            N'' AS ReceiptNo,
            N'' AS ReceiptDate,
            CAST(0 AS decimal(18, 4)) AS ReceiptAmt,
            CAST(CASE WHEN o.OpenAmt > 0 THEN o.OpenAmt ELSE 0 END AS decimal(18, 4)) AS DebitAmt,
            CAST(CASE WHEN o.OpenAmt < 0 THEN -o.OpenAmt ELSE 0 END AS decimal(18, 4)) AS CreditAmt,
            0 AS SortKey,
            0 AS RowType,
            N'' AS VoucherNo,
            0 AS LineId,
            @iStartDate AS iDate
        FROM (
            SELECT
                b.CustomerId,
                MAX(b.CustomerCode) AS CustomerCode,
                MAX(b.CustomerName) AS CustomerName,
                MAX(b.Department) AS Department,
                CAST(SUM(b.SignedAmt) AS decimal(18, 4)) AS OpenAmt
            FROM (
                SELECT
                    d.iBookNo AS CustomerId,
                    ISNULL(acc.sCode, N'') AS CustomerCode,
                    ISNULL(acc.sName, N'') AS CustomerName,
                    ISNULL(dep.sName, N'') AS Department,
                    CAST(-MAX(h.fNet) AS decimal(18, 4)) AS SignedAmt
                FROM dbo.tCore_Header_0 h
                INNER JOIN dbo.tCore_Data_0 d
                    ON d.iHeaderId = h.iHeaderId
                   AND h.iVoucherType = 5634
                   AND ISNULL(h.iAuth, 1) = 1
                   AND ISNULL(h.bCancelled, 0) = 0
                   AND ISNULL(h.bVersion, 0) = 0
                   AND ISNULL(h.bSuspended, 0) = 0
                   AND h.iDate > 0
                   AND h.iDate < @iStartDate
                   AND ISNULL(d.iType, 0) = 0
                   AND ISNULL(d.bVoid, 0) = 0
                   AND d.iBookNo > 0
                   AND d.iBookNo = @CustomerName
                INNER JOIN dbo.mCore_Account acc
                    ON acc.iMasterId = d.iBookNo
                   AND acc.iAccountType IN (5, 7)
                LEFT JOIN dbo.mCore_Department dep
                    ON dep.iMasterId = d.iFaTag
                GROUP BY d.iBookNo, acc.sCode, acc.sName, dep.sName, h.iHeaderId

                UNION ALL

                SELECT
                    v.iMasterId AS CustomerId,
                    ISNULL(acc.sCode, N'') AS CustomerCode,
                    ISNULL(acc.sName, N'') AS CustomerName,
                    ISNULL(dep.sName, N'') AS Department,
                    CAST(SUM(
                        CASE WHEN v.Debit < 0 THEN -v.Debit ELSE 0 END
                        - CASE WHEN v.Credit > 0 THEN v.Credit ELSE 0 END
                    ) AS decimal(18, 4)) AS SignedAmt
                FROM dbo.vtCode_DataFA_0 v
                INNER JOIN dbo.mCore_Account acc
                    ON acc.iMasterId = v.iMasterId
                   AND acc.iAccountType IN (5, 7)
                LEFT JOIN dbo.mCore_Department dep
                    ON dep.iMasterId = v.iFaTag
                WHERE v.iMasterId = @CustomerName
                  AND v.iDate > 0
                  AND v.iDate < @iStartDate
                  AND v.iVoucherType IN (256, 3840, 4096, 4608, 4609, 4610, 8704, 8705, 8707, 8708)
                  AND (v.Debit < 0 OR v.Credit > 0)
                GROUP BY v.iMasterId, acc.sCode, acc.sName, dep.sName
            ) b
            GROUP BY b.CustomerId
            HAVING SUM(b.SignedAmt) <> 0
        ) o

        UNION ALL

        SELECT
            d.iBookNo AS CustomerId,
            ISNULL(acc.sCode, N'') AS CustomerCode,
            ISNULL(acc.sName, N'') AS CustomerName,
            ISNULL(dep.sName, N'') AS Department,
            N'Contract / Sales Order' AS Particulars,
            CASE ISNULL(hd.OpportunityType, 0)
                WHEN 1 THEN N'New Business'
                WHEN 2 THEN N'Additional Business'
                WHEN 3 THEN N'Size Variations'
                WHEN 4 THEN N'Partner Sale'
                WHEN 5 THEN N'Size Variation'
                WHEN 6 THEN N'Cancellation'
                WHEN 7 THEN N'Free of Cost'
                WHEN 8 THEN N'Promotion Prize'
                WHEN 9 THEN N'Amendment'
                WHEN 10 THEN N'Adjustment'
                WHEN 0 THEN N''
                ELSE CAST(hd.OpportunityType AS nvarchar(10))
            END AS OpportunityType,
            ISNULL(h.sVoucherNo, N'') AS ContractNo,
            CASE
                WHEN ISNULL(h.iDate, 0) > 0 THEN
                    RIGHT(N'0' + CAST((h.iDate & 0xff) AS varchar(2)), 2)
                    + N'/'
                    + RIGHT(N'0' + CAST((h.iDate & 0xff00) / 256 AS varchar(2)), 2)
                    + N'/'
                    + CAST((h.iDate & 0xfff0000) / 65536 AS varchar(4))
                ELSE N''
            END AS ContractDate,
            CAST(-MAX(h.fNet) AS decimal(18, 4)) AS ContractAmt,
            N'' AS ReceiptNo,
            N'' AS ReceiptDate,
            CAST(0 AS decimal(18, 4)) AS ReceiptAmt,
            CAST(CASE WHEN MAX(h.fNet) < 0 THEN -MAX(h.fNet) ELSE 0 END AS decimal(18, 4)) AS DebitAmt,
            CAST(CASE WHEN MAX(h.fNet) > 0 THEN MAX(h.fNet) ELSE 0 END AS decimal(18, 4)) AS CreditAmt,
            1 AS SortKey,
            1 AS RowType,
            ISNULL(h.sVoucherNo, N'') AS VoucherNo,
            h.iHeaderId AS LineId,
            h.iDate AS iDate
        FROM dbo.tCore_Header_0 h
        INNER JOIN dbo.tCore_Data_0 d
            ON d.iHeaderId = h.iHeaderId
           AND h.iVoucherType = 5634
           AND ISNULL(h.iAuth, 1) = 1
           AND ISNULL(h.bCancelled, 0) = 0
           AND ISNULL(h.bVersion, 0) = 0
           AND ISNULL(h.bSuspended, 0) = 0
           AND h.iDate > 0
           AND h.iDate BETWEEN @iStartDate AND @iEndDate
           AND ISNULL(d.iType, 0) = 0
           AND ISNULL(d.bVoid, 0) = 0
           AND d.iBookNo > 0
           AND d.iBookNo = @CustomerName
        LEFT JOIN dbo.tCore_HeaderData5634_0 hd
            ON hd.iHeaderId = h.iHeaderId
        INNER JOIN dbo.mCore_Account acc
            ON acc.iMasterId = d.iBookNo
           AND acc.iAccountType IN (5, 7)
        LEFT JOIN dbo.mCore_Department dep
            ON dep.iMasterId = d.iFaTag
        GROUP BY
            d.iBookNo,
            acc.sCode,
            acc.sName,
            dep.sName,
            hd.OpportunityType,
            h.sVoucherNo,
            h.iDate,
            h.iHeaderId

        UNION ALL

        SELECT
            v.iMasterId AS CustomerId,
            ISNULL(acc.sCode, N'') AS CustomerCode,
            ISNULL(acc.sName, N'') AS CustomerName,
            ISNULL(dep.sName, N'') AS Department,
            CASE
                WHEN v.iVoucherType IN (4608, 4609, 4610) THEN N'Receipt'
                WHEN v.iVoucherType = 4096 THEN N'Credit Note'
                WHEN v.iVoucherType = 3840 THEN N'Debit Note'
                WHEN v.iVoucherType IN (8704, 8705, 8707, 8708) THEN N'Journal'
                WHEN v.iVoucherType = 256 THEN N'Opening Balance'
                ELSE ISNULL(vc.sName, N'Ledger')
            END AS Particulars,
            N'' AS OpportunityType,
            N'' AS ContractNo,
            N'' AS ContractDate,
            CAST(0 AS decimal(18, 4)) AS ContractAmt,
            ISNULL(v.sVoucherNo, N'') AS ReceiptNo,
            CASE
                WHEN ISNULL(v.iDate, 0) > 0 THEN
                    RIGHT(N'0' + CAST((v.iDate & 0xff) AS varchar(2)), 2)
                    + N'/'
                    + RIGHT(N'0' + CAST((v.iDate & 0xff00) / 256 AS varchar(2)), 2)
                    + N'/'
                    + CAST((v.iDate & 0xfff0000) / 65536 AS varchar(4))
                ELSE N''
            END AS ReceiptDate,
            CAST(CASE WHEN v.Credit > 0 THEN v.Credit ELSE 0 END AS decimal(18, 4)) AS ReceiptAmt,
            CAST(CASE WHEN v.Debit < 0 THEN -v.Debit ELSE 0 END AS decimal(18, 4)) AS DebitAmt,
            CAST(CASE WHEN v.Credit > 0 THEN v.Credit ELSE 0 END AS decimal(18, 4)) AS CreditAmt,
            1 AS SortKey,
            2 AS RowType,
            ISNULL(v.sVoucherNo, N'') AS VoucherNo,
            v.iBodyId AS LineId,
            v.iDate AS iDate
        FROM dbo.vtCode_DataFA_0 v
        INNER JOIN dbo.mCore_Account acc
            ON acc.iMasterId = v.iMasterId
           AND acc.iAccountType IN (5, 7)
        LEFT JOIN dbo.mCore_Department dep
            ON dep.iMasterId = v.iFaTag
        LEFT JOIN dbo.cCore_vouchers_0 vc
            ON vc.iVoucherType = v.iVoucherType
        WHERE v.iMasterId = @CustomerName
          AND v.iDate > 0
          AND v.iDate BETWEEN @iStartDate AND @iEndDate
          AND v.iVoucherType IN (256, 3840, 4096, 4608, 4609, 4610, 8704, 8705, 8707, 8708)
          AND (v.Debit < 0 OR v.Credit > 0)
    ) u
) x
WHERE x.iDate > 0
