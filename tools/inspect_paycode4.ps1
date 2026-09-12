$cs = "Server=localhost;Database=Focus80E0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
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
WHERE TABLE_NAME LIKE '%paymentcode%'
   OR TABLE_NAME LIKE '%Paymentcode%'
   OR TABLE_NAME LIKE '%paycode%'
   OR TABLE_NAME LIKE '%PayCode%'
   OR TABLE_NAME LIKE 'mCore_CI%'
   OR TABLE_NAME LIKE '%collection%'
   OR TABLE_NAME LIKE '%installment%'
ORDER BY TABLE_NAME
"@ "name search"

Dump @"
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME LIKE 'vaCore_%' AND (
  TABLE_NAME LIKE '%pay%' OR TABLE_NAME LIKE '%advance%' OR TABLE_NAME LIKE '%code%'
)
ORDER BY TABLE_NAME
"@ "vaCore pay"

Dump @"
SELECT TABLE_NAME, COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS
WHERE COLUMN_NAME IN ('PaymentCode','PaymentCodeName','sPaymentCode')
ORDER BY TABLE_NAME
"@ "PaymentCode columns"

Dump @"
SELECT TOP 20 TABLE_NAME FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME LIKE 'cCore_%Field%' OR TABLE_NAME LIKE 'cCore_%Extra%' OR TABLE_NAME LIKE 'cCore_Document%'
ORDER BY TABLE_NAME
"@ "extra field meta"

Dump @"
SELECT hd.PaymentCode, COUNT(*) Cnt,
       MIN(h.sVoucherNo) SampleNo
FROM dbo.tCore_Header_0 h
INNER JOIN dbo.tCore_HeaderData4610_0 hd ON hd.iHeaderId=h.iHeaderId
GROUP BY hd.PaymentCode
ORDER BY hd.PaymentCode
"@ "4610 codes with sample voucher"

$conn.Close()
