$cs = "Server=localhost;Database=Focus80E0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$base = [System.IO.File]::ReadAllText("C:\Users\Hamed Ali Khan\Focus-X-ERP\reports\sql\Monthly Sales Commission.sql")
$inner = $base.Trim()
$whereHits = [regex]::Matches($base, '(?i)\bwhere\b')
Write-Output ("WHERE count = {0}" -f $whereHits.Count)

$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()

function Run-Sql($title, $sql) {
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
                if ($r.IsDBNull($i)) { $parts += "" } else { $parts += [string]$r.GetValue($i) }
            }
            Write-Output ($parts -join " | ")
            if ($c -ge 15) { break }
        }
        $r.Close()
    } catch {
        Write-Output ("SQL ERROR: " + $_.Exception.Message)
        try { $conn.Close(); $conn.Open() } catch {}
    }
}

$cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 180
$cmd.CommandText = "SELECT TOP 1 iDate FROM (`n$inner`n) r"
try {
    $r = $cmd.ExecuteReader()
    Write-Output ("iDate .NET={0} SQL={1}" -f $r.GetFieldType(0).FullName, $r.GetDataTypeName(0))
    if ($r.Read()) { Write-Output ("sample iDate={0}" -f $r.GetValue(0)) }
    $r.Close()
} catch {
    Write-Output ("type probe ERROR: " + $_.Exception.Message)
    try { $conn.Close(); $conn.Open() } catch {}
}

Run-Sql "sample converted dates" @"
SELECT TOP 8 [Month Year], iDate, CAST(iDate AS date) AS AsDate
FROM (
$inner
) r
ORDER BY iDate
"@

$dtInject = $inner + "`r`n  AND iDate >= CONVERT(datetime,'2026-08-01') AND iDate <= CONVERT(datetime,'2026-08-31') OR iDate = 0"
Run-Sql "datetime inject Aug" @"
SELECT Department, COUNT(*) Rows, COUNT(DISTINCT [Month Year]) Months,
       MIN([Month Year]) MinM, MAX([Month Year]) MaxM,
       CAST(MIN(iDate) AS datetime) MinD, CAST(MAX(iDate) AS datetime) MaxD,
       CAST(SUM([Collection Amount]) AS decimal(18,2)) Coll
FROM (
$dtInject
) r
GROUP BY Department
"@

$wrap = @"
SELECT * FROM (
$inner
) z
WHERE iDate >= CONVERT(datetime,'2026-08-01') AND iDate <= CONVERT(datetime,'2026-08-31') OR iDate = 0
"@
Run-Sql "WRAP datetime inject Aug" @"
SELECT COUNT(*) Cnt, COUNT(DISTINCT [Month Year]) Months, MIN([Month Year]) MinM, MAX([Month Year]) MaxM
FROM (
$wrap
) r
"@

$dmy = @"
SET DATEFORMAT dmy;
SELECT COUNT(*) Cnt, COUNT(DISTINCT [Month Year]) Months, MIN([Month Year]) MinM, MAX([Month Year]) MaxM
FROM (
$inner
  AND iDate >= CONVERT(datetime,'20260801',112) AND iDate <= CONVERT(datetime,'20260831',112) OR iDate = 0
) r
"@
Run-Sql "dmy + style112 inject Aug" $dmy

$sep = $inner + "`r`n  AND iDate >= CONVERT(datetime,'2026-09-01') AND iDate <= CONVERT(datetime,'2026-09-30') OR iDate = 0"
Run-Sql "datetime inject Sep 1-30" @"
SELECT COUNT(*) Cnt, COUNT(DISTINCT [Month Year]) Months, MIN([Month Year]) MinM, MAX([Month Year]) MaxM
FROM (
$sep
) r
"@

$conn.Close()
