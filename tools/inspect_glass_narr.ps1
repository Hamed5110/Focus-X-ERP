$cs = "Server=localhost;Database=Focus80G0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
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
                    if ($v.Length -gt 140) { $v = $v.Substring(0,140) + "..." }
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
        try { $r.Close() } catch {}
    }
}

Dump @"
SELECT TABLE_NAME, COLUMN_NAME, DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE COLUMN_NAME LIKE N'%Narrat%'
   OR COLUMN_NAME LIKE N'%Remarks%'
   OR COLUMN_NAME LIKE N'%Notepad%'
   OR COLUMN_NAME LIKE N'%Comment%'
ORDER BY TABLE_NAME, COLUMN_NAME
"@ "narration-like columns"

Dump @"
SELECT COLUMN_NAME, DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = N'tCore_Header_0'
  AND (COLUMN_NAME LIKE N'%Narr%' OR COLUMN_NAME LIKE N'%Remark%' OR COLUMN_NAME LIKE N'%Note%' OR COLUMN_NAME LIKE N's%')
ORDER BY ORDINAL_POSITION
"@ "header text-ish"

Dump @"
SELECT COLUMN_NAME, DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = N'tCore_HeaderData2562_0'
ORDER BY ORDINAL_POSITION
"@ "HeaderData2562"

Dump @"
SELECT COLUMN_NAME, DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = N'tCore_Data2562_0'
ORDER BY ORDINAL_POSITION
"@ "Data2562"

Dump @"
SELECT iMasterId, sCode, sName FROM dbo.mCore_deliverystatus ORDER BY iMasterId
"@ "delivery status"
