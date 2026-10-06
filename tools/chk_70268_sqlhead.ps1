$cs = "Server=localhost;Database=Focus80E0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 60
$cmd.CommandText = "SELECT iReportId, sSqlQuery FROM dbo.cCore_RDQuery_0 WHERE iReportId IN (70266,70267,70268)"
$r = $cmd.ExecuteReader()
while ($r.Read()) {
    $id = $r[0]
    $sql = [string]$r[1]
    Write-Output ""
    Write-Output "===== $id SELECT head ====="
    $idx = $sql.IndexOf("SELECT")
    if ($idx -lt 0) { $idx = 0 }
    $head = $sql.Substring($idx, [Math]::Min(1800, $sql.Length - $idx))
    $head = $head -replace "`r|`n"," "
    Write-Output $head
    Write-Output ""
    Write-Output "===== $id Month Year snippet ====="
    $p = $sql.IndexOf("Month Year")
    if ($p -ge 0) {
        $a = [Math]::Max(0, $p - 200)
        Write-Output ($sql.Substring($a, [Math]::Min(500, $sql.Length - $a)) -replace "`r|`n"," ")
    } else { Write-Output "(no Month Year alias)" }
    $p2 = $sql.IndexOf("DATENAME")
    if ($p2 -ge 0) {
        Write-Output "DATENAME:"
        $a = [Math]::Max(0, $p2 - 80)
        Write-Output ($sql.Substring($a, [Math]::Min(350, $sql.Length - $a)) -replace "`r|`n"," ")
    }
}
$r.Close()
$conn.Close()
