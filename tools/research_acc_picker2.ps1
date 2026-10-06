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
                    if ($v.Length -gt 140) { $v = $v.Substring(0,140) + "..." }
                    $parts += $v
                }
            }
            Write-Output ($parts -join " | ")
            if ($c -ge 30) { break }
        }
        $r.Close()
        if ($c -eq 0) { Write-Output "(no rows)" }
    } catch { Write-Output $_.Exception.Message }
}

Dump @"
SELECT iViewId, iMasterId, sCaption, bDefault
FROM dbo.cCore_MasterView
WHERE iMasterId = 1
"@ "Account master views"

Dump @"
SELECT TOP 25 iMasterId, iTreeId, iFieldId, iOperatorId, iCompareWith, sValue, iConjunction, iSequence
FROM dbo.cCore_ViewConditions
WHERE iMasterId = 1 OR iTreeId IN (SELECT iTreeId FROM dbo.cCore_TreeView WHERE iMasterTypeId = 1)
ORDER BY iTreeId, iSequence
"@ "Account view conditions"

Dump @"
SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = N'cCore_TreeView' ORDER BY ORDINAL_POSITION
"@ "TreeView cols"

Dump @"
SELECT TOP 20 * FROM dbo.cCore_TreeView WHERE iMasterTypeId = 1 OR iMasterId = 1
"@ "Account trees"

Dump @"
SELECT OBJECT_DEFINITION(OBJECT_ID(N'dbo.vCore_GetMasterViews'))
"@ "vCore_GetMasterViews"

Dump @"
SELECT TOP 15 iFieldId, sFilterOnColumnName, iOperator, iFilterField, sLinkField
FROM dbo.cCore_MasterFieldsFilter
WHERE sFilterOnColumnName LIKE N'%Account%' OR sLinkField LIKE N'%Account%' OR iFieldId IN (5002,5003,1)
"@ "master field filters"
