# Quản trị menu và thanh toán nhiều phòng (V11)

## Menu do Admin cấu hình

Trong **Menu tổng → Tùy chỉnh menu**, Admin quản lý ba cấp: menu chính, menu con, chức năng. Mỗi menu có mã duy nhất, tên, mô tả, biểu tượng, màu, thứ tự và trạng thái hiển thị. Menu con có menu cha, tên, thứ tự và trạng thái. Chức năng có tên nút, menu con, nghiệp vụ liên kết, thứ tự và trạng thái.

Chức năng liên kết với một `MaChucNang` đã được ứng dụng hỗ trợ. Admin có thể tạo nhiều đường dẫn đến cùng một nghiệp vụ, đổi tên hoặc ẩn nút. Quyền người dùng luôn được kiểm tra bằng **mã nghiệp vụ gốc** và vai trò, nên việc tạo menu không thể tự cấp quyền. Nghiệp vụ mới chưa có mã xử lý vẫn cần bản cập nhật ứng dụng.

V11 tạo `CauHinhMenu`, `CauHinhMenuCon`, `CauHinhChucNang` và sao chép cấu trúc menu V10 sang đó. Các bảng `MenuTong`, `MenuCon`, `ChucNang` và bảng quyền cũ được giữ nguyên để không mất quyền và lịch sử. Không xóa được menu có menu con, menu con có chức năng, hoặc mục mặc định; mục mặc định có thể đổi tên, sắp xếp, ẩn. Mọi thay đổi được ghi trong `NhatKyThaoTac`.

## Phòng, cọc và hóa đơn

- **Danh mục phòng** có thêm, sửa một phòng, cập nhật hàng loạt và xóa phòng chưa từng có lịch đặt/lưu trú. Phòng có lịch sử được giữ để bảo toàn hóa đơn và báo cáo.
- **Đặt cọc** có màn hình xem các lượt/phòng của cùng khách và từng giao dịch thu, hoàn, mất cọc. Tab **Phiếu cọc** trong báo cáo hóa đơn hiện tên khách, phòng, số cọc và số còn phải trả. Trước checkout, số còn lại là **dự kiến tiền phòng, chưa gồm dịch vụ**; sau checkout dùng tổng hóa đơn thực tế.
- **Lập hóa đơn thanh toán** chọn phòng đang ở, tính tiền và checkout. Với nhiều phòng thuộc cùng `MaKhachHang`, nhân viên có thể chọn 2–20 phòng trong một phiếu tổng. Hệ thống kiểm tra và lưu tất cả hóa đơn con trong một giao dịch cơ sở dữ liệu, ghi **một khoản thu hoặc hoàn ròng** cho lần thanh toán và liên kết chúng với `NhomHoaDon`/`PhanChiaHoaDon`. Khách nhận một phiếu thanh toán tổng; dữ liệu từng phòng vẫn còn để đối soát. Chỉ hỗ trợ tiền mặt, chuyển khoản và POS; công nợ OTA tiếp tục xử lý từng phòng.

Phiếu thanh toán là chứng từ nội bộ, không phải hóa đơn VAT điện tử. Mã tham chiếu ngân hàng/POS phải duy nhất cho lần thanh toán gộp. Để kiểm tra hoặc in lại phiếu tổng, mở **Tài chính → Nhóm bill → Xem / in phiếu tổng**.
