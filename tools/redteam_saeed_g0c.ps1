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
                    if ($v.Length -gt 200) { $v = $v.Substring(0,200) + "..." }
                    $parts += $v
                }
            }
            Write-Output ($parts -join " | ")
        }
        $r.Close()
        if ($c -eq 0) { Write-Output "(no rows)" }
    } catch {
        Write-Output ("ERROR: " + $_.Exception.Message)
        try { if ($r) { $r.Close() } } catch {}
    }
}

Dump @"
SELECT c.name, t.name AS typ
FROM sys.columns c
JOIN sys.types t ON t.user_type_id = c.user_type_id
WHERE c.object_id = OBJECT_ID(N'dbo.tCore_HeaderData4610_0')
ORDER BY c.column_id
"@ "4610 extra columns"

Dump @"
SELECT *
FROM dbo.tCore_HeaderData4610_0
WHERE iHeaderId IN (165821, 165822, 164378)
"@ "4610 extras for both receipts"

Dump @"
SELECT
  h.iHeaderId, h.iVoucherType, h.sVoucherNo,
  CAST((h.iDate & 0xfff0000)/65536 AS varchar(4)) + N'-'
    + RIGHT(N'0'+CAST((h.iDate & 0xff00)/256 AS varchar(2)),2) + N'-'
    + RIGHT(N'0'+CAST((h.iDate & 0xff) AS varchar(2)),2) AS VoucherDate,
  h.fNet, d.iCode, d.iBookNo, d.iFaTag, d.bUpdateFA, d.mAmount1,
  acct.sName, acct.sCode
FROM dbo.tCore_Header_0 h
INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId = h.iHeaderId
LEFT JOIN dbo.mCore_Account acct ON acct.iMasterId = CASE WHEN d.iCode>0 THEN d.iCode ELSE d.iBookNo END
WHERE d.iCode = 21061 OR d.iBookNo = 21061
ORDER BY h.iDate, h.iVoucherType, h.sVoucherNo, d.iBodyId
"@ "ALL vouchers touching AC-8402"

Dump @"
SELECT
  h.sVoucherNo, h.iVoucherType,
  CAST((h.iDate & 0xfff0000)/65536 AS varchar(4)) + N'-'
    + RIGHT(N'0'+CAST((h.iDate & 0xff00)/256 AS varchar(2)),2) + N'-'
    + RIGHT(N'0'+CAST((h.iDate & 0xff) AS varchar(2)),2) AS VoucherDate,
  CAST(ROUND(ABS(h.fNet),0) AS decimal(18,0)) AS NetRnd,
  ABS(h.fNet) AS NetAbs,
  d.iBookNo
FROM dbo.tCore_Header_0 h
INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId = h.iHeaderId AND d.iBookNo = 21061 AND d.iFaTag = 2040
WHERE h.iVoucherType = 5634 AND h.sVoucherNo LIKE N'CON-Atl-%'
  AND ISNULL(h.iAuth,1)=1 AND ISNULL(h.bCancelled,0)=0
  AND ISNULL(h.bVersion,0)=0 AND ISNULL(h.bSuspended,0)=0
GROUP BY h.sVoucherNo, h.iVoucherType, h.iDate, h.fNet, d.iBookNo
"@ "SO match candidates"
