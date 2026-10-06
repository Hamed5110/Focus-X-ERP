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
                    if ($v.Length -gt 900) { $v = $v.Substring(0,900) + "..." }
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

DumpDb Focus80G0 @"
SELECT iReportId, sReportName, iReportType, iSourceType
FROM dbo.cCore_Reports_0
WHERE sReportName LIKE N'%Detail%'
   OR sReportName LIKE N'%Statement of Customer%'
   OR iReportId >= 70266
ORDER BY iReportId
"@ "recent / detail reports"

DumpDb Focus80E0 @"
SELECT iReportId, sReportName, iReportType, iSourceType
FROM dbo.cCore_Reports_0
WHERE sReportName LIKE N'%Statement%'
   OR sReportName LIKE N'%of Customer%'
   OR iReportId >= 70260
ORDER BY iReportId
"@ "E0 statement / recent reports"

DumpDb Focus80G0 @"
SELECT CASE
         WHEN sSqlQuery LIKE N'%AS iDate%' THEN 1 ELSE 0 END AS HasIDate,
       CASE WHEN sSqlQuery LIKE N'%ORDER BY%' THEN 1 ELSE 0 END AS HasOrder,
       CASE WHEN sSqlQuery LIKE N'%nvarchar(10)%' THEN 1 ELSE 0 END AS HasNvarchar10,
       CASE WHEN sSqlQuery LIKE N'%Contract Date%' THEN 1 ELSE 0 END AS HasContractDate,
       RIGHT(sSqlQuery, 300) AS Tail
FROM dbo.cCore_RDQuery_0
WHERE iReportId = 70267
"@ "70267 sql flags/tail"

DumpDb Focus80G0 @"
SELECT l.iLayoutId, c.iFieldId, c.iType, c.sColumn, c.sAliasName, c.iMiscOption
FROM dbo.cCore_ReportLayouts_0 l
JOIN dbo.cCore_ReportColumns_0 c ON c.iLayoutId = l.iLayoutId
WHERE l.iReportId = 70267
ORDER BY c.iFieldId
"@ "70267 all layout cols"

DumpDb Focus80G0 @"
SELECT TOP 5 r.iReportId, r.sReportName, c.iFieldId, c.iType, c.sAliasName, c.sColumn
FROM dbo.cCore_Reports_0 r
JOIN dbo.cCore_RDQuery_0 q ON q.iReportId = r.iReportId
JOIN dbo.cCore_ReportLayouts_0 l ON l.iReportId = r.iReportId
JOIN dbo.cCore_ReportColumns_0 c ON c.iLayoutId = l.iLayoutId
WHERE r.iReportType = 2 AND r.iSourceType = 1
  AND (c.sAliasName LIKE N'%Date%' OR c.sColumn LIKE N'%Date%' OR c.sAliasName = N'iDate')
ORDER BY r.iReportId, c.iFieldId
"@ "Query-dataset Date/iDate columns"

DumpDb Focus80G0 @"
SELECT name FROM sys.views
WHERE name LIKE 'vtCode%' OR name LIKE 'vCore_%FA%' OR name LIKE '%Ledger%' OR name LIKE '%DrCr%'
ORDER BY name
"@ "FA/ledger views"

DumpDb Focus80G0 @"
SELECT TOP 20 c.name, t.name AS typ
FROM sys.columns c
JOIN sys.types t ON t.user_type_id = c.user_type_id
WHERE c.object_id = OBJECT_ID('dbo.vmCore_Account')
ORDER BY c.column_id
"@ "vmCore_Account cols"
