$cs = "Server=localhost;Database=Focus80G0;User Id=sa;Password=$env:FOCUS_SQL_PASSWORD;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
function Dump($sql, $title) {
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
                    if ($v.Length -gt 120) { $v = $v.Substring(0,120) + "..." }
                    $parts += $v
                }
            }
            Write-Output ($parts -join " | ")
            if ($c -ge 40) { break }
        }
        $r.Close()
        if ($c -eq 0) { Write-Output "(no rows)" }
    } catch { Write-Output $_.Exception.Message }
}

Dump @"
SELECT iReportId, sReportName, iReportType, iSourceType
FROM dbo.cCore_Reports_0
WHERE sReportName LIKE N'%Project Tracking%3%'
   OR sReportName LIKE N'%Project Tracking 3%'
   OR sReportName LIKE N'%Statement of Customer%'
   OR sReportName LIKE N'%Customer Statement%'
ORDER BY iReportId
"@ "reports"

Dump @"
SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = N'cCore_ReportParameter_0'
ORDER BY ORDINAL_POSITION
"@ "param cols"

Dump @"
SELECT iMasterId, sCode, sName, bGroup
FROM dbo.mCore_Account
WHERE sName LIKE N'%Trade Receiv%' OR sName LIKE N'%Trade Payab%'
ORDER BY sName
"@ "trade groups"
