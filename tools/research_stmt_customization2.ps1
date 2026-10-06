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
                    if ($v.Length -gt 500) { $v = $v.Substring(0,500) + "..." }
                    $parts += $v
                }
            }
            Write-Output ($parts -join " | ")
            if ($c -ge 120) { Write-Output "(truncated)"; break }
        }
        $r.Close()
        if ($c -eq 0) { Write-Output "(no rows)" }
    } catch {
        Write-Output ("ERROR: " + $_.Exception.Message)
        try { $r.Close() } catch {}
    }
}

Dump @"
SELECT iReportId, sReportName, iModule, iReportType, iSourceType, bAdvance
FROM dbo.cCore_Reports_0
WHERE sReportName LIKE N'%Statement%'
   OR sReportName LIKE N'%Customer%'
   OR sReportName LIKE N'%Ledger%'
ORDER BY iReportId
"@ "statement/customer/ledger reports"

Dump @"
SELECT iReportId, sReportName, iModule, iReportType, iSourceType
FROM dbo.cCore_Reports_0
WHERE sReportName LIKE N'%Commission%'
   OR sReportName LIKE N'%Last Rate%'
   OR sReportName LIKE N'%Cost Reduction%'
ORDER BY iReportId
"@ "working query reports"

Dump @"
SELECT p.iParameterId, r.sReportName, r.iReportType, r.iSourceType,
       p.sFieldName, p.sFieldVariable, p.iControlType, p.iFieldType,
       p.iSelectionMode, p.iFieldId, p.iType, p.sDefault, p.sValue, p.bGroup
FROM dbo.cCore_ReportParameter_0 p
JOIN dbo.cCore_Reports_0 r ON r.iReportId = p.iReportId
WHERE p.iControlType IN (1,2,3,4,5,6,7,8,9,10)
   OR p.sFieldVariable LIKE N'%Customer%'
   OR p.sFieldName LIKE N'%Account%'
   OR p.sFieldName LIKE N'%Customer%'
ORDER BY r.sReportName, p.iParameterId
"@ "account/customer parameters"

Dump @"
SELECT name
FROM sys.views
WHERE name LIKE '%Account%'
   OR name LIKE '%Ledger%'
   OR name LIKE '%FA%'
   OR name LIKE '%TranData%'
   OR name LIKE '%Statement%'
   OR name LIKE '%Outstanding%'
ORDER BY name
"@ "candidate views"
