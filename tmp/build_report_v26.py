from docx import Document
from docx.shared import Cm, Pt, RGBColor
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.enum.table import WD_TABLE_ALIGNMENT, WD_CELL_VERTICAL_ALIGNMENT
from docx.enum.style import WD_STYLE_TYPE
from docx.oxml import OxmlElement
from docx.oxml.ns import qn
from pathlib import Path
import re

ROOT=Path(r'D:\WorkSpace\QLKhachSan')
OUT=ROOT/'Bao_cao_du_an_QLKhachSan_V26.docx'
sql=(ROOT/'Database'/'Database.sql').read_text(encoding='utf-8')
d=Document(); sec=d.sections[0]
sec.page_width=Cm(21); sec.page_height=Cm(29.7)
sec.top_margin=Cm(2); sec.bottom_margin=Cm(2); sec.left_margin=Cm(3); sec.right_margin=Cm(2)
sec.header_distance=Cm(.9); sec.footer_distance=Cm(.9)
styles=d.styles
normal=styles['Normal']; normal.font.name='Times New Roman'; normal.font.size=Pt(12); normal.font.color.rgb=RGBColor(0,0,0)
normal.paragraph_format.line_spacing=1.25; normal.paragraph_format.space_after=Pt(7)
for n,size,space in [('Title',22,14),('Heading 1',16,12),('Heading 2',13,9),('Heading 3',12,7)]:
 s=styles[n]; s.font.name='Times New Roman'; s.font.size=Pt(size); s.font.bold=True; s.font.color.rgb=RGBColor(0,0,0)
 s.paragraph_format.space_before=Pt(space); s.paragraph_format.space_after=Pt(7)
 s.paragraph_format.keep_with_next=True
for style in ['Title','Heading 1','Heading 2','Heading 3']:
 styles[style].element.rPr.rFonts.set(qn('w:eastAsia'),'Times New Roman')
for style in ['Subtitle','TOC 1','TOC 2','Footer','List Bullet']:
 try: s=styles[style]
 except KeyError: s=styles.add_style(style,WD_STYLE_TYPE.PARAGRAPH)
 s.font.name='Times New Roman'; s.font.size=Pt(12); s.font.color.rgb=RGBColor(0,0,0)

def shade(cell,fill='E9EEF4'):
 tcPr=cell._tc.get_or_add_tcPr(); sh=OxmlElement('w:shd'); sh.set(qn('w:fill'),fill); tcPr.append(sh)
def borders(cell):
 tcPr=cell._tc.get_or_add_tcPr(); bd=OxmlElement('w:tcBorders')
 for side in ['top','left','bottom','right']:
  x=OxmlElement('w:'+side); x.set(qn('w:val'),'single'); x.set(qn('w:color'),'D9D9D9'); x.set(qn('w:sz'),'4'); bd.append(x)
 tcPr.append(bd)
def table(headers,rows,widths=None):
 t=d.add_table(rows=1, cols=len(headers)); t.alignment=WD_TABLE_ALIGNMENT.CENTER; t.autofit=False
 for i,h in enumerate(headers):
  c=t.rows[0].cells[i]; c.text=h; shade(c,'E6E6E6'); borders(c)
  for r in c.paragraphs[0].runs: r.font.bold=True; r.font.color.rgb=RGBColor(0,0,0); r.font.size=Pt(12)
 for j,row in enumerate(rows):
  cells=t.add_row().cells
  for i,x in enumerate(row):
   cells[i].text=str(x); borders(cells[i]); cells[i].vertical_alignment=WD_CELL_VERTICAL_ALIGNMENT.CENTER
   if j%2: shade(cells[i],'F5F7FA')
   for p in cells[i].paragraphs:
    p.paragraph_format.space_after=Pt(2); p.paragraph_format.line_spacing=1.1
    for r in p.runs: r.font.size=Pt(12); r.font.color.rgb=RGBColor(0,0,0)
 for row in t.rows:
  if widths:
   for i,w in enumerate(widths): row.cells[i].width=Cm(w)
 return t
def p(x,style=None):
 q=d.add_paragraph(x,style); q.alignment=WD_ALIGN_PARAGRAPH.JUSTIFY if style is None else WD_ALIGN_PARAGRAPH.LEFT
 return q
def h(x,level=1): d.add_heading(x,level)
def page(title,level=2):
 d.add_page_break(); h(title,level)
def bullets(items):
 for x in items: p(x,'List Bullet')
figure_number=0
def figure(filename,caption,demo=False,width=15.7):
 global figure_number
 path=ROOT/'tmp'/'report_figures'/'current'/filename
 if not path.exists(): raise FileNotFoundError(path)
 figure_number+=1
 pic=d.add_paragraph(); pic.alignment=WD_ALIGN_PARAGRAPH.CENTER
 pic.paragraph_format.space_before=Pt(8); pic.paragraph_format.space_after=Pt(3)
 pic.paragraph_format.keep_with_next=True
 pic.add_run().add_picture(str(path),width=Cm(width))
 note='Dữ liệu minh họa' if demo else 'Ảnh chụp trên bản sao V26'
 cap=d.add_paragraph(f'Hình {figure_number}. {caption}. {note}.')
 cap.alignment=WD_ALIGN_PARAGRAPH.CENTER
 cap.paragraph_format.space_after=Pt(10)
 for run in cap.runs: run.font.name='Times New Roman'; run.font.size=Pt(12); run.font.color.rgb=RGBColor(0,0,0)

# Page number only, aligned at the bottom right.
foot=sec.footer.paragraphs[0]; foot.alignment=WD_ALIGN_PARAGRAPH.RIGHT
fld=OxmlElement('w:fldSimple'); fld.set(qn('w:instr'),'PAGE'); foot._p.append(fld)

p('BÁO CÁO DỰ ÁN', 'Title').alignment=WD_ALIGN_PARAGRAPH.CENTER
p('HỆ THỐNG QUẢN LÝ KHÁCH SẠN', 'Title').alignment=WD_ALIGN_PARAGRAPH.CENTER
p('Phiên bản cơ sở dữ liệu V26  •  20 bảng ứng dụng').alignment=WD_ALIGN_PARAGRAPH.CENTER
p('Báo cáo phân tích nghiệp vụ, cơ sở dữ liệu và giao diện', 'Subtitle').alignment=WD_ALIGN_PARAGRAPH.CENTER
d.add_paragraph('')
table(['Thông tin','Nội dung'],[
 ('Nền tảng','.NET 10 WinForms, C#, SQL Server Express'),('Kiến trúc','GUI, BLL, DAL và DTO'),
 ('Phạm vi','Nghiệp vụ vận hành, mô hình 20 bảng và form chức năng'),('Ngày lập','14/10/2026')],[4.3,11.7])
p('Tài liệu này mô tả trạng thái dự án sau khi tinh gọn cơ sở dữ liệu đến V26. Những chức năng từng có trong phiên bản cũ nhưng đã loại bỏ được nêu rõ để tránh hiểu nhầm về phạm vi hiện hành.')

page('MỤC LỤC',1)
q=d.add_paragraph(); fld=OxmlElement('w:fldSimple'); fld.set(qn('w:instr'),'TOC \\o "1-2" \\h \\z \\u'); q._p.append(fld)

