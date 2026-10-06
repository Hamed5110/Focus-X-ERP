$cs = "Server=localhost;Database=Focus80G0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
function Run($title, $sql) {
    Write-Output "==== $title ===="
    $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 45; $cmd.CommandText = $sql
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
            if ($n -ge 20) { Write-Output "... truncated"; break }
        }
        $r.Close()
        Write-Output ("rows_shown=$n")
    } catch { Write-Output ("FAIL: " + $_.Exception.Message) }
}

Run "pipeline" "SELECT iMasterId, sCode, sName FROM dbo.mCore_pipeline ORDER BY iMasterId"
Run "sitestatus" "SELECT iMasterId, sCode, sName FROM dbo.mCore_sitestatus ORDER BY iMasterId"
Run "source type" "SELECT TOP 15 iMasterId, sCode, sName FROM dbo.mCore_SourceType ORDER BY iMasterId"
Run "master 3000-3054 names" "SELECT iMasterTypeId, sMasterName FROM dbo.cCore_MasterDef WHERE iMasterTypeId BETWEEN 3000 AND 3054 ORDER BY iMasterTypeId"

$conn.Close()
