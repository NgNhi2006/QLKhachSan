from pathlib import Path
from docx import Document
from docx.oxml import OxmlElement
from docx.text.paragraph import Paragraph

SRC = Path(r'D:\QLKS.docx')
OUT = Path(r'D:\WorkSpace\QLKhachSan\QLKS_da_chinh_sua.docx')
doc = Document(SRC)

def before(anchor, text, style='Normal'):
    node = OxmlElement('w:p')
    anchor._p.addprevious(node)
    p = Paragraph(node, anchor._parent)
    p.style = style
    p.add_run(text)
    return p

def after(anchor, text, style='Normal'):
    node = OxmlElement('w:p')
    anchor._p.addnext(node)
    p = Paragraph(node, anchor._parent)
    p.style = style
    p.add_run(text)
    return p

def insert_block_before(anchor, rows):
    for style, text in rows:
        before(anchor, text, style)

p = doc.paragraphs
toc_title = p[13]
toc_old = p[14:50]
intro = p[52]
ch1 = p[62]
old_business = [p[i] for i in [63,71,80,88,97,105,114,122,131,139]]
ch3 = p[148]
old_db = [p[i] for i in [149,152,176,191,212,231]]
ch4 = p[252]
old_ui = [p[i] for i in [253,257,265,275,282,286,290,296,300,308,316,322,328,332]]
conclusion = p[337]

ch1.text = 'CHƯƠNG 1. MÔ TẢ CHỨC NĂNG VÀ MÔ TẢ NGHIỆP VỤ'
insert_block_before(old_business[0], [
 ('Heading 2','1.1. Giới thiệu đề tài'),
 ('Heading 3','1.1.1. Tên đề tài và mục tiêu'),
 ('Normal','Đề tài “Xây dựng ứng dụng quản lý khách sạn” hướng đến việc tập trung dữ liệu phòng, khách, đặt phòng, lưu trú, dịch vụ và thanh toán trong một hệ thống. Mục tiêu là hỗ trợ nhân viên xử lý nghiệp vụ hằng ngày và giúp người quản lý tra cứu, đối chiếu số liệu.'),
 ('Heading 3','1.1.2. Phạm vi và công nghệ sử dụng'),
 ('Normal','Ứng dụng được xây dựng bằng C# Windows Forms và sử dụng SQL Server. Phạm vi hiện tại gồm quản lý phòng và lưu trú, dịch vụ, kho, hóa đơn, giao dịch thanh toán, ca trực, tài khoản, phân quyền và báo cáo theo cấu trúc dữ liệu V26.'),
 ('Heading 2','1.2. Mô tả chức năng'),
 ('Normal','Các nhóm chức năng chính gồm: đăng nhập và phân quyền; tra cứu phòng và đặt trước; nhận, gia hạn, chuyển và trả phòng; quản lý khách; gọi và giao dịch vụ; theo dõi kho; lập bill, hóa đơn và thanh toán; quản lý ca trực; tra cứu nhật ký và thống kê. Quyền truy cập được kiểm tra theo chức năng được cấp cho từng tài khoản.'),
 ('Heading 3','1.2.1. Yêu cầu phi chức năng'),
 ('Normal','Hệ thống cần bảo đảm tính nhất quán khi nhiều nhân viên cùng thao tác với phòng, lượt lưu trú và thanh toán. Các thao tác ghi liên quan phải được thực hiện trong giao dịch dữ liệu; thông tin nhạy cảm cần được kiểm soát bằng tài khoản, quyền và nhật ký. Màn hình tra cứu cần cho phép lọc dữ liệu đủ nhanh để phục vụ công việc tại quầy.'),
 ('Heading 2','1.3. Mô tả nghiệp vụ'),
 ('Normal','Luồng nghiệp vụ bắt đầu từ kiểm tra phòng và đặt trước hoặc nhận trực tiếp, tiếp tục với lưu trú và dịch vụ, rồi kết thúc bằng lập bill, thanh toán và bàn giao ca. Các mục sau trình bày quy tắc của từng nhóm công việc.'),
])
for i, heading in enumerate(old_business, 1):
    heading.text = f'1.3.{i}. {heading.text.split(" ", 1)[1]}'
    heading.style = 'Heading 3'
