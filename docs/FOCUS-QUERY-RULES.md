# Focus Query SQL — rules that prevent errors

Focus Query wraps your `SELECT` and maps result columns onto a saved **layout**. Most failures are type/layout issues, not bad SQL Server syntax.

## Forbidden in Focus Query SQL

| Do not use | Why |
|---|---|
| `DECLARE` / local variables | Not allowed in Query reports. Focus binds `@iStartDate` / `@iEndDate` itself |
| `ORDER BY` | Focus may wrap as a view |
| `GO` / `USE` | Multi-batch / context switch breaks the wrapper |
| `dbo.IntToDate()` returning `datetime` | Layout Date/Decimal mismatch → cast errors |
| Text dates (`dd/MM/yyyy`) | Layout expects packed Focus date **number** |
| `Yes` / `No` strings for flags | Use `0` / `1` (int or decimal) |
| `ROUND(... AS decimal(18,0))` on amounts | Can trigger **Apply Customization** format errors |

## Required patterns

### Packed Focus dates

Focus stores many dates as integers:

```text
year  = iDate / 65536
month = (iDate / 256) % 256
day   = iDate % 256
```

In Query SELECT:

```sql
CAST(ISNULL(SomeDate, 0) AS decimal(18, 0)) AS [Sales Order Date]
```

Empty date = **`0`**, not `NULL` (Focus period filter often uses `OR iDate = 0`).

Helpers (SSMS only): `dbo.IntToDate`, `dbo.DateToInt` — avoid in Focus Query SELECT lists.

### Header Date Range — custom Query SQL (verified 10 Sep 2026)

Custom Query reports (`iReportType = 1`, `iSourceType = 1`) do **not** honor the header Date Range unless the SQL uses Focus’s bound period variables. Proven on **70266 Monthly Commission**; same pattern already in **70198** and **70223**.

```sql
AND h.iDate BETWEEN @iStartDate AND @iEndDate
```

| Do | Do not |
|---|---|
| Put `BETWEEN @iStartDate AND @iEndDate` on the header join (`h.iDate` / `ht.iDate`) | `DECLARE @iStartDate` |
| Keep result `iDate` as packed `decimal(18, 0)` | `CONVERT(DATE, CAST(iDate AS VARCHAR(8)), 112)` — `iDate` is **not** YYYYMMDD |
| Keep a trailing `WHERE x.iDate > 0` | Return DateTime / `dd/MM/yyyy` as `iDate` |
| Use packed `DateToInt` (`YEAR*65536 + MONTH*256 + DAY`) | Hard-code a month / `DateToInt(GETDATE())` |
| Let Focus bind `@iStartDate` / `@iEndDate` from the header | Date-type report parameters — DateTime overflows against packed `iDate` |

Live example: 1 Sep 2026 is packed **`132778241`**, not `20260901`. Style 112 on the packed number is NULL.

`@iStartDate` / `@iEndDate` are packed integers from the header picker. YYYYMMDD bounds return 0 rows. DateTime bounds overflow.

Transaction Set alone does **not** filter custom SQL. Default cube/register reports still use a Transaction Set plus Focus appending onto the first `WHERE`:

```sql
AND iDate >= Start AND iDate <= End OR iDate = 0
```

That append is unreliable for custom SQL. Prefer `@iStartDate` / `@iEndDate` on the fact join.

```sql
CAST(h.iDate AS decimal(18, 0)) AS iDate
```

`CAST(0 AS iDate)` makes `OR iDate = 0` keep every row.

Hide `iDate` in the layout (`Miscelleneous` / misc option **66**), type **Fraction**. Use a text column such as Month Year for display.

### Amounts and flags

```sql
CAST(ISNULL(Amt, 0) AS decimal(18, 2)) AS [Total Contract Amount]
CAST(ISNULL(FlagBit, 0) AS decimal(18, 2)) AS [Comprehensive Report Atlas]  -- 0/1
ISNULL(SomeText, N'') AS [Planning month]
```

## Error: `Apply Customization : Input string was not in a correct format.`

### Cause A — layout column map out of sync (most common)

Focus maps layout `FieldId` → SQL column **by order**.

If the layout is missing early columns (e.g. Designer name) but SQL still returns them, amount columns shift onto **text** values. Then Focus does the equivalent of `Decimal.Parse("customer name")` → format error.

**Fix**

1. Layout `cCore_ReportColumns_0` FieldIds `1..N` must match SELECT column order exactly.
2. Or delete Standard layout columns and let Focus regenerate after a clean paste.
3. Never leave gaps in FieldId sequence relative to SQL ordinals.

### Cause B — Date type on packed integers

Layout `iType` values we observed:

| `iType` | Meaning |
|---|---|
| 0 | Text |
| 4 | Date |
| 6 | Fraction / number |

**Sales Order Date** and **`iDate`** must be **Fraction (`6`)**, not Date (`4`), when SQL returns packed integers like `132776735`.

### Cause C — text in a number column

Returning `'31/03/2026'` or `'Yes'` into a Fraction/Date layout column.

## Cube XML → SQL checklist

1. Read cube XML: `TransactionSets`, `DocumentOption`, filters, formulas (`c11-c18`, etc.).
2. Map Field **171 Voucher amount** → header `fNet` once per voucher (not `SUM` of body lines blindly).
3. Map Field **19 Credit** → `mAmount1 > 0` on `iCode` (and sometimes `mAmount2 > 0` on `iBookNo`).
4. Match department with `tCore_Data_0.iFaTag`.
5. Match authorization with `tCore_Header_0.iAuth` / DocumentOption bits.
6. Group the same way as the cube (`Account2.Name` may merge duplicate customer names).

## Column Sign (`-/+`)

Focus packs **Sign** into `cCore_ReportColumns_0.iAlignment` (same as cube `ColumnAlignment`):

```text
iAlignment = (sign << 5) | (horizontalAlign << 3) | decimalsInPackedBits
ColumnSign: None=0, -/+ =1, DR/CR=2, (BRACKET)=3
```

Cube **Balance Amount** uses Sign **`-/+`** → `iAlignment = 48`. Contract/Adv use **None** → `16`.  
Set this in Column Properties → **Sign** → `-/+`, or update `iAlignment` for that FieldId.

## Report metadata tables (Focus8080)

| Table | Use |
|---|---|
| `cCore_Reports_0` | Report id, name, type (`1` = Query, `2` = Cube, …) |
| `cCore_RDQuery_0` | `sSqlQuery` for Query reports |
| `cCore_ReportLayouts_0` | Layout header |
| `cCore_ReportColumns_0` | Column FieldId, `iType`, alias, decimals, misc |
| `cCore_ReportExtraValues_0` | Extra layout values (often width; not always company id) |
