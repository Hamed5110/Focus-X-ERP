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

Write-Host "=== 1) iCode-linked dept-2040 line counts for Ahmed/Hussain status-3 accounts (ALL voucher types) ==="
Q "
SELECT d.iCode AS Acct, h.iVoucherType, COUNT(*) AS Lines
FROM dbo.tCore_Header_0 h
INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId = h.iHeaderId
WHERE d.iCode IN (12606, 15268) AND d.iFaTag = 2040
  AND ISNULL(h.iAuth,1)=1 AND ISNULL(h.bCancelled,0)=0 AND ISNULL(h.bVersion,0)=0
  AND ISNULL(h.bSuspended,0)=0 AND ISNULL(d.bVoid,0)=0
GROUP BY d.iCode, h.iVoucherType
ORDER BY d.iCode, h.iVoucherType
" | Format-Table -AutoSize | Out-String -Width 120 | Write-Host

Write-Host "=== 2) same but ONLY cube credit types (256,4096,4608,4609,4610,8707) ==="
Q "
SELECT d.iCode AS Acct, h.iVoucherType, COUNT(*) AS Lines
FROM dbo.tCore_Header_0 h
INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId = h.iHeaderId
WHERE d.iCode IN (12606, 15268) AND d.iFaTag = 2040
  AND h.iVoucherType IN (256,4096,4608,4609,4610,8707)
  AND ISNULL(h.iAuth,1)=1 AND ISNULL(h.bCancelled,0)=0 AND ISNULL(h.bVersion,0)=0
  AND ISNULL(h.bSuspended,0)=0 AND ISNULL(d.bVoid,0)=0
GROUP BY d.iCode, h.iVoucherType
ORDER BY d.iCode, h.iVoucherType
" | Format-Table -AutoSize | Out-String -Width 120 | Write-Host

Write-Host "=== 3) name-groups of the I/II/III overlap names: members, statuses, iCode-line counts (dept 2040, all VT) ==="
Q "
SELECT a.iMasterId, a.sName, a.ReportStatus,
       (SELECT COUNT(*) FROM dbo.tCore_Data_0 d
        INNER JOIN dbo.tCore_Header_0 h ON h.iHeaderId = d.iHeaderId
        WHERE d.iCode = a.iMasterId AND d.iFaTag = 2040
          AND ISNULL(h.iAuth,1)=1 AND ISNULL(h.bCancelled,0)=0 AND ISNULL(h.bVersion,0)=0
          AND ISNULL(h.bSuspended,0)=0 AND ISNULL(d.bVoid,0)=0) AS ICodeLines
FROM dbo.mCore_Account a
WHERE a.sName IN ('Mr. Ali Abdulmajeed Alshakhs','Mr. Ahmed Isa Marhoon Ali','Mr.ali Alhujairi','Mr. Ahmed Abdulla Jaffar','Mr. Hussain Ali')
ORDER BY a.sName, a.iMasterId
" | Format-Table -AutoSize | Out-String -Width 150 | Write-Host

Write-Host "=== 4) RULE v2 count: status-3 TR accounts with >=1 iCode-linked dept-2040 line (all VT), distinct names ==="
Q "
SELECT COUNT(DISTINCT a.sName) AS NameCount, COUNT(DISTINCT a.iMasterId) AS AcctCount
FROM dbo.mCore_Account a
WHERE a.ReportStatus = 3 AND ISNULL(a.bGroup,0) = 0
  AND a.iMasterId IN (
        SELECT tr.iMasterId FROM dbo.mCore_AccountTreeDetails tr
        WHERE tr.iTreeId = 0 AND tr.iParentId IN (
            SELECT iMasterId FROM dbo.mCore_Account WHERE sName = 'Trade Receivables' AND ISNULL(bGroup,0)=1))
  AND EXISTS (
        SELECT 1 FROM dbo.tCore_Data_0 d
        INNER JOIN dbo.tCore_Header_0 h ON h.iHeaderId = d.iHeaderId
        WHERE d.iCode = a.iMasterId AND d.iFaTag = 2040
          AND ISNULL(h.iAuth,1)=1 AND ISNULL(h.bCancelled,0)=0 AND ISNULL(h.bVersion,0)=0
          AND ISNULL(h.bSuspended,0)=0 AND ISNULL(d.bVoid,0)=0)
" | Format-Table -AutoSize | Out-String -Width 120 | Write-Host

$conn.Close()
