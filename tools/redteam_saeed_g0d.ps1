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

$decl = @"
DECLARE @iStartDate INT = dbo.DateToInt(CONVERT(datetime,'20260801',112));
DECLARE @iEndDate INT = dbo.DateToInt(CONVERT(datetime,'20260831',112));
"@
$q = Get-Content -Raw "C:\Users\Hamed Ali Khan\Focus-X-ERP\reports\sql\Monthly Sales Commission - Atlas Aluminum Detail.sql"
$cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 180
$cmd.CommandText = $decl + " SELECT * FROM ( " + $q + " ) z WHERE z.[Customer Name] LIKE N'%Saeed%' AND z.[Customer Name] LIKE N'%Moham%'"
try {
    Write-Output ""
    Write-Output "========== DETAIL SQL rows for Mohamed/Mohammed Saeed =========="
    $r = $cmd.ExecuteReader()
    $n = $r.FieldCount
    $hdr = @(); for ($i=0; $i -lt $n; $i++) { $hdr += $r.GetName($i) }
    Write-Output ($hdr -join " | ")
    while ($r.Read()) {
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
} catch {
    Write-Output ("DETAIL FAIL: " + $_.Exception.Message)
}

Dump @"
SELECT h.sVoucherNo, h.iVoucherType,
  CAST((h.iDate & 0xfff0000)/65536 AS varchar(4)) + N'-'
    + RIGHT(N'0'+CAST((h.iDate & 0xff00)/256 AS varchar(2)),2) + N'-'
    + RIGHT(N'0'+CAST((h.iDate & 0xff) AS varchar(2)),2) AS VoucherDate,
  h.fNet, d.iCode, d.iBookNo, d.iFaTag, d.mAmount1, acct.sName
FROM dbo.tCore_Header_0 h
JOIN dbo.tCore_Data_0 d ON d.iHeaderId=h.iHeaderId
LEFT JOIN dbo.mCore_Account acct ON acct.iMasterId = CASE WHEN d.iCode>0 THEN d.iCode ELSE d.iBookNo END
WHERE h.sVoucherNo LIKE N'%NDT97%' OR h.sVoucherNo LIKE N'%15613007%'
   OR CAST(d.mAmount1 AS varchar(40)) LIKE N'8500%'
   OR h.sVoucherNo LIKE N'%SQ-Atl-12089%'
ORDER BY h.iHeaderId
"@ "NDT97 / 8500 / quote"

Dump @"
SELECT h.sVoucherNo, h.iVoucherType,
  CAST((h.iDate & 0xfff0000)/65536 AS varchar(4)) + N'-'
    + RIGHT(N'0'+CAST((h.iDate & 0xff00)/256 AS varchar(2)),2) + N'-'
    + RIGHT(N'0'+CAST((h.iDate & 0xff) AS varchar(2)),2) AS VoucherDate,
  h.fNet, d.iCode, d.iBookNo, d.mAmount1
FROM dbo.tCore_Header_0 h
JOIN dbo.tCore_Data_0 d ON d.iHeaderId=h.iHeaderId
WHERE h.sVoucherNo LIKE N'%SQ-Atl-12089%' OR h.sVoucherNo LIKE N'%12089%'
"@ "quotation 12089"

Dump @"
SELECT TOP 20 h.sVoucherNo, h.iVoucherType, h.fNet, d.mAmount1, d.iCode, d.iBookNo
FROM dbo.tCore_Header_0 h
JOIN dbo.tCore_Data_0 d ON d.iHeaderId=h.iHeaderId
WHERE ABS(d.mAmount1-8500)<0.05 OR ABS(h.fNet-8500)<0.05 OR ABS(h.fNet+8500)<0.05
"@ "any 8500 amount"

Dump @"
SELECT vac.iMasterId, vac.iTreeId, vac.Salesmanname, sm.sName
FROM dbo.vaCore_Account vac
LEFT JOIN dbo.mCore_Salesman sm ON sm.iMasterId = vac.Salesmanname
WHERE vac.iMasterId = 21061
"@ "salesman"
