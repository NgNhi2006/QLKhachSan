from copy import deepcopy
from pathlib import Path
from docx import Document
from docx.shared import Pt, Cm, RGBColor
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.enum.table import WD_CELL_VERTICAL_ALIGNMENT
from docx.oxml import OxmlElement
from docx.oxml.ns import qn

ROOT=Path(__file__).resolve().parents[1]
PATH=ROOT/'docs'/'Bao_cao_he_thong_quan_ly_khach_san.docx'
doc=Document(PATH)
doc.sections[0].header.paragraphs[0].clear()
foot=doc.sections[0].footer.paragraphs[0]; foot.clear(); foot.alignment=WD_ALIGN_PARAGRAPH.CENTER
run=foot.add_run(); b=OxmlElement('w:fldChar');b.set(qn('w:fldCharType'),'begin'); i=OxmlElement('w:instrText');i.set(qn('xml:space'),'preserve');i.text='PAGE';e=OxmlElement('w:fldChar');e.set(qn('w:fldCharType'),'end');run._r.extend([b,i,e])

def fragment(): return Document()
def h(d,s,l=2): d.add_heading(s,l)
def p(d,s): d.add_paragraph(s)
def bullets(d,items):
    for x in items:d.add_paragraph(x,'List Bullet')
def numbers(d,items):
    for x in items:d.add_paragraph(x,'List Number')
def t(d,heads,rows):
    z=d.add_table(rows=1,cols=len(heads));z.autofit=False
    for j,v in enumerate(heads):
        c=z.rows[0].cells[j];c.text=v
        sh=OxmlElement('w:shd');sh.set(qn('w:fill'),'173F38');c._tc.get_or_add_tcPr().append(sh)
        for r in c.paragraphs[0].runs:r.font.bold=True;r.font.color.rgb=RGBColor(255,255,255);r.font.size=Pt(8.5)
    for k,row in enumerate(rows):
        cc=z.add_row().cells
        for j,v in enumerate(row):
            cc[j].text=str(v);cc[j].vertical_alignment=WD_CELL_VERTICAL_ALIGNMENT.CENTER
            if k%2:
                sh=OxmlElement('w:shd');sh.set(qn('w:fill'),'F3F6F4');cc[j]._tc.get_or_add_tcPr().append(sh)
            for par in cc[j].paragraphs:
                par.paragraph_format.space_after=Pt(1)
                for r in par.runs:r.font.size=Pt(8.5)
    d.add_paragraph()
def insert_before(target,fragment):
    anchor=next(x for x in doc.paragraphs if x.style.name=='Heading 1' and x.text.startswith(target))._p
    for child in list(fragment.element.body):
        if child.tag!=qn('w:sectPr'): anchor.addprevious(deepcopy(child))
def append(fragment):
    for child in list(fragment.element.body):
        if child.tag!=qn('w:sectPr'): doc.element.body.insert(len(doc.element.body)-1,deepcopy(child))

