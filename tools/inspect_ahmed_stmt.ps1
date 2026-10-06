$cs = "Server=localhost;Database=Focus80G0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
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
                    if ($v.Length -gt 180) { $v = $v.Substring(0,180) + "..." }
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
SELECT iMasterId, sCode, sName FROM dbo.mCore_Account
WHERE sCode = N'AC-7208' OR sName LIKE N'%Ahmed Yusuf Alawainati%'
"@ "customer"

Dump @"
SELECT h.sVoucherNo, hd.OpportunityType, h.fNet, h.iDate, h.iAuth, h.bCancelled
FROM dbo.tCore_Header_0 h
LEFT JOIN dbo.tCore_HeaderData5634_0 hd ON hd.iHeaderId = h.iHeaderId
INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId = h.iHeaderId AND ISNULL(d.iType,0)=0 AND ISNULL(d.bVoid,0)=0
INNER JOIN dbo.mCore_Account a ON a.iMasterId = d.iBookNo AND a.sCode = N'AC-7208'
WHERE h.iVoucherType = 5634
GROUP BY h.sVoucherNo, hd.OpportunityType, h.fNet, h.iDate, h.iAuth, h.bCancelled
ORDER BY h.iDate
"@ "AC-7208 contracts"

Dump @"
SELECT v.sVoucherNo, v.iVoucherType, vc.sName, v.iDate, v.Debit, v.Credit
FROM dbo.vtCode_DataFA_0 v
JOIN dbo.mCore_Account a ON a.iMasterId = v.iMasterId AND a.sCode = N'AC-7208'
LEFT JOIN dbo.cCore_vouchers_0 vc ON vc.iVoucherType = v.iVoucherType
WHERE v.iDate > 0 AND (v.Debit < 0 OR v.Credit > 0)
  AND v.iVoucherType IN (256, 3840, 4096, 4608, 4609, 4610, 8704, 8705, 8707, 8708)
ORDER BY v.iDate, v.sVoucherNo
"@ "AC-7208 FA"

Dump @"
SELECT iVoucherType, sName FROM dbo.cCore_vouchers_0
WHERE iVoucherType IN (256, 3840, 4096, 4608, 4609, 4610, 8704, 8705, 8707, 8708, 5634)
ORDER BY iVoucherType
"@ "voucher names"

$conn.Close()
