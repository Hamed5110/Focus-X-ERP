$cs = "Server=localhost;Database=Focus80E0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 180
function Dump($sql, $title) {
    Write-Output ""
    Write-Output "========== $title =========="
    $cmd.CommandText = $sql
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
                    if ($v.Length -gt 450) { $v = $v.Substring(0,450) + "..." }
                    $parts += $v
                }
            }
            Write-Output ($parts -join " | ")
            if ($c -ge 40) { break }
        }
        $r.Close()
        if ($c -eq 0) { Write-Output "(no rows)" }
    } catch {
        Write-Output ("ERROR: " + $_.Exception.Message)
    }
}

Dump @"
SELECT name FROM sys.objects
WHERE type IN ('FN','TF','IF','P')
  AND (
    name LIKE '%Report%Date%' OR name LIKE '%StartDate%' OR name LIKE '%EndDate%'
    OR name LIKE '%DateToInt%' OR name LIKE '%Period%' OR name LIKE '%AsOn%'
    OR name LIKE '%FilterDate%' OR name LIKE '%GetDate%'
  )
ORDER BY name
"@ "date-related functions"

Dump @"
SELECT TOP 1 q.sSqlQuery
FROM dbo.cCore_RDQuery_0 q
WHERE q.iReportId = 70028
"@ "Receipts Register SQL"

Dump @"
SELECT TOP 1 q.sSqlQuery
FROM dbo.cCore_RDQuery_0 q
WHERE q.iReportId = 70074
"@ "Commission Report 70074 SQL"

Dump @"
SELECT TOP 1 q.sSqlQuery
FROM dbo.cCore_RDQuery_0 q
WHERE q.iReportId = 70256
"@ "PO report 70256 SQL"

Dump @"
SELECT TOP 1 q.sSqlQuery
FROM dbo.cCore_RDQuery_0 q
WHERE q.iReportId = 70064
"@ "Adv Rct by Salesperson SQL"

$conn.Close()
