$path = "C:\Users\Hamed Ali Khan\Documents\Cost Reduction 2026.xlsx"
$excel = New-Object -ComObject Excel.Application
$excel.Visible = $false
$excel.DisplayAlerts = $false
$wb = $excel.Workbooks.Open($path)
foreach ($ws in $wb.Worksheets) {
    Write-Output ("==== SHEET: " + $ws.Name + " used=" + $ws.UsedRange.Rows.Count + "x" + $ws.UsedRange.Columns.Count + " ====")
    $rows = [Math]::Min($ws.UsedRange.Rows.Count, 80)
    $cols = [Math]::Min($ws.UsedRange.Columns.Count, 20)
    for ($r = 1; $r -le $rows; $r++) {
        $parts = @()
        for ($c = 1; $c -le $cols; $c++) {
            $v = $ws.Cells.Item($r, $c).Text
            if ($null -eq $v) { $v = "" }
            $v = [string]$v
            if ($v -ne "") { $parts += ("{0}:{1}" -f $c, $v.Replace("`r", " ").Replace("`n", " ")) }
        }
        if ($parts.Count -gt 0) { Write-Output ("R{0} | {1}" -f $r, ($parts -join " || ")) }
    }
}
$wb.Close($false)
$excel.Quit()
[System.Runtime.Interopservices.Marshal]::ReleaseComObject($wb) | Out-Null
[System.Runtime.Interopservices.Marshal]::ReleaseComObject($excel) | Out-Null
