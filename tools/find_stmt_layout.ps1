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
                    if ($v.Length -gt 500) { $v = $v.Substring(0,500) + "..." }
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
    $conn.Close()
}

foreach ($db in @("Focus80G0","Focus80E0","Focus8080")) {
    DumpDb $db @"
SELECT r.iReportId, r.sReportName, r.iReportType, r.iSourceType
FROM dbo.cCore_Reports_0 r
WHERE r.sReportName LIKE '%Statement of Customer%'
   OR r.sReportName LIKE '%Customer Statement%'
   OR r.sReportName LIKE '%Statement of%'
ORDER BY r.iReportId
"@ "statement report names"

    DumpDb $db @"
SELECT r.iReportId, r.sReportName, c.iColumnId, c.iFieldId, c.iType, c.sColumn, c.sAliasName, c.iDecimalInColumn, c.iMiscOption, c.sFormat
FROM dbo.cCore_Reports_0 r
JOIN dbo.cCore_ReportLayouts_0 l ON l.iReportId = r.iReportId
JOIN dbo.cCore_ReportColumns_0 c ON c.iLayoutId = l.iLayoutId
WHERE c.sAliasName LIKE '%Contract Date%'
   OR c.sAliasName LIKE '%Receipt Date%'
   OR (c.sAliasName LIKE '%Date%' AND r.sReportName LIKE '%Statement%')
ORDER BY r.iReportId, c.iFieldId
"@ "layout columns named Contract/Receipt Date"

    DumpDb $db @"
SELECT r.iReportId, r.sReportName, CASE WHEN q.sSqlQuery LIKE '%Opportunity Type%' THEN 1 ELSE 0 END AS HasOpp,
 CASE WHEN q.sSqlQuery LIKE '%vtCode_DataFA_0%' THEN 1 ELSE 0 END AS HasFA,
 LEN(q.sSqlQuery) AS SqlLen
FROM dbo.cCore_RDQuery_0 q
JOIN dbo.cCore_Reports_0 r ON r.iReportId = q.iReportId
WHERE q.sSqlQuery LIKE '%vtCode_DataFA_0%'
   OR q.sSqlQuery LIKE '%Statement of Customer%'
   OR q.sSqlQuery LIKE '%[Opportunity Type]%'
   OR q.sSqlQuery LIKE '%Contract Date%'
ORDER BY r.iReportId
"@ "RDQuery matching statement SQL"
}
