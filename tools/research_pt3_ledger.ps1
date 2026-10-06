$cs = "Server=localhost;Database=Focus80G0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
try {
    $conn = New-Object System.Data.SqlClient.SqlConnection $cs
    $conn.Open()
} catch {
    $cs = "Server=localhost;Database=Focus80G0;User Id=sa;Password=$env:FOCUS_SQL_PASSWORD;Encrypt=False;TrustServerCertificate=True;"
    $conn = New-Object System.Data.SqlClient.SqlConnection $cs
    $conn.Open()
}
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
                    if ($v.Length -gt 80) { $v = $v.Substring(0,80) + "..." }
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

Dump "SELECT TOP 1 iMasterId, sName FROM dbo.mCore_Department WHERE sName LIKE N'%Atlas%'" "atlas dept"
Dump @"
SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME IN (N'vaCore_Account', N'vmCore_Account', N'vtCode_DataFA_0')
ORDER BY TABLE_NAME, ORDINAL_POSITION
"@ "account/fa cols"
Dump @"
SELECT TOP 3 * FROM dbo.vtCode_DataFA_0 WHERE iMasterId > 0 AND iFaTag = 2040
"@ "fa sample"
