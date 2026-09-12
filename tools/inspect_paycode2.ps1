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
                if ($r.IsDBNull($i)) { $parts += [string]$r.GetValue($i) } else { $parts += [string]$r.GetValue($i) }
            }
            Write-Output ($parts -join " | ")
            if ($c -ge 100) { break }
        }
        $r.Close()
        if ($c -eq 0) { Write-Output "(no rows)" }
    } catch {
        Write-Output ("ERROR: " + $_.Exception.Message)
    }
}

Dump @"
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME LIKE 'mCore_%'
  AND (
    TABLE_NAME LIKE '%code%' OR TABLE_NAME LIKE '%Code%'
    OR TABLE_NAME LIKE '%CI%' OR TABLE_NAME LIKE '%install%'
    OR TABLE_NAME LIKE '%Install%' OR TABLE_NAME LIKE '%collect%'
  )
ORDER BY TABLE_NAME
"@ "mCore code-like"

Dump @"
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME LIKE '%paymentcode%' OR TABLE_NAME LIKE '%Paymentcode%'
   OR TABLE_NAME LIKE '%Payment_Code%' OR TABLE_NAME LIKE 'mCore_CI%'
   OR TABLE_NAME LIKE '%firstpayment%'
ORDER BY TABLE_NAME
"@ "paymentcode tables"

Dump @"
SELECT DISTINCT PaymentCode, COUNT(*) Cnt
FROM dbo.tCore_HeaderData4610_0
GROUP BY PaymentCode
ORDER BY PaymentCode
"@ "4610 PaymentCode values"

Dump @"
SELECT DISTINCT PaymentCode, COUNT(*) Cnt
FROM dbo.tCore_HeaderData4609_0
GROUP BY PaymentCode
ORDER BY PaymentCode
"@ "4609 PaymentCode values"

Dump @"
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.COLUMNS
WHERE COLUMN_NAME = 'sName'
  AND TABLE_NAME LIKE 'mCore_%'
  AND TABLE_NAME NOT LIKE '%Language%'
  AND TABLE_NAME NOT LIKE '%Tree%'
ORDER BY TABLE_NAME
"@ "mCore name masters"

$conn.Close()
