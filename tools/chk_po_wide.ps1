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
    Write-Output ("COLCOUNT=" + $names.Count)
    Write-Output ("FIRST=" + ($names[0..7] -join " | "))
    Write-Output ("LAST=" + ($names[($names.Count-11)..($names.Count-1)] -join " | "))
    $n=0
    while ($r.Read()) {
        $n++
        $item = [string]$r["Item Name"]
        $g = [string]$r["Item Group"]
        if ([string]$r["Vendor Name"] -like "*City Glasses*" -and $item -like "*Sunguard Ds Grey + 12mm A/s+ 6mm Clear Float*") {
            Write-Output ("CityGlasses Jan PY={0} CY={1} Diff={2} Pct={3} Years={4}/{5} Wood={6} Acc={7} Sil={8} Stone={9}" -f `
                $r["Jan Previous Price"], $r["Jan New Price"], $r["Jan Price Difference"], $r["Jan Percentage Difference"], `
                $r["Previous Year"], $r["Current Year"], $r["Wooden Sheets"], $r["Accessories"], $r["Silicone"], $r["Stones"])
        }
        if ($g -like "*Silicone*" -and $n -le 9000) {
            if ([string]$r["Silicone"] -ne "Y") { Write-Output ("BAD SIL FLAG G={0} Item={1} Sil={2}" -f $g, $item, $r["Silicone"]) }
        }
    }
    $r.Close()
    Write-Output ("ROWS=$n")
} catch {
    Write-Output ("FAIL: " + $_.Exception.Message)
}
$conn.Close()
