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
WHERE TABLE_NAME='tCore_HeaderData4608_0' ORDER BY ORDINAL_POSITION
"@ "4608 cols"

Dump @"
SELECT TOP 12
  h.sVoucherNo, dep.sName AS Dept, hd.PaymentCode, hd.TotalContractAmt,
  CAST(SUM(CASE WHEN d.mAmount1>0 THEN d.mAmount1 ELSE 0 END) AS decimal(18,2)) AS Coll
FROM dbo.tCore_Header_0 h
INNER JOIN dbo.tCore_HeaderData4610_0 hd ON hd.iHeaderId=h.iHeaderId
INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId=h.iHeaderId AND d.bUpdateFA=1 AND d.iCode>0
  AND d.iFaTag IN (2040,2057) AND ISNULL(d.iType,0)=0 AND ISNULL(d.bVoid,0)=0
INNER JOIN dbo.mCore_Department dep ON dep.iMasterId=d.iFaTag
WHERE h.iVoucherType=4610 AND ISNULL(h.iAuth,1)=1 AND ISNULL(h.bCancelled,0)=0
  AND h.iDate >= 132777985 AND h.iDate <= 132778015
  AND hd.PaymentCode IN (1,4)
GROUP BY h.sVoucherNo, dep.sName, hd.PaymentCode, hd.TotalContractAmt
ORDER BY Coll DESC
"@ "Aug 2026 first-pay CRM samples"

Dump @"
SELECT
  d.iFaTag, dep.sName,
  hd.PaymentCode,
  COUNT(*) Cnt,
  CAST(SUM(CASE WHEN d.mAmount1>0 THEN d.mAmount1 ELSE 0 END) AS decimal(18,2)) AS Coll
FROM dbo.tCore_Header_0 h
INNER JOIN dbo.tCore_HeaderData4610_0 hd ON hd.iHeaderId=h.iHeaderId
INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId=h.iHeaderId AND d.bUpdateFA=1 AND d.iCode>0
  AND d.iFaTag IN (2040,2057) AND ISNULL(d.iType,0)=0 AND ISNULL(d.bVoid,0)=0
INNER JOIN dbo.mCore_Department dep ON dep.iMasterId=d.iFaTag
WHERE h.iVoucherType=4610 AND ISNULL(h.iAuth,1)=1 AND ISNULL(h.bCancelled,0)=0 AND ISNULL(h.bVersion,0)=0
  AND h.iDate >= 132777985 AND h.iDate <= 132778015
GROUP BY d.iFaTag, dep.sName, hd.PaymentCode
ORDER BY dep.sName, hd.PaymentCode
"@ "Aug 2026 CRM collection by pay code"

Dump @"
SELECT TOP 8 h.sVoucherNo, hd.PaymentCode, hd.ContractAmount
FROM dbo.tCore_Header_0 h
INNER JOIN dbo.tCore_HeaderData4609_0 hd ON hd.iHeaderId=h.iHeaderId
WHERE h.iVoucherType=4609 AND ISNULL(hd.PaymentCode,0)>0
ORDER BY h.iHeaderId DESC
"@ "4609 PaymentCode + ContractAmount"

$conn.Close()
