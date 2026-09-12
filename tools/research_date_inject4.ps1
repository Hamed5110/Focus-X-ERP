$cs = "Server=localhost;Database=Focus80E0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 180
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
                    if ($v.Length -gt 350) { $v = $v.Substring(0,350) + "..." }
                    $parts += $v
                }
            }
            Write-Output ($parts -join " | ")
            if ($c -ge 120) { break }
        }
        $r.Close()
        if ($c -eq 0) { Write-Output "(no rows)" }
    } catch {
        Write-Output ("ERROR: " + $_.Exception.Message)
    }
}

Dump @"
SELECT iColumnId, iFieldId, sColumn, sAliasName, iType, iDecimalInColumn, iMiscOption, iAlignment, sFormat
FROM dbo.cCore_ReportColumns_0
WHERE iLayoutId = 6916
ORDER BY iFieldId, iColumnId
"@ "70266 Standard columns"

Dump @"
SELECT l.iLayoutId, l.iReportId, r.sReportName, r.iReportType, l.sLayoutName,
       l.iFlag, l.bDefault, l.iDateRangeType, l.iDateFormateType, l.bPrintZero
FROM dbo.cCore_ReportLayouts_0 l
INNER JOIN dbo.cCore_Reports_0 r ON r.iReportId = l.iReportId
WHERE r.iReportType = 1
ORDER BY l.iReportId
"@ "all query layout date flags"

Dump @"
SELECT l.iLayoutId, l.iReportId, r.sReportName, r.iReportType,
       l.iDateRangeType, l.iDateFormateType, l.iFlag
FROM dbo.cCore_ReportLayouts_0 l
INNER JOIN dbo.cCore_Reports_0 r ON r.iReportId = l.iReportId
WHERE r.sReportName LIKE N'%Commission%' OR r.iReportId IN (70219,70220,70221,70256,70259,70074)
ORDER BY l.iReportId
"@ "commission + known reports layout dates"

Dump @"
SELECT DISTINCT iDateRangeType, iDateFormateType, COUNT(*) Cnt
FROM dbo.cCore_ReportLayouts_0
GROUP BY iDateRangeType, iDateFormateType
ORDER BY Cnt DESC
"@ "date flag distribution"

Dump @"
SELECT TOP 15 iParameterId, iReportId, *
FROM dbo.cCore_ReportParameter_0
WHERE iReportId IN (70266, 70074, 70256, 70259)
"@ "parameters for key reports"

$conn.Close()
