$cs = "Server=localhost;Database=Focus80G0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
function Dump($sql, $title) {
    Write-Output ""
    Write-Output "========== $title =========="
    $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 120; $cmd.CommandText = $sql
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
                if ($r.IsDBNull($i)) { $parts += "NULL" } else {
                    $v = [string]$r.GetValue($i)
                    $v = $v -replace "`r|`n"," "
                    if ($v.Length -gt 400) { $v = $v.Substring(0,400) + "..." }
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
}

Dump @"
SELECT r.iReportId, r.sReportName, r.iReportType, r.iSourceType
FROM dbo.cCore_Reports_0 r
WHERE r.sReportName LIKE '%Ledger%'
   OR r.sReportName LIKE '%ledger%'
   OR r.sReportName LIKE '%Day Book%'
   OR r.sReportName LIKE '%Account Book%'
   OR r.sReportName LIKE '%statements%'
ORDER BY r.iReportId
"@ "ledger/statement reports"

Dump @"
SELECT COLUMN_NAME, DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'cCore_Reports_0'
ORDER BY ORDINAL_POSITION
"@ "cCore_Reports_0 columns"

Dump @"
SELECT c.iFieldId, c.sColumn, c.sAliasName, c.iType, c.iMiscOption, c.iAlignment, c.iDecimalInColumn, c.sFormat, c.iParentId
FROM dbo.cCore_ReportLayouts_0 l
JOIN dbo.cCore_ReportColumns_0 c ON c.iLayoutId = l.iLayoutId
WHERE l.iReportId = 560
ORDER BY c.iFieldId
"@ "560 ALL columns"

Dump @"
SELECT p.sFieldName, p.sFieldVariable, p.iControlType, p.iFieldType, p.iSelectionMode, p.iFieldId, LEFT(ISNULL(p.sDefault,N''),80) Def, LEFT(ISNULL(p.sValue,N''),80) Val
FROM dbo.cCore_ReportParameter_0 p
WHERE p.iReportId = 560
"@ "560 parameters"
