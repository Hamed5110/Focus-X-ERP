$cs = "Server=localhost;Database=Focus80E0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 90
function Dump($sql, $title) {
    Write-Output ""
    Write-Output "========== $title =========="
    $cmd.CommandText = $sql
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
                if ($r.IsDBNull($i)) { $parts += "" } else {
                    $v = [string]$r.GetValue($i)
                    $v = $v -replace "`r|`n"," "
                    if ($v.Length -gt 350) { $v = $v.Substring(0,350) + "..." }
                    $parts += $v
                }
            }
            Write-Output ($parts -join " | ")
            if ($c -ge 60) { Write-Output "...truncated..."; break }
        }
        $r.Close()
        if ($c -eq 0) { Write-Output "(no rows)" }
    } catch {
        Write-Output ("ERROR: " + $_.Exception.Message)
        try { $conn.Close(); $conn.Open(); $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 90 } catch {}
    }
}

Dump @"
SELECT o.type_desc, o.name
FROM sys.objects o
WHERE o.type IN ('V','P','FN','TF','IF')
  AND (
    o.name LIKE '%IntToDate%' OR o.name LIKE '%DateToInt%' OR o.name LIKE '%GetDate%'
    OR o.name LIKE '%iDate%' OR o.name LIKE '%ConvertDate%' OR o.name LIKE '%ToDate%'
    OR o.name LIKE '%DateTime%' OR o.name LIKE '%Packed%'
  )
ORDER BY o.type_desc, o.name
"@ "date-related objects"

Dump @"
SELECT TOP 40 o.type_desc, o.name
FROM sys.objects o
WHERE o.type = 'V'
  AND (
    OBJECT_DEFINITION(o.object_id) LIKE '%IntToDate%'
    OR OBJECT_DEFINITION(o.object_id) LIKE '%DateToInt%'
    OR OBJECT_DEFINITION(o.object_id) LIKE '%0xfff0000%'
    OR OBJECT_DEFINITION(o.object_id) LIKE '%65536%'
  )
ORDER BY o.name
"@ "views that convert packed dates"

Dump @"
SELECT TOP 40 o.type_desc, o.name
FROM sys.objects o
WHERE o.type IN ('P','FN','TF','IF')
  AND (
    OBJECT_DEFINITION(o.object_id) LIKE '%IntToDate%'
    OR OBJECT_DEFINITION(o.object_id) LIKE '%DateToInt(%'
  )
ORDER BY o.type_desc, o.name
"@ "procs/fns using IntToDate/DateToInt"
