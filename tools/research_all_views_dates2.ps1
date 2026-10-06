$cs = "Server=localhost;Database=Focus80G0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()

function Dump($sql, $title) {
    Write-Output ""
    Write-Output "========== $title =========="
    $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 300; $cmd.CommandText = $sql
    try {
        $r = $cmd.ExecuteReader()
        $n = $r.FieldCount
        $hdr = @(); for ($i=0; $i -lt $n; $i++) { $hdr += $r.GetName($i) }
        Write-Output ($hdr -join " | ")
        $c = 0
        while ($r.Read()) {
            $c++
            $parts = @()
            for ($i=0; $i -lt $n; $i++) {
                if ($r.IsDBNull($i)) { $parts += "" } else {
                    $v = [string]$r.GetValue($i)
                    $v = $v -replace "`r|`n"," "
                    if ($v.Length -gt 700) { $v = $v.Substring(0,700) + "..." }
                    $parts += $v
                }
            }
            Write-Output ($parts -join " | ")
            if ($c -ge 200) { Write-Output "(truncated)"; break }
        }
        $r.Close()
        if ($c -eq 0) { Write-Output "(no rows)" }
    } catch {
        Write-Output ("ERROR: " + $_.Exception.Message)
        try { $r.Close() } catch {}
    }
}

Dump @"
SELECT r.iReportId, r.sReportName, r.iReportType, r.iSourceType
FROM dbo.cCore_Reports_0 r
WHERE r.sReportName LIKE '%Statement of%'
   OR r.sReportName LIKE '%Statmnet%'
   OR r.iReportType = 1
ORDER BY r.iReportId
"@ "query reports and statement of"

Dump @"
SELECT
  CONVERT(varchar(20), CONVERT(date, CAST(20260930 AS varchar(8)), 112), 23) AS Convert112,
  CONVERT(varchar(20), CAST(CAST(20260930 AS char(8)) AS date), 23) AS CastChar8,
  CONVERT(varchar(20), TRY_CONVERT(date, CAST(132778241 AS varchar(8)), 112), 23) AS PackedDirect112,
  CONVERT(varchar(20), CONVERT(date, CAST((132778241 / 65536)*10000 + ((132778241 / 256) % 256)*100 + (132778241 % 256) AS char(8)), 112), 23) AS UnpackThen112,
  CONVERT(varchar(20), dbo.IntToDate(132778241), 23) AS IntToDateSep1,
  CONVERT(varchar(20), dbo.IntToDate(0), 23) AS IntToDateZero,
  SQL_VARIANT_PROPERTY(CONVERT(date, CAST(20260930 AS varchar(8)), 112), 'BaseType') AS ConvertType
"@ "112 vs packed vs IntToDate"

# Scan every view in PowerShell (avoid SQL N-literal issues)
$cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 300
$cmd.CommandText = "SELECT SCHEMA_NAME(schema_id) AS Sch, name, OBJECT_DEFINITION(object_id) AS Def FROM sys.views"
$r = $cmd.ExecuteReader()
$hitConvertDate = 0
$hit112 = 0
$hitIntToDate = 0
$hitCastDate = 0
$hitDateFromParts = 0
$hitIDate = 0
$lines = New-Object System.Collections.Generic.List[string]
while ($r.Read()) {
    $name = [string]$r["name"]
    $sch = [string]$r["Sch"]
    $d = if ($r.IsDBNull($r.GetOrdinal("Def"))) { "" } else { [string]$r["Def"] }
    if ($d -match '(?i)iDate') { $hitIDate++ }
    $flags = @()
    if ($d -match '(?i)CONVERT\s*\(\s*date') { $flags += "CONVERT(date"; $hitConvertDate++ }
    if ($d -match '(?i),\s*112\s*\)') { $flags += "style112"; $hit112++ }
    if ($d -match '(?i)IntToDate') { $flags += "IntToDate"; $hitIntToDate++ }
    if ($d -match '(?i)CAST\s*\(.+AS\s+date\b') { $flags += "CAST AS date"; $hitCastDate++ }
    if ($d -match '(?i)DATEFROMPARTS') { $flags += "DATEFROMPARTS"; $hitDateFromParts++ }
    if ($flags.Count -gt 0) {
        $i = 0
        foreach ($ln in ($d -split "`n")) {
            $i++
            if ($ln -match '(?i)CONVERT\s*\(\s*date|,\s*112\s*\)|IntToDate|CAST\s*\(.+AS\s+date\b|DATEFROMPARTS|CAST\s*\(.+AS\s+datetime') {
                $t = $ln.Trim()
                if ($t.Length -gt 220) { $t = $t.Substring(0,220) + "..." }
                $lines.Add(("{0}.{1} L{2} [{3}] {4}" -f $sch, $name, $i, ($flags -join ","), $t))
            }
        }
    }
}
$r.Close()

Write-Output ""
Write-Output "========== view date-pattern counts =========="
Write-Output ("views mentioning iDate=$hitIDate CONVERT(date)=$hitConvertDate style112=$hit112 IntToDate=$hitIntToDate CAST AS date=$hitCastDate DATEFROMPARTS=$hitDateFromParts matchingLines=$($lines.Count)")
Write-Output ""
Write-Output "========== matching view lines =========="
$shown = 0
foreach ($ln in $lines) {
    Write-Output $ln
    $shown++
    if ($shown -ge 180) { Write-Output "(truncated lines)"; break }
}

Dump @"
SELECT TOP 30 c.TABLE_SCHEMA, c.TABLE_NAME, c.COLUMN_NAME, c.DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS c
JOIN INFORMATION_SCHEMA.VIEWS v ON v.TABLE_SCHEMA = c.TABLE_SCHEMA AND v.TABLE_NAME = c.TABLE_NAME
WHERE c.DATA_TYPE IN ('date','datetime','datetime2','smalldatetime')
  AND (c.TABLE_NAME LIKE '%FA%' OR c.TABLE_NAME LIKE '%Code%' OR c.TABLE_NAME LIKE '%Stmt%' OR c.TABLE_NAME LIKE '%Statement%' OR c.TABLE_NAME LIKE '%Ledger%' OR c.TABLE_NAME LIKE '%Customer%')
ORDER BY c.TABLE_NAME, c.ORDINAL_POSITION
"@ "FA/customer views with SQL date columns"

Dump @"
SELECT TOP 20 COLUMN_NAME, DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'vtCode_DataFA_0'
  AND (COLUMN_NAME LIKE '%Date%' OR COLUMN_NAME = 'iDate')
ORDER BY ORDINAL_POSITION
"@ "vtCode_DataFA_0 date columns"

$conn.Close()
