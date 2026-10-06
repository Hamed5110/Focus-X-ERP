$cs = "Server=localhost;Database=Focus80G0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
function Run($title, $sql) {
    Write-Output "==== $title ===="
    $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 90; $cmd.CommandText = $sql
    try {
        $r = $cmd.ExecuteReader()
        $n = 0
        while ($r.Read()) {
            $n++
            $parts = @()
            for ($i=0; $i -lt $r.FieldCount; $i++) {
                $v = $r.GetValue($i)
                if ($v -is [DBNull]) { $v = "NULL" }
                $parts += ("{0}={1}" -f $r.GetName($i), $v)
            }
            Write-Output ($parts -join " | ")
            if ($n -ge 25) { Write-Output "... truncated"; break }
        }
        $r.Close()
        Write-Output ("rows_shown=$n")
    } catch {
        Write-Output ("FAIL: " + $_.Exception.Message)
        try { if ($r) { $r.Close() } } catch {}
    }
}

Run "FX on POI" @"
SELECT TOP 8
    h.sVoucherNo, h.iVoucherType, i.mRate, i.fQuantity, i.mGross, i.mLocalStockValue,
    fx.fExchangeRate, fx.fLocalExchangeRate, d.mAmount1
FROM dbo.tCore_Header_0 h
INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId = h.iHeaderId
INNER JOIN dbo.tCore_Indta_0 i ON i.iBodyId = d.iBodyId
LEFT JOIN dbo.tCore_Data_FX_0 fx ON fx.iBodyId = d.iBodyId
WHERE h.iVoucherType = 2563
  AND ISNULL(h.bCancelled,0)=0 AND ISNULL(h.bVersion,0)=0
  AND i.mRate > 0
ORDER BY h.iDate DESC
"@

Run "vendor account type" @"
SELECT TOP 10 acc.iAccountType, COUNT(*) Cnt
FROM dbo.tCore_Header_0 h
INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId = h.iHeaderId
INNER JOIN dbo.mCore_Account acc ON acc.iMasterId = d.iBookNo
WHERE h.iVoucherType IN (2562, 2563)
  AND ISNULL(h.bCancelled,0)=0
GROUP BY acc.iAccountType
ORDER BY Cnt DESC
"@

Run "iType iAuth PO lines" @"
SELECT ISNULL(h.iAuth, -1) Auth, ISNULL(d.iType,-1) iType, ISNULL(d.bVoid,0) Void, COUNT(*) Cnt
FROM dbo.tCore_Header_0 h
INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId = h.iHeaderId
WHERE h.iVoucherType IN (2562, 2563)
GROUP BY ISNULL(h.iAuth, -1), ISNULL(d.iType,-1), ISNULL(d.bVoid,0)
ORDER BY Cnt DESC
"@

Run "product code col" @"
SELECT TOP 5 p.iMasterId, p.sCode, p.sName
FROM dbo.mCore_Product p
WHERE p.iMasterId IN (3975, 7510, 4228)
"@

Run "unit master" @"
SELECT TOP 3 u.iMasterId, u.sName
FROM dbo.mCore_Unit u
"@

Run "YoY sample City Glasses item 3975" @"
DECLARE @iEndDate int = 132778263; -- 28 Sep 2026-ish
DECLARE @cy int = @iEndDate / 65536;
DECLARE @py int = @cy - 1;
SELECT TOP 6
    CASE WHEN h.iDate / 65536 = @cy THEN 'CY' ELSE 'PY' END AS Yr,
    h.sVoucherNo, h.iDate, i.mRate, i.fQuantity
FROM dbo.tCore_Header_0 h
INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId = h.iHeaderId
INNER JOIN dbo.tCore_Indta_0 i ON i.iBodyId = d.iBodyId
WHERE h.iVoucherType IN (2562, 2563)
  AND d.iBookNo = 8208 AND i.iProduct = 3975
  AND h.iDate / 65536 IN (@cy, @py)
  AND ISNULL(h.bCancelled,0)=0 AND ISNULL(h.bVersion,0)=0 AND ISNULL(h.bSuspended,0)=0
  AND i.mRate > 0
ORDER BY h.iDate DESC, h.iHeaderId DESC
"@

$conn.Close()
