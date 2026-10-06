$cs = "Server=localhost;Database=Focus80G0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
function Dump($sql, $title) {
    Write-Output ""
    Write-Output "========== $title =========="
    $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 180; $cmd.CommandText = $sql
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
                    if ($v.Length -gt 800) { $v = $v.Substring(0,800) + "..." }
                    $parts += $v
                }
            }
            Write-Output ($parts -join " | ")
            if ($c -ge 80) { Write-Output "(truncated)"; break }
        }
        $r.Close()
        if ($c -eq 0) { Write-Output "(no rows)" }
    } catch {
        Write-Output ("ERROR: " + $_.Exception.Message)
        try { $r.Close() } catch {}
    }
}

Dump @"
SELECT iReportId, sReportName, iReportType, iSourceType
FROM dbo.cCore_Reports_0
WHERE iReportId >= 70262
ORDER BY iReportId
"@ "reports 70262+"

Dump @"
SELECT r.iReportId, r.sReportName, c.iFieldId, c.iType, c.sColumn, c.sAliasName, c.iDecimalInColumn, c.iMiscOption
FROM dbo.cCore_Reports_0 r
JOIN dbo.cCore_ReportLayouts_0 l ON l.iReportId = r.iReportId
JOIN dbo.cCore_ReportColumns_0 c ON c.iLayoutId = l.iLayoutId
WHERE r.iReportId >= 70262
ORDER BY r.iReportId, c.iFieldId
"@ "new report layouts"

$cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 300
$cmd.CommandText = "SELECT SCHEMA_NAME(schema_id) AS Sch, name, OBJECT_DEFINITION(object_id) AS Def FROM sys.views"
$r = $cmd.ExecuteReader()
Write-Output ""
Write-Output "========== CAST AS date / CONVERT date / style 112 / DATEFROMPARTS exact =========="
while ($r.Read()) {
    $name = [string]$r["name"]
    $d = if ($r.IsDBNull($r.GetOrdinal("Def"))) { "" } else { [string]$r["Def"] }
    $keep = $false
    if ($d -match '(?i)CONVERT\s*\(\s*date') { $keep = $true }
    if ($d -match '(?i),\s*112\s*\)') { $keep = $true }
    if ($d -match '(?i)AS\s+date\b') { $keep = $true }
    if ($d -match '(?i)DATEFROMPARTS') { $keep = $true }
    if ($d -match '(?i)vtCode_DataFA') { $keep = $true }
    if (-not $keep) { continue }
    Write-Output ("--- dbo.{0} ---" -f $name)
    $i = 0
    foreach ($ln in ($d -split "`n")) {
        $i++
        if ($ln -match '(?i)CONVERT\s*\(\s*date|,\s*112\s*\)|AS\s+date\b|DATEFROMPARTS|iDate|IntToDate') {
            $t = $ln.Trim()
            if ($t.Length -gt 260) { $t = $t.Substring(0,260) + "..." }
            Write-Output ("L{0}: {1}" -f $i, $t)
        }
    }
}
$r.Close()

Dump @"
SELECT TOP 5 OBJECT_DEFINITION(OBJECT_ID('dbo.vmCore_Account')) AS AccView
"@ "sample master view (expect IntToDate on created)"

$conn.Close()
