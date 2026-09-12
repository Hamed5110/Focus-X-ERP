$rows = Import-Csv 'C:\Users\Hamed Ali Khan\Focus-X-ERP\tools\diag_status3.csv'
Write-Host '--- flip-4 in LOCAL diagnostic CSV ---'
$rows | Where-Object { $_.'Report Status' -match 'Abdulrahman Abdullah|Ahmed Darraj|Sq-atl-924|Sq-atl-939|Sayed Noaman' } | Format-Table -AutoSize | Out-String -Width 200 | Write-Host
