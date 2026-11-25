import os

sql_content = """-- ============================================================================
-- HỆ THỐNG CƠ SỞ DỮ LIỆU QUẢN LÝ KHÁCH SẠN (QLKhachSanApp)
-- Cấu trúc chuẩn hóa: 8 bảng cốt lõi (3NF), khóa chính rõ ràng, hỗ trợ SSMS Diagram
-- ============================================================================

SET NOCOUNT ON;
USE master;
GO
IF DB_ID(N'QLKhachSanApp') IS NULL CREATE DATABASE QLKhachSanApp;
GO
USE QLKhachSanApp;
GO

-- Cấu hình Database Owner để hỗ trợ đầy đủ sơ đồ Database Diagram trong SSMS
BEGIN TRY
    ALTER AUTHORIZATION ON DATABASE::[QLKhachSanApp] TO [sa];
END TRY
BEGIN CATCH
    -- Bỏ qua nếu môi trường Windows Auth không có tài khoản sa
END CATCH;
GO

SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- ============================================================================
-- KHỐI TƯƠNG THÍCH MIGRATION (CHO NANG CAP CSDL NỘI BỘ ỨNG DỤNG)
-- ============================================================================
-- BEGIN LEGACY TABLE RENAME
-- END LEGACY TABLE RENAME
-- BEGIN LEGACY COLUMN RENAME
-- END LEGACY COLUMN RENAME
-- BEGIN MIGRATION V2
-- END MIGRATION V2
-- BEGIN MIGRATION V3
-- END MIGRATION V3
-- BEGIN MIGRATION V4
-- END MIGRATION V4
-- BEGIN MIGRATION V5
-- END MIGRATION V5
-- BEGIN MIGRATION V6
-- END MIGRATION V6
-- BEGIN MIGRATION V7
-- END MIGRATION V7
-- BEGIN MIGRATION V8
-- END MIGRATION V8
-- BEGIN MIGRATION V9
-- END MIGRATION V9
-- BEGIN MIGRATION V10
-- END MIGRATION V10
-- BEGIN MIGRATION V11
-- END MIGRATION V11
-- BEGIN MIGRATION V12
-- END MIGRATION V12
-- BEGIN MIGRATION V13
-- END MIGRATION V13
-- BEGIN MIGRATION V14
-- END MIGRATION V14
-- BEGIN MIGRATION V15
-- END MIGRATION V15
-- BEGIN MIGRATION V16
-- END MIGRATION V16
-- BEGIN MIGRATION V17
-- END MIGRATION V17
-- BEGIN MIGRATION V18
-- END MIGRATION V18
-- BEGIN MIGRATION V19
-- END MIGRATION V19
-- BEGIN MIGRATION V20
-- END MIGRATION V20
-- BEGIN MIGRATION V21
-- END MIGRATION V21
-- BEGIN MIGRATION V22
-- END MIGRATION V22
-- BEGIN MIGRATION V23
-- END MIGRATION V23
-- BEGIN MIGRATION V24
-- END MIGRATION V24
-- BEGIN MIGRATION V25
-- END MIGRATION V25
-- BEGIN MIGRATION V26
-- END MIGRATION V26

-- ============================================================================
-- DỌN DẸP BẢNG THỪA ĐỂ THU GỌN VỀ ĐÚNG CÁC BẢNG CỐT LÕI
-- ============================================================================
BEGIN TRANSACTION;
DECLARE @LegacyTables TABLE (TableName sysname);
INSERT @LegacyTables VALUES 
    (N'KyKeToan'),(N'PhieuThuChi'),(N'SoDuDauKy'),(N'DongSaoKe'),(N'CongNo'),
    (N'PhanBoCongNo'),(N'HangTonKho'),(N'BienDongKho'),(N'TieuThuBuongPhong'),
    (N'ThongTinTaiChinhHoaDon'),(N'DieuChinhHoaDon'),(N'HoaDonHuy'),(N'NhomHoaDon'),
    (N'PhanChiaHoaDon'),(N'MenuTong'),(N'MenuCon'),(N'ChucNangQuanLyPhong'),
    (N'ChucNangDichVu'),(N'ChucNangKhachHang'),(N'ChucNangCaTruc'),(N'ChucNangThuChi'),
    (N'ChucNangHoaDon'),(N'ChucNangBaoCao'),(N'ChucNangNhanVien'),(N'ChucNangHeThong'),
    (N'CauHinhMenu'),(N'CauHinhMenuCon'),(N'CauHinhChucNang'),(N'CauHinhChucNangMoi'),
    (N'DuLieuChucNangMoi'),(N'ChiTietBill'),(N'MaGiamGia'),(N'SuDungMaGiamGia'),
    (N'TraXeThue'),(N'ChangLuuTru'),(N'GiaoDichThanhToan'),(N'NhatKyThaoTac'),
    (N'ChucNang'),(N'PhanQuyenNhanVien');

DECLARE @dropFK nvarchar(max) = N'';
SELECT @dropFK = @dropFK + N'ALTER TABLE dbo.' + QUOTENAME(OBJECT_NAME(parent_object_id)) + 
    N' DROP CONSTRAINT ' + QUOTENAME(name) + N';'
FROM sys.foreign_keys
WHERE OBJECT_NAME(referenced_object_id) IN (SELECT TableName FROM @LegacyTables)
   OR OBJECT_NAME(parent_object_id) IN (SELECT TableName FROM @LegacyTables);
IF LEN(@dropFK) > 0 EXEC sys.sp_executesql @dropFK;

DECLARE @dropTbl nvarchar(max) = N'';
SELECT @dropTbl = @dropTbl + N'DROP TABLE dbo.' + QUOTENAME(t.name) + N';'
FROM sys.tables t
JOIN @LegacyTables l ON t.name = l.TableName;
IF LEN(@dropTbl) > 0 EXEC sys.sp_executesql @dropTbl;

COMMIT;
GO

-- ============================================================================
-- 1. BẢNG PHIÊN BẢN CSDL (HỆ THỐNG)
-- ============================================================================
IF OBJECT_ID(N'dbo.PhienBanCSDL', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PhienBanCSDL (
        PhienBan int NOT NULL CONSTRAINT PK_PhienBanCSDL PRIMARY KEY
    );
    INSERT dbo.PhienBanCSDL (PhienBan) VALUES (26);
END;
GO

-- ============================================================================
-- 2. BẢNG TÀI KHOẢN NHÂN VIÊN
-- Khóa chính: MaNhanVien
-- ============================================================================
IF OBJECT_ID(N'dbo.TaiKhoanNhanVien', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TaiKhoanNhanVien (
        MaNhanVien int IDENTITY(1,1) CONSTRAINT PK_TaiKhoanNhanVien PRIMARY KEY,
        Ma AS MaNhanVien, -- Alias tương thích ngược
        TenDangNhap nvarchar(50) NOT NULL UNIQUE,
        MatKhauBam varbinary(32) NOT NULL,
        MuoiBam varbinary(16) NOT NULL,
        SoLanBam int NOT NULL DEFAULT 210000,
        HoTen nvarchar(100) NOT NULL DEFAULT N'Nhân viên',
        TenHienThi nvarchar(100) NOT NULL DEFAULT N'Nhân viên',
        SoDienThoai nvarchar(20) NULL,
        VaiTro varchar(20) NOT NULL CHECK(VaiTro IN ('Admin','Reception','Accountant','Manager')),
        DangHoatDong bit NOT NULL DEFAULT 1,
        DaLuuTru bit NOT NULL DEFAULT 0,
        SoLanDangNhapSai int NOT NULL DEFAULT 0,
        KhoaDen datetime2 NULL,
        PhienBanBaoMat bigint NOT NULL DEFAULT 1,
        DaTuyChinhQuyen bit NOT NULL DEFAULT 0,
        AnhDaiDien varbinary(max) NULL
    );
END;
GO

-- ============================================================================
-- 3. BẢNG PHÒNG
-- Khóa chính: MaPhong
-- ============================================================================
IF OBJECT_ID(N'dbo.Phong', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Phong (
        MaPhong int IDENTITY(1,1) CONSTRAINT PK_Phong PRIMARY KEY,
        Ma AS MaPhong, -- Alias tương thích ngược
        SoPhong nvarchar(10) NOT NULL UNIQUE,
        Loai nvarchar(20) NOT NULL DEFAULT N'Đơn' CHECK(Loai IN(N'Đơn',N'Đôi',N'VIP')),
        LoaiPhong AS Loai, -- Alias
        DonGiaPhong decimal(18,2) NOT NULL CHECK(DonGiaPhong>0),
        DonGia AS DonGiaPhong, -- Alias
        TienCoc decimal(18,2) NOT NULL DEFAULT 0 CHECK(TienCoc>=0),
        TrangThai varchar(20) NOT NULL DEFAULT 'Trong' CHECK(TrangThai IN('Trong','DaDat','DangO','DangDon','BaoTri')),
        Tang int NOT NULL DEFAULT 1,
        PhienBan bigint NOT NULL DEFAULT 1,
        MaHienThi varchar(20) NULL
    );
END;
GO

-- ============================================================================
-- 4. BẢNG KHÁCH HÀNG
-- Khóa chính: MaKhachHang
-- ============================================================================
IF OBJECT_ID(N'dbo.KhachHang', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.KhachHang (
        MaKhachHang int IDENTITY(1,1) CONSTRAINT PK_KhachHang PRIMARY KEY,
        Ma AS MaKhachHang, -- Alias tương thích ngược
        Ten nvarchar(100) NOT NULL,
        TenKhachHang AS Ten, -- Alias
        HoTen AS Ten, -- Alias
        SoDienThoai nvarchar(20) NOT NULL,
        SoGiayTo nvarchar(20) NOT NULL UNIQUE,
        DiaChi nvarchar(200) NULL,
        ThoiDiemTao datetime2 NOT NULL DEFAULT SYSDATETIME(),
        MaHienThi varchar(20) NULL
    );
END;
GO

-- ============================================================================
-- 5. BẢNG LƯỢT LƯU TRÚ (ĐẶT PHÒNG / Ở PHÒNG)
-- Khóa chính: MaLuotLuuTru
-- Khóa ngoại: MaPhong -> Phong, MaKhachHang -> KhachHang, MaNhanVien -> TaiKhoanNhanVien
-- ============================================================================
IF OBJECT_ID(N'dbo.LuotLuuTru', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.LuotLuuTru (
        MaLuotLuuTru bigint IDENTITY(1,1) CONSTRAINT PK_LuotLuuTru PRIMARY KEY,
        Ma AS MaLuotLuuTru, -- Alias tương thích ngược
        MaPhong int NOT NULL CONSTRAINT FK_LuotLuuTru_Phong REFERENCES dbo.Phong(MaPhong),
        MaKhachHang int NOT NULL CONSTRAINT FK_LuotLuuTru_KhachHang REFERENCES dbo.KhachHang(MaKhachHang),
        TenKhach nvarchar(100) NOT NULL,
        SoDienThoai nvarchar(20) NOT NULL,
        SoGiayTo nvarchar(20) NOT NULL,
        TrangThai varchar(20) NOT NULL CHECK(TrangThai IN('Reserved','Occupied','Paid','Cancelled')),
        ConHieuLuc bit NOT NULL DEFAULT 1,
        ThoiDiemTao datetime2 NOT NULL DEFAULT SYSDATETIME(),
        ThoiDiemDen datetime2 NOT NULL,
        ThoiDiemDi datetime2 NOT NULL,
        ThoiDiemNhanPhong datetime2 NULL,
        ThoiDiemTraPhong datetime2 NULL,
        HanGiuPhong datetime2 NULL,
        TienCoc decimal(18,2) NOT NULL DEFAULT 0 CHECK(TienCoc>=0),
        GhiChu nvarchar(500) NULL,
        PhienBan bigint NOT NULL DEFAULT 1,
        MaNhanVien int NOT NULL CONSTRAINT FK_LuotLuuTru_NhanVien REFERENCES dbo.TaiKhoanNhanVien(MaNhanVien),
        MaNguoiTao AS MaNhanVien -- Alias tương thích ngược
    );
END;
GO

-- ============================================================================
-- 6. BẢNG DỊCH VỤ
-- Khóa chính: MaDichVu
-- ============================================================================
IF OBJECT_ID(N'dbo.DichVu', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.DichVu (
        MaDichVu int IDENTITY(1,1) CONSTRAINT PK_DichVu PRIMARY KEY,
        Ma AS MaDichVu, -- Alias tương thích ngược
        DanhMuc nvarchar(100) NOT NULL,
        Ten nvarchar(150) NOT NULL UNIQUE,
        TenDichVu AS Ten, -- Alias
        DonGia decimal(18,2) NOT NULL CHECK(DonGia>=0),
        DonViTinh nvarchar(20) NOT NULL,
        DangHoatDong bit NOT NULL DEFAULT 1
    );
END;
GO

-- ============================================================================
-- 7. BẢNG YÊU CẦU DỊCH VỤ (SỬ DỤNG DỊCH VỤ)
-- Khóa chính: MaYeuCau
-- Khóa ngoại: MaLuotLuuTru -> LuotLuuTru, MaDichVu -> DichVu, MaNhanVien -> TaiKhoanNhanVien
-- ============================================================================
IF OBJECT_ID(N'dbo.YeuCauDichVu', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.YeuCauDichVu (
        MaYeuCau bigint IDENTITY(1,1) CONSTRAINT PK_YeuCauDichVu PRIMARY KEY,
        Ma AS MaYeuCau, -- Alias tương thích ngược
        MaLuotLuuTru bigint NOT NULL CONSTRAINT FK_YeuCauDichVu_LuotLuuTru REFERENCES dbo.LuotLuuTru(MaLuotLuuTru),
        MaDichVu int NOT NULL CONSTRAINT FK_YeuCauDichVu_DichVu REFERENCES dbo.DichVu(MaDichVu),
        DanhMuc nvarchar(100) NOT NULL,
        Ten nvarchar(150) NOT NULL,
        TenDichVu AS Ten, -- Alias
        SoLuong int NOT NULL CHECK(SoLuong BETWEEN 1 AND 100),
        DonGia decimal(18,2) NOT NULL CHECK(DonGia>=0),
        ThanhTien AS (CONVERT(decimal(18,2), SoLuong * DonGia)),
        ThoiDiemGoi datetime2 NOT NULL DEFAULT SYSDATETIME(),
        ThoiDiemGiao datetime2 NULL,
        SoLuongDaGiao int NOT NULL DEFAULT 0,
        ThoiDiemHuy datetime2 NULL,
        MaNhanVien int NOT NULL CONSTRAINT FK_YeuCauDichVu_NhanVien REFERENCES dbo.TaiKhoanNhanVien(MaNhanVien),
        MaNguoiTao AS MaNhanVien, -- Alias
        MaCaTruc bigint NULL
    );
END;
GO

-- ============================================================================
-- 8. BẢNG HÓA ĐƠN THANH TOÁN
-- Khóa chính: MaHoaDon
-- Khóa ngoại: MaLuotLuuTru -> LuotLuuTru, MaNhanVien -> TaiKhoanNhanVien
-- ============================================================================
IF OBJECT_ID(N'dbo.HoaDon', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.HoaDon (
        MaHoaDon bigint IDENTITY(1,1) CONSTRAINT PK_HoaDon PRIMARY KEY,
        Ma AS MaHoaDon, -- Alias tương thích ngược
        MaLuotLuuTru bigint NOT NULL UNIQUE CONSTRAINT FK_HoaDon_LuotLuuTru REFERENCES dbo.LuotLuuTru(MaLuotLuuTru),
        SoPhongHoaDon nvarchar(10) NOT NULL,
        TenKhach nvarchar(100) NOT NULL,
        ThoiDiemLapHoaDon datetime2 NOT NULL DEFAULT SYSDATETIME(),
        TienPhong decimal(18,2) NOT NULL DEFAULT 0 CHECK(TienPhong>=0),
        TienDichVu decimal(18,2) NOT NULL DEFAULT 0 CHECK(TienDichVu>=0),
        TienCoc decimal(18,2) NOT NULL DEFAULT 0 CHECK(TienCoc>=0),
        TienDaThu decimal(18,2) NOT NULL DEFAULT 0 CHECK(TienDaThu>=0),
        TienDaHoan decimal(18,2) NOT NULL DEFAULT 0 CHECK(TienDaHoan>=0),
        PhuongThucThanhToan nvarchar(30) NOT NULL DEFAULT N'Tiền mặt',
        GhiChu nvarchar(500) NULL,
        MaNhanVien int NOT NULL CONSTRAINT FK_HoaDon_NhanVien REFERENCES dbo.TaiKhoanNhanVien(MaNhanVien),
        MaNguoiTao AS MaNhanVien -- Alias
    );
END;
GO

-- ============================================================================
-- 9. BẢNG CA TRỰC
-- Khóa chính: MaCaTruc
-- Khóa ngoại: MaNhanVien -> TaiKhoanNhanVien
-- ============================================================================
IF OBJECT_ID(N'dbo.CaTruc', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.CaTruc (
        MaCaTruc bigint IDENTITY(1,1) CONSTRAINT PK_CaTruc PRIMARY KEY,
        Ma AS MaCaTruc, -- Alias tương thích ngược
        MaNhanVien int NOT NULL CONSTRAINT FK_CaTruc_NhanVien REFERENCES dbo.TaiKhoanNhanVien(MaNhanVien),
        MaThuNgan AS MaNhanVien, -- Alias
        ThoiDiemMoCa datetime2 NOT NULL DEFAULT SYSDATETIME(),
        ThoiDiemDongCa datetime2 NULL,
        TienMatDauCa decimal(18,2) NOT NULL DEFAULT 0 CHECK(TienMatDauCa>=0),
        TienMatKiemDem decimal(18,2) NULL CHECK(TienMatKiemDem>=0),
        TienMatDuKien decimal(18,2) NULL,
        GiaiTrinh nvarchar(500) NULL,
        TrangThai varchar(12) NOT NULL DEFAULT 'Open' CHECK(TrangThai IN('Open','Submitted','Locked')),
        ThoiDiemKhoa datetime2 NULL,
        MaNguoiKhoa int NULL CONSTRAINT FK_CaTruc_NguoiKhoa REFERENCES dbo.TaiKhoanNhanVien(MaNhanVien)
    );
END;
GO

-- ============================================================================
-- DỮ LIỆU MẪU CỐT LÕI
-- ============================================================================
BEGIN TRANSACTION;

-- Tài khoản quản trị mặc định: admin / 12345
IF NOT EXISTS (SELECT 1 FROM dbo.TaiKhoanNhanVien WHERE TenDangNhap = 'admin')
BEGIN
    INSERT dbo.TaiKhoanNhanVien (TenDangNhap, MatKhauBam, MuoiBam, SoLanBam, HoTen, TenHienThi, VaiTro)
    VALUES (
        'admin',
        0x6d82a33267ebbe1961c7f363293aa45b6d6762bb84eaa90e6db49196379654f1,
        0xa1b2c3d4e5f60718293a4b5c6d7e8f90,
        210000,
        N'Quản Trị Viên',
        N'Quản Trị Viên',
        'Admin'
    );
END;

-- Tài khoản lễ tân mặc định: letan / 12345
IF NOT EXISTS (SELECT 1 FROM dbo.TaiKhoanNhanVien WHERE TenDangNhap = 'letan')
BEGIN
    INSERT dbo.TaiKhoanNhanVien (TenDangNhap, MatKhauBam, MuoiBam, SoLanBam, HoTen, TenHienThi, VaiTro)
    VALUES (
        'letan',
        0x6d82a33267ebbe1961c7f363293aa45b6d6762bb84eaa90e6db49196379654f1,
        0xa1b2c3d4e5f60718293a4b5c6d7e8f90,
        210000,
        N'Lễ Tân Thu Ngân',
        N'Lễ Tân Thu Ngân',
        'Reception'
    );
END;

-- Danh sách 15 phòng tiêu chuẩn
DECLARE @Rooms TABLE (So nvarchar(10), Loai nvarchar(20), Gia decimal(18,2), Coc decimal(18,2), Tang int);
INSERT @Rooms VALUES
    (N'101', N'Đơn', 350000, 150000, 1),
    (N'102', N'Đơn', 350000, 150000, 1),
    (N'103', N'Đơn', 350000, 150000, 1),
    (N'104', N'Đơn', 350000, 150000, 1),
    (N'105', N'Đơn', 350000, 150000, 1),
    (N'201', N'Đôi', 550000, 250000, 2),
    (N'202', N'Đôi', 550000, 250000, 2),
    (N'203', N'Đôi', 550000, 250000, 2),
    (N'204', N'Đôi', 550000, 250000, 2),
    (N'205', N'Đôi', 550000, 250000, 2),
    (N'301', N'VIP', 900000, 400000, 3),
    (N'302', N'VIP', 900000, 400000, 3),
    (N'303', N'VIP', 900000, 400000, 3),
    (N'304', N'VIP', 900000, 400000, 3),
    (N'305', N'VIP', 900000, 400000, 3);

INSERT dbo.Phong (SoPhong, Loai, DonGiaPhong, TienCoc, TrangThai, Tang)
SELECT r.So, r.Loai, r.Gia, r.Coc, 'Trong', r.Tang
FROM @Rooms r
WHERE NOT EXISTS (SELECT 1 FROM dbo.Phong p WHERE p.SoPhong = r.So);

-- Danh mục dịch vụ chuẩn
IF NOT EXISTS (SELECT 1 FROM dbo.DichVu WHERE Ten = N'Nước khoáng Lavie 500ml')
    INSERT dbo.DichVu (DanhMuc, Ten, DonGia, DonViTinh) VALUES (N'Đồ uống / Minibar', N'Nước khoáng Lavie 500ml', 15000, N'Chai');

IF NOT EXISTS (SELECT 1 FROM dbo.DichVu WHERE Ten = N'Coca Cola lon 330ml')
    INSERT dbo.DichVu (DanhMuc, Ten, DonGia, DonViTinh) VALUES (N'Đồ uống / Minibar', N'Coca Cola lon 330ml', 20000, N'Lon');

IF NOT EXISTS (SELECT 1 FROM dbo.DichVu WHERE Ten = N'Bia Heineken lon 330ml')
    INSERT dbo.DichVu (DanhMuc, Ten, DonGia, DonViTinh) VALUES (N'Đồ uống / Minibar', N'Bia Heineken lon 330ml', 30000, N'Lon');

IF NOT EXISTS (SELECT 1 FROM dbo.DichVu WHERE Ten = N'Mì ly Omachi chua cay')
    INSERT dbo.DichVu (DanhMuc, Ten, DonGia, DonViTinh) VALUES (N'Đồ ăn nhanh', N'Mì ly Omachi chua cay', 25000, N'Ly');

IF NOT EXISTS (SELECT 1 FROM dbo.DichVu WHERE Ten = N'Giặt ủi quần áo tiêu chuẩn')
    INSERT dbo.DichVu (DanhMuc, Ten, DonGia, DonViTinh) VALUES (N'Tiện ích & Dịch vụ khác', N'Giặt ủi quần áo tiêu chuẩn', 50000, N'Bộ');

IF NOT EXISTS (SELECT 1 FROM dbo.DichVu WHERE Ten = N'Thuê xe máy tay ga (24h)')
    INSERT dbo.DichVu (DanhMuc, Ten, DonGia, DonViTinh) VALUES (N'Tiện ích & Dịch vụ khác', N'Thuê xe máy tay ga (24h)', 150000, N'xe/24h');

COMMIT;
GO
"""

with open('/sessions/dazzling-gracious-gates/mnt/QLKhachSan/Database/Database.sql', 'w', encoding='utf-8') as f:
    f.write(sql_content)

print("Updated Database/Database.sql successfully! Length:", len(sql_content))
