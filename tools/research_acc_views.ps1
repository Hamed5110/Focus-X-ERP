$cs = "Server=localhost;Database=Focus80G0;User Id=sa;Password=$env:FOCUS_SQL_PASSWORD;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
function Dump($sql, $title) {
    Write-Output ""
    Write-Output "========== $title =========="
    $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 90; $cmd.CommandText = $sql
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
                    if ($v.Length -gt 130) { $v = $v.Substring(0,130) + "..." }
                    $parts += $v
                }
            }
            Write-Output ($parts -join " | ")
            if ($c -ge 25) { break }
        }
        $r.Close()
        if ($c -eq 0) { Write-Output "(no rows)" }
    } catch { Write-Output $_.Exception.Message }
}

Dump @"
SELECT name FROM sys.views
WHERE name LIKE N'%Account%'
ORDER BY name
"@ "all Account views"

Dump @"
SELECT p.iReportId, r.sReportName, p.sFieldName, p.sFieldVariable, p.iControlType, p.iFieldType,
       p.iFieldId, p.iSubParentId, p.bGroup, LEFT(ISNULL(p.sDefault,N''),60) Def
FROM dbo.cCore_ReportParameter_0 p
JOIN dbo.cCore_Reports_0 r ON r.iReportId = p.iReportId
WHERE p.iControlType = 1 OR p.sFieldName LIKE N'%Customer%' OR p.sFieldName LIKE N'%Account%'
"@ "Account/customer params"

Dump @"
SELECT TOP 1 LEFT(OBJECT_DEFINITION(OBJECT_ID(N'dbo.vrCore_Account')), 500)
"@ "vrCore_Account head"

Dump @"
SELECT TOP 1 LEFT(OBJECT_DEFINITION(OBJECT_ID(N'dbo.vCore_Account')), 400)
"@ "vCore_Account head"

Dump @"
SELECT iMasterId, sCode, sName, iAccountType
FROM dbo.mCore_Account
WHERE sName LIKE N'%Ebrahim Saad%' OR sName LIKE N'Trade Payable Foreign'
"@ "ids for search test"

Dump @"
SELECT TOP 8 sName, sCode FROM dbo.vaCore_Account
WHERE ReportStatus = 3 AND ISNULL(bGroup,0)=0 AND sName LIKE N'%Ebrahim%'
GROUP BY sName, sCode
"@ "status3 Ebrahim"
