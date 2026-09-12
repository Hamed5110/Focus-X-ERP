$cs = "Server=localhost;Database=Focus8080;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()

function Run($title, $sql) {
    $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 300; $cmd.CommandText = $sql
    $a = New-Object System.Data.SqlClient.SqlDataAdapter $cmd; $dt = New-Object System.Data.DataTable
    [void]$a.Fill($dt)
    Write-Host "`n== $title =="
    $dt | Format-Table -AutoSize | Out-String -Width 300 | Write-Host
}

Run "Name-groups (TR, non-group, tree 0) with MIXED statuses incl. a status-3 member" @"
SELECT sName, COUNT(DISTINCT iMasterId) AS Accts, MIN(ReportStatus) AS MinSt, MAX(ReportStatus) AS MaxSt,
       COUNT(DISTINCT ReportStatus) AS DistinctStatuses
FROM dbo.vaCore_Account v
WHERE v.iTreeId = 0 AND ISNULL(v.bGroup, 0) = 0
  AND v.iMasterId IN (
      SELECT tr.iMasterId FROM dbo.mCore_AccountTreeDetails tr
      WHERE tr.iTreeId = 0 AND tr.iParentId IN (SELECT iMasterId FROM dbo.mCore_Account WHERE sName = 'Trade Receivables' AND ISNULL(bGroup,0)=1)
  )
GROUP BY sName
HAVING COUNT(DISTINCT iMasterId) > 1 AND MAX(ReportStatus) = 3
ORDER BY sName
"@

Run "ALL duplicate-name groups in TR (any statuses)" @"
SELECT sName, COUNT(DISTINCT iMasterId) AS Accts,
       STRING_AGG(CAST(ReportStatus AS varchar), ',') AS Statuses
FROM dbo.vaCore_Account v
WHERE v.iTreeId = 0 AND ISNULL(v.bGroup, 0) = 0
  AND v.iMasterId IN (
      SELECT tr.iMasterId FROM dbo.mCore_AccountTreeDetails tr
      WHERE tr.iTreeId = 0 AND tr.iParentId IN (SELECT iMasterId FROM dbo.mCore_Account WHERE sName = 'Trade Receivables' AND ISNULL(bGroup,0)=1)
  )
GROUP BY sName
HAVING COUNT(DISTINCT iMasterId) > 1
ORDER BY sName
"@

$conn.Close()
