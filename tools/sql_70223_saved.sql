-- Parameters - Set your dates here
--DECLARE @iStartDate INT = 20240101
--DECLARE @iEndDate INT = 20241231

-- Department-wise comparison: Atlas Aluminum (2040) and Aknan Showroom (2057)
SELECT
    dept.Department,
    dept.ReportStatus,
    
    -- Contract Amount
    dept.ContractAmount,
    
    -- Advance Amount
    dept.AdvanceAmount,
    
    -- Balance Amount
    (ISNULL(dept.ContractAmount,0) - ISNULL(dept.AdvanceAmount,0)) AS [Balance Amount],
    
    -- Plan Value
    dept.PlanAmount,
    
    -- Account Count
    dept.AccCount

FROM (
    -- ========== ATLAS ALUMINUM (2040) ==========
    SELECT 
        'Atlas Aluminum' AS Department,
        COALESCE(acc.ReportStatusName, 'Unknown') AS ReportStatus,
        ISNULL(condt.ContractAmount, 0) AS ContractAmount,
        ISNULL(adv.Cr, 0) AS AdvanceAmount,
        ISNULL(acc.PlanAmount, 0) AS PlanAmount,
        ISNULL(acc.AccCount, 0) AS AccCount
    FROM 
    (
        -- Atlas Account Summary (NO DATE FILTER)
        SELECT ReportStatus, ReportStatusName, SUM(PlanValue) AS PlanAmount, COUNT(DISTINCT iMasterId) AS AccCount
        FROM vmCore_Account
        WHERE bGroup = 0 AND iStatus = 0 AND ReportStatus NOT IN (0,4)
        GROUP BY ReportStatus, ReportStatusName
    ) acc
    LEFT JOIN
    (
        -- Atlas Contract Amount (NO DATE FILTER)
        SELECT accd.ReportStatus, SUM(ht.fNet * -1) AS ContractAmount
        FROM (SELECT iHeaderId, MAX(iFaTag) AS iFaTag, MAX(iBookNo) AS iBookNo FROM tCore_Data_0 GROUP BY iHeaderId) dt
        JOIN tCore_Header_0 ht ON dt.iHeaderId = ht.iHeaderId
        JOIN muCore_Account accd ON dt.iBookNo = accd.iMasterId
        WHERE ht.iVoucherType = 5634 AND dt.iFaTag = 2040
        GROUP BY accd.ReportStatus
    ) condt ON condt.ReportStatus = acc.ReportStatus
    LEFT JOIN
    (
        -- Atlas Advance Amount (WITH DATE FILTER)
        SELECT ReportStatus, SUM(Cr) AS Cr
        FROM (
            SELECT accd.ReportStatus, SUM(CASE WHEN mAmount1 > 0 THEN mAmount1 ELSE 0 END) AS Cr
            FROM tCore_Data_0 dt
            JOIN tCore_Header_0 ht ON dt.iHeaderId = ht.iHeaderId
            JOIN vmCore_Account accd ON dt.iCode = accd.iMasterId AND iAccountType IN (5,7)
            WHERE dt.bUpdateFA = 1 AND ht.bSuspended = 0 AND ht.iVoucherType IN (4610,4609,4608,256,8707)
              AND dt.iFaTag = 2040 AND ht.iDate BETWEEN @iStartDate AND @iEndDate AND accd.ReportStatus NOT IN (0,4)
            GROUP BY accd.ReportStatus
            UNION ALL
            SELECT accd.ReportStatus, SUM(CASE WHEN mAmount2 > 0 THEN mAmount2 ELSE 0 END) AS Cr
            FROM tCore_Data_0 dt
            JOIN tCore_Header_0 ht ON dt.iHeaderId = ht.iHeaderId
            JOIN vmCore_Account accd ON dt.iBookNo = accd.iMasterId AND iAccountType IN (5,7)
            WHERE dt.bUpdateFA = 1 AND ht.bSuspended = 0 AND ht.iVoucherType IN (4610,4609,4608,256,8707)
              AND dt.iFaTag = 2040 AND ht.iDate BETWEEN @iStartDate AND @iEndDate AND accd.ReportStatus NOT IN (0,4)
            GROUP BY accd.ReportStatus
        ) leddt
        GROUP BY ReportStatus
    ) adv ON adv.ReportStatus = acc.ReportStatus

    UNION ALL

    -- ========== AKNAN SHOWROOM (2057) ==========
    SELECT 
        'Aknan Showroom' AS Department,
        COALESCE(acc.ReportStatusAKName, 'Unknown') AS ReportStatus,
        ISNULL(condt.ContractAmount, 0) AS ContractAmount,
        ISNULL(adv.Cr, 0) AS AdvanceAmount,
        ISNULL(acc.PlanAmount, 0) AS PlanAmount,
        ISNULL(acc.AccCount, 0) AS AccCount
    FROM 
    (
        -- Aknan Account Summary (WITH DATE FILTER)
        SELECT ReportStatusAK, ReportStatusAKName, SUM(PlanValueAK) AS PlanAmount, COUNT(DISTINCT iMasterId) AS AccCount
        FROM vmCore_Account
        WHERE bGroup = 0 AND iStatus = 0 AND ReportStatusAK NOT IN (0,4)
          AND iMasterId IN (SELECT DISTINCT iBookNo FROM tCore_Data_0 d JOIN tCore_Header_0 h ON h.iHeaderId = d.iHeaderId WHERE h.iDate BETWEEN @iStartDate AND @iEndDate AND d.iFaTag = 2057)
        GROUP BY ReportStatusAK, ReportStatusAKName
    ) acc
    LEFT JOIN
    (
        -- Aknan Contract Amount (WITH DATE FILTER)
        SELECT accd.ReportStatusAK, SUM(ht.fNet * -1) AS ContractAmount
        FROM (SELECT iHeaderId, MAX(iFaTag) AS iFaTag, MAX(iBookNo) AS iBookNo FROM tCore_Data_0 GROUP BY iHeaderId) dt
        JOIN tCore_Header_0 ht ON dt.iHeaderId = ht.iHeaderId
        JOIN vmCore_Account accd ON dt.iBookNo = accd.iMasterId
        WHERE ht.iVoucherType = 5634 AND dt.iFaTag = 2057 AND ht.iDate BETWEEN @iStartDate AND @iEndDate
        GROUP BY accd.ReportStatusAK
    ) condt ON condt.ReportStatusAK = acc.ReportStatusAK
    LEFT JOIN
    (
        -- Aknan Advance Amount (WITH DATE FILTER)
        SELECT ReportStatusAK, SUM(Cr) AS Cr
        FROM (
            SELECT accd.ReportStatusAK, SUM(CASE WHEN mAmount1 > 0 THEN mAmount1 ELSE 0 END) AS Cr
            FROM tCore_Data_0 dt
            JOIN tCore_Header_0 ht ON dt.iHeaderId = ht.iHeaderId
            JOIN vmCore_Account accd ON dt.iCode = accd.iMasterId AND iAccountType IN (5,7)
            WHERE dt.bUpdateFA = 1 AND ht.bSuspended = 0 AND ht.iVoucherType IN (4610,4609,4608,256,8707)
              AND dt.iFaTag = 2057 AND ht.iDate BETWEEN @iStartDate AND @iEndDate AND accd.ReportStatusAK NOT IN (0,4)
            GROUP BY accd.ReportStatusAK
            UNION ALL
            SELECT accd.ReportStatusAK, SUM(CASE WHEN mAmount2 > 0 THEN mAmount2 ELSE 0 END) AS Cr
            FROM tCore_Data_0 dt
            JOIN tCore_Header_0 ht ON dt.iHeaderId = ht.iHeaderId
            JOIN vmCore_Account accd ON dt.iBookNo = accd.iMasterId AND iAccountType IN (5,7)
            WHERE dt.bUpdateFA = 1 AND ht.bSuspended = 0 AND ht.iVoucherType IN (4610,4609,4608,256,8707)
              AND dt.iFaTag = 2057 AND ht.iDate BETWEEN @iStartDate AND @iEndDate AND accd.ReportStatusAK NOT IN (0,4)
            GROUP BY accd.ReportStatusAK
        ) leddt
        GROUP BY ReportStatusAK
    ) adv ON adv.ReportStatusAK = acc.ReportStatusAK

) dept

-- *** FILTER ONLY THE DESIRED REPORT STATUSES ***
WHERE dept.ReportStatus IN ('1 Pending - I', '2 In Progress - II', '3 Partial Consumed - III')

ORDER BY dept.Department, dept.ReportStatus
