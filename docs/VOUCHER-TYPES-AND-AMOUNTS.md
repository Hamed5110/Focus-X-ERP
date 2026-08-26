# Voucher types and amount fields

IDs below are from **Focus8080** (Atlas). Confirm with `cCore_Vouchers_0` in other companies.

## Project Tracking III — documents

| Voucher type | Name (approx.) | Cube field | SQL amount |
|---|---|---|---|
| **5634** | Contract / Sales Order | Voucher amount (171) | `ABS(SUM(fNet))` once per voucher on `iBookNo` |
| **5635** | Sales Job Order | Voucher amount (171) | same |
| **6145** | Production Note | Voucher amount (171) | same |
| **4609** | Advance Receipts | Credit (19) | `mAmount1 > 0` on `iCode` |
| **256** | Opening Balances | Credit (19) | same |
| **4610** | Advance Receipts CRM | Credit (19) | same |
| **4608** | Receipts | Credit (19) | same |
| **8707** | Journal Entries Receipts Transfer | Credit (19) | `mAmount1` on `iCode` and/or `mAmount2` on `iBookNo` |
| **4096** | Credit Notes | Credit (19) | same as JV pattern |

### Cube formulas (PT III Atlas XML)

```text
Adv. Rct Amount = ROUND(
  ROUND(Credit4609,2) + ROUND(Credit256,2) + ROUND(Credit4610,0)
  + ROUND(Credit4608,2) + ROUND(Credit8707,2) + ROUND(Credit4096,2)
, 0)

Cube XML sets DecimalInColumn=0 only on CRM Adv (4610); other credit splits use 2 decimals; Adv formula uses 0.
Balance Amount  = Total Contract Amount - Adv. Rct Amount   -- signed
SJO Balance     = Total Contract Amount - Sales Job Order
Plan Value      = SUM(PlanValue) / COUNT(PlanValue) on cube; Query usually uses master PlanValue
```

Department filter: **`iFaTag = 2040`** (Atlas Aluminum).  
Account filter: **`ReportStatus = 3`**.

## Purchase Order Job No. wise (reference)

| Item | Value |
|---|---|
| Cube | Report ~70054 |
| PO Local voucher type | **2562** |
| Voucher class (all PO) | **2560** (if reporting whole class) |
| Extra header | `tCore_HeaderData2562_0` |
| Extra body | `tCore_Data2562_0` |
| Job Order tag | `tCore_Data_Tags_0.iTag3010` |

## Inventory aggregation pattern

```sql
SELECT
    AccId,
    ABS(SUM(CASE WHEN iVoucherType = 5634 THEN VoucherAmt ELSE 0 END)) AS ContractAmt,
    ABS(SUM(CASE WHEN iVoucherType = 5635 THEN VoucherAmt ELSE 0 END)) AS SjoAmt,
    ABS(SUM(CASE WHEN iVoucherType = 6145 THEN VoucherAmt ELSE 0 END)) AS ProdNoteAmt
FROM (
    SELECT
        d.iBookNo AS AccId,          -- or a.sName when grouping by name
        h.iVoucherType,
        h.iHeaderId,
        MAX(h.fNet) AS VoucherAmt    -- once per voucher
    FROM tCore_Header_0 h
    JOIN tCore_Data_0 d ON d.iHeaderId = h.iHeaderId
    WHERE h.iVoucherType IN (5634, 5635, 6145)
      AND d.iFaTag = 2040
      AND ISNULL(d.iType, 0) = 0
      AND ISNULL(h.iAuth, 1) = 1     -- when DocumentOption / TR overlay = authorized only
      AND ISNULL(h.bCancelled, 0) = 0
      AND ISNULL(h.bVersion, 0) = 0
      AND ISNULL(h.bSuspended, 0) = 0
      AND ISNULL(d.bVoid, 0) = 0
    GROUP BY d.iBookNo, h.iVoucherType, h.iHeaderId
) x
GROUP BY AccId
```

## FA / Adv. Rct pattern

```sql
-- customer credit lines
SELECT d.iCode AS AccId,
       CASE WHEN d.mAmount1 > 0 THEN d.mAmount1 ELSE 0 END AS CreditAmt
FROM tCore_Header_0 h
JOIN tCore_Data_0 d ON d.iHeaderId = h.iHeaderId
WHERE h.iVoucherType IN (256, 4096, 4608, 4609, 4610, 8707)
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

UNION ALL

-- JV / CN style book credits (avoid double-counting receipt bank lines)
SELECT d.iBookNo,
       CASE WHEN d.mAmount2 > 0 THEN d.mAmount2 ELSE 0 END
FROM tCore_Header_0 h
JOIN tCore_Data_0 d ON d.iHeaderId = h.iHeaderId
WHERE h.iVoucherType IN (256, 4096, 4608, 4609, 4610, 8707)
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
```

## Engine notes (`Focus.RD.BL.dll`)

- Credit: `mAmount1 > 0` → `iCode`; `mAmount2 > 0` → `iBookNo`.
- Typical receipt: customer `iCode` + positive `mAmount1`; bank `iBookNo` + negative `mAmount2`; header `fNet = 0`.
- Inventory party: **`iBookNo`**.
- Ignore versions: `bVersion = 0`, `bSuspended = 0`, `bCancelled = 0`, `bVoid = 0`.
