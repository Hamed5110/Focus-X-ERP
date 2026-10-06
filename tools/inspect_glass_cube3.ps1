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
                    if ($v.Length -gt 140) { $v = $v.Substring(0,140) + "..." }
                    $parts += $v
                }
            }
            Write-Output ($parts -join " | ")
            if ($c -ge 80) { break }
        }
        $r.Close()
        if ($c -eq 0) { Write-Output "(no rows)" }
    } catch { Write-Output $_.Exception.Message }
}

Dump @"
SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME=N'cCore_ReportLayouts_0' ORDER BY ORDINAL_POSITION
"@ "layout cols"

Dump @"
SELECT * FROM dbo.cCore_ReportLayouts_0 WHERE iReportId IN (70252,70248,70255)
"@ "glass layouts"

Dump @"
SELECT iFieldId, sCaption, iDataTypeId, iModuleType
FROM dbo.cCore_Fields
WHERE sCaption LIKE N'%Narrat%' OR sCaption LIKE N'%Item Qty%' OR sCaption LIKE N'%Glass Qty%'
   OR sCaption LIKE N'%Glass Class%' OR sCaption LIKE N'%Delivery Date%'
ORDER BY iFieldId
"@ "narration fields 8080"

Dump @"
SELECT iUniqueId, iFieldId, iVoucherType, bHeader, sFieldName, sVariableName, iDisplayControlType, iMasterLink, bNotAvailableForReports
FROM dbo.cCore_VoucherFields_0
WHERE sFieldName LIKE N'%Narrat%' OR sFieldName LIKE N'%Item Qty%' OR sFieldName LIKE N'%Glass%'
   OR sVariableName LIKE N'%Narrat%' OR sFieldName LIKE N'%Delivery Date%'
ORDER BY iVoucherType, iFieldId
"@ "voucher extra narration"
