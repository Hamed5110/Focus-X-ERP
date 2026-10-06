$cs = "Server=localhost;Database=Focus80G0;User Id=sa;Password=$env:FOCUS_SQL_PASSWORD;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
function Dump($sql, $title) {
    Write-Output ""
    Write-Output "========== $title =========="
    $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 120; $cmd.CommandText = $sql
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
                    if ($v.Length -gt 160) { $v = $v.Substring(0,160) + "..." }
                    $parts += $v
                }
            }
            Write-Output ($parts -join " | ")
            if ($c -ge 50) { Write-Output "(truncated)"; break }
        }
        $r.Close()
        if ($c -eq 0) { Write-Output "(no rows)" }
    } catch { Write-Output $_.Exception.Message }
}

Dump @"
SELECT iFieldId, sCaption FROM dbo.cCore_Fields
WHERE iFieldId IN (1,2,3,19,171,5002,5003,5042)
"@ "known cube field ids"

Dump @"
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.COLUMNS
WHERE COLUMN_NAME LIKE N'%SubParent%' OR COLUMN_NAME LIKE N'%Account2%' OR COLUMN_NAME LIKE N'%iBookNo%'
ORDER BY TABLE_NAME
"@ "tables with SubParent/Account2/iBookNo"

Dump @"
SELECT name, type_desc
FROM sys.objects
WHERE type IN ('P','FN','IF','TF','V')
  AND (
    OBJECT_DEFINITION(object_id) LIKE N'%Account2%'
    OR OBJECT_DEFINITION(object_id) LIKE N'%iBookNo%'
    OR name LIKE N'%Account2%'
    OR name LIKE N'%Acc2%'
  )
ORDER BY type_desc, name
"@ "procs/funcs/views mentioning Account2 or iBookNo"
