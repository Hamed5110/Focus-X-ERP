$cs = "Server=localhost;Database=Focus8080;User Id=sa;Password=$env:FOCUS_SQL_PASSWORD;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$cmd = $conn.CreateCommand()
$cmd.CommandText = @"
SELECT r.iReportId, r.sReportName, q.sSqlQuery
FROM dbo.cCore_RDQuery_0 q
JOIN dbo.cCore_Reports_0 r ON r.iReportId = q.iReportId
WHERE q.sSqlQuery LIKE N'%@Department%' OR q.sSqlQuery LIKE N'%@Status%' OR q.sSqlQuery LIKE N'%@Reportstatus%'
   OR q.sSqlQuery LIKE N'%@VendorAC%' OR q.sSqlQuery LIKE N'%@CustomerName%'
"@
$r = $cmd.ExecuteReader()
while ($r.Read()) {
    Write-Output ""
    Write-Output ("========== {0} {1} ==========" -f $r[0], $r[1])
    $sql = [string]$r[2]
    $sql = $sql -replace "`r|`n"," "
    if ($sql.Length -gt 2500) { $sql = $sql.Substring(0,2500) + "..." }
    Write-Output $sql
}
$r.Close()
$conn.Close()
