-- FINAL FIXED VERSION: All metrics use same voucher type filters
--DECLARE @iStartDate INT = 20240101
--DECLARE @iEndDate INT = 20241231

SELECT 
    COALESCE(atlas.ReportStatusName, aknan.ReportStatusAKName) AS [Report Status],
    
    -- Atlas Columns
    ISNULL(atlas_contract.ContractAmount, 0) AS [Contract Amount (Atlas)],
    ISNULL(atlas_advance.Cr, 0) AS [Advance Amount (Atlas)],
    ISNULL(atlas_contract.ContractAmount, 0) - ISNULL(atlas_advance.Cr, 0) AS [Balance Amount (Atlas)],
    ISNULL(atlas_plan.PlanValue, 0) AS [Plan Value (Atlas)],
    ISNULL(atlas_contract.AccountCount, 0) AS [Account Count (Atlas)],
    
    -- Aknan Columns
    ISNULL(aknan_contract.ContractAmount, 0) AS [Contract Amount (Aknan)],
    ISNULL(aknan_advance.Cr, 0) AS [Advance Amount (Aknan)],
    ISNULL(aknan_contract.ContractAmount, 0) - ISNULL(aknan_advance.Cr, 0) AS [Balance Amount (Aknan)],
    ISNULL(aknan_plan.PlanValue, 0) AS [Plan Value (Aknan)],
    ISNULL(aknan_contract.AccountCount, 0) AS [Account Count (Aknan)]

FROM (
    -- Status list
    SELECT 1 AS StatusID, '1 Pending - I' AS ReportStatusName
    UNION ALL SELECT 2, '2 In Progress - II'
    UNION ALL SELECT 3, '3 Partial Consumed - III'
) AS StatusList

-- ============== ATLAS METRICS ==============

-- Atlas Contract Accounts (5634 voucher type) - ORIGINAL QUERY (NO CHANGE)
LEFT JOIN (
    SELECT 
        a.ReportStatus,
        SUM(ht.fNet * -1) AS ContractAmount,
        COUNT(DISTINCT a.iMasterId) AS AccountCount
    FROM ( 
        SELECT iHeaderId, MAX(iFaTag) AS iFaTag, MAX(iBookNo) AS iBookNo
        FROM tCore_Data_0
        GROUP BY iHeaderId 
    ) dt
    JOIN tCore_Header_0 ht ON dt.iHeaderId = ht.iHeaderId
    JOIN vmCore_Account a ON dt.iBookNo = a.iMasterId
    WHERE ht.iVoucherType = 5634
      AND dt.iFaTag = 2040
      AND a.bGroup = 0
      AND a.iStatus = 0
      AND a.ReportStatus NOT IN (0,4)
    GROUP BY a.ReportStatus
) AS atlas_contract ON StatusList.StatusID = atlas_contract.ReportStatus

-- Atlas Plan Value - UPDATED to include advance vouchers
LEFT JOIN (
    SELECT 
        a.ReportStatus,
        SUM(a.PlanValue) AS PlanValue
    FROM vmCore_Account a
    WHERE a.bGroup = 0 
      AND a.iStatus = 0
      AND a.ReportStatus NOT IN (0,4)
      -- UPDATED: Include accounts that have EITHER contract (5634) OR advance vouchers
      AND EXISTS (
          SELECT 1 FROM tCore_Data_0 dt
          JOIN tCore_Header_0 ht ON dt.iHeaderId = ht.iHeaderId
          WHERE (dt.iBookNo = a.iMasterId AND ht.iVoucherType = 5634)  -- Contract accounts
             OR (dt.iCode = a.iMasterId AND ht.iVoucherType IN (4610,4609,4608,256,8707))  -- Advance accounts
            AND dt.iFaTag = 2040
      )
    GROUP BY a.ReportStatus
) AS atlas_plan ON StatusList.StatusID = atlas_plan.ReportStatus

-- ============== AKNAN METRICS ==============

-- Aknan Contract Accounts (5634 voucher type with date filter) - ORIGINAL QUERY (NO CHANGE)
LEFT JOIN (
    SELECT 
        a.ReportStatusAK,
        SUM(ht.fNet * -1) AS ContractAmount,
        COUNT(DISTINCT a.iMasterId) AS AccountCount
    FROM ( 
        SELECT iHeaderId, MAX(iFaTag) AS iFaTag, MAX(iBookNo) AS iBookNo
        FROM tCore_Data_0
        GROUP BY iHeaderId 
    ) dt
    JOIN tCore_Header_0 ht ON dt.iHeaderId = ht.iHeaderId
    JOIN vmCore_Account a ON dt.iBookNo = a.iMasterId
    WHERE ht.iVoucherType = 5634
      AND dt.iFaTag = 2057
      AND ht.iDate BETWEEN @iStartDate AND @iEndDate
      AND a.bGroup = 0
      AND a.iStatus = 0
      AND a.ReportStatusAK NOT IN (0,4)
    GROUP BY a.ReportStatusAK
) AS aknan_contract ON StatusList.StatusID = aknan_contract.ReportStatusAK

