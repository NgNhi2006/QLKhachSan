SET XACT_ABORT ON;
BEGIN TRANSACTION;
BEGIN TRANSACTION;
DECLARE @lock15 int;
EXEC @lock15=sys.sp_getapplock @Resource=N'QLKhachSan.Write',@LockMode='Exclusive',@LockOwner='Transaction',@LockTimeout=10000;
IF @lock15<0 THROW 51001,N'Không lấy được khóa nâng cấp dữ liệu.',1;
IF NOT EXISTS(SELECT 1 FROM dbo.PhienBanCSDL WHERE PhienBan=14) THROW 51000,N'Cần nâng cấp phiên bản 14 trước.',1;
IF NOT EXISTS(SELECT 1 FROM dbo.PhienBanCSDL WHERE PhienBan=15)
BEGIN
    CREATE TABLE dbo.CauHinhChucNangMoi(
        Ma bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_CustomFunction PRIMARY KEY,
        MaMenuCon bigint NOT NULL REFERENCES dbo.CauHinhMenuCon(Ma),
        TieuDe nvarchar(120) NOT NULL,
        ThuTuHienThi int NOT NULL,
        DangHienThi bit NOT NULL CONSTRAINT DF_CustomFunction_Active DEFAULT 1,
        VaiTro varchar(80) NOT NULL CONSTRAINT DF_CustomFunction_Roles DEFAULT 'Admin',
        CauTruc nvarchar(max) NOT NULL,
        CONSTRAINT CK_CustomFunction_Design CHECK(ISJSON(CauTruc)=1)
    );
    CREATE INDEX IX_CustomFunction_Submenu ON dbo.CauHinhChucNangMoi(MaMenuCon,ThuTuHienThi);
    CREATE TABLE dbo.DuLieuChucNangMoi(
        Ma bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_CustomRecord PRIMARY KEY,
        MaChucNang bigint NOT NULL REFERENCES dbo.CauHinhChucNangMoi(Ma),
        NoiDung nvarchar(max) NOT NULL,
        CapNhatLuc datetime2 NOT NULL CONSTRAINT DF_CustomRecord_Updated DEFAULT SYSUTCDATETIME(),
        CONSTRAINT CK_CustomRecord_Json CHECK(ISJSON(NoiDung)=1)
    );
    CREATE INDEX IX_CustomRecord_Function ON dbo.DuLieuChucNangMoi(MaChucNang,Ma DESC);
    INSERT dbo.PhienBanCSDL(PhienBan) VALUES(15);
END;
COMMIT;
DECLARE @submenu bigint=(SELECT TOP (1) Ma FROM dbo.CauHinhMenuCon ORDER BY Ma);
INSERT dbo.CauHinhChucNangMoi(MaMenuCon,TieuDe,ThuTuHienThi,DangHienThi,VaiTro,CauTruc)
VALUES(@submenu,N'Verification screen',9999,1,'Admin',N'{"Fields":[{"Key":"f1","Label":"Sample","Kind":"text","Required":false,"Wide":true,"Options":""}],"SaveLabel":"Save","NewLabel":"New","DeleteLabel":"Delete"}');
DECLARE @function bigint=SCOPE_IDENTITY();
INSERT dbo.DuLieuChucNangMoi(MaChucNang,NoiDung) VALUES(@function,N'{"f1":"sample"}');
UPDATE dbo.DuLieuChucNangMoi SET NoiDung=N'{"f1":"updated"}' WHERE MaChucNang=@function;
IF NOT EXISTS(SELECT 1 FROM dbo.DuLieuChucNangMoi WHERE MaChucNang=@function AND JSON_VALUE(NoiDung,'$.f1')='updated')
    THROW 51990,N'Custom record verification failed',1;
DELETE dbo.DuLieuChucNangMoi WHERE MaChucNang=@function;
DELETE dbo.CauHinhChucNangMoi WHERE Ma=@function;
SELECT N'PASS: V15 custom function and record SQL' AS Result;
ROLLBACK TRANSACTION;
