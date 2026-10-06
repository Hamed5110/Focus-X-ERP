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
                    if ($v.Length -gt 700) { $v = $v.Substring(0,700) + "..." }
                    $parts += $v
                }
            }
            Write-Output ($parts -join " | ")
            if ($c -ge 150) { Write-Output "(truncated)"; break }
        }
        $r.Close()
        if ($c -eq 0) { Write-Output "(no rows)" }
    } catch {
        Write-Output ("ERROR: " + $_.Exception.Message)
        try { $r.Close() } catch {}
    }
}

Dump @"
SELECT iReportId, sReportName, iModule, iReportType, iSourceType
FROM dbo.cCore_Reports_0
WHERE sReportName LIKE N'%Statement of%'
   OR sReportName LIKE N'%of Customer%'
   OR sReportName LIKE N'%Purchase Order Last%'
   OR sReportName LIKE N'%Monthly Sales%'
   OR sReportName LIKE N'%Atlas Aluminum Detail%'
ORDER BY iReportId
"@ "new reports by name"

Dump @"
SELECT r.iReportId, r.sReportName, r.iReportType, r.iSourceType,
       p.sFieldName, p.sFieldVariable, p.iControlType, p.iFieldType, p.iSelectionMode, p.iFieldId
FROM dbo.cCore_ReportParameter_0 p
JOIN dbo.cCore_Reports_0 r ON r.iReportId = p.iReportId
WHERE p.iControlType = 1
ORDER BY r.iReportId
"@ "all Account controlType=1 parameters"

Dump @"
SELECT r.iReportId, r.sReportName, LEN(q.sSqlQuery) AS SqlLen, LEFT(q.sSqlQuery, 400) AS Head
FROM dbo.cCore_Reports_0 r
JOIN dbo.cCore_RDQuery_0 q ON q.iReportId = r.iReportId
WHERE r.iReportId IN (70036, 70074, 70267, 70125, 70115, 70066)
ORDER BY r.iReportId
"@ "sql heads"

Dump @"
SELECT l.iLayoutId, r.iReportId, r.sReportName, c.iFieldId, c.iType, c.sColumn, c.sAliasName,
       c.iMiscOption, c.iAlignment, c.iDecimalInColumn, c.sFormat
FROM dbo.cCore_ReportLayouts_0 l
JOIN dbo.cCore_Reports_0 r ON r.iReportId = l.iReportId
JOIN dbo.cCore_ReportColumns_0 c ON c.iLayoutId = l.iLayoutId
WHERE r.iReportId IN (70267, 70074, 70036)
ORDER BY r.iReportId, c.iFieldId
"@ "working query layouts"

Dump @"
SELECT COLUMN_NAME, DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'vtCode_DataFA_0'
ORDER BY ORDINAL_POSITION
"@ "vtCode_DataFA_0 columns"

Dump @"
SELECT COLUMN_NAME, DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'vCore_TranData_0'
ORDER BY ORDINAL_POSITION
"@ "vCore_TranData_0 columns"

Dump @"
SELECT COLUMN_NAME, DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'vtAccount_DrCr_0'
ORDER BY ORDINAL_POSITION
"@ "vtAccount_DrCr_0 columns"

Dump @"
SELECT COLUMN_NAME, DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'vCore_AccountBalances_0'
ORDER BY ORDINAL_POSITION
"@ "vCore_AccountBalances_0 columns"
