$cs = "Server=localhost;Database=Focus80G0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$sql = [regex]::Replace((Get-Content -Raw "C:\Users\Hamed Ali Khan\Focus-X-ERP\reports\sql\Customer Statement.sql"), '(?s)/\*.*?\*/', '')
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()

function Run($title, $custId) {
    Write-Output ""
    Write-Output "==== $title cust=$custId ===="
    $decl = @"
DECLARE @iStartDate INT = dbo.DateToInt(CONVERT(datetime,'20260101',112));
DECLARE @iEndDate INT = dbo.DateToInt(CONVERT(datetime,'20261231',112));
DECLARE @CustomerName INT = $custId;
"@
    $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 180
    $cmd.CommandText = $decl + $sql
    try {
        $r = $cmd.ExecuteReader()
        $types = @()
        for ($i=0; $i -lt $r.FieldCount; $i++) {
            $types += ("{0}:{1}" -f $r.GetName($i), $r.GetFieldType($i).Name)
        }
        Write-Output ("TYPES: " + ($types -join " | "))
        $n=0; $nullDates=0; $shown=0
        while ($r.Read()) {
            $n++
            if ($r.IsDBNull($r.GetOrdinal("Contract Date")) -or $r.IsDBNull($r.GetOrdinal("Receipt Date")) -or $r.IsDBNull($r.GetOrdinal("iDate"))) {
                $nullDates++
            }
            if ($shown -lt 8) {
                $shown++
                Write-Output ("{0} | {1} | CON={2} CDate={3} Rct={4} RDate={5} Dr={6} Cr={7} Bal={8}" -f `
                    $r["Particulars"], $r["Customer Name"], `
                    $r["Contract / Sales Order No."], $r["Contract Date"], `
                    $r["Receipt No."], $r["Receipt Date"], `
                    $r["Debit"], $r["Credit"], $r["Balance"])
            }
        }
        $r.Close()
        Write-Output ("ROWS=$n NULL_DATES=$nullDates")
    } catch {
        Write-Output ("FAIL: " + $_.Exception.Message)
        try { $r.Close() } catch {}
    }
}

$cmd = $conn.CreateCommand()
$cmd.CommandText = @"
SELECT TOP 1 acc.iMasterId, acc.sCode, acc.sName
FROM dbo.mCore_Account acc
JOIN dbo.tCore_Data_0 d ON d.iBookNo = acc.iMasterId
JOIN dbo.tCore_Header_0 h ON h.iHeaderId = d.iHeaderId AND h.iVoucherType = 5634
WHERE acc.iAccountType IN (5,7) AND acc.sName LIKE N'%Saeed%'
GROUP BY acc.iMasterId, acc.sCode, acc.sName
"@
$r = $cmd.ExecuteReader()
$cid = 0
if ($r.Read()) {
    $cid = [int]$r[0]
    Write-Output ("SAMPLE {0} {1} {2}" -f $r[0], $r[1], $r[2])
}
$r.Close()

Run "empty picker schema" 0
if ($cid -gt 0) { Run "saeed 2026" $cid }

$conn.Close()
