import os
import docx
from docx.shared import Inches, Pt, RGBColor
from generate_qlks_helpers import (
    add_h1, add_h2, add_h3, add_p, add_bullet, add_styled_table, add_image_box,
    COLOR_BLACK, COLOR_DARK, COLOR_MUTED
)

def build_chapter4(doc):
    add_h1(doc, "CHƯƠNG 4: MÔ TẢ THIẾT KẾ GIAO DIỆN HỆ THỐNG QUẢN LÝ KHÁCH SẠN")

    add_h2(doc, "1. Tổng quan thiết kế giao diện")
    add_p(doc, "Hệ thống quản lý khách sạn được xây dựng trên nền tảng Windows Forms (WinForms), hướng đến giao diện đơn giản, trực quan và dễ sử dụng đối với nhân viên lễ tân và người quản lý. Qua các mẫu giao diện đã thiết kế, hệ thống sử dụng phong cách thống nhất với tông màu nhã nhặn, kết hợp bố cục rõ ràng để tạo cảm giác thân thiện cho người dùng.")
    add_bullet(doc, "Giao diện được chia thành các Form tương ứng với từng nghiệp vụ quản lý của khách sạn.")
    add_bullet(doc, "Các Form sử dụng bố cục rõ ràng, các nút chức năng được đặt ở vị trí thuận tiện để người dùng dễ thao tác.")
    add_bullet(doc, "Menu chính được dùng làm trung tâm điều hướng để mở các chức năng của hệ thống.")
    add_bullet(doc, "Các Form nghiệp vụ sử dụng các điều khiển quen thuộc của WinForms như TextBox, Button, ComboBox, NumericUpDown, DataGridView và FlowLayoutPanel để hiển thị và xử lý dữ liệu.")

    add_h2(doc, "2. Quy chuẩn giao diện chung")
    add_p(doc, "Để đảm bảo tính đồng bộ trên toàn bộ phần mềm, nhóm đã thống nhất một số quy chuẩn giao diện chung:")
    add_bullet(doc, "Phông chữ: Sử dụng phông chữ rõ ràng (Segoe UI / Times New Roman), kích thước chữ vừa phải, dễ đọc trên màn hình máy tính.")
    add_bullet(doc, "Màu sắc: Tông màu chủ đạo là màu xanh đậm kết hợp màu nền sáng, tạo cảm giác chuyên nghiệp và không gây mỏi mắt.")
    add_bullet(doc, "Quy ước màu trạng thái phòng: Màu xanh lá cây biểu thị phòng đang trống; màu đỏ biểu thị phòng đang có khách ở; màu vàng biểu thị phòng đang dọn dẹp; màu xám biểu thị phòng đang sửa chữa, bảo trì.")

    # 3. Form Đăng nhập
    add_h2(doc, "3. Thiết kế Form Đăng nhập")
    add_p(doc, "Form Đăng nhập là màn hình đầu tiên dùng để xác thực người dùng trước khi truy cập vào hệ thống. Giao diện được chia thành hai phần: khu vực hình ảnh bên trái và khu vực nhập thông tin đăng nhập bên phải.")
    add_image_box(doc, "tmp-screenshots/real_form_login.png", "Hình 4.1. Giao diện Form Đăng nhập hệ thống")
    add_bullet(doc, "Tiêu đề “Đăng Nhập” được đặt nổi bật ở phía trên khu vực nhập liệu.")
    add_bullet(doc, "Hai ô nhập liệu dùng để nhập tên đăng nhập và mật khẩu.")
    add_bullet(doc, "Checkbox “Ghi nhớ mật khẩu” hỗ trợ lưu thông tin đăng nhập trên máy tính theo thiết kế của hệ thống.")
    add_bullet(doc, "Nút “Đăng Nhập” thực hiện kiểm tra thông tin tài khoản và mật khẩu. Nếu chính xác, hệ thống sẽ mở Menu chính; nếu sai quá số lần quy định, hệ thống sẽ tạm khóa để bảo vệ tài khoản.")

    # 4. Form Menu chính
    add_h2(doc, "4. Thiết kế Form Menu chính")
    add_p(doc, "Form Menu chính là trung tâm điều hướng của toàn bộ chương trình sau khi người dùng đăng nhập thành công. Giao diện bao gồm thanh thông tin người dùng ở phía trên và khu vực các nhóm chức năng chính ở giữa màn hình.")
    add_image_box(doc, ".verify-build/ui-screens/menu-1920x1080.png", "Hình 4.2. Giao diện Form Menu chính và bảng điều hướng chức năng")
    add_bullet(doc, "Thanh tiêu đề hiển thị tên nhân viên đang đăng nhập, vai trò (Quản lý hoặc Nhân viên) và nút Đăng xuất.")
    add_bullet(doc, "Khu vực menu được chia thành 6 nhóm chức năng lớn: Quản lý phòng, Dịch vụ, Khách hàng, Ca trực, Tài chính, Nhân viên & hệ thống.")
    add_bullet(doc, "Người dùng nhấp vào từng ô chức năng để mở các màn hình nghiệp vụ tương ứng.")

    # 5. Form Sơ đồ phòng
    add_h2(doc, "5. Thiết kế Form Sơ đồ phòng")
    add_p(doc, "Form Sơ đồ phòng là màn hình quan trọng giúp nhân viên theo dõi tình trạng buồng phòng của khách sạn theo thời gian thực.")
    add_image_box(doc, ".verify-build/ui-screens/room-map-1920x1080.png", "Hình 4.3. Giao diện Form Sơ đồ buồng phòng trực quan theo thời gian thực")
    add_bullet(doc, "Khu vực “Bộ lọc”: Cho phép người dùng lọc danh sách phòng theo tầng, loại phòng hoặc trạng thái phòng (phòng trống, đang ở, đang dọn).")
    add_bullet(doc, "Khu vực hiển thị sơ đồ phòng: Sử dụng điều khiển FlowLayoutPanel để bố trí các ô phòng như Phòng 101, Phòng 102, Phòng 201... Mỗi ô phòng có màu sắc tương ứng với trạng thái thực tế.")
    add_bullet(doc, "Khi nhấp vào phòng trống, nhân viên có thể chọn Nhận phòng hoặc Đặt phòng; khi nhấp vào phòng đang có khách, nhân viên có thể xem bill hoặc gọi dịch vụ.")

    # 6. Form Đặt phòng & Nhận phòng
    add_h2(doc, "6. Thiết kế Form Đặt phòng & Nhận phòng")
    add_p(doc, "Form Đặt phòng và Nhận phòng hỗ trợ nhân viên tiếp nhận thông tin khách đến thuê phòng hoặc đặt phòng trước.")
    add_image_box(doc, ".verify-build/ui-screens/booking-1920x1080.png", "Hình 4.4. Giao diện tra cứu và chọn lượt lưu trú của phòng")
    add_bullet(doc, "Khu vực “Thông tin khách hàng”: Dùng để nhập họ tên khách, số điện thoại, số CCCD và loại giấy tờ.")
    add_bullet(doc, "Khu vực “Thông tin thuê phòng”: Chọn phòng muốn đặt, ngày đến dự kiến, ngày đi dự kiến, số tiền cọc (nếu có).")
    add_bullet(doc, "Nút “Nhận phòng trực tiếp”: Dùng khi khách đến ở ngay, hệ thống sẽ lưu thông tin và chuyển phòng sang trạng thái đang có khách.")
    add_bullet(doc, "Nút “Lưu đặt phòng”: Dùng để ghi nhận lịch đặt trước của khách vào cơ sở dữ liệu.")

    # 7. Form Quản lý dịch vụ & Minibar
    add_h2(doc, "7. Thiết kế Form Quản lý dịch vụ & Minibar")
    add_p(doc, "Form Quản lý dịch vụ dùng để quản lý danh sách dịch vụ và đồ uống minibar mà khách sạn cung cấp.")
    add_image_box(doc, "tmp/report_figures/current/bill-editor.png", "Hình 4.5. Giao diện quản lý dịch vụ và chỉnh sửa bill tạm tính")
    add_bullet(doc, "Khu vực “Thông tin dịch vụ”: Dùng để nhập Mã dịch vụ, Tên dịch vụ, Đơn giá và Đơn vị tính.")
    add_bullet(doc, "Nút “Thêm”: Dùng để thêm một dịch vụ hoặc đồ uống mới vào danh mục.")
    add_bullet(doc, "Nút “Sửa”: Cho phép chỉnh sửa giá bán hoặc tên của dịch vụ đang chọn.")
    add_bullet(doc, "Nút “Xóa”: Dùng để xóa hoặc ngừng kinh doanh dịch vụ.")
    add_bullet(doc, "Bảng “Danh sách dịch vụ”: Sử dụng DataGridView để hiển thị tất cả các dịch vụ đang có.")
    add_bullet(doc, "Chức năng thêm vào bill phòng: Cho phép nhân viên chọn phòng và thêm đồ uống khách đã dùng vào hóa đơn.")

    # 8. Form Quản lý hóa đơn
    add_h2(doc, "8. Thiết kế Form Quản lý hóa đơn")
    add_p(doc, "Form Quản lý hóa đơn là nơi nhân viên kiểm tra các chi phí và thực hiện thanh toán khi khách trả phòng.")
    add_image_box(doc, ".verify-build/checkout-qr-final/checkout-workspace-1366x768.png", "Hình 4.6. Giao diện tính tiền và thanh toán trả phòng")
    add_bullet(doc, "Khu vực “Chi tiết hóa đơn”: Hiển thị số phòng, tên khách hàng, thời gian nhận phòng, thời gian trả phòng thực tế.")
    add_bullet(doc, "Bảng danh sách chi phí: Liệt kê chi tiết tiền phòng, các món đồ uống minibar đã dùng, số lượng, đơn giá và thành tiền.")
    add_bullet(doc, "Khu vực tính tổng tiền: Tự động cộng tiền phòng và tiền dịch vụ, trừ đi số tiền cọc khách đã nộp trước đó để tính ra số tiền khách cần thanh toán.")
    add_bullet(doc, "Nút “Thanh toán”: Xác nhận hoàn tất thanh toán hóa đơn.")
    add_bullet(doc, "Nút “In hóa đơn”: Cho phép xem trước và in hóa đơn thanh toán giao cho khách hàng.")

    # 9. Form Thanh toán VietQR
    add_h2(doc, "9. Thiết kế Form Thanh toán VietQR")
    add_p(doc, "Để hỗ trợ khách hàng thanh toán nhanh chóng, hệ thống tích hợp chức năng hiển thị mã QR thanh toán ngân hàng.")
    add_image_box(doc, ".verify-build/checkout-qr-final/checkout-transfer-1366x768.png", "Hình 4.7. Giao diện hiển thị mã QR thanh toán ngân hàng VietQR")
    add_bullet(doc, "Khi nhân viên chọn phương thức “Chuyển khoản”, hệ thống sẽ tự động tạo mã QR tương ứng với số tiền cần thu của hóa đơn.")
    add_bullet(doc, "Khách hàng sử dụng ứng dụng ngân hàng quét mã QR để chuyển khoản chính xác số tiền mà không cần nhập thủ công số tài khoản.")

    # 10. Form Quản lý ca trực
    add_h2(doc, "10. Thiết kế Form Quản lý ca trực")
    add_p(doc, "Form Quản lý ca trực giúp nhân viên theo dõi số tiền thu được trong ca làm việc và thực hiện bàn giao cho ca tiếp theo.")
    add_image_box(doc, ".verify-build/ui-screens/accounting-1920x1080.png", "Hình 4.8. Giao diện xác nhận thanh toán và in biên lai thu ngân")
    add_bullet(doc, "Hiển thị thời gian mở ca, tên nhân viên trực và số tiền mặt đầu ca trong két.")
    add_bullet(doc, "Tổng hợp số tiền đã thu trong ca (tiền mặt và chuyển khoản).")
    add_bullet(doc, "Khu vực kiểm đếm cuối ca: Nhân viên nhập số tiền mặt đếm được trong két, hệ thống sẽ đối chiếu và hiển thị số tiền chênh lệch (nếu có).")
    add_bullet(doc, "Nút “Bàn giao ca”: Xác nhận đóng ca làm việc và in biên bản ca.")

    # 11. Form Quản lý nhân viên
    add_h2(doc, "11. Thiết kế Form Quản lý nhân viên")
    add_p(doc, "Form Quản lý nhân viên dùng để quản lý hồ sơ những người làm việc tại khách sạn.")
    add_image_box(doc, "tmp/report_figures/current/account-1366x768.png", "Hình 4.9. Giao diện quản lý hồ sơ nhân viên và phân quyền")
    add_bullet(doc, "Khu vực “Thông tin nhân viên”: Gồm các ô nhập Mã nhân viên, Họ tên, Số điện thoại và Chức vụ.")
    add_bullet(doc, "Các nút chức năng: Thêm nhân viên mới, Sửa thông tin, Xóa nhân viên theo quy định.")
    add_bullet(doc, "Bảng danh sách nhân viên: Sử dụng DataGridView để hiển thị đầy đủ danh sách nhân sự.")

    # 12. Form Quản lý tài khoản
    add_h2(doc, "12. Thiết kế Form Quản lý tài khoản")
    add_p(doc, "Form Quản lý tài khoản dùng để cấp quyền và quản lý tài khoản đăng nhập cho nhân viên.")
    add_image_box(doc, ".verify-build/account-profile/account-tab-2.png", "Hình 4.10. Giao diện quản lý tài khoản người dùng và đổi mật khẩu")
    add_bullet(doc, "Khu vực thông tin tài khoản: Tên đăng nhập, mật khẩu, họ tên nhân viên và quyền sử dụng (Admin hoặc Nhân viên).")
    add_bullet(doc, "Nút “Khóa/Mở tài khoản”: Cho phép tạm khóa tài khoản khi nhân viên nghỉ việc.")
    add_bullet(doc, "Nút “Đặt lại mật khẩu”: Giúp khôi phục mật khẩu mặc định khi nhân viên quên mật khẩu.")

    # 13. Form Thống kê doanh thu
    add_h2(doc, "13. Thiết kế Form Thống kê doanh thu")
    add_p(doc, "Form Thống kê doanh thu hỗ trợ người quản lý theo dõi kết quả kinh doanh của khách sạn theo từng khoảng thời gian.")
    add_image_box(doc, "tmp-screenshots/revenue_chart.png", "Hình 4.11. Biểu đồ trực quan thống kê doanh thu phòng và dịch vụ")
    add_bullet(doc, "Khu vực “Chọn thời gian”: Cho phép chọn ngày bắt đầu và ngày kết thúc để xem báo cáo.")
    add_bullet(doc, "Nút “Thống kê”: Hệ thống truy vấn cơ sở dữ liệu và tổng hợp số liệu doanh thu.")
    add_bullet(doc, "Bảng hiển thị số liệu: Thống kê tổng doanh thu tiền phòng, tiền dịch vụ minibar, số lượng hóa đơn.")
    add_bullet(doc, "Biểu đồ trực quan: Sử dụng biểu đồ để người quản lý dễ dàng so sánh doanh thu giữa các ngày trong tháng.")
    add_bullet(doc, "Nút “Xuất Excel”: Hỗ trợ xuất dữ liệu ra file Excel (.xlsx) để lưu trữ.")

    # 14. Luồng sử dụng
    add_h2(doc, "14. Luồng sử dụng giao diện")
    add_p(doc, "Luồng sử dụng phần mềm của nhân viên diễn ra theo các bước cơ bản sau:")
    add_bullet(doc, "Người dùng mở chương trình và đăng nhập tại Form Đăng nhập.")
    add_bullet(doc, "Sau khi đăng nhập thành công, hệ thống chuyển đến Form Menu chính.")
    add_bullet(doc, "Nhân viên chọn Sơ đồ phòng để kiểm tra phòng trống hoặc phòng đang có khách.")
    add_bullet(doc, "Thực hiện Nhận phòng cho khách đến hoặc Đặt phòng trước.")
    add_bullet(doc, "Khi khách gọi thêm đồ uống minibar, nhân viên mở bill phòng để thêm món.")
    add_bullet(doc, "Khi khách trả phòng, nhân viên mở Form Hóa đơn, kiểm tra tiền và thanh toán.")
    add_bullet(doc, "Cuối ca, nhân viên kiểm tiền và bàn giao ca trực.")
    add_bullet(doc, "Khi hoàn thành công việc, nhân viên chọn Đăng xuất khỏi hệ thống.")

    # 15. Các Control WinForms
    add_h2(doc, "15. Các Control WinForms được sử dụng")
    add_p(doc, "Trong quá trình xây dựng giao diện hệ thống, nhóm sử dụng các điều khiển chính của Windows Forms:")
    add_bullet(doc, "Form: Dùng làm cửa sổ hiển thị chính cho từng chức năng của phần mềm.")
    add_bullet(doc, "Label: Dùng để hiển thị các tiêu đề, nhãn thông tin hướng dẫn người dùng.")
    add_bullet(doc, "TextBox: Dùng để nhập dữ liệu dạng văn bản như họ tên, số điện thoại, số phòng.")
    add_bullet(doc, "Button: Dùng cho các nút thao tác như Đăng nhập, Thêm, Sửa, Xóa, Lưu, Thanh toán.")
    add_bullet(doc, "ComboBox: Dùng để chọn loại phòng, đơn vị tính, phương thức thanh toán từ danh sách có sẵn.")
    add_bullet(doc, "DateTimePicker: Dùng để chọn ngày giờ nhận phòng, ngày trả phòng và ngày xem thống kê.")
    add_bullet(doc, "DataGridView: Dùng để hiển thị danh sách dữ liệu dạng bảng như danh sách phòng, danh sách nhân viên, hóa đơn.")
    add_bullet(doc, "FlowLayoutPanel: Dùng để sắp xếp các ô phòng trực quan trên sơ đồ buồng phòng.")
    add_bullet(doc, "PictureBox: Dùng để hiển thị hình ảnh minh họa và mã QR thanh toán ngân hàng.")

    # 16. Đánh giá
    add_h2(doc, "16. Đánh giá thiết kế giao diện")
    add_p(doc, "Nhìn chung, giao diện của hệ thống đáp ứng tốt các yêu cầu đề ra:")
    add_bullet(doc, "Bố cục rõ ràng, các chức năng được phân nhóm hợp lý giúp người dùng dễ làm quen và thao tác.")
    add_bullet(doc, "Sơ đồ phòng hiển thị trực quan qua màu sắc giúp nhân viên nhận biết nhanh tình trạng phòng trống.")
    add_bullet(doc, "Các thao tác thêm, sửa, xóa, thanh toán đều có thông báo xác nhận để tránh người dùng bấm nhầm.")

    # 17. Ưu điểm và hạn chế
    add_h2(doc, "17. Ưu điểm và hạn chế của hệ thống")
    add_h3(doc, "17.1. Ưu điểm")
    add_bullet(doc, "Phần mềm đáp ứng đầy đủ các nghiệp vụ quản lý cơ bản của một khách sạn vừa và nhỏ.")
    add_bullet(doc, "Giao diện đơn giản, dễ sử dụng, phù hợp với nhân viên lễ tân.")
    add_bullet(doc, "Dữ liệu được lưu trữ tập trung trên SQL Server, đảm bảo an toàn và không bị mất mát khi tắt máy.")
    add_bullet(doc, "Hỗ trợ thanh toán nhanh bằng mã VietQR, tiện lợi cho khách hàng.")
    add_bullet(doc, "Hỗ trợ thống kê doanh thu và xuất báo cáo ra file Excel giúp người quản lý dễ theo dõi tình hình kinh doanh.")

    add_h3(doc, "17.2. Hạn chế")
    add_bullet(doc, "Phần mềm hiện tại chạy trên máy tính cài đặt Windows, chưa có phiên bản trên điện thoại di động.")
    add_bullet(doc, "Chưa kết nối tự động với các trang web đặt phòng trực tuyến (OTA) như Booking hay Agoda.")
    add_bullet(doc, "Chưa có chức năng tự động sao lưu định kỳ cơ sở dữ liệu lên đám mây.")

    doc.add_page_break()

