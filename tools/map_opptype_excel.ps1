$cs = "Server=localhost;Database=Focus80E0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
function Dump($sql) {
    $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 180; $cmd.CommandText = $sql
    $r = $cmd.ExecuteReader()
    $n = $r.FieldCount
    $hdr = @(); for ($i=0;$i -lt $n;$i++) { $hdr += $r.GetName($i) }
    Write-Output ("COLS: " + ($hdr -join " | "))
    $c = 0
    while ($r.Read()) {
        $parts = @()
        for ($i = 0; $i -lt $n; $i++) {
            if ($r.IsDBNull($i)) { $parts += "" } else { $parts += [string]$r.GetValue($i) }
        }
        Write-Output ($parts -join " | ")
        $c++
        if ($c -ge 80) { break }
    }
    $r.Close()
    Write-Output ("-- shown: $c --")
}

Write-Output "=== OpportunityType on Excel sample SOs ==="
Dump @"
SELECT h.sVoucherNo, hd.OpportunityType, ABS(h.fNet) AS NetAbs, h.iDate, h.iAuth
FROM dbo.tCore_Header_0 h
LEFT JOIN dbo.tCore_HeaderData5634_0 hd ON hd.iHeaderId=h.iHeaderId
WHERE h.sVoucherNo IN (
  'CON-Atl-6882','CON-Atl-6895','CON-Atl-6905','CON-Atl-6912','CON-Atl-6923',
  'CON-Atl-6904','CON-Atl-6932','CON-Atl-6940','CON-Atl-6967','CON-Atl-6886',
  'CON-Atl-6896','CON-Atl-6943','CON-Atl-6883','CON-Atl-6917','CON-Atl-6954'
)
"@

Write-Output "`n=== remaining voucher field columns for OpportunityType ==="
Dump @"
SELECT *
FROM dbo.cCore_VoucherFields_0
WHERE sFieldName='OpportunityType' AND iVoucherType=5634
"@

Write-Output "`n=== Aug 2026 Atlas SO count and sum ABS(fNet) ==="
Dump @"
SELECT COUNT(*) Cnt,
       SUM(ABS(h.fNet)) SumAbs,
       SUM(h.fNet) SumSigned
FROM dbo.tCore_Header_0 h
INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId=h.iHeaderId
WHERE h.iVoucherType=5634
  AND d.iFaTag=2040
  AND ISNULL(h.bCancelled,0)=0 AND ISNULL(h.bVersion,0)=0 AND ISNULL(h.bSuspended,0)=0
  AND ISNULL(d.iType,0)=0 AND ISNULL(d.bVoid,0)=0
  AND ISNULL(h.iAuth,1)=1
  AND (h.iDate/65536)=2026 AND ((h.iDate/256)%256)=8
  AND h.sVoucherNo LIKE 'CON-Atl-%'
"@

Write-Output "`n=== distinct salesman on Aug Atlas SOs ==="
Dump @"
SELECT v.SalesmannameName, COUNT(DISTINCT h.iHeaderId) Cnt, SUM(ABS(h.fNet)) Amt
FROM dbo.tCore_Header_0 h
INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId=h.iHeaderId AND d.iFaTag=2040 AND ISNULL(d.iType,0)=0 AND ISNULL(d.bVoid,0)=0
INNER JOIN dbo.vaCore_Account v ON v.iMasterId=d.iBookNo AND v.iTreeId=0
WHERE h.iVoucherType=5634
  AND ISNULL(h.bCancelled,0)=0 AND ISNULL(h.bVersion,0)=0 AND ISNULL(h.bSuspended,0)=0
  AND ISNULL(h.iAuth,1)=1
  AND (h.iDate/65536)=2026 AND ((h.iDate/256)%256)=8
  AND h.sVoucherNo LIKE 'CON-Atl-%'
GROUP BY v.SalesmannameName
"@

$conn.Close()
