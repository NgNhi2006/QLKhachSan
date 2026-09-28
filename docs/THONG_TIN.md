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
