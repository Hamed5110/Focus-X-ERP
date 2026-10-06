$cs = "Server=localhost;Database=Focus80G0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
function Dump($sql, $title) {
    Write-Output ""
    Write-Output "========== $title =========="
    $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 180; $cmd.CommandText = $sql
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
                if ($r.IsDBNull($i)) { $parts += "NULL" } else {
                    $v = [string]$r.GetValue($i)
                    $v = $v -replace "`r|`n"," "
                    if ($v.Length -gt 900) { $v = $v.Substring(0,900) + "..." }
                    $parts += $v
                }
            }
            Write-Output ($parts -join " | ")
            if ($c -ge 50) { Write-Output "(truncated)"; break }
        }
        $r.Close()
        if ($c -eq 0) { Write-Output "(no rows)" }
    } catch {
        Write-Output ("ERROR: " + $_.Exception.Message)
        try { $r.Close() } catch {}
    }
}

Dump @"
SELECT COLUMN_NAME, DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'vCore_AccountBalances_0'
ORDER BY ORDINAL_POSITION
"@ "vCore_AccountBalances_0 columns"

Dump @"
SELECT COLUMN_NAME, DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'vtAccount_DrCr_0'
ORDER BY ORDINAL_POSITION
"@ "vtAccount_DrCr_0 columns"

Dump @"
SELECT COLUMN_NAME, DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'vtCore_DrCr_0'
ORDER BY ORDINAL_POSITION
"@ "vtCore_DrCr_0 columns"

Dump @"
SELECT o.name, o.type_desc
FROM sys.objects o
WHERE o.type IN ('FN','IF','TF','P')
  AND o.name IN ('Last','LAST','LastValue','RunningValue','fn_Last','fCore_Last')
"@ "Last/RunningValue function exists?"

Dump @"
SELECT COUNT(*) AS ViewsWithIntToDate
FROM sys.sql_modules m
JOIN sys.views v ON v.object_id = m.object_id
WHERE m.definition LIKE '%IntToDate%'
"@ "views using IntToDate"

Dump @"
SELECT COUNT(*) AS ViewsWithStyle112
FROM sys.sql_modules m
JOIN sys.views v ON v.object_id = m.object_id
WHERE m.definition LIKE '%, 112)%' OR m.definition LIKE '%,112)%'
"@ "views using CONVERT style 112"

Dump @"
SELECT COUNT(*) AS ModulesWithLastParen
FROM sys.sql_modules m
JOIN sys.objects o ON o.object_id = m.object_id
WHERE m.definition LIKE '%LAST_VALUE%'
   OR m.definition LIKE '%Last(%'
   OR m.definition LIKE '%SUM(%OVER%'
"@ "modules LAST_VALUE / Last( / SUM OVER"

Dump @"
SELECT o.type_desc, o.name
FROM sys.sql_modules m
JOIN sys.objects o ON o.object_id = m.object_id
WHERE m.definition LIKE '%LAST_VALUE%'
   OR m.definition LIKE '%SUM(%OVER%'
ORDER BY o.type_desc, o.name
"@ "objects with LAST_VALUE or SUM OVER"

Dump @"
SELECT OBJECT_DEFINITION(OBJECT_ID('dbo.GetDatePart')) AS Def
"@ "GetDatePart def"

Dump @"
SELECT OBJECT_DEFINITION(OBJECT_ID('dbo.IntToGregDate')) AS Def
"@ "IntToGregDate def"

Dump @"
SELECT
  dbo.GetDateName(N'm', 132778241) AS MonthName,
  dbo.GetDateName(N'y', 132778241) AS YearName,
  dbo.GetDatePart(N'y', 132778241) AS YearPart,
  dbo.GetDatePart(N'm', 132778241) AS MonthPart,
  dbo.GetDatePart(N'd', 132778241) AS DayPart
"@ "TEST GetDateName GetDatePart 1 Sep 2026"
