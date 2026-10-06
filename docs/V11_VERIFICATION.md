# Biên bản kiểm thử phát hành V11 — 06/10/2026

## Môi trường và phạm vi

- Windows 10, .NET SDK 10.0.401, SQL Server 2025 Express cục bộ.
- Backup `_DbBackup/QLKhachSanApp-before-v10-20261002.bak` có schema V9 được phục hồi thành database thử nghiệm `QLKhachSan_V11_Verify_20261006`. Database vận hành `QLKhachSanApp` không bị ghi bởi bài thử.
- Bản backup V9 là bản dữ liệu cũ duy nhất có sẵn cho đợt thử này. Chưa kiểm chứng nâng cấp trực tiếp từ các bản V1–V8 hoặc V10 trên bản sao dữ liệu thật.
- `Database.sql` được đổi riêng hai dòng tạo/chọn database để trỏ vào bản thử nghiệm. Chạy bằng `sqlcmd -f 65001` vì script chứa chuỗi Unicode tiếng Việt.

## Kết quả

| Ca kiểm thử | Kết quả và bằng chứng |
| --- | --- |
| Nâng cấp V9 → V11 | Đạt; bản sao lên V11. Lần chạy đầu bằng `sqlcmd` thiếu `-f 65001` làm sai tên menu, đã phục hồi lại backup trước khi chạy đúng UTF-8. |
| Chạy lại toàn bộ script V11 | Đạt sau khi sửa giới hạn phiên bản trong `Database.sql` từ `>10` thành `>11`; số dòng của 42 bảng trước/sau giống nhau. |
| Giữ dữ liệu cũ | Phép `EXCEPT` trên toàn bộ cột liên quan cho kết quả 0 bản ghi thiếu hoặc khác ở tài khoản (6), quyền (0), lượt lưu trú (12), hóa đơn (12). Backup không có quyền cá nhân để xác minh trường hợp quyền tùy chỉnh khác 0. |
| Đặt phòng → thu cọc → nhận phòng → dịch vụ → trả phòng | Đạt trên SQL Server thử nghiệm. Hai lượt đặt có tổng cọc 200.000 đồng; dịch vụ được gọi và giao trước khi thanh toán gộp. |
| Thanh toán gộp 2 phòng | Đạt; 2 hóa đơn con, 2 dòng phân chia và đúng 1 giao dịch thanh toán ròng. Tổng `TienCoc` trên hóa đơn là 200.000 đồng. |
| Thanh toán gộp 20 phòng | Đạt; 20 hóa đơn con, 20 dòng phân chia và đúng 1 giao dịch thanh toán ròng. |
| Hoàn ròng 2 phòng | Đạt; 2 hóa đơn con, đúng 1 giao dịch `Refund` bằng số hoàn ròng. Tiền cọc đã thu cộng số ròng bằng tổng hóa đơn. |
| Lỗi giữa giao dịch | Đạt; trigger chỉ trên database thử nghiệm gây lỗi khi tạo hóa đơn thứ hai. Hóa đơn, giao dịch, nhóm và trạng thái lưu trú đều rollback; trigger được gỡ sau bài thử. |
| Gọi ngoài menu khi thiếu quyền | Đạt; gọi trực tiếp `HotelService.GroupCheckoutAsync` với phiên không có quyền bị `BusinessException` chặn ở tầng nghiệp vụ. |
| Build Release | `dotnet build QLKhachSan.slnx -c Release -warnaserror --no-restore` và build `tests/V11Verifier/V11Verifier.csproj`: đều 0 lỗi, 0 cảnh báo. |
| Gói publish | `dotnet publish QLKhachSan/QLKhachSan.csproj -c Release --no-restore -warnaserror -o artifacts/publish-v11` thành công; gói nằm trong thư mục `artifacts/publish-v11/` cục bộ. |

Log SQL và console lưu cục bộ trong `artifacts/v11-verification/` (thư mục ignored). Mã chạy lại các ca giao dịch ở `tests/V11Verifier/`; chỉ truyền connection string của database thử nghiệm có `_Verify_` trong tên.

## Giới hạn còn lại

- Chưa chạy toàn bộ luồng bằng thao tác thủ công trên giao diện WinForms; bài tích hợp gọi cùng các phương thức nghiệp vụ GUI sử dụng.
- Chưa thử trường hợp số ròng bằng 0 hoặc đối soát sao kê Bank/POS; các ca hiện tại xác minh cọc trên hóa đơn và cả khoản thu/hoàn ròng.
- Chưa có backup dữ liệu thật V1–V8 hay V10 để kiểm chứng đường nâng cấp tương ứng. Bản sao V9 có 0 dòng quyền cá nhân, nên chưa chứng minh bảo toàn quyền tùy chỉnh có dữ liệu.
