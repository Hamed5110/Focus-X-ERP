SELECT
    CAST(ISSUE_MARK AS varchar(200)) AS [Report Status],
    CAST(SqlContract AS decimal(18,2)) AS [Total Contract Amount],
    CAST(CubeContract AS decimal(18,2)) AS [Adv. Rct Amount],
    CAST(SqlAdv AS decimal(18,2)) AS [Balance Amount],
    CAST(CubeAdv AS decimal(18,2)) AS [Plan Value],
    CAST(DiffAdv AS decimal(18,2)) AS [No. of Accounts]
FROM (
    SELECT
        ISNULL(s.Code, c.Code) AS Code,
        s.Name AS SqlName,
        ISNULL(s.[Total Contract Amount], 0) AS SqlContract,
        c.CubeContract,
        ISNULL(s.[Adv. Rct Amount], 0) AS SqlAdv,
        c.CubeAdv,
        ISNULL(s.[Adv. Rct Amount], 0) - c.CubeAdv AS DiffAdv,
        CASE WHEN s.Code IS NULL THEN 'CUBE ONLY'
             WHEN c.Code IS NULL THEN 'SQL ONLY'
             ELSE 'AMOUNT DIFF' END + ' | ' + ISNULL(s.Code, c.Code) + ' | ' + ISNULL(s.Name, '') AS ISSUE_MARK
FROM (
    SELECT
        acc.ReportStatus,
        acc.sName AS Name,
        acc.iMasterId,
        (SELECT sCode FROM dbo.mCore_Account mc WHERE mc.iMasterId = acc.iMasterId) AS Code,
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
           /* Cube as-on-date cut-off (packed iDate = Y*65536 + M*256 + D) */
           AND h.iDate <= (YEAR(GETDATE()) * 65536) + (MONTH(GETDATE()) * 256) + DAY(GETDATE())
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
           AND h.iDate <= (YEAR(GETDATE()) * 65536) + (MONTH(GETDATE()) * 256) + DAY(GETDATE())
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
           AND h.iDate <= (YEAR(GETDATE()) * 65536) + (MONTH(GETDATE()) * 256) + DAY(GETDATE())
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
               AND h.iDate <= (YEAR(GETDATE()) * 65536) + (MONTH(GETDATE()) * 256) + DAY(GETDATE())
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
                   AND h.iDate <= (YEAR(GETDATE()) * 65536) + (MONTH(GETDATE()) * 256) + DAY(GETDATE())
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
                   AND h.iDate <= (YEAR(GETDATE()) * 65536) + (MONTH(GETDATE()) * 256) + DAY(GETDATE())
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
              AND h.iDate <= (YEAR(GETDATE()) * 65536) + (MONTH(GETDATE()) * 256) + DAY(GETDATE())
            UNION
            SELECT DISTINCT d.iBookNo
            FROM dbo.tCore_Header_0 h
            INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId = h.iHeaderId
            WHERE h.iVoucherType IN (5634, 5635, 6145)
              AND d.iFaTag = 2040 AND d.iBookNo > 0
              AND ISNULL(h.iAuth, 1) = 1 AND ISNULL(h.bCancelled, 0) = 0
              AND ISNULL(h.bVersion, 0) = 0 AND ISNULL(h.bSuspended, 0) = 0
              AND ISNULL(d.bVoid, 0) = 0
              AND h.iDate <= (YEAR(GETDATE()) * 65536) + (MONTH(GETDATE()) * 256) + DAY(GETDATE())
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

) s
FULL OUTER JOIN (
    VALUES
('1802137', 0.0, 8580.0),
('AC-5528', 38579.0, 20678.0),
('1802452', 0.0, 2550.0),
('AC-4033', 6174.0, 6174.0),
('AC-2488', 8418.0, 9540.0),
('AC-4427', 29382.0, 28043.0),
('AC-1496', -205.0, 0.0),
('AC-6038', 8917.0, 3000.0),
('AC-2048', 8471.0, 9851.0),
('AC-4891', 18009.0, 42498.0),
('AC-4451', 27281.0, 27282.0),
('AC-2577', 5047.0, 3710.0),
('AC-3803', 7928.0, 6377.0),
('AC-4150', 12351.0, 12351.0),
('AC-6859', 3822.0, 3822.0),
('AC-3978', 8550.0, 8550.0),
('AC-3239', 8816.0, 8816.0),
('AC-2720', 7242.0, 7242.0),
('AC-4203', 4880.0, 4880.0),
('AC-6842', 8220.0, 8220.0),
('AC-4027', 6009.0, 6009.0),
('AC-4267', 10861.0, 10484.0),
('AC-3078', 8088.0, 8083.0),
('AC-4448', 8604.0, 8604.0),
('AC-4115', 9030.0, 9030.0),
('AC-3647', 8148.0, 8197.0),
('AC-3488', 7673.0, 7673.0),
('AC-3593', 7004.0, 2600.0),
('AC-6837', 4930.0, 4930.0),
('AC-1886', 7273.0, 7008.0),
('AC-2285', 7063.0, 6817.0),
('AC-3946', 4021.0, 4021.0),
('AC-1425', 3413.0, 4411.0),
('AC-6204', 11401.0, 5700.0),
('AC-4166', 8465.0, 8641.0),
('AC-6898', 3109.0, 3460.0),
('AC-6596', 6757.0, 6757.0),
('AC-7821', 3409.0, 1980.0),
('AC-6542', 15348.0, 14140.0),
('AC-2669', 11138.0, 11138.0),
('AC-6467', 9418.0, 9418.0),
('AC-5642', 7446.0, 7446.0),
('AC-5983', 3723.0, 3723.0),
('AC-3252', 17172.0, 17300.0),
('AC-3733', 15688.0, 15718.0),
('AC-3133', 8858.0, 8949.0),
('AC-7356', 4865.0, 4865.0),
('AC-6276', 8531.0, 8536.0),
('AC-4669', 4978.0, 5038.0),
('AC-5530', 14073.0, 14102.0),
('AC-6389', 13643.0, 13643.0),
('AC-3758', 16885.0, 17185.0),
('AC-2391', -464.0, 3049.0),
('AC-7558', 7876.0, 7876.0),
('AC-5060', 7586.0, 6000.0),
('AC-6910', 5137.0, 5137.0),
('AC-5619', 7870.0, 8020.0),
('AC-2485', 17488.0, 17475.0),
('AC-4655', 11958.0, 11958.0),
('AC-6380', 12767.0, 10470.0),
('AC-7208', 7294.0, 6294.0),
('AC-3300', 9051.0, 9224.0),
('AC-5826', 10598.0, 10600.0),
('AC-6277', 8762.0, 8762.0),
('AC-3638', 6737.0, 6724.0),
('AC-7446', 8793.0, 8793.0),
('AC-3906', 10324.0, 10324.0),
('AC-2991', 11201.0, 11202.0),
('AC-6553', 6872.0, 6452.0),
('AC-4049', 5937.0, 5988.0),
('AC-4751', 3935.0, 3934.0),
('AC-5972', 3342.0, 3342.0),
('AC-6675', 5594.0, 5594.0),
('AC-3450', 7737.0, 7850.0),
('AC-3753', 26842.0, 26990.0),
('AC-5234', 16747.0, 16417.0),
('AC-7056', 4345.0, 4345.0),
('AC-6117', 8608.0, 8594.0),
('AC-2854', 8345.0, 8345.0),
('AC-3018', 22765.0, 21390.0),
('1801312', 81900.0, 40640.0),
('AC-6333', 15193.0, 15193.0),
('AC-3567', 10076.0, 4500.0),
('AC-8140', 6243.0, 6194.0),
('AC-7590', 3665.0, 3700.0),
('AC-2519', 10860.0, 9172.0),
('AC-847', 7533.0, 8213.0),
('AC-5997', 5206.0, 5206.0),
('AC-4004', 8521.0, 8880.0),
('AC-6693', 8562.0, 8562.0),
('AC-5082', 4095.0, 4095.0),
('AC-1113', 7371.0, 7371.0),
('AC-6218', 6853.0, 6853.0),
('AC-5877', 11647.0, 11647.0),
('AC-3947', 8541.0, 8541.0),
('AC-4838', 6468.0, 6468.0),
('AC-5395', 7806.0, 7616.0),
('AC-5836', 14667.0, 14667.0),
('AC-1681', 7321.0, 7321.0),
('AC-6838', 5749.0, 5678.0),
('AC-1936', 8013.0, 8620.0),
('AC-6992', 3342.0, 3342.0),
('AC-4970', 14326.0, 14326.0),
('AC-3317', 14088.0, 11000.0),
('AC-5070', 13804.0, 13890.0),
('AC-5657', 7330.0, 6689.0),
('AC-5592', 6106.0, 6106.0),
('AC-1131', 9587.0, 5762.0),
('AC-4500', 13923.0, 13923.0),
('AC-6813', 4512.0, 4525.0),
('AC-3982', 11244.0, 11241.0),
('AC-6993', 5701.0, 5630.0),
('AC-8070', 3599.0, 3600.0),
('AC-5904', 31122.0, 30842.0),
('AC-6834', 9017.0, 9069.0),
('AC-4933', 6445.0, 6445.0),
('AC-4924', 3168.0, 3168.0),
('AC-3074', 4313.0, 4310.0),
('AC-6206', 8600.0, 8600.0),
('AC-2333', 6102.0, 6100.0),
('AC-3711', 7803.0, 8063.0),
('AC-3862', 7823.0, 7248.0),
('AC-6183', 7283.0, 7283.0),
('1801192', 21684.0, 19180.0),
('AC-4202', 8965.0, 9071.0),
('AC-4710', 17106.0, 17245.0),
('AC-5697', 5864.0, 5862.0),
('AC-5598', 12561.0, 12555.0),
('AC-3373', 11019.0, 13182.0),
('AC-3070', 8811.0, 8557.0),
('AC-4974', 10501.0, 10501.0),
('AC-4810', 7707.0, 7707.0),
('AC-6588', 4705.0, 4705.0),
('AC-5921', 4586.0, 4586.0),
('AC-3398', 11338.0, 11339.0),
('AC-6025', 8090.0, 8090.0),
('AC-4429', 8330.0, 8330.0),
('AC-5185', 6528.0, 6626.0),
('AC-6707', 1365.0, 1365.0),
('AC-4903', 11527.0, 11447.0),
('AC-6104', 7665.0, 7665.0),
('AC-5796', 23128.0, 23128.0),
('AC-4006', 18118.0, 18117.0),
('AC-4108', 5967.0, 3000.0),
('AC-5213', 8977.0, 8977.0),
('AC-7050', 12793.0, 12793.0),
('AC-5942', 7850.0, 7850.0),
('AC-6589', 5520.0, 5520.0),
('AC-6337', 7600.0, 7600.0),
('AC-7059', 6158.0, 6158.0),
('AC-6125', 17361.0, 17369.0),
('AC-4943', 16686.0, 16687.0),
('AC-3832', 12426.0, 12426.0),
('AC-5556', 7919.0, 7919.0),
('AC-6221', 5172.0, 5063.0),
('AC-5059', 5720.0, 5720.0),
('AC-7071', 3005.0, 3005.0),
('AC-2979', 12030.0, 12030.0),
('AC-4516', 7945.0, 7945.0),
('AC-6199', 5960.0, 5961.0),
('AC-5408', 4135.0, 4135.0),
('AC-7294', 6327.0, 6200.0),
('AC-4109', 9586.0, 8585.0),
('AC-1926', 40658.0, 10844.0),
('AC-2246', 10058.0, 9158.0),
('AC-6734', 8745.0, 8850.0),
('AC-2447', 42955.0, 44352.0),
('AC-6061', 26900.0, 26900.0),
('AC-4567', 10173.0, 9956.0),
('AC-1163', 4473.0, 4473.0),
('AC-5324', 12370.0, 12190.0),
('AC-2762', 8577.0, 8978.0),
('AC-5996', 1000.0, 600.0),
('AC-6882', 6777.0, 6800.0),
('AC-4896', 8223.0, 7623.0),
('AC-3810', 11345.0, 11465.0),
('AC-4937', 17368.0, 14704.0),
('AC-6994', 9625.0, 9625.0),
('AC-5180', 10002.0, 9436.0),
('AC-3297', 5803.0, 5294.0),
('AC-5063', 23793.0, 24056.0),
('AC-5212', 11098.0, 11205.0),
('AC-7123', 17758.0, 12057.0),
('AC-6884', 5388.0, 5388.0),
('AC-1291', 10383.0, 10384.0),
('AC-6275', 10182.0, 10182.0),
('AC-5219', 9902.0, 9213.0),
('AC-6519', 10287.0, 10287.0),
('AC-6145', 16002.0, 8000.0),
('AC-3273', 8466.0, 8466.0),
('AC-5890', 5251.0, 5251.0),
('AC-6927', 20201.0, 20200.0),
('AC-7287', 6504.0, 6504.0),
('AC-5760', 6014.0, 5987.0),
('AC-2501', 9571.0, 10053.0),
('AC-1951', 22000.0, 22000.0),
('AC-4983', 43921.0, 39612.0),
('AC-4007', 15357.0, 15654.0),
('AC-5027', 25268.0, 22084.0),
('AC-2570', 4550.0, 4550.0),
('AC-2296', 6678.0, 5000.0),
('AC-4195', 5196.0, 5196.0),
('AC-2071', 10257.0, 10257.0),
('AC-6039', 8391.0, 8899.0),
('AC-6169', 10203.0, 10203.0),
('AC-7074', 5764.0, 5764.0),
('AC-6841', 4611.0, 4611.0),
('AC-5028', 8390.0, 8390.0),
('AC-4444', 6473.0, 6473.0),
('AC-7377', 14800.0, 13800.0),
('AC-6631', 22520.0, 9147.0),
('AC-5913', 10166.0, 10166.0),
('AC-4951', 22746.0, 23539.0),
('AC-4447', 22000.0, 22000.0),
('1801573', 0.0, 4384.0),
('1801721', 7274.0, 7274.0),
('1801747', 1953.0, 5500.0),
('1801786', -1214.0, 7330.0),
('1801793', -7458.0, 8999.0),
('1801794', 0.0, 7877.0),
('1801798', 4197.0, 6635.0),
('AC-4204', 4908.0, 4908.0),
('AC-4196', 5800.0, 5000.0)
) AS c(Code, CubeContract, CubeAdv) ON c.Code = s.Code AND s.ReportStatus = 3
WHERE (s.ReportStatus = 3 OR s.Code IS NULL)
  AND (s.Code IS NULL OR c.Code IS NULL
       OR ISNULL(s.[Total Contract Amount],0) <> c.CubeContract
       OR ISNULL(s.[Adv. Rct Amount],0) <> c.CubeAdv)

) d
ORDER BY d.ISSUE_MARK
