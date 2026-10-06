# Đối chiếu menu tổng, bảng CSDL và mã chức năng

Ứng dụng có **9 menu tổng và 9 bảng chức năng tương ứng**. [V8](../Database/Database.sql#L1549) tạo các bảng chức năng bằng tên tiếng Việt; [phần nâng cấp dữ liệu cũ](../Database/Database.sql#L20) đổi tên bảng tiếng Anh trước khi chạy các migration còn thiếu. Bảng dưới đây dùng **tên hiện tại**; mỗi dòng trong bảng menu là một chức năng thuộc menu con. Các cột `MaChucNang`, `MaMenu` và `TenMenuPhu` liên kết tới `ChucNang` bằng khóa ngoại; ràng buộc `MaMenu` ngăn đưa chức năng sang nhầm menu. Các bảng `Phong`, `LuotLuuTru`, `GiaoDichThanhToan`, `HoaDon` tiếp tục lưu dữ liệu khách sạn. Các bảng menu lưu cấu trúc và khả dụng của chức năng. [V10](../Database/Database.sql#L1718) đổi tên cột và bổ sung mô tả; xem [bảng đối chiếu tên cột](COT_DATABASE.md).

| Menu tổng | Bảng menu | Số chức năng |
| --- | --- | ---: |
| Quản lý phòng | `dbo.ChucNangQuanLyPhong` | 17 |
| Dịch vụ | `dbo.ChucNangDichVu` | 4 |
| Khách hàng | `dbo.ChucNangKhachHang` | 2 |
| Ca trực | `dbo.ChucNangCaTruc` | 1 |
| Thu chi | `dbo.ChucNangThuChi` | 4 |
| Hóa đơn | `dbo.ChucNangHoaDon` | 3 |
| Báo cáo | `dbo.ChucNangBaoCao` | 2 |
| Nhân viên | `dbo.ChucNangNhanVien` | 2 |
| Hệ thống | `dbo.ChucNangHeThong` | 2 |
| **Tổng** | **9 bảng** | **37** |

`BLL/FunctionPolicy.cs` định nghĩa tên, menu con và vai trò ở dòng chỉ ra dưới đây. `GUI/ucDashboard.Navigation.cs:187-244` nhận mã chức năng và mở màn hình; cột **Mã xử lý** chỉ đến hàm giao diện hoặc nhánh thực hiện cụ thể. Các đường dẫn trong cột này tính từ thư mục `QLKhachSan/` và số dòng ứng với mã nguồn hiện tại.

| Bảng menu | Menu con | Chức năng (mã) | Khai báo | Mã xử lý |
| --- | --- | --- | --- | --- |
| `ChucNangQuanLyPhong` | Phòng | Sơ đồ phòng (`room.map`) | `BLL/FunctionPolicy.cs:11` | `GUI/ucDashboard.Navigation.cs:246` |
| `ChucNangQuanLyPhong` | Phòng | Tìm phòng (`room.search`) | `BLL/FunctionPolicy.cs:12` | `GUI/ucDashboard.Navigation.cs:282` |
| `ChucNangQuanLyPhong` | Đặt phòng | Nhận phòng trực tiếp (`room.walkin`) | `BLL/FunctionPolicy.cs:13` | `GUI/ucDashboard.Availability.cs:20,204` |
| `ChucNangQuanLyPhong` | Đặt phòng | Đặt phòng trước (`room.reserve`) | `BLL/FunctionPolicy.cs:14` | `GUI/ucDashboard.Availability.cs:20,204` |
| `ChucNangQuanLyPhong` | Đặt phòng | Thu cọc bổ sung (`room.deposit`) | `BLL/FunctionPolicy.cs:15` | `GUI/ucDashboard.Reservations.cs:178` |
| `ChucNangQuanLyPhong` | Đặt phòng | Nhận phòng đã đặt (`room.checkin`) | `BLL/FunctionPolicy.cs:16` | `GUI/ucDashboard.Reservations.cs:181` |
| `ChucNangQuanLyPhong` | Đặt phòng | Sửa lượt đặt (`room.booking_edit`) | `BLL/FunctionPolicy.cs:17` | `GUI/ucDashboard.Reservations.cs:185` |
| `ChucNangQuanLyPhong` | Đặt phòng | Hủy lượt đặt (`room.booking_cancel`) | `BLL/FunctionPolicy.cs:18` | `GUI/ucDashboard.Reservations.cs:188` |
| `ChucNangQuanLyPhong` | Lưu trú | Lịch đến / đi (`room.schedule`) | `BLL/FunctionPolicy.cs:19` | `GUI/ucDashboard.Navigation.cs:246` |
| `ChucNangQuanLyPhong` | Lưu trú | Trả phòng (`room.checkout`) | `BLL/FunctionPolicy.cs:20` | `GUI/ucDashboard.Reports.cs:9` |
| `ChucNangQuanLyPhong` | Lưu trú | Chuyển phòng (`room.transfer`) | `BLL/FunctionPolicy.cs:21` | `GUI/ucDashboard.Actions.cs:130` |
| `ChucNangQuanLyPhong` | Lưu trú | Gia hạn lưu trú (`room.extend`) | `BLL/FunctionPolicy.cs:22` | `GUI/ucDashboard.Actions.cs:191` |
| `ChucNangQuanLyPhong` | Lưu trú | Lịch đặt / lịch sử (`room.history`) | `BLL/FunctionPolicy.cs:23` | `GUI/ucDashboard.Management.cs:147` |
| `ChucNangQuanLyPhong` | Buồng phòng | Xong dọn phòng (`room.clean`) | `BLL/FunctionPolicy.cs:24` | `GUI/ucDashboard.Actions.cs:306` |
| `ChucNangQuanLyPhong` | Buồng phòng | Bảo trì phòng (`room.maintain`) | `BLL/FunctionPolicy.cs:25` | `GUI/ucDashboard.Actions.cs:246` |
| `ChucNangQuanLyPhong` | Danh mục phòng | Danh mục phòng (`room.catalog`) | `BLL/FunctionPolicy.cs:26` | `GUI/ucDashboard.Catalog.cs:45` |
| `ChucNangQuanLyPhong` | Danh mục phòng | Bảng giá (`room.pricing`) | `BLL/FunctionPolicy.cs:27` | `GUI/ucDashboard.Catalog.cs:8` |
| `ChucNangDichVu` | Yêu cầu dịch vụ | Gọi dịch vụ (`service.order`) | `BLL/FunctionPolicy.cs:28` | `GUI/ucDashboard.Actions.cs:312` |
| `ChucNangDichVu` | Yêu cầu dịch vụ | Xử lý dịch vụ (`service.manage`) | `BLL/FunctionPolicy.cs:29` | `GUI/ucDashboard.Management.cs:66` |
| `ChucNangDichVu` | Danh mục dịch vụ | Danh mục dịch vụ (`service.catalog`) | `BLL/FunctionPolicy.cs:30` | `GUI/ucDashboard.Catalog.cs:163` |
| `ChucNangDichVu` | Kho dịch vụ | Kho minibar (`service.stock`) | `BLL/FunctionPolicy.cs:31` | `GUI/AccountingForm.cs:214` |
| `ChucNangKhachHang` | Hồ sơ | Hồ sơ khách hàng (`customer.profile`) | `BLL/FunctionPolicy.cs:32` | `GUI/ucDashboard.Reports.cs:76` |
| `ChucNangKhachHang` | Lịch sử | Lịch sử khách (`customer.history`) | `BLL/FunctionPolicy.cs:33` | `GUI/ucDashboard.Management.cs:147` |
| `ChucNangCaTruc` | Ca trực | Ca trực / bàn giao (`shift.manage`) | `BLL/FunctionPolicy.cs:34` | `GUI/AccountingForm.cs:127` |
| `ChucNangThuChi` | Thu chi | Các khoản thu / hoàn (`cash.flow`) | `BLL/FunctionPolicy.cs:35` | `GUI/ucDashboard.RoleFunctions.cs:22` |
| `ChucNangThuChi` | Sổ quỹ | Sổ quỹ (`cash.book`) | `BLL/FunctionPolicy.cs:36` | `GUI/AccountingForm.cs:159` |
| `ChucNangThuChi` | Đối soát | Đối soát ngân hàng (`cash.bank`) | `BLL/FunctionPolicy.cs:37` | `GUI/AccountingForm.cs:193` |
| `ChucNangThuChi` | Công nợ | Công nợ (`cash.debt`) | `BLL/FunctionPolicy.cs:38` | `GUI/AccountingForm.cs:177` |
| `ChucNangHoaDon` | Hóa đơn | Hóa đơn / doanh thu (`invoice.list`) | `BLL/FunctionPolicy.cs:39` | `GUI/ucDashboard.Reports.cs:148` |
| `ChucNangHoaDon` | Kiểm soát | Điều chỉnh hóa đơn (`invoice.control`) | `BLL/FunctionPolicy.cs:40` | `GUI/AccountingForm.cs:235` |
| `ChucNangHoaDon` | Nhóm bill | Nhóm bill (`invoice.groups`) | `BLL/FunctionPolicy.cs:41` | `GUI/AccountingForm.cs:261` |
| `ChucNangBaoCao` | Doanh thu | Biểu đồ doanh thu (`report.revenue`) | `BLL/FunctionPolicy.cs:42` | `GUI/ucDashboard.Navigation.cs:246` |
| `ChucNangBaoCao` | Lãi lỗ | Lãi lỗ (`report.profit`) | `BLL/FunctionPolicy.cs:43` | `GUI/AccountingForm.cs:257` |
| `ChucNangNhanVien` | Nhân viên | Danh sách nhân viên (`staff.view`) | `BLL/FunctionPolicy.cs:44` | `GUI/ucDashboard.RoleFunctions.cs:7` |
| `ChucNangNhanVien` | Phân quyền | Tài khoản & phân quyền (`staff.manage`) | `BLL/FunctionPolicy.cs:45` | `GUI/ucDashboard.Reports.cs:271,309` |
| `ChucNangHeThong` | Tài khoản | Đổi mật khẩu (`system.password`) | `BLL/FunctionPolicy.cs:46` | `GUI/ucDashboard.Reports.cs:271,280` |
| `ChucNangHeThong` | Nhật ký | Nhật ký thao tác (`system.audit`) | `BLL/FunctionPolicy.cs:47` | `GUI/ucDashboard.Management.cs:199` |

Luồng quyền: [đọc chín bảng khi đăng nhập](../QLKhachSan/DAL/HotelTransaction.Permissions.cs) → giao với quyền đã cấp cho tài khoản → [kiểm tra quyền chức năng](../QLKhachSan/BLL/FunctionPolicy.cs) → [ẩn các menu và nút không được phép](../QLKhachSan/GUI/ucDashboard.Navigation.cs). Khi Admin cấp quyền, mã chức năng phải có trong bảng menu tương ứng; lần đăng nhập sau sẽ nhận quyền mới.

## Tên bảng sau V9

Các lệnh `CREATE TABLE` trong file SQL hiện dùng tên tiếng Việt. Với CSDL V1–V8 đã cài trước đây, [đoạn chuyển đổi bảng](../Database/Database.sql#L20) đổi tên bảng tại chỗ. [Đoạn chuyển đổi cột](../Database/Database.sql#L169) xử lý tên cột cũ khi nâng cấp V1–V9 lên V10, giữ dữ liệu và các quan hệ. V9 ghi nhận cấu trúc bảng; V10 đổi tên cột và thêm mô tả tiếng Việt.

| Tên cũ | Tên bảng hiện tại |
| --- | --- |
| `SchemaVersion` | `PhienBanCSDL` |
| `Users` | `TaiKhoanNhanVien` |
| `Rooms` | `Phong` |
| `Customers` | `KhachHang` |
| `Stays` | `LuotLuuTru` |
| `StaySegments` | `ChangLuuTru` |
| `Services` | `DichVu` |
| `ServiceOrders` | `YeuCauDichVu` |
| `Invoices` | `HoaDon` |
| `Payments` | `GiaoDichThanhToan` |
| `AuditLog` | `NhatKyThaoTac` |
| `AccountingPeriods` | `KyKeToan` |
| `CashShifts` | `CaTruc` |
| `FinanceVouchers` | `PhieuThuChi` |
| `FinanceOpeningBalances` | `SoDuDauKy` |
| `BankStatementLines` | `DongSaoKe` |
| `FinanceDebts` | `CongNo` |
| `DebtAllocations` | `PhanBoCongNo` |
| `StockItems` | `HangTonKho` |
| `StockMovements` | `BienDongKho` |
| `HousekeepingConsumption` | `TieuThuBuongPhong` |
| `InvoiceFinance` | `ThongTinTaiChinhHoaDon` |
| `InvoiceAdjustments` | `DieuChinhHoaDon` |
| `InvoiceVoids` | `HoaDonHuy` |
| `BillGroups` | `NhomHoaDon` |
| `BillShares` | `PhanChiaHoaDon` |
| `AppMenus` | `MenuTong` |
| `AppSubmenus` | `MenuCon` |
| `AppFunctions` | `ChucNang` |
| `UserFunctionGrants` | `PhanQuyenNhanVien` |
| `MenuRoomFunctions` | `ChucNangQuanLyPhong` |
| `MenuServiceFunctions` | `ChucNangDichVu` |
| `MenuCustomerFunctions` | `ChucNangKhachHang` |
| `MenuShiftFunctions` | `ChucNangCaTruc` |
| `MenuCashFunctions` | `ChucNangThuChi` |
| `MenuInvoiceFunctions` | `ChucNangHoaDon` |
| `MenuReportFunctions` | `ChucNangBaoCao` |
| `MenuStaffFunctions` | `ChucNangNhanVien` |
| `MenuSystemFunctions` | `ChucNangHeThong` |
