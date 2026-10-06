$cs = "Server=localhost;Database=Focus80G0;User Id=sa;Password=$env:FOCUS_SQL_PASSWORD;Encrypt=False;TrustServerCertificate=True;"
$q = [System.IO.File]::ReadAllText("C:\Users\Hamed Ali Khan\Focus-X-ERP\reports\sql\Project Tracking III Report Atlas Ledger.sql")
$start = 2021*65536+256+1
$end = [int](Get-Date).Year*65536+[int](Get-Date).Month*256+[int](Get-Date).Day
$inj = " AND iDate >= $start AND iDate <= $end OR iDate = 0 "
$run = $q.Replace("@CustomerName","0").Replace("@iStartDate","$start").Replace("@iEndDate","$end")

$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
function Parse($title, $sql) {
    $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 15
    $cmd.CommandText = "SET PARSEONLY ON;`n$sql`nSET PARSEONLY OFF;"
    try {
        [void]$cmd.ExecuteNonQuery()
        Write-Output ("PASS  " + $title)
        return $true
    } catch {
        $msg = $_.Exception.Message
        $msg = $msg -replace "`r|`n"," "
        if ($msg.Length -gt 180) { $msg = $msg.Substring(0,180) }
        Write-Output ("FAIL  $title :: $msg")
        return $false
    }
}

Write-Output "===== baseline ====="
Parse "plain" $run | Out-Null

Write-Output ""
Write-Output "===== wrap whole query ====="
Parse "FROM (sql) z WHERE iDate wrap" "SELECT * FROM (`n$run`n) z WHERE iDate >= $start AND iDate <= $end OR iDate = 0"
Parse "FROM (sql) z AND iDate wrap" "SELECT * FROM (`n$run`n) z AND iDate >= $start AND iDate <= $end OR iDate = 0"
Parse "sql + AND iDate append" ($run + "`n" + $inj)
Parse "FROM (sql) AND iDate no alias" "SELECT * FROM (`n$run`n) AND iDate >= $start AND iDate <= $end OR iDate = 0"

Write-Output ""
Write-Output "===== first WHERE insert ====="
$m = [regex]::Match($run, '(?i)\bWHERE\b')
if ($m.Success) {
    $a = $run.Insert($m.Index + $m.Length, $inj)
    Parse ("first WHERE @" + $m.Index) $a | Out-Null
}

Write-Output ""
Write-Output "===== ) word  (all matches) ====="
$rx = [regex]'(?i)\)\s+([A-Za-z_][A-Za-z0-9_]*)'
$ms = $rx.Matches($run)
Write-Output ("count=" + $ms.Count)
$seen = @{}
$n = 0
foreach ($hit in $ms) {
    $tok = $hit.Groups[1].Value
    $key = $tok + "@" + $hit.Index
    if ($seen.ContainsKey($tok) -and $seen[$tok] -ge 2 -and $tok -eq 'AS') { continue }
    if (-not $seen.ContainsKey($tok)) { $seen[$tok] = 0 }
    $seen[$tok]++
    $n++
    if ($n -gt 40) { Write-Output "... stop after 40"; break }
    $pos = $hit.Index + $hit.Length
    $sql2 = $run.Insert($pos, $inj)
    Parse (") $tok @" + $hit.Index) $sql2 | Out-Null
}

Write-Output ""
Write-Output "===== ) word ON  ====="
$rx2 = [regex]'(?i)\)\s+([A-Za-z_][A-Za-z0-9_]*)\s+ON\b'
$ms2 = $rx2.Matches($run)
Write-Output ("count=" + $ms2.Count)
foreach ($hit in $ms2) {
    $pos = $hit.Groups[1].Index + $hit.Groups[1].Length
    $sql2 = $run.Insert($pos, $inj)
    Parse (") $($hit.Groups[1].Value) ON @" + $hit.Index) $sql2 | Out-Null
}

Write-Output ""
Write-Output "===== word ON  (JOIN ON) ====="
$rx3 = [regex]'(?i)(\b[A-Za-z_][A-Za-z0-9_]*)\s+ON\b'
$ms3 = $rx3.Matches($run)
Write-Output ("count=" + $ms3.Count)
foreach ($hit in $ms3) {
    $pos = $hit.Index + $hit.Length
    $sql2 = $run.Insert($pos, $inj)
    Parse ("$($hit.Groups[1].Value) ON @" + $hit.Index) $sql2 | Out-Null
}

$conn.Close()
