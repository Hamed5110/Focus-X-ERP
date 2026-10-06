$cs = "Server=localhost;Database=Focus80G0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$sql = [regex]::Replace((Get-Content -Raw "C:\Users\Hamed Ali Khan\Focus-X-ERP\reports\sql\Customer Statement.sql"), '(?s)/\*.*?\*/', '')
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 90
$decl = @"
DECLARE @iStartDate INT = dbo.DateToInt(CONVERT(datetime,'20260101',112));
DECLARE @iEndDate INT = dbo.DateToInt(CONVERT(datetime,'20260930',112));
DECLARE @CustomerName INT = 17500;
"@
$cmd.CommandText = $decl + $sql
try {
    $r = $cmd.ExecuteReader()
    $n=0; $lastBal=0; $sumBal=0; $sumDr=0; $sumCr=0
    while ($r.Read()) {
        $n++
        $dr = [decimal]$r["Debit"]; $cr = [decimal]$r["Credit"]; $bal = [decimal]$r["Balance"]
        $sumDr += $dr; $sumCr += $cr; $sumBal += $bal; $lastBal = $bal
        Write-Output ("{0} | Opp={1} | CON={2} | CDate={3} | Rct={4} | RDate={5} | Rec={6} | Dr={7} | Cr={8} | Bal={9}" -f `
            $r["Particulars"], $r["Opportunity Type"], $r["Contract / Sales Order No."], $r["Contract Date"], `
            $r["Receipt No."], $r["Receipt Date"], $r["Amount Received"], $dr, $cr, $bal)
    }
    $r.Close()
    Write-Output ("ROWS=$n SumDr=$sumDr SumCr=$sumCr SumBal=$sumBal LastBal=$lastBal Net=$($sumDr-$sumCr)")
} catch {
    Write-Output ("FAIL: " + $_.Exception.Message)
}
$conn.Close()
