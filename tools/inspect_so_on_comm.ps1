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
                    if ($v.Length -gt 160) { $v = $v.Substring(0,160) + "..." }
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
        try { if ($r) { $r.Close() } } catch {}
    }
}

Dump @"
SELECT TABLE_NAME, COLUMN_NAME
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME IN ('tCore_HeaderData4610_0','tCore_HeaderData4609_0','tCore_HeaderData4608_0','tCore_HeaderData5634_0')
  AND (
    COLUMN_NAME LIKE '%Quote%' OR COLUMN_NAME LIKE '%SO%' OR COLUMN_NAME LIKE '%Order%'
    OR COLUMN_NAME LIKE '%Contract%' OR COLUMN_NAME LIKE '%Sales%' OR COLUMN_NAME LIKE '%Ref%'
    OR COLUMN_NAME LIKE '%Doc%'
  )
ORDER BY TABLE_NAME, COLUMN_NAME
"@ "header extra SO/contract cols"

Dump @"
DECLARE @S INT = dbo.DateToInt(CONVERT(datetime,'20260801',112));
DECLARE @E INT = dbo.DateToInt(CONVERT(datetime,'20260831',112));
SELECT
  COUNT(DISTINCT hr.iHeaderId) Rct,
  COUNT(DISTINCT CASE WHEN hs.iHeaderId IS NOT NULL THEN hr.iHeaderId END) WithSO,
  COUNT(DISTINCT hs.sVoucherNo) DistinctSO,
  COUNT(DISTINCT CASE WHEN ISNULL(hd4610.QuoteNo,N'')<>N'' THEN hr.iHeaderId END) HasQuote
FROM dbo.tCore_Header_0 hr
JOIN dbo.tCore_Data_0 dr ON dr.iHeaderId=hr.iHeaderId
   AND hr.iVoucherType IN (4608,4609,4610)
   AND ISNULL(hr.iAuth,1)=1 AND ISNULL(hr.bCancelled,0)=0 AND ISNULL(hr.bVersion,0)=0 AND ISNULL(hr.bSuspended,0)=0
   AND hr.iDate BETWEEN @S AND @E
   AND dr.bUpdateFA=1 AND dr.iCode>0 AND dr.iFaTag=2040
   AND ISNULL(dr.iType,0)=0 AND ISNULL(dr.bVoid,0)=0 AND ISNULL(dr.iAuthStatus,0)<2
JOIN dbo.mCore_Account acct ON acct.iMasterId=dr.iCode AND acct.iAccountType IN (5,7)
LEFT JOIN dbo.tCore_HeaderData4610_0 hd4610 ON hd4610.iHeaderId=hr.iHeaderId
LEFT JOIN dbo.tCore_HeaderData4609_0 hd4609 ON hd4609.iHeaderId=hr.iHeaderId
LEFT JOIN dbo.tCore_HeaderData4608_0 hd4608 ON hd4608.iHeaderId=hr.iHeaderId
LEFT JOIN dbo.tCore_Refrn_0 r ON r.iBodyId=dr.iBodyId
LEFT JOIN dbo.tCore_Data_0 ds ON ds.iBodyId=r.iRef
LEFT JOIN dbo.tCore_Header_0 hs ON hs.iHeaderId=ds.iHeaderId AND hs.iVoucherType=5634
WHERE COALESCE(NULLIF(hd4610.PaymentCode,0), NULLIF(hd4609.PaymentCode,0), NULLIF(hd4608.PaymentCode,0), 0) IN (1,4)
"@ "Aug Atlas 001/004 SO link rate"

Dump @"
DECLARE @S INT = dbo.DateToInt(CONVERT(datetime,'20260801',112));
DECLARE @E INT = dbo.DateToInt(CONVERT(datetime,'20260831',112));
SELECT TOP 30
  ISNULL(acct.sName,N'') AS Cust,
  hr.sVoucherNo AS Rct,
  ISNULL(hs.sVoucherNo, N'') AS SO,
  hs.iDate AS SOPacked,
  CAST(COALESCE(NULLIF(hd4610.TotalContractAmt,0), NULLIF(TRY_CONVERT(decimal(18,4), NULLIF(LTRIM(RTRIM(hd4609.ContractAmount)),N'')),0), 0) AS decimal(18,2)) AS CVal,
  CAST(SUM(CASE WHEN dr.mAmount1>0 THEN dr.mAmount1 ELSE 0 END) AS decimal(18,2)) AS Coll
FROM dbo.tCore_Header_0 hr
JOIN dbo.tCore_Data_0 dr ON dr.iHeaderId=hr.iHeaderId
   AND hr.iVoucherType IN (4608,4609,4610)
   AND ISNULL(hr.iAuth,1)=1 AND ISNULL(hr.bCancelled,0)=0 AND ISNULL(hr.bVersion,0)=0 AND ISNULL(hr.bSuspended,0)=0
   AND hr.iDate BETWEEN @S AND @E
   AND dr.bUpdateFA=1 AND dr.iCode>0 AND dr.iFaTag=2040
   AND ISNULL(dr.iType,0)=0 AND ISNULL(dr.bVoid,0)=0 AND ISNULL(dr.iAuthStatus,0)<2
JOIN dbo.mCore_Account acct ON acct.iMasterId=dr.iCode AND acct.iAccountType IN (5,7)
LEFT JOIN dbo.tCore_HeaderData4610_0 hd4610 ON hd4610.iHeaderId=hr.iHeaderId
LEFT JOIN dbo.tCore_HeaderData4609_0 hd4609 ON hd4609.iHeaderId=hr.iHeaderId
LEFT JOIN dbo.tCore_HeaderData4608_0 hd4608 ON hd4608.iHeaderId=hr.iHeaderId
LEFT JOIN dbo.tCore_Refrn_0 r ON r.iBodyId=dr.iBodyId
LEFT JOIN dbo.tCore_Data_0 ds ON ds.iBodyId=r.iRef
LEFT JOIN dbo.tCore_Header_0 hs ON hs.iHeaderId=ds.iHeaderId AND hs.iVoucherType=5634
WHERE COALESCE(NULLIF(hd4610.PaymentCode,0), NULLIF(hd4609.PaymentCode,0), NULLIF(hd4608.PaymentCode,0), 0) IN (1,4)
GROUP BY acct.sName, hr.sVoucherNo, hs.sVoucherNo, hs.iDate, hd4610.TotalContractAmt, hd4609.ContractAmount
ORDER BY Cust, SO
"@ "Aug sample customer SO"

Dump @"
SELECT name FROM sys.objects WHERE type IN ('FN','FS') AND name LIKE '%Date%' ORDER BY name
"@ "date functions"
