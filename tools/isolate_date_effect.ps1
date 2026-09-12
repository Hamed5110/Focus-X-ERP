$cs = "Server=localhost;Database=Focus8080;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$sql = Get-Content "C:\Users\Hamed Ali Khan\Focus-X-ERP\reports\sql\III Summary Report.sql" -Raw

# Variant without the date filter: comment out the 8 injected lines
$noDate = $sql -replace [regex]::Escape("AND h.iDate <= (YEAR(GETDATE()) * 65536) + (MONTH(GETDATE()) * 256) + DAY(GETDATE())"), "AND 1=1"

$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()

function Run($label, $q) {
    $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 280; $cmd.CommandText = $q
    $a = New-Object System.Data.SqlClient.SqlDataAdapter $cmd
    $dt = New-Object System.Data.DataTable
    [void]$a.Fill($dt)
    Write-Host ("=== " + $label + " ===")
    $dt | Format-Table -AutoSize | Out-String -Width 160 | Write-Host
}

Run "WITH date filter (current repo query)" $sql
Run "WITHOUT date filter (1=1)" $noDate

$conn.Close()
