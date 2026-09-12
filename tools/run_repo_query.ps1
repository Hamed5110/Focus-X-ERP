$cs = "Server=localhost;Database=Focus8080;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$sql = Get-Content "C:\Users\Hamed Ali Khan\Focus-X-ERP\reports\sql\III Summary Report.sql" -Raw
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 280; $cmd.CommandText = $sql
$a = New-Object System.Data.SqlClient.SqlDataAdapter $cmd
$dt = New-Object System.Data.DataTable
[void]$a.Fill($dt)
$dt | Format-Table -AutoSize | Out-String -Width 160 | Write-Host
$conn.Close()
