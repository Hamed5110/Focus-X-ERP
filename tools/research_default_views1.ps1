$cs = "Server=localhost;Database=Focus80E0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 180
function Dump($sql, $title) {
    Write-Output ""
    Write-Output "========== $title =========="
    $cmd.CommandText = $sql
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
                    if ($v.Length -gt 500) { $v = $v.Substring(0,500) + "..." }
                    $parts += $v
                }
            }
            Write-Output ($parts -join " | ")
            if ($c -ge 35) { break }
        }
        $r.Close()
    } catch {
        Write-Output ("ERROR: " + $_.Exception.Message)
        try { $conn.Close(); $conn.Open(); $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 180 } catch {}
    }
}

Dump @"
SELECT name FROM sys.objects
WHERE type IN ('P','V','FN','TF')
  AND (
    name LIKE '%DefaultView%' OR name LIKE '%FormDefault%'
    OR name LIKE '%CreateInternal%' OR name LIKE '%ReportView%'
    OR name LIKE '%TranData%' OR name LIKE '%DataFA%'
  )
ORDER BY type, name
"@ "default view objects"

Dump @"
SELECT COLUMN_NAME, DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME='vtCode_DataFA_0'
  AND (COLUMN_NAME LIKE '%Date%' OR COLUMN_NAME IN ('iDate','iHeaderId','iVoucherType','iFaTag','mAmount1','iCode'))
ORDER BY ORDINAL_POSITION
"@ "vtCode_DataFA date/key cols"

Dump @"
SELECT COLUMN_NAME, DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME='vCore_TranData_0'
  AND COLUMN_NAME IN ('iDate','Date','iHeaderId','iVoucherType','iFaTag','mAmount1','iCode','sVoucherNo')
ORDER BY ORDINAL_POSITION
"@ "TranData key cols"

Dump @"
SELECT TOP 8 TABLE_NAME, COLUMN_NAME, DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE COLUMN_NAME = 'iDate'
  AND TABLE_NAME LIKE 'v%'
ORDER BY TABLE_NAME
"@ "views with column iDate"