insert_block_before(ch3, [
 ('Heading 2','1.4. Khảo sát hiện trạng và yêu cầu người dùng'),
 ('Normal','Cách quản lý bằng sổ hoặc nhiều tệp rời khiến nhân viên phải đối chiếu thủ công tình trạng phòng, lịch đặt, dịch vụ và các khoản đã thu. Người dùng cần một nơi tra cứu theo phòng, khách và thời gian; thao tác đặt và nhận phòng phải kiểm tra xung đột lịch trước khi lưu; bill cần thể hiện rõ tiền phòng, dịch vụ, cọc và số còn thu.'),
 ('Heading 2','1.5. Dữ liệu vào và ra của các biểu mẫu'),
 ('Normal','Đăng nhập nhận tên tài khoản và mật khẩu, trả về phiên làm việc cùng menu theo quyền. Biểu mẫu đặt phòng nhận khách, phòng, ngày đến, số ngày và cọc; kết quả là lượt đặt và giao dịch cọc tương ứng. Biểu mẫu lưu trú nhận thông tin nhận, gia hạn hoặc chuyển phòng; kết quả là các chặng ở và trạng thái phòng. Biểu mẫu dịch vụ nhận lượt, dịch vụ và số lượng; kết quả là yêu cầu, số đã giao và biến động kho nếu có. Biểu mẫu checkout nhận phương thức thanh toán và trả về hóa đơn, chi tiết bill cùng giao dịch thanh toán.'),
])

insert_block_before(ch3, [
 ('Heading 1','CHƯƠNG 2. PHÂN TÍCH CHỨC NĂNG'),
 ('Heading 2','2.1. Đặc tả các chức năng chính'),
 ('Heading 3','2.1.1. Đăng nhập và phân quyền'),
 ('Normal','Nhân viên nhập thông tin đăng nhập. Hệ thống xác thực tài khoản, kiểm tra trạng thái khóa, tải vai trò và các mã chức năng được cấp. Khi quyền hoặc mật khẩu thay đổi, phiên cũ phải hết hiệu lực.'),
 ('Heading 3','2.1.2. Đặt phòng và nhận phòng'),
 ('Normal','Nhân viên chọn khách, phòng và khoảng ngày. Hệ thống kiểm tra phòng bảo trì, lượt đang ở và lịch đặt chồng nhau; trước khi ghi, điều kiện khả dụng được kiểm tra lại trong giao dịch. Nhận phòng từ lượt đặt chuyển trạng thái và tạo chặng lưu trú đầu tiên.'),
 ('Heading 3','2.1.3. Quản lý lưu trú và khách hàng'),
 ('Normal','Gia hạn hoặc chuyển phòng phải giữ lịch sử từng chặng với thời điểm và đơn giá. Hồ sơ khách được tra cứu theo tên, điện thoại hoặc giấy tờ và liên kết với các lượt; thông tin lịch sử không bị mất khi hồ sơ được cập nhật.'),
 ('Heading 3','2.1.4. Dịch vụ và kho'),
 ('Normal','Yêu cầu dịch vụ lưu số lượng gọi và số đã giao. Nhân viên có thể giao từng phần hoặc hủy phần chưa giao. Với dịch vụ gắn mặt hàng, hệ thống kiểm tra tồn và ghi biến động kho theo số lượng thực giao.'),
 ('Heading 3','2.1.5. Bill, thanh toán và ca trực'),
 ('Normal','Bill tạm tính tổng hợp tiền phòng theo chặng và dịch vụ đã phát sinh, sau đó đối chiếu cọc để xác định số cần thu hoặc hoàn. Checkout tạo hóa đơn và giao dịch thanh toán. Ca trực tổng hợp các khoản phát sinh để so với tiền thực đếm khi bàn giao.'),
 ('Heading 3','2.1.6. Báo cáo và nhật ký'),
 ('Normal','Báo cáo lấy số liệu doanh thu từ hóa đơn và chi tiết bill; dòng tiền lấy từ giao dịch thanh toán. Nhật ký giúp tra lại các thay đổi quan trọng. Các chỉ số không được gọi là lợi nhuận khi hệ thống không lưu đầy đủ chi phí.'),
 ('Heading 2','2.2. Kiến trúc hệ thống'),
 ('Normal','Giao diện Windows Forms nhận dữ liệu và thao tác người dùng. Lớp nghiệp vụ kiểm tra quyền, trạng thái và quy tắc xử lý; lớp truy cập dữ liệu đọc ghi SQL Server. Các thao tác có nhiều bước, như nhận phòng, giao dịch vụ và checkout, cần được thực hiện thống nhất trong một giao dịch dữ liệu.'),
 ('Heading 2','2.3. Quan hệ giữa chức năng và dữ liệu'),
 ('Normal','Đặt phòng và nhận phòng sử dụng Phong, khách hàng, lượt lưu trú và các chặng ở. Dịch vụ sử dụng danh mục dịch vụ, yêu cầu dịch vụ, hàng tồn và biến động kho. Checkout sử dụng chặng lưu trú, dịch vụ, hóa đơn, chi tiết bill và giao dịch thanh toán. Ca trực đối chiếu giao dịch thanh toán; phân quyền sử dụng tài khoản, chức năng và bảng quyền; báo cáo đọc các dữ liệu đã chốt.'),
])

