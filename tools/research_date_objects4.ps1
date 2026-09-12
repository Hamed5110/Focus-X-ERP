$cs = "Server=localhost;Database=Focus80E0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 180

Write-Output "========== TranData columns named Date or iDate =========="
$cmd.CommandText = @"
SELECT ORDINAL_POSITION, COLUMN_NAME, DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME='vCore_TranData_0'
  AND (
    COLUMN_NAME = 'iDate' OR COLUMN_NAME = 'Date' OR COLUMN_NAME LIKE 'iDate%'
    OR COLUMN_NAME LIKE '%Header%Date%' OR COLUMN_NAME = 'VoucherDate'
    OR COLUMN_NAME LIKE '%DocDate%'
  )
ORDER BY ORDINAL_POSITION
"@
$r = $cmd.ExecuteReader()
while ($r.Read()) { Write-Output ("{0} | {1} | {2}" -f $r.GetValue(0), $r.GetValue(1), $r.GetValue(2)) }
$r.Close()

Write-Output ""
Write-Output "========== sample TranData date-like from one receipt =========="
$cmd.CommandText = @"
SELECT TOP 1 COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME='vCore_TranData_0' AND COLUMN_NAME IN ('iDate','Date','sVoucherNo','iHeaderId','iVoucherType')
"@
$r = $cmd.ExecuteReader(); while ($r.Read()) { Write-Output $r.GetValue(0) }; $r.Close()

Write-Output ""
Write-Output "========== vtCode_DataFA_0 iDate snippet =========="
$cmd.CommandText = "SELECT OBJECT_DEFINITION(OBJECT_ID('dbo.vtCode_DataFA_0'))"
$d = [string]$cmd.ExecuteScalar()
$lines = $d -split "`n"
$i=0
foreach ($ln in $lines) {
  $i++
  if ($ln -match '(?i)idate|IntToDate|DateToInt') { Write-Output ("L{0}: {1}" -f $i, $ln.Trim()) }
}

Write-Output ""
Write-Output "========== vtCore_LinksData_0 iDate snippet =========="
$cmd.CommandText = "SELECT OBJECT_DEFINITION(OBJECT_ID('dbo.vtCore_LinksData_0'))"
$d = [string]$cmd.ExecuteScalar()
$lines = $d -split "`n"
$i=0
foreach ($ln in $lines) {
  $i++
  if ($ln -match '(?i)idate|IntToDate') { Write-Output ("L{0}: {1}" -f $i, $ln.Trim()) }
}

Write-Output ""
Write-Output "========== search TranData definition for iDate =========="
$cmd.CommandText = "SELECT OBJECT_DEFINITION(OBJECT_ID('dbo.vCore_TranData_0'))"
$d = [string]$cmd.ExecuteScalar()
# find context around iDate
$idx = 0
$hits = 0
while ($hits -lt 8) {
  $p = $d.IndexOf("iDate", $idx, [System.StringComparison]::OrdinalIgnoreCase)
  if ($p -lt 0) { break }
  $s = [Math]::Max(0, $p-80)
  $snip = $d.Substring($s, [Math]::Min(200, $d.Length-$s)) -replace "`r|`n"," "
  Write-Output ("pos={0}: ...{1}..." -f $p, $snip)
  $idx = $p + 5
  $hits++
}

Write-Output ""
Write-Output "========== pCore_CreateInternalViews date bits =========="
$cmd.CommandText = "SELECT OBJECT_DEFINITION(OBJECT_ID('dbo.pCore_CreateInternalViews'))"
$d = [string]$cmd.ExecuteScalar()
Write-Output ("len=" + $d.Length)
$idx = 0
$hits = 0
while ($hits -lt 15) {
  $p = $d.IndexOf("IntToDate", $idx, [System.StringComparison]::OrdinalIgnoreCase)
  if ($p -lt 0) { break }
  $s = [Math]::Max(0, $p-100)
  $snip = $d.Substring($s, [Math]::Min(250, $d.Length-$s)) -replace "`r|`n"," "
  Write-Output ("...{0}..." -f $snip)
  $idx = $p + 9
  $hits++
}

$conn.Close()
