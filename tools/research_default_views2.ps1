$cs = "Server=localhost;Database=Focus80E0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 180

function Snip($name, $pattern) {
    Write-Output ""
    Write-Output "========== $name =========="
    $cmd.CommandText = "SELECT OBJECT_DEFINITION(OBJECT_ID(@n))"
    $p = $cmd.Parameters.Add("@n", [System.Data.SqlDbType]::NVarChar, 256)
    $p.Value = $name
    $d = [string]$cmd.ExecuteScalar()
    $cmd.Parameters.Clear()
    if ([string]::IsNullOrEmpty($d)) { Write-Output "(null)"; return }
    Write-Output ("len=" + $d.Length)
    $idx = 0; $hits = 0
    while ($hits -lt 12) {
        $p2 = $d.IndexOf($pattern, $idx, [System.StringComparison]::OrdinalIgnoreCase)
        if ($p2 -lt 0) { break }
        $s = [Math]::Max(0, $p2 - 120)
        $snip = $d.Substring($s, [Math]::Min(280, $d.Length - $s)) -replace "`r|`n"," "
        Write-Output ("...{0}..." -f $snip)
        $idx = $p2 + $pattern.Length
        $hits++
    }
}

Snip "dbo.pCore_CreateFormDefaultView" "iDate"
Snip "dbo.pCore_CreateFormDefaultView" "DateToInt"
Snip "dbo.pCore_CreateFormDefaultView" "IntToDate"
Snip "dbo.vtCode_DataFA_0" "iDate"

Write-Output ""
Write-Output "========== procs mentioning report date filter =========="
$cmd.CommandText = @"
SELECT TOP 25 o.name
FROM sys.objects o
WHERE o.type = 'P'
  AND (
    OBJECT_DEFINITION(o.object_id) LIKE '%DateToInt%'
    AND (
      OBJECT_DEFINITION(o.object_id) LIKE '%Report%'
      OR OBJECT_DEFINITION(o.object_id) LIKE '%iDate >=%'
      OR OBJECT_DEFINITION(o.object_id) LIKE '%StartDate%'
    )
  )
ORDER BY o.name
"@
try {
    $r = $cmd.ExecuteReader()
    while ($r.Read()) { Write-Output $r.GetValue(0) }
    $r.Close()
} catch { Write-Output $_.Exception.Message }

Write-Output ""
Write-Output "========== 70028 columns Date/iDate =========="
$cmd.CommandText = @"
SELECT c.iFieldId, c.sColumn, c.sAliasName, c.iType
FROM dbo.cCore_ReportColumns_0 c
INNER JOIN dbo.cCore_ReportLayouts_0 l ON l.iLayoutId = c.iLayoutId
WHERE l.iReportId = 70028 AND (c.sColumn LIKE N'%Date%' OR c.sAliasName LIKE N'%Date%' OR c.sColumn = N'iDate')
ORDER BY c.iFieldId
"@
$r = $cmd.ExecuteReader()
while ($r.Read()) { Write-Output ("{0} | {1} | {2} | type={3}" -f $r.GetValue(0),$r.GetValue(1),$r.GetValue(2),$r.GetValue(3)) }
$r.Close()

Write-Output ""
Write-Output "========== vtCode_DataFA sample iDate =========="
$cmd.CommandText = @"
SELECT TOP 3 iHeaderId, iVoucherType, iDate, iFaTag, mAmount1
FROM dbo.vtCode_DataFA_0
WHERE iVoucherType IN (4608,4609,4610) AND iFaTag IN (2040,2057)
ORDER BY iHeaderId DESC
"@
$r = $cmd.ExecuteReader()
while ($r.Read()) { Write-Output ("{0} | vt={1} | iDate={2} | fa={3} | amt={4}" -f $r.GetValue(0),$r.GetValue(1),$r.GetValue(2),$r.GetValue(3),$r.GetValue(4)) }
$r.Close()

$conn.Close()
