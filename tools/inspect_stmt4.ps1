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
            if ($n -ge 30) { Write-Output "... truncated"; break }
        }
        $r.Close()
        Write-Output ("rows_shown=$n")
    } catch {
        Write-Output ("FAIL: " + $_.Exception.Message)
        try { if ($r) { $r.Close() } } catch {}
    }
}

Run "mCore_Type" @"
SELECT iMasterId, sCode, sName FROM dbo.mCore_Type WHERE iMasterId > 0 ORDER BY iMasterId
"@

Run "mCore_typeofproject" @"
SELECT TOP 20 iMasterId, sCode, sName FROM dbo.mCore_typeofproject ORDER BY iMasterId
"@

Run "cCore_Fields cols" @"
SELECT c.name FROM sys.columns c WHERE c.object_id = OBJECT_ID('dbo.cCore_Fields') ORDER BY c.column_id
"@

Run "fields caption Opportunity" @"
SELECT TOP 15 * FROM dbo.cCore_Fields WHERE sCaption LIKE N'%Opport%' OR sName LIKE N'%Opport%'
"@

Run "Refrn iRefType meaning" @"
SELECT r.iRefType, COUNT(*) Cnt,
       SUM(CASE WHEN hc.iVoucherType = 5634 THEN 1 ELSE 0 END) AS ToCON
FROM dbo.tCore_Refrn_0 r
INNER JOIN dbo.tCore_Data_0 dr ON dr.iBodyId = r.iBodyId
INNER JOIN dbo.tCore_Header_0 hr ON hr.iHeaderId = dr.iHeaderId
LEFT JOIN dbo.tCore_Data_0 dc ON dc.iBodyId = CASE WHEN r.iRef BETWEEN 1 AND 2000000000 THEN CAST(r.iRef AS int) ELSE 0 END
LEFT JOIN dbo.tCore_Header_0 hc ON hc.iHeaderId = dc.iHeaderId
WHERE hr.iVoucherType IN (4608, 4609, 4610)
GROUP BY r.iRefType
"@

Run "receipts with CON ref vs without" @"
SELECT
    SUM(CASE WHEN hc.iVoucherType = 5634 THEN 1 ELSE 0 END) AS LinkedCON,
    SUM(CASE WHEN hc.iHeaderId IS NULL THEN 1 ELSE 0 END) AS NoRefHeader,
    COUNT(*) AS RctLines
FROM dbo.tCore_Header_0 hr
INNER JOIN dbo.tCore_Data_0 dr ON dr.iHeaderId = hr.iHeaderId
INNER JOIN dbo.tCore_Refrn_0 r ON r.iBodyId = dr.iBodyId
LEFT JOIN dbo.tCore_Data_0 dc ON dc.iBodyId = CASE WHEN r.iRef BETWEEN 1 AND 2000000000 THEN CAST(r.iRef AS int) ELSE 0 END
LEFT JOIN dbo.tCore_Header_0 hc ON hc.iHeaderId = dc.iHeaderId
WHERE hr.iVoucherType IN (4608, 4609, 4610)
  AND ISNULL(hr.bCancelled,0)=0
  AND dr.bUpdateFA = 1 AND dr.iCode > 0 AND dr.mAmount1 > 0
"@

$conn.Close()
