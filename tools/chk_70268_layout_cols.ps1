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
                    if ($v.Length -gt 160) { $v = $v.Substring(0,160) + "..." }
                    $parts += $v
                }
            }
            Write-Output ($parts -join " | ")
        }
        $r.Close()
    } catch { Write-Output $_.Exception.Message }
}

Dump @"
SELECT iColumnId, iFieldId, sAliasName, iType, iAlignment, iMiscellaneous
FROM dbo.cCore_ReportColumns_0
WHERE iLayoutId = 6919
ORDER BY iFieldId
"@ "70268 layout cols"

Dump @"
SELECT COUNT(*) Cnt FROM dbo.cCore_ReportTransactionSet_0 WHERE iReportId = 70268
"@ "transet count"

Dump @"
SELECT TOP 3 iMasterId, sCode, sName FROM dbo.mCore_Account WHERE sCode = N'AC-847'
"@ "AC-847"

$conn.Close()
