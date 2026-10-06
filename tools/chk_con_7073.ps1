$dbs = @("Focus80G0","Focus80E0","Focus8080")
foreach ($db in $dbs) {
    Write-Output "==== $db ===="
    $cs = "Server=localhost;Database=$db;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
    $conn = New-Object System.Data.SqlClient.SqlConnection $cs
    try { $conn.Open() } catch { Write-Output $_.Exception.Message; continue }
    $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 60
    $cmd.CommandText = @"
SELECT h.sVoucherNo, h.fNet, h.iAuth, h.bCancelled, h.bVersion, h.bSuspended, hd.OpportunityType, h.iDate
FROM dbo.tCore_Header_0 h
LEFT JOIN dbo.tCore_HeaderData5634_0 hd ON hd.iHeaderId = h.iHeaderId
WHERE h.iVoucherType = 5634 AND h.sVoucherNo IN (N'CON-Atl-7073', N'CON-Atl-7074', N'CON-Atl-6142')
"@
    try {
        $r = $cmd.ExecuteReader()
        $n = $r.FieldCount
        $hdr = @(); for ($i=0; $i -lt $n; $i++) { $hdr += $r.GetName($i) }
        Write-Output ($hdr -join " | ")
        $c = 0
        while ($r.Read()) {
            $c++
            $parts = @(); for ($i=0; $i -lt $n; $i++) { if ($r.IsDBNull($i)) { $parts += "" } else { $parts += [string]$r.GetValue($i) } }
            Write-Output ($parts -join " | ")
        }
        $r.Close()
        if ($c -eq 0) { Write-Output "(no rows)" }
    } catch { Write-Output $_.Exception.Message }
    $conn.Close()
}
