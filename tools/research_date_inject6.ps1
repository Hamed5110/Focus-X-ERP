$cs = "Server=localhost;Database=Focus80E0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$base = [System.IO.File]::ReadAllText("C:\Users\Hamed Ali Khan\Focus-X-ERP\tools\sql_70266_saved.sql")
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()

function Test-Inject($title, $suffix) {
    $sql = $base.Trim() + "`r`n" + $suffix
    $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 180
    $cmd.CommandText = @"
SELECT COUNT(*) Cnt, COUNT(DISTINCT [Month Year]) Months, MIN([Month Year]) MinM, MAX([Month Year]) MaxM
FROM (
$sql
) r
"@
    Write-Output ""
    Write-Output "========== $title =========="
    try {
        $r = $cmd.ExecuteReader()
        while ($r.Read()) {
            Write-Output ("cnt={0} months={1} min={2} max={3}" -f $r.GetValue(0), $r.GetValue(1), $r.GetValue(2), $r.GetValue(3))
        }
        $r.Close()
    } catch {
        Write-Output ("SQL ERROR: " + $_.Exception.Message)
    }
}

Test-Inject "NO inject (current saved SQL)" ""
Test-Inject "datetime append (Date Range)" "  AND iDate >= CONVERT(datetime,'2026-08-01') AND iDate <= CONVERT(datetime,'2026-08-31') OR iDate = 0"
Test-Inject "packed append" "  AND iDate >= 132777985 AND iDate <= 132778015 OR iDate = 0"
Test-Inject "DateToInt append" "  AND iDate >= dbo.DateToInt(CONVERT(datetime,'2026-08-01')) AND iDate <= dbo.DateToInt(CONVERT(datetime,'2026-08-31')) OR iDate = 0"
Test-Inject "datetime string append" "  AND iDate >= '2026-08-01' AND iDate <= '2026-08-31' OR iDate = 0"

# Also test wrap
$wrap = @"
SELECT * FROM (
$($base.Trim())
) z
WHERE iDate >= CONVERT(datetime,'2026-08-01') AND iDate <= CONVERT(datetime,'2026-08-31') OR iDate = 0
"@
$cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 180
$cmd.CommandText = @"
SELECT COUNT(*) Cnt, COUNT(DISTINCT [Month Year]) Months, MIN([Month Year]) MinM, MAX([Month Year]) MaxM
FROM (
$wrap
) r
"@
Write-Output ""
Write-Output "========== WRAP datetime on result iDate =========="
try {
    $r = $cmd.ExecuteReader()
    while ($r.Read()) {
        Write-Output ("cnt={0} months={1} min={2} max={3}" -f $r.GetValue(0), $r.GetValue(1), $r.GetValue(2), $r.GetValue(3))
    }
    $r.Close()
} catch {
    Write-Output ("SQL ERROR: " + $_.Exception.Message)
}

# Scalar compare tests
function Scalar($title, $sql) {
    Write-Output ""
    Write-Output "========== $title =========="
    $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 30; $cmd.CommandText = $sql
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
        Write-Output ("SQL ERROR: " + $_.Exception.Message)
    }
}

Scalar "packed decimal vs datetime" @"
SELECT
  CASE WHEN CAST(132777985 AS decimal(18,0)) >= CONVERT(datetime,'2026-08-01') THEN 1 ELSE 0 END AS Gte
"@

Scalar "datetime vs packed" @"
SELECT
  CASE WHEN CONVERT(datetime,'2026-08-01') >= CAST(132777985 AS decimal(18,0)) THEN 1 ELSE 0 END AS Gte
"@

$conn.Close()
