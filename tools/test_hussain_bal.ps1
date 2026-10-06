$cs = "Server=localhost;Database=Focus80G0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$sql = Get-Content -Raw "C:\Users\Hamed Ali Khan\Focus-X-ERP\reports\sql\Customer Statement.sql"
$sql = $sql -replace '(?s)/\*.*?\*/', ''
$sql = $sql.Replace('@CustomerName', '17500')
$sql = $sql.Replace('@iStartDate', '132776193')
$sql = $sql.Replace('@iEndDate', '132778497')
$cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 120; $cmd.CommandText = $sql
$r = $cmd.ExecuteReader()
$n = $r.FieldCount
$hdr = @(); for ($i=0; $i -lt $n; $i++) { $hdr += $r.GetName($i) }
Write-Output ($hdr -join " | ")
$c = 0
while ($r.Read()) {
    $c++
    $parts = @()
    for ($i=0; $i -lt $n; $i++) {
        if ($r.IsDBNull($i)) { $parts += "" } else { $parts += [string]$r.GetValue($i) }
    }
    Write-Output ($parts -join " | ")
}
$r.Close()
Write-Output ("ROWS=" + $c)
$conn.Close()
