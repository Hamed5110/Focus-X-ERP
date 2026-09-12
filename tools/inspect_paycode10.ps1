$cs = "Server=localhost;Database=Focus80E0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 60
$cmd.CommandText = @"
SELECT vf.iVoucherType, v.sName AS VoucherName, vf.sFieldName, vf.iDisplayControlType,
       vf.iMasterLink, vf.sExternalTableName, vf.sExternalValueMember, vf.sExternalDisplayMember,
       vf.sDefaultValue
FROM dbo.cCore_VoucherFields_0 vf
LEFT JOIN dbo.cCore_Vouchers_0 v ON v.iVoucherType = vf.iVoucherType
WHERE vf.sFieldName LIKE N'%Payment%'
  AND vf.iVoucherType IN (4608, 4609, 4610, 4612)
ORDER BY vf.iVoucherType, vf.sFieldName
"@
$r = $cmd.ExecuteReader()
$n = $r.FieldCount
$hdr = @(); for ($i=0; $i -lt $n; $i++) { $hdr += $r.GetName($i) }
Write-Output ($hdr -join " | ")
while ($r.Read()) {
    $parts = @()
    for ($i=0; $i -lt $n; $i++) {
        if ($r.IsDBNull($i)) { $parts += "" } else { $parts += [string]$r.GetValue($i) }
    }
    Write-Output ($parts -join " | ")
}
$r.Close()
$conn.Close()