-- Aknan Plan Value - UPDATED to include advance vouchers
LEFT JOIN (
    SELECT 
        a.ReportStatusAK,
        SUM(a.PlanValueAK) AS PlanValue
    FROM vmCore_Account a
    WHERE a.bGroup = 0 
      AND a.iStatus = 0
      AND a.ReportStatusAK NOT IN (0,4)
      -- UPDATED: Include accounts that have EITHER contract (5634) OR advance vouchers
      AND EXISTS (
          SELECT 1 FROM tCore_Data_0 dt
          JOIN tCore_Header_0 ht ON dt.iHeaderId = ht.iHeaderId
          WHERE (dt.iBookNo = a.iMasterId AND ht.iVoucherType = 5634  -- Contract accounts
                 AND dt.iFaTag = 2057
                 AND ht.iDate BETWEEN @iStartDate AND @iEndDate)
             OR (dt.iCode = a.iMasterId AND ht.iVoucherType IN (4610,4609,4608,256,8707)  -- Advance accounts
                 AND dt.iFaTag = 2057
                 AND ht.iDate BETWEEN @iStartDate AND @iEndDate)
      )
    GROUP BY a.ReportStatusAK
) AS aknan_plan ON StatusList.StatusID = aknan_plan.ReportStatusAK

-- ============== ADVANCE AMOUNTS ==============

-- Atlas Advance Amount (4610,4609,4608,256,8707 voucher types) - ORIGINAL QUERY
LEFT JOIN (
    SELECT ReportStatus, SUM(Cr) AS Cr
    FROM (
        SELECT a.ReportStatus,
               SUM(CASE WHEN mAmount1 > 0 THEN mAmount1 ELSE 0 END) AS Cr
        FROM tCore_Data_0 dt
        JOIN tCore_Header_0 ht ON dt.iHeaderId = ht.iHeaderId
        JOIN vmCore_Account a ON dt.iCode = a.iMasterId AND a.iAccountType IN (5,7)
        WHERE dt.bUpdateFA = 1
          AND ht.bSuspended = 0
          AND ht.iVoucherType IN (4610,4609,4608,256,8707)
          AND dt.iFaTag = 2040
          AND ht.iDate BETWEEN @iStartDate AND @iEndDate
          AND a.ReportStatus NOT IN (0,4)
        GROUP BY a.ReportStatus
        UNION ALL
        SELECT a.ReportStatus,
               SUM(CASE WHEN mAmount2 > 0 THEN mAmount2 ELSE 0 END) AS Cr
        FROM tCore_Data_0 dt
        JOIN tCore_Header_0 ht ON dt.iHeaderId = ht.iHeaderId
        JOIN vmCore_Account a ON dt.iBookNo = a.iMasterId AND a.iAccountType IN (5,7)
        WHERE dt.bUpdateFA = 1
          AND ht.bSuspended = 0
          AND ht.iVoucherType IN (4610,4609,4608,256,8707)
          AND dt.iFaTag = 2040
          AND ht.iDate BETWEEN @iStartDate AND @iEndDate
          AND a.ReportStatus NOT IN (0,4)
        GROUP BY a.ReportStatus
    ) x
    GROUP BY ReportStatus
) AS atlas_advance ON StatusList.StatusID = atlas_advance.ReportStatus

-- Aknan Advance Amount (4610,4609,4608,256,8707 voucher types) - ORIGINAL QUERY
LEFT JOIN (
    SELECT ReportStatusAK, SUM(Cr) AS Cr
    FROM (
        SELECT a.ReportStatusAK,
               SUM(CASE WHEN mAmount1 > 0 THEN mAmount1 ELSE 0 END) AS Cr
        FROM tCore_Data_0 dt
        JOIN tCore_Header_0 ht ON dt.iHeaderId = ht.iHeaderId
        JOIN vmCore_Account a ON dt.iCode = a.iMasterId AND a.iAccountType IN (5,7)
        WHERE dt.bUpdateFA = 1
          AND ht.bSuspended = 0
          AND ht.iVoucherType IN (4610,4609,4608,256,8707)
          AND dt.iFaTag = 2057
          AND ht.iDate BETWEEN @iStartDate AND @iEndDate
          AND a.ReportStatusAK NOT IN (0,4)
        GROUP BY a.ReportStatusAK
        UNION ALL
        SELECT a.ReportStatusAK,
               SUM(CASE WHEN mAmount2 > 0 THEN mAmount2 ELSE 0 END) AS Cr
        FROM tCore_Data_0 dt
        JOIN tCore_Header_0 ht ON dt.iHeaderId = ht.iHeaderId
        JOIN vmCore_Account a ON dt.iBookNo = a.iMasterId AND a.iAccountType IN (5,7)
        WHERE dt.bUpdateFA = 1
          AND ht.bSuspended = 0
          AND ht.iVoucherType IN (4610,4609,4608,256,8707)
          AND dt.iFaTag = 2057
          AND ht.iDate BETWEEN @iStartDate AND @iEndDate
          AND a.ReportStatusAK NOT IN (0,4)
        GROUP BY a.ReportStatusAK
    ) x
    GROUP BY ReportStatusAK
) AS aknan_advance ON StatusList.StatusID = aknan_advance.ReportStatusAK

-- ============== STATUS NAMES ==============

-- Get Atlas status names
LEFT JOIN (
    SELECT DISTINCT ReportStatus, ReportStatusName
    FROM vmCore_Account
    WHERE bGroup = 0 AND iStatus = 0 AND ReportStatus NOT IN (0,4)
) AS atlas ON StatusList.StatusID = atlas.ReportStatus

-- Get Aknan status names
LEFT JOIN (
    SELECT DISTINCT ReportStatusAK, ReportStatusAKName
    FROM vmCore_Account
    WHERE bGroup = 0 AND iStatus = 0 AND ReportStatusAK NOT IN (0,4)
) AS aknan ON StatusList.StatusID = aknan.ReportStatusAK

ORDER BY StatusList.StatusID;
