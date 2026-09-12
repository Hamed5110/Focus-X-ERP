$cs = "Server=localhost;Database=Focus80E0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
function Dump($sql, $title) {
    Write-Output "`n=== $title ==="
    $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 120; $cmd.CommandText = $sql
    try {
        $r = $cmd.ExecuteReader()
        $hdr = @(); for ($i=0;$i -lt $r.FieldCount;$i++) { $hdr += $r.GetName($i) }
        Write-Output ("COLS: " + ($hdr -join " | "))
        $c = 0
        while ($r.Read()) {
            $parts = @()
            for ($i=0;$i -lt $r.FieldCount;$i++) {
                if ($r.IsDBNull($i)) { $parts += "" } else { $parts += ([string]$r.GetValue($i)).Substring(0, [Math]::Min(40, ([string]$r.GetValue($i)).Length)) }
            }
            Write-Output ($parts -join " | ")
            $c++; if ($c -ge 40) { break }
        }
        $r.Close()
        Write-Output "-- $c --"
    } catch {
        Write-Output $_.Exception.Message
        if ($r) { try { $r.Close() } catch {} }
    }
}

Dump @"
SELECT TABLE_NAME, COLUMN_NAME
FROM INFORMATION_SCHEMA.COLUMNS
WHERE COLUMN_NAME LIKE '%Salesman%' OR COLUMN_NAME LIKE '%salesman%'
ORDER BY TABLE_NAME, COLUMN_NAME
"@ "salesman columns"

Dump @"
SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME IN ('vaCore_Account','tCore_HeaderData5634_0','tCore_HeaderData4610_0','tCore_HeaderData4609_0')
  AND (COLUMN_NAME LIKE '%Sales%' OR COLUMN_NAME LIKE '%Emp%' OR COLUMN_NAME LIKE '%Split%' OR COLUMN_NAME LIKE '%Share%')
ORDER BY TABLE_NAME, COLUMN_NAME
"@ "sales-like extras"

Dump @"
SELECT COUNT(*) Cnt, COUNT(DISTINCT Salesmanname) DistinctSm
FROM dbo.vaCore_Account
WHERE iTreeId = 0 AND ISNULL(Salesmanname,0) > 0
"@ "account salesman fill"

Dump @"
SELECT iMasterId, COUNT(DISTINCT Salesmanname) N
FROM dbo.vaCore_Account
WHERE iTreeId = 0 AND ISNULL(Salesmanname,0) > 0
GROUP BY iMasterId
HAVING COUNT(DISTINCT Salesmanname) > 1
"@ "accounts with multiple salesman ids"

$conn.Close()
