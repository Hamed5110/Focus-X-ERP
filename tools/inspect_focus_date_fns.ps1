$cs = "Server=localhost;Database=Focus80E0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
function Dump($sql, $title) {
    Write-Output ""
    Write-Output "========== $title =========="
    $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 60; $cmd.CommandText = $sql
    try {
        $r = $cmd.ExecuteReader()
        $n = $r.FieldCount
        $hdr = @(); for ($i=0; $i -lt $n; $i++) { $hdr += $r.GetName($i) }
        Write-Output ($hdr -join " | ")
        $c = 0
        while ($r.Read()) {
            $c++
            $parts = @()
            for ($i=0; $i -lt $n; $i++) {
                if ($r.IsDBNull($i)) { $parts += "" } else { $parts += [string]$r.GetValue($i) }
            }
            Write-Output ($parts -join " | ")
            if ($c -ge 25) { break }
        }
        $r.Close()
        if ($c -eq 0) { Write-Output "(no rows)" }
    } catch {
        Write-Output ("ERROR: " + $_.Exception.Message)
    }
}

Dump @"
SELECT OBJECT_DEFINITION(OBJECT_ID('dbo.IntToDate')) AS Def
"@ "IntToDate definition"
Dump @"
SELECT OBJECT_DEFINITION(OBJECT_ID('dbo.DateToInt')) AS Def
"@ "DateToInt definition"
Dump @"
SELECT OBJECT_DEFINITION(OBJECT_ID('dbo.GetDateName')) AS Def
"@ "GetDateName definition"
Dump @"
SELECT OBJECT_DEFINITION(OBJECT_ID('dbo.GetDatePart')) AS Def
"@ "GetDatePart definition"
Dump @"
SELECT OBJECT_DEFINITION(OBJECT_ID('dbo.fPay_GetIntDateMonthYear')) AS Def
"@ "fPay_GetIntDateMonthYear definition"
Dump @"
SELECT OBJECT_DEFINITION(OBJECT_ID('dbo.fCore_GetStartEndDateForMonth')) AS Def
"@ "fCore_GetStartEndDateForMonth definition"
Dump @"
SELECT OBJECT_DEFINITION(OBJECT_ID('dbo.fCore_IntToDateTime')) AS Def
"@ "fCore_IntToDateTime definition"

Dump @"
SELECT TOP 8
    h.sVoucherNo,
    h.iDate AS Packed,
    dbo.IntToDate(h.iDate) AS IntToDate,
    dbo.GetDateName(h.iDate) AS GetDateName,
    dbo.DateToInt(dbo.IntToDate(h.iDate)) AS RoundTrip
FROM dbo.tCore_Header_0 h
WHERE h.iVoucherType = 5634 AND h.sVoucherNo LIKE N'CON-Atl-688%'
ORDER BY h.sVoucherNo
"@ "IntToDate vs packed on CON-Atl-688x"

Dump @"
SELECT
    dbo.DateToInt('2026-08-01') AS Aug1,
    dbo.DateToInt('2026-08-31') AS Aug31,
    (2026*65536 + 8*256 + 1) AS FormulaAug1,
    (2026*65536 + 8*256 + 31) AS FormulaAug31
"@ "DateToInt vs Y*65536+M*256+D"

Dump @"
SELECT COLUMN_NAME, DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'tCore_HeaderData5634_0'
  AND (COLUMN_NAME LIKE '%Date%' OR COLUMN_NAME LIKE '%Month%' OR COLUMN_NAME LIKE '%Year%')
ORDER BY ORDINAL_POSITION
"@ "SO extra header date fields"

Dump @"
SELECT TOP 5 * FROM dbo.mCal_Calendar
"@ "mCal_Calendar sample"

$conn.Close()
