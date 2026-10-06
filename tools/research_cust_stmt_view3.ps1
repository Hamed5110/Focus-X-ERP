$cs = "Server=localhost;Database=Focus80G0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
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
                    if ($v.Length -gt 220) { $v = $v.Substring(0,220) + "..." }
                    $parts += $v
                }
            }
            Write-Output ($parts -join " | ")
            if ($c -ge 40) { Write-Output "(truncated)"; break }
        }
        $r.Close()
        if ($c -eq 0) { Write-Output "(no rows)" }
    } catch {
        Write-Output ("ERROR: " + $_.Exception.Message)
        try { $r.Close() } catch {}
    }
}

Dump @"
SELECT TOP 25
  v.sVoucherNo, v.iVoucherType, v.iDate, v.iMasterId, v.Debit, v.Credit, v.iCode, v.iBookNo, v.bUpdateFA
FROM dbo.vtCode_DataFA_0 v
WHERE v.iMasterId = 18732
  AND v.iDate BETWEEN dbo.DateToInt(CONVERT(datetime,'20260101',112)) AND dbo.DateToInt(CONVERT(datetime,'20261231',112))
ORDER BY v.iDate, v.sVoucherNo
"@ "FA view 18732 2026"

Dump @"
SELECT v.iVoucherType, COUNT(*) Cnt,
  CAST(SUM(v.Debit) AS decimal(18,2)) Dr, CAST(SUM(v.Credit) AS decimal(18,2)) Cr
FROM dbo.vtCode_DataFA_0 v
WHERE v.iMasterId = 18732
GROUP BY v.iVoucherType
ORDER BY Cnt DESC
"@ "FA voucher types for 18732"

Dump @"
SELECT
  CAST(SUM(CASE WHEN iDate < dbo.DateToInt(CONVERT(datetime,'20260101',112)) THEN Debit-Credit ELSE 0 END) AS decimal(18,2)) OpenBal,
  CAST(SUM(CASE WHEN iDate BETWEEN dbo.DateToInt(CONVERT(datetime,'20260101',112)) AND dbo.DateToInt(CONVERT(datetime,'20261231',112)) THEN Debit ELSE 0 END) AS decimal(18,2)) PeriodDr,
  CAST(SUM(CASE WHEN iDate BETWEEN dbo.DateToInt(CONVERT(datetime,'20260101',112)) AND dbo.DateToInt(CONVERT(datetime,'20261231',112)) THEN Credit ELSE 0 END) AS decimal(18,2)) PeriodCr
FROM dbo.vtCode_DataFA_0
WHERE iMasterId = 18732
"@ "FA opening vs period 18732"

Dump @"
SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME='cCore_Reports_0' AND COLUMN_NAME LIKE '%Xml%' OR TABLE_NAME='cCore_Reports_0' AND COLUMN_NAME LIKE '%Cube%'
"@ "report xml cols"

Dump @"
SELECT t.iReportId, t.iVoucherType, t.iTranSetId, t.iDocumentOption
FROM dbo.cCore_ReportTransactionSet_0 t
WHERE t.iReportId IN (560, 70115, 70125)
ORDER BY t.iReportId, t.iVoucherType
"@ "cube transaction sets"

Dump @"
SELECT name FROM sys.views
WHERE name LIKE '%FA%' OR name LIKE '%DrCr%' OR name LIKE '%Refrn%' OR name LIKE '%Bill%'
   OR name LIKE '%Outstanding%' OR name LIKE '%Stmt%' OR name LIKE '%Statement%'
ORDER BY name
"@ "FA/bill views"
