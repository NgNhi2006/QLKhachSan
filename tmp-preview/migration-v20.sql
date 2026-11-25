BEGIN TRANSACTION;
DECLARE @lock20 int;
EXEC @lock20=sys.sp_getapplock @Resource=N'QLKhachSan.Write',@LockMode='Exclusive',@LockOwner='Transaction',@LockTimeout=10000;
IF @lock20<0 THROW 51001,N'Không lấy được khóa nâng cấp dữ liệu.',1;
IF NOT EXISTS(SELECT 1 FROM dbo.PhienBanCSDL WHERE PhienBan=19) THROW 51000,N'Cần nâng cấp phiên bản 19 trước.',1;
IF NOT EXISTS(SELECT 1 FROM dbo.PhienBanCSDL WHERE PhienBan=20)
BEGIN
    CREATE TABLE dbo.MaGiamGia (
        MaCode varchar(32) NOT NULL PRIMARY KEY,
        Kieu varchar(10) NOT NULL CHECK(Kieu IN('Percent','Amount')),
        GiaTri decimal(18,2) NOT NULL CHECK(GiaTri>0),
        BatDau datetime2 NOT NULL,
        KetThuc datetime2 NOT NULL,
        GioiHan int NULL CHECK(GioiHan>0),
        DaDung int NOT NULL CONSTRAINT DF_Discount_Used DEFAULT(0) CHECK(DaDung>=0),
        DangHoatDong bit NOT NULL CONSTRAINT DF_Discount_Active DEFAULT(1),
        CONSTRAINT CK_Discount_Range CHECK(BatDau<KetThuc),
        CONSTRAINT CK_Discount_Value CHECK((Kieu='Percent' AND GiaTri<=100) OR (Kieu='Amount' AND GiaTri<=1000000000)),
        CONSTRAINT CK_Discount_Limit CHECK(GioiHan IS NULL OR DaDung<=GioiHan)
    );
    CREATE TABLE dbo.SuDungMaGiamGia (
        MaHoaDon bigint NOT NULL PRIMARY KEY REFERENCES dbo.HoaDon(Ma),
        MaCode varchar(32) NOT NULL REFERENCES dbo.MaGiamGia(MaCode),
        SoTien decimal(18,2) NOT NULL CHECK(SoTien>0),
        ThoiDiem datetime2 NOT NULL CONSTRAINT DF_DiscountUse_Time DEFAULT(SYSDATETIME())
    );
    CREATE INDEX IX_DiscountUse_Code ON dbo.SuDungMaGiamGia(MaCode);
    INSERT dbo.PhienBanCSDL(PhienBan) VALUES(20);
END;
COMMIT;

