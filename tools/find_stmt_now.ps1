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
                    if ($v.Length -gt 600) { $v = $v.Substring(0,600) + "..." }
                    $parts += $v
                }
            }
            Write-Output ($parts -join " | ")
            if ($c -ge 60) { Write-Output "(truncated)"; break }
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
   OR r.sReportName LIKE '%Statement of Cust%'
   OR r.sReportName LIKE '%of Customer%'
ORDER BY r.iReportId
"@ "name match"

    DumpDb $db @"
SELECT TOP 20 r.iReportId, r.sReportName, r.iReportType, LEN(q.sSqlQuery) SqlLen,
  CASE WHEN q.sSqlQuery LIKE '%Hussain%' THEN 1 ELSE 0 END H,
  CASE WHEN q.sSqlQuery LIKE '%Contract Date%' THEN 1 ELSE 0 END CD,
  CASE WHEN q.sSqlQuery LIKE '%decimal(18, 0)) AS [Contract Date]%' THEN 1 ELSE 0 END DecDate,
  CASE WHEN q.sSqlQuery LIKE '%nvarchar(10)) AS [Contract Date]%' THEN 1 ELSE 0 END NvDate
FROM dbo.cCore_RDQuery_0 q
JOIN dbo.cCore_Reports_0 r ON r.iReportId = q.iReportId
WHERE q.sSqlQuery LIKE '%Contract Date%'
   OR q.sSqlQuery LIKE '%@CustomerName%'
   OR r.sReportName LIKE '%Customer%'
ORDER BY r.iReportId DESC
"@ "query/customer"
}
