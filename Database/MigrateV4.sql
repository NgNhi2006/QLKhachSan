SET XACT_ABORT ON;
BEGIN TRANSACTION;
DECLARE @lock int;
EXEC @lock=sys.sp_getapplock @Resource=N'QLKhachSan.Write',@LockMode='Exclusive',@LockOwner='Transaction',@LockTimeout=10000;
IF @lock<0 THROW 51001,N'Không lấy được khóa nâng cấp dữ liệu.',1;
IF NOT EXISTS(SELECT 1 FROM dbo.SchemaVersion WHERE Version=3) THROW 51000,N'Cần nâng cấp phiên bản 3 trước.',1;
IF NOT EXISTS(SELECT 1 FROM dbo.SchemaVersion WHERE Version=4)
BEGIN
    DECLARE @typeConstraint sysname;
    SELECT TOP(1) @typeConstraint=name FROM sys.check_constraints
    WHERE parent_object_id=OBJECT_ID(N'dbo.Rooms') AND definition LIKE N'%Type%' AND definition LIKE N'%VIP%';
    IF @typeConstraint IS NOT NULL
    BEGIN
        DECLARE @dropType nvarchar(300)=N'ALTER TABLE dbo.Rooms DROP CONSTRAINT '+QUOTENAME(@typeConstraint);
        EXEC sys.sp_executesql @dropType;
    END;
    ALTER TABLE dbo.Rooms ADD CONSTRAINT CK_Rooms_TypeV4 CHECK(Type IN(N'Đơn',N'Đôi',N'VIP',N'Tình nhân'));
    DECLARE @number int=401,@created int=0;
    WHILE @created<10
    BEGIN
        IF NOT EXISTS(SELECT 1 FROM dbo.Rooms WHERE Number=CONVERT(nvarchar(10),@number))
        BEGIN
            INSERT dbo.Rooms(Number,Type,Rate,Deposit) VALUES(CONVERT(nvarchar(10),@number),N'Tình nhân',750000,350000);
            SET @created+=1;
        END;
        SET @number+=1;
    END;
    INSERT dbo.SchemaVersion VALUES(4);
END;
COMMIT;
