$cs = "Server=localhost;Database=Focus80G0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
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
                    $v = $v -replace "`r|`n"," "
                    if ($v.Length -gt 140) { $v = $v.Substring(0,140) + "..." }
                    $parts += $v
                }
            }
            Write-Output ($parts -join " | ")
            if ($c -ge 45) { Write-Output "(truncated)"; break }
        }
        $r.Close()
        if ($c -eq 0) { Write-Output "(no rows)" }
    } catch {
        Write-Output ("ERROR: " + $_.Exception.Message)
        try { if ($r) { $r.Close() } } catch {}
    }
}

Dump @"
SELECT c.name FROM sys.columns c
WHERE c.object_id = OBJECT_ID(N'dbo.tCore_Data_0')
ORDER BY c.column_id
"@ "data cols"

Dump @"
SELECT c.name FROM sys.columns c
WHERE c.object_id = OBJECT_ID(N'dbo.tCore_Refrn_0')
ORDER BY c.column_id
"@ "refrn cols"

Dump @"
SELECT iMasterId, sName FROM dbo.mCore_Salesman WHERE sName LIKE N'%Jawad%' OR sName LIKE N'%Jawwad%'
"@ "Jawad salesman"

Dump @"
SELECT TOP 3 *
FROM dbo.tCore_Data_0 d
WHERE d.iHeaderId = 164378
"@ "CON body sample"
