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
                if ($r.IsDBNull($i)) { $parts += "" } else {
                    $v = [string]$r.GetValue($i)
                    $v = $v -replace "`r|`n"," "
                    if ($v.Length -gt 220) { $v = $v.Substring(0,220) + "..." }
                    $parts += $v
                }
            }
            Write-Output ($parts -join " | ")
            if ($c -ge 40) { Write-Output "(truncated)"; break }
        }
        $r.Close()
        if ($c -eq 0) { Write-Output "(no rows)" }
    } catch {
        Write-Output ("ERROR: " + $_.Exception.Message)
        try { if ($r) { $r.Close() } } catch {}
    }
}

Dump @"
SELECT TABLE_NAME, COLUMN_NAME, DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME IN ('tCore_HeaderData4608_0','tCore_HeaderData4609_0','tCore_HeaderData4610_0')
  AND COLUMN_NAME IN ('PaymentCode','TotalContractAmt','ContractAmount')
ORDER BY TABLE_NAME, COLUMN_NAME
"@ "col types"

Dump @"
SELECT TOP 5 PaymentCode, SQL_VARIANT_PROPERTY(PaymentCode,'BaseType') AS T
FROM dbo.tCore_HeaderData4609_0
"@ "4609 PaymentCode sample"

Dump @"
SELECT
  h.iVoucherType,
  CAST(COALESCE(NULLIF(hd4610.PaymentCode,0), NULLIF(hd4609.PaymentCode,0), NULLIF(hd4608.PaymentCode,0), 0) AS int) AS Pay,
  COUNT(DISTINCT h.iHeaderId) AS Hdrs,
  SUM(CASE WHEN ISNULL(TRY_CONVERT(decimal(18,4), hd4610.TotalContractAmt),0)>0 THEN 1 ELSE 0 END) AS Has4610,
  SUM(CASE WHEN ISNULL(TRY_CONVERT(decimal(18,4), hd4609.ContractAmount),0)>0 THEN 1 ELSE 0 END) AS Has4609,
  CAST(SUM(CASE WHEN d.mAmount1>0 THEN d.mAmount1 ELSE 0 END) AS decimal(18,2)) AS Coll
FROM dbo.tCore_Header_0 h
JOIN dbo.tCore_Data_0 d ON d.iHeaderId=h.iHeaderId
LEFT JOIN dbo.tCore_HeaderData4610_0 hd4610 ON hd4610.iHeaderId=h.iHeaderId
LEFT JOIN dbo.tCore_HeaderData4609_0 hd4609 ON hd4609.iHeaderId=h.iHeaderId
LEFT JOIN dbo.tCore_HeaderData4608_0 hd4608 ON hd4608.iHeaderId=h.iHeaderId
WHERE h.iVoucherType IN (4608,4609,4610)
  AND ISNULL(h.iAuth,1)=1 AND ISNULL(h.bCancelled,0)=0 AND ISNULL(h.bVersion,0)=0 AND ISNULL(h.bSuspended,0)=0
  AND h.iDate>0 AND d.bUpdateFA=1 AND d.iCode>0 AND d.iFaTag IN (2040,2057)
  AND ISNULL(d.iType,0)=0 AND ISNULL(d.bVoid,0)=0 AND ISNULL(d.iAuthStatus,0)<2
  AND COALESCE(NULLIF(hd4610.PaymentCode,0), NULLIF(hd4609.PaymentCode,0), NULLIF(hd4608.PaymentCode,0), 0) IN (1,4)
GROUP BY h.iVoucherType,
  CAST(COALESCE(NULLIF(hd4610.PaymentCode,0), NULLIF(hd4609.PaymentCode,0), NULLIF(hd4608.PaymentCode,0), 0) AS int)
ORDER BY 1,2
"@ "T5 first-pay by voucher"

$conn.Close()
