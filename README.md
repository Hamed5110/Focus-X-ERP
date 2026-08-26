# Focus X ERP

Focus Softnet / Focus X report conversions for **ATLAS ALUMINUM W.L.L.**

## Contents

| Path | Description |
|---|---|
| `reports/sql/Project Tracking III Report Atlas.sql` | Focus Query SQL matching cube **Project Tracking III Report Atlas** (Atlas Aluminum, Report Status = 3) |
| `reports/sql/Purchase Order Job No Wise.sql` | Purchase Order Job No. wise query |
| `reports/xml/` | Focus cube XML exports (Atlas / Aknan Project Tracking I–III) |

## Database

- Company DB: `Focus8080`
- Department filter (Atlas): `iFaTag = 2040`

## Notes

- Focus Query: no `DECLARE` / `ORDER BY` / `GO` / `USE`
- Dates: packed Focus integers (`decimal`), empty = `0`
- Flags: numeric `0`/`1` (not Yes/No text)
- Keep a column named `iDate` (often hidden) for period injection
