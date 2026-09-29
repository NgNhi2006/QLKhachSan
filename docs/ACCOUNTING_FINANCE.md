# Phân hệ Tài chính - Kế toán

## Phạm vi triển khai

Mở **Tài chính / Bàn giao ca** từ thanh công cụ. Lễ tân thấy ca của mình; Kế toán, Quản lý và Admin thấy sổ kế toán. Dữ liệu nằm trong SQL Server; `Database/MigrateV6.sql` được ứng dụng chạy sau V5. Sao lưu và kiểm tra khả năng khôi phục trước khi nâng cấp cơ sở dữ liệu.

## Quy trình ca trực

1. Mỗi nhân viên thu ngân **mở ca** và nhập tiền mặt đầu ca. Mỗi người chỉ có một ca `Open`.
2. Thu cọc, checkout, hoàn tiền và tạo dịch vụ phải có ca đang mở. Giao dịch thanh toán và dòng dịch vụ lưu `ShiftId`. Cọc thu trước vẫn là dòng tiền; doanh thu chỉ hình thành khi checkout hoặc ghi nhận cọc không hoàn.
3. Khi bàn giao, nhập tiền mặt thực đếm. SQL tính `OpeningCash + thu tiền mặt - hoàn tiền mặt + phiếu thu tiền mặt - phiếu chi tiền mặt`. Nếu chênh lệch khác 0, phải nhập giải trình. Giao dịch POS, chuyển khoản và OTA hiển thị riêng. Ca chuyển sang `Submitted`.
4. Kế toán duyệt và **khóa ca**. Trigger SQL chặn mọi `INSERT/UPDATE/DELETE` trên thanh toán và dòng dịch vụ thuộc ca đã khóa, kể cả từ máy trạm cũ. Không có thao tác mở khóa.
5. Chỉ khóa tháng khi tất cả ca mở trong tháng đã khóa. Trigger chặn thêm/sửa/xóa thanh toán, hóa đơn, phiếu thu chi, dòng dịch vụ, xuất nhập kho, giảm trừ, hủy và đánh dấu hóa đơn điện tử liên quan kỳ đã khóa.

Các giao dịch trước V6 không có `ShiftId` vì không thể suy ra chính xác ca lịch sử. Cần đối chiếu riêng khi chuyển hệ thống.
Các dịch vụ đã giao trước V6 cũng không có bút toán giá vốn; không tự suy đoán giá mua lịch sử.

## Tiền và chứng từ

- `FinanceVouchers` giữ phiếu thu/chi theo kênh Cash/Bank/POS/OTA, hạng mục, đối tượng, mã tham chiếu và người lập. Tiền cọc có hạng mục riêng để không cộng vào doanh thu.
- **Nhập số dư mở sổ** một lần cho từng kênh, tại đầu ngày bắt đầu sử dụng sổ. Số dư này phải đối chiếu với tiền thực đếm/sao kê tại thời điểm chuyển hệ thống. Báo cáo trước ngày mở sổ bị từ chối để tránh cộng trùng lịch sử.
- `BookAsync` cộng cả `Payments` lẫn `FinanceVouchers`: **đầu kỳ + thu - chi = cuối kỳ**. Phiếu mua tồn kho, trả nợ và tạm ứng được loại khỏi chi phí vận hành sơ bộ.
- QR/POS yêu cầu mã giao dịch khi thu. Chuyển khoản thực tế cần xác minh bằng sao kê; ảnh VietQR không chứng minh đã nhận tiền.
- Tab **Đối soát ngân hàng** nhận các dòng `yyyy-MM-dd HH:mm; Bank/POS; mã; số tiền có dấu`. Hệ thống khớp duy nhất theo kênh, mã và số tiền. Dòng không khớp giữ lại để xử lý; không tự xác nhận giao dịch chỉ vì trùng ngày.

## Công nợ

`FinanceDebts` lưu AR/AP, đối tượng, hóa đơn tham chiếu tùy chọn, hạn và số gốc. Mỗi lần thu hồi/trả nợ lập phiếu thu/chi rồi phân bổ trong `DebtAllocations`; tổng phân bổ không vượt nợ còn lại hoặc phần phiếu chưa dùng. Báo cáo chia tuổi nợ dưới 30, 30–60, 60–90 và trên 90 ngày theo hạn thanh toán. Công nợ OTA phát sinh tự động khi checkout chọn **Công nợ OTA**; kênh này không được tính là tiền mặt đã nhận.

## Minibar và vật tư