def build_conclusion_and_references(doc):
    # KẾT LUẬN
    add_h1(doc, "KẾT LUẬN")
    add_p(doc, "Sau quá trình tìm hiểu, phân tích và thực hiện đề tài “Xây dựng phần mềm quản lý khách sạn”, nhóm chúng em đã hoàn thành được các mục tiêu đề ra ban đầu:")
    add_bullet(doc, "Nắm vững các kiến thức về lập trình hướng đối tượng với ngôn ngữ C# và phát triển ứng dụng Windows Forms trên nền tảng .NET.")
    add_bullet(doc, "Biết cách phân tích yêu cầu nghiệp vụ thực tế và thiết kế cơ sở dữ liệu quan hệ trên Microsoft SQL Server.")
    add_bullet(doc, "Xây dựng được một phần mềm hoàn chỉnh có khả năng hỗ trợ công việc quản lý buồng phòng, đặt phòng, dịch vụ minibar, lập hóa đơn, thanh toán và thống kê doanh thu.")
    add_bullet(doc, "Rèn luyện được kỹ năng làm việc nhóm, trao đổi và phân chia công việc hiệu quả giữa các thành viên.")

    add_p(doc, "Bên cạnh những kết quả đã đạt được, phần mềm vẫn còn một số điểm có thể cải tiến do thời gian thực hiện và kinh nghiệm của nhóm còn hạn chế. Trong thời gian tới, nhóm chúng em sẽ tiếp tục nghiên cứu để bổ sung thêm các tính năng nâng cao như ứng dụng di động cho nhân viên buồng phòng, kết nối hóa đơn điện tử và tối ưu hóa hệ thống tốt hơn.")

    doc.add_page_break()

    # TÀI LIỆU THAM KHẢO
    add_h1(doc, "TÀI LIỆU THAM KHẢO")
    add_p(doc, "Trong quá trình tìm hiểu, phân tích và xây dựng phần mềm quản lý khách sạn, nhóm có tham khảo các tài liệu sau:")

    references = [
        "Microsoft, Windows Forms for .NET – Documentation. Tài liệu hướng dẫn xây dựng giao diện ứng dụng Windows Forms: https://learn.microsoft.com/en-us/dotnet/desktop/winforms/",
        "Microsoft, C# Documentation. Tài liệu về ngôn ngữ lập trình C#, cú pháp, cấu trúc chương trình và lập trình hướng đối tượng: https://learn.microsoft.com/en-us/dotnet/csharp/",
        "Microsoft, SQL Server Documentation. Tài liệu hướng dẫn sử dụng SQL Server, quản lý cơ sở dữ liệu, xây dựng bảng và truy vấn dữ liệu: https://learn.microsoft.com/en-us/sql/sql-server/",
        "Microsoft, ADO.NET Documentation. Tài liệu về kết nối, truy xuất và thao tác dữ liệu giữa C# và SQL Server.",
        "Dapper Tutorial. Tài liệu hướng dẫn sử dụng thư viện Dapper trong C#.",
        "QRCoder Documentation. Hướng dẫn tạo mã QR trong ứng dụng .NET.",
        "Giáo trình môn Lập trình Windows do giảng viên bộ môn cung cấp.",
        "Giáo trình môn Cơ sở dữ liệu hoặc Hệ quản trị cơ sở dữ liệu do giảng viên cung cấp.",
        "Giáo trình môn Phân tích và thiết kế hệ thống thông tin do giảng viên cung cấp.",
        "Các tài liệu bài giảng và hướng dẫn thực hành lập trình C#, Windows Forms trên các diễn đàn công nghệ."
    ]

    for idx, ref in enumerate(references):
        add_bullet(doc, ref, f"{idx + 1}.  ")

print("report_sections_part4.py updated with all images for Chapter 4 and B&W styling.")
