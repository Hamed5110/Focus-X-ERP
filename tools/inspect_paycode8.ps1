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
                if ($r.IsDBNull($i)) { $parts += "" } else {
                    $v = [string]$r.GetValue($i)
                    if ($v.Length -gt 180) { $v = $v.Substring(0,180) + "..." }
                    $parts += $v
                }
            }
            Write-Output ($parts -join " | ")
            if ($c -ge 80) { break }
        }
        $r.Close()
        if ($c -eq 0) { Write-Output "(no rows)" }
    } catch {
        Write-Output ("ERROR: " + $_.Exception.Message)
    }
}

Dump @"
SELECT ff.sFieldName, f.sCaption, ff.iTableId, ff.iDisplayControlType, ff.iLinkType, ff.iLinkTypeId,
       ff.sExternalTableName, ff.sExternalValueMember, ff.sExternalDisplayMember, ff.sDefaultValue
FROM dbo.cCore_FormFields ff
INNER JOIN dbo.cCore_Fields f ON f.iFieldId = ff.iFieldId
WHERE ff.sFieldName LIKE N'%Payment%' OR f.sCaption LIKE N'%Payment%'
"@ "Payment form fields"

Dump @"
SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME='cCore_Tables' OR TABLE_NAME='cCore_Vouchers_0'
ORDER BY TABLE_NAME, ORDINAL_POSITION
"@ "table meta"

Dump @"
SELECT TOP 20 TABLE_NAME FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME LIKE 'cCore_Table%' OR TABLE_NAME LIKE 'cCore_Voucher%'
ORDER BY TABLE_NAME
"@ "cCore table names"

Dump @"
SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='vCore_TranData_0' AND (
 COLUMN_NAME LIKE '%iHeader%' OR COLUMN_NAME LIKE '%sVoucher%' OR COLUMN_NAME LIKE '%VoucherNo%'
)
"@ "tran view keys"

$conn.Close()
