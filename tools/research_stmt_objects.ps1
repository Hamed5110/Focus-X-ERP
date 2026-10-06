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
                    if ($v.Length -gt 700) { $v = $v.Substring(0,700) + "..." }
                    $parts += $v
                }
            }
            Write-Output ($parts -join " | ")
            if ($c -ge 80) { Write-Output "(truncated)"; break }
        }
        $r.Close()
        if ($c -eq 0) { Write-Output "(no rows)" }
    } catch {
        Write-Output ("ERROR: " + $_.Exception.Message)
        try { $r.Close() } catch {}
    }
}

Dump @"
SELECT
  SUM(CASE WHEN type IN ('U') THEN 1 ELSE 0 END) AS UserTables,
  SUM(CASE WHEN type IN ('V') THEN 1 ELSE 0 END) AS Views,
  SUM(CASE WHEN type IN ('P') THEN 1 ELSE 0 END) AS Procs,
  SUM(CASE WHEN type IN ('FN','FS') THEN 1 ELSE 0 END) AS ScalarFn,
  SUM(CASE WHEN type IN ('IF','TF','FT') THEN 1 ELSE 0 END) AS TableFn,
  SUM(CASE WHEN type IN ('TR') THEN 1 ELSE 0 END) AS Triggers
FROM sys.objects
"@ "G0 object counts"

Dump @"
SELECT type, type_desc, COUNT(*) Cnt
FROM sys.objects
WHERE type IN ('U','V','P','FN','FS','IF','TF','FT','TR')
GROUP BY type, type_desc
ORDER BY type
"@ "G0 type breakdown"

Dump @"
SELECT ROUTINE_TYPE, COUNT(*) Cnt
FROM INFORMATION_SCHEMA.ROUTINES
GROUP BY ROUTINE_TYPE
"@ "SO INFORMATION_SCHEMA.ROUTINES counts"

Dump @"
SELECT o.type_desc, o.name, LEFT(ISNULL(m.definition, N''), 120) DefHead
FROM sys.objects o
LEFT JOIN sys.sql_modules m ON m.object_id = o.object_id
WHERE o.type IN ('FN','FS','IF','TF','FT')
  AND (
        o.name LIKE '%Date%'
     OR o.name LIKE '%Last%'
     OR o.name LIKE '%Sum%'
     OR o.name LIKE '%Balance%'
     OR o.name LIKE '%Run%'
     OR o.name LIKE '%Total%'
     OR o.name LIKE '%IntTo%'
     OR o.name LIKE '%GetDate%'
     OR o.name LIKE '%Statement%'
     OR o.name LIKE '%FA%'
     OR o.name LIKE '%Report%'
  )
ORDER BY o.type_desc, o.name
"@ "functions date/balance/last/report"

Dump @"
SELECT o.type_desc, o.name
FROM sys.objects o
WHERE o.type IN ('FN','FS','IF','TF','FT')
ORDER BY o.type_desc, o.name
"@ "ALL functions"

Dump @"
SELECT OBJECT_DEFINITION(OBJECT_ID('dbo.IntToDate')) AS Def
"@ "OBJECT_DEFINITION IntToDate"

Dump @"
SELECT OBJECT_DEFINITION(OBJECT_ID('dbo.DateToInt')) AS Def
"@ "OBJECT_DEFINITION DateToInt"

Dump @"
SELECT OBJECT_DEFINITION(OBJECT_ID('dbo.GetDateName')) AS Def
"@ "OBJECT_DEFINITION GetDateName"

Dump @"
SELECT OBJECT_DEFINITION(OBJECT_ID('dbo.fCore_IntToDateTime')) AS Def
"@ "OBJECT_DEFINITION fCore_IntToDateTime"

Dump @"
SELECT name FROM sys.objects
WHERE type IN ('FN','IF','TF','P') AND name LIKE 'fCore_%'
ORDER BY name
"@ "fCore functions/procs"

Dump @"
SELECT name FROM sys.objects
WHERE type IN ('FN','IF','TF','P') AND (name LIKE 'spCore_%' OR name LIKE 'pCore_%' OR name LIKE '%Statement%' OR name LIKE '%Balance%')
ORDER BY type, name
"@ "spCore / statement / balance routines"
