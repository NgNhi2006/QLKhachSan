# Kiểm kê giao diện và kiểm chứng làm mới

## Phạm vi và nền tảng

Ứng dụng dùng Windows Forms trên .NET 10 (`net10.0-windows`). Điểm vào là `FormLogin`, sau xác thực mở `FormMain` chứa `ucDashboard`. Điều hướng đi từ menu tổng đến menu con rồi đến màn hình chức năng. Danh sách menu và quyền vẫn do `MenuConfiguration`, `FunctionPolicy` và vai trò người dùng quyết định. Các màn hình hiện dùng control WinForms và control tự vẽ; file dự án hiện tham chiếu Krypton Toolkit/Navigator và LiveCharts 0.9.7 nhưng chưa có mã GUI gọi các namespace này. Các form và user control nằm trong `QLKhachSan/GUI`.

| Nhóm | Màn hình và hộp thoại hiện có |
| --- | --- |
| Vào ứng dụng | Đăng nhập, thiết lập quản trị đầu tiên, xác nhận mật khẩu |
| Điều hướng | Menu tổng, menu con, sơ đồ phòng, lịch trình, biểu đồ doanh thu |
| Phòng và lưu trú | Tìm phòng, đặt trước, nhận phòng, đặt nhiều phòng, chọn nhiều phòng, quản lý phòng, bảng giá, đặt cọc, lịch sử cọc, sửa/hủy đặt, chuyển phòng, gia hạn, bảo trì, dọn phòng, trả phòng, thanh toán nhiều phòng |
| Dịch vụ và khách hàng | Gọi/xử lý dịch vụ, danh mục dịch vụ, hồ sơ khách, lịch đặt và lịch sử lưu trú |
| Tài chính và nhân sự | Hóa đơn, doanh thu, thu chi, `AccountingForm` với ca trực, sổ quỹ, ngân hàng, công nợ, kho minibar, lãi lỗ và nhóm bill; nhân viên, tài khoản, phân quyền, đổi/đặt lại mật khẩu, nhật ký thao tác |
| Cấu hình | Tùy chỉnh menu, trình thiết kế màn hình tùy chỉnh, màn hình tùy chỉnh và các hộp thoại chỉnh sửa trường/nút |

## Thành phần dùng chung

- `AppTheme`: màu, phông, nút, bảng và màu trạng thái phòng.
- `Ui`: bảng, ô nhập, bộ chọn ngày/số, thông báo lỗi/xác nhận và `InputDialog`.
- `UiIcons`, `DashboardVisuals`, `LoginVisuals`: biểu tượng và các vùng tự vẽ.
- Các form tạo trực tiếp trong các phần của `ucDashboard` dùng `AppTheme` và các control WinForms tiêu chuẩn.

## Quy tắc triển khai

Nền sáng ấm, chữ xanh đậm, màu xanh đậm cho hành động chính và vàng đồng cho điểm nhấn. Chữ nhập và chữ bảng tối trên nền sáng. Trạng thái phòng giữ màu riêng và luôn có nhãn chữ. Form nhiều trường cuộn dọc; hộp thoại dùng chung giới hạn kích thước theo vùng làm việc của màn hình. Menu con dùng nút một cột trong mỗi nhóm để nhãn dài vẫn đọc được ở chiều rộng hẹp. Icon dùng `UiIcons` sẵn có. Không đổi dữ liệu, mã hành động, thứ tự menu, quyền hoặc luồng nghiệp vụ.

## Kiểm chứng

- `dotnet build QLKhachSan/QLKhachSan.csproj`: thành công. Còn cảnh báo NU1701 vì LiveCharts 0.9.7 nhắm .NET Framework.
- `tests/V11Verifier`: yêu cầu chuỗi kết nối cơ sở dữ liệu `_Verify_` chuyên dụng; chưa chạy được trong checkout này.
- Ngày 07/10/2026: đã chụp và xem trực tiếp Menu tổng, Menu con Quản lý phòng, sơ đồ phòng, đặt phòng, Menu con Tài chính, màn hình Tài chính - Kế toán, Quản lý phòng và Bảng giá phòng ở 1366×768 và 1920×1080 với cơ sở dữ liệu `_Verify_`. Ảnh kiểm tra nằm trong `.verify-build/ui-screens-final` (thư mục kiểm tra cục bộ).
- Sau phản hồi về độ giống ảnh minh họa, Menu tổng được chỉnh thành sáu thẻ lớn 3×2 với kiểu chữ serif, biểu tượng lớn và nút phụ chuyển xuống dưới. Sơ đồ phòng có bộ lọc, bốn chỉ số trạng thái, lưới theo tầng và danh sách khách sắp đến; ảnh kiểm tra mới nằm trong `.verify-build/ui-overview`. Bộ kiểm tra đã xác nhận tìm phòng `205` còn đúng một thẻ với dữ liệu `_Verify_`.
- Bản Debug mặc định tại `QLKhachSan/bin/Debug/net10.0-windows` đã build thành công sau thay đổi giao diện. Chưa kiểm tra thao tác cuối đến lưu dữ liệu và chưa chụp tất cả hộp thoại theo từng vai trò; cần kiểm tiếp các luồng này trước khi ký nghiệm thu toàn bộ Task 6.
