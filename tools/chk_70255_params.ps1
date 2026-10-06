$cs = "Server=localhost;Database=Focus80G0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$cmd = $conn.CreateCommand()
$cmd.CommandText = @"
SELECT p.sFieldName, p.sFieldVariable, p.iControlType, p.iFieldType, p.iSelectionMode, p.iFieldId, p.iType, LEFT(ISNULL(p.sDefault,N''),40) AS Def
FROM dbo.cCore_ReportParameter_0 p
WHERE p.iReportId = 70255
"@
$r = $cmd.ExecuteReader()
$n = $r.FieldCount
$hdr = @(); for ($i=0; $i -lt $n; $i++) { $hdr += $r.GetName($i) }
Write-Output ($hdr -join " | ")
while ($r.Read()) {
    $parts = @(); for ($i=0; $i -lt $n; $i++) { if ($r.IsDBNull($i)) { $parts += "" } else { $parts += [string]$r.GetValue($i) } }
    Write-Output ($parts -join " | ")
}
$r.Close()
$cmd.CommandText = @"
SELECT TOP 1 LEFT(sSqlQuery, 2500) FROM dbo.cCore_RDQuery_0 WHERE iReportId = 70255
"@
try {
    $r2 = $cmd.ExecuteReader()
    if ($r2.Read()) { Write-Output "==== SQL HEAD ===="; Write-Output ([string]$r2.GetValue(0)) }
    $r2.Close()
} catch { Write-Output $_.Exception.Message }
$conn.Close()
