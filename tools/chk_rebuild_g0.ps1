$cs = "Server=localhost;Database=Focus80G0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$decl = @"
DECLARE @iStartDate INT = dbo.DateToInt(CONVERT(datetime,'20260801',112));
DECLARE @iEndDate INT = dbo.DateToInt(CONVERT(datetime,'20260831',112));
"@
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 180
$q = Get-Content -Raw "C:\Users\Hamed Ali Khan\Focus-X-ERP\reports\sql\Monthly Sales Commission - Atlas Aluminum Detail.sql"
$cmd.CommandText = $decl + $q
try {
    $r = $cmd.ExecuteReader()
    $n=0; $c=0; $coll=0; $life=0; $e=0; $so=0
    Write-Output "---- Saeed / sample ----"
    while ($r.Read()) {
        $n++
        $c += [double]$r["Total Contract Amount"]
        $coll += [double]$r["Total Collection"]
        $life += [double]$r["Lifetime Collection"]
        $e += [double]$r["Eligible Amount"]
        if ($r["Contract / Sales Order No."] -ne [DBNull]::Value -and [string]$r["Contract / Sales Order No."] -ne "") { $so++ }
        $name = [string]$r["Customer Name"]
        if ($name -like "*Saeed*" -or $n -le 3) {
            Write-Output ("{0,-32} SO={1,-14} {2}  Rct={3,-14} {4}  C={5,10:N2} Coll={6,8:N2} Life={7,8:N2} Pct={8,6} Gate={9,-3}" -f `
                $name, $r["Contract / Sales Order No."], $r["Contract Date"], `
                $r["Receipt No."], $r["Receipt Date"], $r["Total Contract Amount"], `
                $r["Total Collection"], $r["Lifetime Collection"], $r["Collection %"], $r["Gate Status"])
        }
    }
    $r.Close()
    Write-Output ("ROWS=$n ContractSum=$c CollSum=$coll LifeSum=$life EligSum=$e SOFilled=$so")
} catch {
    Write-Output ("FAIL: " + $_.Exception.Message)
}

$cmd.CommandText = $decl + (Get-Content -Raw "C:\Users\Hamed Ali Khan\Focus-X-ERP\reports\sql\Monthly Sales Commission - Atlas Aluminum.sql")
try {
    $r2 = $cmd.ExecuteReader()
    $sC=0; $sColl=0; $sE=0
    while ($r2.Read()) {
        $sC += [double]$r2["Total Contract Amount"]
        $sColl += [double]$r2["Total Collection"]
        $sE += [double]$r2["Eligible Amount"]
    }
    $r2.Close()
    Write-Output ("SUMMARY C=$sC Coll=$sColl E=$sE")
} catch {
    Write-Output ("SUMMARY FAIL: " + $_.Exception.Message)
}
$conn.Close()
