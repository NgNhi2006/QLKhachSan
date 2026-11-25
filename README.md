# QLKhachSan

Ứng dụng quản lý khách sạn viết bằng Windows Forms trên .NET 10, dùng SQL Server và tổ chức mã theo các lớp GUI, BLL, DAL, DTO. Solution chính là `QLKhachSan.slnx`.

## Yêu cầu

- Windows và .NET SDK 10 để build, chạy từ mã nguồn.
- SQL Server hoặc SQL Server Express. Cấu hình mặc định dùng instance `.\SQLEXPRESS`, database `QLKhachSanApp` và Windows Authentication.
- Visual Studio có hỗ trợ .NET 10 nếu build bằng giao diện.

## Cấu trúc dự án

| Đường dẫn | Nội dung |
| --- | --- |
| `QLKhachSan/` | Ứng dụng WinForms, mã nguồn và tài nguyên giao diện |
| `Database/Database.sql` | Script tạo database theo cấu trúc 7 bảng hiện hành |
| `Database/QuanHe_7_Bang.sql` | Quan hệ và ràng buộc cho cấu trúc 7 bảng |
| `Database/ChuanHoa_7_Bang.sql` | Script chuẩn hóa cấu trúc 7 bảng |
| `tests/` | Các chương trình và script kiểm tra |

Bảy bảng nghiệp vụ là `Phong`, `KhachHang`, `TaiKhoanNhanVien`, `LuotLuuTru`, `HoaDon`, `CaTruc` và `GiaoDichThanhToan`.

## Cấu hình database

1. Cài SQL Server Express hoặc dùng một SQL Server sẵn có.
2. Mở `QLKhachSan/appsettings.json` và sửa `ConnectionString` nếu tên máy chủ, instance hoặc database khác mặc định. Có thể đặt biến môi trường `QLKHACHSAN_CONNECTION_STRING` để ghi đè kết nối.
3. Với database **mới, chưa có dữ liệu**, chạy `Database/Database.sql` trong SQL Server Management Studio để tạo schema. Ứng dụng cũng có thể khởi tạo database trống khi tài khoản SQL có đủ quyền.

`Database/Database.sql` có các lệnh xóa bảng cũ ngoài phạm vi 7 bảng. Trước khi chạy trên database đang có dữ liệu, hãy sao lưu và kiểm tra script trên bản sao. Không dùng script khởi tạo như một bản cập nhật dữ liệu thông thường.

## Build và chạy

Tại thư mục gốc của repo:

```powershell
dotnet restore QLKhachSan.slnx
dotnet build QLKhachSan.slnx -c Release --no-restore
dotnet run --project QLKhachSan/QLKhachSan.csproj -c Release --no-build
```

Hoặc mở `QLKhachSan.slnx` trong Visual Studio, chọn `QLKhachSan` làm startup project và nhấn F5. Khi chạy trên máy khác, cần .NET Desktop Runtime 10 cùng cấu hình SQL Server phù hợp.

Để tạo thư mục phát hành:

```powershell
dotnet publish QLKhachSan/QLKhachSan.csproj -c Release -o artifacts/publish
```

Thư mục `artifacts/`, các thư mục `bin/` và `obj/` được Git bỏ qua.

## Kiểm tra

```powershell
dotnet build QLKhachSan.slnx -c Release
```

`tests/relations_7_tables.sql` dùng để kiểm tra ràng buộc database trên môi trường thử nghiệm; không chạy trực tiếp trên database vận hành. `tests/V11Verifier` là bộ kiểm tra cho schema cũ và chỉ chạy được với database thử nghiệm phù hợp.

## Cấu hình ứng dụng

`QLKhachSan/appsettings.json` được sao chép vào thư mục build. Các trường `BankCode`, `BankAccount` và `BankAccountName` dùng cho thông tin chuyển khoản/VietQR. Nhân viên cần xác nhận tiền thực nhận; ứng dụng không tự đối soát giao dịch ngân hàng.
