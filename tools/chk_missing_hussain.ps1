$cs = "Server=localhost;Database=Focus80G0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 60
$cmd.CommandText = @"
SELECT h.sVoucherNo, h.iDate, h.iAuth, h.bCancelled, ABS(h.fNet) Net, d.iBookNo
FROM dbo.tCore_Header_0 h
JOIN dbo.tCore_Data_0 d ON d.iHeaderId=h.iHeaderId AND d.iBookNo=17500
WHERE h.sVoucherNo IN (N'CON-Atl-7045', N'ATIC-26-1531', N'ATIC-26-1489', N'ATIC-26-1470')
"@
$r = $cmd.ExecuteReader()
while ($r.Read()) {
    Write-Output ("{0} date={1} auth={2} can={3} net={4} book={5}" -f $r[0],$r[1],$r[2],$r[3],$r[4],$r[5])
}
$r.Close()
$conn.Close()
