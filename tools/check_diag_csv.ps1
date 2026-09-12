$rows = Import-Csv 'C:\Users\Hamed Ali Khan\Focus-X-ERP\tools\diag_status3.csv'
Write-Host ('Total rows: ' + $rows.Count)
Write-Host '--- the 2 accounts by id ---'
$rows | Where-Object { $_.'No. of Accounts' -in @('12606','15268') } | Format-Table -AutoSize | Out-String -Width 200 | Write-Host
Write-Host '--- names containing Jaffar or Hussain Ali ---'
$rows | Where-Object { $_.'Report Status' -match 'Jaffar|Hussain Ali' } | Format-Table -AutoSize | Out-String -Width 200 | Write-Host
