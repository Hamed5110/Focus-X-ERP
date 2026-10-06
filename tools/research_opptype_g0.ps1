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
                    if ($v.Length -gt 300) { $v = $v.Substring(0,300) + "..." }
                    $parts += $v
                }
            }
            Write-Output ($parts -join " | ")
            if ($c -ge 60) { Write-Output "(truncated)"; break }
        }
        $r.Close()
        if ($c -eq 0) { Write-Output "(no rows)" }
    } catch {
        Write-Output ("ERROR: " + $_.Exception.Message)
        try { $r.Close() } catch {}
    }
}

Dump @"
SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'cCore_VoucherFields_0'
ORDER BY ORDINAL_POSITION
"@ "VoucherFields columns"

Dump @"
SELECT *
FROM dbo.cCore_VoucherFields_0
WHERE sFieldName LIKE N'%Opport%'
"@ "Opportunity voucher fields"

Dump @"
SELECT TOP 20 iFieldId, sCaption, sFieldName, iDataType, iControlType, iMasterTypeId
FROM dbo.cCore_Fields
WHERE sFieldName LIKE N'%Opport%' OR sCaption LIKE N'%Opport%'
"@ "cCore_Fields opportunity"

Dump @"
SELECT name FROM sys.tables
WHERE name LIKE '%Drop%' OR name LIKE '%Combo%' OR name LIKE '%Pick%'
   OR name LIKE '%Enum%' OR name LIKE '%ListVal%' OR name LIKE '%Option%'
   OR name LIKE '%FieldValue%' OR name LIKE '%DefaultValue%'
ORDER BY name
"@ "dropdown-like tables"

Dump @"
SELECT iMasterTypeId, sMasterName FROM dbo.cCore_MasterDef
WHERE sMasterName LIKE N'%Opport%' OR sMasterName LIKE N'%Business%'
   OR sMasterName LIKE N'%Type%'
ORDER BY iMasterTypeId
"@ "master def opportunity/type"