a=fragment()
h(a,'1.8. Danh mục yêu cầu nghiệp vụ',2)
p(a,'Yêu cầu được nhóm theo công việc của lễ tân, buồng phòng, dịch vụ và kế toán. Cách phân nhóm này cho phép kiểm tra một chức năng từ giao diện đến dữ liệu phát sinh, đồng thời tránh hiểu sai rằng cùng một nút bấm là toàn bộ quy trình.')
t(a,['Mã','Yêu cầu','Đầu vào / kết quả mong đợi'],[
('NV-01','Tìm phòng khả dụng','Ngày đến, số ngày, loại phòng → danh sách và trạng thái phòng phù hợp.'),
('NV-02','Đặt phòng trước','Khách, phòng, thời gian, cọc → lượt Reserved và lịch giữ phòng.'),
('NV-03','Nhận phòng ngay','Khách, phòng, thời gian → lượt Occupied và phòng đang ở.'),
('NV-04','Nhận phòng đã đặt','Lượt Reserved hợp lệ → Occupied, ghi thời điểm nhận thực tế.'),
('NV-05','Điều chỉnh lưu trú','Sửa đặt, chuyển phòng, gia hạn → lưu trạng thái mới và lịch sử chặng.'),
('NV-06','Gọi và giao dịch vụ','Lượt đang ở, mặt hàng, số lượng → yêu cầu chờ, số đã giao, bill.'),
('NV-07','Thanh toán','Bill, cọc, phương thức → hóa đơn, giao dịch và trạng thái Paid.'),
('NV-08','Quản trị kho','Nhập/xuất/trả tài sản → tồn khả dụng và biến động có thể đối chiếu.'),
('NV-09','Đối soát tài chính','Ca trực, phiếu, sao kê, công nợ → số liệu được chốt và giải trình.'),
('NV-10','Báo cáo quản trị','Kỳ xem, nhóm doanh thu → tổng, cơ cấu, xu hướng và lãi lỗ.'),
])
h(a,'1.9. Quy trình đặt phòng trước và nhận phòng',2)
p(a,'Trước khi tạo lượt, nhân viên chọn ngày đến, thời lượng và phòng trên sơ đồ. Hệ thống kết hợp trạng thái hiện tại với các lượt đặt có hiệu lực để xác định khả dụng cho cả khoảng ngày, thay vì chỉ nhìn màu của phòng ở thời điểm hiện tại. Với nhiều phòng, cùng một hồ sơ khách có thể gắn với nhiều lượt để vẫn theo dõi và thanh toán riêng từng phòng.')
numbers(a,[
'Xác định hình thức nhận ngay hoặc đặt trước; lọc phòng theo loại và ngày.',
'Chọn một hoặc nhiều phòng, xem giá/ngày và tiền cọc gợi ý từng phòng.',
'Nhập họ tên, số điện thoại, giấy tờ và dữ liệu liên hệ; kiểm tra giá trị bắt buộc.',
'Kiểm tra chồng lịch, phòng bảo trì, số ngày và cọc; nếu không hợp lệ, giữ nguyên màn hình để sửa.',
'Ghi lượt, cập nhật lịch/trạng thái và giao dịch cọc trong một giao dịch dữ liệu.',
'Hiển thị thông báo hoàn tất với đúng thông tin vừa nhập và số phòng đã xử lý.'
])
p(a,'Đối với đặt trước, thời hạn giữ chỗ phụ thuộc tình trạng cọc. Khi khách đến, thao tác nhận phòng phải kiểm tra lại lượt đặt còn hiệu lực. Khi khách không đến hoặc hủy, nghiệp vụ hoàn/khấu trừ cọc cần phản ánh trong giao dịch thanh toán để số dư quỹ đúng với thực tế.')
h(a,'1.10. Quy trình ở thực tế, chuyển phòng và gia hạn',2)
p(a,'Một lượt đang ở có thể phát sinh nhiều biến động: thêm ngày, đổi phòng, thay đổi đơn giá, nhận dịch vụ hoặc thu thêm cọc. Chặng lưu trú lưu mốc bắt đầu/kết thúc và phòng tương ứng, tạo căn cứ tính tiền phòng khi khách trả. Nhờ đó việc chuyển phòng không làm mất lịch sử phòng đã ở, và hóa đơn có thể trình bày số ngày theo từng giai đoạn.')
t(a,['Tình huống','Kiểm tra trước thao tác','Dữ liệu cần ghi'],[
('Gia hạn','Phòng còn khả dụng trong thời gian tăng thêm; ngày mới hợp lệ.','Ngày đi dự kiến, số ngày/giá liên quan và nhật ký.'),
('Chuyển phòng','Phòng đích trống và không chồng lịch; lượt nguồn đang ở.','Đóng chặng cũ, mở chặng mới, cập nhật phòng đang ở.'),
('Thu cọc thêm','Lượt hợp lệ; số thu dương; tham chiếu không trùng.','Giao dịch cọc và số cọc tích lũy.'),
('Sửa đặt trước','Lượt còn ở trạng thái cho phép; không tạo chồng lịch.','Thời gian/phòng/khách mới cùng dấu vết chỉnh sửa.'),
('Hủy đặt','Lượt chưa thanh toán; xác định tiền cần hoàn/khấu trừ.','Trạng thái Cancelled, giao dịch tiền và lý do.'),
])
h(a,'1.11. Quy trình gọi dịch vụ và trách nhiệm giao hàng',2)
p(a,'Phiếu dịch vụ phân biệt số lượng đã gọi, đã giao và còn chờ. Số đã giao mới phản ánh thực tế cung cấp; sửa tổng số lượng không được nhỏ hơn số đã giao. Bộ phận xử lý cần nhìn trực tiếp danh sách yêu cầu chờ theo phòng để không phải mở từng phòng dò tìm. Tìm kiếm theo số phòng, khách hoặc tên dịch vụ giúp xử lý khi số yêu cầu lớn.')
numbers(a,[
'Lễ tân hoặc nhân viên được cấp quyền chọn lượt đang ở và dịch vụ trong danh mục.',
'Hệ thống kiểm tra loại dịch vụ, đơn vị tính, giá và khả dụng kho nếu dịch vụ liên kết mặt hàng.',
'Yêu cầu được tạo với số gọi và số còn chờ; danh sách xử lý hiển thị phòng, khách và trạng thái.',
'Nhân viên giao toàn bộ hoặc một phần; hệ thống cập nhật số giao và biến động tồn theo loại hàng.',
'Nếu khách đổi ý, chỉ phần chưa giao được hủy; thao tác có lý do và để lại lịch sử.',
'Dòng dịch vụ đã cung cấp được đưa vào báo giá/bill theo phòng liên quan.'
])
p(a,'Với cho thuê xe, đơn vị tồn là xe hoặc chiếc. Khi giao xe, số khả dụng giảm tạm thời; khi có biên nhận trả xe thì tăng lại. Chi phí thuê 24 giờ là đơn giá dịch vụ, không phải số giờ tồn và cũng không làm hao hụt vĩnh viễn một xe.')
h(a,'1.12. Quy trình lập bill, thanh toán và hoàn tiền',2)
p(a,'Bill tạm tính là bản xem trước tại một thời điểm. Tiền phòng cần hiển thị số ngày và giá tương ứng, có thể có nhiều dòng khi đổi phòng hoặc giá. Các dịch vụ phải gắn đúng phòng. Tổng bill của nhóm nhiều phòng là tổng các bill phòng đã chọn; tiền cọc và số cần thu cũng được cộng theo cùng tập phòng. Nhân viên phải có khả năng kiểm tra và sửa từng bill trước khi xác nhận chung.')
t(a,['Bước','Kiểm soát nghiệp vụ','Kết quả'],[
('Báo giá','Tính tiền phòng theo chặng/ngày; lấy dịch vụ và điều chỉnh hợp lệ.','Bill tạm tính theo từng phòng.'),
('Đối chiếu cọc','Cộng giao dịch cọc còn hiệu lực, tránh cộng trùng.','Số cọc được trừ vào bill.'),
('Xác định số phải thu','Tổng hóa đơn trừ cọc; nếu âm thì là khoản cần hoàn.','Số cần thu hoặc hoàn hiển thị rõ.'),
('Chọn phương thức','Tiền mặt, POS, chuyển khoản hoặc công nợ OTA.','Kênh tiền và mã tham chiếu để đối soát.'),
('Xác nhận','Khóa bản báo giá đang thanh toán, kiểm tra lại dữ liệu gốc.','Hóa đơn, dòng bill, giao dịch và trạng thái lưu trú.'),
('Sau trả phòng','Phòng cần được dọn hoặc chuyển trạng thái theo quy trình.','Sơ đồ phòng và lịch sử phản ánh kết quả.'),
])
h(a,'1.13. Quản lý ca trực và đối soát',2)
p(a,'Ca trực tạo phạm vi trách nhiệm cho khoản tiền phát sinh trong một khoảng thời gian. Nhân viên mở ca với số đầu ca, ghi nhận các giao dịch thuộc ca, lập bàn giao và chốt ca. Kế toán đối chiếu giao dịch trong ứng dụng với tiền mặt thực đếm và sao kê ngân hàng. Khi ca đã khóa, thay đổi chứng từ cũ phải qua quy trình điều chỉnh có dấu vết thay vì ghi đè.')
bullets(a,[
'Sổ quỹ thể hiện số dư đầu kỳ, phiếu thu, phiếu chi, hoàn tiền và số dư cuối kỳ.',
'Đối soát ngân hàng so khớp số tiền, ngày giao dịch, kênh và mã tham chiếu; dòng chưa khớp phải nằm trong danh sách xử lý.',
'Công nợ theo dõi khoản phải thu chưa thanh toán và các lần phân bổ thanh toán về hóa đơn.',
'Khóa kỳ kế toán bảo vệ số liệu đã lập báo cáo; nghiệp vụ phát hiện chứng từ thuộc kỳ khóa phải báo rõ nguyên nhân.'
])
h(a,'1.14. Phân quyền và nhật ký thao tác',2)
p(a,'Quyền trong hệ thống có hai chiều: vai trò nhân viên và mã chức năng. Vai trò xác định tập quyền mặc định; quản trị viên có thể cấp quyền chi tiết theo chức năng. Mã chức năng được dùng tại cả điều hướng và lớp nghiệp vụ, giảm rủi ro người dùng gọi được phương thức không có trên giao diện. Nhật ký là bằng chứng cho các thao tác quan trọng như thay đổi đặt phòng, hóa đơn, kho, cọc và tài khoản.')
t(a,['Nguyên tắc','Ứng dụng vào dự án'],[
('Quyền tối thiểu','Lễ tân thao tác đặt/trả phòng; kế toán đối soát; quản trị quản lý tài khoản.'),
('Ẩn và chặn','Menu lọc theo quyền; dịch vụ nghiệp vụ vẫn kiểm tra trước khi truy cập dữ liệu.'),
('Truy vết','Lưu người thực hiện, thời điểm, đối tượng và nội dung thao tác khi phù hợp.'),
('Khóa dữ liệu','Ca/kỳ đã chốt ngăn thay đổi trực tiếp chứng từ tài chính.'),
('Bảo vệ thông tin khách','Giới hạn người xem giấy tờ, số điện thoại và lịch sử lưu trú theo nhu cầu công việc.'),
])
h(a,'1.15. Chỉ số vận hành cần theo dõi',2)
p(a,'Ngoài tổng doanh thu, người quản lý cần các chỉ số đủ để giải thích biến động. Một số chỉ số có thể tính trực tiếp từ dữ liệu hiện có; một số cần thống nhất công thức và kỳ ghi nhận trước khi dùng để đánh giá hiệu quả. Các chỉ số đề xuất dưới đây không thay thế báo cáo tài chính chuẩn mực.')
t(a,['Chỉ số','Công thức/nguồn gợi ý','Ý nghĩa'],[
('Công suất phòng','Đêm phòng đã bán / đêm phòng có thể bán.','Mức sử dụng công suất.'),
('Giá phòng bình quân','Doanh thu phòng / đêm phòng đã bán.','Chất lượng giá bán.'),
('Doanh thu dịch vụ','Tổng dòng bill dịch vụ theo kỳ.','Đóng góp của dịch vụ ngoài phòng.'),
('Số yêu cầu chờ','Tổng phần gọi trừ phần giao/hủy.','Khối lượng dịch vụ chưa hoàn thành.'),
('Tỷ lệ phòng cần dọn','Số phòng đang dọn / số phòng vận hành.','Tải công việc buồng phòng.'),
('Tỷ lệ đối soát khớp','Dòng thanh toán khớp sao kê / tổng dòng cần đối soát.','Chất lượng ghi nhận tiền.'),
('Giá trị tồn kho','Tồn × giá vốn bình quân theo mặt hàng.','Nguồn lực đang nằm trong kho.'),
])
insert_before('2. CƠ SỞ DỮ LIỆU',a)

