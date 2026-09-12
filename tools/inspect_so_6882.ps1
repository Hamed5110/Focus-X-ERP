$cs = "Server=localhost;Database=Focus80E0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
function Dump($sql) {
    $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 180; $cmd.CommandText = $sql
    $r = $cmd.ExecuteReader()
    $n = $r.FieldCount
    $hdr = @(); for ($i=0;$i -lt $n;$i++) { $hdr += $r.GetName($i) }
    Write-Output ("COLS: " + ($hdr -join " | "))
    $c = 0
    while ($r.Read()) {
        $parts = @()
        for ($i = 0; $i -lt $n; $i++) {
            if ($r.IsDBNull($i)) { $parts += "" } else { $parts += [string]$r.GetValue($i) }
        }
        Write-Output ($parts -join " | ")
        $c++
        if ($c -ge 40) { break }
    }
    $r.Close()
    Write-Output ("-- shown: $c --")
}

Write-Output "=== HeaderData 5634 columns ==="
Dump "SELECT COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='tCore_HeaderData5634_0' ORDER BY ORDINAL_POSITION"

Write-Output "`n=== Data 5634 extra columns ==="
Dump "SELECT COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='tCore_Data5634_0' ORDER BY ORDINAL_POSITION"

Write-Output "`n=== HeaderData5634 for CON-Atl-6882 ==="
Dump @"
SELECT hd.*
FROM dbo.tCore_Header_0 h
INNER JOIN dbo.tCore_HeaderData5634_0 hd ON hd.iHeaderId = h.iHeaderId
WHERE h.sVoucherNo = 'CON-Atl-6882'
"@

Write-Output "`n=== SO body lines 6882 ==="
Dump @"
SELECT d.iBodyId, d.iBookNo, d.iCode, d.iFaTag, d.mAmount1, d.mAmount2, d.mOriginalAmount, d.iType, a.sName, a.sCode
FROM dbo.tCore_Header_0 h
INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId = h.iHeaderId
LEFT JOIN dbo.mCore_Account a ON a.iMasterId = d.iBookNo
WHERE h.sVoucherNo = 'CON-Atl-6882'
"@

Write-Output "`n=== tags on 6882 ==="
Dump @"
SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME='tCore_Data_Tags_0' AND COLUMN_NAME LIKE 'iTag%'
ORDER BY COLUMN_NAME
"@

$conn.Close()
