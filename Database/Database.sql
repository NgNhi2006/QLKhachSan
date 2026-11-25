-- ============================================================================
-- HỆ THỐNG CƠ SỞ DỮ LIỆU QUẢN LÝ KHÁCH SẠN (QLKhachSanApp)
-- Cấu trúc thu gọn: CHỈ QUẢN LÝ PHÒNG, ĐẶT PHÒNG, CA TRỰC & HÓA ĐƠN
-- ĐÃ LOẠI BỎ HOÀN TOÀN PHÂN HỆ DỊCH VỤ / MINIBAR
-- Danh sách bảng nghiệp vụ (7 bảng chuẩn 3NF):
--   1. dbo.Phong (PK: MaPhong)
--   2. dbo.KhachHang (PK: MaKhachHang)
--   3. dbo.TaiKhoanNhanVien (PK: MaNhanVien)
--   4. dbo.LuotLuuTru (PK: MaLuotLuuTru)
--   5. dbo.HoaDon (PK: MaHoaDon)
--   6. dbo.CaTruc (PK: MaCaTruc)
--   7. dbo.GiaoDichThanhToan (PK: MaGiaoDich)
-- ============================================================================

USE master;
GO
IF DB_ID(N'QLKhachSanApp') IS NULL
BEGIN
    CREATE DATABASE QLKhachSanApp;
END;
GO
USE QLKhachSanApp;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

-- ============================================================================
-- BƯỚC 1: XÓA CÁC BẢNG DỊCH VỤ VÀ CÁC BẢNG DƯ THỪA KHÁC
-- ============================================================================
BEGIN TRANSACTION;

-- Xóa Foreign Key trỏ vào bảng dịch vụ và các bảng phụ
DECLARE @drop_fk nvarchar(max) = N'';
SELECT @drop_fk = @drop_fk + N'ALTER TABLE dbo.' + QUOTENAME(OBJECT_NAME(parent_object_id)) +
    N' DROP CONSTRAINT ' + QUOTENAME(name) + N';'
FROM sys.foreign_keys
WHERE OBJECT_NAME(referenced_object_id) IN (
    N'DichVu',N'YeuCauDichVu',N'HangTonKho',N'BienDongKho',N'TieuThuBuongPhong',
    N'KyKeToan',N'PhieuThuChi',N'SoDuDauKy',N'DongSaoKe',N'CongNo',
    N'PhanBoCongNo',N'ThongTinTaiChinhHoaDon',N'DieuChinhHoaDon',N'HoaDonHuy',N'NhomHoaDon',
    N'PhanChiaHoaDon',N'MenuTong',N'MenuCon',N'ChucNangQuanLyPhong',
    N'ChucNangDichVu',N'ChucNangKhachHang',N'ChucNangCaTruc',N'ChucNangThuChi',
    N'ChucNangHoaDon',N'ChucNangBaoCao',N'ChucNangNhanVien',N'ChucNangHeThong',
    N'CauHinhMenu',N'CauHinhMenuCon',N'CauHinhChucNang',N'CauHinhChucNangMoi',
    N'DuLieuChucNangMoi',N'ChiTietBill',N'MaGiamGia',N'SuDungMaGiamGia',
    N'TraXeThue',N'ChangLuuTru',N'ChucNang',N'PhanQuyenNhanVien',N'NhatKyThaoTac',N'PhienBanCSDL'
) OR OBJECT_NAME(parent_object_id) IN (
    N'DichVu',N'YeuCauDichVu',N'HangTonKho',N'BienDongKho',N'TieuThuBuongPhong',
    N'KyKeToan',N'PhieuThuChi',N'SoDuDauKy',N'DongSaoKe',N'CongNo',
    N'PhanBoCongNo',N'ThongTinTaiChinhHoaDon',N'DieuChinhHoaDon',N'HoaDonHuy',N'NhomHoaDon',
    N'PhanChiaHoaDon',N'MenuTong',N'MenuCon',N'ChucNangQuanLyPhong',
    N'ChucNangDichVu',N'ChucNangKhachHang',N'ChucNangCaTruc',N'ChucNangThuChi',
    N'ChucNangHoaDon',N'ChucNangBaoCao',N'ChucNangNhanVien',N'ChucNangHeThong',
    N'CauHinhMenu',N'CauHinhMenuCon',N'CauHinhChucNang',N'CauHinhChucNangMoi',
    N'DuLieuChucNangMoi',N'ChiTietBill',N'MaGiamGia',N'SuDungMaGiamGia',
    N'TraXeThue',N'ChangLuuTru',N'ChucNang',N'PhanQuyenNhanVien',N'NhatKyThaoTac',N'PhienBanCSDL'
);
IF LEN(@drop_fk) > 0 EXEC sys.sp_executesql @drop_fk;

