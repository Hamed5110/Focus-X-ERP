$cs = "Server=localhost;Database=Focus80G0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
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
                    if ($v.Length -gt 80) { $v = $v.Substring(0,80) + "..." }
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
SELECT p.iReportId, r.sReportName, p.sFieldName, p.sFieldVariable, p.iControlType, p.iFieldType, p.iSelectionMode, p.iFieldId
FROM dbo.cCore_ReportParameter_0 p
JOIN dbo.cCore_Reports_0 r ON r.iReportId = p.iReportId
WHERE p.sFieldName LIKE N'%Vendor%' OR p.sFieldName LIKE N'%Job%' OR p.sFieldName LIKE N'%Delivery%'
   OR p.sFieldVariable LIKE N'%Vendor%' OR p.sFieldVariable LIKE N'%Job%' OR p.sFieldVariable LIKE N'%Delivery%'
ORDER BY p.iReportId
"@ "vendor/job/delivery params"

Dump @"
SELECT iReportId, sReportName, iReportType FROM dbo.cCore_Reports_0
WHERE iReportId IN (70252, 70248, 70054)
"@ "glass/po reports"
