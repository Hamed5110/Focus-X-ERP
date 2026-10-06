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
            if ($n -ge 20) { Write-Output "... truncated"; break }
        }
        $r.Close()
        Write-Output ("rows_shown=$n")
    } catch {
        Write-Output ("FAIL: " + $_.Exception.Message)
        try { if ($r) { $r.Close() } } catch {}
    }
}

Run "unit tables" @"
SELECT name FROM sys.tables WHERE name LIKE '%Unit%' AND name LIKE 'mCore%' ORDER BY name
"@

Run "auth draft" @"
SELECT h.iAuth, ISNULL(h.bDraft,0) Draft, ISNULL(h.bCancelled,0) Canc, COUNT(*) Cnt
FROM dbo.tCore_Header_0 h
WHERE h.iVoucherType IN (2562, 2563)
GROUP BY h.iAuth, ISNULL(h.bDraft,0), ISNULL(h.bCancelled,0)
ORDER BY Cnt DESC
"@

Run "PY last rate City Glasses 3975" @"
SELECT TOP 5 h.sVoucherNo, h.iDate, i.mRate, (h.iDate / 65536) AS Yr
FROM dbo.tCore_Header_0 h
INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId = h.iHeaderId
INNER JOIN dbo.tCore_Indta_0 i ON i.iBodyId = d.iBodyId
WHERE h.iVoucherType IN (2562, 2563)
  AND d.iBookNo = 8208 AND i.iProduct = 3975
  AND (h.iDate / 65536) = 2025
  AND ISNULL(h.bCancelled,0)=0 AND ISNULL(h.bVersion,0)=0
  AND i.mRate > 0
ORDER BY h.iDate DESC, h.iHeaderId DESC, d.iBodyId DESC
"@

Run "indta unit" @"
SELECT TOP 3 i.iUnit, ISNULL(u.sName, N'?') AS UName
FROM dbo.tCore_Indta_0 i
LEFT JOIN dbo.mCore_Units u ON u.iMasterId = i.iUnit
WHERE i.iUnit > 0
"@

$conn.Close()
