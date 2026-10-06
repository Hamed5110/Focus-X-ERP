$cs = "Server=localhost;Database=Focus80G0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$decl = @"
DECLARE @iStartDate INT = dbo.DateToInt(CONVERT(datetime,'20260101',112));
DECLARE @iEndDate INT = dbo.DateToInt(CONVERT(datetime,'20260928',112));
"@
$sql = Get-Content -Raw "C:\Users\Hamed Ali Khan\Focus-X-ERP\reports\sql\Purchase Order Last Rate Comparison.sql"
$sql = [regex]::Replace($sql, '(?s)/\*.*?\*/', '')
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 180
$cmd.CommandText = $decl + $sql
try {
    $r = $cmd.ExecuteReader()
    $names = @(); for ($i=0; $i -lt $r.FieldCount; $i++) { $names += $r.GetName($i) }
    Write-Output ("COLS: " + ($names -join " | "))
    $n=0; $both=0; $pyOnly=0; $cyOnly=0
    Write-Output "---- City Glasses 6mm Sunguard ----"
    while ($r.Read()) {
        $n++
        $py = [double]$r["Last Purchase Rate Previous Year"]
        $cy = [double]$r["Last Purchase Rate Current Year"]
        if ($py -gt 0 -and $cy -gt 0) { $both++ }
        elseif ($py -gt 0) { $pyOnly++ }
        elseif ($cy -gt 0) { $cyOnly++ }
        if ([string]$r["Vendor Name"] -like "*City Glasses*" -and [string]$r["Item Name"] -like "*Sunguard Ds Grey + 12mm A/s+ 6mm Clear Float*") {
            Write-Output ("PY={0} {1} {2:N4} | CY={3} {4} {5:N4} | Var={6:N4} Pct={7}" -f `
                $r["Previous Year PO No."], $r["Previous Year PO Date"], $py, `
                $r["Current Year PO No."], $r["Current Year PO Date"], $cy, `
                $r["Rate Variance"], $r["Comparative %"])
        }
    }
    $r.Close()
    Write-Output ("ROWS=$n BothYears=$both PrevOnly=$pyOnly CurrOnly=$cyOnly")
} catch {
    Write-Output ("FAIL: " + $_.Exception.Message)
}
$conn.Close()
