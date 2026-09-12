$sql = [System.IO.File]::ReadAllText("C:\Users\Hamed Ali Khan\Focus-X-ERP\reports\sql\III Summary Report.sql")

$cs = "Server=localhost;Database=Focus8080;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"

function Run-Version($query, $label) {
    $conn = New-Object System.Data.SqlClient.SqlConnection $cs
    $conn.Open()
    $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 280
    $cmd.CommandText = $query
    $a = New-Object System.Data.SqlClient.SqlDataAdapter $cmd
    $dt = New-Object System.Data.DataTable
    [void]$a.Fill($dt)
    $conn.Close()
    Write-Host ("=== " + $label + " ===")
    $dt | Format-Table -AutoSize | Out-String -Width 250 | Write-Host
}

# Version A: as-is (with grp filter)
Run-Version $sql "WITH grp filter"

# Version B: grp equality filter disabled (keep join)
$sqlB = $sql.Replace("AND (grp.sName IS NULL OR grp.GroupStatus = acc.ReportStatus)", "AND (1 = 1)")
if ($sqlB -eq $sql) { Write-Host "REPLACE FAILED - filter line not found" }
Run-Version $sqlB "WITHOUT grp filter (baseline)"
