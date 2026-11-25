import docx
from docx.shared import Inches, Pt, RGBColor
from docx.enum.text import WD_ALIGN_PARAGRAPH
from .styles import (
    COLOR_PRIMARY, COLOR_SECONDARY, COLOR_TEXT,
    add_h1, add_h2, add_h3, add_p, add_bullet, add_callout,
    create_styled_table, add_code_block
)

def build_chapter3(doc):
    add_h1(doc, "CHƯƠNG 3. XÂY DỰNG CƠ SỞ DỮ LIỆU")

    # 3.1 Phân tích thực thể
    add_h2(doc, "3.1. Phân tích thực thể")
    add_p(doc, "Cơ sở dữ liệu của phần mềm Quản lý Khách sạn được chuẩn hóa ở dạng chuẩn 3 (3NF), gồm 20 bảng ứng dụng cốt lõi (Core V26). Các thực thể chính bao gồm:")
    add_bullet(doc, "Lưu trữ thông tin tài khoản đăng nhập của nhân viên và ban quản lý, gồm tên đăng nhập, chuỗi băm mật khẩu, muối băm, trạng thái hoạt động, vai trò mặc định, thời gian khóa tạm và phiên bản an ninh SecurityVersion.", "1. TaiKhoanNhanVien (Tài khoản nhân viên): ")
    add_bullet(doc, "Lưu danh mục phòng vật lý, số phòng, hạng phòng (Standard, Superior, Deluxe, VIP), giá niêm yết theo ngày, trạng thái phòng (Trống, Đang ở, Cần dọn, Bảo trì) và số phiên bản đồng thời.", "2. Phong (Phòng lưu trú): ")
    add_bullet(doc, "Lưu thông tin hồ sơ khách hàng: họ tên, số điện thoại, số giấy tờ tùy thân (CCCD/Hộ chiếu), địa chỉ, quốc tịch và thời điểm tạo.", "3. KhachHang (Khách hàng): ")
    add_bullet(doc, "Thực thể trung tâm quản lý toàn bộ vòng đời của một lượt đặt phòng hoặc nhận phòng trực tiếp: mã phòng, mã khách, ngày nhận dự kiến, ngày trả dự kiến, hạn nhận phòng, trạng thái lưu trú, tổng cọc đã thu.", "4. LuotLuuTru (Lượt lưu trú): ")
    add_bullet(doc, "Lưu vết lịch sử thời gian khách ở từng phòng cụ thể khi có sự kiện đổi phòng, phục vụ tính tiền công bằng cho từng khoảng thời gian.", "5. ChangLuuTru (Chặng lưu trú): ")
    add_bullet(doc, "Danh mục các mặt hàng minibar, đồ uống và dịch vụ khách sạn cung cấp, đơn vị tính, đơn giá bán.", "6. DichVu (Dịch vụ khách sạn): ")
    add_bullet(doc, "Các yêu cầu gọi dịch vụ cụ thể gắn với từng lượt lưu trú: số lượng yêu cầu, số lượng đã giao, đơn giá, thời điểm yêu cầu, trạng thái xử lý.", "7. YeuCauDichVu (Yêu cầu dịch vụ buồng phòng): ")
    add_bullet(doc, "Quản lý số lượng tồn kho thực tế của các mặt hàng minibar trong kho khách sạn.", "8. HangTonKho & BienDongKho (Kho hàng & Biến động kho): ")
    add_bullet(doc, "Lưu hóa đơn thanh toán khi khách làm thủ tục check-out: số phòng, tên khách, tổng tiền phòng, tổng tiền dịch vụ, tổng cọc, số tiền thực thu, phương thức thanh toán.", "9. HoaDon & ChiTietBill (Hóa đơn & Chi tiết Bill): ")
    add_bullet(doc, "Lưu vết mọi giao dịch tài chính phát sinh: thu cọc, thu checkout, hoàn cọc thừa, tịch thu cọc quá hạn (Forfeit).", "10. GiaoDichThanhToan (Giao dịch thanh toán): ")
    add_bullet(doc, "Quản lý ca trực thu ngân: thời điểm mở ca, đóng ca, tiền mặt đầu ca, tiền mặt thực đếm cuối ca, tiền dự kiến và biên bản giải trình chênh lệch.", "11. CaTruc (Ca trực thu ngân): ")
    add_bullet(doc, "Ghi lại toàn bộ hành vi thêm, sửa, xóa, đăng nhập của nhân viên kèm thời gian và chi tiết để phục vụ kiểm toán.", "12. NhatKyThaoTac (Nhật ký thao tác hệ thống): ")
    add_bullet(doc, "Lưu trữ cây menu phân cấp và các định nghĩa màn hình động do người quản trị tự cấu hình.", "13. CauHinhMenu & CauHinhChucNang (Cấu trúc menu & Màn hình động): ")

    # 3.2 Lược đồ quan hệ thực thể (ERD)
    add_h2(doc, "3.2. Lược đồ quan hệ thực thể (ERD)")
    add_p(doc, "Mối quan hệ giữa các thực thể cốt lõi trong cơ sở dữ liệu được thể hiện qua sơ đồ cấu trúc quan hệ dưới đây:")
    add_code_block(doc,
"""┌───────────────────┐        ┌───────────────────┐
│     KhachHang     │ 1    N │    LuotLuuTru     │
│ ───────────────── │───────>│ ───────────────── │
│ PK MaKhachHang    │        │ PK MaLuotLuuTru   │
│    Ten, SoDienThoai│       │ FK MaPhong        │
│    SoGiayTo       │        │ FK MaKhachHang    │
└───────────────────┘        │    ThoiDiemBatDau │
                             │    ThoiDiemKetThuc│
                             │    HanNhanPhong   │
                             │    TrangThai      │
                             └─────────┬─────────┘
                                       │ 1
                 ┌─────────────────────┼─────────────────────┐
                 │ 1                   │ 1                   │ 1
                 ▼ N                   ▼ N                   ▼ 1
       ┌───────────────────┐ ┌───────────────────┐ ┌───────────────────┐
       │    ChangLuuTru    │ │   YeuCauDichVu    │ │     HoaDon        │
       │ ───────────────── │ │ ───────────────── │ │ ───────────────── │
       │ PK MaChang        │ │ PK MaYeuCauDichVu │ │ PK MaHoaDon       │
       │ FK MaLuotLuuTru   │ │ FK MaLuotLuuTru   │ │ FK MaLuotLuuTru   │
       │ FK MaPhong        │ │ FK MaDichVu       │ │    TongTienPhong  │
       │    DonGiaMoiNgay  │ │    SoLuong        │ │    TongTienDichVu │
       │    ThoiDiemBatDau │ │    SoLuongDaGiao  │ │    ThucThu        │
       └───────────────────┘ └─────────┬─────────┘ └─────────┬─────────┘
                                       │                     │ 1
                                       │ N                   ▼ N
                                       ▼ 1         ┌───────────────────┐
                             ┌───────────────────┐ │    ChiTietBill    │
                             │      DichVu       │ │ ───────────────── │
                             │ ───────────────── │ │ PK MaChiTiet      │
                             │ PK MaDichVu       │ │ FK MaHoaDon       │
                             │    Ten, DonGia    │ │    Ten, SoLuong   │
                             └───────────────────┘ └───────────────────┘""")

    # 3.3 Thiết kế chi tiết các bảng
    add_h2(doc, "3.3. Thiết kế chi tiết các bảng")
    add_p(doc, "Dưới đây là đặc tả chi tiết của các bảng cơ sở dữ liệu nòng cốt trong hệ thống:")

    hdr_tbl = ["Tên cột", "Kiểu dữ liệu", "Khóa", "Null", "Mô tả ý nghĩa"]

    # Table Phong
    data_phong = [
        ["MaPhong", "INT IDENTITY(1,1)", "PK", "No", "Mã định danh duy nhất của phòng."],
        ["SoPhong", "NVARCHAR(20)", "UQ", "No", "Số hiệu phòng hiển thị (ví dụ: P101, P202)."],
        ["HangPhong", "NVARCHAR(50)", "-", "No", "Hạng phòng (Standard, Superior, Deluxe, VIP)."],
        ["GiaTieuChuan", "DECIMAL(18,2)", "-", "No", "Đơn giá niêm yết mỗi ngày (block 24 giờ)."],
        ["TrangThai", "NVARCHAR(20)", "-", "No", "Trạng thái vật lý: 'Trong', 'DangO', 'CanDon', 'BaoTri'."],
        ["PhienBan", "INT", "-", "No", "Số phiên bản phục vụ kiểm soát đồng thời (Optimistic Lock)."]
    ]
    create_styled_table(doc, hdr_tbl, data_phong, col_widths=[1.4, 1.6, 0.6, 0.6, 2.6], caption="Bảng 3.1. Thiết kế chi tiết bảng Phong (Phòng lưu trú)")

    # Table LuotLuuTru
    data_stay = [
        ["MaLuotLuuTru", "INT IDENTITY(1,1)", "PK", "No", "Mã định danh duy nhất của lượt lưu trú."],
        ["MaPhong", "INT", "FK", "No", "Khóa ngoại tham chiếu bảng Phong(MaPhong)."],
        ["MaKhachHang", "INT", "FK", "Yes", "Khóa ngoại tham chiếu bảng KhachHang(MaKhachHang)."],
        ["TenKhach", "NVARCHAR(100)", "-", "No", "Họ tên khách hàng đại diện."],
        ["SoDienThoai", "VARCHAR(20)", "-", "No", "Số điện thoại liên lạc."],
        ["SoGiayTo", "VARCHAR(30)", "-", "Yes", "Số căn cước công dân hoặc hộ chiếu."],
        ["ThoiDiemBatDau", "DATETIME2", "-", "No", "Thời điểm bắt đầu ở dự kiến hoặc thực tế."],
        ["ThoiDiemKetThuc", "DATETIME2", "-", "No", "Thời điểm trả phòng dự kiến hoặc thực tế."],
        ["HanNhanPhong", "DATETIME2", "-", "Yes", "Hạn chót giữ chỗ. Quá hạn sẽ bị hủy & mất cọc."],
        ["TrangThai", "VARCHAR(20)", "-", "No", "'Reserved' (Chờ nhận), 'Active' (Đang ở), 'Completed', 'Cancelled'."],
        ["TongTienCoc", "DECIMAL(18,2)", "-", "No", "Tổng số tiền cọc đã thu của lượt lưu trú này."],
        ["ConHieuLuc", "BIT", "-", "No", "1: Đang có hiệu lực; 0: Đã hoàn tất hoặc đã hủy."]
    ]
    create_styled_table(doc, hdr_tbl, data_stay, col_widths=[1.4, 1.6, 0.6, 0.6, 2.6], caption="Bảng 3.2. Thiết kế chi tiết bảng LuotLuuTru (Lượt lưu trú)")

    # Table HoaDon
    data_hd = [
        ["MaHoaDon", "INT IDENTITY(1,1)", "PK", "No", "Mã định danh duy nhất của hóa đơn."],
        ["MaLuotLuuTru", "INT", "FK", "No", "Khóa ngoại tham chiếu bảng LuotLuuTru(MaLuotLuuTru)."],
        ["SoPhongHoaDon", "NVARCHAR(20)", "-", "No", "Số phòng tại thời điểm xuất hóa đơn."],
        ["TenKhach", "NVARCHAR(100)", "-", "No", "Họ tên người thanh toán."],
        ["TongTienPhong", "DECIMAL(18,2)", "-", "No", "Tổng tiền phòng các chặng."],
        ["TongTienDichVu", "DECIMAL(18,2)", "-", "No", "Tổng tiền dịch vụ đã giao."],
        ["TienGiamTru", "DECIMAL(18,2)", "-", "No", "Số tiền chiết khấu hoặc giảm trừ."],
        ["ThucThu", "DECIMAL(18,2)", "-", "No", "Số tiền thực tế khách cần thanh toán."],
        ["PhuongThucThanhToan", "VARCHAR(20)", "-", "No", "'Cash', 'BankTransfer', 'CreditCard'."],
        ["ThoiDiemLapHoaDon", "DATETIME2", "-", "No", "Thời điểm tạo hóa đơn."],
        ["MaNguoiTao", "INT", "FK", "Yes", "Khóa ngoại tham chiếu nhân viên lập hóa đơn."]
    ]
    create_styled_table(doc, hdr_tbl, data_hd, col_widths=[1.4, 1.6, 0.6, 0.6, 2.6], caption="Bảng 3.3. Thiết kế chi tiết bảng HoaDon (Hóa đơn thanh toán)")

    # Table CaTruc
    data_ca = [
        ["MaCaTruc", "INT IDENTITY(1,1)", "PK", "No", "Mã định danh duy nhất của ca trực thu ngân."],
        ["MaThuNgan", "INT", "FK", "No", "Khóa ngoại tham chiếu nhân viên trực ca."],
        ["ThoiDiemMoCa", "DATETIME2", "-", "No", "Thời điểm bắt đầu mở ca trực."],
        ["ThoiDiemDongCa", "DATETIME2", "-", "Yes", "Thời điểm nộp bàn giao ca trực."],
        ["TienDauCa", "DECIMAL(18,2)", "-", "No", "Số tiền mặt ban đầu nhận bàn giao từ ca trước."],
        ["TienMatDuKien", "DECIMAL(18,2)", "-", "Yes", "Số tiền mặt trên sổ sách tính toán được."],
        ["TienMatThucTe", "DECIMAL(18,2)", "-", "Yes", "Số tiền mặt thực tế kiểm đếm lúc kết ca."],
        ["GiaiTrinh", "NVARCHAR(500)", "-", "Yes", "Lý do giải trình nếu phát sinh chênh lệch quỹ."],
        ["ThoiDiemKhoa", "DATETIME2", "-", "Yes", "Thời điểm Kế toán/Quản lý duyệt khóa ca."]
    ]
    create_styled_table(doc, hdr_tbl, data_ca, col_widths=[1.4, 1.6, 0.6, 0.6, 2.6], caption="Bảng 3.4. Thiết kế chi tiết bảng CaTruc (Ca trực thu ngân)")

    # 3.4 Mối liên hệ giữa các bảng
    add_h2(doc, "3.4. Mối liên hệ và khóa ngoại giữa các bảng")
    add_p(doc, "Các ràng buộc khóa ngoại (Foreign Keys) bảo đảm tính toàn vẹn tham chiếu giữa các thực thể:")
    add_bullet(doc, "FK_LuotLuuTru_Phong: LuotLuuTru.MaPhong -> Phong.MaPhong (Quy tắc NO ACTION, không cho phép xóa phòng khi đang có dữ liệu lượt ở).", "• ")
    add_bullet(doc, "FK_LuotLuuTru_KhachHang: LuotLuuTru.MaKhachHang -> KhachHang.MaKhachHang (Hỗ trợ lưu hồ sơ khách hàng trung thành).", "• ")
    add_bullet(doc, "FK_ChangLuuTru_LuotLuuTru: ChangLuuTru.MaLuotLuuTru -> LuotLuuTru.MaLuotLuuTru (Liên kết các chặng phòng của một lượt khách).", "• ")
    add_bullet(doc, "FK_YeuCauDichVu_LuotLuuTru: YeuCauDichVu.MaLuotLuuTru -> LuotLuuTru.MaLuotLuuTru (Ràng buộc các dịch vụ tiêu dùng vào lượt khách).", "• ")
    add_bullet(doc, "FK_HoaDon_LuotLuuTru: HoaDon.MaLuotLuuTru -> LuotLuuTru.MaLuotLuuTru (Mỗi lượt lưu trú chỉ có duy nhất một hóa đơn thanh toán).", "• ")
    add_bullet(doc, "FK_ChiTietBill_HoaDon: ChiTietBill.MaHoaDon -> HoaDon.MaHoaDon (Lưu vết các dòng chi tiết trên bill in cho khách).", "• ")

    # 3.5 Ràng buộc toàn vẹn và chỉ mục
    add_h2(doc, "3.5. Ràng buộc toàn vẹn và chỉ mục")
    add_p(doc, "Hệ thống thiết lập các ràng buộc toàn vẹn (Check Constraints) và chỉ mục độc nhất (Unique Indexes) nhằm ngăn chặn dữ liệu rác từ mức CSDL:")
    add_bullet(doc, "CK_LuotLuuTru_Ngay: Bắt buộc ThoiDiemKetThucDuKien > ThoiDiemBatDau.", "• Ràng buộc ngày tháng hợp lệ: ")
    add_bullet(doc, "CK_Phong_Gia: Bắt buộc GiaTieuChuan >= 0; CK_HoaDon_Tien: Bắt buộc ThucThu >= 0.", "• Ràng buộc số tiền không âm: ")
    add_bullet(doc, "IX_Phong_SoPhong (UNIQUE): Đảm bảo số phòng không bao giờ bị trùng lặp.", "• Chỉ mục số phòng duy nhất: ")
    add_bullet(doc, "IX_TaiKhoanNhanVien_TenDangNhap (UNIQUE): Đảm bảo tên tài khoản nhân viên là duy nhất trong toàn hệ thống.", "• Chỉ mục tài khoản duy nhất: ")
    add_bullet(doc, "IX_GiaoDichThanhToan_Forfeit (UNIQUE FILTERED): Mỗi lượt lưu trú chỉ được phép ghi nhận duy nhất một giao dịch tịch thu cọc Forfeit khi quá hạn.", "• Chỉ mục chống tịch thu cọc trùng: ")

    # 3.8 Cơ chế Schema Migration
    add_h2(doc, "3.8. Cơ chế nâng cấp Schema Migration tự động")
    add_p(doc, "Một điểm nhấn kỹ thuật quan trọng của dự án là lớp SchemaMigrator.cs. Khi phần mềm khởi động, hàm MigrateDatabaseAsync() tự động kết nối vào SQL Server và kiểm tra giá trị Version lớn nhất trong bảng PhienBanCSDL.")
    add_bullet(doc, "Nếu cơ sở dữ liệu chưa tồn tại, hệ thống tự động chạy toàn bộ script Database.sql để tạo cơ sở dữ liệu QLKhachSanApp hoàn chỉnh kèm theo bảng, ràng buộc và dữ liệu mẫu chuẩn.", "• Khởi tạo lần đầu: ")
    add_bullet(doc, "Nếu CSDL đang ở phiên bản cũ (từ V1 đến V25), hệ thống tự động đọc và thực thi lần lượt các đoạn script nâng cấp (Migration Steps) trong một Transaction an toàn.", "• Nâng cấp không mất dữ liệu: ")
    add_bullet(doc, "Các script migration được viết với tính chất Idempotent (chạy lại nhiều lần không sinh lỗi). Nếu có bất kỳ lỗi nào xảy ra trong quá trình nâng cấp, Transaction sẽ tự động Rollback và hiển thị hộp thoại thông báo chi tiết mã lỗi SQL và số dòng để người dùng xử lý.", "• Chống lỗi và tự Rollback: ")

    # 3.10 Mã lệnh DDL tạo từng bảng
    add_h2(doc, "3.10. Mã lệnh DDL tạo các bảng nòng cốt")
    add_p(doc, "Đoạn mã DDL SQL Server chuẩn định nghĩa các bảng trọng yếu:")
    add_code_block(doc,
"""-- BẢNG PHÒNG LƯU TRÚ
CREATE TABLE dbo.Phong (
    MaPhong INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Phong PRIMARY KEY,
    SoPhong NVARCHAR(20) NOT NULL,
    HangPhong NVARCHAR(50) NOT NULL CONSTRAINT DF_Phong_Hang DEFAULT ('Standard'),
    GiaTieuChuan DECIMAL(18,2) NOT NULL CONSTRAINT DF_Phong_Gia DEFAULT (0),
    TrangThai NVARCHAR(20) NOT NULL CONSTRAINT DF_Phong_TrangThai DEFAULT ('Trong'),
    PhienBan INT NOT NULL CONSTRAINT DF_Phong_PhienBan DEFAULT (1),
    CONSTRAINT UQ_Phong_SoPhong UNIQUE (SoPhong),
    CONSTRAINT CK_Phong_Gia CHECK (GiaTieuChuan >= 0)
);

-- BẢNG LƯỢT LƯU TRÚ
CREATE TABLE dbo.LuotLuuTru (
    MaLuotLuuTru INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_LuotLuuTru PRIMARY KEY,
    MaPhong INT NOT NULL CONSTRAINT FK_LuotLuuTru_Phong REFERENCES dbo.Phong(MaPhong),
    MaKhachHang INT NULL CONSTRAINT FK_LuotLuuTru_KhachHang REFERENCES dbo.KhachHang(MaKhachHang),
    TenKhach NVARCHAR(100) NOT NULL,
    SoDienThoai VARCHAR(20) NOT NULL,
    SoGiayTo VARCHAR(30) NULL,
    ThoiDiemBatDau DATETIME2 NOT NULL,
    ThoiDiemKetThuc DATETIME2 NOT NULL,
    HanNhanPhong DATETIME2 NULL,
    TrangThai VARCHAR(20) NOT NULL CONSTRAINT DF_LuotLuuTru_TrangThai DEFAULT ('Reserved'),
    TongTienCoc DECIMAL(18,2) NOT NULL CONSTRAINT DF_LuotLuuTru_Coc DEFAULT (0),
    ConHieuLuc BIT NOT NULL CONSTRAINT DF_LuotLuuTru_ConHieuLuc DEFAULT (1),
    CONSTRAINT CK_LuotLuuTru_Ngay CHECK (ThoiDiemKetThuc > ThoiDiemBatDau)
);""")

    # 3.14 Bảo mật dữ liệu
    add_h2(doc, "3.14. Bảo mật dữ liệu")
    add_p(doc, "Chính sách bảo mật cơ sở dữ liệu được triển khai theo nhiều tầng bảo vệ:")
    add_bullet(doc, "Tuyệt đối không lưu trữ mật khẩu rõ (plaintext). Mật khẩu được băm bằng thuật toán PBKDF2 với hàm băm nền HMAC-SHA512, sử dụng muối ngẫu nhiên (Cryptographic Salt) độ dài 16 bytes và 10.000 vòng lặp lặp lại, đạt tiêu chuẩn bảo mật của NIST.", "• Mã hóa mật khẩu chuẩn PBKDF2: ")
    add_bullet(doc, "Chuỗi kết nối SQL Server mặc định dùng Integrated Security (Windows Authentication) cho môi trường nội bộ, không ghi hardcode tên đăng nhập và mật khẩu sa trong tệp mã nguồn.", "• Xác thực kết nối Windows: ")
    add_bullet(doc, "Mọi biến động dữ liệu nhạy cảm (tạo tài khoản, đổi quyền, hủy cọc, xóa dịch vụ) đều tự động ghi một bản ghi vào bảng NhatKyThaoTac kèm thông tin: mã nhân viên, hành động, mô tả chi tiết và thời điểm thực hiện.", "• Nhật ký kiểm toán không thể xóa: ")

    doc.add_page_break()
