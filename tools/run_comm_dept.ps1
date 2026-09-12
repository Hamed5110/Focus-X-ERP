$cs = "Server=localhost;Database=Focus80E0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$base = [System.IO.File]::ReadAllText("C:\Users\Hamed Ali Khan\Focus-X-ERP\reports\sql\Monthly Sales Commission.sql")
$q = @"
SELECT Department, [Month Year], Salesman,
       [Contract Amount], [CRM Advance], [Advance Receipts], [Receipts Amount],
       [Qualified Amount], [Paid %], [Team Qualified Total], [Team Rate %],
       [Team Commission], [Team Members], [Share Each]
FROM ($base) r
WHERE r.Department = N'Atlas Aluminum'
  AND r.[Month Year] = N'Aug-2026'
ORDER BY Salesman
"@
$cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 180; $cmd.CommandText = $q
try {
    $r = $cmd.ExecuteReader()
    $n = $r.FieldCount
    $hdr = @(); for ($i=0;$i -lt $n;$i++) { $hdr += $r.GetName($i) }
    Write-Output ($hdr -join " | ")
    while ($r.Read()) {
        $parts = @()
        for ($i=0;$i -lt $n;$i++) {
            if ($r.IsDBNull($i)) { $parts += "" } else { $parts += [string]$r.GetValue($i) }
        }
        Write-Output ($parts -join " | ")
    }
    $r.Close()
} catch {
    Write-Output ("SQL ERROR: " + $_.Exception.Message)
}
$conn.Close()
