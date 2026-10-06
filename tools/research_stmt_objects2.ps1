$cs = "Server=localhost;Database=Focus80G0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
function Dump($sql, $title) {
    Write-Output ""
    Write-Output "========== $title =========="
    $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 180; $cmd.CommandText = $sql
    try {
        $r = $cmd.ExecuteReader()
        $n = $r.FieldCount
        $hdr = @(); for ($i=0; $i -lt $n; $i++) { $hdr += $r.GetName($i) }
        Write-Output ($hdr -join " | ")
        $c = 0
        while ($r.Read()) {
            $c++
            $parts = @()
            for ($i=0; $i -lt $n; $i++) {
                if ($r.IsDBNull($i)) { $parts += "NULL" } else {
                    $v = [string]$r.GetValue($i)
                    $v = $v -replace "`r|`n"," "
                    if ($v.Length -gt 500) { $v = $v.Substring(0,500) + "..." }
                    $parts += $v
                }
            }
            Write-Output ($parts -join " | ")
            if ($c -ge 60) { Write-Output "(truncated)"; break }
        }
        $r.Close()
        if ($c -eq 0) { Write-Output "(no rows)" }
    } catch {
        Write-Output ("ERROR: " + $_.Exception.Message)
        try { $r.Close() } catch {}
    }
}

Dump @"
SELECT name
FROM sys.views
WHERE name LIKE '%Statement%'
   OR name LIKE '%Balance%'
   OR name LIKE '%DataFA%'
   OR name LIKE '%Customer%'
   OR name LIKE '%AccountBal%'
   OR name LIKE '%DrCr%'
   OR name LIKE '%Outstanding%'
   OR name LIKE '%Ledger%'
ORDER BY name
"@ "views name match statement/FA/balance"

Dump @"
SELECT TABLE_NAME, COLUMN_NAME, DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME IN (
  'vtCode_DataFA_0','vCore_AccountBalances_0','vaCore_Account',
  'tCore_Header_0','tCore_Data_0','mCore_Account'
)
  AND COLUMN_NAME IN ('iDate','Debit','Credit','Balance','iMasterId','iBookNo','iCode','fNet')
ORDER BY TABLE_NAME, ORDINAL_POSITION
"@ "key columns on core tables/views"

Dump @"
SELECT
  CAST(dbo.IntToDate(0) AS varchar(30)) AS IntToDate_0,
  CAST(dbo.IntToDate(132778241) AS varchar(30)) AS IntToDate_1Sep2026,
  dbo.DateToInt(CONVERT(datetime,'20260901',112)) AS DateToInt_1Sep2026,
  132778241 AS Packed_1Sep2026,
  CONVERT(varchar(12), 132778241, 112) AS Convert112_on_packed_int
"@ "TEST IntToDate DateToInt CONVERT112"

Dump @"
SELECT
  CAST((132778241 & 0xfff0000) / 65536 AS varchar(4))
  + '-' + RIGHT('0' + CAST((132778241 & 0xff00) / 256 AS varchar(2)), 2)
  + '-' + RIGHT('0' + CAST((132778241 & 0xff) AS varchar(2)), 2) AS BitmaskUnpack
"@ "TEST 70268 bitmask unpack"

Dump @"
SELECT TOP 3
  v.sVoucherNo,
  v.iDate AS PackedDate,
  CAST(dbo.IntToDate(v.iDate) AS varchar(30)) AS IntToDateVal,
  CAST((v.iDate & 0xfff0000) / 65536 AS varchar(4))
    + '-' + RIGHT('0' + CAST((v.iDate & 0xff00) / 256 AS varchar(2)), 2)
    + '-' + RIGHT('0' + CAST((v.iDate & 0xff) AS varchar(2)), 2) AS Unpack,
  v.Debit, v.Credit
FROM dbo.vtCode_DataFA_0 v
WHERE v.iMasterId = 17500 AND v.iDate > 0
ORDER BY v.iDate
"@ "TEST FA view Hussain 17500 vs IntToDate"

Dump @"
SELECT iMasterId, sCode, sName
FROM dbo.mCore_Account
WHERE sCode = 'AC-6993' OR sName LIKE '%Hussain Alsaleem%'
"@ "TEST Hussain account id"
