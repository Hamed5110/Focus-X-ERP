$cs = "Server=localhost;Database=Focus80E0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$decl = @"
DECLARE @iStartDate INT = dbo.DateToInt(CONVERT(datetime,'20260801',112));
DECLARE @iEndDate INT = dbo.DateToInt(CONVERT(datetime,'20260831',112));
"@
$detail = $decl + (Get-Content -Raw "C:\Users\Hamed Ali Khan\Focus-X-ERP\reports\sql\Monthly Sales Commission - Atlas Aluminum Detail.sql")
$summary = $decl + (Get-Content -Raw "C:\Users\Hamed Ali Khan\Focus-X-ERP\reports\sql\Monthly Sales Commission - Atlas Aluminum.sql")
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 180

function Get-Rows($sql) {
    $cmd.CommandText = $sql
    $r = $cmd.ExecuteReader()
    $cols = @(); for ($i=0; $i -lt $r.FieldCount; $i++) { $cols += $r.GetName($i) }
    $rows = @()
    while ($r.Read()) {
        $o = [ordered]@{}
        foreach ($c in $cols) { $o[$c] = $r[$c] }
        $rows += [pscustomobject]$o
    }
    $r.Close()
    ,$rows
}

$d = Get-Rows $detail
$s = Get-Rows $summary
Write-Output ("DETAIL COLS: " + (($d[0].PSObject.Properties.Name) -join " | "))
Write-Output ("DETAIL ROWS=$($d.Count)  SUMMARY ROWS=$($s.Count)")
Write-Output ""
Write-Output "=== CUSTOMER ROWS ==="
$d | Sort-Object Salesman, "Customer Name" | ForEach-Object {
    Write-Output ("{0,-28} {1,-36} C={2,10:N2} Coll={3,8:N2} Life={4,8:N2} {5,6:N1}% Gate={6,-3} CE={7,8:N2} E={8,8:N2} NE={9,8:N2}" -f `
        $_.Salesman, $_."Customer Name", $_."Total Contract Amount", $_."Total Collection", `
        $_."Lifetime Collection", $_."Collection %", $_."Gate Status", `
        $_."Collection Eligible Amount", $_."Eligible Amount", $_."Not Eligible Amount")
}

Write-Output ""
Write-Output "=== SUM DETAIL vs SUMMARY (TRUE MODE) ==="
$ok = $true
foreach ($sm in ($s | ForEach-Object { $_.Salesman } | Sort-Object -Unique)) {
    $dr = @($d | Where-Object { $_.Salesman -eq $sm })
    $sr = @($s | Where-Object { $_.Salesman -eq $sm })[0]
    $dC = ($dr | Measure-Object "Total Contract Amount" -Sum).Sum
    $dColl = ($dr | Measure-Object "Total Collection" -Sum).Sum
    $dCE = ($dr | Measure-Object "Collection Eligible Amount" -Sum).Sum
    $dE = ($dr | Measure-Object "Eligible Amount" -Sum).Sum
    $dNE = ($dr | Measure-Object "Not Eligible Amount" -Sum).Sum
    $chk = {
        param($a,$b,$tol=0.02)
        [math]::Abs([double]$a - [double]$b) -le $tol
    }
    $pass = (& $chk $dC $sr."Total Contract Amount") -and (& $chk $dColl $sr."Total Collection") -and `
            (& $chk $dCE $sr."Collection Eligible Amount") -and (& $chk $dE $sr."Eligible Amount") -and `
            (& $chk $dNE $sr."Not Eligible Amount")
    if (-not $pass) { $ok = $false }
    Write-Output ("{0,-28} match={1}  dC={2:N2}/{3:N2} dColl={4:N2}/{5:N2} dCE={6:N2}/{7:N2} dE={8:N2}/{9:N2} dNE={10:N2}/{11:N2}" -f `
        $sm, $pass, $dC, $sr."Total Contract Amount", $dColl, $sr."Total Collection", `
        $dCE, $sr."Collection Eligible Amount", $dE, $sr."Eligible Amount", $dNE, $sr."Not Eligible Amount")
}
$dOv = ($d | Measure-Object "Overall Sales" -Sum).Sum
$sOv = ($s | Measure-Object "Overall Sales" -Sum).Sum
$yes = @($d | Where-Object { $_."Gate Status" -eq "Yes" }).Count
$no  = @($d | Where-Object { $_."Gate Status" -eq "No" }).Count
Write-Output ("Overall Sales detail={0:N2} summary={1:N2}" -f $dOv, $sOv)
Write-Output ("Gate Yes={0} No={1}  TEAM match={2}" -f $yes, $no, $ok)
$conn.Close()
