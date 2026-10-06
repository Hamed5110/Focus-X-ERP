$cs = "Server=localhost;Database=Focus80G0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
$cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 60
$cmd.CommandText = @"
SELECT h.iHeaderId, h.iVoucherType, h.sVoucherNo, h.fNet,
  CAST((h.iDate & 0xfff0000)/65536 AS varchar(4)) + N'-'
    + RIGHT(N'0'+CAST((h.iDate & 0xff00)/256 AS varchar(2)),2) + N'-'
    + RIGHT(N'0'+CAST((h.iDate & 0xff) AS varchar(2)),2) AS Dt,
  d.iCode, d.iBookNo, d.iFaTag
FROM dbo.tCore_Header_0 h
JOIN dbo.tCore_Data_0 d ON d.iHeaderId=h.iHeaderId
WHERE h.sVoucherNo LIKE N'SQ-Atl-12089%'
"@
$r = $cmd.ExecuteReader()
$n=$r.FieldCount; $h=@(); for($i=0;$i -lt $n;$i++){ $h += $r.GetName($i) }
Write-Output ($h -join " | ")
while($r.Read()){
  $p=@(); for($i=0;$i -lt $n;$i++){ if($r.IsDBNull($i)){$p+=""}else{$p+=[string]$r.GetValue($i)} }
  Write-Output ($p -join " | ")
}
$r.Close()
if (-not $h) { Write-Output "(no quote rows)" }

$cmd.CommandText = @"
SELECT vac.iMasterId, vac.Salesmanname, sm.sName
FROM dbo.vaCore_Account vac
LEFT JOIN dbo.mCore_Salesman sm ON sm.iMasterId = vac.Salesmanname
WHERE vac.iMasterId = 21061 AND vac.iTreeId = 0
"@
$r = $cmd.ExecuteReader()
Write-Output "---- account salesman now ----"
while($r.Read()){ Write-Output ("{0} sm={1} {2}" -f $r[0], $r[1], $r[2]) }
$r.Close(); $conn.Close()
