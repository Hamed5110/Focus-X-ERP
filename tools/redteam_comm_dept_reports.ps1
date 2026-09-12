$cs = "Server=localhost;Database=Focus80E0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()

function Run-SqlFile($path, $start, $end) {
    $body = Get-Content -Raw $path
    $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 180
    $cmd.CommandText = @"
DECLARE @iStartDate INT = dbo.DateToInt(CONVERT(datetime,'$start',112))
DECLARE @iEndDate INT = dbo.DateToInt(CONVERT(datetime,'$end',112))
$body
"@
    $r = $cmd.ExecuteReader()
    $rows = New-Object System.Collections.Generic.List[object]
    while ($r.Read()) {
        $o = [ordered]@{}
        for ($i=0; $i -lt $r.FieldCount; $i++) {
            $name = $r.GetName($i)
            if ($r.IsDBNull($i)) { $o[$name] = $null } else { $o[$name] = $r.GetValue($i) }
        }
        $rows.Add([pscustomobject]$o) | Out-Null
    }
    $r.Close()
    return $rows
}

function Audit($title, $rows, $expectDept, $expectMonth, $minBand, $bands) {
    Write-Output ""
    Write-Output "========== $title =========="
    $n = $rows.Count
    $depts = $rows | ForEach-Object { $_.'Department' } | Sort-Object -Unique
    $months = $rows | ForEach-Object { $_.'Month Year' } | Sort-Object -Unique
    $blankCust = @($rows | Where-Object { [string]::IsNullOrWhiteSpace($_.'Customer Name') }).Count
    $vatCust = @($rows | Where-Object { $_.'Customer Name' -eq 'Vat Output' }).Count
    $coll = 0.0; $elig = 0.0
    foreach ($x in $rows) { $coll += [double]$x.'Collection Amount'; $elig += [double]$x.'Eligible Collection' }
    $teamElig = 0.0; $rate = 0.0
    if ($n -gt 0) { $teamElig = [double]$rows[0].'Team Eligible Collection'; $rate = [double]$rows[0].'Team Rate %' }
    $teamMismatch = [Math]::Abs($elig - $teamElig)
    Write-Output ("rows={0} coll={1:N2} elig={2:N2} teamElig={3:N2} rate={4} teamDiff={5:N2}" -f $n, $coll, $elig, $teamElig, $rate, $teamMismatch)
    Write-Output ("depts=[{0}] months=[{1}] blankCust={2} vatCust={3}" -f ($depts -join ','), ($months -join ','), $blankCust, $vatCust)

    $fail = @()
    if ($depts.Count -ne 1 -or $depts[0] -ne $expectDept) { $fail += "FAIL dept expected $expectDept got $($depts -join ',')" } else { Write-Output "PASS dept" }
    if ($months.Count -ne 1 -or $months[0] -ne $expectMonth) { $fail += "FAIL month expected $expectMonth got $($months -join ',')" } else { Write-Output "PASS month" }
    if ($blankCust -gt 0) { $fail += "FAIL blank customer $blankCust" } else { Write-Output "PASS customer name filled" }
    if ($vatCust -gt 0) { $fail += "FAIL Vat Output as customer $vatCust" } else { Write-Output "PASS no VAT customer" }
    if ($teamMismatch -gt 0.05) { $fail += "FAIL team elig != sum elig" } else { Write-Output "PASS team elig = sum rows" }

    $expectRate = 0.0
    foreach ($b in $bands) {
        if ($elig -ge $b.Min) { $expectRate = $b.Pct; break }
    }
    if ([Math]::Abs($rate - $expectRate) -gt 0.001) { $fail += "FAIL rate got $rate expect $expectRate for elig $elig" } else { Write-Output "PASS rate band $expectRate" }

    $badFirst = 0
    foreach ($x in $rows) {
        $pay = [string]$x.'Payment Code'
        $e = [double]$x.'Eligible Collection'
        $c = [double]$x.'Collection Amount'
        $cv = [double]$x.'Contract Value'
        if ($pay -like '*Second Payment*' -and $e -ne $c -and $c -gt 0) { $badFirst++ }
        if (($pay -like '*First Payment*') -and $e -gt 0 -and $cv -le 0) { $badFirst++ }
    }
    if ($badFirst -gt 0) { $fail += "FAIL first/second eligibility $badFirst" } else { Write-Output "PASS first/second eligibility checks" }

    if ($fail.Count -eq 0) { Write-Output "VERDICT: TRUE" } else { $fail | ForEach-Object { Write-Output $_ }; Write-Output "VERDICT: FAIL" }
}

$atlas = "C:\Users\Hamed Ali Khan\Focus-X-ERP\reports\sql\Monthly Sales Commission - Atlas Aluminum.sql"
$aknan = "C:\Users\Hamed Ali Khan\Focus-X-ERP\reports\sql\Monthly Sales Commission - Aknan Showroom.sql"
$atlasBands = @(
    @{ Min = 300000; Pct = 1.70 },
    @{ Min = 250000; Pct = 1.50 },
    @{ Min = 200000; Pct = 1.20 },
    @{ Min = 150000; Pct = 1.00 }
)
$aknanBands = @(
    @{ Min = 80000; Pct = 1.50 },
    @{ Min = 70000; Pct = 1.20 },
    @{ Min = 50000; Pct = 1.00 }
)

try {
    $a8 = Run-SqlFile $atlas '20260801' '20260831'
    Audit "Atlas Aug 2026" $a8 "Atlas Aluminum" "August-2026" 150000 $atlasBands
} catch { Write-Output ("Atlas Aug ERROR: " + $_.Exception.Message) }

try {
    $a7 = Run-SqlFile $atlas '20260701' '20260731'
    Audit "Atlas Jul 2026" $a7 "Atlas Aluminum" "July-2026" 150000 $atlasBands
} catch { Write-Output ("Atlas Jul ERROR: " + $_.Exception.Message) }

try {
    $k8 = Run-SqlFile $aknan '20260801' '20260831'
    Audit "Aknan Aug 2026" $k8 "Aknan Showroom" "August-2026" 50000 $aknanBands
} catch { Write-Output ("Aknan Aug ERROR: " + $_.Exception.Message) }

try {
    $k7 = Run-SqlFile $aknan '20260701' '20260731'
    Audit "Aknan Jul 2026" $k7 "Aknan Showroom" "July-2026" 50000 $aknanBands
} catch { Write-Output ("Aknan Jul ERROR: " + $_.Exception.Message) }

Write-Output ""
Write-Output "========== sample Atlas Aug customers =========="
$a8 | Select-Object -First 6 | ForEach-Object {
    Write-Output ("{0} | {1} | {2} | coll={3} elig={4}" -f $_.'Salesman', $_.'Customer Name', $_.'Payment Code', $_.'Collection Amount', $_.'Eligible Collection')
}

$conn.Close()
