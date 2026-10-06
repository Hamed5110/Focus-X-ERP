$cs = "Server=localhost;Database=Focus80G0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
function RunRange($label, $from, $to) {
    $decl = @"
DECLARE @iStartDate INT = dbo.DateToInt(CONVERT(datetime,'$from',112));
DECLARE @iEndDate INT = dbo.DateToInt(CONVERT(datetime,'$to',112));
"@
    $sql = Get-Content -Raw "C:\Users\Hamed Ali Khan\Focus-X-ERP\reports\sql\Purchase Order Last Rate Comparison.sql"
    $sql = [regex]::Replace($sql, '(?s)/\*.*?\*/', '')
    $conn = New-Object System.Data.SqlClient.SqlConnection $cs
    $conn.Open()
    $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 180
    $cmd.CommandText = $decl + $sql
    Write-Output "==== $label $from to $to ===="
    try {
        $r = $cmd.ExecuteReader()
        $names = @(); for ($i=0; $i -lt $r.FieldCount; $i++) { $names += $r.GetName($i) }
        Write-Output ("COLS: " + ($names -join " | "))
        $n=0; $both=0
        while ($r.Read()) {
            $n++
            $py = [double]$r["Last Purchase Rate Previous Year"]
            $cy = [double]$r["Last Purchase Rate Current Year"]
            if ($py -gt 0 -and $cy -gt 0) { $both++ }
            if ([string]$r["Item Name"] -like "*Sunguard Ds Grey + 12mm A/s+ 6mm Clear Float*") {
                Write-Output ("Item={0} Vendor={1} PY={2:N4} CY={3:N4} Var={4:N4} Pct={5}" -f `
                    $r["Item Name"], $r["Vendor Name"], $py, $cy, $r["Rate Variance"], $r["Comparative %"])
            }
        }
        $r.Close()
        Write-Output ("ROWS=$n BothYears=$both")
    } catch {
        Write-Output ("FAIL: " + $_.Exception.Message)
    }
    $conn.Close()
}
RunRange "CY2025" "20250101" "20251231"
RunRange "CY2026" "20260101" "20261231"
