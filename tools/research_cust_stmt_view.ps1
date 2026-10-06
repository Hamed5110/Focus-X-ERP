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
SELECT name, type_desc
FROM sys.objects
WHERE name LIKE '%[Cc]ustomer%' OR name LIKE '%[Ss]tatement%' OR name LIKE '%[Ss]tatmnet%'
   OR name LIKE '%[Ll]edger%' OR name LIKE '%DataFA%' OR name LIKE '%DrCr%'
   OR name LIKE '%Outstanding%' OR name LIKE '%Receivab%'
ORDER BY type_desc, name
"@ "objects customer/statement/ledger"

Dump @"
SELECT TABLE_SCHEMA, TABLE_NAME, TABLE_TYPE
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME LIKE '%ustomer%' OR TABLE_NAME LIKE '%tatement%' OR TABLE_NAME LIKE '%tatmnet%'
   OR TABLE_NAME LIKE '%Ledger%' OR TABLE_NAME LIKE '%DataFA%' OR TABLE_NAME LIKE '%BillWise%'
   OR TABLE_NAME LIKE '%Outstanding%'
ORDER BY TABLE_TYPE, TABLE_NAME
"@ "tables/views name match"

Dump @"
SELECT iReportId, sReportName, iReportType, iSourceType
FROM dbo.cCore_Reports_0
WHERE sReportName LIKE N'%Statement%' OR sReportName LIKE N'%Statmnet%'
   OR sReportName LIKE N'%Ledger%'
ORDER BY iReportId
"@ "statement/ledger reports"
