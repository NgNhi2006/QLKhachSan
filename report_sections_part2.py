import docx
from docx.shared import Inches, Pt, RGBColor
from generate_qlks_helpers import (
    add_h1, add_h2, add_h3, add_p, add_bullet, add_styled_table,
    COLOR_PRIMARY, COLOR_SECONDARY, COLOR_DARK_TEXT, COLOR_MUTED
)

def build_chapter1(doc):
    add_h1(doc, "CHƯƠNG 1. TỔNG QUAN ĐỀ TÀI")

    add_h2(doc, "1. Lý do chọn đề tài")
    add_p(doc, "Hiện nay, cùng với sự phát triển của công nghệ thông tin, việc ứng dụng các phần mềm vào hoạt động quản lý kinh doanh ngày càng trở nên phổ biến. Đối với các khách sạn và nhà nghỉ, số lượng khách hàng, buồng phòng, dịch vụ minibar và hóa đơn phát sinh mỗi ngày khá lớn. Nếu thực hiện việc quản lý bằng sổ sách hoặc các phương pháp thủ công sẽ mất nhiều thời gian, dễ xảy ra sai sót và gây khó khăn trong việc kiểm tra, thống kê doanh thu.")

    add_p(doc, "Xuất phát từ những vấn đề trên, nhóm chúng em lựa chọn đề tài “Xây dựng phần mềm quản lý khách sạn”. Đề tài hướng đến việc xây dựng một phần mềm hỗ trợ nhân viên và người quản lý thực hiện các công việc như quản lý phòng, đặt phòng, quản lý dịch vụ, lập hóa đơn, thanh toán, quản lý nhân viên và thống kê doanh thu.")

    add_p(doc, "Thông qua đề tài, nhóm có thể vận dụng những kiến thức đã được học về lập trình, cơ sở dữ liệu và phân tích thiết kế hệ thống để xây dựng một phần mềm có tính thực tế, góp phần nâng cao hiệu quả quản lý và hoạt động của khách sạn.")

    add_h2(doc, "2. Mục tiêu của đề tài")
    add_p(doc, "Đề tài được thực hiện với các mục tiêu chính sau:")
    add_bullet(doc, "Xây dựng phần mềm hỗ trợ quản lý hoạt động của khách sạn.")
    add_bullet(doc, "Quản lý thông tin các phòng, loại phòng và bảng giá phòng.")
    add_bullet(doc, "Quản lý trạng thái và thông tin các phòng trong khách sạn (phòng trống, phòng đang có khách, phòng đang dọn dẹp, phòng bảo trì).")
    add_bullet(doc, "Hỗ trợ nhân viên đặt phòng trước và tiếp nhận khách nhận phòng trực tiếp.")
    add_bullet(doc, "Quản lý việc gọi và sử dụng các dịch vụ buồng phòng, minibar của khách hàng.")
    add_bullet(doc, "Hỗ trợ nhân viên lập hóa đơn và thực hiện thanh toán cho khách hàng bằng tiền mặt hoặc chuyển khoản mã VietQR.")
    add_bullet(doc, "Quản lý thông tin nhân viên và tài khoản đăng nhập hệ thống.")
    add_bullet(doc, "Hỗ trợ tìm kiếm, thêm, sửa và xóa thông tin trong hệ thống theo phân quyền.")
    add_bullet(doc, "Thống kê doanh thu theo khoảng thời gian.")
    add_bullet(doc, "Lưu trữ dữ liệu tập trung trong cơ sở dữ liệu SQL Server.")
    add_bullet(doc, "Xây dựng giao diện đơn giản, dễ sử dụng và phù hợp với nghiệp vụ của khách sạn.")

    add_h2(doc, "3. Đối tượng nghiên cứu")
    add_p(doc, "Đối tượng nghiên cứu của đề tài là quy trình quản lý và kinh doanh tại khách sạn, bao gồm:")
    add_bullet(doc, "Quản lý phòng và sơ đồ buồng phòng.")
    add_bullet(doc, "Quản lý đặt phòng và lưu trú của khách.")
    add_bullet(doc, "Quản lý dịch vụ phòng và minibar.")
    add_bullet(doc, "Quản lý khách hàng.")
    add_bullet(doc, "Quản lý nhân viên.")
    add_bullet(doc, "Quản lý tài khoản.")
    add_bullet(doc, "Quản lý hóa đơn và chi tiết hóa đơn.")
    add_bullet(doc, "Thanh toán và quản lý ca trực thu ngân.")
    add_bullet(doc, "Thống kê và báo cáo doanh thu.")
    add_p(doc, "Bên cạnh đó, nhóm nghiên cứu các công nghệ và công cụ được sử dụng để xây dựng phần mềm, bao gồm lập trình ứng dụng Windows Forms trên nền tảng .NET và hệ quản trị cơ sở dữ liệu SQL Server.")

    add_h2(doc, "4. Phạm vi của đề tài")
    add_h3(doc, "4.1. Phạm vi chức năng")
    add_p(doc, "Phần mềm tập trung vào các chức năng chính:")
    add_bullet(doc, "Đăng nhập và phân quyền người sử dụng.")
    add_bullet(doc, "Quản lý thông tin và trạng thái phòng trên sơ đồ trực quan.")
    add_bullet(doc, "Quản lý đặt phòng trước và nhận phòng trực tiếp.")
    add_bullet(doc, "Quản lý danh mục dịch vụ và kho minibar.")
    add_bullet(doc, "Thêm dịch vụ buồng phòng vào hóa đơn.")
    add_bullet(doc, "Đổi phòng và gia hạn lưu trú.")
    add_bullet(doc, "Thanh toán hóa đơn bằng tiền mặt hoặc quét mã QR ngân hàng.")
    add_bullet(doc, "Quản lý ca trực thu ngân và đối soát tiền mặt khi giao ca.")
    add_bullet(doc, "Quản lý nhân viên và tài khoản đăng nhập.")
    add_bullet(doc, "Tra cứu hóa đơn và lịch sử lưu trú.")
    add_bullet(doc, "Thống kê doanh thu theo khoảng thời gian.")

    add_h3(doc, "4.2. Phạm vi dữ liệu")
    add_p(doc, "Dữ liệu của hệ thống được lưu trữ và quản lý bằng SQL Server. Các thông tin được lưu trữ bao gồm thông tin nhân viên, tài khoản, phòng, loại phòng, khách hàng, lượt lưu trú, chặng lưu trú, dịch vụ, kho hàng, hóa đơn, chi tiết hóa đơn, giao dịch thanh toán và các dữ liệu liên quan.")

    add_h3(doc, "4.3. Phạm vi người sử dụng")
    add_p(doc, "Phần mềm hướng đến các nhóm người sử dụng chính:")
    add_bullet(doc, "Quản lý: Có quyền quản lý nhân viên, tài khoản, phòng, bảng giá, dịch vụ, phân quyền và xem thống kê doanh thu.")
    add_bullet(doc, "Nhân viên: Thực hiện các nghiệp vụ bán hàng và phục vụ như kiểm tra phòng trống, nhận phòng, đặt phòng, gọi dịch vụ minibar, lập hóa đơn và thanh toán cho khách.")

    add_h2(doc, "5. Phương pháp thực hiện")
    add_p(doc, "Trong quá trình thực hiện đề tài, nhóm sử dụng các phương pháp sau:")
    add_bullet(doc, "Khảo sát và phân tích: Tìm hiểu quy trình hoạt động của khách sạn và xác định các nghiệp vụ cần thiết cho hệ thống.")
    add_bullet(doc, "Phân tích thiết kế hệ thống: Xác định các đối tượng, chức năng và mối quan hệ giữa các thành phần trong hệ thống.")
    add_bullet(doc, "Thiết kế cơ sở dữ liệu: Xây dựng các bảng dữ liệu và mối quan hệ giữa các bảng nhằm đảm bảo dữ liệu được lưu trữ một cách hợp lý và toàn vẹn.")
    add_bullet(doc, "Lập trình: Sử dụng ngôn ngữ C# và nền tảng Windows Forms (.NET 10) để xây dựng giao diện và các chức năng của phần mềm.")
    add_bullet(doc, "Kiểm thử: Kiểm tra từng chức năng của hệ thống, phát hiện và khắc phục các lỗi trong quá trình sử dụng.")

    add_h2(doc, "6. Công nghệ và công cụ sử dụng")
    add_p(doc, "Để xây dựng phần mềm, nhóm sử dụng các công nghệ và công cụ chính sau:")

    tech_headers = ["STT", "Công nghệ / Công cụ", "Mô tả mục đích sử dụng"]
    tech_data = [
        ["1", "C# (C-Sharp)", "Ngôn ngữ lập trình chính dùng để xây dựng các chức năng và xử lý logic của phần mềm."],
        ["2", "Windows Forms (.NET 10)", "Nền tảng xây dựng giao diện ứng dụng trên hệ điều hành Windows."],
        ["3", "Microsoft SQL Server", "Hệ quản trị cơ sở dữ liệu dùng để lưu trữ và quản lý dữ liệu của hệ thống."],
        ["4", "Dapper", "Thư viện hỗ trợ kết nối và truy vấn cơ sở dữ liệu nhanh chóng, tiện lợi."],
        ["5", "Visual Studio / SSMS", "Môi trường phát triển tích hợp (IDE) dùng để viết mã nguồn và quản lý cơ sở dữ liệu."],
        ["6", "QRCoder", "Thư viện tạo mã VietQR phục vụ thanh toán chuyển khoản nhanh chóng."]
    ]
    tech_widths = [Inches(0.6), Inches(2.4), Inches(3.8)]
    add_styled_table(doc, tech_headers, tech_data, tech_widths)

    doc.add_page_break()

