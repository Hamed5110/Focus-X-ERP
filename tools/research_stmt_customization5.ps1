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
                    if ($v.Length -gt 1000) { $v = $v.Substring(0,1000) + "..." }
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

DumpDb Focus80E0 @"
SELECT l.iLayoutId, c.iColumnId, c.iFieldId, c.iType, c.sColumn, c.sAliasName, c.iMiscOption, c.iAlignment, c.iDecimalInColumn, c.sFormat
FROM dbo.cCore_ReportLayouts_0 l
JOIN dbo.cCore_ReportColumns_0 c ON c.iLayoutId = l.iLayoutId
WHERE l.iReportId = 70268
ORDER BY c.iFieldId
"@ "70268 detail layout"

DumpDb Focus80E0 @"
SELECT CASE WHEN sSqlQuery LIKE N'%TOP 100%' THEN 1 ELSE 0 END AS HasTop,
       CASE WHEN sSqlQuery LIKE N'%ORDER BY%' THEN 1 ELSE 0 END AS HasOrder,
       CASE WHEN sSqlQuery LIKE N'%OVER (%' THEN 1 ELSE 0 END AS HasOver,
       CASE WHEN sSqlQuery LIKE N'%NULLIF%' THEN 1 ELSE 0 END AS HasNullIf,
       CASE WHEN sSqlQuery LIKE N'%AS iDate%' THEN 1 ELSE 0 END AS HasIDate,
       CASE WHEN sSqlQuery LIKE N'%nvarchar(10)%' THEN 1 ELSE 0 END AS HasNv10,
       CASE WHEN sSqlQuery LIKE N'%N''Yes''%' THEN 1 ELSE 0 END AS HasYes,
       LEFT(sSqlQuery, 500) AS Head
FROM dbo.cCore_RDQuery_0
WHERE iReportId = 70268
"@ "70268 sql flags"

DumpDb Focus80E0 @"
SELECT iReportId, sReportName, iReportType, iSourceType
FROM dbo.cCore_Reports_0
WHERE sReportName LIKE N'%Statement of%'
   OR sReportName LIKE N'%Last Rate%'
   OR sReportName LIKE N'%Cost Reduction%'
   OR sReportName LIKE N'%Purchase Order Last%'
ORDER BY iReportId
"@ "E0 new named reports"

DumpDb Focus80E0 @"
SELECT l.iLayoutId, c.iFieldId, c.iType, c.sColumn, c.sAliasName, c.iMiscOption
FROM dbo.cCore_ReportLayouts_0 l
JOIN dbo.cCore_ReportColumns_0 c ON c.iLayoutId = l.iLayoutId
JOIN dbo.cCore_Reports_0 r ON r.iReportId = l.iReportId
WHERE r.iReportId = 70266
ORDER BY c.iFieldId
"@ "E0 70266 layout (Query type 1)"

DumpDb Focus80G0 @"
SELECT c.iFieldId, c.iType, c.sColumn, c.sAliasName, c.iMiscOption
FROM dbo.cCore_ReportLayouts_0 l
JOIN dbo.cCore_ReportColumns_0 c ON c.iLayoutId = l.iLayoutId
WHERE l.iReportId = 70201
ORDER BY c.iFieldId
"@ "G0 70201 Detailed Trial Query layout"

DumpDb Focus80E0 @"
SELECT p.sFieldName, p.sFieldVariable, p.iControlType, p.iFieldType, p.iSelectionMode, p.iFieldId, p.sDefault, p.sValue
FROM dbo.cCore_ReportParameter_0 p
WHERE p.iReportId IN (70266, 70267, 70268, 70260, 70261)
"@ "E0 query report parameters"
