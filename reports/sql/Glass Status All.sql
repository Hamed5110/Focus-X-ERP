/* ============================================================================
   Glass Status All — Query from cube 70252 (Glass Status All (1).xml)

   Cube: class Purchase Orders 2560, DocumentOption 62 (auth + unauth,
   not suspended). No Delivery Status = Ready filter (that is 70248 Glass Status).
   Grouping DocNo. Amount = field 171 voucher amount (line ABS(mAmount1) per job).
   Extra glass fields live on PO Local 2562 header. Narration = sNarration
   (header extra; COALESCE Local / Import / Service / class).

   Focus help (Report Designer): Parameters = Field name + Variable name
   + Field type, then use that variable in the SQL.
   Working Query reports (70200, 70224) only do column = @Param.
   Field type must be Account / Job Order / Delivery Status — not View.
   Every picker must have a value (empty @token is deleted → syntax error).
   Do not DECLARE. Do not ORDER BY. Hide iDate Fraction.
   ============================================================================ */
SELECT
    CAST(ISNULL(x.DocNo, N'') AS nvarchar(40)) AS [DocNo],
    CAST(ISNULL(x.JobOrderName, N'') AS nvarchar(120)) AS [Job Order.Name],
    CAST(ISNULL(x.VendorName, N'') AS nvarchar(120)) AS [VendorAC.Name],
    CAST(ISNULL(x.Amount, 0) AS decimal(18, 2)) AS [Amount],
    CAST(ISNULL(x.MainGroupName, N'') AS nvarchar(80)) AS [Main Group.Name],
    CAST(ISNULL(x.DeliveryStatusName, N'') AS nvarchar(40)) AS [Delivery Status],
    CAST(ISNULL(x.ItemQty, N'') AS nvarchar(40)) AS [Item Qty],
    CAST(ISNULL(x.GlassQty, 0) AS decimal(18, 2)) AS [Glass Qty],
    CAST(ISNULL(x.GlassClassName, N'') AS nvarchar(40)) AS [Glass Classification],
    CAST(NULLIF(x.DeliveryDateText, N'') AS nvarchar(10)) AS [Delivery Date],
    CAST(NULLIF(x.InstallDateText, N'') AS nvarchar(10)) AS [Order Installation Date],
    CAST(ISNULL(x.Narration, N'') AS nvarchar(500)) AS [Narration],
    CAST(ISNULL(x.iDate, 0) AS decimal(18, 0)) AS iDate
FROM (
    SELECT
        h.sVoucherNo AS DocNo,
        ISNULL(jo.sName, N'') AS JobOrderName,
        ISNULL(acc.sName, N'') AS VendorName,
        CAST(SUM(ABS(ISNULL(d.mAmount1, 0))) AS decimal(18, 2)) AS Amount,
        ISNULL(mg.sName, N'') AS MainGroupName,
        ISNULL(ds.sName, N'') AS DeliveryStatusName,
        ISNULL(hd.ItemQty, N'') AS ItemQty,
        CAST(ISNULL(hd.GlassQty, 0) AS decimal(18, 2)) AS GlassQty,
        CASE ISNULL(hd.GlassClassification, 0)
            WHEN 1 THEN N'Big'
            WHEN 2 THEN N'Small'
            WHEN 3 THEN N'Big & Small'
            ELSE N''
        END AS GlassClassName,
        CASE
            WHEN ISNULL(hd.DeliveryDate, 0) > 0 THEN
                CAST((hd.DeliveryDate / 65536) AS varchar(4))
                + N'-'
                + RIGHT(N'0' + CAST(((hd.DeliveryDate / 256) % 256) AS varchar(2)), 2)
                + N'-'
                + RIGHT(N'0' + CAST((hd.DeliveryDate % 256) AS varchar(2)), 2)
            ELSE N''
        END AS DeliveryDateText,
        CASE
            WHEN ISNULL(hd.OrderInsatllationDate, 0) > 0 THEN
                CAST((hd.OrderInsatllationDate / 65536) AS varchar(4))
                + N'-'
                + RIGHT(N'0' + CAST(((hd.OrderInsatllationDate / 256) % 256) AS varchar(2)), 2)
                + N'-'
                + RIGHT(N'0' + CAST((hd.OrderInsatllationDate % 256) AS varchar(2)), 2)
            ELSE N''
        END AS InstallDateText,
        ISNULL(NULLIF(LTRIM(RTRIM(COALESCE(hd.sNarration, hi.sNarration, hs.sNarration, hc.sNarration, N''))), N''), N'') AS Narration,
        h.iDate AS iDate
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
       AND d.iBookNo = @VendorAC
       AND EXISTS (
            SELECT 1
            FROM dbo.tCore_Data_Tags_0 tjo
            WHERE tjo.iBodyId = d.iBodyId
              AND tjo.iTag3010 = @JobOrder
       )
       AND EXISTS (
            SELECT 1
            FROM dbo.tCore_Data_0 dx
            LEFT JOIN dbo.tCore_Data_Tags_0 tds
                ON tds.iBodyId = dx.iBodyId
            LEFT JOIN dbo.tCore_Data2562_0 xds
                ON xds.iBodyId = dx.iBodyId
            WHERE dx.iBodyId = d.iBodyId
              AND CASE
                    WHEN ISNULL(tds.iTag3080, 0) > 0 THEN tds.iTag3080
                    ELSE ISNULL(xds.DeliveryStatus, 0)
                  END = @DeliveryStatus
       )
    LEFT JOIN dbo.tCore_HeaderData2562_0 hd
        ON hd.iHeaderId = h.iHeaderId
    LEFT JOIN dbo.tCore_HeaderData2563_0 hi
        ON hi.iHeaderId = h.iHeaderId
    LEFT JOIN dbo.tCore_HeaderData2564_0 hs
        ON hs.iHeaderId = h.iHeaderId
    LEFT JOIN dbo.tCore_HeaderData2560_0 hc
        ON hc.iHeaderId = h.iHeaderId
    LEFT JOIN dbo.tCore_Data_Tags_0 tags
        ON tags.iBodyId = d.iBodyId
    LEFT JOIN dbo.tCore_Data2562_0 d2562
        ON d2562.iBodyId = d.iBodyId
    LEFT JOIN dbo.mCore_Account acc
        ON acc.iMasterId = d.iBookNo
    LEFT JOIN dbo.mCore_JobOrder jo
        ON jo.iMasterId = tags.iTag3010
    LEFT JOIN dbo.mCore_maingroup mg
        ON mg.iMasterId = tags.iTag3054
    LEFT JOIN dbo.mCore_deliverystatus ds
        ON ds.iMasterId = CASE
            WHEN ISNULL(tags.iTag3080, 0) > 0 THEN tags.iTag3080
            ELSE ISNULL(d2562.DeliveryStatus, 0)
        END
    GROUP BY
        h.sVoucherNo,
        jo.sName,
        acc.sName,
        mg.sName,
        ds.sName,
        hd.ItemQty,
        hd.GlassQty,
        hd.GlassClassification,
        hd.DeliveryDate,
        hd.OrderInsatllationDate,
        hd.sNarration,
        hi.sNarration,
        hs.sNarration,
        hc.sNarration,
        h.iDate
) x
WHERE x.iDate > 0
