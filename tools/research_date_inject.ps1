$cs = "Server=localhost;Database=Focus80E0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
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
                if ($r.IsDBNull($i)) { $parts += "" } else {
                    $v = [string]$r.GetValue($i)
                    $v = $v -replace "`r|`n"," "
                    if ($v.Length -gt 400) { $v = $v.Substring(0,400) + "..." }
                    $parts += $v
                }
            }
            Write-Output ($parts -join " | ")
            if ($c -ge 80) { break }
        }
        $r.Close()
        if ($c -eq 0) { Write-Output "(no rows)" }
    } catch {
        Write-Output ("ERROR: " + $_.Exception.Message)
    }
}

Dump @"
SELECT COLUMN_NAME, DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME='cCore_Reports_0'
ORDER BY ORDINAL_POSITION
"@ "cCore_Reports_0 all columns"

Dump @"
SELECT r.iReportId, r.sName, r.iType
FROM dbo.cCore_Reports_0 r
WHERE r.sName LIKE N'%Commission%' OR r.sName LIKE N'%commission%'
ORDER BY r.iReportId
"@ "commission reports by sName"

Dump @"
SELECT TOP 5 * FROM dbo.cCore_Reports_0
"@ "sample report row keys"

$conn.Close()
