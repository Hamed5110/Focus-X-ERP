$cs = "Server=localhost;Database=Focus8080;User Id=sa;Password=$env:FOCUS_SQL_PASSWORD;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
function Dump($sql, $title) {
    Write-Output ""
    Write-Output "========== $title =========="
    $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 60; $cmd.CommandText = $sql
    $r = $cmd.ExecuteReader()
    $n = $r.FieldCount
    $hdr = @(); for ($i=0; $i -lt $n; $i++) { $hdr += $r.GetName($i) }
    Write-Output ($hdr -join " | ")
    $c = 0
    while ($r.Read()) {
        $c++
        $parts = @(); for ($i=0; $i -lt $n; $i++) {
            if ($r.IsDBNull($i)) { $parts += "" } else { $parts += ([string]$r.GetValue($i) -replace "`r|`n"," ") }
        }
        Write-Output ($parts -join " | ")
        if ($c -ge 20) { break }
    }
    $r.Close()
}

Dump @"
SELECT IDENT_CURRENT('cCore_ReportColumns_0') AS colIdent, IDENT_CURRENT('cCore_ReportParameter_0') AS paramIdent
"@ "idents"

Dump @"
SELECT TOP 3 iColumnId, iLayoutId, iFieldId, iType, sColumn, sAliasName, fColumnWidth, iMiscOption, iAlignment, iParentId, iSubParentId, iDecimalInColumn
FROM dbo.cCore_ReportColumns_0 WHERE iFieldId = 17077222 AND iMiscOption = 64
"@ "narr col template"

Dump @"
SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME=N'cCore_ReportParameter_0'
"@ "param cols again"

Dump @"
SELECT iParameterId, iReportId, sFieldName, sFieldVariable, iControlType, iFieldType, iSelectionMode, iFieldId, iSubParentId, iType, sDefault, bGroup, bDefault
FROM dbo.cCore_ReportParameter_0 WHERE iReportId IN (70054,70255,70022)
"@ "po params"
