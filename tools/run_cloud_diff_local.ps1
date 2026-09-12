$cs = "Server=localhost;Database=Focus8080;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$q = [System.IO.File]::ReadAllText("C:\Users\Hamed Ali Khan\Focus-X-ERP\tools\cloud_diff_query.sql")
$cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 280; $cmd.CommandText = $q
$a = New-Object System.Data.SqlClient.SqlDataAdapter $cmd
$dt = New-Object System.Data.DataTable
try {
    [void]$a.Fill($dt)
    Write-Host ("rows: " + $dt.Rows.Count)
    $dt.Rows | ForEach-Object {
        Write-Host ("{0} | {1} | sqlC={2} cubeC={3} dC={4} | sqlA={5} cubeA={6} dA={7} | {8}" -f $_.Issue, $_.Code, $_.SqlContract, $_.CubeContract, $_.DiffContract, $_.SqlAdv, $_.CubeAdv, $_.DiffAdv, $_.SqlName)
    }
} catch {
    Write-Host ("SQL ERROR: " + $_.Exception.Message)
}
$conn.Close()
