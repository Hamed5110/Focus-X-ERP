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

Run "tables like opportunitytype" @"
SELECT name FROM sys.objects WHERE name LIKE '%pportunit%' OR name LIKE '%pportunity%'
ORDER BY name
"@

Run "cCore_Fields OpportunityType" @"
SELECT TOP 10 iId, sCaption, sFieldName, iMasterType
FROM dbo.cCore_Fields
WHERE sFieldName LIKE N'%Opport%' OR sCaption LIKE N'%Opport%'
"@

Run "mCore_opportunitytype guess" @"
SELECT name FROM sys.tables WHERE name LIKE 'mCore_%' AND (
  name LIKE '%channel%' OR name LIKE '%segment%' OR name LIKE '%typeof%'
  OR name LIKE '%project%' OR name LIKE '%building%'
) ORDER BY name
"@

Run "vaCore CON extra names" @"
SELECT c.name FROM sys.columns c
WHERE c.object_id IN (OBJECT_ID('dbo.vaCore_Account'), OBJECT_ID('dbo.vCore_HeaderData5634'))
  AND c.name LIKE '%Opport%'
"@

Run "master id for OpportunityType" @"
SELECT TOP 20 iMasterId, sCode, sName FROM dbo.mCore_MarketSegment
"@

Run "all mCore with Type in last part" @"
SELECT name FROM sys.tables WHERE name LIKE 'mCore_%Type' OR name LIKE 'mCore_%type'
ORDER BY name
"@

$conn.Close()
