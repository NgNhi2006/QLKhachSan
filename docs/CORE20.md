# CSDL gọn V26: 20 bảng ứng dụng

> **Tài liệu lịch sử.** Bản hiện hành chuyển sang 7 bảng lõi theo
> [README](../README.md); các bảng và chức năng V26 dưới đây không còn là phạm vi triển khai.

V25 bỏ 9 bảng `ChucNang*` trùng dữ liệu và hai bảng menu gốc `MenuTong`, `MenuCon`.
V26 bỏ thêm 17 bảng của các phân hệ mở rộng. `sysdiagrams` là bảng do SQL Server
tạo để lưu sơ đồ, không nằm trong 20 bảng ứng dụng.

## Bảng được giữ

| Nhóm | Bảng |
| --- | --- |
| Hệ thống và quyền | `PhienBanCSDL`, `TaiKhoanNhanVien`, `NhatKyThaoTac`, `ChucNang`, `PhanQuyenNhanVien` |
| Menu | `CauHinhMenu`, `CauHinhMenuCon`, `CauHinhChucNang` |
| Lưu trú | `Phong`, `KhachHang`, `LuotLuuTru`, `ChangLuuTru` |
| Dịch vụ và kho | `DichVu`, `YeuCauDichVu`, `HangTonKho`, `BienDongKho` |
| Hóa đơn và thanh toán | `ChiTietBill`, `HoaDon`, `GiaoDichThanhToan`, `CaTruc` |

## Chức năng bị bỏ hoặc thu hẹp

- Không còn khóa kỳ kế toán, sổ quỹ, phiếu thu chi, sao kê đối soát và công nợ.
- Không còn hủy/điều chỉnh hóa đơn, thông tin e-Invoice, nhóm hoặc chia bill,
  thanh toán nhiều phòng cùng lúc, mã giảm giá và thanh toán công nợ OTA.
- Không còn màn hình tự thiết kế và dữ liệu của chúng; menu có sẵn vẫn hoạt động.
- Không còn báo dùng của buồng phòng và theo dõi trả xe thuê. Kho hàng và
  nhập/xuất kho cơ bản vẫn được giữ. Báo cáo đối chiếu minibar không còn số liệu
  buồng phòng.
- Báo cáo doanh thu chỉ tính hóa đơn, chi tiết bill và cọc không hoàn còn trong
  CSDL gọn. Dữ liệu điều chỉnh/giảm giá trước V26 chỉ còn trong bản sao lưu.

## Dữ liệu và nâng cấp

Migration V26 **xóa dữ liệu** trong 17 bảng bị bỏ. Đặc biệt, trên CSDL đã kiểm tra
có dữ liệu nhóm bill, phân chia bill và thông tin tài chính hóa đơn. Hãy sao lưu
và thử khôi phục bản sao trước khi chạy ứng dụng mới; ứng dụng tự nâng schema
khi đăng nhập. Không mở CSDL V26 bằng bản ứng dụng cũ.

Đã thử V25 và V26 trên CSDL bản sao tạm: còn đúng 20
bảng ứng dụng, chạy lại migration không đổi kết quả; tải dashboard/menu, xem
khách, hóa đơn, doanh thu, ca trực, kho; tạo lượt ở, gọi và giao dịch vụ rồi
checkout thành công. Sau đó đã sao lưu/kiểm tra bản sao lưu và áp dụng V25–V26
lên `QLKhachSanApp`; CSDL chính hiện ở V26 với 20 bảng ứng dụng.
