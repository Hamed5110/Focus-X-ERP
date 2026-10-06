$cs = "Server=localhost;Database=Focus80G0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
function Dump($sql, $title) {
    Write-Output ""
    Write-Output "========== $title =========="
    $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 120; $cmd.CommandText = $sql
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
            if ($c -ge 80) { Write-Output "(truncated)"; break }
        }
        $r.Close()
        if ($c -eq 0) { Write-Output "(no rows)" }
    } catch {
        Write-Output ("ERROR: " + $_.Exception.Message)
        try { $r.Close() } catch {}
    }
}

Dump @"
SELECT iMasterId, sCode, sName FROM dbo.mCore_Account WHERE sCode = N'AC-6993' OR sName LIKE N'%Hussain Alsaleem%'
"@ "customer"

Dump @"
SELECT h.iHeaderId, h.sVoucherNo, h.iDate, h.fNet, h.fGross, h.fDiscount,
       hd.OpportunityType, h.iAuth, h.bCancelled, h.bVersion, h.bSuspended
FROM dbo.tCore_Header_0 h
LEFT JOIN dbo.tCore_HeaderData5634_0 hd ON hd.iHeaderId = h.iHeaderId
WHERE h.iVoucherType = 5634 AND h.sVoucherNo LIKE N'%7035%'
"@ "header 7035"

Dump @"
SELECT h.sVoucherNo, d.iBodyId, d.iBookNo, d.iCode, d.iType, d.bVoid, d.iFaTag,
       d.fNet AS LineFNet, d.fGross, d.mAmount1, d.mAmount2, d.fQuantity
FROM dbo.tCore_Header_0 h
INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId = h.iHeaderId
WHERE h.iVoucherType = 5634 AND h.sVoucherNo LIKE N'%7035%'
"@ "body 7035"

Dump @"
SELECT
    hd.OpportunityType,
    CASE ISNULL(hd.OpportunityType, 0)
        WHEN 1 THEN N'New Business'
        WHEN 2 THEN N'Additional Business'
        WHEN 3 THEN N'Size Variations'
        WHEN 4 THEN N'Partner Sale'
        WHEN 5 THEN N'Size Variation'
        WHEN 6 THEN N'Cancellation'
        WHEN 7 THEN N'Free of Cost'
        WHEN 8 THEN N'Promotion Prize'
        WHEN 9 THEN N'Amendment'
        WHEN 10 THEN N'Adjustment'
        ELSE CAST(ISNULL(hd.OpportunityType,0) AS nvarchar(10))
    END AS OppName,
    COUNT(*) AS Docs,
    SUM(CASE WHEN h.fNet < 0 THEN 1 ELSE 0 END) AS NegFNet,
    SUM(CASE WHEN h.fNet > 0 THEN 1 ELSE 0 END) AS PosFNet,
    SUM(CASE WHEN h.fNet = 0 THEN 1 ELSE 0 END) AS ZeroFNet,
    MIN(h.fNet) AS MinFNet,
    MAX(h.fNet) AS MaxFNet
FROM dbo.tCore_Header_0 h
LEFT JOIN dbo.tCore_HeaderData5634_0 hd ON hd.iHeaderId = h.iHeaderId
WHERE h.iVoucherType = 5634
  AND ISNULL(h.iAuth,1)=1 AND ISNULL(h.bCancelled,0)=0 AND ISNULL(h.bVersion,0)=0 AND ISNULL(h.bSuspended,0)=0
GROUP BY hd.OpportunityType
ORDER BY hd.OpportunityType
"@ "5634 fNet sign by OpportunityType"

Dump @"
SELECT TOP 30
    h.sVoucherNo,
    hd.OpportunityType,
    h.fNet,
    -h.fNet AS Flip,
    ABS(h.fNet) AS AbsFNet,
    acc.sCode
FROM dbo.tCore_Header_0 h
INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId = h.iHeaderId AND ISNULL(d.iType,0)=0 AND ISNULL(d.bVoid,0)=0
INNER JOIN dbo.mCore_Account acc ON acc.iMasterId = d.iBookNo
LEFT JOIN dbo.tCore_HeaderData5634_0 hd ON hd.iHeaderId = h.iHeaderId
WHERE h.iVoucherType = 5634 AND d.iBookNo = 17500
  AND ISNULL(h.iAuth,1)=1 AND ISNULL(h.bCancelled,0)=0 AND ISNULL(h.bVersion,0)=0 AND ISNULL(h.bSuspended,0)=0
GROUP BY h.sVoucherNo, hd.OpportunityType, h.fNet, acc.sCode
ORDER BY h.sVoucherNo DESC
"@ "Hussain contracts signed fNet"

$conn.Close()
