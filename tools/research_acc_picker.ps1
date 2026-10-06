$cs = "Server=localhost;Database=Focus80G0;User Id=sa;Password=$env:FOCUS_SQL_PASSWORD;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
function Dump($sql, $title) {
    Write-Output ""
    Write-Output "========== $title =========="
    $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 90; $cmd.CommandText = $sql
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
                    if ($v.Length -gt 150) { $v = $v.Substring(0,150) + "..." }
                    $parts += $v
                }
            }
            Write-Output ($parts -join " | ")
            if ($c -ge 35) { break }
        }
        $r.Close()
        if ($c -eq 0) { Write-Output "(no rows)" }
    } catch { Write-Output $_.Exception.Message }
}

Dump @"
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME LIKE N'%MasterView%'
   OR TABLE_NAME LIKE N'%ViewCondition%'
   OR TABLE_NAME LIKE N'%MasterDef%'
   OR TABLE_NAME LIKE N'%MasterFieldsFilter%'
   OR TABLE_NAME LIKE N'%FilterCustomization%'
ORDER BY TABLE_NAME
"@ "filter/view tables"

Dump @"
SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = N'cCore_MasterFieldsFilter' ORDER BY ORDINAL_POSITION
"@ "MasterFieldsFilter cols"

Dump @"
SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = N'cCore_ViewConditions' ORDER BY ORDINAL_POSITION
"@ "ViewConditions cols"

Dump @"
SELECT TOP 20 * FROM dbo.cCore_MasterView
"@ "MasterView sample"

Dump @"
SELECT name FROM sys.objects
WHERE type IN ('P','V','FN')
  AND (
    name LIKE N'%Account%'
    OR name LIKE N'%MasterSearch%'
    OR name LIKE N'%Popup%'
    OR name LIKE N'%Parameter%'
  )
ORDER BY name
"@ "account/search/param objects"
