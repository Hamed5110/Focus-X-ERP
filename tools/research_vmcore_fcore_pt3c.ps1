$cs = "Server=localhost;Database=Focus80G0;User Id=sa;Password=$env:FOCUS_SQL_PASSWORD;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
function Dump($sql, $title) {
    Write-Output ""
    Write-Output "========== $title =========="
    $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 90; $cmd.CommandText = $sql
    try {
        $r = $cmd.ExecuteReader()
        $n = $r.FieldCount
        $hdr = @(); for ($i=0; $i -lt $n; $i++) { $hdr += $r.GetName($i) }
        Write-Output ($hdr -join " | ")
        $c = 0
        while ($r.Read()) {
            $c++
            $parts = @()
            for ($i=0; $i -lt $n; $i++) {
                if ($r.IsDBNull($i)) { $parts += "" } else {
                    $v = [string]$r.GetValue($i)
                    $v = $v -replace "`r|`n"," "
                    if ($v.Length -gt 80) { $v = $v.Substring(0,80) + "..." }
                    $parts += $v
                }
            }
            Write-Output ($parts -join " | ")
            if ($c -ge 10) { break }
        }
        $r.Close()
        if ($c -eq 0) { Write-Output "(no rows)" }
    } catch { Write-Output $_.Exception.Message }
}

Dump @"
SELECT *
FROM dbo.vmCore_Account
WHERE sCode = N'AC-847'
"@ "AC-847 both rows raw"

Dump @"
SELECT c.name
FROM sys.columns c
WHERE c.object_id = OBJECT_ID(N'dbo.vmCore_Account')
ORDER BY c.column_id
"@ "all vm cols"

Dump @"
SELECT TOP 20 iReportId, LEFT(sSqlQuery, 200)
FROM dbo.cCore_RDQuery_0
WHERE sSqlQuery LIKE N'%CASE @%' AND sSqlQuery NOT LIKE N'%fnRD_%'
ORDER BY iReportId DESC
"@ "CASE param queries"
