/* ============================================================================
   Purchase Order Cost Reduction — Excel layout
   Documents: Purchase Order - Local 2562, Purchase Order - Import 2563

   One row per vendor + item.
   Months are horizontal: Jan previous vs Jan current ... Dec.
   Previous = last PO rate in that month last year.
   New = last PO rate in that month of the header year.
   Difference = previous - new (positive = cheaper), same as Cost Reduction 2026.xlsx.
   Header 01/01/2026-31/12/2026 means Jan-2025 vs Jan-2026, and so on.

   Last 10 columns are item-group flags Y / N like the sheet:
   Wooden Sheets, Accessories, Appliances, Consumables, Silicone,
   Corner Joints, Powder Coating, Tools and Machineries, Seals, Stones.

   Report Type Query. Vendor Name first. Sort by vendor, item.
   Do not DECLARE. Do not convert iDate. Do not Sum Y/N or %.
   ============================================================================ */
SELECT TOP 100 PERCENT
    x.VendorName AS [Vendor Name],
    CAST(x.ItemGroup AS nvarchar(80)) AS [Item Group],
    CAST(x.ItemCode AS nvarchar(80)) AS [Item Code],
    CAST(x.ItemName AS nvarchar(120)) AS [Item Name],
    CAST(NULLIF(x.UnitName, N'') AS nvarchar(40)) AS [Unit],
    CAST(NULLIF(x.Department, N'') AS nvarchar(80)) AS [Department],
    CAST(x.PrevYear AS nvarchar(4)) AS [Previous Year],
    CAST(x.CurrYear AS nvarchar(4)) AS [Current Year],
    CAST(x.JanPy AS decimal(18, 4)) AS [Jan Previous Price],
    CAST(x.JanCy AS decimal(18, 4)) AS [Jan New Price],
    CAST(x.JanPy - x.JanCy AS decimal(18, 4)) AS [Jan Price Difference],
    CAST(CASE WHEN x.JanPy > 0 THEN (x.JanPy - x.JanCy) * 100.0 / x.JanPy ELSE 0 END AS decimal(18, 2)) AS [Jan Percentage Difference],
    CAST(x.FebPy AS decimal(18, 4)) AS [Feb Previous Price],
    CAST(x.FebCy AS decimal(18, 4)) AS [Feb New Price],
    CAST(x.FebPy - x.FebCy AS decimal(18, 4)) AS [Feb Price Difference],
    CAST(CASE WHEN x.FebPy > 0 THEN (x.FebPy - x.FebCy) * 100.0 / x.FebPy ELSE 0 END AS decimal(18, 2)) AS [Feb Percentage Difference],
    CAST(x.MarPy AS decimal(18, 4)) AS [Mar Previous Price],
    CAST(x.MarCy AS decimal(18, 4)) AS [Mar New Price],
    CAST(x.MarPy - x.MarCy AS decimal(18, 4)) AS [Mar Price Difference],
    CAST(CASE WHEN x.MarPy > 0 THEN (x.MarPy - x.MarCy) * 100.0 / x.MarPy ELSE 0 END AS decimal(18, 2)) AS [Mar Percentage Difference],
    CAST(x.AprPy AS decimal(18, 4)) AS [Apr Previous Price],
    CAST(x.AprCy AS decimal(18, 4)) AS [Apr New Price],
    CAST(x.AprPy - x.AprCy AS decimal(18, 4)) AS [Apr Price Difference],
    CAST(CASE WHEN x.AprPy > 0 THEN (x.AprPy - x.AprCy) * 100.0 / x.AprPy ELSE 0 END AS decimal(18, 2)) AS [Apr Percentage Difference],
    CAST(x.MayPy AS decimal(18, 4)) AS [May Previous Price],
    CAST(x.MayCy AS decimal(18, 4)) AS [May New Price],
    CAST(x.MayPy - x.MayCy AS decimal(18, 4)) AS [May Price Difference],
    CAST(CASE WHEN x.MayPy > 0 THEN (x.MayPy - x.MayCy) * 100.0 / x.MayPy ELSE 0 END AS decimal(18, 2)) AS [May Percentage Difference],
    CAST(x.JunPy AS decimal(18, 4)) AS [Jun Previous Price],
    CAST(x.JunCy AS decimal(18, 4)) AS [Jun New Price],
    CAST(x.JunPy - x.JunCy AS decimal(18, 4)) AS [Jun Price Difference],
    CAST(CASE WHEN x.JunPy > 0 THEN (x.JunPy - x.JunCy) * 100.0 / x.JunPy ELSE 0 END AS decimal(18, 2)) AS [Jun Percentage Difference],
    CAST(x.JulPy AS decimal(18, 4)) AS [Jul Previous Price],
    CAST(x.JulCy AS decimal(18, 4)) AS [Jul New Price],
    CAST(x.JulPy - x.JulCy AS decimal(18, 4)) AS [Jul Price Difference],
    CAST(CASE WHEN x.JulPy > 0 THEN (x.JulPy - x.JulCy) * 100.0 / x.JulPy ELSE 0 END AS decimal(18, 2)) AS [Jul Percentage Difference],
    CAST(x.AugPy AS decimal(18, 4)) AS [Aug Previous Price],
    CAST(x.AugCy AS decimal(18, 4)) AS [Aug New Price],
    CAST(x.AugPy - x.AugCy AS decimal(18, 4)) AS [Aug Price Difference],
    CAST(CASE WHEN x.AugPy > 0 THEN (x.AugPy - x.AugCy) * 100.0 / x.AugPy ELSE 0 END AS decimal(18, 2)) AS [Aug Percentage Difference],
    CAST(x.SepPy AS decimal(18, 4)) AS [Sep Previous Price],
    CAST(x.SepCy AS decimal(18, 4)) AS [Sep New Price],
    CAST(x.SepPy - x.SepCy AS decimal(18, 4)) AS [Sep Price Difference],
    CAST(CASE WHEN x.SepPy > 0 THEN (x.SepPy - x.SepCy) * 100.0 / x.SepPy ELSE 0 END AS decimal(18, 2)) AS [Sep Percentage Difference],
    CAST(x.OctPy AS decimal(18, 4)) AS [Oct Previous Price],
    CAST(x.OctCy AS decimal(18, 4)) AS [Oct New Price],
    CAST(x.OctPy - x.OctCy AS decimal(18, 4)) AS [Oct Price Difference],
    CAST(CASE WHEN x.OctPy > 0 THEN (x.OctPy - x.OctCy) * 100.0 / x.OctPy ELSE 0 END AS decimal(18, 2)) AS [Oct Percentage Difference],
    CAST(x.NovPy AS decimal(18, 4)) AS [Nov Previous Price],
    CAST(x.NovCy AS decimal(18, 4)) AS [Nov New Price],
    CAST(x.NovPy - x.NovCy AS decimal(18, 4)) AS [Nov Price Difference],
    CAST(CASE WHEN x.NovPy > 0 THEN (x.NovPy - x.NovCy) * 100.0 / x.NovPy ELSE 0 END AS decimal(18, 2)) AS [Nov Percentage Difference],
    CAST(x.DecPy AS decimal(18, 4)) AS [Dec Previous Price],
    CAST(x.DecCy AS decimal(18, 4)) AS [Dec New Price],
    CAST(x.DecPy - x.DecCy AS decimal(18, 4)) AS [Dec Price Difference],
    CAST(CASE WHEN x.DecPy > 0 THEN (x.DecPy - x.DecCy) * 100.0 / x.DecPy ELSE 0 END AS decimal(18, 2)) AS [Dec Percentage Difference],
    CAST(CASE WHEN x.ItemGroup LIKE N'%Wooden%' OR x.ItemGroup LIKE N'%Melamine%' OR x.ItemGroup LIKE N'%Chipboard%' OR x.ItemGroup LIKE N'%MDF%' OR x.ItemGroup LIKE N'%Plywood%' OR x.ItemGroup LIKE N'%Fiber Sheet%' THEN N'Y' ELSE N'N' END AS nvarchar(2)) AS [Wooden Sheets],
    CAST(CASE WHEN x.ItemGroup LIKE N'%Accessories%' THEN N'Y' ELSE N'N' END AS nvarchar(2)) AS [Accessories],
    CAST(CASE WHEN x.ItemGroup LIKE N'%Appliance%' THEN N'Y' ELSE N'N' END AS nvarchar(2)) AS [Appliances],
    CAST(CASE WHEN x.ItemGroup LIKE N'%Consumable%' THEN N'Y' ELSE N'N' END AS nvarchar(2)) AS [Consumables],
    CAST(CASE WHEN x.ItemGroup LIKE N'%Silicone%' OR x.ItemGroup LIKE N'%Silicone%' THEN N'Y' ELSE N'N' END AS nvarchar(2)) AS [Silicone],
    CAST(CASE WHEN x.ItemGroup LIKE N'%Corner%' OR x.ItemGroup LIKE N'%Joint%' OR x.ItemGroup LIKE N'%Wedge%' THEN N'Y' ELSE N'N' END AS nvarchar(2)) AS [Corner Joints],
    CAST(CASE WHEN x.ItemGroup LIKE N'%Powder%' OR x.ItemGroup LIKE N'%Coating%' THEN N'Y' ELSE N'N' END AS nvarchar(2)) AS [Powder Coating],
    CAST(CASE WHEN x.ItemGroup LIKE N'%Tool%' OR x.ItemGroup LIKE N'%Machin%' THEN N'Y' ELSE N'N' END AS nvarchar(2)) AS [Tools and Machineries],
    CAST(CASE WHEN (x.ItemGroup LIKE N'%Seal%' OR x.ItemGroup LIKE N'%Gasket%') AND x.ItemGroup NOT LIKE N'%Silicone%' THEN N'Y' ELSE N'N' END AS nvarchar(2)) AS [Seals],
    CAST(CASE WHEN x.ItemGroup LIKE N'%Stone%' OR x.ItemGroup LIKE N'%Marble%' THEN N'Y' ELSE N'N' END AS nvarchar(2)) AS [Stones],
    CAST(x.iDate AS decimal(18, 0)) AS iDate
