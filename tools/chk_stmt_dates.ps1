$cs = "Server=localhost;Database=Focus80G0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$sql = [regex]::Replace((Get-Content -Raw "C:\Users\Hamed Ali Khan\Focus-X-ERP\reports\sql\Customer Statement.sql"), '(?s)/\*.*?\*/', '')
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 60
$cmd.CommandText = @"
SELECT TOP 1 d.iBookNo, acc.sName
FROM dbo.tCore_Header_0 h
JOIN dbo.tCore_Data_0 d ON d.iHeaderId = h.iHeaderId AND h.iVoucherType = 5634 AND h.sVoucherNo = N'CON-Atl-6142'
JOIN dbo.mCore_Account acc ON acc.iMasterId = d.iBookNo
"@
$r = $cmd.ExecuteReader()
$cid = 0
if ($r.Read()) { $cid = [int]$r[0]; Write-Output ("CUST {0} {1}" -f $r[0], $r[1]) }
$r.Close()
$decl = @"
DECLARE @iStartDate INT = dbo.DateToInt(CONVERT(datetime,'20260101',112));
DECLARE @iEndDate INT = dbo.DateToInt(CONVERT(datetime,'20261231',112));
DECLARE @CustomerName INT = $cid;
"@
$cmd.CommandText = $decl + $sql
$r = $cmd.ExecuteReader()
$types = @()
for ($i=0; $i -lt $r.FieldCount; $i++) { $types += ("{0}:{1}" -f $r.GetName($i), $r.GetFieldType($i).Name) }
Write-Output ("TYPES: " + ($types -join " | "))
$n=0
while ($r.Read()) {
    $n++
    if ($n -le 12) {
        Write-Output ("{0} | Opp={1} | CON={2} | CDate={3} | Rct={4} | RDate={5} | Amt={6} | Bal={7}" -f `
            $r["Particulars"], $r["Opportunity Type"], $r["Contract / Sales Order No."], $r["Contract Date"], `
            $r["Receipt No."], $r["Receipt Date"], $r["Amount Received"], $r["Balance"])
    }
}
$r.Close()
Write-Output "ROWS=$n"
$conn.Close()
