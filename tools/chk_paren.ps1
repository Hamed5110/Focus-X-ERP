$path = "C:\Users\Hamed Ali Khan\Focus-X-ERP\reports\sql\Monthly Sales Commission - Atlas Aluminum Detail.sql"
$sql = Get-Content -Raw $path
# strip comments
$lines = Get-Content $path
$bal = 0
$i = 0
foreach ($line in $lines) {
    $i++
    $inStr = $false
    $chars = $line.ToCharArray()
    for ($k=0; $k -lt $chars.Length; $k++) {
        $ch = $chars[$k]
        if ($ch -eq "'") { $inStr = -not $inStr; continue }
        if ($inStr) { continue }
        if ($ch -eq '(') { $bal++ }
        if ($ch -eq ')') { $bal--; if ($bal -lt 0) { Write-Output "NEG at line $i : $line"; $bal = 0 } }
    }
}
Write-Output "FINAL BALANCE=$bal"

$cs = "Server=localhost;Database=Focus80G0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 60
$decl = "DECLARE @iStartDate INT = 1; DECLARE @iEndDate INT = 1; "
$cmd.CommandText = $decl + ($sql -replace '(?s)/\*.*?\*/','')
try {
    $null = $cmd.ExecuteReader()
    Write-Output "SQL SERVER: OK"
} catch {
    Write-Output ("SQL SERVER: " + $_.Exception.Message)
}
$conn.Close()