ch3.text = 'CHƯƠNG 3. XÂY DỰNG CƠ SỞ DỮ LIỆU'
for i, h in enumerate(old_db, 1):
    h.text = f'3.{i}. {h.text.split(" ", 1)[1]}'
ch4.text = 'CHƯƠNG 4. CHƯƠNG TRÌNH GIAO DIỆN'
insert_block_before(old_ui[0], [
 ('Heading 2','4.1. Môi trường và cách chạy chương trình'),
 ('Normal','Ứng dụng sử dụng C# Windows Forms và SQL Server. Trước khi chạy cần có cơ sở dữ liệu đúng phiên bản mà chương trình hỗ trợ và cấu hình kết nối phù hợp. Người dùng đăng nhập bằng tài khoản đã được cấp quyền để mở các chức năng tương ứng.'),
 ('Heading 2','4.2. Thiết kế giao diện chung'),
 ('Normal','Giao diện chính tổ chức chức năng theo menu và quyền của tài khoản. Vùng nội dung thay đổi theo mục được chọn; sơ đồ phòng, danh sách lượt, bill và các màn hình quản lý dùng bộ lọc để tìm bản ghi cần xử lý. Thông báo lỗi cần nêu rõ điều kiện không hợp lệ để nhân viên sửa trước khi lưu.'),
 ('Heading 2','4.3. Các màn hình của chương trình'),
])
for i, h in enumerate(old_ui, 1):
    h.text = f'4.3.{i}. {h.text.split(" ", 1)[1]}'
    h.style = 'Heading 3'
insert_block_before(conclusion, [
 ('Heading 2','4.4. Kịch bản kiểm thử'),
 ('Normal','Các kịch bản cần kiểm tra gồm: đăng nhập đúng, sai mật khẩu và tài khoản bị khóa; đặt phòng trùng lịch; nhận phòng từ lượt đặt; gia hạn và chuyển phòng khi phòng đích bận; giao dịch vụ vượt tồn; checkout khi còn yêu cầu chờ; đối chiếu cọc và số phải thu; bàn giao ca có chênh lệch; tài khoản thiếu quyền truy cập. Mỗi trường hợp cần ghi dữ liệu đầu vào, kết quả mong đợi và kết quả thực tế sau khi chạy chương trình.'),
 ('Heading 2','4.5. Hướng dẫn sử dụng chương trình'),
 ('Normal','Đăng nhập bằng tài khoản được cấp. Tại sơ đồ phòng, lọc theo loại phòng và khoảng ngày rồi chọn đặt trước hoặc nhận trực tiếp. Trong thời gian lưu trú, mở lượt để cập nhật khách, gia hạn, chuyển phòng hoặc gọi dịch vụ. Trước khi trả phòng, kiểm tra bill, tiền cọc, số còn thu và phương thức thanh toán rồi xác nhận checkout. Cuối ca, đối chiếu giao dịch với tiền thực đếm và bàn giao theo quyền được cấp.'),
])

for old in toc_old:
    old._element.getparent().remove(old._element)

toc_title.style = 'TOC Heading'
after(toc_title, '', 'Normal')

doc.save(OUT)
print(OUT)