b=fragment()
h(b,'2.7. Danh mục 48 bảng theo miền dữ liệu',2)
p(b,'Bảng dưới đây là danh mục đối chiếu từ các câu lệnh CREATE TABLE trong Database.sql. Một số bảng phục vụ cấu hình và tương thích lịch sử, do đó không nên xem mỗi bảng là một màn hình độc lập. Khi bảo trì, cần tra cứu migration đã áp dụng và quan hệ khóa ngoại trước khi sửa lược đồ.')
groups=[
('Nền tảng và người dùng','PhienBanCSDL','Lưu phiên bản migration đã áp dụng.'),('Nền tảng và người dùng','TaiKhoanNhanVien','Tài khoản, vai trò và trạng thái người dùng.'),('Nền tảng và người dùng','NhatKyThaoTac','Truy vết tác động vào hệ thống.'),
('Phòng và lưu trú','Phong','Danh mục và trạng thái phòng.'),('Phòng và lưu trú','KhachHang','Hồ sơ định danh khách.'),('Phòng và lưu trú','LuotLuuTru','Đặt trước, đang ở, đã trả, đã hủy.'),('Phòng và lưu trú','ChangLuuTru','Các chặng phòng/giá trong một lượt.'),
('Dịch vụ và kho','DichVu','Danh mục dịch vụ và đơn giá.'),('Dịch vụ và kho','YeuCauDichVu','Số gọi, giao, hủy dịch vụ.'),('Dịch vụ và kho','HangTonKho','Mặt hàng, tồn, giá vốn, ngưỡng.'),('Dịch vụ và kho','BienDongKho','Nhật ký nhập và xuất kho.'),('Dịch vụ và kho','TieuThuBuongPhong','Ghi nhận buồng phòng báo dùng.'),('Dịch vụ và kho','TraXeThue','Biên nhận trả xe cho thuê.'),
('Hóa đơn và tiền','HoaDon','Hóa đơn kết toán lượt lưu trú.'),('Hóa đơn và tiền','ChiTietBill','Dòng tiền phòng, dịch vụ, điều chỉnh.'),('Hóa đơn và tiền','GiaoDichThanhToan','Cọc, thu, hoàn và khấu trừ.'),('Hóa đơn và tiền','HoaDonHuy','Thông tin hủy hóa đơn.'),('Hóa đơn và tiền','DieuChinhHoaDon','Điều chỉnh sau phát hành.'),('Hóa đơn và tiền','ThongTinTaiChinhHoaDon','Dữ liệu tài chính bổ sung cho hóa đơn.'),('Hóa đơn và tiền','NhomHoaDon','Nhóm bill liên quan.'),('Hóa đơn và tiền','PhanChiaHoaDon','Phân bổ/chia hóa đơn.'),('Hóa đơn và tiền','MaGiamGia','Mã và quy tắc giảm giá.'),('Hóa đơn và tiền','SuDungMaGiamGia','Lịch sử áp dụng mã giảm giá.'),
('Tài chính','CaTruc','Ca thu ngân và bàn giao.'),('Tài chính','KyKeToan','Kỳ và trạng thái khóa.'),('Tài chính','PhieuThuChi','Chứng từ thu chi.'),('Tài chính','SoDuDauKy','Số dư mở kỳ.'),('Tài chính','DongSaoKe','Giao dịch sao kê ngân hàng.'),('Tài chính','CongNo','Khoản phải thu.'),('Tài chính','PhanBoCongNo','Phân bổ tiền về công nợ.'),
('Điều hướng và quyền','MenuTong','Nhóm menu cấp cao.'),('Điều hướng và quyền','MenuCon','Menu nhánh.'),('Điều hướng và quyền','ChucNang','Danh mục mã chức năng.'),('Điều hướng và quyền','PhanQuyenNhanVien','Quyền cấp riêng cho nhân viên.'),
('Danh mục chức năng','ChucNangQuanLyPhong','Chức năng thuộc quản lý phòng.'),('Danh mục chức năng','ChucNangDichVu','Chức năng thuộc dịch vụ.'),('Danh mục chức năng','ChucNangKhachHang','Chức năng thuộc khách hàng.'),('Danh mục chức năng','ChucNangCaTruc','Chức năng thuộc ca trực.'),('Danh mục chức năng','ChucNangThuChi','Chức năng thuộc thu chi.'),('Danh mục chức năng','ChucNangHoaDon','Chức năng thuộc hóa đơn.'),('Danh mục chức năng','ChucNangBaoCao','Chức năng thuộc báo cáo.'),('Danh mục chức năng','ChucNangNhanVien','Chức năng thuộc nhân viên.'),('Danh mục chức năng','ChucNangHeThong','Chức năng thuộc hệ thống.'),
('Cấu hình và mở rộng','CauHinhMenu','Cấu hình menu tổng.'),('Cấu hình và mở rộng','CauHinhMenuCon','Cấu hình menu con.'),('Cấu hình và mở rộng','CauHinhChucNang','Cấu hình chức năng.'),('Cấu hình và mở rộng','CauHinhChucNangMoi','Định nghĩa chức năng tùy biến.'),('Cấu hình và mở rộng','DuLieuChucNangMoi','Dữ liệu chức năng tùy biến.')]
t(b,['Miền','Bảng','Vai trò'],groups)
h(b,'2.8. Khóa, chỉ mục và chống dữ liệu trùng',2)
p(b,'SQL Server sử dụng khóa chính cho từng bảng, khóa ngoại để bảo toàn tham chiếu và chỉ mục duy nhất có điều kiện cho các quy tắc đặc thù. Chỉ mục có điều kiện giúp cho phép lưu lịch sử nhưng chặn trạng thái hoạt động trùng nhau. Ví dụ một phòng có thể có nhiều lượt trong quá khứ, nhưng không được có hai lượt cùng trạng thái đang ở.')
t(b,['Chỉ mục/điều kiện','Quy tắc được bảo vệ','Lỗi cần hiển thị cho người dùng'],[
('UX_Stays_ActiveRoom','Một lượt còn hiệu lực trên mỗi phòng theo điều kiện chỉ mục.','Phòng đã có lượt còn hiệu lực.'),
('UX_Stays_OccupiedRoom','Một phòng không có hai lượt Occupied.','Phòng đang có khách.'),
('UX_Segments_Open','Một lượt chỉ có một chặng chưa kết thúc.','Lượt có chặng mở cần xử lý.'),
('UX_CashShifts_Open','Một thu ngân chỉ có một ca Open.','Cần chốt ca hiện tại trước.'),
('UX_Payments_ExternalReference','Mã tham chiếu ngoài không trùng trong cùng phương thức.','Giao dịch đã được ghi nhận.'),
('UX_Users_ActiveUsername','Tên đăng nhập còn hoạt động không trùng.','Tên tài khoản đã tồn tại.'),
])
h(b,'2.9. Giao dịch đồng thời và toàn vẹn',2)
p(b,'Các thao tác như đặt nhiều phòng, nhận cọc, trả phòng và điều chỉnh kho thường cập nhật hơn một bảng. Nếu chỉ ghi từng bảng riêng lẻ, lỗi giữa chừng sẽ tạo ra lượt ở mà thiếu giao dịch hoặc hóa đơn mà phòng chưa được giải phóng. HotelRepository.RunAsync khởi tạo transaction với mức cô lập Serializable, dùng sp_getapplock khi cần và rollback nếu phát sinh lỗi. Đây là lớp bảo vệ bên cạnh ràng buộc SQL.')
numbers(b,[
'Đọc dữ liệu hiện tại trong transaction để tránh quyết định trên trạng thái đã cũ.',
'Áp dụng kiểm tra nghiệp vụ và quyền người dùng trước thao tác ghi.',
'Ghi các bảng liên quan cùng một transaction; chỉ commit khi toàn bộ lệnh thành công.',
'Nếu lỗi khóa, ràng buộc hoặc kiểm tra nghiệp vụ, rollback và trả thông báo có thể hiểu được.',
'Tải lại dữ liệu giao diện sau commit để trạng thái phòng, bill và tồn kho đồng bộ.'
])
h(b,'2.10. Từ điển dữ liệu chi tiết: phòng và lưu trú',2)
t(b,['Thực thể','Nhóm thuộc tính','Ý nghĩa và kiểm tra'],[
('Phong','Mã phòng, số/mã hiển thị','Định danh nội bộ và số phòng lễ tân nhìn thấy; mã hiển thị cần duy nhất.'),
('Phong','Loại, giá mỗi ngày, cọc gợi ý','Cơ sở lọc, báo giá và chính sách thu cọc.'),
('Phong','Trạng thái, phiên bản','Hiển thị vận hành và kiểm tra cập nhật đồng thời.'),
('KhachHang','Họ tên, điện thoại, giấy tờ','Định danh, liên lạc và tránh tạo hồ sơ sai.'),
('LuotLuuTru','Phòng, khách, thời điểm đến/đi','Gắn người ở với tài nguyên và khoảng lưu trú.'),
('LuotLuuTru','Trạng thái, cọc, hiệu lực','Phân biệt đặt, đang ở, đã trả, đã hủy; kiểm tra lịch.'),
('ChangLuuTru','Mốc mở/đóng, phòng, đơn giá','Tính tiền theo từng chặng khi chuyển phòng hoặc đổi giá.'),
])
p(b,'Một thay đổi ngày đi hoặc phòng cần được phản ánh đồng thời ở lịch khả dụng và chặng lưu trú. Để báo giá có thể kiểm chứng, hệ thống phải giữ thông tin giá đã áp dụng trong lịch sử thay vì đọc duy nhất giá danh mục hiện hành.')
h(b,'2.11. Từ điển dữ liệu chi tiết: dịch vụ và tồn kho',2)
t(b,['Thực thể','Thông tin chính','Quy tắc đối chiếu'],[
('DichVu','Tên, nhóm, đơn vị, giá','Giá và đơn vị tại thời điểm gọi cần lưu vào yêu cầu/bill.'),
('YeuCauDichVu','Lượt, số gọi, số giao, số hủy','Còn chờ = gọi − giao − hủy; kết quả không âm.'),
('HangTonKho','Dịch vụ liên kết, tồn, giá vốn, ngưỡng','Cảnh báo theo ngưỡng; hàng tiêu hao không bán vượt khả dụng.'),
('BienDongKho','Loại nhập/xuất, số lượng, giá trị, thời điểm','Căn cứ truy xuất nguyên nhân thay đổi tồn.'),
('TieuThuBuongPhong','Phòng, mặt hàng, số dùng','Đối soát minibar do buồng phòng báo.'),
('TraXeThue','Dịch vụ xe, lượt, số trả','Khôi phục số xe khả dụng khi khách trả.'),
])
p(b,'Tồn kho không nên được suy ra bằng cách cộng số lần gọi dịch vụ: yêu cầu có thể chưa giao, bị hủy hoặc là tài sản cho thuê sẽ trả lại. Báo cáo tồn phải dựa trên số lượng nhập, xuất thực tế và giao dịch hoàn trả phù hợp loại mặt hàng.')
h(b,'2.12. Từ điển dữ liệu chi tiết: hóa đơn và dòng tiền',2)
t(b,['Thực thể','Thông tin chính','Vai trò kế toán'],[
('HoaDon','Lượt, thời điểm lập, tổng tiền, cọc, cần thu/hoàn','Đại diện kết quả kết toán một lượt.'),
('ChiTietBill','Loại dòng, mô tả, số lượng, đơn giá, thành tiền','Giải thích nguồn hình thành tổng bill.'),
('GiaoDichThanhToan','Loại giao dịch, tiền, kênh, mã tham chiếu','Phản ánh dòng tiền thực thu, hoàn, cọc, khấu trừ.'),
('DieuChinhHoaDon','Hóa đơn, nội dung, giá trị điều chỉnh','Giữ dấu vết thay đổi sau khi lập.'),
('HoaDonHuy','Hóa đơn, lý do, thời điểm hủy','Phân biệt hủy chứng từ với xóa dữ liệu.'),
('NhomHoaDon / PhanChiaHoaDon','Nhóm và phân bổ bill','Theo dõi thanh toán liên quan nhiều phòng.'),
])
p(b,'Cần phân biệt doanh thu và thu tiền. Ví dụ thu cọc 250.000 đồng làm tăng tiền quỹ nhưng chưa làm phát sinh toàn bộ doanh thu lưu trú. Khi thanh toán, tiền cọc được đối trừ vào khoản phải thu. Nếu hoàn cọc, hệ thống ghi một giao dịch hoàn; nếu giữ cọc vì hủy, cần có căn cứ và loại giao dịch thích hợp.')
h(b,'2.13. Từ điển dữ liệu chi tiết: tài chính và quản trị',2)
t(b,['Thực thể','Nội dung','Ứng dụng thực tế'],[
('CaTruc','Thu ngân, thời điểm mở/đóng, trạng thái','Bàn giao và xác định trách nhiệm tiền.'),
('KyKeToan','Khoảng kỳ, trạng thái khóa','Ngăn sửa chứng từ thuộc kỳ chốt.'),
('PhieuThuChi','Khoản thu/chi, kênh, tham chiếu','Sổ quỹ và giải trình chi phí.'),
('SoDuDauKy','Kỳ, tài khoản/quỹ, số dư','Tính số dư tích lũy.'),
('DongSaoKe','Ngày, số tiền, diễn giải, mã ngân hàng','Đối chiếu giao dịch chuyển khoản.'),
('CongNo / PhanBoCongNo','Khoản phải thu và lần phân bổ','Theo dõi nợ và thu hồi nợ.'),
('TaiKhoanNhanVien / PhanQuyenNhanVien','Tài khoản, vai trò, quyền chức năng','Kiểm soát truy cập.'),
('NhatKyThaoTac','Người, thời điểm, hành động, đối tượng','Kiểm toán thay đổi.'),
])
h(b,'2.14. Ma trận dữ liệu theo luồng nghiệp vụ',2)
t(b,['Luồng','Bảng đọc','Bảng ghi hoặc cập nhật'],[
('Đặt trước','Phong, LuotLuuTru, KhachHang','KhachHang, LuotLuuTru, GiaoDichThanhToan, NhatKyThaoTac'),
('Nhận phòng','LuotLuuTru, Phong','LuotLuuTru, Phong, ChangLuuTru, NhatKyThaoTac'),
('Gọi/giao dịch vụ','DichVu, HangTonKho, LuotLuuTru','YeuCauDichVu, BienDongKho, HangTonKho'),
('Chuyển phòng','LuotLuuTru, ChangLuuTru, Phong','ChangLuuTru, LuotLuuTru, Phong'),
('Trả phòng','LuotLuuTru, YeuCauDichVu, GiaoDichThanhToan','HoaDon, ChiTietBill, GiaoDichThanhToan, Phong'),
('Đối soát','GiaoDichThanhToan, DongSaoKe, CaTruc','DongSaoKe, PhieuThuChi, NhatKyThaoTac'),
('Báo cáo doanh thu','HoaDon, ChiTietBill, DieuChinhHoaDon','Không ghi nếu chỉ xem báo cáo.'),
])
p(b,'Ma trận thể hiện quan hệ nghiệp vụ ở mức khái niệm. Tên bảng thực tế được lớp truy cập dữ liệu sử dụng tùy nhánh xử lý, và một thao tác có thể ghi thêm bảng nhật ký/đối soát liên quan. Mục tiêu của ma trận là hỗ trợ kiểm thử và đánh giá tác động khi thay đổi lược đồ.')
h(b,'2.15. Kế hoạch kiểm soát chất lượng dữ liệu',2)
bullets(b,[
'Kiểm tra định dạng và tính duy nhất của giấy tờ, số phòng, tên đăng nhập, tham chiếu thanh toán trước khi ghi.',
'Chuẩn hóa thời điểm lưu trữ và định dạng ngày giờ khi xuất báo cáo; kiểm tra ngày đi sau ngày đến.',
'Không dùng trạng thái màu trên giao diện làm nguồn dữ liệu duy nhất; tính khả dụng từ lượt còn hiệu lực và lịch đặt.',
'Đối chiếu tổng dòng bill với tổng hóa đơn; đối chiếu tổng giao dịch tiền với số đã thu/hoàn.',
'Đối chiếu tồn hiện hành với tổng biến động kho; cảnh báo chênh lệch trước khi khóa ca/kỳ.',
'Lưu bản sao lưu trước migration, chạy kiểm thử khôi phục và ghi lại phiên bản lược đồ sau nâng cấp.'
])
insert_before('3. GIAO DIỆN MODULE VÀ CHỨC NĂNG',b)

