-- SQL Server / SSMS: execute the entire script. Existing databases and rows are preserved.
SET NOCOUNT ON;
USE master;
GO
IF DB_ID(N'QLKhachSanApp') IS NULL CREATE DATABASE QLKhachSanApp;
GO
USE QLKhachSanApp;
GO
SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET ARITHABORT ON;
SET NUMERIC_ROUNDABORT OFF;
BEGIN TRANSACTION;
IF OBJECT_ID(N'dbo.SchemaVersion') IS NULL
BEGIN
CREATE TABLE dbo.SchemaVersion(Version int NOT NULL PRIMARY KEY);
CREATE TABLE dbo.Users(
 Id int IDENTITY PRIMARY KEY, Username nvarchar(50) NOT NULL UNIQUE,
 PasswordHash varbinary(32) NOT NULL, Salt varbinary(16) NOT NULL, Iterations int NOT NULL CHECK(Iterations>=100000),
 Role varchar(20) NOT NULL CHECK(Role IN ('Admin','Reception')), Active bit NOT NULL DEFAULT 1,
 FailedAttempts int NOT NULL DEFAULT 0, LockedUntil datetime2 NULL);
CREATE TABLE dbo.Rooms(
 Id int IDENTITY PRIMARY KEY, Number nvarchar(10) NOT NULL UNIQUE,
 Type nvarchar(20) NOT NULL CHECK(Type IN(N'Đơn',N'Đôi',N'VIP')),
 Rate decimal(18,2) NOT NULL CHECK(Rate>0), Deposit decimal(18,2) NOT NULL CHECK(Deposit>=0),
 Status varchar(20) NOT NULL DEFAULT 'Trong' CHECK(Status IN('Trong','DaDat','DangO','DangDon','BaoTri')),
 Version bigint NOT NULL DEFAULT 1);
CREATE TABLE dbo.Customers(
 Id int IDENTITY PRIMARY KEY, Name nvarchar(100) NOT NULL, Phone nvarchar(20) NOT NULL,
 IdentityNumber nvarchar(20) NOT NULL UNIQUE, Created datetime2 NOT NULL DEFAULT SYSDATETIME());
CREATE TABLE dbo.Stays(
 Id bigint IDENTITY PRIMARY KEY, RoomId int NOT NULL REFERENCES dbo.Rooms(Id),
 CustomerId int NOT NULL REFERENCES dbo.Customers(Id),
 GuestName nvarchar(100) NOT NULL, Phone nvarchar(20) NOT NULL, IdentityNumber nvarchar(20) NOT NULL,
 Status varchar(20) NOT NULL CHECK(Status IN('Reserved','Occupied','Paid','Cancelled')),
 IsActive bit NOT NULL, Created datetime2 NOT NULL, Arrival datetime2 NOT NULL, Departure datetime2 NOT NULL,
 CheckIn datetime2 NULL, CheckOut datetime2 NULL, HoldUntil datetime2 NULL,
 Deposit decimal(18,2) NOT NULL CHECK(Deposit>=0), Version bigint NOT NULL DEFAULT 1,
 CreatedBy int NOT NULL REFERENCES dbo.Users(Id),
 CONSTRAINT CK_Stays_Departure CHECK(Departure>COALESCE(CheckIn,Arrival)), CHECK((IsActive=1 AND Status IN('Reserved','Occupied')) OR (IsActive=0 AND Status IN('Paid','Cancelled'))));
CREATE UNIQUE INDEX UX_Stays_ActiveRoom ON dbo.Stays(RoomId) WHERE IsActive=1;
CREATE INDEX IX_Stays_Customer ON dbo.Stays(CustomerId,Status);
CREATE TABLE dbo.StaySegments(
 Id bigint IDENTITY PRIMARY KEY, StayId bigint NOT NULL REFERENCES dbo.Stays(Id),
 RoomId int NOT NULL REFERENCES dbo.Rooms(Id), Started datetime2 NOT NULL, Ended datetime2 NULL,
 Rate decimal(18,2) NOT NULL CHECK(Rate>0), CHECK(Ended IS NULL OR Ended>=Started));
