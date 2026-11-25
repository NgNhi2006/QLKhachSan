import docx
from docx.shared import Inches, Pt, RGBColor
from docx.enum.text import WD_ALIGN_PARAGRAPH
from .styles import (
    COLOR_PRIMARY, COLOR_SECONDARY, COLOR_TEXT,
    add_h1, add_h2, add_h3, add_p, add_bullet, add_callout,
    create_styled_table
)

def build_chapter1(doc):
    add_h1(doc, "CHƯƠNG 1. MÔ TẢ CHỨC NĂNG VÀ MÔ TẢ NGHIỆP VỤ")

    # 1.1 Giới thiệu đề tài
    add_h2(doc, "1.1. Giới thiệu đề tài")
    add_h3(doc, "1.1.1. Tên đề tài và mục tiêu xây dựng")
    add_p(doc, "Tên đề tài: Xây dựng phần mềm Quản lý Khách sạn (Hotel Management System).", bold=True)
    add_p(doc, "Mục tiêu tổng quát của đề tài là xây dựng một hệ thống phần mềm chuyên nghiệp, hoàn chỉnh trên nền tảng máy tính để bàn (Windows Desktop Application) nhằm tin học hóa và tự động hóa toàn diện các nghiệp vụ quản lý phòng, đặt phòng, tiếp nhận khách lưu trú, cung ứng dịch vụ, quản lý kho minibar, xuất hóa đơn thanh toán và quản trị ca trực tại các khách sạn, nhà nghỉ, resort quy mô vừa và nhỏ.")

    add_p(doc, "Các mục tiêu cụ thể bao gồm:")
    add_bullet(doc, "Xây dựng giao diện đồ họa người dùng theo phong cách phẳng hiện đại (Modern Fluent UI), trực quan hóa sơ đồ phòng theo màu sắc trạng thái thời gian thực, dễ sử dụng cho nhân viên lễ tân mà không đòi hỏi kỹ năng tin học phức tạp.", "• Về giao diện người dùng: ")
    add_bullet(doc, "Tách biệt hoàn toàn giữa trạng thái vật lý của phòng và lịch đặt phòng theo thời gian; hỗ trợ một phòng có thể nhận nhiều lượt đặt nếu các khoảng thời gian không giao nhau; tích hợp chính sách giữ chỗ 24h (chưa cọc) và 15 ngày (đã cọc); tự động phát hiện và hủy lượt đặt quá hạn.", "• Về quản lý đặt phòng và lưu trú: ")
    add_bullet(doc, "Hỗ trợ linh hoạt các phương thức tiền mặt, chuyển khoản ngân hàng và thẻ POS; tự động tạo mã thanh toán VietQR động chuẩn NAPAS 247 nhúng trực tiếp số tiền và mã hóa đơn, giúp khách hàng quét mã chuyển tiền chính xác tuyệt đối.", "• Về thanh toán và xuất hóa đơn: ")
    add_bullet(doc, "Quản lý dòng tiền mặt chặt chẽ thông qua quy trình mở ca với tiền đầu ca, kết ca với tiền thực đếm, ghi nhận giải trình chênh lệch và cơ chế khóa ca trực ngăn chặn sửa đổi hồi tố.", "• Về quản lý ca trực và sổ quỹ: ")
    add_bullet(doc, "Hệ thống tự động kiểm tra phiên bản CSDL và áp dụng các script nâng cấp (Migration) tự động ngay khi khởi động mà không làm mất mát bất kỳ dữ liệu lịch sử nào.", "• Về cơ sở dữ liệu và độ tin cậy: ")
    add_bullet(doc, "Bảo vệ tài khoản bằng thuật toán băm mật khẩu PBKDF2 SHA-512 kết hợp muối ngẫu nhiên (Salt); cơ chế tự động khóa tài khoản tạm thời khi đăng nhập sai quá số lần quy định; phân quyền chức năng chi tiết theo vai trò.", "• Về an toàn bảo mật: ")

    add_h3(doc, "1.1.2. Phạm vi và công nghệ sử dụng")
    add_p(doc, "Phần mềm được thiết kế chạy cục bộ hoặc trong mạng nội bộ (LAN) của khách sạn, hỗ trợ đồng thời nhiều máy trạm lễ tân và máy quản lý cùng kết nối về máy chủ cơ sở dữ liệu SQL Server tập trung. Toàn bộ các công nghệ và công cụ sử dụng trong quá trình phát triển được tổng hợp trong Bảng 1.1.")

    hdr_1_1 = ["Thành phần", "Công nghệ / Công cụ", "Mục đích sử dụng & Ưu điểm"]
    data_1_1 = [
        ["Ngôn ngữ lập trình", "C# (phiên bản C# 10 / C# 12)", "Ngôn ngữ hướng đối tượng hiện đại, mạnh mẽ, an toàn kiểu dữ liệu, hỗ trợ bất đồng bộ (async/await) toàn diện."],
        ["Nền tảng ứng dụng", ".NET 10 (Desktop Windows)", "Framework hiệu năng cao mới nhất của Microsoft, tối ưu tốc độ thực thi, khởi động tức thì và tiết kiệm bộ nhớ."],
        ["Giao diện người dùng", "Windows Forms (Thuần mã nguồn - Code-only)", "Toàn bộ giao diện được vẽ bằng mã C# hướng đối tượng kết hợp bộ điều khiển ModernControls tùy biến, không phụ thuộc vào Designer kéo thả, hiển thị mượt mà trên mọi tỷ lệ màn hình DPI."],
        ["Hệ quản trị CSDL", "Microsoft SQL Server 2019 / 2022 / Express", "Hệ quản trị CSDL quan hệ chuẩn doanh nghiệp, hỗ trợ đầy đủ tính chất ACID, khóa giao dịch sp_getapplock, chỉ mục và sao lưu định kỳ."],
        ["Truy cập dữ liệu", "Microsoft.Data.SqlClient (ADO.NET)", "Thư viện truy cập CSDL chính thức của Microsoft, sử dụng Parameterized SQL Queries chống triệt để tấn công SQL Injection."],
        ["Bảo mật mật khẩu", "PBKDF2 SHA-512 (Key Derivation)", "Tiêu chuẩn mật mã học hàng đầu, sử dụng muối ngẫu nhiên 16 bytes và 10.000 vòng lặp, ngăn chặn tấn công từ điển và Rainbow Table."],
        ["Tích hợp thanh toán", "VietQR Open API (Chuẩn NAPAS 247)", "Tạo mã QR thanh toán động chứa thông tin số tài khoản, tên chủ thẻ, số tiền và nội dung hóa đơn tự động."],
        ["Công cụ phát triển", "Visual Studio 2022 / VS Code / Git", "Môi trường phát triển tích hợp chuyên nghiệp, quản lý phiên bản mã nguồn phân tán."]
    ]
    create_styled_table(doc, hdr_1_1, data_1_1, col_widths=[1.5, 2.0, 3.2], caption="Bảng 1.1. Danh mục công nghệ và công cụ sử dụng trong dự án")

    # 1.2 Mô tả chức năng
    add_h2(doc, "1.2. Mô tả chức năng")
    add_h3(doc, "1.2.1. Danh mục các nhóm chức năng chính")
    add_p(doc, "Hệ thống quản lý khách sạn được chia thành 8 phân hệ chức năng nòng cốt. Bảng 1.2 liệt kê chi tiết các chức năng và ý nghĩa nghiệp vụ của từng chức năng.")

    hdr_1_2 = ["STT", "Nhóm chức năng", "Chức năng cụ thể", "Mô tả nghiệp vụ"]
    data_1_2 = [
        ["1", "Quản lý Tài khoản & Phân quyền", "Đăng nhập / Đăng xuất", "Xác thực danh tính bằng tên đăng nhập và mật khẩu băm; có tùy chọn ẩn/hiện mật khẩu và ghi nhớ đăng nhập an toàn bằng Windows DPAPI."],
        ["2", "Quản lý Tài khoản & Phân quyền", "Quản trị người dùng & Phân quyền", "Tạo tài khoản nhân viên, đặt lại mật khẩu, khóa/mở tài khoản, cấp quyền chi tiết từng chức năng cho 4 vai trò: Admin, Reception, Accountant, Manager."],
        ["3", "Sơ đồ phòng & Lưu trú", "Sơ đồ phòng trực quan", "Hiển thị trạng thái vật lý của tất cả phòng (Trống, Đang ở, Cần dọn, Bảo trì) bằng màu sắc; dấu hiệu nhận biết phòng có lịch đặt trước."],
        ["4", "Sơ đồ phòng & Lưu trú", "Nhận phòng trực tiếp (Walk-in)", "Chọn phòng trống trên sơ đồ, nhập thông tin khách hàng, số ngày dự kiến và tiến hành nhận phòng ngay lập tức."],
        ["5", "Sơ đồ phòng & Lưu trú", "Đặt phòng trước (Booking)", "Tra cứu phòng trống theo khoảng thời gian đến và số ngày ở; nhập thông tin khách; hỗ trợ đặt phòng lẻ và đặt phòng theo đoàn."],
        ["6", "Sơ đồ phòng & Lưu trú", "Thu cọc & Quản lý giữ chỗ", "Ghi nhận tiền cọc khách gửi (nhiều đợt); mở rộng hạn giữ chỗ lên 15 ngày; tự động xử lý hủy lượt và mất cọc (Forfeit) khi quá hạn."],
        ["7", "Sơ đồ phòng & Lưu trú", "Đổi phòng & Gia hạn lưu trú", "Chuyển khách sang phòng khác cùng hoặc khác hạng phòng; hệ thống tự chia tiền phòng theo giá từng chặng; gia hạn thêm ngày ở."],
        ["8", "Dịch vụ & Minibar", "Gọi dịch vụ buồng phòng", "Thêm yêu cầu dịch vụ ăn uống, giặt ủi, nước uống minibar cho phòng đang ở; ghi nhận số lượng, đơn giá và người tạo yêu cầu."],
        ["9", "Dịch vụ & Minibar", "Giao dịch vụ & Xuất kho", "Xác nhận giao từng phần hoặc giao đủ dịch vụ; tự động sinh phiếu xuất kho trừ tồn kho hàng hóa tương ứng."],
        ["10", "Dịch vụ & Minibar", "Quản lý danh mục & Tồn kho", "Quản lý danh mục mặt hàng, đơn vị tính, đơn giá bán, mức tồn kho an toàn và lịch sử các lần biến động kho."],
        ["11", "Thanh toán & Hóa đơn", "Tính tiền & Trả phòng", "Tính toán tự động tiền phòng theo block 24 giờ, cộng tiền dịch vụ đã giao, trừ các khoản tiền cọc đã thu; xuất hóa đơn thanh toán."],
        ["12", "Thanh toán & Hóa đơn", "Tạo mã VietQR thanh toán", "Tự động sinh mã VietQR chuyển khoản chứa đầy đủ số tiền và cú pháp chuyển khoản giúp khách quét thanh toán tức thì."],
        ["13", "Kế toán & Ca trực", "Quản lý ca trực thu ngân", "Mở ca với tiền mặt đầu ca; nộp tiền cuối ca với biên bản giải trình chênh lệch; khóa ca trực không cho chỉnh sửa."],
        ["14", "Kế toán & Ca trực", "Đối soát & Báo cáo sổ quỹ", "Theo dõi dòng tiền thu chi theo từng phương thức (Tiền mặt, Ngân hàng, Thẻ POS); hỗ trợ nhập sao kê đối soát."],
        ["15", "Báo cáo & Thống kê", "Báo cáo doanh thu & Lấp đầy", "Thống kê doanh thu theo ngày/tháng/khoảng thời gian; tỷ lệ lấp đầy phòng (Occupancy Rate); xuất file CSV UTF-8."],
        ["16", "Tùy biến hệ thống", "Thiết kế màn hình động", "Cho phép Admin tự tạo thêm các trường nhập liệu và màn hình nghiệp vụ phụ (Custom Screen Designer) lưu trữ động trong CSDL."]
    ]
    create_styled_table(doc, hdr_1_2, data_1_2, col_widths=[0.5, 1.8, 1.8, 2.6], caption="Bảng 1.2. Danh mục các nhóm chức năng chính của hệ thống")

    add_h3(doc, "1.2.2. Yêu cầu phi chức năng")
    add_bullet(doc, "Hệ thống tự động kiểm tra phiên bản CSDL khi khởi động thông qua lớp SchemaMigrator. Nếu phát hiện CSDL cũ hoặc thiếu bảng, hệ thống tự động chạy các script nâng cấp theo thứ tự mà không cần người dùng thao tác lệnh SQL thủ công, đảm bảo chạy lại nhiều lần không lỗi và không trùng lặp dữ liệu.", "• Tính tin cậy và tự phục hồi: ")
    add_bullet(doc, "Mọi thao tác ghi dữ liệu trọng yếu (đặt phòng, đổi phòng, hủy cọc, thanh toán hóa đơn) đều được thực thi trong một Database Transaction với mức cô lập phù hợp và cơ chế khóa tài nguyên sp_getapplock, cam kết tính toàn vẹn tuyệt đối, tự động Rollback toàn phần nếu có lỗi xảy ra.", "• Tính toàn vẹn giao dịch (ACID): ")
    add_bullet(doc, "Tốc độ phản hồi giao diện tức thì (dưới 500ms cho các thao tác tải danh sách phòng, nạp dịch vụ, tính bill); các truy vấn CSDL được tối ưu hóa bằng chỉ mục (Index) trên các cột thường xuyên tìm kiếm và lọc.", "• Hiệu năng xử lý: ")
    add_bullet(doc, "Mật khẩu nhân viên được mã hóa một chiều an toàn bằng thuật toán PBKDF2 SHA-512 với Salt; cơ chế SecurityVersion làm vô hiệu hóa tức thì các phiên đăng nhập cũ khi tài khoản bị khóa hoặc đổi quyền; ghi nhận đầy đủ nhật ký thao tác (Audit Log) lưu lại ai, làm gì, lúc nào.", "• An toàn thông tin: ")
    add_bullet(doc, "Giao diện hỗ trợ tự động co giãn theo tỷ lệ DPI màn hình (100%, 125%, 150%) trên Windows 10/11; hỗ trợ chuyển đổi giao diện sáng/tối (Dark/Light mode) và các bảng màu chuyên nghiệp giúp nhân viên làm việc trong thời gian dài không mỏi mắt.", "• Khả năng thích ứng giao diện: ")

    # 1.3 Mô tả nghiệp vụ
    add_h2(doc, "1.3. Mô tả nghiệp vụ")
    add_h3(doc, "1.3.1. Đối tượng sử dụng và vai trò")
    add_p(doc, "Phần mềm phân định rõ quyền hạn của 4 nhóm đối tượng người dùng trong khách sạn:")
    add_bullet(doc, "Có toàn quyền tối cao trong hệ thống: quản trị tài khoản nhân viên, cấp phát và thu hồi quyền chức năng, xem nhật ký thao tác hệ thống, cấu hình tham số hệ thống, thiết kế màn hình động và can thiệp điều chỉnh các sai sót nghiệp vụ.", "1. Quản trị viên hệ thống (Admin): ")
    add_bullet(doc, "Thực hiện các thao tác vận hành hàng ngày: kiểm tra sơ đồ phòng, tiếp nhận khách vãng lai, đặt phòng trước, thu cọc, làm thủ tục check-in, đổi phòng, gọi dịch vụ minibar, làm thủ tục check-out, xuất hóa đơn và quản lý ca trực cá nhân.", "2. Nhân viên Lễ tân (Receptionist): ")
    add_bullet(doc, "Theo dõi toàn bộ số liệu doanh thu, kiểm tra sổ quỹ tiền mặt, xem danh sách hóa đơn, duyệt các khoản giảm trừ/chiết khấu, kiểm tra sao kê ngân hàng và thực hiện khóa ca thu ngân / khóa kỳ kế toán.", "3. Kế toán khách sạn (Accountant): ")
    add_bullet(doc, "Theo dõi tổng thể tình hình vận hành, xem báo cáo doanh thu, tỷ lệ lấp đầy phòng, quản lý danh mục phòng, thiết lập bảng giá phòng và điều hành phân công bảo trì sửa chữa.", "4. Quản lý khách sạn (Manager): ")

    add_h3(doc, "1.3.2. Quy trình nghiệp vụ tổng quát")
    add_p(doc, "Quy trình nghiệp vụ tổng thể của một khách lưu trú từ khi liên hệ đến khi kết thúc kỳ nghỉ trải qua 6 bước khép kín:")
    add_bullet(doc, "Khách hàng liên hệ đặt phòng qua điện thoại hoặc trực tiếp. Lễ tân mở màn hình tra cứu phòng trống theo ngày đến và số ngày lưu trú. Hệ thống lọc danh sách phòng thỏa mãn, lễ tân nhập thông tin khách (Họ tên, SĐT, CCCD/Passport) và tạo bản ghi giữ chỗ.", "Bước 1 - Đặt phòng & Thu cọc: ")
    add_bullet(doc, "Sau khi tạo đặt phòng, nếu khách chưa nộp cọc, hệ thống giữ phòng tối đa 24 giờ. Khi khách chuyển khoản hoặc nộp tiền mặt, lễ tân tạo phiếu thu cọc, hệ thống tự động nâng hạn giữ chỗ lên tối đa 15 ngày.", "Bước 2 - Quản lý hạn giữ chỗ: ")
    add_bullet(doc, "Khách đến khách sạn trong thời hạn quy định. Lễ tân đối chiếu giấy tờ tùy thân, bấm nút 'Nhận phòng'. Trạng thái phòng chuyển sang 'Đang ở' (Occupied), lượt đặt chuyển sang trạng thái có hiệu lực và kích hoạt chặng lưu trú đầu tiên.", "Bước 3 - Nhận phòng (Check-in): ")
    add_bullet(doc, "Trong quá trình lưu trú, nếu khách sử dụng đồ uống minibar hoặc gọi dịch vụ giặt ủi, lễ tân ghi nhận yêu cầu vào hệ thống. Khi dịch vụ được bàn giao, hệ thống ghi nhận xuất kho và tính tiền vào tài khoản phòng của khách.", "Bước 4 - Sử dụng dịch vụ phát sinh: ")
    add_bullet(doc, "Khách yêu cầu trả phòng. Lễ tân bấm 'Trả phòng', hệ thống mở bảng tính tiền chi tiết: tự động tính tiền phòng theo số ngày ở thực tế, cộng toàn bộ tiền dịch vụ đã giao, trừ đi tiền cọc đã nộp. Lễ tân chọn phương thức thanh toán (Tiền mặt hoặc Quét mã VietQR) và in hóa đơn cho khách.", "Bước 5 - Trả phòng (Check-out) & Thanh toán: ")
    add_bullet(doc, "Sau khi khách rời đi, trạng thái phòng tự động chuyển sang 'Cần dọn' (Dirty). Bộ phận buồng phòng tiến hành vệ sinh, thay ga gối. Sau khi dọn sạch, lễ tân bấm 'Đã dọn xong', phòng quay lại trạng thái 'Trống' sẵn sàng đón khách tiếp theo.", "Bước 6 - Dọn phòng & Khôi phục trạng thái: ")

    add_h3(doc, "1.3.3. Các trạng thái của phòng và lượt lưu trú")
    add_p(doc, "Hệ thống phân tách rành mạch giữa trạng thái vật lý của phòng và trạng thái nghiệp vụ của lượt lưu trú, được mô tả trong Bảng 1.3.")

    hdr_1_3 = ["Đối tượng", "Trạng thái", "Ý nghĩa nghiệp vụ", "Màu sắc hiển thị trên UI"]
    data_1_3 = [
        ["Phòng vật lý", "Trống (Clean)", "Phòng đã dọn dẹp sạch sẽ, sẵn sàng đón khách vào ở ngay.", "Xanh lá cây (#2E7D32)"],
        ["Phòng vật lý", "Đang ở (Occupied)", "Khách đang lưu trú tại phòng.", "Đỏ đậm (#C62828)"],
        ["Phòng vật lý", "Cần dọn (Dirty)", "Khách vừa trả phòng, phòng cần nhân viên dọn vệ sinh.", "Cam hổ phách (#EF6C00)"],
        ["Phòng vật lý", "Bảo trì (Maintenance)", "Phòng đang hỏng hóc hoặc sửa chữa thiết bị, không nhận khách.", "Xám ghi (#546E7A)"],
        ["Lượt lưu trú", "Chờ nhận (Reserved)", "Khách đã đặt phòng trước, đang trong thời hạn giữ chỗ.", "Xanh dương (#1565C0)"],
        ["Lượt lưu trú", "Đang ở (Active)", "Khách đã làm thủ tục nhận phòng và đang lưu trú.", "Đỏ (#D32F2F)"],
        ["Lượt lưu trú", "Hoàn tất (Completed)", "Khách đã thanh toán hóa đơn và hoàn tất trả phòng.", "Xanh lục (#388E3C)"],
        ["Lượt lưu trú", "Đã hủy (Cancelled)", "Lượt đặt bị hủy trước hạn; hệ thống hoàn trả toàn bộ cọc.", "Tím nhạt (#7B1FA2)"],
        ["Lượt lưu trú", "Mất cọc (Forfeit)", "Lượt đặt quá hạn nhận mà khách không đến (No-Show); cọc bị tịch thu.", "Đỏ thẫm (#B71C1C)"]
    ]
    create_styled_table(doc, hdr_1_3, data_1_3, col_widths=[1.4, 1.6, 2.7, 1.5], caption="Bảng 1.3. Bảng trạng thái buồng phòng và lượt lưu trú")

    add_h3(doc, "1.3.4. Quy tắc nghiệp vụ cốt lõi")
    add_p(doc, "Để đảm bảo tính nhất quán và chặt chẽ của hệ thống, các quy tắc nghiệp vụ cốt lõi (Bảng 1.4) được cài đặt trực tiếp tại tầng xử lý nghiệp vụ BLL và các ràng buộc cơ sở dữ liệu:")

    hdr_1_4 = ["Mã quy tắc", "Tên quy tắc", "Nội dung quy tắc nghiệp vụ chi tiết"]
    data_1_4 = [
        ["QT01", "Quy tắc tính tiền phòng (Block 24 giờ)", "Thời gian lưu trú tối thiểu tính là 1 ngày. Tổng số giờ lưu trú được làm tròn lên ngày 24 giờ tiếp theo. Nếu khách đổi phòng, thời gian thực tế ở phòng cũ tính theo đơn giá phòng cũ; khoảng thời gian còn thiếu tính theo giá phòng mới."],
        ["QT02", "Chính sách thời hạn giữ chỗ đặt phòng", "Thời lượng lưu trú đặt trước từ 1 đến 60 ngày. Lượt đặt chưa thu cọc chỉ được giữ tối đa 24 giờ kể từ thời điểm tạo. Khi phát sinh giao dịch thu cọc, hạn giữ chỗ tối đa được tự động mở rộng lên 15 ngày, nhưng hạn nhận phòng không được vượt quá ngày trả phòng."],
        ["QT03", "Chính sách xử lý quá hạn & Tịch thu cọc", "Tại thời điểm chính xác hoặc sau hạn nhận phòng mà khách chưa check-in, hệ thống tự động đánh dấu hủy lượt đặt. Tiền cọc đã thu sẽ bị tịch thu toàn bộ (Forfeit - No Show) để bồi thường thiệt hại giữ phòng; hệ thống từ chối mọi yêu cầu hoàn cọc."],
        ["QT04", "Quy tắc phòng chống xung đột lịch (No Overlap)", "Một phòng vật lý được phép có nhiều lượt đặt trước với điều kiện các khoảng thời gian [Ngày đến, Ngày đi) hoàn toàn không giao nhau. Khi làm thủ tục nhận phòng thực tế, bắt buộc phòng phải ở trạng thái Trống và đã dọn xong."],
        ["QT05", "Quy tắc giao dịch vụ trước khi Checkout", "Hệ thống chỉ cho phép thực hiện thủ tục Check-out khi tất cả các yêu cầu dịch vụ buồng phòng đã được xác nhận giao đủ hoặc hủy bỏ. Tuyệt đối không cho phép thanh toán khi còn dịch vụ đang chờ xử lý."],
        ["QT06", "Quy tắc bảo vệ sổ ca & Chống sửa hồi tố", "Mọi giao dịch thanh toán phải gắn liền với một Ca trực đang mở. Khi ca trực đã được bàn giao và khóa, trigger trong cơ sở dữ liệu sẽ chặn mọi thao tác thêm, sửa, xóa tác động vào các chứng từ thuộc ca đó."]
    ]
    create_styled_table(doc, hdr_1_4, data_1_4, col_widths=[1.0, 1.8, 4.4], caption="Bảng 1.4. Danh mục các quy tắc nghiệp vụ cốt lõi của hệ thống")

    # 1.4 Khảo sát hiện trạng và yêu cầu người dùng
    add_h2(doc, "1.4. Khảo sát hiện trạng và yêu cầu người dùng")
    add_h3(doc, "1.4.1. Hiện trạng quản lý tại các khách sạn vừa và nhỏ")
    add_p(doc, "Qua khảo sát thực tế tại các khách sạn quy mô từ 15 đến 60 phòng trên địa bàn, hầu hết các đơn vị vẫn áp dụng mô hình quản lý thủ công kết hợp sổ sách Excel rời rạc. Một số bất cập điển hình ghi nhận được bao gồm:")
    add_bullet(doc, "Sử dụng sổ tay lễ tân hoặc bảng Excel độc lập trên từng máy tính, dẫn đến tình trạng hai lễ tân khác ca cùng nhận cọc cho một phòng vào cùng một thời điểm.", "• Xung đột dữ liệu đặt phòng: ")
    add_bullet(doc, "Công thức tính tiền phòng theo giờ, theo đêm hoặc đổi phòng giữa kỳ nghỉ rất phức tạp, dễ gây sai sót tính nhầm tiền của khách dẫn đến tranh cãi.", "• Sai sót trong thanh toán: ")
    add_bullet(doc, "Đồ uống trong minibar thường xuyên bị thất thoát do không có phiếu xuất kho liên kết tự động với yêu cầu buồng phòng.", "• Mất kiểm soát kho minibar: ")
    add_bullet(doc, "Cuối ca trực, lễ tân mất từ 30 đến 45 phút đếm tiền, cộng sổ thủ công, chênh lệch quỹ không giải trình được rõ ràng.", "• Quá trình bàn giao ca chậm chạp: ")

    add_h3(doc, "1.4.2. Giải pháp phần mềm đề xuất")
    add_p(doc, "Giải pháp được xây dựng nhằm giải quyết triệt để các bài toán thực tế nêu trên bằng một hệ thống tập trung:")
    add_bullet(doc, "Toàn bộ máy trạm lễ tân và quản lý cùng kết nối về cơ sở dữ liệu SQL Server trung tâm, đảm bảo dữ liệu phòng và lịch đặt được đồng bộ hóa tức thời.", "• Đồng bộ hóa tập trung: ")
    add_bullet(doc, "Mọi phép tính tiền phòng, tiền dịch vụ, chiết khấu và cọc đều do máy tính thực hiện theo thuật toán chuẩn xác đến từng đồng.", "• Tự động hóa tính toán: ")
    add_bullet(doc, "Giao diện sơ đồ phòng trực quan sinh động giúp nhân viên nhìn vào là nhận diện được ngay tình trạng khách sạn hiện tại.", "• Trực quan hóa vận hành: ")
    add_bullet(doc, "Tích hợp công nghệ thanh toán QR code chuẩn quốc gia và cơ chế khóa ca trực an toàn.", "• Hiện đại hóa thanh toán: ")

    # 1.5 Mô tả dữ liệu vào/ra
    add_h2(doc, "1.5. Mô tả dữ liệu vào/ra của các biểu mẫu chính")
    add_p(doc, "Bảng 1.5 đến Bảng 1.8 mô tả đặc tả kỹ thuật chi tiết về kiểu điều khiển, độ dài, tính bắt buộc và ràng buộc kiểm tra của từng trường nhập liệu trên các biểu mẫu chính.")

    hdr_io = ["Tên trường", "Tên hiển thị", "Kiểu điều khiển", "Kiểu dữ liệu", "Bắt buộc", "Quy tắc kiểm tra hợp lệ"]
    data_login = [
        ["TenDangNhap", "Tên đăng nhập", "TextBox", "VARCHAR(50)", "Có", "4-30 ký tự, chỉ chứa chữ cái, số, dấu chấm hoặc gạch dưới."],
        ["MatKhau", "Mật khẩu", "PasswordBox", "NVARCHAR(100)", "Có", "Tối thiểu 6 ký tự. Kiểm tra khớp với bản băm PBKDF2."],
        ["GhiNho", "Ghi nhớ đăng nhập", "CheckBox", "BOOLEAN", "Không", "Mặc định false; nếu chọn sẽ lưu an toàn qua Windows DPAPI."]
    ]
    create_styled_table(doc, hdr_io, data_login, col_widths=[1.2, 1.4, 1.2, 1.1, 0.8, 1.5], caption="Bảng 1.5. Đặc tả dữ liệu vào/ra - Biểu mẫu Đăng nhập")

    data_booking = [
        ["MaPhong", "Phòng chọn", "ComboBox", "INT", "Có", "Phải chọn phòng khả dụng trong khoảng thời gian lưu trú."],
        ["TenKhach", "Họ tên khách", "TextBox", "NVARCHAR(100)", "Có", "Tối đa 100 ký tự, không được để trống."],
        ["SoDienThoai", "Số điện thoại", "TextBox", "VARCHAR(20)", "Có", "Từ 9-11 chữ số, đúng định dạng số điện thoại Việt Nam."],
        ["SoGiayTo", "CCCD / Hộ chiếu", "TextBox", "VARCHAR(30)", "Không", "Định dạng 9 hoặc 12 số CCCD hoặc số hộ chiếu hợp lệ."],
        ["NgayBatDau", "Ngày nhận phòng", "DateTimePicker", "DATETIME2", "Có", "Phải từ thời điểm hiện tại trở đi."],
        ["NgayKetThuc", "Ngày trả phòng", "DateTimePicker", "DATETIME2", "Có", "Phải sau Ngày nhận phòng ít nhất 1 ngày."],
        ["TienCoc", "Tiền cọc nộp", "NumericTextBox", "DECIMAL(18,2)", "Không", "Số tiền >= 0. Nếu nộp cọc thì hạn giữ chỗ chuyển thành 15 ngày."]
    ]
    create_styled_table(doc, hdr_io, data_booking, col_widths=[1.2, 1.4, 1.2, 1.1, 0.8, 1.5], caption="Bảng 1.6. Đặc tả dữ liệu vào/ra - Biểu mẫu Đặt phòng")

    data_checkout = [
        ["MaLuotLuuTru", "Mã lượt ở", "Label (ReadOnly)", "INT", "Có", "Mã định danh duy nhất của lượt khách đang ở."],
        ["TongTienPhong", "Tiền phòng", "Label (ReadOnly)", "DECIMAL(18,2)", "Có", "Hệ thống tự tính theo số chặng và block 24h."],
        ["TongTienDichVu", "Tiền dịch vụ", "Label (ReadOnly)", "DECIMAL(18,2)", "Có", "Tổng tiền của các yêu cầu dịch vụ đã giao."],
        ["TienCocDaThu", "Đã thu cọc", "Label (ReadOnly)", "DECIMAL(18,2)", "Có", "Tổng các giao dịch cọc khách đã thanh toán."],
        ["PhuongThuc", "Phương thức", "ComboBox", "VARCHAR(20)", "Có", "Một trong ba giá trị: 'Cash', 'BankTransfer', 'CreditCard'."],
        ["TienKhachDua", "Khách trả", "NumericTextBox", "DECIMAL(18,2)", "Có", "Phải >= Số tiền thực thu cần thanh toán."]
    ]
    create_styled_table(doc, hdr_io, data_checkout, col_widths=[1.2, 1.4, 1.2, 1.1, 0.8, 1.5], caption="Bảng 1.7. Đặc tả dữ liệu vào/ra - Biểu mẫu Trả phòng (Check-out)")

    # 1.6 Danh mục thông báo
    add_h2(doc, "1.6. Danh mục thông báo và cảnh báo của hệ thống")
    add_p(doc, "Hệ thống giao tiếp với người dùng hoàn toàn bằng tiếng Việt có dấu thông qua các hộp thoại thông báo chuyên nghiệp. Bảng 1.12 tổng hợp các thông báo và cảnh báo thường gặp nhất.")

    hdr_msg = ["Mã TB", "Loại thông báo", "Nội dung thông báo hiển thị", "Nguyên nhân & Hướng xử lý"]
    data_msg = [
        ["TB01", "Lỗi xác thực", "Tên đăng nhập hoặc mật khẩu không chính xác.", "Người dùng nhập sai thông tin. Kiểm tra lại bàn phím và CapsLock."],
        ["TB02", "Cảnh báo bảo mật", "Tài khoản của bạn đã bị khóa tạm thời 5 phút do nhập sai mật khẩu quá 5 lần liên tiếp.", "Cơ chế bảo vệ chống tấn công dò mật khẩu tự động kích hoạt."],
        ["TB03", "Cảnh báo nghiệp vụ", "Phòng {Số phòng} đã có khách đặt trong khoảng thời gian từ {Ngày đến} đến {Ngày đi}. Vui lòng chọn phòng khác!", "Xung đột lịch phòng. Chọn phòng khác hoặc điều chỉnh thời gian đặt."],
        ["TB04", "Cảnh báo nghiệp vụ", "Lượt đặt phòng này đã quá hạn nhận phòng ({Hạn nhận}). Hệ thống đã tự động hủy lượt đặt!", "Khách không đến nhận phòng đúng hẹn. Tiền cọc bị tịch thu (Forfeit)."],
        ["TB05", "Ràng buộc kiểm tra", "Không thể thanh toán trả phòng khi vẫn còn dịch vụ ở trạng thái 'Đang chờ'. Vui lòng giao hoặc hủy dịch vụ trước!", "Bảo đảm toàn bộ dịch vụ phát sinh được ghi nhận đầy đủ trước khi chốt hóa đơn."],
        ["TB06", "Cảnh báo phân quyền", "Bạn không có quyền thực hiện chức năng này. Vui lòng liên hệ Quản trị viên để được cấp quyền!", "Tài khoản không sở hữu quyền tương ứng trong phân quyền FunctionPolicy."],
        ["TB07", "Thông báo thành công", "Lưu đặt phòng thành công cho khách hàng {Tên khách}. Hạn giữ chỗ: {Thời hạn}!", "Giao dịch tạo lượt lưu trú và cọc đã được commit an toàn vào CSDL."],
        ["TB08", "Thông báo thành công", "Thanh toán hóa đơn phòng {Số phòng} thành công. Đã in hóa đơn và chuyển trạng thái phòng sang 'Cần dọn'!", "Hoàn tất chu trình check-out và kích hoạt quy trình dọn phòng."]
    ]
    create_styled_table(doc, hdr_msg, data_msg, col_widths=[0.8, 1.4, 2.8, 2.2], caption="Bảng 1.12. Danh mục thông báo và cảnh báo của hệ thống")

    doc.add_page_break()
