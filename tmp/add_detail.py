from pathlib import Path
from copy import deepcopy
from docx import Document
from docx.oxml import OxmlElement
from docx.oxml.ns import qn
from docx.shared import Pt,RGBColor

f=Path(__file__).resolve().parents[1]/'docs'/'Bao_cao_he_thong_quan_ly_khach_san.docx'; d=Document(f)
def frag(): return Document()
def h(x,s): x.add_heading(s,2)
def p(x,s): x.add_paragraph(s)
def tb(x,heads,rows):
 t=x.add_table(rows=1,cols=len(heads));t.autofit=False
 for j,v in enumerate(heads):
  c=t.rows[0].cells[j];c.text=v;sh=OxmlElement('w:shd');sh.set(qn('w:fill'),'173F38');c._tc.get_or_add_tcPr().append(sh)
  for r in c.paragraphs[0].runs:r.font.bold=True;r.font.color.rgb=RGBColor(255,255,255);r.font.size=Pt(11)
 for k,row in enumerate(rows):
  cells=t.add_row().cells
  for j,v in enumerate(row):
   cells[j].text=str(v)
   if k%2:sh=OxmlElement('w:shd');sh.set(qn('w:fill'),'F3F6F4');cells[j]._tc.get_or_add_tcPr().append(sh)
   for pp in cells[j].paragraphs:
    for r in pp.runs:r.font.size=Pt(11)
 x.add_paragraph()
def put(anchor,sub):
 a=next(v for v in d.paragraphs if v.style.name=='Heading 1' and v.text.startswith(anchor))._p
 for child in list(sub.element.body):
  if child.tag!=qn('w:sectPr'):a.addprevious(deepcopy(child))
def add(sub):
 for child in list(sub.element.body):
  if child.tag!=qn('w:sectPr'):d.element.body.insert(len(d.element.body)-1,deepcopy(child))

