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
                if ($r.IsDBNull($i)) { $parts += "" } else { $parts += [string]$r.GetValue($i) }
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
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME LIKE '%payment%' OR TABLE_NAME LIKE '%Payment%' OR TABLE_NAME LIKE '%PayCode%'
ORDER BY TABLE_NAME
"@ "payment-like tables"

Dump @"
SELECT TABLE_NAME, COLUMN_NAME, DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE COLUMN_NAME LIKE '%Payment%' OR COLUMN_NAME LIKE '%PayCode%' OR COLUMN_NAME LIKE '%paycode%'
ORDER BY TABLE_NAME, COLUMN_NAME
"@ "payment-like columns"

Dump @"
SELECT COLUMN_NAME, DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME IN ('tCore_HeaderData4610_0','tCore_HeaderData4609_0','tCore_HeaderData4608_0')
ORDER BY TABLE_NAME, ORDINAL_POSITION
"@ "receipt header extra columns"

Dump @"
SELECT iMasterId, sCode, sName, iStatus
FROM dbo.mCore_PaymentCode
ORDER BY iMasterId
"@ "mCore_PaymentCode"

Dump @"
SELECT iMasterId, sCode, sName, iStatus
FROM dbo.mCore_paymentcode
ORDER BY iMasterId
"@ "mCore_paymentcode"

$conn.Close()