CREATE UNIQUE INDEX UX_Segments_Open ON dbo.StaySegments(StayId) WHERE Ended IS NULL;
CREATE INDEX IX_Segments_Stay ON dbo.StaySegments(StayId);
CREATE TABLE dbo.Services(
 Id int IDENTITY PRIMARY KEY, Category nvarchar(100) NOT NULL, Name nvarchar(150) NOT NULL UNIQUE,
 Price decimal(18,2) NOT NULL CHECK(Price>=0), Unit nvarchar(20) NOT NULL, Active bit NOT NULL DEFAULT 1);
CREATE TABLE dbo.ServiceOrders(
 Id bigint IDENTITY PRIMARY KEY, StayId bigint NOT NULL REFERENCES dbo.Stays(Id),
 ServiceId int NOT NULL REFERENCES dbo.Services(Id), Category nvarchar(100) NOT NULL, Name nvarchar(150) NOT NULL,
 Quantity int NOT NULL CHECK(Quantity BETWEEN 1 AND 100), Price decimal(18,2) NOT NULL CHECK(Price>=0),
 Ordered datetime2 NOT NULL, Delivered datetime2 NULL, CreatedBy int NOT NULL REFERENCES dbo.Users(Id));
CREATE INDEX IX_Orders_Stay ON dbo.ServiceOrders(StayId);
CREATE INDEX IX_Orders_Pending ON dbo.ServiceOrders(Ordered) WHERE Delivered IS NULL;
CREATE TABLE dbo.Invoices(
 Id bigint IDENTITY PRIMARY KEY, StayId bigint NOT NULL UNIQUE REFERENCES dbo.Stays(Id),
 RoomNumber nvarchar(10) NOT NULL, GuestName nvarchar(100) NOT NULL, Issued datetime2 NOT NULL,
 RoomCharge decimal(18,2) NOT NULL CHECK(RoomCharge>=0), ServiceCharge decimal(18,2) NOT NULL CHECK(ServiceCharge>=0),
 Deposit decimal(18,2) NOT NULL CHECK(Deposit>=0), Collected decimal(18,2) NOT NULL CHECK(Collected>=0),
 Refunded decimal(18,2) NOT NULL CHECK(Refunded>=0), Method nvarchar(30) NOT NULL,
 CreatedBy int NOT NULL REFERENCES dbo.Users(Id), CHECK(RoomCharge+ServiceCharge=Deposit+Collected-Refunded));
CREATE INDEX IX_Invoices_Issued ON dbo.Invoices(Issued);
CREATE TABLE dbo.Payments(
 Id bigint IDENTITY PRIMARY KEY, StayId bigint NOT NULL REFERENCES dbo.Stays(Id),
 Kind varchar(20) NOT NULL CHECK(Kind IN('Deposit','Checkout','Refund','Forfeit')),
 Amount decimal(18,2) NOT NULL CHECK(Amount>0), Created datetime2 NOT NULL,
 Method nvarchar(30) NOT NULL, Note nvarchar(300) NOT NULL, CreatedBy int NOT NULL REFERENCES dbo.Users(Id));
CREATE INDEX IX_Payments_Date ON dbo.Payments(Created);
CREATE TABLE dbo.AuditLog(
 Id bigint IDENTITY PRIMARY KEY, UserId int NOT NULL REFERENCES dbo.Users(Id), Action nvarchar(50) NOT NULL,
 Detail nvarchar(500) NOT NULL, Created datetime2 NOT NULL DEFAULT SYSDATETIME());
INSERT dbo.SchemaVersion VALUES(1);
END;
IF NOT EXISTS(SELECT 1 FROM dbo.SchemaVersion WHERE Version=1) THROW 51000, 'Unsupported schema version',1;
-- Compatible correction for installations made with the initial script.
DECLARE @oldDateConstraint sysname;
SELECT @oldDateConstraint=name FROM sys.check_constraints
WHERE parent_object_id=OBJECT_ID(N'dbo.Stays') AND definition=N'([Departure]>[Arrival])';
IF @oldDateConstraint IS NOT NULL
BEGIN
 DECLARE @dropDateConstraint nvarchar(500)=N'ALTER TABLE dbo.Stays DROP CONSTRAINT '+QUOTENAME(@oldDateConstraint);
 EXEC sys.sp_executesql @dropDateConstraint;
