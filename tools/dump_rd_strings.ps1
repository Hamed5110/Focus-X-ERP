$dll = "C:\inetpub\wwwroot\Focus.RD.Web\bin\Focus.RD.BL.dll"
$b = [System.IO.File]::ReadAllBytes($dll)
# UTF-16LE strings
$uni = [System.Text.Encoding]::Unicode.GetString($b)
$needles = @(
  "TempTable", "GetPagewiseData", "iDate >=", "iDate <=", "OR iDate",
  "AND iDate", ") Temp", "where", "WHERE", "{13}", "{6}",
  "GetPagewiseDataLite", "iDate = 0", "Date Range"
)
foreach ($n in $needles) {
    $idx = 0
    $hits = 0
    Write-Output ("===== UNICODE [$n] =====")
    while ($hits -lt 8) {
        $idx = $uni.IndexOf($n, $idx)
        if ($idx -lt 0) { break }
        $hits++
        $a = [Math]::Max(0, $idx - 80)
        $chunk = $uni.Substring($a, [Math]::Min(220, $uni.Length - $a))
        $chunk = $chunk -replace "[^\u0020-\u007E]+", " "
        Write-Output ("@$idx : $chunk")
        $idx++
    }
    if ($hits -eq 0) { Write-Output "(none)" }
}

$asc = [System.Text.Encoding]::ASCII.GetString($b)
Write-Output ""
Write-Output "===== ASCII TempTable contexts ====="
$idx = 0
$hits = 0
while ($hits -lt 12) {
    $idx = $asc.IndexOf("TempTable", $idx)
    if ($idx -lt 0) { break }
    $hits++
    $a = [Math]::Max(0, $idx - 120)
    $chunk = $asc.Substring($a, [Math]::Min(300, $asc.Length - $a))
    $chunk = $chunk -replace "[^\x20-\x7E]+", " | "
    Write-Output ("@$idx : $chunk")
    $idx++
}

Write-Output ""
Write-Output "===== ASCII iDate contexts ====="
$idx = 0
$hits = 0
while ($hits -lt 20) {
    $idx = $asc.IndexOf("iDate", $idx)
    if ($idx -lt 0) { break }
    $hits++
    $a = [Math]::Max(0, $idx - 40)
    $chunk = $asc.Substring($a, [Math]::Min(120, $asc.Length - $a))
    $chunk = $chunk -replace "[^\x20-\x7E]+", " "
    if ($chunk -match "OR|AND|>=|Temp|where|WHERE|Select") {
        Write-Output ("@$idx : $chunk")
    }
    $idx++
}
