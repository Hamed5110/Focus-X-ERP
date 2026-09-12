$cs = "Server=localhost;Database=Focus80E0;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;"
$conn = New-Object System.Data.SqlClient.SqlConnection $cs
$conn.Open()
function Dump($sql, $title) {
    Write-Output ""
    Write-Output "========== $title =========="
    $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 120; $cmd.CommandText = $sql
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
                if ($r.IsDBNull($i)) { $parts += "" } else { $parts += [string]$r.GetValue($i) }
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

Dump "SELECT iMasterId, sCode, sName, iStatus FROM dbo.mCore_typeofadvances ORDER BY iMasterId" "typeofadvances"
Dump "SELECT iMasterId, sCode, sName, iStatus FROM dbo.mCore_otherpayments ORDER BY iMasterId" "otherpayments"
Dump "SELECT iMasterId, sCode, sName FROM dbo.mCore_advancereceiptsno ORDER BY iMasterId" "advancereceiptsno"

# search First Payment / CI- across likely text columns
$tables = @('mCore_typeofadvances','mCore_otherpayments','mCore_Type','mCore_commissiontype')
foreach ($t in $tables) {
    Dump "SELECT iMasterId, sCode, sName FROM dbo.$t WHERE sName LIKE N'%Payment%' OR sCode LIKE N'%CI%' OR sName LIKE N'%CI%'" "$t filter"
}

Dump @"
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME LIKE 'mCore_%' AND TABLE_NAME NOT LIKE '%Language%' AND TABLE_NAME NOT LIKE '%Tree%'
"@ "all mCore"

$conn.Close()
