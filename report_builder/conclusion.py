import docx
from docx.shared import Inches, Pt, RGBColor
from docx.enum.text import WD_ALIGN_PARAGRAPH
from .styles import (
    COLOR_PRIMARY, COLOR_SECONDARY, COLOR_TEXT,
    add_h1, add_h2, add_p, add_bullet, create_styled_table
)

def build_conclusion(doc):
    add_h1(doc, "KẾT LUẬN VÀ HƯỚNG PHÁT TRIỂN")

    # 1. Kết quả đạt được
    add_h2(doc, "1. Kết quả đạt được")
    add_p(doc, "Sau một thời gian nghiêm túc nghiên cứu, phân tích nghiệp vụ và lập trình triển khai thực tế, đề tài “Xây dựng phần mềm Quản lý Khách sạn (Hotel Management System)” đã hoàn thành xuất sắc các mục tiêu đề ra ban đầu, đạt được nhiều kết quả quan trọng cả về mặt lý thuyết lẫn ứng dụng thực tiễn:")

    add_bullet(doc, "Nắm vững và vận dụng thành thạo mô hình kiến trúc phân tầng 3-Tier (GUI – BLL – DAL – DTO), mẫu thiết kế Repository Pattern và Transaction Script trên nền tảng Microsoft .NET 10 mới nhất với ngôn ngữ C# hiện đại.", "• Về mặt học thuật và kiến trúc phần mềm: ")
    add_bullet(doc, "Thiết kế thành công hệ thống CSDL quan hệ chuẩn hóa 3NF gồm 20 bảng ứng dụng cốt lõi, thiết lập đầy đủ các ràng buộc toàn vẹn, khóa ngoại và chỉ mục tối ưu hóa truy vấn; đặc biệt là cơ chế tự động nâng cấp CSDL SchemaMigrator an toàn, không làm mất mát dữ liệu lịch sử.", "• Về mặt cơ sở dữ liệu: ")
    add_bullet(doc, "Giải quyết trọn vẹn và chuẩn xác các bài toán nghiệp vụ khách sạn thực tế: quản lý sơ đồ phòng trực quan, tách bạch trạng thái vật lý và lịch đặt theo thời gian, chống xung đột lịch phòng (Overbooking), tính tiền phòng theo block 24h, đổi phòng linh hoạt, quản lý gọi/giao dịch vụ minibar, kiểm soát xuất nhập tồn kho, quản lý ca trực thu ngân và đối soát dòng tiền.", "• Về mặt nghiệp vụ lưu trú: ")
    add_bullet(doc, "Xây dựng giao diện đồ họa thuần mã nguồn (Code-only) theo phong cách phẳng hiện đại (Modern Fluent UI), hỗ trợ co giãn đa độ phân giải, chống mờ trên màn hình High-DPI; tích hợp công nghệ thanh toán quét mã VietQR động chuẩn NAPAS 247 và module độc đáo Trình thiết kế màn hình động (Custom Screen Designer).", "• Về mặt giao diện và tính năng nâng cao: ")
    add_bullet(doc, "Ứng dụng đã vượt qua toàn bộ 35 kịch bản kiểm thử (Test Cases) từ cơ bản đến nâng cao, đảm bảo hoạt động ổn định, mượt mà và không phát sinh lỗi ngoại lệ chưa được kiểm soát.", "• Về kiểm thử và độ ổn định: ")

    # 2. Hạn chế
    add_h2(doc, "2. Hạn chế")
    add_p(doc, "Bên cạnh những kết quả nổi bật đã đạt được, hệ thống vẫn còn tồn tại một số hạn chế nhất định do giới hạn về mặt thời gian và nguồn lực thử nghiệm thực tế:")
    add_bullet(doc, "Hiện tại ứng dụng hoạt động tối ưu trong môi trường mạng nội bộ (Local Area Network - LAN) của khách sạn; chưa xây dựng phiên bản ứng dụng di động (Mobile App) trên iOS/Android dành riêng cho nhân viên buồng phòng hoặc chủ khách sạn theo dõi từ xa.", "• Nền tảng triển khai: ")
    add_bullet(doc, "Chưa tích hợp trực tiếp giao diện lập trình ứng dụng (API) đồng bộ hai chiều với các sàn thương mại điện tử du lịch trực tuyến (OTA - Online Travel Agencies) phổ biến như Agoda, Booking.com, Traveloka; nhân viên lễ tân vẫn phải nhập thông tin đặt phòng từ các kênh này vào hệ thống thủ công.", "• Kênh phân phối trực tuyến: ")
    add_bullet(doc, "Chưa kết nối trực tiếp với phần cứng khóa thẻ từ (RFID Key Card) của các hãng phổ biến (như Adel, Hune, Orbita) để tự động ghi thẻ phòng khi khách check-in và xóa thẻ khi khách check-out.", "• Tích hợp thiết bị ngoại vi: ")

    # 3. Hướng phát triển
    add_h2(doc, "3. Hướng phát triển")
    add_p(doc, "Để phần mềm ngày càng hoàn thiện, tiến tới thương mại hóa và ứng dụng rộng rãi tại các chuỗi khách sạn chuyên nghiệp, nhóm đề xuất các hướng phát triển trong tương lai như sau:")
    add_bullet(doc, "Xây dựng thêm phiên bản Web (ASP.NET Core Web API / React) và ứng dụng di động (Flutter / .NET MAUI) cho phép nhân viên buồng phòng cập nhật trạng thái dọn dẹp tức thì qua điện thoại; chủ khách sạn xem báo cáo doanh thu thời gian thực mọi lúc mọi nơi.", "1. Mở rộng đa nền tảng (Web & Mobile): ")
    add_bullet(doc, "Phát triển Channel Manager Module kết nối với các kênh bán phòng trực tuyến OTA, tự động đóng/mở phòng và cập nhật giá bán tức thời khi có khách đặt phòng trên mạng.", "2. Tích hợp cổng kết nối OTA tự động: ")
    add_bullet(doc, "Tích hợp SDK điều khiển đầu đọc/ghi thẻ từ thông minh, hỗ trợ in hóa đơn nhiệt tự động qua cổng USB/LAN và tích hợp hệ thống phát hành Hóa đơn điện tử (e-Invoice) theo Thông tư 78 của Tổng cục Thuế.", "3. Tích hợp phần cứng thông minh & Hóa đơn điện tử: ")
    add_bullet(doc, "Ứng dụng các thuật toán Trí tuệ nhân tạo (AI) và Học máy (Machine Learning) để phân tích dữ liệu lịch sử đặt phòng, dự báo xu hướng mùa cao điểm và đề xuất chiến lược giá phòng linh hoạt (Dynamic Pricing) nhằm tối đa hóa doanh thu cho khách sạn.", "4. Ứng dụng Trí tuệ nhân tạo (AI): ")

    doc.add_page_break()

    # Tài liệu tham khảo
    add_h1(doc, "TÀI LIỆU THAM KHẢO")
    add_p(doc, "Các tài liệu giáo trình, sách chuyên khảo và tài liệu kỹ thuật được tham khảo trong quá trình thực hiện đề tài bao gồm:")

    refs = [
        "[1] Bộ môn Công nghệ Phần mềm - Trường Cao đẳng Công nghệ Việt - Hàn, Giáo trình Lập trình Windows với C# .NET, NXB Thông tin và Truyền thông, 2024.",
        "[2] Phạm Hữu Khang, Lập trình ứng dụng Windows Forms và CSDL SQL Server với C#, NXB Lao động - Xã hội, 2022.",
        "[3] Microsoft Corporation, .NET 10 Documentation & C# 12 Language Reference, https://learn.microsoft.com/en-us/dotnet/, 2026.",
        "[4] Microsoft Corporation, Microsoft.Data.SqlClient Documentation and Best Practices, https://learn.microsoft.com/en-us/sql/connect/ado-net/, 2025.",
        "[5] Martin Fowler, Patterns of Enterprise Application Architecture (Repository Pattern & Transaction Script), Addison-Wesley Professional, 2018.",
        "[6] Robert C. Martin, Clean Architecture: A Craftsman's Guide to Software Structure and Design, Prentice Hall, 2017.",
        "[7] Cổng thanh toán quốc gia NAPAS, Hướng dẫn tích hợp chuẩn mã QR thanh toán quốc gia VietQR, https://vietqr.io/, 2025.",
        "[8] NIST (National Institute of Standards and Technology), Recommendation for Password-Based Key Derivation (PBKDF2 - SP 800-132), U.S. Department of Commerce."
    ]
    for r in refs:
        add_p(doc, r, space_after=4)
