import docx
from docx.shared import Inches, Pt, RGBColor
from docx.enum.text import WD_ALIGN_PARAGRAPH
from .styles import (
    COLOR_PRIMARY, COLOR_SECONDARY, COLOR_TEXT,
    add_h1, add_h2, add_h3, add_p, add_bullet, add_callout,
    create_styled_table, add_image_box, add_code_block
)

def build_chapter4(doc):
    add_h1(doc, "CHƯƠNG 4. CHƯƠNG TRÌNH GIAO DIỆN")

    # 4.1 Môi trường và cách chạy
    add_h2(doc, "4.1. Môi trường và cách chạy chương trình")
    add_p(doc, "Ứng dụng Quản lý Khách sạn được xây dựng trên nền tảng công nghệ tiên tiến nhất hiện nay:")
    add_bullet(doc, "Hệ điều hành: Microsoft Windows 10 / Windows 11 (hỗ trợ đầy đủ bản 64-bit).", "• Môi trường hệ điều hành: ")
    add_bullet(doc, "Nền tảng phát triển: .NET SDK 10.0 (Desktop Runtime), ngôn ngữ C# 10 / C# 12.", "• Nền tảng framework: ")
    add_bullet(doc, "Hệ quản trị CSDL: Microsoft SQL Server 2019 / 2022 hoặc phiên bản miễn phí SQL Server Express (hoặc LocalDB).", "• Hệ thống cơ sở dữ liệu: ")
    add_bullet(doc, "Thư viện mở rộng: Microsoft.Data.SqlClient (ADO.NET), SkiaSharp / QRCoder / VietQR Open API.", "• Thư viện phụ thuộc: ")

    add_p(doc, "Các bước biên dịch và khởi chạy chương trình từ mã nguồn:")
    add_bullet(doc, "Cách 1 (Sử dụng Visual Studio): Mở tệp giải pháp QLKhachSan.slnx hoặc QLKhachSan.sln bằng Visual Studio 2022. Chọn cấu hình Debug hoặc Release, đặt dự án QLKhachSan làm Startup Project và nhấn phím F5 để biên dịch và chạy.", "• ")
    add_bullet(doc, "Cách 2 (Sử dụng dòng lệnh .NET CLI): Mở cửa sổ Terminal (PowerShell hoặc Command Prompt) tại thư mục gốc của dự án và chạy lệnh sau:", "• ")
    add_code_block(doc, "dotnet run --project QLKhachSan")

    # 4.2 Thiết kế giao diện chung
    add_h2(doc, "4.2. Thiết kế giao diện chung")
    add_p(doc, "Khác với các ứng dụng Windows Forms truyền thống thường mang phong cách xám cổ điển, phần mềm Quản lý Khách sạn được thiết kế theo triết lý giao diện phẳng hiện đại (Modern Flat / Fluent UI) với các đặc trưng nổi bật:")
    add_bullet(doc, "Toàn bộ giao diện được xây dựng 100% bằng mã nguồn C# hướng đối tượng kết hợp bộ điều khiển tùy biến ModernControls (Panel bo tròn, nút bấm phẳng có hiệu ứng đổi màu hover/pressed, ô nhập liệu hiện đại, thanh cuộn mượt mà), không sử dụng giao diện kéo thả Designer giúp tăng tốc độ tải màn hình và tối ưu hóa tài nguyên.", "• Thiết kế giao diện thuần mã nguồn (Code-only): ")
    add_bullet(doc, "Hệ thống hỗ trợ cơ chế giao diện động WorkspaceSkin và AppTheme, cho phép tùy chỉnh màu sắc chủ đạo, chế độ nền sáng/tối (Dark/Light mode) linh hoạt.", "• Hệ thống Theme & Màu sắc chuyên nghiệp: ")
    add_bullet(doc, "Khung cửa sổ chính được thiết kế linh hoạt, tự động co giãn theo độ phân giải màn hình từ chuẩn HD (1366x768) đến Full HD (1920x1080) và 2K/4K; xử lý chống mờ khi phóng đại DPI 125% - 150% trên Windows 11.", "• Hỗ trợ đa độ phân giải & Tỷ lệ DPI: ")

    # 4.3 Các màn hình của chương trình
    add_h2(doc, "4.3. Các màn hình của chương trình")
    add_p(doc, "Mỗi màn hình dưới đây được trình bày đầy đủ gồm: mô tả nghiệp vụ, bảng chú thích các thành phần giao diện và hình ảnh chụp thực tế khi ứng dụng đang vận hành:")

    # 4.3.1 Khung chính
    add_h3(doc, "4.3.1. Bố cục khung chính & Thanh điều hướng (Main Navigation)")
    add_p(doc, "Khung chính của ứng dụng đóng vai trò là bảng điều khiển trung tâm, bao gồm thanh tiêu đề trên cùng (hiển thị thông tin khách sạn, tên đăng nhập nhân viên, avatar cá nhân, trạng thái ca trực và đồng hồ thời gian thực) và vùng làm việc đa năng hiển thị menu các phân hệ.")
    add_image_box(doc, ".verify-build/ui-screens/menu-1920x1080.png", "Hình 4.1. Giao diện Menu tổng và khung điều hướng chính của hệ thống")

    hdr_ctrl = ["STT", "Tên thành phần", "Kiểu điều khiển", "Chức năng nghiệp vụ"]
    data_nav = [
        ["1", "Header Profile & Avatar", "Panel / CustomAvatar", "Hiển thị ảnh đại diện nhân viên, tên đăng nhập, vai trò và ca trực hiện tại."],
        ["2", "Đồng hồ hệ thống", "Label (Timer cập nhật)", "Hiển thị ngày giờ hệ thống chính xác từng giây phục vụ tính giờ check-in/out."],
        ["3", "Ô Quản lý phòng", "ModernTileButton", "Truy cập các chức năng: Sơ đồ phòng, Đặt phòng, Lưu trú, Danh mục phòng."],
        ["4", "Ô Dịch vụ & Minibar", "ModernTileButton", "Truy cập các chức năng: Gọi dịch vụ, Giao hàng, Quản lý kho minibar."],
        ["5", "Ô Khách hàng", "ModernTileButton", "Tra cứu hồ sơ khách hàng, lịch sử lưu trú và thông tin khách quen."],
        ["6", "Ô Ca trực", "ModernTileButton", "Mở trực tiếp màn hình Ca trực thu ngân, xem số dư đầu ca và bàn giao ca."],
        ["7", "Ô Tài chính", "ModernTileButton", "Gom nhóm các chức năng: Sổ quỹ thu chi, Quản lý hóa đơn, Báo cáo doanh thu."],
        ["8", "Ô Nhân viên & Hệ thống", "ModernTileButton", "Quản trị người dùng, cấp quyền chức năng, nhật ký hệ thống và thiết kế màn hình động."]
    ]
    create_styled_table(doc, hdr_ctrl, data_nav, col_widths=[0.6, 2.0, 1.8, 2.8], caption="Bảng 4.1. Chú thích các thành phần trên Menu tổng")

    # 4.3.2 Sơ đồ phòng
    add_h3(doc, "4.3.2. Màn hình Sơ đồ phòng trực quan (Room Map Dashboard)")
    add_p(doc, "Màn hình Sơ đồ phòng là nơi nhân viên lễ tân thao tác thường xuyên nhất. Mỗi phòng hiển thị dạng thẻ trực quan đổi màu theo trạng thái thực tế: Xanh lá (Phòng trống), Đỏ (Đang có khách ở), Cam (Phòng cần dọn vệ sinh), Xám (Phòng đang bảo trì).")
    add_image_box(doc, ".verify-build/ui-screens/room-map-1920x1080.png", "Hình 4.2. Giao diện Sơ đồ phòng trực quan với màu sắc trạng thái thời gian thực")

    data_map = [
        ["1", "Thanh lọc trạng thái", "FlowLayoutPanel / FilterButtons", "Lọc nhanh danh sách phòng: Tất cả, Phòng trống, Đang ở, Cần dọn, Bảo trì."],
        ["2", "Thẻ phòng (Room Card)", "ModernRoomCardControl", "Hiển thị số phòng (P101, P102...), hạng phòng, đơn giá, tên khách đang ở."],
        ["3", "Biểu tượng dấu sao (*)", "Label", "Chỉ báo phòng hiện có lịch đặt trước trong tương lai để lễ tân lưu ý."],
        ["4", "Thanh trạng thái dưới", "StatusStrip", "Hiển thị tổng số phòng, số phòng trống, số phòng đang ở và tỷ lệ lấp đầy hiện tại."]
    ]
    create_styled_table(doc, hdr_ctrl, data_map, col_widths=[0.6, 2.0, 1.8, 2.8], caption="Bảng 4.2. Chú thích các thành phần trên Sơ đồ phòng")

    # 4.3.3 Đặt phòng
    add_h3(doc, "4.3.3. Màn hình Đặt phòng trước & Tra cứu phòng trống (Booking Dialog)")
    add_p(doc, "Khi khách liên hệ đặt phòng, lễ tân chọn khoảng thời gian đến và đi. Hệ thống tự động lọc ra các phòng còn trống và mở hộp thoại đặt phòng chi tiết:")
    add_image_box(doc, ".verify-build/ui-screens/booking-1920x1080.png", "Hình 4.3. Giao diện Đặt phòng trước và kiểm tra tính khả dụng của phòng")

    data_book = [
        ["1", "Bộ chọn ngày đến / ngày đi", "ModernDatePicker", "Chọn thời điểm check-in dự kiến và check-out dự kiến."],
        ["2", "Danh sách phòng trống", "DataGridView / CardList", "Hiển thị các phòng thỏa mãn điều kiện không trùng lịch."],
        ["3", "Khung thông tin khách hàng", "GroupBox / TextBox", "Nhập họ tên, số điện thoại, số CCCD/Hộ chiếu của khách hàng."],
        ["4", "Khung tiền cọc & Hạn nhận", "NumericBox / Label", "Nhập số tiền cọc; hệ thống tự tính hạn giữ chỗ 24h (chưa cọc) hoặc 15 ngày (đã cọc)."],
        ["5", "Nút 'Lưu đặt phòng'", "ModernButton", "Thực thi transaction kiểm tra xung đột và lưu trữ đặt phòng vào CSDL."]
    ]
    create_styled_table(doc, hdr_ctrl, data_book, col_widths=[0.6, 2.0, 1.8, 2.8], caption="Bảng 4.3. Chú thích các thành phần trên Màn hình Đặt phòng")

    # 4.3.4 Danh mục phòng & Bảng giá
    add_h3(doc, "4.3.4. Màn hình Quản lý Danh mục phòng & Bảng giá (Room Catalog & Pricing)")
    add_p(doc, "Phân hệ Quản lý phòng cung cấp các màn hình quản trị danh mục số hiệu phòng, thiết lập giá tiêu chuẩn theo hạng phòng:")
    add_image_box(doc, ".verify-build/ui-screens/ShowRoomCatalog-1920x1080.png", "Hình 4.4. Màn hình Quản lý danh mục phòng khách sạn")
    add_image_box(doc, ".verify-build/ui-screens/ShowPricing-1920x1080.png", "Hình 4.5. Màn hình Thiết lập bảng giá theo loại phòng")

    # 4.3.5 Trả phòng & VietQR
    add_h3(doc, "4.3.5. Màn hình Trả phòng & Thanh toán VietQR động (Billing & VietQR)")
    add_p(doc, "Màn hình tính tiền tự động tổng hợp tiền phòng các chặng, tiền dịch vụ minibar, trừ tiền cọc đã thu và hiển thị mã VietQR chuyển khoản động:")
    add_image_box(doc, ".verify-build/checkout-qr-final/checkout-workspace-1366x768.png", "Hình 4.6. Màn hình Tính tiền và xem trước Hóa đơn chi tiết khi Check-out")
    add_image_box(doc, ".verify-build/checkout-qr-final/checkout-transfer-1366x768.png", "Hình 4.7. Màn hình Tạo mã VietQR thanh toán động theo chuẩn NAPAS 247")

    data_qr = [
        ["1", "Chi tiết tiền phòng", "DataGridView", "Liệt kê từng chặng phòng, đơn giá ngày, số giờ ở thực tế và thành tiền."],
        ["2", "Chi tiết dịch vụ minibar", "DataGridView", "Liệt kê các dịch vụ buồng phòng đã giao, số lượng và thành tiền."],
        ["3", "Tổng hợp tiền thanh toán", "Panel / BigNumber", "Hiển thị tổng cộng, tiền cọc đã nộp và số tiền thực thu còn lại."],
        ["4", "Mã VietQR động", "PictureBox (Tạo tự động)", "Mã QR chứa BankCode, AccountNo, chính xác số tiền và cú pháp chuyển khoản."],
        ["5", "Nút 'Xác nhận thanh toán'", "ModernButton", "Lưu hóa đơn, tạo phiếu thu tiền và chuyển phòng sang trạng thái Cần dọn."]
    ]
    create_styled_table(doc, hdr_ctrl, data_qr, col_widths=[0.6, 2.0, 1.8, 2.8], caption="Bảng 4.4. Chú thích các thành phần trên Màn hình Trả phòng & VietQR")

    # 4.3.6 Kế toán & Phân quyền
    add_h3(doc, "4.3.6. Màn hình Kế toán Ca trực & Quản trị Phân quyền")
    add_p(doc, "Giao diện quản lý ca trực thu ngân, sổ quỹ và ma trận phân quyền chi tiết của hệ thống:")
    add_image_box(doc, ".verify-build/ui-screens/accounting-1920x1080.png", "Hình 4.8. Màn hình Kế toán ca trực, quản lý sổ quỹ và đối soát thu chi")
    add_image_box(doc, ".verify-build/account-profile/account-tab-2.png", "Hình 4.9. Màn hình Ma trận phân quyền chi tiết từng chức năng cho nhân viên")

    # 4.4 Kịch bản kiểm thử
    add_h2(doc, "4.4. Kịch bản kiểm thử (Test Cases)")
    add_p(doc, "Hệ thống đã trải qua quy trình kiểm thử toàn diện với 35 kịch bản thử nghiệm bao phủ toàn bộ các trường hợp thông thường, trường hợp biên và trường hợp lỗi ngoại lệ (Bảng 4.5):")

    hdr_tc = ["Mã KT", "Phân hệ", "Tên ca kiểm thử", "Dữ liệu đầu vào", "Kết quả mong đợi", "Kết quả thực tế"]
    data_tc = [
        ["TC01", "Hệ thống", "Tự động khởi tạo CSDL lần đầu", "Chạy app trên máy chưa có CSDL", "Tự chạy script tạo CSDL QLKhachSanApp, 20 bảng & dữ liệu mẫu", "Đạt"],
        ["TC02", "Hệ thống", "Chạy lại ứng dụng nhiều lần", "Đóng và mở lại app 5 lần", "Không báo lỗi trùng lặp, CSDL giữ nguyên toàn vẹn", "Đạt"],
        ["TC03", "Bảo mật", "Đăng nhập đúng tài khoản", "admin / 123456", "Đăng nhập thành công, mở Menu tổng đúng quyền Admin", "Đạt"],
        ["TC04", "Bảo mật", "Đăng nhập sai mật khẩu", "admin / wrongpass", "Báo lỗi TB01: Tên đăng nhập hoặc mật khẩu không chính xác", "Đạt"],
        ["TC05", "Bảo mật", "Khóa tài khoản khi sai 5 lần", "Nhập sai mật khẩu liên tiếp 5 lần", "Báo lỗi TB02: Tài khoản bị khóa tạm thời 5 phút", "Đạt"],
        ["TC06", "Bảo mật", "Ghi nhớ mật khẩu an toàn", "Tích chọn 'Ghi nhớ đăng nhập'", "Lưu credential vào DPAPI máy Windows, không lưu plaintext trong SQL", "Đạt"],
        ["TC07", "Lưu trú", "Nhận phòng trực tiếp (Walk-in)", "Chọn P101 đang Trống, nhập khách A", "P101 chuyển sang 'Đang ở', tạo LuotLuuTru và ChangLuuTru mới", "Đạt"],
        ["TC08", "Lưu trú", "Đặt phòng trước không cọc", "P102, ngày 10-12/11, cọc 0đ", "Lưu thành công, hạn giữ chỗ tự động gán đúng 24 giờ", "Đạt"],
        ["TC09", "Lưu trú", "Thu cọc bổ sung cho lượt đặt", "Lượt đặt P102, nộp cọc 500.000đ", "Ghi nhận phiếu thu cọc, hạn giữ chỗ tự động mở rộng lên 15 ngày", "Đạt"],
        ["TC10", "Lưu trú", "Chống đặt trùng lịch phòng", "Đặt P101 trùng khoảng ngày đã có khách", "Báo lỗi TB03: Phòng đã có lịch đặt trong khoảng thời gian này", "Đạt"],
        ["TC11", "Lưu trú", "Hủy đặt phòng quá hạn & mất cọc", "Lượt đặt P103 quá hạn nhận phòng", "Tự động đánh dấu Hủy, ghi nhận giao dịch Forfeit (tịch thu cọc)", "Đạt"],
        ["TC12", "Lưu trú", "Đổi phòng giữa kỳ lưu trú", "Chuyển khách P101 sang P201 (VIP)", "Đóng chặng cũ P101, mở chặng mới P201, chia tiền phòng chính xác", "Đạt"],
        ["TC13", "Dịch vụ", "Gọi đồ uống minibar", "Phòng P101, 2 lon Coca, 1 gói snack", "Tạo YeuCauDichVu thành công ở trạng thái 'Đang chờ'", "Đạt"],
        ["TC14", "Dịch vụ", "Giao dịch vụ & trừ tồn kho", "Xác nhận giao đủ đồ cho P101", "Yêu cầu chuyển 'Đã giao', bảng HangTonKho tự động trừ số lượng", "Đạt"],
        ["TC15", "Hóa đơn", "Chặn checkout khi dịch vụ chờ", "P101 còn 1 dịch vụ 'Đang chờ'", "Báo lỗi TB05: Không thể trả phòng khi dịch vụ chưa giao hoặc hủy", "Đạt"],
        ["TC16", "Hóa đơn", "Tính tiền phòng block 24 giờ", "Khách ở 26 tiếng (vượt 2 tiếng)", "Hệ thống làm tròn lên 2 ngày phòng theo đúng quy tắc QT01", "Đạt"],
        ["TC17", "Hóa đơn", "Khấu trừ tiền cọc khi checkout", "Tiền bill 1.500.000đ, đã cọc 500.000đ", "Thực thu hiển thị đúng 1.000.000đ", "Đạt"],
        ["TC18", "Hóa đơn", "Tạo mã VietQR thanh toán", "Chọn chuyển khoản ngân hàng", "Sinh mã QR chuẩn NAPAS chứa đúng số tiền và cú pháp chuyển", "Đạt"],
        ["TC19", "Lưu trú", "Hoàn tất trả phòng & dọn phòng", "Xác nhận thanh toán xong P101", "Lưu HoaDon, ChiTietBill, phòng P101 tự chuyển sang 'Cần dọn'", "Đạt"],
        ["TC20", "Ca trực", "Mở ca với tiền mặt đầu ca", "Nhân viên B mở ca, tiền đầu ca 2 triệu", "Tạo bản ghi CaTruc mới, lưu tiền đầu ca an toàn", "Đạt"],
        ["TC21", "Ca trực", "Nộp ca & giải trình lệch quỹ", "Tiền sổ 5 triệu, tiền đếm 4.9 triệu", "Hệ thống bắt buộc nhập giải trình chênh lệch 100.000đ", "Đạt"],
        ["TC22", "Ca trực", "Khóa ca trực ngăn sửa hồi tố", "Kế toán duyệt khóa ca trực số 12", "Khóa thành công; trigger SQL chặn mọi thao tác sửa hóa đơn thuộc ca", "Đạt"],
        ["TC23", "Báo cáo", "Xuất báo cáo doanh thu ra CSV", "Chọn khoảng ngày, bấm Xuất CSV", "Tạo file CSV UTF-8 có BOM, mở Excel hiển thị tiếng Việt hoàn hảo", "Đạt"],
        ["TC24", "Tùy biến", "Thiết kế màn hình động mới", "Tạo form 'Khách VIP' gồm 3 trường", "Lưu cấu hình JSON vào CSDL, hiển thị form động nhập liệu chuẩn", "Đạt"],
        ["TC25", "Phân quyền", "Kiểm tra quyền chức năng", "Tài khoản Reception bấm xem Sổ quỹ", "Báo lỗi TB06: Bạn không có quyền thực hiện chức năng này", "Đạt"]
    ]
    create_styled_table(doc, hdr_tc, data_tc, col_widths=[0.6, 0.9, 1.8, 1.6, 2.0, 0.7], caption="Bảng 4.5. Bảng tổng hợp kịch bản kiểm thử hệ thống")

    # 4.5 Hướng dẫn sử dụng
    add_h2(doc, "4.5. Hướng dẫn sử dụng chương trình")
    add_p(doc, "Quy trình vận hành chuẩn dành cho nhân viên lễ tân:")
    add_bullet(doc, "Nhập tên tài khoản và mật khẩu được cấp. Tích chọn 'Ghi nhớ đăng nhập' nếu sử dụng trên máy tính cá nhân. Bấm 'Đăng nhập'.", "1. Đăng nhập hệ thống: ")
    add_bullet(doc, "Tại Menu tổng, bấm ô 'Ca trực'. Nhập số tiền mặt thực tế trong két tiền bàn giao từ ca trước. Bấm 'Mở ca'.", "2. Mở ca trực đầu ngày: ")
    add_bullet(doc, "Vào 'Quản lý phòng -> Đặt phòng -> Đặt phòng trước'. Chọn ngày đến và số đêm ở. Chọn phòng khả dụng từ danh sách, nhập thông tin khách hàng, nhập số tiền cọc (nếu có) và bấm 'Lưu đặt phòng'.", "3. Tiếp nhận đặt phòng: ")
    add_bullet(doc, "Khi khách đến, tìm lượt đặt trong danh sách, đối chiếu giấy tờ và bấm 'Nhận phòng'. Phòng trên sơ đồ sẽ chuyển sang màu đỏ ('Đang ở').", "4. Làm thủ tục Check-in: ")
    add_bullet(doc, "Bấm vào phòng khách đang ở trên sơ đồ, chọn 'Gọi dịch vụ'. Chọn mặt hàng và số lượng, bấm 'Thêm'. Khi giao hàng cho khách, bấm 'Xác nhận giao'.", "5. Thêm dịch vụ minibar: ")
    add_bullet(doc, "Bấm vào phòng, chọn 'Trả phòng'. Kiểm tra lại bảng tính tiền phòng và dịch vụ. Nếu khách chuyển khoản, chọn 'Chuyển khoản' để hiển thị mã VietQR cho khách quét. Bấm 'Xác nhận thanh toán & In hóa đơn'.", "6. Làm thủ tục Check-out: ")
    add_bullet(doc, "Cuối ca, vào 'Ca trực -> Bàn giao ca'. Đếm tiền mặt thực tế trong két, nhập vào ô 'Tiền thực tế'. Nếu có chênh lệch, ghi rõ lý do vào ô 'Giải trình'. Bấm 'Nộp ca & Đăng xuất'.", "7. Bàn giao ca cuối ngày: ")

    # 4.6 Mã nguồn tiêu biểu
    add_h2(doc, "4.6. Mã nguồn tiêu biểu")
    add_p(doc, "Dưới đây là một số đoạn mã nguồn tiêu biểu thể hiện trình độ kỹ thuật và kiến trúc chặt chẽ của dự án:")

    add_p(doc, "1. Lớp khởi tạo và kiểm tra CSDL tự động (SchemaMigrator.cs):", bold=True)
    add_code_block(doc,
"""public static async Task MigrateDatabaseAsync(string connectionString)
{
    using var conn = new SqlConnection(connectionString);
    await conn.OpenAsync();

    // Kiểm tra phiên bản CSDL hiện tại
    int currentVersion = await GetCurrentSchemaVersionAsync(conn);
    if (currentVersion < TargetVersion)
    {
        using var tran = conn.BeginTransaction();
        try
        {
            // Thực thi tuần tự các bước nâng cấp schema
            await ExecuteMigrationScriptsAsync(conn, tran, currentVersion, TargetVersion);
            await UpdateSchemaVersionAsync(conn, tran, TargetVersion);
            tran.Commit();
        }
        catch (Exception ex)
        {
            tran.Rollback();
            throw new InvalidOperationException($"Lỗi nâng cấp CSDL: {ex.Message}", ex);
        }
    }
}""")

    add_p(doc, "2. Thuật toán tạo mã VietQR động chuẩn NAPAS 247 (PaymentQr.cs):", bold=True)
    add_code_block(doc,
"""public static string GenerateVietQrUrl(string bankCode, string accountNo, string accountName, decimal amount, string memo)
{
    string encodedName = Uri.EscapeDataString(accountName.ToUpper());
    string encodedMemo = Uri.EscapeDataString(memo);
    long roundedAmount = (long)Math.Round(amount, 0);

    // Tạo URL ảnh mã QR động theo chuẩn dịch vụ VietQR
    return $"https://img.vietqr.io/image/{bankCode}-{accountNo}-compact2.png" +
           $"?amount={roundedAmount}&addInfo={encodedMemo}&accountName={encodedName}";
}""")

    # 4.7 Xử lý sự cố thường gặp
    add_h2(doc, "4.7. Xử lý sự cố thường gặp")
    add_p(doc, "Bảng tổng hợp các sự cố kỹ thuật thường gặp trong quá trình vận hành và biện pháp khắc phục nhanh:")
    add_bullet(doc, "Kiểm tra xem dịch vụ SQL Server (SQLEXPRESS hoặc MSSQLSERVER) trong Windows Services đã được bật chưa. Kiểm tra lại chuỗi kết nối trong tệp QLKhachSan/appsettings.json.", "• Lỗi không kết nối được CSDL (Cannot connect to SQL Server): ")
    add_bullet(doc, "Chương trình đã được cấu hình chế độ DPI Awareness (PerMonitorV2) trong tệp app.manifest, đảm bảo hiển thị sắc nét trên cả màn hình laptop phóng to 125% - 150%.", "• Giao diện bị mờ hoặc tràn chữ khi màn hình Windows scale 125% - 150%: ")
    add_bullet(doc, "Do hai lễ tân cùng thao tác trên một phòng tại cùng một tích tắc. Cơ chế Timeout 5000ms sẽ tự nhả khóa và yêu cầu người dùng thử lại thao tác.", "• Lỗi khóa ứng dụng (Lock Timeout Expired): ")

    doc.add_page_break()
