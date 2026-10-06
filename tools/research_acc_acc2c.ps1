$cs = "Server=localhost;Database=Focus80G0;User Id=sa;Password=$env:FOCUS_SQL_PASSWORD;Encrypt=False;TrustServerCertificate=True;"
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
                    if ($v.Length -gt 180) { $v = $v.Substring(0,180) + "..." }
                    $parts += $v
                }
            }
            Write-Output ($parts -join " | ")
            if ($c -ge 40) { break }
        }
        $r.Close()
        if ($c -eq 0) { Write-Output "(no rows)" }
    } catch { Write-Output $_.Exception.Message }
}

Dump @"
SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = N'vtCode_DataFA_0'
ORDER BY ORDINAL_POSITION
"@ "vtCode_DataFA_0 cols"

Dump @"
SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = N'tCore_Data_0'
  AND (COLUMN_NAME LIKE N'%Code%' OR COLUMN_NAME LIKE N'%Book%' OR COLUMN_NAME LIKE N'%Master%'
    OR COLUMN_NAME LIKE N'%Acc%' OR COLUMN_NAME LIKE N'%Tag%')
ORDER BY COLUMN_NAME
"@ "tCore_Data_0 acc cols"

Dump @"
SELECT OBJECT_DEFINITION(OBJECT_ID(N'dbo.fCore_OpeningBalanaceOn_0'))
"@ "fn OpeningBalance"

Dump @"
SELECT OBJECT_DEFINITION(OBJECT_ID(N'dbo.vCore_AccountBalances_0'))
"@ "view AccountBalances"

Dump @"
SELECT OBJECT_DEFINITION(OBJECT_ID(N'dbo.vtAccount_DrCr_0'))
"@ "view vtAccount_DrCr"
