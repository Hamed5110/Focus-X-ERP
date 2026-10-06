$cs = "Server=localhost;Database=Focus80E0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
function Dump($sql, $title) {
    Write-Output ""
    Write-Output "========== $title =========="
    $cmd = $conn.CreateCommand(); $cmd.CommandText = $sql
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
                    if ($v.Length -gt 200) { $v = $v.Substring(0,200) }
                    $parts += $v
                }
            }
            Write-Output ($parts -join " | ")
            if ($c -ge 40) { break }
        }
        $r.Close()
        if ($c -eq 0) { Write-Output "(no rows)" }
    } catch { Write-Output $_.Exception.Message; try { $r.Close() } catch {} }
}

Dump "SELECT COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='cCore_ReportColumnsFunction_0' ORDER BY ORDINAL_POSITION" "function table cols"

Dump @"
SELECT f.*, c.sAliasName, c.iMiscOption
FROM dbo.cCore_ReportColumnsFunction_0 f
JOIN dbo.cCore_ReportColumns_0 c ON c.iColumnId = f.iColumnId
JOIN dbo.cCore_ReportLayouts_0 l ON l.iLayoutId = c.iLayoutId
WHERE l.iReportId IN (70267, 70268, 560)
ORDER BY l.iReportId, c.iFieldId
"@ "70267/70268/560 column functions"
