$cs = "Server=localhost;Database=Focus80E0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$body = Get-Content -Raw "C:\Users\Hamed Ali Khan\Focus-X-ERP\reports\sql\Monthly Sales Commission - Atlas Aluminum.sql"
$cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 180
$cmd.CommandText = @"
DECLARE @iStartDate INT = dbo.DateToInt(CONVERT(datetime,'20260801',112))
DECLARE @iEndDate INT = dbo.DateToInt(CONVERT(datetime,'20260831',112))
$body
"@
$r = $cmd.ExecuteReader()
$sql = @()
while ($r.Read()) {
    $sql += [pscustomobject]@{
        Sm = [string]$r["Salesman"]
        Coll = [double]$r["Collection Amount"]
        Elig = [double]$r["Eligible Collection"]
        Team = [double]$r["Team Eligible Collection"]
        Rate = [double]$r["Team Rate %"]
        Mem = [double]$r["Team Members"]
        Share = [double]$r["Share Each"]
    }
}
$r.Close(); $conn.Close()
Write-Output ("SUM coll={0:N2} elig={1:N2} teamElig={2:N2} members={3} share={4:N2}" -f `
    ($sql | Measure-Object Coll -Sum).Sum, ($sql | Measure-Object Elig -Sum).Sum, `
    ($sql | Measure-Object Team -Sum).Sum, ($sql | Measure-Object Mem -Sum).Sum, `
    ($sql | Measure-Object Share -Sum).Sum)
$sql | Group-Object Sm | ForEach-Object {
    Write-Output ("  {0,-32} elig={1,10:N2} teamSum={2,10:N2} mem={3} rateMax={4}" -f `
        $_.Name, ($_.Group | Measure-Object Elig -Sum).Sum, ($_.Group | Measure-Object Team -Sum).Sum, `
        ($_.Group | Measure-Object Mem -Sum).Sum, ($_.Group | Measure-Object Rate -Maximum).Maximum)
}
