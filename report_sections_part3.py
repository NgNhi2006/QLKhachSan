import docx
from docx.shared import Inches, Pt, RGBColor
from generate_qlks_helpers import (
    add_h1, add_h2, add_h3, add_p, add_bullet, add_styled_table, add_image_box,
    COLOR_BLACK, COLOR_DARK, COLOR_MUTED
)

def build_chapter3(doc):
    add_h1(doc, "CHƯƠNG 3: XÂY DỰNG CƠ SỞ DỮ LIỆU QUẢN LÝ KHÁCH SẠN")

    add_h2(doc, "1. Phân tích yêu cầu")
    add_p(doc, "Hệ thống quản lý khách sạn được xây dựng nhằm hỗ trợ tin học hóa và tối ưu toàn diện các hoạt động nghiệp vụ cốt lõi của khách sạn. Mô hình tập trung vào việc quản lý phòng, khách hàng, lưu trú/đặt phòng, ca trực thu ngân và hóa đơn thanh toán.")
    add_p(doc, "Các chức năng chính cần lưu trữ dữ liệu:")
    add_bullet(doc, "Quản lý nhân viên và tài khoản đăng nhập phân quyền (Admin, Lễ tân).")
    add_bullet(doc, "Quản lý danh mục phòng, phân loại phòng (Đơn, Đôi, VIP) và trạng thái buồng phòng (Trống, Đang ở, Đã đặt, Bảo trì).")
    add_bullet(doc, "Quản lý thông tin hồ sơ và định danh khách lưu trú.")
    add_bullet(doc, "Quản lý các lượt đặt cọc phòng trước và nhận phòng trực tiếp.")
    add_bullet(doc, "Quản lý ca trực thu ngân, theo dõi tiền mặt đầu ca, chốt ca bàn giao và ghi nhận chênh lệch.")
    add_bullet(doc, "Lập hóa đơn thanh toán khi trả phòng, cấn trừ cọc và theo dõi dòng tiền thanh toán.")

    add_h2(doc, "2. Các bảng trong cơ sở dữ liệu")
    add_p(doc, "Để đáp ứng các yêu cầu quản lý nêu trên một cách tối ưu, gọn gàng và chuẩn mực theo chuẩn hóa dữ liệu (3NF), cơ sở dữ liệu được thiết kế gồm 7 bảng cốt lõi với khóa chính tường minh theo từng thực thể, loại bỏ hoàn toàn các phân hệ rườm rà dư thừa:")
    add_bullet(doc, "Bảng Phong: Quản lý danh sách các phòng, loại phòng, giá thuê và trạng thái buồng phòng.")
    add_bullet(doc, "Bảng KhachHang: Quản lý thông tin hồ sơ và định danh khách lưu trú.")
    add_bullet(doc, "Bảng TaiKhoanNhanVien: Lưu trữ thông tin nhân sự và tài khoản đăng nhập hệ thống.")
    add_bullet(doc, "Bảng LuotLuuTru: Quản lý các lượt đặt cọc, nhận phòng và trả phòng của khách.")
    add_bullet(doc, "Bảng HoaDon: Quản lý thanh toán tiền phòng, cấn trừ cọc và xuất hóa đơn lưu trú.")
    add_bullet(doc, "Bảng CaTruc: Quản lý phân ca làm việc, kiểm đếm tiền mặt đầu/cuối ca của thu ngân.")
    add_bullet(doc, "Bảng GiaoDichThanhToan: Quản lý chi tiết các giao dịch tiền cọc và thanh toán tiền phòng theo từng ca trực.")

    add_h2(doc, "3. Thiết kế các bảng")
    add_p(doc, "Dưới đây là thiết kế chi tiết cấu trúc các bảng trong cơ sở dữ liệu của hệ thống:")

    tbl_widths = [Inches(1.5), Inches(1.5), Inches(1.4), Inches(2.4)]
    tbl_hdr = ["Trường", "Kiểu dữ liệu", "Ràng buộc", "Diễn giải"]

    # TRANG 1: 3.1 & 3.2
    doc.add_page_break()

    # 3.1 Phong
    add_h3(doc, "3.1. Bảng Phong")
    add_p(doc, "Bảng lưu danh sách các phòng trong khách sạn cùng giá và trạng thái.")
    data_phong = [
        ["MaPhong", "INT", "PK, IDENTITY", "Mã định danh phòng"],
        ["SoPhong", "NVARCHAR(10)", "NOT NULL, UNIQUE", "Số phòng (P.101, P.201...)"],
        ["LoaiPhong", "NVARCHAR(20)", "NOT NULL", "Loại phòng (Đơn, Đôi, VIP)"],
        ["DonGiaPhong", "DECIMAL(18,2)", "NOT NULL", "Đơn giá thuê theo ngày (VNĐ)"],
        ["TienCoc", "DECIMAL(18,2)", "NOT NULL, DEFAULT 0", "Tiền cọc tiêu chuẩn khi đặt"],
        ["TrangThai", "VARCHAR(20)", "NOT NULL", "Trạng thái (Trong, DangO, DaDat...)"],
        ["Tang", "INT", "NOT NULL, DEFAULT 1", "Tầng lầu vị trí phòng"]
    ]
    add_styled_table(doc, tbl_hdr, data_phong, tbl_widths)

    # 3.2 KhachHang
    add_h3(doc, "3.2. Bảng KhachHang")
    add_p(doc, "Bảng lưu thông tin khách hàng đến đặt và thuê phòng.")
    data_kh = [
        ["MaKhachHang", "INT", "PK, IDENTITY", "Mã định danh khách hàng"],
        ["TenKhachHang", "NVARCHAR(100)", "NOT NULL", "Họ và tên khách hàng"],
        ["SoDienThoai", "NVARCHAR(20)", "NOT NULL", "Số điện thoại liên lạc"],
        ["SoGiayTo", "NVARCHAR(20)", "NOT NULL, UNIQUE", "Số CCCD / Hộ chiếu"],
        ["DiaChi", "NVARCHAR(200)", "NULL", "Địa chỉ cư trú của khách"],
        ["ThoiDiemTao", "DATETIME2", "NOT NULL", "Thời điểm tạo hồ sơ khách"]
    ]
    add_styled_table(doc, tbl_hdr, data_kh, tbl_widths)

    # TRANG 2: 3.3 & 3.4
    doc.add_page_break()

    # 3.3 TaiKhoanNhanVien
    add_h3(doc, "3.3. Bảng TaiKhoanNhanVien")
    add_p(doc, "Bảng lưu thông tin tài khoản đăng nhập và nhân viên trong khách sạn.")
    data_tknv = [
        ["MaNhanVien", "INT", "PK, IDENTITY", "Mã định danh nhân viên"],
        ["TenDangNhap", "NVARCHAR(50)", "NOT NULL, UNIQUE", "Tên đăng nhập hệ thống"],
        ["MatKhauBam", "VARBINARY(32)", "NOT NULL", "Mật khẩu đã mã hóa (PBKDF2 SHA-512)"],
        ["MuoiBam", "VARBINARY(16)", "NOT NULL", "Chuỗi muối bảo mật mật khẩu"],
        ["HoTen", "NVARCHAR(100)", "NOT NULL", "Họ tên đầy đủ của nhân viên"],
        ["SoDienThoai", "NVARCHAR(20)", "NULL", "Số điện thoại liên hệ"],
        ["VaiTro", "VARCHAR(20)", "NOT NULL", "Vai trò (Admin, Reception...)"],
        ["DangHoatDong", "BIT", "NOT NULL, DEFAULT 1", "Trạng thái kích hoạt tài khoản"]
    ]
    add_styled_table(doc, tbl_hdr, data_tknv, tbl_widths)

    # 3.4 LuotLuuTru
    add_h3(doc, "3.4. Bảng LuotLuuTru")
    add_p(doc, "Bảng lưu thông tin chi tiết các lượt đặt phòng và ở phòng của khách.")
    data_llt = [
        ["MaLuotLuuTru", "BIGINT", "PK, IDENTITY", "Mã định danh lượt lưu trú"],
        ["MaPhong", "INT", "FK -> Phong(MaPhong)", "Mã phòng được đặt/ở"],
        ["MaKhachHang", "INT", "FK -> KhachHang", "Mã khách hàng đại diện"],
        ["TenKhach", "NVARCHAR(100)", "NOT NULL", "Tên khách hiển thị trên phiếu"],
        ["SoDienThoai", "NVARCHAR(20)", "NOT NULL", "Số điện thoại liên lạc"],
        ["SoGiayTo", "NVARCHAR(20)", "NOT NULL", "Số CCCD/Hộ chiếu"],
        ["TrangThai", "VARCHAR(20)", "NOT NULL", "Trạng thái (Reserved, Occupied...)"],
        ["ThoiDiemDen", "DATETIME2", "NOT NULL", "Thời điểm dự kiến đến"],
        ["ThoiDiemDi", "DATETIME2", "NOT NULL", "Thời điểm dự kiến đi"],
        ["ThoiDiemNhanPhong", "DATETIME2", "NULL", "Thời điểm check-in thực tế"],
        ["ThoiDiemTraPhong", "DATETIME2", "NULL", "Thời điểm check-out thực tế"],
        ["TienCoc", "DECIMAL(18,2)", "NOT NULL, DEFAULT 0", "Tiền cọc giữ chỗ đã đóng"],
        ["MaNhanVien", "INT", "FK -> TaiKhoanNhanVien", "Nhân viên thực hiện tạo lượt"]
    ]
    add_styled_table(doc, tbl_hdr, data_llt, tbl_widths)

    # TRANG 3: 3.5, 3.6 & 3.7
    doc.add_page_break()

    # 3.5 HoaDon
    add_h3(doc, "3.5. Bảng HoaDon")
    add_p(doc, "Bảng lưu trữ thông tin hóa đơn khi khách thanh toán trả phòng.")
    data_hd = [
        ["MaHoaDon", "BIGINT", "PK, IDENTITY", "Mã định danh hóa đơn"],
        ["MaLuotLuuTru", "BIGINT", "NOT NULL, UNIQUE, FK", "Mã lượt lưu trú thanh toán"],
        ["SoPhongHoaDon", "NVARCHAR(10)", "NOT NULL", "Số phòng hiển thị trên bill"],
        ["TenKhach", "NVARCHAR(100)", "NOT NULL", "Tên khách thanh toán"],
        ["ThoiDiemLapHoaDon", "DATETIME2", "NOT NULL", "Thời điểm xuất hóa đơn"],
        ["TienPhong", "DECIMAL(18,2)", "NOT NULL", "Tổng tiền thuê phòng"],
        ["TienCoc", "DECIMAL(18,2)", "NOT NULL", "Tiền cọc cấn trừ vào bill"],
        ["TienDaThu", "DECIMAL(18,2)", "NOT NULL", "Số tiền thực tế đã thu thêm"],
        ["PhuongThucThanhToan", "NVARCHAR(30)", "NOT NULL", "Hình thức (Tiền mặt, CK, Thẻ)"],
        ["MaNhanVien", "INT", "FK -> TaiKhoanNhanVien", "Nhân viên xuất hóa đơn"]
    ]
    add_styled_table(doc, tbl_hdr, data_hd, tbl_widths)

    # 3.6 CaTruc
    add_h3(doc, "3.6. Bảng CaTruc")
    add_p(doc, "Bảng quản lý ca làm việc và kiểm đếm tiền mặt khi giao ca của thu ngân.")
    data_catruc = [
        ["MaCaTruc", "BIGINT", "PK, IDENTITY", "Mã định danh ca trực"],
        ["MaNhanVien", "INT", "FK -> TaiKhoanNhanVien", "Nhân viên trực ca thu ngân"],
        ["ThoiDiemMoCa", "DATETIME2", "NOT NULL", "Thời điểm bắt đầu mở ca"],
        ["ThoiDiemDongCa", "DATETIME2", "NULL", "Thời điểm kết ca bàn giao"],
        ["TienMatDauCa", "DECIMAL(18,2)", "NOT NULL", "Tiền mặt đầu ca trong két quầy"],
        ["TienMatKiemDem", "DECIMAL(18,2)", "NULL", "Tiền mặt thực đếm khi giao ca"],
        ["TienMatDuKien", "DECIMAL(18,2)", "NULL", "Tiền mặt lý thuyết hệ thống tính"],
        ["GiaiTrinh", "NVARCHAR(500)", "NULL", "Giải trình nếu có chênh lệch tiền"],
        ["TrangThai", "VARCHAR(12)", "NOT NULL", "Trạng thái (Open, Submitted, Locked)"]
    ]
    add_styled_table(doc, tbl_hdr, data_catruc, tbl_widths)

    # 3.7 GiaoDichThanhToan
    add_h3(doc, "3.7. Bảng GiaoDichThanhToan")
    add_p(doc, "Bảng lưu trữ chi tiết các giao dịch thu tiền cọc và thanh toán tiền phòng theo từng ca trực.")
    data_gdtt = [
        ["MaGiaoDich", "BIGINT", "PK, IDENTITY", "Mã định danh giao dịch thanh toán"],
        ["MaLuotLuuTru", "BIGINT", "FK -> LuotLuuTru", "Mã lượt lưu trú phát sinh thu tiền"],
        ["LoaiGiaoDich", "VARCHAR(20)", "NOT NULL", "Loại (Deposit, Checkout, Refund...)"],
        ["SoTien", "DECIMAL(18,2)", "NOT NULL", "Số tiền giao dịch thực tế"],
        ["ThoiDiemTao", "DATETIME2", "NOT NULL", "Thời điểm phát sinh giao dịch"],
        ["PhuongThucThanhToan", "NVARCHAR(30)", "NOT NULL", "Hình thức (Tiền mặt, CK, POS)"],
        ["MaNhanVien", "INT", "FK -> TaiKhoanNhanVien", "Nhân viên thực hiện thu tiền"],
        ["MaCaTruc", "BIGINT", "NULL, FK -> CaTruc", "Ca trực tiếp nhận dòng tiền"]
    ]
    add_styled_table(doc, tbl_hdr, data_gdtt, tbl_widths)

    # TRANG 4: SƠ ĐỒ QUAN HỆ VỚI HÌNH ẢNH ERD & SSMS DIAGRAM
    doc.add_page_break()
    add_h2(doc, "4. Sơ đồ quan hệ thực thể (Database Diagram)")
    add_p(doc, "Dựa trên thiết kế các bảng và các ràng buộc khóa ngoại chuẩn mực, mối quan hệ giữa các thực thể cốt lõi trong hệ thống được thể hiện qua sơ đồ quan hệ trong SQL Server Management Studio (SSMS) dưới đây:")

    add_image_box(doc, "tmp-screenshots/ssms_diagram.png", "Hình 3.1. Sơ đồ quan hệ thực thể (Database Diagram) trong SQL Server Management Studio (SSMS)")

    add_p(doc, "Các mối quan hệ chính trên sơ đồ:")
    add_bullet(doc, "Một Khách hàng có thể có nhiều Lượt lưu trú (KhachHang 1 - N LuotLuuTru qua MaKhachHang).")
    add_bullet(doc, "Một Phòng có thể phục vụ nhiều Lượt lưu trú theo thời gian (Phong 1 - N LuotLuuTru qua MaPhong).")
    add_bullet(doc, "Một Nhân viên có thể tạo nhiều Lượt lưu trú, lập nhiều Hóa đơn và mở nhiều Ca trực (TaiKhoanNhanVien 1 - N qua MaNhanVien).")
    add_bullet(doc, "Mỗi Lượt lưu trú khi thanh toán tương ứng duy nhất với một Hóa đơn (LuotLuuTru 1 - 1 HoaDon qua MaLuotLuuTru).")
    add_bullet(doc, "Một Lượt lưu trú có thể phát sinh nhiều giao dịch thanh toán cọc và trả phòng (LuotLuuTru 1 - N GiaoDichThanhToan).")
    add_bullet(doc, "Một Ca trực ghi nhận nhiều giao dịch thanh toán trong ca (CaTruc 1 - N GiaoDichThanhToan qua MaCaTruc).")

    add_p(doc, "Với thiết kế chuẩn hóa này, toàn bộ khóa chính đều được đặt tên rõ ràng theo tên thực thể (MaPhong, MaKhachHang, MaNhanVien, MaLuotLuuTru, MaHoaDon, MaCaTruc, MaGiaoDich), loại bỏ hoàn toàn tình trạng đặt tên trơ trọi chữ 'Ma', giúp sơ đồ trực quan, trong sáng và loại trừ nguy cơ xung đột tên cột khi truy vấn.")
