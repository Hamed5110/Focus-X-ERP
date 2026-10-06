$cs = "Server=localhost;Database=Focus80G0;User Id=sa;Password=$env:FOCUS_SQL_PASSWORD;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
function Dump($sql, $title) {
    Write-Output ""
    Write-Output "========== $title =========="
    $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 60; $cmd.CommandText = $sql
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
                    if ($v.Length -gt 80) { $v = $v.Substring(0,80) + "..." }
                    $parts += $v
                }
            }
            Write-Output ($parts -join " | ")
            if ($c -ge 25) { break }
        }
        $r.Close()
        if ($c -eq 0) { Write-Output "(no rows)" }
    } catch { Write-Output $_.Exception.Message }
}

Dump "SELECT iTreeId, iMasterTypeId, sTreeName, LEFT(ISNULL(sCondition,N''),80) Cond FROM dbo.cCore_TreeView WHERE iMasterTypeId = 1" "Account trees"

Dump @"
SELECT iMasterId, sName FROM dbo.mCore_AccountType
"@ "AccountType try"

Dump @"
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME LIKE N'%AccountType%' OR TABLE_NAME LIKE N'%accounttype%'
"@ "accounttype tables"

Dump @"
SELECT TOP 8 acc.sCode, acc.sName, acc.iAccountType, t.sName AS TypeName
FROM dbo.mCore_Account acc
LEFT JOIN dbo.mCore_AccountType t ON t.iMasterId = acc.iAccountType
WHERE acc.sName LIKE N'%Trade Pay%' OR acc.sName LIKE N'%Ebrahim Saad%' OR acc.sCode LIKE N'AC-847'
"@ "types of payable vs customer"

Dump @"
SELECT iFieldId, sCaption FROM dbo.cCore_Fields WHERE iFieldId IN (5004,5023,5316)
"@ "field captions"

Dump @"
SELECT COUNT(*) Cnt, iAccountType
FROM dbo.mCore_Account
WHERE ISNULL(bGroup,0)=0
GROUP BY iAccountType
ORDER BY COUNT(*) DESC
"@ "leaf accounts by type"
