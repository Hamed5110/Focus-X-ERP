$cs = "Server=localhost;Database=Focus80G0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
function Dump($sql, $title) {
    Write-Output ""
    Write-Output "========== $title =========="
    $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 180; $cmd.CommandText = $sql
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
                if ($r.IsDBNull($i)) { $parts += "NULL" } else {
                    $v = [string]$r.GetValue($i)
                    $v = $v -replace "`r|`n"," "
                    if ($v.Length -gt 800) { $v = $v.Substring(0,800) + "..." }
                    $parts += $v
                }
            }
            Write-Output ($parts -join " | ")
            if ($c -ge 40) { Write-Output "(truncated)"; break }
        }
        $r.Close()
        if ($c -eq 0) { Write-Output "(no rows)" }
    } catch {
        Write-Output ("ERROR: " + $_.Exception.Message)
        try { $r.Close() } catch {}
    }
}

Dump @"
SELECT LEFT(OBJECT_DEFINITION(OBJECT_ID('dbo.vtCode_DataFA_0')), 800) AS DefHead
"@ "vtCode_DataFA_0 def head"

Dump @"
SELECT LEFT(OBJECT_DEFINITION(OBJECT_ID('dbo.vCore_AccountBalances_0')), 800) AS DefHead
"@ "vCore_AccountBalances_0 def head"

Dump @"
SELECT LEFT(OBJECT_DEFINITION(OBJECT_ID('dbo.vtAccount_DrCr_0')), 600) AS DefHead
"@ "vtAccount_DrCr_0 def head"

Dump @"
SELECT *
FROM dbo.vCore_AccountBalances_0
WHERE iMasterId = 17500
"@ "TEST account balances Hussain 17500"

Dump @"
SELECT
  CAST(SUM(CASE WHEN Debit < 0 THEN -Debit ELSE 0 END) AS decimal(18,2)) AS SumDr,
  CAST(SUM(CASE WHEN Credit > 0 THEN Credit ELSE 0 END) AS decimal(18,2)) AS SumCr,
  CAST(
    SUM(CASE WHEN Debit < 0 THEN -Debit ELSE 0 END)
    - SUM(CASE WHEN Credit > 0 THEN Credit ELSE 0 END)
  AS decimal(18,2)) AS Closing_SumDelta,
  CAST((
    SELECT SUM(B) FROM (
      SELECT SUM(CASE WHEN Debit < 0 THEN -Debit ELSE 0 END - CASE WHEN Credit > 0 THEN Credit ELSE 0 END)
        OVER (ORDER BY iDate, iBodyId) AS B
      FROM dbo.vtCode_DataFA_0
      WHERE iMasterId = 17500
        AND iDate BETWEEN 132776193 AND 132778495
        AND iVoucherType IN (256, 3840, 4096, 4608, 4609, 4610, 8704, 8705, 8707, 8708)
        AND (Debit < 0 OR Credit > 0)
    ) z
  ) AS decimal(18,2)) AS SumOfRunning
FROM dbo.vtCode_DataFA_0
WHERE iMasterId = 17500
  AND iDate BETWEEN 132776193 AND 132778495
  AND iVoucherType IN (256, 3840, 4096, 4608, 4609, 4610, 8704, 8705, 8707, 8708)
  AND (Debit < 0 OR Credit > 0)
"@ "TEST prefix-sum vs last identity FA Hussain 2026"

Dump @"
SELECT TOP 5 ROUTINE_NAME, ROUTINE_TYPE, LEFT(ROUTINE_DEFINITION, 200) AS ROUTINE_DEFINITION
FROM INFORMATION_SCHEMA.ROUTINES
WHERE SPECIFIC_NAME IN ('IntToDate','DateToInt','GetDateName','GetDatePart')
"@ "SO INFORMATION_SCHEMA.ROUTINES sample"
