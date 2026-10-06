$cs = "Server=localhost;Database=Focus80G0;User Id=sa;Password=$env:FOCUS_SQL_PASSWORD;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
function Dump($sql, $title) {
    Write-Output ""
    Write-Output "========== $title =========="
    $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 30; $cmd.CommandText = $sql
    try {
        $r = $cmd.ExecuteReader()
        $n = $r.FieldCount
        $hdr = @(); for ($i=0; $i -lt $n; $i++) { $hdr += $r.GetName($i) }
        Write-Output ($hdr -join " | ")
        while ($r.Read()) {
            $parts = @()
            for ($i=0; $i -lt $n; $i++) {
                if ($r.IsDBNull($i)) { $parts += "" } else {
                    $v = [string]$r.GetValue($i)
                    $v = $v -replace "`r|`n"," "
                    if ($v.Length -gt 250) { $v = $v.Substring(0,250) + "..." }
                    $parts += $v
                }
            }
            Write-Output ($parts -join " | ")
        }
        $r.Close()
    } catch { Write-Output $_.Exception.Message }
}

Dump "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME=N'cCore_ReportParameter_0' ORDER BY ORDINAL_POSITION" "param cols"
Dump "SELECT * FROM dbo.cCore_ReportParameter_0 WHERE iReportId IN (70198,70223,70268)" "params"
Dump "SELECT iReportId, iLayoutId FROM dbo.cCore_ReportLayouts_0 WHERE iReportId IN (70198,70223,70268)" "layouts"
Dump @"
SELECT iReportId, LEN(sSqlQuery) L,
 CHARINDEX('WHERE', sSqlQuery) FirstWhere,
 CHARINDEX('JOIN', sSqlQuery) FirstJoin,
 CHARINDEX(' iDate', sSqlQuery) FirstiDate
FROM dbo.cCore_RDQuery_0 WHERE iReportId IN (70198,70223,70268)
"@ "sql markers"

# dump first 2500 chars of 70198 and 70223
foreach ($id in 70198, 70223) {
    $cmd = $conn.CreateCommand(); $cmd.CommandText = "SELECT sSqlQuery FROM dbo.cCore_RDQuery_0 WHERE iReportId = $id"
    $sql = [string]$cmd.ExecuteScalar()
    Write-Output ""
    Write-Output "========== $id sql first 2000 =========="
    if ($sql.Length -gt 2000) { Write-Output ($sql.Substring(0,2000)) } else { Write-Output $sql }
    Write-Output ""
    Write-Output "========== $id sql last 800 =========="
    if ($sql.Length -gt 800) { Write-Output ($sql.Substring($sql.Length-800)) } else { Write-Output $sql }
}

$conn.Close()
