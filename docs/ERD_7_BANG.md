# ERD thực tế của CSDL 7 bảng

Đối chiếu với `sys.key_constraints`, `sys.foreign_keys`, `sys.indexes` trên
`QLKhachSanApp` ngày 25/11/2026. `sysdiagrams` là bảng hỗ trợ SSMS, không thuộc
7 bảng nghiệp vụ. Migration: `Database/QuanHe_7_Bang.sql`.

| Bảng cha (PK) | Bảng con (FK) | Lực lượng |
| --- | --- | --- |
| `Phong(MaPhong)` | `LuotLuuTru(MaPhong)` | 1–N; một phòng có nhiều lượt theo thời gian |
| `KhachHang(MaKhachHang)` | `LuotLuuTru(MaKhachHang)` | 1–N |
| `TaiKhoanNhanVien(MaNhanVien)` | `LuotLuuTru(MaNhanVien)` | 1–N; nhân viên tạo lượt |
| `TaiKhoanNhanVien(MaNhanVien)` | `CaTruc(MaNhanVien)` | 1–N; nhiều ca, tối đa một ca `Open` |
| `LuotLuuTru(MaLuotLuuTru)` | `GiaoDichThanhToan(MaLuotLuuTru)` | 1–N; có thể chưa có giao dịch |
| `CaTruc(MaCaTruc)` | `GiaoDichThanhToan(MaCaTruc)` | 1–N; FK có thể NULL |
| `LuotLuuTru(MaLuotLuuTru)` | `HoaDon(MaLuotLuuTru)` | 1–0..1; FK và UNIQUE không lọc ở hóa đơn |

PK của bảy bảng lần lượt là `Phong.MaPhong`, `KhachHang.MaKhachHang`,
`TaiKhoanNhanVien.MaNhanVien`, `LuotLuuTru.MaLuotLuuTru`, `HoaDon.MaHoaDon`,
`CaTruc.MaCaTruc`, `GiaoDichThanhToan.MaGiaoDich`.

Các UNIQUE có điều kiện vẫn giữ vì phục vụ nghiệp vụ riêng:

- `UX_Stays_OccupiedRoom`: một phòng chỉ có một lượt `Occupied` hiện thời.
- `UX_CashShifts_Open`: một nhân viên chỉ có một ca `Open`.
- `UX_Payments_Forfeit`: mỗi lượt chỉ có một giao dịch `Forfeit`.

`LuotLuuTru.TenKhach`, `SoDienThoai`, `SoGiayTo` là ảnh chụp thông tin khách
gắn với lượt; không thay bằng dữ liệu hiện tại trong `KhachHang`. Chức năng sửa
khách cập nhật có chủ ý cả hồ sơ khách lẫn ảnh chụp của lượt được sửa. Trong dữ
liệu hiện tại không có dòng nào khác ba thông tin này với hồ sơ khách.

`HoaDon.TienDaThu` là số thu thêm khi checkout và `TienDaHoan` là cọc thừa hoàn
tại lúc lập hóa đơn; cả hai được chốt cùng giao dịch tạo hóa đơn. `HoaDon.PhuongThucThanhToan`
là hình thức lúc trả phòng, không đại diện cho toàn bộ các lần thu. Mỗi lần thu,
hoàn và phương thức của lần đó được đọc từ `GiaoDichThanhToan`.

`Ma` trong các bảng này là cột computed alias cho PK tên đầy đủ. Trong
`LuotLuuTru`, `HoaDon`, `GiaoDichThanhToan`, `MaNguoiTao` là alias cho
`MaNhanVien` (người tạo lượt/hóa đơn/giao dịch); `CaTruc.MaThuNgan` là alias
cho `CaTruc.MaNhanVien`. Không đổi tên các cột này vì truy vấn cũ vẫn dùng alias.

Lịch phòng dùng khoảng nửa mở `[giờ nhận, giờ trả)`. Lượt còn hiệu lực được
định nghĩa bằng `ConHieuLuc=1`; hai lượt liền kề được phép. App kiểm tra trong
transaction Serializable với application lock; trigger
`TR_LuotLuuTru_KhongTrungLich` chặn thêm/sửa chồng lịch cả khi SQL được gọi trực tiếp.

Trước migration, các truy vấn kiểm tra không phát hiện lượt chồng giờ, hóa đơn
trùng lượt hoặc hóa đơn mồ côi. Kiểm thử trên bản sao
`QLKhachSan_Relations_Verify_20261125` đạt các trường hợp trong
`tests/relations_7_tables.sql`; hai lệnh đặt đồng thời cùng phòng có đúng một
lệnh được ghi. `DBCC CHECKDB` trên CSDL thử nghiệm và chính không báo lỗi.

Chưa có FK từ `HoaDon.MaNhanVien` và `GiaoDichThanhToan.MaNhanVien` tới
`TaiKhoanNhanVien` trong schema hiện tại; ERD nộp bài cần thể hiện chúng là
thuộc tính tham chiếu bằng mã nhưng chưa được SQL Server ràng buộc, hoặc bổ sung
FK trong một migration riêng sau khi kiểm tra dữ liệu mồ côi.
