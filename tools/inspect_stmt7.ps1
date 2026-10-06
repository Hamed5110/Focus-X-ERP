$cs = "Server=localhost;Database=Focus80G0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
function Run($title, $sql) {
    Write-Output "==== $title ===="
    $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 60; $cmd.CommandText = $sql
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

Run "typeofcontracts" @"
SELECT iMasterId, sCode, sName FROM dbo.mCore_typeofcontracts ORDER BY iMasterId
"@

Run "master names containing opport or contract" @"
SELECT iMasterTypeId, sMasterName, sVariable
FROM dbo.cCore_MasterDef
WHERE sMasterName LIKE N'%contract%' OR sMasterName LIKE N'%opport%'
   OR sVariable LIKE N'%opport%' OR sMasterName LIKE N'%sale%'
ORDER BY sMasterName
"@

Run "typeofadvances" @"
SELECT iMasterId, sCode, sName FROM dbo.mCore_typeofadvances ORDER BY iMasterId
"@

Run "iRef join via iHeaderId" @"
SELECT TOP 8
    r.iRef, r.iRefType, r.mAmount,
    hc.sVoucherNo, hc.iVoucherType
FROM dbo.tCore_Refrn_0 r
INNER JOIN dbo.tCore_Data_0 dr ON dr.iBodyId = r.iBodyId
INNER JOIN dbo.tCore_Header_0 hr ON hr.iHeaderId = dr.iHeaderId
LEFT JOIN dbo.tCore_Header_0 hc ON hc.iHeaderId = CASE WHEN r.iRef BETWEEN 1 AND 2000000000 THEN CAST(r.iRef AS int) ELSE 0 END
WHERE hr.iVoucherType IN (4608,4609,4610) AND hr.sVoucherNo LIKE N'ATIC-26-1372'
"@

Run "Refrn iRef is body or header" @"
SELECT
    SUM(CASE WHEN dc.iBodyId IS NOT NULL THEN 1 ELSE 0 END) AS AsBody,
    SUM(CASE WHEN hc.iHeaderId IS NOT NULL THEN 1 ELSE 0 END) AS AsHeader
FROM dbo.tCore_Refrn_0 r
INNER JOIN dbo.tCore_Data_0 dr ON dr.iBodyId = r.iBodyId
INNER JOIN dbo.tCore_Header_0 hr ON hr.iHeaderId = dr.iHeaderId AND hr.iVoucherType IN (4608,4609,4610)
LEFT JOIN dbo.tCore_Data_0 dc ON dc.iBodyId = CASE WHEN r.iRef BETWEEN 1 AND 2000000000 THEN CAST(r.iRef AS int) ELSE -1 END
LEFT JOIN dbo.tCore_Header_0 hc ON hc.iHeaderId = CASE WHEN r.iRef BETWEEN 1 AND 2000000000 THEN CAST(r.iRef AS int) ELSE -1 END
"@

$conn.Close()