page('MỞ ĐẦU',1)
p('Lý do chọn đề tài', 'Heading 2')
p('Trong hoạt động kinh doanh khách sạn, chất lượng phục vụ phụ thuộc nhiều vào khả năng tổ chức và xử lý thông tin kịp thời. Mỗi ngày, khách sạn phải theo dõi tình trạng phòng, tiếp nhận khách, phục vụ các yêu cầu trong thời gian lưu trú và hoàn tất thanh toán khi khách trả phòng. Khi số lượng phòng, khách lưu trú và dịch vụ phát sinh tăng lên, việc quản lý bằng phương pháp thủ công dễ gây chậm trễ, sai sót và khó khăn trong quá trình tra cứu. Vì vậy, ứng dụng công nghệ thông tin vào công tác quản lý là một nhu cầu thiết thực.')
p('Xuất phát từ thực tế đó, đề tài “Xây dựng hệ thống quản lý khách sạn” được lựa chọn nhằm tìm hiểu quy trình hoạt động của khách sạn và xây dựng một công cụ hỗ trợ công việc quản lý thường ngày. Hệ thống hướng tới việc tổ chức thông tin tập trung, giúp nhân viên thực hiện công việc thuận tiện hơn và giúp người quản lý theo dõi tình hình hoạt động một cách rõ ràng. Việc sử dụng cùng một hệ thống cũng tạo điều kiện để các bộ phận tra cứu thông tin nhất quán và phối hợp tốt hơn trong quá trình phục vụ khách.')
p('Bên cạnh ý nghĩa thực tiễn, đề tài còn là cơ hội vận dụng những kiến thức đã học về phân tích yêu cầu, thiết kế cơ sở dữ liệu và xây dựng phần mềm vào một bài toán cụ thể. Quá trình thực hiện giúp làm rõ mối liên hệ giữa quy trình nghiệp vụ, dữ liệu cần lưu trữ và giao diện mà người dùng thao tác. Báo cáo trình bày ba nội dung chính: nghiệp vụ quản lý khách sạn, cơ sở dữ liệu của hệ thống và các form chức năng trong ứng dụng.')

