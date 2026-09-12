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

**Report columns (29 Aug 2026, user request): only 5 — Name, Total Contract Amount, Adv. Rct Amount, Balance Amount, Plan Value.** Layout 6904 trimmed to match positionally (FieldIds 1..5).

| Column | Cube XML formula | Logic |
|---|---|---|
| Total Contract Amount | c11 = Voucher amount | Auth SO **5634** → once per voucher `MAX(fNet)`, keyed by **name**, then **signed** `ROUND(SUM(fNet) * -1, 0)` (minus-dominated accounts show minus) |
| Adv. Rct Amount | c18 = `c12+…+c17` | Sum of Credits on types 4609, 256, 4610, 4608, 8707, 4096 with **per-type decimals**, then `ROUND(sum, 0)` |
| Balance Amount | c19 = `c11-c18` | `Contract - Adv` with the **signed** Contract (user rule: Balance = displayed Contract − displayed Adv) |
| Plan Value | c26 = `c24/c25` | Account `PlanValue` |

(Dropped columns: Sales Order Date, CPR/CR Number, Sales Job Order, Production Note Amount, Sales Module, Sales Job Order Balance.)

### Voucher amount sign rule (verified 29 Aug 2026)

Cube "Voucher amount" engine value = **`ABS(SUM(fNet))` at account level** — mixed-sign SO vouchers **net first, then ABS**.

Proof (Focus8080): Abdulrahman Abdullah SO lines net to `+205.07` → cube `205` (`SUM(ABS)` would be `3656.69`). Sq-atl-939 nets `+7458.29` → cube `7458`. Normal accounts net negative → ABS = same positive value.

Do **not** use `SUM(ABS(fNet))` (overcounts mixed-sign accounts).

**Display convention (user request, 29 Aug 2026):** the report's **Total Contract Amount** column shows the **signed** net (`SUM(fNet) * -1`) so minus-dominated accounts display with a minus, matching how Focus shows those vouchers. Only 4 accounts are minus-dominated: Abdulrahman Abdullah (-205), Mr. Ahmed Darraj (-464), Sq-atl-924 (-1,214), Sq-atl-939 (-7,458). **Balance Amount = signed Contract − Adv** (user's simple rule), so on those 4 accounts Balance also differs from the cube by design (e.g. Sq-atl-939: -7,458 − 8,999 = -16,457 vs cube -1,541). Note: the cube Excel export shows these 4 contracts as positive — the sign difference on those 4 rows is intended.

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

## Summary report — III Summary (report 70257, layout 6906)

Query SQL: [`../reports/sql/III Summary Report.sql`](../reports/sql/III%20Summary%20Report.sql)

**Totals per Report Status** built by wrapping the detail query (70256) and grouping — guaranteed to stay in sync with the detail engine. Statuses **1 Pending - I**, **2 In Progress - II**, **3 Partial Consumed - III** (`ReportStatus IN (1,2,3)`); department Atlas Aluminum (**iFaTag = 2040**) as in the detail query.

| Column | Value |
|---|---|
| Report Status | `mCore_reportstatus.sName` per status id |
| Total Contract Amount | `SUM` of detail rows per status |
| Adv. Rct Amount | `SUM` of detail rows per status |
| Balance Amount | `SUM` of detail rows per status (= Total Contract − Adv. Rct exactly) |
| Plan Value | `SUM` of detail rows per status |
| No. of Accounts | `COUNT(*)` of detail rows per status |

**Cube row-inclusion rule (verified 29 Aug 2026 vs PT I/II/III cube exports):** the cube builds rows from its transaction sets, so an account appears **only when its OWN master has ≥ 1 direct Atlas (2040) authorized live document** (SO 5634 or credit types 256/4096/4608/4609/4610/8707). Accounts with no direct activity — or with amounts only via a same-name twin — get no cube row. The summary applies the same rule (INNER JOIN to the activity set).

Note: the "Trade Receivables group" hypothesis was tested and is a **no-op** — all status-1/2/3 accounts already sit under `Trade Receivables` (iMasterId **280**) in the main account tree (`mCore_AccountTreeDetails`, iTreeId 0). The real filter is activity, not group.

Verified 29 Aug 2026 (live report 70257 vs cube exports 362696 / 314796 / 60810):

