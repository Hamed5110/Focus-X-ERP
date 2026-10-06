$cs = "Server=localhost;Database=Focus80E0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$sql = @"
DECLARE @iStartDate INT = dbo.DateToInt(CONVERT(datetime,'20260801',112));
DECLARE @iEndDate INT = dbo.DateToInt(CONVERT(datetime,'20260831',112));
"@ + (Get-Content -Raw "C:\Users\Hamed Ali Khan\Focus-X-ERP\reports\sql\Monthly Sales Commission - Atlas Aluminum Detail.sql")
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 180; $cmd.CommandText = $sql
try {
    $r = $cmd.ExecuteReader()
    $cols = @(); for ($i=0; $i -lt $r.FieldCount; $i++) { $cols += $r.GetName($i) }
    $n=0; while ($r.Read()) { $n++ }
    $r.Close()
    Write-Output ("OK rows=$n cols=" + ($cols -join ", "))
} catch {
    Write-Output ("FAIL: " + $_.Exception.Message)
}
$conn.Close()
