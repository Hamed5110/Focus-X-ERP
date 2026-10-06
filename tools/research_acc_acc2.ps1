function DumpDb($db, $sql, $title) {
    Write-Output ""
    Write-Output "========== [$db] $title =========="
    $cs = "Server=localhost;Database=$db;User Id=sa;Password=$env:FOCUS_SQL_PASSWORD;Encrypt=False;TrustServerCertificate=True;"
    $conn = New-Object System.Data.SqlClient.SqlConnection $cs
    try { $conn.Open() } catch { Write-Output $_.Exception.Message; return }
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
            if ($c -ge 40) { Write-Output "(truncated)"; break }
        }
        $r.Close()
        if ($c -eq 0) { Write-Output "(no rows)" }
    } catch { Write-Output $_.Exception.Message }
    $conn.Close()
}

foreach ($db in @("Focus80G0","Focus80E0","Focus8080")) {
    DumpDb $db @"
SELECT iFieldId, sCaption, iDataTypeId, iModuleType
FROM dbo.cCore_Fields
WHERE sCaption LIKE N'%Account%'
ORDER BY iFieldId
"@ "cCore_Fields Account*"
}