page('1 MÔ TẢ NGHIỆP VỤ',1)
business=[
('1.1 Bối cảnh và mục tiêu','Khách sạn cần biết phòng nào còn khả dụng, khách nào đang ở, dịch vụ nào đã giao và số tiền phải thu khi trả phòng. Hệ thống tập trung dữ liệu vào SQL Server để nhiều thao tác ở quầy dựa trên cùng một nguồn thông tin. Các đối tượng chính là phòng, khách hàng, lượt lưu trú, dịch vụ, hóa đơn và giao dịch thanh toán.','Kết quả mong muốn là giảm ghi chép thủ công, kiểm soát trùng lịch và cung cấp lịch sử đủ để đối chiếu giao dịch.'),
('1.2 Vai trò và phân quyền','Admin quản lý tài khoản, vai trò, quyền chức năng và nhật ký. Reception xử lý phòng, đặt phòng, dịch vụ và ca cá nhân theo quyền được cấp. Manager theo dõi vận hành và báo cáo. Accountant theo dõi các chức năng tài chính còn được hỗ trợ.','Quyền được kiểm tra ở điều hướng và lớp nghiệp vụ; việc chỉ ẩn nút trên form không đủ bảo vệ dữ liệu.'),
('1.3 Vòng đời phòng','Phòng có trạng thái vật lý như trống, đang ở, đang dọn và bảo trì. Lịch đặt trước nằm ở lượt lưu trú nên một phòng có thể trống hiện tại nhưng đã có khách đặt ngày sau. Trước khi nhận hoặc đặt phòng, ứng dụng xét cả trạng thái hiện tại lẫn khoảng ngày dự kiến.','Khi giao dịch được xác nhận, trạng thái và lịch phải phản ánh cùng một kết quả để nhân viên không nhận nhầm phòng.'),
('1.4 Đặt phòng trước','Nhân viên chọn ngày đến, số ngày, phòng và khách. Hệ thống kiểm tra khoảng ngày, lịch chồng, tình trạng bảo trì và hạn giữ chỗ. Đặt trước có thể đi cùng tiền cọc; khoản cọc được ghi tại giao dịch thanh toán.','Lượt chưa cọc giữ tối đa 24 giờ; lượt đã cọc giữ tối đa 15 ngày kể từ lúc tạo, theo chính sách hiện hành trong mã nguồn.'),
('1.5 Nhận phòng và lưu trú','Nhận phòng trực tiếp tạo lượt đang ở; nhận lượt đã đặt chuyển lượt chờ sang đang ở sau khi kiểm tra lại khả dụng. Chặng lưu trú giữ lịch sử phòng, thời gian và đơn giá. Gia hạn hoặc chuyển phòng tạo căn cứ tính tiền thay vì ghi đè toàn bộ quá trình ở.','Mỗi thao tác thay đổi lưu trú cần kiểm tra lịch trong transaction vì màn hình có thể hiển thị dữ liệu cũ.'),
('1.6 Khách hàng','Hồ sơ khách chứa thông tin nhận diện, liên hệ và lịch sử liên quan đến các lượt ở. Nhân viên nhập khách khi đặt hoặc nhận phòng, có thể tìm theo tên, điện thoại hoặc giấy tờ. Thông tin khách được dùng trên danh sách đặt, bill và báo cáo.','Khi sửa hồ sơ, các hóa đơn đã lập vẫn phải giữ quan hệ đến đúng khách đã giao dịch.'),
('1.7 Gọi và giao dịch vụ','Yêu cầu dịch vụ gắn với lượt lưu trú. Nhân viên có thể gọi, sửa số lượng còn chờ, giao từng phần hoặc hủy phần chưa giao. Dịch vụ gắn hàng tồn kho cần kiểm tra tồn trước khi giao và tạo biến động kho.','Chỉ phần thực tế được cung cấp mới được ghi nhận vào bill theo quy tắc tính tiền của hệ thống.'),
('1.8 Tính bill và trả phòng','Bill tạm tính cộng tiền phòng theo chặng lưu trú và các dịch vụ hợp lệ. Khi xác nhận trả phòng, hệ thống tạo hóa đơn và giao dịch thanh toán, xử lý cọc đã thu và hoàn phần cọc thừa nếu có. Sau đó phòng chuyển sang trạng thái phục vụ bước dọn phòng.','Báo giá chỉ là ảnh chụp tại một thời điểm; checkout phải kiểm tra lại trạng thái và tổng tiền trước khi ghi.'),
('1.9 Ca trực và dòng tiền','Ca trực gắn trách nhiệm của người thu trong khoảng làm việc. Nhân viên mở ca, xem giao dịch, nhập tiền thực đếm và bàn giao; người có quyền khóa ca. Số tiền dự kiến tại V26 dựa trên giao dịch thanh toán còn lưu.','Không còn phiếu thu chi độc lập, sổ quỹ hoặc đối soát ngân hàng trong cấu trúc 20 bảng.'),
('1.10 Kho và kiểm soát tồn','Danh mục hàng tồn và biến động kho hỗ trợ nhập, xuất và tiêu thụ gắn với dịch vụ. Tồn khả dụng được kiểm tra tại thời điểm giao dịch. Người dùng xem mặt hàng, lượng còn và lịch sử biến động để phát hiện thiếu hàng.','V26 không còn dữ liệu báo dùng buồng phòng và theo dõi trả xe thuê.'),
('1.11 Báo cáo và kiểm tra','Báo cáo doanh thu dựa vào hóa đơn, chi tiết bill và khoản cọc không hoàn; có thể lọc theo thời gian và loại dữ liệu. Nhật ký thao tác lưu dấu vết của các thay đổi quan trọng. Các chỉ số là thông tin quản trị của ứng dụng, không thay thế báo cáo kế toán đầy đủ.','Hệ thống không còn bảng chi phí, công nợ và điều chỉnh hóa đơn nên báo cáo lãi lỗ hoặc công nợ đã bị loại khỏi phạm vi.'),
('1.12 Ngoại lệ và giới hạn','Các tình huống phải xử lý gồm phòng trùng lịch, đặt quá hạn, dịch vụ vượt tồn, checkout khi dịch vụ còn chờ, cọc không đủ điều kiện hoàn, thiếu quyền và phiên đăng nhập hết hiệu lực. Form cần giữ dữ liệu đang nhập khi lỗi nghiệp vụ xảy ra.','Giao dịch SQL phải rollback khi một bước trong quy trình ghi thất bại; thông báo lỗi cần nêu điều kiện cần sửa.')]
business_detail=[
 ('Phạm vi sử dụng','Dữ liệu phòng được dùng cho lịch đặt và trạng thái hiện tại; hồ sơ khách gắn với lượt; các yêu cầu dịch vụ đi theo lượt đang ở. Hóa đơn chỉ được lập ở bước kết thúc lưu trú. Cách tách này giúp người quản lý tra lại vì sao một khoản tiền xuất hiện trên bill.','Nhân viên nhìn thấy cùng một thông tin từ danh sách phòng, lịch đặt và màn hình thanh toán. Khi một giao dịch hoàn tất, các màn hình liên quan phải tải lại để không hiển thị trạng thái cũ.'),
 ('Phân chia trách nhiệm','Quản trị viên cấp quyền cụ thể trên từng mã chức năng. Vai trò đặt giới hạn công việc, còn quyền cá nhân quyết định chức năng thực tế được sử dụng. Nhân viên lễ tân không thể suy ra quyền kế toán từ việc nhìn thấy dữ liệu phòng.','Các thao tác đổi quyền, khóa tài khoản và đổi mật khẩu làm mất hiệu lực phiên cũ; nhật ký giúp xác định ai đã thực hiện thay đổi quan trọng.'),
 ('Đánh giá khả dụng','Trạng thái vật lý và lịch đặt là hai dữ liệu khác nhau. Một phòng đang trống vẫn có thể bị đặt trong khoảng khách mới muốn ở. Khi lọc phòng, hệ thống xét khoảng ngày; khi lưu, kiểm tra lần nữa trong giao dịch SQL.','Phòng vừa checkout chuyển sang bước dọn, không lập tức xem là sẵn sàng nhận khách. Bảo trì cũng chặn đặt hoặc nhận phòng theo điều kiện hiện hành.'),
 ('Tạo và thay đổi đặt trước','Nhân viên nhập khách, phòng, ngày đến, thời lượng và khoản cọc nếu đã thu. Thao tác sửa lịch có thể đổi phòng hoặc ngày nhưng không được làm lịch mới chồng với lượt khác. Hủy trước hạn xử lý hoàn cọc; lượt quá hạn được giải phóng lịch theo chính sách no show.','Tiền cọc được lưu thành giao dịch riêng. Cọc không hoàn là cách phân loại khoản đã thu, không tạo thêm một lần thu tiền mới.'),
 ('Diễn biến một lượt ở','Lượt là đối tượng xuyên suốt từ đặt trước đến trả phòng. Chặng lưu trú giữ mỗi giai đoạn sử dụng phòng với thời điểm và giá tương ứng. Khi chuyển phòng, chặng cũ kết thúc và chặng mới bắt đầu; hóa đơn vẫn gắn với một lượt.','Cách lưu theo chặng giúp tính tiền và giải thích bill khi có gia hạn, đổi phòng hoặc đổi đơn giá giữa kỳ.'),
 ('Sử dụng hồ sơ khách','Một hồ sơ khách có thể xuất hiện ở nhiều lượt. Tra cứu theo tên, số điện thoại hoặc giấy tờ giúp tìm khách cũ và giảm nhập lại. Danh sách lịch sử thể hiện đặt trước, đang ở và đã kết thúc; dữ liệu thanh toán vẫn gắn với lượt phát sinh.','Khi cập nhật thông tin khách, cần kiểm tra dữ liệu nhận diện và không làm đứt liên kết lịch sử đã có.'),
 ('Tiến độ phục vụ','Yêu cầu lưu số lượng gọi và số lượng đã giao. Giao một phần giúp bộ phận phục vụ xử lý khi chưa đủ hàng hoặc khách yêu cầu nhận nhiều đợt. Nhân viên chỉ được hủy phần chưa giao; phần đã giao là sự kiện đã phát sinh.','Nếu dịch vụ liên kết mặt hàng, số tồn được kiểm tra ngay trong giao dịch giao hàng và ghi biến động kho tương ứng.'),
 ('Đối chiếu trước xác nhận','Bill tạm tính cần tách tiền phòng, dịch vụ, cọc đã thu và số còn phải thanh toán. Nhân viên kiểm tra thời gian ở, chặng giá và trạng thái yêu cầu dịch vụ trước khi xác nhận. Trường hợp dịch vụ còn chờ phải được xử lý theo quy tắc checkout.','Sau khi tạo hóa đơn, dữ liệu được dùng cho thống kê doanh thu; giao dịch thanh toán dùng cho đối chiếu tiền thực thu và hoàn cọc.'),
 ('Bàn giao ca','Ca có người mở, thời điểm bắt đầu, tiền đầu ca, khoản phát sinh và tiền thực đếm khi nộp. Khoản phát sinh lấy từ GiaoDichThanhToan; vì V26 không còn PhieuThuChi, số dự kiến không bao gồm phiếu thu chi độc lập.','Người được cấp quyền xem chênh lệch và khóa ca sau bàn giao. Các màn hình tài chính cũ về sổ quỹ hoặc đối soát không còn dữ liệu để vận hành.'),
 ('Kiểm soát hàng','HangTonKho giữ danh mục và mức tồn; BienDongKho ghi từng biến động để giải thích thay đổi. Nhập hàng tăng tồn, xuất hoặc giao dịch vụ gắn kho làm giảm tồn. Khi số lượng không đủ, thao tác phải bị từ chối trước khi cập nhật yêu cầu dịch vụ.','V26 không ghi tiêu thụ buồng phòng và biên nhận trả xe thuê, do đó các báo cáo dựa trên hai nguồn này không còn đầy đủ.'),
 ('Nguồn số liệu báo cáo','Doanh thu lấy từ hóa đơn và chi tiết bill, còn tiền thu lấy từ giao dịch thanh toán. Cọc giữ trước checkout không được cộng như doanh thu phòng; cọc mất được trình bày theo chính sách riêng. Bộ lọc thời gian quyết định tập dữ liệu của bảng và biểu đồ.','Các chỉ số cần được diễn giải theo nguồn dữ liệu thực có. Không suy ra lợi nhuận từ doanh thu khi không còn bảng chi phí và sổ kế toán.'),
 ('Phục hồi khi lỗi','Lỗi có thể xuất hiện do người khác vừa nhận phòng, lượt đặt đã hết hạn hoặc tồn kho thay đổi giữa lúc form mở và lúc bấm lưu. BLL phải kiểm tra lại trong transaction, trả thông báo cụ thể và giữ nguyên dữ liệu nhập để nhân viên sửa.','Sau rollback, không được có hóa đơn thiếu chi tiết, giao dịch thanh toán không gắn lượt, hoặc kho giảm mà yêu cầu dịch vụ chưa chuyển trạng thái.')]