a=frag()
h(a,'1.16. Bảng quyết định trạng thái phòng')
p(a,'Trạng thái phòng là kết quả của nhiều nguồn: tình trạng vật lý, lượt đang ở và lịch đặt sắp tới. Khi màn hình dùng duy nhất một cột trạng thái để tô màu, cần quy định thứ tự ưu tiên để cùng một phòng không hiển thị hai ý nghĩa mâu thuẫn. Màu trên sơ đồ chỉ là cách diễn đạt; hành động đặt phòng phải kiểm tra lại dữ liệu trong transaction tại thời điểm ghi.')
tb(a,['Tình huống','Nhãn ưu tiên','Thao tác được phép'],[
('Có lượt Occupied','Đang ở','Mở chi tiết, dịch vụ, thanh toán; không nhận thêm lượt cùng thời điểm.'),
('Phòng đang bảo trì','Bảo trì','Xem thông tin, kết thúc bảo trì theo quyền; không phân phòng.'),
('Phòng cần dọn','Đang dọn','Xác nhận xong dọn, sau đó kiểm tra khả dụng.'),
('Có lịch Reserved trong kỳ lọc','Có lịch đặt','Xem lịch, nhận phòng khi đến hạn, chọn ngày khác nếu còn trống.'),
('Không có xung đột','Trống','Chọn đặt trước hoặc nhận ngay theo ngày đang lọc.'),
])
p(a,'Ví dụ phòng 106 đang trống tại thời điểm hiện tại nhưng đã có lịch đặt ngày mai. Khi chọn nhận phòng ngay trong ba ngày, hệ thống không được dựa vào nhãn “Trống” của hôm nay; phải xét toàn bộ khoảng từ hôm nay đến ngày đi. Nếu khoảng chọn chỉ nằm trước lịch đặt và chính sách cho phép, phòng có thể được nhận trong khoảng ngắn hơn.')
h(a,'1.17. Xử lý lỗi và phục hồi thao tác tại quầy')
p(a,'Lễ tân thường thao tác trong lúc khách chờ, vì vậy thông báo lỗi cần chỉ rõ dữ liệu nào cần sửa. Một thông báo chung “Không thể hoàn tất thao tác” làm tăng thời gian dò nguyên nhân và dễ khiến người dùng bấm lặp, tạo rủi ro ghi trùng. Kết quả thất bại phải giữ lại nội dung đã nhập trên màn hình, trừ khi dữ liệu nhạy cảm cần xóa theo chính sách bảo mật.')
tb(a,['Loại lỗi','Ví dụ thông báo phù hợp','Cách phục hồi'],[
('Xung đột phòng','Phòng 103 đã có lịch trong khoảng đã chọn.','Đổi phòng hoặc ngày, tải lại khả dụng.'),
('Số lượng dịch vụ','Còn chờ 51, không thể giao 52.','Nhập số giao không vượt phần chờ.'),
('Kho hết hàng','Mặt hàng còn 0 đơn vị khả dụng.','Chọn mặt hàng khác hoặc nhập kho.'),
('Trùng thanh toán','Mã tham chiếu này đã được ghi nhận.','Kiểm tra giao dịch trước khi thanh toán lại.'),
('Ca/kỳ đã khóa','Chứng từ thuộc kỳ đã chốt.','Dùng chức năng điều chỉnh được cấp quyền.'),
('Thiếu quyền','Tài khoản không có quyền quản lý danh mục.','Liên hệ quản trị cấp quyền phù hợp.'),
])
p(a,'Khi mất kết nối SQL Server, ứng dụng cần cho biết thao tác chưa được xác nhận hay đã hoàn tất ở máy chủ. Trước khi thử lại giao dịch tiền, người dùng nên tra cứu mã tham chiếu hoặc hóa đơn để tránh thu hai lần. Với các lỗi có thể tự phục hồi, nút thử lại chỉ nên xuất hiện sau khi trạng thái dữ liệu đã được đồng bộ lại.')
h(a,'1.18. Vai trò của dữ liệu gốc và dữ liệu chốt')
p(a,'Danh mục phòng và dịch vụ có thể đổi giá theo thời gian. Tuy nhiên một lượt lưu trú hoặc yêu cầu dịch vụ đã tạo phải giữ căn cứ tính tiền tại thời điểm phát sinh. Vì vậy cần phân biệt dữ liệu gốc hiện hành (giá trong danh mục) với dữ liệu đã chốt cho giao dịch (đơn giá trong chặng lưu trú, yêu cầu dịch vụ và dòng bill). Nếu chỉ truy xuất giá hiện tại, hóa đơn cũ có thể thay đổi khi quản lý cập nhật bảng giá.')
tb(a,['Dữ liệu','Có thể cập nhật','Nguyên tắc khi đã phát sinh giao dịch'],[
('Giá phòng danh mục','Có, theo quyền quản lý.','Không sửa hồi tố giá chặng đã chốt.'),
('Giá dịch vụ danh mục','Có, theo quyền quản lý.','Giữ giá đã gọi/giao và giá dòng bill.'),
('Thông tin khách','Có thể chỉnh hồ sơ.','Giữ thông tin cần thiết trên hóa đơn/lượt để truy vết.'),
('Bill tạm tính','Có thể sửa trước xác nhận theo quyền.','Sau xác nhận dùng điều chỉnh/hủy có dấu vết.'),
('Cọc và thanh toán','Không xóa như dữ liệu nháp.','Ghi hoàn/đảo bút toán hoặc điều chỉnh phù hợp.'),
])
p(a,'Sự phân biệt này cũng giúp báo cáo quản trị chính xác. Báo cáo tháng trước phải dựa trên giao dịch và hóa đơn của tháng trước, không phụ thuộc bảng giá hoặc tồn kho của ngày hiện tại. Khi phát hiện sai sót sau khóa kỳ, quy trình điều chỉnh phải chỉ rõ thời điểm ghi nhận và mối liên hệ với chứng từ gốc.')
h(a,'1.19. Ma trận rủi ro vận hành')
p(a,'Các rủi ro dưới đây được chọn từ những luồng có nhiều bảng dữ liệu và nhiều vai trò cùng tham gia. Ma trận giúp ưu tiên kiểm thử, đào tạo nhân viên và quy định trách nhiệm kiểm tra cuối ca. Mức độ là đánh giá định tính để hỗ trợ vận hành, không phải kết quả đo xác suất sự cố của hệ thống.')
tb(a,['Rủi ro','Tác động','Biện pháp kiểm soát'],[
('Đặt trùng phòng','Khách đến nhưng không có phòng.','Kiểm tra lịch trong transaction và chỉ mục duy nhất.'),
('Thu trùng tiền','Chênh lệch quỹ, khiếu nại.','Mã tham chiếu duy nhất, tra cứu trước khi thử lại.'),
('Giao quá số chờ','Sai bill và tồn kho.','Kiểm tra số còn chờ tại thời điểm giao.'),
('Xe thuê bị trừ vĩnh viễn','Báo hết xe giả.','Luồng trả xe và đối chiếu xe đang thuê.'),
('Sửa chứng từ đã khóa','Báo cáo kỳ cũ thay đổi.','Khóa ca/kỳ và điều chỉnh có nhật ký.'),
('Lộ thông tin khách','Ảnh hưởng quyền riêng tư.','Phân quyền, nhật ký và hạn chế xuất dữ liệu.'),
('Không có bản sao lưu','Mất dữ liệu khi lỗi máy chủ.','Lịch sao lưu, lưu ngoài máy và diễn tập khôi phục.'),
])
put('2. CƠ SỞ DỮ LIỆU',a)