Tạo `StockItems`, có thể gắn `ServiceId` của danh mục dịch vụ. Phiếu nhập làm tăng tồn và tính lại **giá vốn bình quân gia quyền**. Khi lễ tân giao dịch vụ đã gắn hàng kho, cùng một SQL transaction ghi `StockMovements.Kind='Sale'`, giảm tồn và chốt `UnitCost` tại thời điểm xuất. Thiếu tồn thì giao dịch bị từ chối. Có phiếu xuất hủy, dùng nội bộ và báo tiêu thụ buồng phòng. Tab kho đối chiếu **số trên bill / buồng phòng báo / lượng xuất bán / tồn sổ sách**; sai lệch cần kiểm tra thực tế.

## Hóa đơn

- Hủy hóa đơn lưu lý do chuẩn, giải trình, người duyệt và thời điểm trong `InvoiceVoids`. Hóa đơn gốc không bị xóa. Báo cáo lãi/lỗ loại doanh thu của hóa đơn đã hủy; khoản phải thu chưa gạch nợ được hủy cùng giao dịch. Hóa đơn đã gạch nợ phải xử lý chứng từ hoàn/điều chỉnh trước.
- Giảm trừ lưu loại dịch vụ kém/VIP/voucher, số tiền, lý do và người phê duyệt. Báo cáo lãi/lỗ trừ khoản này ở kỳ phê duyệt.
- Tách/gộp lưu các phần thanh toán vào `BillGroups` và `BillShares`. Một hóa đơn phải được phân bổ đủ tổng tiền; nhiều dòng cùng hóa đơn là tách, nhiều hóa đơn cùng nhóm là gộp. Đây là **bảng phân bổ nội bộ**, không phát hành hóa đơn thuế con.
- Đánh dấu số e-Invoice đã phát hành một lần; số phải duy nhất. Trường VAT 8/10%, phí phục vụ 0/5% và đơn vị làm tròn lưu trong `InvoiceFinance` để đối chiếu chứng từ. Ứng dụng chưa kết nối nhà cung cấp hóa đơn điện tử và chưa tính cộng VAT/phí vào bill gốc. Kế toán phải kiểm tra số tiền trên chứng từ điện tử trước khi đánh dấu.
- Void và giảm trừ **không tự tạo khoản hoàn tiền**. Khi tiền đã thực thu, kế toán cần lập phiếu chi/hoàn tương ứng và lưu chứng từ gốc; không dùng trạng thái hủy để giả định tiền đã rời quỹ.

## Chỉ số và giới hạn báo cáo

`ADR = doanh thu phòng / phòng đêm bán`; `RevPAR = doanh thu phòng / phòng đêm sẵn có`; `Occupancy = phòng đêm bán / phòng đêm sẵn có × 100%`. Báo cáo sơ bộ tính `doanh thu phòng + minibar + dịch vụ khác - giảm trừ - giá vốn xuất bán - chi phí vận hành`.

Phòng đêm bán được xếp theo ngày checkout của hóa đơn; phòng đêm sẵn có dùng số phòng hiện tại nhân số ngày báo cáo. Hệ thống chưa lưu lịch sử bảo trì phòng theo ngày, nên ADR/RevPAR/Occupancy của kỳ quá khứ là **chỉ số quản trị sơ bộ**, chưa đủ để quyết toán hoặc so sánh với PMS có room inventory theo đêm. Giá vốn bán ghi ngày giao hàng; doanh thu ghi ngày checkout nên hai kỳ có thể lệch thời điểm.

## Xuất và in

Mỗi lưới dữ liệu có menu chuột phải **Xuất Excel (.xlsx)**; file có tiêu đề, ngày lập, hàng tiêu đề và kiểu số `#,##0 VNĐ` cho cột tiền. Tab kế toán cũng có nút Xuất Excel. Xem trước bản in áp dụng cho dòng ca, phiếu thu/chi và toàn bộ báo cáo công nợ. Phiếu thanh toán khách sạn hiện có giữ chức năng xem trước/in riêng.

## Kiểm tra triển khai

`dotnet build QLKhachSan.slnx -c Release -warnaserror` kiểm tra biên dịch. Cần kiểm thử migration và nghiệp vụ trên bản sao SQL Server trước khi dùng thật: mở/nộp/khóa ca, thử sửa giao dịch ca khóa, khóa tháng rồi thử ghi chứng từ cũ, nhận đủ và thiếu tiền, đối soát QR/POS, tồn kho âm, phân bổ nợ nhiều đợt, và kiểm tra file `.xlsx` bằng Excel. Máy phát triển hiện chưa kết nối được `SQLEXPRESS`, nên các bài kiểm thử SQL tích hợp chưa được thực hiện tại đây.
