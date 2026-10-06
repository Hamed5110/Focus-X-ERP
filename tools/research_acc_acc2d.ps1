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
        while ($r.Read()) {
            for ($i=0; $i -lt $n; $i++) {
                $v = if ($r.IsDBNull($i)) { "" } else { [string]$r.GetValue($i) }
                Write-Output $v
            }
        }
        $r.Close()
    } catch { Write-Output $_.Exception.Message }
}

Dump "SELECT OBJECT_DEFINITION(OBJECT_ID(N'dbo.vtCode_DataFA_0'))" "vtCode_DataFA_0 def"
Dump "SELECT OBJECT_DEFINITION(OBJECT_ID(N'dbo.vtAccount_DrCr_0'))" "vtAccount_DrCr_0 def"
Dump "SELECT OBJECT_DEFINITION(OBJECT_ID(N'dbo.vCore_AccountBalances_0'))" "vCore_AccountBalances_0 def"
Dump "SELECT OBJECT_DEFINITION(OBJECT_ID(N'dbo.fCore_OpeningBalanaceOn_0'))" "fCore_OpeningBalanaceOn_0 def"
