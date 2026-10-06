# Tên cột CSDL sau nâng cấp V10

V10 đổi tên 120 tên cột khác nhau sang tiếng Việt không dấu trong 39 bảng và thêm mô tả `MS_Description` bằng tiếng Việt không dấu cho từng bảng, từng cột. Dữ liệu, khóa chính, khóa ngoại và lịch sử nghiệp vụ được giữ lại. Các tên thuộc mã C# (DTO), giá trị trạng thái/nghiệp vụ như `Deposit`, `Refund`, và từ khóa SQL Server không phải tên cột.

Ví dụ bảng `dbo.BienDongKho` trong SSMS có các cột `Ma`, `MaMatHang`, `ThoiDiemBienDong`, `LoaiGiaoDich`, `SoLuong`, `GiaVonDonVi`, `MaYeuCauDichVu`, `LyDo`, `MaNguoiTao`.

```sql
SELECT Ma, MaMatHang, ThoiDiemBienDong, LoaiGiaoDich, SoLuong, GiaVonDonVi, MaYeuCauDichVu, LyDo, MaNguoiTao
FROM dbo.BienDongKho;
```

| Tên cột cũ | Tên cột hiện tại |
| --- | --- |
| `Action` | `HanhDong` |
| `Active` | `DangHoatDong` |
| `AllowedRoles` | `VaiTroDuocPhep` |
| `Amount` | `SoTien` |
| `ApprovedBy` | `MaNguoiPheDuyet` |
| `Archived` | `DaLuuTru` |
| `Arrival` | `ThoiDiemDen` |
| `AsOf` | `TinhDenThoiDiem` |
| `AverageCost` | `GiaVonBinhQuan` |
| `Cancelled` | `ThoiDiemHuy` |
| `CancelledAt` | `ThoiDiemHuyBo` |
| `CancelledBy` | `MaNguoiHuy` |
| `CancelReason` | `LyDoHuy` |
| `CashierId` | `MaThuNgan` |
| `Category` | `DanhMuc` |
| `Channel` | `KenhThanhToan` |
| `CheckIn` | `ThoiDiemNhanPhong` |
| `CheckOut` | `ThoiDiemTraPhong` |
| `ClosedAt` | `ThoiDiemDongCa` |
| `Collected` | `TienDaThu` |
| `CountedCash` | `TienMatKiemDem` |
| `Counterparty` | `DoiTac` |
| `Created` | `ThoiDiemTao` |
| `CreatedAt` | `ThoiDiemLap` |
| `CreatedBy` | `MaNguoiTao` |
| `CustomerId` | `MaKhachHang` |
| `DebtId` | `MaCongNo` |
| `DebtType` | `LoaiCongNo` |
| `Delivered` | `ThoiDiemGiao` |
| `DeliveredQuantity` | `SoLuongDaGiao` |
| `Departure` | `ThoiDiemDi` |
| `Deposit` | `TienCoc` |
| `Detail` | `ChiTiet` |
| `DueAt` | `HanThanhToan` |
| `EInvoiceIssuedAt` | `ThoiDiemPhatHanhHoaDonDienTu` |
| `EInvoiceNumber` | `SoHoaDonDienTu` |
| `Ended` | `ThoiDiemKetThuc` |
| `ExpectedCash` | `TienMatDuKien` |
| `Explanation` | `GiaiTrinh` |
| `ExternalReference` | `MaThamChieuNgoai` |
| `FailedAttempts` | `SoLanDangNhapSai` |
| `FunctionCode` | `MaChucNang` |
| `GroupId` | `MaNhomHoaDon` |
| `GuestName` | `TenKhach` |
| `HappenedAt` | `ThoiDiemBienDong` |
| `HoldUntil` | `HanGiuPhong` |
| `Id` | `Ma` |
| `IdentityNumber` | `SoGiayTo` |
| `ImportedAt` | `ThoiDiemNhap` |
| `ImportedBy` | `MaNguoiNhap` |
| `InvoiceId` | `MaHoaDon` |
| `IsActive` | `ConHieuLuc` |
| `Issued` | `ThoiDiemLapHoaDon` |
| `IssuedAt` | `ThoiDiemPhatSinhCongNo` |
| `ItemId` | `MaMatHang` |
| `Iterations` | `SoLanBam` |
| `Kind` | `LoaiGiaoDich` |
| `LockedAt` | `ThoiDiemKhoa` |
| `LockedBy` | `MaNguoiKhoa` |
| `LockedUntil` | `KhoaDen` |
| `MatchedPaymentId` | `MaGiaoDichDaDoiSoat` |
| `MatchedVoucherId` | `MaPhieuDaDoiSoat` |
| `MenuCode` | `MaMenu` |
| `Method` | `PhuongThucThanhToan` |
| `Name` | `Ten` |
| `Note` | `GhiChu` |
| `Number` | `SoPhong` |
| `OccurredAt` | `ThoiDiemGiaoDich` |
| `OpenedAt` | `ThoiDiemMoCa` |
| `OpeningCash` | `TienMatDauCa` |
| `Ordered` | `ThoiDiemGoi` |
| `PasswordHash` | `MatKhauBam` |
| `Payer` | `NguoiThanhToan` |
| `PeriodStart` | `NgayDauKy` |
| `PermissionsCustomized` | `DaTuyChinhQuyen` |
| `Phone` | `SoDienThoai` |
| `PostedAt` | `ThoiDiemHachToan` |
| `Price` | `DonGia` |
| `Quantity` | `SoLuong` |
| `Rate` | `DonGiaPhong` |
| `Reason` | `LyDo` |
| `ReasonCode` | `MaLyDo` |
| `Reference` | `MaThamChieu` |
| `Refunded` | `TienDaHoan` |
| `ReorderLevel` | `NguongDatHang` |
| `ReportedAt` | `ThoiDiemBaoCao` |
| `ReversalReason` | `LyDoDaoButToan` |
| `ReversedAt` | `ThoiDiemDaoButToan` |
| `ReversedBy` | `MaNguoiDaoButToan` |
| `Role` | `VaiTro` |
| `RoomCharge` | `TienPhong` |
| `RoomId` | `MaPhong` |
| `RoomNumber` | `SoPhongHoaDon` |
| `RoundingUnit` | `DonViLamTron` |
| `Salt` | `MuoiBam` |
| `SecurityVersion` | `PhienBanBaoMat` |
| `ServiceCharge` | `TienDichVu` |
| `ServiceId` | `MaDichVu` |
| `ServiceOrderId` | `MaYeuCauDichVu` |
| `ServiceRate` | `TyLePhiDichVu` |
| `SetAt` | `ThoiDiemThietLap` |
| `SetBy` | `MaNguoiThietLap` |
| `ShiftId` | `MaCaTruc` |
| `SortOrder` | `ThuTuHienThi` |
| `Started` | `ThoiDiemBatDau` |
| `Status` | `TrangThai` |
| `StayId` | `MaLuotLuuTru` |
| `SubmenuTitle` | `TenMenuPhu` |
| `Title` | `TieuDe` |
| `Type` | `Loai` |
| `Unit` | `DonViTinh` |
| `UnitCost` | `GiaVonDonVi` |
| `UserId` | `MaNhanVien` |
| `Username` | `TenDangNhap` |
| `VatRate` | `TyLeVAT` |
| `Version` | `PhienBan` |
| `VoidedAt` | `ThoiDiemHuyHoaDon` |
| `VoidedBy` | `MaNguoiHuyHoaDon` |
| `VoucherId` | `MaPhieuThuChi` |
| `VoucherType` | `LoaiPhieu` |

Để xem mô tả ngay trong SQL Server:

```sql
SELECT t.name AS TenBang, c.name AS TenCot, CAST(p.value AS nvarchar(4000)) AS MoTa
FROM sys.tables AS t
JOIN sys.columns AS c ON c.object_id = t.object_id
LEFT JOIN sys.extended_properties AS p
    ON p.major_id = t.object_id AND p.minor_id = c.column_id AND p.name = N'MS_Description'
WHERE t.schema_id = SCHEMA_ID(N'dbo')
ORDER BY t.name, c.column_id;
```

Các nhãn `Columns`, `Keys`, `Constraints`, `Data Type` trong SSMS là giao diện của công cụ, không phải tên đối tượng trong CSDL. `dbo` là schema mặc định của SQL Server; `dbo.Phong` nghĩa là bảng `Phong` thuộc schema `dbo`.
