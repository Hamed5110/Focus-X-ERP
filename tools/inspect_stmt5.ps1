$cs = "Server=localhost;Database=Focus80G0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
function Run($title, $sql) {
    Write-Output "==== $title ===="
    $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 90; $cmd.CommandText = $sql
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
            if ($n -ge 25) { Write-Output "... truncated"; break }
        }
        $r.Close()
        Write-Output ("rows_shown=$n")
    } catch {
        Write-Output ("FAIL: " + $_.Exception.Message)
        try { if ($r) { $r.Close() } } catch {}
    }
}

Run "fields Opportunity" @"
SELECT iFieldId, sCaption, iDataTypeId, iModuleType
FROM dbo.cCore_Fields
WHERE sCaption LIKE N'%Opport%'
"@

Run "screen fields 5634 opportunity" @"
SELECT TOP 20 sf.*
FROM dbo.cCore_VoucherScreenFields_0 sf
WHERE sf.iVoucherType = 5634
"@

Run "master def opportunity" @"
SELECT TOP 20 iMasterType, sMasterName, sTableName
FROM dbo.cCore_MasterDef
WHERE sMasterName LIKE N'%Opport%' OR sTableName LIKE N'%Opport%'
   OR sMasterName LIKE N'%Type%'
"@

Run "tables mCore_op" @"
SELECT name FROM sys.tables WHERE name LIKE 'mCore_o%' OR name LIKE 'mCore_O%' ORDER BY name
"@

Run "cCore_MasterDef cols" @"
SELECT c.name FROM sys.columns c WHERE c.object_id = OBJECT_ID('dbo.cCore_MasterDef') ORDER BY c.column_id
"@

$conn.Close()
