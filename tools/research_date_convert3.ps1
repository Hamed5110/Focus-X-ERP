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

Dump @"
SET DATEFORMAT dmy;
SELECT
  dbo.IntToDate(132777985) AS IntToDate_Aug1_dmy,
  dbo.IntToDate(132778015) AS IntToDate_Aug31_dmy,
  CAST(DATEFROMPARTS(2026,8,1) AS datetime) AS DFP_Aug1,
  CONVERT(datetime,'20260801',112) AS C112_Aug1,
  CASE WHEN dbo.IntToDate(132777985) >= CONVERT(datetime,'20260801',112)
        AND dbo.IntToDate(132777985) <= CONVERT(datetime,'20260831',112) THEN 1 ELSE 0 END AS IntToDateInAug_dmy
"@ "Aug packed under dmy"

Dump @"
SELECT LEFT(q.sSqlQuery, 800)
FROM dbo.cCore_RDQuery_0 q
WHERE q.iReportId = 70256
"@ "PO SQL start"

Dump @"
SELECT c.iFieldId, c.sColumn, c.sAliasName, c.iType
FROM dbo.cCore_ReportColumns_0 c
WHERE c.iLayoutId = 6904
ORDER BY c.iFieldId
"@ "PO 70256 columns"