business_steps=[
 ['Khởi tạo danh mục phòng, dịch vụ và tài khoản có quyền.','Tiếp nhận khách, tạo hoặc tra cứu hồ sơ rồi chọn phòng và dịch vụ.','Theo dõi lượt cho đến checkout, đối chiếu hóa đơn và giao dịch thanh toán.'],
 ['Admin tạo tài khoản và chọn vai trò phù hợp công việc.','Cấp các mã chức năng trong giới hạn vai trò.','Nhân viên đăng nhập lại để menu và quyền nghiệp vụ đồng bộ.'],
 ['Lọc phòng theo loại, trạng thái và khoảng ngày muốn ở.','Đối chiếu lượt đang ở cùng lịch đặt có hiệu lực.','Kiểm tra lại trong transaction trước khi xác nhận đặt hoặc nhận.'],
 ['Nhập ngày đến, số ngày, phòng và thông tin khách.','Kiểm tra chồng lịch, hạn giữ chỗ và khoản cọc thực nhận.','Lưu lượt cùng giao dịch cọc; khi hủy thì xử lý hoàn hoặc cọc mất theo hạn.'],
 ['Chọn lượt đặt hợp lệ hoặc lập lượt nhận trực tiếp.','Ghi chặng đầu tiên với phòng và giá tại thời điểm nhận.','Khi gia hạn/chuyển phòng, kết thúc chặng cũ và ghi chặng mới.'],
 ['Tìm khách theo tên, điện thoại hoặc giấy tờ.','Dùng hồ sơ phù hợp hoặc tạo hồ sơ mới sau khi kiểm tra trùng.','Gắn khách vào lượt và giữ lịch sử qua những lần lưu trú sau.'],
 ['Chọn lượt đang ở, dịch vụ và số lượng gọi.','Nhân viên giao toàn bộ hoặc từng phần; kho giảm theo số thực giao.','Hủy phần chờ nếu khách đổi ý, giữ phần đã giao trong lịch sử.'],
 ['Tải bill gồm tiền phòng theo chặng và dịch vụ đã phát sinh.','Đối chiếu cọc, số còn thu, phương thức và yêu cầu dịch vụ còn chờ.','Xác nhận checkout để tạo hóa đơn, chi tiết và giao dịch thanh toán.'],
 ['Nhân viên mở ca với số tiền đầu ca.','Trong ca, tổng hợp khoản thu và hoàn từ giao dịch thanh toán.','Nhập tiền thực đếm, giải trình chênh lệch, bàn giao và khóa ca.'],
 ['Khởi tạo mặt hàng, đơn vị và tồn đầu.','Nhập/xuất hoặc giao dịch vụ gắn kho để tạo biến động.','Kiểm tra tồn cuối và tra lịch sử khi có chênh lệch.'],
 ['Chọn khoảng ngày và mức tổng hợp báo cáo.','Đọc hóa đơn, chi tiết bill và cọc không hoàn theo quy tắc.','Đối chiếu số tổng trên bảng, biểu đồ và dữ liệu xuất.'],
 ['Giữ dữ liệu trên form khi BLL trả lỗi nghiệp vụ.','Rollback toàn bộ câu lệnh đã chạy trong transaction.','Tải lại dữ liệu liên quan và cho phép người dùng sửa đầu vào.']]
business=business[1:11]
business_detail=business_detail[1:11]
business_steps=business_steps[1:11]
for bi,(title,a,b) in enumerate(business):
 if bi and bi%2==0: d.add_page_break()
 h(re.sub(r'^1\.\d+',f'1.{bi+1}',title),2)
 p(a); p(business_detail[bi][1]); p(business_detail[bi][2])
 for si,step in enumerate(business_steps[bi],1): p(f'Bước {si}. {step}')
 p(b)

page('2 CƠ SỞ DỮ LIỆU',1)
h('2.1 Quan hệ và luồng ghi dữ liệu',2)
table(['Luồng','Quan hệ chính','Kiểm tra'],[('Đặt và ở','KhachHang → LuotLuuTru → ChangLuuTru','Không trùng lịch và phòng hợp lệ'),('Dịch vụ','LuotLuuTru → YeuCauDichVu → DichVu','Số giao, trạng thái, tồn'),('Thanh toán','LuotLuuTru → HoaDon → ChiTietBill; GiaoDichThanhToan','Tổng tiền, cọc, một hóa đơn mỗi lượt'),('Kho','HangTonKho → BienDongKho','Số lượng, giá vốn, chứng từ'),('Quyền','TaiKhoanNhanVien → PhanQuyenNhanVien → ChucNang','Vai trò và quyền được cấp')],[3,7,6])
p('Mũi tên thể hiện quan hệ logic, không hàm ý mọi bảng đều liên kết trực tiếp bằng một khóa ngoại. Tên cột và ràng buộc thực tế cần đối chiếu với Database.sql ở đúng phiên bản triển khai.')

