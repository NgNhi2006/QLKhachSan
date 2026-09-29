# Thông tin và giải thích mã nguồn QLKhachSan

[← README](../README.md)

**Cập nhật V6:** tài liệu này mô tả luồng lưu trú và bổ sung bản đồ mã nguồn kế toán ở [mục 20](#20-phân-hệ-tài-chính---kế-toán-v6). Xem thêm [quy trình và giới hạn Tài chính - Kế toán](ACCOUNTING_FINANCE.md). Các địa chỉ dòng của phần lưu trú là mốc tham khảo; ưu tiên tìm theo tên hàm khi mã thay đổi.

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
13. [Giải thích cửa sổ Hồ sơ khách hàng trong ảnh](#13-giải-thích-cửa-sổ-hồ-sơ-khách-hàng-trong-ảnh)
14. [Bản đồ nguồn và nơi lưu dữ liệu của các màn hình](#14-bản-đồ-nguồn-và-nơi-lưu-dữ-liệu-của-các-màn-hình)
15. [Bản đồ mã nguồn và thuật ngữ](#phan-15)
16. [Giao diện và sự kiện](#phan-16)
17. [Nghiệp vụ và dữ liệu](#phan-17)
18. [Cơ sở dữ liệu và cấu hình](#phan-18)
19. [Ví dụ theo dõi một thao tác](#phan-19)
20. [Phân hệ Tài chính - Kế toán V6](#20-phân-hệ-tài-chính---kế-toán-v6)

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
| Mở ứng dụng | `Program.cs` → `GUI/FormLogin.cs` → `DAL/SchemaMigrator.cs` | Kiểm tra `SchemaVersion`, tự chạy migration V2–V6 nếu cần. Kết nối lấy từ `DAL/DatabaseHelper.cs` và `appsettings.json`; lỗi migration hiển thị phiên bản, mã SQL và dòng lỗi. |
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
| Admin | Vận hành, tài chính, danh mục và tài khoản | `BLL/RolePolicy.cs`; `BLL/HotelService.Finance.cs` |
| Reception | Đặt/nhận/trả phòng, cọc, dịch vụ, tra khách và ca cá nhân | `BLL/RolePolicy.cs`; `BLL/HotelService.Finance.cs` |
| Accountant | Xem/ghi sổ kế toán, duyệt khóa ca/kỳ, không thực hiện nghiệp vụ lễ tân | `BLL/RolePolicy.cs`; `BLL/HotelService.Finance.cs` |
| Manager | Xem vận hành, ghi sổ tài chính, quản lý danh mục/bảo trì; không ghi đặt/thu/trả phòng | `BLL/RolePolicy.cs`; `BLL/HotelService.Finance.cs` |

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
| **Đặt trước / Giữ chỗ** | Giữ một khoảng ngày và tùy chọn nhận cọc | `GUI/ucDashboard.cs:309` → `GUI/ucDashboard.Actions.cs:56,95` → `BLL/HotelService.cs:48` | Đọc `Rooms`, `Stays`; ghi `Customers`, `Stays` (`DAL/HotelTransaction.Commands.cs:7-10`); tăng `Rooms.Version` nhưng giữ trạng thái phòng (`BLL/HotelService.cs:65`); nếu có cọc ghi `Payments` (`DAL/HotelTransaction.Commands.cs:27-28`); ghi `AuditLog`. [Xem từng ô, từng cột và khóa liên kết ở mục 10.3](#103-đặt-trước-và-nhận-phòng-ngay-mỗi-ô-đi-vào-tham-số-nào). |
| **Nhận phòng** từ bảng/lịch | Đổi lượt Reserved sang Occupied khi phòng thực sự trống | `GUI/ucDashboard.cs:317-324` hoặc `:236-249` → `BLL/HotelService.cs:72` | Đọc `Stays`, `Rooms` và lịch trùng; sửa `Stays` (`DAL/HotelTransaction.Commands.cs:20`), thêm `StaySegments` (`:18`), sửa `Rooms` (`:13`). |
| **Sửa thông tin khách** | Trong **bảng đặt trước**, bấm nút trên đúng dòng khách; sửa họ tên/SĐT/CCCD, hoặc đổi phòng, ngày đến/trả và hạn nhận; phải ghi lý do | `GUI/ucDashboard.cs:202,317-324` → `GUI/ucDashboard.Management.cs:43,53` → `BLL/HotelService.Management.cs:27` | Đọc `Stays`, `Rooms` và lịch trùng; sửa/thêm `Customers`, sửa `Stays` (`DAL/HotelTransaction.Management.cs:38-41`), tăng `Rooms.Version` của phòng liên quan (`BLL/HotelService.Management.cs:38-39`); ghi `AuditLog`. Cọc đã thu được giữ nguyên. [Xem đường sửa chi tiết ở mục 10.3.3](#sua-thong-tin-khach). |
| **Hủy đặt phòng** | Hủy trước hạn thì hoàn cọc; quá hạn thì ghi cọc không hoàn | `GUI/ucDashboard.Actions.cs:102,109` → `BLL/HotelService.cs:85` | Đọc `Stays` và tiền cọc; sửa `Stays` (`DAL/HotelTransaction.Commands.cs:23`), thêm `Payments` loại Refund hoặc Forfeit nếu tiền >0 (`:27-28`), tăng `Rooms.Version` nhưng không đổi trạng thái (`BLL/HotelService.cs:96`), ghi `AuditLog`. |
| Tự hủy lượt quá hạn | Xử lý lượt chưa check-in khi làm mới hoặc đến chu kỳ 60 giây | `GUI/ucDashboard.cs:39-49,301` → `BLL/HotelService.cs:99` | Đọc `Stays`; sửa `Stays`, thêm `Payments` loại Forfeit nếu mất cọc, tăng `Rooms.Version` (`BLL/HotelService.cs:107`); ghi `AuditLog`. |
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

### 6.1 Hồ sơ khách hàng: trả lời được nguồn của từng dòng và từng cột

Đường đi khi mở màn hoặc nhấn **Tìm khách** là [`btnQuanLyKhach_Click` → `LoadCustomers()`](../QLKhachSan/GUI/ucDashboard.Reports.cs#L76) → [`HotelService.CustomersAsync(search)`](../QLKhachSan/BLL/HotelService.cs#L21) → [`HotelTransaction.CustomersAsync(search)`](../QLKhachSan/DAL/HotelRepository.cs#L111) → `grid.DataSource` ở [`ucDashboard.Reports.cs:106`](../QLKhachSan/GUI/ucDashboard.Reports.cs#L106). Chữ trong ô tìm là `search.Text`; DAL tìm trong `Customers.Name`, `Customers.Phone` hoặc `Customers.IdentityNumber`. Không nhập chữ thì lấy tối đa 200 khách có `Id` mới nhất. Bấm tìm **không sửa bảng**.

| Cột trong **Danh sách khách hàng** | Giá trị gốc và phép tính | Mã nối dữ liệu; nơi đưa lên màn hình |
| --- | --- | --- |
| Họ tên, SĐT, CCCD/Hộ chiếu | `Customers.Name`, `Customers.Phone`, `Customers.IdentityNumber` | `Customers.Id` đi vào `CustomerSummary.Id`; GUI đổi tên cột ở [`ucDashboard.Reports.cs:107–112`](../QLKhachSan/GUI/ucDashboard.Reports.cs#L107). |
| Lượt đã thanh toán | `COUNT(Invoices.Id)`; không có cột tổng này trong `Customers` | SQL [`Customers c LEFT JOIN Stays s ON s.CustomerId=c.Id LEFT JOIN Invoices i ON i.StayId=s.Id`](../QLKhachSan/DAL/HotelRepository.cs#L111). Chỉ lượt có hóa đơn mới được đếm. |
| Tổng chi | `COALESCE(SUM(Invoices.RoomCharge + Invoices.ServiceCharge),0)`; không cộng lại tiền cọc đã thu | Cùng phép nối trên; `CustomerSummary.Total` được GUI định dạng ở [`ucDashboard.Reports.cs:112`](../QLKhachSan/GUI/ucDashboard.Reports.cs#L112). |

Khi chọn một hàng, GUI lấy **`CustomerSummary.Id`**, không lấy tên (hai người có thể trùng tên). [`SelectionChanged`](../QLKhachSan/GUI/ucDashboard.Reports.cs#L118) gọi hai đường đọc riêng:

| Tab | Chuỗi hàm và câu nối SQL | Cột hiện trên tab lấy từ đâu? |
| --- | --- | --- |
| **Dịch vụ đã sử dụng** | `service.CustomerOrdersAsync(customer.Id)` ở [`GUI:126`](../QLKhachSan/GUI/ucDashboard.Reports.cs#L126) → [`BLL/HotelService.cs:23`](../QLKhachSan/BLL/HotelService.cs#L23) → [`DAL/HotelRepository.cs:112`](../QLKhachSan/DAL/HotelRepository.cs#L112): `ServiceOrders o JOIN Stays s ON s.Id=o.StayId WHERE s.CustomerId=@p0`. | `Ngày` = `o.Ordered`; `TênDịchVụ` = `o.Name`; `SốLượng` = `o.Quantity`; `ThànhTiền` = `ServiceLine.Total` (`Quantity×Price`, bằng 0 nếu đã hủy); `TrạngThái` do GUI xét `o.Cancelled`. Phép biến đổi/điền bảng ở [`GUI:130`](../QLKhachSan/GUI/ucDashboard.Reports.cs#L130). Tối đa 500 dòng mới nhất. |
| **Hóa đơn** | `service.CustomerInvoicesAsync(customer.Id)` ở [`GUI:133`](../QLKhachSan/GUI/ucDashboard.Reports.cs#L133) → [`BLL/HotelService.cs:22`](../QLKhachSan/BLL/HotelService.cs#L22) → [`DAL/HotelRepository.cs:115`](../QLKhachSan/DAL/HotelRepository.cs#L115): `Invoices i JOIN Stays s ON s.Id=i.StayId WHERE s.CustomerId=@p0`. | `MãHóaĐơn` = `i.Id`; `Phòng` = `i.RoomNumber`; `NgàyLập` = `i.Issued`; `TổngTiền` = `Invoice.Total` (`RoomCharge+ServiceCharge`); `HìnhThức` = `i.Method`. GUI điền bảng ở [`GUI:135`](../QLKhachSan/GUI/ucDashboard.Reports.cs#L135). Tối đa 200 hóa đơn mới nhất. |

Ví dụ người đọc hỏi: “Dòng dịch vụ này thuộc khách nào?” Câu trả lời có thể chỉ thẳng `ServiceOrders.StayId → Stays.Id → Stays.CustomerId → Customers.Id`, hàm DAL `CustomerOrdersAsync` ở dòng 112, hàm BLL cùng tên ở dòng 23 và nơi hiển thị ở `ucDashboard.Reports.cs:130`. [Mục 13](#13-giải-thích-cửa-sổ-hồ-sơ-khách-hàng-trong-ảnh) giải thích thêm quá trình tạo những hàng gốc này.

### 6.2 Hóa đơn / Doanh thu: mỗi số lấy từ đâu?

Nút **XEM BÁO CÁO** gọi [`LoadReport()`](../QLKhachSan/GUI/ucDashboard.Reports.cs#L219) với `day.Value` và `through.Value` → [`HotelService.PeriodReportAsync`](../QLKhachSan/BLL/HotelService.Management.cs#L42). BLL lấy trọn ngày cuối bằng cách đổi nó thành đầu ngày kế tiếp, rồi gọi ba hàm DAL: [`InvoicesAsync`](../QLKhachSan/DAL/HotelRepository.cs#L114), [`RevenueAsync`](../QLKhachSan/DAL/HotelRepository.cs#L110), [`PaymentsAsync`](../QLKhachSan/DAL/HotelTransaction.Management.cs#L46). Ba danh sách nằm trong `PeriodReport`: hóa đơn đã chốt, doanh thu theo hạng mục, và từng lần thu/hoàn. GUI gán chúng cho bảng ở [`ucDashboard.Reports.cs:224–249`](../QLKhachSan/GUI/ucDashboard.Reports.cs#L224).

| Thẻ số ở đầu báo cáo | Công thức chính xác trong [`LoadReport`](../QLKhachSan/GUI/ucDashboard.Reports.cs#L235) | Bảng gốc |
| --- | --- | --- |
| **Doanh thu hóa đơn** | Tổng `Invoice.Total = Invoice.RoomCharge + Invoice.ServiceCharge` của các hóa đơn trong khoảng ngày | [`Invoices`](../QLKhachSan/DAL/HotelRepository.cs#L114); mỗi hóa đơn được tạo khi checkout. |
| **Thu cọc** | Tổng `PaymentEntry.Amount` với `Kind='Deposit'` | `Payments`; các lần đặt/thu cọc bổ sung đã ghi từ trước. |
| **Hoàn cọc** | Tổng `PaymentEntry.Amount` với `Kind='Refund'` | `Payments`; đây là tiền trả ra. |
| **Cọc không hoàn** | Tổng `PaymentEntry.Amount` với `Kind='Forfeit'` | `Payments`; ghi nhận khoản cọc cũ bị mất, **không phải một lần thu mới**. |
| **Dòng tiền thuần** | Tổng `PaymentEntry.CashFlow`: Deposit/Checkout là dương, Refund là âm, Forfeit bằng 0 | `Payments`; công thức ở [`DTO/HotelModels.cs:46`](../QLKhachSan/DTO/HotelModels.cs#L46). |

| Bảng/tab trong báo cáo | Từng cột hiện ra và nguồn | Liên kết và vị trí mã |
| --- | --- | --- |
| **Hóa đơn** | `Hóa đơn` = `Invoices.Id`; `Phòng` = `Invoices.RoomNumber`; `Khách` = `Invoices.GuestName`; `Lập lúc` = `Invoices.Issued`; `Tổng tiền` = `RoomCharge+ServiceCharge` tính ở [`Invoice.Total`](../QLKhachSan/DTO/HotelModels.cs#L32); `Thu thêm` = `Invoices.Collected`; `Hình thức` = `Invoices.Method`. | [`InvoicesAsync`](../QLKhachSan/DAL/HotelRepository.cs#L114) đọc `Invoices` trực tiếp theo `Issued`; GUI đổi tên/ẩn cột ở [`ucDashboard.Reports.cs:226–234`](../QLKhachSan/GUI/ucDashboard.Reports.cs#L226). Tên khách và số phòng là bản chụp lưu trên hóa đơn lúc checkout, nên danh sách này không cần nối lại `Customers`/`Rooms`. `Invoices.StayId → Stays.Id` dùng khi xem chi tiết. |
| **Thu chi** | `Thời điểm` = `Payments.Created`; `Khách` = `Stays.GuestName`; `Loại` = `Payments.Kind` được GUI dịch thành chữ; `Số tiền` = `Payments.Amount`; `Thu − chi` = `PaymentEntry.CashFlow`; `Hình thức` = `Payments.Method`. Chọn hàng còn thấy `Payments.Note` và `Users.Username` trong dòng chi tiết. | [`PaymentsAsync`](../QLKhachSan/DAL/HotelTransaction.Management.cs#L46): `Payments p JOIN Stays s ON s.Id=p.StayId JOIN Users u ON u.Id=p.CreatedBy`. GUI điền/định dạng ở [`ucDashboard.Reports.cs:240–247`](../QLKhachSan/GUI/ucDashboard.Reports.cs#L240), phần chi tiết ở dòng 207–211. |
| **Theo hạng mục** | `HạngMục` và `DoanhThu` là `RevenueItem.Category/Total` được GUI đổi tên ở [`ucDashboard.Reports.cs:248`](../QLKhachSan/GUI/ucDashboard.Reports.cs#L248). | [`RevenueAsync`](../QLKhachSan/DAL/HotelRepository.cs#L110) gộp bằng `UNION ALL`: `Invoices.RoomCharge` thành “Tiền phòng”; `ServiceOrders.Category` với `Price×Quantity` **nối `Invoices` bằng `i.StayId=o.StayId`** để chỉ tính lượt đã xuất hóa đơn, bỏ món hủy; `Payments.Amount` của `Forfeit` thành “Cọc không hoàn”. Sau đó `GROUP BY Category`. |

**Phân biệt hai câu hỏi:** “Hóa đơn đã ghi nhận bao nhiêu?” đọc `Invoices.RoomCharge + ServiceCharge`. “Tiền thực vào quỹ bao nhiêu?” đọc các dòng `Payments` và trừ `Refund`; không cộng `Forfeit` lần nữa. Hóa đơn được lọc theo `Invoices.Issued`, còn giao dịch thu/hoàn được lọc theo `Payments.Created`, nên hai tổng của cùng một ngày có thể khác nhau. Bấm **XUẤT HÓA ĐƠN CSV** hoặc **XUẤT THU CHI CSV** chỉ chuyển `currentReport` đang hiển thị thành tệp (`GUI/ucDashboard.Reports.cs:255–256` → `GUI/ucDashboard.Export.cs:33,41`), không truy vấn lại SQL. Bấm **XEM / IN HÓA ĐƠN** lấy `invoice.StayId` từ hàng đang chọn → `InvoiceStayAsync`/`InvoiceOrdersAsync` → `Stays` và `ServiceOrders` để in phiếu, không tạo hóa đơn mới (`GUI/ucDashboard.Reports.cs:257–264`).

### 6.3 Tài khoản và nhân viên: bảng nào quyết định quyền?

Tên/vai trò trên đầu màn **Tài khoản cá nhân** lấy từ `UserSession.Username/Role` mà [`AuthService.LoginAsync`](../QLKhachSan/BLL/AuthService.cs#L33) đã tạo khi đăng nhập, không phải `SELECT` mới mỗi lần vẽ nhãn ([`GUI:278`](../QLKhachSan/GUI/ucDashboard.Reports.cs#L278)). Khi thực sự thao tác, [`RequireUserAsync`](../QLKhachSan/DAL/HotelRepository.cs#L77) vẫn đối chiếu phiên với hàng `Users` còn hoạt động. Danh sách vai trò trong ô chọn lấy từ [`RolePolicy.Roles`](../QLKhachSan/BLL/RolePolicy.cs#L5), còn vai trò đã lưu của từng tài khoản là `Users.Role`. Danh sách tài khoản đọc trực tiếp **một bảng `Users`**, không nối với hồ sơ khách `Customers`.

| Danh sách / cột | Chuỗi hàm đọc và bảng SQL | Điều người đọc cần hiểu |
| --- | --- | --- |
| **Nhân viên và phân quyền** của Admin | [`LoadRows()`](../QLKhachSan/GUI/ucDashboard.Reports.cs#L334) → [`auth.UsersAsync(user)`](../QLKhachSan/BLL/AuthService.Management.cs#L14) → [`db.UsersAsync()`](../QLKhachSan/DAL/HotelTransaction.Management.cs#L47) → `SELECT Id,Username,Role,Active,LockedUntil,SecurityVersion FROM Users WHERE Archived=0 ORDER BY Username`. | `Tên đăng nhập` = `Users.Username`; `Vai trò` = `Users.Role`; `Đang hoạt động` = `Users.Active`; `Khóa đến` = `Users.LockedUntil`. `Id` và `Version` vẫn ở DTO `UserInfo` nhưng GUI ẩn; `UserInfo.Version` nhận từ `Users.SecurityVersion`, dùng phát hiện hàng đã bị sửa. Chọn một hàng chỉ đổi ô vai trò/checkbox trên màn ([`GUI:326–343`](../QLKhachSan/GUI/ucDashboard.Reports.cs#L326)); chưa lưu SQL. |
| **Nhân viên khách sạn** chỉ xem | [`ShowEmployees.LoadRows()`](../QLKhachSan/GUI/ucDashboard.RoleFunctions.cs#L12) → [`auth.EmployeesAsync(user)`](../QLKhachSan/BLL/AuthService.Management.cs#L8) → cùng `db.UsersAsync()` ở dòng 47. | BLL lọc bỏ tài khoản có `Role='Admin'`; GUI hiển thị `Username`, `Role`, `Active`, `LockedUntil`, ẩn `Id`/`Version`. Nút **LÀM MỚI** chạy lại cùng đường đọc; không ghi bảng. |
| **Danh sách đặt lại mật khẩu** | [`ShowResetPassword.LoadRows()`](../QLKhachSan/GUI/ucDashboard.Reports.cs#L423) → `auth.UsersAsync` → `db.UsersAsync`. | GUI lọc bỏ tài khoản đang sử dụng bằng `Id != user.Id` ở [`GUI:425`](../QLKhachSan/GUI/ucDashboard.Reports.cs#L425). Nhãn bên phải lấy `Username/Role` của hàng đã chọn. |

| Nút | Dữ liệu người dùng nhập → hàm BLL → SQL được lưu | Kiểm tra và kết quả |
| --- | --- | --- |
| **Đổi mật khẩu của tôi** | `oldPassword.Text`, `password.Text` ở [`GUI:290–300`](../QLKhachSan/GUI/ucDashboard.Reports.cs#L290) → [`AuthService.ChangePasswordAsync`](../QLKhachSan/BLL/AuthService.cs#L67) → [`AccountAsync`](../QLKhachSan/DAL/HotelRepository.cs#L83) đọc băm cũ từ `Users` → [`ChangePasswordAsync`](../QLKhachSan/DAL/HotelRepository.cs#L91) `UPDATE Users.PasswordHash/Salt/Iterations`, tăng `SecurityVersion`. | GUI so hai ô mật khẩu mới; BLL kiểm mật khẩu cũ; ghi `AuditLog`. Phiên của chính người đổi được tăng phiên bản sau khi lưu. |
| **Lưu quyền / trạng thái** | `UserInfo.Id/Version` của hàng chọn, `editRole.SelectedItem`, `active.Checked` ở [`GUI:346–350`](../QLKhachSan/GUI/ucDashboard.Reports.cs#L346) → [`AuthService.UpdateUserAsync`](../QLKhachSan/BLL/AuthService.Management.cs#L18) → [`db.UpdateUserAsync`](../QLKhachSan/DAL/HotelTransaction.Management.cs#L50): `UPDATE Users SET Role,Active,SecurityVersion=SecurityVersion+1`. | BLL đọc lại `Users`, so `Version`, không cho tự sửa hoặc vô hiệu Admin cuối; ghi `AuditLog`; GUI gọi lại `LoadRows()` để hiện dữ liệu mới. `SecurityVersion` đổi làm phiên cũ mất hiệu lực. |
| **Đặt lại mật khẩu nhân viên** | Mật khẩu mới và `UserInfo.Id` ở [`GUI:353–366`](../QLKhachSan/GUI/ucDashboard.Reports.cs#L353) hoặc [`GUI:435–444`](../QLKhachSan/GUI/ucDashboard.Reports.cs#L435) → [`AuthService.ResetPasswordAsync`](../QLKhachSan/BLL/AuthService.Management.cs#L29) → DAL `ChangePasswordAsync` ở dòng 91. | Chỉ Admin; BLL so `Version` và cấm tự đặt lại qua màn nhân viên. Lưu băm/salt mới, tăng `SecurityVersion`, ghi `AuditLog`; nhân viên cần đăng nhập lại. |
| **Đăng ký tài khoản** | `name.Text`, `password.Text`, `role.SelectedItem` ở [`GUI:374–385`](../QLKhachSan/GUI/ucDashboard.Reports.cs#L374) → [`AuthService.CreateUserAsync`](../QLKhachSan/BLL/AuthService.cs#L54) → [`db.CreateUserAsync`](../QLKhachSan/DAL/HotelRepository.cs#L86): `INSERT Users(Username,PasswordHash,Salt,Iterations,Role)`. | BLL yêu cầu Admin, kiểm tên/vai trò và `AccountAsync` tìm trùng; ghi `AuditLog`. Danh sách Admin chỉ thấy tài khoản mới sau khi mở hoặc tải lại màn. |

Ví dụ người đọc hỏi: “Ô **Đang hoạt động** của nhân viên này do đâu?” Câu trả lời là `Users.Active` → `HotelTransaction.UsersAsync` (`DAL/HotelTransaction.Management.cs:47`) → `AuthService.UsersAsync` (`BLL/AuthService.Management.cs:14`) → `ShowAccounts.LoadRows` (`GUI/ucDashboard.Reports.cs:334–340`) → cột `Đang hoạt động`. Nút **Lưu quyền / trạng thái** đi chiều ngược lại và `UPDATE Users.Active` ở `DAL/HotelTransaction.Management.cs:50`. Như vậy có thể chỉ rõ **bảng gốc, mã nối, hàm đọc, hàm ghi và vị trí hiển thị** cho câu hỏi của khách hàng.

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

**Các bảng bổ sung ở V6** được tạo bởi [`Database/MigrateV6.sql`](../Database/MigrateV6.sql). `AccountingPeriods` lưu tháng đã khóa; `CashShifts` lưu ca và số đếm; `Payments` và `ServiceOrders` có thêm `ShiftId`, `Payments` có thêm `ExternalReference`. `FinanceOpeningBalances`, `FinanceVouchers`, `BankStatementLines` phục vụ sổ quỹ và sao kê. `FinanceDebts` cùng `DebtAllocations` lưu nợ và các đợt gạch nợ. `StockItems`, `StockMovements`, `HousekeepingConsumption` lưu tồn, giá vốn và báo tiêu thụ. `InvoiceFinance`, `InvoiceAdjustments`, `InvoiceVoids`, `BillGroups`, `BillShares` lưu thông tin hóa đơn và phân bổ bill. Các trigger trong migration chặn sửa chứng từ thuộc ca hoặc kỳ đã khóa.

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
- **Nâng cấp schema:** `Database/Setup.sql:18-80` tạo bản 1. `MigrateV2.sql` bổ sung kiểm soát lịch/dịch vụ, `MigrateV3.sql` thêm vai trò và lưu trữ tài khoản cũ, `MigrateV4.sql` thêm phòng, `MigrateV5.sql` chuyển hạng VIP, `MigrateV6.sql` thêm kế toán. `DAL/SchemaMigrator.cs` chạy tuần tự phần còn thiếu khi login. Lỗi SQL khi nâng cấp được bọc trong `SchemaMigrationException` và hiện phiên bản, mã, dòng lỗi trên login. Bản cài trống có 50 phòng từ `Setup.sql:94-108`, thêm 10 ở V4 thành 60 phòng nếu không bị sửa/xóa.
- **Xuất CSV:** `GUI/ucDashboard.Export.cs:11,17,33,41,49` tạo file UTF-8 có BOM để Excel đọc tiếng Việt, đồng thời chặn ô bắt đầu bằng ký tự công thức. Không xóa hay thay đổi bản ghi SQL.
- **Xuất Excel:** `GUI/Ui.cs` gắn menu xuất `.xlsx` cho các `DataGridView` tạo qua `Ui.Grid()`. `GUI/ExcelExport.cs` ghi file với tiêu đề, ngày lập và kiểu số tiền `#,##0 VNĐ`; màn hình kế toán có thêm nút **Xuất Excel**.
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

`ShowBooking(reserve,selected)` mở form (`GUI/ucDashboard.Actions.cs:56-60`). Nút **Đặt trước / Giữ chỗ** gọi nó với `reserve=true` (`GUI/ucDashboard.cs:309`); nhận ngay dùng `false`. Danh sách phòng ban đầu lấy từ `data.Rooms`, danh sách lịch lấy từ `data.Stays` để lọc sơ bộ (`:58,76-84`). `data` là `DashboardData` vừa được `Reload()` lấy qua `HotelService.DashboardAsync()` → `RoomsAsync()` đọc `dbo.Rooms` và `ActiveStaysAsync()` đọc `dbo.Stays` (`GUI/ucDashboard.cs:110-118`; `BLL/HotelService.cs:14-20`; `DAL/HotelRepository.cs:94,98`). Người dùng nhìn thấy tên/số phòng từ `Room.Number`, loại/giá/cọc từ cùng hàng `Rooms`; chưa có câu SQL mới chỉ vì mở danh sách.

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

#### 10.3.1 Nguồn và ý nghĩa của các ô trên form đặt trước

| Ô/người dùng thấy | Giá trị ban đầu hoặc lúc thay đổi lấy từ đâu? | Có được lưu ngay khi sửa ô không? |
| --- | --- | --- |
| **Chọn phòng** | [`ShowBooking:58`](../QLKhachSan/GUI/ucDashboard.Actions.cs#L58) lấy `data.Rooms` (SQL `Rooms`) và bỏ phòng `BaoTri`. Khi đổi ngày đến/số ngày, `FilterRooms()` ở [`GUI:76–84`](../QLKhachSan/GUI/ucDashboard.Actions.cs#L76) dùng `data.Stays` (SQL `Stays`) để ẩn phòng có lượt giao thời gian; phòng đang có khách vẫn có thể hiện nếu khoảng ngày tới không giao lượt cũ. Phòng được chọn mang `Room.Id`, `Version`, `Status`, `Deposit`. | Chưa. Đây là lọc trong RAM; lúc bấm lưu BLL đọc SQL mới nhất và kiểm tra lại lịch. |
| **Họ tên, SĐT, CCCD/Hộ chiếu** | Người dùng nhập vào `name`, `phone`, `identity` ở [`GUI:62,86`](../QLKhachSan/GUI/ucDashboard.Actions.cs#L62); form không tự điền từ `Customers`. | Chưa. Khi lưu, `ValidateGuest()` sửa khoảng trắng/chuẩn hóa giấy tờ và kiểm định dạng (`BLL/HotelService.cs:27–34`). |
| **Ngày giờ dự kiến đến** và **số ngày thuê** | `arrival` khởi tạo từ `ServerNow.AddHours(2)`, `days` mặc định 1 và tối đa 60 ở [`GUI:63`](../QLKhachSan/GUI/ucDashboard.Actions.cs#L63). `ServerNow` lấy từ `SELECT SYSDATETIME()` của SQL Server khi dashboard tải (`DAL/HotelRepository.cs:76`). Ngày trả được tính `arrival.AddDays(days)`. | Chưa. BLL kiểm ngày đến không ở quá khứ và số ngày 1–60; lúc lưu mới vào `Stays.Arrival/Departure`. |
| **Đã thu tiền cọc / Số tiền thực thu** | `Room.Deposit` từ `Rooms.Deposit` chỉ là **số gợi ý**, được điền vào ô tiền khi chọn phòng ([`GUI:73–75`](../QLKhachSan/GUI/ucDashboard.Actions.cs#L73)). Checkbox xác nhận đã thu thực tế. Nếu không chọn, BLL dùng cọc `0` dù ô còn hiển thị gợi ý; nếu chọn thì số thực thu phải lớn hơn 0. | Chưa. Khi lưu có cọc, `Stays.Deposit` giữ tổng cọc và `Payments` giữ **dòng thu tiền**. QR không xác nhận ngân hàng đã chuyển tiền. |
| **Hạn cuối nhận phòng** | [`UpdateHold()`](../QLKhachSan/GUI/ucDashboard.Actions.cs#L68) gợi ý tối đa 24 giờ từ `ServerNow` khi chưa cọc, 15 ngày khi đã có số cọc dương; nếu mốc ấy vượt ngày trả dự kiến, form gợi ý trước ngày trả một phút. Người dùng vẫn có thể chỉnh `receiveBy.Value`. | Chưa. BLL dùng **giờ SQL mới tại lúc bấm lưu**, yêu cầu `Arrival ≤ HoldUntil < Departure` và không vượt mốc 24 giờ/15 ngày từ lúc tạo. |
| **Hình thức thu cọc / QR** | Danh sách “Tiền mặt”, “Chuyển khoản” là mảng cố định ở [`GUI:85`](../QLKhachSan/GUI/ucDashboard.Actions.cs#L85). [`PaymentQr.Add`](../QLKhachSan/GUI/PaymentQr.cs#L7) đọc tài khoản ngân hàng từ `appsettings.json`, hiển thị số tiền và nội dung có SĐT; ảnh QR lấy qua VietQR nếu có cấu hình. | Đổi lựa chọn/nhìn QR chưa lưu gì. Sau khi nhân viên xác nhận đã thu và bấm lưu, `Payments.Method` mới nhận phương thức. |

**Lọc trên form không phải bảo đảm cuối cùng.** `FilterRooms()` chỉ xem bản `data.Stays` đang có trong RAM. Nếu máy khác vừa nhận hoặc đặt phòng, danh sách trên form có thể cũ. [`HotelService.CreateStayAsync`](../QLKhachSan/BLL/HotelService.cs#L48) chạy trong [`HotelRepository.RunAsync`](../QLKhachSan/DAL/HotelRepository.cs#L12), kiểm lại quyền phiên, `Rooms.Version/Status`, giờ SQL và lịch phòng trước khi ghi. [`EnsureAvailableAsync`](../QLKhachSan/DAL/HotelTransaction.Management.cs#L13) hỏi `Stays` có hàng `RoomId` đang hoạt động với `COALESCE(CheckIn,Arrival) < ngày trả mới` **và** `Departure > ngày đến mới` không. Đây là hai khoảng ngày giao nhau; hai lượt liền kề không giao nên có thể đặt.

Sau khi nhận tham số, `BLL/HotelService.cs:48-71` gọi `ValidateGuest` (`:27-34`), kiểm tra phương thức (`:35-38`), thời lượng 1–60 ngày, tiền cọc nguyên đồng, trạng thái phòng và hạn giữ. `Write()` kiểm vai trò Admin/Reception và `RequireUserAsync()` đối chiếu phiên với `Users` (`BLL/HotelService.cs:12`; `DAL/HotelRepository.cs:77-80`). BLL **lấy phòng mới nhất** bằng `RoomAsync(id)` (`DAL/HotelRepository.cs:95`), **lấy giờ SQL** bằng `NowAsync()` (`:76`) và kiểm tra lịch bằng `EnsureAvailableAsync` (`DAL/HotelTransaction.Management.cs:13-17`). Nếu dữ liệu thay đổi hoặc điều kiện sai, không có lượt đặt được lưu nửa chừng.

Khi mọi điều kiện đúng, `db.CreateStayAsync(...)` ghi `Customers` và `Stays` (`DAL/HotelTransaction.Commands.cs:7-11`). Nhận ngay còn đổi `Rooms.Status` sang `DangO` và thêm `StaySegments`; đặt trước giữ nguyên trạng thái vật lý phòng (`BLL/HotelService.cs:66-68`). Nếu cọc lớn hơn 0, `PaymentAsync` thêm `Payments.Kind='Deposit'`; số 0 không thêm dòng (`DAL/HotelTransaction.Commands.cs:27-28`). Cuối cùng ghi `AuditLog` rồi `Changed(...)` tải dashboard lại (`GUI/ucDashboard.cs:120-123`).

#### 10.3.2 Sau khi bấm LƯU ĐẶT PHÒNG, cột nào vào bảng nào?

| Bảng/giá trị | Lệnh và cột nhận dữ liệu | Mã liên kết để đọc lại |
| --- | --- | --- |
| `Customers` | [`HotelTransaction.CreateStayAsync:9`](../QLKhachSan/DAL/HotelTransaction.Commands.cs#L9) tìm `IdentityNumber` trùng: có thì `UPDATE Name,Phone`, chưa có thì `INSERT Name,Phone,IdentityNumber`. Vì vậy một người đặt nhiều lần vẫn dùng cùng hồ sơ theo giấy tờ. | `Customers.Id` được tìm bằng `IdentityNumber`, rồi ghi vào `Stays.CustomerId`. |
| `Stays` | [`INSERT ... OUTPUT INSERTED.Id`](../QLKhachSan/DAL/HotelTransaction.Commands.cs#L10) lưu `RoomId`, `CustomerId`, bản chụp `GuestName/Phone/IdentityNumber`, `Status='Reserved'`, `IsActive=1`, `Created` bằng giờ SQL, `Arrival`, `Departure=Arrival+days`, `CheckIn=NULL`, `HoldUntil`, `Deposit`, `CreatedBy`. `OUTPUT` trả mã lượt mới về BLL. | `Stays.RoomId → Rooms.Id`; `Stays.CustomerId → Customers.Id`; `Stays.CreatedBy → Users.Id`. Mã lượt `Stays.Id` là chìa khóa cho cọc, dịch vụ và hóa đơn sau này. |
| `Rooms` | [`SetRoomAsync`](../QLKhachSan/DAL/HotelTransaction.Commands.cs#L13) `UPDATE Rooms.Version=Version+1` và giữ `Status` hiện tại với đặt trước. | Phòng **không tự thành Đang ở** chỉ vì đã có lịch. `RoomId` nối lượt với phòng; `Version` báo form khác đã cũ. |
| `Payments` và `AuditLog` | Nếu cọc >0, [`PaymentAsync`](../QLKhachSan/DAL/HotelTransaction.Commands.cs#L27) thêm `StayId`, `Kind='Deposit'`, `Amount`, `Created`, `Method`, `Note`, `CreatedBy`. Không cọc thì không thêm dòng `Payments`. [`AuditAsync`](../QLKhachSan/DAL/HotelRepository.cs#L92) thêm hành động `Reserve`. | `Payments.StayId → Stays.Id`; `Payments.CreatedBy`/`AuditLog.UserId → Users.Id`. Số `Stays.Deposit` là tổng đang giữ, còn `Payments` giải thích từng lần tiền vào. |
| Chưa có `StaySegments`, `ServiceOrders`, `Invoices` | Đặt trước **chưa phải nhận phòng**, chưa ở thực tế, chưa gọi món và chưa checkout. | `StaySegments` chỉ xuất hiện lúc nhận phòng; `Invoices` xuất hiện sau khi hoàn tất checkout. |

Tất cả lệnh trên nằm trong **một giao dịch** ở `HotelRepository.RunAsync` (`DAL/HotelRepository.cs:12-30`): nếu thêm `Payments` hoặc đổi `Rooms.Version` lỗi, thao tác rollback cả `Customers` và `Stays`. Sau khi commit, `Changed()` gọi `Reload()` → `DashboardAsync()` đọc lại `Rooms`/`Stays`. [`RenderBookings()`](../QLKhachSan/GUI/ucDashboard.cs#L198) chỉ lấy `Stays.Status='Reserved'`, sắp theo ngày đến; cột `Phòng` lấy `Stays.RoomId → Rooms.Id → Rooms.Number`, còn `Khách`, `SĐT`, `CCCD`, `Ngày đến`, `Hạn giữ`, `Cọc` lấy từ các cột của `Stays`. `Tình trạng` do GUI so `Stays.HoldUntil` với giờ SQL; dấu `*` ở ô phòng là số lượt Reserved của phòng đó ([`ucDashboard.cs:170`](../QLKhachSan/GUI/ucDashboard.cs#L170)).

Lượt đặt không có hóa đơn ngay. Khi khách đến, nút **Nhận phòng** ở [`ucDashboard.cs:317–324`](../QLKhachSan/GUI/ucDashboard.cs#L317) gọi `CheckInAsync`: lúc ấy `Stays.Status` đổi thành `Occupied`, `Rooms.Status` thành `DangO`, và `StaySegments` bắt đầu ghi giờ/giá ở thực tế (`BLL/HotelService.cs:72-84`). Nếu `HoldUntil` qua giờ SQL mà chưa nhận, `ExpireReservationsAsync` hủy lượt, ghi `Payments.Forfeit` khi có cọc và `AuditLog.NoShow` (`BLL/HotelService.cs:99-111`). Hủy trước hạn thì có thể hoàn cọc theo [`CancelAsync`](../QLKhachSan/BLL/HotelService.cs#L85).

**Ví dụ cụ thể:** chọn phòng 101, nhập khách An, 2 ngày và cọc 200.000đ. `Room` phòng 101 và `GuestInput("An", số điện thoại, giấy tờ)` đi vào `CreateStayAsync`. BLL kiểm tra lịch 2 ngày, DAL lưu một `Stays` mới và một `Payments` cọc 200.000đ. Khi tải lại, danh sách đặt có An và ô phòng 101 thêm dấu `*`; phòng vẫn có thể hiện “Trống” cho đến lúc nhận khách.

<a id="sua-thong-tin-khach"></a>

#### 10.3.3 Khách báo sai thông tin: sửa ở đâu?

Sau khi đã bấm **LƯU ĐẶT PHÒNG**, không mở lại form tạo mới để nhập thêm một lượt. Trên dashboard, tìm **bảng đặt trước** phía dưới sơ đồ phòng, tìm đúng dòng theo phòng, tên hoặc SĐT, rồi bấm **Sửa thông tin khách** ngay trên dòng đó ([`RenderBookings`](../QLKhachSan/GUI/ucDashboard.cs#L198) tạo nút ở dòng 202). [`dgvDatCoc_CellContentClick`](../QLKhachSan/GUI/ucDashboard.cs#L317) lấy `BookingRow.Id` → tìm `Stay` có cùng `Stays.Id` trong `data.Stays` → gọi [`ShowEditBooking(stay)`](../QLKhachSan/GUI/ucDashboard.Management.cs#L43). Admin và Reception thấy nút; vai trò chỉ xem không có nút này.

| Trong form **Sửa lịch đặt / thông tin khách** | Giá trị được điền ban đầu | Khi bấm **LƯU THAY ĐỔI** sẽ đi đâu? |
| --- | --- | --- |
| **Họ tên** | `stay.Guest` từ `Stays.GuestName` ở [`GUI:48`](../QLKhachSan/GUI/ucDashboard.Management.cs#L48). | `name.Text` đi vào `GuestInput.Name` → `ValidateGuest` → `Customers.Name` của hồ sơ theo giấy tờ đã nhập **và** `Stays.GuestName` của lượt đang sửa. |
| **SĐT** | `stay.Phone` từ `Stays.Phone` ở cùng dòng 48. | `phone.Text` → `GuestInput.Phone` → `Customers.Phone` và `Stays.Phone`; không tự đổi SĐT của mọi lượt cũ khác. |
| **CCCD / Hộ chiếu** | `stay.Identity` từ `Stays.IdentityNumber` ở cùng dòng 48. | `identity.Text` → `GuestInput.Identity` → tìm/tạo `Customers` theo `IdentityNumber`; `Stays.IdentityNumber` và `Stays.CustomerId` được cập nhật theo giấy tờ mới. |
| **Phòng, ngày đến, số ngày, hạn cuối nhận** | `Stays.RoomId → Rooms.Id`, `Stays.Arrival`, `Stays.Departure−Arrival`, `Stays.HoldUntil` ở [`GUI:46–50`](../QLKhachSan/GUI/ucDashboard.Management.cs#L46). | Có thể **giữ nguyên** khi chỉ sửa khách. Nếu đổi lịch/phòng, BLL vẫn kiểm tra phòng và khoảng ngày không trùng trước khi cập nhật `Stays`. |
| **Lý do thay đổi** | Ô trống; nhân viên tự ghi vì sao sửa, ví dụ “Khách cung cấp sai SĐT lúc đặt”. | [`Reason`](../QLKhachSan/BLL/HotelService.Management.cs#L12) yêu cầu 3–300 ký tự; nội dung được ghi vào `AuditLog.Detail` qua [`AuditAsync`](../QLKhachSan/DAL/HotelRepository.cs#L92). |
| **Cọc đã thu** | Chỉ hiện trong dòng chú thích từ `Stays.Deposit` ([`GUI:52`](../QLKhachSan/GUI/ucDashboard.Management.cs#L52)). | Form sửa khách **không đổi cọc** và không thêm `Payments`; muốn thu thêm dùng **Thu cọc bổ sung**. |

Đường lưu đầy đủ: nút ở [`GUI:53–56`](../QLKhachSan/GUI/ucDashboard.Management.cs#L53) → [`HotelService.UpdateBookingAsync`](../QLKhachSan/BLL/HotelService.Management.cs#L27) → [`HotelTransaction.UpdateBookingAsync`](../QLKhachSan/DAL/HotelTransaction.Management.cs#L38). BLL đọc lại `Stays` và `Rooms`, so `Version`, kiểm lượt còn `Reserved`, chưa quá `HoldUntil`, xác thực khách/lý do và lịch phòng. Nếu giờ đến dự kiến **đã qua nhưng hạn nhận chưa hết**, nhân viên vẫn sửa được họ tên/SĐT/giấy tờ khi **giữ nguyên** giờ đến cũ; nếu đổi giờ đến, phải chọn giờ chưa qua. DAL dòng 40 tìm hoặc thêm `Customers` theo giấy tờ mới; dòng 41 `UPDATE` đúng `Stays.Id` đang chọn, chép lại tên/SĐT/giấy tờ và đổi `CustomerId` tới hồ sơ vừa tìm. Cả thao tác nằm trong một transaction; thành công thì ghi `AuditLog` và dashboard đọc lại hàng đã sửa.

**Ví dụ sửa sai CCCD:** lượt `Stays.Id=21` đang trỏ `Customers.Id=7` theo giấy tờ nhập nhầm. Nhân viên sửa CCCD trên **chính dòng đặt trước đó** và ghi lý do. Nếu giấy tờ đúng đã có trong `Customers`, `Stays.CustomerId` chuyển sang ID của hồ sơ ấy; nếu chưa có, SQL tạo hồ sơ mới rồi gán ID mới. `Stays.GuestName/Phone/IdentityNumber` cũng được sửa cho lượt 21. Hàng `Customers.Id=7` cũ **không bị xóa**; những lượt khác từng trỏ tới nó không tự đổi. Vì thế màn [Hồ sơ khách hàng](#13-giải-thích-cửa-sổ-hồ-sơ-khách-hàng-trong-ảnh) có thể còn một hồ sơ cũ không gắn lượt nào. Đây là điều cần giải thích khi người đọc hỏi “vì sao sau khi sửa giấy tờ lại thấy hai hồ sơ?”.

**Giới hạn hiện tại:** nút này chỉ áp dụng cho lượt `Reserved` còn hạn nhận. Sau khi đã nhận phòng (`Occupied`), hủy hoặc thanh toán, `UpdateBookingAsync` từ chối; màn **Hồ sơ khách hàng** chỉ tra cứu, không có nút sửa trực tiếp. Đừng tạo lượt mới chỉ để sửa thông tin của một lượt đang chờ nhận.

### 10.4 Nhận lượt đã đặt, sửa, hủy và thu cọc

| Việc người dùng làm | Dữ liệu GUI lấy và truyền vào BLL | BLL đọc/kiểm tra | DAL ghi và kết quả |
| --- | --- | --- | --- |
| **Nhận phòng** ở bảng đặt/lịch | Dòng đã chọn có `BookingRow.Id` hoặc `ScheduleRow.Id`; GUI tìm `Stay` tương ứng trong `data.Stays`, gọi `CheckInAsync(stay)` (`GUI/ucDashboard.cs:317-324,236-249`). | `BLL/HotelService.cs:72-84` đọc lại `Stays`, `Rooms`, giờ SQL và lịch `Stays`; kiểm tra lượt còn Reserved, chưa quá hạn, phòng `Trong`, không trùng lịch. | `DAL/HotelTransaction.Commands.cs:20` sửa `Stays` sang Occupied, `:18` thêm `StaySegments`, `:13` sửa `Rooms` sang Đang ở; tải lại để bảng/lịch thay đổi. |
| **Sửa thông tin khách** | `ShowEditBooking(stay)` điền form từ `Stay` cũ; `GUI/ucDashboard.Management.cs:46-56` truyền `stay`, `target` từ phòng được chọn, `GuestInput` từ ba ô, `arrival`, `days`, `receiveBy`, `reason` vào `UpdateBookingAsync`. | `BLL/HotelService.Management.cs:27-40` đọc `Stays`, `Rooms`, giờ SQL; kiểm tra quyền, hạn, lý do 3–300 ký tự, phòng phù hợp và lịch trống. | `DAL/HotelTransaction.Management.cs:38-41` cập nhật `Customers` theo giấy tờ và `Stays` theo dữ liệu mới; `Rooms.Version` tăng ở phòng mới và phòng cũ nếu chuyển (`BLL/HotelService.Management.cs:38-39`); `AuditLog` lưu lý do; tải lại form/bảng. |
| **Hủy đặt** | `ShowCancel(stay)` trước tiên gọi `RefundQuoteAsync(stay)` để lấy tiền được hoàn từ `Stays.Deposit` và giờ SQL (`GUI/ucDashboard.Actions.cs:102-109`; `BLL/HotelService.Management.cs:19-23`). Sau xác nhận, truyền `stay`, `method.SelectedItem`, `expectedRefund` vào `CancelAsync`. | `BLL/HotelService.cs:85-98` đọc lại `Stays` và giờ SQL. Nếu số tiền hoàn đã đổi vì vừa quá hạn, không dùng bảng tính cũ. | `DAL/HotelTransaction.Commands.cs:23` đóng lượt Cancelled; `:27` ghi Refund trước hạn hoặc Forfeit sau hạn (nếu số tiền >0), ghi `AuditLog`; tải lại, lịch đặt biến mất khỏi danh sách hoạt động. |
| **Thu cọc bổ sung** | Combo `choice` lấy các `Reserved` từ `data.Stays` (`GUI/ucDashboard.Management.cs:15`); GUI truyền `selected.Stay`, `amount.Value`, `method.SelectedItem` (`:36-40`) vào `AddDepositAsync`. | `BLL/HotelService.Management.cs:50-62` đọc lại `Stays`, giờ SQL; kiểm tra số tiền >0, chưa quá hạn. Cọc lần đầu mở giới hạn tối đa đến 15 ngày từ lúc tạo, nhưng `HoldUntil` được chặn trước `Stays.Departure`; nếu đã có cọc thì giữ nguyên hạn. | `DAL/HotelTransaction.Management.cs:37` tăng `Stays.Deposit`, `DAL/HotelTransaction.Commands.cs:27` thêm `Payments.Deposit`; tải lại số cọc trên dashboard. |

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

## 13. Giải thích cửa sổ Hồ sơ khách hàng trong ảnh

Hãy tưởng tượng có **ba cuốn sổ**: sổ **khách** ghi một người là ai, sổ **lượt ở** ghi người đó đến khách sạn lần nào, sổ **hóa đơn** ghi lần ở nào đã chốt bao nhiêu tiền. Cửa sổ trong ảnh ghép ba cuốn sổ ấy để tạo một hàng “khách hàng”. Phần dịch vụ bên dưới dùng thêm cuốn sổ thứ tư là `ServiceOrders`.

```text
Customers.Id ──< Stays.CustomerId
                  │
                  ├── Stays.Id ──< ServiceOrders.StayId
                  └── Stays.Id ─── Invoices.StayId (tối đa một hóa đơn/lượt)
```

Ký hiệu `──<` nghĩa là “một bản ghi bên trái có thể nối tới nhiều bản ghi bên phải”. Ví dụ **một khách** có thể đến ở ba lần; mỗi lần có thể gọi nhiều món. `Customers.Id`, `Stays.Id` là mã số do SQL tạo. `CustomerId`, `StayId` là các mã tham chiếu, giống viết số phiếu lên một tờ giấy để biết nó thuộc ai (`Database/Setup.sql:32-69`).

### 13.1 Dữ liệu được lưu từ khi nào?

1. Khách chưa tự tạo hồ sơ trong màn hình này. Khi lễ tân **Đặt trước** hoặc **Nhận phòng ngay**, GUI gom tên, SĐT và CCCD/hộ chiếu thành `GuestInput` (`GUI/ucDashboard.Actions.cs:62,95-98`). BLL kiểm tra định dạng tại `BLL/HotelService.cs:27-34,48-71`.
2. `DAL/HotelTransaction.Commands.cs:9` tìm `Customers` bằng `IdentityNumber`. Có rồi thì cập nhật `Name`, `Phone`; chưa có thì `INSERT Customers`. `Customers.IdentityNumber` là duy nhất (`Database/Setup.sql:32-34`), nên cùng giấy tờ chỉ có một hồ sơ khách.
3. Ngay sau đó `DAL/HotelTransaction.Commands.cs:10-11` `INSERT Stays`. `Stays.CustomerId` lấy `Customers.Id` vừa tìm; đồng thời `Stays.GuestName`, `Phone`, `IdentityNumber` giữ **thông tin của lượt tại lúc tạo** (`Database/Setup.sql:35-44`). Form **Sửa thông tin khách** có thể chủ động sửa thông tin của lượt Reserved này (`DAL/HotelTransaction.Management.cs:38-41`); thay đổi hồ sơ khách sau đó không tự viết lại mọi lượt ở cũ.
4. Nếu khách gọi món, `DAL/HotelTransaction.Commands.cs:25` `INSERT ServiceOrders` với `StayId` là mã lượt ở và `ServiceId` là mã món trong `Services`. `Category`, `Name`, `Price` cũng được chụp vào dòng gọi món, nên sau này thay giá danh mục không làm thay đổi dòng cũ (`Database/Setup.sql:53-60`).
5. Khi checkout, `DAL/HotelTransaction.Commands.cs:29` `INSERT Invoices` với `StayId` là lượt vừa chốt. Hóa đơn lưu riêng `RoomNumber`, `GuestName`, tiền phòng, tiền dịch vụ, cọc, thu thêm và hoàn tiền tại thời điểm chốt (`Database/Setup.sql:63-69`). Tiền thực thu/hoàn từng lần còn được ghi ở `Payments` (`DAL/HotelTransaction.Commands.cs:27-28`).

Nếu sửa **CCCD/hộ chiếu của một lượt Reserved** thành giấy tờ mới, `UpdateBookingAsync` tìm hoặc tạo một hàng `Customers` cho giấy tờ mới rồi đổi `Stays.CustomerId` sang ID đó (`DAL/HotelTransaction.Management.cs:38-41`). Hàng `Customers` cũ **không bị xóa**. Nếu không còn lượt nào nối tới nó, danh sách khách vẫn có thể hiện hàng cũ với 0 lượt thanh toán vì truy vấn dùng `LEFT JOIN` (`DAL/HotelRepository.cs:111`).

### 13.2 Bấm **Hồ sơ khách hàng** thì chương trình đọc gì?

Nút ngoài dashboard nối đến `btnQuanLyKhach_Click` (`GUI/ucDashboard.Reports.cs:76`). Khi cửa sổ mở, `LoadCustomers()` chạy luôn (`:103-116,146`). Nút **Tìm khách** hoặc phím Enter chạy lại cùng hàm (`:144-145`). Chuỗi đi theo đường:

```text
Ô tìm search.Text
  → GUI LoadCustomers() [ucDashboard.Reports.cs:103-105]
  → BLL HotelService.CustomersAsync(search) [HotelService.cs:21]
  → DAL HotelTransaction.CustomersAsync(search.Trim()) [HotelRepository.cs:111]
  → SQL SELECT từ Customers + Stays + Invoices
  → List<CustomerSummary> [HotelModels.cs:34]
  → grid.DataSource [ucDashboard.Reports.cs:106]
```

Nếu ô tìm trống, SQL lấy tối đa **200 khách** mới nhất theo `Customers.Id`; nếu có chữ, SQL tìm trong `Customers.Name`, `Phone`, `IdentityNumber` (`DAL/HotelRepository.cs:111`). Cửa sổ này **chỉ đọc**, bấm Tìm không `INSERT` hoặc `UPDATE`.

| Cột nhìn thấy trong ảnh | Cột SQL thực sự cung cấp | Cách ghép/tính và nơi hiển thị |
| --- | --- | --- |
| **Họ tên** | `Customers.Name` | SQL chọn `c.Name`; `CustomerSummary.Name` nhận giá trị; GUI đổi tên cột ở `GUI/ucDashboard.Reports.cs:107-112`. |
| **SĐT** | `Customers.Phone` | SQL chọn `c.Phone`; đây là SĐT hồ sơ khách hiện tại. SĐT tại từng lượt cũ còn có trong `Stays.Phone`. |
| **CCCD/Hộ chiếu** | `Customers.IdentityNumber` | SQL chọn `c.IdentityNumber`. Đây là trường dùng để tìm lại đúng khách khi tạo/sửa lượt ở. |
| **Lượt đã thanh...** | **Không có cột `Visits` lưu sẵn.** SQL tính `COUNT(i.Id)` | `Customers c LEFT JOIN Stays s ON s.CustomerId=c.Id LEFT JOIN Invoices i ON i.StayId=s.Id`. Mỗi `Invoice` là một lượt **đã thanh toán**, vì `Invoices.StayId` là duy nhất (`Database/Setup.sql:64`). Lượt mới đặt hoặc đã hủy chưa có hóa đơn nên chưa được đếm. Tên cột đầy đủ trong code là **Lượt đã thanh toán** (`GUI/ucDashboard.Reports.cs:107`); dấu `...` trên ảnh do cột hiển thị bị hẹp. |
| **Tổng chi** | **Không có cột `Total` lưu sẵn trong `Customers`.** SQL tính `COALESCE(SUM(i.RoomCharge+i.ServiceCharge),0)` | Chỉ cộng tiền phòng + tiền dịch vụ từ các hóa đơn đã phát hành; không cộng riêng `Payments.Deposit` lần nữa. Khách chưa có hóa đơn hiện `0`. GUI định dạng có dấu phân cách hàng nghìn (`GUI/ucDashboard.Reports.cs:112`). |

Đây là câu `SELECT` của `DAL/HotelRepository.cs:111` được xuống dòng và đặt tên cho hai cột tính toán để dễ theo dõi:

```sql
SELECT TOP (200)
    c.Id, c.Name, c.Phone, c.IdentityNumber,
    COUNT(i.Id) AS Visits,
    COALESCE(SUM(i.RoomCharge + i.ServiceCharge), 0) AS Total
FROM dbo.Customers AS c
LEFT JOIN dbo.Stays AS s ON s.CustomerId = c.Id
LEFT JOIN dbo.Invoices AS i ON i.StayId = s.Id
WHERE @p0 = N''
   OR CHARINDEX(@p0, c.Name) > 0
   OR CHARINDEX(@p0, c.Phone) > 0
   OR CHARINDEX(@p0, c.IdentityNumber) > 0
GROUP BY c.Id, c.Name, c.Phone, c.IdentityNumber
ORDER BY c.Id DESC;
```

Đọc như một câu tiếng Việt: **FROM** mở sổ khách; hai **JOIN** lần theo ID sang lượt ở rồi hóa đơn; **WHERE** giữ những khách khớp ô tìm (`@p0`) hoặc lấy tất cả nếu ô trống; **GROUP BY** gom các hàng của cùng một khách; **COUNT** đếm hóa đơn; **SUM** cộng tiền; **ORDER BY** đưa hồ sơ mới lên trước. `@p0` được truyền từ `search.Text` qua BLL/DAL, không phải một cột có tên `@p0` trong database (`DAL/HotelRepository.cs:50-54,111`).

`LEFT JOIN` nghĩa là **vẫn hiện khách dù chưa có lượt ở hoặc chưa có hóa đơn**. `COUNT(i.Id)` khi đó là 0; `SUM(...)` không có số để cộng nên `COALESCE(...,0)` đổi thành 0. SQL thực tế ở `DAL/HotelRepository.cs:111`; các trường được đưa vào `CustomerSummary` theo thứ tự ở cuối cùng dòng đó (`DTO/HotelModels.cs:34`).

**Ví dụ dễ kiểm tra:** khách A có `Customers.Id=7`. Hai lượt `Stays.Id=21,22` đều có `CustomerId=7`. Lượt 21 đã checkout và có một `Invoices` tổng 500.000đ; lượt 22 còn Reserved, chưa có hóa đơn. Hàng khách A sẽ có **Lượt đã thanh toán = 1**, **Tổng chi = 500.000đ**. Đây là số đọc/tính lúc mở hoặc tìm lại, không phải hai giá trị được lưu trong `Customers`.

### 13.3 Bấm chọn một hàng khách thì hai tab bên dưới lấy gì?

Khi chọn hàng, `grid.SelectionChanged` ở `GUI/ucDashboard.Reports.cs:118-143` lấy đối tượng `CustomerSummary` đang chọn. GUI dùng **`customer.Id`**, không dùng tên hay số điện thoại, để lấy đúng dữ liệu liên quan. Nếu người dùng đổi hàng trong lúc SQL đang chạy, code kiểm tra ID một lần nữa trước khi điền bảng (`:124-129`).

| Tab/cột | Đường đi của mã | Nguồn SQL và ý nghĩa |
| --- | --- | --- |
| **Dịch vụ đã sử dụng** | `customer.Id` → `service.CustomerOrdersAsync(id)` (`GUI/ucDashboard.Reports.cs:126`; `BLL/HotelService.cs:23`) → `db.CustomerOrdersAsync(id)` (`DAL/HotelRepository.cs:112`) → `history.DataSource` (`GUI/ucDashboard.Reports.cs:130`). | `ServiceOrders o JOIN Stays s ON s.Id=o.StayId WHERE s.CustomerId=@p0`, tối đa 500 dòng gần nhất. Nghĩa là “lấy món của **mọi lượt ở** thuộc khách này”. Dòng đã hủy vẫn hiện để xem lịch sử. |
| Cột **Ngày** của dịch vụ | `ServiceOrders.Ordered` → `ServiceLine.Ordered` | Thời điểm gọi món; GUI định dạng ngày giờ ở `GUI/ucDashboard.Reports.cs:130-131`. |
| **TênDịchVụ** và **SốLượng** | `ServiceOrders.Name`, `ServiceOrders.Quantity` | Tên/đơn giá tại lúc gọi đã được chụp vào đơn, không cần lấy tên hiện tại từ `Services` để dựng lịch sử. |
| **ThànhTiền** | `ServiceOrders.Quantity`, `Price`, `Cancelled` → `ServiceLine.Total` (`DTO/HotelModels.cs:22-25`) | Chưa hủy: số lượng × giá chụp. Đã hủy: 0. |
| **TrạngThái** | `ServiceOrders.Cancelled` | `NULL` → “Có hiệu lực”; có thời điểm hủy → “Đã hủy” (`GUI/ucDashboard.Reports.cs:130`). Đây là chữ GUI tạo, không phải cột `TrangThai` trong SQL. |
| **Hóa đơn** | `customer.Id` → `service.CustomerInvoicesAsync(id)` (`GUI/ucDashboard.Reports.cs:133`; `BLL/HotelService.cs:22`) → `db.CustomerInvoicesAsync(id)` (`DAL/HotelRepository.cs:115`) → `invoices.DataSource` (`GUI/ucDashboard.Reports.cs:135`). | `Invoices i JOIN Stays s ON s.Id=i.StayId WHERE s.CustomerId=@p0`, tối đa 200 hóa đơn gần nhất. Hóa đơn của khách khác không vào bảng vì `CustomerId` không khớp. |
| **MãHóaĐơn, Phòng, NgàyLập, HìnhThức** | `Invoices.Id`, `RoomNumber`, `Issued`, `Method` | GUI lấy từ `Invoice` và đặt nhãn hiển thị (`GUI/ucDashboard.Reports.cs:135-137`). `RoomNumber` là số phòng đã ghi trên hóa đơn lúc chốt. |
| **TổngTiền** của hóa đơn | `Invoices.RoomCharge + Invoices.ServiceCharge` → `Invoice.Total` (`DTO/HotelModels.cs:29-32`) | Không lấy từ `Payments.Amount`, vì một hóa đơn có thể được thanh toán bằng cọc đã thu trước cộng tiền thu lúc checkout. |

Nếu tab dịch vụ trống như trong ảnh, nghĩa là truy vấn `CustomerOrdersAsync(customer.Id)` không trả dòng dịch vụ cho **khách đang chọn**. Nó không chứng minh khách chưa từng ở hoặc chưa có hóa đơn; hãy bấm tab **Hóa đơn** để xem riêng. Nếu chọn khách khác, hai tab sẽ chạy truy vấn lại theo ID của khách mới.

### 13.4 Tại sao có dữ liệu ở ba nơi giống nhau?

- `Customers.Name/Phone/IdentityNumber`: hồ sơ **hiện tại**, dùng tìm khách và tránh tạo trùng theo giấy tờ.
- `Stays.GuestName/Phone/IdentityNumber`: **thông tin của một lần đặt/ở**, được ghi lúc tạo và có thể được sửa bằng form **Sửa thông tin khách** khi lượt còn Reserved. Một khách đổi SĐT sau này không tự làm mọi lượt cũ đổi theo.
- `Invoices.GuestName/RoomNumber`: **bản chụp khi phát hành hóa đơn**. Sau này đổi tên hồ sơ hoặc số phòng trong danh mục không nên làm phiếu cũ “đổi theo”.
- `ServiceOrders.Name/Price`: **bản chụp món và giá lúc gọi**. `Services.Name/Price` là danh mục hiện tại; quản lý sửa danh mục không làm dòng đã gọi thay đổi.

Đó là lý do nhìn các bảng thấy tên/giá có vẻ được lưu nhiều lần: mỗi bản phục vụ **một mốc thời gian khác nhau**. Liên kết bằng ID cho biết bản nào thuộc khách/lượt nào; bản chụp giữ lịch sử dễ đối chiếu. Xem cột và khóa ngoại trong `Database/Setup.sql:32-69`, câu ghi ở `DAL/HotelTransaction.Commands.cs:9-10,25,29`.

## 14. Bản đồ nguồn và nơi lưu dữ liệu của các màn hình

Đọc bảng này theo câu hỏi: **“Con số/chữ trên màn hình lấy từ cuốn sổ nào, có được lưu y nguyên không?”** `Reload()` gọi `DashboardAsync()` để lấy các danh sách rồi `Render()` tính các số cần hiện (`GUI/ucDashboard.cs:110-118,132-176`; `BLL/HotelService.cs:14-20`). Khi chưa nhấn **Làm mới** hoặc chưa có hành động tải lại, màn hình có thể đang giữ bản dữ liệu của lần tải trước.

### 14.1 Những gì nhìn thấy trên dashboard trong ảnh

| Vùng trong ảnh | Nguồn và phép tính | Lưu ở đâu? / Liên kết nào? | Mã đọc và vẽ |
| --- | --- | --- | --- |
| **Tổng quan quản trị · admin** | Tên `admin` là `UserSession.Username` sau đăng nhập; “Tổng quan quản trị” do `Role` chọn chữ phù hợp. | Tài khoản/vai trò gốc nằm ở `Users.Username/Role`; `UserSession` chỉ giữ bản đang đăng nhập trong bộ nhớ. | `BLL/AuthService.cs:33-52` → `GUI/ucDashboard.Roles.cs:7-18` → `GUI/ucDashboard.cs:117`. |
| **Đồng hồ góc trên** | Lúc tải lấy `SYSDATETIME()` từ SQL Server rồi cộng thời gian đã trôi trên máy để cập nhật mỗi giây. | Không có bảng “đồng hồ”. Giờ gốc đọc bằng `DAL/HotelRepository.cs:76`; màn hình tính ở `GUI/ucDashboard.cs:25-26,38`. | `GUI/ucDashboard.cs:110-118`. |
| **Công suất phòng 0 / 60** | Đếm `Rooms.Status='DangO'` chia cho tổng số hàng `Rooms`. 0 / 60 nghĩa là hiện không phòng nào đang ở trong 60 phòng đang có. | Từng phòng và trạng thái lưu tại `Rooms`. `0 / 60` và phần trăm **không lưu thành cột riêng**. | `DAL/HotelRepository.cs:94` → `BLL/HotelService.cs:19` → `GUI/ucDashboard.cs:134-137`. |
| **Phòng sẵn sàng / phòng trống** | Đếm `Rooms.Status='Trong'`. | Nguồn `Rooms.Status`, tổng đếm chỉ ở bộ nhớ GUI. Một phòng trống có thể vẫn có lịch đặt tương lai trong `Stays`. | `GUI/ucDashboard.cs:138`; `DAL/HotelRepository.cs:94`. |
| **Lượt đặt chờ nhận** | Lọc `data.Stays` có `Status='Reserved'`, đếm lượt có `Deposit>0` và bằng 0. | Lượt đặt, cọc và ngày đến nằm trong `Stays`; mỗi lần thu cọc thực tế còn có một `Payments.Deposit`. `Stays.RoomId → Rooms.Id`. | `DAL/HotelRepository.cs:98` → `GUI/ucDashboard.cs:135,139-140`. |
| **Khách sắp trả hôm nay** | Đếm lượt `Stays.Status='Occupied'` có `Departure.Date` bằng ngày SQL đang hiển thị. | `Stays.Departure` và `Stays.Status`; số đếm không được lưu riêng. | `GUI/ucDashboard.cs:141`. |
| **Doanh thu hôm nay** | Cộng các `RevenueItem.Total`: tiền phòng từ `Invoices.RoomCharge`, dịch vụ từ `ServiceOrders.Price × Quantity` nối với `Invoices`, cọc không hoàn từ `Payments.Forfeit`. | Dữ liệu gốc ở `Invoices`, `ServiceOrders`, `Payments`; tổng trên thẻ **được tính khi đọc**, không có cột `TodayRevenue`. `ServiceOrders.StayId → Invoices.StayId` là đường nối dịch vụ với hóa đơn. | `DAL/HotelRepository.cs:110` → `BLL/HotelService.cs:19` → `GUI/ucDashboard.cs:142`. |
| **Các ô phòng 101, 102…** | Số/loại/trạng thái từ `Rooms`; dấu `*` nếu có `Stays.Status='Reserved'` cùng `RoomId`. | `Rooms.Id` liên kết `Stays.RoomId`; dấu `*` là chữ GUI ghép lúc vẽ, không lưu vào `Rooms.Number`. | `DAL/HotelRepository.cs:94,98` → `GUI/ucDashboard.cs:158-173`. |
| **Phòng đang dọn** bên phải | Lọc `Rooms.Status='DangDon'`. | `Rooms.Status` đổi sang Đang dọn khi checkout/chuyển phòng (`DAL/HotelTransaction.Commands.cs:13`). Danh sách chỉ là bộ lọc. | `GUI/ucDashboard.cs:250-259`. |
| **Dịch vụ đang chờ** bên phải | Lọc `ServiceOrders.Delivered IS NULL AND Cancelled IS NULL`; ghép tên khách/phòng từ `Stays` và `Rooms` đã tải. | `ServiceOrders.StayId → Stays.Id`, `Stays.RoomId → Rooms.Id`. Phần chưa giao = `Quantity - DeliveredQuantity`. | `DAL/HotelRepository.cs:108` → `GUI/ucDashboard.cs:261-288`. |
| **Bảng đặt trước phía dưới** | Chỉ lấy `Stays.Status='Reserved'`; số phòng tìm qua `Stays.RoomId → Rooms.Id`; chữ “quá hạn” so `HoldUntil` với giờ SQL. | Các giá trị gốc ở `Stays` và `Rooms`. Trạng thái chữ của dòng do GUI tạo; khi quá hạn được xử lý, BLL mới sửa `Stays`/`Payments`. | `GUI/ucDashboard.cs:198-207`; `BLL/HotelService.cs:99-111`. |

**Lưu ý về vai trò:** cùng năm thẻ nhưng Accountant thấy hóa đơn, doanh thu hóa đơn, khoản thu và khoản hoàn. GUI đổi tên ở `GUI/ucDashboard.Roles.cs:27-43` và tính từ `PeriodReport.Invoices/Payments` tại `GUI/ucDashboard.cs:143-149`; không dùng phép đếm phòng cho bốn thẻ này. Manager và Reception có nhãn khác nhưng vẫn lấy dữ liệu theo quyền trong `BLL/HotelService.cs:18-20`.

### 14.2 Một thao tác ghi sẽ tạo hoặc đổi những “cuốn sổ” nào?

| Thao tác | Đọc trước để kiểm tra | Ghi nơi nào? | Sau đó màn hình lấy lại từ đâu? |
| --- | --- | --- | --- |
| Tạo đặt phòng | `Rooms` theo ID, `Stays` để tránh trùng lịch, giờ SQL | `Customers` tìm theo giấy tờ và cập nhật/thêm; `Stays` thêm lượt; `Rooms.Version` tăng nhưng `Rooms.Status` giữ nguyên khi chỉ đặt trước; `Payments` thêm cọc nếu có; `AuditLog` ghi người làm (`BLL/HotelService.cs:64-69`). `Stays.CustomerId → Customers.Id`, `Stays.RoomId → Rooms.Id`. | `Reload()` đọc `Rooms`/`Stays` để cập nhật bảng đặt và dấu `*` (`GUI/ucDashboard.cs:110-118,170-171,198-200`). |
| Nhận khách đã đặt | `Stays` theo ID, `Rooms` theo ID, lịch `Stays` | Sửa `Stays.Status/CheckIn`, thêm `StaySegments`, sửa `Rooms.Status`; ghi `AuditLog`. `StaySegments.StayId → Stays.Id`. | `Reload()` lấy trạng thái phòng và lượt ở; danh sách đặt giảm một, phòng chuyển Đang ở. |
| Gọi dịch vụ | `Stays` đang ở, `Services` đang bán | Thêm `ServiceOrders` và tăng `Stays.Version`; ghi `AuditLog`. `ServiceOrders.StayId → Stays.Id`, `ServiceOrders.ServiceId → Services.Id`. | `PendingAsync()` lấy món chưa giao và đặt vào cột yêu cầu bên phải (`DAL/HotelRepository.cs:108`). |
| Thu cọc | `Stays` theo ID, giờ SQL | Tăng `Stays.Deposit`, thêm `Payments.Deposit`, ghi `AuditLog`. `Payments.StayId → Stays.Id`. | `Reload()` hiện cọc mới trong dòng đặt; báo cáo thu chi đọc `Payments` (`DAL/HotelTransaction.Management.cs:46`). |
| Checkout | `Stays`, `Rooms`, `StaySegments`, `ServiceOrders`, giờ SQL | Thêm `Invoices`, có thể thêm `Payments.Checkout`/`Refund`, đóng `StaySegments`, đóng `Stays`, đổi `Rooms.Status`, ghi `AuditLog`. `Invoices.StayId → Stays.Id`. | Dashboard đọc phòng Đang dọn; màn hình khách đọc hóa đơn qua `CustomerInvoicesAsync()`; báo cáo đọc `Invoices`/`Payments`. |
| Sửa danh mục | `Rooms` hoặc `Services` hiện tại, `Stays` nếu phòng có lịch | Sửa/thêm `Rooms` hoặc `Services`, ghi `AuditLog`; **không** viết lại `StaySegments.Rate`, `ServiceOrders.Price` hay `Invoices` đã chốt. | `Reload()` đọc danh mục mới cho sơ đồ/menu; hóa đơn cũ vẫn đọc dữ liệu chụp lúc phát sinh. |

**`AuditLog` là sổ “ai làm gì”**, không thay thế sổ nghiệp vụ. Ví dụ “Thu cọc” ghi số tiền thực ở `Payments`, còn `AuditLog` chỉ ghi chi tiết thao tác và `UserId` của nhân viên (`DAL/HotelRepository.cs:92`; `Database/Setup.sql:71-79`).

Một số lệnh còn **tăng `Rooms.Version` dù trạng thái phòng không đổi**: đặt trước, hủy đặt, xử lý quá hạn và sửa lịch (`BLL/HotelService.cs:65,96,107`; `BLL/HotelService.Management.cs:38-39`; `DAL/HotelTransaction.Commands.cs:13-16`). `Version` là số để phát hiện màn hình cũ, không phải một trạng thái phòng mới. Vì vậy khi tra “bảng nào có `UPDATE`”, cần tính cả cập nhật phiên bản này.

### 14.3 Dữ liệu nằm ở SQL, trên máy, hay chỉ hiện tạm?

| Nơi giữ | Ví dụ | Nếu tắt ứng dụng thì sao? |
| --- | --- | --- |
| **SQL Server** database `QLKhachSanApp` theo cấu hình mặc định | Mười bảng ở mục 7: `Users`, `Rooms`, `Customers`, `Stays`, `StaySegments`, `Services`, `ServiceOrders`, `Invoices`, `Payments`, `AuditLog` cùng `SchemaVersion`. Đường kết nối lấy từ `appsettings.json` hoặc biến môi trường `QLKHACHSAN_CONNECTION_STRING` (`DAL/DatabaseHelper.cs:18-24`). | Dữ liệu vẫn còn trong database. Vị trí tệp vật lý `.mdf` do SQL Server quản lý, không phải tệp trong thư mục mã nguồn này. |
| **Tệp cấu hình ứng dụng** | `QLKhachSan/appsettings.json` chứa địa chỉ database, tên khách sạn và cấu hình QR; `AppSettings.Load()` đọc lúc chạy (`DAL/DatabaseHelper.cs:18-24`). | Vẫn còn trong thư mục chạy/publish; thay tệp cấu hình có thể đổi nơi app kết nối, nhưng không tự chuyển dữ liệu từ database cũ sang mới. |
| **Tệp ghi nhớ đăng nhập của Windows** | `remembered-login.dat` trong thư mục LocalApplicationData của tài khoản Windows, được bảo vệ bằng DPAPI (`GUI/RememberedLogin.cs:9-51`). | Vẫn còn trên máy/tài khoản Windows đó; đây **không** phải bảng `Users` trong SQL. |
| **Tệp CSV do người dùng chọn nơi lưu** | Báo cáo hóa đơn, thu chi, lịch sử (`GUI/ucDashboard.Export.cs:17-64`). | Vẫn còn ở vị trí đã lưu; là bản xuất, không phải database đang hoạt động. |
| **Bộ nhớ tạm khi app đang mở** | `DashboardData data`, `BillQuote bill`, giỏ món `cart`, hàng đang chọn, con số tổng được GUI tính (`GUI/ucDashboard.cs:16`; `GUI/ucDashboard.Reports.cs:11`; `GUI/ucDashboard.Actions.cs:339`). | Mất khi tắt form/app. Muốn có lại, chương trình đọc SQL và tính lại; giỏ chưa bấm **Gửi yêu cầu** thì chưa được lưu. |

**Tự kiểm tra:** tìm một ô trên giao diện và hỏi ba câu: (1) giá trị gốc nằm trong bảng nào? (2) nếu là số tổng, hàm nào tính nó và nối bằng ID nào? (3) nút nào đã ghi giá trị gốc ban đầu? Với cột **Tổng chi** của hồ sơ khách, câu trả lời lần lượt là `Invoices.RoomCharge/ServiceCharge`, `CustomersAsync` nối `Customers.Id → Stays.CustomerId → Invoices.StayId` rồi `SUM`, và nút **Hoàn tất check-out** đã tạo hóa đơn. Cột tổng chỉ được tính khi xem, không có lệnh lưu riêng cho nó.

<a id="phan-15"></a>

## 15. Bản đồ mã nguồn và thuật ngữ

Tài liệu này dành cho mọi người muốn tìm hiểu mã nguồn. Mục tiêu là mở **bất kỳ tệp nguồn nào** trong dự án và trả lời được: nó chạy lúc nào, nhận dữ liệu từ đâu, gọi hàm nào tiếp theo, có đọc/ghi database không, và kết quả hiện ở đâu. Những dòng dựng giao diện lặp lại trong `*.Designer.cs` được giải thích theo **nhóm điều khiển và sự kiện**, vì chúng đều cùng nguyên tắc; các hàm nghiệp vụ và SQL được giải thích riêng theo đầu vào/đầu ra.

### Trình tự đọc

1. Đọc [Thông tin chức năng](#mục-lục) để thấy nút → hàm → bảng bằng ví dụ thực tế.
2. Đọc [Giao diện và sự kiện](#phan-16): cửa sổ, nút, biểu đồ, màu và cách `Click` gọi hàm.
3. Đọc [Nghiệp vụ và dữ liệu](#phan-17): quyền, đặt/nhận/trả phòng, dịch vụ, tính tiền, tài khoản, DTO.
4. Đọc [SQL và cấu hình](#phan-18): câu `SELECT`/`INSERT`/`UPDATE`, khóa ngoại, giao dịch, script tạo/nâng cấp bảng.
5. Dùng [các ví dụ theo dõi thao tác](#phan-19) để lần theo một thao tác từ giao diện đến database và tự kiểm tra hiểu biết.

**Cách hiểu đường dẫn:** `GUI/ucDashboard.cs:301` nghĩa là dòng 301 trong `QLKhachSan/GUI/ucDashboard.cs`. Số dòng đúng với mã ở thời điểm viết tài liệu; sau khi sửa mã, hãy tìm **tên hàm** nếu dòng dịch chuyển. Liên kết tệp trong các bảng cho phép mở mã thật.

### Từ điển 1 phút

| Từ trong mã | Hiểu đơn giản | Ví dụ trong dự án |
| --- | --- | --- |
| `class` | Một “bản thiết kế” để tạo đối tượng | `HotelService` là người kiểm tra nghiệp vụ; `Room` là mẫu phiếu phòng. |
| `record` | Mẫu dữ liệu chủ yếu để chuyển giá trị | `GuestInput` gồm tên, SĐT, giấy tờ (`DTO/HotelModels.cs:13`). |
| `method`/hàm | Một công việc có tên, nhận tham số, có thể trả kết quả | `CustomersAsync(search)` nhận chữ tìm, trả danh sách khách. |
| `event` | Chuông báo một việc vừa xảy ra | `btnDangNhap.Click += btnDangNhap_Click`: nhấn nút thì gọi hàm (`GUI/FormLogin.Designer.cs:113`). |
| `async`/`await` | Làm việc chờ SQL mà không “đóng băng” giao diện | `await service.DashboardAsync()` chờ dữ liệu rồi vẽ. |
| `GUI` | Màn hình và tương tác | Form login, dashboard, hộp thoại, biểu đồ. |
| `BLL` | Luật nghiệp vụ và quyền | Kiểm tra phòng trống, cọc, tính hóa đơn. |
| `DAL` | Phần nói chuyện với SQL Server | Các hàm `Query`, `Execute`, `Scalar`. |
| `DTO` | Phiếu dữ liệu đi giữa các lớp | `Room`, `Stay`, `Invoice`, `BillQuote`. |
| `SELECT` | Đọc sổ, không đổi dữ liệu | Danh sách phòng từ `dbo.Rooms`. |
| `INSERT`/`UPDATE` | Thêm hoặc sửa sổ | Tạo `Stays`; đổi `Rooms.Status`. |
| `Id`/khóa chính | Số riêng của một hàng | `Customers.Id=7`. |
| `CustomerId`/khóa ngoại | Số viết trên hàng khác để nối đúng chủ | `Stays.CustomerId=7` nối khách 7. |
| `transaction` | Một gói công việc: tất cả cùng lưu hoặc cùng hủy | Checkout phải lưu hóa đơn, thanh toán và trạng thái phòng cùng nhau. |
| `Version` | Số nhận biết bản dữ liệu đã bị người khác sửa | Form cũ có `Version=2`, SQL đã là 3 thì yêu cầu làm mới. |
| `null` | Chưa có giá trị | `Stay.CheckIn=null` khi mới đặt trước; `ServiceLine.Cancelled=null` khi chưa hủy. |

### Toàn cảnh đường đi dữ liệu

```text
Program.Main
  └─ FormLogin: nhập tên/mật khẩu
       └─ AuthService: xác thực và trả UserSession
            └─ FormMain chứa ucDashboard
                 ├─ HotelService: đọc dashboard và kiểm tra nghiệp vụ
                 │    └─ HotelRepository / HotelTransaction: SQL Server
                 └─ Render: Room/Stay/Invoice DTO → chữ, bảng, biểu đồ
```

`GUI` không nên được hiểu là nơi dữ liệu đã được lưu. Ví dụ giỏ dịch vụ chỉ là biến trong bộ nhớ cho đến khi nhấn **Gửi yêu cầu**. `BLL` vẫn đọc lại SQL trước khi ghi, vì một nhân viên khác có thể đã đổi phòng trong lúc bạn mở form. `DAL` chạy các lệnh trong giao dịch và trả `DTO` cho `BLL`; `GUI` nhận DTO rồi hiển thị. Xem ví dụ đầy đủ trong [Thông tin, mục 10](#10-luồng-dữ-liệu-chi-tiết-nhận-thông-tin-từ-hàm-nào).

### Bản đồ mọi tệp nguồn

Mỗi tệp dưới đây có lời giải thích trong chương được chỉ ở cột cuối. `partial` nghĩa là một lớp lớn được chia ra nhiều tệp, không phải nhiều dashboard độc lập.

| Tệp | Vai trò chính | Đọc ở |
| --- | --- | --- |
| [`QLKhachSan.slnx`](../QLKhachSan.slnx) | Solution gom project | [SQL và cấu hình](#phan-18) |
| [`QLKhachSan.csproj`](../QLKhachSan/QLKhachSan.csproj) | Chọn .NET/WinForms, package SQL, nhúng migration, copy cấu hình | [SQL và cấu hình](#phan-18) |
| [`Program.cs`](../QLKhachSan/Program.cs) | Điểm bắt đầu, vòng login → dashboard → logout | [Giao diện](#phan-16) |
| [`appsettings.json`](../QLKhachSan/appsettings.json) | Địa chỉ database, QR, thông tin khách sạn | [SQL và cấu hình](#phan-18) |
| [`DTO/HotelModels.cs`](../QLKhachSan/DTO/HotelModels.cs) | Các mẫu `Room`, `Stay`, `Invoice`, `BillQuote`… | [Nghiệp vụ](#phan-17) |
| [`BLL/RolePolicy.cs`](../QLKhachSan/BLL/RolePolicy.cs) | Quyền theo vai trò | [Nghiệp vụ](#phan-17) |
| [`BLL/AuthService.cs`](../QLKhachSan/BLL/AuthService.cs) | Tạo Admin, login, tạo tài khoản, đổi mật khẩu | [Nghiệp vụ](#phan-17) |
| [`BLL/AuthService.Management.cs`](../QLKhachSan/BLL/AuthService.Management.cs) | Xem/đổi quyền, đặt lại mật khẩu nhân viên | [Nghiệp vụ](#phan-17) |
| [`BLL/HotelService.cs`](../QLKhachSan/BLL/HotelService.cs) | Dashboard và các nghiệp vụ lưu trú cốt lõi | [Nghiệp vụ](#phan-17) |
| [`BLL/HotelService.Management.cs`](../QLKhachSan/BLL/HotelService.Management.cs) | Cọc, sửa đặt, dịch vụ, danh mục, báo cáo | [Nghiệp vụ](#phan-17) |
| [`BLL/BillingPolicy.cs`](../QLKhachSan/BLL/BillingPolicy.cs) | Thuật toán tính tiền phòng | [Nghiệp vụ](#phan-17) |
| [`DAL/DatabaseHelper.cs`](../QLKhachSan/DAL/DatabaseHelper.cs) | Đọc cấu hình và chuỗi kết nối | [SQL và cấu hình](#phan-18) |
| [`DAL/SchemaMigrator.cs`](../QLKhachSan/DAL/SchemaMigrator.cs) | Tự nâng schema đến V6 khi mở app; báo lỗi migration có vị trí SQL | [SQL và cấu hình](#phan-18) |
| [`DTO/FinanceModels.cs`](../QLKhachSan/DTO/FinanceModels.cs) | Ca, phiếu, công nợ, kho, chỉ số tài chính | [Tài chính V6](#20-phân-hệ-tài-chính---kế-toán-v6) |
| [`BLL/HotelService.Finance.cs`](../QLKhachSan/BLL/HotelService.Finance.cs) | Quyền và quy tắc nghiệp vụ kế toán | [Tài chính V6](#20-phân-hệ-tài-chính---kế-toán-v6) |
| [`DAL/HotelTransaction.Finance.cs`](../QLKhachSan/DAL/HotelTransaction.Finance.cs) | SQL sổ quỹ, ca, công nợ, kho, hóa đơn và báo cáo | [Tài chính V6](#20-phân-hệ-tài-chính---kế-toán-v6) |
| [`DAL/HotelRepository.cs`](../QLKhachSan/DAL/HotelRepository.cs) | Giao dịch, tham số SQL, đọc dữ liệu chính và tài khoản | [SQL và cấu hình](#phan-18) |
| [`DAL/HotelTransaction.Commands.cs`](../QLKhachSan/DAL/HotelTransaction.Commands.cs) | Các lệnh ghi phòng, lượt ở, món, hóa đơn, thanh toán | [SQL và cấu hình](#phan-18) |
| [`DAL/HotelTransaction.Management.cs`](../QLKhachSan/DAL/HotelTransaction.Management.cs) | Lịch, báo cáo, danh mục, nhân viên, nhật ký | [SQL và cấu hình](#phan-18) |
| [`Database/Setup.sql`](../Database/Setup.sql) | Tạo database schema V1, phòng và món mẫu | [SQL và cấu hình](#phan-18) |
| [`Database/MigrateV2.sql`](../Database/MigrateV2.sql), [`V3`](../Database/MigrateV3.sql), [`V4`](../Database/MigrateV4.sql), [`V5`](../Database/MigrateV5.sql), [`V6`](../Database/MigrateV6.sql) | Nâng cấu trúc lên bản đang dùng | [SQL và cấu hình](#phan-18) |
| [`GUI/AccountingForm.cs`](../QLKhachSan/GUI/AccountingForm.cs) | Các tab kế toán, nút thao tác và bản xem trước khi in | [Tài chính V6](#20-phân-hệ-tài-chính---kế-toán-v6) |
| [`GUI/ExcelExport.cs`](../QLKhachSan/GUI/ExcelExport.cs) | Xuất lưới `.xlsx` | [Tài chính V6](#20-phân-hệ-tài-chính---kế-toán-v6) |
| [`GUI/FormLogin.cs`](../QLKhachSan/GUI/FormLogin.cs) và [`FormLogin.Designer.cs`](../QLKhachSan/GUI/FormLogin.Designer.cs) | Logic login và chỗ tạo/nối điều khiển | [Giao diện](#phan-16) |
| [`GUI/FormMain.cs`](../QLKhachSan/GUI/FormMain.cs) và [`FormMain.Designer.cs`](../QLKhachSan/GUI/FormMain.Designer.cs) | Cửa sổ chính chứa dashboard | [Giao diện](#phan-16) |
| [`GUI/ucDashboard.cs`](../QLKhachSan/GUI/ucDashboard.cs) và [`ucDashboard.Designer.cs`](../QLKhachSan/GUI/ucDashboard.Designer.cs) | Nạp dữ liệu, vẽ sơ đồ và các điều khiển nền | [Giao diện](#phan-16) |
| [`GUI/ucDashboard.Actions.cs`](../QLKhachSan/GUI/ucDashboard.Actions.cs) | Form đặt, hủy, chuyển, gia hạn, bảo trì, gọi món | [Giao diện](#phan-16) |
| [`GUI/ucDashboard.Management.cs`](../QLKhachSan/GUI/ucDashboard.Management.cs) | Form cọc, sửa lịch, xử lý món, lịch sử, audit | [Giao diện](#phan-16) |
| [`GUI/ucDashboard.Catalog.cs`](../QLKhachSan/GUI/ucDashboard.Catalog.cs) | Form danh mục phòng, dịch vụ và bảng giá | [Giao diện](#phan-16) |
| [`GUI/ucDashboard.Reports.cs`](../QLKhachSan/GUI/ucDashboard.Reports.cs) | Checkout, khách, báo cáo, tài khoản | [Giao diện](#phan-16) |
| [`GUI/ucDashboard.RoleFunctions.cs`](../QLKhachSan/GUI/ucDashboard.RoleFunctions.cs) | Nhân viên chỉ xem, thu chi, chuyển tab biểu đồ | [Giao diện](#phan-16) |
| [`GUI/ucDashboard.Roles.cs`](../QLKhachSan/GUI/ucDashboard.Roles.cs) | Nhãn/chức năng dashboard theo vai trò | [Giao diện](#phan-16) |
| [`GUI/ucDashboard.Export.cs`](../QLKhachSan/GUI/ucDashboard.Export.cs) | Xuất CSV và in phiếu | [Giao diện](#phan-16) |
| [`GUI/ucDashboard.Appearance.cs`](../QLKhachSan/GUI/ucDashboard.Appearance.cs) | Bố cục, màu, icon và điều chỉnh kích cỡ dashboard | [Giao diện](#phan-16) |
| [`GUI/RevenueOverview.cs`](../QLKhachSan/GUI/RevenueOverview.cs) | Thẻ doanh thu và biểu đồ tự vẽ | [Giao diện](#phan-16) |
| [`GUI/PaymentQr.cs`](../QLKhachSan/GUI/PaymentQr.cs) | Tạo ảnh QR khi chọn chuyển khoản | [Giao diện](#phan-16) |
| [`GUI/RememberedLogin.cs`](../QLKhachSan/GUI/RememberedLogin.cs) | Ghi nhớ login có bảo vệ trên Windows | [Giao diện](#phan-16) |
| [`GUI/Ui.cs`](../QLKhachSan/GUI/Ui.cs) | Hộp thoại, ô nhập, bảng và thông báo dùng chung | [Giao diện](#phan-16) |
| [`GUI/UiIcons.cs`](../QLKhachSan/GUI/UiIcons.cs) | Vẽ icon vector bằng code | [Giao diện](#phan-16) |
| [`GUI/AppTheme.cs`](../QLKhachSan/GUI/AppTheme.cs) | Bảng màu/font, `RoomTile` tự vẽ | [Giao diện](#phan-16) |
| [`GUI/LoginVisuals.cs`](../QLKhachSan/GUI/LoginVisuals.cs) | Các control tự vẽ của màn login | [Giao diện](#phan-16) |
| [`GUI/DashboardVisuals.cs`](../QLKhachSan/GUI/DashboardVisuals.cs) | Logo và thẻ số liệu tự vẽ | [Giao diện](#phan-16) |
| [`GUI/FormLogin.resx`](../QLKhachSan/GUI/FormLogin.resx), [`FormMain.resx`](../QLKhachSan/GUI/FormMain.resx), [`ucDashboard.resx`](../QLKhachSan/GUI/ucDashboard.resx) | Tài nguyên do WinForms Designer quản lý; xem cùng form tương ứng | [Giao diện](#phan-16) |

Các tệp `*.resx` đi cùng WinForms chứa tài nguyên/metadata của form; chúng không có câu SQL hay luật nghiệp vụ. `bin/` và `obj/`, kể cả các tệp `*.g.cs`, `AssemblyInfo.cs` trong `obj/`, là đầu ra build tự sinh, không phải nơi sửa mã ứng dụng. Khi public source, người đọc nên đọc `Program.cs` trước rồi lần theo các liên kết ở trên.


<a id="phan-16"></a>

## 16. Giao diện và sự kiện

### 1. Quy tắc đọc một màn hình WinForms

Mỗi màn hình thường có hai phần. `*.Designer.cs` tạo các ô nhập, nút, bảng và gắn **sự kiện**. Tệp `.cs` cùng tên chứa hàm chạy khi sự kiện xảy ra. Ví dụ trong [`FormLogin.Designer.cs:113`](../QLKhachSan/GUI/FormLogin.Designer.cs#L113), `btnDangNhap.Click += btnDangNhap_Click` nghĩa là “khi nhấn nút, gọi hàm `btnDangNhap_Click`”. Hàm nằm ở [`FormLogin.cs:108`](../QLKhachSan/GUI/FormLogin.cs#L108). Hãy tìm `Click +=`, `CheckedChanged +=`, `Load +=` để tìm điểm bắt đầu một chức năng.

`GUI` giữ trạng thái đang hiển thị trong RAM: chữ ở `TextBox`, hàng của `DataGridView`, món trong giỏ. Chỉ khi hàm gọi `HotelService`/`AuthService`, rồi `HotelRepository` chạy `INSERT`/`UPDATE`, dữ liệu mới được lưu ở SQL Server. Vì vậy việc sửa chữ trên màn hình chưa có nghĩa là database đã đổi.

### 2. Khởi động và đăng nhập

| Mã | Khi nào chạy, nhận gì | Gọi tiếp và kết quả |
| --- | --- | --- |
| [`Program.Main`](../QLKhachSan/Program.cs#L9) | Windows khởi động ứng dụng | Mở `FormLogin`; khi login trả `UserSession`, mở `FormMain`; logout quay lại login. |
| [`FormLogin.Designer.InitializeComponent`](../QLKhachSan/GUI/FormLogin.Designer.cs#L29) | Khi tạo form | Tạo ô tên, mật khẩu, nút đăng nhập, nút đóng; đặt vị trí, màu, kích thước và nối sự kiện. Không đọc SQL. |
| [`FormLogin.FormLogin`](../QLKhachSan/GUI/FormLogin.cs#L22) | Form vừa tạo | Tạo phần chọn tài khoản/ghi nhớ, đọc tài khoản đã lưu từ `RememberedLogin`, chuẩn bị giao diện. |
| [`BeginWindowDrag`](../QLKhachSan/GUI/FormLogin.cs#L63) | Kéo vùng tiêu đề | Gửi thao tác kéo cửa sổ cho Windows; không đổi dữ liệu. |
| [`SetWorking`](../QLKhachSan/GUI/FormLogin.cs#L69) | Bắt đầu/kết thúc tác vụ chờ | Khóa hoặc mở nút và đổi dòng trạng thái để tránh nhấn đăng nhập nhiều lần. |
| [`SyncAccount`](../QLKhachSan/GUI/FormLogin.cs#L78) | Chọn tài khoản đã nhớ | Đưa tên/mật khẩu tương ứng vào ô nhập. Nguồn là tệp cấu hình cục bộ đã bảo vệ, không phải bảng `Users`. |
| [`InitializeAsync`](../QLKhachSan/GUI/FormLogin.cs#L90) | Form xuất hiện | Gọi `SchemaMigrator.EnsureAsync` để nâng bảng; hỏi `AuthService.NeedsSetupAsync` xem có cần tạo Admin đầu tiên. |
| [`chkHienMatKhau_CheckedChanged`](../QLKhachSan/GUI/FormLogin.cs#L107) | Chọn “Hiện mật khẩu” | Đổi `UseSystemPasswordChar`; chỉ đổi cách hiển thị, không lưu mật khẩu. |
| [`btnDangNhap_Click`](../QLKhachSan/GUI/FormLogin.cs#L108) | Nhấn “Đăng nhập” | Đọc hai ô nhập; nếu hệ thống trống thì `SetupAsync`, còn lại `LoginAsync`; thành công nhận `UserSession`, cập nhật tệp ghi nhớ theo lựa chọn, đóng login để `Program` mở dashboard. |

**Ví dụ tự lần theo:** đặt breakpoint tại `btnDangNhap_Click`, xem giá trị `accountPicker.Text`, tiếp tục vào `AuthService.LoginAsync`, rồi `HotelTransaction.AccountAsync`. Câu `SELECT` ở đó đọc `dbo.Users`. Phần băm mật khẩu nằm ở BLL; mật khẩu gốc không được lấy từ SQL. Xem [chương nghiệp vụ](#phan-17).

### 3. Cửa sổ chính và dashboard

| Tệp/hàm | Vai trò dễ hiểu | Nguồn dữ liệu và nơi hiển thị |
| --- | --- | --- |
| [`FormMain.Designer.cs`](../QLKhachSan/GUI/FormMain.Designer.cs) | Khung cửa sổ ứng dụng | Dựng control của form; không truy vấn database. |
| [`FormMain.cs:9`](../QLKhachSan/GUI/FormMain.cs#L9) | Tạo `ucDashboard` trong cửa sổ chính | Nhận `UserSession` từ login; khi đăng xuất dừng các hoạt động đang chạy và trả về vòng login. |
| [`ucDashboard.Designer.cs:18`](../QLKhachSan/GUI/ucDashboard.Designer.cs#L18) | Dựng nút điều hướng, vùng sơ đồ, các bảng và thẻ số liệu | Đây là “bản vẽ” giao diện. Tìm tên nút tại đây rồi tìm hàm sự kiện cùng tên trong các tệp `ucDashboard*.cs`. |
| [`ucDashboard.cs:30`](../QLKhachSan/GUI/ucDashboard.cs#L30) | Tạo dashboard với `HotelService`, timer và trạng thái hiện tại | `HotelService` được tạo từ phiên đăng nhập; timer kích hoạt cập nhật định kỳ. |
| [`ucDashboard.cs:57`](../QLKhachSan/GUI/ucDashboard.cs#L57) | Ẩn/hiện chức năng theo vai trò | Dùng `RolePolicy`; việc ẩn nút giúp dễ dùng, còn BLL vẫn kiểm tra quyền trước khi đọc/ghi. |
| [`Run`](../QLKhachSan/GUI/ucDashboard.cs#L94) | Bọc một thao tác bất đồng bộ | Chặn thao tác lặp, bắt lỗi và hiển thị thông báo; lệnh SQL vẫn do BLL/DAL quyết định. |
| [`Reload`](../QLKhachSan/GUI/ucDashboard.cs#L110) | Làm mới toàn màn hình | Gọi xử lý lượt đặt quá hạn rồi `HotelService.DashboardAsync`, nhận `DashboardData`. |
| [`Render`](../QLKhachSan/GUI/ucDashboard.cs#L132) | Chuyển DTO thành chữ, màu, hàng bảng, thẻ phòng | Đọc dữ liệu đã lấy về; không tự `SELECT`. |
| [`Render` phần thẻ phòng](../QLKhachSan/GUI/ucDashboard.cs#L158) | Vẽ số và trạng thái phòng | Nguồn `DashboardData.Rooms`, tức `dbo.Rooms`; dấu có lịch còn dựa vào `Stays`. |
| [`Render` phần đặt trước/lịch](../QLKhachSan/GUI/ucDashboard.cs#L198) | Điền các bảng đặt trước và lịch hôm nay | Nguồn các lượt `Stays`, truy vấn lịch ở DAL. |
| [`Render` phần dọn phòng/dịch vụ](../QLKhachSan/GUI/ucDashboard.cs#L250) | Điền danh sách cần dọn, dịch vụ đang chờ | Dựa trạng thái `Rooms` và `ServiceOrders`. |
| [`Render` phần doanh thu](../QLKhachSan/GUI/ucDashboard.cs#L290) | Điền số tiền và biểu đồ | Nguồn `Invoices`, `Payments`, `ServiceOrders` qua BLL/DAL. |

Các nút điều hướng ở [`ucDashboard.cs:301`](../QLKhachSan/GUI/ucDashboard.cs#L301) trở đi gọi các màn hình chức năng trong những tệp `partial` dưới đây. `partial` chỉ là cách chia **cùng một lớp** `ucDashboard` thành các tệp nhỏ. Vì vậy một hàm trong `ucDashboard.Actions.cs` có thể dùng các control được tạo ở `ucDashboard.Designer.cs`.

### 4. Mỗi nhóm thao tác trên dashboard

| Tệp | Mở các hàm nào | Đường đi và bảng được dùng |
| --- | --- | --- |
| [`ucDashboard.Actions.cs`](../QLKhachSan/GUI/ucDashboard.Actions.cs) | `SelectStay` chọn lượt; `RoomAction`, `ShowBooking`, `ShowCancel`, đổi/gia hạn/bảo trì/dọn phòng; giỏ và `AddServices` | Đọc các phòng/lượt từ dashboard hoặc BLL. Nút lưu gọi `HotelService.CreateStayAsync`, `CheckInAsync`, `CancelAsync`, `TransferAsync`, `ExtendAsync`, `SetRoomStatusAsync`, `CleanAllAsync`, `AddServicesAsync`; DAL thay đổi `Customers`, `Stays`, `Rooms`, `StaySegments`, `ServiceOrders`, `Payments` tùy nghiệp vụ. Giỏ ở RAM cho đến lúc gửi yêu cầu. |
| [`ucDashboard.Management.cs`](../QLKhachSan/GUI/ucDashboard.Management.cs) | Thu cọc, sửa đặt, xử lý dịch vụ, lịch sử, nhật ký | Đọc `Stays`, `ServiceOrders`, `Payments`, `AuditLog`; nút xác nhận gọi `HotelService.Management` để ghi. |
| [`ucDashboard.Catalog.cs`](../QLKhachSan/GUI/ucDashboard.Catalog.cs) | Bảng giá, danh mục phòng, danh mục dịch vụ, lưu giá | Đọc/ghi `Rooms` và `Services` qua `HotelService.CatalogAsync`, `SaveRoomAsync`, `UpdateRoomsAsync`, `SaveServiceAsync`. |
| [`ucDashboard.Reports.cs`](../QLKhachSan/GUI/ucDashboard.Reports.cs) | Trả phòng, hồ sơ khách, hóa đơn, tài khoản, đổi/đặt lại mật khẩu | Hồ sơ khách gọi `CustomersAsync` → `CustomerOrdersAsync`/`CustomerInvoicesAsync` theo `CustomerId`; báo cáo đọc `Invoices`, `Payments`; tài khoản gọi `AuthService`, bảng `Users`; checkout ghi `Invoices`, `Payments`, `Stays`, `Rooms`. |
| [`ucDashboard.RoleFunctions.cs`](../QLKhachSan/GUI/ucDashboard.RoleFunctions.cs) | Nhân viên, thu chi, biểu đồ doanh thu | Dùng các hàm đọc của `AuthService`/`HotelService`; hiển thị `Users`, `Payments`, dữ liệu tổng hợp doanh thu. |
| [`ucDashboard.Roles.cs`](../QLKhachSan/GUI/ucDashboard.Roles.cs) | Tiêu đề/vùng nhìn riêng từng vai trò | Quyết định nút và thông điệp theo `UserSession.Role`; không ghi SQL. |
| [`ucDashboard.Export.cs`](../QLKhachSan/GUI/ucDashboard.Export.cs) | Xuất CSV, xem/in phiếu | Nhận hàng bảng và báo cáo đã lấy, chuyển thành tệp CSV hoặc bản in. `Csv` ở dòng 11 vô hiệu chuỗi dễ bị Excel hiểu là công thức. Bản in không tự tạo hóa đơn mới trong SQL. |

Ví dụ ảnh **Hồ sơ khách hàng**: nút mở màn ở [`ucDashboard.Reports.cs:76`](../QLKhachSan/GUI/ucDashboard.Reports.cs#L76), danh sách gọi `LoadCustomers` ở dòng 103 → `HotelService.CustomersAsync` → `HotelTransaction.CustomersAsync` → `Customers` nối `Stays` nối `Invoices`. Khi chọn một khách, dòng 118–143 gọi `CustomerOrdersAsync` và `CustomerInvoicesAsync`; khóa nối là `Customers.Id = Stays.CustomerId`, `Stays.Id = ServiceOrders.StayId` hoặc `Invoices.StayId`. Xem [mô tả từng cột trên ảnh](#mục-lục).

### 5. Các tệp chỉ giúp giao diện dễ dùng và đẹp hơn

| Tệp/hàm | Giải thích | Có đọc/ghi SQL? |
| --- | --- | --- |
| [`ucDashboard.Appearance.cs:10`](../QLKhachSan/GUI/ucDashboard.Appearance.cs#L10) `ApplyAppearance` | Tô màu, căn thanh bên, thẻ số liệu, tiêu đề, bố cục. `AddRoomSection`, `ResizeRoomTiles`, `ReflowSidebar`, `DecorateButton` quản lý vị trí/kiểu nút. | Không. |
| [`AppTheme.cs`](../QLKhachSan/GUI/AppTheme.cs) | Màu, font, viền tròn, kiểu bảng; `RoomColor`/`RoomTint` đổi trạng thái phòng thành màu; `RoomTile` vẽ thẻ và hiệu ứng rê chuột. | Không. |
| [`DashboardVisuals.cs`](../QLKhachSan/GUI/DashboardVisuals.cs) | `DashboardLogo`, `DashboardMetricCard` tự vẽ logo/thẻ theo trạng thái hover. `OnPaint` là lúc Windows yêu cầu vẽ lại. | Không. |
| [`LoginVisuals.cs`](../QLKhachSan/GUI/LoginVisuals.cs) | Các panel/nút tùy biến của login; `OnPaint` vẽ nền, viền, chữ; `TrackFocus` đổi viền khi ô nhập được chọn. `Dispose` trả tài nguyên vẽ. | Không. |
| [`UiIcons.cs`](../QLKhachSan/GUI/UiIcons.cs) | Tạo hình biểu tượng từ mã vẽ; `Kind` chọn loại biểu tượng theo tên. | Không. |
| [`RevenueOverview.cs`](../QLKhachSan/GUI/RevenueOverview.cs) | Nhận các `RevenueDay` từ dashboard; `SetData` gán số liệu rồi vẽ cột; mouse event hiện gợi ý. SQL được đọc trước khi gọi tệp này. | Không. |
| [`Ui.cs`](../QLKhachSan/GUI/Ui.cs) | `Error` đổi lỗi thành thông báo; `Log` ghi lỗi vào tệp cục bộ; `Confirm`, `Grid`, `Text`, `Money`, `Combo` tạo control chuẩn; `InputDialog.Action` chờ hàm lưu, khóa nút, báo lỗi rồi đóng khi thành công. | Không trực tiếp. Hàm `save` truyền vào có thể gọi BLL. |
| [`PaymentQr.cs`](../QLKhachSan/GUI/PaymentQr.cs) | Đọc cấu hình ngân hàng, dựng ảnh QR theo số tiền; đổi ô số tiền/phương thức thì ảnh cập nhật. Đây chỉ là hướng dẫn chuyển khoản; nhân viên phải xác nhận tiền thực nhận trước khi lưu `Payments`. | Không. |
| [`RememberedLogin.cs`](../QLKhachSan/GUI/RememberedLogin.cs) | `Load`/`Save`/`ForgetPassword` đọc/ghi tệp ghi nhớ dưới tài khoản Windows hiện tại, có bảo vệ DPAPI. Không phải cơ sở dữ liệu khách sạn. | Không. |

Ba tệp [`FormLogin.resx`](../QLKhachSan/GUI/FormLogin.resx), [`FormMain.resx`](../QLKhachSan/GUI/FormMain.resx), [`ucDashboard.resx`](../QLKhachSan/GUI/ucDashboard.resx) là tài nguyên đi cùng WinForms Designer. `*.Designer.cs` nói **control nào và đặt ở đâu**; `*.resx` giữ tài nguyên/metadata mà Designer cần. Chúng không chứa hàm xử lý nút hay SQL, nên muốn hiểu một thao tác hãy đọc `.Designer.cs` rồi `.cs` tương ứng.

### 6. Công thức tìm nguồn dữ liệu cho một con số trên màn hình

1. Tìm chữ hiển thị hoặc tên control bằng `rg -n 'HỒ SƠ KHÁCH|TênDịchVụ|btn' QLKhachSan/GUI`.
2. Trong `Designer.cs`, tìm dòng gắn sự kiện. Trong `.cs`, xem handler lấy `Text`, `Value` hay dòng đang chọn của bảng nào.
3. Tìm hàm `service.*Async` mà handler gọi. Đọc [BLL](#phan-17) để biết quyền và điều kiện.
4. Trong [DAL](#phan-18), xem câu `SELECT`/`INSERT`/`UPDATE` và `JOIN` để biết bảng, khóa liên kết, nơi lưu.
5. Quay lại `Render` hoặc `DataSource` của GUI để biết kết quả được hiển thị ở đâu. Nếu không thấy `INSERT`/`UPDATE`, thao tác đó chỉ đọc hoặc chỉ sửa màn hình.

Các nút nghiệp vụ cụ thể, kể cả trường nhập, điều kiện và lỗi, nằm trong [Thông tin chức năng](#mục-lục). Chương này giúp người đọc hiểu **cách đọc** mã giao diện; chương kia là bản đồ **từng chức năng**.


<a id="phan-17"></a>

## 17. Nghiệp vụ và dữ liệu

### 1. Vì sao có BLL và DTO?

GUI nhận điều người dùng nhập. BLL (Business Logic Layer) quyết định thao tác **có hợp lệ không**: đủ quyền chưa, phòng còn trống không, cọc có quá hạn không, hóa đơn đã chốt chưa. DAL thực sự gửi SQL. Nếu chỉ khóa một nút trên GUI, mã khác vẫn có thể gọi hàm; vì thế BLL kiểm tra lại trong giao dịch trước khi ghi. DTO (Data Transfer Object) là những “phiếu” có kiểu rõ ràng để các lớp trao đổi với nhau.

Ví dụ: [`GuestInput`](../QLKhachSan/DTO/HotelModels.cs#L13) là thông tin khách nhập; [`Stay`](../QLKhachSan/DTO/HotelModels.cs#L14) là lượt ở đọc từ `Stays`; [`BillQuote`](../QLKhachSan/DTO/HotelModels.cs#L52) là bảng tính trước checkout. `BillQuote` không tự lưu hóa đơn; chỉ `CheckoutAsync` mới ghi `Invoices`.

#### Tất cả mẫu dữ liệu trong `DTO/HotelModels.cs`

| Mẫu và dòng | Dùng để làm gì; nguồn hoặc nơi lưu |
| --- | --- |
| [`RoomStatus`, `StayStatus`](../QLKhachSan/DTO/HotelModels.cs#L3) | Các trạng thái cho phép; tương ứng chữ lưu trong `Rooms.Status`, `Stays.Status`. |
| [`UserSession`](../QLKhachSan/DTO/HotelModels.cs#L5) | Id/tên/vai trò/phiên đăng nhập trả sau khi kiểm tra `Users`; GUI truyền vào BLL để kiểm quyền. |
| [`Room`](../QLKhachSan/DTO/HotelModels.cs#L9) | Một hàng của `Rooms`, gồm số, loại, giá, cọc, trạng thái, `Version`. |
| [`GuestInput`](../QLKhachSan/DTO/HotelModels.cs#L13) | Tên/SĐT/giấy tờ từ form; BLL kiểm tra rồi DAL lưu hoặc cập nhật `Customers`, chép thông tin cần thiết vào `Stays`. |
| [`Stay`](../QLKhachSan/DTO/HotelModels.cs#L14) | Một hàng `Stays`: khách, phòng, ngày đến/đi, cọc, hạn nhận, phiên bản. |
| [`Segment`](../QLKhachSan/DTO/HotelModels.cs#L17) | Một đoạn ở tại một phòng với đơn giá đã chốt từ `StaySegments`; dùng tính tiền khi đổi phòng. |
| [`ServiceItem`, `ServiceCatalogItem`](../QLKhachSan/DTO/HotelModels.cs#L18) | Món đang bán hoặc cả món ngưng bán từ `Services`; dùng cho gọi món và quản lý danh mục. |
| [`ServiceLine`, `OrderInput`](../QLKhachSan/DTO/HotelModels.cs#L22) | Dòng món đã đặt từ `ServiceOrders`, và yêu cầu thêm món từ form. `ServiceLine.Total` trả 0 nếu đã hủy. |
| [`TodayScheduleItem`](../QLKhachSan/DTO/HotelModels.cs#L28) | Lượt dự kiến đến/đi hôm nay, tổng hợp từ `Stays` và `Rooms`. |
| [`Invoice`](../QLKhachSan/DTO/HotelModels.cs#L29) | Hóa đơn đã lưu ở `Invoices`; `Total` cộng tiền phòng và dịch vụ. |
| [`CustomerSummary`](../QLKhachSan/DTO/HotelModels.cs#L34) | Hàng trên hồ sơ khách: `Customers` nối `Stays` và `Invoices` để tính số lượt đã thanh toán/tổng chi. |
| [`RevenueItem`, `RevenueDay`](../QLKhachSan/DTO/HotelModels.cs#L35) | Doanh thu theo nhóm/theo ngày, tổng hợp từ `Invoices`, `ServiceOrders`, `Payments`. |
| [`DashboardData`](../QLKhachSan/DTO/HotelModels.cs#L41) | Một gói phòng, lượt, dịch vụ, doanh thu để `ucDashboard.Render` vẽ màn hình. |
| [`PaymentEntry`, `PeriodReport`, `DailyReport`](../QLKhachSan/DTO/HotelModels.cs#L43) | Hàng thu/chi và báo cáo; `PaymentEntry.CashFlow` tính tiền vào/ra, `Forfeit` bằng 0 vì đó không phải lần thu mới. |
| [`UserInfo`, `AuditEntry`](../QLKhachSan/DTO/HotelModels.cs#L49) | Hàng nhân viên từ `Users`, hàng nhật ký từ `AuditLog` nối `Users`. |
| [`BillQuote`](../QLKhachSan/DTO/HotelModels.cs#L52) | Phép tính checkout: tiền phòng, dịch vụ, cọc, cần thu, cần hoàn. Là dữ liệu tạm để người dùng xác nhận. |
| [`BusinessException`](../QLKhachSan/DTO/HotelModels.cs#L59) | Lỗi nghiệp vụ có câu dễ hiểu, được `Ui.Error` hiện thành thông báo. |

### 2. Đăng nhập, quyền và phiên

[`RolePolicy.cs:6`](../QLKhachSan/BLL/RolePolicy.cs#L6) cho biết vai trò nào được vận hành, xem tài chính, xem vận hành, quản lý tài khoản/danh mục hoặc bảo trì. Admin có quyền rộng nhất; Reception vận hành; Accountant xem tài chính; Manager xem vận hành/tài chính và quản lý danh mục theo chính sách mã. GUI dùng chính sách để ẩn nút; [`HotelService.cs:9`](../QLKhachSan/BLL/HotelService.cs#L9) dùng nó để từ chối lời gọi trái quyền.

| Hàm trong `AuthService` | Đầu vào → việc làm → dữ liệu |
| --- | --- |
| [`Hash`](../QLKhachSan/BLL/AuthService.cs#L11), `Validate` | Kiểm tên/mật khẩu; dùng PBKDF2 SHA-512, salt và số vòng lặp để tạo mã băm. SQL `Users` lưu băm/salt/số vòng, không lưu mật khẩu đọc được. |
| [`NeedsSetupAsync`](../QLKhachSan/BLL/AuthService.cs#L18) | Hỏi `UserCountAsync` → `SELECT COUNT_BIG` ở `Users` chưa lưu trữ để biết có cần tạo Admin đầu tiên. |
| [`SetupAsync`](../QLKhachSan/BLL/AuthService.cs#L19) | Nhận tên/mật khẩu lần đầu → kiểm không còn tài khoản, lưu Admin bằng `CreateUserAsync`, tạo `UserSession`. |
| [`LoginAsync`](../QLKhachSan/BLL/AuthService.cs#L33) | Nhận tên/mật khẩu → `AccountAsync` đọc `Users` → kiểm băm, trạng thái khóa và lần thử → `LoginResultAsync` cập nhật đếm thất bại hoặc xóa đếm khi đúng → trả phiên. |
| [`CreateUserAsync`](../QLKhachSan/BLL/AuthService.cs#L54) | Admin nhập tài khoản mới và vai trò → kiểm quyền, băm mật khẩu, thêm `Users`, ghi `AuditLog`. |
| [`ChangePasswordAsync`](../QLKhachSan/BLL/AuthService.cs#L67) | Người đang đăng nhập đưa mật khẩu cũ/mới → xác minh cũ → `ChangePasswordAsync` tại DAL cập nhật băm/salt, tăng `SecurityVersion` để phiên cũ mất hiệu lực. |
| [`EmployeesAsync`, `UsersAsync`](../QLKhachSan/BLL/AuthService.Management.cs#L8) | Đọc danh sách `Users` theo quyền để hiện màn nhân viên/tài khoản. |
| [`UpdateUserAsync`](../QLKhachSan/BLL/AuthService.Management.cs#L18) | Admin đổi vai trò/khóa mở → kiểm không tự khóa và còn Admin hợp lệ → DAL cập nhật `Users`/ghi audit. |
| [`ResetPasswordAsync`](../QLKhachSan/BLL/AuthService.Management.cs#L29) | Admin đặt mật khẩu mới cho nhân viên → băm rồi lưu vào `Users`, tăng `SecurityVersion`. |

`UserSession` không phải bản quyền vĩnh viễn: [`HotelTransaction.RequireUserAsync`](../QLKhachSan/DAL/HotelRepository.cs#L77) đối chiếu lại `Users.Active`, vai trò và `SecurityVersion` ở mỗi giao dịch. Bởi vậy khi quản trị viên khóa tài khoản hoặc đổi quyền, phiên cũ không thể tiếp tục thao tác.

**Đọc mã hiện tại cho đúng:** `Validate` đang giới hạn mật khẩu mới ở 1–5 ký tự, còn form thiết lập Admin cũng báo giới hạn này. Mã băm bảo vệ cách lưu mật khẩu, nhưng giới hạn ngắn như vậy không phù hợp để triển khai với tài khoản thật; khi thay chính sách phải sửa đồng bộ BLL, ô nhập và tài liệu.

### 3. Khung kiểm tra chung của `HotelService`

[`Read`, `FinanceRead`, `OperationsRead`, `Write`, `CatalogWrite`](../QLKhachSan/BLL/HotelService.cs#L9) đều gọi `repository.RunAsync`. Chúng khác nhau ở điều kiện vai trò và kiểu khóa đọc/ghi. Đây là cửa bắt buộc trước các hàm dưới đây. [`ValidateGuest`](../QLKhachSan/BLL/HotelService.cs#L27) kiểm tra thông tin khách; `Method` kiểm phương thức thanh toán; `RoomUnchanged`/`StayUnchanged` so `Version` của form cũ với SQL mới để tránh ghi đè. [`ReservationHoldLimit`](../QLKhachSan/BLL/HotelService.cs#L47) cho hạn giữ tối đa: 1 ngày chưa cọc, 15 ngày đã cọc kể từ lúc tạo.

#### Đọc dữ liệu

| Hàm BLL | Hàm DAL / bảng nguồn → kết quả |
| --- | --- |
| [`DashboardAsync`](../QLKhachSan/BLL/HotelService.cs#L14) | `RoomsAsync`, `ActiveStaysAsync`, `PendingAsync`, `RevenueAsync` và các hàm liên quan → `DashboardData` cho `Render`. |
| [`CustomersAsync`](../QLKhachSan/BLL/HotelService.cs#L21) | `CustomersAsync` → `Customers` LEFT JOIN `Stays` LEFT JOIN `Invoices`, trả danh sách hồ sơ. |
| [`CustomerInvoicesAsync`, `CustomerOrdersAsync`](../QLKhachSan/BLL/HotelService.cs#L22) | Theo `CustomerId`, DAL nối `Stays` với `Invoices`/`ServiceOrders`, trả hai tab bên dưới hồ sơ. |
| [`InvoicesAsync`, `RevenueAsync`, `ReportAsync`](../QLKhachSan/BLL/HotelService.cs#L24) | Đọc hóa đơn, nhóm doanh thu cho một ngày từ `Invoices`, `ServiceOrders`, `Payments`. |
| [`ServerNowAsync`, `RefundQuoteAsync`](../QLKhachSan/BLL/HotelService.Management.cs#L18) | Giờ từ `SYSDATETIME()` của SQL; số tiền có thể hoàn từ lượt đặt và hạn nhận hiện tại. |
| [`StayHistoryAsync`, `TodayScheduleAsync`, `StayOrdersAsync`](../QLKhachSan/BLL/HotelService.Management.cs#L24) | Lịch sử `Stays`, lịch dự kiến, tất cả món của một lượt gồm món hủy. |
| [`PeriodReportAsync`](../QLKhachSan/BLL/HotelService.Management.cs#L42) | Khoảng ngày → `Invoices`, doanh thu, `Payments`; trả `PeriodReport`. |
| [`InvoiceStayAsync`, `InvoiceOrdersAsync`](../QLKhachSan/BLL/HotelService.Management.cs#L48) | Tìm lượt và món tương ứng hóa đơn để xem/in. |
| [`CatalogAsync`, `AuditsAsync`](../QLKhachSan/BLL/HotelService.Management.cs#L106) | Danh mục `Services`; nhật ký `AuditLog` theo ngày. |

#### Thao tác ghi: nhận gì, kiểm gì, lưu đâu?

| Hàm BLL | Diễn giải luồng và bảng được thay đổi |
| --- | --- |
| [`CreateStayAsync`](../QLKhachSan/BLL/HotelService.cs#L48) | Form đưa phòng, khách, ngày đến, số ngày, cọc, hạn nhận. BLL kiểm quyền/khách/ngày/phòng/lịch trùng, đọc giờ SQL; DAL thêm hoặc tìm `Customers`, thêm `Stays`; nhận ngay còn đổi `Rooms`, thêm `StaySegments`; có cọc thì thêm `Payments`; ghi `AuditLog`. |
| [`CheckInAsync`](../QLKhachSan/BLL/HotelService.cs#L72) | Chọn lượt đặt → kiểm hạn nhận, phiên bản, phòng trống và lịch → đổi `Stays` thành Occupied, `Rooms` thành đang ở, thêm `StaySegments`, ghi audit. |
| [`CancelAsync`](../QLKhachSan/BLL/HotelService.cs#L85) | Chọn lượt đặt và phương thức hoàn → tính số hoàn theo giờ SQL; đổi `Stays` thành Cancelled, nếu hoàn thì thêm `Payments` loại Refund; ghi audit. |
| [`ExpireReservationsAsync`](../QLKhachSan/BLL/HotelService.cs#L99) | Dashboard định kỳ gọi → tìm đặt quá hạn chưa nhận, đóng `Stays`; cọc đã thu được ghi `Payments.Kind='Forfeit'` để báo cáo, không tạo tiền vào lần nữa. |
| [`TransferAsync`](../QLKhachSan/BLL/HotelService.cs#L112) | Chọn lượt đang ở và phòng mới → kiểm trống/lịch → kết thúc `StaySegments` cũ, mở đoạn mới với giá phòng mới, cập nhật `Stays.RoomId`, đổi trạng thái hai `Rooms`, ghi audit. |
| [`ExtendAsync`](../QLKhachSan/BLL/HotelService.cs#L128) | Số ngày thêm → kiểm không đè lịch đã đặt → cập nhật `Stays.Departure` và phiên bản, ghi audit. |
| [`AddServicesAsync`](../QLKhachSan/BLL/HotelService.cs#L138) | Giỏ `OrderInput` → kiểm lượt còn hoạt động, món còn bán, số lượng → thêm `ServiceOrders` (chép giá/tên lúc đặt), tăng phiên bản `Stays`, audit. |
| [`DeliverAsync`](../QLKhachSan/BLL/HotelService.cs#L151) | Giao toàn bộ dịch vụ chưa giao của lượt → cập nhật `ServiceOrders.DeliveredQuantity`/`Delivered`, audit. |
| [`SetRoomStatusAsync`, `CleanAllAsync`](../QLKhachSan/BLL/HotelService.cs#L157) | Bảo trì hoặc dọn phòng → kiểm trạng thái và quyền → cập nhật `Rooms.Status`/`Version`, audit. |
| [`QuoteAsync`](../QLKhachSan/BLL/HotelService.cs#L186) | **Chỉ đọc** `Stays`, `StaySegments`, `ServiceOrders`, `Rooms`; `Quote` tính tiền tạm, trả `BillQuote` để người dùng xem. |
| [`CheckoutAsync`](../QLKhachSan/BLL/HotelService.cs#L191) | Người dùng xác nhận `BillQuote` và phương thức → đọc lại phiên bản/giá trong giao dịch, kiểm dịch vụ đã giao → thêm `Invoices`, thêm `Payments` phần thu/hoàn, đóng `Stays`/`StaySegments`, đổi `Rooms` sang chờ dọn, audit. |
| [`UpdateBookingAsync`](../QLKhachSan/BLL/HotelService.Management.cs#L27) | Thay khách/phòng/ngày/hạn nhận với lý do → kiểm quyền, lịch và bản mới nhất → cập nhật `Customers`/`Stays`, audit; giữ cọc đã thu. |
| [`AddDepositAsync`](../QLKhachSan/BLL/HotelService.Management.cs#L50) | Số tiền thực thu/phương thức → kiểm hạn và số dương → tăng `Stays.Deposit`, thêm `Payments.Kind='Deposit'`, audit. |
| [`ChangeOrderAsync`, `DeliverOrderAsync`, `CancelPendingOrdersAsync`](../QLKhachSan/BLL/HotelService.Management.cs#L64) | Sửa số lượng, giao từng phần hoặc hủy món còn chờ → cập nhật `ServiceOrders`, tăng phiên bản `Stays`, ghi audit; món đã hủy giữ để tra cứu. |
| [`SaveServiceAsync`, `SaveRoomAsync`, `UpdateRoomsAsync`](../QLKhachSan/BLL/HotelService.Management.cs#L107) | Form danh mục/giá → kiểm quyền, giá và phiên bản → thêm/sửa `Services` hoặc `Rooms`, audit. |

### 4. Tính tiền phòng bằng một ví dụ nhỏ

[`BillingPolicy.RoomCharge`](../QLKhachSan/BLL/BillingPolicy.cs#L9) nhận các `Segment` và thời điểm trả. Một đoạn là thời gian ở một phòng với **giá chụp tại lúc vào phòng** (`StaySegments.Rate`), nên việc đổi giá danh mục về sau không sửa lịch sử. Hàm kiểm tra các đoạn nối liên tục; tính thời gian thực của từng đoạn theo giá tương ứng; làm tròn **tổng thời gian ở** lên ngày 24 giờ, tối thiểu một ngày. Thời gian còn thiếu của ngày cuối tính theo giá đoạn cuối, sau đó làm tròn tiền VND. Ví dụ khách ở 30 giờ: tính thành hai ngày, không làm tròn riêng từng đoạn khi khách đổi phòng. Công thức cụ thể nằm ngay trong hàm; hãy đặt breakpoint và xem `segments`, `checkout`, kết quả sau mỗi bước.

### 5. Cách tự kiểm tra một hàm BLL

Lấy `AddDepositAsync` làm ví dụ: (1) tìm nút **Thu cọc bổ sung** trong [GUI](#phan-16); (2) xem BLL nhận `Stay selected`, `amount`, `method`; (3) xem `Write` kiểm quyền và `StayUnchanged` kiểm dữ liệu cũ; (4) tìm lời gọi DAL `AddDepositAsync`, `PaymentAsync`, `AuditAsync`; (5) mở [SQL](#phan-18) để thấy `UPDATE Stays`, `INSERT Payments`, `INSERT AuditLog`; (6) xem GUI gọi `Reload` và số cọc mới hiện ở đâu. Làm y hệt với mỗi dòng trong bảng trên.


<a id="phan-18"></a>

## 18. Cơ sở dữ liệu và cấu hình

### 1. Chạy chương trình cần những tệp nào?

[`QLKhachSan.slnx`](../QLKhachSan.slnx) gom solution; [`QLKhachSan.csproj`](../QLKhachSan/QLKhachSan.csproj) chọn .NET 10 Windows, WinForms, gói `Microsoft.Data.SqlClient`, sao chép `appsettings.json` vào thư mục chạy và nhúng các script migration. [`appsettings.json`](../QLKhachSan/appsettings.json) chứa chuỗi kết nối SQL, cấu hình QR và thông tin khách sạn. Không nên public mật khẩu SQL hay tài khoản thật cùng mã nguồn. [`AppSettings.Load`](../QLKhachSan/DAL/DatabaseHelper.cs#L18) đọc tệp cấu hình ở thư mục ứng dụng; biến môi trường `QLKHACHSAN_CONNECTION_STRING` có thể thay chuỗi kết nối. [`DatabaseHelper.GetConnection`](../QLKhachSan/DAL/DatabaseHelper.cs#L29) tạo `SqlConnection` từ đó.

Trước khi dùng app mới, chạy [`Database/Setup.sql`](../Database/Setup.sql) trên SQL Server để tạo bảng/dữ liệu mẫu. Khi mở login, [`SchemaMigrator.EnsureAsync`](../QLKhachSan/DAL/SchemaMigrator.cs#L7) xem `SchemaVersion` và thực hiện lần lượt `MigrateV2.sql` đến `MigrateV5.sql` còn thiếu. Mỗi migration thay đổi cấu trúc hoặc dữ liệu để chương trình mới đọc được database cũ. `Setup.sql` không phải nút xóa và tạo lại mỗi lần mở app.

### 2. Sơ đồ bảng: số Id nối dữ liệu như thế nào?

```text
Users.Id ───────────┬── Stays.CreatedBy
                    ├── ServiceOrders.CreatedBy
                    ├── Payments.CreatedBy
                    ├── Invoices.CreatedBy
                    └── AuditLog.UserId

Customers.Id ── Stays.CustomerId
Rooms.Id ────── Stays.RoomId ────── StaySegments.RoomId
Stays.Id ──────┬── StaySegments.StayId
               ├── ServiceOrders.StayId ── Services.Id (ServiceId)
               ├── Payments.StayId
               └── Invoices.StayId
```

`Id` là mã riêng của một hàng. `CustomerId` trên `Stays` cho biết lượt này thuộc khách nào. Khóa ngoại trong [`Setup.sql:20`](../Database/Setup.sql#L20) giữ các liên kết hợp lệ. Tên/giá món và giá phòng còn được **chép vào giao dịch lịch sử**: `ServiceOrders.Name/Price`, `StaySegments.Rate`. Nhờ đó sửa bảng giá hôm nay không làm hóa đơn cũ thay đổi. Phần lớn bảng được tạo ở [`Setup.sql:20–79`](../Database/Setup.sql#L20), phòng/món mẫu ở khoảng dòng 94 trở đi.

| Bảng | Mỗi hàng nghĩa là gì | Ai ghi; ai đọc |
| --- | --- | --- |
| `dbo.Users` | Một tài khoản: vai trò, băm mật khẩu, trạng thái, phiên bảo mật | `AuthService` → DAL tạo/đổi; login, phân quyền, màn nhân viên đọc. |
| `dbo.Customers` | Một hồ sơ tên, SĐT, giấy tờ | Tạo/sửa đặt phòng ghi; hồ sơ khách tìm và nối lịch sử. |
| `dbo.Rooms` | Một phòng và trạng thái **vật lý**, giá/cọc hiện tại | Danh mục, nhận/trả/dọn/bảo trì ghi; sơ đồ phòng đọc. Lịch đặt tương lai nằm ở `Stays`, không chiếm trạng thái phòng hôm nay. |
| `dbo.Stays` | Một lần đặt hoặc ở: khách, phòng, khoảng ngày, cọc, hạn nhận, trạng thái | Tạo/nhận/sửa/đổi/gia hạn/hủy/trả ghi; hầu hết màn vận hành đọc. |
| `dbo.StaySegments` | Một đoạn thời gian ở một phòng với giá tại lúc đó | Nhận phòng mở, đổi phòng đóng/mở, trả phòng đóng; `BillingPolicy` đọc để tính tiền. |
| `dbo.Services` | Danh mục món/dịch vụ có thể chọn | Admin/Manager sửa; form gọi món đọc món còn hoạt động. |
| `dbo.ServiceOrders` | Một dòng món đã gọi, số lượng/giao/hủy và giá chụp lại | Gửi yêu cầu/giao/sửa/hủy ghi; chờ dịch vụ, hồ sơ khách, hóa đơn đọc. |
| `dbo.Payments` | Một lần thu cọc, thu lúc trả, hoàn, hoặc ghi nhận cọc mất | Thu cọc/hủy/trả/quá hạn ghi; thu chi/báo cáo đọc. `Forfeit` là phân loại cọc cũ, `CashFlow=0`. |
| `dbo.Invoices` | Hóa đơn được chốt một lần cho một lượt ở | Checkout ghi; hồ sơ khách, doanh thu, in phiếu đọc. |
| `dbo.AuditLog` | Ai làm gì, khi nào, chi tiết | Các nghiệp vụ ghi; màn nhật ký đọc. |
| `dbo.SchemaVersion` | Mức cấu trúc database hiện tại | Migration đọc/ghi; không phải dữ liệu khách sạn. |

Ví dụ ảnh hồ sơ khách: [`CustomersAsync`](../QLKhachSan/DAL/HotelRepository.cs#L111) dùng `Customers c LEFT JOIN Stays s ON s.CustomerId=c.Id LEFT JOIN Invoices i ON i.StayId=s.Id`. `COUNT(i.Id)` đếm hóa đơn, `SUM(i.RoomCharge+i.ServiceCharge)` cộng tiền đã chốt. Nhấn một khách thì [`CustomerOrdersAsync`](../QLKhachSan/DAL/HotelRepository.cs#L112) lấy món theo `s.CustomerId`; [`CustomerInvoicesAsync`](../QLKhachSan/DAL/HotelRepository.cs#L115) lấy hóa đơn cũng theo khóa đó. “Không thấy món” nghĩa là truy vấn không tìm thấy hàng phù hợp cho khách đang chọn, chưa chắc là món bị xóa. Chi tiết trên ảnh ở [Thông tin chức năng](#mục-lục).

### 3. `HotelRepository`: lớp mở giao dịch và đọc dữ liệu

[`HotelRepository.RunAsync`](../QLKhachSan/DAL/HotelRepository.cs#L12) mở `SqlConnection`, bắt đầu transaction mức `Serializable`, lấy khóa ứng dụng qua [`LockAsync`](../QLKhachSan/DAL/HotelRepository.cs#L75), gọi hàm được BLL đưa vào rồi commit. Nếu có lỗi, giao dịch rollback: ví dụ checkout đã thêm hóa đơn nhưng lỗi khi đổi phòng thì cả gói bị hủy. Khóa `Shared` cho đọc, `Exclusive` cho ghi; nhờ đó các thao tác đồng thời không cùng chiếm một phòng. `RequireUserAsync` cũng kiểm lại quyền phiên từ `Users`.

[`Param`, `Command`](../QLKhachSan/DAL/HotelRepository.cs#L36) đặt giá trị vào tham số `@p0`, `@p1` thay vì nối thẳng chữ người dùng vào SQL; [`Execute`](../QLKhachSan/DAL/HotelRepository.cs#L56) trả số hàng thay đổi, [`Scalar`](../QLKhachSan/DAL/HotelRepository.cs#L61) trả một giá trị, [`Query`](../QLKhachSan/DAL/HotelRepository.cs#L66) đọc nhiều hàng và dùng hàm `map` tạo DTO. Ví dụ `MapRoom` biến một hàng `SqlDataReader` thành `Room`; `MapStay` biến hàng `Stays` thành `Stay`. `NowAsync` hỏi giờ máy chủ SQL, tránh quyết định quá hạn bằng đồng hồ máy trạm.

| Nhóm hàm đọc trong [`HotelRepository.cs`](../QLKhachSan/DAL/HotelRepository.cs) | SQL chính → nơi kết quả đi đến |
| --- | --- |
| `UserCountAsync`, `AccountAsync`, `LoginResultAsync`, `CreateUserAsync`, `ChangePasswordAsync` | `Users`; kết quả vào login/quản lý tài khoản. Hai hàm đầu chỉ đọc; ba hàm sau ghi. |
| `RoomsAsync`, `RoomAsync` | `SELECT ... FROM Rooms` → sơ đồ, chọn phòng, kiểm tra trước ghi. |
| `ActiveStaysAsync`, `StayAsync`, `SegmentsAsync` | `Stays`, `StaySegments` → lịch và tính tiền. |
| `MenuAsync`, `OrdersAsync`, `AllOrdersAsync`, `PendingAsync` | `Services`, `ServiceOrders` → gọi món, giao món, lịch sử. `OrdersAsync` bỏ món hủy; `AllOrdersAsync` vẫn trả chúng. |
| `RevenueAsync`, `InvoicesAsync` | `Invoices` kết hợp `ServiceOrders` và `Payments` → doanh thu và hóa đơn theo khoảng `[từ, đến)`. |
| `CustomersAsync`, `CustomerOrdersAsync`, `CustomerInvoicesAsync` | Liên kết theo `CustomerId`/`StayId` → ba bảng trên màn hồ sơ khách. |
| `AuditAsync` | `INSERT AuditLog` với người làm và nội dung; màn nhật ký đọc bằng `AuditsAsync` ở tệp Management. |

`[từ, đến)` nghĩa là gồm đầu khoảng nhưng không gồm đầu ngày sau. Ví dụ xem ngày 28/09 sẽ dùng `>= 28/09 00:00` và `< 29/09 00:00`; dữ liệu đúng nửa đêm ngày 29 thuộc ngày 29.

### 4. Lệnh ghi trong `HotelTransaction.Commands.cs`

| Hàm và dòng | SQL tác động đến đâu? | Vì sao có bước ấy? |
| --- | --- | --- |
| [`CreateStayAsync:7`](../QLKhachSan/DAL/HotelTransaction.Commands.cs#L7) | Tìm/thêm/cập nhật `Customers`, `INSERT Stays` | Một khách có thể có nhiều lượt; lượt mang lịch và thông tin lúc đặt. |
| [`SetRoomAsync:13`](../QLKhachSan/DAL/HotelTransaction.Commands.cs#L13) | `UPDATE Rooms.Status/Version` | Sơ đồ phản ánh tình trạng vật lý và báo form cũ đã lỗi thời. |
| [`StartSegmentAsync`, `EndSegmentAsync`](../QLKhachSan/DAL/HotelTransaction.Commands.cs#L18) | `INSERT`/`UPDATE StaySegments` | Ghi thời điểm và giá của từng đoạn để tính tiền khi đổi phòng. |
| [`CheckInAsync`, `TransferAsync`, `ExtendAsync`, `CloseStayAsync`](../QLKhachSan/DAL/HotelTransaction.Commands.cs#L20) | `UPDATE Stays` | Đổi trạng thái, phòng, ngày trả; `CloseStayAsync` đặt `IsActive=0`. |
| [`TouchStayAsync`](../QLKhachSan/DAL/HotelTransaction.Commands.cs#L24) | Tăng `Stays.Version` | Buộc form báo giá cũ phải đọc lại sau khi dịch vụ thay đổi. |
| [`AddOrderAsync`, `DeliverAsync`](../QLKhachSan/DAL/HotelTransaction.Commands.cs#L25) | `INSERT`/`UPDATE ServiceOrders` | Giữ ảnh chụp món và đánh dấu đã giao. |
| [`PaymentAsync`](../QLKhachSan/DAL/HotelTransaction.Commands.cs#L27) | `INSERT Payments` nếu số tiền cần ghi phù hợp | Giữ lịch sử từng lần thu/hoàn để đối chiếu, thay vì chỉ có tổng cọc. |
| [`InvoiceAsync`](../QLKhachSan/DAL/HotelTransaction.Commands.cs#L29) | `INSERT Invoices`, trả `INSERTED.Id` | Tạo chứng từ đã chốt và lấy mã hóa đơn cho GUI/in. |

### 5. Lệnh quản lý trong `HotelTransaction.Management.cs`

| Hàm | Nguồn hoặc nơi lưu |
| --- | --- |
| [`RevenueTrendAsync`](../QLKhachSan/DAL/HotelTransaction.Management.cs#L7) | Gom hóa đơn/cọc mất theo ngày cho biểu đồ. |
| [`EnsureAvailableAsync`](../QLKhachSan/DAL/HotelTransaction.Management.cs#L13) | `SELECT COUNT_BIG` trong `Stays` để chặn khoảng ngày phòng giao nhau; `excluding` bỏ lượt đang sửa. |
| [`StayHistoryAsync`, `TodayScheduleAsync`](../QLKhachSan/DAL/HotelTransaction.Management.cs#L19) | Lịch sử và lịch đến/đi từ `Stays`, có thông tin `Rooms`. |
| [`AddDepositAsync`, `UpdateBookingAsync`](../QLKhachSan/DAL/HotelTransaction.Management.cs#L37) | Tăng cọc/đổi thông tin trong `Stays`, cập nhật hồ sơ `Customers` khi sửa khách. |
| [`UpdateOrderAsync`, `CancelOrderAsync`, `DeliverOrderAsync`](../QLKhachSan/DAL/HotelTransaction.Management.cs#L43) | Sửa số lượng/hủy/giao từng phần trong `ServiceOrders`. |
| [`PaymentsAsync`](../QLKhachSan/DAL/HotelTransaction.Management.cs#L46) | `Payments JOIN Stays JOIN Users` → bảng thu chi có tên khách và nhân viên. |
| [`UsersAsync`, `ActiveAdminsAsync`, `ArchiveUsersAsync`, `UpdateUserAsync`](../QLKhachSan/DAL/HotelTransaction.Management.cs#L47) | Đọc/ghi `Users`; đếm Admin còn hiệu lực trước khi thay đổi. |
| [`CatalogAsync`, `SaveServiceAsync`, `SaveRoomAsync`](../QLKhachSan/DAL/HotelTransaction.Management.cs#L51) | Danh mục `Services` và `Rooms`; `Id=0` nghĩa là thêm mới, `Id>0` là sửa có kiểm phiên bản. |
| [`AuditsAsync`](../QLKhachSan/DAL/HotelTransaction.Management.cs#L58) | `AuditLog JOIN Users`, tối đa 1.000 hàng trong ngày. |

### 6. Migration và những ràng buộc phải hiểu

| Tệp | Thay đổi chính |
| --- | --- |
| [`Setup.sql`](../Database/Setup.sql) | Tạo 10 bảng nghiệp vụ và `SchemaVersion`, dữ liệu phòng/dịch vụ mẫu, khóa chính và khóa ngoại. |
| [`MigrateV2.sql`](../Database/MigrateV2.sql) | Thêm `SecurityVersion`, phiên bản danh mục, trạng thái giao/hủy món, một số unique index; đưa phòng `DaDat` cũ về trạng thái vật lý `Trong`. |
| [`MigrateV3.sql`](../Database/MigrateV3.sql) | Bốn vai trò và lưu trữ tài khoản cũ (`Archived`) khi nâng từ mô hình đăng nhập cũ. |
| [`MigrateV4.sql`](../Database/MigrateV4.sql) | Bổ sung loại phòng và phòng mẫu 401–410. |
| [`MigrateV5.sql`](../Database/MigrateV5.sql) | Điều chỉnh phân loại phòng 401–410. |

Một `UNIQUE` index bảo vệ quy tắc ngay trong SQL, ví dụ không tạo hai hóa đơn cho cùng lượt. `Version` giúp phát hiện hai người mở cùng form; BLL/DAL chỉ lưu nếu dữ liệu gốc còn đúng. `AuditLog` giúp tìm ai đã làm thao tác, nhưng không thay thế bản sao lưu dữ liệu. Khi tự sửa bảng, hãy hiểu cả khóa ngoại, index và migration; thêm cột thủ công vào một máy sẽ làm máy khác thiếu cột khi public source.

### 7. Cách đọc một câu SQL lần đầu

Đọc từ `FROM` trước để biết **bảng chính**; đọc `JOIN ... ON` để biết **nối bằng mã nào**; đọc `WHERE` để biết **chọn hàng nào**; rồi đọc `SELECT` để biết **trả cột nào**. Trong `CustomerInvoicesAsync`, `FROM Invoices i JOIN Stays s ON s.Id=i.StayId WHERE s.CustomerId=@p0` nghĩa là lấy hóa đơn của những lượt thuộc khách có Id do GUI gửi xuống. `@p0` là tham số, không phải chữ ghép vào SQL. Sau SQL, xem hàm `r => new Invoice(...)`: đó là thứ tự cột đi vào DTO; cuối cùng tìm nơi GUI gán `DataSource` để biết dữ liệu hiện ở đâu.


<a id="phan-19"></a>

## 19. Ví dụ theo dõi một thao tác

### Chuẩn bị

Đọc [README](../README.md) để cài .NET/SQL Server, tạo database bằng `Setup.sql` và chạy app. Dùng **database thử nghiệm**, không dùng dữ liệu thật khi làm bài. Trong Visual Studio, nhấn vào lề trái cạnh dòng mã để đặt breakpoint; chạy `F5`; dùng `F10` đi qua một dòng, `F11` bước vào hàm. Cửa sổ **Locals** hiện giá trị biến. `Ctrl+Shift+F` tìm tên hàm trong toàn solution. Nếu mã đã đổi và số dòng tài liệu lệch, tìm theo **tên hàm**.

Một chức năng hoàn chỉnh thường có năm câu trả lời: **(1) ai kích hoạt**, **(2) hàm nào nhận dữ liệu**, **(3) BLL kiểm điều gì**, **(4) DAL đọc/ghi bảng nào, nối bằng cột nào**, **(5) kết quả hiển thị và tồn tại ở đâu**. Viết năm câu này cho mỗi bài; đừng chỉ chép tên lớp.

### Ví dụ 1. Từ nút Đăng nhập đến `Users`

1. Mở [`FormLogin.Designer.cs:113`](../QLKhachSan/GUI/FormLogin.Designer.cs#L113), xem sự kiện `Click` gọi tên hàm nào.
2. Đặt breakpoint ở [`btnDangNhap_Click`](../QLKhachSan/GUI/FormLogin.cs#L108). Xem tên/mật khẩu đến từ ô nhập nào. Nếu mới tạo database, nhánh nào gọi `SetupAsync`?
3. Bước vào [`AuthService.LoginAsync`](../QLKhachSan/BLL/AuthService.cs#L33), xem nó gọi `AccountAsync`; ở [`HotelRepository.cs:83`](../QLKhachSan/DAL/HotelRepository.cs#L83), đọc `FROM`, `WHERE` của câu SQL.
4. Trả lời: database có cột mật khẩu gốc không? Khi nhập sai, hàm nào tăng số lần thất bại? Khi đúng, `UserSession` đi đến form nào?

**Gợi ý đáp án:** nguồn là ô login; `Users` chứa băm, salt, số vòng; `LoginResultAsync` cập nhật lần thử; `Program.Main` nhận phiên và tạo `FormMain`. Tệp `RememberedLogin` là ghi nhớ cục bộ của Windows, không phải chứng thực SQL.

### Ví dụ 2. Phòng 101 hiện “Trống” từ đâu?

1. Trong [`ucDashboard.cs:110`](../QLKhachSan/GUI/ucDashboard.cs#L110), theo `Reload` đến [`HotelService.DashboardAsync`](../QLKhachSan/BLL/HotelService.cs#L14).
2. Trong [`RoomsAsync`](../QLKhachSan/DAL/HotelRepository.cs#L94), chỉ ra `FROM dbo.Rooms`; xem `MapRoom` biến các cột thành `Room`.
3. Trong [`Render`](../QLKhachSan/GUI/ucDashboard.cs#L132) và [`AppTheme.RoomColor`](../QLKhachSan/GUI/AppTheme.cs#L45), xem trạng thái thành chữ/màu thế nào. Dấu có lịch đặt lấy từ `Stays`, nên phòng có lịch tương lai vẫn có thể hiện “Trống” hôm nay.
4. Trả lời: đổi màu trên `RoomTile` có đổi `Rooms.Status` không? Muốn lưu trạng thái, cần hàm BLL/DAL nào?

**Gợi ý đáp án:** `Render` chỉ vẽ; một thao tác bảo trì/dọn/nhận/trả gọi BLL tương ứng rồi DAL `SetRoomAsync` mới cập nhật bảng.

### Ví dụ 3. Hồ sơ khách trong ảnh: từng cột đến từ đâu?

1. Tìm [`btnQuanLyKhach_Click`](../QLKhachSan/GUI/ucDashboard.Reports.cs#L76); mở hàm cục bộ [`LoadCustomers`](../QLKhachSan/GUI/ucDashboard.Reports.cs#L103).
2. Theo `CustomersAsync(search)` qua [`HotelService`](../QLKhachSan/BLL/HotelService.cs#L21) đến [`HotelRepository`](../QLKhachSan/DAL/HotelRepository.cs#L111). Đọc `Customers c LEFT JOIN Stays s ... LEFT JOIN Invoices i ...`.
3. Ghi mỗi cột: **Họ tên** = `Customers.Name`; **SĐT** = `Customers.Phone`; **CCCD/Hộ chiếu** = `Customers.IdentityNumber`; **Lượt đã thanh toán** = số hóa đơn `Invoices.Id` nối theo lượt; **Tổng chi** = tổng `Invoices.RoomCharge + ServiceCharge`. Các giá trị cuối là **tổng hợp**, không có cột `TongChi` trong `Customers`.
4. Chọn một khách; xem handler [`ucDashboard.Reports.cs:118`](../QLKhachSan/GUI/ucDashboard.Reports.cs#L118) gọi `CustomerOrdersAsync(customer.Id)` và `CustomerInvoicesAsync(customer.Id)`. Viết đường nối để lấy dịch vụ: `Customers.Id → Stays.CustomerId → Stays.Id → ServiceOrders.StayId`.
5. Trả lời: hai tab trống là vì không có kết quả truy vấn hay dữ liệu vừa bị xóa? Hàng dịch vụ đã hủy có thể có `Total=0` theo DTO không?

**Gợi ý đáp án:** bảng trống có nghĩa truy vấn trả 0 hàng với khách đã chọn; không chứng minh có lệnh `DELETE`. `CustomerOrdersAsync` lấy cả dòng hủy và `ServiceLine.Total` không tính dòng hủy. Xem [mô tả đầy đủ theo ảnh](#mục-lục).

### Ví dụ 4. Từ Đặt trước đến tiền cọc

1. Trong [`ucDashboard.Actions.cs`](../QLKhachSan/GUI/ucDashboard.Actions.cs), tìm `ShowBooking`, xem form tạo `GuestInput`, chọn phòng, ngày, phương thức và cọc.
2. Theo [`HotelService.CreateStayAsync`](../QLKhachSan/BLL/HotelService.cs#L48): liệt kê điều kiện về phòng, ngày giao nhau, hạn giữ, quyền người dùng.
3. Theo [`HotelTransaction.CreateStayAsync`](../QLKhachSan/DAL/HotelTransaction.Commands.cs#L7): `Customers` và `Stays` nhận dữ liệu gì? Nếu thu cọc, [`PaymentAsync`](../QLKhachSan/DAL/HotelTransaction.Commands.cs#L27) ghi bảng nào?
4. Tạo một lượt thử. Mở màn lịch sử, xem lượt từ `Stays`; mở thu chi, xem khoản cọc từ `Payments`. So sánh `Stays.Deposit` (tổng cọc của lượt) với nhiều dòng `Payments` (lịch sử từng lần thu).

**Gợi ý đáp án:** một số tiền có thể xuất hiện trong nhiều màn vì chúng cùng tham chiếu một lượt qua `StayId`; không phải mỗi màn lưu một bản ghi cọc độc lập.

### Ví dụ 5. Gọi dịch vụ, giao món, rồi trả phòng

1. Thêm một món vào giỏ, **chưa nhấn Gửi yêu cầu**. Tìm biến giỏ ở [`ucDashboard.Actions.cs`](../QLKhachSan/GUI/ucDashboard.Actions.cs). Đã có `ServiceOrders` mới trong SQL chưa?
2. Nhấn gửi, theo [`AddServicesAsync`](../QLKhachSan/BLL/HotelService.cs#L138) đến [`AddOrderAsync`](../QLKhachSan/DAL/HotelTransaction.Commands.cs#L25). Món trong `Services` cung cấp tên/giá hiện tại; `ServiceOrders` giữ tên/giá đã chụp vào ngày gọi.
3. Giao món bằng [`DeliverOrderAsync`](../QLKhachSan/BLL/HotelService.Management.cs#L83); xem `DeliveredQuantity`, `Delivered` trong `ServiceOrders`. Hãy thử mở báo giá khi còn món chưa giao và đọc điều kiện BLL.
4. Theo [`QuoteAsync`](../QLKhachSan/BLL/HotelService.cs#L186): `StaySegments` cho tiền phòng, `ServiceOrders` cho dịch vụ, `Stays.Deposit` cho cọc. Báo giá là biến tạm.
5. Khi xác nhận, [`CheckoutAsync`](../QLKhachSan/BLL/HotelService.cs#L191) tính lại và ghi `Invoices`, `Payments`, đóng `Stays`/`StaySegments`, đổi `Rooms`. Vì tất cả trong một transaction, nếu bước nào lỗi thì không có hóa đơn nửa chừng.

**Gợi ý đáp án:** không có `ServiceOrders` mới trước khi gửi; `QuoteAsync` không tạo hóa đơn; hóa đơn chỉ xuất hiện sau checkout thành công.

### Ví dụ 6. Đổi vai trò và kiểm quyền

1. Tìm nút tài khoản ở [`ucDashboard.Reports.cs`](../QLKhachSan/GUI/ucDashboard.Reports.cs) rồi `AuthService.UpdateUserAsync` trong [`AuthService.Management.cs:18`](../QLKhachSan/BLL/AuthService.Management.cs#L18).
2. Tìm [`RolePolicy`](../QLKhachSan/BLL/RolePolicy.cs). Ghi vai trò nào xem tài chính, vai trò nào sửa phòng. So với các nút đang ẩn/hiện trong dashboard.
3. Xem [`HotelTransaction.RequireUserAsync`](../QLKhachSan/DAL/HotelRepository.cs#L77) đọc lại `Users.SecurityVersion`. Trả lời: vì sao ẩn nút không đủ làm bảo mật? Nếu người khác khóa tài khoản đang mở, yêu cầu tiếp theo sẽ ra sao?

**Gợi ý đáp án:** quyền được kiểm ở BLL và phiên kiểm lại ở DAL trong từng giao dịch; thay đổi quyền/khóa làm phiên cũ không còn hợp lệ.

### Tự mở rộng sang bất kỳ chức năng nào

Chọn một nút trong [bảng chức năng](#mục-lục). Điền mẫu sau vào vở hoặc issue của bạn:

```text
Tên thao tác:
Control và sự kiện Click/Changed: tệp, tên hàm
Dữ liệu người dùng nhập: TextBox/ComboBox/hàng được chọn
Hàm BLL và quyền/điều kiện cần qua:
Hàm DAL, lệnh SELECT/INSERT/UPDATE, bảng và cột JOIN:
Giá trị trả về: DTO nào; GUI đưa vào control nào:
Điều gì xảy ra nếu lỗi giữa chừng? Có transaction/rollback không?
Một câu mô tả để người chưa biết lập trình cũng hiểu:
```

Sau khi viết, tìm tên hàm bằng `rg -n 'TenHam' QLKhachSan` để đối chiếu với mã thật. Đó cũng là cách cập nhật tài liệu khi dự án thay đổi: sửa mô tả luồng và số dòng cùng lúc với sửa code.

## 20. Phân hệ Tài chính - Kế toán V6

Nút **Tài chính / Bàn giao ca** mở [`AccountingForm`](../QLKhachSan/GUI/AccountingForm.cs). Màn hình chọn khoảng **Từ/Đến** (tối đa 367 ngày), tải các tab từ `HotelService.Finance` và có nút **Làm mới**, **Xuất Excel**. Lễ tân chỉ thấy **Ca trực** của mình; Admin, Kế toán và Quản lý thấy thêm **Sổ quỹ**, **Đối soát ngân hàng**, **Công nợ**, **Kho minibar**, **Hóa đơn**, **Nhóm bill**, **Lãi lỗ**. BLL kiểm tra quyền trước khi gọi SQL; nút trên giao diện không phải kiểm soát quyền duy nhất.

| Thao tác trên `AccountingForm` | BLL và điều kiện chính | DAL và bảng ghi/đọc |
| --- | --- | --- |
| **Mở ca → Bàn giao ca → Khóa ca** | `OpenCashShiftAsync`, `SubmitCashShiftAsync`, `LockCashShiftAsync`; đối chiếu tiền lý thuyết với tiền thực đếm, yêu cầu giải trình khi lệch | `CashShifts`, `Payments.ShiftId`, `ServiceOrders.ShiftId`, `FinanceVouchers.ShiftId`; trigger khóa ca trong `MigrateV6.sql` |
| **Khóa kỳ tháng** | `LockAccountingPeriodAsync`; yêu cầu các ca liên quan đã khóa | `AccountingPeriods` và trigger chặn ghi vào kỳ cũ |
| **Nhập số dư mở sổ / Lập phiếu thu chi** | `SetOpeningBalanceAsync`, `PostVoucherAsync`; tiền mặt cần ca mở | `FinanceOpeningBalances`, `FinanceVouchers`, `Payments`; `BookAsync` tính đầu kỳ + thu − chi = cuối kỳ |
| **Nhập sao kê / Đối chiếu lại** | `ImportBankStatementsAsync`, `ReconcilePendingBankAsync`; khớp kênh, mã, số tiền | `BankStatementLines`, `Payments`, `FinanceVouchers` |
| **Ghi công nợ / Gạch nợ theo phiếu** | `CreateDebtAsync`, `AllocateDebtAsync`; không phân bổ vượt nợ hoặc phần phiếu chưa dùng | `FinanceDebts`, `DebtAllocations`, `FinanceVouchers`; danh sách tính tuổi nợ |
| **Thêm mặt hàng / Nhập xuất kho / Buồng phòng báo dùng** | `AddStockItemAsync`, `MoveStockAsync`, `ReportHousekeepingAsync`; xuất bán chốt giá vốn | `StockItems`, `StockMovements`, `HousekeepingConsumption`, `ServiceOrders` |
| **Giảm trừ / Hủy hóa đơn / Đánh dấu e-Invoice** | `AddInvoiceAdjustmentAsync`, `VoidInvoiceAsync`, `MarkEInvoiceAsync`; ghi lý do, người duyệt hoặc số hóa đơn điện tử | `InvoiceAdjustments`, `InvoiceVoids`, `InvoiceFinance`, `Invoices` |
| **Tách / gộp nhóm bill** | `GroupBillsAsync`; phân bổ đủ giá trị hóa đơn | `BillGroups`, `BillShares`; đây là phân bổ nội bộ |
| **Xem chỉ số / Lãi lỗ** | `FinanceSummaryAsync`; ADR, RevPAR, lấp đầy và lợi nhuận sơ bộ | Tổng hợp từ hóa đơn, dịch vụ, kho và phiếu chi; xem giới hạn trong [tài liệu kế toán](ACCOUNTING_FINANCE.md#chỉ-số-và-giới-hạn-báo-cáo) |

Các hàm SQL tương ứng nằm trong [`HotelTransaction.Finance.cs`](../QLKhachSan/DAL/HotelTransaction.Finance.cs), các kiểu dữ liệu nằm trong [`FinanceModels.cs`](../QLKhachSan/DTO/FinanceModels.cs). Bản in phiếu thu/chi, biên bản ca và công nợ được mở qua `PrintPreviewDialog` trong `AccountingForm`; lưới được xuất `.xlsx` bằng [`ExcelExport.cs`](../QLKhachSan/GUI/ExcelExport.cs). Hóa đơn điện tử chỉ được **ghi nhận số đã phát hành**, ứng dụng không tự phát hành; VAT/phí phục vụ/đơn vị làm tròn hiện là dữ liệu đối chiếu, không tự cộng vào bill gốc. Báo cáo lấp đầy quá khứ chưa có lịch sử phòng khả dụng theo từng đêm.
