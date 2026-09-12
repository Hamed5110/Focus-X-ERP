$cs = "Server=localhost;Database=Focus80E0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
function Dump($sql, $title) {
    Write-Output ""
    Write-Output "========== $title =========="
    $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 90; $cmd.CommandText = $sql
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
                    if ($v.Length -gt 220) { $v = $v.Substring(0,220) + "..." }
                    $parts += $v
                }
            }
            Write-Output ($parts -join " | ")
        }
        $r.Close()
        if ($c -eq 0) { Write-Output "(no rows)" }
    } catch {
        Write-Output ("ERROR: " + $_.Exception.Message)
    }
}

Dump @"
SELECT iReportId, sReportName, iReportType, iSourceType, iModule
FROM dbo.cCore_Reports_0
WHERE sReportName LIKE N'%Commission%'
ORDER BY iReportId
"@ "commission reports"

Dump @"
SELECT r.iReportId, r.sReportName, r.iReportType, r.iSourceType,
  CASE WHEN q.sSqlQuery LIKE N'%@iStartDate%' THEN 1 ELSE 0 END AS HasStart,
  CASE WHEN q.sSqlQuery LIKE N'%Customer Name%' OR q.sSqlQuery LIKE N'%CustomerName%' THEN 1 ELSE 0 END AS HasCust,
  CASE WHEN q.sSqlQuery LIKE N'%iFaTag = 2040%' THEN 1 ELSE 0 END AS AtlasOnly,
  CASE WHEN q.sSqlQuery LIKE N'%iFaTag = 2057%' THEN 1 ELSE 0 END AS AknanOnly,
  LEN(q.sSqlQuery) AS SqlLen
FROM dbo.cCore_Reports_0 r
LEFT JOIN dbo.cCore_RDQuery_0 q ON q.iReportId = r.iReportId
WHERE r.sReportName LIKE N'%Commission%'
ORDER BY r.iReportId
"@ "sql flags"

Dump @"
SELECT p.iReportId, r.sReportName, p.sFieldName, p.sFieldVariable, p.iControlType
FROM dbo.cCore_ReportParameter_0 p
JOIN dbo.cCore_Reports_0 r ON r.iReportId = p.iReportId
WHERE r.sReportName LIKE N'%Commission%'
"@ "parameters"

Dump @"
SELECT t.iReportId, r.sReportName, t.iVoucherType, t.iTranSetId, t.iDocumentOption
FROM dbo.cCore_ReportTransactionSet_0 t
JOIN dbo.cCore_Reports_0 r ON r.iReportId = t.iReportId
WHERE r.sReportName LIKE N'%Commission%'
"@ "transaction sets"

$conn.Close()