b=frag()
h(b,'2.16. Trình tự migration và kiểm tra sau nâng cấp')
p(b,'SchemaMigrator nâng lược đồ theo phiên bản. Khi triển khai lên dữ liệu đang vận hành, không nên sửa trực tiếp các bảng bằng tay rồi giả định migration sẽ tự hiểu trạng thái đó. Một bản nâng cấp an toàn cần sao lưu trước, chạy trên bản sao dữ liệu, ghi thời gian dừng hệ thống dự kiến và kiểm tra lại các ràng buộc sau khi chạy.')
tb(b,['Giai đoạn','Việc thực hiện','Bằng chứng cần lưu'],[
('Trước nâng cấp','Ghi phiên bản hiện tại, sao lưu và thử khôi phục.','Tên bản sao lưu, kết quả phục hồi.'),
('Thử nghiệm','Chạy migration V2–V23 còn thiếu trên bản sao.','Log nâng cấp, lỗi nếu có.'),
('Kiểm tra dữ liệu','So số bản ghi, tổng hóa đơn, tổng cọc, tồn kho.','Bảng đối chiếu trước/sau.'),
('Triển khai','Tạm dừng ghi, sao lưu lần cuối, chạy nâng cấp.','Phiên bản mới trong PhienBanCSDL.'),
('Sau triển khai','Đăng nhập, đặt thử, xem bill, đối soát và báo cáo.','Biên bản kiểm tra nghiệp vụ.'),
])
p(b,'Nếu migration thất bại giữa chừng, cần xem thông báo và trạng thái phiên bản trước khi chạy lại. Việc khôi phục phải dựa trên bản sao lưu đã kiểm thử; không xóa bảng lỗi theo cảm tính vì có thể làm mất dữ liệu lịch sử. Các thay đổi ràng buộc duy nhất phải kiểm tra dữ liệu cũ có trùng hay không trước khi tạo chỉ mục.')
h(b,'2.17. Đối chiếu tiền, doanh thu và công nợ')
p(b,'Ba đại lượng tiền thu, doanh thu và công nợ có thể khác nhau trong cùng một ngày. Tiền cọc làm tăng tiền quỹ trước khi hóa đơn được lập. Doanh thu được ghi nhận khi có hóa đơn hợp lệ theo kỳ báo cáo. Công nợ phát sinh khi hóa đơn chưa được thu đủ, ví dụ kênh OTA thanh toán sau. Một báo cáo quản trị cần hiển thị tên chỉ số và nguồn dữ liệu để người xem không cộng lẫn ba loại.')
tb(b,['Tình huống','Dòng tiền','Doanh thu / công nợ'],[
('Khách đặt trước và cọc','Có thu cọc.','Chưa coi toàn bộ tiền phòng là doanh thu.'),
('Khách trả phòng, trả đủ','Có thu phần còn lại.','Hóa đơn tạo doanh thu theo hạng mục.'),
('Khách trả phòng, OTA nợ','Chưa thu hết tiền.','Có doanh thu và khoản phải thu.'),
('Hoàn cọc vì hủy','Có dòng tiền ra.','Xử lý hoàn/khấu trừ theo chính sách hủy.'),
('Hủy/điều chỉnh hóa đơn','Có thể cần hoàn/thu bổ sung.','Doanh thu điều chỉnh và lưu chứng từ gốc.'),
])
p(b,'Đối chiếu cuối kỳ nên so: tổng hóa đơn hợp lệ, tổng dòng bill, tổng giao dịch thanh toán theo kênh, số công nợ đầu/cuối kỳ và tổng sao kê đã khớp. Chênh lệch cần có danh sách chứng từ cụ thể, không chỉ một con số tổng. Đây là cơ sở để đánh giá lãi lỗ và hao hụt sau này.')
h(b,'2.18. Kiểm kê kho và giá vốn bình quân')
p(b,'Mặt hàng dịch vụ có thể nhập nhiều đợt với giá mua khác nhau. Giá vốn bình quân giúp ước lượng chi phí xuất kho của hàng tiêu hao; số tồn và giá vốn cần được cập nhật theo biến động thực nhập, thực xuất. Mục “Bán trên bill” dùng đối chiếu với lượng xuất: nếu xuất nhiều hơn bán, có thể là tiêu dùng nội bộ, hư hỏng hoặc sai thao tác. Cần ghi lý do thay vì tự động kết luận thất thoát.')
tb(b,['Biến động','Ảnh hưởng tồn','Ảnh hưởng giá vốn / kiểm soát'],[
('Nhập kho 100 đơn vị','Tăng 100.','Cập nhật giá vốn bình quân theo giá nhập.'),
('Giao hàng tiêu hao 3 đơn vị','Giảm 3.','Ghi giá vốn xuất cho dịch vụ đã giao.'),
('Hủy phần chưa giao','Không giảm thêm.','Không ghi giá vốn của hàng chưa giao.'),
('Cho thuê 1 xe','Giảm khả dụng tạm thời.','Không coi xe là chi phí tiêu hao.'),
('Trả 1 xe','Tăng khả dụng.','Đối chiếu tình trạng xe và biên nhận.'),
('Kiểm kê thiếu 2 đơn vị','Điều chỉnh sau xác minh.','Lưu lý do, người duyệt và chứng từ.'),
])
p(b,'Khi nhập số lượng có dấu phân tách như 11,000, giao diện phải định nghĩa rõ đó là mười một nghìn hay số thập phân theo vùng ngôn ngữ. Với đơn vị hàng nguyên, nhập 11.000 cần được diễn giải thành 11.000 đơn vị và hiển thị lại trước khi lưu. Nên dùng điều khiển số có định dạng vùng nhất quán để tránh nhập một số nhưng hệ thống ghi số khác.')
h(b,'2.19. Chính sách lưu giữ và bảo vệ dữ liệu')
p(b,'Cơ sở dữ liệu chứa tên khách, số điện thoại, giấy tờ và lịch sử lưu trú, nên việc bảo vệ dữ liệu phải được thiết kế cùng nghiệp vụ. Tài khoản nhân viên chỉ xem thông tin cần thiết cho công việc; tài khoản SQL của ứng dụng không nên dùng quyền quản trị máy chủ. Bản sao lưu cần được kiểm soát truy cập và mã hóa theo chính sách của đơn vị triển khai.')
tb(b,['Lớp bảo vệ','Biện pháp đề xuất','Cách kiểm tra'],[
('Ứng dụng','Vai trò và quyền chức năng; tự khóa phiên khi cần.','Đăng nhập thử từng vai trò.'),
('Cơ sở dữ liệu','Tài khoản riêng, giới hạn quyền, kết nối an toàn.','Rà quyền SQL và cấu hình kết nối.'),
('Nhật ký','Lưu ai làm gì, khi nào, trên đối tượng nào.','Tra thao tác mẫu trong NhatKyThaoTac.'),
('Sao lưu','Lưu nhiều phiên bản, bản sao tách khỏi máy chạy.','Thử phục hồi định kỳ.'),
('Xuất dữ liệu','Giới hạn danh sách khách/hóa đơn theo quyền.','Kiểm tra file xuất và lịch sử xuất.'),
('Xóa lưu trữ','Quy định thời hạn, không xóa chứng từ tài chính tùy tiện.','Đối chiếu chính sách lưu giữ.'),
])
p(b,'Báo cáo không xác nhận dự án đã có đầy đủ các biện pháp triển khai hạ tầng nêu trên. Đây là các yêu cầu vận hành cần được đơn vị sử dụng phê duyệt và kiểm tra tại môi trường thật. Đặc biệt, sự tồn tại của chức năng đổi mật khẩu không thay thế cho chính sách sao lưu, phân quyền cơ sở dữ liệu và kiểm soát thiết bị đầu cuối.')
put('3. GIAO DIỆN MODULE VÀ CHỨC NĂNG',b)

