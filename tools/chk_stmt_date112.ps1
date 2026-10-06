$cs = "Server=localhost;Database=Focus80G0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$sql = [regex]::Replace((Get-Content -Raw "C:\Users\Hamed Ali Khan\Focus-X-ERP\reports\sql\Customer Statement.sql"), '(?s)/\*.*?\*/', '')
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 90
$cmd.CommandText = "SELECT TOP 1 d.iBookNo FROM dbo.tCore_Header_0 h JOIN dbo.tCore_Data_0 d ON d.iHeaderId=h.iHeaderId AND h.iVoucherType=5634 AND h.sVoucherNo=N'CON-Atl-6142'"
$cid = [int]$cmd.ExecuteScalar()
$decl = @"
DECLARE @iStartDate INT = dbo.DateToInt(CONVERT(datetime,'20260101',112));
DECLARE @iEndDate INT = dbo.DateToInt(CONVERT(datetime,'20261231',112));
DECLARE @CustomerName INT = $cid;
"@
$cmd.CommandText = $decl + $sql
$r = $cmd.ExecuteReader()
$types = @()
for ($i=0; $i -lt $r.FieldCount; $i++) {
    $types += ("{0}:{1}" -f $r.GetName($i), $r.GetFieldType($i).Name)
}
Write-Output ("TYPES: " + ($types -join " | "))
$n=0
while ($r.Read()) {
    $n++
    if ($n -le 6) {
        $cd = if ($r.IsDBNull($r.GetOrdinal("Contract Date"))) { "" } else { ([datetime]$r["Contract Date"]).ToString("yyyy-MM-dd") }
        $rd = if ($r.IsDBNull($r.GetOrdinal("Receipt Date"))) { "" } else { ([datetime]$r["Receipt Date"]).ToString("yyyy-MM-dd") }
        Write-Output ("{0} | Opp={1} | CON={2} | CDate={3} | Rct={4} | RDate={5}" -f `
            $r["Particulars"], $r["Opportunity Type"], $r["Contract / Sales Order No."], $cd, $r["Receipt No."], $rd)
    }
}
$r.Close()
Write-Output "ROWS=$n"
$conn.Close()
