$cs = "Server=localhost;Database=Focus80G0;User Id=sa;Password=$env:FOCUS_SQL_PASSWORD;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
function Dump($sql, $title) {
    Write-Output ""
    Write-Output "========== $title =========="
    $cmd = $conn.CreateCommand(); $cmd.CommandText = $sql
    try {
        $r = $cmd.ExecuteReader()
        $n = $r.FieldCount
        $hdr = @(); for ($i=0; $i -lt $n; $i++) { $hdr += $r.GetName($i) }
        Write-Output ($hdr -join " | ")
        $c=0
        while($r.Read() -and $c -lt 15){
            $parts=@()
            for($i=0;$i -lt $n;$i++){
                $v = if($r.IsDBNull($i)){""}else{([string]$r.GetValue($i) -replace "`r|`n"," ")}
                if($v.Length -gt 200){ $v = $v.Substring(0,200)+"..." }
                $parts += $v
            }
            Write-Output ($parts -join " | ")
            $c++
        }
        $r.Close()
    } catch { Write-Output $_.Exception.Message }
}

Dump @"
SELECT p.name, t.name typ, p.has_default_value, p.default_value
FROM sys.parameters p
JOIN sys.types t ON t.user_type_id = p.user_type_id
WHERE p.object_id = OBJECT_ID(N'dbo.fCore_GetDateRange')
"@ "GetDateRange params"

Dump @"
SELECT OBJECT_DEFINITION(OBJECT_ID(N'dbo.fCore_GetDateRange'))
"@ "GetDateRange def"

Dump @"
SELECT p.name, t.name typ
FROM sys.parameters p
JOIN sys.types t ON t.user_type_id = p.user_type_id
WHERE p.object_id = OBJECT_ID(N'dbo.fCore_GetStartEndDateForMonth')
"@ "StartEndMonth params"

Dump @"
SELECT OBJECT_DEFINITION(OBJECT_ID(N'dbo.fCore_GetDate'))
"@ "fCore_GetDate"
