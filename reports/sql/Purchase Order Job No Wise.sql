/*
  Purchase Order Job No. wise
  Converted from Focus cube: Purchase Order Job No. wise (ReportId 70054)

  Database : Focus8080
  Document : all vouchers of Purchase Order class (iVoucherClass = 2560)
             Purchases Orders / PO Local / PO Import / PO Service

  Tables used
  -----------
  tCore_Header_0           voucher header (DocNo, voucher amount, class)
  tCore_HeaderData2562_0   PO Local extra header fields
                           ItemQty, GlassQty, GlassClassification,
                           DeliveryDate, OrderInsatllationDate
  tCore_Data_0             voucher body (vendor book account, line amount)
  tCore_Data2562_0         PO Local extra body fields (Notepad)
  tCore_Data_Tags_0        extra masters on each body line
                           iTag3010 = Job Order
                           iTag3054 = Main Group
                           iTag3080 = Delivery Status
  mCore_Account            VendorAC.Name   (iBookNo)
  mCore_JobOrder           Job Order.Name
  mCore_maingroup          Main Group.Name
  mCore_deliverystatus     Delivery Status.Name

  Parameter
  ---------
  @OrderInstallationDate   optional. Leave NULL for all dates.
                           Example: '2026-08-23'
*/

DECLARE @OrderInstallationDate date = NULL;

;WITH BodyLines AS (
    SELECT
        d.iHeaderId,
        tags.iTag3010 AS JobOrderId,
        d.iBookNo     AS VendorId,
        tags.iTag3054 AS MainGroupId,
        tags.iTag3080 AS DeliveryStatusId,
        d2562.Notepad,
        ABS(d.mAmount1) AS LineAmount
    FROM dbo.tCore_Data_0 AS d
    LEFT JOIN dbo.tCore_Data2562_0 AS d2562
        ON d2562.iBodyId = d.iBodyId
    LEFT JOIN dbo.tCore_Data_Tags_0 AS tags
        ON tags.iBodyId = d.iBodyId
)
SELECT
    h.sVoucherNo AS DocNo,
    jo.sName     AS [Job Order.Name],
    acc.sName    AS [VendorAC.Name],
    CAST(SUM(b.LineAmount) AS decimal(18, 2)) AS Amount,
    mg.sName     AS [Main Group.Name],
    MAX(b.Notepad) AS Notepad,
    ds.sName     AS [Delivery Status],
    hd.ItemQty   AS [Item Qty],
    CAST(hd.GlassQty AS decimal(18, 2)) AS [Glass Qty],
    CASE hd.GlassClassification
        WHEN 1 THEN N'Big'
        WHEN 2 THEN N'Small'
        WHEN 3 THEN N'Big & Small'
        ELSE N''
    END AS [Glass Classification],
    CASE
        WHEN ISNULL(hd.DeliveryDate, 0) = 0 THEN NULL
        ELSE dbo.IntToDate(hd.DeliveryDate)
    END AS [Delivery Date],
    CASE
        WHEN ISNULL(hd.OrderInsatllationDate, 0) = 0 THEN NULL
        ELSE dbo.IntToDate(hd.OrderInsatllationDate)
    END AS [Order Installation Date]
FROM dbo.tCore_Header_0 AS h
LEFT JOIN dbo.tCore_HeaderData2562_0 AS hd
    ON hd.iHeaderId = h.iHeaderId
INNER JOIN BodyLines AS b
    ON b.iHeaderId = h.iHeaderId
LEFT JOIN dbo.mCore_Account AS acc
    ON acc.iMasterId = b.VendorId
LEFT JOIN dbo.mCore_JobOrder AS jo
    ON jo.iMasterId = b.JobOrderId
LEFT JOIN dbo.mCore_maingroup AS mg
    ON mg.iMasterId = b.MainGroupId
LEFT JOIN dbo.mCore_deliverystatus AS ds
    ON ds.iMasterId = b.DeliveryStatusId
WHERE h.iVoucherClass = 2560
  AND ISNULL(h.bCancelled, 0) = 0
  AND ISNULL(h.bDraft, 0) = 0
  AND ISNULL(h.bSuspended, 0) = 0
  AND (
        @OrderInstallationDate IS NULL
        OR (
            ISNULL(hd.OrderInsatllationDate, 0) <> 0
            AND CONVERT(date, dbo.IntToDate(hd.OrderInsatllationDate)) = @OrderInstallationDate
        )
      )
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
    hd.OrderInsatllationDate
ORDER BY
    jo.sName,
    h.sVoucherNo;
