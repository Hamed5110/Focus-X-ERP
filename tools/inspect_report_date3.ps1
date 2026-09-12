$cs = "Server=localhost;Database=Focus80E0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
function Dump($sql, $title) {
    Write-Output ""
    Write-Output "========== $title =========="
    $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 60; $cmd.CommandText = $sql
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
    }
}

Dump @"
SELECT
  CAST(132777985 AS decimal(18,0)) AS Packed,
  CASE WHEN CAST(132777985 AS decimal(18,0)) >= CAST('2026-08-01' AS datetime) THEN 1 ELSE 0 END AS PackedGteDt,
  CASE WHEN CAST(132777985 AS decimal(18,0)) <= CAST('2026-08-31' AS datetime) THEN 1 ELSE 0 END AS PackedLteDt,
  CASE WHEN CAST(132448513 AS decimal(18,0)) >= CAST('2026-08-01' AS datetime) THEN 1 ELSE 0 END AS Jan2021Gte,
  CASE WHEN dbo.IntToDate(132777985) >= CAST('2026-08-01' AS datetime)
        AND dbo.IntToDate(132777985) <= CAST('2026-08-31' AS datetime) THEN 1 ELSE 0 END AS IntToDateInAug,
  CASE WHEN dbo.IntToDate(132448513) >= CAST('2026-08-01' AS datetime)
        AND dbo.IntToDate(132448513) <= CAST('2026-08-31' AS datetime) THEN 1 ELSE 0 END AS IntToDateJanInAug,
  dbo.DateToInt(CAST('2026-08-01' AS datetime)) AS DateToIntAug1,
  dbo.DateToInt(CAST('2026-08-31' AS datetime)) AS DateToIntAug31
"@ "packed vs datetime compare"

Dump @"
SELECT r.iReportId, r.sReportName, r.iReportType, LEFT(q.sSqlQuery, 180) AS SqlStart
FROM dbo.cCore_Reports_0 r
INNER JOIN dbo.cCore_RDQuery_0 q ON q.iReportId = r.iReportId
WHERE r.iReportType = 1
ORDER BY r.iReportId
"@ "query reports SQL start"

Dump @"
SELECT r.iReportId, r.sReportName, r.iReportType, q.sSqlQuery
FROM dbo.cCore_Reports_0 r
LEFT JOIN dbo.cCore_RDQuery_0 q ON q.iReportId = r.iReportId
WHERE r.sReportName LIKE N'%Monthly Commission%'
"@ "monthly commission saved sql"

$conn.Close()
