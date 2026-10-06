$cs = "Server=localhost;Database=Focus80G0;User Id=sa;Password=$env:FOCUS_SQL_PASSWORD;Encrypt=False;TrustServerCertificate=True;"
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
                    if ($v.Length -gt 110) { $v = $v.Substring(0,110) + "..." }
                    $parts += $v
                }
            }
            Write-Output ($parts -join " | ")
            if ($c -ge 45) { break }
        }
        $r.Close()
        if ($c -eq 0) { Write-Output "(no rows)" }
    } catch { Write-Output $_.Exception.Message }
}

Dump @"
SELECT p.iControlType, COUNT(*) AS Cnt,
       MIN(p.sFieldName) AS ExName, MIN(p.sFieldVariable) AS ExVar,
       MIN(r.sReportName) AS ExReport
FROM dbo.cCore_ReportParameter_0 p
JOIN dbo.cCore_Reports_0 r ON r.iReportId = p.iReportId
GROUP BY p.iControlType
ORDER BY p.iControlType
"@ "control types in use"

Dump @"
SELECT TOP 25 p.iReportId, r.sReportName, p.sFieldName, p.sFieldVariable,
       p.iControlType, p.iFieldType, p.iSelectionMode, p.iFieldId, p.bGroup,
       LEFT(ISNULL(p.sValue,N''),90) Val, LEFT(ISNULL(p.sDefault,N''),90) Def
FROM dbo.cCore_ReportParameter_0 p
JOIN dbo.cCore_Reports_0 r ON r.iReportId = p.iReportId
WHERE p.iControlType IN (0,2,3,4,5,6,7,8,9,10,11,12)
   OR p.sFieldName LIKE N'%Select%'
   OR p.sFieldVariable LIKE N'%Select%'
   OR ISNULL(p.sValue,N'') <> N''
ORDER BY p.iControlType, p.iReportId
"@ "selection-like params"

Dump @"
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME LIKE N'%cCore%Field%' OR TABLE_NAME LIKE N'%Enum%' OR TABLE_NAME LIKE N'%ControlType%'
ORDER BY TABLE_NAME
"@ "field/enum tables"

Dump @"
SELECT TOP 30 iId, sName FROM dbo.cCore_Fields
WHERE sName LIKE N'%Control%' OR sName LIKE N'%Parameter%' OR sName LIKE N'%Selection%'
"@ "cCore_Fields try"
