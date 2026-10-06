$cs = "Server=localhost;Database=Focus80G0;User Id=sa;Password=$env:FOCUS_SQL_PASSWORD;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()

$vw = [System.IO.File]::ReadAllText("C:\Users\Hamed Ali Khan\Focus-X-ERP\reports\sql\vRD_PT3AtlasAccounts.sql")
$cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 60; $cmd.CommandText = $vw
[void]$cmd.ExecuteNonQuery()
$cmd = $conn.CreateCommand(); $cmd.CommandText = "GRANT SELECT ON dbo.vRD_PT3AtlasAccounts TO PUBLIC"
[void]$cmd.ExecuteNonQuery()

$q = [System.IO.File]::ReadAllText("C:\Users\Hamed Ali Khan\Focus-X-ERP\reports\sql\Project Tracking III Report Atlas Ledger.sql")
$upd = $conn.CreateCommand()
$upd.CommandText = "UPDATE dbo.cCore_RDQuery_0 SET sSqlQuery=@q WHERE iReportId=70268"
[void]$upd.Parameters.AddWithValue("@q", $q)
[void]$upd.ExecuteNonQuery()

$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT COUNT(*) FROM dbo.vRD_PT3AtlasAccounts"
Write-Output ("view rows=" + $cmd.ExecuteScalar())

$start = 2021*65536+256+1
$end = [int](Get-Date).Year*65536+[int](Get-Date).Month*256+[int](Get-Date).Day
$run = $q.Replace("@CustomerName","0").Replace("@iStartDate","$start").Replace("@iEndDate","$end")
$wrap = $run -replace "WHERE a.iDate >= 0", "WHERE a.iDate >= 0 AND iDate >= $start AND iDate <= $end OR iDate = 0"

$cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 180; $cmd.CommandText = $wrap
$sw = [System.Diagnostics.Stopwatch]::StartNew()
try {
    $rd = $cmd.ExecuteReader()
    $c=0; $ac=$null
    while($rd.Read()){
        $c++
        if([string]$rd["Code"] -eq "AC-847"){
            $ac = "{0}/{1}/{2}" -f $rd["Total Contract Amount"],$rd["Adv. Rct Amount"],$rd["Balance Amount"]
        }
    }
    $rd.Close()
    $sw.Stop()
    Write-Output ("wrap rows={0} AC-847={1} ms={2}" -f $c, $ac, $sw.ElapsedMilliseconds)
} catch {
    $sw.Stop()
    Write-Output ("WRAP FAIL ms={0} {1}" -f $sw.ElapsedMilliseconds, $_.Exception.Message)
}

$one = $q.Replace("@CustomerName","5247").Replace("@iStartDate","$start").Replace("@iEndDate","$end")
$one = $one -replace "WHERE a.iDate >= 0", "WHERE a.iDate >= 0 AND iDate >= $start AND iDate <= $end OR iDate = 0"
$cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 180; $cmd.CommandText = $one
try {
    $rd = $cmd.ExecuteReader(); $c=0; $code=""; $bal=""
    while($rd.Read()){ $c++; $code=[string]$rd["Code"]; $bal=[string]$rd["Balance Amount"] }
    $rd.Close()
    Write-Output ("cust 5247 rows={0} code={1} bal={2}" -f $c, $code, $bal)
} catch {
    Write-Output ("CUST FAIL " + $_.Exception.Message)
}

# confirm no function call / no DECLARE in saved SQL
$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT CASE WHEN sSqlQuery LIKE N'%fnRD_%' THEN 1 ELSE 0 END HasFn, CASE WHEN sSqlQuery LIKE N'%vmCore_Account%' OR sSqlQuery LIKE N'%vRD_PT3%' THEN 1 ELSE 0 END HasView FROM dbo.cCore_RDQuery_0 WHERE iReportId=70268"
$rd = $cmd.ExecuteReader(); $rd.Read()
Write-Output ("saved HasFn={0} HasView={1}" -f $rd[0], $rd[1])
$rd.Close()
$conn.Close()
