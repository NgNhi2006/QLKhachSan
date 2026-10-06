
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
-- BEGIN LEGACY TABLE RENAME
SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET ARITHABORT ON;
SET NUMERIC_ROUNDABORT OFF;
BEGIN TRANSACTION;
IF OBJECT_ID(N'dbo.SchemaVersion', N'U') IS NOT NULL
BEGIN
    DECLARE @lock int;
    EXEC @lock = sys.sp_getapplock
        @Resource = N'QLKhachSan.Write', @LockMode = 'Exclusive',
        @LockOwner = 'Transaction', @LockTimeout = 10000;
    IF @lock < 0 THROW 51001, N'Không lấy được khóa nâng cấp dữ liệu.', 1;

    DECLARE @PhienBanCu int = (SELECT MAX(Version) FROM dbo.SchemaVersion);
    IF @PhienBanCu NOT BETWEEN 1 AND 8
        THROW 51000, N'Phiên bản CSDL cũ không tương thích.', 1;

    DECLARE @TenBang TABLE (
        ThuTu int IDENTITY(1,1) PRIMARY KEY,
        TenCu sysname NOT NULL,
        TenMoi sysname NOT NULL
    );
    INSERT @TenBang (TenCu, TenMoi)
    VALUES
        (N'Users', N'TaiKhoanNhanVien'),
        (N'Rooms', N'Phong'),
        (N'Customers', N'KhachHang'),
        (N'Stays', N'LuotLuuTru'),
        (N'StaySegments', N'ChangLuuTru'),
        (N'Services', N'DichVu'),
        (N'ServiceOrders', N'YeuCauDichVu'),
        (N'Invoices', N'HoaDon'),
        (N'Payments', N'GiaoDichThanhToan'),
        (N'AuditLog', N'NhatKyThaoTac'),
        (N'AccountingPeriods', N'KyKeToan'),
        (N'CashShifts', N'CaTruc'),
        (N'FinanceVouchers', N'PhieuThuChi'),
        (N'FinanceOpeningBalances', N'SoDuDauKy'),
        (N'BankStatementLines', N'DongSaoKe'),
        (N'FinanceDebts', N'CongNo'),
        (N'DebtAllocations', N'PhanBoCongNo'),
        (N'StockItems', N'HangTonKho'),
        (N'StockMovements', N'BienDongKho'),
        (N'HousekeepingConsumption', N'TieuThuBuongPhong'),
        (N'InvoiceFinance', N'ThongTinTaiChinhHoaDon'),
        (N'InvoiceAdjustments', N'DieuChinhHoaDon'),
        (N'InvoiceVoids', N'HoaDonHuy'),
        (N'BillGroups', N'NhomHoaDon'),
        (N'BillShares', N'PhanChiaHoaDon'),
        (N'AppMenus', N'MenuTong'),
        (N'AppSubmenus', N'MenuCon'),
        (N'AppFunctions', N'ChucNang'),
        (N'UserFunctionGrants', N'PhanQuyenNhanVien'),
        (N'MenuRoomFunctions', N'ChucNangQuanLyPhong'),
        (N'MenuServiceFunctions', N'ChucNangDichVu'),
        (N'MenuCustomerFunctions', N'ChucNangKhachHang'),
        (N'MenuShiftFunctions', N'ChucNangCaTruc'),
        (N'MenuCashFunctions', N'ChucNangThuChi'),
        (N'MenuInvoiceFunctions', N'ChucNangHoaDon'),
        (N'MenuReportFunctions', N'ChucNangBaoCao'),
        (N'MenuStaffFunctions', N'ChucNangNhanVien'),
        (N'MenuSystemFunctions', N'ChucNangHeThong'),
        (N'SchemaVersion', N'PhienBanCSDL');

    -- Lưu trigger trước khi đổi tên; định nghĩa SQL của trigger không tự đổi theo sp_rename.
    DECLARE @TriggerCu TABLE (Ten sysname PRIMARY KEY, DinhNghia nvarchar(max) NOT NULL);
    INSERT @TriggerCu (Ten, DinhNghia)
    SELECT t.name, m.definition
    FROM sys.triggers AS t
    JOIN sys.sql_modules AS m ON m.object_id = t.object_id
    WHERE t.name IN (
        N'TR_Payments_AccountingGuard',
        N'TR_Invoices_AccountingGuard',
        N'TR_Vouchers_AccountingGuard',
        N'TR_Stock_AccountingGuard',
        N'TR_Debts_AccountingGuard',
        N'TR_Orders_AccountingGuard',
        N'TR_Adjustments_AccountingGuard',
        N'TR_Voids_AccountingGuard',
        N'TR_EInvoice_AccountingGuard',
        N'TR_BillShares_AccountingGuard',
        N'TR_Shifts_AccountingGuard',
        N'TR_OpeningBalances_Immutable',
        N'TR_AccountingPeriods_Immutable'
    );
    IF (@PhienBanCu >= 6 AND (SELECT COUNT(*) FROM @TriggerCu) <> 13)
        OR (@PhienBanCu < 6 AND EXISTS (SELECT 1 FROM @TriggerCu))
        THROW 51009, N'Trigger kế toán của phiên bản cũ không khớp.', 1;

    DECLARE @TenTrigger sysname, @DinhNghia nvarchar(max), @CauLenh nvarchar(max);
    DECLARE TriggerCu CURSOR LOCAL FAST_FORWARD FOR
        SELECT Ten, DinhNghia FROM @TriggerCu ORDER BY Ten;
    OPEN TriggerCu;
    FETCH NEXT FROM TriggerCu INTO @TenTrigger, @DinhNghia;
    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @CauLenh = N'DROP TRIGGER dbo.' + QUOTENAME(@TenTrigger);
        EXEC sys.sp_executesql @CauLenh;
        FETCH NEXT FROM TriggerCu INTO @TenTrigger, @DinhNghia;
    END;
    CLOSE TriggerCu;
    DEALLOCATE TriggerCu;

    DECLARE @ThuTu int = 1, @TenCu sysname, @TenMoi sysname, @DuongDanCu nvarchar(300);
    WHILE @ThuTu <= 39
    BEGIN
        SELECT @TenCu = TenCu, @TenMoi = TenMoi FROM @TenBang WHERE ThuTu = @ThuTu;
        SET @DuongDanCu = N'dbo.' + @TenCu;
        IF OBJECT_ID(@DuongDanCu, N'U') IS NOT NULL
        BEGIN
            IF OBJECT_ID(N'dbo.' + @TenMoi) IS NOT NULL
                THROW 51011, N'Tên bảng mới đã tồn tại.', 1;
            EXEC sys.sp_rename @objname = @DuongDanCu, @newname = @TenMoi, @objtype = N'OBJECT';
        END;
        SET @ThuTu += 1;
    END;
    IF OBJECT_ID(N'dbo.PhienBanCSDL', N'U') IS NULL
        THROW 51010, N'Không đổi được tên bảng phiên bản.', 1;

    DECLARE TriggerMoi CURSOR LOCAL FAST_FORWARD FOR
        SELECT Ten, DinhNghia FROM @TriggerCu ORDER BY Ten;
    OPEN TriggerMoi;
    FETCH NEXT FROM TriggerMoi INTO @TenTrigger, @DinhNghia;
    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @ThuTu = 1;
        WHILE @ThuTu <= 39
        BEGIN
            SELECT @TenCu = TenCu, @TenMoi = TenMoi FROM @TenBang WHERE ThuTu = @ThuTu;
            SET @DinhNghia = REPLACE(@DinhNghia, N'dbo.' + @TenCu, N'dbo.' + @TenMoi);
            SET @ThuTu += 1;
        END;
        EXEC sys.sp_executesql @DinhNghia;
        FETCH NEXT FROM TriggerMoi INTO @TenTrigger, @DinhNghia;
    END;
    CLOSE TriggerMoi;
    DEALLOCATE TriggerMoi;
END;
COMMIT;
-- END LEGACY TABLE RENAME
GO

