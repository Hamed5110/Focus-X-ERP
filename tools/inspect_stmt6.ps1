$cs = "Server=localhost;Database=Focus80G0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
function Run($title, $sql) {
    Write-Output "==== $title ===="
    $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 60; $cmd.CommandText = $sql
    try {
        $r = $cmd.ExecuteReader()
        $n = 0
        while ($r.Read()) {
            $n++
            $parts = @()
            for ($i=0; $i -lt $r.FieldCount; $i++) {
                $v = $r.GetValue($i)
                if ($v -is [DBNull]) { $v = "NULL" }
                $parts += ("{0}={1}" -f $r.GetName($i), $v)
            }
            Write-Output ($parts -join " | ")
            if ($n -ge 40) { Write-Output "... truncated"; break }
        }
        $r.Close()
        Write-Output ("rows_shown=$n")
    } catch {
        Write-Output ("FAIL: " + $_.Exception.Message)
        try { if ($r) { $r.Close() } } catch {}
    }
}

Run "master def opportunity" @"
SELECT iMasterTypeId, sMasterName, sVariable
FROM dbo.cCore_MasterDef
WHERE sMasterName LIKE N'%Opport%' OR sVariable LIKE N'%Opport%'
   OR sMasterName LIKE N'%Lead%' OR sMasterName LIKE N'%CRM%'
"@

Run "all master names with Type" @"
SELECT iMasterTypeId, sMasterName, sVariable
FROM dbo.cCore_MasterDef
WHERE sMasterName LIKE N'%Type%'
ORDER BY sMasterName
"@

Run "dependent master 5634" @"
SELECT TOP 20 * FROM dbo.cCore_VoucherDependentMaster_0 WHERE iVoucherType = 5634
"@

Run "field 306676 extra" @"
SELECT TOP 10 * FROM dbo.cCore_Fields WHERE iFieldId IN (306676, 307233)
"@

Run "sample AC-8402 Saeed contracts receipts" @"
SELECT TOP 12 h.sVoucherNo, h.iVoucherType, h.iDate, d.iCode, d.iBookNo, d.mAmount1, acc.sName
FROM dbo.tCore_Header_0 h
INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId = h.iHeaderId
LEFT JOIN dbo.mCore_Account acc ON acc.iMasterId = CASE WHEN h.iVoucherType = 5634 THEN d.iBookNo ELSE d.iCode END
WHERE acc.sName LIKE N'%Mohamed Saeed%'
  AND h.iVoucherType IN (5634, 4608, 4609, 4610)
  AND ISNULL(h.bCancelled,0)=0 AND ISNULL(h.bVersion,0)=0
ORDER BY h.iDate
"@

$conn.Close()
