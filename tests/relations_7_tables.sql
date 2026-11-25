-- Chỉ chạy trên bản sao database thử nghiệm, không chạy trên QLKhachSanApp.
SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;
IF DB_NAME() NOT LIKE N'%[_]Verify[_]%'
    THROW 51900, N'Chỉ chạy kiểm thử trên database _Verify_.', 1;

DECLARE @room int=(SELECT TOP(1) MaPhong FROM dbo.Phong ORDER BY MaPhong);
DECLARE @customer int=(SELECT TOP(1) MaKhachHang FROM dbo.KhachHang ORDER BY MaKhachHang);
DECLARE @employee int=(SELECT TOP(1) MaNhanVien FROM dbo.TaiKhoanNhanVien WHERE DangHoatDong=1 ORDER BY MaNhanVien);
IF @room IS NULL OR @customer IS NULL OR @employee IS NULL THROW 51901, N'Thiếu dữ liệu nền để kiểm thử.', 1;
DECLARE @first bigint,@second bigint;

INSERT dbo.LuotLuuTru(MaPhong,MaKhachHang,TenKhach,SoDienThoai,SoGiayTo,TrangThai,ConHieuLuc,ThoiDiemTao,ThoiDiemDen,ThoiDiemDi,HanGiuPhong,TienCoc,MaNhanVien)
SELECT @room,@customer,Ten,SoDienThoai,SoGiayTo,'Reserved',1,SYSDATETIME(),'2035-01-01T00:00:00','2035-01-02T00:00:00','2035-01-01T12:00:00',0,@employee FROM dbo.KhachHang WHERE MaKhachHang=@customer;
SET @first=CONVERT(bigint,SCOPE_IDENTITY());
INSERT dbo.LuotLuuTru(MaPhong,MaKhachHang,TenKhach,SoDienThoai,SoGiayTo,TrangThai,ConHieuLuc,ThoiDiemTao,ThoiDiemDen,ThoiDiemDi,HanGiuPhong,TienCoc,MaNhanVien)
SELECT @room,@customer,Ten,SoDienThoai,SoGiayTo,'Reserved',1,SYSDATETIME(),'2035-01-02T00:00:00','2035-01-03T00:00:00','2035-01-02T12:00:00',0,@employee FROM dbo.KhachHang WHERE MaKhachHang=@customer;
SET @second=CONVERT(bigint,SCOPE_IDENTITY());
PRINT N'PASS: hai lượt liên tiếp cùng phòng';

BEGIN TRY
    INSERT dbo.LuotLuuTru(MaPhong,MaKhachHang,TenKhach,SoDienThoai,SoGiayTo,TrangThai,ConHieuLuc,ThoiDiemTao,ThoiDiemDen,ThoiDiemDi,HanGiuPhong,TienCoc,MaNhanVien)
    SELECT @room,@customer,Ten,SoDienThoai,SoGiayTo,'Reserved',1,SYSDATETIME(),'2035-01-01T12:00:00','2035-01-02T12:00:00','2035-01-02T00:00:00',0,@employee FROM dbo.KhachHang WHERE MaKhachHang=@customer;
    THROW 51902, N'LỖI: tạo lượt chồng giờ không bị chặn.', 1;
END TRY
BEGIN CATCH
    IF ERROR_NUMBER()<>51011 THROW;
    PRINT N'PASS: chặn tạo lượt chồng giờ';
END CATCH;

BEGIN TRY
    UPDATE dbo.LuotLuuTru SET ThoiDiemDen='2035-01-01T12:00:00',ThoiDiemDi='2035-01-02T12:00:00' WHERE MaLuotLuuTru=@second;
    THROW 51903, N'LỖI: sửa lượt thành chồng giờ không bị chặn.', 1;
END TRY
BEGIN CATCH
    IF ERROR_NUMBER()<>51011 THROW;
    PRINT N'PASS: chặn sửa lượt thành chồng giờ';
END CATCH;

IF EXISTS(SELECT 1 FROM dbo.HoaDon WHERE MaLuotLuuTru=@first) THROW 51904, N'LỖI: lượt mới đã có hóa đơn.', 1;
PRINT N'PASS: lượt có thể chưa có hóa đơn';

INSERT dbo.HoaDon(MaLuotLuuTru,SoPhongHoaDon,TenKhach,ThoiDiemLapHoaDon,TienPhong,TienDichVu,TienCoc,TienDaThu,TienDaHoan,PhuongThucThanhToan,MaNhanVien)
SELECT @first,p.SoPhong,s.TenKhach,SYSDATETIME(),0,0,0,0,0,N'Tiền mặt',@employee FROM dbo.LuotLuuTru s JOIN dbo.Phong p ON p.MaPhong=s.MaPhong WHERE s.MaLuotLuuTru=@first;
BEGIN TRY
    INSERT dbo.HoaDon(MaLuotLuuTru,SoPhongHoaDon,TenKhach,ThoiDiemLapHoaDon,TienPhong,TienDichVu,TienCoc,TienDaThu,TienDaHoan,PhuongThucThanhToan,MaNhanVien)
    SELECT @first,p.SoPhong,s.TenKhach,SYSDATETIME(),0,0,0,0,0,N'Tiền mặt',@employee FROM dbo.LuotLuuTru s JOIN dbo.Phong p ON p.MaPhong=s.MaPhong WHERE s.MaLuotLuuTru=@first;
    THROW 51905, N'LỖI: hóa đơn thứ hai không bị chặn.', 1;
END TRY
BEGIN CATCH
    IF ERROR_NUMBER() NOT IN (2601,2627) THROW;
    PRINT N'PASS: chặn hóa đơn thứ hai';
END CATCH;

INSERT dbo.CaTruc(MaNhanVien,TienMatDauCa,ThoiDiemDongCa,TienMatKiemDem,TrangThai)
VALUES(@employee,0,SYSDATETIME(),0,'Submitted'),(@employee,0,SYSDATETIME(),0,'Submitted');
IF (SELECT COUNT(*) FROM dbo.CaTruc WHERE MaNhanVien=@employee AND TrangThai='Submitted')<2 THROW 51906, N'LỖI: không lưu được hai ca.', 1;
PRINT N'PASS: một nhân viên nhiều ca';

INSERT dbo.GiaoDichThanhToan(MaLuotLuuTru,LoaiGiaoDich,SoTien,ThoiDiemTao,PhuongThucThanhToan,GhiChu,MaNhanVien)
VALUES(@first,'Deposit',10000,SYSDATETIME(),N'Tiền mặt',N'Kiểm thử',@employee),(@first,'Deposit',15000,SYSDATETIME(),N'Chuyển khoản',N'Kiểm thử',@employee);
IF (SELECT COUNT(*) FROM dbo.GiaoDichThanhToan WHERE MaLuotLuuTru=@first)<2 THROW 51907, N'LỖI: không lưu được hai giao dịch.', 1;
PRINT N'PASS: một lượt nhiều giao dịch và nhiều phương thức';
