$cs = "Server=localhost;Database=Focus80E0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 180
function Get-Def($name) {
    $cmd.CommandText = "SELECT OBJECT_DEFINITION(OBJECT_ID(@n))"
    $p = $cmd.Parameters.Add("@n", [System.Data.SqlDbType]::NVarChar, 256)
    $p.Value = $name
    $d = $cmd.ExecuteScalar()
    $cmd.Parameters.Clear()
    return [string]$d
}
function Show-Snippet($name) {
    Write-Output ""
    Write-Output "========== $name =========="
    $d = Get-Def $name
    if ([string]::IsNullOrEmpty($d) -or $d -eq "") { Write-Output "(null def)"; return }
    Write-Output ("len=" + $d.Length)
    $lines = $d -split "`n"
    $i = 0
    foreach ($ln in $lines) {
        $i++
        if ($ln -match '(?i)idate|IntToDate|DateToInt|datetime|65536|0xff') {
            Write-Output ("L{0}: {1}" -f $i, ($ln.Trim()))
        }
    }
}

Show-Snippet "dbo.vCore_TranData_0"
Show-Snippet "dbo.vCore_HeaderData_0"
Show-Snippet "dbo.vCore_TransAuth_0"

Write-Output ""
Write-Output "========== TranData columns matching date =========="
$cmd.CommandText = @"
SELECT COLUMN_NAME, DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME='vCore_TranData_0'
  AND (COLUMN_NAME LIKE '%Date%' OR COLUMN_NAME LIKE '%iDate%' OR DATA_TYPE LIKE '%date%')
ORDER BY ORDINAL_POSITION
"@
$r = $cmd.ExecuteReader()
while ($r.Read()) { Write-Output ("{0} | {1}" -f $r.GetValue(0), $r.GetValue(1)) }
$r.Close()

Write-Output ""
Write-Output "========== HeaderData columns matching date =========="
$cmd.CommandText = @"
SELECT COLUMN_NAME, DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME='vCore_HeaderData_0'
  AND (COLUMN_NAME LIKE '%Date%' OR COLUMN_NAME LIKE '%iDate%' OR DATA_TYPE LIKE '%date%')
ORDER BY ORDINAL_POSITION
"@
$r = $cmd.ExecuteReader()
while ($r.Read()) { Write-Output ("{0} | {1}" -f $r.GetValue(0), $r.GetValue(1)) }
$r.Close()

Write-Output ""
Write-Output "========== views selecting tCore_Header_0.iDate =========="
$cmd.CommandText = @"
SELECT TOP 30 o.name
FROM sys.views o
WHERE OBJECT_DEFINITION(o.object_id) LIKE '%tCore_Header_0%'
  AND OBJECT_DEFINITION(o.object_id) LIKE '%iDate%'
ORDER BY o.name
"@
$r = $cmd.ExecuteReader()
while ($r.Read()) { Write-Output $r.GetValue(0) }
$r.Close()

$conn.Close()