meta=[
('PhienBanCSDL','Lưu phiên bản schema và thời điểm nâng cấp','Ứng dụng đọc khi khởi động hoặc đăng nhập để chạy migration còn thiếu.'),
('TaiKhoanNhanVien','Lưu danh tính đăng nhập, vai trò và trạng thái tài khoản','Không xóa tài khoản đã có lịch sử giao dịch; khóa tài khoản để bảo toàn dấu vết.'),
('NhatKyThaoTac','Ghi hành động quan trọng, người thực hiện và thời điểm','Phục vụ tra soát khi sửa đặt phòng, thanh toán, kho hoặc quyền.'),
('ChucNang','Danh mục mã chức năng được phân quyền','V25 hợp nhất quyền vào bảng này, bỏ chín bảng ChucNang theo nhóm.'),
('PhanQuyenNhanVien','Liên kết tài khoản với chức năng được phép','BLL kiểm tra quyền trước khi cho phép thực thi.'),
('CauHinhMenu','Menu chính hiện hành','Chỉ hiển thị mục mà người dùng được quyền sử dụng.'),
('CauHinhMenuCon','Menu con của từng nhóm','Lưu thứ tự, tên và trạng thái điều hướng.'),
('CauHinhChucNang','Ánh xạ chức năng vào menu con','Phục vụ điều hướng tới màn hình có sẵn.'),
('Phong','Danh mục phòng, loại, giá và tình trạng vật lý','Lịch đặt nằm ở LuotLuuTru, không suy ra chỉ từ trạng thái phòng.'),
('KhachHang','Hồ sơ khách và thông tin liên hệ','Là nguồn tra cứu khi đặt và nhận phòng.'),
('LuotLuuTru','Lượt đặt, đang ở và kết thúc','Chứa thời gian, phòng, khách, trạng thái và điều kiện giữ chỗ.'),
('ChangLuuTru','Các giai đoạn phòng và giá trong một lượt','Cho phép tính tiền khi chuyển phòng hoặc đổi giá.'),
('DichVu','Danh mục dịch vụ và đơn giá','Có thể liên kết mặt hàng kho để kiểm soát khi giao.'),
('YeuCauDichVu','Yêu cầu và tiến độ giao dịch vụ','Phân biệt số gọi, số giao và phần còn chờ.'),
('HangTonKho','Danh mục mặt hàng và số tồn','Hỗ trợ theo dõi hàng phục vụ dịch vụ.'),
('BienDongKho','Lịch sử nhập, xuất và điều chỉnh tồn','Tạo dấu vết thay vì chỉ thay số tồn hiện tại.'),
('ChiTietBill','Dòng tiền phòng hoặc dịch vụ của bill','Cho phép giải thích tổng tiền trên hóa đơn.'),
('HoaDon','Chứng từ thanh toán khi kết thúc lượt','Là cơ sở thống kê doanh thu của V26.'),
('GiaoDichThanhToan','Thu cọc, thu checkout, hoàn và cọc không hoàn','Theo dõi tiền thực thu riêng với doanh thu.'),
('CaTruc','Mở, bàn giao và khóa ca','Số dự kiến dựa vào các giao dịch thanh toán.')]
db_detail=[
 ('Mỗi migration chỉ được áp dụng khi phiên bản hiện tại thấp hơn phiên bản đích. Bảng này là căn cứ để ứng dụng biết phần nâng cấp nào còn thiếu.','Chỉ tiến trình nâng cấp ghi vào bảng; người dùng nghiệp vụ không thêm, sửa hoặc xóa phiên bản bằng form. Khi chạy lại migration, kết quả phải giữ nguyên.'),
 ('Tài khoản liên kết với nhật ký, ca trực và nhiều giao dịch do nhân viên thực hiện. Vai trò là mức quyền nền; trạng thái khóa quyết định có được đăng nhập hay không.','Mật khẩu không hiển thị lại trên form. Đổi mật khẩu, đặt lại mật khẩu hoặc khóa tài khoản phải vô hiệu hóa phiên cũ. Ưu tiên khóa thay vì xóa người dùng đã có lịch sử.'),
 ('Một dòng nhật ký mô tả thao tác, người làm, đối tượng và thời điểm. Bảng này giúp đối chiếu sự thay đổi giữa dữ liệu hiện tại với hành động đã thực hiện.','Màn hình tra cứu là dạng chỉ đọc. Khi thao tác nghiệp vụ rollback, không được để nhật ký ghi nhận thành công sai thực tế.'),
 ('Mỗi chức năng có mã để lớp quyền và menu cùng tham chiếu. Sau V25, chín bảng ChucNang theo phân hệ đã được loại bỏ vì lặp danh mục này.','Chức năng bị vô hiệu hóa ở V26 không nên xuất hiện như một tác vụ có thể sử dụng dù mã chức năng lịch sử còn tồn tại.'),
 ('Bảng nối tài khoản với chức năng. Khi tải menu, hệ thống lọc các chức năng mà người dùng được cấp; khi gọi BLL, quyền được kiểm tra lại.','Quản trị viên chỉ cấp trong giới hạn vai trò. Thay đổi quyền cần được ghi nhật ký và yêu cầu đăng nhập lại nếu phiên hiện tại đã cũ.'),
 ('Menu chính là tầng điều hướng cao nhất. Danh mục hiện hành nằm trong nhóm CauHinhMenu thay cho MenuTong đã xóa.','Form chính đọc tên, thứ tự và trạng thái hiển thị rồi kết hợp quyền của người đăng nhập; menu không có chức năng hợp lệ không cần xuất hiện.'),
 ('Mỗi menu con thuộc một menu chính và nhóm các chức năng gần nhau. Bảng này giữ cấu trúc điều hướng mà giao diện hiện đang đọc.','Khi một menu bị ẩn hoặc thay đổi thứ tự, cần kiểm tra liên kết của chức năng bên dưới để không tạo mục điều hướng mồ côi.'),
 ('Bảng liên kết menu con với mã chức năng và thông tin hiển thị. Màn hình chức năng thực tế vẫn do mã WinForms cung cấp.','V26 không còn màn hình tự thiết kế. Thêm một cấu hình mới không tự sinh quy trình nghiệp vụ nếu ứng dụng chưa có form và BLL tương ứng.'),
 ('Phong là nguồn số phòng, loại, giá và trạng thái vật lý. Một phòng liên quan đến nhiều lượt theo thời gian và nhiều chặng lưu trú.','Cập nhật giá danh mục không được làm mất giá đã chốt trong chặng cũ. Bảo trì hoặc đổi trạng thái phải xét khách đang ở và lịch đặt có hiệu lực.'),
 ('KhachHang liên kết với LuotLuuTru. Một khách có thể có nhiều lần ở; việc tra cứu cần tránh tạo nhiều hồ sơ cho cùng người do khác cách gõ tên.','Sửa số điện thoại hoặc giấy tờ là cập nhật hồ sơ, không xóa lịch sử ở. Xóa vật lý khách đã có lượt có thể làm mất quan hệ nên cần hạn chế.'),
 ('LuotLuuTru là bản ghi trung tâm của đặt trước, nhận phòng và checkout. Nó gắn khách, phòng, thời gian dự kiến, trạng thái và các chứng từ phát sinh.','Kiểm tra trùng lịch trong transaction. Hủy hoặc quá hạn cập nhật trạng thái để giải phóng lịch; không xóa lượt nếu đã có cọc, dịch vụ hoặc hóa đơn.'),
 ('Mỗi chặng lưu thời gian, phòng và đơn giá của một giai đoạn trong cùng lượt. Chuyển phòng tạo ranh giới giữa hai chặng để giải thích tiền phòng.','Không để các chặng của cùng lượt chồng nhau hoặc thiếu thời điểm kết thúc khi tính bill. Chặng đã dùng để lập hóa đơn cần được giữ ổn định.'),
 ('DichVu cung cấp tên, đơn vị tính, giá và trạng thái sử dụng. Dịch vụ có thể liên kết hàng tồn để kiểm tra lúc giao.','Thay giá danh mục không được tự sửa giá của yêu cầu đã phát sinh. Ngừng kinh doanh một dịch vụ nên dùng trạng thái thay vì xóa bản ghi có lịch sử.'),
 ('YeuCauDichVu gắn dịch vụ với lượt và lưu số lượng gọi, đã giao, trạng thái. Một yêu cầu có thể được giao nhiều lần.','Sửa tổng số không được nhỏ hơn phần đã giao; hủy chỉ phần chờ. Checkout phải kiểm tra yêu cầu chưa hoàn tất theo chính sách hiện hành.'),
 ('HangTonKho giữ mặt hàng, đơn vị, lượng còn và dữ liệu phục vụ kiểm soát tồn. Nhiều dịch vụ có thể cần tra cùng danh mục hàng theo cấu hình.','Không cho xuất vượt tồn. Điều chỉnh tồn phải có biến động tương ứng để người quản lý giải thích chênh lệch giữa tồn đầu, nhập, xuất và tồn cuối.'),
 ('BienDongKho là lịch sử tăng giảm tồn theo nghiệp vụ nhập, xuất hoặc giao dịch vụ. Các bản ghi cung cấp chứng cứ cho số tồn hiện tại.','Khi giao dịch vụ gắn kho thất bại, biến động kho phải rollback cùng yêu cầu. Không xóa biến động cũ chỉ để sửa con số tồn.'),
 ('ChiTietBill tách dòng tiền phòng và dịch vụ, hỗ trợ giải thích tổng hóa đơn. Dòng bill gắn với hóa đơn được lập tại checkout.','Số lượng, đơn giá và thành tiền phải nhất quán. Nếu cần kiểm tra doanh thu theo loại, dùng các dòng bill thay vì suy đoán từ tổng hóa đơn.'),
 ('HoaDon là chứng từ chốt của lượt. Mỗi hóa đơn có tổng tiền, thời điểm và liên kết đến khách hoặc lượt theo lược đồ thực tế.','V26 không còn bảng điều chỉnh hoặc hủy hóa đơn. Vì vậy form hiện hành không được cung cấp chức năng dựa vào các bảng này; doanh thu dựa trên hóa đơn còn trong cơ sở dữ liệu.'),
 ('GiaoDichThanhToan ghi thu cọc, thu checkout, hoàn và cọc không hoàn. Một lượt có thể phát sinh nhiều giao dịch theo tiến trình.','Phải phân biệt doanh thu với tiền thực thu: thu cọc là khoản thu trước, cọc không hoàn không phải lần thu mới, hoàn tiền làm giảm dòng tiền. Số ca trực lấy từ các giao dịch thích hợp.'),
 ('CaTruc gắn nhân viên và khoảng làm việc với tiền đầu ca, tiền bàn giao, trạng thái. Màn hình ca lọc các giao dịch thuộc ca để đối chiếu.','Không còn PhieuThuChi nên số dự kiến chỉ xét nguồn thanh toán đang lưu. Sau bàn giao hoặc khóa, các thao tác sửa ca phải tuân theo quyền và trạng thái.')]
