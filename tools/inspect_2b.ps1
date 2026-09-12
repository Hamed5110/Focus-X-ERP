$cs = "Server=localhost;Database=Focus8080;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()

function Run($title, $sql) {
    $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 180; $cmd.CommandText = $sql
    $a = New-Object System.Data.SqlClient.SqlDataAdapter $cmd; $dt = New-Object System.Data.DataTable
    [void]$a.Fill($dt)
    Write-Host "`n== $title =="
    $dt | Format-Table -AutoSize | Out-String -Width 300 | Write-Host
}

Run "SO lines detail: iType / bVoid / rowcount per header" @"
SELECT h.iHeaderId, h.sVoucherNo, d.iBookNo, d.iType, d.bVoid, COUNT(*) AS Lines
FROM dbo.tCore_Header_0 h
INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId = h.iHeaderId
WHERE h.iVoucherType = 5634 AND d.iFaTag = 2040 AND d.iBookNo IN (12606, 15268)
GROUP BY h.iHeaderId, h.sVoucherNo, d.iBookNo, d.iType, d.bVoid
ORDER BY d.iBookNo, h.iHeaderId
"@

Run "Do they pass the activity subquery?" @"
SELECT act.iMasterId
FROM (
    SELECT DISTINCT d.iBookNo AS iMasterId
    FROM dbo.tCore_Header_0 h
    INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId = h.iHeaderId AND h.iVoucherType = 5634 AND d.iFaTag = 2040 AND d.iBookNo > 0 AND ISNULL(d.iType, 0) = 0 AND ISNULL(h.iAuth, 1) = 1 AND ISNULL(h.bCancelled, 0) = 0 AND ISNULL(h.bVersion, 0) = 0 AND ISNULL(h.bSuspended, 0) = 0 AND ISNULL(d.bVoid, 0) = 0
    UNION
    SELECT d.iCode
    FROM dbo.tCore_Header_0 h
    INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId = h.iHeaderId AND h.iVoucherType IN (256, 4096, 4608, 4609, 4610, 8707) AND d.bUpdateFA = 1 AND d.iFaTag = 2040 AND d.iCode > 0 AND ISNULL(d.iType, 0) = 0 AND ISNULL(h.iAuth, 1) = 1 AND ISNULL(d.iAuthStatus, 0) < 2 AND ISNULL(h.bCancelled, 0) = 0 AND ISNULL(h.bVersion, 0) = 0 AND ISNULL(h.bSuspended, 0) = 0 AND ISNULL(d.bVoid, 0) = 0
    UNION
    SELECT d.iBookNo
    FROM dbo.tCore_Header_0 h
    INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId = h.iHeaderId AND h.iVoucherType IN (256, 4096, 4608, 4609, 4610, 8707) AND d.bUpdateFA = 1 AND d.iFaTag = 2040 AND d.iBookNo > 0 AND d.iBookNo <> d.iCode AND ISNULL(d.iType, 0) = 0 AND ISNULL(h.iAuth, 1) = 1 AND ISNULL(d.iAuthStatus, 0) < 2 AND ISNULL(h.bCancelled, 0) = 0 AND ISNULL(h.bVersion, 0) = 0 AND ISNULL(h.bSuspended, 0) = 0 AND ISNULL(d.bVoid, 0) = 0
) act
WHERE act.iMasterId IN (12606, 15268)
"@

Run "Credit lines detail: bUpdateFA / iAuthStatus / iType / bVoid" @"
SELECT h.iHeaderId, h.iVoucherType, h.sVoucherNo, d.iCode, d.iBookNo, d.bUpdateFA, d.iAuthStatus, d.iType, d.bVoid, d.mAmount1, d.mAmount2
FROM dbo.tCore_Header_0 h
INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId = h.iHeaderId
WHERE h.iVoucherType IN (256, 4096, 4608, 4609, 4610, 8707) AND d.iFaTag = 2040 AND (d.iCode IN (12606, 15268) OR d.iBookNo IN (12606, 15268))
ORDER BY d.iCode, h.iHeaderId
"@

$conn.Close()
