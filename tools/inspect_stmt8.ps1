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
            if ($n -ge 30) { Write-Output "... truncated"; break }
        }
        $r.Close()
        Write-Output ("rows_shown=$n")
    } catch {
        Write-Output ("FAIL: " + $_.Exception.Message)
        try { if ($r) { $r.Close() } } catch {}
    }
}

Run "dropdown tables" @"
SELECT name FROM sys.tables
WHERE name LIKE '%Drop%' OR name LIKE '%ListVal%' OR name LIKE '%Enum%'
   OR name LIKE 'cCore_Extra%' OR name LIKE '%FieldValue%'
ORDER BY name
"@

Run "master def all 30xx around opport" @"
SELECT iMasterTypeId, sMasterName FROM dbo.cCore_MasterDef
WHERE iMasterTypeId BETWEEN 3055 AND 3090
ORDER BY iMasterTypeId
"@

Run "contractstatusmaster" @"
SELECT iMasterId, sCode, sName FROM dbo.mCore_contractstatusmaster ORDER BY iMasterId
"@

Run "tables containing opportunitytype col + master" @"
SELECT t.name
FROM sys.tables t
JOIN sys.columns c ON c.object_id = t.object_id AND c.name = 'OpportunityType'
ORDER BY t.name
"@

$conn.Close()
