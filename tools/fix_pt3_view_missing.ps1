$csMaster = "Server=localhost;Database=master;User Id=sa;Password=$env:FOCUS_SQL_PASSWORD;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $csMaster
$conn.Open()
$cmd = $conn.CreateCommand()
$cmd.CommandText = @"
SELECT name FROM sys.databases
WHERE name LIKE N'Focus8%'
ORDER BY name
"@
$rd = $cmd.ExecuteReader()
$dbs = @()
while ($rd.Read()) { $dbs += [string]$rd[0] }
$rd.Close()
$conn.Close()
Write-Output ("dbs=" + ($dbs -join ", "))

$vw = [System.IO.File]::ReadAllText("C:\Users\Hamed Ali Khan\Focus-X-ERP\reports\sql\vRD_PT3AtlasAccounts.sql")
$q = [System.IO.File]::ReadAllText("C:\Users\Hamed Ali Khan\Focus-X-ERP\reports\sql\Project Tracking III Report Atlas Ledger.sql")

foreach ($db in $dbs) {
    $cs = "Server=localhost;Database=$db;User Id=sa;Password=$env:FOCUS_SQL_PASSWORD;Encrypt=False;TrustServerCertificate=True;"
    $c = New-Object System.Data.SqlClient.SqlConnection $cs
    try { $c.Open() } catch { Write-Output "$db OPEN FAIL $($_.Exception.Message)"; continue }
    $cmd = $c.CreateCommand()
    $cmd.CommandText = @"
SELECT
  CASE WHEN OBJECT_ID(N'dbo.vmCore_Account') IS NULL THEN 0 ELSE 1 END HasVm,
  CASE WHEN OBJECT_ID(N'dbo.vRD_PT3AtlasAccounts') IS NULL THEN 0 ELSE 1 END HasV,
  CASE WHEN OBJECT_ID(N'dbo.cCore_RDQuery_0') IS NULL THEN 0 ELSE 1 END HasQ
"@
    $rd = $cmd.ExecuteReader(); $rd.Read()
    $hasVm = [int]$rd[0]; $hasV = [int]$rd[1]; $hasQ = [int]$rd[2]
    $rd.Close()

    $hasRpt = 0
    $rptDb = ""
    if ($hasQ -eq 1) {
        $cmd = $c.CreateCommand()
        $cmd.CommandText = "SELECT COUNT(*), MAX(LEFT(sSqlQuery,80)) FROM dbo.cCore_RDQuery_0 WHERE iReportId=70268"
        try {
            $rd = $cmd.ExecuteReader(); $rd.Read()
            $hasRpt = [int]$rd[0]
            $rptDb = [string]$rd[1]
            $rd.Close()
        } catch { }
    }
    Write-Output ("{0} HasVm={1} HasV={2} HasQ={3} Rpt70268={4} {5}" -f $db, $hasVm, $hasV, $hasQ, $hasRpt, $rptDb)

    if ($hasVm -eq 1) {
        try {
            $cmd = $c.CreateCommand(); $cmd.CommandTimeout = 60; $cmd.CommandText = $vw
            [void]$cmd.ExecuteNonQuery()
            $cmd = $c.CreateCommand(); $cmd.CommandText = "GRANT SELECT ON dbo.vRD_PT3AtlasAccounts TO PUBLIC"
            [void]$cmd.ExecuteNonQuery()
            $cmd = $c.CreateCommand(); $cmd.CommandText = "SELECT COUNT(*) FROM dbo.vRD_PT3AtlasAccounts"
            $n = $cmd.ExecuteScalar()
            Write-Output ("  created view rows={0}" -f $n)
        } catch {
            Write-Output ("  VIEW FAIL " + $_.Exception.Message)
        }
    }
    if ($hasRpt -gt 0) {
        $upd = $c.CreateCommand()
        $upd.CommandText = "UPDATE dbo.cCore_RDQuery_0 SET sSqlQuery=@q WHERE iReportId=70268"
        [void]$upd.Parameters.AddWithValue("@q", $q)
        [void]$upd.ExecuteNonQuery()
        Write-Output "  70268 SQL refreshed"
    }
    $c.Close()
}
