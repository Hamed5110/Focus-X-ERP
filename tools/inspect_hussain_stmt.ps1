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
                    if ($v.Length -gt 160) { $v = $v.Substring(0,160) + "..." }
                    $parts += $v
                }
            }
            Write-Output ($parts -join " | ")
            if ($c -ge 25) { Write-Output "(truncated)"; break }
        }
        $r.Close()
        if ($c -eq 0) { Write-Output "(no rows)" }
    } catch {
        Write-Output ("ERROR: " + $_.Exception.Message)
        try { $r.Close() } catch {}
    }
}

Dump @"
SELECT iMasterId, sCode, sName FROM dbo.mCore_Account WHERE sCode = N'AC-6993'
"@ "AC-6993"

Dump @"
SELECT TOP 20 v.sVoucherNo, v.iVoucherType, vc.sName, v.iDate, v.Debit, v.Credit
FROM dbo.vtCode_DataFA_0 v
JOIN dbo.mCore_Account a ON a.iMasterId = v.iMasterId AND a.sCode = N'AC-6993'
LEFT JOIN dbo.cCore_vouchers_0 vc ON vc.iVoucherType = v.iVoucherType
WHERE v.iDate BETWEEN dbo.DateToInt(CONVERT(datetime,'20260901',112)) AND dbo.DateToInt(CONVERT(datetime,'20260930',112))
ORDER BY v.iDate, v.sVoucherNo
"@ "AC-6993 Sep 2026 FA"
