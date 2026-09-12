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

Run "Dept-2040 activity (ALL voucher types) for every member of the 8 mixed name-groups" @"
WITH Names AS (
    SELECT v.iMasterId, v.sName, v.sCode, v.ReportStatus
    FROM dbo.vaCore_Account v
    WHERE v.iTreeId = 0 AND ISNULL(v.bGroup, 0) = 0
      AND v.sName IN (N'Mr. Ahmed Abdulla Jaffar', N'Mr. Hussain Ali', N'Mr. Ebrahim Ahmed', N'Mr. Ebrahim Habib',
                      N'Mr. Hassan Abdul Amir', N'Mr. Hassan Ali', N'Mr. Hussain Alsaleem', N'Mr. Khalil Ebrahim')
)
SELECT n.sName, n.iMasterId, n.sCode, n.ReportStatus,
       ISNULL(act.CodeLines, 0) AS CodeLines, ISNULL(bk.BookLines, 0) AS BookLines,
       ISNULL(act.CodeLines, 0) + ISNULL(bk.BookLines, 0) AS TotalDept2040Lines
FROM Names n
LEFT JOIN (
    SELECT d.iCode AS iMasterId, COUNT(*) AS CodeLines
    FROM dbo.tCore_Header_0 h
    INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId = h.iHeaderId
    WHERE d.iFaTag = 2040 AND d.iCode > 0
      AND ISNULL(h.bCancelled, 0) = 0 AND ISNULL(h.bVersion, 0) = 0 AND ISNULL(h.bSuspended, 0) = 0 AND ISNULL(d.bVoid, 0) = 0
    GROUP BY d.iCode
) act ON act.iMasterId = n.iMasterId
OUTER APPLY (
    SELECT COUNT(*) AS BookLines
    FROM dbo.tCore_Header_0 h
    INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId = h.iHeaderId
    WHERE d.iFaTag = 2040 AND d.iBookNo = n.iMasterId AND d.iBookNo <> d.iCode
      AND ISNULL(h.bCancelled, 0) = 0 AND ISNULL(h.bVersion, 0) = 0 AND ISNULL(h.bSuspended, 0) = 0 AND ISNULL(d.bVoid, 0) = 0
) bk
ORDER BY n.sName, n.ReportStatus DESC, n.iMasterId
"@

$conn.Close()
