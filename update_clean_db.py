with open('/sessions/dazzling-gracious-gates/mnt/QLKhachSan/Database/Database.sql', 'r', encoding='utf-8') as f:
    text = f.read()

# Make sure GiaoDichThanhToan is NOT in @LegacyTables
text = text.replace("(N'ChangLuuTru'),(N'GiaoDichThanhToan'),(N'NhatKyThaoTac'),", "(N'ChangLuuTru'),(N'NhatKyThaoTac'),")

# Add CREATE TABLE dbo.GiaoDichThanhToan if not present
giao_dich_table = """-- ============================================================================
-- 8. BẢNG GIAO DỊCH THANH TOÁN (TIỀN CỌC / THANH TOÁN CA TRỰC)
-- Khóa chính: MaGiaoDich
-- Khóa ngoại: MaLuotLuuTru -> LuotLuuTru, MaNhanVien -> TaiKhoanNhanVien, MaCaTruc -> CaTruc
-- ============================================================================
IF OBJECT_ID(N'dbo.GiaoDichThanhToan', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.GiaoDichThanhToan (
        MaGiaoDich bigint IDENTITY(1,1) CONSTRAINT PK_GiaoDichThanhToan PRIMARY KEY,
        Ma AS MaGiaoDich, -- Alias tương thích ngược
        MaLuotLuuTru bigint NOT NULL CONSTRAINT FK_GiaoDich_LuotLuuTru REFERENCES dbo.LuotLuuTru(MaLuotLuuTru),
        LoaiGiaoDich varchar(20) NOT NULL CHECK(LoaiGiaoDich IN('Deposit','Checkout','Refund','Forfeit')),
        SoTien decimal(18,2) NOT NULL CHECK(SoTien>0),
        ThoiDiemTao datetime2 NOT NULL DEFAULT SYSDATETIME(),
        PhuongThucThanhToan nvarchar(30) NOT NULL,
        GhiChu nvarchar(300) NOT NULL DEFAULT N'',
        MaNhanVien int NOT NULL CONSTRAINT FK_GiaoDich_NhanVien REFERENCES dbo.TaiKhoanNhanVien(MaNhanVien),
        MaNguoiTao AS MaNhanVien, -- Alias tương thích ngược
        MaCaTruc bigint NULL CONSTRAINT FK_GiaoDich_CaTruc REFERENCES dbo.CaTruc(MaCaTruc),
        MaThamChieuNgoai nvarchar(100) NULL
    );
    CREATE INDEX IX_Payments_Shift ON dbo.GiaoDichThanhToan(MaCaTruc) WHERE MaCaTruc IS NOT NULL;
END;
GO
"""

if "CREATE TABLE dbo.GiaoDichThanhToan" not in text:
    # Insert right before -- DỮ LIỆU MẪU CỐT LÕI
    text = text.replace("-- ============================================================================\n-- DỮ LIỆU MẪU CỐT LÕI", giao_dich_table + "\n-- ============================================================================\n-- DỮ LIỆU MẪU CỐT LÕI")

with open('/sessions/dazzling-gracious-gates/mnt/QLKhachSan/Database/Database.sql', 'w', encoding='utf-8') as f:
    f.write(text)

print("GiaoDichThanhToan ensured in Database.sql!")
