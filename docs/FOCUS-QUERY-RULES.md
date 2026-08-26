# Focus Query SQL — rules that prevent errors

Focus Query wraps your `SELECT` and maps result columns onto a saved **layout**. Most failures are type/layout issues, not bad SQL Server syntax.

## Forbidden in Focus Query SQL

| Do not use | Why |
|---|---|
| `DECLARE` / variables | Not allowed in Query reports |
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

### Period injection column `iDate`

Focus injects the report period onto a result column named **`iDate`**.

```sql
CAST(0 AS decimal(18, 0)) AS iDate
```

Hide `iDate` in the layout (`Miscelleneous` / misc option **66**). Do **not** put real Sales Order dates in `iDate` unless you want the period to filter by that date.

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
