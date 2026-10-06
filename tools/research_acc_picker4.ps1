$cs = "Server=localhost;Database=Focus80G0;User Id=sa;Password=$env:FOCUS_SQL_PASSWORD;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
function Dump($sql, $title) {
    Write-Output ""
    Write-Output "========== $title =========="
    $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 60; $cmd.CommandText = $sql
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
                    $parts += $v
                }
            }
            Write-Output ($parts -join " | ")
            if ($c -ge 20) { break }
        }
        $r.Close()
        if ($c -eq 0) { Write-Output "(no rows)" }
    } catch { Write-Output $_.Exception.Message }
}

Dump @"
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME LIKE N'%ccount%ype%' OR TABLE_NAME LIKE N'%Accounttype%'
"@ "type tables"

Dump @"
SELECT acc.sCode, acc.sName, acc.iAccountType, acc.ReportStatus
FROM dbo.vaCore_Account acc
WHERE acc.sName LIKE N'%Ebrahim Saad%' OR acc.sCode = N'AC-847'
   OR acc.sName LIKE N'Trade Payable%'
GROUP BY acc.sCode, acc.sName, acc.iAccountType, acc.ReportStatus
"@ "sample types"

Dump @"
SELECT TOP 15 iMasterId, sName, sCode FROM dbo.mCore_accounttype
"@ "mCore_accounttype"

Dump @"
SELECT TOP 15 iMasterId, sName FROM dbo.vmCore_Account
WHERE iMasterId IN (5,6,1,33) OR sName LIKE N'%Debtor%' OR sName LIKE N'%Creditor%' OR sName LIKE N'%Customer%'
"@ "type names guess"
