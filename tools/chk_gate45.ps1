$cs = "Server=localhost;Database=Focus80E0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$decl = @"
DECLARE @iStartDate INT = dbo.DateToInt(CONVERT(datetime,'20260801',112));
DECLARE @iEndDate INT = dbo.DateToInt(CONVERT(datetime,'20260831',112));
"@
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 180
$cmd.CommandText = $decl + (Get-Content -Raw "C:\Users\Hamed Ali Khan\Focus-X-ERP\reports\sql\Monthly Sales Commission - Atlas Aluminum.sql")
$r = $cmd.ExecuteReader()
$sC=0; $sColl=0; $sCE=0; $sE=0; $sNE=0; $sO=0; $n=0
while ($r.Read()) {
    $n++
    $sC += [double]$r["Total Contract Amount"]
    $sColl += [double]$r["Total Collection"]
    $sCE += [double]$r["Collection Eligible Amount"]
    $sE += [double]$r["Eligible Amount"]
    $sNE += [double]$r["Not Eligible Amount"]
    $sO += [double]$r["Overall Sales"]
    Write-Output ("{0,-28} C={1,12:N2} Coll={2,10:N2} CE={3,10:N2} E={4,12:N2} NE={5,10:N2} O={6,12:N2}" -f `
        $r["Salesman"], $r["Total Contract Amount"], $r["Total Collection"], `
        $r["Collection Eligible Amount"], $r["Eligible Amount"], $r["Not Eligible Amount"], $r["Overall Sales"])
}
$r.Close()
Write-Output ("SUMMARY rows=$n C=$sC Coll=$sColl CE=$sCE E=$sE NE=$sNE Overall=$sO")

$cmd.CommandText = $decl + (Get-Content -Raw "C:\Users\Hamed Ali Khan\Focus-X-ERP\reports\sql\Monthly Sales Commission - Atlas Aluminum Detail.sql")
$r2 = $cmd.ExecuteReader()
$yes=0; $no=0; $dC=0; $dColl=0; $dE=0; $dCE=0; $dn=0
while ($r2.Read()) {
    $dn++
    $dC += [double]$r2["Total Contract Amount"]
    $dColl += [double]$r2["Total Collection"]
    $dE += [double]$r2["Eligible Amount"]
    $dCE += [double]$r2["Collection Eligible Amount"]
    if ($r2["Gate Status"] -eq "Yes") { $yes++ } else { $no++ }
}
$r2.Close(); $conn.Close()
Write-Output ("DETAIL rows=$dn C=$dC Coll=$dColl CE=$dCE E=$dE GateYes=$yes GateNo=$no")
