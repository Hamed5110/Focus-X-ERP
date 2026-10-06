function DumpDb($db, $sql, $title) {
    Write-Output ""
    Write-Output "========== [$db] $title =========="
    $cs = "Server=localhost;Database=$db;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
    $conn = New-Object System.Data.SqlClient.SqlConnection $cs
    $conn.Open()
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
                    if ($v.Length -gt 400) { $v = $v.Substring(0,400) + "..." }
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
    $conn.Close()
}

DumpDb Focus80G0 @"
SELECT TOP 30 r.iReportId, r.sReportName, r.iReportType, r.iSourceType
FROM dbo.cCore_Reports_0 r
ORDER BY r.iReportId DESC
"@ "G0 latest reports"

DumpDb Focus80G0 @"
SELECT r.iReportId, r.sReportName, r.iReportType, r.iSourceType
FROM dbo.cCore_Reports_0 r
WHERE r.iReportType = 1 AND r.iReportId >= 70260
ORDER BY r.iReportId
"@ "G0 query reports 70260+"

DumpDb Focus80G0 @"
SELECT r.iReportId, r.sReportName, LEN(q.sSqlQuery) SqlLen,
  CASE WHEN q.sSqlQuery LIKE '%Opening Balance%' THEN 1 ELSE 0 END HasOpen,
  CASE WHEN q.sSqlQuery LIKE '%8707%' THEN 1 ELSE 0 END Has8707,
  CASE WHEN q.sSqlQuery LIKE '%@CustomerName%' THEN 1 ELSE 0 END HasCust
FROM dbo.cCore_RDQuery_0 q
JOIN dbo.cCore_Reports_0 r ON r.iReportId = q.iReportId
WHERE q.sSqlQuery LIKE '%Opening Balance%'
   OR q.sSqlQuery LIKE '%@CustomerName%'
   OR q.sSqlQuery LIKE '%New Business%'
   OR q.sSqlQuery LIKE '%8707, 8708%'
ORDER BY r.iReportId
"@ "G0 query with statement markers"

DumpDb Focus80E0 @"
SELECT TOP 20 r.iReportId, r.sReportName, r.iReportType, r.iSourceType
FROM dbo.cCore_Reports_0 r
ORDER BY r.iReportId DESC
"@ "E0 latest reports"

DumpDb Focus80G0 @"
SELECT TOP 15 c.iColumnId, c.iLayoutId, l.iReportId, r.sReportName, c.iFieldId, c.iType, c.sAliasName, c.iDecimalInColumn, c.iMiscOption
FROM dbo.cCore_ReportColumns_0 c
JOIN dbo.cCore_ReportLayouts_0 l ON l.iLayoutId = c.iLayoutId
JOIN dbo.cCore_Reports_0 r ON r.iReportId = l.iReportId
WHERE c.sAliasName IN ('Contract Date','Receipt Date','Balance')
  AND r.iReportType = 1
ORDER BY l.iReportId DESC, c.iFieldId
"@ "G0 Query layouts with those aliases"