| Report Status | Contract | Adv | Balance | Plan Value | Accounts | Cube rows |
|---|---|---|---|---|---|---|
| 1 Pending - I | 928,352 | 362,809 | 565,543 | 640,827.76 | 91 | **exact match (91)** |
| 2 In Progress - II | 876,351 | 462,476 | 413,875 | 576,047.09 | 101 | **exact match** after the as-on-date rule (Adv 462,476 = cube II) |
| 3 Partial Consumed - III | 2,266,052 | 2,180,290 | 85,762 | 138,772.97 | 223 | 222 snapshot at 11:38 — stale |

Residual status-3 differences vs the 11:38 snapshot are **intra-day churn**, not logic errors: 4 accounts changed status 3→1/2 between the 11:38 and 12:22/12:24 exports (AC-2071 → PT I; AC-6277/7074/7287 → PT II — confirmed present in those exports), 12 accounts had SO/receipts entered or authorized after 11:38 (e.g. AC-6631 CON-Atl-6672/6872, AC-5657 CN-2-00435), and 5 active accounts (AC-3758, AC-506, AC-5850, AC-6584, AC-6698) were simply not in that TR session's grid (lesson 7). A fresh cube run matches the SQL.

## Reconciliation lessons

1. **Unauthorized SJO** (`iAuth` 0 or 4) inflate SJO if not filtered — TR 180 run behaves like DocumentOption **30** (authorized only).
2. **Same customer name, other account codes** (e.g. Closed-IV twin **AC-7997** Status 4, or Status 0 twins) still add into cube name totals when the Status-3 row is shown.
3. Cube Excel **Grand Total footer** may not equal `SUM(rows)` (e.g. Contract footer often **~18,684 low**). Compare **row sums** and per-code amounts — not the cube Grand Total line.
4. Focus Query export may show absolute Balance on rows while Grand Total stays signed — layout Absolute / Sign `-/+` setting.
5. **Apply Customization** errors after SQL edits → layout FieldId sequence must match SELECT order; keep Sales Order Date / `iDate` as **Fraction**, not Date.
6. Raw 2-decimal SQL can look “wrong” next to cube Excel integers; apply cube `DecimalInColumn` rounding above.
7. **Trade Receivables overlay (critical):** filename `Trade_Receivables_180_*_Project_Tracking_III_*` means the cube was opened **from Trade Receivables**. That run only lists accounts in that TR session. The SQL Query lists **all** Report Status = 3 accounts.  
   - Latest check (26 Aug 2026): **222 common codes → Contract/Adv diffs = 0**; SQL-only **AC-3488** (+7673) and **AC-5850** (+7597) = entire total gap **15,270**.  
   - Same codes drop in/out of different TR exports (e.g. `225784` had AC-3488; `527369` had neither).  
   - To compare amounts: match on **Code**, ignore cube Grand Total, and either accept SQL’s full Status-3 set or restrict SQL to the TR account list for that run.
8. **As-on-date rule (verified 30 Aug 2026):** the PT I/II/III cube exports are run with `[As on date <today>]` and **exclude documents dated after that date**. Focus packs dates as `iDate = YEAR*65536 + MONTH*256 + DAY`; the run-time filter `h.iDate <= (YEAR(GETDATE())*65536) + (MONTH(GETDATE())*256) + DAY(GETDATE())` reproduces the cut-off in the SQL report. Proof: receipt **ATIC-26-1247** (Mr. Ahmed Hammad, 301.55, dated **30/10/2026** — two months in the future) is excluded by the cube II export (Adv 451,460) but included by the unfiltered **Comprehensive Project Tracking Report** (Adv 451,762) — the **302** gap is exactly that receipt. The Comprehensive report ignores its own as-on date for the Adv columns, so it will always sit slightly above the PT I/II/III exports by the total of future-dated documents (30 Aug 2026: II +302 Adv; III +586 Adv / +803 Contract). The SQL summary follows the PT I/II/III cube semantics.
9. **Stale result sets / caching:** Focus X can keep showing a previous run's numbers after a query is updated in the designer. Symptom seen 30 Aug 2026: a reconciliation workbook showed "As per report" Adv II = 451,762 (pre-fix value) although report **70259** already had the date filter deployed (verified via `RD/RD/LoadRDDefinitionValue` — 24 `GETDATE()` occurrences = all 8 filter lines). A **fresh run** (open the report, Ok on the date bar) at 10:48 returned the corrected values. Always re-run the report after pasting a new query; never compare against an export made before the paste.

