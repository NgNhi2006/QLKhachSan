upgrade_sql = """-- ============================================================================
-- SCRIPT CHUẨN HÓA CSDL QLKhachSanApp: THU GỌN VỀ 7-8 BẢNG & SỬA LỖI DIAGRAM
-- Chạy script này trong SQL Server Management Studio (SSMS) bằng phím F5
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

-- 1. Cho phép tạo Diagram trong SSMS không bị lỗi quyền sở hữu
BEGIN TRY
    ALTER AUTHORIZATION ON DATABASE::[QLKhachSanApp] TO [sa];
END TRY
BEGIN CATCH
END CATCH;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

-- 2. XÓA TOÀN BỘ CÁC BẢNG DƯ THỪA ĐỂ THU GỌN VỀ ĐÚNG 7-8 BẢNG CỐT LÕI
BEGIN TRANSACTION;

-- Xóa các Foreign Key trỏ vào hoặc xuất phát từ các bảng dư thừa
DECLARE @drop_fk nvarchar(max) = N'';
SELECT @drop_fk = @drop_fk + N'ALTER TABLE dbo.' + QUOTENAME(OBJECT_NAME(parent_object_id)) + 
    N' DROP CONSTRAINT ' + QUOTENAME(name) + N';'
FROM sys.foreign_keys
WHERE OBJECT_NAME(referenced_object_id) IN (
    N'KyKeToan',N'PhieuThuChi',N'SoDuDauKy',N'DongSaoKe',N'CongNo',
    N'PhanBoCongNo',N'HangTonKho',N'BienDongKho',N'TieuThuBuongPhong',
    N'ThongTinTaiChinhHoaDon',N'DieuChinhHoaDon',N'HoaDonHuy',N'NhomHoaDon',
    N'PhanChiaHoaDon',N'MenuTong',N'MenuCon',N'ChucNangQuanLyPhong',
    N'ChucNangDichVu',N'ChucNangKhachHang',N'ChucNangCaTruc',N'ChucNangThuChi',
    N'ChucNangHoaDon',N'ChucNangBaoCao',N'ChucNangNhanVien',N'ChucNangHeThong',
    N'CauHinhMenu',N'CauHinhMenuCon',N'CauHinhChucNang',N'CauHinhChucNangMoi',
    N'DuLieuChucNangMoi',N'ChiTietBill',N'MaGiamGia',N'SuDungMaGiamGia',
    N'TraXeThue',N'ChangLuuTru',N'ChucNang',N'PhanQuyenNhanVien',N'NhatKyThaoTac'
) OR OBJECT_NAME(parent_object_id) IN (
    N'KyKeToan',N'PhieuThuChi',N'SoDuDauKy',N'DongSaoKe',N'CongNo',
    N'PhanBoCongNo',N'HangTonKho',N'BienDongKho',N'TieuThuBuongPhong',
    N'ThongTinTaiChinhHoaDon',N'DieuChinhHoaDon',N'HoaDonHuy',N'NhomHoaDon',
    N'PhanChiaHoaDon',N'MenuTong',N'MenuCon',N'ChucNangQuanLyPhong',
    N'ChucNangDichVu',N'ChucNangKhachHang',N'ChucNangCaTruc',N'ChucNangThuChi',
    N'ChucNangHoaDon',N'ChucNangBaoCao',N'ChucNangNhanVien',N'ChucNangHeThong',
    N'CauHinhMenu',N'CauHinhMenuCon',N'CauHinhChucNang',N'CauHinhChucNangMoi',
    N'DuLieuChucNangMoi',N'ChiTietBill',N'MaGiamGia',N'SuDungMaGiamGia',
    N'TraXeThue',N'ChangLuuTru',N'ChucNang',N'PhanQuyenNhanVien',N'NhatKyThaoTac'
);
IF LEN(@drop_fk) > 0 EXEC sys.sp_executesql @drop_fk;

-- Xóa các bảng dư thừa
DECLARE @drop_tables nvarchar(max) = N'';
SELECT @drop_tables = @drop_tables + N'DROP TABLE dbo.' + QUOTENAME(name) + N';'
FROM sys.tables
WHERE name IN (
    N'KyKeToan',N'PhieuThuChi',N'SoDuDauKy',N'DongSaoKe',N'CongNo',
    N'PhanBoCongNo',N'HangTonKho',N'BienDongKho',N'TieuThuBuongPhong',
    N'ThongTinTaiChinhHoaDon',N'DieuChinhHoaDon',N'HoaDonHuy',N'NhomHoaDon',
    N'PhanChiaHoaDon',N'MenuTong',N'MenuCon',N'ChucNangQuanLyPhong',
    N'ChucNangDichVu',N'ChucNangKhachHang',N'ChucNangCaTruc',N'ChucNangThuChi',
    N'ChucNangHoaDon',N'ChucNangBaoCao',N'ChucNangNhanVien',N'ChucNangHeThong',
    N'CauHinhMenu',N'CauHinhMenuCon',N'CauHinhChucNang',N'CauHinhChucNangMoi',
    N'DuLieuChucNangMoi',N'ChiTietBill',N'MaGiamGia',N'SuDungMaGiamGia',
    N'TraXeThue',N'ChangLuuTru',N'ChucNang',N'PhanQuyenNhanVien',N'NhatKyThaoTac'
);
IF LEN(@drop_tables) > 0 EXEC sys.sp_executesql @drop_tables;

COMMIT;
GO

-- 3. ĐỔI TÊN KHÓA CHÍNH TỪ 'Ma' THÀNH TÊN RÕ RÀNG THEO THỰC THỂ
-- Để khi mở Database Diagram trong SSMS sẽ thấy: MaPhong, MaKhachHang, MaNhanVien...
BEGIN TRANSACTION;

-- Xóa các FK hiện tại giữa các bảng cốt lõi trước khi đổi tên cột
DECLARE @drop_all_core_fk nvarchar(max) = N'';
SELECT @drop_all_core_fk = @drop_all_core_fk + N'ALTER TABLE dbo.' + QUOTENAME(OBJECT_NAME(parent_object_id)) + 
    N' DROP CONSTRAINT ' + QUOTENAME(name) + N';'
FROM sys.foreign_keys;
IF LEN(@drop_all_core_fk) > 0 EXEC sys.sp_executesql @drop_all_core_fk;

-- Bảng Phong: đổi Ma -> MaPhong
IF COL_LENGTH(N'dbo.Phong', N'MaPhong') IS NULL AND COL_LENGTH(N'dbo.Phong', N'Ma') IS NOT NULL
BEGIN
    EXEC sp_rename N'dbo.Phong.Ma', N'MaPhong', N'COLUMN';
    -- Thêm cột alias tính toán 'Ma' để code cũ truy vấn Ma vẫn chạy 100%
    EXEC(N'ALTER TABLE dbo.Phong ADD Ma AS MaPhong;');
END;

-- Bảng KhachHang: đổi Ma -> MaKhachHang
IF COL_LENGTH(N'dbo.KhachHang', N'MaKhachHang') IS NULL AND COL_LENGTH(N'dbo.KhachHang', N'Ma') IS NOT NULL
BEGIN
    EXEC sp_rename N'dbo.KhachHang.Ma', N'MaKhachHang', N'COLUMN';
    EXEC(N'ALTER TABLE dbo.KhachHang ADD Ma AS MaKhachHang;');
END;

-- Bảng TaiKhoanNhanVien: đổi Ma -> MaNhanVien
IF COL_LENGTH(N'dbo.TaiKhoanNhanVien', N'MaNhanVien') IS NULL AND COL_LENGTH(N'dbo.TaiKhoanNhanVien', N'Ma') IS NOT NULL
BEGIN
    EXEC sp_rename N'dbo.TaiKhoanNhanVien.Ma', N'MaNhanVien', N'COLUMN';
    EXEC(N'ALTER TABLE dbo.TaiKhoanNhanVien ADD Ma AS MaNhanVien;');
END;

-- Bảng LuotLuuTru: đổi Ma -> MaLuotLuuTru, đổi MaNguoiTao -> MaNhanVien
IF COL_LENGTH(N'dbo.LuotLuuTru', N'MaLuotLuuTru') IS NULL AND COL_LENGTH(N'dbo.LuotLuuTru', N'Ma') IS NOT NULL
BEGIN
    EXEC sp_rename N'dbo.LuotLuuTru.Ma', N'MaLuotLuuTru', N'COLUMN';
    EXEC(N'ALTER TABLE dbo.LuotLuuTru ADD Ma AS MaLuotLuuTru;');
END;
IF COL_LENGTH(N'dbo.LuotLuuTru', N'MaNhanVien') IS NULL AND COL_LENGTH(N'dbo.LuotLuuTru', N'MaNguoiTao') IS NOT NULL
BEGIN
    EXEC sp_rename N'dbo.LuotLuuTru.MaNguoiTao', N'MaNhanVien', N'COLUMN';
    EXEC(N'ALTER TABLE dbo.LuotLuuTru ADD MaNguoiTao AS MaNhanVien;');
END;

-- Bảng DichVu: đổi Ma -> MaDichVu
IF COL_LENGTH(N'dbo.DichVu', N'MaDichVu') IS NULL AND COL_LENGTH(N'dbo.DichVu', N'Ma') IS NOT NULL
BEGIN
    EXEC sp_rename N'dbo.DichVu.Ma', N'MaDichVu', N'COLUMN';
    EXEC(N'ALTER TABLE dbo.DichVu ADD Ma AS MaDichVu;');
END;

-- Bảng YeuCauDichVu: đổi Ma -> MaYeuCau, đổi MaNguoiTao -> MaNhanVien
IF COL_LENGTH(N'dbo.YeuCauDichVu', N'MaYeuCau') IS NULL AND COL_LENGTH(N'dbo.YeuCauDichVu', N'Ma') IS NOT NULL
BEGIN
    EXEC sp_rename N'dbo.YeuCauDichVu.Ma', N'MaYeuCau', N'COLUMN';
    EXEC(N'ALTER TABLE dbo.YeuCauDichVu ADD Ma AS MaYeuCau;');
END;
IF COL_LENGTH(N'dbo.YeuCauDichVu', N'MaNhanVien') IS NULL AND COL_LENGTH(N'dbo.YeuCauDichVu', N'MaNguoiTao') IS NOT NULL
BEGIN
    EXEC sp_rename N'dbo.YeuCauDichVu.MaNguoiTao', N'MaNhanVien', N'COLUMN';
    EXEC(N'ALTER TABLE dbo.YeuCauDichVu ADD MaNguoiTao AS MaNhanVien;');
END;

-- Bảng HoaDon: đổi Ma -> MaHoaDon, đổi MaNguoiTao -> MaNhanVien
IF COL_LENGTH(N'dbo.HoaDon', N'MaHoaDon') IS NULL AND COL_LENGTH(N'dbo.HoaDon', N'Ma') IS NOT NULL
BEGIN
    EXEC sp_rename N'dbo.HoaDon.Ma', N'MaHoaDon', N'COLUMN';
    EXEC(N'ALTER TABLE dbo.HoaDon ADD Ma AS MaHoaDon;');
END;
IF COL_LENGTH(N'dbo.HoaDon', N'MaNhanVien') IS NULL AND COL_LENGTH(N'dbo.HoaDon', N'MaNguoiTao') IS NOT NULL
BEGIN
    EXEC sp_rename N'dbo.HoaDon.MaNguoiTao', N'MaNhanVien', N'COLUMN';
    EXEC(N'ALTER TABLE dbo.HoaDon ADD MaNguoiTao AS MaNhanVien;');
END;

-- Bảng CaTruc: đổi Ma -> MaCaTruc, đổi MaThuNgan -> MaNhanVien
IF COL_LENGTH(N'dbo.CaTruc', N'MaCaTruc') IS NULL AND COL_LENGTH(N'dbo.CaTruc', N'Ma') IS NOT NULL
BEGIN
    EXEC sp_rename N'dbo.CaTruc.Ma', N'MaCaTruc', N'COLUMN';
    EXEC(N'ALTER TABLE dbo.CaTruc ADD Ma AS MaCaTruc;');
END;
IF COL_LENGTH(N'dbo.CaTruc', N'MaNhanVien') IS NULL AND COL_LENGTH(N'dbo.CaTruc', N'MaThuNgan') IS NOT NULL
BEGIN
    EXEC sp_rename N'dbo.CaTruc.MaThuNgan', N'MaNhanVien', N'COLUMN';
    EXEC(N'ALTER TABLE dbo.CaTruc ADD MaThuNgan AS MaNhanVien;');
END;

-- Bảng GiaoDichThanhToan: đổi Ma -> MaGiaoDich
IF COL_LENGTH(N'dbo.GiaoDichThanhToan', N'MaGiaoDich') IS NULL AND COL_LENGTH(N'dbo.GiaoDichThanhToan', N'Ma') IS NOT NULL
BEGIN
    EXEC sp_rename N'dbo.GiaoDichThanhToan.Ma', N'MaGiaoDich', N'COLUMN';
    EXEC(N'ALTER TABLE dbo.GiaoDichThanhToan ADD Ma AS MaGiaoDich;');
END;

-- 4. TẠO LẠI CÁC KHÓA NGOẠI CHUẨN MỰC NỐI TƯỜNG MINH
-- LuotLuuTru -> Phong
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_LuotLuuTru_Phong')
    ALTER TABLE dbo.LuotLuuTru ADD CONSTRAINT FK_LuotLuuTru_Phong FOREIGN KEY (MaPhong) REFERENCES dbo.Phong(MaPhong);

-- LuotLuuTru -> KhachHang
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_LuotLuuTru_KhachHang')
    ALTER TABLE dbo.LuotLuuTru ADD CONSTRAINT FK_LuotLuuTru_KhachHang FOREIGN KEY (MaKhachHang) REFERENCES dbo.KhachHang(MaKhachHang);

-- LuotLuuTru -> TaiKhoanNhanVien
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_LuotLuuTru_NhanVien')
    ALTER TABLE dbo.LuotLuuTru ADD CONSTRAINT FK_LuotLuuTru_NhanVien FOREIGN KEY (MaNhanVien) REFERENCES dbo.TaiKhoanNhanVien(MaNhanVien);

-- YeuCauDichVu -> LuotLuuTru
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_YeuCauDichVu_LuotLuuTru')
    ALTER TABLE dbo.YeuCauDichVu ADD CONSTRAINT FK_YeuCauDichVu_LuotLuuTru FOREIGN KEY (MaLuotLuuTru) REFERENCES dbo.LuotLuuTru(MaLuotLuuTru);

-- YeuCauDichVu -> DichVu
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_YeuCauDichVu_DichVu')
    ALTER TABLE dbo.YeuCauDichVu ADD CONSTRAINT FK_YeuCauDichVu_DichVu FOREIGN KEY (MaDichVu) REFERENCES dbo.DichVu(MaDichVu);

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

-- 5. KIỂM TRA LẠI SỐ LƯỢNG BẢNG
SELECT name AS TenBang, create_date AS NgayTao 
FROM sys.tables 
WHERE name <> N'sysdiagrams'
ORDER BY name;
GO
"""

with open('/sessions/dazzling-gracious-gates/mnt/QLKhachSan/Database/ChuanHoa_7_Bang.sql', 'w', encoding='utf-8') as f:
    f.write(upgrade_sql)

print("Created Database/ChuanHoa_7_Bang.sql!")
