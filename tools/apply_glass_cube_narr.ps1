$cs = "Server=localhost;Database=Focus8080;User Id=sa;Password=$env:FOCUS_SQL_PASSWORD;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$cmd = $conn.CreateCommand()
$cmd.CommandTimeout = 60

function Scalar($sql) {
    $c = $conn.CreateCommand(); $c.CommandText = $sql
    $v = $c.ExecuteScalar()
    if ($v -eq $null -or $v -is [DBNull]) { return $null }
    return $v
}

$hasNarr = Scalar "SELECT COUNT(*) FROM dbo.cCore_ReportColumns_0 WHERE iLayoutId = 6899 AND iFieldId = 17077222"
Write-Output "narration columns on 70252 layout: $hasNarr"

if ([int]$hasNarr -eq 0) {
    $cmd.CommandText = @"
INSERT INTO dbo.cCore_ReportColumns_0
    (iLayoutId, iFieldId, sFormula, iType, sColumn, sAliasName, sGroupName, fColumnWidth, iMiscOption, iAlignment, byFont, iParentId, iSubParentId, sToolTip, sFormat, iDecimalInColumn)
VALUES
    (6899, 17077222, N'', 0, N'Extra Fields.Narration', N'Narration', N'', 200, 64, 0, NULL, 0, 0, N'', N'', 0)
"@
    [void]$cmd.ExecuteNonQuery()
    Write-Output "inserted Narration column"
} else {
    Write-Output "Narration already present"
}

$params = @(
    @{ Name = 'Vendor AC'; Var = '@VendorAC'; Ctrl = 1 },
    @{ Name = 'Job Order'; Var = '@JobOrder'; Ctrl = 3010 },
    @{ Name = 'Delivery Status'; Var = '@DeliveryStatus'; Ctrl = 3080 }
)
foreach ($p in $params) {
    $n = Scalar ("SELECT COUNT(*) FROM dbo.cCore_ReportParameter_0 WHERE iReportId = 70252 AND sFieldVariable = N'{0}'" -f $p.Var)
    Write-Output ("param {0}: {1}" -f $p.Var, $n)
    if ([int]$n -eq 0) {
        $ins = $conn.CreateCommand()
        $ins.CommandText = @"
INSERT INTO dbo.cCore_ReportParameter_0
    (iReportId, sFieldName, sFieldVariable, iControlType, iFieldType, iSelectionMode, sValue, iFieldId, iSubParentId, iType, sDefault, bGroup, bDefault)
VALUES
    (70252, @name, @var, @ctrl, 1, 0, N'', 1, NULL, NULL, N'', 0, 0)
"@
        [void]$ins.Parameters.AddWithValue('@name', $p.Name)
        [void]$ins.Parameters.AddWithValue('@var', $p.Var)
        [void]$ins.Parameters.AddWithValue('@ctrl', $p.Ctrl)
        [void]$ins.ExecuteNonQuery()
        Write-Output ("inserted {0}" -f $p.Var)
    }
}

Write-Output ""
Write-Output "========== 70252 columns =========="
$cmd.CommandText = @"
SELECT iColumnId, iFieldId, sColumn, sAliasName, iType, iMiscOption
FROM dbo.cCore_ReportColumns_0 WHERE iLayoutId = 6899 ORDER BY iColumnId
"@
$r = $cmd.ExecuteReader()
while ($r.Read()) { Write-Output ("{0} | {1} | {2} | {3}" -f $r[0], $r[1], $r[2], $r[3]) }
$r.Close()

Write-Output ""
Write-Output "========== 70252 params =========="
$cmd.CommandText = "SELECT iParameterId, sFieldName, sFieldVariable, iControlType FROM dbo.cCore_ReportParameter_0 WHERE iReportId = 70252"
$r = $cmd.ExecuteReader()
while ($r.Read()) { Write-Output ("{0} | {1} | {2} | {3}" -f $r[0], $r[1], $r[2], $r[3]) }
$r.Close()
$conn.Close()