def build_chapter2(doc):
    add_h1(doc, "CHƯƠNG 2: KHẢO SÁT VÀ MÔ TẢ NGHIỆP VỤ QUẢN LÝ KHÁCH SẠN")

    add_h2(doc, "1. Khái niệm nghiệp vụ quản lý khách sạn")
    add_p(doc, "Nghiệp vụ quản lý khách sạn là tập hợp các hoạt động được thực hiện nhằm quản lý và kiểm soát toàn bộ quá trình hoạt động kinh doanh của khách sạn, từ việc quản lý buồng phòng, khách hàng, đặt phòng, dịch vụ minibar, nhân viên, tài khoản đến quá trình lập hóa đơn, thanh toán và thống kê doanh thu.")

    add_p(doc, "Trong quá trình kinh doanh, nhân viên tiếp nhận yêu cầu của khách hàng, lựa chọn phòng phù hợp, ghi nhận thông tin khách, tiếp nhận các dịch vụ khách gọi và lập hóa đơn khi khách trả phòng. Khi khách thanh toán, hệ thống lưu lại thông tin hóa đơn, cập nhật trạng thái phòng và sử dụng dữ liệu này để phục vụ cho việc thống kê, báo cáo doanh thu.")

    add_p(doc, "Hệ thống quản lý khách sạn giúp giảm việc quản lý thủ công, hạn chế sai sót khi tính tiền phòng và dịch vụ, dễ dàng tra cứu thông tin và hỗ trợ người quản lý theo dõi tình hình kinh doanh của khách sạn.")

    add_h2(doc, "2. Mô tả hoạt động nghiệp vụ của khách sạn")
    add_p(doc, "Hoạt động nghiệp vụ tại khách sạn diễn ra theo các bước sau:")
    add_bullet(doc, "Khách hàng liên hệ đặt phòng trước hoặc đến trực tiếp khách sạn để thuê phòng.")
    add_bullet(doc, "Nhân viên kiểm tra sơ đồ phòng trống trên hệ thống để chọn phòng phù hợp cho khách.")
    add_bullet(doc, "Nhân viên ghi nhận thông tin khách hàng (họ tên, số điện thoại, CCCD), thời gian nhận phòng và trả phòng dự kiến.")
    add_bullet(doc, "Nếu khách đặt trước, nhân viên có thể ghi nhận tiền đặt cọc giữ chỗ của khách.")
    add_bullet(doc, "Khi khách nhận phòng, trạng thái phòng được chuyển sang 'Đang ở'.")
    add_bullet(doc, "Trong quá trình lưu trú, nếu khách sử dụng các dịch vụ minibar, đồ uống hoặc giặt ủi, nhân viên sẽ thêm dịch vụ vào bill của phòng.")
    add_bullet(doc, "Khi khách trả phòng, nhân viên kiểm tra số lượng dịch vụ đã dùng và thực hiện tính tổng tiền thanh toán.")
    add_bullet(doc, "Nhân viên xác nhận thanh toán (tiền mặt hoặc chuyển khoản QR), hệ thống lưu hóa đơn và in hóa đơn cho khách.")
    add_bullet(doc, "Phòng được chuyển về trạng thái 'Đang dọn' để nhân viên buồng phòng dọn dẹp trước khi đón khách mới.")
    add_bullet(doc, "Cuối ca, nhân viên kiểm đếm tiền mặt và bàn giao ca trực.")
    add_bullet(doc, "Dữ liệu được lưu trữ để người quản lý thống kê doanh thu và theo dõi hoạt động kinh doanh.")

    add_h2(doc, "3. Các đối tượng tham gia nghiệp vụ")
    add_p(doc, "Trong hệ thống quản lý khách sạn có các đối tượng chính sau:")

    add_h3(doc, "3.1. Quản lý")
    add_p(doc, "Quản lý có quyền hạn cao nhất trong hệ thống, chịu trách nhiệm:")
    add_bullet(doc, "Quản lý danh mục phòng và giá phòng.")
    add_bullet(doc, "Quản lý danh mục dịch vụ và kho hàng minibar.")
    add_bullet(doc, "Quản lý nhân viên và tài khoản đăng nhập.")
    add_bullet(doc, "Phân quyền sử dụng chức năng cho nhân viên.")
    add_bullet(doc, "Theo dõi các hóa đơn và ca trực.")
    add_bullet(doc, "Xem thống kê, báo cáo doanh thu của khách sạn.")

    add_h3(doc, "3.2. Nhân viên")
    add_p(doc, "Nhân viên là người trực tiếp sử dụng phần mềm để phục vụ khách hàng, chịu trách nhiệm:")
    add_bullet(doc, "Kiểm tra tình trạng phòng trên sơ đồ buồng phòng.")
    add_bullet(doc, "Thực hiện nhận phòng trực tiếp hoặc đặt phòng trước cho khách.")
    add_bullet(doc, "Ghi nhận các dịch vụ và đồ uống minibar khách sử dụng.")
    add_bullet(doc, "Lập hóa đơn và thực hiện thanh toán khi khách trả phòng.")
    add_bullet(doc, "Thực hiện mở ca, kiểm đếm tiền mặt và bàn giao ca trực.")

    add_h3(doc, "3.3. Khách hàng")
    add_p(doc, "Khách hàng là người sử dụng dịch vụ lưu trú tại khách sạn:")
    add_bullet(doc, "Lựa chọn phòng và đặt phòng.")
    add_bullet(doc, "Cung cấp thông tin cá nhân để làm thủ tục nhận phòng.")
    add_bullet(doc, "Sử dụng các dịch vụ trong phòng và minibar.")
    add_bullet(doc, "Thanh toán tiền phòng và các dịch vụ đã sử dụng.")
    add_p(doc, "Khách hàng không trực tiếp sử dụng phần mềm, mà các yêu cầu của khách được nhân viên lễ tân nhập vào hệ thống.")

    add_h2(doc, "4. Các nghiệp vụ chính cần quản lý")
    add_p(doc, "Dựa trên hoạt động thực tế của khách sạn, hệ thống được chia thành các nhóm nghiệp vụ chính sau:")

    add_h3(doc, "4.1. Quản lý phòng & sơ đồ phòng")
    add_p(doc, "Quản lý danh sách các phòng mà khách sạn đang kinh doanh, bao gồm số phòng, loại phòng (Standard, Deluxe, VIP...), đơn giá và trạng thái phòng.")
    add_p(doc, "Mục đích: giúp nhân viên dễ dàng theo dõi phòng nào còn trống, phòng nào đang có khách và hỗ trợ quản lý cập nhật giá phòng.")

    add_h3(doc, "4.2. Quản lý đặt phòng & lưu trú")
    add_p(doc, "Quản lý thông tin đặt phòng của khách hàng trước khi đến nhận phòng, bao gồm thời gian đến, thời gian đi dự kiến, tiền đặt cọc và hạn nhận phòng.")
    add_p(doc, "Mục đích: giúp khách sạn chủ động sắp xếp phòng, tránh tình trạng trùng lịch và theo dõi tiền cọc của khách.")

    add_h3(doc, "4.3. Quản lý hóa đơn")
    add_p(doc, "Quản lý toàn bộ quá trình tính tiền và thanh toán của khách hàng, bao gồm:")
    add_bullet(doc, "Tính tiền phòng theo thời gian ở thực tế.")
    add_bullet(doc, "Cộng tiền các dịch vụ buồng phòng và minibar khách đã dùng.")
    add_bullet(doc, "Trừ tiền cọc khách đã nộp trước đó.")
    add_bullet(doc, "Tính tổng số tiền thực thu, thanh toán và in hóa đơn.")
    add_p(doc, "Đây là nghiệp vụ trung tâm của hệ thống, vì các thông tin về phòng, dịch vụ và nhân viên đều liên quan đến quá trình lập hóa đơn.")

    add_h3(doc, "4.4. Quản lý dịch vụ & kho minibar")
    add_p(doc, "Quản lý danh mục các dịch vụ, nước uống và đồ ăn nhẹ cung cấp tại phòng. Theo dõi số lượng tồn kho để kịp thời nhập thêm hàng khi cần thiết.")

    add_h3(doc, "4.5. Quản lý nhân viên")
    add_p(doc, "Quản lý thông tin của những người làm việc tại khách sạn, bao gồm mã nhân viên, họ tên, số điện thoại, chức vụ và trạng thái làm việc.")

    add_h3(doc, "4.6. Quản lý tài khoản")
    add_p(doc, "Quản lý tài khoản đăng nhập của nhân viên vào hệ thống, bao gồm tên đăng nhập, mật khẩu, khóa/mở tài khoản và phân quyền người sử dụng.")

    add_h3(doc, "4.7. Thống kê – báo cáo")
    add_p(doc, "Hệ thống tổng hợp dữ liệu từ các hóa đơn đã thanh toán để cung cấp thông tin cho người quản lý, bao gồm:")
    add_bullet(doc, "Doanh thu theo ngày, theo tháng.")
    add_bullet(doc, "Số lượng hóa đơn và lượt khách lưu trú.")
    add_bullet(doc, "Tỷ lệ phòng có khách và các dịch vụ được sử dụng nhiều nhất.")

    add_h2(doc, "5. Quy trình nghiệp vụ tổng quát")
    add_p(doc, "Quy trình nghiệp vụ tổng quát trong phần mềm được thực hiện qua các bước:")
    add_bullet(doc, "Bước 1: Nhân viên mở sơ đồ phòng để kiểm tra phòng trống.", "1. ")
    add_bullet(doc, "Bước 2: Chọn phòng và thực hiện Nhận phòng trực tiếp hoặc Đặt phòng trước.", "2. ")
    add_bullet(doc, "Bước 3: Nhập thông tin khách hàng, số ngày ở dự kiến và tiền cọc (nếu có).", "3. ")
    add_bullet(doc, "Bước 4: Khi khách gọi dịch vụ minibar, nhân viên chọn phòng và thêm dịch vụ vào bill.", "4. ")
    add_bullet(doc, "Bước 5: Khi khách trả phòng, nhân viên mở màn hình thanh toán, kiểm tra chi phí và chọn phương thức thanh toán (tiền mặt hoặc chuyển khoản QR).", "5. ")
    add_bullet(doc, "Bước 6: Nhân viên xác nhận thanh toán, in hóa đơn giao cho khách và phòng chuyển sang trạng thái dọn dẹp.", "6. ")
    add_bullet(doc, "Bước 7: Cuối ca trực, nhân viên kiểm đếm tiền và thực hiện bàn giao ca.", "7. ")

    doc.add_page_break()

print("report_sections_part2.py updated with authentic student tone.")
