$cs = "Server=localhost;Database=Focus80E0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
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
                    if ($v.Length -gt 200) { $v = $v.Substring(0,200) + "..." }
                    $parts += $v
                }
            }
            Write-Output ($parts -join " | ")
            if ($c -ge 40) { Write-Output "(truncated)"; break }
        }
        $r.Close()
        if ($c -eq 0) { Write-Output "(no rows)" }
    } catch {
        Write-Output ("ERROR: " + $_.Exception.Message)
        try { if ($r) { $r.Close() } } catch {}
    }
}

Dump @"
SELECT COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME='tCore_Refrn_0' ORDER BY ORDINAL_POSITION
"@ "tCore_Refrn_0 columns"

Dump @"
SELECT TOP 8 * FROM dbo.tCore_Refrn_0
"@ "sample refrn"

Dump @"
SELECT TOP 8 * FROM dbo.tCore_Links_0
"@ "sample links"

Dump @"
SELECT
  COUNT(*) Cnt,
  SUM(CASE WHEN ISNULL(d.iInvTag,0)>0 THEN 1 ELSE 0 END) AS HasInvTag
FROM dbo.tCore_Header_0 h
JOIN dbo.tCore_Data_0 d ON d.iHeaderId=h.iHeaderId
WHERE h.iVoucherType IN (4608,4609,4610) AND d.bUpdateFA=1 AND d.iFaTag IN (2040,2057)
"@ "receipt iInvTag fill"

Dump @"
SELECT TOP 10
  h.sVoucherNo, h.iVoucherType, d.iInvTag, d.iCode, d.mAmount1,
  so.sVoucherNo AS SONo, so.iVoucherType AS SOType
FROM dbo.tCore_Header_0 h
JOIN dbo.tCore_Data_0 d ON d.iHeaderId=h.iHeaderId
LEFT JOIN dbo.tCore_Header_0 so ON so.iHeaderId = d.iInvTag
WHERE h.iVoucherType IN (4608,4609,4610)
  AND ISNULL(d.iInvTag,0)>0
  AND d.iFaTag IN (2040,2057)
"@ "receipt iInvTag -> header?"
