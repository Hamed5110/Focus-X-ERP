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
        if ($c -ge 30) { break }
    }
    $r.Close()
    Write-Output ("-- shown: $c --")
}

Write-Output "=== OpportunityType voucher field ==="
Dump @"
SELECT iUniqueId, iFieldId, iVoucherType, bHeader, sFieldName, iDisplayControlType, iMasterLink,
       sExternalTableName, sExternalValueMember, sExternalDisplayMember, sGroupName
FROM dbo.cCore_VoucherFields_0
WHERE sFieldName LIKE '%Opport%'
"@

Write-Output "`n=== caption for those field ids ==="
Dump @"
SELECT iFieldId, sCaption, iDataTypeId, iModuleType
FROM dbo.cCore_Fields
WHERE sCaption LIKE '%Opport%' OR sCaption LIKE '%Business%' OR iFieldId IN (
    SELECT iFieldId FROM dbo.cCore_VoucherFields_0 WHERE sFieldName LIKE '%Opport%'
)
"@

Write-Output "`n=== search New Business across remaining masters ==="
Dump @"
SELECT t.name
FROM sys.tables t
INNER JOIN sys.columns c ON c.object_id=t.object_id AND c.name='sName'
WHERE t.name LIKE 'mCore_%' AND t.name NOT LIKE '%Language%' AND t.name NOT LIKE '%Tree%'
ORDER BY t.name
"@

$conn.Close()
