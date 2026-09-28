# QLKhachSan — quản lý khách sạn (phiên bản dữ liệu 5)

Ứng dụng WinForms .NET 10, SQL Server Express, phân tầng GUI / BLL / DAL / DTO.

## Đọc tài liệu

**[Thông tin và giải thích mã nguồn](docs/THONG_TIN.md)** — một tài liệu duy nhất để xem chức năng, nút bấm, luồng dữ liệu, mã giao diện, nghiệp vụ, SQL, cấu hình và các ví dụ theo dõi thao tác.

Đây là liên kết Markdown đến một tệp khác trong dự án: bấm **Thông tin** trên GitHub hoặc trong trình xem Markdown sẽ mở tài liệu chi tiết. Tài liệu có mục lục, ví dụ dễ hiểu và bảng tra cứu **ô nhập/dữ liệu nguồn → nút → hàm GUI → hàm BLL → câu SQL/bảng → kết quả trên màn hình**. Số dòng được đối chiếu với mã nguồn hiện tại; nếu sửa code sau này, số dòng có thể dịch chuyển.

## Chạy ứng dụng

- Mở `QLKhachSan.slnx`, chọn `QLKhachSan` làm startup project và F5; hoặc `dotnet run --project QLKhachSan`.
- Cần .NET SDK 10 để build; bản publish cần .NET Desktop Runtime 10.
- Kết nối mặc định: `.\SQLEXPRESS`, database `QLKhachSanApp`, Windows Authentication.
- Máy mới: chạy `Database/Setup.sql` trong SSMS. Lần mở ứng dụng tiếp theo tự nâng schema qua các bản `Database/MigrateV2.sql` đến `Database/MigrateV5.sql`.
- Có thể chạy migration thủ công trong đúng database; không cần SQLCMD mode. Migration dùng transaction, khóa và có thể chạy lại.
- Khi nâng cấp: đóng tất cả phiên ứng dụng cũ, sao lưu database và kiểm tra khôi phục trước. Bản cũ không tương thích với schema mới.
- Migration giữ dữ liệu và hạn nhận của các lượt đặt cũ. Trạng thái vật lý `DaDat` được chuyển thành `Trong`; lịch đặt vẫn nằm trong `Stays`.
- Migration V3 lưu trữ và vô hiệu hóa toàn bộ tài khoản đăng nhập cũ, giữ các bản ghi để hóa đơn và nhật ký không mất người thực hiện. Lần đăng nhập đầu sau nâng cấp cần tạo Admin mới; có thể dùng lại tên đăng nhập cũ. Tài khoản SQL chạy migration cần quyền thay đổi schema.

## Cấu hình

`QLKhachSan/appsettings.json` được copy vào thư mục build/publish. Có thể thay connection string bằng biến môi trường `QLKHACHSAN_CONNECTION_STRING`.

Điền `BankCode`, `BankAccount`, `BankAccountName` để bật VietQR khi chọn chuyển khoản. Nhân viên tự kiểm tra tiền thực nhận; chưa có đối soát tự động.

Kết nối SQL Express cục bộ dùng `Encrypt=True;TrustServerCertificate=True`. Khi triển khai máy chủ riêng, dùng chứng chỉ hợp lệ và `TrustServerCertificate=False`. Quyền ứng dụng không thay thế quyền SQL của tài khoản Windows.

## Chính sách đặt phòng và cọc

- Lịch đặt tách khỏi trạng thái vật lý của phòng. Một phòng có nhiều lượt đặt nếu khoảng `[ngày đến, ngày trả)` không giao nhau. Các lượt liền kề được phép đặt; lúc nhận phòng vẫn bắt buộc phòng đã trống và dọn xong.
- Đặt trước có thời lượng lưu trú 1–60 ngày. Lượt chưa thu cọc giữ tối đa 24 giờ, lượt đã thu cọc giữ tối đa 15 ngày, tính từ lúc tạo đặt phòng; ngày đến phải trước hạn giữ. Đặt trước được trên phòng đang ở nếu khoảng ngày không trùng. Không đặt phòng đang bảo trì.
- Hạn nhận có thể chỉnh từ giờ đến đến trước ngày trả nhưng không vượt giới hạn giữ chỗ. Thu cọc lần đầu trong hạn chuyển giới hạn từ 24 giờ sang 15 ngày tính từ lúc tạo đặt phòng. Lượt cũ giữ nguyên hạn đã lưu.
- **Quá hạn nhận mà chưa check-in: hủy lượt, không hoàn toàn bộ tiền cọc.** Chính xác tại hạn cũng được xem là quá hạn. Lượt không có cọc cũng được hủy để giải phóng lịch.
- Hủy trước hạn: hoàn toàn bộ cọc. Nếu màn hình hoàn tiền đã mở nhưng sau đó vượt hạn, hệ thống từ chối số tiền hoàn cũ và yêu cầu mở lại.
- Tự xử lý quá hạn lúc mở dashboard, làm mới và mỗi phút khi dashboard rảnh. Không có Windows service độc lập: khi ứng dụng đóng hoặc đang mở dialog, xử lý được thực hiện ở lần refresh tiếp theo. Nghiệp vụ nhận/sửa/thu thêm/hủy vẫn kiểm tra hạn theo giờ SQL ngay tại giao dịch.
- Cọc có thể nhập số tiền thực thu, thu nhiều đợt. Thu thêm không kéo dài hạn nhận. Không nhận thêm cọc hoặc sửa lịch của lượt đã quá hạn.
- Cọc mất được lưu bằng `Payments.Kind='Forfeit'`, có unique index cho mỗi lượt và audit `NoShow`. Đây là ghi nhận khoản cọc đã thu, **không phải lần thu tiền mới**. Không tạo `Refund` cho lượt quá hạn.
- Kiểm tra trùng lịch được thực hiện trong transaction khi đặt/sửa lịch, nhận, đổi và gia hạn phòng. Nhận sớm/muộn vẫn giữ số ngày dự kiến; nếu ngày trả mới đè lên lịch khác thì bị từ chối.

