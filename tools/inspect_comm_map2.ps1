$cs = "Server=localhost;Database=Focus80E0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
function Dump($sql) {
    $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 180; $cmd.CommandText = $sql
    $r = $cmd.ExecuteReader()
    $n = $r.FieldCount
    $hdr = @(); for ($i=0;$i -lt $n;$i++) { $hdr += $r.GetName($i) }
    Write-Output ("COLS: " + ($hdr -join " | "))
    $c = 0
    while ($r.Read()) {
        $parts = @()
        for ($i = 0; $i -lt $n; $i++) {
            if ($r.IsDBNull($i)) { $parts += "" } else { $parts += [string]$r.GetValue($i) }
        }
        Write-Output ($parts -join " | ")
        $c++
        if ($c -ge 40) { break }
    }
    $r.Close()
    Write-Output ("-- shown: $c --")
}

Write-Output "=== account salesman AC-8194 ==="
Dump @"
SELECT TOP 3 iMasterId, sCode, sName, Salesmanname, SalesmannameName, Designername, DesignernameName
FROM dbo.vaCore_Account WHERE sCode='AC-8194' AND iTreeId=0
"@

Write-Output "`n=== header columns with sales/emp ==="
Dump @"
SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME='tCore_Header_0'
  AND (COLUMN_NAME LIKE '%Sales%' OR COLUMN_NAME LIKE '%Emp%' OR COLUMN_NAME LIKE '%User%' OR COLUMN_NAME LIKE '%Created%')
ORDER BY COLUMN_NAME
"@

Write-Output "`n=== opportunity type tables/cols ==="
Dump @"
SELECT TABLE_NAME, COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS
WHERE COLUMN_NAME LIKE '%Opportunity%'
ORDER BY TABLE_NAME, COLUMN_NAME
"@

Write-Output "`n=== extra field master names ==="
Dump @"
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME LIKE '%ExtraField%' OR TABLE_NAME LIKE '%MasterExtra%' OR TABLE_NAME LIKE '%opportunitytype%'
   OR TABLE_NAME LIKE 'mCore_newbusiness%' OR TABLE_NAME LIKE 'mCore_Opportunity%'
ORDER BY TABLE_NAME
"@

Write-Output "`n=== links for body 632877 / txn ==="
Dump @"
SELECT TOP 20 * FROM dbo.tCore_Links_0
WHERE iTransactionId IN (632877,161270) OR iRefId IN (632877,161270)
"@

Write-Output "`n=== receipts for AC-8194 Aug-ish ==="
Dump @"
SELECT TOP 15 h.sVoucherNo, h.iVoucherType, h.iDate, h.fNet, d.mAmount1, d.mAmount2, d.iCode, d.iBookNo, d.iFaTag
FROM dbo.tCore_Header_0 h
INNER JOIN dbo.tCore_Data_0 d ON d.iHeaderId=h.iHeaderId
WHERE h.iVoucherType IN (256,4096,4608,4609,4610,8707)
  AND (d.iCode=19823 OR d.iBookNo=19823)
  AND ISNULL(h.bCancelled,0)=0 AND ISNULL(h.bVersion,0)=0
ORDER BY h.iDate DESC
"@

$conn.Close()