c=fragment()
h(c,'3.10. Danh sách mã chức năng theo module',2)
p(c,'FunctionPolicy khai báo mã chức năng ổn định cho các module. Mã này là cầu nối giữa menu, quyền của vai trò và kiểm tra ở lớp nghiệp vụ. Bảng dưới mô tả các nhóm thay vì lặp toàn bộ tên hiển thị trên giao diện; khi đổi nhãn, mã quyền nên được giữ ổn định để không làm sai dữ liệu phân quyền.')
t(c,['Module','Mã chức năng','Tác vụ được bao phủ'],[
('Phòng','room.map, room.search, room.walkin, room.reserve','Sơ đồ, tìm kiếm và tạo lượt.'),
('Phòng','room.deposit, room.checkin, room.booking_edit, room.booking_cancel','Cọc, nhận phòng, sửa/hủy đặt.'),
('Phòng','room.schedule, room.checkout, room.transfer, room.extend, room.history','Lịch, trả phòng, chuyển, gia hạn, lịch sử.'),
('Phòng','room.clean, room.maintain, room.catalog, room.pricing','Dọn, bảo trì, danh mục, giá.'),
('Dịch vụ','service.order, service.manage, service.inventory, service.catalog, service.stock','Gọi/giao dịch vụ, tồn, danh mục, nhập/xuất.'),
('Khách','customer.profile, customer.history','Hồ sơ và lịch sử khách.'),
('Ca','shift.manage','Ca trực và bàn giao.'),
('Tài chính','cash.flow, cash.book, cash.bank, cash.debt','Thu chi, quỹ, ngân hàng, công nợ.'),
('Hóa đơn','invoice.create, invoice.list, invoice.control, invoice.groups','Lập, xem, chỉnh/hủy, nhóm bill.'),
('Báo cáo','report.revenue, report.profit','Doanh thu và lãi lỗ.'),
('Hệ thống','staff.view, staff.manage, system.password, system.audit','Nhân viên, quyền, mật khẩu, nhật ký.'),
])
h(c,'3.11. Thiết kế màn hình Sơ đồ phòng và đặt phòng',2)
p(c,'Sơ đồ phòng là vùng thao tác chính của lễ tân. Mỗi ô cần ưu tiên số phòng, trạng thái bằng màu và nhãn, giá/ngày và thông tin khách ở khi có thể hiển thị. Màu giúp quét nhanh nhưng chữ vẫn cần để người dùng phân biệt khi khó nhận màu. Nhấp một lần chọn phòng cho thao tác nhiều phòng; nhấp đúp mở luồng đặt/nhận phòng phù hợp. Với phòng đang ở, thao tác hướng đến màn hình thanh toán hoặc chi tiết lưu trú theo ngữ cảnh.')
t(c,['Thành phần','Dữ liệu hiển thị','Tương tác chính'],[
('Bộ lọc','Số phòng, loại, ngày đến, số ngày','Lọc ngay và cập nhật số phòng phù hợp.'),
('Chú giải','Trống, đang ở, đang dọn, bảo trì, có lịch đặt','Giúp đọc sơ đồ nhất quán.'),
('Ô phòng','Số, loại, giá, trạng thái, khách nếu có','Chọn, mở chi tiết, chuyển luồng.'),
('Chế độ','Nhận phòng ngay / Đặt phòng trước','Thay đổi điều kiện ngày và nhãn hành động.'),
('Khu vực chọn','Số phòng đã chọn và hành động tiếp tục','Tạo lượt cho một hoặc nhiều phòng.'),
])
p(c,'Các ô không nên nháy khi chọn hoặc thay đổi màu đột ngột. Số phòng đang bảo trì hoặc có lịch đặt phải có hành vi rõ: nếu không chọn được cho khoảng ngày hiện tại, lý do cần hiển thị ở ngay ô hoặc vùng trợ giúp.')
h(c,'3.12. Thiết kế bảng hoàn tất đặt phòng',2)
p(c,'Sau khi lưu thành công, hộp thoại hoàn tất cần phản chiếu dữ liệu vừa nhập để lễ tân kiểm tra trước mặt khách. Nội dung tối thiểu gồm họ tên, điện thoại, giấy tờ, phòng, ngày đến, ngày đi, hình thức nhận/đặt, tổng cọc và mã lượt. Nếu nhiều phòng, mỗi phòng nên có một dòng riêng. Hộp thoại chỉ xuất hiện sau commit, để tránh hiển thị thành công khi lưu thất bại.')
t(c,['Mục','Mục đích kiểm tra'],[
('Thông tin khách','Phát hiện sai tên, số điện thoại hoặc giấy tờ ngay sau nhập.'),
('Danh sách phòng','Xác nhận đúng số và số lượng phòng đã chọn.'),
('Thời gian','Kiểm tra ngày đến/đi, số ngày, tránh giữ nhầm lịch.'),
('Tiền cọc','Đối chiếu số đã thu và phương thức với khách.'),
('Trạng thái','Biết đã nhận phòng ngay hay mới đặt trước.'),
])
h(c,'3.13. Thiết kế màn hình Xử lý dịch vụ',2)
p(c,'Danh sách ưu tiên yêu cầu còn chờ, hiển thị phòng, tên khách, dịch vụ, số đã gọi, số đã giao, số còn chờ, thành tiền và trạng thái. Người dùng chọn một dòng sẽ thấy khung thao tác với số lượng. Nút chính phải nêu rõ hành động, ví dụ “Giao thêm 2”; tác vụ ít dùng như chuyển phòng hoặc hủy phần chờ có thể nằm trong menu thao tác phụ lớn, dễ đọc.')
bullets(c,[
'Khi tăng tổng số lượng, trường nhập phải được giải thích là “Tổng số lượng mới”, không dùng chung với trường “Giao thêm”.',
'Không cho giao lớn hơn phần còn chờ; thông báo lỗi nên nêu “Còn chờ 51, đã nhập 52”.',
'Mỗi phòng có nhiều yêu cầu; danh sách phải hiển thị tất cả dòng và lọc theo phòng khi cần.',
'Sau khi giao hoặc sửa, dữ liệu dòng và bộ đếm chờ cần cập nhật cùng lúc.'
])
h(c,'3.14. Thiết kế màn hình Kho dịch vụ và Kho minibar',2)
p(c,'Màn hình Kho dịch vụ phục vụ việc nhìn nhanh mặt hàng còn, sắp hết, hết và số khả dụng cho thuê. Kho minibar là nơi nhập/xuất và giải trình biến động. Cột “Tồn” là số đơn vị hàng hoặc số xe, không phải số giờ thuê. Cột “Giá vốn bình quân” dùng phân tích chi phí; “Ngưỡng nhập” là mốc cảnh báo cần bổ sung. Cột “Bán trên bill” phản ánh số lượng đã tính tiền, không tự động bằng số đã xuất kho nếu có thất thoát hoặc hoàn trả.')
t(c,['Tín hiệu','Điều kiện gợi ý','Hành động người dùng'],[
('Còn hàng','Tồn khả dụng lớn hơn ngưỡng','Có thể nhận yêu cầu trong giới hạn.'),
('Sắp hết','Tồn khả dụng không lớn hơn ngưỡng nhưng còn dương','Kiểm tra kế hoạch nhập.'),
('Hết','Tồn khả dụng bằng 0','Chặn giao hàng tiêu hao và đề xuất nhập.'),
('Xe đang thuê','Tổng xe trừ số đang thuê nhỏ hơn tổng xe','Chờ trả xe, không ghi hao hụt kho.'),
('Chênh lệch','Số kiểm kê khác số hệ thống','Lập điều chỉnh có lý do và người duyệt.'),
])
h(c,'3.15. Thiết kế màn hình Thanh toán một và nhiều phòng',2)
p(c,'Khu vực bill và khu vực thanh toán nên cùng nằm trong một luồng, tránh mở tab mới khi chuyển giữa phòng và nhóm phòng. Mỗi phòng có thể mở chi tiết để thấy tiền phòng theo số ngày, dịch vụ đã dùng và cọc. Với nhiều phòng, tổng nhóm cần cập nhật ngay khi người dùng chọn/bỏ phòng hoặc sửa bill phòng. Trước khi bấm xác nhận, cần thấy rõ tổng bill, tổng cọc, cần thu và cần hoàn.')
t(c,['Khối giao diện','Nội dung ưu tiên','Rủi ro cần tránh'],[
('Danh sách phòng','Số phòng, khách, tổng, cọc, cần thu','Không biết dịch vụ thuộc phòng nào.'),
('Chi tiết bill','Ngày ở, đơn giá, số lượng dịch vụ, điều chỉnh','Chỉ hiển thị số 1 cho tiền phòng dù ở nhiều ngày.'),
('Bill tổng','Tổng tất cả phòng đang chọn','Tính cả phòng đã bỏ chọn.'),
('Thanh toán','Phương thức, mã tham chiếu, xác nhận thu/hoàn','Ghi trùng tham chiếu hoặc nhầm số thu.'),
('Sau thanh toán','Biên nhận và trạng thái phòng','Chốt hóa đơn nhưng phòng vẫn hiển thị đang ở.'),
])
h(c,'3.16. Thiết kế màn hình Doanh thu và lãi lỗ',2)
p(c,'Trang doanh thu cần có thẻ tổng doanh thu kỳ chọn và tổng lũy kế, đồ thị xu hướng theo giờ/ngày/tháng/năm, biểu đồ cơ cấu theo tiền phòng, dịch vụ và khoản khác, bảng chi tiết có thể lọc theo hạng mục. Biểu đồ đường phù hợp xem xu hướng; biểu đồ tròn dùng khi số nhóm nguồn thu ít; biểu đồ cột giúp so sánh các ngày hoặc nhóm. Cùng một bộ lọc thời gian phải áp dụng nhất quán cho thẻ, đồ thị và bảng.')
t(c,['Chỉ số hiển thị','Nguồn','Lưu ý diễn giải'],[
('Tổng doanh thu','Hóa đơn và điều chỉnh hợp lệ','Không cộng cọc như doanh thu lần thứ hai.'),
('Doanh thu phòng','Dòng bill tiền phòng','Có thể khác tiền thu trong kỳ.'),
('Doanh thu dịch vụ','Dòng bill dịch vụ đã kết toán','Phân tách theo danh mục nếu cần.'),
('Giá vốn','Biến động kho và chi phí đã ghi nhận','Không coi xe cho thuê đã trả là hàng tiêu hao.'),
('Lãi gộp/ước tính','Doanh thu trừ chi phí tương ứng','Phụ thuộc độ đầy đủ dữ liệu chi phí.'),
])
h(c,'3.17. Kịch bản kiểm thử chức năng',2)
p(c,'Để xác nhận các màn hình và nghiệp vụ hoạt động cùng nhau, cần chạy kịch bản từ đầu đến cuối trên cơ sở dữ liệu thử nghiệm. Bảng dưới là bộ kiểm thử đại diện; kết quả kỳ vọng chỉ rõ điều kiện chấp nhận, không khẳng định đã chạy toàn bộ kịch bản trên dữ liệu sản xuất.')
t(c,['ID','Bước thử','Kết quả kỳ vọng'],[
('TC-01','Đặt 2 phòng trống cùng thời gian cho một khách.','Hai lượt được tạo; thông báo hoàn tất liệt kê đủ 2 phòng.'),
('TC-02','Đặt phòng chồng với một lượt còn hiệu lực.','Bị chặn và nêu phòng/thời gian xung đột.'),
('TC-03','Nhận phòng đã đặt và thu cọc bổ sung.','Trạng thái Occupied, cọc và giao dịch khớp.'),
('TC-04','Chuyển phòng sau một ngày ở.','Có hai chặng, bill hiển thị số ngày/giá từng chặng.'),
('TC-05','Gọi 5 dịch vụ, giao 2, hủy 1.','Còn chờ 2; bill chỉ gồm phần theo quy tắc tính tiền.'),
('TC-06','Tăng tổng yêu cầu lên 52 rồi giao 52.','Giao quá 51 phần còn chờ bị chặn, thông báo rõ số chờ.'),
('TC-07','Cho thuê 1 xe rồi trả.','Khả dụng giảm 1 khi giao và tăng 1 khi trả.'),
('TC-08','Thanh toán nhóm 2 phòng, bỏ chọn 1 phòng.','Tổng nhóm chỉ tính phòng đang chọn.'),
('TC-09','Nhập trùng mã tham chiếu thanh toán.','Không tạo giao dịch trùng.'),
('TC-10','Khóa ca/kỳ rồi sửa chứng từ cũ.','Thao tác bị chặn và chỉ dẫn quy trình điều chỉnh.'),
('TC-11','Đăng nhập vai trò Reception vào màn hình phân quyền.','Không hiển thị/không thực hiện được thao tác quản trị.'),
('TC-12','Xem doanh thu theo ngày và theo tháng.','Tổng các nhóm bằng tổng kỳ tương ứng.'),
])
h(c,'3.18. Yêu cầu phi chức năng và khả năng sử dụng',2)
t(c,['Tiêu chí','Mục tiêu triển khai/kiểm tra'],[
('Dễ đọc','Cỡ chữ đủ lớn, nút chính nổi bật, trạng thái có cả màu và nhãn.'),
('Phản hồi','Sau lưu, giao, trả phòng có trạng thái đang xử lý và kết quả cụ thể.'),
('Nhất quán','Cùng một thuật ngữ số gọi/đã giao/còn chờ ở các màn hình.'),
('Tìm kiếm','Tra cứu phòng, khách và dịch vụ không cần mở từng bản ghi.'),
('Toàn vẹn','Không ghi dở khi một bước trong transaction thất bại.'),
('Bảo mật','Giới hạn chức năng theo vai trò và quyền cấp riêng; nhật ký thao tác.'),
('Khả năng phục hồi','Có bản sao lưu, kiểm thử khôi phục và kế hoạch nâng cấp lược đồ.'),
('Truy cập dữ liệu','Tối ưu truy vấn danh sách lớn; phân trang/lọc khi dữ liệu tăng.'),
])
h(c,'3.19. Hướng dẫn triển khai và vận hành',2)
numbers(c,[
'Chuẩn bị Windows phù hợp, .NET Desktop Runtime theo bản biên dịch và SQL Server.',
'Tạo cơ sở dữ liệu thử nghiệm; cấu hình chuỗi kết nối và tài khoản SQL có quyền cần thiết.',
'Chạy migration theo phiên bản đến V23, kiểm tra bảng PhienBanCSDL và ràng buộc.',
'Tạo tài khoản quản trị ban đầu, kiểm tra quyền của bốn vai trò trên giao diện và lớp nghiệp vụ.',
'Nhập danh mục phòng, giá, dịch vụ, mặt hàng kho, tồn đầu kỳ và số dư đầu kỳ theo quy trình phê duyệt.',
'Chạy kịch bản đặt/nhận/trả phòng, dịch vụ, xe thuê, thanh toán và đối soát bằng dữ liệu thử.',
'Thiết lập lịch sao lưu và diễn tập phục hồi; chỉ sau đó mới đưa dữ liệu vận hành vào hệ thống.'
])
p(c,'Các bước trên là hướng dẫn triển khai được suy ra từ kiến trúc và cấu trúc dữ liệu của dự án. Đường dẫn cấu hình, cách đóng gói bộ cài, quyền SQL cụ thể và lịch sao lưu cần do đơn vị triển khai xác nhận theo môi trường thực tế.')
h(c,'3.20. Giới hạn hiện tại và hướng phát triển',2)
p(c,'Báo cáo này được lập từ mã nguồn và ảnh minh họa trong dự án. Một số chỉ số quản trị, khả năng in/chia sẻ chứng từ và điều kiện vận hành phụ thuộc dữ liệu thực tế. Những điểm sau là hướng mở rộng hợp lý khi hệ thống có quy mô phòng, dịch vụ và nhân viên lớn hơn; chúng không được trình bày như chức năng đã hoàn thiện.')
bullets(c,[
'Bổ sung dashboard vận hành theo thời gian gần thực với cảnh báo phòng sắp trả và yêu cầu dịch vụ quá hạn.',
'Tách chính sách giá theo mùa, ngày trong tuần, kênh bán và gói dịch vụ, nhưng giữ giá chốt trong lượt để báo giá không thay đổi hồi tố.',
'Tăng chi tiết báo cáo lãi lỗ theo phòng, nhóm dịch vụ và kỳ kế toán; thống nhất phân bổ chi phí gián tiếp.',
'Thêm kiểm kê kho định kỳ, phê duyệt chênh lệch và báo cáo hàng chậm luân chuyển.',
'Tự động hóa đối soát ngân hàng và cảnh báo giao dịch trùng hoặc chưa khớp.',
'Mở rộng kiểm thử tự động cho đặt nhiều phòng, thanh toán nhóm, hoàn cọc và trạng thái xe thuê.'
])
h(c,'3.21. Nguồn đối chiếu trong dự án',2)
t(c,['Nguồn','Nội dung sử dụng'],[
('QLKhachSan/BLL/HotelService*.cs','Luồng nghiệp vụ phòng, thanh toán, dịch vụ, kho và báo cáo.'),
('QLKhachSan/BLL/FunctionPolicy.cs; RolePolicy.cs','Mã chức năng, vai trò và kiểm tra quyền.'),
('QLKhachSan/DAL/HotelRepository.cs; HotelTransaction*.cs','Transaction, truy vấn, thao tác dữ liệu.'),
('QLKhachSan/DAL/SchemaMigrator.cs; Database/Database.sql','Phiên bản V23, bảng, ràng buộc, chỉ mục.'),
('QLKhachSan/DTO/*.cs','Mô hình dữ liệu và trạng thái.'),
('QLKhachSan/GUI/ucDashboard*.cs; AccountingForm.cs','Điều hướng, màn hình, tương tác người dùng.'),
('docs/ui-concepts-2026/implemented/*.png','Ảnh giao diện minh họa.'),
])
if False: append(c)
doc.save(PATH)
print(PATH)
