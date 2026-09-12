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

$asOn = 132778014  # 2026-08-30 packed

Write-Host "=== Future-dated 5634 SOs (ANY account status) contributing to status-1/2/3 NAMES ==="
(Q "
SELECT a.sName, v.ReportStatus AS OwnStatus, h.sVoucherNo, h.iDate, h.fNet
FROM dbo.tCore_Header_0 h
INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId = h.iHeaderId
INNER JOIN dbo.mCore_Account a ON a.iMasterId = d.iBookNo
INNER JOIN dbo.vaCore_Account v ON v.iMasterId = d.iBookNo AND v.iTreeId = 0
WHERE h.iVoucherType = 5634 AND d.iFaTag = 2040 AND d.iBookNo > 0
  AND ISNULL(d.iType, 0) = 0 AND ISNULL(h.iAuth, 1) = 1
  AND ISNULL(h.bCancelled, 0) = 0 AND ISNULL(h.bVersion, 0) = 0 AND ISNULL(h.bSuspended, 0) = 0
  AND ISNULL(d.bVoid, 0) = 0
  AND h.iDate > $asOn
  AND a.sName IN (
      SELECT v2.sName FROM dbo.vaCore_Account v2
      WHERE v2.iTreeId = 0 AND ISNULL(v2.bGroup,0) = 0 AND v2.ReportStatus IN (1,2,3)
  )
ORDER BY a.sName, h.sVoucherNo
").Rows | Format-Table -AutoSize | Out-String -Width 160 | Write-Host

$conn.Close()
