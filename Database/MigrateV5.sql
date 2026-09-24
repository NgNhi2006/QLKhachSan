SET XACT_ABORT ON;
BEGIN TRANSACTION;
DECLARE @lock int;
EXEC @lock=sys.sp_getapplock @Resource=N'QLKhachSan.Write',@LockMode='Exclusive',@LockOwner='Transaction',@LockTimeout=10000;
IF @lock<0 THROW 51001,N'Không lấy được khóa nâng cấp dữ liệu.',1;
IF NOT EXISTS(SELECT 1 FROM dbo.SchemaVersion WHERE Version=4) THROW 51000,N'Cần nâng cấp phiên bản 4 trước.',1;
IF NOT EXISTS(SELECT 1 FROM dbo.SchemaVersion WHERE Version=5)
BEGIN
    UPDATE dbo.Rooms SET Type=N'VIP'
    WHERE Number IN (N'401',N'402',N'403',N'404',N'405',N'406',N'407',N'408',N'409',N'410');
    INSERT dbo.SchemaVersion VALUES(5);
END;
COMMIT;
