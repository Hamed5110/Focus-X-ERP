$cs = "Server=localhost;Database=Focus80E0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 60
function Dump($sql, $title) {
    Write-Output ""
    Write-Output "========== $title =========="
    $cmd.CommandText = $sql
    try {
        $r = $cmd.ExecuteReader()
        $n = $r.FieldCount
        $hdr = @(); for ($i=0; $i -lt $n; $i++) { $hdr += $r.GetName($i) }
        Write-Output ($hdr -join " | ")
        while ($r.Read()) {
            $parts = @()
            for ($i=0; $i -lt $n; $i++) {
                if ($r.IsDBNull($i)) { $parts += "" } else {
                    $v = [string]$r.GetValue($i)
                    if ($v.Length -gt 8000) { $v = $v.Substring(0,8000) + "...TRUNC" }
                    $parts += $v
                }
            }
            Write-Output ($parts -join " | ")
        }
        $r.Close()
    } catch {
        Write-Output ("ERROR: " + $_.Exception.Message)
    }
}

Dump "SELECT OBJECT_DEFINITION(OBJECT_ID('dbo.fCore_GetDate')) AS Def" "fCore_GetDate"
Dump "SELECT OBJECT_DEFINITION(OBJECT_ID('dbo.fCore_GetDateRange')) AS Def" "fCore_GetDateRange"
Dump "SELECT OBJECT_DEFINITION(OBJECT_ID('dbo.fCore_GetDate')) AS Def" "fCore_GetDate again"
Dump @"
SELECT OBJECT_NAME(object_id) N, type, LEFT(OBJECT_DEFINITION(object_id), 400) D
FROM sys.objects
WHERE OBJECT_DEFINITION(object_id) LIKE '%GetDateRange%'
   OR name IN ('fCore_GetDate','fCore_GetDateRange','fCore_GetStartEndDateForMonth')
"@ "refs to GetDateRange"

Dump "SELECT OBJECT_DEFINITION(OBJECT_ID('dbo.fCore_GetStartEndDateForMonth')) AS Def" "fCore_GetStartEndDateForMonth"

# params of those functions
Dump @"
SELECT OBJECT_NAME(object_id) N, name, TYPE_NAME(user_type_id) T, max_length, parameter_id
FROM sys.parameters
WHERE object_id IN (OBJECT_ID('dbo.fCore_GetDate'), OBJECT_ID('dbo.fCore_GetDateRange'), OBJECT_ID('dbo.fCore_GetStartEndDateForMonth'))
ORDER BY object_id, parameter_id
"@ "function parameters"

$conn.Close()
