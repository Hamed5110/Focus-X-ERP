$cs = "Server=localhost;Database=Focus80E0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$base = [System.IO.File]::ReadAllText("C:\Users\Hamed Ali Khan\Focus-X-ERP\reports\sql\Monthly Sales Commission.sql")
Write-Output ("sql chars: " + $base.Length)
$cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 180
$cmd.CommandText = "SELECT TOP 5 * FROM (`n$base`n) r"
try {
    $r = $cmd.ExecuteReader()
    $n = $r.FieldCount
    $hdr = @(); for ($i=0;$i -lt $n;$i++) { $hdr += $r.GetName($i) }
    Write-Output ($hdr -join " | ")
    $c = 0
    while ($r.Read()) {
        $parts = @()
        for ($i=0;$i -lt $n;$i++) {
            if ($r.IsDBNull($i)) { $parts += "" } else { $parts += [string]$r.GetValue($i) }
        }
        Write-Output ($parts -join " | ")
        $c++
    }
    $r.Close()
    Write-Output ("rows: $c")
} catch {
    Write-Output ("SQL ERROR: " + $_.Exception.Message)
}
$conn.Close()
