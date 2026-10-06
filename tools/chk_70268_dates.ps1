function Dump($db, $sql, $title) {
    Write-Output ""
    Write-Output "========== [$db] $title =========="
    $cs = "Server=localhost;Database=$db;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
    $conn = New-Object System.Data.SqlClient.SqlConnection $cs
    $conn.Open()
    $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 180; $cmd.CommandText = $sql
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
            if ($c -ge 80) { Write-Output "(truncated)"; break }
        }
        $r.Close()
        if ($c -eq 0) { Write-Output "(no rows)" }
    } catch {
        Write-Output ("ERROR: " + $_.Exception.Message)
        try { $r.Close() } catch {}
    }
    $conn.Close()
}

Dump Focus80E0 @"
SELECT r.iReportId, r.sReportName, r.iReportType, r.iSourceType
FROM dbo.cCore_Reports_0 r
WHERE r.sReportName LIKE '%Commission%'
   OR r.sReportName LIKE '%Atlas%'
   OR r.sReportName LIKE '%Detail%'
ORDER BY r.iReportId
"@ "commission reports"

Dump Focus80E0 @"
SELECT l.iLayoutId, l.sLayoutName, c.iColumnId, c.iFieldId, c.iType, c.sColumn, c.sAliasName, c.iDecimalInColumn, c.iMiscOption, c.sFormat, c.iAlignment
FROM dbo.cCore_ReportLayouts_0 l
JOIN dbo.cCore_ReportColumns_0 c ON c.iLayoutId = l.iLayoutId
WHERE l.iReportId IN (70266, 70267, 70268)
ORDER BY l.iReportId, l.iLayoutId, c.iFieldId
"@ "70266-70268 all layout columns"

Dump Focus80E0 @"
SELECT iReportId,
  CASE WHEN sSqlQuery LIKE '%nvarchar(10)) AS [Contract Date]%' THEN 1 ELSE 0 END AS NvContractDate,
  CASE WHEN sSqlQuery LIKE '%decimal(18, 0)) AS [Contract Date]%' THEN 1 ELSE 0 END AS DecContractDate,
  CASE WHEN sSqlQuery LIKE '%SODateText%' THEN 1 ELSE 0 END AS HasSODateText,
  CASE WHEN sSqlQuery LIKE '%ReceiptDateText%' THEN 1 ELSE 0 END AS HasRctDateText,
  CHARINDEX('Contract Date', sSqlQuery) AS PosCD,
  LEN(sSqlQuery) AS SqlLen
FROM dbo.cCore_RDQuery_0
WHERE iReportId IN (70266, 70267, 70268)
"@ "70266-70268 SQL date flags"

Dump Focus80G0 @"
SELECT r.iReportId, r.sReportName, r.iReportType, r.iSourceType
FROM dbo.cCore_Reports_0 r
WHERE r.sReportName LIKE '%Commission%'
ORDER BY r.iReportId
"@ "G0 commission reports"
