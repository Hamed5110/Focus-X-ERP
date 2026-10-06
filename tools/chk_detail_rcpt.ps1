$cs = "Server=localhost;Database=Focus80E0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
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
    $cols = @(); for ($i=0; $i -lt $r.FieldCount; $i++) { $cols += $r.GetName($i) }
    Write-Output ("COLS: " + ($cols -join " | "))
    $n=0; $c=0; $coll=0; $e=0
    while ($r.Read()) {
        $n++
        $c += [double]$r["Total Contract Amount"]
        $coll += [double]$r["Total Collection"]
        $e += [double]$r["Eligible Amount"]
        if ($n -le 8) {
            Write-Output ("{0,-32} SO={1,-14} {2}  Rct={3,-14} {4}  Coll={5:N2} C={6:N2}" -f `
                $r["Customer Name"], $r["Contract / Sales Order No."], $r["Contract Date"], `
                $r["Receipt No."], $r["Receipt Date"], $r["Total Collection"], $r["Total Contract Amount"])
        }
    }
    $r.Close()
    Write-Output ("ROWS=$n ContractSum=$c CollSum=$coll EligSum=$e")
} catch {
    Write-Output ("FAIL: " + $_.Exception.Message)
}
$cmd.CommandText = $decl + (Get-Content -Raw "C:\Users\Hamed Ali Khan\Focus-X-ERP\reports\sql\Monthly Sales Commission - Atlas Aluminum.sql")
$r2 = $cmd.ExecuteReader()
$sC=0; $sColl=0; $sE=0
while ($r2.Read()) {
    $sC += [double]$r2["Total Contract Amount"]
    $sColl += [double]$r2["Total Collection"]
    $sE += [double]$r2["Eligible Amount"]
}
$r2.Close(); $conn.Close()
Write-Output ("SUMMARY C=$sC Coll=$sColl E=$sE")
