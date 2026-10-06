$cs = "Server=localhost;Database=Focus80G0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$cmd = $conn.CreateCommand()
$cmd.CommandTimeout = 60
$cmd.CommandText = @"
SELECT
    h.sVoucherNo,
    hd.OpportunityType,
    h.fNet AS StoredFNet,
    CAST(-MAX(h.fNet) AS decimal(18, 2)) AS ContractAmt,
    CAST(CASE WHEN MAX(h.fNet) < 0 THEN -MAX(h.fNet) ELSE 0 END AS decimal(18, 2)) AS DebitAmt,
    CAST(CASE WHEN MAX(h.fNet) > 0 THEN -MAX(h.fNet) ELSE 0 END AS decimal(18, 2)) AS CreditDisplay
FROM dbo.tCore_Header_0 h
INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId = h.iHeaderId AND ISNULL(d.iType,0)=0 AND ISNULL(d.bVoid,0)=0
LEFT JOIN dbo.tCore_HeaderData5634_0 hd ON hd.iHeaderId = h.iHeaderId
WHERE h.iVoucherType = 5634 AND h.sVoucherNo = N'CON-Atl-7035'
  AND ISNULL(h.iAuth,1)=1 AND ISNULL(h.bCancelled,0)=0 AND ISNULL(h.bVersion,0)=0 AND ISNULL(h.bSuspended,0)=0
GROUP BY h.sVoucherNo, hd.OpportunityType, h.fNet
"@
$r = $cmd.ExecuteReader()
$hdr = @(); for ($i=0; $i -lt $r.FieldCount; $i++) { $hdr += $r.GetName($i) }
Write-Output ($hdr -join " | ")
while ($r.Read()) {
    $parts = @(); for ($i=0; $i -lt $r.FieldCount; $i++) { $parts += [string]$r.GetValue($i) }
    Write-Output ($parts -join " | ")
}
$r.Close()
$conn.Close()