**Cloud verification 30 Aug 2026 10:48 (report 70259, fresh run, as-on 30/08) vs same-morning cube exports (145256 / 264376 / 643162):**

| Report Status | SQL Contract | Cube Contract | SQL Adv | Cube Adv | SQL Accts | Cube Accts | Result |
|---|---|---|---|---|---|---|---|
| 1 Pending - I | 974,812 | 974,812 | 366,651 | 366,651 | 96 | 96 | **exact** |
| 2 In Progress - II | 873,650 | 873,650 | 451,460 | 451,460 | 100 | 100 | **exact** (302 future-dated receipt correctly excluded) |
| 3 Partial Consumed - III | 2,242,813 | 2,267,819 | 2,150,745 | 2,177,152 | 219 | 223 | intra-day churn between the 09:00 cube export and the 10:48 SQL run (~4 accounts / ~26 K edited on the cloud); a same-minute cube III export matches |

### Lesson 10 - Prove churn with an in-SQL diff (no scripts on the cloud)

When a stale cube export and a fresh SQL run disagree, do not guess - embed the
cube export rows as a `VALUES` table inside a diagnostic query and `FULL OUTER
JOIN` it against the live detail rows, so the diff is computed **on the cloud
server itself** (runtime-only, no scripts, no SQL login). Run it by temporarily
swapping the query into the report via `LoadRDDefinitionValue` /
`SaveRDDefinitionValue`, run the report, then restore the original query.

30 Aug 2026 - cube III export 145256 (09:00, 223 rows) embedded as VALUES vs the
live cloud detail (11:05). Every fils of the summary gap explained by 7 accounts:

| Account | Cube III 09:00 | Live SQL 11:05 | Root cause (verified on cloud) |
|---|---|---|---|
| AC-3297 Mrs. Fatima Jawad Ahmed | 5,803 / 5,294 | - | ReportStatus flipped 3 -> **4 (Closed)** after 09:00 |
| AC-4115 Mohamed Yousif Hassan | 9,030 / 9,030 | - | ReportStatus flipped 3 -> **4** after 09:00 |
| AC-6834 Mr. Isa Abdullah Isa Marzooq | 9,017 / 9,069 | - | ReportStatus flipped 3 -> **4** after 09:00 |
| AC-8070 Mr. Hussain Faisal Ali | 3,599 / 3,600 | - | ReportStatus flipped 3 -> **4** after 09:00 |
| AC-3758 Mr. Ahmed Abdulla Jaffar | 16,885 / 17,185 | - | twin AC-5251 (status 0) became contributing -> name-group status 0 -> hidden from III |
| AC-6826 Dr. Muneer Mahdi | - | 17,688 / 17,771 | twin AC-7997 flipped 0 -> **4** -> name-group status 0 -> 3 -> **entered** III (was in no 09:00 cube) |
| AC-4451 Buthayna Ahmed Alsadiq | 27,281 / 27,282 | 28,921 / 27,282 | SO edited/added +1,640 (activity rows 201 -> 203) |

Net: -44,334 + 17,688 + 1,640 = **-25,006 Contract** and -44,178 + 17,771 =
**-26,407 Adv**, -5 + 1 = **-4 accounts** - exactly the summary gap. The query
is correct; the cube exports are point-in-time snapshots. A cube III export
re-run at the same minute as the SQL report matches it.

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
- **Reverse Sign / absolute display**: SQL produces final signs directly — Contract = **signed** `SUM(fNet) * -1` (minus-dominated accounts show minus), SJO/PN = `ABS(SUM(fNet))`, Balance = `ContractAbs - Adv` (signed), SJO Balance = `ABS(ContractAbs - SJO)`. Layout: amount columns `iAlignment = 18`, **Balance Amount `iAlignment = 48`** (`-/+`) so overpaid accounts export as negative like the cube. Do not enable Reverse Sign in the layout.
- Verified 29 Aug 2026 (export 487465 vs live SQL): **223/223 accounts, 7/7 measures, 0 mismatches**.
