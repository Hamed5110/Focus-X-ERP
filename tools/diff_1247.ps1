$cs = "Server=localhost;Database=Focus8080;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()

function Q($sql) {
    $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 280; $cmd.CommandText = $sql
    $a = New-Object System.Data.SqlClient.SqlDataAdapter $cmd
    $dt = New-Object System.Data.DataTable
    [void]$a.Fill($dt)
    , $dt
}

function Show-Diff($dt, $label) {
    Write-Host ("=== " + $label + " (" + $dt.Rows.Count + " rows) ===")
    $rowArray = @($dt.Rows)
    if ($rowArray.Count -lt 2) {
        foreach ($r in $rowArray) { $r | Format-List | Out-String -Width 200 | Write-Host }
        return
    }
    foreach ($col in $dt.Columns) {
        $cn = $col.ColumnName
        $vals = @()
        foreach ($r in $rowArray) { $vals += "$($r[$cn])" }
        if (($vals | Select-Object -Unique).Count -gt 1) {
            Write-Host ($cn.PadRight(30) + " : " + ($vals -join " | "))
        }
    }
}

Show-Diff (Q "
SELECT * FROM dbo.tCore_Header_0
WHERE iVoucherType = 4610 AND sVoucherNo IN ('ATIC-26-1231','ATIC-26-1234','ATIC-26-1247')
ORDER BY sVoucherNo
") "HEADER diff: 1231 | 1234 | 1247"

Show-Diff (Q "
SELECT d.* FROM dbo.tCore_Data_0 d
INNER JOIN dbo.tCore_Header_0 h ON h.iHeaderId = d.iHeaderId
WHERE h.iVoucherType = 4610 AND h.sVoucherNo IN ('ATIC-26-1231','ATIC-26-1234','ATIC-26-1247')
  AND d.iFaTag = 2040
ORDER BY h.sVoucherNo, d.iSerialNo
") "DATA-LINE diff: 1231 | 1234 | 1247"

$conn.Close()