END;
IF NOT EXISTS(SELECT 1 FROM sys.check_constraints WHERE parent_object_id=OBJECT_ID(N'dbo.Stays') AND name=N'CK_Stays_Departure')
 ALTER TABLE dbo.Stays ADD CONSTRAINT CK_Stays_Departure CHECK(Departure>COALESCE(CheckIn,Arrival));
IF NOT EXISTS(SELECT 1 FROM dbo.Rooms)
BEGIN
 DECLARE @i int=1;
 WHILE @i<=50
 BEGIN
 INSERT dbo.Rooms(Number,Type,Rate,Deposit) VALUES(
 CONVERT(nvarchar(10),CASE WHEN @i<=20 THEN 100+@i WHEN @i<=40 THEN 200+@i-20 ELSE 300+@i-40 END),
 CASE WHEN @i<=20 THEN N'Đơn' WHEN @i<=40 THEN N'Đôi' ELSE N'VIP' END,
 CASE WHEN @i<=40 THEN 500000 ELSE 1000000 END,CASE WHEN @i<=40 THEN 250000 ELSE 500000 END);
 SET @i+=1;
 END;
END;
COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS(SELECT 1 FROM dbo.Services WHERE Name=N'Nước suối Lavie 500ml') INSERT dbo.Services(Category,Name,Price,Unit) VALUES(N'Nước uống & Giải khát',N'Nước suối Lavie 500ml',15000,N'Chai');
IF NOT EXISTS(SELECT 1 FROM dbo.Services WHERE Name=N'Coca-Cola / Pepsi lon') INSERT dbo.Services(Category,Name,Price,Unit) VALUES(N'Nước uống & Giải khát',N'Coca-Cola / Pepsi lon',25000,N'Lon');
IF NOT EXISTS(SELECT 1 FROM dbo.Services WHERE Name=N'Bò Húc (Red Bull Thái)') INSERT dbo.Services(Category,Name,Price,Unit) VALUES(N'Nước uống & Giải khát',N'Bò Húc (Red Bull Thái)',30000,N'Lon');
IF NOT EXISTS(SELECT 1 FROM dbo.Services WHERE Name=N'Cà phê sữa đá pha phin') INSERT dbo.Services(Category,Name,Price,Unit) VALUES(N'Nước uống & Giải khát',N'Cà phê sữa đá pha phin',35000,N'Ly');
IF NOT EXISTS(SELECT 1 FROM dbo.Services WHERE Name=N'Nước dừa tươi nguyên quả') INSERT dbo.Services(Category,Name,Price,Unit) VALUES(N'Nước uống & Giải khát',N'Nước dừa tươi nguyên quả',40000,N'Quả');
IF NOT EXISTS(SELECT 1 FROM dbo.Services WHERE Name=N'Bia Heineken bạc') INSERT dbo.Services(Category,Name,Price,Unit) VALUES(N'Bia, Rượu & Đồ nhậu',N'Bia Heineken bạc',35000,N'Lon');
IF NOT EXISTS(SELECT 1 FROM dbo.Services WHERE Name=N'Bia Tiger nâu / Crystal') INSERT dbo.Services(Category,Name,Price,Unit) VALUES(N'Bia, Rượu & Đồ nhậu',N'Bia Tiger nâu / Crystal',30000,N'Lon');
IF NOT EXISTS(SELECT 1 FROM dbo.Services WHERE Name=N'Mực một nắng nướng xé sợi') INSERT dbo.Services(Category,Name,Price,Unit) VALUES(N'Bia, Rượu & Đồ nhậu',N'Mực một nắng nướng xé sợi',150000,N'Đĩa');
IF NOT EXISTS(SELECT 1 FROM dbo.Services WHERE Name=N'Khô bò sợi lá chanh loại 1') INSERT dbo.Services(Category,Name,Price,Unit) VALUES(N'Bia, Rượu & Đồ nhậu',N'Khô bò sợi lá chanh loại 1',80000,N'Đĩa');
IF NOT EXISTS(SELECT 1 FROM dbo.Services WHERE Name=N'Đậu phộng tỏi ớt / Hạt điều') INSERT dbo.Services(Category,Name,Price,Unit) VALUES(N'Bia, Rượu & Đồ nhậu',N'Đậu phộng tỏi ớt / Hạt điều',40000,N'Gói');
IF NOT EXISTS(SELECT 1 FROM dbo.Services WHERE Name=N'Chả bò gân cắt lát') INSERT dbo.Services(Category,Name,Price,Unit) VALUES(N'Bia, Rượu & Đồ nhậu',N'Chả bò gân cắt lát',75000,N'Đĩa');
IF NOT EXISTS(SELECT 1 FROM dbo.Services WHERE Name=N'Mì ly Modern / Hảo Hảo tôm chua cay') INSERT dbo.Services(Category,Name,Price,Unit) VALUES(N'Đồ ăn vặt & Đồ ăn nhanh',N'Mì ly Modern / Hảo Hảo tôm chua cay',25000,N'Ly');
IF NOT EXISTS(SELECT 1 FROM dbo.Services WHERE Name=N'Mì tôm xào trứng xúc xích') INSERT dbo.Services(Category,Name,Price,Unit) VALUES(N'Đồ ăn vặt & Đồ ăn nhanh',N'Mì tôm xào trứng xúc xích',45000,N'Đĩa');
IF NOT EXISTS(SELECT 1 FROM dbo.Services WHERE Name=N'Snack khoai tây Lay''s lớn') INSERT dbo.Services(Category,Name,Price,Unit) VALUES(N'Đồ ăn vặt & Đồ ăn nhanh',N'Snack khoai tây Lay''s lớn',30000,N'Gói');
IF NOT EXISTS(SELECT 1 FROM dbo.Services WHERE Name=N'Bánh mì kẹp xúc xích pate') INSERT dbo.Services(Category,Name,Price,Unit) VALUES(N'Đồ ăn vặt & Đồ ăn nhanh',N'Bánh mì kẹp xúc xích pate',35000,N'Cái');
IF NOT EXISTS(SELECT 1 FROM dbo.Services WHERE Name=N'Khô gà xé cay bơ tỏi') INSERT dbo.Services(Category,Name,Price,Unit) VALUES(N'Đồ ăn vặt & Đồ ăn nhanh',N'Khô gà xé cay bơ tỏi',50000,N'Hũ');
IF NOT EXISTS(SELECT 1 FROM dbo.Services WHERE Name=N'Giặt sấy thông thường (dưới 3kg)') INSERT dbo.Services(Category,Name,Price,Unit) VALUES(N'Giặt là & Chăm sóc trang phục',N'Giặt sấy thông thường (dưới 3kg)',60000,N'Lượt');
IF NOT EXISTS(SELECT 1 FROM dbo.Services WHERE Name=N'Ủi / Là phẳng sơ mi, quần âu') INSERT dbo.Services(Category,Name,Price,Unit) VALUES(N'Giặt là & Chăm sóc trang phục',N'Ủi / Là phẳng sơ mi, quần âu',25000,N'Chiếc');
IF NOT EXISTS(SELECT 1 FROM dbo.Services WHERE Name=N'Giặt khô áo vest / Váy dạ hội cao cấp') INSERT dbo.Services(Category,Name,Price,Unit) VALUES(N'Giặt là & Chăm sóc trang phục',N'Giặt khô áo vest / Váy dạ hội cao cấp',120000,N'Bộ');
IF NOT EXISTS(SELECT 1 FROM dbo.Services WHERE Name=N'Thuê xe máy tay ga (24h)') INSERT dbo.Services(Category,Name,Price,Unit) VALUES(N'Tiện ích & Dịch vụ khác',N'Thuê xe máy tay ga (24h)',150000,N'Ngày');
IF NOT EXISTS(SELECT 1 FROM dbo.Services WHERE Name=N'Đưa đón sân bay 4 chỗ') INSERT dbo.Services(Category,Name,Price,Unit) VALUES(N'Tiện ích & Dịch vụ khác',N'Đưa đón sân bay 4 chỗ',250000,N'Chuyến');
IF NOT EXISTS(SELECT 1 FROM dbo.Services WHERE Name=N'Dọn phòng đột xuất theo yêu cầu') INSERT dbo.Services(Category,Name,Price,Unit) VALUES(N'Tiện ích & Dịch vụ khác',N'Dọn phòng đột xuất theo yêu cầu',50000,N'Lần');
COMMIT;
GO