-- Xóa hẳn các bảng dịch vụ và bảng phụ
DECLARE @drop_tables nvarchar(max) = N'';
SELECT @drop_tables = @drop_tables + N'DROP TABLE dbo.' + QUOTENAME(name) + N';'
FROM sys.tables
WHERE name IN (
    N'DichVu',N'YeuCauDichVu',N'HangTonKho',N'BienDongKho',N'TieuThuBuongPhong',
    N'KyKeToan',N'PhieuThuChi',N'SoDuDauKy',N'DongSaoKe',N'CongNo',
    N'PhanBoCongNo',N'ThongTinTaiChinhHoaDon',N'DieuChinhHoaDon',N'HoaDonHuy',N'NhomHoaDon',
    N'PhanChiaHoaDon',N'MenuTong',N'MenuCon',N'ChucNangQuanLyPhong',
    N'ChucNangDichVu',N'ChucNangKhachHang',N'ChucNangCaTruc',N'ChucNangThuChi',
    N'ChucNangHoaDon',N'ChucNangBaoCao',N'ChucNangNhanVien',N'ChucNangHeThong',
    N'CauHinhMenu',N'CauHinhMenuCon',N'CauHinhChucNang',N'CauHinhChucNangMoi',
    N'DuLieuChucNangMoi',N'ChiTietBill',N'MaGiamGia',N'SuDungMaGiamGia',
    N'TraXeThue',N'ChangLuuTru',N'ChucNang',N'PhanQuyenNhanVien',N'NhatKyThaoTac',N'PhienBanCSDL'
);
IF LEN(@drop_tables) > 0 EXEC sys.sp_executesql @drop_tables;

COMMIT;
GO

-- ============================================================================
-- BƯỚC 2: TẠO MỚI CÁC BẢNG CỐT LÕI (NẾU CHƯA CÓ)
-- ============================================================================

