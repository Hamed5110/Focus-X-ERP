$cs = "Server=localhost;Database=Focus80E0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$sql = Get-Content -Raw "C:\Users\Hamed Ali Khan\Focus-X-ERP\reports\sql\Monthly Sales Commission - Atlas Aluminum.sql"
$sql = @"
DECLARE @iStartDate INT = dbo.DateToInt(CONVERT(datetime,'20260801',112));
DECLARE @iEndDate INT = dbo.DateToInt(CONVERT(datetime,'20260831',112));
"@ + $sql
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 180
$cmd.CommandText = $sql
$r = $cmd.ExecuteReader()
$names = @(); for ($i=0; $i -lt $r.FieldCount; $i++) { $names += $r.GetName($i) }
Write-Output ("COLS: " + ($names -join " | "))
$sumC=0; $sumColl=0; $sumCE=0; $sumE=0; $sumNE=0; $sumO=0; $sumP=0
while ($r.Read()) {
    $sumC += [double]$r["Total Contract Amount"]
    $sumColl += [double]$r["Total Collection"]
    $sumCE += [double]$r["Collection Eligible Amount"]
    $sumE += [double]$r["Eligible Amount"]
    $sumNE += [double]$r["Not Eligible Amount"]
    $sumO += [double]$r["Overall Sales"]
    $sumP += [double]$r["Total Commission Amount"]
    Write-Output ("{0,-32} C={1,10:N2} Coll={2,10:N2} CollElig={3,10:N2} Elig={4,10:N2} Not={5,10:N2} Ov={6,10:N2} R={7} Pay={8:N2}" -f `
        $r["Salesman"], $r["Total Contract Amount"], $r["Total Collection"], $r["Collection Eligible Amount"], `
        $r["Eligible Amount"], $r["Not Eligible Amount"], $r["Overall Sales"], $r["Commission Ratio"], $r["Total Commission Amount"])
}
Write-Output ("SUM C={0:N2} Coll={1:N2} CollElig={2:N2} Elig={3:N2} Not={4:N2} Overall(Rn1)={5:N2} Pay={6:N2}" -f $sumC,$sumColl,$sumCE,$sumE,$sumNE,$sumO,$sumP)
$r.Close()

$cmd.CommandText = @"
DECLARE @iStartDate INT = dbo.DateToInt(CONVERT(datetime,'20260901',112));
DECLARE @iEndDate INT = dbo.DateToInt(CONVERT(datetime,'20260912',112));
"@ + (Get-Content -Raw "C:\Users\Hamed Ali Khan\Focus-X-ERP\reports\sql\Monthly Sales Commission - Atlas Aluminum.sql")
$r2 = $cmd.ExecuteReader()
$n=0
while ($r2.Read()) { $n++; Write-Output ("SEP ROW {0} {1} {2}" -f $r2["Salesman"], $r2["Total Collection"], $r2["Collection Eligible Amount"]) }
Write-Output ("SEP 1-12 ROWS=$n")
$r2.Close(); $conn.Close()