for idx,(name,role,use) in enumerate(meta,1):
 group=(0 if idx<=5 else 1 if idx<=8 else 2 if idx<=12 else 3 if idx<=16 else 4)
 group_titles=['Hệ thống và phân quyền','Cấu hình menu','Phòng và lưu trú','Dịch vụ và kho','Hóa đơn và ca trực']
 if idx in [1,6,9,13,17]: page(f'2.{group+2} {group_titles[group]}')
 elif idx not in [2,4,7,14,16]: d.add_page_break()
 q=d.add_paragraph(); q.paragraph_format.space_before=Pt(10); q.paragraph_format.keep_with_next=True
 r=q.add_run(f'Bảng {name}'); r.bold=True; r.font.size=Pt(12)
 p(role+'. '+use)
 m=re.search(r'CREATE TABLE dbo\.'+re.escape(name)+r'\s*\((.*?)\n\s*\);',sql,re.S|re.I)
 rows=[]
 if m:
  for line in m.group(1).splitlines():
   line=line.strip().rstrip(',')
   mm=re.match(r'^([A-Za-z][A-Za-z0-9_]*)\s+([A-Za-z]+(?:\([^)]*\))?)',line)
   if mm and mm.group(1).upper() not in {'CONSTRAINT','PRIMARY','FOREIGN','UNIQUE'}:
    rows.append((mm.group(1),mm.group(2)))
 if not rows: rows=[('Xem Database.sql','Cấu trúc sau migration')]
 table(['Cột ở định nghĩa gốc','Kiểu dữ liệu'],rows[:18],[9,7])
 if len(rows)>18: p(f'Bảng còn {len(rows)-18} cột trong định nghĩa gốc; cần tra lược đồ V26 khi viết truy vấn mới.')
 p(db_detail[idx-1][0])
 p(db_detail[idx-1][1])

page('2.7 Các bảng đã loại bỏ và tác động')
removed=[('V25','9 bảng ChucNang theo nhóm','Quyền đọc trực tiếp từ ChucNang; không mất chức năng cấp quyền'),('V25','MenuTong, MenuCon','Điều hướng dùng CauHinhMenu và CauHinhMenuCon'),('V26','KyKeToan, PhieuThuChi, SoDuDauKy','Không còn khóa kỳ, phiếu thu chi và sổ quỹ'),('V26','DongSaoKe, CongNo, PhanBoCongNo','Không còn đối soát sao kê và công nợ'),('V26','TieuThuBuongPhong, TraXeThue','Không còn báo dùng buồng phòng và lịch trả xe'),('V26','ThongTinTaiChinhHoaDon, DieuChinhHoaDon, HoaDonHuy','Không còn hóa đơn điện tử, điều chỉnh và hủy'),('V26','NhomHoaDon, PhanChiaHoaDon','Không còn gộp hoặc chia bill nhiều phòng'),('V26','MaGiamGia, SuDungMaGiamGia','Không còn mã giảm giá'),('V26','CauHinhChucNangMoi, DuLieuChucNangMoi','Không còn màn hình tự thiết kế')]
table(['Phiên bản','Bảng','Tác động'],removed,[2,6,8])
p('Dữ liệu của các bảng đã xóa chỉ còn trong bản sao lưu trước V26. Khi cần tra lịch sử nhóm bill, phân chia bill hoặc thông tin tài chính hóa đơn, quản trị viên phải đọc bản sao lưu thay vì truy vấn cơ sở dữ liệu đang chạy.')

