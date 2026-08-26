# Focus X ERP

Focus Softnet / Focus X knowledge base and report conversions for **ATLAS ALUMINUM W.L.L.** (database `Focus8080`).

## Quick start

1. Read **[docs/README.md](docs/README.md)** — index of everything we learned.
2. Open the official table dictionary:
   - Excel: [docs/dictionary/Focus9_SQL_Tables.xlsx](docs/dictionary/Focus9_SQL_Tables.xlsx)
   - CSV: [docs/dictionary/Focus9_SQL_Tables.csv](docs/dictionary/Focus9_SQL_Tables.csv)
3. Use SQL under `reports/sql/` inside a Focus **Query** report (not a cube Transaction Set).

## Repository layout

```text
docs/
  README.md                      Knowledge-base index
  FOCUS-QUERY-RULES.md           Focus Query + Apply Customization rules
  CORE-TABLES.md                 Masters / tCore_Header / tCore_Data map
  VOUCHER-TYPES-AND-AMOUNTS.md   Voucher IDs, Credit vs fNet patterns
  PROJECT-TRACKING-III.md        Atlas PT III algorithm + reconciliation
  dictionary/
    Focus9_SQL_Tables.xlsx       Official Focus SQL tables (Excel)
    Focus9_SQL_Tables.csv        Same as CSV
reports/
  sql/
    Project Tracking III Report Atlas.sql
    Purchase Order Job No Wise.sql
  xml/                           Cube XML exports (Atlas / Aknan I–III)
```

## Company constants (this tenant)

| Item | Value |
|---|---|
| Database | `Focus8080` |
| Atlas department `iFaTag` | `2040` |
| Aknan department `iFaTag` | `2057` |
| Trade Receivables group | code `180` |

## Focus Query essentials

- No `DECLARE`, `ORDER BY`, `GO`, `USE`.
- Dates = packed Focus integers as `decimal(18,0)`; empty = `0`.
- Keep a (usually hidden) column named **`iDate`** for period injection.
- Flags as `0`/`1`, not `Yes`/`No`.
- Layout `FieldId` order must match SELECT column order or you get:  
  **`Apply Customization : Input string was not in a correct format.`**

## License / usage

Internal Atlas reporting aids. Do not commit passwords, `.fbak` dumps, or live credentials.
