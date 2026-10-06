$cs = "Server=localhost;Database=Focus80G0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
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
                if ($r.IsDBNull($i)) { $parts += "NULL" } else {
                    $v = [string]$r.GetValue($i)
                    $v = $v -replace "`r|`n"," "
                    if ($v.Length -gt 180) { $v = $v.Substring(0,180) }
                    $parts += $v
                }
            }
            Write-Output ($parts -join " | ")
            if ($c -ge 50) { Write-Output "(truncated)"; break }
        }
        $r.Close()
        if ($c -eq 0) { Write-Output "(no rows)" }
    } catch { Write-Output $_.Exception.Message; try { $r.Close() } catch {} }
}

Dump "SELECT iFunctionId, iType, sValue, COUNT(*) Cnt FROM dbo.cCore_ReportColumnsFunction_0 GROUP BY iFunctionId, iType, sValue ORDER BY iFunctionId, iType" "all function ids"

Dump @"
SELECT TOP 30 sTopic, sFunctionName, LEFT(sHelpText, 200) Help
FROM dbo.cCore_ScriptHelp
WHERE sFunctionName LIKE '%Last%' OR sFunctionName LIKE '%Sum%' OR sFunctionName LIKE '%Total%'
   OR sTopic LIKE '%Last%' OR sHelpText LIKE '%Grand Total%'
"@ "script help last/sum"

Dump @"
SELECT t.name
FROM sys.tables t
WHERE t.name LIKE '%Function%' OR t.name LIKE '%Aggregate%' OR t.name LIKE '%Total%'
ORDER BY t.name
"@ "function-like tables"

Dump @"
SELECT c.sAliasName, c.iType, c.iMiscOption, f.iFunctionId, f.iType AS FnType, f.sValue
FROM dbo.cCore_ReportLayouts_0 l
JOIN dbo.cCore_ReportColumns_0 c ON c.iLayoutId = l.iLayoutId
LEFT JOIN dbo.cCore_ReportColumnsFunction_0 f ON f.iColumnId = c.iColumnId
WHERE l.iReportId = 560 AND (c.sAliasName LIKE '%Balance%' OR c.sAliasName IN ('Debit','Credit','Date'))
"@ "560 statement balance functions"
