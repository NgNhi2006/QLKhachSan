$ErrorActionPreference = 'Stop'
$connection = [System.Data.SqlClient.SqlConnection]::new('Server=.\SQLEXPRESS;Database=QLKhachSanApp;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;Connect Timeout=8;Application Name=QLKSReportRead')
$connection.Open()
function Query([string]$sql) {
    $command = $connection.CreateCommand()
    $command.CommandText = $sql
    $table = [System.Data.DataTable]::new()
    $adapter = [System.Data.SqlClient.SqlDataAdapter]::new($command)
    [void]$adapter.Fill($table)
    $rows = @()
    foreach ($row in $table.Rows) {
        $entry = [ordered]@{}
        foreach ($column in $table.Columns) { $entry[$column.ColumnName] = if ($row[$column] -is [DBNull]) { $null } else { $row[$column] } }
        $rows += $entry
    }
    return ,$rows
}
$result = [ordered]@{}
$result.columns = Query @'
SELECT t.name AS [table], c.column_id AS ordinal, c.name AS [column],
  ty.name AS base_type, c.max_length, c.precision, c.scale, c.is_nullable, c.is_identity,
  dc.definition AS default_definition,
  CASE WHEN pk.column_id IS NULL THEN 0 ELSE 1 END AS is_pk,
  fk.ref_table AS fk_table, fk.ref_column AS fk_column
FROM sys.tables t
JOIN sys.columns c ON c.object_id=t.object_id
JOIN sys.types ty ON ty.user_type_id=c.user_type_id
LEFT JOIN sys.default_constraints dc ON dc.object_id=c.default_object_id
LEFT JOIN (
  SELECT ic.object_id, ic.column_id FROM sys.indexes i JOIN sys.index_columns ic ON ic.object_id=i.object_id AND ic.index_id=i.index_id WHERE i.is_primary_key=1
) pk ON pk.object_id=t.object_id AND pk.column_id=c.column_id
LEFT JOIN (
  SELECT fkc.parent_object_id, fkc.parent_column_id, rt.name AS ref_table, rc.name AS ref_column
  FROM sys.foreign_key_columns fkc JOIN sys.tables rt ON rt.object_id=fkc.referenced_object_id
  JOIN sys.columns rc ON rc.object_id=fkc.referenced_object_id AND rc.column_id=fkc.referenced_column_id
) fk ON fk.parent_object_id=t.object_id AND fk.parent_column_id=c.column_id
WHERE t.is_ms_shipped=0 AND t.name<>'sysdiagrams'
ORDER BY t.name,c.column_id
'@
$result.foreign_keys = Query @'
SELECT fk.name, pt.name AS parent_table, pc.name AS parent_column, rt.name AS ref_table, rc.name AS ref_column, fk.delete_referential_action_desc AS on_delete
FROM sys.foreign_keys fk JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id=fk.object_id
JOIN sys.tables pt ON pt.object_id=fkc.parent_object_id JOIN sys.columns pc ON pc.object_id=pt.object_id AND pc.column_id=fkc.parent_column_id
JOIN sys.tables rt ON rt.object_id=fkc.referenced_object_id JOIN sys.columns rc ON rc.object_id=rt.object_id AND rc.column_id=fkc.referenced_column_id
WHERE pt.is_ms_shipped=0 ORDER BY pt.name,fk.name,fkc.constraint_column_id
'@
$result.checks = Query @'
SELECT t.name AS [table], cc.name, cc.definition FROM sys.check_constraints cc JOIN sys.tables t ON t.object_id=cc.parent_object_id
WHERE t.is_ms_shipped=0 ORDER BY t.name,cc.name
'@
$result.indexes = Query @'
SELECT t.name AS [table], i.name, i.is_unique, i.has_filter, i.filter_definition, STRING_AGG(c.name,', ') WITHIN GROUP (ORDER BY ic.key_ordinal) AS [columns]
FROM sys.indexes i JOIN sys.tables t ON t.object_id=i.object_id JOIN sys.index_columns ic ON ic.object_id=i.object_id AND ic.index_id=i.index_id
JOIN sys.columns c ON c.object_id=i.object_id AND c.column_id=ic.column_id
WHERE t.is_ms_shipped=0 AND i.is_primary_key=0 AND i.type_desc<>'HEAP' AND ic.key_ordinal>0
GROUP BY t.name,i.name,i.is_unique,i.has_filter,i.filter_definition ORDER BY t.name,i.name
'@
$result.views = Query 'SELECT name FROM sys.views WHERE is_ms_shipped=0 ORDER BY name'
$result.counts = Query @'
SELECT 'Phong' AS [table],COUNT_BIG(*) AS [count] FROM dbo.Phong
UNION ALL SELECT 'KhachHang',COUNT_BIG(*) FROM dbo.KhachHang
UNION ALL SELECT 'LuotLuuTru',COUNT_BIG(*) FROM dbo.LuotLuuTru
UNION ALL SELECT 'DichVu',COUNT_BIG(*) FROM dbo.DichVu
UNION ALL SELECT 'YeuCauDichVu',COUNT_BIG(*) FROM dbo.YeuCauDichVu
UNION ALL SELECT 'HoaDon',COUNT_BIG(*) FROM dbo.HoaDon
UNION ALL SELECT 'GiaoDichThanhToan',COUNT_BIG(*) FROM dbo.GiaoDichThanhToan
UNION ALL SELECT 'CaTruc',COUNT_BIG(*) FROM dbo.CaTruc
'@
$result.room_status = Query 'SELECT TrangThai,COUNT_BIG(*) AS SoPhong FROM dbo.Phong GROUP BY TrangThai ORDER BY TrangThai'
$result.stay_status = Query 'SELECT TrangThai,COUNT_BIG(*) AS SoLuot FROM dbo.LuotLuuTru GROUP BY TrangThai ORDER BY TrangThai'
$result.db_version = Query 'SELECT MAX(PhienBan) AS PhienBan FROM dbo.PhienBanCSDL'
$connection.Close()
$result | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath 'D:\WorkSpace\QLKhachSan\tmp-build\schema_report.json' -Encoding utf8
