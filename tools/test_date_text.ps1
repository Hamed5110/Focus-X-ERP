$cs = "Server=localhost;Database=Focus80E0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 60
$cmd.CommandText = @"
SELECT
  132514052 AS Packed,
  (132514052 & 0xfff0000)/65536 AS Y,
  (132514052 & 0xff00)/256 AS M,
  132514052 & 0xff AS D,
  RIGHT(N'00' + CAST((132514052 & 0xff) AS varchar(2)), 2)
    + N'/'
    + RIGHT(N'00' + CAST((132514052 & 0xff00)/256 AS varchar(2)), 2)
    + N'/'
    + CAST((132514052 & 0xfff0000)/65536 AS varchar(4)) AS DateText,
  CONVERT(datetime,
    CAST((132514052 & 0xfff0000)/65536 AS varchar(4))
    + RIGHT(N'0' + CAST((132514052 & 0xff00)/256 AS varchar(2)), 2)
    + RIGHT(N'0' + CAST((132514052 & 0xff) AS varchar(2)), 2)
  , 112) AS Date112
"@
$r = $cmd.ExecuteReader()
while ($r.Read()) {
  Write-Output ("Packed={0} Y={1} M={2} D={3} DateText={4} Date112={5}" -f $r.GetValue(0),$r.GetValue(1),$r.GetValue(2),$r.GetValue(3),$r.GetValue(4),$r.GetValue(5))
}
$r.Close()

$base = [System.IO.File]::ReadAllText("C:\Users\Hamed Ali Khan\Focus-X-ERP\reports\sql\Monthly Sales Commission.sql")
$cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 180
$cmd.CommandText = "SELECT TOP 5 [Month Year], iDate FROM (`n$($base.Trim())`n) r ORDER BY 1"
try {
    $r = $cmd.ExecuteReader()
    Write-Output ("iDate SQL type=" + $r.GetDataTypeName(1))
    while ($r.Read()) {
        Write-Output ("{0} | {1}" -f $r.GetValue(0), $r.GetValue(1))
    }
    $r.Close()
} catch {
    Write-Output $_.Exception.Message
}
$conn.Close()
