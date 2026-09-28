# Thông tin toàn bộ ứng dụng QLKhachSan

[← Về README chính](../README.md)

Tài liệu này giải thích chương trình như một khách sạn có **quầy tiếp tân, người kiểm tra quy tắc và kho sổ sách**. Bạn có thể đọc từ trên xuống để hiểu luồng, hoặc tìm tên nút trong các bảng. Mỗi địa chỉ dạng `GUI/ucDashboard.cs:301` nghĩa là **tệp ở dòng 301**, tính từ thư mục `QLKhachSan/`. Ví dụ, đường dẫn đầy đủ của địa chỉ đó là [`QLKhachSan/GUI/ucDashboard.cs`](../QLKhachSan/GUI/ucDashboard.cs). Số dòng khớp với phiên bản mã khi viết tài liệu; nếu thêm/xóa code, hãy tìm tên hàm được ghi cạnh số dòng.

## Mục lục

1. [Chương trình hoạt động ra sao?](#1-chương-trình-hoạt-động-ra-sao)
2. [Đăng nhập và quyền sử dụng](#2-đăng-nhập-và-quyền-sử-dụng)
3. [Dashboard và các nút điều hướng](#3-dashboard-và-các-nút-điều-hướng)
4. [Đặt phòng, nhận phòng, trả phòng](#4-đặt-phòng-nhận-phòng-trả-phòng)
5. [Dịch vụ, phòng và danh mục](#5-dịch-vụ-phòng-và-danh-mục)
6. [Khách hàng, báo cáo, tài khoản](#6-khách-hàng-báo-cáo-tài-khoản)
7. [Tất cả bảng database](#7-tất-cả-bảng-database)
8. [Quy tắc tính tiền và giữ chỗ](#8-quy-tắc-tính-tiền-và-giữ-chỗ)
9. [Bảo vệ dữ liệu, nâng cấp và xuất file](#9-bảo-vệ-dữ-liệu-nâng-cấp-và-xuất-file)
10. [Luồng dữ liệu chi tiết: nhận thông tin từ hàm nào?](#10-luồng-dữ-liệu-chi-tiết-nhận-thông-tin-từ-hàm-nào)
11. [Tra từng hàm đọc SQL và bảng nguồn](#11-tra-từng-hàm-đọc-sql-và-bảng-nguồn)
12. [Đọc code và xử lý lỗi bằng ví dụ](#12-đọc-code-và-xử-lý-lỗi-bằng-ví-dụ)

## 1. Chương trình hoạt động ra sao?

Hãy tưởng tượng **nút bấm là chuông gọi việc**. Mã `GUI` nghe chuông và lấy dữ liệu bạn nhập. Mã `BLL` là người kiểm tra nội quy: có quyền không, phòng có trống không, tiền tính đúng không. Mã `DAL` cầm bút đọc/ghi vào các bảng SQL Server. `DTO` là các mẫu phiếu dùng để chuyển thông tin giữa ba nơi ấy.

```text
Bạn nhấn nút → GUI nhận sự kiện → BLL kiểm tra → DAL chạy SQL → database lưu
                                            ↓
                        GUI tải dữ liệu mới và vẽ lại dashboard
```

Ví dụ khi nhấn **Đặt trước / Giữ chỗ**: sự kiện ở `GUI/ucDashboard.cs:309` mở form `GUI/ucDashboard.Actions.cs:56`; nút **Lưu đặt phòng** ở dòng 95 gọi `BLL/HotelService.cs:48`; sau khi kiểm tra, `DAL/HotelTransaction.Commands.cs:7-10` cập nhật `Customers` và thêm `Stays`. Nếu nhận cọc, lệnh ở `DAL/HotelTransaction.Commands.cs:27` thêm `Payments`. Cuối cùng `GUI/ucDashboard.cs:120-123` tải lại màn hình.

| Phần | Vai trò | Tệp bắt đầu đọc |
| --- | --- | --- |
| Khởi động | Mở login, rồi dashboard; đăng xuất quay về login | [`Program.cs`](../QLKhachSan/Program.cs) |
| GUI | Form, tab, nút, bảng, màu, ô nhập | [`GUI/ucDashboard.cs`](../QLKhachSan/GUI/ucDashboard.cs), [`GUI/FormLogin.cs`](../QLKhachSan/GUI/FormLogin.cs) |
| BLL | Quyền, kiểm tra nghiệp vụ, tính tiền | [`BLL/HotelService.cs`](../QLKhachSan/BLL/HotelService.cs), [`BLL/HotelService.Management.cs`](../QLKhachSan/BLL/HotelService.Management.cs), [`BLL/AuthService.cs`](../QLKhachSan/BLL/AuthService.cs) |
| DAL | Câu SQL, giao dịch, khóa, đọc/ghi | [`DAL/HotelRepository.cs`](../QLKhachSan/DAL/HotelRepository.cs), [`DAL/HotelTransaction.Commands.cs`](../QLKhachSan/DAL/HotelTransaction.Commands.cs), [`DAL/HotelTransaction.Management.cs`](../QLKhachSan/DAL/HotelTransaction.Management.cs) |
| DTO | Hình dạng các bản ghi như Room, Stay, Invoice | [`DTO/HotelModels.cs`](../QLKhachSan/DTO/HotelModels.cs) |
| Database | Tạo bảng và nâng cấp cấu trúc | [`Database/Setup.sql`](../Database/Setup.sql), [`DAL/SchemaMigrator.cs`](../QLKhachSan/DAL/SchemaMigrator.cs) |

**Cách đọc các bảng bên dưới:** “Đọc” giống mở sổ xem, `SELECT` trong SQL. “Ghi” giống thêm/sửa phiếu, `INSERT`/`UPDATE`. Tên bảng không có tiền tố đều thuộc `dbo`. Với các nút ghi dữ liệu, chương trình thường còn ghi `AuditLog` để biết ai đã làm; xem `DAL/HotelRepository.cs:92`.

## 2. Đăng nhập và quyền sử dụng

| Điều bạn làm | Mã nhận nút → xử lý | Database và giải thích |
| --- | --- | --- |
| Mở ứng dụng | `Program.cs` → `GUI/FormLogin.cs:90` → `DAL/SchemaMigrator.cs:7` | Kiểm tra `SchemaVersion`, tự chạy migration V2–V5 nếu cần. Kết nối lấy từ `DAL/DatabaseHelper.cs:19` và `appsettings.json`. |
| Lần đầu tạo Admin | `GUI/FormLogin.cs:108-118` → `BLL/AuthService.cs:18-32` | Xem `Users` có tài khoản đang dùng chưa, rồi thêm Admin vào `Users`. |
| **Đăng nhập** | `GUI/FormLogin.cs:108` → `BLL/AuthService.cs:33` → `DAL/HotelRepository.cs:83,88` | Tìm `Users` theo tên, kiểm tra mật khẩu băm, trạng thái hoạt động và khóa tạm. Đăng nhập sai cập nhật số lần sai trong `Users`. |
| Chọn tài khoản đã nhớ | `GUI/FormLogin.cs:78` → `GUI/RememberedLogin.cs:11` | Đọc tệp trên **máy Windows hiện tại**, không đọc database. |
| **Ghi nhớ mật khẩu** | `GUI/FormLogin.cs:108` → `GUI/RememberedLogin.cs:30,47` | Lưu cục bộ bằng cơ chế bảo vệ dữ liệu của tài khoản Windows; không tạo cột mật khẩu rõ trong SQL. Bỏ ghi nhớ dùng `RememberedLogin.cs:38`. |
| **Hiện mật khẩu** | `GUI/FormLogin.cs:107` | Chỉ đổi cách hiển thị ô nhập; không sửa dữ liệu. |
| **Đóng** cửa sổ login | `GUI/FormLogin.cs:106` | Đóng giao diện; không ghi SQL. |
| **Đăng xuất** | `GUI/ucDashboard.cs:302` → `Program.cs` | Dừng đồng hồ dashboard rồi quay về login; không xóa dữ liệu. |

Mật khẩu được băm bằng PBKDF2 SHA-512 với muối ngẫu nhiên (`BLL/AuthService.cs:9-16`), nên database lưu `PasswordHash` và `Salt`, không lưu mật khẩu gốc. Sau nhiều lần sai, tài khoản bị khóa tạm 5 phút (`DAL/HotelRepository.cs:88-90`). Mỗi phiên mang `SecurityVersion`; đổi mật khẩu, đổi quyền hoặc khóa tài khoản sẽ làm phiên cũ hết hiệu lực (`DAL/HotelRepository.cs:77-80,91`; `DAL/HotelTransaction.Management.cs:50`).

| Vai trò | Được làm gì | Nơi kiểm tra |
| --- | --- | --- |
| Admin | Vận hành, xem tài chính, quản lý danh mục và tài khoản | `BLL/RolePolicy.cs:6-11`; `BLL/HotelService.cs:9-13` |
| Reception | Đặt/nhận/trả phòng, cọc, dịch vụ, tra khách | `BLL/RolePolicy.cs:6,9` |
| Accountant | Xem hóa đơn, doanh thu, thu/hoàn; không ghi nghiệp vụ | `BLL/RolePolicy.cs:7` |
| Manager | Xem vận hành và tài chính, quản lý danh mục/bảo trì; không ghi đặt/thu/trả | `BLL/RolePolicy.cs:7,9-11` |

Menu và các tab được ẩn/hiện theo quyền tại `GUI/ucDashboard.cs:57-86`; tên và thẻ thống kê riêng theo vai trò ở `GUI/ucDashboard.Roles.cs:7-69`. BLL **kiểm tra quyền lần nữa**, nên việc hiện nút không tự tạo quyền ghi.

## 3. Dashboard và các nút điều hướng

Dashboard tải `Rooms`, các `Stays` đang hoạt động, `ServiceOrders` chờ xử lý, `Services` đang bán và doanh thu từ `Invoices`/`Payments`: `GUI/ucDashboard.cs:110-118` → `BLL/HotelService.cs:14-20` → `DAL/HotelRepository.cs:94,98,102,108,110`. Lịch nhận/trả đọc `Stays`, `Rooms`, `Invoices` qua `BLL/HotelService.Management.cs:25` và `DAL/HotelTransaction.Management.cs:20-36`. Đồng hồ dùng giờ SQL, sau đó cộng thời gian đã trôi (`GUI/ucDashboard.cs:25-26,38`).

| Vùng hoặc nút | Điều dễ hiểu | Mã GUI và nơi đọc/ghi |
| --- | --- | --- |
| **Làm mới** | Tải sổ mới nhất, xử lý đặt phòng quá hạn | `GUI/ucDashboard.cs:301` → `BLL/HotelService.cs:99` và `DashboardAsync:14`; có thể cập nhật `Stays`, `Payments`, `AuditLog` nếu có quá hạn. |
| 5 thẻ số liệu | Đếm phòng đang ở, phòng trống, lượt đặt, khách sắp trả, doanh thu; Accountant thấy chỉ số tài chính | `GUI/ucDashboard.cs:132-149`; nguồn ở đoạn tải dashboard bên trên. Thẻ 1 chuyển tab phòng `:307`, thẻ 2 mở nhận phòng `:308`, thẻ 3 mở đặt trước `:309`, thẻ 4 mở checkout `:311`; thẻ 5 chỉ hiển thị. |
| **Tìm phòng** | Tìm đúng số phòng rồi mở hành động tương ứng trạng thái | `GUI/ucDashboard.cs:312-315` → `GUI/ucDashboard.Actions.cs:39`; tra trong dữ liệu `Rooms` đã tải. |
| Ô phòng trên sơ đồ | Dấu `*` báo phòng có lịch đặt; màu báo trạng thái vật lý | Vẽ tại `GUI/ucDashboard.cs:158-174`, bấm tại `:164` → `GUI/ucDashboard.Actions.cs:39`. Dữ liệu từ `Rooms`, `Stays`. |
| Bảng đặt phòng: **Nhận / Sửa / Hủy** | Làm việc với đúng lượt đặt trong dòng đó | Dựng bảng `GUI/ucDashboard.cs:198`; bắt nút `:317-324`; BLL chi tiết ở mục 4. |
| Lịch **Chờ nhận / Đã nhận / Chờ trả / Đã trả** | Chia lịch hôm nay thành bốn nhóm và bấm để nhận/trả | Tạo tab `GUI/ucDashboard.cs:180-196`, đổ dữ liệu `:213-249`, bấm `:236-249`. SQL: `DAL/HotelTransaction.Management.cs:20-36`. |
| Danh sách phòng đang dọn | Bấm phòng để đánh dấu đã dọn | `GUI/ucDashboard.cs:250-259` → `GUI/ucDashboard.Actions.cs:39`; xem lưu ý về quyền ở mục 5. |
| Yêu cầu dịch vụ bên phải | Xác nhận giao tất cả hoặc hủy phần chưa giao | `GUI/ucDashboard.cs:261-287` → `BLL/HotelService.cs:151` / `BLL/HotelService.Management.cs:91`. |
| Biểu đồ doanh thu | Xem số liệu 7 hoặc 30 ngày, mở báo cáo chi tiết | `GUI/ucDashboard.cs:290`, `GUI/RevenueOverview.cs:26,28`; đọc `Invoices`, `ServiceOrders`, `Payments` qua `DAL/HotelRepository.cs:110`. |

Dashboard tự tải lại mỗi 60 giây khi đang hiển thị và không bận (`GUI/ucDashboard.cs:13-14,39-49`). Đây là **bộ hẹn giờ trong ứng dụng**: đóng ứng dụng thì không có công việc chạy ngầm. Nút nào ghi xong sẽ gọi `GUI/ucDashboard.cs:120-129` để vẽ lại dữ liệu.

## 4. Đặt phòng, nhận phòng, trả phòng

| Nút / chức năng | Nó làm gì, nói đơn giản | GUI → BLL | SQL: đọc → ghi |
| --- | --- | --- | --- |
| **+ Nhận phòng ngay**, thẻ phòng trống | Tạo khách và lượt ở ngay, phòng đổi sang đang ở | `GUI/ucDashboard.cs:308,310` → `GUI/ucDashboard.Actions.cs:56,95` → `BLL/HotelService.cs:48` | Đọc `Rooms`, `Stays` để kiểm tra; ghi `Customers`, `Stays` (`DAL/HotelTransaction.Commands.cs:7-10`), `StaySegments` (`:18`), `Rooms` (`:13-16`), `AuditLog`. |
| **Đặt trước / Giữ chỗ** | Giữ một khoảng ngày và tùy chọn nhận cọc | `GUI/ucDashboard.cs:309` → `GUI/ucDashboard.Actions.cs:56,95` → `BLL/HotelService.cs:48` | Đọc `Rooms`, `Stays`; ghi `Customers`, `Stays` (`DAL/HotelTransaction.Commands.cs:7-10`); nếu có cọc ghi `Payments` (`:27-28`); ghi `AuditLog`. |
| **Nhận phòng** từ bảng/lịch | Đổi lượt Reserved sang Occupied khi phòng thực sự trống | `GUI/ucDashboard.cs:317-324` hoặc `:236-249` → `BLL/HotelService.cs:72` | Đọc `Stays`, `Rooms` và lịch trùng; sửa `Stays` (`DAL/HotelTransaction.Commands.cs:20`), thêm `StaySegments` (`:18`), sửa `Rooms` (`:13`). |
| **Sửa lịch / khách** | Đổi phòng, tên/SĐT/CCCD, ngày đến/trả và hạn nhận; phải ghi lý do | `GUI/ucDashboard.Management.cs:43,53` → `BLL/HotelService.Management.cs:27` | Đọc `Stays`, `Rooms` và lịch trùng; sửa/thêm `Customers` và sửa `Stays` (`DAL/HotelTransaction.Management.cs:38-41`); ghi `AuditLog`. Cọc đã thu được giữ nguyên. |
| **Hủy đặt phòng** | Hủy trước hạn thì hoàn cọc; quá hạn thì ghi cọc không hoàn | `GUI/ucDashboard.Actions.cs:102,109` → `BLL/HotelService.cs:85` | Đọc `Stays` và tiền cọc; sửa `Stays` (`DAL/HotelTransaction.Commands.cs:23`), thêm `Payments` loại Refund hoặc Forfeit (`:27`), ghi `AuditLog`. |
| Tự hủy lượt quá hạn | Xử lý lượt chưa check-in khi làm mới hoặc đến chu kỳ 60 giây | `GUI/ucDashboard.cs:39-49,301` → `BLL/HotelService.cs:99` | Đọc `Stays`; sửa `Stays`, thêm `Payments` loại Forfeit nếu mất cọc; ghi `AuditLog`. |
| **Thu cọc bổ sung / Ghi nhận thu** | Ghi số tiền thật đã nhận, không gia hạn hạn giữ | `GUI/ucDashboard.Management.cs:12,36` → `BLL/HotelService.Management.cs:50` | Đọc `Stays`; tăng `Stays.Deposit` (`DAL/HotelTransaction.Management.cs:37`), thêm `Payments` loại Deposit (`DAL/HotelTransaction.Commands.cs:27`). |
| **Chuyển đổi phòng / Xác nhận chuyển** | Kết thúc thời gian tính giá phòng cũ, chuyển sang phòng mới | `GUI/ucDashboard.Actions.cs:116,168` → `BLL/HotelService.cs:112` | Đọc `Rooms`, `Stays`, `StaySegments` và lịch trùng; sửa `Rooms` cũ/mới (`DAL/HotelTransaction.Commands.cs:13`), đóng/mở `StaySegments` (`:19,18`), sửa `Stays.RoomId` (`:21`). |
| **Gia hạn trả phòng / Lưu** | Kéo dài ngày trả nếu không đụng lịch khác | `GUI/ucDashboard.Actions.cs:177,223` → `BLL/HotelService.cs:128` | Đọc `Stays` và lịch trùng; sửa `Stays.Departure` (`DAL/HotelTransaction.Commands.cs:22`). |
| **Trả phòng / Hoàn tất check-out** | Chốt tiền, thu thêm hoặc hoàn thừa, phát hành một hóa đơn | `GUI/ucDashboard.cs:311` hoặc ô phòng → `GUI/ucDashboard.Reports.cs:9,67` → `BLL/HotelService.cs:186,191` | Đọc `Stays`, `StaySegments`, `ServiceOrders`, `Rooms`; thêm `Invoices` (`DAL/HotelTransaction.Commands.cs:29`), thêm `Payments` Checkout/Refund (`:27`), đóng `StaySegments` (`:19`), sửa `Stays` Paid (`:23`) và `Rooms` thành Đang dọn (`:13`). |

**Đọc lịch đặt:** khoảng `[ngày đến, ngày trả)` gồm ngày đến nhưng không gồm ngày trả. Chẳng hạn khách A ở 1–3/10 và khách B ở 3–5/10 thì không trùng. Kiểm tra xung đột thực hiện ở `DAL/HotelTransaction.Management.cs:13-18` và BLL từng nghiệp vụ, ngay trong giao dịch SQL. Lịch đặt chỉ là `Stays.Status='Reserved'`; nó **không** đổi trạng thái vật lý của `Rooms`. Vì thế phòng có thể đang ở nhưng vẫn có dấu `*` cho lịch tương lai.

## 5. Dịch vụ, phòng và danh mục

| Nút / chức năng | Ý nghĩa | GUI → BLL | SQL: đọc → ghi |
| --- | --- | --- | --- |
| **+ Thêm dịch vụ / Bar**: Thêm/Xóa khỏi giỏ | Chuẩn bị danh sách món trên màn hình, chưa lưu SQL | `GUI/ucDashboard.Actions.cs:298,339,347` | Đọc menu `Services` (`DAL/HotelRepository.cs:102`); thay giỏ ở bộ nhớ. |
| **Gửi yêu cầu** | Gọi món cho lượt đang ở, lưu giá tại thời điểm gọi | `GUI/ucDashboard.Actions.cs:350` → `BLL/HotelService.cs:138` | Đọc `Stays`, `Services`; thêm `ServiceOrders` (`DAL/HotelTransaction.Commands.cs:25`), tăng `Stays.Version` (`:24`). |
| **Xử lý dịch vụ / Giao một phần** | Giao một số lượng của một dòng | `GUI/ucDashboard.Management.cs:60,125` → `BLL/HotelService.Management.cs:83` | Đọc `ServiceOrders`, `Stays`; tăng `DeliveredQuantity` và có thể điền `Delivered` (`DAL/HotelTransaction.Management.cs:45`). |
| **Đổi số lượng** | Sửa tổng số món và ghi lý do, không giảm dưới số đã giao | `GUI/ucDashboard.Management.cs:126` → `BLL/HotelService.Management.cs:64` | Đọc `ServiceOrders`; sửa `Quantity` (`DAL/HotelTransaction.Management.cs:43`). |
| **Hủy dòng** | Hủy món chưa giao; dòng vẫn ở lịch sử, tiền tính là 0 | `GUI/ucDashboard.Management.cs:127` → `BLL/HotelService.Management.cs:64` | Đọc rồi sửa `ServiceOrders.Cancelled/CancelReason` (`DAL/HotelTransaction.Management.cs:44`). |
| **Xác nhận tất cả / Hủy tất cả chưa giao** | Xử lý hàng loạt món của một lượt ở | `GUI/ucDashboard.Management.cs:131,135` → `BLL/HotelService.cs:151` / `BLL/HotelService.Management.cs:91` | Sửa các `ServiceOrders` phù hợp (`DAL/HotelTransaction.Commands.cs:26`; `DAL/HotelTransaction.Management.cs:43-45`). |
| Nút nhanh ở cột yêu cầu dịch vụ | Cùng hành động xác nhận/hủy chưa giao | `GUI/ucDashboard.cs:261-287` → các BLL trên | `ServiceOrders`, `Stays`, `AuditLog`. |
| **Đã dọn xong tất cả** | Đổi các phòng Đang dọn sang Trống | `GUI/ucDashboard.Actions.cs:292` → `BLL/HotelService.cs:169` | Đọc/sửa `Rooms.Status` (`DAL/HotelTransaction.Commands.cs:13`). |
| Bấm một phòng Đang dọn | Đổi riêng phòng đó sang Trống | `GUI/ucDashboard.cs:164,258` → `GUI/ucDashboard.Actions.cs:39` → `BLL/HotelService.cs:157` | Sửa `Rooms.Status` (`DAL/HotelTransaction.Commands.cs:13`). **Lưu ý mã hiện tại:** BLL này dùng quyền quản lý danh mục, nên Reception có thể thấy nút nhưng bị từ chối; nút dọn tất cả dùng quyền vận hành. |
| **Bật / Tắt bảo trì** | Chặn nhận/đặt phòng khi bảo trì, phải xử lý hết lịch liên quan | `GUI/ucDashboard.Actions.cs:232,284` → `BLL/HotelService.cs:157` | Đọc `Rooms`, `Stays`; sửa `Rooms.Status` (`DAL/HotelTransaction.Commands.cs:13`). Admin/Manager. |
| **Danh mục phòng / Thêm phòng** | Tạo số phòng, loại, giá/ngày, cọc gợi ý | `GUI/ucDashboard.cs:83` → `GUI/ucDashboard.Catalog.cs:58,143` → `BLL/HotelService.Management.cs:116` | Đọc `Rooms`; thêm `Rooms` (`DAL/HotelTransaction.Management.cs:55-57`). |
| **Chọn tất cả / Áp dụng cho phòng đã chọn** | Chọn nhiều phòng rồi sửa giá/cọc/loại trong một lần | `GUI/ucDashboard.Catalog.cs:121,158` → `BLL/HotelService.Management.cs:131` | Chọn tất cả chỉ đổi lựa chọn trên màn hình. Áp dụng thì sửa `Rooms` (`DAL/HotelTransaction.Management.cs:55-57`). |
| **Danh mục dịch vụ / Thêm dịch vụ / Lưu dịch vụ** | Tạo/sửa món, nhóm, đơn vị, giá, trạng thái bán | `GUI/ucDashboard.cs:84` → `GUI/ucDashboard.Catalog.cs:176,199-204` → `BLL/HotelService.Management.cs:106-115` | Đọc/sửa/thêm `Services` (`DAL/HotelTransaction.Management.cs:51-54`). “Thêm” chỉ xóa lựa chọn để nhập món mới; “Lưu” mới ghi SQL. |
| **Bảng giá / Lưu giá phòng / Lưu giá dịch vụ** | Sửa giá một phòng hoặc một món | `GUI/ucDashboard.cs:85` → `GUI/ucDashboard.Catalog.cs:8,43-54` → `BLL/HotelService.Management.cs:107,116` | Sửa `Rooms.Rate` hoặc `Services.Price` (`DAL/HotelTransaction.Management.cs:52-57`). Giá đã chụp vào `StaySegments`/`ServiceOrders` cũ không đổi. |

## 6. Khách hàng, báo cáo, tài khoản

| Nút / chức năng | Bạn sẽ thấy gì | GUI → BLL | SQL: đọc → ghi |
| --- | --- | --- | --- |
| **Hồ sơ khách hàng / Tìm** | Tìm tên, SĐT hoặc CCCD; xem lượt, hóa đơn, dịch vụ | `GUI/ucDashboard.Reports.cs:76,144` → `BLL/HotelService.cs:21-23` | Đọc `Customers` nối `Stays`, `Invoices`, `ServiceOrders` (`DAL/HotelRepository.cs:111,115,112`); không ghi. |
| **Lịch đặt / Lịch sử / Tra cứu / Xuất** | Xem lượt gần đây và lưu CSV | `GUI/ucDashboard.Management.cs:141,188,190` → `BLL/HotelService.Management.cs:24` | Đọc `Stays` (`DAL/HotelTransaction.Management.cs:19`); xuất file ở `GUI/ucDashboard.Export.cs:49`, không sửa SQL. |
| **Hóa đơn / Doanh thu / Xem báo cáo** | Tổng hợp tiền phòng, dịch vụ, cọc mất và dòng tiền theo ngày | `GUI/ucDashboard.Reports.cs:148,254` → `BLL/HotelService.Management.cs:42` | Đọc `Invoices`, `ServiceOrders`, `Payments`, `Stays`, `Users` (`DAL/HotelRepository.cs:110,114`; `DAL/HotelTransaction.Management.cs:46`). |
| **Xuất hóa đơn CSV / Xuất thu chi CSV** | Tạo bảng CSV để mở bằng Excel | `GUI/ucDashboard.Reports.cs:255-256` → `GUI/ucDashboard.Export.cs:33,41,49` | Chỉ đọc báo cáo đang hiển thị; không ghi database. |
| **Xem / In hóa đơn** | Xem chi tiết một hóa đơn, có thể in bằng máy in Windows | `GUI/ucDashboard.Reports.cs:257` → `GUI/ucDashboard.Export.cs:66` | Đọc `Invoices`, `Stays`, `ServiceOrders` qua `BLL/HotelService.Management.cs:48-49`. Đây là phiếu nội bộ, chưa phải hóa đơn điện tử thuế. |
| **Các khoản thu / hoàn / Xem thu chi / Xuất CSV** | Xem từng giao dịch cọc, checkout, hoàn, mất cọc | `GUI/ucDashboard.RoleFunctions.cs:22,38-39` → `BLL/HotelService.Management.cs:42` | Đọc `Payments` nối `Stays`, `Users` (`DAL/HotelTransaction.Management.cs:46`); CSV không ghi SQL. |
| **Thống kê doanh thu** | Chuyển tới biểu đồ | `GUI/ucDashboard.RoleFunctions.cs:43-46` | Dữ liệu nguồn `Invoices`, `ServiceOrders`, `Payments` như mục biểu đồ ở trên. |
| **Nhật ký thao tác / Xem** | Xem tối đa 1.000 thao tác theo ngày | `GUI/ucDashboard.Management.cs:193,199` → `BLL/HotelService.Management.cs:150` | Đọc `AuditLog` nối `Users` (`DAL/HotelTransaction.Management.cs:58`). |
| **Nhân viên / Làm mới** (Manager) | Xem danh sách tài khoản, không chỉnh | `GUI/ucDashboard.RoleFunctions.cs:7,18` → `BLL/AuthService.Management.cs:8` | Đọc `Users` (`DAL/HotelTransaction.Management.cs:47`). |
| **Tài khoản cá nhân / Đổi mật khẩu** | Đổi mật khẩu của chính mình | `GUI/ucDashboard.Reports.cs:271,293` → `BLL/AuthService.cs:67` | Đọc/sửa `Users.PasswordHash`, `Salt`, `SecurityVersion` (`DAL/HotelRepository.cs:83,91`). |
| **Lưu quyền/trạng thái** (Admin) | Đổi vai trò hoặc khóa/mở tài khoản nhân viên | `GUI/ucDashboard.Reports.cs:346` → `BLL/AuthService.Management.cs:18` | Đọc/sửa `Users` (`DAL/HotelTransaction.Management.cs:47-50`). Không tự khóa/đổi quyền tài khoản đang dùng. |
| **Đặt lại mật khẩu nhân viên** | Admin cấp mật khẩu mới cho người khác | `GUI/ucDashboard.Reports.cs:353,361` hoặc `:391,435` → `BLL/AuthService.Management.cs:29` | Sửa `Users.PasswordHash`, `Salt`, `SecurityVersion` (`DAL/HotelRepository.cs:91`). |
| **Đăng ký tài khoản** | Admin tạo người dùng mới và chọn quyền | `GUI/ucDashboard.Reports.cs:374,382` → `BLL/AuthService.cs:54` | Thêm `Users` (`DAL/HotelRepository.cs:86`). |

## 7. Tất cả bảng database

Lệnh gốc tạo các bảng ở [`Database/Setup.sql`](../Database/Setup.sql). Một **khóa ngoại** giống một mã tham chiếu: phiếu ở phải chỉ đến bản ghi có thật ở bảng trái. Ví dụ `Stays.CustomerId` chỉ đến `Customers.Id`.

| Bảng | Hiểu như cuốn sổ | Quan hệ và nơi dùng chính |
| --- | --- | --- |
| `SchemaVersion` | Sổ đánh dấu database đã nâng cấp đến bản nào | Tạo `Database/Setup.sql:20`; đọc ở `DAL/SchemaMigrator.cs:7-12`; migration thêm số bản. |
| `Users` | Nhân viên, vai trò, mật khẩu băm, khóa đăng nhập | Tạo `Database/Setup.sql:21`; đăng nhập/quản lý ở `DAL/HotelRepository.cs:77-91` và `DAL/HotelTransaction.Management.cs:47-50`. `CreatedBy` của nhiều bảng chỉ về đây. |
| `Rooms` | Số phòng, loại, giá, cọc gợi ý, trạng thái vật lý | Tạo `Database/Setup.sql:26`; đọc `DAL/HotelRepository.cs:94-95`, sửa trạng thái `DAL/HotelTransaction.Commands.cs:13`, sửa danh mục `DAL/HotelTransaction.Management.cs:55-57`. |
| `Customers` | Hồ sơ người ở theo CCCD/giấy tờ | Tạo `Database/Setup.sql:32`; tìm `DAL/HotelRepository.cs:111`; tạo/cập nhật khi đặt/sửa ở `DAL/HotelTransaction.Commands.cs:9`, `DAL/HotelTransaction.Management.cs:40`. |
| `Stays` | Một lượt đặt hoặc lưu trú: phòng, khách, ngày, hạn giữ, cọc, trạng thái | Tạo `Database/Setup.sql:35`; đọc `DAL/HotelRepository.cs:98-99`; sửa ở `DAL/HotelTransaction.Commands.cs:10,20-24`, `DAL/HotelTransaction.Management.cs:37-41`. Nối `Rooms`, `Customers`, `Users`. |
| `StaySegments` | Các chặng ở từng phòng, giờ bắt đầu/kết thúc, **giá đã lưu** | Tạo `Database/Setup.sql:47`; đọc `DAL/HotelRepository.cs:100`; thêm/đóng `DAL/HotelTransaction.Commands.cs:18-19`. Nối `Stays`, `Rooms`. Dùng khi chuyển phòng và tính tiền. |
| `Services` | Danh mục món/dịch vụ hiện bán và đơn giá hiện tại | Tạo `Database/Setup.sql:53`; menu `DAL/HotelRepository.cs:102`, quản lý `DAL/HotelTransaction.Management.cs:51-54`. |
| `ServiceOrders` | Món khách đã gọi, số lượng, giá **tại lúc gọi**, số đã giao hoặc hủy | Tạo `Database/Setup.sql:56`; thêm `DAL/HotelTransaction.Commands.cs:25`, sửa `DAL/HotelTransaction.Management.cs:43-45`; đọc `DAL/HotelRepository.cs:106-108`. Nối `Stays`, `Services`, `Users`. |
| `Invoices` | Hóa đơn chốt của một lượt ở, tiền phòng/dịch vụ/cọc/thu/hoàn | Tạo `Database/Setup.sql:63`; thêm `DAL/HotelTransaction.Commands.cs:29`; đọc `DAL/HotelRepository.cs:113-115`. Nối `Stays`, `Users`; một lượt chỉ có một hóa đơn. |
| `Payments` | Mỗi lần thu cọc, thu checkout, hoàn hoặc ghi cọc mất | Tạo `Database/Setup.sql:71`; thêm `DAL/HotelTransaction.Commands.cs:27`; đọc `DAL/HotelTransaction.Management.cs:46`. Nối `Stays`, `Users`. |
| `AuditLog` | Ai đã thao tác gì, lúc nào | Tạo `Database/Setup.sql:77`; thêm `DAL/HotelRepository.cs:92`; đọc `DAL/HotelTransaction.Management.cs:58`. Nối `Users`. |

**Nhớ sự khác nhau:** `Customers` là *người*, `Stays` là *lần người đó đến ở*, `Rooms` là *căn phòng*, `StaySegments` là *người ấy ở phòng nào trong từng đoạn thời gian*. Một khách có thể ở nhiều lần; một lần ở có thể chuyển qua nhiều phòng. `Payments` là tiền đi vào/ra, còn `Invoices` là phiếu tổng kết khi kết thúc.

| Loại `Payments.Kind` | Nghĩa | `CashFlow` trong `DTO/HotelModels.cs:46` |
| --- | --- | --- |
| `Deposit` | Tiền cọc nhận từ khách | Dương |
| `Checkout` | Tiền thu thêm lúc trả | Dương |
| `Refund` | Tiền trả lại khách | Âm |
| `Forfeit` | Ghi cọc đã thu nhưng không hoàn vì quá hạn nhận | **0**: không thu thêm một lần nữa |

Số tiền bằng 0 không tạo dòng `Payments` (`DAL/HotelTransaction.Commands.cs:27-28`). Ví dụ đặt phòng không cọc rồi quá hạn sẽ hủy lượt, nhưng không có giao dịch cọc mất trị giá 0 trong sổ thu chi.

## 8. Quy tắc tính tiền và giữ chỗ

- **Đặt trước:** thời lượng 1–60 ngày. Không cọc được giữ tối đa 24 giờ; có cọc tối đa 15 ngày tính từ khi tạo. Hạn nhận phải trước ngày trả. Mã kiểm tra chính ở `BLL/HotelService.cs:48-71` và sửa lịch ở `BLL/HotelService.Management.cs:27-41`.
- **Hết hạn:** `HoldUntil` đến hoặc qua giờ SQL thì lượt chưa nhận bị hủy. Không hoàn cọc cho trường hợp không đến; có dòng `Forfeit` và nhật ký `NoShow`. Mã ở `BLL/HotelService.cs:99-111`; giờ lấy `DAL/HotelRepository.cs:76`. Việc này chạy lúc vào dashboard, làm mới, hoặc chu kỳ một phút khi app mở (`GUI/ucDashboard.cs:39-49,301`).
- **Nhận phòng:** phòng phải thực sự `Trong`, lịch không được chồng lên lịch khác. Đặt trước không tự chiếm phòng vật lý. Xem `BLL/HotelService.cs:72-84`.
- **Tiền phòng:** tối thiểu một ngày. Tổng thời gian ở được làm tròn lên theo từng 24 giờ; khi đổi phòng, thời gian thực dùng giá của từng `StaySegments`, phần ngày còn thiếu dùng giá phòng cuối. Xem `BLL/BillingPolicy.cs:9` và `DAL/HotelRepository.cs:100`.
- **Tiền dịch vụ:** dòng chưa hủy tính `Quantity × Price` đã chụp khi gọi; `DTO/HotelModels.cs:25`. Đổi giá trong danh mục không sửa hóa đơn của lượt cũ.
- **Trả phòng:** mọi dịch vụ chưa hủy phải giao đủ. Bảng tính checkout chỉ còn hiệu lực 10 phút và hết hiệu lực nếu lượt ở thay đổi. BLL kiểm tra lại ngay lúc xác nhận (`BLL/HotelService.cs:178-222`). Nếu cọc nhiều hơn hóa đơn, tạo `Refund`; nếu thiếu, tạo `Checkout`.
- **Báo cáo:** doanh thu = tiền phòng + dịch vụ trên hóa đơn + cọc không hoàn; dòng tiền thu/chi dùng `Payments.CashFlow`. Hai khái niệm khác nhau: ví dụ nhận cọc hôm nay cho khách trả phòng tuần sau là tiền **đã thu hôm nay**, nhưng chưa phải doanh thu hóa đơn hôm nay. Câu SQL doanh thu ở `DAL/HotelRepository.cs:110`; danh sách thu chi ở `DAL/HotelTransaction.Management.cs:46`.

## 9. Bảo vệ dữ liệu, nâng cấp và xuất file

- **Không để hai người ghi đè nhau:** `DAL/HotelRepository.cs:12-34` chạy giao dịch SQL mức `Serializable`; `:75` lấy khóa ứng dụng. `Version` ở `Rooms`, `Stays`, `Services` giúp từ chối thay đổi dựa trên dữ liệu đã cũ. Mọi giá trị người dùng đưa vào SQL được truyền như tham số (`DAL/HotelRepository.cs:36-73`).
- **Nâng cấp schema:** `Database/Setup.sql:18-80` tạo bản 1. `Database/MigrateV2.sql:18-30` thêm phiên bản bảo mật, giao dịch dịch vụ từng phần, chỉ mục tránh hai khách cùng chiếm phòng. `Database/MigrateV3.sql:17-32` thêm bốn vai trò và lưu trữ tài khoản cũ. `Database/MigrateV4.sql:17-28` bổ sung loại phòng và 10 phòng 401–410; `Database/MigrateV5.sql:7-11` chuyển các phòng đó thành VIP. `DAL/SchemaMigrator.cs:7` chạy tuần tự phần còn thiếu khi login. Bản cài trống có 50 phòng từ `Setup.sql:94-108`, thêm 10 ở V4 thành 60 phòng nếu không bị sửa/xóa.
- **Xuất CSV:** `GUI/ucDashboard.Export.cs:11,17,33,41,49` tạo file UTF-8 có BOM để Excel đọc tiếng Việt, đồng thời chặn ô bắt đầu bằng ký tự công thức. Không xóa hay thay đổi bản ghi SQL.
- **In và QR:** `GUI/ucDashboard.Export.cs:66` in phiếu thanh toán nội bộ. `GUI/PaymentQr.cs:58-61` đổi nội dung QR theo cách trả tiền; QR chuyển khoản chỉ hỗ trợ hiển thị thông tin thanh toán. Nhân viên vẫn phải kiểm tra tiền thực nhận rồi xác nhận, vì chương trình chưa kết nối ngân hàng để đối soát tự động. Cấu hình ở [`appsettings.json`](../QLKhachSan/appsettings.json).

Nếu muốn lần theo bất kỳ thao tác nào: tìm tên nút ở mục 3–6, mở vị trí `GUI`, theo tên hàm sang `BLL`, rồi đến `DAL` để xem đúng câu `SELECT`/`INSERT`/`UPDATE` và bảng liên quan.

## 10. Luồng dữ liệu chi tiết: nhận thông tin từ hàm nào?

Phần này trả lời chính xác câu hỏi **“dữ liệu ấy đến từ đâu?”**. Có hai nguồn:

- **Người dùng nhập:** `TextBox.Text` (chữ), `NumericUpDown.Value` (số tiền/số ngày), `DateTimePicker.Value` (ngày giờ), `ComboBox.SelectedItem` (phòng, tài khoản, phương thức) hoặc `CheckBox.Checked` (xác nhận).
- **Database đã tải:** `data.Rooms`, `data.Stays`, `data.Menu`, `data.Pending` là bản sao để vẽ dashboard (`GUI/ucDashboard.cs:16,110-118`). `data` giúp chọn nhanh trên màn hình, nhưng **BLL đọc lại SQL lúc ghi** để kiểm tra dữ liệu còn mới.

Một hàm có hậu tố `Async` trả kết quả sau khi SQL xong. Ví dụ `await service.DashboardAsync()` nghĩa là “chờ lấy dữ liệu dashboard”; `await Changed(...)` nghĩa là “chờ lưu rồi tải lại”. `Run(...)` tại `GUI/ucDashboard.cs:94-109` khóa tạm các nút để tránh bấm lặp và đưa lỗi ra màn hình. `Changed(...)` tại `:120-129` gọi lệnh ghi rồi `Reload()`; nếu **đã lưu thành công nhưng tải lại lỗi**, màn hình yêu cầu nhấn **Làm mới**, không coi giao dịch đã lưu là thất bại.

### 10.1 Mở app và đăng nhập: từ ô nhập đến phiên làm việc

1. `Program.cs` mở `FormLogin`. `GUI/FormLogin.cs:90-104` gọi `SchemaMigrator.EnsureAsync()` để nâng cấp database nếu cần, sau đó `auth.NeedsSetupAsync()`; hàm này gọi `HotelRepository.RunAsync(false, ...)` và `UserCountAsync()` để đọc `Users` (`BLL/AuthService.cs:18`; `DAL/HotelRepository.cs:82`). Nếu chưa có tài khoản, nút đổi thành **Tạo quản trị**.
2. Khi nhấn nút, `GUI/FormLogin.cs:108-122` lấy `accountPicker.Text` và `txtMatKhau.Text`. Nếu là lần đầu, `AuthService.SetupAsync(username,password)` kiểm tra tên/mật khẩu, tạo `Salt` và `PasswordHash`, rồi `CreateUserAsync(...)` thêm Admin vào `Users` (`BLL/AuthService.cs:19-31`; `DAL/HotelRepository.cs:86`).
3. Ở lần đăng nhập bình thường, hai chuỗi trên truyền vào `AuthService.LoginAsync(username,password)` (`GUI/FormLogin.cs:122`; `BLL/AuthService.cs:33-52`). Hàm đọc tài khoản bằng `AccountAsync(username)` từ `Users` (`DAL/HotelRepository.cs:83-85`), băm mật khẩu nhập và so với hash đã lưu. `LoginResultAsync(id,ok)` ghi số lần sai/khóa tạm hoặc xóa bộ đếm khi đúng (`DAL/HotelRepository.cs:88-90`).
4. Nếu đúng, BLL trả một `UserSession` gồm `Id`, `Username`, `Role`, `SecurityVersion` (`DTO/HotelModels.cs:5-8`). `FormLogin.Session` nhận phiên này; `Program.cs` chuyển sang `FormMain`, nơi tạo `ucDashboard(user)` (`GUI/ucDashboard.cs:30-34`). Nếu sai, GUI hiển thị lỗi và xóa ô mật khẩu (`GUI/FormLogin.cs:131`).
5. Checkbox **Ghi nhớ mật khẩu** chỉ quyết định dữ liệu lưu trên máy: `GUI/FormLogin.cs:125` gọi `RememberedLogin.Save(username, password hoặc chuỗi rỗng)`. Tệp được bảo vệ bằng tài khoản Windows ở `GUI/RememberedLogin.cs:9,47-51`; SQL `Users` vẫn chỉ lưu mật khẩu băm.

**Ví dụ:** nhập `admin` và mật khẩu. GUI chỉ gửi hai chuỗi này vào BLL. BLL không tin “đã đăng nhập” chỉ vì GUI nói thế: nó tự lấy hàng `Users` và so sánh. Sau khi vào dashboard, mỗi hành động BLL lại gọi `RequireUserAsync(user)` để kiểm tra phiên vẫn có quyền (`BLL/HotelService.cs:9-13`; `DAL/HotelRepository.cs:77-80`).

### 10.2 Tải dashboard: SQL trả về những gì, ai nhận kết quả?

1. `ucDashboard` khởi tạo `HotelRepository`, `HotelService` và `AuthService` (`GUI/ucDashboard.cs:30-34`). Sự kiện `Load` hoặc nút **Làm mới** gọi `Reload()` (`GUI/ucDashboard.cs:44-49,301`).
2. `Reload()` gọi `service.DashboardAsync(revenueOverview?.Days ?? 7)` (`GUI/ucDashboard.cs:110-112`). Tham số `7` hay `30` là số ngày biểu đồ; đổi bộ chọn ở `GUI/RevenueOverview.cs:28` dẫn đến `GUI/ucDashboard.cs:295` tải lại.
3. `BLL/HotelService.cs:14-20` lấy giờ từ `db.NowAsync()` rồi, tùy quyền, gọi `RoomsAsync`, `ActiveStaysAsync`, `PendingAsync`, `MenuAsync`, `RevenueAsync` và `RevenueTrendAsync`. Các hàm DAL đọc `Rooms`, `Stays`, `ServiceOrders`, `Services`, `Invoices`, `Payments` (`DAL/HotelRepository.cs:94,98,102,108,110`; `DAL/HotelTransaction.Management.cs:7-11`). Kết quả được gói thành `DashboardData` (`DTO/HotelModels.cs:41-42`).
4. `Reload()` còn gọi `TodayScheduleAsync(fresh.ServerNow.Date)` cho người có quyền xem vận hành; Accountant gọi `PeriodReportAsync(...)` cho số liệu hôm nay (`GUI/ucDashboard.cs:113-114`). Sau đó GUI đặt `data=fresh` và `Render()` (`:116-118`).
5. `Render()` đếm từ `data.Rooms`/`data.Stays` và điền 5 thẻ (`GUI/ucDashboard.cs:132-149`), vẽ ô phòng (`:158-175`), bảng đặt (`:198`), lịch (`:213`), phòng cần dọn (`:250`), yêu cầu dịch vụ (`:261`) và biểu đồ (`:290`). **Vẽ màn hình chỉ đọc dữ liệu**, chưa ghi SQL.

### 10.3 Đặt trước và nhận phòng ngay: mỗi ô đi vào tham số nào?

`ShowBooking(reserve,selected)` mở form (`GUI/ucDashboard.Actions.cs:56-60`). `reserve=true` nghĩa là đặt trước; `false` nghĩa là nhận ngay. Danh sách phòng ban đầu lấy từ `data.Rooms`, danh sách lịch lấy từ `data.Stays` để lọc sơ bộ (`:58,76-84`).

| Trên form | GUI lấy từ đâu | Tham số truyền cho `HotelService.CreateStayAsync` tại `GUI/ucDashboard.Actions.cs:95-98` |
| --- | --- | --- |
| Phòng | `room.SelectedItem` là bản ghi `Room` trong `data.Rooms` | `selected` / `chosen` |
| Họ tên, SĐT, CCCD/hộ chiếu | `name.Text`, `phone.Text`, `identity.Text` | `new GuestInput(...)` |
| Đặt trước hay nhận ngay | Nút đã mở `ShowBooking(true/false)` | `reserve` |
| Ngày dự kiến đến | `arrival.Value` | `arrival`; nhận ngay thì BLL thay bằng giờ SQL |
| Số ngày dự kiến | `days.Value` | `days` |
| Đã nhận cọc? | `deposit.Checked` | `takeDeposit` khi `reserve=true` |
| Cách nhận tiền | `method.SelectedItem` | `method` |
| Số cọc thực nhận | `amount.Value`; phòng có mức gợi ý từ `Room.Deposit` | `depositAmount` khi đặt trước; nhận ngay là 0 |
| Hạn cuối nhận phòng | `receiveBy.Value` | `receiveBy` khi đặt trước; nhận ngay là `null` |

Sau khi nhận tham số, `BLL/HotelService.cs:48-71` gọi `ValidateGuest` (`:27-34`), kiểm tra cách trả tiền (`:35-38`), thời lượng 1–60 ngày, giá trị cọc, trạng thái phòng và hạn giữ. BLL **lấy phòng mới nhất** bằng `RoomAsync(id)` (`DAL/HotelRepository.cs:95`), **lấy giờ SQL** bằng `NowAsync()` (`:76`) và **hỏi còn lịch trống không** bằng `EnsureAvailableAsync(roomId,from,until)` (`DAL/HotelTransaction.Management.cs:13-17`, `SELECT COUNT_BIG(*) FROM dbo.Stays`). Nếu có người vừa đặt trước đó, hành động bị từ chối dù form cũ vẫn từng hiển thị phòng trống.

Khi mọi điều kiện đúng, `db.CreateStayAsync(...)` ghi `Customers` và `Stays` (`DAL/HotelTransaction.Commands.cs:7-11`). Nhận ngay còn đổi `Rooms.Status` sang `DangO` và thêm `StaySegments`; đặt trước giữ nguyên trạng thái vật lý phòng (`BLL/HotelService.cs:66-68`). Nếu cọc lớn hơn 0, `PaymentAsync` thêm `Payments.Kind='Deposit'`; số 0 không thêm dòng (`DAL/HotelTransaction.Commands.cs:27-28`). Cuối cùng ghi `AuditLog` rồi `Changed(...)` tải dashboard lại (`GUI/ucDashboard.cs:120-123`).

**Ví dụ cụ thể:** chọn phòng 101, nhập khách An, 2 ngày và cọc 200.000đ. `Room` phòng 101 và `GuestInput("An", số điện thoại, giấy tờ)` đi vào `CreateStayAsync`. BLL kiểm tra lịch 2 ngày, DAL lưu một `Stays` mới và một `Payments` cọc 200.000đ. Khi tải lại, danh sách đặt có An và ô phòng 101 thêm dấu `*`; phòng vẫn có thể hiện “Trống” cho đến lúc nhận khách.

### 10.4 Nhận lượt đã đặt, sửa, hủy và thu cọc

| Việc người dùng làm | Dữ liệu GUI lấy và truyền vào BLL | BLL đọc/kiểm tra | DAL ghi và kết quả |
| --- | --- | --- | --- |
| **Nhận phòng** ở bảng đặt/lịch | Dòng đã chọn có `BookingRow.Id` hoặc `ScheduleRow.Id`; GUI tìm `Stay` tương ứng trong `data.Stays`, gọi `CheckInAsync(stay)` (`GUI/ucDashboard.cs:317-324,236-249`). | `BLL/HotelService.cs:72-84` đọc lại `Stays`, `Rooms`, giờ SQL và lịch `Stays`; kiểm tra lượt còn Reserved, chưa quá hạn, phòng `Trong`, không trùng lịch. | `DAL/HotelTransaction.Commands.cs:20` sửa `Stays` sang Occupied, `:18` thêm `StaySegments`, `:13` sửa `Rooms` sang Đang ở; tải lại để bảng/lịch thay đổi. |
| **Sửa lịch / khách** | `ShowEditBooking(stay)` điền form từ `Stay` cũ; `GUI/ucDashboard.Management.cs:46-56` truyền `stay`, `target` từ phòng được chọn, `GuestInput` từ ba ô, `arrival`, `days`, `receiveBy`, `reason` vào `UpdateBookingAsync`. | `BLL/HotelService.Management.cs:27-40` đọc `Stays`, `Rooms`, giờ SQL; kiểm tra quyền, hạn, lý do 3–300 ký tự, phòng phù hợp và lịch trống. | `DAL/HotelTransaction.Management.cs:38-41` cập nhật `Customers` theo giấy tờ và `Stays` theo dữ liệu mới; `AuditLog` lưu lý do; tải lại form/bảng. |
| **Hủy đặt** | `ShowCancel(stay)` trước tiên gọi `RefundQuoteAsync(stay)` để lấy tiền được hoàn từ `Stays.Deposit` và giờ SQL (`GUI/ucDashboard.Actions.cs:102-109`; `BLL/HotelService.Management.cs:19-23`). Sau xác nhận, truyền `stay`, `method.SelectedItem`, `expectedRefund` vào `CancelAsync`. | `BLL/HotelService.cs:85-98` đọc lại `Stays` và giờ SQL. Nếu số tiền hoàn đã đổi vì vừa quá hạn, không dùng bảng tính cũ. | `DAL/HotelTransaction.Commands.cs:23` đóng lượt Cancelled; `:27` ghi Refund trước hạn hoặc Forfeit sau hạn (nếu số tiền >0), ghi `AuditLog`; tải lại, lịch đặt biến mất khỏi danh sách hoạt động. |
| **Thu cọc bổ sung** | Combo `choice` lấy các `Reserved` từ `data.Stays` (`GUI/ucDashboard.Management.cs:15`); GUI truyền `selected.Stay`, `amount.Value`, `method.SelectedItem` (`:36-40`) vào `AddDepositAsync`. | `BLL/HotelService.Management.cs:50-62` đọc lại `Stays`, giờ SQL; kiểm tra số tiền >0, chưa quá hạn. Lần cọc đầu đổi hạn giữ tối đa thành 15 ngày tính từ lúc tạo. | `DAL/HotelTransaction.Management.cs:37` tăng `Stays.Deposit`, `DAL/HotelTransaction.Commands.cs:27` thêm `Payments.Deposit`; tải lại số cọc trên dashboard. |

**Điểm dễ nhầm:** trên form sửa lịch, cọc đã thu chỉ hiện để đọc; hàm `UpdateBookingAsync` không nhận tham số cọc. Muốn thu thêm phải vào **Thu cọc bổ sung**. Và nút **Nhận phòng** chỉ truyền `Stay` đã chọn, không lấy lại thông tin khách từ ô nhập; thông tin ấy đã được lưu khi đặt.

### 10.5 Chuyển phòng, gia hạn, dọn phòng, bảo trì

| Nút | Dữ liệu nguồn → hàm nhận | Hàm kiểm tra → SQL → điều thấy sau đó |
| --- | --- | --- |
| **Xác nhận chuyển** | `from.SelectedItem` chứa `Stay` phòng đang ở; danh sách `groupLists` chứa `Room` mới lấy từ `data.Rooms` (`GUI/ucDashboard.Actions.cs:116-124,168-174`). Hai bản ghi đi vào `service.TransferAsync(stay,room)`. | `BLL/HotelService.cs:112-127` đọc `Stays`, cả hai `Rooms`, giờ SQL và lịch phòng mới qua `EnsureAvailableAsync`. DAL đóng `StaySegments` cũ, mở đoạn mới với giá phòng mới, sửa `Stays.RoomId`, chuyển phòng cũ sang Đang dọn, phòng mới sang Đang ở (`DAL/HotelTransaction.Commands.cs:18-21,13`). `Changed` vẽ lại hai ô phòng. |
| **Xác nhận gia hạn** | Combo `stay` từ `StayCombo()` lấy các lượt Occupied trong `data.Stays` (`GUI/ucDashboard.Actions.cs:10,177-181`); `days.Value` là số ngày thêm. GUI gọi `service.ExtendAsync(choice.Stay,(int)days.Value)` (`:223-229`). | `BLL/HotelService.cs:128-137` đọc lại `Stays` và giờ SQL, nhận 1–30 ngày/lần, kiểm tra lịch mới trong `Stays`. `DAL/HotelTransaction.Commands.cs:22` sửa `Stays.Departure`; màn hình hiện hạn trả mới. |
| **Đã dọn xong tất cả** | GUI chọn `data.Rooms` có `Status==DangDon` rồi gọi `CleanAllAsync(rooms)` (`GUI/ucDashboard.Actions.cs:292-296`). | `BLL/HotelService.cs:169-177` đọc lại từng `Room`, so `Version` và trạng thái; `DAL/HotelTransaction.Commands.cs:13` đổi `Rooms.Status` sang Trống trong cùng một giao dịch; các ô phòng đổi màu. |
| Bấm một ô phòng **Đang dọn** | Ô phòng giữ bản ghi `Room` trong `Tag` (`GUI/ucDashboard.cs:171`), click gọi `RoomAction(room)` → `SetRoomStatusAsync(room,Trong)` (`GUI/ucDashboard.Actions.cs:39-54`). | `BLL/HotelService.cs:157-168` dùng `CatalogWrite`, đọc lại `Rooms`, kiểm tra trạng thái; DAL sửa `Rooms.Status`. **Hiện tại Reception có thể thấy ô này nhưng BLL từ chối vì quyền `CatalogWrite`; đây là hành vi thật của code, khác nút dọn tất cả.** |
| **Bắt đầu/Kết thúc bảo trì** | Người dùng chọn `Room` từ `data.Rooms` trạng thái Trống hoặc Bảo trì (`GUI/ucDashboard.Actions.cs:232-235,284-289`). GUI gọi `SetRoomStatusAsync(room,BaoTri hoặc Trong)`. | `BLL/HotelService.cs:157-168` đọc `Rooms`, khi bật bảo trì còn đọc `ActiveStaysAsync` từ `Stays` để chắc không còn lượt đặt/ở. `DAL/HotelTransaction.Commands.cs:13` sửa `Rooms.Status`; ô phòng hiện trạng thái mới. |

### 10.6 Gọi món và xử lý dịch vụ: giá đến từ đâu?

1. Khi dashboard tải, `DashboardAsync()` gọi `db.MenuAsync()` lấy các `Services` có `Active=1` (`BLL/HotelService.cs:19`; `DAL/HotelRepository.cs:102`). `ShowBooking` không dùng danh mục này; nút **Thêm dịch vụ / Bar** mới mở danh sách `data.Menu` và danh sách lượt đang ở (`GUI/ucDashboard.Actions.cs:298-308`).
2. Người dùng chọn món, số lượng, bấm **Thêm**. `GUI/ucDashboard.Actions.cs:339` thêm cặp `(ServiceItem, Quantity)` vào biến `cart` **trong bộ nhớ**. Bấm **Xóa** (`:347`) chỉ bỏ món khỏi giỏ, chưa sửa SQL.
3. Bấm **Gửi yêu cầu** (`:350`) lấy `Stay` đang chọn và biến mỗi món thành `OrderInput(ServiceId,Quantity)` (`DTO/HotelModels.cs:27`). `BLL/HotelService.cs:138-150` đọc lại `Stays`, lấy menu mới nhất bằng `MenuAsync()`, kiểm tra món vẫn còn bán và số lượng hợp lệ.
4. `DAL/HotelTransaction.Commands.cs:25` thêm từng dòng `ServiceOrders` với `Category`, `Name`, `Price` lấy từ `ServiceItem` vừa đọc. Đây là **bản chụp giá**: sửa `Services.Price` ngày mai không sửa giá khách đã gọi hôm nay. `TouchStayAsync()` tăng `Stays.Version` để hóa đơn cũ phải tính lại (`:24`). `Changed` tải lại, yêu cầu xuất hiện ở cột bên phải.
5. Màn hình **Xử lý dịch vụ** chọn một `Stay` bằng `SelectStay(...)` (`GUI/ucDashboard.Management.cs:60-63`). `RefreshOrders()` gọi `service.StayOrdersAsync(selected.Id)` (`:101-115`); BLL gọi `AllOrdersAsync(stayId)` để `SELECT` mọi dòng `ServiceOrders` của lượt ấy, kể cả đã hủy (`BLL/HotelService.Management.cs:26`; `DAL/HotelRepository.cs:107`).

| Nút trong xử lý dịch vụ | Giá trị được truyền | Hàm BLL và câu SQL |
| --- | --- | --- |
| **Giao thêm** | `selected` = lượt ở, `Line().Id` = dòng được chọn, `quantity.Value` = số **giao thêm** (`GUI/ucDashboard.Management.cs:125`). | `DeliverOrderAsync(stay,orderId,quantity)` ở `BLL/HotelService.Management.cs:83-90` đọc `Stays`/`ServiceOrders`, không cho giao vượt số còn lại; `DAL/HotelTransaction.Management.cs:45` tăng `DeliveredQuantity`, đặt `Delivered` khi đủ. |
| **Sửa số lượng** | Lượt ở, dòng, `quantity.Value` = **tổng số lượng mới**, `reason.Text` (`GUI/ucDashboard.Management.cs:126`). | `ChangeOrderAsync(...,cancel:false)` ở `BLL/HotelService.Management.cs:64-81` kiểm tra lý do và không cho thấp hơn đã giao; `DAL/HotelTransaction.Management.cs:43` sửa `ServiceOrders.Quantity`. |
| **Hủy dòng** | Lượt ở, dòng, lý do; GUI truyền `cancel:true` (`GUI/ucDashboard.Management.cs:127-130`). | Cùng `ChangeOrderAsync`, chỉ cho hủy nếu chưa giao đơn vị nào; `DAL/HotelTransaction.Management.cs:44` điền `Cancelled` và `CancelReason`. Dòng vẫn đọc được bằng `AllOrdersAsync`, nhưng `ServiceLine.Total=0` (`DTO/HotelModels.cs:25`). |
| **Xác nhận tất cả** | Chỉ `Stay` đã chọn (`GUI/ucDashboard.Management.cs:131-134`) hoặc cột yêu cầu bên phải (`GUI/ucDashboard.cs:271-275`). | `BLL/HotelService.cs:151-156` → `DAL/HotelTransaction.Commands.cs:26` đặt mọi dòng chưa hủy/chưa giao đủ thành đã giao. |
| **Hủy tất cả chưa giao** | `Stay` đã chọn và `reason.Text` (`GUI/ucDashboard.Management.cs:135-138`; cột phải lấy lý do ở `GUI/ucDashboard.cs:278-284`). | `BLL/HotelService.Management.cs:91-105`: dòng chưa giao gì thì `CancelOrderAsync` (`DAL/HotelTransaction.Management.cs:44`); dòng đã giao một phần thì `UpdateOrderAsync` giảm `Quantity` bằng số đã giao (`:43`). Phần đã giao vẫn tính tiền. |

Sau mỗi lệnh dịch vụ, BLL tăng `Stays.Version` (`DAL/HotelTransaction.Commands.cs:24`) và ghi `AuditLog`. GUI `RefreshOrders()` đọc lại các dòng, đồng thời `Reload()` làm mới cột yêu cầu (`GUI/ucDashboard.Management.cs:101-115`; `GUI/ucDashboard.cs:261-288`).

### 10.7 Checkout: bảng tính nhận số từ những hàm nào?

1. Người dùng bấm thẻ **Chờ trả**, ô phòng Đang ở hoặc nút **Trả phòng**. GUI chọn `Stay` từ `data.Stays` (`GUI/ucDashboard.cs:311`; `GUI/ucDashboard.Actions.cs:39-47`; `GUI/ucDashboard.cs:236-248`), rồi `ShowCheckout(stay)` gọi `service.QuoteAsync(stay)` (`GUI/ucDashboard.Reports.cs:9-11`).
2. `BLL/HotelService.cs:186-190` đọc lại `Stays`, kiểm tra đúng lượt đang ở. `Quote(...)` tại `:178-185` đọc `Room`, mọi `ServiceOrders` chưa hủy (`DAL/HotelRepository.cs:106`), và các `StaySegments` (`:100`). `BillingPolicy.RoomCharge(...)` tính tiền phòng (`BLL/BillingPolicy.cs:9`); `ServiceLine.Total` tính từng món (`DTO/HotelModels.cs:25`). Kết quả là `BillQuote` (`DTO/HotelModels.cs:52-57`) với `Total`, `ToCollect`, `ToRefund`.
3. GUI hiển thị `bill.RoomCharge`, `bill.Lines`, `bill.Total`, `bill.Stay.Deposit`, `bill.ToCollect` hoặc `bill.ToRefund` (`GUI/ucDashboard.Reports.cs:26-39`). Phương thức tiền mặt/chuyển khoản lấy từ `method.SelectedItem` (`:49`). Nếu chọn chuyển khoản và cần thu thêm, `UpdatePayment()` dùng `bill.ToCollect`, `stay.Id` và cấu hình ngân hàng để vẽ QR (`:50-64`). **QR không xác nhận tiền đã vào ngân hàng.**
4. Người dùng phải tích ô **Đã thu đủ tiền / hoàn đủ tiền** (`GUI/ucDashboard.Reports.cs:66-69`). Nút **Hoàn tất check-out** truyền **chính `BillQuote` đang hiển thị** và phương thức vào `CheckoutAsync(bill,method)` (`:67-72`).
5. `BLL/HotelService.cs:191-209` đọc lại `Stays`, giờ SQL và tính lại hóa đơn; từ chối bảng tính quá 10 phút, lượt đã đổi, số tiền đổi hoặc còn món chưa giao. Sau đó `DAL/HotelTransaction.Commands.cs:29` thêm `Invoices`; `:19` đóng đoạn ở; `:23` sửa `Stays` sang Paid; `:27` thêm `Payments` Checkout/Refund nếu số tiền >0; `:13` sửa `Rooms` sang Đang dọn. Cả chuỗi là một giao dịch (`DAL/HotelRepository.cs:12-29`): lỗi ở giữa sẽ rollback, không có hóa đơn “nửa chừng”.
6. `CheckoutAsync` trả `invoiceId`; GUI báo số hóa đơn rồi `Changed()` tải lại dashboard (`GUI/ucDashboard.Reports.cs:70-72`). Phòng hiện **Đang dọn**, lượt không còn trong `ActiveStaysAsync()`.

**Ví dụ:** tiền phòng 1.000.000đ, món 100.000đ, cọc 300.000đ. `BillQuote.Total=1.100.000đ`, `ToCollect=800.000đ`, `ToRefund=0`. SQL có một `Invoices` tổng 1.100.000đ và thêm `Payments.Checkout` 800.000đ; khoản cọc 300.000đ đã có dòng `Payments.Deposit` khi nhận cọc. Vì vậy không cộng 1.100.000đ và 800.000đ thành một “doanh thu” mới.

### 10.8 Khách hàng, lịch sử, doanh thu, in và CSV

| Người dùng xem gì? | Giá trị đầu vào và chuỗi hàm | Dữ liệu SQL trả về → nơi hiện kết quả |
| --- | --- | --- |
| **Tìm khách** | `search.Text` ở `GUI/ucDashboard.Reports.cs:89,103-105,144` → `service.CustomersAsync(search)` (`BLL/HotelService.cs:21`) → `db.CustomersAsync(search.Trim())` (`DAL/HotelRepository.cs:111`). | `SELECT TOP(200)` từ `Customers LEFT JOIN Stays LEFT JOIN Invoices`; trả `List<CustomerSummary>` (`DTO/HotelModels.cs:34`) vào `grid.DataSource` (`GUI/ucDashboard.Reports.cs:106`). |
| **Chọn một khách trong bảng** | `grid.CurrentRow.DataBoundItem` cho `CustomerSummary.Id` (`GUI/ucDashboard.Reports.cs:118-125`). ID này truyền vào `CustomerOrdersAsync(id)` và `CustomerInvoicesAsync(id)` (`:126,133`; `BLL/HotelService.cs:22-23`). | DAL `SELECT` `ServiceOrders JOIN Stays` (`DAL/HotelRepository.cs:112`) và `Invoices JOIN Stays` (`:115`) cùng `CustomerId`; điền tab dịch vụ và tab hóa đơn (`GUI/ucDashboard.Reports.cs:130,135`). Chọn dòng không ghi SQL. |
| **Lịch đặt / Lịch sử** | Nếu tick **Xem cả lịch sử**, `search.Text` đi vào `StayHistoryAsync(search)` (`GUI/ucDashboard.Management.cs:159,170-176`; `BLL/HotelService.Management.cs:24`); không tick thì `DashboardAsync().Stays` được GUI lọc theo `from.Value`, `until.Value` và ô tìm. | Lịch sử: `SELECT TOP(500)` từ `Stays` (`DAL/HotelTransaction.Management.cs:19`), gồm Paid/Cancelled. Chế độ khoảng ngày: `ActiveStaysAsync()` từ `Stays` (`DAL/HotelRepository.cs:98`), nên chỉ gồm lượt còn hoạt động; `grid.DataSource` nhận danh sách đã lọc. |
| **Lịch hôm nay** | `Reload()` truyền `fresh.ServerNow.Date` vào `TodayScheduleAsync(day)` (`GUI/ucDashboard.cs:113`; `BLL/HotelService.Management.cs:25`). | DAL `TodayScheduleAsync` (`DAL/HotelTransaction.Management.cs:20-36`) ghép bốn nhóm từ `Stays JOIN Rooms` và `Invoices JOIN Stays`; `RenderSchedule()` chia theo `Stage` (`GUI/ucDashboard.cs:213-234`). |
| **Xem báo cáo** | `day.Value` và `through.Value` ở `GUI/ucDashboard.Reports.cs:162-164,219-223` → `PeriodReportAsync(from,through)` (`BLL/HotelService.Management.cs:42-46`). BLL giới hạn tối đa 367 ngày và đổi ngày cuối thành mốc đầu **ngày kế tiếp** để lấy hết ngày được chọn. | DAL lấy `InvoicesAsync` từ `Invoices` (`DAL/HotelRepository.cs:114`), `RevenueAsync` từ `Invoices`, `ServiceOrders`, `Payments` (`:110`), và `PaymentsAsync` từ `Payments JOIN Stays JOIN Users` (`DAL/HotelTransaction.Management.cs:46`). `PeriodReport` (`DTO/HotelModels.cs:48`) được `LoadReport()` đưa vào các tab và 5 thẻ (`GUI/ucDashboard.Reports.cs:224-250`). |
| **Biểu đồ 7/30 ngày** | `RevenueOverview.Days` (`GUI/RevenueOverview.cs:28`) → `DashboardAsync(days)` (`GUI/ucDashboard.cs:112`) → `RevenueTrendAsync(from,until)` (`BLL/HotelService.cs:19`). | DAL gom theo từng ngày từ `Invoices` và `Payments.Forfeit` (`DAL/HotelTransaction.Management.cs:7-11`), tạo ngày trống bằng 0; `RevenueOverview.SetData(data)` vẽ đồ thị (`GUI/ucDashboard.cs:290-299`). |
| **Xem / In hóa đơn** | Người dùng chọn `Invoice` trong `grid.DataSource` (`GUI/ucDashboard.Reports.cs:257-264`). `invoice.StayId` đi vào `InvoiceStayAsync` và `InvoiceOrdersAsync` (`BLL/HotelService.Management.cs:48-49`). | DAL đọc `Stays` (`DAL/HotelRepository.cs:99`) và **mọi** `ServiceOrders` (`:107`); `PrintInvoice(invoice,stay,lines)` lọc dòng chưa hủy và tạo phiếu in (`GUI/ucDashboard.Export.cs:66-70`). Không thêm `Invoices` mới, không ghi SQL. |
| **Xuất CSV** | Nút xuất dùng `currentReport` đã được `LoadReport()` đọc, hoặc dữ liệu đang ở `DataGridView` (`GUI/ucDashboard.Reports.cs:255-256`; `GUI/ucDashboard.Management.cs:190`; `GUI/ucDashboard.RoleFunctions.cs:39`). | `GUI/ucDashboard.Export.cs:17-64` mở hộp chọn nơi lưu, ghi file CSV từ dữ liệu **đang xem**. Không chạy `INSERT`/`UPDATE`. Nếu vừa có giao dịch mới sau khi mở báo cáo, cần bấm **Xem báo cáo** trước khi xuất để CSV cập nhật. |

### 10.9 Danh mục, bảng giá, nhân viên và nhật ký

| Nút / màn hình | Đầu vào GUI → BLL | BLL/DAL làm gì → dữ liệu hiện lại |
| --- | --- | --- |
| **Danh mục phòng → Thêm phòng** | `number.Text`, `newType.SelectedItem`, `newRate.Value`, `newDeposit.Value` tạo `Room` có `Id=0` (`GUI/ucDashboard.Catalog.cs:96-102,143-149`) → `SaveRoomAsync(room)` (`BLL/HotelService.Management.cs:116-130`). | BLL kiểm tra số phòng, loại, giá, cọc, trùng số bằng `RoomsAsync()`; DAL `INSERT Rooms` (`DAL/HotelTransaction.Management.cs:55-56`); `LoadRows()` gọi `Reload()` lấy `Rooms` mới (`GUI/ucDashboard.Catalog.cs:122-128`). |
| **Áp dụng cho phòng đã chọn** | `SelectedRooms()` lấy các hàng chọn trong lưới; ba checkbox xác định có đổi giá, cọc, loại hay không (`GUI/ucDashboard.Catalog.cs:107-121,158-168`). Chưa tick nghĩa là truyền `null` và giữ giá cũ. | `UpdateRoomsAsync(selected,rate?,deposit?,type?)` (`BLL/HotelService.Management.cs:131-149`) kiểm tra tối đa 200 phòng, bản ghi không cũ, không đổi giá/loại nếu còn `Stays` hoạt động. DAL `UPDATE Rooms` từng phòng (`DAL/HotelTransaction.Management.cs:57`) trong **một** giao dịch; lỗi một phòng thì rollback tất cả. |
| **Danh mục dịch vụ → Thêm dịch vụ** | `GUI/ucDashboard.Catalog.cs:199` chỉ xóa lựa chọn cũ để tạo phiếu mới; **chưa gọi BLL**. | Không ghi SQL. |
| **Danh mục dịch vụ → Lưu dịch vụ** | `category.Text`, `name.Text`, `price.Value`, `unit.Text`, `active.Checked`, ID/Version của dòng chọn thành `ServiceCatalogItem` (`GUI/ucDashboard.Catalog.cs:180-204`) → `SaveServiceAsync(item)` (`BLL/HotelService.Management.cs:107-115`). | BLL kiểm tra giá, tên không trùng từ `CatalogAsync()`; DAL `INSERT` nếu `Id=0`, còn lại `UPDATE Services ... WHERE Version=...` (`DAL/HotelTransaction.Management.cs:51-54`). Tải lại menu/dashboard sau khi lưu. |
| **Bảng giá → Lưu giá phòng/dịch vụ** | Chọn hàng `Room` hoặc `ServiceCatalogItem`, lấy `roomPrice.Value`/`servicePrice.Value`, tạo bản sao `with {Rate=...}` hoặc `with {Price=...}` (`GUI/ucDashboard.Catalog.cs:41-54`). | Gọi cùng `SaveRoomAsync` hoặc `SaveServiceAsync` nói trên; `UPDATE Rooms`/`Services`, rồi tải lại bảng giá. Giá dòng ở cũ không bị tính lại. |
| **Đổi mật khẩu của tôi** | `oldPassword.Text`, `password.Text`, `again.Text` (`GUI/ucDashboard.Reports.cs:290-300`) → `auth.ChangePasswordAsync(user,old,new)` (`BLL/AuthService.cs:67-80`). | BLL xác nhận hai ô mới khớp, kiểm tra mật khẩu hiện tại qua `Users`; DAL `UPDATE Users` hash/salt, tăng `SecurityVersion` (`DAL/HotelRepository.cs:91`); phiên hiện tại được cập nhật để tiếp tục dùng. |
| **Đăng ký tài khoản** | `name.Text`, `password.Text`, `role.SelectedItem` (`GUI/ucDashboard.Reports.cs:374-385`) → `auth.CreateUserAsync(actor,username,password,role)` (`BLL/AuthService.cs:54-65`). | BLL yêu cầu Admin, kiểm tra tên/mật khẩu/vai trò, `AccountAsync()` kiểm tra trùng; DAL `INSERT Users` (`DAL/HotelRepository.cs:86`) và `AuditLog`. |
| **Lưu quyền / trạng thái** | `Selected()` lấy `UserInfo` đã chọn; `editRole.SelectedItem`, `active.Checked` (`GUI/ucDashboard.Reports.cs:326,346-351`) → `auth.UpdateUserAsync(actor,selected,role,active)` (`BLL/AuthService.Management.cs:18-28`). | BLL đọc lại `Users`, so `Version`, không cho tự sửa và không cho tắt Admin cuối. DAL `UPDATE Users`, tăng `SecurityVersion` (`DAL/HotelTransaction.Management.cs:50`); `LoadRows()` gọi `auth.UsersAsync()` đọc lại bảng. |
| **Đổi/Đặt lại mật khẩu nhân viên** | Chọn `UserInfo`, nhập hai ô mật khẩu mới (`GUI/ucDashboard.Reports.cs:353-366` hoặc `:435-444`) → `auth.ResetPasswordAsync(actor,selected,password)` (`BLL/AuthService.Management.cs:29-43`). | BLL yêu cầu Admin, so hai ô, so `Version`, không cho tự đặt lại; DAL `UPDATE Users` hash/salt và `SecurityVersion` (`DAL/HotelRepository.cs:91`). Nhân viên đang đăng nhập phải đăng nhập lại. |
| **Nhân viên / Làm mới** | `ShowEmployees()` gọi `auth.EmployeesAsync(user)` (`GUI/ucDashboard.RoleFunctions.cs:7-19`; `BLL/AuthService.Management.cs:8-13`). | DAL `SELECT Users` (`DAL/HotelTransaction.Management.cs:47`), BLL bỏ tài khoản Admin khỏi danh sách; không ghi SQL. |
| **Nhật ký thao tác / Xem** | `day.Value` đi vào `service.AuditsAsync(day)` (`GUI/ucDashboard.Management.cs:193-199`; `BLL/HotelService.Management.cs:150`). | DAL `SELECT TOP(1000)` từ `AuditLog JOIN Users` theo ngày (`DAL/HotelTransaction.Management.cs:58`); bảng hiển thị ai làm, lúc nào, hành động gì. |

### 10.10 Những nút và ô chỉ thay đổi giao diện

Các thao tác dưới đây **không gọi hàm BLL để ghi database tại thời điểm bấm**. Chúng giúp chọn dữ liệu hoặc chuẩn bị biểu mẫu; chỉ nút xác nhận/lưu ở các mục trên mới gửi dữ liệu đi.

| Điều khiển | Nguồn thông tin và tác dụng | Vị trí |
| --- | --- | --- |
| Checkbox **Hiện mật khẩu** | Đổi `UseSystemPasswordChar` để nhìn nội dung ô đang nhập; không đọc/ghi `Users`. | `GUI/FormLogin.cs:107` |
| Dropdown tài khoản nhớ | Đọc `remembered-login.dat` của Windows qua `RememberedLogin.Load()` để điền ô mật khẩu; chưa xác thực với SQL cho đến khi bấm Đăng nhập. | `GUI/FormLogin.cs:78-88`; `GUI/RememberedLogin.cs:11-29` |
| Tìm phòng trên dashboard | `txtTimPhong.Text` so với `data.Rooms` đã tải; chỉ sau đó mới gọi `RoomAction(room)` để mở thao tác theo trạng thái. | `GUI/ucDashboard.cs:312-315` |
| Tìm phòng trong form chuyển/bảo trì | Lọc `Room` ở các danh sách trong bộ nhớ; chưa sửa `Rooms`. | `GUI/ucDashboard.Actions.cs:147-166,266-283` |
| Đổi ngày/số ngày trên form đặt | `FilterRooms()` dùng `data.Stays` để ẩn phòng có lịch giao nhau trên giao diện; BLL vẫn kiểm tra lại trong SQL khi Lưu. | `GUI/ucDashboard.Actions.cs:76-84` |
| Chọn thẻ, tab lịch hoặc tab báo cáo | Chỉ chuyển vùng hiển thị hay lọc các hàng đã có trong `todaySchedule`/`currentReport`. | `GUI/ucDashboard.cs:213-234,307`; `GUI/ucDashboard.Reports.cs:176-190` |
| **Thêm** / **Xóa** món trong giỏ | Thay biến `cart` trong bộ nhớ; `ServiceOrders` chỉ được tạo khi bấm **Gửi yêu cầu**. | `GUI/ucDashboard.Actions.cs:339,347,350` |
| **Chọn tất cả phòng trong mục** | Gọi `grid.SelectAll()`; chưa sửa `Rooms`. | `GUI/ucDashboard.Catalog.cs:121` |
| **Thêm dịch vụ** trong danh mục | Bỏ lựa chọn cũ và xóa form để nhập món mới; chưa thêm `Services`. | `GUI/ucDashboard.Catalog.cs:199` |
| Nút **Đóng** ở hộp thoại | Đóng cửa sổ; dữ liệu đã lưu bằng nút khác vẫn còn, dữ liệu form chưa lưu bị bỏ. | Ví dụ `GUI/ucDashboard.Management.cs:93`; `GUI/ucDashboard.Reports.cs:198` |

`PaymentQr.Add(...)` tại `GUI/PaymentQr.cs:7-68` cũng chỉ hỗ trợ hiển thị. Nó nhận hàm `amount()` và `reference()` từ form đặt/cọc (`GUI/ucDashboard.Actions.cs:92`; `GUI/ucDashboard.Management.cs:28`), đọc cấu hình ngân hàng, rồi tạo địa chỉ ảnh QR (`GUI/PaymentQr.cs:33-35`). Khi đổi số tiền hoặc phương thức, `Refresh()` chạy lại (`:37-61`). **Không có hàm nào ở đây đọc số dư ngân hàng hay tự tạo `Payments`.** `Payments` chỉ được ghi khi người dùng xác nhận nghiệp vụ cọc/checkout.

## 11. Tra từng hàm đọc SQL và bảng nguồn

Bảng này giúp trả lời nhanh **“chức năng ấy SELECT từ bảng nào?”**. Các dòng thuộc `HotelTransaction` được chia giữa `DAL/HotelRepository.cs` và `DAL/HotelTransaction.Management.cs` nhưng đều chạy trong cùng một giao dịch do `HotelRepository.RunAsync` mở. `Query` đọc nhiều hàng; `Scalar` đọc một giá trị như số đếm; `Execute` chạy lệnh ghi (`DAL/HotelRepository.cs:56-73`).

| Hàm DAL | SELECT/nguồn dữ liệu | BLL/GUI nhận kết quả để làm gì |
| --- | --- | --- |
| `NowAsync`, `DAL/HotelRepository.cs:76` | `SELECT SYSDATETIME()` từ SQL Server | BLL kiểm tra hạn theo cùng đồng hồ; GUI hiện đồng hồ dashboard. |
| `RequireUserAsync`, `:77-80` | `Users` theo ID, tên, vai trò, phiên bản bảo mật, đang hoạt động | Mọi thao tác xác nhận người dùng còn quyền. |
| `UserCountAsync`, `:82` | `Users` chưa lưu trữ | Login quyết định có phải tạo Admin đầu tiên không. |
| `AccountAsync`, `:83-85` | `Users` theo Username | Đăng nhập, đổi mật khẩu, kiểm tra tên trùng. |
| `RoomsAsync`/`RoomAsync`, `:94-95` | `Rooms` tất cả/theo ID | Dashboard, form chọn phòng; BLL đọc lại một phòng trước khi ghi. |
| `ActiveStaysAsync`/`StayAsync`, `:98-99` | `Stays` còn hoạt động/theo ID | Dashboard, chọn lượt; BLL đọc lại lượt để tránh sửa thông tin cũ. |
| `SegmentsAsync`, `:100` | `StaySegments` theo `StayId` | Tính tiền phòng và các đoạn khi chuyển phòng. |
| `MenuAsync`, `:102` | `Services` có `Active=1` | Menu gọi dịch vụ; BLL xác nhận món còn bán. |
| `OrdersAsync`/`AllOrdersAsync`, `:106-107` | `ServiceOrders` theo `StayId`, không hủy / gồm cả đã hủy | Checkout chỉ tính dòng chưa hủy; quản lý và in lịch sử dùng mọi dòng. |
| `PendingAsync`, `:108` | `ServiceOrders` chưa giao xong và chưa hủy | Cột yêu cầu dịch vụ trên dashboard. |
| `RevenueAsync`, `:110` | `Invoices` + `ServiceOrders` + `Payments.Forfeit` | Thẻ doanh thu và báo cáo theo hạng mục. |
| `CustomersAsync`, `:111` | `Customers LEFT JOIN Stays LEFT JOIN Invoices` | Danh sách khách, lượt đã thanh toán, tổng chi. |
| `CustomerOrdersAsync`, `:112` | `ServiceOrders JOIN Stays` theo CustomerId | Tab dịch vụ trong hồ sơ khách. |
| `InvoicesAsync`, `:113-114` | `Invoices` trong khoảng ngày | Báo cáo hóa đơn. |
| `CustomerInvoicesAsync`, `:115` | `Invoices JOIN Stays` theo CustomerId | Tab hóa đơn trong hồ sơ khách. |
| `RevenueTrendAsync`, `DAL/HotelTransaction.Management.cs:7-11` | `Invoices` + `Payments.Forfeit`, gom theo ngày | Biểu đồ 7/30 ngày. |
| `EnsureAvailableAsync`, `:13-17` | Đếm `Stays` đang hoạt động của một phòng có khoảng ngày giao nhau | Chặn đặt/nhận/chuyển/gia hạn trùng lịch. |
| `StayHistoryAsync`, `:19` | 500 dòng `Stays` gần nhất, có tìm theo tên/giấy tờ/SĐT | Lịch sử gồm cả Paid và Cancelled. |
| `TodayScheduleAsync`, `:20-36` | `Stays JOIN Rooms`, `Invoices JOIN Stays` | Bốn tab lịch hôm nay. |
| `PaymentsAsync`, `:46` | `Payments JOIN Stays JOIN Users` | Thu/hoàn, nhân viên thực hiện, báo cáo dòng tiền. |
| `UsersAsync`/`ActiveAdminsAsync`, `:47-48` | `Users` / số Admin đang hoạt động | Màn hình nhân viên, kiểm tra còn Admin khi đổi quyền. |
| `CatalogAsync`, `:51` | `Services` kể cả ngừng bán | Màn hình danh mục và bảng giá dịch vụ. |
| `AuditsAsync`, `:58` | `AuditLog JOIN Users`, tối đa 1.000 dòng | Nhật ký thao tác. |

**Cách đọc một câu SQL thật:** trong `DAL/HotelTransaction.Management.cs:16`, `RoomId=@p0` có nghĩa chỉ xem phòng đang chọn; `COALESCE(CheckIn,Arrival)<@p2 AND Departure>@p1` có nghĩa khoảng thời gian cũ cắt qua khoảng mới. Các giá trị `@p0`, `@p1`… đến từ tham số C# ngay sau chuỗi SQL, không phải nối trực tiếp chữ người dùng vào câu SQL (`DAL/HotelRepository.cs:36-54`).

## 12. Đọc code và xử lý lỗi bằng ví dụ

Giả sử hai lễ tân cùng mở phòng 101 khi nó còn trống. Cả hai thấy `data.Rooms` cũ. Người thứ nhất nhấn nhận trước, SQL lưu lượt và đổi `Rooms.Version`. Người thứ hai nhấn sau: `HotelRepository.RunAsync(true,...)` lấy khóa ghi (`DAL/HotelRepository.cs:12-22,75`), BLL đọc phòng mới bằng `RoomAsync()` và `RoomUnchanged(...)` so bản cũ (`BLL/HotelService.cs:39-45,48-57`). Thao tác thứ hai báo “Phòng đã thay đổi”, không tạo lượt trùng. Đây là lý do cần **đọc lại ở BLL** dù GUI đã lọc phòng.

| Nếu có lỗi | Điều xảy ra trong code | Người dùng nên làm |
| --- | --- | --- |
| Nhập tên/số/giấy tờ sai | `ValidateGuest` ném `BusinessException` (`BLL/HotelService.cs:27-34`); `Run`/dialog đưa thông báo (`GUI/ucDashboard.cs:94-109`). | Sửa ô theo lời báo; chưa có `INSERT`. |
| Dữ liệu đã đổi ở máy khác | `Version`/trạng thái hoặc kiểm tra lịch không khớp (`BLL/HotelService.cs:39-45`; `DAL/HotelTransaction.Management.cs:13-17`). | Nhấn **Làm mới**, chọn lại bản ghi. |
| SQL lỗi sau khi bắt đầu ghi | `HotelRepository.RunAsync` rollback toàn giao dịch (`DAL/HotelRepository.cs:25-29`). | Xem thông báo, kiểm tra kết nối; không coi những bước trước lỗi là đã lưu riêng. |
| Ghi thành công nhưng tải dashboard lại lỗi | `Changed()` bắt lỗi từ `Reload()` và hiện “ĐÃ LƯU — chưa tải lại được” (`GUI/ucDashboard.cs:120-129`). | Nhấn **Làm mới**; tránh bấm lại nút ghi ngay để khỏi lặp giao dịch. |
| Phiên bị đổi quyền/khóa | `RequireUserAsync` so `SecurityVersion`, `Role`, `Active` với `Users` (`DAL/HotelRepository.cs:77-80`). | Đăng nhập lại bằng tài khoản hợp lệ. |

**Thứ tự đọc code khuyến nghị:** bắt đầu từ tên nút trong mục 3–6 → đối chiếu bảng “dữ liệu nguồn → hàm” ở mục 10 → xem đúng hàm `BLL` để hiểu điều kiện → tìm hàm `DAL` ở mục 11 để đọc `SELECT`/`INSERT`/`UPDATE` → quay lại `Changed`/`Reload` để hiểu kết quả xuất hiện ở đâu. Khi sửa mã về sau, tìm tên hàm nếu số dòng đã thay đổi.
