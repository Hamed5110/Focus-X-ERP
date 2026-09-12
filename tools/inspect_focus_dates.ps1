$cs = "Server=localhost;Database=Focus80E0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
function Dump($sql, $title) {
    Write-Output ""
    Write-Output "========== $title =========="
    $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 90; $cmd.CommandText = $sql
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
        }
        $r.Close()
        if ($c -eq 0) { Write-Output "(no rows)" }
    } catch {
        Write-Output ("ERROR: " + $_.Exception.Message)
    }
}

Dump @"
SELECT name, type_desc
FROM sys.objects
WHERE name LIKE '%Date%' OR name LIKE '%date%' OR name LIKE '%Cal%'
ORDER BY type_desc, name
"@ "date-like objects"

Dump @"
SELECT ROUTINE_NAME, ROUTINE_TYPE
FROM INFORMATION_SCHEMA.ROUTINES
WHERE ROUTINE_NAME LIKE '%Date%' OR ROUTINE_NAME LIKE '%date%' OR ROUTINE_NAME LIKE '%IntTo%' OR ROUTINE_NAME LIKE '%ToInt%'
ORDER BY ROUTINE_NAME
"@ "date functions"

Dump @"
SELECT COLUMN_NAME, DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'tCore_Header_0'
  AND (COLUMN_NAME LIKE '%Date%' OR COLUMN_NAME LIKE '%Month%' OR COLUMN_NAME LIKE '%Year%' OR COLUMN_NAME IN ('iDate','iDueDate','iCreatedDate','iModifiedDate'))
ORDER BY ORDINAL_POSITION
"@ "tCore_Header_0 date columns"

Dump @"
SELECT TABLE_NAME, COLUMN_NAME, DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE COLUMN_NAME IN ('iDate','iMonth','iYear','iMonthId','sMonth','MonthYear')
   OR (COLUMN_NAME LIKE '%Month%' AND TABLE_NAME LIKE 'mCal%')
ORDER BY TABLE_NAME, COLUMN_NAME
"@ "iDate / month columns"

Dump @"
SELECT TOP 15
    h.sVoucherNo,
    h.iVoucherType,
    h.iDate AS Packed,
    h.iDate / 65536 AS Year_div,
    (h.iDate / 256) % 256 AS Month_our,
    h.iDate % 256 AS Day_our,
    ((h.iDate % 65536) / 256) AS Day_web,
    ((h.iDate % 65536) % 256) AS Month_web
FROM dbo.tCore_Header_0 h
WHERE h.iVoucherType IN (5634, 4609, 4610)
  AND ISNULL(h.bCancelled,0)=0 AND ISNULL(h.bVersion,0)=0
  AND h.iDate >= (2026*65536 + 8*256 + 1)
  AND h.iDate <= (2026*65536 + 8*256 + 31)
ORDER BY h.iDate, h.sVoucherNo
"@ "Aug 2026 unpack both formulas"

Dump @"
SELECT TOP 12
    h.sVoucherNo,
    h.iDate AS Packed,
    h.iDate / 65536 AS Y,
    (h.iDate / 256) % 256 AS M,
    h.iDate % 256 AS D
FROM dbo.tCore_Header_0 h
WHERE h.iVoucherType = 5634
  AND ISNULL(h.bCancelled,0)=0 AND ISNULL(h.bVersion,0)=0
  AND h.iDate > 0
ORDER BY h.iDate
"@ "oldest SO packed dates"

Dump @"
SELECT
    h.iDate / 65536 AS Y,
    (h.iDate / 256) % 256 AS M,
    COUNT(*) AS Cnt,
    MIN(h.iDate) AS MinPacked,
    MAX(h.iDate) AS MaxPacked,
    MIN(h.iDate % 256) AS MinDay,
    MAX(h.iDate % 256) AS MaxDay
FROM dbo.tCore_Header_0 h
WHERE h.iVoucherType IN (5634, 4609, 4610)
  AND ISNULL(h.bCancelled,0)=0 AND ISNULL(h.bVersion,0)=0
  AND h.iDate > 0
GROUP BY h.iDate / 65536, (h.iDate / 256) % 256
ORDER BY Y, M
"@ "SO/receipt month histogram"

$conn.Close()
