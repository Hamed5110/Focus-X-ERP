$cs = "Server=localhost;Database=Focus80G0;User Id=sa;Password=$env:FOCUS_SQL_PASSWORD;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$inj = " AND iDate >= 1 AND iDate <= 2 OR iDate = 0 "
function Parse($title, $sql) {
    $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 5
    $cmd.CommandText = "SET PARSEONLY ON;`n$sql;`nSET PARSEONLY OFF;"
    try {
        [void]$cmd.ExecuteNonQuery()
        Write-Output ("PASS  $title")
    } catch {
        $msg = $_.Exception.Message -replace "`r|`n"," "
        if ($msg -match 'OR') { $tag = "OR-ERR" } elseif ($msg -match 'AND') { $tag = "AND-ERR" } else { $tag = "OTHER" }
        if ($msg.Length -gt 140) { $msg = $msg.Substring(0,140) }
        Write-Output ("FAIL-$tag  $title :: $msg")
    }
}

$mini = @"
SELECT CAST(ISNULL(m.sName, N'') AS nvarchar(120)) AS [Name], CAST(0 AS decimal(18, 0)) AS iDate
FROM dbo.mCore_Account m
WHERE m.iMasterId > 0
"@

Write-Output "===== mini baseline ====="
Parse "mini" $mini

Write-Output "`n===== wrap styles ====="
Parse "wrap WHERE" "SELECT * FROM ($mini) z WHERE iDate >= 1 AND iDate <= 2 OR iDate = 0"
Parse "wrap AND" "SELECT * FROM ($mini) z AND iDate >= 1 AND iDate <= 2 OR iDate = 0"
Parse "wrap noalias AND" "SELECT * FROM ($mini) AND iDate >= 1 AND iDate <= 2 OR iDate = 0"
Parse "append AND" ($mini + $inj)
Parse "first WHERE insert" ($mini -replace "WHERE m.iMasterId > 0", "WHERE m.iMasterId > 0$inj")

Write-Output "`n===== ) AS inject (CAST ISNULL) ====="
$pos = $mini.IndexOf(") AS nvarchar")
Parse ") AS nvarchar" $mini.Insert($pos+1, $inj)
$pos = $mini.IndexOf(") AS [Name]")
if ($pos -lt 0) { $pos = $mini.IndexOf("AS [Name]") }
Parse "AS [Name]" $mini.Insert($pos, $inj)
$pos = $mini.IndexOf(") AS iDate")
Parse ") AS iDate" $mini.Insert($pos+1, $inj)
$pos = $mini.IndexOf("0 AS decimal")
Parse "0 AS decimal" $mini.Insert($pos+1, $inj)

Write-Output "`n===== EXISTS close ====="
$ex = @"
SELECT CAST(ISNULL((
    SELECT SUM(h.fNet)
    FROM dbo.tCore_Header_0 h
    WHERE h.iVoucherType = 5634
      AND EXISTS (
            SELECT 1
            FROM dbo.tCore_Data_0 d
            WHERE d.iHeaderId = h.iHeaderId
      )
), 0) AS decimal(18, 2)) AS Amt, CAST(0 AS decimal(18,0)) AS iDate
FROM dbo.mCore_Account m
WHERE m.iMasterId > 0
"@
Parse "exists baseline" $ex
$pos = $ex.LastIndexOf(") AS decimal")
Parse "exists ) AS decimal" $ex.Insert($pos+1, $inj)
$pos = $ex.IndexOf(") , 0)") 
# inject after EXISTS closing paren before comma of ISNULL
$pos = $ex.IndexOf("      )")
Parse "exists inner ) " $ex.Insert($pos+6, $inj)

Write-Output "`n===== comma join ====="
$cj = @"
SELECT CAST(ISNULL((
    SELECT SUM(d.mAmount1)
    FROM dbo.tCore_Header_0 h
    , dbo.tCore_Data_0 d
    WHERE d.iHeaderId = h.iHeaderId
), 0) AS decimal(18, 2)) AS Amt, CAST(0 AS decimal(18,0)) AS iDate
FROM dbo.mCore_Account m
WHERE m.iMasterId > 0
"@
Parse "comma baseline" $cj
$pos = $cj.IndexOf(", dbo.tCore_Data_0 d")
Parse "after d alias" $cj.Insert($pos + ", dbo.tCore_Data_0 d".Length, $inj)

$conn.Close()
