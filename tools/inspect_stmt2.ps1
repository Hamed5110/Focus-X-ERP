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

Run "opportunity type master" @"
SELECT name FROM sys.tables
WHERE name LIKE '%opportunity%' OR name LIKE '%OpportunityType%'
   OR name LIKE 'mCore_Opport%' OR name LIKE 'mCore_opportunity%'
ORDER BY name
"@

Run "masters containing Type in name" @"
SELECT name FROM sys.tables
WHERE name LIKE 'mCore_%type%' OR name LIKE 'mCore_%Type%'
ORDER BY name
"@

Run "OpportunityType values on CON" @"
SELECT hd.OpportunityType, COUNT(*) Cnt
FROM dbo.tCore_HeaderData5634_0 hd
GROUP BY hd.OpportunityType
ORDER BY Cnt DESC
"@

Run "sample CON with opp" @"
SELECT TOP 8 h.sVoucherNo, h.iDate, h.fNet, hd.OpportunityType, hd.ContractValue, d.iBookNo, acc.sName
FROM dbo.tCore_Header_0 h
INNER JOIN dbo.tCore_HeaderData5634_0 hd ON hd.iHeaderId = h.iHeaderId
INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId = h.iHeaderId AND d.iBookNo > 0
LEFT JOIN dbo.mCore_Account acc ON acc.iMasterId = d.iBookNo
WHERE h.iVoucherType = 5634 AND ISNULL(h.bCancelled,0)=0 AND ISNULL(h.bVersion,0)=0
ORDER BY h.iDate DESC
"@

Run "Refrn on receipts pointing to CON" @"
SELECT TOP 10
    hr.sVoucherNo AS Rct, hr.iVoucherType, r.mAmount, r.iRef, r.iRefType, r.iAccount, r.iCode,
    hc.sVoucherNo AS RefVou, hc.iVoucherType AS RefType
FROM dbo.tCore_Refrn_0 r
INNER JOIN dbo.tCore_Data_0 dr ON dr.iBodyId = r.iBodyId
INNER JOIN dbo.tCore_Header_0 hr ON hr.iHeaderId = dr.iHeaderId
LEFT JOIN dbo.tCore_Data_0 dc ON dc.iBodyId = CAST(r.iRef AS int)
LEFT JOIN dbo.tCore_Header_0 hc ON hc.iHeaderId = dc.iHeaderId
WHERE hr.iVoucherType IN (4608, 4609, 4610)
ORDER BY hr.iDate DESC
"@

$conn.Close()
