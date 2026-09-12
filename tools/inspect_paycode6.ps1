$cs = "Server=localhost;Database=Focus80E0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
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
                if ($r.IsDBNull($i)) { $parts += "" } else {
                    $v = [string]$r.GetValue($i)
                    if ($v.Length -gt 160) { $v = $v.Substring(0,160) + "..." }
                    $parts += $v
                }
            }
            Write-Output ($parts -join " | ")
            if ($c -ge 80) { break }
        }
        $r.Close()
        if ($c -eq 0) { Write-Output "(no rows)" }
    } catch {
        Write-Output ("ERROR: " + $_.Exception.Message)
    }
}

Dump @"
SELECT sFieldName, sCaption, sAPIName, iDisplayControlType, sExternalTableName, sExternalValueMember, sExternalDisplayMember, iLinkType, iLinkTypeId
FROM dbo.cCore_FormFields
WHERE sFieldName LIKE N'%Payment%' OR sCaption LIKE N'%Payment%' OR sAPIName LIKE N'%Payment%'
"@ "FormFields Payment"

Dump @"
SELECT TOP 15
    h.sVoucherNo, hd.PaymentCode AS HdCode, v.PaymentCode AS VwCode,
    hd.TotalContractAmt, hd.OpportunityType
FROM dbo.tCore_Header_0 h
INNER JOIN dbo.tCore_HeaderData4610_0 hd ON hd.iHeaderId=h.iHeaderId
LEFT JOIN dbo.vCore_TranData_0 v ON v.iHeaderId=h.iHeaderId
WHERE h.iVoucherType=4610 AND ISNULL(hd.PaymentCode,0) > 0
ORDER BY h.iHeaderId DESC
"@ "ATIC header vs view PaymentCode"

Dump @"
SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME='vCore_TranData_0' AND COLUMN_NAME LIKE '%Payment%'
"@ "view payment cols"

Dump @"
SELECT iFieldId, sCaption FROM dbo.cCore_FieldsLanguage_0
WHERE sCaption LIKE N'%Payment%'
"@ "field captions"

$conn.Close()
