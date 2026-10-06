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
                    if ($v.Length -gt 180) { $v = $v.Substring(0,180) + "..." }
                    $parts += $v
                }
            }
            Write-Output ($parts -join " | ")
            if ($c -ge 30) { Write-Output "(truncated)"; break }
        }
        $r.Close()
        if ($c -eq 0) { Write-Output "(no rows)" }
    } catch {
        Write-Output ("ERROR: " + $_.Exception.Message)
        try { $r.Close() } catch {}
    }
}

Dump @"
SELECT v.iVoucherType, ISNULL(vc.sName, N'?') AS VName, COUNT(*) Cnt,
  CAST(SUM(CASE WHEN v.Credit>0 THEN v.Credit ELSE 0 END) AS decimal(18,2)) Cr,
  CAST(SUM(CASE WHEN v.Debit<0 THEN -v.Debit ELSE 0 END) AS decimal(18,2)) DrAbs
FROM dbo.vtCode_DataFA_0 v
JOIN dbo.mCore_Account a ON a.iMasterId = v.iMasterId AND a.iAccountType IN (5,7)
LEFT JOIN dbo.cCore_vouchers_0 vc ON vc.iVoucherType = v.iVoucherType
WHERE v.iDate BETWEEN dbo.DateToInt(CONVERT(datetime,'20260101',112)) AND dbo.DateToInt(CONVERT(datetime,'20261231',112))
GROUP BY v.iVoucherType, vc.sName
ORDER BY Cnt DESC
"@ "FA voucher mix on customers 2026"

Dump @"
SELECT TOP 5 COLUMN_NAME, DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME='vCore_DlqtMngtPendingBills_0'
ORDER BY ORDINAL_POSITION
"@ "pending bills cols"

Dump @"
SELECT iVoucherType, sName FROM dbo.cCore_vouchers_0
WHERE iVoucherType IN (3332, 4608, 4609, 4610, 5634, 256, 8707, 4096, 768)
ORDER BY iVoucherType
"@ "voucher names"

Dump @"
SELECT COUNT(*) Cnt5634InFA
FROM dbo.vtCode_DataFA_0
WHERE iVoucherType = 5634
"@ "contracts in FA view"

Dump @"
SELECT TOP 8 sVoucherNo, iDate, fNet, iAuth
FROM dbo.tCore_Header_0
WHERE iVoucherType = 3332 AND sVoucherNo LIKE N'SI-26-01021%'
"@ "SI-26-01021 header"
