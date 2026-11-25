BEGIN TRANSACTION;
DECLARE @lock19 int;
EXEC @lock19=sys.sp_getapplock @Resource=N'QLKhachSan.Write',@LockMode='Exclusive',@LockOwner='Transaction',@LockTimeout=10000;
IF @lock19<0 THROW 51001,N'Không lấy được khóa nâng cấp dữ liệu.',1;
IF NOT EXISTS(SELECT 1 FROM dbo.PhienBanCSDL WHERE PhienBan=18) THROW 51000,N'Cần nâng cấp phiên bản 18 trước.',1;
IF NOT EXISTS(SELECT 1 FROM dbo.PhienBanCSDL WHERE PhienBan=19)
BEGIN
    IF COL_LENGTH(N'dbo.ChiTietBill',N'ThanhTien') IS NULL
        EXEC(N'ALTER TABLE dbo.ChiTietBill ADD ThanhTien decimal(18,2) NULL');
    EXEC(N'UPDATE dbo.ChiTietBill SET ThanhTien=SoLuong*DonGia WHERE ThanhTien IS NULL');
    EXEC(N'ALTER TABLE dbo.ChiTietBill ALTER COLUMN ThanhTien decimal(18,2) NOT NULL');
    IF NOT EXISTS(SELECT 1 FROM sys.check_constraints WHERE parent_object_id=OBJECT_ID(N'dbo.ChiTietBill') AND name=N'CK_BillDetails_Amount')
        EXEC(N'ALTER TABLE dbo.ChiTietBill ADD CONSTRAINT CK_BillDetails_Amount CHECK(ThanhTien>=0)');
    INSERT dbo.PhienBanCSDL(PhienBan) VALUES(19);
END;
COMMIT;