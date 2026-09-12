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
            if ($c -ge 25) { break }
        }
        $r.Close()
    } catch {
        Write-Output ("ERROR: " + $_.Exception.Message)
        try { if ($r) { $r.Close() } } catch {}
    }
}

Dump @"
SELECT a.iMasterId, a.sCode, a.sName, a.iAccountType
FROM dbo.mCore_Account a
WHERE a.iMasterId IN (82, 20943, 19823, 6308)
"@ "sample accounts"

Dump @"
SELECT a.iAccountType, COUNT(*) Cnt,
  CAST(SUM(CASE WHEN d.mAmount1>0 THEN d.mAmount1 ELSE 0 END) AS decimal(18,2)) AS Coll
FROM dbo.tCore_Header_0 h
JOIN dbo.tCore_Data_0 d ON d.iHeaderId=h.iHeaderId AND d.bUpdateFA=1 AND d.iFaTag IN (2040,2057)
JOIN dbo.mCore_Account a ON a.iMasterId=d.iCode
WHERE h.iVoucherType IN (4608,4609,4610) AND ISNULL(h.bCancelled,0)=0 AND h.iDate>0
GROUP BY a.iAccountType
ORDER BY 2 DESC
"@ "receipt line account types"

Dump @"
SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME='mCore_Account' AND COLUMN_NAME LIKE '%Type%'
"@ "account type cols"
