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
                if ($r.IsDBNull($i)) { $parts += "" } else {
                    $v = [string]$r.GetValue($i)
                    $v = $v -replace "`r|`n"," "
                    if ($v.Length -gt 80) { $v = $v.Substring(0,80) + "..." }
                    $parts += $v
                }
            }
            Write-Output ($parts -join " | ")
            if ($c -ge 50) { Write-Output "(truncated)"; break }
        }
        $r.Close()
        if ($c -eq 0) { Write-Output "(no rows)" }
    } catch {
        Write-Output ("ERROR: " + $_.Exception.Message)
        try { $r.Close() } catch {}
    }
}

Dump @"
SELECT TOP 20 c.sAliasName, c.sGroupName, c.iParentId, c.iSubParentId, c.iMiscOption, c.iType, r.sReportName, r.iGroupStyle
FROM dbo.cCore_ReportColumns_0 c
JOIN dbo.cCore_ReportLayouts_0 l ON l.iLayoutId = c.iLayoutId
JOIN dbo.cCore_Reports_0 r ON r.iReportId = l.iReportId
WHERE ISNULL(c.sGroupName, N'') <> N''
"@ "columns with sGroupName"

Dump @"
SELECT iReportType, iSourceType, iGroupStyle, COUNT(*) AS n
FROM dbo.cCore_Reports_0
GROUP BY iReportType, iSourceType, iGroupStyle
ORDER BY n DESC
"@ "report type mix"

Dump @"
SELECT TOP 15 r.iReportId, r.sReportName, r.iReportType, r.iSourceType, r.iGroupStyle
FROM dbo.cCore_Reports_0 r
WHERE r.iReportType = 1 AND r.sReportName LIKE N'%Commission%' OR r.sReportName LIKE N'%Detail%'
ORDER BY r.iReportId DESC
"@ "query detail reports"