## Nghiệp vụ và giao diện

Vai trò đăng nhập: **Admin** (vận hành, báo cáo, tài khoản), **Reception** (phòng, lịch đặt, dịch vụ, khách), **Accountant** (hóa đơn, doanh thu, thu chi), **Manager** (xem vận hành và báo cáo). Accountant và Manager không được ghi nghiệp vụ. Admin quản lý tạo, đổi quyền, khóa và đặt lại mật khẩu nhân viên trong một màn hình **Tài khoản & Nhân viên**.

- Sơ đồ hiển thị trạng thái vật lý; dấu `*` sau số phòng báo có lịch đặt. Nhận lượt đã đặt bằng nút **Nhận phòng** ở danh sách đặt hoặc lịch đến/đi.
- **Sửa lịch / khách**: sửa phòng, thông tin khách, ngày đến, thời lượng, hạn nhận; bắt buộc lý do; giữ nguyên tiền đã thu.
- **Lịch đặt / Lịch sử**: xem lượt hoạt động theo khoảng ngày hoặc tra cứu 500 lượt gần nhất theo tên/SĐT/giấy tờ, gồm lượt hủy. Xuất CSV UTF-8.
- **Thu cọc bổ sung**: chọn lượt, số tiền, phương thức và xác nhận thực thu.
- **Xử lý dịch vụ**: sửa số lượng có lý do, hủy dòng chưa giao, giao từng phần. Sau khi giao một phần chỉ được giảm xuống ít nhất số đã giao. Dòng hủy còn trong lịch sử, không tính tiền. Không sửa dịch vụ của lượt đã thanh toán.
- Danh mục phòng và dịch vụ được lưu trong database để phục vụ đặt phòng, gọi dịch vụ và tính tiền. Admin và Manager có các mục **Danh mục phòng**, **Danh mục dịch vụ**, **Bảng giá** trên dashboard. Bảo trì phòng chỉ khi đã xử lý hết lịch đặt.
- **Tài khoản & Nhân viên** (Admin): tạo tài khoản, đổi quyền, khóa/mở và đặt lại mật khẩu trên cùng một màn hình. Không sửa quyền hoặc khóa tài khoản đang sử dụng.
- Đổi mật khẩu, đặt lại mật khẩu, đổi quyền hoặc khóa tài khoản làm mất hiệu lực phiên cũ. Phiên tự đổi mật khẩu được cập nhật sau khi giao dịch thành công.
- **Nhật ký thao tác** (Admin): xem tối đa 1.000 thao tác gần nhất theo ngày.

## Tính tiền và báo cáo

- Tiền phòng: tối thiểu 1 ngày, làm tròn tổng thời gian ở lên ngày 24 giờ. Nếu đổi phòng, chia thời gian thực theo giá đã lưu; phần ngày còn thiếu tính theo giá phòng cuối. Làm tròn tiền phòng đến 1 đồng.
- Bảng tính checkout giữ giá 10 phút; nghiệp vụ khác thay đổi lượt sẽ làm bảng tính cũ hết hiệu lực. Chỉ checkout khi dịch vụ chưa hủy đã giao đủ. Mỗi lượt chỉ có một hóa đơn.
- Cọc thừa khi checkout vẫn được hoàn; chính sách mất cọc chỉ áp dụng khách đặt phòng quá hạn chưa nhận.
- Ngày lập hóa đơn và ngày thu checkout lấy thời điểm xác nhận. Thời điểm chốt giá phòng được giữ riêng qua thời điểm kết thúc `StaySegments`.
- **Hóa đơn / Doanh thu**: khoảng ngày tối đa 367 ngày; doanh thu hóa đơn và cọc không hoàn hiển thị riêng. Thu cọc, thu checkout, hoàn cọc, phương thức và nhân viên được liệt kê để đối chiếu thu/chi.
- `Forfeit` có dòng tiền bằng 0. Không cộng lại cọc mất vào tiền thực thu. Báo cáo thu/chi chưa thay thế nghiệp vụ đóng/bàn giao ca có số dư đầu/cuối ca.
- Xuất CSV mở bằng Excel; chuỗi bắt đầu bằng ký tự công thức được vô hiệu hóa. Xem trước/in phiếu thanh toán đã lưu, phân trang dịch vụ dài. Có thể dùng máy in PDF của Windows. Đây là phiếu nội bộ, chưa có hóa đơn thuế điện tử.
- Dashboard dùng giờ SQL Server làm mốc; máy trạm và SQL Server cần cùng múi giờ Việt Nam.

## Build và xuất bản

```powershell
dotnet build QLKhachSan.slnx -c Release -warnaserror
dotnet publish QLKhachSan/QLKhachSan.csproj -c Release --no-restore -o artifacts/publish
```

Không chạy ứng dụng cũ sau khi nâng schema. Bản này chưa có đặt đoàn/nhiều khách trong phòng, giá giờ/ngày lễ/khuyến mãi, đồng bộ OTA, đối soát ngân hàng tự động hoặc dịch vụ xử lý quá hạn khi ứng dụng đóng.
