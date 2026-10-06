$cs = "Server=localhost;Database=Focus8080;User Id=sa;Password=$env:FOCUS_SQL_PASSWORD;Encrypt=False;TrustServerCertificate=True;"
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
                    if ($v.Length -gt 160) { $v = $v.Substring(0,160) + "..." }
                    $parts += $v
                }
            }
            Write-Output ($parts -join " | ")
            if ($c -ge 60) { break }
        }
        $r.Close()
        if ($c -eq 0) { Write-Output "(no rows)" }
    } catch { Write-Output $_.Exception.Message }
}

Dump @"
SELECT iColumnId, iLayoutId, iFieldId, sColumn, sAliasName
FROM dbo.cCore_ReportColumns_0
WHERE iFieldId IN (300006,17077222,306832,17084048,17083059)
"@ "existing narration columns"

Dump @"
SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME=N'cCore_ReportParameter_0' ORDER BY ORDINAL_POSITION
"@ "param cols"

Dump @"
SELECT * FROM dbo.cCore_ReportParameter_0 WHERE iReportId IN (70252,70248,70255,70125)
"@ "glass params"

Dump @"
SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME=N'cCore_FilterCustomization' ORDER BY ORDINAL_POSITION
"@ "filter custom cols"

Dump @"
SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME=N'cCore_ReportColumnsFilter_0' ORDER BY ORDINAL_POSITION
"@ "col filter cols"

Dump @"
SELECT TOP 5 * FROM dbo.cCore_ReportColumnsFilter_0
"@ "col filter sample"

Dump @"
SELECT iFieldId, sCaption FROM dbo.cCore_Fields WHERE iFieldId IN (305843,300006,307291) OR sCaption LIKE N'%Notepad%' OR sCaption LIKE N'%Narrat%'
"@ "notepad vs narr"

Dump @"
SELECT iFilterId, iFilterGroupId, iFieldId, iOperator, sValue, sCompareText, iType, iSubParentId
FROM dbo.cCore_ReportFilter_0 WHERE iFieldId IN (5002,305081,307309,4)
"@ "dim filters"
