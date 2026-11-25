SET XACT_ABORT ON;
BEGIN TRANSACTION;
DECLARE @lock26 int;
EXEC @lock26=sys.sp_getapplock @Resource=N'QLKhachSan.Write',@LockMode='Exclusive',@LockOwner='Transaction',@LockTimeout=10000;
IF @lock26<0 THROW 51001,N'Không lấy được khóa nâng cấp dữ liệu.',1;
IF NOT EXISTS(SELECT 1 FROM dbo.PhienBanCSDL WHERE PhienBan=25) THROW 51000,N'Cần nâng cấp phiên bản 25 trước.',1;
IF NOT EXISTS(SELECT 1 FROM dbo.PhienBanCSDL WHERE PhienBan=26)
BEGIN
    DECLARE @remove TABLE (Ten sysname PRIMARY KEY);
    INSERT @remove(Ten) VALUES
        (N'KyKeToan'),(N'PhieuThuChi'),(N'SoDuDauKy'),(N'DongSaoKe'),
        (N'CongNo'),(N'PhanBoCongNo'),(N'TieuThuBuongPhong'),
        (N'ThongTinTaiChinhHoaDon'),(N'DieuChinhHoaDon'),(N'HoaDonHuy'),
        (N'NhomHoaDon'),(N'PhanChiaHoaDon'),(N'CauHinhChucNangMoi'),
        (N'DuLieuChucNangMoi'),(N'MaGiamGia'),(N'SuDungMaGiamGia'),(N'TraXeThue');

    -- Các trigger kế toán cũ tham chiếu kỳ khóa và chứng từ đã loại bỏ.
    DECLARE @sql26 nvarchar(max)=N'';
    SELECT @sql26=@sql26+N'DROP TRIGGER dbo.'+QUOTENAME(t.name)+N';'
    FROM sys.triggers t
    WHERE OBJECT_SCHEMA_NAME(t.object_id)=N'dbo' AND t.name IN (
        N'TR_Payments_AccountingGuard',N'TR_Invoices_AccountingGuard',
        N'TR_Vouchers_AccountingGuard',N'TR_Stock_AccountingGuard',
        N'TR_Debts_AccountingGuard',N'TR_Orders_AccountingGuard',
        N'TR_Adjustments_AccountingGuard',N'TR_Voids_AccountingGuard',
        N'TR_EInvoice_AccountingGuard',N'TR_BillShares_AccountingGuard',
        N'TR_Shifts_AccountingGuard',N'TR_OpeningBalances_Immutable',
        N'TR_AccountingPeriods_Immutable');
    IF LEN(@sql26)>0 EXEC sys.sp_executesql @sql26;

    -- Bỏ khóa ngoại trỏ vào các bảng sẽ bỏ; các bảng giữ lại vẫn nguyên dữ liệu.
    SET @sql26=N'';
    SELECT @sql26=@sql26+N'ALTER TABLE dbo.'+QUOTENAME(OBJECT_NAME(f.parent_object_id))+
        N' DROP CONSTRAINT '+QUOTENAME(f.name)+N';'
    FROM sys.foreign_keys f
    WHERE OBJECT_NAME(f.referenced_object_id) IN (SELECT Ten FROM @remove);
    IF LEN(@sql26)>0 EXEC sys.sp_executesql @sql26;

    SET @sql26=N'';
    SELECT @sql26=@sql26+N'DROP TABLE dbo.'+QUOTENAME(Ten)+N';' FROM @remove;
    EXEC sys.sp_executesql @sql26;
    IF (SELECT COUNT(*) FROM sys.tables WHERE schema_id=SCHEMA_ID(N'dbo') AND name<>N'sysdiagrams')<>20
        THROW 51008,N'Số bảng sau khi rút gọn không bằng 20.',1;
    INSERT dbo.PhienBanCSDL(PhienBan) VALUES(26);
END;
COMMIT;
