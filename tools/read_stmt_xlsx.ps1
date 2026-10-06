$path = "C:\Users\Hamed Ali Khan\Downloads\Mr__Ahmed_Yusuf_Alawainati___M_539123_Statement_of_Customer_539123_ATLAS_ALUMINUM_W_L_L_.xlsx"
if (-not (Test-Path $path)) { Write-Output "MISSING: $path"; Get-ChildItem "C:\Users\Hamed Ali Khan\Downloads\*Statement*" | Select-Object Name, FullName; exit 1 }
Write-Output ("SIZE=" + (Get-Item $path).Length)

$excel = New-Object -ComObject Excel.Application
$excel.Visible = $false
$excel.DisplayAlerts = $false
$wb = $excel.Workbooks.Open($path)
Write-Output ("SHEETS=" + $wb.Worksheets.Count)
foreach ($ws in $wb.Worksheets) {
    $ur = $ws.UsedRange
    $rc = $ur.Rows.Count
    $cc = $ur.Columns.Count
    Write-Output ("==== SHEET: " + $ws.Name + " used=" + $rc + "x" + $cc + " ====")
    $rows = [Math]::Min($rc, 60)
    $cols = [Math]::Min($cc, 30)
    for ($r = 1; $r -le $rows; $r++) {
        $parts = @()
        for ($c = 1; $c -le $cols; $c++) {
            $v = $ws.Cells.Item($r, $c).Text
            if ($null -eq $v) { $v = "" }
            $v = [string]$v
            $v = $v.Replace("`r", " ").Replace("`n", " ").Trim()
            if ($v -ne "") { $parts += ("{0}:{1}" -f $c, $v) }
        }
        if ($parts.Count -gt 0) { Write-Output ("R{0} | {1}" -f $r, ($parts -join " || ")) }
    }
}
$wb.Close($false)
$excel.Quit()
[System.Runtime.Interopservices.Marshal]::ReleaseComObject($wb) | Out-Null
[System.Runtime.Interopservices.Marshal]::ReleaseComObject($excel) | Out-Null
