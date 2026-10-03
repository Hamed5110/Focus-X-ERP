/* ============================================================================
   Glass Status All Summary — totals by Job Order, Vendor, Delivery Status

   Same documents as cube 70252 / Glass Status All.sql (PO class 2560, option 62).
   Optional filters (0 = all). Focus substitutes @Param = 0 as a bare 0,
   which becomes OR 0 and SQL Server raises non-boolean near OR.
   Use CASE @Param WHEN 0 THEN col ELSE @Param END only.
   Glass Qty is once per PO header, then summed. Do not DECLARE.
   Packed @iStartDate/@iEndDate. Hide iDate Fraction.
   ============================================================================ */
SELECT
    CAST(ISNULL(x.JobOrderName, N'') AS nvarchar(120)) AS [Job Order.Name],
    CAST(ISNULL(x.VendorName, N'') AS nvarchar(120)) AS [VendorAC.Name],
    CAST(ISNULL(x.DeliveryStatusName, N'') AS nvarchar(40)) AS [Delivery Status],
    CAST(ISNULL(x.POCount, 0) AS decimal(18, 2)) AS [PO Count],
    CAST(ISNULL(x.Amount, 0) AS decimal(18, 2)) AS [Amount],
    CAST(ISNULL(x.GlassQty, 0) AS decimal(18, 2)) AS [Glass Qty],
    CAST(ISNULL(x.iDate, 0) AS decimal(18, 0)) AS iDate
FROM (
    SELECT
        p.JobOrderName,
        p.VendorName,
        p.DeliveryStatusName,
        CAST(COUNT(*) AS decimal(18, 2)) AS POCount,
        CAST(SUM(p.Amount) AS decimal(18, 2)) AS Amount,
        CAST(SUM(p.GlassQty) AS decimal(18, 2)) AS GlassQty,
        MAX(p.iDate) AS iDate
    FROM (
        SELECT
            h.iHeaderId,
            ISNULL(jo.sName, N'') AS JobOrderName,
            ISNULL(acc.sName, N'') AS VendorName,
            ISNULL(ds.sName, N'') AS DeliveryStatusName,
            CAST(SUM(ABS(ISNULL(d.mAmount1, 0))) AS decimal(18, 2)) AS Amount,
            CAST(MAX(ISNULL(hd.GlassQty, 0)) AS decimal(18, 2)) AS GlassQty,
            MAX(h.iDate) AS iDate
        FROM dbo.tCore_Header_0 h
        INNER JOIN dbo.tCore_Data_0 d
            ON d.iHeaderId = h.iHeaderId
           AND h.iVoucherClass = 2560
           AND ISNULL(h.bCancelled, 0) = 0
           AND ISNULL(h.bVersion, 0) = 0
           AND ISNULL(h.bSuspended, 0) = 0
           AND ISNULL(h.bDraft, 0) = 0
           AND h.iDate > 0
           AND h.iDate BETWEEN @iStartDate AND @iEndDate
           AND ISNULL(d.iType, 0) = 0
           AND ISNULL(d.bVoid, 0) = 0
        LEFT JOIN dbo.tCore_HeaderData2562_0 hd
            ON hd.iHeaderId = h.iHeaderId
        LEFT JOIN dbo.tCore_Data_Tags_0 tags
            ON tags.iBodyId = d.iBodyId
        LEFT JOIN dbo.tCore_Data2562_0 d2562
            ON d2562.iBodyId = d.iBodyId
        LEFT JOIN dbo.mCore_Account acc
            ON acc.iMasterId = d.iBookNo
        LEFT JOIN dbo.mCore_JobOrder jo
            ON jo.iMasterId = tags.iTag3010
        LEFT JOIN dbo.mCore_deliverystatus ds
            ON ds.iMasterId = CASE
                WHEN ISNULL(tags.iTag3080, 0) > 0 THEN tags.iTag3080
                ELSE ISNULL(d2562.DeliveryStatus, 0)
            END
        WHERE ISNULL(d.iBookNo, 0) = CASE @VendorAC WHEN 0 THEN ISNULL(d.iBookNo, 0) ELSE @VendorAC END
          AND ISNULL(tags.iTag3010, 0) = CASE @JobOrder WHEN 0 THEN ISNULL(tags.iTag3010, 0) ELSE @JobOrder END
          AND ISNULL(
                CASE
                    WHEN ISNULL(tags.iTag3080, 0) > 0 THEN tags.iTag3080
                    ELSE d2562.DeliveryStatus
                END,
                0
              ) = CASE @DeliveryStatus
                    WHEN 0 THEN ISNULL(
                        CASE
                            WHEN ISNULL(tags.iTag3080, 0) > 0 THEN tags.iTag3080
                            ELSE d2562.DeliveryStatus
                        END,
                        0
                    )
                    ELSE @DeliveryStatus
                  END
        GROUP BY
            h.iHeaderId,
            jo.sName,
            acc.sName,
            ds.sName
    ) p
    GROUP BY
        p.JobOrderName,
        p.VendorName,
        p.DeliveryStatusName
) x
WHERE x.iDate > 0
