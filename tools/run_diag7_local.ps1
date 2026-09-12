$cs = "Server=localhost;Database=Focus8080;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$q = @"
SELECT CAST(a.sCode + ' | ' + a.sName AS varchar(200)) AS [Report Status],CAST(a.iMasterId AS decimal(18,2)) AS [Total Contract Amount],CAST(a.ReportStatus AS decimal(18,2)) AS [Adv. Rct Amount],CAST(ISNULL(act.Cnt,0) AS decimal(18,2)) AS [Balance Amount],CAST(ISNULL(sib.SibCount,1) AS decimal(18,2)) AS [Plan Value],CAST(ISNULL(sib.MinStatus,-1) AS decimal(18,2)) AS [No. of Accounts] FROM dbo.vaCore_Account a OUTER APPLY (SELECT COUNT(*) AS Cnt FROM dbo.tCore_Header_0 h INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId=h.iHeaderId WHERE ((d.iCode=a.iMasterId) OR (h.iVoucherType IN (5634,5635,6145) AND d.iBookNo=a.iMasterId)) AND d.iFaTag=2040 AND ISNULL(h.iAuth,1)=1 AND ISNULL(h.bCancelled,0)=0 AND ISNULL(h.bVersion,0)=0 AND ISNULL(h.bSuspended,0)=0 AND ISNULL(d.bVoid,0)=0 AND h.iDate <= (YEAR(GETDATE())*65536)+(MONTH(GETDATE())*256)+DAY(GETDATE())) act OUTER APPLY (SELECT COUNT(DISTINCT a2.iMasterId) AS SibCount, MIN(a2.ReportStatus) AS MinStatus FROM dbo.vaCore_Account a2 WHERE a2.iTreeId=0 AND a2.sName=a.sName AND ISNULL(a2.bGroup,0)=0) sib WHERE a.iTreeId=0 AND ISNULL(a.bGroup,0)=0 AND (a.sCode IN ('AC-3297','AC-3758','AC-4115','AC-6834','AC-8070','AC-6826','AC-4451') OR a.sName IN (SELECT sName FROM dbo.vaCore_Account WHERE iTreeId=0 AND sCode IN ('AC-3297','AC-3758','AC-4115','AC-6834','AC-8070','AC-6826','AC-4451'))) ORDER BY a.sName, a.iMasterId
"@
$cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 120; $cmd.CommandText = $q
$a = New-Object System.Data.SqlClient.SqlDataAdapter $cmd
$dt = New-Object System.Data.DataTable
try {
    [void]$a.Fill($dt)
    Write-Host ("rows: " + $dt.Rows.Count)
    $dt.Rows | ForEach-Object { Write-Host ($_[0].ToString() + " | id=" + $_[1] + " status=" + $_[2] + " act=" + $_[3] + " sibs=" + $_[4] + " minSib=" + $_[5]) }
} catch {
    Write-Host ("SQL ERROR: " + $_.Exception.Message)
}
$conn.Close()
