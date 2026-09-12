$cs = "Server=localhost;Database=Focus8080;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$q = [System.IO.File]::ReadAllText("C:\Users\Hamed Ali Khan\Focus-X-ERP\tools\cloud_diff_query6.sql")
$cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 280; $cmd.CommandText = $q
$a = New-Object System.Data.SqlClient.SqlDataAdapter $cmd
$dt = New-Object System.Data.DataTable
try {
    [void]$a.Fill($dt)
    Write-Host ("rows: " + $dt.Rows.Count)
    $dt.Rows | Select-Object -First 30 | ForEach-Object {
        Write-Host ($_[0].ToString() + " | " + $_[1] + " | " + $_[2] + " | " + $_[3] + " | " + $_[4] + " | " + $_[5])
    }
} catch {
    Write-Host ("SQL ERROR: " + $_.Exception.Message)
}
$conn.Close()
