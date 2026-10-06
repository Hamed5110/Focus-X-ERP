$cs = "Server=localhost;Database=Focus80G0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$decl = @"
DECLARE @iStartDate INT = dbo.DateToInt(CONVERT(datetime,'20260801',112));
DECLARE @iEndDate INT = dbo.DateToInt(CONVERT(datetime,'20260831',112));
"@
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 180
$cmd.CommandText = $decl + (Get-Content -Raw "C:\Users\Hamed Ali Khan\Focus-X-ERP\reports\sql\Monthly Sales Commission - Atlas Aluminum Detail.sql")
try {
    $r = $cmd.ExecuteReader()
    $names = @(); for ($i=0; $i -lt $r.FieldCount; $i++) { $names += $r.GetName($i) }
    Write-Output ("COLS: " + ($names -join " | "))
    $n=0; $c=0; $coll=0; $e=0; $o=0; $no=0
    Write-Output "---- Saeed ----"
    while ($r.Read()) {
        $n++
        $c += [double]$r["Total Contract Amount"]
        $coll += [double]$r["Total Collection"]
        $e += [double]$r["Eligible Amount"]
        $o += [double]$r["Overall Sales"]
        if ([string]$r["Gate Status"] -ne "Yes") { $no++ }
        if ([string]$r["Customer Name"] -like "*Mohamed Saeed") {
            Write-Output ("SO={0} Rct={1} C={2:N2} Coll={3:N2} Pct={4} Gate={5} E={6:N2}" -f `
                $r["Contract / Sales Order No."], $r["Receipt No."], `
                $r["Total Contract Amount"], $r["Total Collection"], `
                $r["Collection %"], $r["Gate Status"], $r["Eligible Amount"])
        }
    }
    $r.Close()
    Write-Output ("ROWS=$n ContractSum=$c CollSum=$coll EligSum=$e OverallPrint=$o NonYes=$no")
} catch {
    Write-Output ("FAIL: " + $_.Exception.Message)
}
$conn.Close()
