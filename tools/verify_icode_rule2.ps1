$cs = "Server=localhost;Database=Focus8080;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()

function Q($sql) {
    $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 280; $cmd.CommandText = $sql
    $a = New-Object System.Data.SqlClient.SqlDataAdapter $cmd
    $dt = New-Object System.Data.DataTable
    [void]$a.Fill($dt)
    return $dt
}

Write-Host "=== 1) FULL detail of 12606/15268 iCode-linked dept-2040 lines ==="
Q "
SELECT d.iCode AS Acct, h.iVoucherType, h.sVoucherNo, h.iDate,
       d.iType, d.bUpdateFA, d.iAuthStatus, d.mAmount1, d.mAmount2, d.iBookNo, d.bVoid
FROM dbo.tCore_Header_0 h
INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId = h.iHeaderId
WHERE d.iCode IN (12606, 15268) AND d.iFaTag = 2040
  AND ISNULL(h.iAuth,1)=1 AND ISNULL(h.bCancelled,0)=0 AND ISNULL(h.bVersion,0)=0
  AND ISNULL(h.bSuspended,0)=0 AND ISNULL(d.bVoid,0)=0
ORDER BY d.iCode, h.iVoucherType
" | Format-Table -AutoSize | Out-String -Width 200 | Write-Host

Write-Host "=== 2) name-groups of overlap names: members, statuses (vaCore_Account tree 0), iCode-line counts ==="
Q "
SELECT v.iMasterId, v.sName, v.ReportStatus, v.iTreeId,
       (SELECT COUNT(*) FROM dbo.tCore_Data_0 d
        INNER JOIN dbo.tCore_Header_0 h ON h.iHeaderId = d.iHeaderId
        WHERE d.iCode = v.iMasterId AND d.iFaTag = 2040
          AND ISNULL(h.iAuth,1)=1 AND ISNULL(h.bCancelled,0)=0 AND ISNULL(h.bVersion,0)=0
          AND ISNULL(h.bSuspended,0)=0 AND ISNULL(d.bVoid,0)=0) AS ICodeLines
FROM dbo.vaCore_Account v
WHERE v.sName IN ('Mr. Ali Abdulmajeed Alshakhs','Mr. Ahmed Isa Marhoon Ali','Mr.ali Alhujairi','Mr. Ahmed Abdulla Jaffar','Mr. Hussain Ali')
  AND ISNULL(v.bGroup,0) = 0
ORDER BY v.sName, v.iMasterId, v.iTreeId
" | Format-Table -AutoSize | Out-String -Width 150 | Write-Host

$conn.Close()
