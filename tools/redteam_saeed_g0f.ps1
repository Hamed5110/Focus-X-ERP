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
                    if ($v.Length -gt 180) { $v = $v.Substring(0,180) + "..." }
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
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME LIKE N'%VoucherType%' OR TABLE_NAME LIKE N'%5634%'
ORDER BY TABLE_NAME
"@ "type tables"

Dump @"
SELECT c.name FROM sys.columns c
WHERE c.object_id = OBJECT_ID(N'dbo.tCore_HeaderData5634_0')
ORDER BY c.column_id
"@ "5634 extras cols"

Dump @"
SELECT * FROM dbo.tCore_HeaderData5634_0 WHERE iHeaderId = 164378
"@ "5634 extras CON-Atl-6955"

Dump @"
SELECT TOP 20 * FROM dbo.tCore_Refrn_0
WHERE iHeaderId IN (165821,165822,164378)
   OR iRefHeaderId IN (165821,165822,164378)
"@ "refrn 1383/1384/6955"

Dump @"
SELECT h.sVoucherNo, h.iVoucherType, h.iDate, d.iCode, d.mAmount1, d.iFaTag
FROM dbo.tCore_Header_0 h
JOIN dbo.tCore_Data_0 d ON d.iHeaderId=h.iHeaderId
WHERE h.sVoucherNo LIKE N'%1383%' AND h.iDate BETWEEN 132777985 AND 132778015
"@ "any Aug voucher with 1383"

Dump @"
SELECT h.sVoucherNo, h.iVoucherType,
  CAST((h.iDate & 0xfff0000)/65536 AS varchar(4)) + N'-'
    + RIGHT(N'0'+CAST((h.iDate & 0xff00)/256 AS varchar(2)),2) + N'-'
    + RIGHT(N'0'+CAST((h.iDate & 0xff) AS varchar(2)),2) AS VoucherDate,
  d.iCode, d.mAmount1
FROM dbo.tCore_Header_0 h
JOIN dbo.tCore_Data_0 d ON d.iHeaderId=h.iHeaderId
WHERE h.sVoucherNo LIKE N'SQ-%' AND (d.iCode=21061 OR d.iBookNo=21061)
"@ "quotes for AC-8402"
