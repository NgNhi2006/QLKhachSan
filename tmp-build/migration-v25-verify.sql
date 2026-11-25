SET XACT_ABORT ON;
BEGIN TRANSACTION;
DECLARE @lock25 int;
EXEC @lock25=sys.sp_getapplock @Resource=N'QLKhachSan.Write',@LockMode='Exclusive',@LockOwner='Transaction',@LockTimeout=10000;
IF @lock25<0 THROW 51001,N'Không lấy được khóa nâng cấp dữ liệu.',1;
IF NOT EXISTS(SELECT 1 FROM dbo.PhienBanCSDL WHERE PhienBan=24) THROW 51000,N'Cần nâng cấp phiên bản 24 trước.',1;
IF NOT EXISTS(SELECT 1 FROM dbo.PhienBanCSDL WHERE PhienBan=25)
BEGIN
    -- ChucNang đã chứa toàn bộ mã, menu và thứ tự. Chín bảng dưới đây chỉ
    -- lặp lại các thuộc tính đó và không lưu dữ liệu nghiệp vụ riêng.
    IF EXISTS (SELECT 1 FROM dbo.ChucNangQuanLyPhong WHERE MaChucNang NOT IN (SELECT MaChucNang FROM dbo.ChucNang))
        THROW 51008,N'Dữ liệu chức năng phòng không khớp.',1;
    DROP TABLE dbo.ChucNangQuanLyPhong;
    DROP TABLE dbo.ChucNangDichVu;
    DROP TABLE dbo.ChucNangKhachHang;
    DROP TABLE dbo.ChucNangCaTruc;
    DROP TABLE dbo.ChucNangThuChi;
    DROP TABLE dbo.ChucNangHoaDon;
    DROP TABLE dbo.ChucNangBaoCao;
    DROP TABLE dbo.ChucNangNhanVien;
    DROP TABLE dbo.ChucNangHeThong;
    -- Menu đang hiển thị lấy từ CauHinhMenu/CauHinhMenuCon. Hai bảng menu
    -- gốc này chỉ còn giữ ràng buộc lịch sử cho ChucNang.
    ALTER TABLE dbo.ChucNang DROP CONSTRAINT FK_AppFunctions_Submenu;
    DROP TABLE dbo.MenuCon;
    DROP TABLE dbo.MenuTong;
    INSERT dbo.PhienBanCSDL(PhienBan) VALUES(25);
END;
COMMIT;
