$cs = "Server=localhost;Database=Focus80E0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
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
                    if ($v.Length -gt 200) { $v = $v.Substring(0,200) + "..." }
                    $parts += $v
                }
            }
            Write-Output ($parts -join " | ")
            if ($c -ge 40) { break }
        }
        $r.Close()
        if ($c -eq 0) { Write-Output "(no rows)" }
    } catch {
        Write-Output ("ERROR: " + $_.Exception.Message)
    }
}

Dump @"
SELECT COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME='cCore_Reports_0'
  AND (COLUMN_NAME LIKE '%Date%' OR COLUMN_NAME LIKE '%Query%' OR COLUMN_NAME LIKE '%Type%'
       OR COLUMN_NAME LIKE '%Filter%' OR COLUMN_NAME LIKE '%Period%' OR COLUMN_NAME IN ('sName','iReportId','iType'))
ORDER BY ORDINAL_POSITION
"@ "cCore_Reports date-like cols"

Dump @"
SELECT COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME='cCore_RDQuery_0' ORDER BY ORDINAL_POSITION
"@ "cCore_RDQuery cols"

Dump @"
SELECT COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME='cCore_ReportParameter_0' ORDER BY ORDINAL_POSITION
"@ "cCore_ReportParameter cols"

Dump @"
SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME='cCore_Reports_0' ORDER BY ORDINAL_POSITION
"@ "all report cols"

Dump @"
SELECT iReportId, sName, iType FROM dbo.cCore_Reports_0
WHERE sName LIKE N'%Commission%' OR sName LIKE N'%commission%' OR sName LIKE N'%iii Project%'
ORDER BY iReportId
"@ "commission/iii reports"

$conn.Close()
