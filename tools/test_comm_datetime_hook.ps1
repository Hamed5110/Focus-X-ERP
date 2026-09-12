$cs = "Server=localhost;Database=Focus80E0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$base = [System.IO.File]::ReadAllText("C:\Users\Hamed Ali Khan\Focus-X-ERP\reports\sql\Monthly Sales Commission.sql")
$whereHits = [regex]::Matches($base, '(?i)\bwhere\b')
Write-Output ("WHERE count = {0}" -f $whereHits.Count)
foreach ($m in $whereHits) {
    $line = ($base.Substring(0, $m.Index) -split "`n").Count
    $snip = $base.Substring($m.Index, [Math]::Min(80, $base.Length - $m.Index)) -replace "`r|`n"," "
    Write-Output ("  line {0}: {1}" -f $line, $snip)
}

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
        while ($r.Read()) {
            $parts = @()
            for ($i=0; $i -lt $n; $i++) {
                if ($r.IsDBNull($i)) { $parts += "" } else { $parts += [string]$r.GetValue($i) }
            }
            Write-Output ($parts -join " | ")
        }
        $r.Close()
    } catch {
        if ($cmd.Transaction) {}
        Write-Output ("SQL ERROR: " + $_.Exception.Message)
        try { $conn.Close(); $conn.Open() } catch {}
    }
}

$inner = $base.Trim()
Run-Sql "schema of iDate column" @"
SELECT TOP 0 * FROM (
$inner
) r
"@

# get type via dummy
$cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 180
$cmd.CommandText = "SELECT TOP 1 iDate FROM (`n$inner`n) r"
try {
    $r = $cmd.ExecuteReader()
    $dt = $r.GetFieldType(0)
    $sqlDt = $r.GetDataTypeName(0)
    Write-Output ("iDate .NET type={0} SQL type={1}" -f $dt.FullName, $sqlDt)
    $r.Close()
} catch {
    Write-Output ("type probe ERROR: " + $_.Exception.Message)
    try { $conn.Close(); $conn.Open() } catch {}
}

Run-Sql "no inject months" @"
SELECT COUNT(*) Cnt, COUNT(DISTINCT [Month Year]) Months, MIN([Month Year]) MinM, MAX([Month Year]) MaxM
FROM (
$inner
) r
"@

$dtInject = $inner + "`r`n  AND iDate >= CONVERT(datetime,'2026-08-01') AND iDate <= CONVERT(datetime,'2026-08-31') OR iDate = 0"
Run-Sql "datetime inject Aug 2026 (Date Range)" @"
SELECT Department, COUNT(*) Rows, COUNT(DISTINCT [Month Year]) Months,
       MIN([Month Year]) MinM, MAX([Month Year]) MaxM,
       CAST(SUM([Collection Amount]) AS decimal(18,2)) Coll,
       CAST(SUM([Eligible Collection]) AS decimal(18,2)) Elig,
       MAX([Team Rate %]) Rate
FROM (
$dtInject
) r
GROUP BY Department
ORDER BY Department
"@

Run-Sql "datetime inject month list" @"
SELECT DISTINCT [Month Year] FROM (
$dtInject
) r
ORDER BY 1
"@

$pkInject = $inner + "`r`n  AND iDate >= 132777985 AND iDate <= 132778015 OR iDate = 0"
Run-Sql "packed inject Aug 2026" @"
SELECT COUNT(*) Cnt, COUNT(DISTINCT [Month Year]) Months, MIN([Month Year]) MinM, MAX([Month Year]) MaxM
FROM (
$pkInject
) r
"@

Run-Sql "DATEFROMPARTS safety on receipt headers" @"
SELECT COUNT(*) BadPacked
FROM dbo.tCore_Header_0 h
WHERE h.iVoucherType IN (4608, 4609, 4610)
  AND h.iDate > 0
  AND (
       (h.iDate & 0xff00) / 256 NOT BETWEEN 1 AND 12
    OR (h.iDate & 0xff) NOT BETWEEN 1 AND 31
  )
"@

$conn.Close()
