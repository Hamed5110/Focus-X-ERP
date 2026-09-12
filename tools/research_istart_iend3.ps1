$cs = "Server=localhost;Database=Focus80E0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
function Dump($sql, $title) {
    Write-Output ""
    Write-Output "========== $title =========="
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
SELECT iReportId, sReportName, iReportType, iSourceType
FROM dbo.cCore_Reports_0
WHERE iReportId IN (70198, 70199, 70223, 70224, 70266, 70231, 70265)
"@ "report headers"

Dump @"
SELECT iReportId, iParameterId, sFieldName, sFieldVariable, iControlType, iFieldType, iSelectionMode, sValue, iFieldId, iType, sDefault, bDefault
FROM dbo.cCore_ReportParameter_0
WHERE iReportId IN (70198, 70199, 70223, 70224, 70266, 70231, 70265)
ORDER BY iReportId, iParameterId
"@ "params on those reports"

Dump @"
SELECT iReportId, LEN(sSqlQuery) AS L
FROM dbo.cCore_RDQuery_0
WHERE iReportId IN (70198, 70223)
"@ "sql lengths"

# write full SQL to files
foreach ($id in @(70198, 70223)) {
    $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 60
    $cmd.CommandText = "SELECT sSqlQuery FROM dbo.cCore_RDQuery_0 WHERE iReportId = $id"
    $sql = [string]$cmd.ExecuteScalar()
    $out = "C:\Users\Hamed Ali Khan\Focus-X-ERP\tools\sql_$id`_saved.sql"
    Set-Content -Path $out -Value $sql -Encoding UTF8
    Write-Output "WROTE $out length=$($sql.Length)"
}

Dump @"
SELECT iReportId, iVoucherType, iTranSetId, iDocumentOption
FROM dbo.cCore_ReportTransactionSet_0
WHERE iReportId IN (70198, 70223, 70266)
"@ "TS for those reports"
