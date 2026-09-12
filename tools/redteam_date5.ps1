$cs = "Server=localhost;Database=Focus80E0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$base = [System.IO.File]::ReadAllText("C:\Users\Hamed Ali Khan\Focus-X-ERP\reports\sql\Monthly Sales Commission.sql")
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 180

Write-Output "========== DateToInt previous month bounds =========="
$cmd.CommandText = @"
SELECT
  CONVERT(date, GETDATE()) AS Today,
  CONVERT(date, DATEADD(month, DATEDIFF(month, 0, GETDATE()) - 1, 0)) AS PrevStart,
  CONVERT(date, DATEADD(day, -1, DATEADD(month, DATEDIFF(month, 0, GETDATE()), 0))) AS PrevEnd,
  dbo.DateToInt(DATEADD(month, DATEDIFF(month, 0, GETDATE()) - 1, 0)) AS PackedStart,
  dbo.DateToInt(DATEADD(day, -1, DATEADD(month, DATEDIFF(month, 0, GETDATE()), 0))) AS PackedEnd,
  (2026*65536 + 8*256 + 1) AS ExpectAug1,
  (2026*65536 + 8*256 + 31) AS ExpectAug31
"@
$r = $cmd.ExecuteReader()
$n = $r.FieldCount
$hdr = @(); for ($i=0; $i -lt $n; $i++) { $hdr += $r.GetName($i) }
Write-Output ($hdr -join " | ")
while ($r.Read()) {
  $parts = @(); for ($i=0; $i -lt $n; $i++) { $parts += [string]$r.GetValue($i) }
  Write-Output ($parts -join " | ")
}
$r.Close()

Write-Output ""
Write-Output "========== report months after DateToInt previous-month filter =========="
$cmd.CommandText = @"
SELECT Department, COUNT(*) Rows, COUNT(DISTINCT [Month Year]) Months,
       MIN([Month Year]) MinM, MAX([Month Year]) MaxM,
       CAST(SUM([Collection Amount]) AS decimal(18,2)) Coll,
       CAST(SUM([Eligible Collection]) AS decimal(18,2)) Elig
FROM (
$($base.Trim())
) r
GROUP BY Department
ORDER BY Department
"@
try {
    $r = $cmd.ExecuteReader()
    $n = $r.FieldCount
    $hdr = @(); for ($i=0; $i -lt $n; $i++) { $hdr += $r.GetName($i) }
    Write-Output ($hdr -join " | ")
    while ($r.Read()) {
        $parts = @(); for ($i=0; $i -lt $n; $i++) { $parts += [string]$r.GetValue($i) }
        Write-Output ($parts -join " | ")
    }
    $r.Close()
} catch {
    Write-Output ("ERROR: " + $_.Exception.Message)
}

$conn.Close()
Write-Output ""
Write-Output "========== RED TEAM =========="
Write-Output "T1 Date option drives custom SQL: FALSIFIED (70266 iSourceType=1 TranSets=0; all working date reports have TS)"
Write-Output "T2 packed vs datetime inject is why picker fails: INCONCLUSIVE / likely irrelevant because Focus never injects without a Transaction Set"
Write-Output "T3 converting iDate to text/datetime fixes picker: FALSIFIED (Apply Customization; Focus views keep iDate packed)"
Write-Output "T4 native Query/Cube date option works via Transaction Set: CONFIRMED (70028,70064,70220)"
Write-Output "T5 custom SQL in this tenant uses GETDATE/DateToInt in the query: CONFIRMED (70259)"
