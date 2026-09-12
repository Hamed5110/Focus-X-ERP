$cs = "Server=localhost;Database=Focus80E0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 60
function Dump($sql, $title) {
    Write-Output ""
    Write-Output "========== $title =========="
    $cmd.CommandText = $sql
    try {
        $r = $cmd.ExecuteReader()
        $n = $r.FieldCount
        $hdr = @(); for ($i=0; $i -lt $n; $i++) { $hdr += $r.GetName($i) }
        Write-Output ($hdr -join " | ")
        while ($r.Read()) {
            $parts = @()
            for ($i=0; $i -lt $n; $i++) {
                if ($r.IsDBNull($i)) { $parts += "" } else { $parts += [string]$r.GetValue($i) }
            }
            Write-Output ($parts -join " | ")
        }
        $r.Close()
    } catch {
        Write-Output ("ERROR: " + $_.Exception.Message)
        try { $conn.Close(); $conn.Open(); $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 60 } catch {}
    }
}

Dump "SELECT @@DATEFORMAT AS DateFormat, @@LANGUAGE AS Lang, DATEPART(m, GETDATE()) AS M" "session dateformat"

Dump @"
SELECT
  132514052 AS Packed,
  (132514052 & 0xfff0000)/65536 AS Y,
  (132514052 & 0xff00)/256 AS M,
  132514052 & 0xff AS D,
  dbo.IntToDate(132514052) AS IntToDate,
  DATEFROMPARTS((132514052 & 0xfff0000)/65536, (132514052 & 0xff00)/256, 132514052 & 0xff) AS DFP,
  CONVERT(datetime,
    CAST((132514052 & 0xfff0000)/65536 AS varchar(4))
    + RIGHT('0'+CAST((132514052 & 0xff00)/256 AS varchar(2)),2)
    + RIGHT('0'+CAST((132514052 & 0xff) AS varchar(2)),2)
  , 112) AS Style112
"@ "convert 132514052"

Dump @"
SET DATEFORMAT dmy;
SELECT dbo.IntToDate(132514052) AS IntToDate_dmy;
"@ "IntToDate under dmy"

Dump @"
SET DATEFORMAT mdy;
SELECT dbo.IntToDate(132514052) AS IntToDate_mdy;
"@ "IntToDate under mdy"

Dump @"
SET DATEFORMAT dmy;
SELECT CAST(('2023' + '/' + '1' + '/' + '4') AS datetime) AS CastYMD_dmy;
"@ "string Y/M/D under dmy"

Dump @"
SET DATEFORMAT dmy;
SELECT CAST(DATEFROMPARTS(2023,1,4) AS datetime) AS DFP_dmy,
       CASE WHEN CAST(DATEFROMPARTS(2026,9,1) AS datetime) >= CONVERT(datetime,'20260901',112)
             AND CAST(DATEFROMPARTS(2026,9,1) AS datetime) <= CONVERT(datetime,'20260930',112) THEN 1 ELSE 0 END AS InSep
"@ "DATEFROMPARTS under dmy"

Dump @"
SELECT r.iReportId, r.sReportName, c.sColumn, c.sAliasName, c.iType, c.sFormat, c.iMiscOption
FROM dbo.cCore_ReportColumns_0 c
INNER JOIN dbo.cCore_ReportLayouts_0 l ON l.iLayoutId = c.iLayoutId
INNER JOIN dbo.cCore_Reports_0 r ON r.iReportId = l.iReportId
WHERE r.iReportType = 1 AND (c.iType = 4 OR c.sColumn = N'iDate' OR c.sAliasName = N'iDate')
ORDER BY c.iType, r.iReportId
"@ "query Date/iDate columns"

Dump @"
SELECT iType, COUNT(*) Cnt
FROM dbo.cCore_ReportColumns_0
WHERE sColumn = N'iDate' OR sAliasName = N'iDate'
GROUP BY iType
"@ "iDate types"

$conn.Close()