-- 1. TaiKhoanNhanVien
IF OBJECT_ID(N'dbo.TaiKhoanNhanVien', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TaiKhoanNhanVien (
        MaNhanVien int IDENTITY(1,1) CONSTRAINT PK_TaiKhoanNhanVien PRIMARY KEY,
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
        QuyenChucNang nvarchar(max) NULL,
        AnhDaiDien varbinary(max) NULL
    );
END;
GO

-- 3. Phong
IF OBJECT_ID(N'dbo.Phong', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Phong (
        MaPhong int IDENTITY(1,1) CONSTRAINT PK_Phong PRIMARY KEY,
        SoPhong nvarchar(10) NOT NULL UNIQUE,
        Loai nvarchar(20) NOT NULL DEFAULT N'Đơn' CHECK(Loai IN(N'Đơn',N'Đôi',N'VIP')),
        DonGiaPhong decimal(18,2) NOT NULL CHECK(DonGiaPhong>0),
        TienCoc decimal(18,2) NOT NULL DEFAULT 0 CHECK(TienCoc>=0),
        TrangThai varchar(20) NOT NULL DEFAULT 'Trong' CHECK(TrangThai IN('Trong','DaDat','DangO','DangDon','BaoTri')),
        Tang int NOT NULL DEFAULT 1,
        PhienBan bigint NOT NULL DEFAULT 1,
        MaHienThi varchar(20) NULL
    );
END;
GO

-- 4. KhachHang
IF OBJECT_ID(N'dbo.KhachHang', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.KhachHang (
        MaKhachHang int IDENTITY(1,1) CONSTRAINT PK_KhachHang PRIMARY KEY,
        Ten nvarchar(100) NOT NULL,
        SoDienThoai nvarchar(20) NOT NULL,
        SoGiayTo nvarchar(20) NOT NULL UNIQUE,
        DiaChi nvarchar(200) NULL,
        ThoiDiemTao datetime2 NOT NULL DEFAULT SYSDATETIME(),
        MaHienThi varchar(20) NULL
    );
END;
GO

-- 5. LuotLuuTru
IF OBJECT_ID(N'dbo.LuotLuuTru', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.LuotLuuTru (
        MaLuotLuuTru bigint IDENTITY(1,1) CONSTRAINT PK_LuotLuuTru PRIMARY KEY,
        MaPhong int NOT NULL,
        MaKhachHang int NOT NULL,
        -- Ảnh chụp thông tin khách của lượt này; KhachHang có thể được cập nhật về sau.
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
        MaNhanVien int NOT NULL
    );
END;
GO

-- 6. HoaDon
IF OBJECT_ID(N'dbo.HoaDon', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.HoaDon (
        MaHoaDon bigint IDENTITY(1,1) CONSTRAINT PK_HoaDon PRIMARY KEY,
        MaLuotLuuTru bigint NOT NULL UNIQUE,
        SoPhongHoaDon nvarchar(10) NOT NULL,
        TenKhach nvarchar(100) NOT NULL,
        ThoiDiemLapHoaDon datetime2 NOT NULL DEFAULT SYSDATETIME(),
        TienPhong decimal(18,2) NOT NULL DEFAULT 0 CHECK(TienPhong>=0),
        TienDichVu decimal(18,2) NOT NULL DEFAULT 0,
        TienCoc decimal(18,2) NOT NULL DEFAULT 0 CHECK(TienCoc>=0),
        -- Snapshot số thu checkout/hoàn cọc thừa tại lúc lập hóa đơn; các lần thu/hoàn
        -- và phương thức thực tế nằm trong từng dòng GiaoDichThanhToan.
        TienDaThu decimal(18,2) NOT NULL DEFAULT 0 CHECK(TienDaThu>=0),
        TienDaHoan decimal(18,2) NOT NULL DEFAULT 0 CHECK(TienDaHoan>=0),
        PhuongThucThanhToan nvarchar(30) NOT NULL DEFAULT N'Tiền mặt',
        GhiChu nvarchar(500) NULL,
        MaNhanVien int NOT NULL,
        MaCaTruc bigint NULL
    );
END;
GO

-- 7. CaTruc
IF OBJECT_ID(N'dbo.CaTruc', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.CaTruc (
        MaCaTruc bigint IDENTITY(1,1) CONSTRAINT PK_CaTruc PRIMARY KEY,
        MaNhanVien int NOT NULL,
        ThoiDiemMoCa datetime2 NOT NULL DEFAULT SYSDATETIME(),
        ThoiDiemDongCa datetime2 NULL,
        TienMatDauCa decimal(18,2) NOT NULL DEFAULT 0 CHECK(TienMatDauCa>=0),
        TienMatKiemDem decimal(18,2) NULL CHECK(TienMatKiemDem>=0),
        TienMatDuKien decimal(18,2) NULL,
        GiaiTrinh nvarchar(500) NULL,
        TrangThai varchar(12) NOT NULL DEFAULT 'Open' CHECK(TrangThai IN('Open','Submitted','Locked')),
        ThoiDiemKhoa datetime2 NULL,
        MaNguoiKhoa int NULL
    );
END;
GO

-- 8. GiaoDichThanhToan
IF OBJECT_ID(N'dbo.GiaoDichThanhToan', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.GiaoDichThanhToan (
        MaGiaoDich bigint IDENTITY(1,1) CONSTRAINT PK_GiaoDichThanhToan PRIMARY KEY,
        MaLuotLuuTru bigint NOT NULL,
        LoaiGiaoDich varchar(20) NOT NULL CHECK(LoaiGiaoDich IN('Deposit','Checkout','Refund','Forfeit')),
        SoTien decimal(18,2) NOT NULL CHECK(SoTien>0),
        ThoiDiemTao datetime2 NOT NULL DEFAULT SYSDATETIME(),
        PhuongThucThanhToan nvarchar(30) NOT NULL,
        GhiChu nvarchar(300) NOT NULL DEFAULT N'',
        MaNhanVien int NOT NULL,
        MaCaTruc bigint NULL,
        MaThamChieuNgoai nvarchar(100) NULL
    );
END;
GO

-- ============================================================================
-- BƯỚC 3: ĐỔI TÊN CỘT 'Ma' TRÊN CSDL CŨ THÀNH TÊN TƯỜNG MINH THEO THỰC THỂ
-- ============================================================================
BEGIN TRANSACTION;

-- Xóa các FK hiện có để đổi tên cột không bị lỗi ràng buộc
DECLARE @drop_core_fk nvarchar(max) = N'';
SELECT @drop_core_fk = @drop_core_fk + N'ALTER TABLE dbo.' + QUOTENAME(OBJECT_NAME(parent_object_id)) +
    N' DROP CONSTRAINT ' + QUOTENAME(name) + N';'
FROM sys.foreign_keys;
IF LEN(@drop_core_fk) > 0 EXEC sys.sp_executesql @drop_core_fk;

-- Bảng Phong: đổi Ma -> MaPhong
IF COL_LENGTH(N'dbo.Phong', N'MaPhong') IS NULL AND COL_LENGTH(N'dbo.Phong', N'Ma') IS NOT NULL
BEGIN
    EXEC sp_rename N'dbo.Phong.Ma', N'MaPhong', N'COLUMN';
END;
IF COL_LENGTH(N'dbo.Phong', N'Ma') IS NULL AND COL_LENGTH(N'dbo.Phong', N'MaPhong') IS NOT NULL
BEGIN
    EXEC(N'ALTER TABLE dbo.Phong ADD Ma AS MaPhong;');
END;
IF COL_LENGTH(N'dbo.Phong', N'Tang') IS NULL
BEGIN
    ALTER TABLE dbo.Phong ADD Tang int NOT NULL CONSTRAINT DF_Phong_Tang DEFAULT 1;
END;

-- Bảng KhachHang: đổi Ma -> MaKhachHang
IF COL_LENGTH(N'dbo.KhachHang', N'MaKhachHang') IS NULL AND COL_LENGTH(N'dbo.KhachHang', N'Ma') IS NOT NULL
BEGIN
    EXEC sp_rename N'dbo.KhachHang.Ma', N'MaKhachHang', N'COLUMN';
END;
IF COL_LENGTH(N'dbo.KhachHang', N'Ma') IS NULL AND COL_LENGTH(N'dbo.KhachHang', N'MaKhachHang') IS NOT NULL
BEGIN
    EXEC(N'ALTER TABLE dbo.KhachHang ADD Ma AS MaKhachHang;');
END;

-- Bảng TaiKhoanNhanVien: đổi Ma -> MaNhanVien
IF COL_LENGTH(N'dbo.TaiKhoanNhanVien', N'MaNhanVien') IS NULL AND COL_LENGTH(N'dbo.TaiKhoanNhanVien', N'Ma') IS NOT NULL
BEGIN
    EXEC sp_rename N'dbo.TaiKhoanNhanVien.Ma', N'MaNhanVien', N'COLUMN';
END;
IF COL_LENGTH(N'dbo.TaiKhoanNhanVien', N'Ma') IS NULL AND COL_LENGTH(N'dbo.TaiKhoanNhanVien', N'MaNhanVien') IS NOT NULL
BEGIN
    EXEC(N'ALTER TABLE dbo.TaiKhoanNhanVien ADD Ma AS MaNhanVien;');
END;
IF COL_LENGTH(N'dbo.TaiKhoanNhanVien', N'HoTen') IS NULL
BEGIN
    ALTER TABLE dbo.TaiKhoanNhanVien ADD HoTen nvarchar(100) NOT NULL CONSTRAINT DF_TaiKhoanNhanVien_HoTen DEFAULT N'Nhân viên';
END;

-- Bảng LuotLuuTru: đổi Ma -> MaLuotLuuTru, MaNguoiTao -> MaNhanVien
IF COL_LENGTH(N'dbo.LuotLuuTru', N'MaLuotLuuTru') IS NULL AND COL_LENGTH(N'dbo.LuotLuuTru', N'Ma') IS NOT NULL
BEGIN
    EXEC sp_rename N'dbo.LuotLuuTru.Ma', N'MaLuotLuuTru', N'COLUMN';
END;
IF COL_LENGTH(N'dbo.LuotLuuTru', N'Ma') IS NULL AND COL_LENGTH(N'dbo.LuotLuuTru', N'MaLuotLuuTru') IS NOT NULL
BEGIN
    EXEC(N'ALTER TABLE dbo.LuotLuuTru ADD Ma AS MaLuotLuuTru;');
END;
IF COL_LENGTH(N'dbo.LuotLuuTru', N'MaNhanVien') IS NULL AND COL_LENGTH(N'dbo.LuotLuuTru', N'MaNguoiTao') IS NOT NULL
BEGIN
    EXEC sp_rename N'dbo.LuotLuuTru.MaNguoiTao', N'MaNhanVien', N'COLUMN';
END;
IF COL_LENGTH(N'dbo.LuotLuuTru', N'MaNguoiTao') IS NULL AND COL_LENGTH(N'dbo.LuotLuuTru', N'MaNhanVien') IS NOT NULL
BEGIN
    EXEC(N'ALTER TABLE dbo.LuotLuuTru ADD MaNguoiTao AS MaNhanVien;');
END;

-- Bảng HoaDon: đổi Ma -> MaHoaDon, MaNguoiTao -> MaNhanVien
IF COL_LENGTH(N'dbo.HoaDon', N'MaHoaDon') IS NULL AND COL_LENGTH(N'dbo.HoaDon', N'Ma') IS NOT NULL
BEGIN
    EXEC sp_rename N'dbo.HoaDon.Ma', N'MaHoaDon', N'COLUMN';
END;
IF COL_LENGTH(N'dbo.HoaDon', N'Ma') IS NULL AND COL_LENGTH(N'dbo.HoaDon', N'MaHoaDon') IS NOT NULL
BEGIN
    EXEC(N'ALTER TABLE dbo.HoaDon ADD Ma AS MaHoaDon;');
END;
IF COL_LENGTH(N'dbo.HoaDon', N'MaNhanVien') IS NULL AND COL_LENGTH(N'dbo.HoaDon', N'MaNguoiTao') IS NOT NULL
BEGIN
    EXEC sp_rename N'dbo.HoaDon.MaNguoiTao', N'MaNhanVien', N'COLUMN';
END;
IF COL_LENGTH(N'dbo.HoaDon', N'MaNguoiTao') IS NULL AND COL_LENGTH(N'dbo.HoaDon', N'MaNhanVien') IS NOT NULL
BEGIN
    EXEC(N'ALTER TABLE dbo.HoaDon ADD MaNguoiTao AS MaNhanVien;');
END;

-- Bảng CaTruc: đổi Ma -> MaCaTruc, MaThuNgan -> MaNhanVien
IF COL_LENGTH(N'dbo.CaTruc', N'MaCaTruc') IS NULL AND COL_LENGTH(N'dbo.CaTruc', N'Ma') IS NOT NULL
BEGIN
    EXEC sp_rename N'dbo.CaTruc.Ma', N'MaCaTruc', N'COLUMN';
END;
IF COL_LENGTH(N'dbo.CaTruc', N'Ma') IS NULL AND COL_LENGTH(N'dbo.CaTruc', N'MaCaTruc') IS NOT NULL
BEGIN
    EXEC(N'ALTER TABLE dbo.CaTruc ADD Ma AS MaCaTruc;');
END;
IF COL_LENGTH(N'dbo.CaTruc', N'MaNhanVien') IS NULL AND COL_LENGTH(N'dbo.CaTruc', N'MaThuNgan') IS NOT NULL
BEGIN
    EXEC sp_rename N'dbo.CaTruc.MaThuNgan', N'MaNhanVien', N'COLUMN';
END;
IF COL_LENGTH(N'dbo.CaTruc', N'MaThuNgan') IS NULL AND COL_LENGTH(N'dbo.CaTruc', N'MaNhanVien') IS NOT NULL
BEGIN
    EXEC(N'ALTER TABLE dbo.CaTruc ADD MaThuNgan AS MaNhanVien;');
END;

-- Bảng GiaoDichThanhToan: đổi Ma -> MaGiaoDich
IF OBJECT_ID(N'dbo.GiaoDichThanhToan') IS NOT NULL
BEGIN
    IF COL_LENGTH(N'dbo.GiaoDichThanhToan', N'MaGiaoDich') IS NULL AND COL_LENGTH(N'dbo.GiaoDichThanhToan', N'Ma') IS NOT NULL
    BEGIN
        EXEC sp_rename N'dbo.GiaoDichThanhToan.Ma', N'MaGiaoDich', N'COLUMN';
    END;
    IF COL_LENGTH(N'dbo.GiaoDichThanhToan', N'Ma') IS NULL AND COL_LENGTH(N'dbo.GiaoDichThanhToan', N'MaGiaoDich') IS NOT NULL
    BEGIN
        EXEC(N'ALTER TABLE dbo.GiaoDichThanhToan ADD Ma AS MaGiaoDich;');
    END;
    IF COL_LENGTH(N'dbo.GiaoDichThanhToan', N'MaNhanVien') IS NULL AND COL_LENGTH(N'dbo.GiaoDichThanhToan', N'MaNguoiTao') IS NOT NULL
    BEGIN
        EXEC sp_rename N'dbo.GiaoDichThanhToan.MaNguoiTao', N'MaNhanVien', N'COLUMN';
    END;
    IF COL_LENGTH(N'dbo.GiaoDichThanhToan', N'MaNguoiTao') IS NULL AND COL_LENGTH(N'dbo.GiaoDichThanhToan', N'MaNhanVien') IS NOT NULL
    BEGIN
        EXEC(N'ALTER TABLE dbo.GiaoDichThanhToan ADD MaNguoiTao AS MaNhanVien;');
    END;
END;

-- ============================================================================
-- BƯỚC 4: TẠO LẠI CÁC RÀNG BUỘC KHÓA NGOẠI CHUẨN MỰC
-- ============================================================================

-- LuotLuuTru -> Phong
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_LuotLuuTru_Phong')
    ALTER TABLE dbo.LuotLuuTru ADD CONSTRAINT FK_LuotLuuTru_Phong FOREIGN KEY (MaPhong) REFERENCES dbo.Phong(MaPhong);

-- LuotLuuTru -> KhachHang
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_LuotLuuTru_KhachHang')
    ALTER TABLE dbo.LuotLuuTru ADD CONSTRAINT FK_LuotLuuTru_KhachHang FOREIGN KEY (MaKhachHang) REFERENCES dbo.KhachHang(MaKhachHang);

-- LuotLuuTru -> TaiKhoanNhanVien
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_LuotLuuTru_NhanVien')
    ALTER TABLE dbo.LuotLuuTru ADD CONSTRAINT FK_LuotLuuTru_NhanVien FOREIGN KEY (MaNhanVien) REFERENCES dbo.TaiKhoanNhanVien(MaNhanVien);

-- HoaDon -> LuotLuuTru
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_HoaDon_LuotLuuTru')
    ALTER TABLE dbo.HoaDon ADD CONSTRAINT FK_HoaDon_LuotLuuTru FOREIGN KEY (MaLuotLuuTru) REFERENCES dbo.LuotLuuTru(MaLuotLuuTru);

-- CaTruc -> TaiKhoanNhanVien
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_CaTruc_NhanVien')
    ALTER TABLE dbo.CaTruc ADD CONSTRAINT FK_CaTruc_NhanVien FOREIGN KEY (MaNhanVien) REFERENCES dbo.TaiKhoanNhanVien(MaNhanVien);

-- GiaoDichThanhToan -> LuotLuuTru & CaTruc
IF OBJECT_ID(N'dbo.GiaoDichThanhToan') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_GiaoDich_LuotLuuTru')
        ALTER TABLE dbo.GiaoDichThanhToan ADD CONSTRAINT FK_GiaoDich_LuotLuuTru FOREIGN KEY (MaLuotLuuTru) REFERENCES dbo.LuotLuuTru(MaLuotLuuTru);
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_GiaoDich_CaTruc') AND COL_LENGTH(N'dbo.GiaoDichThanhToan', N'MaCaTruc') IS NOT NULL
        ALTER TABLE dbo.GiaoDichThanhToan ADD CONSTRAINT FK_GiaoDich_CaTruc FOREIGN KEY (MaCaTruc) REFERENCES dbo.CaTruc(MaCaTruc);
END;

COMMIT;
GO

-- ============================================================================
-- BƯỚC 5: KHỞI TẠO DỮ LIỆU MẪU (NẾU CHƯA CÓ)
-- ============================================================================
BEGIN TRANSACTION;

-- Tài khoản quản trị mặc định: admin / 12345
IF NOT EXISTS (SELECT 1 FROM dbo.TaiKhoanNhanVien WHERE TenDangNhap = 'admin' AND DaLuuTru = 0)
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

-- Tài khoản lễ tân: letan / 12345
IF NOT EXISTS (SELECT 1 FROM dbo.TaiKhoanNhanVien WHERE TenDangNhap = 'letan' AND DaLuuTru = 0)
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

-- Tài khoản kế toán: ketoan / 12345
IF NOT EXISTS (SELECT 1 FROM dbo.TaiKhoanNhanVien WHERE TenDangNhap = 'ketoan' AND DaLuuTru = 0)
BEGIN
    INSERT dbo.TaiKhoanNhanVien (TenDangNhap, MatKhauBam, MuoiBam, SoLanBam, HoTen, TenHienThi, VaiTro)
    VALUES (
        'ketoan',
        0x6d82a33267ebbe1961c7f363293aa45b6d6762bb84eaa90e6db49196379654f1,
        0xa1b2c3d4e5f60718293a4b5c6d7e8f90,
        210000,
        N'Kế Toán Khách Sạn',
        N'Kế Toán Khách Sạn',
        'Accountant'
    );
END;

-- Tài khoản quản lý: quanly / 12345
IF NOT EXISTS (SELECT 1 FROM dbo.TaiKhoanNhanVien WHERE TenDangNhap = 'quanly' AND DaLuuTru = 0)
BEGIN
    INSERT dbo.TaiKhoanNhanVien (TenDangNhap, MatKhauBam, MuoiBam, SoLanBam, HoTen, TenHienThi, VaiTro)
    VALUES (
        'quanly',
        0x6d82a33267ebbe1961c7f363293aa45b6d6762bb84eaa90e6db49196379654f1,
        0xa1b2c3d4e5f60718293a4b5c6d7e8f90,
        210000,
        N'Quản Lý Khách Sạn',
        N'Quản Lý Khách Sạn',
        'Manager'
    );
END;

-- 15 phòng tiêu chuẩn
IF (SELECT COUNT(*) FROM dbo.Phong) < 15
BEGIN
    DECLARE @DefaultRooms TABLE (So nvarchar(10), Loai nvarchar(20), Gia decimal(18,2), Coc decimal(18,2), Tang int);
    INSERT @DefaultRooms VALUES
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
    FROM @DefaultRooms r
    WHERE NOT EXISTS (SELECT 1 FROM dbo.Phong p WHERE p.SoPhong = r.So);
END;

COMMIT;
GO

-- ============================================================================
-- BƯỚC 6: KIỂM TRA KẾT QUẢ DANH SÁCH BẢNG SAU CHUẨN HÓA (ĐÚNG 7 BẢNG)
-- ============================================================================
SELECT name AS TenBang, create_date AS NgayTao
FROM sys.tables
WHERE name <> N'sysdiagrams'
ORDER BY name;
GO
