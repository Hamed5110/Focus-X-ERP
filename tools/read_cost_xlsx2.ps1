$path = "C:\Users\Hamed Ali Khan\Documents\Cost Reduction 2026.xlsx"
$excel = New-Object -ComObject Excel.Application
$excel.Visible = $false
$excel.DisplayAlerts = $false
$wb = $excel.Workbooks.Open($path)
$ws = $wb.Worksheets.Item(1)
Write-Output "---- ROW1 all non-empty ----"
for ($c = 1; $c -le 67; $c++) {
    $v = [string]$ws.Cells.Item(1, $c).Text
    if ($v -ne "") { Write-Output ("C{0}={1}" -f $c, $v) }
}
Write-Output "---- ROW2 all non-empty ----"
for ($c = 1; $c -le 67; $c++) {
    $v = [string]$ws.Cells.Item(2, $c).Text
    if ($v -ne "") { Write-Output ("C{0}={1}" -f $c, $v) }
}
Write-Output "---- ROW3 leftover cols ----"
for ($c = 20; $c -le 67; $c++) {
    $v = [string]$ws.Cells.Item(3, $c).Text
    if ($v -ne "") { Write-Output ("C{0}={1}" -f $c, $v) }
}
$wb.Close($false)
$excel.Quit()
[System.Runtime.Interopservices.Marshal]::ReleaseComObject($wb) | Out-Null
[System.Runtime.Interopservices.Marshal]::ReleaseComObject($excel) | Out-Null
