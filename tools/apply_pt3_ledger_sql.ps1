$cs = "Server=localhost;Database=Focus80G0;User Id=sa;Password=$env:FOCUS_SQL_PASSWORD;Encrypt=False;TrustServerCertificate=True;"
$sql = [System.IO.File]::ReadAllText("C:\Users\Hamed Ali Khan\Focus-X-ERP\reports\sql\Project Tracking III Report Atlas Ledger.sql")
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$tx = $conn.BeginTransaction()

$cmd = $conn.CreateCommand(); $cmd.Transaction = $tx
$cmd.CommandText = "UPDATE dbo.cCore_RDQuery_0 SET sSqlQuery = @q WHERE iReportId = 70268"
[void]$cmd.Parameters.Add("@q", [System.Data.SqlDbType]::NVarChar, -1)
$cmd.Parameters["@q"].Value = $sql
$n = $cmd.ExecuteNonQuery()
Write-Output "updated query rows=$n len=$($sql.Length)"

$cmd = $conn.CreateCommand(); $cmd.Transaction = $tx
$cmd.CommandText = "SELECT COUNT(*) FROM dbo.cCore_ReportTransactionSet_0 WHERE iReportId = 70268"
Write-Output ("transet=" + $cmd.ExecuteScalar())

$cmd = $conn.CreateCommand(); $cmd.Transaction = $tx
$cmd.CommandText = @"
SELECT c.iColumnId, c.iFieldId, c.sAliasName, c.iType, c.iMiscOption
FROM dbo.cCore_ReportLayouts_0 l
JOIN dbo.cCore_ReportColumns_0 c ON c.iLayoutId = l.iLayoutId
WHERE l.iReportId = 70268
ORDER BY c.iFieldId
"@
$r = $cmd.ExecuteReader()
$cols = @()
while ($r.Read()) {
    $cols += [pscustomobject]@{ Id=$r[0]; Field=$r[1]; Alias=[string]$r[2]; Type=$r[3]; Misc=$r[4] }
    Write-Output ("col id=$($r[0]) field=$($r[1]) alias=$($r[2]) type=$($r[3]) misc=$($r[4])")
}
$r.Close()

$idate = $cols | Where-Object { $_.Alias -eq 'iDate' -or $_.Field -eq 21 }
if ($idate) {
    $cmd = $conn.CreateCommand(); $cmd.Transaction = $tx
    $cmd.CommandText = "DELETE FROM dbo.cCore_ReportColumns_0 WHERE iColumnId = @id"
    [void]$cmd.Parameters.AddWithValue("@id", $idate.Id)
    $d = $cmd.ExecuteNonQuery()
    Write-Output "deleted iDate layout column id=$($idate.Id) rows=$d"
} else {
    Write-Output "no iDate layout column"
}

$tx.Commit()

$cmd = $conn.CreateCommand()
$cmd.CommandText = @"
SELECT r.iReportId, r.iReportType, r.iSourceType, LEN(q.sSqlQuery) SqlLen,
       CHARINDEX('AS iDate', q.sSqlQuery) HasIDate,
       CHARINDEX('WHERE h.iDate > 0', q.sSqlQuery) HasHook,
       (SELECT COUNT(*) FROM dbo.cCore_ReportTransactionSet_0 t WHERE t.iReportId = 70268) TranCnt,
       (SELECT COUNT(*) FROM dbo.cCore_ReportColumns_0 c JOIN dbo.cCore_ReportLayouts_0 l ON l.iLayoutId=c.iLayoutId WHERE l.iReportId=70268) ColCnt
FROM dbo.cCore_Reports_0 r
JOIN dbo.cCore_RDQuery_0 q ON q.iReportId = r.iReportId
WHERE r.iReportId = 70268
"@
$r = $cmd.ExecuteReader()
while ($r.Read()) {
    Write-Output ("saved type=$($r[1]) src=$($r[2]) sqlLen=$($r[3]) asIDate=$($r[4]) hook=$($r[5]) tran=$($r[6]) cols=$($r[7])")
}
$r.Close()
$conn.Close()
