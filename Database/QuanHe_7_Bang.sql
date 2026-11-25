-- Migration không xóa dữ liệu: lực lượng quan hệ và chống trùng lịch của schema 7 bảng.
-- Chạy lại được. sysdiagrams (nếu có) không thuộc 7 bảng ứng dụng.
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF EXISTS (
    SELECT 1 FROM dbo.LuotLuuTru
    WHERE ConHieuLuc=1 AND ThoiDiemDi<=COALESCE(ThoiDiemNhanPhong,ThoiDiemDen)
)
    THROW 51010, N'Lượt còn hiệu lực có giờ trả không sau giờ nhận; cần xử lý dữ liệu trước migration.', 1;

IF EXISTS (
    SELECT 1 FROM dbo.LuotLuuTru a
    JOIN dbo.LuotLuuTru b ON a.MaLuotLuuTru<b.MaLuotLuuTru AND a.MaPhong=b.MaPhong
    WHERE a.ConHieuLuc=1 AND b.ConHieuLuc=1
      AND COALESCE(a.ThoiDiemNhanPhong,a.ThoiDiemDen)<b.ThoiDiemDi
      AND COALESCE(b.ThoiDiemNhanPhong,b.ThoiDiemDen)<a.ThoiDiemDi
)
    THROW 51011, N'Có hai lượt còn hiệu lực trùng phòng và chồng giờ; cần xử lý trước migration.', 1;

IF EXISTS (SELECT 1 FROM dbo.HoaDon GROUP BY MaLuotLuuTru HAVING COUNT(*)>1)
    THROW 51012, N'Một lượt có nhiều hóa đơn; cần xử lý trước migration.', 1;

IF EXISTS (
    SELECT 1 FROM dbo.HoaDon h LEFT JOIN dbo.LuotLuuTru s ON s.MaLuotLuuTru=h.MaLuotLuuTru
    WHERE s.MaLuotLuuTru IS NULL
)
    THROW 51013, N'Hóa đơn tham chiếu lượt lưu trú không tồn tại.', 1;

-- Chỉ gỡ UNIQUE không lọc trên đúng một khóa ngoại của bảng con. UNIQUE có
-- điều kiện (một phòng đang ở, một ca đang mở, một khoản mất cọc) được giữ.
DECLARE @drop nvarchar(max)=N'';
SELECT @drop=@drop+
    CASE WHEN kc.object_id IS NULL
         THEN N'DROP INDEX '+QUOTENAME(i.name)+N' ON dbo.'+QUOTENAME(t.name)+N';'
         ELSE N'ALTER TABLE dbo.'+QUOTENAME(t.name)+N' DROP CONSTRAINT '+QUOTENAME(kc.name)+N';' END
FROM sys.indexes i
JOIN sys.tables t ON t.object_id=i.object_id AND t.schema_id=SCHEMA_ID(N'dbo')
JOIN sys.index_columns ic ON ic.object_id=i.object_id AND ic.index_id=i.index_id
JOIN sys.columns c ON c.object_id=ic.object_id AND c.column_id=ic.column_id
LEFT JOIN sys.key_constraints kc ON kc.parent_object_id=i.object_id AND kc.unique_index_id=i.index_id
WHERE i.is_unique=1 AND i.is_primary_key=0 AND i.filter_definition IS NULL
  AND ic.key_ordinal=1 AND ic.is_included_column=0
  AND NOT EXISTS (SELECT 1 FROM sys.index_columns other
                  WHERE other.object_id=i.object_id AND other.index_id=i.index_id AND other.key_ordinal>1)
  AND ((t.name=N'LuotLuuTru' AND c.name=N'MaPhong')
    OR (t.name=N'CaTruc' AND c.name=N'MaNhanVien')
    OR (t.name=N'GiaoDichThanhToan' AND c.name=N'MaLuotLuuTru'));
