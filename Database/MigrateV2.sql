-- Run in the application database after Setup.sql. Atomic, repeatable upgrade.
SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET ARITHABORT ON;
SET NUMERIC_ROUNDABORT OFF;
BEGIN TRANSACTION;
DECLARE @lock int;
EXEC @lock=sys.sp_getapplock @Resource=N'QLKhachSan.Write',@LockMode='Exclusive',@LockOwner='Transaction',@LockTimeout=10000;
IF @lock<0 THROW 51001,N'Không lấy được khóa nâng cấp dữ liệu.',1;
IF OBJECT_ID(N'dbo.SchemaVersion') IS NULL THROW 51000,N'Hãy chạy Database/Setup.sql trước.',1;
IF EXISTS(SELECT 1 FROM dbo.SchemaVersion WHERE Version>2) THROW 51000,N'Ứng dụng cũ hơn phiên bản dữ liệu.',1;
IF NOT EXISTS(SELECT 1 FROM dbo.SchemaVersion WHERE Version=2)
BEGIN
    ALTER TABLE dbo.Users ADD SecurityVersion bigint NOT NULL CONSTRAINT DF_Users_SecurityVersion DEFAULT 1;
    ALTER TABLE dbo.Services ADD Version bigint NOT NULL CONSTRAINT DF_Services_Version DEFAULT 1;
    ALTER TABLE dbo.ServiceOrders ADD Cancelled datetime2 NULL, CancelReason nvarchar(300) NULL,
        DeliveredQuantity int NOT NULL CONSTRAINT DF_Orders_DeliveredQuantity DEFAULT 0;
    EXEC(N'UPDATE dbo.ServiceOrders SET DeliveredQuantity=Quantity WHERE Delivered IS NOT NULL;
        ALTER TABLE dbo.ServiceOrders ADD CONSTRAINT CK_Orders_DeliveredQuantity CHECK(DeliveredQuantity>=0 AND DeliveredQuantity<=Quantity);');
    DROP INDEX UX_Stays_ActiveRoom ON dbo.Stays;
    CREATE UNIQUE INDEX UX_Stays_OccupiedRoom ON dbo.Stays(RoomId) WHERE Status='Occupied';
    CREATE INDEX IX_Stays_ReservationDates ON dbo.Stays(RoomId,Arrival,Departure) INCLUDE(Status,IsActive,HoldUntil);
    CREATE UNIQUE INDEX UX_Payments_Forfeit ON dbo.Payments(StayId) WHERE Kind='Forfeit';
    -- Reservations no longer describe the physical room status. Keep existing deadlines.
    UPDATE dbo.Rooms SET Status='Trong',Version=Version+1 WHERE Status='DaDat';
    INSERT dbo.SchemaVersion VALUES(2);
END;
COMMIT;
