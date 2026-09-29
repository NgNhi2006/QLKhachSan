SET XACT_ABORT ON;
BEGIN TRANSACTION;
DECLARE @lock int;
EXEC @lock=sys.sp_getapplock @Resource=N'QLKhachSan.Write',@LockMode='Exclusive',@LockOwner='Transaction',@LockTimeout=10000;
IF @lock<0 THROW 51001,N'Không lấy được khóa nâng cấp dữ liệu.',1;
IF NOT EXISTS(SELECT 1 FROM dbo.SchemaVersion WHERE Version=5) THROW 51000,N'Cần nâng cấp phiên bản 5 trước.',1;
IF NOT EXISTS(SELECT 1 FROM dbo.SchemaVersion WHERE Version=6)
BEGIN
 CREATE TABLE dbo.AccountingPeriods(
  PeriodStart date NOT NULL PRIMARY KEY,
  LockedAt datetime2 NOT NULL DEFAULT SYSDATETIME(),
  LockedBy int NOT NULL REFERENCES dbo.Users(Id),
  Note nvarchar(300) NOT NULL);
 CREATE TABLE dbo.CashShifts(
  Id bigint IDENTITY PRIMARY KEY, CashierId int NOT NULL REFERENCES dbo.Users(Id),
  OpenedAt datetime2 NOT NULL DEFAULT SYSDATETIME(), ClosedAt datetime2 NULL,
  OpeningCash decimal(18,2) NOT NULL CHECK(OpeningCash>=0),
  CountedCash decimal(18,2) NULL CHECK(CountedCash>=0),
  ExpectedCash decimal(18,2) NULL, Explanation nvarchar(500) NULL,
  Status varchar(12) NOT NULL DEFAULT 'Open' CHECK(Status IN('Open','Submitted','Locked')),
  LockedAt datetime2 NULL, LockedBy int NULL REFERENCES dbo.Users(Id),
  CONSTRAINT CK_CashShifts_Close CHECK((Status='Open' AND ClosedAt IS NULL AND CountedCash IS NULL) OR (Status<>'Open' AND ClosedAt IS NOT NULL AND CountedCash IS NOT NULL)));
 CREATE UNIQUE INDEX UX_CashShifts_Open ON dbo.CashShifts(CashierId) WHERE Status='Open';
 ALTER TABLE dbo.Payments ADD ShiftId bigint NULL REFERENCES dbo.CashShifts(Id), ExternalReference nvarchar(100) NULL;
 ALTER TABLE dbo.ServiceOrders ADD ShiftId bigint NULL REFERENCES dbo.CashShifts(Id);
 -- Compile these statements after ALTER TABLE; SQL Server otherwise binds the old column metadata.
 EXEC(N'CREATE INDEX IX_Payments_Shift ON dbo.Payments(ShiftId) WHERE ShiftId IS NOT NULL');
 EXEC(N'CREATE UNIQUE INDEX UX_Payments_ExternalReference ON dbo.Payments(Method,ExternalReference) WHERE ExternalReference IS NOT NULL');
 CREATE TABLE dbo.FinanceVouchers(
  Id bigint IDENTITY PRIMARY KEY, VoucherType varchar(10) NOT NULL CHECK(VoucherType IN('Receipt','Payment')),
  PostedAt datetime2 NOT NULL DEFAULT SYSDATETIME(), Channel varchar(12) NOT NULL CHECK(Channel IN('Cash','Bank','POS','OTA')),
  Category nvarchar(80) NOT NULL, Amount decimal(18,2) NOT NULL CHECK(Amount>0),
  Counterparty nvarchar(150) NOT NULL, Reference nvarchar(100) NULL,
  Note nvarchar(500) NOT NULL, CreatedBy int NOT NULL REFERENCES dbo.Users(Id),
  ShiftId bigint NULL REFERENCES dbo.CashShifts(Id), ReversedAt datetime2 NULL,
  ReversedBy int NULL REFERENCES dbo.Users(Id), ReversalReason nvarchar(500) NULL);
 CREATE INDEX IX_FinanceVouchers_Posted ON dbo.FinanceVouchers(PostedAt,Channel);
 CREATE TABLE dbo.FinanceOpeningBalances(
  Channel varchar(12) NOT NULL PRIMARY KEY CHECK(Channel IN('Cash','Bank','POS','OTA')),
  AsOf datetime2 NOT NULL, Amount decimal(18,2) NOT NULL,
  SetAt datetime2 NOT NULL DEFAULT SYSDATETIME(), SetBy int NOT NULL REFERENCES dbo.Users(Id),
  Note nvarchar(300) NOT NULL);
 CREATE UNIQUE INDEX UX_FinanceVouchers_Reference ON dbo.FinanceVouchers(Channel,Reference) WHERE Reference IS NOT NULL AND ReversedAt IS NULL;
 CREATE TABLE dbo.BankStatementLines(
  Id bigint IDENTITY PRIMARY KEY, OccurredAt datetime2 NOT NULL,
  Channel varchar(4) NOT NULL CHECK(Channel IN('Bank','POS')),
  Reference nvarchar(100) NOT NULL, Amount decimal(18,2) NOT NULL CHECK(Amount<>0),
  ImportedAt datetime2 NOT NULL DEFAULT SYSDATETIME(), ImportedBy int NOT NULL REFERENCES dbo.Users(Id),
  MatchedPaymentId bigint NULL REFERENCES dbo.Payments(Id),
  MatchedVoucherId bigint NULL REFERENCES dbo.FinanceVouchers(Id),
  CONSTRAINT UX_BankStatementLines_Reference UNIQUE(Channel,Reference),
  CONSTRAINT CK_BankStatementLines_Match CHECK(MatchedPaymentId IS NULL OR MatchedVoucherId IS NULL));
 CREATE TABLE dbo.FinanceDebts(
  Id bigint IDENTITY PRIMARY KEY, DebtType varchar(2) NOT NULL CHECK(DebtType IN('AR','AP')),
  Counterparty nvarchar(150) NOT NULL, InvoiceId bigint NULL REFERENCES dbo.Invoices(Id),
  IssuedAt datetime2 NOT NULL DEFAULT SYSDATETIME(), DueAt date NOT NULL,
  Amount decimal(18,2) NOT NULL CHECK(Amount>0), Note nvarchar(500) NOT NULL,
  CreatedBy int NOT NULL REFERENCES dbo.Users(Id),
  CancelledAt datetime2 NULL, CancelledBy int NULL REFERENCES dbo.Users(Id), CancelReason nvarchar(500) NULL);
 CREATE INDEX IX_FinanceDebts_Due ON dbo.FinanceDebts(DebtType,DueAt);
 CREATE TABLE dbo.DebtAllocations(
  Id bigint IDENTITY PRIMARY KEY, DebtId bigint NOT NULL REFERENCES dbo.FinanceDebts(Id),
  VoucherId bigint NOT NULL REFERENCES dbo.FinanceVouchers(Id),
  Amount decimal(18,2) NOT NULL CHECK(Amount>0), CreatedAt datetime2 NOT NULL DEFAULT SYSDATETIME(),
  CreatedBy int NOT NULL REFERENCES dbo.Users(Id), CONSTRAINT UX_DebtAllocations UNIQUE(DebtId,VoucherId));
 CREATE TABLE dbo.StockItems(
  Id int IDENTITY PRIMARY KEY, ServiceId int NULL UNIQUE REFERENCES dbo.Services(Id),
  Name nvarchar(150) NOT NULL UNIQUE, Unit nvarchar(30) NOT NULL,
  Quantity decimal(18,3) NOT NULL DEFAULT 0 CHECK(Quantity>=0),
  AverageCost decimal(18,2) NOT NULL DEFAULT 0 CHECK(AverageCost>=0),
  ReorderLevel decimal(18,3) NOT NULL DEFAULT 0 CHECK(ReorderLevel>=0), Active bit NOT NULL DEFAULT 1);
 CREATE TABLE dbo.StockMovements(
  Id bigint IDENTITY PRIMARY KEY, ItemId int NOT NULL REFERENCES dbo.StockItems(Id),
  HappenedAt datetime2 NOT NULL DEFAULT SYSDATETIME(), Kind varchar(12) NOT NULL CHECK(Kind IN('Purchase','Sale','Spoilage','Internal','Adjustment')),
  Quantity decimal(18,3) NOT NULL CHECK(Quantity<>0), UnitCost decimal(18,2) NOT NULL CHECK(UnitCost>=0),
  ServiceOrderId bigint NULL REFERENCES dbo.ServiceOrders(Id),
  Reason nvarchar(500) NOT NULL, CreatedBy int NOT NULL REFERENCES dbo.Users(Id));
 CREATE INDEX IX_StockMovements_Date ON dbo.StockMovements(HappenedAt,ItemId);
 CREATE TABLE dbo.HousekeepingConsumption(
  Id bigint IDENTITY PRIMARY KEY, ItemId int NOT NULL REFERENCES dbo.StockItems(Id),
  StayId bigint NULL REFERENCES dbo.Stays(Id), ReportedAt datetime2 NOT NULL DEFAULT SYSDATETIME(),
  Quantity decimal(18,3) NOT NULL CHECK(Quantity>0), Note nvarchar(300) NOT NULL,
  CreatedBy int NOT NULL REFERENCES dbo.Users(Id));
 CREATE TABLE dbo.InvoiceFinance(
  InvoiceId bigint NOT NULL PRIMARY KEY REFERENCES dbo.Invoices(Id),
  VatRate decimal(5,2) NOT NULL DEFAULT 0 CHECK(VatRate IN(0,8,10)),
  ServiceRate decimal(5,2) NOT NULL DEFAULT 0 CHECK(ServiceRate IN(0,5)),
  RoundingUnit int NOT NULL DEFAULT 1 CHECK(RoundingUnit IN(1,100,500,1000)),
  EInvoiceNumber nvarchar(100) NULL UNIQUE, EInvoiceIssuedAt datetime2 NULL,
  CONSTRAINT CK_InvoiceFinance_EInvoice CHECK((EInvoiceNumber IS NULL AND EInvoiceIssuedAt IS NULL) OR (EInvoiceNumber IS NOT NULL AND EInvoiceIssuedAt IS NOT NULL)));
 CREATE TABLE dbo.InvoiceAdjustments(
  Id bigint IDENTITY PRIMARY KEY, InvoiceId bigint NOT NULL REFERENCES dbo.Invoices(Id),
  Category varchar(20) NOT NULL CHECK(Category IN('ServiceFailure','VIP','Voucher')),
  Amount decimal(18,2) NOT NULL CHECK(Amount>0), Reason nvarchar(500) NOT NULL,
  ApprovedBy int NOT NULL REFERENCES dbo.Users(Id), CreatedAt datetime2 NOT NULL DEFAULT SYSDATETIME());
 CREATE TABLE dbo.InvoiceVoids(
  InvoiceId bigint NOT NULL PRIMARY KEY REFERENCES dbo.Invoices(Id),
  ReasonCode varchar(20) NOT NULL CHECK(ReasonCode IN('GuestCancelled','WrongRoomType','RoomChange')),
  Explanation nvarchar(500) NOT NULL, VoidedAt datetime2 NOT NULL DEFAULT SYSDATETIME(),
  VoidedBy int NOT NULL REFERENCES dbo.Users(Id));
 CREATE TABLE dbo.BillGroups(
  Id bigint IDENTITY PRIMARY KEY, Name nvarchar(150) NOT NULL,
  CreatedAt datetime2 NOT NULL DEFAULT SYSDATETIME(), CreatedBy int NOT NULL REFERENCES dbo.Users(Id));
 CREATE TABLE dbo.BillShares(
  Id bigint IDENTITY PRIMARY KEY, GroupId bigint NOT NULL REFERENCES dbo.BillGroups(Id),
  InvoiceId bigint NOT NULL REFERENCES dbo.Invoices(Id), Payer nvarchar(150) NOT NULL,
  Amount decimal(18,2) NOT NULL CHECK(Amount>0));
 CREATE INDEX IX_BillShares_Invoice ON dbo.BillShares(InvoiceId);
 INSERT dbo.SchemaVersion VALUES(6);
