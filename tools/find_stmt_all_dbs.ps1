$csMaster = "Server=localhost;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $csMaster
$conn.Open()
$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT name FROM sys.databases WHERE name LIKE 'Focus%' OR name LIKE '%Atlas%' ORDER BY name"
$r = $cmd.ExecuteReader()
$dbs = @()
while ($r.Read()) { $dbs += $r.GetString(0) }
$r.Close()
$conn.Close()
Write-Output ("DBS: " + ($dbs -join ", "))

foreach ($db in $dbs) {
    $cs = "Server=localhost;Database=$db;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
    $c = New-Object System.Data.SqlClient.SqlConnection $cs
    try { $c.Open() } catch { Write-Output "[$db] open fail"; continue }
    $q = $c.CreateCommand(); $q.CommandTimeout = 60
    $q.CommandText = @"
IF OBJECT_ID('dbo.cCore_Reports_0') IS NULL SELECT 'no reports table' AS x
ELSE SELECT CAST(iReportId AS varchar(20)) + ' | ' + ISNULL(sReportName,'') + ' | t=' + CAST(iReportType AS varchar(10)) AS x
FROM dbo.cCore_Reports_0
WHERE sReportName LIKE '%Statement%' OR sReportName LIKE '%of Customer%'
"@
    try {
        $rr = $q.ExecuteReader()
        $n = 0
        while ($rr.Read()) {
            $n++
            if ($n -eq 1) { Write-Output "" ; Write-Output "===== $db =====" }
            Write-Output ([string]$rr[0])
        }
        $rr.Close()
    } catch {
        Write-Output "[$db] $($_.Exception.Message)"
        try { $rr.Close() } catch {}
    }
    $c.Close()
}
