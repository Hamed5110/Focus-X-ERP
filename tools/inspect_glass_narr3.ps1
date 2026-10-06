$cs = "Server=localhost;Database=Focus80G0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
function Dump($sql, $title) {
    Write-Output ""
    Write-Output "========== $title =========="
    $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 60; $cmd.CommandText = $sql
    $r = $cmd.ExecuteReader()
    $n = $r.FieldCount
    $hdr = @(); for ($i=0; $i -lt $n; $i++) { $hdr += $r.GetName($i) }
    Write-Output ($hdr -join " | ")
    $c = 0
    while ($r.Read()) {
        $c++
        $parts = @(); for ($i=0; $i -lt $n; $i++) {
            if ($r.IsDBNull($i)) { $parts += "" } else { $parts += ([string]$r.GetValue($i) -replace "`r|`n"," ") }
        }
        Write-Output ($parts -join " | ")
        if ($c -ge 25) { break }
    }
    $r.Close()
    if ($c -eq 0) { Write-Output "(no rows)" }
}

Dump @"
SELECT TABLE_NAME, COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME IN (N'tCore_HeaderData2560_0',N'tCore_HeaderData2562_0',N'tCore_HeaderData2563_0',N'tCore_HeaderData2564_0')
ORDER BY TABLE_NAME, ORDINAL_POSITION
"@ "header extra cols"

Dump @"
SELECT h.iVoucherType, COUNT(*) AS n
FROM dbo.tCore_Header_0 h
WHERE h.iVoucherClass = 2560 AND ISNULL(h.bCancelled,0)=0 AND ISNULL(h.bVersion,0)=0 AND ISNULL(h.bSuspended,0)=0
GROUP BY h.iVoucherType
"@ "PO class counts"

Dump @"
SELECT TOP 3 iTag3010, iTag3054, iTag3080 FROM dbo.tCore_Data_Tags_0 WHERE iTag3010 > 0
"@ "tags exist"
