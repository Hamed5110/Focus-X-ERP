$cs = "Server=localhost;Database=Focus80E0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 90
function Dump($sql, $title) {
    Write-Output ""
    Write-Output "========== $title =========="
    $cmd.CommandText = $sql
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
                    if ($v.Length -gt 2000) { $v = $v.Substring(0,2000) + "..." }
                    $parts += $v
                }
            }
            Write-Output ($parts -join " | ")
            if ($c -ge 40) { break }
        }
        $r.Close()
    } catch {
        Write-Output ("ERROR: " + $_.Exception.Message)
    }
}

Dump "SELECT OBJECT_DEFINITION(OBJECT_ID('dbo.IntToDate')) AS Def" "IntToDate"
Dump "SELECT OBJECT_DEFINITION(OBJECT_ID('dbo.DateToInt')) AS Def" "DateToInt"
Dump "SELECT OBJECT_DEFINITION(OBJECT_ID('dbo.GetDatePart')) AS Def" "GetDatePart"
Dump "SELECT OBJECT_DEFINITION(OBJECT_ID('dbo.fCore_IntToDateTime')) AS Def" "fCore_IntToDateTime"

Dump @"
SELECT
  132514052 AS Packed,
  dbo.IntToDate(132514052) AS IntToDate,
  dbo.fCore_IntToDateTime(132514052) AS fCore,
  DATEFROMPARTS((132514052 & 0xfff0000)/65536, (132514052 & 0xff00)/256, 132514052 & 0xff) AS DateFromParts,
  (132514052 & 0xfff0000)/65536 AS Y,
  (132514052 & 0xff00)/256 AS M,
  132514052 & 0xff AS D,
  dbo.GetDateName(N'm', 132514052) AS MonName,
  dbo.GetDatePart(N'y', 132514052) AS GP_Y,
  dbo.GetDatePart(N'm', 132514052) AS GP_M,
  dbo.GetDatePart(N'd', 132514052) AS GP_D
"@ "decode screenshot 132514052"

Dump @"
SELECT iType, COUNT(*) Cnt
FROM dbo.cCore_ReportColumns_0
WHERE sAliasName = N'iDate' OR sColumn = N'iDate'
GROUP BY iType
"@ "iDate layout types"

Dump @"
SELECT TOP 20 r.iReportId, r.sReportName, c.sColumn, c.sAliasName, c.iType, c.sFormat, c.iMiscOption
FROM dbo.cCore_ReportColumns_0 c
INNER JOIN dbo.cCore_ReportLayouts_0 l ON l.iLayoutId = c.iLayoutId
INNER JOIN dbo.cCore_Reports_0 r ON r.iReportId = l.iReportId
WHERE (c.sAliasName = N'iDate' OR c.sColumn = N'iDate' OR c.iType = 4)
  AND r.iReportType = 1
ORDER BY c.iType, r.iReportId
"@ "query reports Date or iDate columns"

Dump @"
SELECT q.sSqlQuery
FROM dbo.cCore_RDQuery_0 q
WHERE q.iReportId = 70256
"@ "PO 70256 SQL"

$conn.Close()
