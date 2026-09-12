$cs = "Server=localhost;Database=Focus80E0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
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
                    if ($v.Length -gt 220) { $v = $v.Substring(0,220) + "..." }
                    $parts += $v
                }
            }
            Write-Output ($parts -join " | ")
            if ($c -ge 30) { break }
        }
        $r.Close()
        if ($c -eq 0) { Write-Output "(no rows)" }
    } catch {
        Write-Output ("ERROR: " + $_.Exception.Message)
    }
}

Dump @"
SELECT iReportId, sReportName, iReportType, iSourceType, iFilterGroupId
FROM dbo.cCore_Reports_0
WHERE sReportName LIKE N'%Commission%' OR sReportName LIKE N'%commission%'
   OR sReportName LIKE N'%iii%' OR sReportName LIKE N'%Query%'
ORDER BY iReportId
"@ "reports"

Dump @"
SELECT TOP 15 iParameterId, iReportId, sFieldName, sFieldVariable, iControlType, iFieldType, iSelectionMode, iType
FROM dbo.cCore_ReportParameter_0
WHERE sFieldName LIKE N'%Date%' OR sFieldVariable LIKE N'%Date%' OR iControlType IN (2,3,4)
ORDER BY iReportId
"@ "date parameters"

Dump @"
SELECT COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME='cCore_ReportColumns_0'
  AND (COLUMN_NAME LIKE '%Type%' OR COLUMN_NAME LIKE '%Name%' OR COLUMN_NAME LIKE '%Field%' OR COLUMN_NAME LIKE '%Misc%')
ORDER BY ORDINAL_POSITION
"@ "report column type cols"

$conn.Close()
