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
                    if ($v.Length -gt 140) { $v = $v.Substring(0,140) + "..." }
                    $parts += $v
                }
            }
            Write-Output ($parts -join " | ")
            if ($c -ge 50) { Write-Output "(truncated)"; break }
        }
        $r.Close()
        if ($c -eq 0) { Write-Output "(no rows)" }
    } catch {
        Write-Output ("ERROR: " + $_.Exception.Message)
        try { if ($r) { $r.Close() } } catch {}
    }
}

Dump @"
SELECT TOP 8 h.sVoucherNo, h.iDate, h.iVoucherType, d.iCode, d.iBookNo, d.iFaTag, d.mAmount1, hd.ContractValue, hd.RefNo, acct.sName
FROM dbo.tCore_Header_0 h
JOIN dbo.tCore_Data_0 d ON d.iHeaderId=h.iHeaderId
LEFT JOIN dbo.tCore_HeaderData5634_0 hd ON hd.iHeaderId=h.iHeaderId
LEFT JOIN dbo.mCore_Account acct ON acct.iMasterId=d.iCode
WHERE h.iVoucherType=5634 AND h.sVoucherNo LIKE N'CON-Atl-%'
  AND ISNULL(h.bCancelled,0)=0 AND ISNULL(h.bVersion,0)=0
ORDER BY h.iHeaderId DESC
"@ "sample 5634 lines"

Dump @"
SELECT
  SUM(CASE WHEN d.iCode>0 THEN 1 ELSE 0 END) CodeLines,
  SUM(CASE WHEN d.iBookNo>0 THEN 1 ELSE 0 END) BookLines,
  COUNT(*) Lines,
  COUNT(DISTINCT h.iHeaderId) Hdrs
FROM dbo.tCore_Header_0 h
JOIN dbo.tCore_Data_0 d ON d.iHeaderId=h.iHeaderId
WHERE h.iVoucherType=5634 AND ISNULL(h.bCancelled,0)=0 AND ISNULL(h.bVersion,0)=0
  AND d.iFaTag=2040
"@ "5634 Atlas customer fields"

Dump @"
DECLARE @S INT = dbo.DateToInt(CONVERT(datetime,'20260801',112));
DECLARE @E INT = dbo.DateToInt(CONVERT(datetime,'20260831',112));
;WITH rct AS (
  SELECT
    dr.iCode AS CustId,
    ISNULL(acct.sName,N'') AS Cust,
    CAST(COALESCE(NULLIF(hd4610.TotalContractAmt,0), NULLIF(TRY_CONVERT(decimal(18,4), NULLIF(LTRIM(RTRIM(hd4609.ContractAmount)),N'')),0), 0) AS decimal(18,2)) AS CVal
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
  WHERE COALESCE(NULLIF(hd4610.PaymentCode,0), NULLIF(hd4609.PaymentCode,0), NULLIF(hd4608.PaymentCode,0), 0) IN (1,4)
  GROUP BY dr.iCode, acct.sName,
           CAST(COALESCE(NULLIF(hd4610.TotalContractAmt,0), NULLIF(TRY_CONVERT(decimal(18,4), NULLIF(LTRIM(RTRIM(hd4609.ContractAmount)),N'')),0), 0) AS decimal(18,2))
),
so AS (
  SELECT
    COALESCE(NULLIF(d.iCode,0), d.iBookNo) AS CustId,
    h.sVoucherNo AS SONo,
    h.iDate AS SODate,
    CAST(ISNULL(hd.ContractValue,0) AS decimal(18,2)) AS CVal,
    CAST(ABS(h.fNet) AS decimal(18,2)) AS NetAbs
  FROM dbo.tCore_Header_0 h
  JOIN dbo.tCore_Data_0 d ON d.iHeaderId=h.iHeaderId AND d.iFaTag=2040
  LEFT JOIN dbo.tCore_HeaderData5634_0 hd ON hd.iHeaderId=h.iHeaderId
  WHERE h.iVoucherType=5634
    AND ISNULL(h.iAuth,1)=1 AND ISNULL(h.bCancelled,0)=0 AND ISNULL(h.bVersion,0)=0 AND ISNULL(h.bSuspended,0)=0
    AND h.sVoucherNo LIKE N'CON-Atl-%'
)
SELECT
  r.Cust, r.CVal,
  so.SONo, so.SODate, so.CVal AS SOVal, so.NetAbs,
  CASE WHEN so.SONo IS NULL THEN N'NO' ELSE N'YES' END AS Hit
FROM rct r
LEFT JOIN so ON so.CustId=r.CustId AND r.CVal>0 AND (
     ABS(so.CVal - r.CVal) < 0.05
  OR ABS(so.NetAbs - r.CVal) < 0.05
)
ORDER BY Hit, r.Cust
"@ "match receipt contract to 5634 by cust+value"

Dump @"
SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME='tCore_HeaderData5634_0' ORDER BY ORDINAL_POSITION
"@ "5634 all extra cols"
