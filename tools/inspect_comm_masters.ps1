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
        if ($c -ge 80) { break }
    }
    $r.Close()
    Write-Output ("-- shown: $c --")
}

Write-Output "=== salesman master ==="
Dump "SELECT iMasterId, sCode, sName, iStatus FROM dbo.mCore_Salesman ORDER BY iMasterId"

Write-Output "`n=== opportunity-like masters ==="
Dump "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME LIKE '%opport%' OR TABLE_NAME LIKE '%OpportunityType%' OR TABLE_NAME LIKE '%opportunitytype%' OR TABLE_NAME LIKE 'mCore_Type%' ORDER BY TABLE_NAME"

Write-Output "`n=== mCore_Type ==="
Dump "SELECT TOP 20 * FROM dbo.mCore_Type"

Write-Output "`n=== commissiontype ==="
Dump "SELECT * FROM dbo.mCore_commissiontype"

Write-Output "`n=== typeofcontracts ==="
Dump "SELECT iMasterId, sCode, sName FROM dbo.mCore_typeofcontracts"

Write-Output "`n=== links columns ==="
Dump "SELECT COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='tCore_Links_0' ORDER BY ORDINAL_POSITION"

Write-Output "`n=== links for header 161270 ==="
Dump "SELECT * FROM dbo.tCore_Links_0 WHERE iHeaderId=161270 OR iRefHeaderId=161270"

$conn.Close()
