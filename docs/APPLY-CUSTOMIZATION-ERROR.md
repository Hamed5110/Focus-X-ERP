# Fix: `Apply Customization : Input string was not in a correct format`

Focus error is .NET `FormatException` (often `Decimal.Parse` / `Convert.ToDecimal`) — see [Microsoft Docs](https://learn.microsoft.com/en-us/dotnet/api/system.decimal.parse). It happens when the **layout** tries to read a column as a number and the SQL returns text for that ordinal.

## Root cause (confirmed 2026-08-29)

**Focus pairs the i-th layout column with the i-th SQL result column positionally.**

Hiding a column in the Focus customization UI **deletes the layout row but never updates the SQL**. The remaining layout rows keep their old FieldIds (gaps: `1,2,3,11..18`), while the SQL still returns all columns. After that, layout row #4 (`Total Contract Amount`, numeric) is paired with SQL column #4 (`Code`, text like `AC-3488`) → `Decimal.Parse("AC-3488")` → this error, on **every** run/apply.

Tables involved (per `Focus9 SQL Tables.csv`):

| Table | Purpose |
|---|---|
| `cCore_RDQuery_0` | Stores report designer query data (the SQL) |
| `cCore_ReportLayouts_0` | Report layouts |
| `cCore_ReportColumns_0` | Stores report columns data (layout rows) |

## Permanent fix

### 1. SQL SELECT list must exactly match visible layout columns
Same **count**, same **order**, same **types**. If you hide/show columns in the Focus UI, update the SQL to match (or re-add the layout rows).

### 2. Layout FieldIds must be sequential 1..N
No gaps. After hiding columns in the UI, renumber:

```sql
-- example: renumber layout 6904 to 1..11 by iColumnId order
UPDATE cCore_ReportColumns_0 SET iFieldId = 4 WHERE iColumnId = 107711;
```

### 3. Types that must match

| SQL column | Layout |
|---|---|
| `Sales Order Date` / `iDate` | **iType 6 Fraction** (packed integer), never Date/text |
| Amounts (`Contract`, `Adv`, `Balance`, `SJO`, `PN`, `Plan`, …) | **iType 6 Fraction** |
| Text (`Name`, `CPR/CR Number`, `Sales Module`, …) | **iType 0 Text** |

### 4. Do not return text into numeric columns

```sql
-- bad (causes Input string error)
'31/03/2026' AS [Sales Order Date]
'Yes'      AS [Advance Payment Status]

-- good
CAST(ISNULL(SODate, 0) AS decimal(18, 0)) AS [Sales Order Date]
CAST(ISNULL(Flag, 0) AS decimal(18, 2))   AS [Advance Payment Status]
```

### 5. Keep layout flags consistent
- All amount columns should use the **same** `iMiscOption` base flag (`64` in this company).
- Do **not** set `ReverseSign`/`Absolute` bits in the layout when the SQL already handles sign (e.g. `* -1`).

### 6. If error persists
- Confirm `cCore_RDQuery_0.sSqlQuery` column count = `cCore_ReportColumns_0` row count for the layout.
- Confirm FieldIds are `1..N` with no gaps.
- Confirm every layout row has `iType` matching the SQL value type at that ordinal.
- Confirm `iMiscOption` is uniform for same-type columns.
