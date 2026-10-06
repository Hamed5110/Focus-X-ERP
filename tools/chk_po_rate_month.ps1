$cs = "Server=localhost;Database=Focus80G0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$decl = @"
DECLARE @iStartDate INT = dbo.DateToInt(CONVERT(datetime,'20260101',112));
DECLARE @iEndDate INT = dbo.DateToInt(CONVERT(datetime,'20261231',112));
"@
$sql = [regex]::Replace((Get-Content -Raw "C:\Users\Hamed Ali Khan\Focus-X-ERP\reports\sql\Purchase Order Last Rate Comparison.sql"), '(?s)/\*.*?\*/', '')
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 180
$cmd.CommandText = $decl + $sql
try {
    $r = $cmd.ExecuteReader()
    $names = @(); for ($i=0; $i -lt $r.FieldCount; $i++) { $names += $r.GetName($i) }
    Write-Output ("COLS: " + ($names -join " | "))
    $n=0; $dec=0; $inc=0; $groups = @{}
    Write-Output "---- Wacker / Silicone samples ----"
    while ($r.Read()) {
        $n++
        $g = [string]$r["Item Group"]
        if (-not $groups.ContainsKey($g)) { $groups[$g] = 0 }
        $groups[$g]++
        $st = [string]$r["Change Status"]
        if ($st -eq "Decrease") { $dec++ }
        if ($st -eq "Increase") { $inc++ }
        $nm = [string]$r["Item Name"]
        if ($nm -like "*Wacker*" -or $nm -like "*Sunguard Ds Grey + 12mm A/s+ 6mm Clear Float*") {
            if ($n -lt 50000) {
                Write-Output ("V={0} G={1} Item={2} Mo={3} PY={4} CY={5} Diff={6} Pct={7} St={8}" -f `
                    $r["Vendor Name"], $g, $nm, $r["Month"], `
                    $r["Previous Price"], $r["New Price"], $r["Price Difference"], `
                    $r["Percentage Difference"], $st)
            }
        }
    }
    $r.Close()
    Write-Output ("ROWS=$n Decrease=$dec Increase=$inc")
    Write-Output "---- Item Group counts (top) ----"
    $groups.GetEnumerator() | Sort-Object Value -Descending | Select-Object -First 15 | ForEach-Object {
        Write-Output ("{0} = {1}" -f $_.Key, $_.Value)
    }
} catch {
    Write-Output ("FAIL: " + $_.Exception.Message)
}
$conn.Close()
