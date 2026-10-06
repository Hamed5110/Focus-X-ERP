$cs = "Server=localhost;Database=Focus80G0;User Id=sa;Password=$env:FOCUS_SQL_PASSWORD;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
function Dump($sql, $title, $max=40) {
    Write-Output ""
    Write-Output "========== $title =========="
    $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 60; $cmd.CommandText = $sql
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
                    if ($v.Length -gt 100) { $v = $v.Substring(0,100) + "..." }
                    $parts += $v
                }
            }
            Write-Output ($parts -join " | ")
            if ($c -ge $max) { break }
        }
        $r.Close()
        if ($c -eq 0) { Write-Output "(no rows)" }
    } catch { Write-Output $_.Exception.Message }
}

Dump @"
SELECT IDENT_CURRENT('cCore_Reports_0') RepIdent,
       IDENT_CURRENT('cCore_ReportLayouts_0') LayIdent,
       IDENT_CURRENT('cCore_ReportColumns_0') ColIdent,
       IDENT_CURRENT('cCore_ReportParameter_0') ParIdent,
       (SELECT MAX(iReportId) FROM dbo.cCore_Reports_0) MaxRep,
       (SELECT MAX(iLayoutId) FROM dbo.cCore_ReportLayouts_0) MaxLay,
       (SELECT MAX(iColumnId) FROM dbo.cCore_ReportColumns_0) MaxCol,
       (SELECT MAX(iParameterId) FROM dbo.cCore_ReportParameter_0) MaxPar
"@ "idents"

Dump @"
SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = N'cCore_ReportColumns_0' ORDER BY ORDINAL_POSITION
"@ "column cols"

Dump @"
SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = N'cCore_ReportLayouts_0' ORDER BY ORDINAL_POSITION
"@ "layout cols"

Dump @"
SELECT TOP 1 * FROM dbo.cCore_Reports_0 WHERE iReportId = 70241
"@ "70241 report row"

Dump @"
SELECT TOP 1 * FROM dbo.cCore_ReportLayouts_0 WHERE iReportId = 70241
"@ "70241 layout"

Dump @"
SELECT TOP 25 iColumnId, iLayoutId, iFieldId, sAliasName, iType, iAlignment, iWidth, iMiscellaneous
FROM dbo.cCore_ReportColumns_0
WHERE iLayoutId = (SELECT iLayoutId FROM dbo.cCore_ReportLayouts_0 WHERE iReportId = 70241)
ORDER BY iColumnId
"@ "70241 columns"

Dump @"
SELECT TOP 5 iId, iParentId, sCaption, iReportId, iMenuId
FROM dbo.cCore_AppMenu
WHERE iReportId IN (70241,70245,70153,70258) OR sCaption LIKE N'%Tracking III%'
"@ "menu try"

Dump @"
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME LIKE N'%Menu%' OR TABLE_NAME LIKE N'%cCore_Report%'
ORDER BY TABLE_NAME
"@ "menu/report tables"
