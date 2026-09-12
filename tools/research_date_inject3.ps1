$cs = "Server=localhost;Database=Focus80E0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$cmd = $conn.CreateCommand()
$cmd.CommandTimeout = 180

function Dump($sql, $title) {
    Write-Output ""
    Write-Output "========== $title =========="
    $cmd.CommandText = $sql
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
                    if ($v.Length -gt 600) { $v = $v.Substring(0,600) + "..." }
                    $parts += $v
                }
            }
            Write-Output ($parts -join " | ")
            if ($c -ge 80) { break }
        }
        $r.Close()
        if ($c -eq 0) { Write-Output "(no rows)" }
    } catch {
        Write-Output ("ERROR: " + $_.Exception.Message)
    }
}

# Save full 70266 SQL
$cmd.CommandText = "SELECT sSqlQuery FROM dbo.cCore_RDQuery_0 WHERE iReportId = 70266"
$sql70266 = [string]$cmd.ExecuteScalar()
$out = "C:\Users\Hamed Ali Khan\Focus-X-ERP\tools\sql_70266_saved.sql"
[System.IO.File]::WriteAllText($out, $sql70266)
Write-Output ("70266 SQL length={0} saved to {1}" -f $sql70266.Length, $out)
$whereHits = [regex]::Matches($sql70266, '(?i)\bwhere\b')
Write-Output ("WHERE count={0}" -f $whereHits.Count)
foreach ($m in $whereHits) {
    $start = [Math]::Max(0, $m.Index - 80)
    $len = [Math]::Min(220, $sql70266.Length - $start)
    $snip = $sql70266.Substring($start, $len) -replace "`r|`n"," "
    Write-Output ("  pos={0}: ...{1}..." -f $m.Index, $snip)
}

$idateHits = [regex]::Matches($sql70266, '(?i)\biDate\b')
Write-Output ("iDate count={0}" -f $idateHits.Count)
foreach ($m in $idateHits) {
    $start = [Math]::Max(0, $m.Index - 50)
    $len = [Math]::Min(140, $sql70266.Length - $start)
    $snip = $sql70266.Substring($start, $len) -replace "`r|`n"," "
    Write-Output ("  pos={0}: ...{1}..." -f $m.Index, $snip)
}

Dump @"
SELECT iColumnId, iFieldId, sAlias, iType, iDecimalInColumn, iMiscOption, iAlignment
FROM dbo.cCore_ReportColumns_0
WHERE iLayoutId IN (SELECT iLayoutId FROM dbo.cCore_ReportLayouts_0 WHERE iReportId = 70266)
ORDER BY iFieldId
"@ "70266 layout columns"

Dump @"
SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME='cCore_ReportColumns_0' ORDER BY ORDINAL_POSITION
"@ "ReportColumns cols"

Dump @"
SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME='cCore_ReportLayouts_0' ORDER BY ORDINAL_POSITION
"@ "ReportLayouts cols"

Dump @"
SELECT iLayoutId, iReportId, sLayoutName
FROM dbo.cCore_ReportLayouts_0
WHERE iReportId IN (70266, 70074, 70256, 70198)
"@ "layouts for key reports"

$conn.Close()