page('3 FORM VÀ CHỨC NĂNG GIAO DIỆN',1)
forms=[
('3.1 Form đăng nhập','FormLogin','Tên đăng nhập, mật khẩu và trạng thái phiên.','Đối chiếu tài khoản đang hoạt động, mật khẩu và vai trò; mở màn hình chính theo quyền.','Thông báo lỗi xác thực; không cấp quyền khi đăng nhập thất bại.'),
('3.2 Menu chính và điều hướng','FormMain, ucDashboard.Navigation','Nhóm menu và chức năng được phép.','Đọc CauHinhMenu, CauHinhMenuCon, CauHinhChucNang và quyền cá nhân.','Chuyển đến form phù hợp; mục không được phép không xuất hiện.'),
('3.3 Sơ đồ phòng','ucDashboard.RoomOverview','Số phòng, loại, giá, trạng thái vật lý và dấu lịch đặt.','Lọc theo loại hoặc trạng thái; mở đặt, nhận, bảo trì theo quyền.','Trạng thái phải được xác nhận lại khi ghi giao dịch.'),
('3.4 Đặt phòng','ucDashboard.BookingDialog, MultipleBooking','Ngày đến, số ngày, phòng, khách và cọc.','Thêm lượt; sửa hoặc hủy đặt; tìm theo phòng, khách, điện thoại.','Kiểm tra chồng lịch, hạn giữ chỗ và trạng thái phòng.'),
('3.5 Hồ sơ khách','ucDashboard.Management, BookingRecords','Tên, liên hệ, giấy tờ và lịch sử lượt ở.','Thêm, cập nhật hồ sơ; tìm kiếm khách và các lượt liên quan.','Không xóa lịch sử thanh toán khi sửa thông tin khách.'),
('3.6 Lưu trú và chuyển phòng','ucDashboard.Actions, Reservations','Lượt đang ở, phòng hiện tại, ngày đến và dự kiến đi.','Nhận phòng, gia hạn, chuyển phòng; cập nhật chặng lưu trú.','Không ghi đè chặng cũ; kiểm tra khả dụng phòng đích.'),
('3.7 Dịch vụ','ucDashboard.Catalog, Actions','Danh mục và yêu cầu đang chờ theo phòng.','Tạo yêu cầu, sửa số lượng, giao từng phần, hủy phần chờ, tìm theo phòng.','Số giao không vượt số gọi; hàng gắn kho phải đủ tồn.'),
('3.8 Kho','ucDashboard.Inventory, AccountingForm','Mặt hàng, tồn và lịch sử nhập xuất.','Nhập, xuất, cập nhật danh mục và tra biến động.','Tồn không âm theo quy tắc nghiệp vụ; không có báo dùng buồng phòng.'),
('3.9 Bill và checkout','ucDashboard.BillPreview, BillEditor, CheckoutWorkspace','Tiền phòng, dịch vụ, cọc, số còn phải thu.','Xem và chỉnh bill hợp lệ; xác nhận phương thức, lập hóa đơn.','Checkout chỉ khi lượt và dịch vụ thỏa điều kiện.'),
('3.10 Hóa đơn và thanh toán','ucDashboard.CheckoutReceipt, Revenue','Danh sách hóa đơn, giao dịch thu và hoàn.','Lọc ngày, xem chi tiết, xuất dữ liệu và thống kê doanh thu.','Không còn hủy/điều chỉnh hóa đơn, giảm giá và OTA công nợ.'),
('3.11 Ca trực','AccountingForm','Ca mở, tiền đầu ca, giao dịch và số bàn giao.','Mở, nộp, khóa ca; tìm và lọc theo thời gian hoặc người trực.','Số dự kiến dựa vào GiaoDichThanhToan.'),
('3.12 Tài khoản và quyền','ucDashboard.AccountWorkspace, RoleFunctions','Tài khoản, vai trò, trạng thái và chức năng được cấp.','Tạo, khóa/mở, đặt lại mật khẩu và cập nhật quyền.','Thay đổi quyền làm phiên cũ hết hiệu lực.'),
('3.13 Nhật ký thao tác','ucDashboard.Management','Thời điểm, tài khoản, hành động và đối tượng.','Tìm/lọc để tra soát; dữ liệu chủ yếu chỉ đọc.','Không dùng chức năng xóa nhật ký trong quy trình bình thường.'),
('3.14 Thống kê và xuất dữ liệu','ucDashboard.Revenue, Reports, Export','Doanh thu, công suất/phân loại còn được hỗ trợ và bảng dữ liệu.','Chọn kỳ, làm mới, xuất Excel hoặc CSV ở màn hình có hỗ trợ.','Giải thích nguồn số liệu; không xem là lãi lỗ kế toán.')]
form_detail=[
 ('Người dùng nhập tên và mật khẩu rồi bấm đăng nhập. AuthService xác thực và tải vai trò; kết quả hợp lệ dẫn đến FormMain. Nếu tài khoản bị khóa hoặc mật khẩu sai, form báo lỗi mà không mở phiên.','Form chỉ có thao tác nhập và xác thực; không có thêm, sửa, xóa tài khoản. Thay đổi mật khẩu và quản lý tài khoản nằm ở khu vực khác. Khi kiểm thử cần thử tài khoản hợp lệ, sai mật khẩu, bị khóa và phiên cũ sau đổi quyền.'),
 ('FormMain dựng cây điều hướng từ ba bảng CauHinhMenu và quyền của tài khoản. Nhân viên chọn menu chính, menu con rồi chức năng. Mục bị vô hiệu hóa theo V26 không được dẫn đến màn hình trống hoặc lỗi SQL.','Tìm kiếm toàn cục và lối quay lại menu giúp di chuyển giữa các phân hệ. Việc đổi tên hoặc thứ tự menu là cấu hình dữ liệu, còn hành vi của màn hình chức năng vẫn do mã ứng dụng quyết định.'),
 ('Sơ đồ phòng kết hợp danh mục Phong với lượt đang ở và lịch đặt sắp tới. Người dùng nhìn màu hoặc nhãn trạng thái để chọn tác vụ; dấu lịch đặt giúp tránh nhầm phòng trống hiện tại với phòng trống cả kỳ.','Bộ lọc theo loại và trạng thái chỉ thu hẹp danh sách hiển thị. Khi bấm nhận phòng hoặc đặt trước, BLL kiểm tra lại thời gian và khả dụng trong transaction, vì dữ liệu trên sơ đồ có thể đã thay đổi.'),
 ('Form đặt trước cho nhập ngày đến, số ngày, phòng, khách và cọc. Danh sách phòng khả dụng được tính theo khoảng ngày. Sau khi chọn, nhân viên xem lại giá, hạn nhận và dữ liệu khách trước khi lưu.','Các lệnh sửa hoặc hủy đặt chỉ hiện cho lượt phù hợp. Tra cứu theo phòng, tên hoặc điện thoại dẫn tới bản ghi cần xử lý. Hủy trước hạn và quá hạn có quy tắc cọc khác nhau nên thông báo xác nhận phải thể hiện rõ số tiền hoàn hoặc không hoàn.'),
 ('Màn hình khách trình bày hồ sơ liên hệ cùng lịch sử lượt. Khi nhập khách mới, trường nhận diện và thông tin liên lạc cần được rà soát để hạn chế hồ sơ trùng. Sửa khách cập nhật hồ sơ dùng cho tra cứu sau này.','Tìm kiếm theo tên, điện thoại hoặc giấy tờ giúp mở lại khách cũ. Xóa khách đã có lượt và hóa đơn không phải thao tác thường lệ; hệ thống giữ liên kết dữ liệu lịch sử.'),
 ('Danh sách lượt cho thấy phòng đang ở, thời gian đến, ngày dự kiến trả và các thao tác tiếp theo. Nhận phòng từ lượt đặt chuyển trạng thái sau khi kiểm tra hạn. Gia hạn thay đổi ngày dự kiến; chuyển phòng tạo chặng mới.','Form không được chỉ thay số phòng trên một dòng vì sẽ mất lịch sử tính tiền. Khi chọn phòng đích, cần xem lịch theo toàn bộ khoảng ở còn lại và từ chối nếu chồng lượt khác.'),
 ('Nhân viên chọn lượt và dịch vụ trong danh mục, nhập số lượng rồi tạo yêu cầu. Bảng xử lý thể hiện phòng, khách, dịch vụ, số gọi và số đã giao. Giao từng phần cập nhật tiến độ và có thể giảm tồn kho.','Sửa số gọi phải lớn hơn hoặc bằng số đã giao. Hủy chỉ phần chưa phục vụ. Bộ lọc theo phòng hoặc dịch vụ giúp tìm yêu cầu chờ; các yêu cầu đã hủy vẫn cần có trong lịch sử khi tra soát.'),
 ('Kho hiển thị danh mục hàng, số tồn và các biến động. Nhập kho tăng số lượng; xuất kho hoặc giao dịch vụ gắn mặt hàng làm giảm. Người quản lý lọc mặt hàng, thời gian và loại biến động để đối chiếu.','Không xem báo dùng buồng phòng hoặc trả xe thuê là một nguồn biến động còn hoạt động ở V26. Khi kiểm tra tồn, phải so số hiện tại với tổng nhập trừ xuất; chênh lệch cần được điều chỉnh có dấu vết.'),
 ('Bill preview nhóm các dòng phòng và dịch vụ, kèm tổng, cọc đã thu và số phải thu. Người dùng kiểm tra chặng lưu trú, thời gian, đơn giá và yêu cầu đã giao. CheckoutWorkspace nhận phương thức thanh toán trước khi chốt.','Sau khi xác nhận, hóa đơn và thanh toán được ghi trong cùng quy trình; in/xem phiếu lấy dữ liệu đã chốt. Không dùng màn hình nhóm bill hoặc mã giảm giá vì bảng hỗ trợ đã bị loại bỏ.'),
 ('Danh sách hóa đơn cho lọc theo kỳ và mở chi tiết dòng bill. Giao dịch thanh toán hiển thị các khoản cọc, thu tại checkout và hoàn, giúp giải thích sự khác biệt giữa doanh thu và tiền mặt thực nhận.','Xuất dữ liệu phải theo bộ lọc hiện tại. V26 không còn form hủy, điều chỉnh hóa đơn hoặc công nợ OTA; các nút cũ nếu còn trong giao diện cần được ẩn hoặc chặn nhất quán.'),
 ('Người trực nhập tiền đầu ca khi mở. Trong ca, màn hình tổng hợp giao dịch thanh toán phát sinh; lúc nộp ca nhập tiền thực đếm và giải trình chênh lệch. Người có quyền xem bàn giao rồi khóa.','Tìm ca theo nhân viên hoặc khoảng thời gian hỗ trợ đối chiếu. Số dự kiến không bao gồm sổ quỹ và phiếu thu chi vì các bảng đó đã được xóa; đây là giới hạn cần hiển thị rõ trong báo cáo ca.'),
 ('Admin xem tài khoản, vai trò, trạng thái khóa và quyền chức năng. Tạo mới cần tên đăng nhập hợp lệ; đổi vai trò hoặc quyền cập nhật phạm vi menu sau lần tải mới. Đặt lại mật khẩu là thao tác nhạy cảm.','Không xóa vật lý tài khoản đã có nhật ký hoặc giao dịch. Khóa tài khoản chặn đăng nhập mới; phiên cũ phải hết hiệu lực sau thay đổi quyền, mật khẩu hoặc trạng thái.'),
 ('Nhật ký được trình bày theo thời gian và tài khoản để người quản trị xác định thao tác đã thực hiện. Bộ lọc theo ngày hoặc người dùng giúp rút ngắn danh sách lớn. Chi tiết cần thể hiện hành động và đối tượng chịu tác động.','Đây là màn hình tra soát, không phải form CRUD thông thường. Nhân viên thường không được sửa hoặc xóa nhật ký; giá trị của nó nằm ở khả năng đối chiếu với phòng, dịch vụ, hóa đơn và quyền.'),
 ('Form báo cáo nhận khoảng ngày, tải số liệu và trình bày bảng hoặc biểu đồ doanh thu. Người dùng có thể chuyển mức tổng hợp phù hợp với màn hình và xuất dữ liệu khi chức năng hỗ trợ.','Khi lọc, tổng trên biểu đồ phải bằng tổng của cùng tập hóa đơn hoặc dòng bill. Không đặt tên chỉ số là lợi nhuận nếu không có chi phí; khoản cọc đang giữ cũng không được cộng như doanh thu phòng.')]
