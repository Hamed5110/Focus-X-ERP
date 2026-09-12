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
                    if ($v.Length -gt 120) { $v = $v.Substring(0,120) + "..." }
                    $parts += $v
                }
            }
            Write-Output ($parts -join " | ")
            if ($c -ge 60) { break }
        }
        $r.Close()
        if ($c -eq 0) { Write-Output "(no rows)" }
    } catch {
        Write-Output ("ERROR: " + $_.Exception.Message)
    }
}

Dump @"
SELECT COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME='vCore_TranData_0' AND (
  COLUMN_NAME LIKE '%Pay%' OR COLUMN_NAME LIKE '%Contract%' OR COLUMN_NAME LIKE '%Code%'
)
ORDER BY ORDINAL_POSITION
"@ "vCore_TranData pay cols"

Dump @"
SELECT TOP 5 PaymentCode FROM dbo.vCore_TranData_0 WHERE PaymentCode IS NOT NULL
"@ "vCore_TranData sample"

Dump @"
SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME IN ('cCore_Fields','cCore_FormFields','cCore_MasterFields')
ORDER BY TABLE_NAME, ORDINAL_POSITION
"@ "field meta cols"

Dump @"
SELECT TOP 30 * FROM dbo.cCore_Fields
WHERE sName LIKE N'%Payment%' OR sCaption LIKE N'%Payment%' OR sFieldName LIKE N'%Payment%'
"@ "cCore_Fields payment"

Dump @"
SELECT TOP 20 TABLE_NAME FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME LIKE '%DropDown%' OR TABLE_NAME LIKE '%Combo%' OR TABLE_NAME LIKE '%ListItem%'
   OR TABLE_NAME LIKE 'cCore_Field%'
ORDER BY TABLE_NAME
"@ "dropdown tables"

$conn.Close()
