import docx
from docx.shared import Inches, Pt, RGBColor
from docx.enum.text import WD_ALIGN_PARAGRAPH
from .styles import (
    COLOR_PRIMARY, COLOR_SECONDARY, COLOR_TEXT,
    add_h1, add_h2, add_h3, add_p, add_bullet, add_callout,
    create_styled_table, add_code_block
)

def build_chapter2(doc):
    add_h1(doc, "CHƯƠNG 2. PHÂN TÍCH CHỨC NĂNG")

    # 2.1 Đặc tả chi tiết các chức năng chính
    add_h2(doc, "2.1. Đặc tả chi tiết các chức năng chính")

    add_h3(doc, "2.1.1. Chức năng Xác thực & Quản trị tài khoản")
    add_p(doc, "Chức năng chịu trách nhiệm nhận diện người dùng, kiểm soát quyền truy cập và bảo vệ an toàn cho toàn bộ hệ thống:")
    add_bullet(doc, "Khi người dùng nhập tên đăng nhập và mật khẩu, hệ thống tìm kiếm trong bảng TaiKhoanNhanVien. Mật khẩu nhập vào được băm cùng chuỗi muối (Salt) lưu trong CSDL bằng thuật toán PBKDF2 SHA-512 với 10.000 vòng lặp. Nếu chuỗi băm trùng khớp, tài khoản đang ở trạng thái DangHoatDong=1 và thời điểm hiện tại vượt qua mốc KhoaDen (nếu bị khóa tạm), hệ thống sẽ cấp phiên làm việc (Session) mới.", "• Cơ chế xác thực an toàn: ")
    add_bullet(doc, "Nếu nhập sai mật khẩu liên tiếp 5 lần, tài khoản sẽ tự động bị khóa trong 5 phút (KhoaDen = DATEADD(minute, 5, GETDATE())). Số lần đăng nhập sai (SoLanDangNhapSai) được reset về 0 ngay khi đăng nhập đúng.", "• Chống tấn công dò mật khẩu (Brute-Force): ")
    add_bullet(doc, "Mỗi tài khoản lưu một trường SecurityVersion. Khi Admin đổi quyền, đổi mật khẩu hoặc khóa tài khoản của nhân viên, SecurityVersion tăng lên 1 đơn vị. Các phiên làm việc cũ kiểm tra thấy sai lệch SecurityVersion sẽ tự động bị đăng xuất ngay lập tức.", "• Quản lý phiên và SecurityVersion: ")

    add_h3(doc, "2.1.2. Chức năng Sơ đồ phòng & Đặt phòng trực quan")
    add_p(doc, "Sơ đồ phòng là trung tâm điều khiển vận hành của khách sạn:")
    add_bullet(doc, "Toàn bộ danh sách phòng được vẽ động dạng lưới các ô thẻ (Room Card). Mỗi ô hiển thị rõ: Số phòng (P101, P102...), Loại phòng (Standard, Deluxe, VIP), Đơn giá/ngày và Trạng thái vật lý thông qua màu nền tương ứng. Dấu hiệu '*' hiển thị sau số phòng báo hiệu phòng này hiện đang có lịch đặt trước trong tương lai.", "• Sơ đồ phòng trực quan: ")
    add_bullet(doc, "Khi lễ tân chọn tính năng 'Đặt phòng trước', màn hình hiển thị bộ lọc: Ngày đến (NgayBatDau) và Ngày đi (NgayKetThuc). Hệ thống thực thi truy vấn tìm kiếm các phòng thỏa mãn điều kiện: không trùng với bất kỳ khoảng [BatDau, KetThuc) nào của các lượt đặt còn hiệu lực khác, và không ở trạng thái Bảo trì.", "• Thuật toán tìm phòng trống: ")
    add_bullet(doc, "Đối với các đoàn khách du lịch hoặc hội nghị cần đặt cùng lúc nhiều phòng, hệ thống cung cấp giao diện Đặt phòng theo đoàn (Multiple Booking), cho phép chọn nhanh nhiều phòng trống cùng hạng hoặc khác hạng, áp dụng chung thông tin trưởng đoàn và một mã giao dịch cọc duy nhất.", "• Đặt phòng theo đoàn: ")

    add_h3(doc, "2.1.3. Chức năng Quản lý Lưu trú (Check-in, Đổi phòng, Check-out)")
    add_p(doc, "Quản lý xuyên suốt vòng đời lưu trú của khách tại khách sạn:")
    add_bullet(doc, "Chỉ cho phép nhận phòng khi phòng vật lý đang ở trạng thái 'Trống' và đã dọn vệ sinh xong. Khi nhận phòng, hệ thống chuyển trạng thái phòng sang 'Đang ở', tạo bản ghi Chặng lưu trú (ChangLuuTru) đầu tiên ghi nhận thời điểm bắt đầu và đơn giá phòng tại thời điểm chốt hợp đồng.", "• Nhận phòng (Check-in): ")
    add_bullet(doc, "Khi khách có nhu cầu chuyển sang phòng khác, hệ thống đóng chặng lưu trú hiện tại (cập nhật ThoiDiemKetThuc của ChangLuuTru cũ) và mở chặng lưu trú mới cho phòng mới. Tiền phòng cuối cùng sẽ được tính công bằng theo tổng số tiền của từng chặng thời gian thực tế.", "• Đổi phòng linh hoạt: ")
    add_bullet(doc, "Khách có thể yêu cầu ở thêm; hệ thống kiểm tra khoảng thời gian gia hạn có bị đè lên lịch của khách khác hay không. Nếu không trùng, hệ thống cập nhật ngày trả dự kiến.", "• Gia hạn thời gian ở: ")

    add_h3(doc, "2.1.4. Chức năng Dịch vụ & Kho Minibar")
    add_p(doc, "Quản lý tiêu dùng tại phòng và kiểm soát xuất nhập tồn hàng hóa:")
    add_bullet(doc, "Lễ tân hoặc buồng phòng tạo yêu cầu dịch vụ (YeuCauDichVu) gắn trực tiếp với mã lượt lưu trú (MaLuotLuuTru), bao gồm: mặt hàng minibar, đồ uống, giặt ủi, thuê xe.", "• Gọi dịch vụ buồng phòng: ")
    add_bullet(doc, "Hệ thống hỗ trợ quy trình giao hàng từng phần. Khi giao hàng thành công, hệ thống tự động ghi nhận một bản ghi Biến động kho (BienDongKho) với loại xuất bán ('XuatBan'), tự động trừ số lượng tồn kho trong HangTonKho.", "• Xuất bán và trừ tồn kho tự động: ")
    add_bullet(doc, "Các mặt hàng nước giải khát, đồ ăn vặt minibar hết hạn sử dụng hoặc hư hỏng được nhân viên lập phiếu xuất hủy/dùng nội bộ có ghi rõ lý do để phục vụ đối soát kiểm kê cuối tháng.", "• Quản lý hao hụt minibar: ")

    add_h3(doc, "2.1.5. Chức năng Thanh toán hóa đơn & VietQR động")
    add_p(doc, "Khâu chốt doanh thu và hoàn tất dịch vụ cho khách hàng:")
    add_bullet(doc, "Thời gian ở được tính tối thiểu 1 ngày (24 giờ). Các giờ tiếp theo được làm tròn lên block 24 giờ kế tiếp. Tổng hóa đơn = Tiền phòng (các chặng) + Tiền dịch vụ (đã giao) - Tiền cọc đã nộp.", "• Thuật toán tính tiền chuẩn xác: ")
    add_bullet(doc, "Hệ thống tích hợp module PaymentQr.cs. Khi lễ tân bấm chọn phương thức 'Chuyển khoản ngân hàng', phần mềm tự động gọi dịch vụ VietQR Open API, truyền các tham số: Mã ngân hàng (BankCode), Số tài khoản (BankAccount), Tên chủ tài khoản, Số tiền cần thanh toán chính xác đến từng đồng và Nội dung chuyển khoản (Ví dụ: 'P101 THANH TOAN HD125'). Một mã QR chuẩn NAPAS hiển thị trực tiếp trên màn hình để khách quét và thanh toán tức thì qua App Mobile Banking của mọi ngân hàng Việt Nam.", "• Thanh toán quét mã VietQR động: ")
    add_bullet(doc, "Lưu trữ toàn bộ danh sách các dòng dịch vụ và tiền phòng vào bảng ChiTietBill để in hóa đơn đẹp mắt, lưu trữ phục vụ tra cứu lịch sử kể cả khi bảng giá dịch vụ sau này có thay đổi.", "• Lưu trữ chi tiết Bill: ")

    add_h3(doc, "2.1.6. Chức năng Quản lý Ca trực & Sổ quỹ")
    add_p(doc, "Quản lý minh bạch dòng tiền mặt tại quầy lễ tân:")
    add_bullet(doc, "Nhân viên bắt đầu ca trực phải mở ca (CaTruc), nhập số tiền mặt đầu ca được bàn giao lại từ ca trước.", "• Mở ca trực: ")
    add_bullet(doc, "Trong ca, toàn bộ tiền thu cọc, thu checkout, chi hoàn cọc được ghi nhận gắn chặt với mã ca trực hiện tại (MaCaTruc).", "• Theo dõi dòng tiền trong ca: ")
    add_bullet(doc, "Cuối ca, thu ngân đếm tiền mặt thực tế và nhập vào hệ thống. Phần mềm tự động so khớp với số tiền trên sổ sách (TienMatDuKien = TienDauCa + ThuTienMat - ChiTienMat). Nếu có chênh lệch thừa/thiếu, nhân viên bắt buộc phải nhập lý do giải trình.", "• Kết ca & Giải trình chênh lệch: ")
    add_bullet(doc, "Kế toán hoặc Quản lý kiểm tra và duyệt Khóa ca (ThoiDiemKhoa). Sau khi khóa, toàn bộ các phiếu thu chi và hóa đơn trong ca trực không thể bị sửa đổi hay xóa bỏ.", "• Khóa ca an toàn: ")

    add_h3(doc, "2.1.7. Chức năng Trình thiết kế màn hình động (Custom Screen Designer)")
    add_p(doc, "Một tính năng độc đáo và vượt trội của phần mềm, mang lại khả năng mở rộng không giới hạn mà không cần viết lại mã nguồn C#:")
    add_bullet(doc, "Người quản trị (Admin) có thể vào mục 'Tùy chỉnh menu -> Thiết kế chức năng' để tạo thêm một màn hình quản lý mới.", "• Thiết kế biểu mẫu không cần code: ")
    add_bullet(doc, "Hỗ trợ đa dạng các loại trường nhập liệu: Văn bản đơn dòng (Text), Văn bản nhiều dòng (TextArea), Số nguyên/Số thực (Number), Ngày tháng (Date), Lựa chọn ComboBox (Dropdown), Đánh dấu (Checkbox). Cho phép cấu hình hiển thị toàn hàng (Full width) hoặc nửa hàng (Half width).", "• Đa dạng kiểu trường: ")
    add_bullet(doc, "Toàn bộ cấu trúc giao diện được lưu dạng JSON trong bảng CauHinhChucNang. Dữ liệu người dùng nhập trên màn hình động được lưu linh hoạt trong bảng DuLieuChucNangMoi.", "• Lưu trữ cấu trúc và dữ liệu động: ")

    add_h3(doc, "2.1.8. Chức năng Báo cáo thống kê & Doanh thu")
    add_p(doc, "Hỗ trợ đắc lực cho ban quản lý trong việc đánh giá hiệu quả kinh doanh:")
    add_bullet(doc, "Thống kê chi tiết doanh thu theo từng nguồn: Doanh thu tiền phòng, Doanh thu dịch vụ minibar, Doanh thu từ tiền cọc bị tịch thu (Forfeit). Phân tách rõ ràng giữa tiền mặt và chuyển khoản ngân hàng.", "• Báo cáo doanh thu đa chiều: ")
    add_bullet(doc, "Đo lường hiệu suất khai thác phòng của khách sạn theo từng tuần, tháng hoặc mùa cao điểm.", "• Tỷ lệ lấp đầy phòng (Occupancy Rate): ")
    add_bullet(doc, "Tất cả các bảng báo cáo đều tích hợp tính năng xuất file CSV định dạng UTF-8 có BOM (Byte Order Mark) giúp mở trực tiếp trên Microsoft Excel không bị lỗi font tiếng Việt.", "• Xuất dữ liệu báo cáo: ")

    # 2.2 Kiến trúc hệ thống
    add_h2(doc, "2.2. Kiến trúc hệ thống")
    add_h3(doc, "2.2.1. Mô hình phân tầng 3-Tier (GUI - BLL - DAL - DTO)")
    add_p(doc, "Ứng dụng được kiến trúc theo mô hình 3 tầng (3-Tier Architecture) kinh điển, kết hợp tầng DTO trung gian, giúp tách biệt rành mạch trách nhiệm của từng thành phần, tăng tính tái sử dụng và dễ dàng bảo trì:")

    add_bullet(doc, "Bao gồm các Form (FormLogin, FormMain, AccountingForm), UserControl (ucDashboard và các tệp partial actions, booking, billing, catalog, navigation) và thư viện điều khiển ModernControls. Tầng này chỉ làm nhiệm vụ tiếp nhận sự kiện từ người dùng (chuột, bàn phím), hiển thị dữ liệu và không chứa logic tính toán nghiệp vụ hay câu lệnh SQL.", "1. Tầng Giao diện (Presentation Layer - GUI): ")
    add_bullet(doc, "Bao gồm các lớp AuthService, HotelService, FunctionPolicy, RolePolicy. Tầng này đóng vai trò 'bộ não' của ứng dụng: kiểm tra tính hợp lệ của dữ liệu, kiểm tra quyền hạn nhân viên, áp dụng các quy tắc nghiệp vụ (tính tiền phòng block 24h, kiểm tra thời hạn giữ chỗ 24h/15 ngày, chính sách tịch thu cọc) trước khi ủy thác cho tầng dưới.", "2. Tầng Nghiệp vụ (Business Logic Layer - BLL): ")
    add_bullet(doc, "Bao gồm các lớp HotelRepository, HotelTransaction.Commands, HotelTransaction.Management, SchemaMigrator, DatabaseHelper. Tầng này trực tiếp giao tiếp với SQL Server qua kết nối ADO.NET SqlConnection/SqlCommand, quản lý transaction, sử dụng truy vấn tham số hóa (Parameterized Queries) và thực thi các thao tác CRUD.", "3. Tầng Truy cập Dữ liệu (Data Access Layer - DAL): ")
    add_bullet(doc, "Bao gồm các lớp chứa dữ liệu thuần túy (POCO - Plain Old CLR Objects) như RoomDto, StayDto, ServiceOrderDto, InvoiceDto, UserAccountDto. DTO đóng vai trò 'vật mang tin' chuyển giao dữ liệu an toàn xuyên suốt qua lại giữa các tầng mà không làm lộ cấu trúc CSDL bên dưới.", "4. Tầng Đối tượng Truyền dữ liệu (Data Transfer Objects - DTO): ")

    add_h3(doc, "2.2.2. Mẫu thiết kế Repository Pattern & Transaction Script")
    add_p(doc, "Phần mềm áp dụng kết hợp hai mẫu thiết kế kinh điển:")
    add_bullet(doc, "HotelRepository đóng vai trò lớp trung gian trừu tượng hóa các thao tác đọc dữ liệu CSDL. Các tầng trên chỉ cần gọi các phương thức như GetActiveRoomsAsync(), GetCustomerHistoryAsync() mà không cần quan tâm đến cú pháp SQL bên dưới.", "• Repository Pattern: ")
    add_bullet(doc, "Đối với các nghiệp vụ phức tạp đòi hỏi ghi dữ liệu vào nhiều bảng đồng thời (ví dụ: tạo lượt ở, nhận cọc, tạo khách hàng mới), logic được đóng gói thành các Transaction Script hoàn chỉnh trong HotelTransaction.Commands.cs, chạy trọn vẹn trong một SqlTransaction với khóa độc quyền sp_getapplock.", "• Transaction Script & Khóa giao dịch: ")

    add_h3(doc, "2.2.3. Luồng dữ liệu tổng quát giữa các tầng")
    add_p(doc, "Sơ đồ luồng dữ liệu minh họa thao tác Đặt phòng trước của người dùng:")
    add_code_block(doc,
"""[Người dùng]
     │ (1) Nhập thông tin ngày ở & chọn phòng trên màn hình BookingDialog
     ▼
[Tầng GUI: ucDashboard.Actions.cs]
     │ (2) Đóng gói dữ liệu vào CreateStayDto và gọi BLL
     ▼
[Tầng BLL: HotelService.cs -> CreateStayAsync]
     │ (3) Kiểm tra quyền Reception/Admin, kiểm tra ngày đến < ngày đi
     │ (4) Kiểm tra tính hợp lệ số điện thoại và hạn nhận phòng
     ▼
[Tầng DAL: HotelTransaction.Commands.cs -> ExecuteCreateStayTransaction]
     │ (5) Mở SqlTransaction & yêu cầu sp_getapplock
     │ (6) Truy vấn kiểm tra chống xung đột lịch phòng trong CSDL
     │ (7) INSERT/UPDATE KhachHang -> INSERT LuotLuuTru -> INSERT GiaoDichThanhToan (nếu có cọc)
     │ (8) Ghi nhật ký vào NhatKyThaoTac -> Commit Transaction
     ▼
[Cơ sở dữ liệu: SQL Server]
     │ (9) Dữ liệu lưu vĩnh viễn trên đĩa cứng
     ▼
[Tầng GUI]
     │ (10) Nhận kết quả thành công -> Cập nhật lại sơ đồ phòng thời gian thực""")

    # 2.3 Các câu lệnh truy vấn chính
    add_h2(doc, "2.3. Các câu lệnh truy vấn nghiệp vụ chính")
    add_p(doc, "Dưới đây là một số câu lệnh truy vấn cốt lõi được sử dụng trong hệ thống:")

    add_p(doc, "1. Truy vấn kiểm tra xung đột lịch đặt phòng (Overlap Detection):", bold=True)
    add_code_block(doc,
"""SELECT COUNT(1)
FROM dbo.LuotLuuTru
WHERE MaPhong = @MaPhong
  AND ConHieuLuc = 1
  AND TrangThai IN ('Reserved', 'Active')
  AND MaLuotLuuTru <> @ExcludeStayId
  AND NOT (ThoiDiemKetThucDuKien <= @NgayBatDau OR ThoiDiemBatDau >= @NgayKetThuc);""")

    add_p(doc, "2. Truy vấn tính toán tổng tiền phòng theo lịch sử các chặng lưu trú (ChangLuuTru):", bold=True)
    add_code_block(doc,
"""SELECT
    c.MaChang, c.MaPhong, p.SoPhong, c.DonGiaMoiNgay,
    c.ThoiDiemBatDau, c.ThoiDiemKetThuc,
    DATEDIFF(hour, c.ThoiDiemBatDau, ISNULL(c.ThoiDiemKetThuc, GETDATE())) AS SoGioThucTe
FROM dbo.ChangLuuTru c
INNER JOIN dbo.Phong p ON c.MaPhong = p.MaPhong
WHERE c.MaLuotLuuTru = @MaLuotLuuTru
ORDER BY c.ThoiDiemBatDau ASC;""")

    add_p(doc, "3. Khóa ứng dụng độc quyền bảo vệ giao dịch đặt phòng (sp_getapplock):", bold=True)
    add_code_block(doc,
"""DECLARE @lockResult int;
EXEC @lockResult = sys.sp_getapplock
    @Resource = N'QLKhachSan.BookingLock',
    @LockMode = 'Exclusive',
    @LockOwner = 'Transaction',
    @LockTimeout = 5000;
IF @lockResult < 0 THROW 50001, N'Không thể lấy khóa đặt phòng, vui lòng thử lại.', 1;""")

    # 2.4 Ma trận chức năng - dữ liệu
    add_h2(doc, "2.4. Ma trận chức năng – dữ liệu (CRUD Matrix)")
    add_p(doc, "Ma trận CRUD thể hiện mối quan hệ tương tác giữa 8 nhóm chức năng chính của phần mềm và 20 bảng cơ sở dữ liệu (C: Create, R: Read, U: Update, D: Delete):")

    hdr_crud = ["Nhóm chức năng", "TaiKhoan", "Phong", "KhachHang", "LuotLuuTru", "ChangLuuTru", "DichVu", "YeuCauDV", "HangTonKho", "HoaDon", "GiaoDich", "CaTruc"]
    data_crud = [
        ["1. Xác thực & Tài khoản", "C R U", "-", "-", "-", "-", "-", "-", "-", "-", "-", "R"],
        ["2. Sơ đồ & Trạng thái", "-", "R U", "R", "R", "R", "-", "-", "-", "-", "-", "-"],
        ["3. Đặt phòng & Lưu trú", "-", "R U", "C R U", "C R U D", "C R U", "-", "-", "-", "-", "C R", "R"],
        ["4. Dịch vụ & Minibar", "-", "R", "-", "R", "-", "R", "C R U D", "R U", "-", "-", "R"],
        ["5. Trả phòng & Hóa đơn", "-", "R U", "R", "R U", "R U", "R", "R", "R", "C R", "C R", "R U"],
        ["6. Kế toán & Ca trực", "-", "-", "-", "R", "-", "-", "-", "-", "R", "R", "C R U"],
        ["7. Tùy biến Menu/Screen", "R", "-", "-", "-", "-", "-", "-", "-", "-", "-", "-"],
        ["8. Báo cáo & Thống kê", "R", "R", "R", "R", "R", "R", "R", "R", "R", "R", "R"]
    ]
    create_styled_table(doc, hdr_crud, data_crud, col_widths=[1.5, 0.6, 0.5, 0.6, 0.6, 0.6, 0.5, 0.6, 0.6, 0.5, 0.6, 0.5], caption="Bảng 2.1. Ma trận chức năng - dữ liệu (CRUD Matrix)")

    doc.add_page_break()