FROM (
    SELECT
        ISNULL(MAX(CASE WHEN r.IsCurr = 1 THEN r.VendorName END),
               MAX(CASE WHEN r.IsCurr = 0 THEN r.VendorName END)) AS VendorName,
        ISNULL(MAX(CASE WHEN r.IsCurr = 1 THEN r.ItemGroup END),
               MAX(CASE WHEN r.IsCurr = 0 THEN r.ItemGroup END)) AS ItemGroup,
        ISNULL(MAX(CASE WHEN r.IsCurr = 1 THEN r.ItemCode END),
               MAX(CASE WHEN r.IsCurr = 0 THEN r.ItemCode END)) AS ItemCode,
        ISNULL(MAX(CASE WHEN r.IsCurr = 1 THEN r.ItemName END),
               MAX(CASE WHEN r.IsCurr = 0 THEN r.ItemName END)) AS ItemName,
        ISNULL(MAX(CASE WHEN r.IsCurr = 1 THEN r.UnitName END),
               MAX(CASE WHEN r.IsCurr = 0 THEN r.UnitName END)) AS UnitName,
        ISNULL(MAX(CASE WHEN r.IsCurr = 1 THEN r.Department END),
               MAX(CASE WHEN r.IsCurr = 0 THEN r.Department END)) AS Department,
        MAX(r.CurrYear) AS CurrYear,
        MAX(r.CurrYear) - 1 AS PrevYear,
        ISNULL(MAX(CASE WHEN r.MonthNo = 1 AND r.IsCurr = 0 THEN r.Rate END), 0) AS JanPy,
        ISNULL(MAX(CASE WHEN r.MonthNo = 1 AND r.IsCurr = 1 THEN r.Rate END), 0) AS JanCy,
        ISNULL(MAX(CASE WHEN r.MonthNo = 2 AND r.IsCurr = 0 THEN r.Rate END), 0) AS FebPy,
        ISNULL(MAX(CASE WHEN r.MonthNo = 2 AND r.IsCurr = 1 THEN r.Rate END), 0) AS FebCy,
        ISNULL(MAX(CASE WHEN r.MonthNo = 3 AND r.IsCurr = 0 THEN r.Rate END), 0) AS MarPy,
        ISNULL(MAX(CASE WHEN r.MonthNo = 3 AND r.IsCurr = 1 THEN r.Rate END), 0) AS MarCy,
        ISNULL(MAX(CASE WHEN r.MonthNo = 4 AND r.IsCurr = 0 THEN r.Rate END), 0) AS AprPy,
        ISNULL(MAX(CASE WHEN r.MonthNo = 4 AND r.IsCurr = 1 THEN r.Rate END), 0) AS AprCy,
        ISNULL(MAX(CASE WHEN r.MonthNo = 5 AND r.IsCurr = 0 THEN r.Rate END), 0) AS MayPy,
        ISNULL(MAX(CASE WHEN r.MonthNo = 5 AND r.IsCurr = 1 THEN r.Rate END), 0) AS MayCy,
        ISNULL(MAX(CASE WHEN r.MonthNo = 6 AND r.IsCurr = 0 THEN r.Rate END), 0) AS JunPy,
        ISNULL(MAX(CASE WHEN r.MonthNo = 6 AND r.IsCurr = 1 THEN r.Rate END), 0) AS JunCy,
        ISNULL(MAX(CASE WHEN r.MonthNo = 7 AND r.IsCurr = 0 THEN r.Rate END), 0) AS JulPy,
        ISNULL(MAX(CASE WHEN r.MonthNo = 7 AND r.IsCurr = 1 THEN r.Rate END), 0) AS JulCy,
        ISNULL(MAX(CASE WHEN r.MonthNo = 8 AND r.IsCurr = 0 THEN r.Rate END), 0) AS AugPy,
        ISNULL(MAX(CASE WHEN r.MonthNo = 8 AND r.IsCurr = 1 THEN r.Rate END), 0) AS AugCy,
        ISNULL(MAX(CASE WHEN r.MonthNo = 9 AND r.IsCurr = 0 THEN r.Rate END), 0) AS SepPy,
        ISNULL(MAX(CASE WHEN r.MonthNo = 9 AND r.IsCurr = 1 THEN r.Rate END), 0) AS SepCy,
        ISNULL(MAX(CASE WHEN r.MonthNo = 10 AND r.IsCurr = 0 THEN r.Rate END), 0) AS OctPy,
        ISNULL(MAX(CASE WHEN r.MonthNo = 10 AND r.IsCurr = 1 THEN r.Rate END), 0) AS OctCy,
        ISNULL(MAX(CASE WHEN r.MonthNo = 11 AND r.IsCurr = 0 THEN r.Rate END), 0) AS NovPy,
        ISNULL(MAX(CASE WHEN r.MonthNo = 11 AND r.IsCurr = 1 THEN r.Rate END), 0) AS NovCy,
        ISNULL(MAX(CASE WHEN r.MonthNo = 12 AND r.IsCurr = 0 THEN r.Rate END), 0) AS DecPy,
        ISNULL(MAX(CASE WHEN r.MonthNo = 12 AND r.IsCurr = 1 THEN r.Rate END), 0) AS DecCy,
        ISNULL(MAX(CASE WHEN r.IsCurr = 1 THEN r.PackedDate END),
               MAX(CASE WHEN r.IsCurr = 0 THEN r.PackedDate END)) AS iDate
    FROM (
        SELECT
            l.VendorId,
            l.VendorName,
            l.ProductId,
            l.ItemGroup,
            l.ItemCode,
            l.ItemName,
            l.UnitName,
            l.Department,
            l.MonthNo,
            l.CurrYear,
            l.Rate,
            l.PackedDate,
            l.IsCurr,
            ROW_NUMBER() OVER (
                PARTITION BY l.VendorId, l.ProductId, l.MonthNo, l.IsCurr
                ORDER BY l.PackedDate DESC, l.HeaderId DESC, l.BodyId DESC
            ) AS Rn
        FROM (
            SELECT
                d.iBookNo AS VendorId,
                ISNULL(acc.sName, N'') AS VendorName,
                i.iProduct AS ProductId,
                ISNULL(NULLIF(grp.sName, N''), N'(blank)') AS ItemGroup,
                ISNULL(p.sCode, N'') AS ItemCode,
                ISNULL(p.sName, N'') AS ItemName,
                ISNULL(un.sName, N'') AS UnitName,
                ISNULL(dep.sName, N'') AS Department,
                (h.iDate / 256) % 256 AS MonthNo,
                @iEndDate / 65536 AS CurrYear,
                CAST(i.mRate AS decimal(18, 4)) AS Rate,
                h.iDate AS PackedDate,
                CASE
                    WHEN h.iDate BETWEEN @iStartDate AND @iEndDate THEN 1
                    ELSE 0
                END AS IsCurr,
                h.iHeaderId AS HeaderId,
                d.iBodyId AS BodyId
            FROM dbo.tCore_Header_0 h
            INNER JOIN dbo.tCore_Data_0 d
                ON d.iHeaderId = h.iHeaderId
               AND h.iVoucherType IN (2562, 2563)
               AND ISNULL(h.iAuth, 1) = 1
               AND ISNULL(h.bCancelled, 0) = 0
               AND ISNULL(h.bVersion, 0) = 0
               AND ISNULL(h.bSuspended, 0) = 0
               AND ISNULL(h.bDraft, 0) = 0
               AND h.iDate > 0
               AND (
                    h.iDate BETWEEN @iStartDate AND @iEndDate
                    OR h.iDate BETWEEN @iStartDate - 65536 AND @iEndDate - 65536
                   )
               AND ISNULL(d.iType, 0) = 0
               AND ISNULL(d.bVoid, 0) = 0
               AND d.iBookNo > 0
            INNER JOIN dbo.tCore_Indta_0 i
                ON i.iBodyId = d.iBodyId
               AND i.iProduct > 0
               AND i.mRate > 0
            INNER JOIN dbo.mCore_Account acc
                ON acc.iMasterId = d.iBookNo
            INNER JOIN dbo.mCore_Product p
                ON p.iMasterId = i.iProduct
            LEFT JOIN dbo.mCore_ProductTreeDetails td
                ON td.iMasterId = i.iProduct
               AND td.iTreeId = 0
            LEFT JOIN dbo.mCore_Product grp
                ON grp.iMasterId = td.iParentId
            LEFT JOIN dbo.mCore_Units un
                ON un.iMasterId = i.iUnit
            LEFT JOIN dbo.mCore_Department dep
                ON dep.iMasterId = d.iFaTag
        ) l
    ) r
    WHERE r.Rn = 1
    GROUP BY r.VendorId, r.ProductId
) x
WHERE x.iDate > 0
ORDER BY x.VendorName, x.ItemGroup, x.ItemName