IF @drop<>N'' EXEC sys.sp_executesql @drop;

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE parent_object_id=OBJECT_ID(N'dbo.LuotLuuTru') AND referenced_object_id=OBJECT_ID(N'dbo.Phong') AND name=N'FK_LuotLuuTru_Phong')
    ALTER TABLE dbo.LuotLuuTru WITH CHECK ADD CONSTRAINT FK_LuotLuuTru_Phong FOREIGN KEY(MaPhong) REFERENCES dbo.Phong(MaPhong);
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE parent_object_id=OBJECT_ID(N'dbo.CaTruc') AND referenced_object_id=OBJECT_ID(N'dbo.TaiKhoanNhanVien') AND name=N'FK_CaTruc_NhanVien')
    ALTER TABLE dbo.CaTruc WITH CHECK ADD CONSTRAINT FK_CaTruc_NhanVien FOREIGN KEY(MaNhanVien) REFERENCES dbo.TaiKhoanNhanVien(MaNhanVien);
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE parent_object_id=OBJECT_ID(N'dbo.GiaoDichThanhToan') AND referenced_object_id=OBJECT_ID(N'dbo.LuotLuuTru') AND name=N'FK_GiaoDich_LuotLuuTru')
    ALTER TABLE dbo.GiaoDichThanhToan WITH CHECK ADD CONSTRAINT FK_GiaoDich_LuotLuuTru FOREIGN KEY(MaLuotLuuTru) REFERENCES dbo.LuotLuuTru(MaLuotLuuTru);
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE parent_object_id=OBJECT_ID(N'dbo.HoaDon') AND referenced_object_id=OBJECT_ID(N'dbo.LuotLuuTru') AND name=N'FK_HoaDon_LuotLuuTru')
    ALTER TABLE dbo.HoaDon WITH CHECK ADD CONSTRAINT FK_HoaDon_LuotLuuTru FOREIGN KEY(MaLuotLuuTru) REFERENCES dbo.LuotLuuTru(MaLuotLuuTru);

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes i JOIN sys.index_columns ic ON ic.object_id=i.object_id AND ic.index_id=i.index_id
    JOIN sys.columns c ON c.object_id=ic.object_id AND c.column_id=ic.column_id
    WHERE i.object_id=OBJECT_ID(N'dbo.HoaDon') AND i.is_unique=1 AND i.filter_definition IS NULL
      AND c.name=N'MaLuotLuuTru' AND ic.key_ordinal=1
      AND NOT EXISTS (SELECT 1 FROM sys.index_columns other WHERE other.object_id=i.object_id AND other.index_id=i.index_id AND other.key_ordinal>1)
)
    CREATE UNIQUE INDEX UX_HoaDon_MaLuotLuuTru ON dbo.HoaDon(MaLuotLuuTru);

COMMIT;
GO

CREATE OR ALTER TRIGGER dbo.TR_LuotLuuTru_KhongTrungLich
ON dbo.LuotLuuTru
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS(SELECT 1 FROM inserted WHERE ConHieuLuc=1) RETURN;

    DECLARE @result int;
    EXEC @result=sys.sp_getapplock @Resource=N'QLKhachSan.BookingOverlap',
        @LockMode=N'Exclusive', @LockOwner=N'Transaction', @LockTimeout=10000;
    IF @result<0 THROW 51014, N'Không khóa được lịch phòng; hãy thử lại.', 1;

    IF EXISTS (SELECT 1 FROM inserted WHERE ConHieuLuc=1 AND ThoiDiemDi<=COALESCE(ThoiDiemNhanPhong,ThoiDiemDen))
        THROW 51010, N'Giờ trả phải sau giờ nhận.', 1;

    IF EXISTS (
        SELECT 1 FROM inserted i
        JOIN dbo.LuotLuuTru s ON s.MaPhong=i.MaPhong AND s.MaLuotLuuTru<>i.MaLuotLuuTru
        WHERE i.ConHieuLuc=1 AND s.ConHieuLuc=1
          AND COALESCE(i.ThoiDiemNhanPhong,i.ThoiDiemDen)<s.ThoiDiemDi
          AND COALESCE(s.ThoiDiemNhanPhong,s.ThoiDiemDen)<i.ThoiDiemDi
    )
        THROW 51011, N'Phòng đã có lượt còn hiệu lực chồng khoảng thời gian.', 1;
END;
GO