c=frag()
h(c,'3.22. Đặc tả tương tác và trạng thái màn hình')
p(c,'Mỗi màn hình nghiệp vụ nên có bốn trạng thái rõ ràng: đang tải, có dữ liệu, không có dữ liệu và lỗi. Ví dụ bộ lọc dịch vụ “Chờ giao” có thể trả về rỗng vì mọi yêu cầu đã được xử lý; khi đó cần hiện thông điệp “Không có yêu cầu đang chờ” thay vì một bảng trắng không giải thích. Trong lúc gửi lệnh thanh toán, nút xác nhận phải tạm khóa để tránh bấm hai lần.')
tb(c,['Màn hình','Không có dữ liệu','Đang xử lý / lỗi'],[
('Sơ đồ phòng','Nêu không có phòng phù hợp với ngày/loại.','Giữ bộ lọc, đề nghị đổi ngày khi tải lỗi.'),
('Yêu cầu dịch vụ','Nêu không có yêu cầu chờ.','Khóa nút giao, giữ số lượng đã nhập nếu lỗi.'),
('Kho','Nêu chưa có mặt hàng liên kết.','Không hiển thị tồn cũ như tồn mới khi tải lỗi.'),
('Thanh toán','Nêu chưa chọn phòng hoặc bill chưa hợp lệ.','Chặn bấm lặp, tra mã tham chiếu sau lỗi mạng.'),
('Doanh thu','Nêu kỳ chọn chưa có hóa đơn.','Giữ bộ lọc kỳ và nêu lỗi nguồn dữ liệu.'),
])
p(c,'Sau thao tác thành công, màn hình cần tải lại đúng vùng dữ liệu đã đổi và hiển thị kết quả đã lưu. Hộp thoại xác nhận chỉ nên dùng cho hành động có tác động đáng kể như hủy đặt, hủy hóa đơn hoặc chốt ca; dùng quá nhiều hộp thoại cho thao tác thông thường sẽ làm chậm quy trình tại quầy.')
h(c,'3.23. Quy tắc nhập liệu và thông báo')
p(c,'Một trường nhập tốt cần có nhãn rõ, đơn vị đo, giá trị mặc định hợp lý và kiểm tra tại chỗ. Số ngày ở không nên lẫn với số lượng phòng; số xe thuê không nên lẫn với số giờ thuê. Với số tiền và số lượng, định dạng hiển thị có dấu phân tách hàng nghìn nhưng giá trị lưu phải nhất quán. Mã giao dịch chỉ nên bắt buộc với kênh cần đối soát; trường ghi chú cần khác mã tham chiếu nếu nghiệp vụ sử dụng hai ý nghĩa.')
tb(c,['Trường','Kiểm tra','Cách hiển thị lỗi'],[
('Ngày đến/đi','Ngày đi sau ngày đến; khoảng ngày hợp lệ.','Hiển thị ngay dưới bộ chọn ngày.'),
('Số ngày','Số nguyên dương trong giới hạn nghiệp vụ.','Nêu giới hạn thay vì chỉ đổi màu viền.'),
('Số lượng dịch vụ','Không nhỏ hơn đã giao, giao thêm không vượt chờ.','Nêu đã giao/còn chờ và mức tối đa.'),
('Số lượng nhập kho','Đơn vị nguyên hoặc thập phân đúng quy định.','Hiển thị lại giá trị chuẩn hóa trước lưu.'),
('Tiền cọc','Không âm, theo quy tắc cọc của phòng.','Nêu mức tối đa/tối thiểu áp dụng.'),
('Mã tham chiếu','Không trùng trong phạm vi kênh.','Đề nghị xem giao dịch cũ.'),
])
p(c,'Thông báo thành công phải gắn đối tượng: “Đã nhận phòng 103 cho khách…” hữu ích hơn “Hoàn tất”. Khi có nhiều phòng, cần liệt kê các phòng thành công hoặc nếu giao dịch được thiết kế toàn bộ hoặc không gì cả, nêu rõ không phòng nào được ghi khi một phòng bị xung đột.')
h(c,'3.24. Khả năng đọc và bố cục ở các độ phân giải')
p(c,'Ứng dụng WinForms thường chạy trên màn hình quầy có kích cỡ khác nhau. Bảng dài phải có tiêu đề cột đầy đủ, có thể cuộn và giữ vùng thao tác chính trong tầm nhìn. Cột số tiền căn phải, cột tên và trạng thái dễ quét bằng mắt. Nút hành động phụ không nên bị thu nhỏ đến mức khó đọc; nếu dùng menu thả xuống, chiều rộng và cỡ chữ phải tương xứng với các mục bên trong.')
tb(c,['Vùng','Ưu tiên bố cục','Kiểm tra thực tế'],[
('Sơ đồ phòng','Ô phòng đủ rộng cho số, giá và trạng thái.','Thử ở màn 1366×768 và 1920×1080.'),
('Bảng dịch vụ','Cột phòng, dịch vụ, gọi/giao/chờ không bị cắt.','Thử nhiều dòng và tên dịch vụ dài.'),
('Thanh toán','Tổng cần thu và xác nhận luôn nhìn thấy.','Thử bill dài và nhiều phòng.'),
('Kho','Đơn vị, tồn, ngưỡng, giá vốn dễ phân biệt.','Thử số lượng/giá lớn.'),
('Biểu đồ','Có chú giải, đơn vị, khoảng thời gian.','Thử kỳ không có dữ liệu và nhiều nhóm.'),
])
p(c,'Màu trạng thái cần có độ tương phản đủ với chữ. Các trạng thái Trống, Đang ở, Đang dọn, Bảo trì và Có lịch đặt nên giữ cùng màu trong toàn bộ ứng dụng; không dùng cùng một màu cho hai ý nghĩa khác nhau. Ngoài màu, nhãn trạng thái phải còn hiển thị khi in ảnh hoặc sử dụng màn hình chất lượng thấp.')
h(c,'3.25. Mẫu kịch bản nghiệm thu theo vai trò')
p(c,'Nghiệm thu cần được thực hiện bởi người dùng đại diện cho từng vai trò chứ không chỉ bằng tài khoản quản trị. Cùng một giao diện có thể hiển thị và cho phép hành động khác nhau tùy quyền. Biên bản nghiệm thu nên ghi phiên bản ứng dụng, phiên bản lược đồ, dữ liệu thử, người kiểm tra và kết quả từng bước.')
tb(c,['Vai trò','Kịch bản chính','Điều kiện chấp nhận'],[
('Reception','Đặt 2 phòng, nhận phòng, gọi dịch vụ, trả phòng.','Thông tin khách/phòng/bill đúng; không thấy chức năng quản trị.'),
('Accountant','Đối soát tiền, kiểm tra công nợ, nhập kho.','Số tiền và tồn khớp chứng từ; không thể nhận phòng nếu không có quyền.'),
('Manager','Xem sơ đồ, danh mục, doanh thu và lãi lỗ.','Bộ lọc và tổng báo cáo đúng; quyền chỉnh phù hợp.'),
('Admin','Cấp quyền, sửa danh mục, xem nhật ký.','Quyền áp dụng sau khi đăng nhập lại; nhật ký có thao tác.'),
])
p(c,'Một phiên nghiệm thu cũng cần kiểm tra các tình huống lỗi: SQL Server tạm ngắt, mã tham chiếu trùng, phòng vừa được tài khoản khác đặt, kho vừa hết và ca vừa bị khóa. Mục tiêu là xác nhận hệ thống thông báo rõ và không ghi dữ liệu dở, không chỉ kiểm tra giao diện khi mọi thứ diễn ra thuận lợi.')
if False:add(c)
d.save(f);print(f)
