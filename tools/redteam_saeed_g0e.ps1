$csG = "Server=localhost;Database=Focus80G0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$csE = "Server=localhost;Database=Focus80E0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"

function Dump($cs, $sql, $title) {
    Write-Output ""
    Write-Output "========== $title =========="
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
                    if ($v.Length -gt 180) { $v = $v.Substring(0,180) + "..." }
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
    $conn.Close()
}

Dump $csE @"
SELECT acct.iMasterId, acct.sName, acct.sCode
FROM dbo.mCore_Account acct
WHERE acct.sCode = N'AC-8402' OR acct.sName LIKE N'Mr. Mohamed Saeed'
"@ "E0 account"

Dump $csE @"
SELECT h.sVoucherNo, h.iVoucherType,
  CAST((h.iDate & 0xfff0000)/65536 AS varchar(4)) + N'-'
    + RIGHT(N'0'+CAST((h.iDate & 0xff00)/256 AS varchar(2)),2) + N'-'
    + RIGHT(N'0'+CAST((h.iDate & 0xff) AS varchar(2)),2) AS VoucherDate,
  d.iCode, d.mAmount1, hd.TotalContractAmt, hd.PaymentCode
FROM dbo.tCore_Header_0 h
JOIN dbo.tCore_Data_0 d ON d.iHeaderId=h.iHeaderId
LEFT JOIN dbo.tCore_HeaderData4610_0 hd ON hd.iHeaderId=h.iHeaderId
WHERE h.iVoucherType=4610 AND d.iCode=21061
"@ "E0 receipts 21061"

Dump $csG @"
SELECT SUM(d.mAmount1) BodySum, ABS(MAX(h.fNet)) HdrNet, COUNT(*) Lines
FROM dbo.tCore_Header_0 h
JOIN dbo.tCore_Data_0 d ON d.iHeaderId=h.iHeaderId
WHERE h.sVoucherNo=N'CON-Atl-6955' AND h.iVoucherType=5634
"@ "CON body sum vs fNet"

Dump $csG @"
SELECT iVoucherType, sName
FROM dbo.mCore_VoucherType_0
WHERE sName LIKE N'%NDT%' OR sName LIKE N'%Card%' OR sName LIKE N'%Clear%'
   OR iVoucherType IN (256, 97, 3332)
"@ "voucher types card/NDT"

Dump $csG @"
SELECT TOP 30 h.sVoucherNo, h.iVoucherType, vt.sName,
  CAST((h.iDate & 0xfff0000)/65536 AS varchar(4)) + N'-'
    + RIGHT(N'0'+CAST((h.iDate & 0xff00)/256 AS varchar(2)),2) + N'-'
    + RIGHT(N'0'+CAST((h.iDate & 0xff) AS varchar(2)),2) AS VoucherDate,
  d.iCode, d.mAmount1, acct.sName
FROM dbo.tCore_Header_0 h
JOIN dbo.tCore_Data_0 d ON d.iHeaderId=h.iHeaderId
LEFT JOIN dbo.mCore_VoucherType_0 vt ON vt.iVoucherType=h.iVoucherType
LEFT JOIN dbo.mCore_Account acct ON acct.iMasterId=d.iCode
WHERE (d.iCode=21061 OR d.iBookNo=21061)
   OR h.sVoucherNo LIKE N'%ATIC-26-1383%'
   OR h.sVoucherNo LIKE N'%ATIC-26-1384%'
   OR EXISTS (
        SELECT 1 FROM dbo.tCore_Header_0 h2
        WHERE h2.iHeaderId=h.iHeaderId AND h2.sVoucherNo LIKE N'NDT%'
      )
ORDER BY h.iDate DESC
"@ "related vouchers"

Dump $csG @"
SELECT TOP 20 h.sVoucherNo, h.iVoucherType,
  CAST((h.iDate & 0xfff0000)/65536 AS varchar(4)) + N'-'
    + RIGHT(N'0'+CAST((h.iDate & 0xff00)/256 AS varchar(2)),2) + N'-'
    + RIGHT(N'0'+CAST((h.iDate & 0xff) AS varchar(2)),2) AS VoucherDate,
  d.iCode, d.mAmount1, d.iFaTag
FROM dbo.tCore_Header_0 h
JOIN dbo.tCore_Data_0 d ON d.iHeaderId=h.iHeaderId
WHERE h.sVoucherNo LIKE N'NDT%' AND (d.mAmount1 IN (2000,1250,3250) OR d.iCode=21061)
"@ "NDT amounts 2000/1250"
