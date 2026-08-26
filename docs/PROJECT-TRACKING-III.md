# Project Tracking III Report Atlas — algorithm

Cube: **Project Tracking III Report Atlas** (XML report id **70153**).  
Query SQL: [`../reports/sql/Project Tracking III Report Atlas.sql`](../reports/sql/Project%20Tracking%20III%20Report%20Atlas.sql)

## Business grain

One row per customer project account with **Report Status = 3** (Partial Consumed - III), department **Atlas Aluminum (2040)**.

Cube groups by **`Account2.Name`**. Duplicate masters that share the same `sName` (e.g. closed copy of the same customer) contribute amounts to the Status-3 row when rolling up by name.

## Filters

| Filter | SQL |
|---|---|
| Report Status 3 | `vaCore_Account.ReportStatus = 3`, `bGroup = 0` |
| Department Atlas | `tCore_Data_0.iFaTag = 2040` |
| Authorized docs (TR 180 overlay) | `tCore_Header_0.iAuth = 1` |
| Live docs | `bCancelled/bVersion/bSuspended/bVoid = 0`, inventory `iType = 0`, FA `bUpdateFA = 1`, `iAuthStatus < 2` |

## Measures

| Column | Logic |
|---|---|
| Total Contract Amount | Auth SO **5634** → `ABS(SUM(fNet))` once per voucher, keyed by **name** |
| Adv. Rct Amount | Sum of Credits on types 4609, 256, 4610, 4608, 8707, 4096 |
| Balance Amount | `Contract - Adv` (**signed**; overpayment is negative) |
| Sales Job Order | Auth SJO **5635** |
| Production Note Amount | Auth PN **6145** |
| Sales Job Order Balance | `Contract - SJO` |
| Plan Value | Account `PlanValue` |
| Sales Order Date | `MIN` packed `iDate` of SO vouchers (or `0`) |

## Reconciliation lessons

1. **Unauthorized SJO** (`iAuth` 0 or 4) inflate SJO if not filtered — TR 180 run behaves like DocumentOption **30** (authorized only).
2. **Same customer name, other account codes** (e.g. Closed-IV twin) still add into cube name totals.
3. Cube Excel **Grand Total footer** may not equal `SUM(rows)` (negative balances treated differently). Compare **row sums** and per-code amounts.
4. Focus Query export may show absolute Balance on rows while Grand Total stays signed — layout Absolute/Reverse setting.
5. **Apply Customization** errors after SQL edits → layout FieldId sequence must match SELECT order; keep Sales Order Date / `iDate` as **Fraction**, not Date.

## Worked examples (Focus8080)

| Symptom | Cause |
|---|---|
| SJO high on AC-5530 / AC-4974 / AC-5890 | Extra SJO with `iAuth` 4 or 0 |
| Contract/Adv/SJO high gap ~1788 on Dr. Muneer Mahdi | Same name **AC-7997** (Closed) Atlas docs rolled into **AC-6826** |
| Adv gap ~247 on Mr. Ahmed Abdulla Jaffar | Same name **AC-5251** receipt rolled into **AC-3758** |

## Layout tips (Query report)

- Hide `iDate`.
- Do not convert packed dates to text in SQL.
- After changing SELECT list width/order, rebuild layout columns or FieldIds `1..N` will mis-parse amounts as text.
