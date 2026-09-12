# Monthly Sales Team Commission (Focus Query)

Company database: **Focus80E0** (latest live restore).  
Query files:
- Combined: `reports/sql/Monthly Sales Commission.sql` (report 70266)
- Atlas only: `reports/sql/Monthly Sales Commission - Atlas Aluminum.sql` (**new** Query)
- Atlas detail (V&V): `reports/sql/Monthly Sales Commission - Atlas Aluminum Detail.sql` (**new** Query, customer grain)
- Aknan only: `reports/sql/Monthly Sales Commission - Aknan Showroom.sql` (**new** Query)

Create the two department reports as **new** Query reports (do not overwrite 70266). Atlas is **one row per salesman**. Aknan / combined still include Customer Name. `iDate` stays packed Fraction, hidden.
Default FA view `vtCode_DataFA_0` keeps `iDate` as packed `tCore_Header_0.iDate`. Custom Query date filter (verified 10 Sep 2026): `h.iDate BETWEEN @iStartDate AND @iEndDate` — Focus binds those from the header Date Range (same as 70198 / 70223). Do not `DECLARE`, do not convert result `iDate`, do not hard-code a month. Result `iDate` stays packed Fraction, hidden. **Month Year** is the readable date. See `docs/FOCUS-QUERY-RULES.md`.
**Atlas Aluminum** uses only CI-001 / CI-004. The 50% test is the **per-contract gate**. The tier is on **department Overall Sales** (unique **qualified** contract amounts in the period). Payout is **qualified contract amount × team rate**, shown per salesman. **Collection Eligible Amount** is period first-pay cash on those same qualified contracts (NetSuite-style collections column; not the payout base).

**Aknan** and the combined file still use the older team rules (second pay auto-eligible; rate on department month total):

| Code | Name | Atlas | Aknan / combined |
|---|---|---|---|
| 1 | CI - 001 First Payment Bahrain | In scope; contract ≥ 50% | Contract ≥ 50% |
| 4 | CI - 004 First Payment KSA | In scope; contract ≥ 50% | Contract ≥ 50% |
| 2 / 5 | Second Payment BH / KSA | **Excluded** | Always eligible |
| 3 / 6 / 7 / 8 | Additional / scrap / maintenance | **Excluded** | Shown, not eligible |

| Atlas eligible collection | Rate | Aknan eligible collection | Rate |
|---|---|---|---|
| 150,000 to < 200,000 | 1.0% | 50,000 to < 70,000 | 1.0% |
| 200,000 to < 250,000 | 1.2% | 70,000 to < 80,000 | 1.2% |
| 250,000 to < 300,000 | 1.5% | 80,000 to < 100,000 | 1.5% |
| 300,000 and above | 1.7% | above 100,000 | 1.5% (same rate) |

Below the first band (Atlas < 150,000 / Aknan < 50,000) the rate is **0%**.

## Atlas rules vs export `Monthly_Commission_Atlas901133` (12 Sep 2026)

The export is the **old** query + grouping Sum. It is not the Atlas rule set.

| Atlas rule | Export | New Atlas SQL |
|---|---|---|
| Only CI-001 / CI-004 | All codes (002, 008, …) | First pay only |
| 50% per contract | Same idea, but second pay auto-eligible | Lifetime CI-001/CI-004 on that contract through `@iEndDate` ≥ 50% |
| Tier on **that salesman’s** qualified total in the period | Department team total + equal Share Each | Per salesman; no team split |
| < 150,000 qualified → 0% | Team 115,783 → 0% (right rate, wrong base) | Every Aug salesman < 150k → **0%** |
| Inclusive lower / exclusive upper | `>=` cascade (same) | Same |

Export layout Sum still inflates: Team Eligible **12,967,715** = 115,783.17 × 112 rows; Members **672** = 6 × 112.

### Atlas live check (Overall Sales = 50%-qualified contracts only)

| Month | All contracts | Eligible (in Overall) | Not Eligible | Collection | Collection Eligible | Rate |
|---|---:|---:|---:|---:|---:|---|
| Jul 2026 | 217,496.11 | 113,497.78 | 103,998.33 | 97,062.47 | (see query) | **0%** (< 150k) |
| Aug 2026 | 114,011.35 | 93,643.80 | 20,367.55 | 55,481.17 | **47,859.56** | **0%** (< 150k) |

Old (wrong for Atlas) Aug all-codes: collection 132,205.76, eligible 115,783.18.

Last column `iDate` is packed `dbo.DateToInt` — hide it, type **Fraction**. Do not convert it to text or DateTime.

## Atlas detail report (V&V)

New Query: `reports/sql/Monthly Sales Commission - Atlas Aluminum Detail.sql`. Do **not** overwrite the salesman summary.

One row per **salesman + customer + contract**. Same CI-001/004 + 50% gate. Extra audit columns: Lifetime Collection, Collection %, Gate Status (Yes/No).

August 2026 true-mode: 25 customer rows (19 Yes / 6 No). Sum by salesman matches the summary (within 0.02). Overall Sales **93,643.80**. Cash customer 121.00 has no contract value — sits in Total Collection only.

## Layout (Atlas)

Atlas is a **salesman payout** list (Salesforce / CaptivateIQ grain), not a receipt register.

Columns: Department, Month Year, Salesman, Total Contract Amount, Total Collection, **Collection Eligible Amount**, Eligible Amount, Not Eligible Amount, Overall Sales (Rn=1), Commission Ratio (Rn=1), Total Commission Amount, hidden `iDate`.

Delete the old receipt columns (Customer Name, Payment Code, Team *, Share Each). Rebuild FieldIds. **Do not add row groups.** Do not Sum Commission Ratio. Hide `iDate` (Fraction).

The last grid that still showed Share Each **1,327,778,017** had mapped Share Each to packed `iDate`. Collection **184,143** was grouping Sum — true Aug collection is **55,481.17**.

## What the report does

One row per receipt (layout groups **Department → Month Year → Salesman**).
Atlas Aluminum (`iFaTag` 2040) and Aknan Showroom (`iFaTag` 2057) are two independent teams.

| Input | Focus object |
|---|---|
| Collection | Receipts **4608**, Advance Receipts **4609**, Advance Receipts CRM **4610** |
| Payment Code | Extra field `PaymentCode` (dropdown 1–8 on the receipt header) |
| Contract value | `tCore_HeaderData4610_0.TotalContractAmt` (CRM receipt) |
| Salesman | Account extra `vaCore_Account.Salesmanname` → `mCore_Salesman` |

Atlas: rate and commission are per salesman (no equal split). Aknan / combined: team commission is still split equally.

HR 20 working days / disciplinary rules are **not** auto-applied.

## August 2026 check on Focus80E0

| Department | Collection | Collection Eligible | Eligible (contracts) | Rate |
|---|---|---|---|---|
| Atlas Aluminum (CI-001/004 only) | 55,481.17 | 47,859.56 | 93,643.80 | 0% (Overall < 150k) |
| Aknan Showroom (old team rules) | 58,149.12 | 43,596.86 | 0% (below 50k) |

## Not in this query yet

Designer department (Document 2): 3,000 BHD target + 60,000 approved files. Need the Focus screen that stores approved manufacturing files before that section can be added.
