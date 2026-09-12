$cs = "Server=localhost;Database=Focus80E0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
function Show($id, $title) {
    $cmd = $conn.CreateCommand(); $cmd.CommandText = "SELECT sSqlQuery FROM dbo.cCore_RDQuery_0 WHERE iReportId = $id"
    $sql = [string]$cmd.ExecuteScalar()
    Write-Output ""
    Write-Output "========== $title ($id) len=$($sql.Length) =========="
    if ([string]::IsNullOrEmpty($sql)) { Write-Output "(no sql)"; return }
    $head = $sql.Substring(0, [Math]::Min(400, $sql.Length))
    Write-Output $head
    Write-Output "---"
    Write-Output ("2040 count=" + ([regex]::Matches($sql, 'iFaTag = 2040')).Count)
    Write-Output ("2057 count=" + ([regex]::Matches($sql, 'iFaTag = 2057')).Count)
    Write-Output ("IN 2040,2057=" + ([regex]::Matches($sql, 'iFaTag IN \(2040, 2057\)')).Count)
    Write-Output ("Customer Name=" + $sql.Contains("[Customer Name]"))
    Write-Output ("@iStartDate=" + $sql.Contains("@iStartDate"))
}

Show 70266 "combined"
Show 70267 "atlas"
$conn.Close()
