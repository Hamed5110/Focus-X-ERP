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
        for ($i=0;$i -lt $n;$i++) {
            if ($r.IsDBNull($i)) { $parts += "" } else { $parts += [string]$r.GetValue($i) }
        }
        Write-Output ($parts -join " | ")
        $c++
        if ($c -ge 40) { break }
    }
    $r.Close()
    Write-Output ("-- shown: $c --")
}

Write-Output "=== field captions ==="
Dump @"
SELECT iFieldId, sCaption, iDataTypeId FROM dbo.cCore_Fields
WHERE iFieldId IN (5042, 1879048194, 305128, 5002, 171, 19, 114)
"@

Write-Output "`n=== salesman field on voucher 5634 ==="
Dump @"
SELECT iFieldId, sFieldName, iMasterLink, sExternalTableName, sGroupName, bHeader
FROM dbo.cCore_VoucherFields_0
WHERE iVoucherType=5634 AND (sFieldName LIKE '%Sales%' OR sFieldName LIKE '%sales%')
"@

Write-Output "`n=== tag master links around 14/15 ==="
Dump @"
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME LIKE '%MasterTag%' OR TABLE_NAME LIKE '%TagMaster%' OR TABLE_NAME LIKE 'cCore_Tags%'
ORDER BY TABLE_NAME
"@

Write-Output "`n=== Aug 2026 Atlas SO by ACCOUNT salesman vs iTag14 ==="
Dump @"
SELECT TOP 15 h.sVoucherNo,
       MAX(smAcc.sName) AccSalesman,
       MAX(smTag.sName) Tag14Salesman,
       MAX(-h.fNet) ContractAmt
FROM dbo.tCore_Header_0 h
INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId=h.iHeaderId AND d.iFaTag=2040 AND ISNULL(d.iType,0)=0
LEFT JOIN dbo.tCore_Data_Tags_0 t ON t.iBodyId=d.iBodyId
LEFT JOIN (
    SELECT iMasterId, MAX(Salesmanname) Salesmanname FROM dbo.vaCore_Account WHERE iTreeId=0 GROUP BY iMasterId
) vac ON vac.iMasterId=d.iBookNo
LEFT JOIN dbo.mCore_Salesman smAcc ON smAcc.iMasterId=vac.Salesmanname
LEFT JOIN dbo.mCore_Salesman smTag ON smTag.iMasterId=t.iTag14
WHERE h.iVoucherType=5634 AND ISNULL(h.bCancelled,0)=0 AND ISNULL(h.bVersion,0)=0 AND ISNULL(h.iAuth,1)=1
  AND (h.iDate/65536)=2026 AND ((h.iDate/256)%256)=8
  AND h.sVoucherNo LIKE 'CON-Atl-%'
GROUP BY h.sVoucherNo
ORDER BY h.sVoucherNo
"@

$conn.Close()
