$cs = "Server=localhost;Database=Focus80G0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$sql = Get-Content -Raw "C:\Users\Hamed Ali Khan\Focus-X-ERP\reports\sql\Customer Statement.sql"
# strip comment block
$sql = $sql -replace '(?s)/\*.*?\*/', ''
$sql = $sql.Replace('@CustomerName', '18732')
$sql = $sql.Replace('@iStartDate', '132776193')  # 1 Jan 2026 = 2026*65536+1*256+1
$sql = $sql.Replace('@iEndDate', '132778270')    # 30 Sep 2026 = 2026*65536+9*256+30
$cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 120; $cmd.CommandText = $sql
try {
    $r = $cmd.ExecuteReader()
    $n = $r.FieldCount
    $hdr = @(); for ($i=0; $i -lt $n; $i++) { $hdr += $r.GetName($i) }
    Write-Output ($hdr -join " | ")
    while ($r.Read()) {
        $parts = @()
        for ($i=0; $i -lt $n; $i++) {
            if ($r.IsDBNull($i)) { $parts += "" } else { $parts += [string]$r.GetValue($i) }
        }
        Write-Output ($parts -join " | ")
    }
    $r.Close()
} catch {
    Write-Output ("ERROR: " + $_.Exception.Message)
}
$conn.Close()

Write-Output ""
Write-Output "packed check 27 Jan 2026:"
Write-Output (2026*65536 + 1*256 + 27)
Write-Output "1 Jan 2026:"
Write-Output (2026*65536 + 1*256 + 1)
Write-Output "30 Sep 2026:"
Write-Output (2026*65536 + 9*256 + 30)
