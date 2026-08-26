# Focus X / Focus 9 — knowledge base

Practical notes from converting **Focus cubes** to **Focus Query (SQL)** reports on company database **Focus8080** (Atlas Aluminum W.L.L.).

## Dictionary

| File | Description |
|---|---|
| [dictionary/Focus9_SQL_Tables.xlsx](dictionary/Focus9_SQL_Tables.xlsx) | Official Focus 8/9 SQL table dictionary (Excel) |
| [dictionary/Focus9_SQL_Tables.csv](dictionary/Focus9_SQL_Tables.csv) | Same dictionary as CSV |

Use the dictionary to find table purpose, identity fields, and linked tables. This folder documents **what we verified live** beyond the dictionary.

## Docs in this folder

| Doc | Topic |
|---|---|
| [FOCUS-QUERY-RULES.md](FOCUS-QUERY-RULES.md) | Focus Query / Apply Customization gotchas |
| [CORE-TABLES.md](CORE-TABLES.md) | Core transaction & master tables we use |
| [VOUCHER-TYPES-AND-AMOUNTS.md](VOUCHER-TYPES-AND-AMOUNTS.md) | Voucher type IDs, Credit vs Voucher amount |
| [PROJECT-TRACKING-III.md](PROJECT-TRACKING-III.md) | Atlas Project Tracking III algorithm |

## Environment (this company)

| Item | Value |
|---|---|
| SQL database | `Focus8080` |
| Atlas department (`iFaTag`) | `2040` |
| Aknan department | `2057` |
| Trade Receivables group code | `180` (`iMasterId` 280) |
| Focus web | `C:\inetpub\wwwroot\Focus9w`, `FocusX` |
| Report engine DLL | `Focus.RD.BL.dll` (AdvancedEngine cubes) |
| Report metadata | `cCore_Reports_0`, `cCore_RDQuery_0`, `cCore_ReportLayouts_0`, `cCore_ReportColumns_0` |