form_figures={
 1:[('menu-1366x768.png','Màn hình tổng quan và các nhóm chức năng',False,15.7),
    ('rooms-menu-1366x768.png','Menu quản lý phòng',False,15.7)],
 2:[('room-map-1366x768.png','Sơ đồ trạng thái và bộ lọc phòng',False,15.7),
    ('ShowRoomCatalog-1366x768.png','Danh mục phòng và thao tác cập nhật',False,15.7),
    ('ShowPricing-1366x768.png','Bảng giá phòng',False,15.7)],
 3:[('booking-1366x768.png','Chọn phòng để đặt trước',False,15.7)],
 6:[('stay-picker.png','Hộp thoại chọn lượt để xử lý dịch vụ',True,12.5)],
 8:[('bill-editor.png','Chỉnh sửa bill tạm tính',True,15.0),
    ('checkout-workspace-1366x768.png','Xem tổng tiền và xác nhận thanh toán',True,15.7)],
 9:[('checkout-transfer-1366x768.png','Thanh toán chuyển khoản và mã QR',True,15.7),
    ('finance-menu-1366x768.png','Các chức năng tài chính hiện được hiển thị',False,15.7)],
 10:[('accounting-1366x768.png','Danh sách và thao tác ca trực',False,15.7)],
 11:[('account-1366x768.png','Hồ sơ tài khoản và đổi mật khẩu',True,15.7)]
}
for fi,(title,file,show,ops,guard) in enumerate(forms):
 h(title,2)
 p(f'Thành phần mã nguồn: {file}.')
 table(['Nội dung','Đặc tả'],[('Hiển thị dữ liệu',show),('Nhập và thao tác',ops),('Kiểm tra và kết quả',guard)],[4,12])
 p(form_detail[fi][0])
 p(form_detail[fi][1])
 for filename,caption,demo,width in form_figures.get(fi,[]): figure(filename,caption,demo,width)

page('KẾT LUẬN',1)
p('Dự án hiện có một lõi vận hành gồm 20 bảng ứng dụng, bao phủ phòng, khách, lưu trú, dịch vụ, kho, bill, thanh toán, ca trực, tài khoản và quyền. Việc tinh gọn làm cấu trúc dễ quản lý hơn nhưng cũng thu hẹp phạm vi: không còn sổ quỹ, công nợ, đối soát ngân hàng, hóa đơn điều chỉnh/hủy, nhóm bill, giảm giá và màn hình tự thiết kế.')
p('Khi phát triển tiếp, ưu tiên giữ sự nhất quán giữa form, BLL và schema V26. Mọi chức năng mới cần xác định rõ quy tắc nghiệp vụ, dữ liệu lưu, quyền truy cập, thao tác tìm kiếm/thống kê và phương án nâng cấp. Thay đổi cơ sở dữ liệu phải đi kèm bản sao lưu và kiểm thử khôi phục.')
page('TÀI LIỆU THAM KHẢO',1)
p('Các nguồn dưới đây là tài liệu nội bộ và mã nguồn của chính dự án, đối chiếu theo trạng thái ngày 14/10/2026. Những tài liệu mô tả phiên bản cũ chỉ được dùng để hiểu lịch sử thay đổi; phạm vi chức năng hiện hành lấy theo V26.')
for i,src in enumerate([
 'QLKhachSan. Database/Database.sql. Lược đồ, ràng buộc và dữ liệu khởi tạo SQL Server.',
 'QLKhachSan. QLKhachSan/GUI. Mã nguồn các form đăng nhập, dashboard, đặt phòng, checkout, kho và báo cáo.',
 'QLKhachSan. QLKhachSan/BLL. Mã nguồn quy tắc nghiệp vụ, phân quyền và xác thực.',
 'QLKhachSan. QLKhachSan/DAL. Mã nguồn truy vấn, giao dịch và SchemaMigrator.',
 'QLKhachSan. docs/CORE20.md. Phạm vi cơ sở dữ liệu gọn V26 và các chức năng bị loại bỏ.',
 'QLKhachSan. README.md. Hướng dẫn vận hành, cấu hình và mô tả dự án.',
 'QLKhachSan. QLKhachSan/QLKhachSan.csproj. Cấu hình nền tảng và thư viện sử dụng.'
],1): p(f'[{i}] {src}')

# Update field values when Word opens; prevent orphan headers and preserve table headers.
settings=d.settings.element
u=OxmlElement('w:updateFields'); u.set(qn('w:val'),'true'); settings.append(u)
for t in d.tables:
 trPr=t.rows[0]._tr.get_or_add_trPr(); rep=OxmlElement('w:tblHeader'); rep.set(qn('w:val'),'true'); trPr.append(rep)
d.save(OUT)
print(OUT)
print('Manual pages:', 1+sum(1 for x in d.paragraphs if x._p.xpath('.//w:br[@w:type="page"]')))
