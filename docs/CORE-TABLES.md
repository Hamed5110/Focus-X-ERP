# Core Focus SQL tables (verified)

Company DB examples below are from **Focus8080**. Prefer views `va*` / `v*` when they already join language/name fields.

## Masters — accounts

| Object | Role |
|---|---|
| `mCore_Account` | Account master (`iMasterId`, `sCode`, `sName`, `bGroup`, `iStatus`) |
| `mCore_AccountTree` / `mCore_AccountTreeDetails` | Account tree (Default tree `iTreeId=0`, Projection `3`) |
| `vaCore_Account` | Account + extras + name fields (may **duplicate** rows by language/location → `GROUP BY iMasterId`) |
| `muCore_Account` / `muCore_Account_Details` | Extra account fields (PlanValue, ReportStatus, …) |

### Important `vaCore_Account` fields (Atlas PT III)

| Column | Notes |
|---|---|
| `ReportStatus` | Filter `= 3` for “Partial Consumed - III” |
| `ReportStatusName` | Display label (may contain NBSP) |
| `PlanValue` | `decimal` |
| `Planningmonth` | Text (`Nov-2025`, `--`, …) — not a date |
| `CPRCRNumber` | Text |
| `ComprehensiveReportAtlas`, `SJOprocessing` | `bit` → expose as `0`/`1` |
| `FinalMeasurementStatusName` | Use name on view; no separate `mCore_finalmeasurementstatus` in this DB |
| `SalesmannameName`, `DesignernameName`, `PipelineName`, `SiteStatusName`, `AccountControllerName` | Resolved master names |

### Account tree tip

Trade Receivables **180** is group `iMasterId = 280`. Children hang under `mCore_AccountTreeDetails.iParentId = 280`. Cube “of Trade Receivables 180” is a **drill-down context**, not always a SQL `iParentId` filter on every report.

## Masters — department

| Object | Role |
|---|---|
| `mCore_Department` | Department master |
| `tCore_Data_0.iFaTag` | Body line department tag (Atlas Aluminum = **2040**, Aknan = **2057**) |

## Transactions — header / body

| Object | Role |
|---|---|
| `tCore_Header_0` | Voucher header |
| `tCore_Data_0` | Voucher body lines |
| `tCore_Data_Tags_0` | Tag fields on body (Job Order, Main Group, …) |
| `tCore_HeaderData{VoucherType}_0` | Extra header fields for that voucher type (e.g. `2562` PO Local) |
| `tCore_Data{VoucherType}_0` | Extra body fields for that voucher type |

### `tCore_Header_0` keys

| Column | Meaning |
|---|---|
| `iHeaderId` | PK |
| `iVoucherType` | Document type id |
| `iVoucherClass` | Class (e.g. Purchase Order class `2560`) |
| `sVoucherNo` | Document number |
| `iDate` | Packed voucher date |
| `fNet` | Header net (often **negative** for inventory docs; receipts often `0`) |
| `iAuth` | Authorization: **`1` = authorized**, `0` unauthorized, `4` pending/partial |
| `bCancelled`, `bVersion`, `bSuspended`, `bDraft` | Ignore old/cancelled/suspended/draft as needed |

### `tCore_Data_0` keys

| Column | Meaning |
|---|---|
| `iBodyId` / `iHeaderId` | Line identity |
| `iBookNo` | Book / party for inventory (customer on SO/SJO/PN) |
| `iCode` | Account code for FA lines (customer on receipts) |
| `iFaTag` | Department |
| `mAmount1` / `mAmount2` | Line amounts (Credit uses positive side) |
| `bUpdateFA` | Posted to FA when `1` |
| `iAuthStatus` | Line auth; commonly `< 2` for posted |
| `iType` | Line type; use `0` for normal |
| `bVoid` | Voided line |

## Credit vs inventory party (critical)

| Document set | Customer / party lives on | Amount rule |
|---|---|---|
| Accounting / receipts (Credit field) | **`iCode`** | `CASE WHEN mAmount1 > 0 THEN mAmount1 ELSE 0 END` |
| JV / CN sometimes | **`iBookNo`** (when ≠ `iCode`) | `CASE WHEN mAmount2 > 0 THEN mAmount2 ELSE 0 END` |
| Contract / SJO / Production Note | **`iBookNo`** | Header **`fNet` once per (`party`, voucher type, `iHeaderId`)**, then `ABS(SUM(fNet))` |

Do **not** `SUM(ABS(fNet))` across lines of the same voucher (body repeats header net). Do **not** use bank `iBookNo` for Adv. Rct on normal receipts (bank has negative `mAmount2`).

## DocumentOption (cube XML)

From Focus Report Designer (2-bit fields for suspended / verification / authorization):

| Value | Typical meaning |
|---|---|
| **30** | Not suspended, all verification, **authorized only** |
| **62** | Not suspended, all verification, **authorized + unauthorized** |

Trade Receivables drill-down may overlay parent document options (e.g. auth-only) even if the child cube XML says `62`.

## Report designer metadata

| Table | Role |
|---|---|
| `cCore_Reports_0` | Reports (`iReportType`: Query≈1, Cube≈2) |
| `cCore_RDQuery_0` | Query SQL text |
| `cCore_ReportLayouts_0` | Layouts |
| `cCore_ReportColumns_0` | Layout columns (`iFieldId`, `iType`, `iDecimalInColumn`, `iMiscOption`) |
| `cCore_Vouchers_0` / `cCore_vouchers_0` | Voucher type catalogue (names) |

## Naming conventions

| Prefix | Typical meaning |
|---|---|
| `mCore_` | Master |
| `muCore_` | Master user/extra fields |
| `tCore_` | Transaction |
| `cCore_` | Config / company / report definition |
| `va` / `v` / `vm` / `vi` | Views |
| `_0` suffix | Company / partition instance |
