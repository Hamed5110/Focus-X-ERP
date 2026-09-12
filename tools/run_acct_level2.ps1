$sql = [System.IO.File]::ReadAllText("C:\Users\Hamed Ali Khan\Focus-X-ERP\reports\sql\III Summary Report.sql")

# Turn the summary query into an account-level query: replace outer aggregation with SELECT *
$outer = @"
SELECT
    (SELECT sName FROM dbo.mCore_reportstatus WHERE iMasterId = x.ReportStatus) AS [Report Status],
    CAST(ISNULL(SUM(x.[Total Contract Amount]), 0) AS decimal(18, 2)) AS [Total Contract Amount],
    CAST(ISNULL(SUM(x.[Adv. Rct Amount]), 0) AS decimal(18, 2)) AS [Adv. Rct Amount],
    CAST(ISNULL(SUM(x.[Balance Amount]), 0) AS decimal(18, 2)) AS [Balance Amount],
    CAST(ISNULL(SUM(x.[Plan Value]), 0) AS decimal(18, 2)) AS [Plan Value],
    COUNT(*) AS [No. of Accounts]
FROM (
"@
if (-not $sql.Contains($outer)) { Write-Host "OUTER BLOCK NOT FOUND"; exit 1 }
$sql = $sql.Replace($outer, "SELECT x.ReportStatus, x.Name, x.[Total Contract Amount], x.[Adv. Rct Amount], x.[Balance Amount], x.[Plan Value] FROM (")
$sql = $sql.Replace("GROUP BY x.ReportStatus", "")
$sql = $sql.Replace("ORDER BY x.ReportStatus", "")
$sql = $sql + "`nORDER BY x.ReportStatus, x.Name"

$cs = "Server=localhost;Database=Focus8080;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 280
$cmd.CommandText = $sql
$a = New-Object System.Data.SqlClient.SqlDataAdapter $cmd
$dt = New-Object System.Data.DataTable
[void]$a.Fill($dt)
$conn.Close()
Write-Host ("rows: " + $dt.Rows.Count)
$dt | Export-Csv -Path "C:\Users\Hamed Ali Khan\Focus-X-ERP\tools\acct_level_all.csv" -NoTypeInformation -Encoding UTF8
$dt | Group-Object ReportStatus | ForEach-Object { Write-Host ("status " + $_.Name + ": " + $_.Count) }