-- BEGIN LEGACY COLUMN RENAME
-- Nâng cấp CSDL V1–V9: đổi tên cột tiếng Anh tại chỗ, không xóa bảng hoặc dữ liệu.
SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET ARITHABORT ON;
SET NUMERIC_ROUNDABORT OFF;
BEGIN TRANSACTION;
IF OBJECT_ID(N'dbo.PhienBanCSDL', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.PhienBanCSDL', N'Version') IS NOT NULL
BEGIN
    DECLARE @KhoaCot int;
    EXEC @KhoaCot = sys.sp_getapplock
        @Resource = N'QLKhachSan.Write', @LockMode = 'Exclusive',
        @LockOwner = 'Transaction', @LockTimeout = 10000;
    IF @KhoaCot < 0 THROW 51001, N'Không lấy được khóa nâng cấp tên cột.', 1;

    DECLARE @Cot TABLE (TenCu sysname PRIMARY KEY, TenMoi sysname NOT NULL);
    INSERT @Cot (TenCu, TenMoi) VALUES
        (N'PermissionsCustomized', N'DaTuyChinhQuyen'),
        (N'DeliveredQuantity', N'SoLuongDaGiao'),
        (N'ExternalReference', N'MaThamChieuNgoai'),
        (N'EInvoiceIssuedAt', N'ThoiDiemPhatHanhHoaDonDienTu'),
        (N'MatchedPaymentId', N'MaGiaoDichDaDoiSoat'),
        (N'MatchedVoucherId', N'MaPhieuDaDoiSoat'),
        (N'SecurityVersion', N'PhienBanBaoMat'),
        (N'EInvoiceNumber', N'SoHoaDonDienTu'),
        (N'FailedAttempts', N'SoLanDangNhapSai'),
        (N'IdentityNumber', N'SoGiayTo'),
        (N'ReversalReason', N'LyDoDaoButToan'),
        (N'ServiceOrderId', N'MaYeuCauDichVu'),
        (N'ServiceCharge', N'TienDichVu'),
        (N'AllowedRoles', N'VaiTroDuocPhep'),
        (N'CancelReason', N'LyDoHuy'),
        (N'Counterparty', N'DoiTac'),
        (N'ExpectedCash', N'TienMatDuKien'),
        (N'FunctionCode', N'MaChucNang'),
        (N'PasswordHash', N'MatKhauBam'),
        (N'ReorderLevel', N'NguongDatHang'),
        (N'RoundingUnit', N'DonViLamTron'),
        (N'SubmenuTitle', N'TenMenuPhu'),
        (N'AverageCost', N'GiaVonBinhQuan'),
        (N'CancelledAt', N'ThoiDiemHuyBo'),
        (N'CancelledBy', N'MaNguoiHuy'),
        (N'CountedCash', N'TienMatKiemDem'),
        (N'Explanation', N'GiaiTrinh'),
        (N'LockedUntil', N'KhoaDen'),
        (N'OpeningCash', N'TienMatDauCa'),
        (N'PeriodStart', N'NgayDauKy'),
        (N'ServiceRate', N'TyLePhiDichVu'),
        (N'VoucherType', N'LoaiPhieu'),
        (N'ApprovedBy', N'MaNguoiPheDuyet'),
        (N'CustomerId', N'MaKhachHang'),
        (N'HappenedAt', N'ThoiDiemBienDong'),
        (N'ImportedAt', N'ThoiDiemNhap'),
        (N'ImportedBy', N'MaNguoiNhap'),
        (N'Iterations', N'SoLanBam'),
        (N'OccurredAt', N'ThoiDiemGiaoDich'),
        (N'ReasonCode', N'MaLyDo'),
        (N'ReportedAt', N'ThoiDiemBaoCao'),
        (N'ReversedAt', N'ThoiDiemDaoButToan'),
        (N'ReversedBy', N'MaNguoiDaoButToan'),
        (N'RoomCharge', N'TienPhong'),
        (N'RoomNumber', N'SoPhongHoaDon'),
        (N'Cancelled', N'ThoiDiemHuy'),
        (N'CashierId', N'MaThuNgan'),
        (N'Collected', N'TienDaThu'),
        (N'CreatedAt', N'ThoiDiemLap'),
        (N'CreatedBy', N'MaNguoiTao'),
        (N'Delivered', N'ThoiDiemGiao'),
        (N'Departure', N'ThoiDiemDi'),
        (N'GuestName', N'TenKhach'),
        (N'HoldUntil', N'HanGiuPhong'),
        (N'InvoiceId', N'MaHoaDon'),
        (N'Reference', N'MaThamChieu'),
        (N'ServiceId', N'MaDichVu'),
        (N'SortOrder', N'ThuTuHienThi'),
        (N'VoucherId', N'MaPhieuThuChi'),
        (N'Archived', N'DaLuuTru'),
        (N'Category', N'DanhMuc'),
        (N'CheckOut', N'ThoiDiemTraPhong'),
        (N'ClosedAt', N'ThoiDiemDongCa'),
        (N'DebtType', N'LoaiCongNo'),
        (N'IsActive', N'ConHieuLuc'),
        (N'IssuedAt', N'ThoiDiemPhatSinhCongNo'),
        (N'LockedAt', N'ThoiDiemKhoa'),
        (N'LockedBy', N'MaNguoiKhoa'),
        (N'MenuCode', N'MaMenu'),
        (N'OpenedAt', N'ThoiDiemMoCa'),
        (N'PostedAt', N'ThoiDiemHachToan'),
        (N'Quantity', N'SoLuong'),
        (N'Refunded', N'TienDaHoan'),
        (N'UnitCost', N'GiaVonDonVi'),
        (N'Username', N'TenDangNhap'),
        (N'VoidedAt', N'ThoiDiemHuyHoaDon'),
        (N'VoidedBy', N'MaNguoiHuyHoaDon'),
        (N'Arrival', N'ThoiDiemDen'),
        (N'Channel', N'KenhThanhToan'),
        (N'CheckIn', N'ThoiDiemNhanPhong'),
        (N'Created', N'ThoiDiemTao'),
        (N'Deposit', N'TienCoc'),
        (N'GroupId', N'MaNhomHoaDon'),
        (N'Ordered', N'ThoiDiemGoi'),
        (N'ShiftId', N'MaCaTruc'),
        (N'Started', N'ThoiDiemBatDau'),
        (N'VatRate', N'TyLeVAT'),
        (N'Version', N'PhienBan'),
        (N'Action', N'HanhDong'),
        (N'Active', N'DangHoatDong'),
        (N'Amount', N'SoTien'),
        (N'DebtId', N'MaCongNo'),
        (N'Detail', N'ChiTiet'),
        (N'Issued', N'ThoiDiemLapHoaDon'),
        (N'ItemId', N'MaMatHang'),
        (N'Method', N'PhuongThucThanhToan'),
        (N'Number', N'SoPhong'),
        (N'Reason', N'LyDo'),
        (N'RoomId', N'MaPhong'),
        (N'Status', N'TrangThai'),
        (N'StayId', N'MaLuotLuuTru'),
        (N'UserId', N'MaNhanVien'),
        (N'DueAt', N'HanThanhToan'),
        (N'Ended', N'ThoiDiemKetThuc'),
        (N'Payer', N'NguoiThanhToan'),
        (N'Phone', N'SoDienThoai'),
        (N'Price', N'DonGia'),
        (N'SetAt', N'ThoiDiemThietLap'),
        (N'SetBy', N'MaNguoiThietLap'),
        (N'Title', N'TieuDe'),
        (N'AsOf', N'TinhDenThoiDiem'),
        (N'Kind', N'LoaiGiaoDich'),
        (N'Name', N'Ten'),
        (N'Note', N'GhiChu'),
        (N'Rate', N'DonGiaPhong'),
        (N'Role', N'VaiTro'),
        (N'Salt', N'MuoiBam'),
        (N'Type', N'Loai'),
        (N'Unit', N'DonViTinh'),
        (N'Id', N'Ma');

    -- Chỉ mục có điều kiện phụ thuộc trực tiếp vào tên cột; tạo lại sau khi đổi tên.
    DECLARE @ChiMuc TABLE (Bang sysname, Ten sysname, CauLenh nvarchar(max));
    INSERT @ChiMuc (Bang, Ten, CauLenh) VALUES
        (N'LuotLuuTru', N'UX_Stays_ActiveRoom', N'CREATE UNIQUE INDEX UX_Stays_ActiveRoom ON dbo.LuotLuuTru(MaPhong) WHERE ConHieuLuc=1'),
        (N'ChangLuuTru', N'UX_Segments_Open', N'CREATE UNIQUE INDEX UX_Segments_Open ON dbo.ChangLuuTru(MaLuotLuuTru) WHERE ThoiDiemKetThuc IS NULL'),
        (N'YeuCauDichVu', N'IX_Orders_Pending', N'CREATE INDEX IX_Orders_Pending ON dbo.YeuCauDichVu(ThoiDiemGoi) WHERE ThoiDiemGiao IS NULL'),
        (N'LuotLuuTru', N'UX_Stays_OccupiedRoom', N'CREATE UNIQUE INDEX UX_Stays_OccupiedRoom ON dbo.LuotLuuTru(MaPhong) WHERE TrangThai=''Occupied'''),
        (N'GiaoDichThanhToan', N'UX_Payments_Forfeit', N'CREATE UNIQUE INDEX UX_Payments_Forfeit ON dbo.GiaoDichThanhToan(MaLuotLuuTru) WHERE LoaiGiaoDich=''Forfeit'''),
        (N'TaiKhoanNhanVien', N'UX_Users_ActiveUsername', N'CREATE UNIQUE INDEX UX_Users_ActiveUsername ON dbo.TaiKhoanNhanVien(TenDangNhap) WHERE DaLuuTru=0'),
        (N'CaTruc', N'UX_CashShifts_Open', N'CREATE UNIQUE INDEX UX_CashShifts_Open ON dbo.CaTruc(MaThuNgan) WHERE TrangThai=''Open'''),
        (N'GiaoDichThanhToan', N'IX_Payments_Shift', N'CREATE INDEX IX_Payments_Shift ON dbo.GiaoDichThanhToan(MaCaTruc) WHERE MaCaTruc IS NOT NULL'),
        (N'GiaoDichThanhToan', N'UX_Payments_ExternalReference', N'CREATE UNIQUE INDEX UX_Payments_ExternalReference ON dbo.GiaoDichThanhToan(PhuongThucThanhToan,MaThamChieuNgoai) WHERE MaThamChieuNgoai IS NOT NULL'),
        (N'PhieuThuChi', N'UX_FinanceVouchers_Reference', N'CREATE UNIQUE INDEX UX_FinanceVouchers_Reference ON dbo.PhieuThuChi(KenhThanhToan,MaThamChieu) WHERE MaThamChieu IS NOT NULL AND ThoiDiemDaoButToan IS NULL');
    DELETE i FROM @ChiMuc AS i
    WHERE NOT EXISTS (SELECT 1 FROM sys.indexes AS x
        WHERE x.object_id=OBJECT_ID(N'dbo.'+i.Bang) AND x.name=i.Ten);

    DECLARE @BangChiMuc sysname, @TenChiMuc sysname, @LenhChiMuc nvarchar(max);
    DECLARE XoaChiMuc CURSOR LOCAL FAST_FORWARD FOR SELECT Bang,Ten FROM @ChiMuc;
    OPEN XoaChiMuc;
    FETCH NEXT FROM XoaChiMuc INTO @BangChiMuc,@TenChiMuc;
    WHILE @@FETCH_STATUS=0
    BEGIN
        SET @LenhChiMuc=N'DROP INDEX '+QUOTENAME(@TenChiMuc)+N' ON dbo.'+QUOTENAME(@BangChiMuc);
        EXEC sys.sp_executesql @LenhChiMuc;
        FETCH NEXT FROM XoaChiMuc INTO @BangChiMuc,@TenChiMuc;
    END;
    CLOSE XoaChiMuc;
    DEALLOCATE XoaChiMuc;

    -- Ràng buộc CHECK giữ biểu thức theo tên cột cũ nên phải tạo lại.
    DECLARE @KiemTra TABLE (Bang sysname, Ten sysname, DinhNghia nvarchar(max), DaTat bit, KhongTinCay bit);
    INSERT @KiemTra (Bang,Ten,DinhNghia,DaTat,KhongTinCay)
    SELECT t.name,c.name,c.definition,c.is_disabled,c.is_not_trusted
    FROM sys.check_constraints AS c
    JOIN sys.tables AS t ON t.object_id=c.parent_object_id
    WHERE t.schema_id=SCHEMA_ID(N'dbo');
    DECLARE @BangKiemTra sysname, @TenKiemTra sysname, @DinhNghiaKiemTra nvarchar(max),
            @DaTat bit, @KhongTinCay bit;
    DECLARE XoaKiemTra CURSOR LOCAL FAST_FORWARD FOR SELECT Bang,Ten FROM @KiemTra;
    OPEN XoaKiemTra;
    FETCH NEXT FROM XoaKiemTra INTO @BangKiemTra,@TenKiemTra;
    WHILE @@FETCH_STATUS=0
    BEGIN
        SET @LenhChiMuc=N'ALTER TABLE dbo.'+QUOTENAME(@BangKiemTra)
            +N' DROP CONSTRAINT '+QUOTENAME(@TenKiemTra);
        EXEC sys.sp_executesql @LenhChiMuc;
        FETCH NEXT FROM XoaKiemTra INTO @BangKiemTra,@TenKiemTra;
    END;
    CLOSE XoaKiemTra;
    DEALLOCATE XoaKiemTra;

    DECLARE @TriggerCot TABLE (Ten sysname PRIMARY KEY, DinhNghia nvarchar(max) NOT NULL);
    INSERT @TriggerCot (Ten, DinhNghia)
    SELECT t.name, m.definition
    FROM sys.triggers AS t
    JOIN sys.sql_modules AS m ON m.object_id=t.object_id
    WHERE t.name IN (
        N'TR_Payments_AccountingGuard', N'TR_Invoices_AccountingGuard',
        N'TR_Vouchers_AccountingGuard', N'TR_Stock_AccountingGuard',
        N'TR_Debts_AccountingGuard', N'TR_Orders_AccountingGuard',
        N'TR_Adjustments_AccountingGuard', N'TR_Voids_AccountingGuard',
        N'TR_EInvoice_AccountingGuard', N'TR_BillShares_AccountingGuard',
        N'TR_Shifts_AccountingGuard', N'TR_OpeningBalances_Immutable',
        N'TR_AccountingPeriods_Immutable'
    );
    IF (SELECT COUNT(*) FROM @TriggerCot) NOT IN (0, 13)
        THROW 51009, N'Trigger kế toán không đầy đủ; chưa đổi tên cột.', 1;

    DECLARE @TenTrigger sysname, @DinhNghia nvarchar(max), @Lenh nvarchar(max);
    DECLARE XoaTrigger CURSOR LOCAL FAST_FORWARD FOR
        SELECT Ten FROM @TriggerCot ORDER BY Ten;
    OPEN XoaTrigger;
    FETCH NEXT FROM XoaTrigger INTO @TenTrigger;
    WHILE @@FETCH_STATUS=0
    BEGIN
        SET @Lenh=N'DROP TRIGGER dbo.'+QUOTENAME(@TenTrigger);
        EXEC sys.sp_executesql @Lenh;
        FETCH NEXT FROM XoaTrigger INTO @TenTrigger;
    END;
    CLOSE XoaTrigger;
    DEALLOCATE XoaTrigger;

    DECLARE @Bang sysname, @TenCu sysname, @TenMoi sysname, @DuongDan nvarchar(500), @KetQuaDoiTen int;
    DECLARE DoiCot CURSOR LOCAL FAST_FORWARD FOR
        SELECT t.name, c.name, m.TenMoi
        FROM sys.tables AS t
        JOIN sys.columns AS c ON c.object_id=t.object_id
        JOIN @Cot AS m ON m.TenCu=c.name
        WHERE t.schema_id=SCHEMA_ID(N'dbo')
          AND t.name IN (N'PhienBanCSDL', N'TaiKhoanNhanVien', N'Phong', N'KhachHang', N'LuotLuuTru', N'ChangLuuTru', N'DichVu', N'YeuCauDichVu', N'HoaDon', N'GiaoDichThanhToan', N'NhatKyThaoTac', N'KyKeToan', N'CaTruc', N'PhieuThuChi', N'SoDuDauKy', N'DongSaoKe', N'CongNo', N'PhanBoCongNo', N'HangTonKho', N'BienDongKho', N'TieuThuBuongPhong', N'ThongTinTaiChinhHoaDon', N'DieuChinhHoaDon', N'HoaDonHuy', N'NhomHoaDon', N'PhanChiaHoaDon', N'MenuTong', N'MenuCon', N'ChucNang', N'PhanQuyenNhanVien', N'ChucNangQuanLyPhong', N'ChucNangDichVu', N'ChucNangKhachHang', N'ChucNangCaTruc', N'ChucNangThuChi', N'ChucNangHoaDon', N'ChucNangBaoCao', N'ChucNangNhanVien', N'ChucNangHeThong')
        ORDER BY LEN(c.name) DESC, t.name, c.name;
    OPEN DoiCot;
    FETCH NEXT FROM DoiCot INTO @Bang, @TenCu, @TenMoi;
    WHILE @@FETCH_STATUS=0
    BEGIN
        SET @DuongDan=N'dbo.'+QUOTENAME(@Bang)+N'.'+QUOTENAME(@TenCu);
        EXEC @KetQuaDoiTen=sys.sp_rename @objname=@DuongDan, @newname=@TenMoi, @objtype=N'COLUMN';
        IF @KetQuaDoiTen<>0 THROW 51013, N'Không đổi được một tên cột; đã hủy toàn bộ giao dịch.', 1;
        FETCH NEXT FROM DoiCot INTO @Bang, @TenCu, @TenMoi;
    END;
    CLOSE DoiCot;
    DEALLOCATE DoiCot;

    DECLARE TaoKiemTra CURSOR LOCAL FAST_FORWARD FOR
        SELECT Bang,Ten,DinhNghia,DaTat,KhongTinCay FROM @KiemTra;
    OPEN TaoKiemTra;
    FETCH NEXT FROM TaoKiemTra INTO @BangKiemTra,@TenKiemTra,@DinhNghiaKiemTra,@DaTat,@KhongTinCay;
    WHILE @@FETCH_STATUS=0
    BEGIN
        DECLARE DoiTenTrongKiemTra CURSOR LOCAL FAST_FORWARD FOR
            SELECT TenCu,TenMoi FROM @Cot ORDER BY LEN(TenCu) DESC,TenCu;
        OPEN DoiTenTrongKiemTra;
        FETCH NEXT FROM DoiTenTrongKiemTra INTO @TenCu,@TenMoi;
        WHILE @@FETCH_STATUS=0
        BEGIN
            SET @DinhNghiaKiemTra=REPLACE(@DinhNghiaKiemTra COLLATE Latin1_General_100_BIN2,@TenCu,@TenMoi);
            FETCH NEXT FROM DoiTenTrongKiemTra INTO @TenCu,@TenMoi;
        END;
        CLOSE DoiTenTrongKiemTra;
        DEALLOCATE DoiTenTrongKiemTra;
        SET @DinhNghiaKiemTra=REPLACE(REPLACE(@DinhNghiaKiemTra,N'''TienCoc''',N'''Deposit'''),N'''ThoiDiemHuy''',N'''Cancelled''');
        SET @LenhChiMuc=N'ALTER TABLE dbo.'+QUOTENAME(@BangKiemTra)
            +CASE WHEN @KhongTinCay=1 THEN N' WITH NOCHECK' ELSE N' WITH CHECK' END
            +N' ADD CONSTRAINT '+QUOTENAME(@TenKiemTra)+N' CHECK '+@DinhNghiaKiemTra;
        EXEC sys.sp_executesql @LenhChiMuc;
        IF @DaTat=1
        BEGIN
            SET @LenhChiMuc=N'ALTER TABLE dbo.'+QUOTENAME(@BangKiemTra)
                +N' NOCHECK CONSTRAINT '+QUOTENAME(@TenKiemTra);
            EXEC sys.sp_executesql @LenhChiMuc;
        END;
        FETCH NEXT FROM TaoKiemTra INTO @BangKiemTra,@TenKiemTra,@DinhNghiaKiemTra,@DaTat,@KhongTinCay;
    END;
    CLOSE TaoKiemTra;
    DEALLOCATE TaoKiemTra;

    DECLARE TaoChiMuc CURSOR LOCAL FAST_FORWARD FOR SELECT CauLenh FROM @ChiMuc;
    OPEN TaoChiMuc;
    FETCH NEXT FROM TaoChiMuc INTO @LenhChiMuc;
    WHILE @@FETCH_STATUS=0
    BEGIN
        EXEC sys.sp_executesql @LenhChiMuc;
        FETCH NEXT FROM TaoChiMuc INTO @LenhChiMuc;
    END;
    CLOSE TaoChiMuc;
    DEALLOCATE TaoChiMuc;

    -- sp_rename không sửa nội dung trigger: thay tên cột rồi tạo lại ngay trong giao dịch.
    DECLARE TaoTrigger CURSOR LOCAL FAST_FORWARD FOR
        SELECT Ten, DinhNghia FROM @TriggerCot ORDER BY Ten;
    OPEN TaoTrigger;
    FETCH NEXT FROM TaoTrigger INTO @TenTrigger, @DinhNghia;
    WHILE @@FETCH_STATUS=0
    BEGIN
        DECLARE DoiTenTrongTrigger CURSOR LOCAL FAST_FORWARD FOR
            SELECT TenCu, TenMoi FROM @Cot ORDER BY LEN(TenCu) DESC, TenCu;
        OPEN DoiTenTrongTrigger;
        FETCH NEXT FROM DoiTenTrongTrigger INTO @TenCu, @TenMoi;
        WHILE @@FETCH_STATUS=0
        BEGIN
            SET @DinhNghia=REPLACE(@DinhNghia COLLATE Latin1_General_100_BIN2,@TenCu,@TenMoi);
            FETCH NEXT FROM DoiTenTrongTrigger INTO @TenCu, @TenMoi;
        END;
        CLOSE DoiTenTrongTrigger;
        DEALLOCATE DoiTenTrongTrigger;
        EXEC sys.sp_executesql @DinhNghia;
        FETCH NEXT FROM TaoTrigger INTO @TenTrigger, @DinhNghia;
    END;
    CLOSE TaoTrigger;
    DEALLOCATE TaoTrigger;

    IF COL_LENGTH(N'dbo.PhienBanCSDL', N'PhienBan') IS NULL
        THROW 51012, N'Không đổi được tên cột phiên bản.', 1;
END;
COMMIT;
-- END LEGACY COLUMN RENAME
GO

BEGIN TRANSACTION;
IF OBJECT_ID(N'dbo.PhienBanCSDL') IS NULL
BEGIN
    -- dbo.PhienBanCSDL — hệ thống
    --   Menu: nội bộ
    --   Chức năng: theo dõi V1–V10
    --   Xem: PhienBan.
    CREATE TABLE dbo.PhienBanCSDL (
        PhienBan int NOT NULL PRIMARY KEY
    );

    -- dbo.TaiKhoanNhanVien — nhân viên
    --   Menu tổng: Nhân viên/Hệ thống
    --   Menu phụ: Phân quyền/Tài khoản
    --   Chức năng: staff.view, staff.manage, system.password
    --   Xem: Ma, TenDangNhap, VaiTro, DangHoatDong, KhoaDen (không xem MatKhauBam/MuoiBam).
    CREATE TABLE dbo.TaiKhoanNhanVien (
        Ma int IDENTITY PRIMARY KEY,
        TenDangNhap nvarchar(50) NOT NULL UNIQUE,
        MatKhauBam varbinary(32) NOT NULL,
        MuoiBam varbinary(16) NOT NULL,
        SoLanBam int NOT NULL CHECK(SoLanBam>=100000),
        VaiTro varchar(20) NOT NULL CHECK(VaiTro IN ('Admin','Reception')),
        DangHoatDong bit NOT NULL DEFAULT 1,
        SoLanDangNhapSai int NOT NULL DEFAULT 0,
        KhoaDen datetime2 NULL
    );

    -- dbo.Phong — phòng
    --   Menu tổng: Quản lý phòng
    --   Menu phụ: Phòng/Đặt phòng/Buồng phòng/Danh mục phòng
    --   Chức năng: room.map, room.search, room.walkin, room.reserve, room.clean, room.maintain, room.catalog,
    --   room.pricing
    --   Xem: SoPhong, Loai, DonGiaPhong, TienCoc, TrangThai.
    CREATE TABLE dbo.Phong (
        Ma int IDENTITY PRIMARY KEY,
        SoPhong nvarchar(10) NOT NULL UNIQUE,
        Loai nvarchar(20) NOT NULL CHECK(Loai IN(N'Đơn',N'Đôi',N'VIP')),
        DonGiaPhong decimal(18,2) NOT NULL CHECK(DonGiaPhong>0),
        TienCoc decimal(18,2) NOT NULL CHECK(TienCoc>=0),
        TrangThai varchar(20) NOT NULL DEFAULT 'Trong' CHECK(TrangThai IN('Trong','DaDat','DangO','DangDon','BaoTri')),
        PhienBan bigint NOT NULL DEFAULT 1
    );

    -- dbo.KhachHang — khách
    --   Menu tổng: Khách hàng
    --   Menu phụ: Hồ sơ/Lịch sử
    --   Chức năng: customer.profile, customer.history
    --   Xem: Ten, SoDienThoai, SoGiayTo, ThoiDiemTao (chỉ người được cấp quyền).
    CREATE TABLE dbo.KhachHang (
        Ma int IDENTITY PRIMARY KEY,
        Ten nvarchar(100) NOT NULL,
        SoDienThoai nvarchar(20) NOT NULL,
        SoGiayTo nvarchar(20) NOT NULL UNIQUE,
        ThoiDiemTao datetime2 NOT NULL DEFAULT SYSDATETIME()
    );

    -- dbo.LuotLuuTru — lượt đặt/lưu trú
    --   Menu tổng: Quản lý phòng/Khách hàng
    --   Menu phụ: Đặt phòng/Lưu trú/Lịch sử
    --   Chức năng: room.reserve, room.checkin, room.booking_edit, room.booking_cancel, room.schedule,
    --   room.history, customer.history
    --   Xem: MaPhong, TenKhach, TrangThai, ThoiDiemDen, ThoiDiemDi, HanGiuPhong, TienCoc.
    CREATE TABLE dbo.LuotLuuTru (
        Ma bigint IDENTITY PRIMARY KEY,
        MaPhong int NOT NULL REFERENCES dbo.Phong(Ma),
        MaKhachHang int NOT NULL REFERENCES dbo.KhachHang(Ma),
        TenKhach nvarchar(100) NOT NULL,
        SoDienThoai nvarchar(20) NOT NULL,
        SoGiayTo nvarchar(20) NOT NULL,
        TrangThai varchar(20) NOT NULL CHECK(TrangThai IN('Reserved','Occupied','Paid','Cancelled')),
        ConHieuLuc bit NOT NULL,
        ThoiDiemTao datetime2 NOT NULL,
        ThoiDiemDen datetime2 NOT NULL,
        ThoiDiemDi datetime2 NOT NULL,
        ThoiDiemNhanPhong datetime2 NULL,
        ThoiDiemTraPhong datetime2 NULL,
        HanGiuPhong datetime2 NULL,
        TienCoc decimal(18,2) NOT NULL CHECK(TienCoc>=0),
        PhienBan bigint NOT NULL DEFAULT 1,
        MaNguoiTao int NOT NULL REFERENCES dbo.TaiKhoanNhanVien(Ma),
        CONSTRAINT CK_Stays_Departure CHECK(ThoiDiemDi>COALESCE(ThoiDiemNhanPhong,ThoiDiemDen)),
        CHECK((ConHieuLuc=1 AND TrangThai IN('Reserved','Occupied')) OR (ConHieuLuc=0 AND TrangThai IN('Paid','Cancelled')))
    );

CREATE UNIQUE INDEX UX_Stays_ActiveRoom ON dbo.LuotLuuTru(MaPhong) WHERE ConHieuLuc=1;
CREATE INDEX IX_Stays_Customer ON dbo.LuotLuuTru(MaKhachHang,TrangThai);

    -- dbo.ChangLuuTru — chặng ở
    --   Menu tổng: Quản lý phòng
    --   Menu phụ: Lưu trú
    --   Chức năng: room.transfer, room.extend, room.checkout, room.history
    --   Xem: MaLuotLuuTru, MaPhong, ThoiDiemBatDau, ThoiDiemKetThuc, DonGiaPhong.
    CREATE TABLE dbo.ChangLuuTru (
        Ma bigint IDENTITY PRIMARY KEY,
        MaLuotLuuTru bigint NOT NULL REFERENCES dbo.LuotLuuTru(Ma),
        MaPhong int NOT NULL REFERENCES dbo.Phong(Ma),
        ThoiDiemBatDau datetime2 NOT NULL,
        ThoiDiemKetThuc datetime2 NULL,
        DonGiaPhong decimal(18,2) NOT NULL CHECK(DonGiaPhong>0),
        CHECK(ThoiDiemKetThuc IS NULL OR ThoiDiemKetThuc>=ThoiDiemBatDau)
    );

CREATE UNIQUE INDEX UX_Segments_Open ON dbo.ChangLuuTru(MaLuotLuuTru) WHERE ThoiDiemKetThuc IS NULL;
CREATE INDEX IX_Segments_Stay ON dbo.ChangLuuTru(MaLuotLuuTru);

    -- dbo.DichVu — danh mục dịch vụ
    --   Menu tổng: Dịch vụ
    --   Menu phụ: Danh mục dịch vụ/Yêu cầu dịch vụ
    --   Chức năng: service.catalog, service.order
    --   Xem: DanhMuc, Ten, DonGia, DonViTinh, DangHoatDong.
    CREATE TABLE dbo.DichVu (
        Ma int IDENTITY PRIMARY KEY,
        DanhMuc nvarchar(100) NOT NULL,
        Ten nvarchar(150) NOT NULL UNIQUE,
        DonGia decimal(18,2) NOT NULL CHECK(DonGia>=0),
        DonViTinh nvarchar(20) NOT NULL,
        DangHoatDong bit NOT NULL DEFAULT 1
    );

    -- dbo.YeuCauDichVu — dịch vụ khách gọi
    --   Menu tổng: Dịch vụ
    --   Menu phụ: Yêu cầu dịch vụ
    --   Chức năng: service.order, service.manage
    --   Xem: MaLuotLuuTru, MaDichVu, Ten, SoLuong, DonGia, ThoiDiemGoi, ThoiDiemGiao.
    CREATE TABLE dbo.YeuCauDichVu (
        Ma bigint IDENTITY PRIMARY KEY,
        MaLuotLuuTru bigint NOT NULL REFERENCES dbo.LuotLuuTru(Ma),
        MaDichVu int NOT NULL REFERENCES dbo.DichVu(Ma),
        DanhMuc nvarchar(100) NOT NULL,
        Ten nvarchar(150) NOT NULL,
        SoLuong int NOT NULL CHECK(SoLuong BETWEEN 1 AND 100),
        DonGia decimal(18,2) NOT NULL CHECK(DonGia>=0),
        ThoiDiemGoi datetime2 NOT NULL,
        ThoiDiemGiao datetime2 NULL,
        MaNguoiTao int NOT NULL REFERENCES dbo.TaiKhoanNhanVien(Ma)
    );

CREATE INDEX IX_Orders_Stay ON dbo.YeuCauDichVu(MaLuotLuuTru);
CREATE INDEX IX_Orders_Pending ON dbo.YeuCauDichVu(ThoiDiemGoi) WHERE ThoiDiemGiao IS NULL;

    -- dbo.HoaDon — hóa đơn lưu trú
    --   Menu tổng: Hóa đơn/Báo cáo
    --   Menu phụ: Hóa đơn/Doanh thu
    --   Chức năng: invoice.list, report.revenue
    --   Xem: MaLuotLuuTru, TenKhach, ThoiDiemLapHoaDon, TienPhong, TienDichVu, TienCoc, TienDaThu, TienDaHoan.
    CREATE TABLE dbo.HoaDon (
        Ma bigint IDENTITY PRIMARY KEY,
        MaLuotLuuTru bigint NOT NULL UNIQUE REFERENCES dbo.LuotLuuTru(Ma),
        SoPhongHoaDon nvarchar(10) NOT NULL,
        TenKhach nvarchar(100) NOT NULL,
        ThoiDiemLapHoaDon datetime2 NOT NULL,
        TienPhong decimal(18,2) NOT NULL CHECK(TienPhong>=0),
        TienDichVu decimal(18,2) NOT NULL CHECK(TienDichVu>=0),
        TienCoc decimal(18,2) NOT NULL CHECK(TienCoc>=0),
        TienDaThu decimal(18,2) NOT NULL CHECK(TienDaThu>=0),
        TienDaHoan decimal(18,2) NOT NULL CHECK(TienDaHoan>=0),
        PhuongThucThanhToan nvarchar(30) NOT NULL,
        MaNguoiTao int NOT NULL REFERENCES dbo.TaiKhoanNhanVien(Ma),
        CHECK(TienPhong+TienDichVu=TienCoc+TienDaThu-TienDaHoan)
    );

CREATE INDEX IX_Invoices_Issued ON dbo.HoaDon(ThoiDiemLapHoaDon);

    -- dbo.GiaoDichThanhToan — dòng tiền cọc/thu/hoàn
    --   Menu tổng: Quản lý phòng/Thu chi
    --   Menu phụ: Đặt phòng/Lưu trú/Thu chi
    --   Chức năng: room.deposit, room.checkout, room.booking_cancel, cash.flow
    --   Xem: MaLuotLuuTru, LoaiGiaoDich, SoTien, ThoiDiemTao, PhuongThucThanhToan, GhiChu.
    CREATE TABLE dbo.GiaoDichThanhToan (
        Ma bigint IDENTITY PRIMARY KEY,
        MaLuotLuuTru bigint NOT NULL REFERENCES dbo.LuotLuuTru(Ma),
        LoaiGiaoDich varchar(20) NOT NULL CHECK(LoaiGiaoDich IN('Deposit','Checkout','Refund','Forfeit')),
        SoTien decimal(18,2) NOT NULL CHECK(SoTien>0),
        ThoiDiemTao datetime2 NOT NULL,
        PhuongThucThanhToan nvarchar(30) NOT NULL,
        GhiChu nvarchar(300) NOT NULL,
        MaNguoiTao int NOT NULL REFERENCES dbo.TaiKhoanNhanVien(Ma)
    );

CREATE INDEX IX_Payments_Date ON dbo.GiaoDichThanhToan(ThoiDiemTao);

    -- dbo.NhatKyThaoTac — nhật ký thao tác
    --   Menu tổng: Hệ thống
    --   Menu phụ: Nhật ký
    --   Chức năng: system.audit
    --   Xem: MaNhanVien, HanhDong, ChiTiet, ThoiDiemTao.
    CREATE TABLE dbo.NhatKyThaoTac (
        Ma bigint IDENTITY PRIMARY KEY,
        MaNhanVien int NOT NULL REFERENCES dbo.TaiKhoanNhanVien(Ma),
        HanhDong nvarchar(50) NOT NULL,
        ChiTiet nvarchar(500) NOT NULL,
        ThoiDiemTao datetime2 NOT NULL DEFAULT SYSDATETIME()
    );

INSERT dbo.PhienBanCSDL VALUES(1);
END;
IF NOT EXISTS(SELECT 1 FROM dbo.PhienBanCSDL WHERE PhienBan=1) THROW 51000, 'Unsupported schema version',1;
-- Điều chỉnh tương thích cho CSDL đã cài bằng bản SQL đầu tiên.
DECLARE @oldDateConstraint sysname;
SELECT @oldDateConstraint=name FROM sys.check_constraints
WHERE parent_object_id=OBJECT_ID(N'dbo.LuotLuuTru') AND definition=N'([ThoiDiemDi]>[ThoiDiemDen])';
IF @oldDateConstraint IS NOT NULL
BEGIN
 DECLARE @dropDateConstraint nvarchar(500)=N'ALTER TABLE dbo.LuotLuuTru DROP CONSTRAINT '+QUOTENAME(@oldDateConstraint);
 EXEC sys.sp_executesql @dropDateConstraint;
END;
IF NOT EXISTS(SELECT 1 FROM sys.check_constraints WHERE parent_object_id=OBJECT_ID(N'dbo.LuotLuuTru') AND name=N'CK_Stays_Departure')
 ALTER TABLE dbo.LuotLuuTru ADD CONSTRAINT CK_Stays_Departure CHECK(ThoiDiemDi>COALESCE(ThoiDiemNhanPhong,ThoiDiemDen));
IF NOT EXISTS(SELECT 1 FROM dbo.Phong)
BEGIN
 DECLARE @i int=1;
 WHILE @i<=50
 BEGIN
 INSERT dbo.Phong(SoPhong,Loai,DonGiaPhong,TienCoc) VALUES(
 CONVERT(nvarchar(10),CASE WHEN @i<=20 THEN 100+@i WHEN @i<=40 THEN 200+@i-20 ELSE 300+@i-40 END),
 CASE WHEN @i<=20 THEN N'Đơn' WHEN @i<=40 THEN N'Đôi' ELSE N'VIP' END,
 CASE WHEN @i<=40 THEN 500000 ELSE 1000000 END,CASE WHEN @i<=40 THEN 250000 ELSE 500000 END);
 SET @i+=1;
 END;
END;
COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (SELECT 1 FROM dbo.DichVu WHERE Ten = N'Nước suối Lavie 500ml')
    INSERT dbo.DichVu (DanhMuc, Ten, DonGia, DonViTinh)
    VALUES (N'Nước uống & Giải khát', N'Nước suối Lavie 500ml', 15000, N'Chai');

IF NOT EXISTS (SELECT 1 FROM dbo.DichVu WHERE Ten = N'Coca-Cola / Pepsi lon')
    INSERT dbo.DichVu (DanhMuc, Ten, DonGia, DonViTinh)
    VALUES (N'Nước uống & Giải khát', N'Coca-Cola / Pepsi lon', 25000, N'Lon');

IF NOT EXISTS (SELECT 1 FROM dbo.DichVu WHERE Ten = N'Bò Húc (Red Bull Thái)')
    INSERT dbo.DichVu (DanhMuc, Ten, DonGia, DonViTinh)
    VALUES (N'Nước uống & Giải khát', N'Bò Húc (Red Bull Thái)', 30000, N'Lon');

IF NOT EXISTS (SELECT 1 FROM dbo.DichVu WHERE Ten = N'Cà phê sữa đá pha phin')
    INSERT dbo.DichVu (DanhMuc, Ten, DonGia, DonViTinh)
    VALUES (N'Nước uống & Giải khát', N'Cà phê sữa đá pha phin', 35000, N'Ly');

IF NOT EXISTS (SELECT 1 FROM dbo.DichVu WHERE Ten = N'Nước dừa tươi nguyên quả')
    INSERT dbo.DichVu (DanhMuc, Ten, DonGia, DonViTinh)
    VALUES (N'Nước uống & Giải khát', N'Nước dừa tươi nguyên quả', 40000, N'Quả');

IF NOT EXISTS (SELECT 1 FROM dbo.DichVu WHERE Ten = N'Bia Heineken bạc')
    INSERT dbo.DichVu (DanhMuc, Ten, DonGia, DonViTinh)
    VALUES (N'Bia, Rượu & Đồ nhậu', N'Bia Heineken bạc', 35000, N'Lon');

IF NOT EXISTS (SELECT 1 FROM dbo.DichVu WHERE Ten = N'Bia Tiger nâu / Crystal')
    INSERT dbo.DichVu (DanhMuc, Ten, DonGia, DonViTinh)
    VALUES (N'Bia, Rượu & Đồ nhậu', N'Bia Tiger nâu / Crystal', 30000, N'Lon');

IF NOT EXISTS (SELECT 1 FROM dbo.DichVu WHERE Ten = N'Mực một nắng nướng xé sợi')
    INSERT dbo.DichVu (DanhMuc, Ten, DonGia, DonViTinh)
    VALUES (N'Bia, Rượu & Đồ nhậu', N'Mực một nắng nướng xé sợi', 150000, N'Đĩa');

IF NOT EXISTS (SELECT 1 FROM dbo.DichVu WHERE Ten = N'Khô bò sợi lá chanh loại 1')
    INSERT dbo.DichVu (DanhMuc, Ten, DonGia, DonViTinh)
    VALUES (N'Bia, Rượu & Đồ nhậu', N'Khô bò sợi lá chanh loại 1', 80000, N'Đĩa');

IF NOT EXISTS (SELECT 1 FROM dbo.DichVu WHERE Ten = N'Đậu phộng tỏi ớt / Hạt điều')
    INSERT dbo.DichVu (DanhMuc, Ten, DonGia, DonViTinh)
    VALUES (N'Bia, Rượu & Đồ nhậu', N'Đậu phộng tỏi ớt / Hạt điều', 40000, N'Gói');

IF NOT EXISTS (SELECT 1 FROM dbo.DichVu WHERE Ten = N'Chả bò gân cắt lát')
    INSERT dbo.DichVu (DanhMuc, Ten, DonGia, DonViTinh)
    VALUES (N'Bia, Rượu & Đồ nhậu', N'Chả bò gân cắt lát', 75000, N'Đĩa');

IF NOT EXISTS (SELECT 1 FROM dbo.DichVu WHERE Ten = N'Mì ly Modern / Hảo Hảo tôm chua cay')
    INSERT dbo.DichVu (DanhMuc, Ten, DonGia, DonViTinh)
    VALUES (N'Đồ ăn vặt & Đồ ăn nhanh', N'Mì ly Modern / Hảo Hảo tôm chua cay', 25000, N'Ly');

IF NOT EXISTS (SELECT 1 FROM dbo.DichVu WHERE Ten = N'Mì tôm xào trứng xúc xích')
    INSERT dbo.DichVu (DanhMuc, Ten, DonGia, DonViTinh)
    VALUES (N'Đồ ăn vặt & Đồ ăn nhanh', N'Mì tôm xào trứng xúc xích', 45000, N'Đĩa');

IF NOT EXISTS (SELECT 1 FROM dbo.DichVu WHERE Ten = N'Snack khoai tây Lay''s lớn')
    INSERT dbo.DichVu (DanhMuc, Ten, DonGia, DonViTinh)
    VALUES (N'Đồ ăn vặt & Đồ ăn nhanh', N'Snack khoai tây Lay''s lớn', 30000, N'Gói');

IF NOT EXISTS (SELECT 1 FROM dbo.DichVu WHERE Ten = N'Bánh mì kẹp xúc xích pate')
    INSERT dbo.DichVu (DanhMuc, Ten, DonGia, DonViTinh)
    VALUES (N'Đồ ăn vặt & Đồ ăn nhanh', N'Bánh mì kẹp xúc xích pate', 35000, N'Cái');

IF NOT EXISTS (SELECT 1 FROM dbo.DichVu WHERE Ten = N'Khô gà xé cay bơ tỏi')
    INSERT dbo.DichVu (DanhMuc, Ten, DonGia, DonViTinh)
    VALUES (N'Đồ ăn vặt & Đồ ăn nhanh', N'Khô gà xé cay bơ tỏi', 50000, N'Hũ');

IF NOT EXISTS (SELECT 1 FROM dbo.DichVu WHERE Ten = N'Giặt sấy thông thường (dưới 3kg)')
    INSERT dbo.DichVu (DanhMuc, Ten, DonGia, DonViTinh)
    VALUES (N'Giặt là & Chăm sóc trang phục', N'Giặt sấy thông thường (dưới 3kg)', 60000, N'Lượt');

IF NOT EXISTS (SELECT 1 FROM dbo.DichVu WHERE Ten = N'Ủi / Là phẳng sơ mi, quần âu')
    INSERT dbo.DichVu (DanhMuc, Ten, DonGia, DonViTinh)
    VALUES (N'Giặt là & Chăm sóc trang phục', N'Ủi / Là phẳng sơ mi, quần âu', 25000, N'Chiếc');

IF NOT EXISTS (SELECT 1 FROM dbo.DichVu WHERE Ten = N'Giặt khô áo vest / Váy dạ hội cao cấp')
    INSERT dbo.DichVu (DanhMuc, Ten, DonGia, DonViTinh)
    VALUES (N'Giặt là & Chăm sóc trang phục', N'Giặt khô áo vest / Váy dạ hội cao cấp', 120000, N'Bộ');

IF NOT EXISTS (SELECT 1 FROM dbo.DichVu WHERE Ten = N'Thuê xe máy tay ga (24h)')
    INSERT dbo.DichVu (DanhMuc, Ten, DonGia, DonViTinh)
    VALUES (N'Tiện ích & Dịch vụ khác', N'Thuê xe máy tay ga (24h)', 150000, N'Ngày');

IF NOT EXISTS (SELECT 1 FROM dbo.DichVu WHERE Ten = N'Đưa đón sân bay 4 chỗ')
    INSERT dbo.DichVu (DanhMuc, Ten, DonGia, DonViTinh)
    VALUES (N'Tiện ích & Dịch vụ khác', N'Đưa đón sân bay 4 chỗ', 250000, N'Chuyến');

IF NOT EXISTS (SELECT 1 FROM dbo.DichVu WHERE Ten = N'Dọn phòng đột xuất theo yêu cầu')
    INSERT dbo.DichVu (DanhMuc, Ten, DonGia, DonViTinh)
    VALUES (N'Tiện ích & Dịch vụ khác', N'Dọn phòng đột xuất theo yêu cầu', 50000, N'Lần');

COMMIT;
GO

-- BEGIN MIGRATION V2
-- Chạy trong CSDL ứng dụng sau khi tạo cấu trúc ban đầu; nâng cấp trong giao dịch và có thể chạy lại.
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
IF OBJECT_ID(N'dbo.PhienBanCSDL') IS NULL THROW 51000,N'Hãy chạy Database/Database.sql trước.',1;
IF EXISTS(SELECT 1 FROM dbo.PhienBanCSDL WHERE PhienBan>11) THROW 51000,N'Ứng dụng cũ hơn phiên bản dữ liệu.',1;
IF NOT EXISTS(SELECT 1 FROM dbo.PhienBanCSDL WHERE PhienBan=2)
BEGIN
    ALTER TABLE dbo.TaiKhoanNhanVien ADD PhienBanBaoMat bigint NOT NULL CONSTRAINT DF_Users_SecurityVersion DEFAULT 1;
    ALTER TABLE dbo.DichVu ADD PhienBan bigint NOT NULL CONSTRAINT DF_Services_Version DEFAULT 1;
    ALTER TABLE dbo.YeuCauDichVu ADD ThoiDiemHuy datetime2 NULL, LyDoHuy nvarchar(300) NULL,
        SoLuongDaGiao int NOT NULL CONSTRAINT DF_Orders_DeliveredQuantity DEFAULT 0;
    EXEC(N'UPDATE dbo.YeuCauDichVu SET SoLuongDaGiao=SoLuong WHERE ThoiDiemGiao IS NOT NULL;
        ALTER TABLE dbo.YeuCauDichVu ADD CONSTRAINT CK_Orders_DeliveredQuantity CHECK(SoLuongDaGiao>=0 AND SoLuongDaGiao<=SoLuong);');
    DROP INDEX UX_Stays_ActiveRoom ON dbo.LuotLuuTru;
    CREATE UNIQUE INDEX UX_Stays_OccupiedRoom ON dbo.LuotLuuTru(MaPhong) WHERE TrangThai='Occupied';
    CREATE INDEX IX_Stays_ReservationDates ON dbo.LuotLuuTru(MaPhong,ThoiDiemDen,ThoiDiemDi) INCLUDE(TrangThai,ConHieuLuc,HanGiuPhong);
    CREATE UNIQUE INDEX UX_Payments_Forfeit ON dbo.GiaoDichThanhToan(MaLuotLuuTru) WHERE LoaiGiaoDich='Forfeit';
    -- Lượt đặt trước không còn là trạng thái vật lý của phòng; giữ nguyên hạn nhận phòng đã lưu.
    UPDATE dbo.Phong SET TrangThai='Trong',PhienBan=PhienBan+1 WHERE TrangThai='DaDat';
    INSERT dbo.PhienBanCSDL VALUES(2);
END;
COMMIT;
-- END MIGRATION V2
GO

-- BEGIN MIGRATION V3
SET XACT_ABORT ON;
BEGIN TRANSACTION;
DECLARE @lock int;
EXEC @lock=sys.sp_getapplock @Resource=N'QLKhachSan.Write',@LockMode='Exclusive',@LockOwner='Transaction',@LockTimeout=10000;
IF @lock<0 THROW 51001,N'Không lấy được khóa nâng cấp dữ liệu.',1;
IF NOT EXISTS(SELECT 1 FROM dbo.PhienBanCSDL WHERE PhienBan=2) THROW 51000,N'Cần nâng cấp phiên bản 2 trước.',1;
IF NOT EXISTS(SELECT 1 FROM dbo.PhienBanCSDL WHERE PhienBan=3)
BEGIN
    DECLARE @roleConstraint sysname;
    SELECT @roleConstraint=name FROM sys.check_constraints
    WHERE parent_object_id=OBJECT_ID(N'dbo.TaiKhoanNhanVien') AND definition LIKE '%Reception%' AND definition LIKE '%Admin%';
    IF @roleConstraint IS NOT NULL
    BEGIN
        DECLARE @dropRole nvarchar(300)=N'ALTER TABLE dbo.TaiKhoanNhanVien DROP CONSTRAINT '+QUOTENAME(@roleConstraint);
        EXEC sys.sp_executesql @dropRole;
    END;
    ALTER TABLE dbo.TaiKhoanNhanVien ADD CONSTRAINT CK_Users_RoleV3 CHECK(VaiTro IN('Admin','Reception','Accountant','Manager'));
    ALTER TABLE dbo.TaiKhoanNhanVien ADD DaLuuTru bit NOT NULL CONSTRAINT DF_Users_Archived DEFAULT 0;
    DECLARE @usernameConstraint sysname;
    SELECT @usernameConstraint=kc.name FROM sys.key_constraints kc
    JOIN sys.index_columns ic ON ic.object_id=kc.parent_object_id AND ic.index_id=kc.unique_index_id
    JOIN sys.columns c ON c.object_id=ic.object_id AND c.column_id=ic.column_id
    WHERE kc.parent_object_id=OBJECT_ID(N'dbo.TaiKhoanNhanVien') AND kc.type='UQ' AND c.name='TenDangNhap';
    IF @usernameConstraint IS NOT NULL
    BEGIN
        DECLARE @dropUsername nvarchar(300)=N'ALTER TABLE dbo.TaiKhoanNhanVien DROP CONSTRAINT '+QUOTENAME(@usernameConstraint);
        EXEC sys.sp_executesql @dropUsername;
    END;
    EXEC(N'CREATE UNIQUE INDEX UX_Users_ActiveUsername ON dbo.TaiKhoanNhanVien(TenDangNhap) WHERE DaLuuTru=0');
    -- Giữ tài khoản cũ để hóa đơn và nhật ký còn tham chiếu được, nhưng ngừng cho đăng nhập.
    EXEC(N'UPDATE dbo.TaiKhoanNhanVien SET DaLuuTru=1,DangHoatDong=0,PhienBanBaoMat=PhienBanBaoMat+1,KhoaDen=NULL WHERE DaLuuTru=0');
    INSERT dbo.PhienBanCSDL VALUES(3);
END;
COMMIT;
-- END MIGRATION V3
GO

-- BEGIN MIGRATION V4
SET XACT_ABORT ON;
BEGIN TRANSACTION;
DECLARE @lock int;
EXEC @lock=sys.sp_getapplock @Resource=N'QLKhachSan.Write',@LockMode='Exclusive',@LockOwner='Transaction',@LockTimeout=10000;
IF @lock<0 THROW 51001,N'Không lấy được khóa nâng cấp dữ liệu.',1;
IF NOT EXISTS(SELECT 1 FROM dbo.PhienBanCSDL WHERE PhienBan=3) THROW 51000,N'Cần nâng cấp phiên bản 3 trước.',1;
IF NOT EXISTS(SELECT 1 FROM dbo.PhienBanCSDL WHERE PhienBan=4)
BEGIN
    DECLARE @typeConstraint sysname;
    SELECT TOP(1) @typeConstraint=name FROM sys.check_constraints
    WHERE parent_object_id=OBJECT_ID(N'dbo.Phong') AND definition LIKE N'%Loai%' AND definition LIKE N'%VIP%';
    IF @typeConstraint IS NOT NULL
    BEGIN
        DECLARE @dropType nvarchar(300)=N'ALTER TABLE dbo.Phong DROP CONSTRAINT '+QUOTENAME(@typeConstraint);
        EXEC sys.sp_executesql @dropType;
    END;
    ALTER TABLE dbo.Phong ADD CONSTRAINT CK_Rooms_TypeV4 CHECK(Loai IN(N'Đơn',N'Đôi',N'VIP',N'Tình nhân'));
    DECLARE @number int=401,@created int=0;
    WHILE @created<10
    BEGIN
        IF NOT EXISTS(SELECT 1 FROM dbo.Phong WHERE SoPhong=CONVERT(nvarchar(10),@number))
        BEGIN
            INSERT dbo.Phong(SoPhong,Loai,DonGiaPhong,TienCoc) VALUES(CONVERT(nvarchar(10),@number),N'Tình nhân',750000,350000);
            SET @created+=1;
        END;
        SET @number+=1;
    END;
    INSERT dbo.PhienBanCSDL VALUES(4);
END;
COMMIT;
-- END MIGRATION V4
GO

-- BEGIN MIGRATION V5
SET XACT_ABORT ON;
BEGIN TRANSACTION;
DECLARE @lock int;
EXEC @lock=sys.sp_getapplock @Resource=N'QLKhachSan.Write',@LockMode='Exclusive',@LockOwner='Transaction',@LockTimeout=10000;
IF @lock<0 THROW 51001,N'Không lấy được khóa nâng cấp dữ liệu.',1;
IF NOT EXISTS(SELECT 1 FROM dbo.PhienBanCSDL WHERE PhienBan=4) THROW 51000,N'Cần nâng cấp phiên bản 4 trước.',1;
IF NOT EXISTS(SELECT 1 FROM dbo.PhienBanCSDL WHERE PhienBan=5)
BEGIN
    UPDATE dbo.Phong SET Loai=N'VIP'
    WHERE SoPhong IN (N'401',N'402',N'403',N'404',N'405',N'406',N'407',N'408',N'409',N'410');
    INSERT dbo.PhienBanCSDL VALUES(5);
END;
COMMIT;
-- END MIGRATION V5
GO

-- BEGIN MIGRATION V6
SET XACT_ABORT ON;
BEGIN TRANSACTION;
DECLARE @lock int;
EXEC @lock=sys.sp_getapplock @Resource=N'QLKhachSan.Write',@LockMode='Exclusive',@LockOwner='Transaction',@LockTimeout=10000;
IF @lock<0 THROW 51001,N'Không lấy được khóa nâng cấp dữ liệu.',1;
IF NOT EXISTS(SELECT 1 FROM dbo.PhienBanCSDL WHERE PhienBan=5) THROW 51000,N'Cần nâng cấp phiên bản 5 trước.',1;
IF NOT EXISTS(SELECT 1 FROM dbo.PhienBanCSDL WHERE PhienBan=6)
BEGIN
    -- dbo.KyKeToan — kỳ kế toán đã khóa
    --   Menu tổng: Ca trực
    --   Menu phụ: Ca trực
    --   Chức năng: shift.manage
    --   Xem: NgayDauKy, ThoiDiemKhoa, MaNguoiKhoa, GhiChu.
    CREATE TABLE dbo.KyKeToan (
        NgayDauKy date NOT NULL PRIMARY KEY,
        ThoiDiemKhoa datetime2 NOT NULL DEFAULT SYSDATETIME(),
        MaNguoiKhoa int NOT NULL REFERENCES dbo.TaiKhoanNhanVien(Ma),
        GhiChu nvarchar(300) NOT NULL
    );

    -- dbo.CaTruc — ca thu ngân
    --   Menu tổng: Ca trực
    --   Menu phụ: Ca trực
    --   Chức năng: shift.manage
    --   Xem: MaThuNgan, ThoiDiemMoCa, ThoiDiemDongCa, TienMatDauCa, TienMatKiemDem, TienMatDuKien, TrangThai.
    CREATE TABLE dbo.CaTruc (
        Ma bigint IDENTITY PRIMARY KEY,
        MaThuNgan int NOT NULL REFERENCES dbo.TaiKhoanNhanVien(Ma),
        ThoiDiemMoCa datetime2 NOT NULL DEFAULT SYSDATETIME(),
        ThoiDiemDongCa datetime2 NULL,
        TienMatDauCa decimal(18,2) NOT NULL CHECK(TienMatDauCa>=0),
        TienMatKiemDem decimal(18,2) NULL CHECK(TienMatKiemDem>=0),
        TienMatDuKien decimal(18,2) NULL,
        GiaiTrinh nvarchar(500) NULL,
        TrangThai varchar(12) NOT NULL DEFAULT 'Open' CHECK(TrangThai IN('Open','Submitted','Locked')),
        ThoiDiemKhoa datetime2 NULL,
        MaNguoiKhoa int NULL REFERENCES dbo.TaiKhoanNhanVien(Ma),
        CONSTRAINT CK_CashShifts_Close CHECK((TrangThai='Open' AND ThoiDiemDongCa IS NULL AND TienMatKiemDem IS NULL) OR (TrangThai<>'Open' AND ThoiDiemDongCa IS NOT NULL AND TienMatKiemDem IS NOT NULL))
    );

 CREATE UNIQUE INDEX UX_CashShifts_Open ON dbo.CaTruc(MaThuNgan) WHERE TrangThai='Open';
 ALTER TABLE dbo.GiaoDichThanhToan ADD MaCaTruc bigint NULL REFERENCES dbo.CaTruc(Ma), MaThamChieuNgoai nvarchar(100) NULL;
 ALTER TABLE dbo.YeuCauDichVu ADD MaCaTruc bigint NULL REFERENCES dbo.CaTruc(Ma);
 -- Tạo chỉ mục sau ALTER TABLE để SQL Server nhận diện các cột vừa thêm.
 EXEC(N'CREATE INDEX IX_Payments_Shift ON dbo.GiaoDichThanhToan(MaCaTruc) WHERE MaCaTruc IS NOT NULL');
 EXEC(N'CREATE UNIQUE INDEX UX_Payments_ExternalReference ON dbo.GiaoDichThanhToan(PhuongThucThanhToan,MaThamChieuNgoai) WHERE MaThamChieuNgoai IS NOT NULL');

    -- dbo.PhieuThuChi — phiếu thu/chi
    --   Menu tổng: Thu chi
    --   Menu phụ: Sổ quỹ/Công nợ
    --   Chức năng: cash.book, cash.debt
    --   Xem: LoaiPhieu, ThoiDiemHachToan, KenhThanhToan, DanhMuc, SoTien, DoiTac, MaThamChieu.
    CREATE TABLE dbo.PhieuThuChi (
        Ma bigint IDENTITY PRIMARY KEY,
        LoaiPhieu varchar(10) NOT NULL CHECK(LoaiPhieu IN('Receipt','Payment')),
        ThoiDiemHachToan datetime2 NOT NULL DEFAULT SYSDATETIME(),
        KenhThanhToan varchar(12) NOT NULL CHECK(KenhThanhToan IN('Cash','Bank','POS','OTA')),
        DanhMuc nvarchar(80) NOT NULL,
        SoTien decimal(18,2) NOT NULL CHECK(SoTien>0),
        DoiTac nvarchar(150) NOT NULL,
        MaThamChieu nvarchar(100) NULL,
        GhiChu nvarchar(500) NOT NULL,
        MaNguoiTao int NOT NULL REFERENCES dbo.TaiKhoanNhanVien(Ma),
        MaCaTruc bigint NULL REFERENCES dbo.CaTruc(Ma),
        ThoiDiemDaoButToan datetime2 NULL,
        MaNguoiDaoButToan int NULL REFERENCES dbo.TaiKhoanNhanVien(Ma),
        LyDoDaoButToan nvarchar(500) NULL
    );

 CREATE INDEX IX_FinanceVouchers_Posted ON dbo.PhieuThuChi(ThoiDiemHachToan,KenhThanhToan);

    -- dbo.SoDuDauKy — số dư đầu kỳ
    --   Menu tổng: Thu chi
    --   Menu phụ: Sổ quỹ
    --   Chức năng: cash.book
    --   Xem: KenhThanhToan, TinhDenThoiDiem, SoTien, ThoiDiemThietLap.
    CREATE TABLE dbo.SoDuDauKy (
        KenhThanhToan varchar(12) NOT NULL PRIMARY KEY CHECK(KenhThanhToan IN('Cash','Bank','POS','OTA')),
        TinhDenThoiDiem datetime2 NOT NULL,
        SoTien decimal(18,2) NOT NULL,
        ThoiDiemThietLap datetime2 NOT NULL DEFAULT SYSDATETIME(),
        MaNguoiThietLap int NOT NULL REFERENCES dbo.TaiKhoanNhanVien(Ma),
        GhiChu nvarchar(300) NOT NULL
    );

 CREATE UNIQUE INDEX UX_FinanceVouchers_Reference ON dbo.PhieuThuChi(KenhThanhToan,MaThamChieu) WHERE MaThamChieu IS NOT NULL AND ThoiDiemDaoButToan IS NULL;

    -- dbo.DongSaoKe — sao kê ngân hàng/POS
    --   Menu tổng: Thu chi
    --   Menu phụ: Đối soát
    --   Chức năng: cash.bank
    --   Xem: ThoiDiemGiaoDich, KenhThanhToan, MaThamChieu, SoTien, MaGiaoDichDaDoiSoat, MaPhieuDaDoiSoat.
    CREATE TABLE dbo.DongSaoKe (
        Ma bigint IDENTITY PRIMARY KEY,
        ThoiDiemGiaoDich datetime2 NOT NULL,
        KenhThanhToan varchar(4) NOT NULL CHECK(KenhThanhToan IN('Bank','POS')),
        MaThamChieu nvarchar(100) NOT NULL,
        SoTien decimal(18,2) NOT NULL CHECK(SoTien<>0),
        ThoiDiemNhap datetime2 NOT NULL DEFAULT SYSDATETIME(),
        MaNguoiNhap int NOT NULL REFERENCES dbo.TaiKhoanNhanVien(Ma),
        MaGiaoDichDaDoiSoat bigint NULL REFERENCES dbo.GiaoDichThanhToan(Ma),
        MaPhieuDaDoiSoat bigint NULL REFERENCES dbo.PhieuThuChi(Ma),
        CONSTRAINT UX_BankStatementLines_Reference UNIQUE(KenhThanhToan,MaThamChieu),
        CONSTRAINT CK_BankStatementLines_Match CHECK(MaGiaoDichDaDoiSoat IS NULL OR MaPhieuDaDoiSoat IS NULL)
    );

    -- dbo.CongNo — công nợ phải thu/phải trả
    --   Menu tổng: Thu chi
    --   Menu phụ: Công nợ
    --   Chức năng: cash.debt
    --   Xem: LoaiCongNo, DoiTac, MaHoaDon, HanThanhToan, SoTien, ThoiDiemHuyBo.
    CREATE TABLE dbo.CongNo (
        Ma bigint IDENTITY PRIMARY KEY,
        LoaiCongNo varchar(2) NOT NULL CHECK(LoaiCongNo IN('AR','AP')),
        DoiTac nvarchar(150) NOT NULL,
        MaHoaDon bigint NULL REFERENCES dbo.HoaDon(Ma),
        ThoiDiemPhatSinhCongNo datetime2 NOT NULL DEFAULT SYSDATETIME(),
        HanThanhToan date NOT NULL,
        SoTien decimal(18,2) NOT NULL CHECK(SoTien>0),
        GhiChu nvarchar(500) NOT NULL,
        MaNguoiTao int NOT NULL REFERENCES dbo.TaiKhoanNhanVien(Ma),
        ThoiDiemHuyBo datetime2 NULL,
        MaNguoiHuy int NULL REFERENCES dbo.TaiKhoanNhanVien(Ma),
        LyDoHuy nvarchar(500) NULL
    );

 CREATE INDEX IX_FinanceDebts_Due ON dbo.CongNo(LoaiCongNo,HanThanhToan);

    -- dbo.PhanBoCongNo — phân bổ phiếu vào công nợ
    --   Menu tổng: Thu chi
    --   Menu phụ: Công nợ
    --   Chức năng: cash.debt
    --   Xem: MaCongNo, MaPhieuThuChi, SoTien, ThoiDiemLap.
    CREATE TABLE dbo.PhanBoCongNo (
        Ma bigint IDENTITY PRIMARY KEY,
        MaCongNo bigint NOT NULL REFERENCES dbo.CongNo(Ma),
        MaPhieuThuChi bigint NOT NULL REFERENCES dbo.PhieuThuChi(Ma),
        SoTien decimal(18,2) NOT NULL CHECK(SoTien>0),
        ThoiDiemLap datetime2 NOT NULL DEFAULT SYSDATETIME(),
        MaNguoiTao int NOT NULL REFERENCES dbo.TaiKhoanNhanVien(Ma),
        CONSTRAINT UX_DebtAllocations UNIQUE(MaCongNo,MaPhieuThuChi)
    );

    -- dbo.HangTonKho — mặt hàng tồn kho
    --   Menu tổng: Dịch vụ
    --   Menu phụ: Kho dịch vụ
    --   Chức năng: service.stock
    --   Xem: Ten, DonViTinh, SoLuong, GiaVonBinhQuan, NguongDatHang, DangHoatDong.
    CREATE TABLE dbo.HangTonKho (
        Ma int IDENTITY PRIMARY KEY,
        MaDichVu int NULL UNIQUE REFERENCES dbo.DichVu(Ma),
        Ten nvarchar(150) NOT NULL UNIQUE,
        DonViTinh nvarchar(30) NOT NULL,
        SoLuong decimal(18,3) NOT NULL DEFAULT 0 CHECK(SoLuong>=0),
        GiaVonBinhQuan decimal(18,2) NOT NULL DEFAULT 0 CHECK(GiaVonBinhQuan>=0),
        NguongDatHang decimal(18,3) NOT NULL DEFAULT 0 CHECK(NguongDatHang>=0),
        DangHoatDong bit NOT NULL DEFAULT 1
    );

    -- dbo.BienDongKho — nhập/xuất/điều chỉnh kho
    --   Menu tổng: Dịch vụ
    --   Menu phụ: Kho dịch vụ
    --   Chức năng: service.stock
    --   Xem: MaMatHang, ThoiDiemBienDong, LoaiGiaoDich, SoLuong, GiaVonDonVi, LyDo.
    CREATE TABLE dbo.BienDongKho (
        Ma bigint IDENTITY PRIMARY KEY,
        MaMatHang int NOT NULL REFERENCES dbo.HangTonKho(Ma),
        ThoiDiemBienDong datetime2 NOT NULL DEFAULT SYSDATETIME(),
        LoaiGiaoDich varchar(12) NOT NULL CHECK(LoaiGiaoDich IN('Purchase','Sale','Spoilage','Internal','Adjustment')),
        SoLuong decimal(18,3) NOT NULL CHECK(SoLuong<>0),
        GiaVonDonVi decimal(18,2) NOT NULL CHECK(GiaVonDonVi>=0),
        MaYeuCauDichVu bigint NULL REFERENCES dbo.YeuCauDichVu(Ma),
        LyDo nvarchar(500) NOT NULL,
        MaNguoiTao int NOT NULL REFERENCES dbo.TaiKhoanNhanVien(Ma)
    );

 CREATE INDEX IX_StockMovements_Date ON dbo.BienDongKho(ThoiDiemBienDong,MaMatHang);

    -- dbo.TieuThuBuongPhong — báo tiêu thụ buồng phòng để đối chiếu kho
    --   Menu tổng: Dịch vụ
    --   Menu phụ: Kho dịch vụ
    --   Chức năng: service.stock
    --   Xem: MaMatHang, MaLuotLuuTru, ThoiDiemBaoCao, SoLuong, GhiChu.
    CREATE TABLE dbo.TieuThuBuongPhong (
        Ma bigint IDENTITY PRIMARY KEY,
        MaMatHang int NOT NULL REFERENCES dbo.HangTonKho(Ma),
        MaLuotLuuTru bigint NULL REFERENCES dbo.LuotLuuTru(Ma),
        ThoiDiemBaoCao datetime2 NOT NULL DEFAULT SYSDATETIME(),
        SoLuong decimal(18,3) NOT NULL CHECK(SoLuong>0),
        GhiChu nvarchar(300) NOT NULL,
        MaNguoiTao int NOT NULL REFERENCES dbo.TaiKhoanNhanVien(Ma)
    );

    -- dbo.ThongTinTaiChinhHoaDon — thông tin thuế/phí/số hóa đơn điện tử
    --   Menu tổng: Hóa đơn
    --   Menu phụ: Kiểm soát
    --   Chức năng: invoice.control
    --   Xem: MaHoaDon, TyLeVAT, TyLePhiDichVu, DonViLamTron, SoHoaDonDienTu.
    CREATE TABLE dbo.ThongTinTaiChinhHoaDon (
        MaHoaDon bigint NOT NULL PRIMARY KEY REFERENCES dbo.HoaDon(Ma),
        TyLeVAT decimal(5,2) NOT NULL DEFAULT 0 CHECK(TyLeVAT IN(0,8,10)),
        TyLePhiDichVu decimal(5,2) NOT NULL DEFAULT 0 CHECK(TyLePhiDichVu IN(0,5)),
        DonViLamTron int NOT NULL DEFAULT 1 CHECK(DonViLamTron IN(1,100,500,1000)),
        SoHoaDonDienTu nvarchar(100) NULL UNIQUE,
        ThoiDiemPhatHanhHoaDonDienTu datetime2 NULL,
        CONSTRAINT CK_InvoiceFinance_EInvoice CHECK((SoHoaDonDienTu IS NULL AND ThoiDiemPhatHanhHoaDonDienTu IS NULL) OR (SoHoaDonDienTu IS NOT NULL AND ThoiDiemPhatHanhHoaDonDienTu IS NOT NULL))
    );

    -- dbo.DieuChinhHoaDon — giảm trừ hóa đơn
    --   Menu tổng: Hóa đơn
    --   Menu phụ: Kiểm soát
    --   Chức năng: invoice.control
    --   Xem: MaHoaDon, DanhMuc, SoTien, LyDo, MaNguoiPheDuyet, ThoiDiemLap.
    CREATE TABLE dbo.DieuChinhHoaDon (
        Ma bigint IDENTITY PRIMARY KEY,
        MaHoaDon bigint NOT NULL REFERENCES dbo.HoaDon(Ma),
        DanhMuc varchar(20) NOT NULL CHECK(DanhMuc IN('ServiceFailure','VIP','Voucher')),
        SoTien decimal(18,2) NOT NULL CHECK(SoTien>0),
        LyDo nvarchar(500) NOT NULL,
        MaNguoiPheDuyet int NOT NULL REFERENCES dbo.TaiKhoanNhanVien(Ma),
        ThoiDiemLap datetime2 NOT NULL DEFAULT SYSDATETIME()
    );

    -- dbo.HoaDonHuy — hóa đơn đã hủy
    --   Menu tổng: Hóa đơn
    --   Menu phụ: Kiểm soát
    --   Chức năng: invoice.control
    --   Xem: MaHoaDon, MaLyDo, GiaiTrinh, ThoiDiemHuyHoaDon, MaNguoiHuyHoaDon.
    CREATE TABLE dbo.HoaDonHuy (
        MaHoaDon bigint NOT NULL PRIMARY KEY REFERENCES dbo.HoaDon(Ma),
        MaLyDo varchar(20) NOT NULL CHECK(MaLyDo IN('GuestCancelled','WrongRoomType','RoomChange')),
        GiaiTrinh nvarchar(500) NOT NULL,
        ThoiDiemHuyHoaDon datetime2 NOT NULL DEFAULT SYSDATETIME(),
        MaNguoiHuyHoaDon int NOT NULL REFERENCES dbo.TaiKhoanNhanVien(Ma)
    );

    -- dbo.NhomHoaDon — nhóm bill
    --   Menu tổng: Hóa đơn
    --   Menu phụ: Nhóm bill
    --   Chức năng: invoice.groups
    --   Xem: Ma, Ten, ThoiDiemLap, MaNguoiTao.
    CREATE TABLE dbo.NhomHoaDon (
        Ma bigint IDENTITY PRIMARY KEY,
        Ten nvarchar(150) NOT NULL,
        ThoiDiemLap datetime2 NOT NULL DEFAULT SYSDATETIME(),
        MaNguoiTao int NOT NULL REFERENCES dbo.TaiKhoanNhanVien(Ma)
    );

    -- dbo.PhanChiaHoaDon — phần tiền chia cho người trả trong nhóm bill
    --   Menu tổng: Hóa đơn
    --   Menu phụ: Nhóm bill
    --   Chức năng: invoice.groups
    --   Xem: MaNhomHoaDon, MaHoaDon, NguoiThanhToan, SoTien.
    CREATE TABLE dbo.PhanChiaHoaDon (
        Ma bigint IDENTITY PRIMARY KEY,
        MaNhomHoaDon bigint NOT NULL REFERENCES dbo.NhomHoaDon(Ma),
        MaHoaDon bigint NOT NULL REFERENCES dbo.HoaDon(Ma),
        NguoiThanhToan nvarchar(150) NOT NULL,
        SoTien decimal(18,2) NOT NULL CHECK(SoTien>0)
    );

 CREATE INDEX IX_BillShares_Invoice ON dbo.PhanChiaHoaDon(MaHoaDon);
 INSERT dbo.PhienBanCSDL VALUES(6);
END;
-- Trigger bảo vệ dữ liệu trên mọi máy, kể cả ứng dụng bản cũ; dữ liệu lịch sử vẫn đọc được.
EXEC(N'CREATE OR ALTER TRIGGER dbo.TR_Payments_AccountingGuard ON dbo.GiaoDichThanhToan AFTER INSERT,UPDATE,DELETE AS
BEGIN
 SET NOCOUNT ON;
 IF EXISTS(SELECT 1 FROM inserted WHERE EXISTS(SELECT 1 FROM dbo.KyKeToan p WHERE p.NgayDauKy=DATEFROMPARTS(YEAR(inserted.ThoiDiemTao),MONTH(inserted.ThoiDiemTao),1)))
 OR EXISTS(SELECT 1 FROM deleted WHERE EXISTS(SELECT 1 FROM dbo.KyKeToan p WHERE p.NgayDauKy=DATEFROMPARTS(YEAR(deleted.ThoiDiemTao),MONTH(deleted.ThoiDiemTao),1)))
 THROW 51101,N''Kỳ kế toán đã khóa.'',1;
 IF EXISTS(SELECT 1 FROM inserted i JOIN dbo.CaTruc s ON s.Ma=i.MaCaTruc WHERE s.TrangThai=''Locked'')
 OR EXISTS(SELECT 1 FROM deleted d JOIN dbo.CaTruc s ON s.Ma=d.MaCaTruc WHERE s.TrangThai=''Locked'')
 THROW 51102,N''Ca đã khóa.'',1;
END');
EXEC(N'CREATE OR ALTER TRIGGER dbo.TR_Invoices_AccountingGuard ON dbo.HoaDon AFTER INSERT,UPDATE,DELETE AS
BEGIN
 SET NOCOUNT ON;
 IF EXISTS(SELECT 1 FROM inserted i JOIN dbo.KyKeToan p ON p.NgayDauKy=DATEFROMPARTS(YEAR(i.ThoiDiemLapHoaDon),MONTH(i.ThoiDiemLapHoaDon),1))
 OR EXISTS(SELECT 1 FROM deleted d JOIN dbo.KyKeToan p ON p.NgayDauKy=DATEFROMPARTS(YEAR(d.ThoiDiemLapHoaDon),MONTH(d.ThoiDiemLapHoaDon),1))
 THROW 51101,N''Kỳ kế toán đã khóa.'',1;
 IF EXISTS(SELECT 1 FROM deleted d JOIN dbo.GiaoDichThanhToan x ON x.MaLuotLuuTru=d.MaLuotLuuTru JOIN dbo.CaTruc s ON s.Ma=x.MaCaTruc WHERE s.TrangThai=''Locked'')
 THROW 51102,N''Hóa đơn thuộc ca đã khóa.'',1;
END');
EXEC(N'CREATE OR ALTER TRIGGER dbo.TR_Vouchers_AccountingGuard ON dbo.PhieuThuChi AFTER INSERT,UPDATE,DELETE AS
BEGIN
 SET NOCOUNT ON;
 IF EXISTS(SELECT 1 FROM inserted i JOIN dbo.KyKeToan p ON p.NgayDauKy=DATEFROMPARTS(YEAR(i.ThoiDiemHachToan),MONTH(i.ThoiDiemHachToan),1))
 OR EXISTS(SELECT 1 FROM deleted d JOIN dbo.KyKeToan p ON p.NgayDauKy=DATEFROMPARTS(YEAR(d.ThoiDiemHachToan),MONTH(d.ThoiDiemHachToan),1))
 THROW 51101,N''Kỳ kế toán đã khóa.'',1;
 IF EXISTS(SELECT 1 FROM inserted i JOIN dbo.CaTruc s ON s.Ma=i.MaCaTruc WHERE s.TrangThai=''Locked'')
 OR EXISTS(SELECT 1 FROM deleted d JOIN dbo.CaTruc s ON s.Ma=d.MaCaTruc WHERE s.TrangThai=''Locked'')
 THROW 51102,N''Ca đã khóa.'',1;
END');
EXEC(N'CREATE OR ALTER TRIGGER dbo.TR_Stock_AccountingGuard ON dbo.BienDongKho AFTER INSERT,UPDATE,DELETE AS
BEGIN
 SET NOCOUNT ON;
 IF EXISTS(SELECT 1 FROM inserted i JOIN dbo.KyKeToan p ON p.NgayDauKy=DATEFROMPARTS(YEAR(i.ThoiDiemBienDong),MONTH(i.ThoiDiemBienDong),1))
 OR EXISTS(SELECT 1 FROM deleted d JOIN dbo.KyKeToan p ON p.NgayDauKy=DATEFROMPARTS(YEAR(d.ThoiDiemBienDong),MONTH(d.ThoiDiemBienDong),1))
 THROW 51101,N''Kỳ kế toán đã khóa.'',1;
END');
EXEC(N'CREATE OR ALTER TRIGGER dbo.TR_Debts_AccountingGuard ON dbo.CongNo AFTER INSERT,UPDATE,DELETE AS
BEGIN
 SET NOCOUNT ON;
 IF EXISTS(SELECT 1 FROM inserted i JOIN dbo.KyKeToan p ON p.NgayDauKy=DATEFROMPARTS(YEAR(i.ThoiDiemPhatSinhCongNo),MONTH(i.ThoiDiemPhatSinhCongNo),1))
 OR EXISTS(SELECT 1 FROM deleted d JOIN dbo.KyKeToan p ON p.NgayDauKy=DATEFROMPARTS(YEAR(d.ThoiDiemPhatSinhCongNo),MONTH(d.ThoiDiemPhatSinhCongNo),1))
 THROW 51101,N''Kỳ kế toán đã khóa.'',1;
END');
EXEC(N'CREATE OR ALTER TRIGGER dbo.TR_Orders_AccountingGuard ON dbo.YeuCauDichVu AFTER INSERT,UPDATE,DELETE AS
BEGIN
 SET NOCOUNT ON;
 IF EXISTS(SELECT 1 FROM inserted i JOIN dbo.KyKeToan p ON p.NgayDauKy=DATEFROMPARTS(YEAR(i.ThoiDiemGoi),MONTH(i.ThoiDiemGoi),1))
 OR EXISTS(SELECT 1 FROM deleted d JOIN dbo.KyKeToan p ON p.NgayDauKy=DATEFROMPARTS(YEAR(d.ThoiDiemGoi),MONTH(d.ThoiDiemGoi),1))
 THROW 51101,N''Kỳ kế toán đã khóa.'',1;
 IF EXISTS(SELECT 1 FROM inserted i JOIN dbo.CaTruc s ON s.Ma=i.MaCaTruc WHERE s.TrangThai=''Locked'')
 OR EXISTS(SELECT 1 FROM deleted d JOIN dbo.CaTruc s ON s.Ma=d.MaCaTruc WHERE s.TrangThai=''Locked'')
 THROW 51102,N''Dịch vụ thuộc ca đã khóa.'',1;
END');
EXEC(N'CREATE OR ALTER TRIGGER dbo.TR_Adjustments_AccountingGuard ON dbo.DieuChinhHoaDon AFTER INSERT,UPDATE,DELETE AS
BEGIN
 SET NOCOUNT ON;
 IF EXISTS(SELECT 1 FROM inserted i JOIN dbo.HoaDon v ON v.Ma=i.MaHoaDon JOIN dbo.KyKeToan p ON p.NgayDauKy=DATEFROMPARTS(YEAR(v.ThoiDiemLapHoaDon),MONTH(v.ThoiDiemLapHoaDon),1))
 OR EXISTS(SELECT 1 FROM deleted d JOIN dbo.HoaDon v ON v.Ma=d.MaHoaDon JOIN dbo.KyKeToan p ON p.NgayDauKy=DATEFROMPARTS(YEAR(v.ThoiDiemLapHoaDon),MONTH(v.ThoiDiemLapHoaDon),1))
 THROW 51101,N''Kỳ kế toán đã khóa.'',1;
END');
EXEC(N'CREATE OR ALTER TRIGGER dbo.TR_Voids_AccountingGuard ON dbo.HoaDonHuy AFTER INSERT,UPDATE,DELETE AS
BEGIN
 SET NOCOUNT ON;
 IF EXISTS(SELECT 1 FROM inserted i JOIN dbo.HoaDon v ON v.Ma=i.MaHoaDon JOIN dbo.KyKeToan p ON p.NgayDauKy=DATEFROMPARTS(YEAR(v.ThoiDiemLapHoaDon),MONTH(v.ThoiDiemLapHoaDon),1))
 OR EXISTS(SELECT 1 FROM deleted d JOIN dbo.HoaDon v ON v.Ma=d.MaHoaDon JOIN dbo.KyKeToan p ON p.NgayDauKy=DATEFROMPARTS(YEAR(v.ThoiDiemLapHoaDon),MONTH(v.ThoiDiemLapHoaDon),1))
 THROW 51101,N''Kỳ kế toán đã khóa.'',1;
END');
EXEC(N'CREATE OR ALTER TRIGGER dbo.TR_EInvoice_AccountingGuard ON dbo.ThongTinTaiChinhHoaDon AFTER INSERT,UPDATE,DELETE AS
BEGIN
 SET NOCOUNT ON;
 IF EXISTS(SELECT 1 FROM inserted i JOIN dbo.HoaDon v ON v.Ma=i.MaHoaDon JOIN dbo.KyKeToan p ON p.NgayDauKy=DATEFROMPARTS(YEAR(v.ThoiDiemLapHoaDon),MONTH(v.ThoiDiemLapHoaDon),1))
 OR EXISTS(SELECT 1 FROM deleted d JOIN dbo.HoaDon v ON v.Ma=d.MaHoaDon JOIN dbo.KyKeToan p ON p.NgayDauKy=DATEFROMPARTS(YEAR(v.ThoiDiemLapHoaDon),MONTH(v.ThoiDiemLapHoaDon),1))
 THROW 51101,N''Kỳ kế toán đã khóa.'',1;
END');
EXEC(N'CREATE OR ALTER TRIGGER dbo.TR_BillShares_AccountingGuard ON dbo.PhanChiaHoaDon AFTER INSERT,UPDATE,DELETE AS
BEGIN
 SET NOCOUNT ON;
 IF EXISTS(SELECT 1 FROM inserted i JOIN dbo.HoaDon v ON v.Ma=i.MaHoaDon JOIN dbo.KyKeToan p ON p.NgayDauKy=DATEFROMPARTS(YEAR(v.ThoiDiemLapHoaDon),MONTH(v.ThoiDiemLapHoaDon),1))
 OR EXISTS(SELECT 1 FROM deleted d JOIN dbo.HoaDon v ON v.Ma=d.MaHoaDon JOIN dbo.KyKeToan p ON p.NgayDauKy=DATEFROMPARTS(YEAR(v.ThoiDiemLapHoaDon),MONTH(v.ThoiDiemLapHoaDon),1))
 THROW 51101,N''Kỳ kế toán đã khóa.'',1;
END');
EXEC(N'CREATE OR ALTER TRIGGER dbo.TR_Shifts_AccountingGuard ON dbo.CaTruc AFTER INSERT,UPDATE,DELETE AS
BEGIN
 SET NOCOUNT ON;
 IF EXISTS(SELECT 1 FROM deleted WHERE TrangThai=''Locked'')
 THROW 51102,N''Ca đã khóa, không được sửa hoặc mở lại.'',1;
 IF EXISTS(SELECT 1 FROM inserted i JOIN dbo.KyKeToan p ON p.NgayDauKy=DATEFROMPARTS(YEAR(i.ThoiDiemMoCa),MONTH(i.ThoiDiemMoCa),1))
 OR EXISTS(SELECT 1 FROM deleted d JOIN dbo.KyKeToan p ON p.NgayDauKy=DATEFROMPARTS(YEAR(d.ThoiDiemMoCa),MONTH(d.ThoiDiemMoCa),1))
 THROW 51101,N''Kỳ kế toán đã khóa.'',1;
END');
EXEC(N'CREATE OR ALTER TRIGGER dbo.TR_OpeningBalances_Immutable ON dbo.SoDuDauKy AFTER INSERT,UPDATE,DELETE AS
BEGIN
 SET NOCOUNT ON;
 IF EXISTS(SELECT 1 FROM deleted) THROW 51103,N''Số dư mở sổ là chứng từ một lần; không được sửa hoặc xóa.'',1;
 IF EXISTS(SELECT 1 FROM inserted i JOIN dbo.KyKeToan p ON p.NgayDauKy=DATEFROMPARTS(YEAR(i.TinhDenThoiDiem),MONTH(i.TinhDenThoiDiem),1))
 THROW 51101,N''Kỳ kế toán đã khóa.'',1;
END');
EXEC(N'CREATE OR ALTER TRIGGER dbo.TR_AccountingPeriods_Immutable ON dbo.KyKeToan AFTER UPDATE,DELETE AS
BEGIN
 SET NOCOUNT ON;
 THROW 51104,N''Kỳ đã khóa không thể sửa hoặc mở lại.'',1;
END');
COMMIT;
-- END MIGRATION V6
GO

-- BEGIN MIGRATION V7
SET XACT_ABORT ON;
BEGIN TRANSACTION;
DECLARE @lock int;
EXEC @lock=sys.sp_getapplock @Resource=N'QLKhachSan.Write',@LockMode='Exclusive',@LockOwner='Transaction',@LockTimeout=10000;
IF @lock<0 THROW 51001,N'Không lấy được khóa nâng cấp dữ liệu.',1;
IF NOT EXISTS(SELECT 1 FROM dbo.PhienBanCSDL WHERE PhienBan=6) THROW 51000,N'Cần nâng cấp phiên bản 6 trước.',1;
IF NOT EXISTS(SELECT 1 FROM dbo.PhienBanCSDL WHERE PhienBan=7)
BEGIN
 ALTER TABLE dbo.TaiKhoanNhanVien ADD DaTuyChinhQuyen bit NOT NULL CONSTRAINT DF_Users_PermissionsCustomized DEFAULT 0;

    -- dbo.MenuTong — 9 MENU TỔNG
    --   Chức năng: điều hướng
    --   Xem: MaMenu, TieuDe, ThuTuHienThi để xem tên và thứ tự menu.
    CREATE TABLE dbo.MenuTong (
        MaMenu varchar(40) NOT NULL CONSTRAINT PK_AppMenus PRIMARY KEY,
        TieuDe nvarchar(100) NOT NULL,
        ThuTuHienThi int NOT NULL
    );

    -- dbo.MenuCon — 24 MENU PHỤ
    --   Mỗi dòng: thuộc MenuTong.MaMenu
    --   Xem: MaMenu, TieuDe, ThuTuHienThi để xem menu phụ của từng menu tổng.
    CREATE TABLE dbo.MenuCon (
        MaMenu varchar(40) NOT NULL REFERENCES dbo.MenuTong(MaMenu),
        TieuDe nvarchar(100) NOT NULL,
        ThuTuHienThi int NOT NULL,
        CONSTRAINT PK_AppSubmenus PRIMARY KEY(MaMenu,TieuDe)
    );

    -- dbo.ChucNang — 37 CHỨC NĂNG
    --   Menu tổng: qua MaMenu
    --   Menu phụ: qua TenMenuPhu
    --   Xem: MaChucNang, TieuDe, MaMenu, TenMenuPhu, VaiTroDuocPhep, ThuTuHienThi.
    CREATE TABLE dbo.ChucNang (
        MaChucNang varchar(80) NOT NULL CONSTRAINT PK_AppFunctions PRIMARY KEY,
        TieuDe nvarchar(120) NOT NULL,
        MaMenu varchar(40) NOT NULL,
        TenMenuPhu nvarchar(100) NOT NULL,
        VaiTroDuocPhep varchar(80) NOT NULL,
        ThuTuHienThi int NOT NULL,
        CONSTRAINT FK_AppFunctions_Submenu FOREIGN KEY(MaMenu,TenMenuPhu) REFERENCES dbo.MenuCon(MaMenu,TieuDe)
    );

    -- dbo.PhanQuyenNhanVien — quyền cá nhân
    --   Menu tổng: Nhân viên
    --   Menu phụ: Phân quyền
    --   Chức năng: staff.manage
    --   Xem: MaNhanVien, MaChucNang để xem nhân viên được cấp quyền nào.
    CREATE TABLE dbo.PhanQuyenNhanVien (
        MaNhanVien int NOT NULL REFERENCES dbo.TaiKhoanNhanVien(Ma),
        MaChucNang varchar(80) NOT NULL REFERENCES dbo.ChucNang(MaChucNang),
        CONSTRAINT PK_UserFunctionGrants PRIMARY KEY(MaNhanVien,MaChucNang)
    );

    -- Mỗi dòng: mã menu tổng, tên hiển thị, thứ tự; SELECT các cột này từ dbo.MenuTong.
    INSERT dbo.MenuTong(MaMenu, TieuDe, ThuTuHienThi)
    VALUES
    ('rooms', N'Quản lý phòng', 1),
    ('services', N'Dịch vụ', 2),
    ('customers', N'Khách hàng', 3),
    ('shifts', N'Ca trực', 4),
    ('cash', N'Thu chi', 5),
    ('invoices', N'Hóa đơn', 6),
    ('reports', N'Báo cáo', 7),
    ('staff', N'Nhân viên', 8),
    ('system', N'Hệ thống', 9);
    -- Mỗi dòng: menu tổng chứa nó, tên menu phụ, thứ tự; SELECT theo MaMenu để lọc.
    INSERT dbo.MenuCon(MaMenu, TieuDe, ThuTuHienThi)
    VALUES
    ('rooms', N'Phòng', 1),
    ('rooms', N'Đặt phòng', 2),
    ('rooms', N'Lưu trú', 3),
    ('rooms', N'Buồng phòng', 4),
    ('rooms', N'Danh mục phòng', 5),
    ('services', N'Yêu cầu dịch vụ', 6),
    ('services', N'Danh mục dịch vụ', 7),
    ('services', N'Kho dịch vụ', 8),
    ('customers', N'Hồ sơ', 9),
    ('customers', N'Lịch sử', 10),
    ('shifts', N'Ca trực', 11),
    ('cash', N'Thu chi', 12),
    ('cash', N'Sổ quỹ', 13),
    ('cash', N'Đối soát', 14),
    ('cash', N'Công nợ', 15),
    ('invoices', N'Hóa đơn', 16),
    ('invoices', N'Kiểm soát', 17),
    ('invoices', N'Nhóm bill', 18),
    ('reports', N'Doanh thu', 19),
    ('reports', N'Lãi lỗ', 20),
    ('staff', N'Nhân viên', 21),
    ('staff', N'Phân quyền', 22),
    ('system', N'Tài khoản', 23),
    ('system', N'Nhật ký', 24);
    -- MaChucNang = mã nút; TieuDe = tên hiển thị; MaMenu = menu tổng;
    -- TenMenuPhu = menu phụ; VaiTroDuocPhep = vai trò; ThuTuHienThi = thứ tự.
    INSERT dbo.ChucNang (MaChucNang, TieuDe, MaMenu, TenMenuPhu, VaiTroDuocPhep, ThuTuHienThi)
    VALUES
    -- Quản lý phòng / Phòng
    --   Xem: Phong.SoPhong, Loai, TrangThai và lịch LuotLuuTru đang hoạt động.
    ('room.map', N'Sơ đồ phòng', 'rooms', N'Phòng', 'Admin,Reception,Manager', 1),
    --   Xem: Phong.SoPhong, Loai, DonGiaPhong, TrangThai theo số phòng.
    ('room.search', N'Tìm phòng', 'rooms', N'Phòng', 'Admin,Reception,Manager', 2),

    -- Quản lý phòng / Đặt phòng
    --   Xem: Phong.SoPhong, TrangThai; LuotLuuTru.ThoiDiemDen, ThoiDiemDi để lọc phòng trống hiện tại.
    ('room.walkin', N'Nhận phòng trực tiếp', 'rooms', N'Đặt phòng', 'Admin,Reception', 3),
    --   Xem: Phong.SoPhong, Loai, DonGiaPhong; LuotLuuTru.ThoiDiemDen, ThoiDiemDi để lọc lịch còn chỗ.
    ('room.reserve', N'Đặt phòng trước', 'rooms', N'Đặt phòng', 'Admin,Reception', 4),
    --   Xem: LuotLuuTru.TenKhach, HanGiuPhong, TienCoc và GiaoDichThanhToan.LoaiGiaoDich, SoTien.
    ('room.deposit', N'Thu cọc bổ sung', 'rooms', N'Đặt phòng', 'Admin,Reception', 5),
    --   Xem: LuotLuuTru.MaPhong, TenKhach, TrangThai, ThoiDiemDen, HanGiuPhong; Phong.TrangThai.
    ('room.checkin', N'Nhận phòng đã đặt', 'rooms', N'Đặt phòng', 'Admin,Reception', 6),
    --   Xem: LuotLuuTru.TenKhach, SoDienThoai, MaPhong, ThoiDiemDen, ThoiDiemDi, HanGiuPhong.
    ('room.booking_edit', N'Sửa lượt đặt', 'rooms', N'Đặt phòng', 'Admin,Reception', 7),
    --   Xem: LuotLuuTru.TrangThai, HanGiuPhong, TienCoc; GiaoDichThanhToan.LoaiGiaoDich, SoTien.
    ('room.booking_cancel', N'Hủy lượt đặt', 'rooms', N'Đặt phòng', 'Admin,Reception', 8),

    -- Quản lý phòng / Lưu trú
    --   Xem: LuotLuuTru.ThoiDiemDen, ThoiDiemDi, TrangThai, TenKhach; Phong.SoPhong.
    ('room.schedule', N'Lịch đến / đi', 'rooms', N'Lưu trú', 'Admin,Reception,Manager', 9),
    --   Xem: LuotLuuTru, ChangLuuTru.DonGiaPhong, YeuCauDichVu.DonGia/SoLuong, GiaoDichThanhToan.SoTien, HoaDon.
    ('room.checkout', N'Trả phòng', 'rooms', N'Lưu trú', 'Admin,Reception', 10),
    --   Xem: LuotLuuTru.MaPhong; ChangLuuTru.MaPhong, ThoiDiemBatDau, ThoiDiemKetThuc; Phong.TrangThai.
    ('room.transfer', N'Chuyển phòng', 'rooms', N'Lưu trú', 'Admin,Reception', 11),
    --   Xem: LuotLuuTru.ThoiDiemDen, ThoiDiemDi, TrangThai và lịch cùng phòng.
    ('room.extend', N'Gia hạn lưu trú', 'rooms', N'Lưu trú', 'Admin,Reception', 12),
    --   Xem: LuotLuuTru.TenKhach, TrangThai, ThoiDiemDen, ThoiDiemDi; Phong.SoPhong.
    ('room.history', N'Lịch đặt / lịch sử', 'rooms', N'Lưu trú', 'Admin,Reception,Manager', 13),

    -- Quản lý phòng / Buồng phòng
    --   Xem: Phong.SoPhong, TrangThai để nhận biết phòng đang dọn.
    ('room.clean', N'Xong dọn phòng', 'rooms', N'Buồng phòng', 'Admin,Reception', 14),
    --   Xem: Phong.SoPhong, TrangThai và LuotLuuTru đang hoạt động.
    ('room.maintain', N'Bảo trì phòng', 'rooms', N'Buồng phòng', 'Admin,Manager', 15),

    -- Quản lý phòng / Danh mục phòng
    --   Xem: Phong.SoPhong, Loai, DonGiaPhong, TienCoc, TrangThai.
    ('room.catalog', N'Danh mục phòng', 'rooms', N'Danh mục phòng', 'Admin,Manager', 16),
    --   Xem: Phong.Loai, DonGiaPhong, TienCoc.
    ('room.pricing', N'Bảng giá', 'rooms', N'Danh mục phòng', 'Admin,Manager', 17),

    -- Dịch vụ / Yêu cầu dịch vụ
    --   Xem: DichVu.Ten, DonGia, DonViTinh và YeuCauDichVu của lượt ở.
    ('service.order', N'Gọi dịch vụ', 'services', N'Yêu cầu dịch vụ', 'Admin,Reception', 18),
    --   Xem: YeuCauDichVu.Ten, SoLuong, SoLuongDaGiao, ThoiDiemHuy.
    ('service.manage', N'Xử lý dịch vụ', 'services', N'Yêu cầu dịch vụ', 'Admin,Reception', 19),

    -- Dịch vụ / Danh mục dịch vụ
    --   Xem: DichVu.DanhMuc, Ten, DonGia, DonViTinh, DangHoatDong.
    ('service.catalog', N'Danh mục dịch vụ', 'services', N'Danh mục dịch vụ', 'Admin,Manager', 20),

    -- Dịch vụ / Kho dịch vụ
    --   Xem: HangTonKho.SoLuong, GiaVonBinhQuan, NguongDatHang; BienDongKho.LoaiGiaoDich, SoLuong.
    ('service.stock', N'Kho minibar', 'services', N'Kho dịch vụ', 'Admin,Accountant,Manager', 21),

    -- Khách hàng / Hồ sơ
    --   Xem: KhachHang.Ten, SoDienThoai, SoGiayTo, ThoiDiemTao.
    ('customer.profile', N'Hồ sơ khách hàng', 'customers', N'Hồ sơ', 'Admin,Reception,Manager', 22),

    -- Khách hàng / Lịch sử
    --   Xem: KhachHang.Ma, LuotLuuTru.TrangThai/ThoiDiemDen/ThoiDiemDi, HoaDon.ThoiDiemLapHoaDon.
    ('customer.history', N'Lịch sử khách', 'customers', N'Lịch sử', 'Admin,Reception,Manager', 23),

    -- Ca trực / Ca trực
    --   Xem: CaTruc.ThoiDiemMoCa, TienMatKiemDem, TienMatDuKien, TrangThai; KyKeToan.NgayDauKy.
    ('shift.manage', N'Ca trực / bàn giao', 'shifts', N'Ca trực', 'Admin,Reception,Accountant,Manager', 24),

    -- Thu chi / Thu chi
    --   Xem: GiaoDichThanhToan.LoaiGiaoDich, SoTien, PhuongThucThanhToan, ThoiDiemTao.
    ('cash.flow', N'Các khoản thu / hoàn', 'cash', N'Thu chi', 'Admin,Accountant,Manager', 25),

    -- Thu chi / Sổ quỹ
    --   Xem: PhieuThuChi.LoaiPhieu, KenhThanhToan, SoTien; SoDuDauKy.SoTien.
    ('cash.book', N'Sổ quỹ', 'cash', N'Sổ quỹ', 'Admin,Accountant,Manager', 26),

    -- Thu chi / Đối soát
    --   Xem: DongSaoKe.KenhThanhToan, MaThamChieu, SoTien, MaGiaoDichDaDoiSoat, MaPhieuDaDoiSoat.
    ('cash.bank', N'Đối soát ngân hàng', 'cash', N'Đối soát', 'Admin,Accountant,Manager', 27),

    -- Thu chi / Công nợ
    --   Xem: CongNo.DoiTac, SoTien, HanThanhToan; PhanBoCongNo.SoTien.
    ('cash.debt', N'Công nợ', 'cash', N'Công nợ', 'Admin,Accountant,Manager', 28),

    -- Hóa đơn / Hóa đơn
    --   Xem: HoaDon.ThoiDiemLapHoaDon, TenKhach, TienPhong, TienDichVu, TienDaThu.
    ('invoice.list', N'Hóa đơn / doanh thu', 'invoices', N'Hóa đơn', 'Admin,Accountant,Manager', 29),

    -- Hóa đơn / Kiểm soát
    --   Xem: ThongTinTaiChinhHoaDon.SoHoaDonDienTu; DieuChinhHoaDon.SoTien; HoaDonHuy.MaLyDo.
    ('invoice.control', N'Điều chỉnh hóa đơn', 'invoices', N'Kiểm soát', 'Admin,Accountant,Manager', 30),

    -- Hóa đơn / Nhóm bill
    --   Xem: NhomHoaDon.Ten; PhanChiaHoaDon.MaHoaDon, NguoiThanhToan, SoTien.
    ('invoice.groups', N'Nhóm bill', 'invoices', N'Nhóm bill', 'Admin,Accountant,Manager', 31),

    -- Báo cáo / Doanh thu
    --   Xem: HoaDon.ThoiDiemLapHoaDon, TienPhong, TienDichVu; GiaoDichThanhToan.LoaiGiaoDich='Forfeit'.
    ('report.revenue', N'Biểu đồ doanh thu', 'reports', N'Doanh thu', 'Admin,Accountant,Manager', 32),

    -- Báo cáo / Lãi lỗ
    --   Xem: HoaDon, PhieuThuChi, BienDongKho theo kỳ để tính thu/chi/giá vốn.
    ('report.profit', N'Lãi lỗ', 'reports', N'Lãi lỗ', 'Admin,Accountant,Manager', 33),

    -- Nhân viên / Nhân viên
    --   Xem: TaiKhoanNhanVien.TenDangNhap, VaiTro, DangHoatDong, KhoaDen.
    ('staff.view', N'Danh sách nhân viên', 'staff', N'Nhân viên', 'Admin,Manager', 34),

    -- Nhân viên / Phân quyền
    --   Xem: TaiKhoanNhanVien.TenDangNhap, VaiTro; PhanQuyenNhanVien.MaChucNang.
    ('staff.manage', N'Tài khoản & phân quyền', 'staff', N'Phân quyền', 'Admin', 35),

    -- Hệ thống / Tài khoản
    --   Xem: TaiKhoanNhanVien.TenDangNhap, PhienBanBaoMat; không SELECT MatKhauBam/MuoiBam.
    ('system.password', N'Đổi mật khẩu', 'system', N'Tài khoản', 'Admin,Reception,Accountant,Manager', 36),

    -- Hệ thống / Nhật ký
    --   Xem: NhatKyThaoTac.HanhDong, ChiTiet, ThoiDiemTao; TaiKhoanNhanVien.TenDangNhap.
    ('system.audit', N'Nhật ký thao tác', 'system', N'Nhật ký', 'Admin,Manager', 37);

 INSERT dbo.PhienBanCSDL VALUES(7);
END;
COMMIT;
-- END MIGRATION V7
GO

-- BEGIN MIGRATION V8
SET XACT_ABORT ON;
BEGIN TRANSACTION;
DECLARE @lock int;
EXEC @lock=sys.sp_getapplock @Resource=N'QLKhachSan.Write',@LockMode='Exclusive',@LockOwner='Transaction',@LockTimeout=10000;
IF @lock<0 THROW 51001,N'Không lấy được khóa nâng cấp dữ liệu.',1;
IF NOT EXISTS(SELECT 1 FROM dbo.PhienBanCSDL WHERE PhienBan=7)
    THROW 51000,N'Cần nâng cấp phiên bản 7 trước.',1;
IF NOT EXISTS(SELECT 1 FROM dbo.PhienBanCSDL WHERE PhienBan=8)
BEGIN
    -- Mỗi chức năng chỉ thuộc một menu tổng và một menu phụ. Khóa ghép giúp
    -- bảng menu từ chối chức năng được gán sang menu tổng khác.
    CREATE UNIQUE INDEX UX_AppFunctions_MenuRoute
        ON dbo.ChucNang(MaChucNang,MaMenu,TenMenuPhu);

    -- dbo.ChucNangQuanLyPhong — menu tổng Quản lý phòng
    --   Menu phụ: Phòng/Đặt phòng/Lưu trú/Buồng phòng/Danh mục phòng
    --   Chức năng: 17 mã room.*
    --   Xem: MaChucNang, MaMenu, TenMenuPhu, ThuTuHienThi.
    CREATE TABLE dbo.ChucNangQuanLyPhong (
        MaChucNang varchar(80) NOT NULL CONSTRAINT PK_MenuRoomFunctions PRIMARY KEY,
        MaMenu varchar(40) NOT NULL CONSTRAINT CK_MenuRoomFunctions_Menu CHECK(MaMenu='rooms'),
        TenMenuPhu nvarchar(100) NOT NULL,
        ThuTuHienThi int NOT NULL,
        CONSTRAINT FK_MenuRoomFunctions_Function FOREIGN KEY(MaChucNang,MaMenu,TenMenuPhu)
            REFERENCES dbo.ChucNang(MaChucNang,MaMenu,TenMenuPhu)
    );

    -- dbo.ChucNangDichVu — menu tổng Dịch vụ
    --   Menu phụ: Yêu cầu dịch vụ/Danh mục dịch vụ/Kho dịch vụ
    --   Chức năng: 4 mã service.*
    --   Xem: MaChucNang, MaMenu, TenMenuPhu, ThuTuHienThi.
    CREATE TABLE dbo.ChucNangDichVu (
        MaChucNang varchar(80) NOT NULL CONSTRAINT PK_MenuServiceFunctions PRIMARY KEY,
        MaMenu varchar(40) NOT NULL CONSTRAINT CK_MenuServiceFunctions_Menu CHECK(MaMenu='services'),
        TenMenuPhu nvarchar(100) NOT NULL,
        ThuTuHienThi int NOT NULL,
        CONSTRAINT FK_MenuServiceFunctions_Function FOREIGN KEY(MaChucNang,MaMenu,TenMenuPhu)
            REFERENCES dbo.ChucNang(MaChucNang,MaMenu,TenMenuPhu)
    );

    -- dbo.ChucNangKhachHang — menu tổng Khách hàng
    --   Menu phụ: Hồ sơ/Lịch sử
    --   Chức năng: customer.profile, customer.history
    --   Xem: MaChucNang, MaMenu, TenMenuPhu, ThuTuHienThi.
    CREATE TABLE dbo.ChucNangKhachHang (
        MaChucNang varchar(80) NOT NULL CONSTRAINT PK_MenuCustomerFunctions PRIMARY KEY,
        MaMenu varchar(40) NOT NULL CONSTRAINT CK_MenuCustomerFunctions_Menu CHECK(MaMenu='customers'),
        TenMenuPhu nvarchar(100) NOT NULL,
        ThuTuHienThi int NOT NULL,
        CONSTRAINT FK_MenuCustomerFunctions_Function FOREIGN KEY(MaChucNang,MaMenu,TenMenuPhu)
            REFERENCES dbo.ChucNang(MaChucNang,MaMenu,TenMenuPhu)
    );

    -- dbo.ChucNangCaTruc — menu tổng Ca trực
    --   Menu phụ: Ca trực
    --   Chức năng: shift.manage
    --   Xem: MaChucNang, MaMenu, TenMenuPhu, ThuTuHienThi.
    CREATE TABLE dbo.ChucNangCaTruc (
        MaChucNang varchar(80) NOT NULL CONSTRAINT PK_MenuShiftFunctions PRIMARY KEY,
        MaMenu varchar(40) NOT NULL CONSTRAINT CK_MenuShiftFunctions_Menu CHECK(MaMenu='shifts'),
        TenMenuPhu nvarchar(100) NOT NULL,
        ThuTuHienThi int NOT NULL,
        CONSTRAINT FK_MenuShiftFunctions_Function FOREIGN KEY(MaChucNang,MaMenu,TenMenuPhu)
            REFERENCES dbo.ChucNang(MaChucNang,MaMenu,TenMenuPhu)
    );

    -- dbo.ChucNangThuChi — menu tổng Thu chi
    --   Menu phụ: Thu chi/Sổ quỹ/Đối soát/Công nợ
    --   Chức năng: 4 mã cash.*
    --   Xem: MaChucNang, MaMenu, TenMenuPhu, ThuTuHienThi.
    CREATE TABLE dbo.ChucNangThuChi (
        MaChucNang varchar(80) NOT NULL CONSTRAINT PK_MenuCashFunctions PRIMARY KEY,
        MaMenu varchar(40) NOT NULL CONSTRAINT CK_MenuCashFunctions_Menu CHECK(MaMenu='cash'),
        TenMenuPhu nvarchar(100) NOT NULL,
        ThuTuHienThi int NOT NULL,
        CONSTRAINT FK_MenuCashFunctions_Function FOREIGN KEY(MaChucNang,MaMenu,TenMenuPhu)
            REFERENCES dbo.ChucNang(MaChucNang,MaMenu,TenMenuPhu)
    );

    -- dbo.ChucNangHoaDon — menu tổng Hóa đơn
    --   Menu phụ: Hóa đơn/Kiểm soát/Nhóm bill
    --   Chức năng: 3 mã invoice.*
    --   Xem: MaChucNang, MaMenu, TenMenuPhu, ThuTuHienThi.
    CREATE TABLE dbo.ChucNangHoaDon (
        MaChucNang varchar(80) NOT NULL CONSTRAINT PK_MenuInvoiceFunctions PRIMARY KEY,
        MaMenu varchar(40) NOT NULL CONSTRAINT CK_MenuInvoiceFunctions_Menu CHECK(MaMenu='invoices'),
        TenMenuPhu nvarchar(100) NOT NULL,
        ThuTuHienThi int NOT NULL,
        CONSTRAINT FK_MenuInvoiceFunctions_Function FOREIGN KEY(MaChucNang,MaMenu,TenMenuPhu)
            REFERENCES dbo.ChucNang(MaChucNang,MaMenu,TenMenuPhu)
    );

    -- dbo.ChucNangBaoCao — menu tổng Báo cáo
    --   Menu phụ: Doanh thu/Lãi lỗ
    --   Chức năng: report.revenue, report.profit
    --   Xem: MaChucNang, MaMenu, TenMenuPhu, ThuTuHienThi.
    CREATE TABLE dbo.ChucNangBaoCao (
        MaChucNang varchar(80) NOT NULL CONSTRAINT PK_MenuReportFunctions PRIMARY KEY,
        MaMenu varchar(40) NOT NULL CONSTRAINT CK_MenuReportFunctions_Menu CHECK(MaMenu='reports'),
        TenMenuPhu nvarchar(100) NOT NULL,
        ThuTuHienThi int NOT NULL,
        CONSTRAINT FK_MenuReportFunctions_Function FOREIGN KEY(MaChucNang,MaMenu,TenMenuPhu)
            REFERENCES dbo.ChucNang(MaChucNang,MaMenu,TenMenuPhu)
    );

    -- dbo.ChucNangNhanVien — menu tổng Nhân viên
    --   Menu phụ: Nhân viên/Phân quyền
    --   Chức năng: staff.view, staff.manage
    --   Xem: MaChucNang, MaMenu, TenMenuPhu, ThuTuHienThi.
    CREATE TABLE dbo.ChucNangNhanVien (
        MaChucNang varchar(80) NOT NULL CONSTRAINT PK_MenuStaffFunctions PRIMARY KEY,
        MaMenu varchar(40) NOT NULL CONSTRAINT CK_MenuStaffFunctions_Menu CHECK(MaMenu='staff'),
        TenMenuPhu nvarchar(100) NOT NULL,
        ThuTuHienThi int NOT NULL,
        CONSTRAINT FK_MenuStaffFunctions_Function FOREIGN KEY(MaChucNang,MaMenu,TenMenuPhu)
            REFERENCES dbo.ChucNang(MaChucNang,MaMenu,TenMenuPhu)
    );

    -- dbo.ChucNangHeThong — menu tổng Hệ thống
    --   Menu phụ: Tài khoản/Nhật ký
    --   Chức năng: system.password, system.audit
    --   Xem: MaChucNang, MaMenu, TenMenuPhu, ThuTuHienThi.
    CREATE TABLE dbo.ChucNangHeThong (
        MaChucNang varchar(80) NOT NULL CONSTRAINT PK_MenuSystemFunctions PRIMARY KEY,
        MaMenu varchar(40) NOT NULL CONSTRAINT CK_MenuSystemFunctions_Menu CHECK(MaMenu='system'),
        TenMenuPhu nvarchar(100) NOT NULL,
        ThuTuHienThi int NOT NULL,
        CONSTRAINT FK_MenuSystemFunctions_Function FOREIGN KEY(MaChucNang,MaMenu,TenMenuPhu)
            REFERENCES dbo.ChucNang(MaChucNang,MaMenu,TenMenuPhu)
    );

    INSERT dbo.ChucNangQuanLyPhong(MaChucNang,MaMenu,TenMenuPhu,ThuTuHienThi)
        SELECT MaChucNang,MaMenu,TenMenuPhu,ThuTuHienThi FROM dbo.ChucNang WHERE MaMenu='rooms';
    INSERT dbo.ChucNangDichVu(MaChucNang,MaMenu,TenMenuPhu,ThuTuHienThi)
        SELECT MaChucNang,MaMenu,TenMenuPhu,ThuTuHienThi FROM dbo.ChucNang WHERE MaMenu='services';
    INSERT dbo.ChucNangKhachHang(MaChucNang,MaMenu,TenMenuPhu,ThuTuHienThi)
        SELECT MaChucNang,MaMenu,TenMenuPhu,ThuTuHienThi FROM dbo.ChucNang WHERE MaMenu='customers';
    INSERT dbo.ChucNangCaTruc(MaChucNang,MaMenu,TenMenuPhu,ThuTuHienThi)
        SELECT MaChucNang,MaMenu,TenMenuPhu,ThuTuHienThi FROM dbo.ChucNang WHERE MaMenu='shifts';
    INSERT dbo.ChucNangThuChi(MaChucNang,MaMenu,TenMenuPhu,ThuTuHienThi)
        SELECT MaChucNang,MaMenu,TenMenuPhu,ThuTuHienThi FROM dbo.ChucNang WHERE MaMenu='cash';
    INSERT dbo.ChucNangHoaDon(MaChucNang,MaMenu,TenMenuPhu,ThuTuHienThi)
        SELECT MaChucNang,MaMenu,TenMenuPhu,ThuTuHienThi FROM dbo.ChucNang WHERE MaMenu='invoices';
    INSERT dbo.ChucNangBaoCao(MaChucNang,MaMenu,TenMenuPhu,ThuTuHienThi)
        SELECT MaChucNang,MaMenu,TenMenuPhu,ThuTuHienThi FROM dbo.ChucNang WHERE MaMenu='reports';
    INSERT dbo.ChucNangNhanVien(MaChucNang,MaMenu,TenMenuPhu,ThuTuHienThi)
        SELECT MaChucNang,MaMenu,TenMenuPhu,ThuTuHienThi FROM dbo.ChucNang WHERE MaMenu='staff';
    INSERT dbo.ChucNangHeThong(MaChucNang,MaMenu,TenMenuPhu,ThuTuHienThi)
        SELECT MaChucNang,MaMenu,TenMenuPhu,ThuTuHienThi FROM dbo.ChucNang WHERE MaMenu='system';

    IF (SELECT COUNT(*) FROM dbo.ChucNangQuanLyPhong)<>17
        OR (SELECT COUNT(*) FROM dbo.ChucNangDichVu)<>4
        OR (SELECT COUNT(*) FROM dbo.ChucNangKhachHang)<>2
        OR (SELECT COUNT(*) FROM dbo.ChucNangCaTruc)<>1
        OR (SELECT COUNT(*) FROM dbo.ChucNangThuChi)<>4
        OR (SELECT COUNT(*) FROM dbo.ChucNangHoaDon)<>3
        OR (SELECT COUNT(*) FROM dbo.ChucNangBaoCao)<>2
        OR (SELECT COUNT(*) FROM dbo.ChucNangNhanVien)<>2
        OR (SELECT COUNT(*) FROM dbo.ChucNangHeThong)<>2
        THROW 51008,N'Danh sách chức năng của 9 menu không khớp phiên bản ứng dụng.',1;

    INSERT dbo.PhienBanCSDL VALUES(8);
END;
COMMIT;
-- END MIGRATION V8
GO

-- BEGIN MIGRATION V9
-- V9 đánh dấu CSDL đã dùng 39 tên bảng tiếng Việt. Bảng của bản cũ được
-- đổi tên trong phần LEGACY TABLE RENAME ở đầu file trước khi tới đây.
SET XACT_ABORT ON;
BEGIN TRANSACTION;
DECLARE @lock int;
EXEC @lock = sys.sp_getapplock
    @Resource = N'QLKhachSan.Write', @LockMode = 'Exclusive',
    @LockOwner = 'Transaction', @LockTimeout = 10000;
IF @lock < 0 THROW 51001, N'Không lấy được khóa nâng cấp dữ liệu.', 1;
IF OBJECT_ID(N'dbo.PhienBanCSDL', N'U') IS NULL
    THROW 51000, N'Thiếu bảng phiên bản CSDL.', 1;
IF NOT EXISTS (SELECT 1 FROM dbo.PhienBanCSDL WHERE PhienBan = 8)
    THROW 51000, N'Cần nâng cấp đến V8 trước.', 1;
IF NOT EXISTS (SELECT 1 FROM dbo.PhienBanCSDL WHERE PhienBan = 9)
    INSERT dbo.PhienBanCSDL (PhienBan) VALUES (9);
COMMIT;
-- END MIGRATION V9
GO

-- BEGIN MIGRATION V10
-- Bổ sung mô tả tiếng Việt không dấu cho bảng/cột và ghi nhận tên cột mới.
SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF NOT EXISTS (SELECT 1 FROM dbo.PhienBanCSDL WHERE PhienBan=9)
    THROW 51000, N'Cần nâng cấp phiên bản 9 trước.', 1;
IF NOT EXISTS (SELECT 1 FROM dbo.PhienBanCSDL WHERE PhienBan=10)
BEGIN
    DECLARE @MoTaBang TABLE (TenBang sysname PRIMARY KEY, MoTa nvarchar(300));
    INSERT @MoTaBang (TenBang,MoTa) VALUES
        (N'PhienBanCSDL', N'he thong'),
        (N'TaiKhoanNhanVien', N'nhan vien'),
        (N'Phong', N'phong'),
        (N'KhachHang', N'khach'),
        (N'LuotLuuTru', N'luot dat/luu tru'),
        (N'ChangLuuTru', N'chang o'),
        (N'DichVu', N'danh muc dich vu'),
        (N'YeuCauDichVu', N'dich vu khach goi'),
        (N'HoaDon', N'hoa don luu tru'),
        (N'GiaoDichThanhToan', N'dong tien coc/thu/hoan'),
        (N'NhatKyThaoTac', N'nhat ky thao tac'),
        (N'KyKeToan', N'ky ke toan da khoa'),
        (N'CaTruc', N'ca thu ngan'),
        (N'PhieuThuChi', N'phieu thu/chi'),
        (N'SoDuDauKy', N'so du dau ky'),
        (N'DongSaoKe', N'sao ke ngan hang/POS'),
        (N'CongNo', N'cong no phai thu/phai tra'),
        (N'PhanBoCongNo', N'phan bo phieu vao cong no'),
        (N'HangTonKho', N'mat hang ton kho'),
        (N'BienDongKho', N'nhap/xuat/dieu chinh kho'),
        (N'TieuThuBuongPhong', N'bao tieu thu buong phong de doi chieu kho'),
        (N'ThongTinTaiChinhHoaDon', N'thong tin thue/phi/so hoa don dien tu'),
        (N'DieuChinhHoaDon', N'giam tru hoa don'),
        (N'HoaDonHuy', N'hoa don da huy'),
        (N'NhomHoaDon', N'nhom bill'),
        (N'PhanChiaHoaDon', N'phan tien chia cho nguoi tra trong nhom bill'),
        (N'MenuTong', N'9 MENU TONG'),
        (N'MenuCon', N'24 MENU PHU'),
        (N'ChucNang', N'37 CHUC NANG'),
        (N'PhanQuyenNhanVien', N'quyen ca nhan'),
        (N'ChucNangQuanLyPhong', N'menu tong Quan ly phong'),
        (N'ChucNangDichVu', N'menu tong Dich vu'),
        (N'ChucNangKhachHang', N'menu tong Khach hang'),
        (N'ChucNangCaTruc', N'menu tong Ca truc'),
        (N'ChucNangThuChi', N'menu tong Thu chi'),
        (N'ChucNangHoaDon', N'menu tong Hoa don'),
        (N'ChucNangBaoCao', N'menu tong Bao cao'),
        (N'ChucNangNhanVien', N'menu tong Nhan vien'),
        (N'ChucNangHeThong', N'menu tong He thong');
    DECLARE @MoTaCot TABLE (TenCot sysname PRIMARY KEY, MoTa nvarchar(200));
    INSERT @MoTaCot (TenCot,MoTa) VALUES
        (N'HanhDong', N'Hanh Dong'),
        (N'DangHoatDong', N'Dang Hoat Dong'),
        (N'VaiTroDuocPhep', N'Vai Tro Duoc Phep'),
        (N'SoTien', N'So Tien'),
        (N'MaNguoiPheDuyet', N'Ma Nguoi Phe Duyet'),
        (N'DaLuuTru', N'Da Luu Tru'),
        (N'ThoiDiemDen', N'Thoi Diem Den'),
        (N'TinhDenThoiDiem', N'Tinh Den Thoi Diem'),
        (N'GiaVonBinhQuan', N'Gia Von Binh Quan'),
        (N'ThoiDiemHuy', N'Thoi Diem Huy'),
        (N'ThoiDiemHuyBo', N'Thoi Diem Huy Bo'),
        (N'MaNguoiHuy', N'Ma Nguoi Huy'),
        (N'LyDoHuy', N'Ly Do Huy'),
        (N'MaThuNgan', N'Ma Thu Ngan'),
        (N'DanhMuc', N'Danh Muc'),
        (N'KenhThanhToan', N'Kenh Thanh Toan'),
        (N'ThoiDiemNhanPhong', N'Thoi Diem Nhan Phong'),
        (N'ThoiDiemTraPhong', N'Thoi Diem Tra Phong'),
        (N'ThoiDiemDongCa', N'Thoi Diem Dong Ca'),
        (N'TienDaThu', N'Tien Da Thu'),
        (N'TienMatKiemDem', N'Tien Mat Kiem Dem'),
        (N'DoiTac', N'Doi Tac'),
        (N'ThoiDiemTao', N'Thoi Diem Tao'),
        (N'ThoiDiemLap', N'Thoi Diem Lap'),
        (N'MaNguoiTao', N'Ma Nguoi Tao'),
        (N'MaKhachHang', N'Ma Khach Hang'),
        (N'MaCongNo', N'Ma Cong No'),
        (N'LoaiCongNo', N'Loai Cong No'),
        (N'ThoiDiemGiao', N'Thoi Diem Giao'),
        (N'SoLuongDaGiao', N'So Luong Da Giao'),
        (N'ThoiDiemDi', N'Thoi Diem Di'),
        (N'TienCoc', N'Tien Coc'),
        (N'ChiTiet', N'Chi Tiet'),
        (N'HanThanhToan', N'Han Thanh Toan'),
        (N'ThoiDiemPhatHanhHoaDonDienTu', N'Thoi Diem Phat Hanh Hoa Don Dien Tu'),
        (N'SoHoaDonDienTu', N'So Hoa Don Dien Tu'),
        (N'ThoiDiemKetThuc', N'Thoi Diem Ket Thuc'),
        (N'TienMatDuKien', N'Tien Mat Du Kien'),
        (N'GiaiTrinh', N'Giai Trinh'),
        (N'MaThamChieuNgoai', N'Ma Tham Chieu Ngoai'),
        (N'SoLanDangNhapSai', N'So Lan Dang Nhap Sai'),
        (N'MaChucNang', N'Ma Chuc Nang'),
        (N'MaNhomHoaDon', N'Ma Nhom Hoa Don'),
        (N'TenKhach', N'Ten Khach'),
        (N'ThoiDiemBienDong', N'Thoi Diem Bien Dong'),
        (N'HanGiuPhong', N'Han Giu Phong'),
        (N'Ma', N'Ma'),
        (N'SoGiayTo', N'So Giay To'),
        (N'ThoiDiemNhap', N'Thoi Diem Nhap'),
        (N'MaNguoiNhap', N'Ma Nguoi Nhap'),
        (N'MaHoaDon', N'Ma Hoa Don'),
        (N'ConHieuLuc', N'Con Hieu Luc'),
        (N'ThoiDiemLapHoaDon', N'Thoi Diem Lap Hoa Don'),
        (N'ThoiDiemPhatSinhCongNo', N'Thoi Diem Phat Sinh Cong No'),
        (N'MaMatHang', N'Ma Mat Hang'),
        (N'SoLanBam', N'So Lan Bam'),
        (N'LoaiGiaoDich', N'Loai Giao Dich'),
        (N'ThoiDiemKhoa', N'Thoi Diem Khoa'),
        (N'MaNguoiKhoa', N'Ma Nguoi Khoa'),
        (N'KhoaDen', N'Khoa Den'),
        (N'MaGiaoDichDaDoiSoat', N'Ma Giao Dich Da Doi Soat'),
        (N'MaPhieuDaDoiSoat', N'Ma Phieu Da Doi Soat'),
        (N'MaMenu', N'Ma Menu'),
        (N'PhuongThucThanhToan', N'Phuong Thuc Thanh Toan'),
        (N'Ten', N'Ten'),
        (N'GhiChu', N'Ghi Chu'),
        (N'SoPhong', N'So Phong'),
        (N'ThoiDiemGiaoDich', N'Thoi Diem Giao Dich'),
        (N'ThoiDiemMoCa', N'Thoi Diem Mo Ca'),
        (N'TienMatDauCa', N'Tien Mat Dau Ca'),
        (N'ThoiDiemGoi', N'Thoi Diem Goi'),
        (N'MatKhauBam', N'Mat Khau Bam'),
        (N'NguoiThanhToan', N'Nguoi Thanh Toan'),
        (N'NgayDauKy', N'Ngay Dau Ky'),
        (N'DaTuyChinhQuyen', N'Da Tuy Chinh Quyen'),
        (N'SoDienThoai', N'So Dien Thoai'),
        (N'ThoiDiemHachToan', N'Thoi Diem Hach Toan'),
        (N'DonGia', N'Don Gia'),
        (N'SoLuong', N'So Luong'),
        (N'DonGiaPhong', N'Don Gia Phong'),
        (N'LyDo', N'Ly Do'),
        (N'MaLyDo', N'Ma Ly Do'),
        (N'MaThamChieu', N'Ma Tham Chieu'),
        (N'TienDaHoan', N'Tien Da Hoan'),
        (N'NguongDatHang', N'Nguong Dat Hang'),
        (N'ThoiDiemBaoCao', N'Thoi Diem Bao Cao'),
        (N'LyDoDaoButToan', N'Ly Do Dao But Toan'),
        (N'ThoiDiemDaoButToan', N'Thoi Diem Dao But Toan'),
        (N'MaNguoiDaoButToan', N'Ma Nguoi Dao But Toan'),
        (N'VaiTro', N'Vai Tro'),
        (N'TienPhong', N'Tien Phong'),
        (N'MaPhong', N'Ma Phong'),
        (N'SoPhongHoaDon', N'So Phong Hoa Don'),
        (N'DonViLamTron', N'Don Vi Lam Tron'),
        (N'MuoiBam', N'Muoi Bam'),
        (N'PhienBanBaoMat', N'Phien Ban Bao Mat'),
        (N'TienDichVu', N'Tien Dich Vu'),
        (N'MaDichVu', N'Ma Dich Vu'),
        (N'MaYeuCauDichVu', N'Ma Yeu Cau Dich Vu'),
        (N'TyLePhiDichVu', N'Ty Le Phi Dich Vu'),
        (N'ThoiDiemThietLap', N'Thoi Diem Thiet Lap'),
        (N'MaNguoiThietLap', N'Ma Nguoi Thiet Lap'),
        (N'MaCaTruc', N'Ma Ca Truc'),
        (N'ThuTuHienThi', N'Thu Tu Hien Thi'),
        (N'ThoiDiemBatDau', N'Thoi Diem Bat Dau'),
        (N'TrangThai', N'Trang Thai'),
        (N'MaLuotLuuTru', N'Ma Luot Luu Tru'),
        (N'TenMenuPhu', N'Ten Menu Phu'),
        (N'TieuDe', N'Tieu De'),
        (N'Loai', N'Loai'),
        (N'DonViTinh', N'Don Vi Tinh'),
        (N'GiaVonDonVi', N'Gia Von Don Vi'),
        (N'MaNhanVien', N'Ma Nhan Vien'),
        (N'TenDangNhap', N'Ten Dang Nhap'),
        (N'TyLeVAT', N'Ty Le VAT'),
        (N'PhienBan', N'Phien Ban'),
        (N'ThoiDiemHuyHoaDon', N'Thoi Diem Huy Hoa Don'),
        (N'MaNguoiHuyHoaDon', N'Ma Nguoi Huy Hoa Don'),
        (N'MaPhieuThuChi', N'Ma Phieu Thu Chi'),
        (N'LoaiPhieu', N'Loai Phieu');
    DECLARE @TenBang sysname, @TenCot sysname, @MoTa nvarchar(500);
    DECLARE BangMoTa CURSOR LOCAL FAST_FORWARD FOR
        SELECT TenBang,MoTa FROM @MoTaBang ORDER BY TenBang;
    OPEN BangMoTa;
    FETCH NEXT FROM BangMoTa INTO @TenBang,@MoTa;
    WHILE @@FETCH_STATUS=0
    BEGIN
        IF EXISTS (SELECT 1 FROM sys.extended_properties
                   WHERE major_id=OBJECT_ID(N'dbo.'+@TenBang) AND minor_id=0 AND name=N'MS_Description')
            EXEC sys.sp_updateextendedproperty N'MS_Description',@MoTa,
                N'SCHEMA',N'dbo',N'TABLE',@TenBang;
        ELSE
            EXEC sys.sp_addextendedproperty N'MS_Description',@MoTa,
                N'SCHEMA',N'dbo',N'TABLE',@TenBang;
        FETCH NEXT FROM BangMoTa INTO @TenBang,@MoTa;
    END;
    CLOSE BangMoTa;
    DEALLOCATE BangMoTa;

    DECLARE CotMoTa CURSOR LOCAL FAST_FORWARD FOR
        SELECT t.name,c.name,N'Cot '+d.MoTa+N' trong bang '+b.MoTa
        FROM sys.tables AS t
        JOIN @MoTaBang AS b ON b.TenBang=t.name
        JOIN sys.columns AS c ON c.object_id=t.object_id
        JOIN @MoTaCot AS d ON d.TenCot=c.name
        WHERE t.schema_id=SCHEMA_ID(N'dbo')
        ORDER BY t.name,c.column_id;
    OPEN CotMoTa;
    FETCH NEXT FROM CotMoTa INTO @TenBang,@TenCot,@MoTa;
    WHILE @@FETCH_STATUS=0
    BEGIN
        IF EXISTS (SELECT 1 FROM sys.extended_properties AS p
                   JOIN sys.columns AS c ON c.object_id=p.major_id AND c.column_id=p.minor_id
                   WHERE p.major_id=OBJECT_ID(N'dbo.'+@TenBang)
                     AND c.name=@TenCot AND p.name=N'MS_Description')
            EXEC sys.sp_updateextendedproperty N'MS_Description',@MoTa,
                N'SCHEMA',N'dbo',N'TABLE',@TenBang,N'COLUMN',@TenCot;
        ELSE
            EXEC sys.sp_addextendedproperty N'MS_Description',@MoTa,
                N'SCHEMA',N'dbo',N'TABLE',@TenBang,N'COLUMN',@TenCot;
        FETCH NEXT FROM CotMoTa INTO @TenBang,@TenCot,@MoTa;
    END;
    CLOSE CotMoTa;
    DEALLOCATE CotMoTa;
    INSERT dbo.PhienBanCSDL (PhienBan) VALUES(10);
END;
COMMIT;
-- END MIGRATION V10
GO

-- BEGIN MIGRATION V11
BEGIN TRANSACTION;
DECLARE @lock11 int;
EXEC @lock11=sys.sp_getapplock @Resource=N'QLKhachSan.Write',@LockMode='Exclusive',@LockOwner='Transaction',@LockTimeout=10000;
IF @lock11<0 THROW 51001,N'Không lấy được khóa nâng cấp dữ liệu.',1;
IF NOT EXISTS(SELECT 1 FROM dbo.PhienBanCSDL WHERE PhienBan=10) THROW 51000,N'Cần nâng cấp phiên bản 10 trước.',1;
IF NOT EXISTS(SELECT 1 FROM dbo.PhienBanCSDL WHERE PhienBan=11)
BEGIN
    INSERT dbo.ChucNang(MaChucNang,TieuDe,MaMenu,TenMenuPhu,VaiTroDuocPhep,ThuTuHienThi)
        VALUES('invoice.create',N'Lập hóa đơn thanh toán','invoices',N'Hóa đơn','Admin,Reception',28);
    INSERT dbo.ChucNangHoaDon(MaChucNang,MaMenu,TenMenuPhu,ThuTuHienThi)
        VALUES('invoice.create','invoices',N'Hóa đơn',28);
    CREATE TABLE dbo.CauHinhMenu (
        MaMenu varchar(40) NOT NULL CONSTRAINT PK_MenuConfig PRIMARY KEY,
        TieuDe nvarchar(100) NOT NULL,
        MoTa nvarchar(180) NOT NULL CONSTRAINT DF_MenuConfig_Description DEFAULT N'',
        BieuTuong varchar(30) NOT NULL CONSTRAINT DF_MenuConfig_Icon DEFAULT 'receipt',
        MauSac varchar(7) NOT NULL CONSTRAINT DF_MenuConfig_Color DEFAULT '#534C84',
        ThuTuHienThi int NOT NULL,
        DangHienThi bit NOT NULL CONSTRAINT DF_MenuConfig_Active DEFAULT 1,
        CoSan bit NOT NULL CONSTRAINT DF_MenuConfig_Builtin DEFAULT 0,
        CONSTRAINT CK_MenuConfig_Color CHECK(MauSac LIKE '#[0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F]')
    );
    CREATE TABLE dbo.CauHinhMenuCon (
        Ma bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_SubmenuConfig PRIMARY KEY,
        MaMenu varchar(40) NOT NULL REFERENCES dbo.CauHinhMenu(MaMenu),
        TieuDe nvarchar(100) NOT NULL,
        ThuTuHienThi int NOT NULL,
        DangHienThi bit NOT NULL CONSTRAINT DF_SubmenuConfig_Active DEFAULT 1,
        CoSan bit NOT NULL CONSTRAINT DF_SubmenuConfig_Builtin DEFAULT 0,
        CONSTRAINT UQ_SubmenuConfig_Name UNIQUE(MaMenu,TieuDe)
    );
    CREATE TABLE dbo.CauHinhChucNang (
        Ma bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_FunctionConfig PRIMARY KEY,
        MaMenuCon bigint NOT NULL REFERENCES dbo.CauHinhMenuCon(Ma),
        TieuDe nvarchar(120) NOT NULL,
        MaThaoTac varchar(80) NOT NULL REFERENCES dbo.ChucNang(MaChucNang),
        ThuTuHienThi int NOT NULL,
        DangHienThi bit NOT NULL CONSTRAINT DF_FunctionConfig_Active DEFAULT 1,
        CoSan bit NOT NULL CONSTRAINT DF_FunctionConfig_Builtin DEFAULT 0
    );
    CREATE INDEX IX_SubmenuConfig_Menu ON dbo.CauHinhMenuCon(MaMenu,ThuTuHienThi);
    CREATE INDEX IX_FunctionConfig_Submenu ON dbo.CauHinhChucNang(MaMenuCon,ThuTuHienThi);
    INSERT dbo.CauHinhMenu(MaMenu,TieuDe,ThuTuHienThi,CoSan)
        SELECT MaMenu,TieuDe,ThuTuHienThi,1 FROM dbo.MenuTong;
    UPDATE dbo.CauHinhMenu SET
        MoTa=CASE MaMenu
            WHEN 'rooms' THEN N'Sơ đồ phòng, đặt và nhận phòng'
            WHEN 'services' THEN N'Yêu cầu, danh mục và kho'
            WHEN 'customers' THEN N'Hồ sơ và lịch sử lưu trú'
            WHEN 'shifts' THEN N'Theo dõi và bàn giao ca'
            WHEN 'cash' THEN N'Giao dịch, sổ quỹ và công nợ'
            WHEN 'invoices' THEN N'Danh sách và đối soát'
            WHEN 'reports' THEN N'Doanh thu và kết quả kinh doanh'
            WHEN 'staff' THEN N'Nhân sự và tài khoản'
            ELSE N'Bảo mật và nhật ký hoạt động' END,
        BieuTuong=CASE MaMenu WHEN 'rooms' THEN 'bed' WHEN 'services' THEN 'service'
            WHEN 'customers' THEN 'people' WHEN 'shifts' THEN 'clock' WHEN 'cash' THEN 'money'
            WHEN 'invoices' THEN 'receipt' WHEN 'reports' THEN 'chart' WHEN 'staff' THEN 'people'
            ELSE 'key' END,
        MauSac=CASE MaMenu WHEN 'rooms' THEN '#534C84' WHEN 'services' THEN '#B56B3E'
            WHEN 'customers' THEN '#377A68' WHEN 'shifts' THEN '#916079' WHEN 'cash' THEN '#3E699D'
            WHEN 'invoices' THEN '#B56B3E' WHEN 'reports' THEN '#534C84' WHEN 'staff' THEN '#377A68'
            ELSE '#6A6D82' END;
    INSERT dbo.CauHinhMenuCon(MaMenu,TieuDe,ThuTuHienThi,CoSan)
        SELECT MaMenu,TieuDe,ThuTuHienThi,1 FROM dbo.MenuCon;
    INSERT dbo.CauHinhChucNang(MaMenuCon,TieuDe,MaThaoTac,ThuTuHienThi,CoSan)
        SELECT sm.Ma,f.TieuDe,f.MaChucNang,f.ThuTuHienThi,1
        FROM dbo.ChucNang f JOIN dbo.CauHinhMenuCon sm ON sm.MaMenu=f.MaMenu AND sm.TieuDe=f.TenMenuPhu;
    INSERT dbo.PhienBanCSDL(PhienBan) VALUES(11);
END;
COMMIT;
-- END MIGRATION V11
GO

-- ============================================================================
-- TRA CỨU CHỈ ĐỌC SAU KHI CÀI ĐẶT: bỏ dấu "--" ở truy vấn muốn chạy.
-- Các câu dưới đây không chạy khi cài đặt. TOP (100) giới hạn số dòng trả về.
-- Menu tổng = MenuTong; menu phụ = MenuCon; nút/chức năng = ChucNang.
-- Mã nào có trong bảng ChucNang* mới được ứng dụng coi là khả dụng.
-- ============================================================================
-- SELECT MaMenu, TieuDe AS MenuTong, ThuTuHienThi FROM dbo.MenuTong ORDER BY ThuTuHienThi;
-- SELECT MaMenu, TieuDe AS MenuPhu, ThuTuHienThi FROM dbo.MenuCon ORDER BY MaMenu, ThuTuHienThi;
-- SELECT m.TieuDe AS MenuTong, f.TenMenuPhu AS MenuPhu, f.TieuDe AS ChucNang,
--        f.MaChucNang, f.VaiTroDuocPhep, f.ThuTuHienThi
-- FROM dbo.ChucNang AS f JOIN dbo.MenuTong AS m ON m.MaMenu=f.MaMenu
-- ORDER BY m.ThuTuHienThi, f.ThuTuHienThi;
-- Ví dụ xem riêng các chức năng của Quản lý phòng từ bảng menu tương ứng:
-- SELECT x.TenMenuPhu AS MenuPhu, f.TieuDe AS ChucNang, x.MaChucNang, f.VaiTroDuocPhep
-- FROM dbo.ChucNangQuanLyPhong AS x JOIN dbo.ChucNang AS f ON f.MaChucNang=x.MaChucNang
-- ORDER BY x.ThuTuHienThi;
-- Thay ChucNangQuanLyPhong bằng một trong tám bảng ChucNang* còn lại để xem menu khác.
-- Với tài khoản chưa tùy chỉnh quyền (DaTuyChinhQuyen=0), ứng dụng dùng quyền theo vai trò.
-- SELECT u.TenDangNhap, u.VaiTro, u.DaTuyChinhQuyen, g.MaChucNang
-- FROM dbo.TaiKhoanNhanVien AS u LEFT JOIN dbo.PhanQuyenNhanVien AS g ON g.MaNhanVien=u.Ma
-- WHERE u.DaLuuTru=0 ORDER BY u.TenDangNhap, g.MaChucNang;

-- MENU TỔNG Quản lý phòng | menu phụ Phòng/Đặt phòng/Lưu trú/Buồng phòng/Danh mục phòng:
-- SELECT TOP (100) Ma, SoPhong, Loai, TrangThai, DonGiaPhong, TienCoc FROM dbo.Phong ORDER BY SoPhong;
-- SELECT TOP (100) MaPhong, TenKhach, TrangThai, ThoiDiemDen, ThoiDiemDi, HanGiuPhong FROM dbo.LuotLuuTru ORDER BY Ma DESC;
-- MENU TỔNG Dịch vụ | menu phụ Yêu cầu dịch vụ/Danh mục dịch vụ/Kho dịch vụ:
-- SELECT TOP (100) DanhMuc, Ten, DonGia, DonViTinh, DangHoatDong FROM dbo.DichVu ORDER BY Ten;
-- SELECT TOP (100) Ten, SoLuong, SoLuongDaGiao, DonGia, ThoiDiemGoi FROM dbo.YeuCauDichVu ORDER BY Ma DESC;
-- SELECT TOP (100) Ten, SoLuong, GiaVonBinhQuan, NguongDatHang FROM dbo.HangTonKho ORDER BY Ten;
-- MENU TỔNG Khách hàng | menu phụ Hồ sơ/Lịch sử (dữ liệu cá nhân chỉ xem khi được phép):
-- SELECT TOP (100) Ma, Ten, SoDienThoai, SoGiayTo, ThoiDiemTao FROM dbo.KhachHang ORDER BY Ma DESC;
-- MENU TỔNG Ca trực | menu phụ Ca trực:
-- SELECT TOP (100) MaThuNgan, ThoiDiemMoCa, ThoiDiemDongCa, TienMatDauCa, TienMatKiemDem, TrangThai FROM dbo.CaTruc ORDER BY Ma DESC;
-- MENU TỔNG Thu chi | menu phụ Thu chi/Sổ quỹ/Đối soát/Công nợ:
-- SELECT TOP (100) LoaiGiaoDich, SoTien, PhuongThucThanhToan, ThoiDiemTao FROM dbo.GiaoDichThanhToan ORDER BY Ma DESC;
-- SELECT TOP (100) LoaiPhieu, KenhThanhToan, SoTien, DoiTac, ThoiDiemHachToan FROM dbo.PhieuThuChi ORDER BY Ma DESC;
-- SELECT TOP (100) KenhThanhToan, MaThamChieu, SoTien, MaGiaoDichDaDoiSoat FROM dbo.DongSaoKe ORDER BY Ma DESC;
-- SELECT TOP (100) LoaiCongNo, DoiTac, SoTien, HanThanhToan FROM dbo.CongNo ORDER BY Ma DESC;
-- MENU TỔNG Hóa đơn | menu phụ Hóa đơn/Kiểm soát/Nhóm bill:
-- SELECT TOP (100) Ma, TenKhach, ThoiDiemLapHoaDon, TienPhong, TienDichVu, TienDaThu FROM dbo.HoaDon ORDER BY Ma DESC;
-- SELECT TOP (100) MaHoaDon, DanhMuc, SoTien, LyDo FROM dbo.DieuChinhHoaDon ORDER BY Ma DESC;
-- SELECT TOP (100) Ma, Ten, ThoiDiemLap FROM dbo.NhomHoaDon ORDER BY Ma DESC;
-- MENU TỔNG Báo cáo | menu phụ Doanh thu/Lãi lỗ (ví dụ dữ liệu nguồn, chưa phải công thức báo cáo đầy đủ):
-- SELECT TOP (100) ThoiDiemLapHoaDon, TienPhong, TienDichVu FROM dbo.HoaDon ORDER BY ThoiDiemLapHoaDon DESC;
-- SELECT TOP (100) ThoiDiemBienDong, LoaiGiaoDich, SoLuong, GiaVonDonVi FROM dbo.BienDongKho ORDER BY Ma DESC;
-- MENU TỔNG Nhân viên | menu phụ Nhân viên/Phân quyền (không đọc mật khẩu băm hoặc MuoiBam):
-- SELECT TOP (100) Ma, TenDangNhap, VaiTro, DangHoatDong, KhoaDen FROM dbo.TaiKhoanNhanVien WHERE DaLuuTru=0 ORDER BY TenDangNhap;
-- MENU TỔNG Hệ thống | menu phụ Tài khoản/Nhật ký:
-- SELECT TOP (100) a.HanhDong, a.ChiTiet, a.ThoiDiemTao, u.TenDangNhap
-- FROM dbo.NhatKyThaoTac AS a JOIN dbo.TaiKhoanNhanVien AS u ON u.Ma=a.MaNhanVien ORDER BY a.Ma DESC;

-- CÁC TRIGGER V6: kiểm tra khi GHI dữ liệu; SELECT ở trên vẫn đọc được lịch sử.
-- TR_Payments_AccountingGuard: Thu chi/Đặt phòng; chặn sửa GiaoDichThanhToan trong ca hoặc kỳ đã khóa.
-- TR_Invoices_AccountingGuard: Hóa đơn; chặn sửa HoaDon trong ca hoặc kỳ đã khóa.
-- TR_Vouchers_AccountingGuard: Thu chi/Sổ quỹ; chặn sửa PhieuThuChi trong ca hoặc kỳ đã khóa.
-- TR_Stock_AccountingGuard: Dịch vụ/Kho dịch vụ; chặn sửa BienDongKho trong kỳ đã khóa.
-- TR_Debts_AccountingGuard: Thu chi/Công nợ; chặn sửa CongNo trong kỳ đã khóa.
-- TR_Orders_AccountingGuard: Dịch vụ/Yêu cầu dịch vụ; chặn sửa YeuCauDichVu trong ca hoặc kỳ đã khóa.
-- TR_Adjustments_AccountingGuard: Hóa đơn/Kiểm soát; chặn sửa DieuChinhHoaDon trong kỳ đã khóa.
-- TR_Voids_AccountingGuard: Hóa đơn/Kiểm soát; chặn sửa HoaDonHuy trong kỳ đã khóa.
-- TR_EInvoice_AccountingGuard: Hóa đơn/Kiểm soát; chặn sửa ThongTinTaiChinhHoaDon trong kỳ đã khóa.
-- TR_BillShares_AccountingGuard: Hóa đơn/Nhóm bill; chặn sửa PhanChiaHoaDon trong kỳ đã khóa.
-- TR_Shifts_AccountingGuard: Ca trực; chặn sửa ca đã khóa hoặc ca thuộc kỳ đã khóa.
-- TR_OpeningBalances_Immutable: Thu chi/Sổ quỹ; số dư mở sổ là chứng từ một lần và không ghi vào kỳ đã khóa.
-- TR_AccountingPeriods_Immutable: Ca trực; kỳ đã khóa không thể sửa hoặc mở lại.
