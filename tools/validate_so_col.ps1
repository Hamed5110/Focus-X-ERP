$cs = "Server=localhost;Database=Focus80E0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$decl = @"
DECLARE @iStartDate INT = dbo.DateToInt(CONVERT(datetime,'20260801',112));
DECLARE @iEndDate INT = dbo.DateToInt(CONVERT(datetime,'20260831',112));
"@
$sql = $decl + (Get-Content -Raw "C:\Users\Hamed Ali Khan\Focus-X-ERP\reports\sql\Monthly Sales Commission - Atlas Aluminum Detail.sql")
$sum = $decl + (Get-Content -Raw "C:\Users\Hamed Ali Khan\Focus-X-ERP\reports\sql\Monthly Sales Commission - Atlas Aluminum.sql")
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 180
$cmd.CommandText = $sql
$r = $cmd.ExecuteReader()
$cols = @(); for ($i=0; $i -lt $r.FieldCount; $i++) { $cols += $r.GetName($i) }
Write-Output ("COLS: " + ($cols -join " | "))
$n=0; $with=0; $cSum=0; $eSum=0
while ($r.Read()) {
    $n++
    $so = [string]$r["Contract / Sales Order No."]
    if ($so -ne "") { $with++ }
    $cSum += [double]$r["Total Contract Amount"]
    $eSum += [double]$r["Eligible Amount"]
    Write-Output ("{0,-36} {1,-16} {2,-12} C={3,10:N2} Gate={4}" -f $r["Customer Name"], $so, $r["Contract Date"], $r["Total Contract Amount"], $r["Gate Status"])
}
$r.Close()
Write-Output ("ROWS=$n WITH_SO=$with  ContractSum=$cSum  EligSum=$eSum")

$cmd.CommandText = $sum
$r2 = $cmd.ExecuteReader()
$sC=0; $sE=0
while ($r2.Read()) {
    $sC += [double]$r2["Total Contract Amount"]
    $sE += [double]$r2["Eligible Amount"]
}
$r2.Close(); $conn.Close()
Write-Output ("SUMMARY Contract=$sC Elig=$sE  matchC=$([math]::Abs($cSum-$sC) -le 0.05) matchE=$([math]::Abs($eSum-$sE) -le 0.05)")