END;
-- Database guards protect every workstation, including old clients. Historical rows remain readable.
EXEC(N'CREATE OR ALTER TRIGGER dbo.TR_Payments_AccountingGuard ON dbo.Payments AFTER INSERT,UPDATE,DELETE AS
BEGIN
 SET NOCOUNT ON;
 IF EXISTS(SELECT 1 FROM inserted WHERE EXISTS(SELECT 1 FROM dbo.AccountingPeriods p WHERE p.PeriodStart=DATEFROMPARTS(YEAR(inserted.Created),MONTH(inserted.Created),1)))
 OR EXISTS(SELECT 1 FROM deleted WHERE EXISTS(SELECT 1 FROM dbo.AccountingPeriods p WHERE p.PeriodStart=DATEFROMPARTS(YEAR(deleted.Created),MONTH(deleted.Created),1)))
 THROW 51101,N''Kỳ kế toán đã khóa.'',1;
 IF EXISTS(SELECT 1 FROM inserted i JOIN dbo.CashShifts s ON s.Id=i.ShiftId WHERE s.Status=''Locked'')
 OR EXISTS(SELECT 1 FROM deleted d JOIN dbo.CashShifts s ON s.Id=d.ShiftId WHERE s.Status=''Locked'')
 THROW 51102,N''Ca đã khóa.'',1;
END');
EXEC(N'CREATE OR ALTER TRIGGER dbo.TR_Invoices_AccountingGuard ON dbo.Invoices AFTER INSERT,UPDATE,DELETE AS
BEGIN
 SET NOCOUNT ON;
 IF EXISTS(SELECT 1 FROM inserted i JOIN dbo.AccountingPeriods p ON p.PeriodStart=DATEFROMPARTS(YEAR(i.Issued),MONTH(i.Issued),1))
 OR EXISTS(SELECT 1 FROM deleted d JOIN dbo.AccountingPeriods p ON p.PeriodStart=DATEFROMPARTS(YEAR(d.Issued),MONTH(d.Issued),1))
 THROW 51101,N''Kỳ kế toán đã khóa.'',1;
 IF EXISTS(SELECT 1 FROM deleted d JOIN dbo.Payments x ON x.StayId=d.StayId JOIN dbo.CashShifts s ON s.Id=x.ShiftId WHERE s.Status=''Locked'')
 THROW 51102,N''Hóa đơn thuộc ca đã khóa.'',1;
END');
EXEC(N'CREATE OR ALTER TRIGGER dbo.TR_Vouchers_AccountingGuard ON dbo.FinanceVouchers AFTER INSERT,UPDATE,DELETE AS
BEGIN
 SET NOCOUNT ON;
 IF EXISTS(SELECT 1 FROM inserted i JOIN dbo.AccountingPeriods p ON p.PeriodStart=DATEFROMPARTS(YEAR(i.PostedAt),MONTH(i.PostedAt),1))
 OR EXISTS(SELECT 1 FROM deleted d JOIN dbo.AccountingPeriods p ON p.PeriodStart=DATEFROMPARTS(YEAR(d.PostedAt),MONTH(d.PostedAt),1))
 THROW 51101,N''Kỳ kế toán đã khóa.'',1;
 IF EXISTS(SELECT 1 FROM inserted i JOIN dbo.CashShifts s ON s.Id=i.ShiftId WHERE s.Status=''Locked'')
 OR EXISTS(SELECT 1 FROM deleted d JOIN dbo.CashShifts s ON s.Id=d.ShiftId WHERE s.Status=''Locked'')
 THROW 51102,N''Ca đã khóa.'',1;
END');
EXEC(N'CREATE OR ALTER TRIGGER dbo.TR_Stock_AccountingGuard ON dbo.StockMovements AFTER INSERT,UPDATE,DELETE AS
BEGIN
 SET NOCOUNT ON;
 IF EXISTS(SELECT 1 FROM inserted i JOIN dbo.AccountingPeriods p ON p.PeriodStart=DATEFROMPARTS(YEAR(i.HappenedAt),MONTH(i.HappenedAt),1))
 OR EXISTS(SELECT 1 FROM deleted d JOIN dbo.AccountingPeriods p ON p.PeriodStart=DATEFROMPARTS(YEAR(d.HappenedAt),MONTH(d.HappenedAt),1))
 THROW 51101,N''Kỳ kế toán đã khóa.'',1;
END');
EXEC(N'CREATE OR ALTER TRIGGER dbo.TR_Debts_AccountingGuard ON dbo.FinanceDebts AFTER INSERT,UPDATE,DELETE AS
BEGIN
 SET NOCOUNT ON;
 IF EXISTS(SELECT 1 FROM inserted i JOIN dbo.AccountingPeriods p ON p.PeriodStart=DATEFROMPARTS(YEAR(i.IssuedAt),MONTH(i.IssuedAt),1))
 OR EXISTS(SELECT 1 FROM deleted d JOIN dbo.AccountingPeriods p ON p.PeriodStart=DATEFROMPARTS(YEAR(d.IssuedAt),MONTH(d.IssuedAt),1))
 THROW 51101,N''Kỳ kế toán đã khóa.'',1;
END');
EXEC(N'CREATE OR ALTER TRIGGER dbo.TR_Orders_AccountingGuard ON dbo.ServiceOrders AFTER INSERT,UPDATE,DELETE AS
BEGIN
 SET NOCOUNT ON;
 IF EXISTS(SELECT 1 FROM inserted i JOIN dbo.AccountingPeriods p ON p.PeriodStart=DATEFROMPARTS(YEAR(i.Ordered),MONTH(i.Ordered),1))
 OR EXISTS(SELECT 1 FROM deleted d JOIN dbo.AccountingPeriods p ON p.PeriodStart=DATEFROMPARTS(YEAR(d.Ordered),MONTH(d.Ordered),1))
 THROW 51101,N''Kỳ kế toán đã khóa.'',1;
 IF EXISTS(SELECT 1 FROM inserted i JOIN dbo.CashShifts s ON s.Id=i.ShiftId WHERE s.Status=''Locked'')
 OR EXISTS(SELECT 1 FROM deleted d JOIN dbo.CashShifts s ON s.Id=d.ShiftId WHERE s.Status=''Locked'')
 THROW 51102,N''Dịch vụ thuộc ca đã khóa.'',1;
END');
EXEC(N'CREATE OR ALTER TRIGGER dbo.TR_Adjustments_AccountingGuard ON dbo.InvoiceAdjustments AFTER INSERT,UPDATE,DELETE AS
BEGIN
 SET NOCOUNT ON;
 IF EXISTS(SELECT 1 FROM inserted i JOIN dbo.Invoices v ON v.Id=i.InvoiceId JOIN dbo.AccountingPeriods p ON p.PeriodStart=DATEFROMPARTS(YEAR(v.Issued),MONTH(v.Issued),1))
 OR EXISTS(SELECT 1 FROM deleted d JOIN dbo.Invoices v ON v.Id=d.InvoiceId JOIN dbo.AccountingPeriods p ON p.PeriodStart=DATEFROMPARTS(YEAR(v.Issued),MONTH(v.Issued),1))
 THROW 51101,N''Kỳ kế toán đã khóa.'',1;
END');
EXEC(N'CREATE OR ALTER TRIGGER dbo.TR_Voids_AccountingGuard ON dbo.InvoiceVoids AFTER INSERT,UPDATE,DELETE AS
BEGIN
 SET NOCOUNT ON;
 IF EXISTS(SELECT 1 FROM inserted i JOIN dbo.Invoices v ON v.Id=i.InvoiceId JOIN dbo.AccountingPeriods p ON p.PeriodStart=DATEFROMPARTS(YEAR(v.Issued),MONTH(v.Issued),1))
 OR EXISTS(SELECT 1 FROM deleted d JOIN dbo.Invoices v ON v.Id=d.InvoiceId JOIN dbo.AccountingPeriods p ON p.PeriodStart=DATEFROMPARTS(YEAR(v.Issued),MONTH(v.Issued),1))
 THROW 51101,N''Kỳ kế toán đã khóa.'',1;
END');
EXEC(N'CREATE OR ALTER TRIGGER dbo.TR_EInvoice_AccountingGuard ON dbo.InvoiceFinance AFTER INSERT,UPDATE,DELETE AS
BEGIN
 SET NOCOUNT ON;
 IF EXISTS(SELECT 1 FROM inserted i JOIN dbo.Invoices v ON v.Id=i.InvoiceId JOIN dbo.AccountingPeriods p ON p.PeriodStart=DATEFROMPARTS(YEAR(v.Issued),MONTH(v.Issued),1))
 OR EXISTS(SELECT 1 FROM deleted d JOIN dbo.Invoices v ON v.Id=d.InvoiceId JOIN dbo.AccountingPeriods p ON p.PeriodStart=DATEFROMPARTS(YEAR(v.Issued),MONTH(v.Issued),1))
 THROW 51101,N''Kỳ kế toán đã khóa.'',1;
END');
EXEC(N'CREATE OR ALTER TRIGGER dbo.TR_BillShares_AccountingGuard ON dbo.BillShares AFTER INSERT,UPDATE,DELETE AS
BEGIN
 SET NOCOUNT ON;
 IF EXISTS(SELECT 1 FROM inserted i JOIN dbo.Invoices v ON v.Id=i.InvoiceId JOIN dbo.AccountingPeriods p ON p.PeriodStart=DATEFROMPARTS(YEAR(v.Issued),MONTH(v.Issued),1))
 OR EXISTS(SELECT 1 FROM deleted d JOIN dbo.Invoices v ON v.Id=d.InvoiceId JOIN dbo.AccountingPeriods p ON p.PeriodStart=DATEFROMPARTS(YEAR(v.Issued),MONTH(v.Issued),1))
 THROW 51101,N''Kỳ kế toán đã khóa.'',1;
END');
EXEC(N'CREATE OR ALTER TRIGGER dbo.TR_Shifts_AccountingGuard ON dbo.CashShifts AFTER INSERT,UPDATE,DELETE AS
BEGIN
 SET NOCOUNT ON;
 IF EXISTS(SELECT 1 FROM deleted WHERE Status=''Locked'')
 THROW 51102,N''Ca đã khóa, không được sửa hoặc mở lại.'',1;
 IF EXISTS(SELECT 1 FROM inserted i JOIN dbo.AccountingPeriods p ON p.PeriodStart=DATEFROMPARTS(YEAR(i.OpenedAt),MONTH(i.OpenedAt),1))
 OR EXISTS(SELECT 1 FROM deleted d JOIN dbo.AccountingPeriods p ON p.PeriodStart=DATEFROMPARTS(YEAR(d.OpenedAt),MONTH(d.OpenedAt),1))
 THROW 51101,N''Kỳ kế toán đã khóa.'',1;
END');
EXEC(N'CREATE OR ALTER TRIGGER dbo.TR_OpeningBalances_Immutable ON dbo.FinanceOpeningBalances AFTER INSERT,UPDATE,DELETE AS
BEGIN
 SET NOCOUNT ON;
 IF EXISTS(SELECT 1 FROM deleted) THROW 51103,N''Số dư mở sổ là chứng từ một lần; không được sửa hoặc xóa.'',1;
 IF EXISTS(SELECT 1 FROM inserted i JOIN dbo.AccountingPeriods p ON p.PeriodStart=DATEFROMPARTS(YEAR(i.AsOf),MONTH(i.AsOf),1))
 THROW 51101,N''Kỳ kế toán đã khóa.'',1;
END');
EXEC(N'CREATE OR ALTER TRIGGER dbo.TR_AccountingPeriods_Immutable ON dbo.AccountingPeriods AFTER UPDATE,DELETE AS
BEGIN
 SET NOCOUNT ON;
 THROW 51104,N''Kỳ đã khóa không thể sửa hoặc mở lại.'',1;
END');
COMMIT;
