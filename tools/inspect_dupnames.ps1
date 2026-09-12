$cs = "Server=localhost;Database=Focus8080;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()

function Run($title, $sql) {
    $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 180; $cmd.CommandText = $sql
    $a = New-Object System.Data.SqlClient.SqlDataAdapter $cmd; $dt = New-Object System.Data.DataTable
    [void]$a.Fill($dt)
    Write-Host "`n== $title =="
    $dt | Format-Table -AutoSize | Out-String -Width 300 | Write-Host
}

Run "ALL accounts named like the 2 excluded ones (all trees, groups included)" @"
SELECT iMasterId, sName, sCode, ReportStatus, bGroup, iParentId, iTreeId
FROM dbo.vaCore_Account
WHERE sName IN (N'Mr. Ahmed Abdulla Jaffar', N'Mr. Hussain Ali')
ORDER BY sName, iMasterId, iTreeId
"@

Run "Duplicate non-group account names among status 1/2/3 TR accounts (tree 0)" @"
SELECT sName, COUNT(DISTINCT iMasterId) AS Accts
FROM dbo.vaCore_Account v
WHERE v.iTreeId = 0 AND ISNULL(v.bGroup, 0) = 0 AND v.ReportStatus IN (1, 2, 3)
  AND v.iMasterId IN (
      SELECT tr.iMasterId FROM dbo.mCore_AccountTreeDetails tr
      WHERE tr.iTreeId = 0 AND tr.iParentId IN (SELECT iMasterId FROM dbo.mCore_Account WHERE sName = 'Trade Receivables' AND ISNULL(bGroup,0)=1)
  )
GROUP BY sName
HAVING COUNT(DISTINCT iMasterId) > 1
ORDER BY sName
"@

$conn.Close()
