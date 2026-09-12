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
$cols = @(); for ($i=0; $i -lt $r.FieldCount; $i++) { $cols += $r.GetName($i) }
Write-Output ("COLS: " + ($cols -join " | "))
$sql = @()
while ($r.Read()) {
    $o = [ordered]@{}
    foreach ($c in $cols) { $o[$c] = $r[$c] }
    $sql += [pscustomobject]$o
}
$r.Close()
$coll = ($sql | Measure-Object "Collection Amount" -Sum).Sum
$elig = ($sql | Measure-Object "Eligible Collection" -Sum).Sum
Write-Output ("Aug rows={0} coll={1:N2} elig={2:N2} pays={3}" -f $sql.Count, $coll, $elig, (($sql."Payment Code" | Sort-Object -Unique) -join "; "))
$sql | Group-Object Salesman | ForEach-Object {
    $e = ($_.Group | Measure-Object "Eligible Collection" -Sum).Sum
    $se = [double]($_.Group[0]."Salesman Eligible Collection")
    $rt = [double]($_.Group[0]."Salesman Rate %")
    Write-Output ("  {0,-32} rows={1} elig={2,10:N2} smElig={3,10:N2} rate={4}" -f $_.Name, $_.Count, $e, $se, $rt)
}

$cmd2 = $conn.CreateCommand(); $cmd2.CommandTimeout = 180
$cmd2.CommandText = @"
DECLARE @iStartDate INT = dbo.DateToInt(CONVERT(datetime,'20260701',112))
DECLARE @iEndDate INT = dbo.DateToInt(CONVERT(datetime,'20260731',112))
$body
"@
$r2 = $cmd2.ExecuteReader()
$sql2 = @()
while ($r2.Read()) {
    $sql2 += [pscustomobject]@{
        Coll = [double]$r2["Collection Amount"]
        Elig = [double]$r2["Eligible Collection"]
        Sm   = [string]$r2["Salesman"]
        Rate = [double]$r2["Salesman Rate %"]
        SE   = [double]$r2["Salesman Eligible Collection"]
    }
}
$r2.Close(); $conn.Close()
Write-Output ("Jul rows={0} coll={1:N2} elig={2:N2}" -f $sql2.Count, ($sql2 | Measure-Object Coll -Sum).Sum, ($sql2 | Measure-Object Elig -Sum).Sum)
