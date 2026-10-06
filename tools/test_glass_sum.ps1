$cs = "Server=localhost;Database=Focus80G0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$sql = Get-Content -Raw "C:\Users\Hamed Ali Khan\Focus-X-ERP\reports\sql\Glass Status All Summary.sql"
$sql = $sql -replace '(?s)/\*.*?\*/', ''
$sql = $sql.Replace('@VendorAC', '0')
$sql = $sql.Replace('@JobOrder', '0')
$sql = $sql.Replace('@DeliveryStatus', '0')
$sql = $sql.Replace('@iStartDate', '132776193')
$sql = $sql.Replace('@iEndDate', '132778497')
$cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 180; $cmd.CommandText = $sql
try {
    $r = $cmd.ExecuteReader()
    $n = $r.FieldCount
    $hdr = @(); for ($i=0; $i -lt $n; $i++) { $hdr += $r.GetName($i) }
    Write-Output ($hdr -join " | ")
    $c = 0
    while ($r.Read()) {
        $c++
        if ($c -le 8) {
            $parts = @()
            for ($i=0; $i -lt $n; $i++) {
                if ($r.IsDBNull($i)) { $parts += "" } else {
                    $v = [string]$r.GetValue($i)
                    if ($v.Length -gt 50) { $v = $v.Substring(0,50) + "..." }
                    $parts += $v
                }
            }
            Write-Output ($parts -join " | ")
        }
    }
    $r.Close()
    Write-Output ("ROWS=" + $c)
} catch {
    Write-Output ("ERROR: " + $_.Exception.Message)
}
$conn.Close()
