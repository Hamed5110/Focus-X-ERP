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
                    if ($v.Length -gt 900) { $v = $v.Substring(0,900) + "..." }
                    $parts += $v
                }
            }
            Write-Output ($parts -join " | ")
            if ($c -ge 120) { Write-Output "(truncated)"; break }
        }
        $r.Close()
        if ($c -eq 0) { Write-Output "(no rows)" }
    } catch {
        Write-Output ("ERROR: " + $_.Exception.Message)
        try { $r.Close() } catch {}
    }
}

Dump @"
SELECT COUNT(*) AS ViewCount FROM sys.views
"@ "view count"

Dump @"
SELECT OBJECT_DEFINITION(OBJECT_ID('dbo.IntToDate')) AS Def
"@ "IntToDate"

Dump @"
SELECT OBJECT_DEFINITION(OBJECT_ID('dbo.DateToInt')) AS Def
"@ "DateToInt"

Dump @"
SELECT OBJECT_DEFINITION(OBJECT_ID('dbo.fCore_IntToDateTime')) AS Def
"@ "fCore_IntToDateTime"

Dump @"
SELECT
  SCHEMA_NAME(v.schema_id) AS Sch,
  v.name,
  CASE WHEN d LIKE N'%CONVERT(date%' OR d LIKE N'%CONVERT(DATE%' THEN 1 ELSE 0 END AS HasConvertDate,
  CASE WHEN d LIKE N'%CONVERT(datetime%' THEN 1 ELSE 0 END AS HasConvertDatetime,
  CASE WHEN d LIKE N'%, 112)%' OR d LIKE N%',112)%' THEN 1 ELSE 0 END AS HasStyle112,
  CASE WHEN d LIKE N'%CAST(%AS DATE%' OR d LIKE N'%AS date%' THEN 1 ELSE 0 END AS HasCastDate,
  CASE WHEN d LIKE N'%IntToDate%' THEN 1 ELSE 0 END AS HasIntToDate,
  CASE WHEN d LIKE N'%DATEFROMPARTS%' THEN 1 ELSE 0 END AS HasDateFromParts,
  CASE WHEN d LIKE N'%iDate%' THEN 1 ELSE 0 END AS HasiDate
FROM sys.views v
CROSS APPLY (SELECT ISNULL(OBJECT_DEFINITION(v.object_id), N'') AS d) x
WHERE d LIKE N'%CONVERT(date%'
   OR d LIKE N'%CONVERT(DATE%'
   OR d LIKE N'%, 112)%'
   OR d LIKE N%',112)%'
   OR d LIKE N'%CAST(%AS DATE%'
   OR d LIKE N'%AS date)%'
   OR d LIKE N'%IntToDate%'
   OR d LIKE N'%DATEFROMPARTS%'
ORDER BY v.name
"@ "views with date conversion"

Dump @"
SELECT r.iReportId, r.sReportName, r.iReportType, r.iSourceType
FROM dbo.cCore_Reports_0 r
WHERE r.sReportName LIKE N'%Statement%'
   OR r.sReportName LIKE N'%Customer%'
ORDER BY r.iReportId
"@ "customer/statement reports"

Dump @"
SELECT r.iReportId, r.sReportName, c.iFieldId, c.iType, c.sColumn, c.sAliasName, c.sFormat, c.iDecimalInColumn, c.iMiscOption
FROM dbo.cCore_Reports_0 r
JOIN dbo.cCore_ReportLayouts_0 l ON l.iReportId = r.iReportId
JOIN dbo.cCore_ReportColumns_0 c ON c.iLayoutId = l.iLayoutId
WHERE r.sReportName LIKE N'%Customer statement%'
   OR r.sReportName LIKE N'%Customer Statement%'
   OR r.sReportName LIKE N'%Statement of Customer%'
ORDER BY r.iReportId, c.iFieldId
"@ "customer statement layout types"

$conn.Close()
