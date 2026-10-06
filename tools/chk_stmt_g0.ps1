$cs = "Server=localhost;Database=Focus80G0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$decl = @"
DECLARE @iStartDate INT = dbo.DateToInt(CONVERT(datetime,'20260101',112));
DECLARE @iEndDate INT = dbo.DateToInt(CONVERT(datetime,'20261231',112));
"@
$sql = [regex]::Replace((Get-Content -Raw "C:\Users\Hamed Ali Khan\Focus-X-ERP\reports\sql\Customer Statement.sql"), '(?s)/\*.*?\*/', '')
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 180
$cmd.CommandText = $decl + $sql
try {
    $r = $cmd.ExecuteReader()
    $names = @(); for ($i=0; $i -lt $r.FieldCount; $i++) { $names += $r.GetName($i) }
    Write-Output ("COLS: " + ($names -join " | "))
    $n=0; $shown=0
    Write-Output "---- Mohamed Saeed sample ----"
    while ($r.Read()) {
        $n++
        if ([string]$r["Customer Name"] -like "*Mohamed Saeed*") {
            $shown++
            if ($shown -le 20) {
                Write-Output ("{0} {1} Opp={2} CON={3} {4} CAmt={5} Rct={6} {7} Rec={8} Dr={9} Cr={10} Bal={11}" -f `
                    $r["Particulars"], $r["Customer Name"], $r["Opportunity Type"], `
                    $r["Contract / Sales Order No."], $r["Contract Date"], $r["Contract Amount"], `
                    $r["Receipt No."], $r["Receipt Date"], $r["Amount Received"], `
                    $r["Debit"], $r["Credit"], $r["Balance"])
            }
        }
    }
    $r.Close()
    Write-Output ("ROWS=$n SaeedLines=$shown")
} catch {
    Write-Output ("FAIL: " + $_.Exception.Message)
}
$conn.Close()
