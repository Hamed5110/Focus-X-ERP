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
        Cust = [string]$r["Customer Name"]
        Pay  = [string]$r["Payment Code"]
        Coll = [double]$r["Collection Amount"]
        CVal = [double]$r["Contract Value"]
        Elig = [double]$r["Eligible Collection"]
        Team = [double]$r["Team Eligible Collection"]
        Rate = [double]$r["Team Rate %"]
        Sm   = [string]$r["Salesman"]
    }
}
$r.Close(); $conn.Close()

Write-Output ("SQL rows={0} coll={1:N2} elig={2:N2} depts unique salesman={3}" -f $sql.Count, ($sql | Measure-Object Coll -Sum).Sum, ($sql | Measure-Object Elig -Sum).Sum, ($sql.Sm | Sort-Object -Unique).Count)
Write-Output ("SQL first elig={0:N2} second elig={1:N2}" -f (($sql | Where-Object { $_.Pay -like '*First*' } | Measure-Object Elig -Sum).Sum), (($sql | Where-Object { $_.Pay -like '*Second*' } | Measure-Object Elig -Sum).Sum))

# dump for python
$out = "C:\Users\Hamed Ali Khan\Focus-X-ERP\tools\atlas_aug_sql.csv"
$sql | Export-Csv -Path $out -NoTypeInformation -Encoding UTF8
Write-Output "wrote $out"
