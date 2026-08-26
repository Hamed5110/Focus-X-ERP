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
| Total Contract Amount | Auth SO **5634** → `ABS(SUM(fNet))` once per voucher, keyed by **name**, then `ROUND(..., 0)` (cube `DecimalInColumn=0`) |
| Adv. Rct Amount | Sum of Credits on types 4609, 256, 4610, 4608, 8707, 4096 with **per-type decimals**, then `ROUND(sum, 0)` |
| Balance Amount | `Contract - Adv` (**signed**; overpayment is negative) |
| Sales Job Order | Auth SJO **5635**, `ROUND(..., 0)` |
| Production Note Amount | Auth PN **6145**, `ROUND(..., 0)` |
| Sales Job Order Balance | `Contract - SJO` |
| Plan Value | Account `PlanValue` |
| Sales Order Date | `MIN` packed `iDate` of SO vouchers (or `0`) |

### Adv rounding (cube XML / ASP.NET Focus.RD)

Cube formula: `Adv = c12+…+c17` with `DecimalInColumn=0` on the formula.

Hidden Credit columns do **not** all use the same decimals:

| Voucher type | Alias | `DecimalInColumn` |
|---|---|---|
| 4609 | Credit Adv Rct | **2** |
| 256 | Credit opt | **2** |
| **4610** | CRM Adv rct | **0** |
| 4608 | Credit rct | **2** |
| 8707 | Credit jv | **2** |
| 4096 | Credit Note | **2** |

SQL (matches cube Excel row-by-row):

```sql
ROUND(SUM(
  CASE WHEN iVoucherType = 4610 THEN ROUND(TypeAmt, 0)
       ELSE ROUND(TypeAmt, 2) END
), 0)
```

Example **AC-2991**: 4609=`3500.90` + ROUND(4610=`7700.56`,0)=`7701` → `11201.90` → ROUND→`11202` (not `ROUND(11201.46)`=`11201`).

Microsoft note: SQL Server `ROUND` uses half-away-from-zero; default .NET `Math.Round` uses banker’s rounding — use T-SQL `ROUND` to match Focus cube export integers.

## Reconciliation lessons

1. **Unauthorized SJO** (`iAuth` 0 or 4) inflate SJO if not filtered — TR 180 run behaves like DocumentOption **30** (authorized only).
2. **Same customer name, other account codes** (e.g. Closed-IV twin) still add into cube name totals.
3. Cube Excel **Grand Total footer** may not equal `SUM(rows)` (e.g. Contract footer `2266050` vs row-sum `2284734`). Compare **row sums** and per-code amounts — not the cube Grand Total line.
4. Focus Query export may show absolute Balance on rows while Grand Total stays signed — layout Absolute/Reverse setting.
5. **Apply Customization** errors after SQL edits → layout FieldId sequence must match SELECT order; keep Sales Order Date / `iDate` as **Fraction**, not Date.
6. Raw 2-decimal SQL can look “wrong” next to cube Excel integers; apply cube `DecimalInColumn` rounding above.

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
- **Sign `-/+`** (Focus column property): cube **Balance Amount** uses Sign `-/+` (`ColumnAlignment = 48` = sign`1` + right`2` + dec`0`). Contract/Adv use Sign **None** (`16`). Without `-/+` on Balance, Focus may treat negatives as absolute and Grand Total / Excel export will not match the cube.
- Enum (`focusenum.js`): `None=0`, `-/+=1`, `DR/CR=2`, `(BRACKET)=3`. Packed as `(sign<<5)|(hAlign<<3)|decimals`.
