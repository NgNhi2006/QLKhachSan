import json
from collections import defaultdict
from pathlib import Path

from docx import Document
from docx.enum.table import WD_CELL_VERTICAL_ALIGNMENT, WD_TABLE_ALIGNMENT
from docx.enum.text import WD_ALIGN_PARAGRAPH, WD_TAB_ALIGNMENT, WD_TAB_LEADER
from docx.oxml import OxmlElement
from docx.oxml.ns import qn
from docx.shared import Cm, Pt

ROOT = Path(r'D:\WorkSpace\QLKhachSan')
SOURCE = ROOT / 'QLKS_sua_phan_co_so_du_lieu.docx'
OUT = ROOT / 'QLKS_co_so_du_lieu_khong_phien_ban.docx'
SCHEMA = json.loads((ROOT / 'tmp-build' / 'schema_report.json').read_text(encoding='utf-8-sig'))
doc = Document(SOURCE)

ORDER = [
    'TaiKhoanNhanVien','NhatKyThaoTac','ChucNang','PhanQuyenNhanVien',
    'CauHinhMenu','CauHinhMenuCon','CauHinhChucNang','Phong','KhachHang',
    'LuotLuuTru','ChangLuuTru','DichVu','YeuCauDichVu','HangTonKho','BienDongKho',
    'ChiTietBill','HoaDon','GiaoDichThanhToan','CaTruc'
]
GROUPS = [
    ('Hệ thống và phân quyền', ORDER[:4], 'Đăng nhập, chức năng được cấp và nhật ký thao tác.'),
    ('Cấu hình menu', ORDER[4:7], 'Cấu trúc menu chính, menu con và đường dẫn tới chức năng.'),
    ('Phòng và lưu trú', ORDER[7:11], 'Danh mục phòng, hồ sơ khách, lượt ở và các chặng tính tiền phòng.'),
    ('Dịch vụ và kho', ORDER[11:15], 'Danh mục dịch vụ, yêu cầu phục vụ, mặt hàng và biến động tồn.'),
    ('Hóa đơn, thanh toán và ca trực', ORDER[15:], 'Dòng bill, hóa đơn, tiền thực thu và bàn giao ca.')
]
INTRO = {
    'PhienBanCSDL':'Ghi phiên bản lược đồ để ứng dụng biết migration nào đã chạy.',
    'TaiKhoanNhanVien':'Lưu tài khoản đăng nhập, vai trò, trạng thái khóa và dữ liệu xác thực.',
    'NhatKyThaoTac':'Ghi tài khoản, hành động, chi tiết và thời điểm để tra soát.',
    'ChucNang':'Danh mục chức năng mà menu và cơ chế phân quyền cùng sử dụng.',
    'PhanQuyenNhanVien':'Bảng nối tài khoản với từng chức năng được cấp.',
    'CauHinhMenu':'Cấu hình menu chính hiển thị trên giao diện.',
    'CauHinhMenuCon':'Các nhóm menu con thuộc một menu chính.',
    'CauHinhChucNang':'Gắn chức năng và thứ tự hiển thị vào menu con.',
    'Phong':'Danh mục phòng, loại, giá, cọc và trạng thái vật lý.',
    'KhachHang':'Hồ sơ nhận diện khách để tra cứu lại ở các lần lưu trú.',
    'LuotLuuTru':'Bản ghi đặt phòng, nhận phòng, trả phòng và trạng thái lượt.',
    'ChangLuuTru':'Từng khoảng ở với phòng và đơn giá riêng, dùng khi chuyển phòng.',
    'DichVu':'Danh mục dịch vụ, đơn giá và trạng thái kinh doanh.',
    'YeuCauDichVu':'Dịch vụ khách gọi, số lượng đã giao và tiến độ phục vụ.',
    'HangTonKho':'Mặt hàng, số tồn, giá vốn và ngưỡng cần nhập thêm.',
    'BienDongKho':'Lịch sử nhập, xuất hoặc điều chỉnh số lượng hàng.',
    'ChiTietBill':'Các dòng tiền phòng, dịch vụ và khoản khác trên hóa đơn.',
    'HoaDon':'Chứng từ chốt tiền khi kết thúc lượt lưu trú.',
    'GiaoDichThanhToan':'Các khoản thu cọc, thu khi trả phòng, hoàn và cọc không hoàn.',
    'CaTruc':'Thời gian mở, bàn giao, khóa ca và số tiền đối chiếu.'
}
MEANING = {
    'AnhDaiDien':'Ảnh đại diện','BieuTuong':'Mã biểu tượng','ChiTiet':'Nội dung thao tác','CoSan':'Cấu hình có sẵn','ConHieuLuc':'Lượt còn hiệu lực','DaLuuTru':'Tài khoản lưu trữ','DaTuyChinhQuyen':'Đã chỉnh quyền riêng','DangHienThi':'Trạng thái hiển thị','DangHoatDong':'Trạng thái hoạt động','DanhMuc':'Nhóm dịch vụ hoặc dòng bill','DonGia':'Đơn giá tại thời điểm ghi','DonGiaPhong':'Đơn giá phòng','DonViTinh':'Đơn vị tính','GhiChu':'Ghi chú giao dịch','GiaVonBinhQuan':'Giá vốn bình quân','GiaVonDonVi':'Giá vốn một đơn vị','GiaiTrinh':'Giải trình chênh lệch','HanGiuPhong':'Hạn giữ phòng đặt','HanhDong':'Mã hành động','IconPng':'Ảnh biểu tượng PNG','KhoaDen':'Khóa đăng nhập đến','Loai':'Loại phòng','LoaiGiaoDich':'Loại giao dịch','LyDo':'Lý do biến động','LyDoHuy':'Lý do hủy','LyDoSua':'Lý do sửa dòng bill','Ma':'Mã bản ghi','MaCaTruc':'Mã ca trực','MaChucNang':'Mã chức năng','MaDichVu':'Mã dịch vụ','MaHienThi':'Mã hiển thị','MaHoaDon':'Mã hóa đơn','MaKhachHang':'Mã khách hàng','MaLuotLuuTru':'Mã lượt lưu trú','MaMatHang':'Mã mặt hàng','MaMenu':'Mã menu chính','MaMenuCon':'Mã menu con','MaNguoiKhoa':'Mã người khóa ca','MaNguoiTao':'Mã nhân viên tạo','MaNhanVien':'Mã nhân viên','MaPhong':'Mã phòng','MaThamChieuNgoai':'Mã tham chiếu thanh toán','MaThaoTac':'Mã thao tác','MaThuNgan':'Mã thu ngân','MaYeuCauDichVu':'Mã yêu cầu dịch vụ','MatKhauBam':'Giá trị băm mật khẩu','MauSac':'Màu hiển thị','MoTa':'Mô tả','MuoiBam':'Muối băm mật khẩu','NguongDatHang':'Ngưỡng đặt thêm hàng','PhienBan':'Phiên bản dữ liệu','PhienBanBaoMat':'Phiên bản phiên đăng nhập','PhuongThucThanhToan':'Phương thức thanh toán','SoDienThoai':'Số điện thoại','SoGiayTo':'Số giấy tờ','SoLanBam':'Số vòng băm','SoLanDangNhapSai':'Số lần đăng nhập sai','SoLuong':'Số lượng','SoLuongDaGiao':'Số lượng đã giao','SoPhong':'Số phòng','SoPhongHoaDon':'Số phòng khi lập hóa đơn','SoTien':'Số tiền giao dịch','Ten':'Tên','TenDangNhap':'Tên đăng nhập','TenHienThi':'Tên hiển thị','TenKhach':'Tên khách tại thời điểm lưu','TenMenuPhu':'Tên menu phụ','ThanhTien':'Thành tiền dòng bill','ThoiDiemBatDau':'Thời điểm bắt đầu','ThoiDiemBienDong':'Thời điểm biến động','ThoiDiemDen':'Thời điểm dự kiến đến','ThoiDiemDi':'Thời điểm dự kiến đi','ThoiDiemDongCa':'Thời điểm đóng ca','ThoiDiemGiao':'Thời điểm giao dịch vụ','ThoiDiemGoi':'Thời điểm gọi dịch vụ','ThoiDiemHuy':'Thời điểm hủy yêu cầu','ThoiDiemKetThuc':'Thời điểm kết thúc','ThoiDiemKhoa':'Thời điểm khóa ca','ThoiDiemLapHoaDon':'Thời điểm lập hóa đơn','ThoiDiemMoCa':'Thời điểm mở ca','ThoiDiemNhanPhong':'Thời điểm nhận phòng','ThoiDiemTao':'Thời điểm tạo','ThoiDiemTraPhong':'Thời điểm trả phòng','ThuTuHienThi':'Thứ tự hiển thị','TienCoc':'Tiền cọc','TienDaHoan':'Tiền đã hoàn','TienDaThu':'Tiền đã thu','TienDichVu':'Tiền dịch vụ','TienMatDauCa':'Tiền mặt đầu ca','TienMatDuKien':'Tiền mặt dự kiến','TienMatKiemDem':'Tiền mặt kiểm đếm','TienPhong':'Tiền phòng','TieuDe':'Tên hiển thị','TrangThai':'Trạng thái','VaiTro':'Vai trò tài khoản','VaiTroDuocPhep':'Vai trò được phép'
}

cols = defaultdict(list)
for col in SCHEMA['columns']:
    cols[col['table']].append(col)
assert set(ORDER).issubset(set(cols)) and len(ORDER)==19
assert sum(len(cols[t]) for t in ORDER)==166

def shade(cell, fill):
    pr=cell._tc.get_or_add_tcPr(); sh=OxmlElement('w:shd'); sh.set(qn('w:fill'),fill); pr.append(sh)
def no_split(row):
    pr=row._tr.get_or_add_trPr(); el=OxmlElement('w:cantSplit'); pr.append(el)
def repeat_header(row):
    pr=row._tr.get_or_add_trPr(); el=OxmlElement('w:tblHeader'); el.set(qn('w:val'),'true'); pr.append(el)
def celltext(cell, text, bold=False, center=False):
    cell.text=str(text); cell.vertical_alignment=WD_CELL_VERTICAL_ALIGNMENT.CENTER
    for p in cell.paragraphs:
        p.alignment=WD_ALIGN_PARAGRAPH.CENTER if center else WD_ALIGN_PARAGRAPH.LEFT
        p.paragraph_format.space_before=Pt(0); p.paragraph_format.space_after=Pt(0)
        for r in p.runs:
            r.font.name='Times New Roman'; r.font.size=Pt(8.5); r.bold=bold
def table(headers, rows, widths):
    t=doc.add_table(rows=1, cols=len(headers)); t.style='Table Grid'; t.autofit=False; t.alignment=WD_TABLE_ALIGNMENT.CENTER
    for col,w in zip(t.columns,widths): col.width=Cm(w)
    repeat_header(t.rows[0]); no_split(t.rows[0])
    for cell,title,w in zip(t.rows[0].cells,headers,widths):
        cell.width=Cm(w); celltext(cell,title,True,True); shade(cell,'DCE6F1')
    for i,values in enumerate(rows):
        row=t.add_row(); no_split(row)
        for cell,value,w in zip(row.cells,values,widths):
            cell.width=Cm(w); celltext(cell,value,False,False)
            if i%2: shade(cell,'F6F8FB')
    doc.add_paragraph().paragraph_format.space_after=Pt(1)
    return t
def p(text='',style='Normal',bold=False):
    para=doc.add_paragraph(style=style); run=para.add_run(text); run.bold=bold
    para.paragraph_format.space_after=Pt(5)
    return para
def h(n,title):
    para=p(f'2.{n} {title}','Heading 2'); para.paragraph_format.space_before=Pt(11); return para
def sub(n,title):
    para=p(f'2.3.{n} Bảng {title}','Heading 3'); para.paragraph_format.space_before=Pt(7); para.paragraph_format.keep_with_next=True; return para
def cap(number,title):
    para=p(f'Bảng 2.{number}. {title}'); para.alignment=WD_ALIGN_PARAGRAPH.CENTER; para.paragraph_format.keep_with_next=True
    for r in para.runs: r.italic=True; r.bold=True
    return para
def code(lines):
    for line in lines:
        para=p(line)
        para.paragraph_format.left_indent=Cm(.6); para.paragraph_format.space_after=Pt(0)
        for r in para.runs: r.font.name='Consolas'; r.font.size=Pt(8.5)
    p('')

def sqltype(c):
    ty=c['base_type']; size=c['max_length']
    if ty in ('nvarchar','nchar'): return f'{ty}({"MAX" if size==-1 else size//2})'
    if ty in ('varchar','char','varbinary','binary'): return f'{ty}({"MAX" if size==-1 else size})'
    if ty in ('decimal','numeric'): return f'{ty}({c["precision"]},{c["scale"]})'
    if ty in ('datetime2','time','datetimeoffset'): return f'{ty}({c["scale"]})'
    return ty
def field_note(t,c):
    n=c['column']; note=MEANING.get(n,n)
    if n=='PhienBan' and t=='PhienBanCSDL': return 'Phiên bản lược đồ'
    if n=='PhienBan' and t in ('Phong','LuotLuuTru','DichVu'): return 'Phiên bản bản ghi'
    if n=='LoaiGiaoDich' and t=='BienDongKho': return 'Loại nhập, xuất hoặc điều chỉnh'
    if n=='LoaiGiaoDich' and t=='GiaoDichThanhToan': return 'Thu cọc, thu, hoàn hoặc cọc mất'
    if n=='DanhMuc' and t=='ChiTietBill': return 'Loại dòng bill'
    if n=='DanhMuc' and t in ('DichVu','YeuCauDichVu'): return 'Danh mục dịch vụ'
    return note

unique_single=defaultdict(set)
for ix in SCHEMA['indexes']:
    if ix['table'] in ORDER and ix['is_unique'] and ',' not in ix['columns'] and not ix['has_filter']:
        unique_single[ix['table']].add(ix['columns'])
def constraint(t,c):
    n=c['column']; tags=[]
    if c['is_pk']: tags.append('PK')
    if c['fk_table']: tags.append('FK')
    if n in unique_single[t]: tags.append('UNIQUE')
    if c['is_identity']: tags.append('IDENTITY')
    if c['default_definition']: tags.append('DEFAULT')
    return ', '.join(tags) if tags else '—'

# Delete the previous chapter 2 while retaining the title and every other chapter.
chapter2=next(x for x in doc.paragraphs if x.style.name=='Heading 1' and x.text.startswith('2 CƠ SỞ'))
chapter3=next(x for x in doc.paragraphs if x.style.name=='Heading 1' and x.text.startswith('3 FORM'))
start=chapter2._element; end=chapter3._element
e=start.getnext()
while e is not None and e is not end:
    nxt=e.getnext(); e.getparent().remove(e); e=nxt
original_tail=doc.element.body[-2]

h(1,'Phân tích thực thể')
p('Phần này trình bày 19 bảng phục vụ các chức năng của hệ thống quản lý khách sạn. Các bảng được chia theo nghiệp vụ như sau:')
cap(1,'Nhóm thực thể trong cơ sở dữ liệu')
table(['Nhóm','Các bảng','Chức năng'],[(g,', '.join(ts),d) for g,ts,d in GROUPS],[3.0,6.4,6.0])

h(2,'Lược đồ quan hệ')
p('Lược đồ xoay quanh LuotLuuTru. Một khách có thể có nhiều lượt; một lượt gắn với phòng, các chặng ở, dịch vụ, thanh toán và hóa đơn. Các quan hệ chính:')
for line in [
    'KhachHang → LuotLuuTru ← Phong; LuotLuuTru → ChangLuuTru.',
    'LuotLuuTru → YeuCauDichVu ← DichVu; HangTonKho → BienDongKho.',
    'LuotLuuTru → HoaDon → ChiTietBill; LuotLuuTru → GiaoDichThanhToan.',
    'TaiKhoanNhanVien → PhanQuyenNhanVien ← ChucNang; CauHinhMenu → CauHinhMenuCon → CauHinhChucNang.'
]: p(line)
p('Mũi tên thể hiện hướng tham chiếu nghiệp vụ; danh sách khóa ngoại thực tế nằm ở mục 2.4.')

h(3,'Thiết kế chi tiết các bảng')
table_no=2
for i,t in enumerate(ORDER,1):
    sub(i,t); p(INTRO[t]); cap(table_no,f'Cấu trúc bảng dbo.{t}')
    rows=[]
    for c in cols[t]:
        rows.append([c['column'],sqltype(c),constraint(t,c),'Có' if c['is_nullable'] else 'Không',field_note(t,c)])
    table(['Tên cột','Kiểu dữ liệu','Ràng buộc','Null','Mô tả'],rows,[3.6,2.6,2.6,1.4,5.0])
    table_no+=1

h(4,'Mối liên hệ giữa các bảng')
p('Lược đồ V26 khai báo 28 khóa ngoại. Bảng dưới ghi đúng bảng con, cột tham chiếu và bảng cha trong SQL Server. Tất cả đều có quy tắc xóa NO ACTION: không thể xóa bản ghi cha khi còn bản ghi con tham chiếu.')
cap(table_no,'Các quan hệ khóa ngoại'); table_no+=1
table(['Bảng con','Cột khóa ngoại','Bảng cha','Cột được tham chiếu'],[
    [x['parent_table'],x['parent_column'],x['ref_table'],x['ref_column']] for x in SCHEMA['foreign_keys'] if x['parent_table'] in ORDER
],[3.3,4.5,3.7,3.5])

h(5,'Ràng buộc toàn vẹn và chỉ mục')
p('Ngoài khóa chính và khóa ngoại, các bảng được trình bày có 40 ràng buộc CHECK và 28 chỉ mục phụ. Các quy tắc ảnh hưởng trực tiếp đến nghiệp vụ được tóm tắt dưới đây.')
cap(table_no,'Ràng buộc nghiệp vụ tiêu biểu'); table_no+=1
rules=[
    ('Phong','Loai','Chỉ nhận Đơn, Đôi hoặc VIP.'),
    ('Phong','DonGiaPhong, TienCoc','Giá phòng phải dương; cọc không âm.'),
    ('LuotLuuTru','ThoiDiemDi, TrangThai','Ngày đi sau ngày đến hoặc nhận phòng; hiệu lực khớp trạng thái.'),
    ('ChangLuuTru','ThoiDiemKetThuc','Nếu có thời điểm kết thúc thì không trước thời điểm bắt đầu.'),
    ('YeuCauDichVu','SoLuong, SoLuongDaGiao','Số gọi từ 1 đến 100; số đã giao từ 0 đến số gọi.'),
    ('HoaDon','Các khoản tiền','Tiền phòng + dịch vụ = cọc + đã thu − đã hoàn.'),
    ('GiaoDichThanhToan','SoTien, LoaiGiaoDich','Số tiền dương; loại giao dịch thuộc Deposit, Checkout, Refund, Forfeit.'),
    ('HangTonKho','SoLuong','Số tồn không âm.'),
    ('CaTruc','TrangThai, ThoiDiemDongCa','Ca mở chưa có thời điểm đóng; ca nộp hoặc khóa đã có thời điểm đóng và tiền kiểm đếm.')
]
table(['Bảng','Cột liên quan','Quy tắc'],rules,[3.0,4.0,8.0])
cap(table_no,'Chỉ mục phục vụ tra cứu và chống trùng'); table_no+=1
index_names=['UX_Stays_OccupiedRoom','IX_Stays_ReservationDates','UX_Segments_Open','IX_Orders_Pending','IX_BillDetails_Invoice','IX_Invoices_Issued','UX_Payments_Forfeit','UX_CashShifts_Open','UX_Users_ActiveUsername','IX_StockMovements_Date']
selected=[x for x in SCHEMA['indexes'] if x['name'] in index_names]
table(['Bảng','Chỉ mục','Cột chính','Ý nghĩa'],[
    [x['table'],x['name'],x['columns'],'Duy nhất có điều kiện' if x['is_unique'] else 'Tăng tốc tra cứu'] for x in selected
],[2.6,4.5,4.0,3.9])

h(6,'Dữ liệu đã nhập vào các bảng')
p('Số bản ghi dưới đây được đếm trên cơ sở dữ liệu QLKhachSanApp phiên bản 26 ngày 15/10/2026. Bảng chỉ trình bày số lượng và trạng thái, không đưa thông tin cá nhân của khách vào báo cáo.')
cap(table_no,'Số bản ghi theo bảng'); table_no+=1
table(['Bảng','Số bản ghi'],[[x['table'],x['count']] for x in SCHEMA['counts']],[10.5,4.5])
cap(table_no,'Trạng thái phòng và lượt lưu trú'); table_no+=1
table(['Nguồn','Trạng thái','Số bản ghi'],[['Phong',x['TrangThai'],x['SoPhong']] for x in SCHEMA['room_status']]+[['LuotLuuTru',x['TrangThai'],x['SoLuot']] for x in SCHEMA['stay_status']],[5.0,5.0,5.0])

h(7,'View hỗ trợ truy vấn')
p('Cơ sở dữ liệu V26 hiện không khai báo view. Nếu cần dùng chung dữ liệu lượt ở và tên phòng cho nhiều màn hình, có thể bổ sung view sau. Đây là đề xuất thiết kế, chưa được tạo trong cơ sở dữ liệu hiện tại.')
code([
    'CREATE VIEW dbo.vw_LuotLuuTruTongQuan AS',
    'SELECT l.Ma, p.SoPhong, l.TenKhach, l.TrangThai,',
    '       l.ThoiDiemDen, l.ThoiDiemDi',
    'FROM dbo.LuotLuuTru AS l',
    'JOIN dbo.Phong AS p ON p.Ma = l.MaPhong;'
])

h(8,'Nhập dữ liệu vào cơ sở dữ liệu')
p('Người dùng nhập dữ liệu qua form phòng, khách, đặt phòng và dịch vụ; lớp BLL kiểm tra nghiệp vụ trước khi DAL ghi bằng câu lệnh có tham số. Khi cần nhập trực tiếp trong SQL Server, phải tuân thứ tự bảng cha trước bảng con. Ví dụ minh họa tạo một hồ sơ khách:')
code([
    "INSERT INTO dbo.KhachHang(Ten, SoDienThoai, SoGiayTo)",
    "VALUES (N'Khách minh họa', N'0900000000', N'GIAYTO_MOI');"
])
p('Ví dụ này chỉ minh họa cú pháp; không được chạy lặp trên cơ sở dữ liệu vận hành vì SoGiayTo có ràng buộc duy nhất.')

h(9,'Kết nối ứng dụng Visual Studio với SQL Server')
p('Dự án dùng Microsoft.Data.SqlClient. Chuỗi kết nối được đọc từ appsettings.json hoặc biến môi trường QLKHACHSAN_CONNECTION_STRING. Cấu hình hiện tại dùng máy chủ .\\SQLEXPRESS, cơ sở dữ liệu QLKhachSanApp, xác thực Windows và mã hóa kết nối.')
code([
    'using Microsoft.Data.SqlClient;',
    'using var connection = new SqlConnection(AppSettings.Load().ConnectionString);',
    'await connection.OpenAsync();'
])

h(10,'Mã lệnh tạo từng bảng')
p('Các lệnh CREATE TABLE và migration của các bảng nằm trong Database\\Database.sql. Ví dụ dưới trích cấu trúc cốt lõi của bảng Phong; các cột mở rộng và chỉ mục được thêm ở các migration tiếp theo.')
code([
    'CREATE TABLE dbo.Phong (',
    '    Ma int IDENTITY PRIMARY KEY,',
    '    SoPhong nvarchar(10) NOT NULL UNIQUE,',
    '    Loai nvarchar(20) NOT NULL,',
    '    DonGiaPhong decimal(18,2) NOT NULL,',
    '    TienCoc decimal(18,2) NOT NULL,',
    "    TrangThai varchar(20) NOT NULL DEFAULT 'Trong',",
    '    PhienBan bigint NOT NULL DEFAULT 0',
    ');'
])
p('Ở V26, bảng Phong còn có MaHienThi. Khi triển khai không chạy riêng đoạn trích trên mà dùng script và migration đầy đủ để tránh thiếu cột hoặc ràng buộc.')

h(11,'Các truy vấn mẫu và kết quả')
p('Các kết quả dưới đây lấy từ dữ liệu đang có tại thời điểm lập báo cáo; chạy lại truy vấn sau khi phát sinh giao dịch sẽ cho số lượng khác.')
p('Truy vấn 1: đếm phòng theo trạng thái.',bold=True)
code(['SELECT TrangThai, COUNT(*) AS SoPhong','FROM dbo.Phong','GROUP BY TrangThai ORDER BY TrangThai;'])
cap(table_no,'Kết quả truy vấn trạng thái phòng'); table_no+=1
table(['Trạng thái','Số phòng'],[[x['TrangThai'],x['SoPhong']] for x in SCHEMA['room_status']],[10.5,4.5])
p('Truy vấn 2: đếm lượt lưu trú theo trạng thái.',bold=True)
code(['SELECT TrangThai, COUNT(*) AS SoLuot','FROM dbo.LuotLuuTru','GROUP BY TrangThai ORDER BY TrangThai;'])
cap(table_no,'Kết quả truy vấn trạng thái lượt lưu trú'); table_no+=1
table(['Trạng thái','Số lượt'],[[x['TrangThai'],x['SoLuot']] for x in SCHEMA['stay_status']],[10.5,4.5])

h(12,'Kiểm thử các ràng buộc toàn vẹn')
p('Các tình huống sau là ca kiểm thử đề xuất dựa trên ràng buộc đang có; không chạy lệnh làm sai dữ liệu trên cơ sở dữ liệu vận hành.')
cap(table_no,'Ca kiểm thử ràng buộc'); table_no+=1
table(['Tình huống','Ràng buộc','Kết quả cần đạt'],[
    ('Thêm phòng trùng SoPhong','UNIQUE trên Phong.SoPhong','SQL Server từ chối bản ghi trùng.'),
    ('Thêm phòng có DonGiaPhong ≤ 0','CHECK trên Phong','Không ghi được phòng.'),
    ('Thêm lượt với MaPhong không tồn tại','FK LuotLuuTru → Phong','Không ghi được lượt.'),
    ('Giao dịch vụ nhiều hơn số đã gọi','CHECK SoLuongDaGiao ≤ SoLuong','Không cập nhật được yêu cầu.'),
    ('Mở ca thứ hai cho cùng thu ngân','UX_CashShifts_Open','Không tồn tại hai ca Open cùng thu ngân.')
],[5.1,4.3,5.6])

h(13,'Sao lưu và phục hồi cơ sở dữ liệu')
p('Sao lưu trước mỗi lần nâng phiên bản lược đồ. Database.sql có migration xóa các bảng của phân hệ đã ngừng dùng ở V26, vì vậy bản sao lưu phải được kiểm tra trước khi nâng cấp. Ví dụ lệnh sao lưu và kiểm tra tệp:')
code([
    'BACKUP DATABASE QLKhachSanApp',
    "TO DISK = N'C:\\Backup\\QLKhachSanApp.bak' WITH INIT;",
    "RESTORE VERIFYONLY FROM DISK = N'C:\\Backup\\QLKhachSanApp.bak';"
])
p('Khi cần phục hồi, thử trên một cơ sở dữ liệu khác trước, kiểm tra số bảng, phiên bản và dữ liệu nghiệp vụ rồi mới đưa vào sử dụng.')

h(14,'Bảo mật dữ liệu')
p('Mật khẩu được dẫn xuất bằng PBKDF2-SHA512 với muối riêng 16 byte và 210.000 vòng băm; bảng TaiKhoanNhanVien lưu giá trị băm, muối và số vòng. DAL dùng SqlParameter cho giá trị nhập, quyền chức năng được kiểm tra theo tài khoản, còn NhatKyThaoTac lưu dấu các thao tác cần tra soát. Kết nối SQL Server dùng xác thực Windows theo cấu hình hiện tại.')

# Insert chapter 2 content before the untouched chapter 3.
first=original_tail.getnext(); made=[]
while first is not None and first.tag!=qn('w:sectPr'):
    nxt=first.getnext(); made.append(first); first=nxt
for e in made: end.addprevious(e)

# Replace only chapter 2 entries in the manual TOC. Page numbers are filled after rendering.
for para in list(doc.paragraphs):
    if para.style.name=='toc 2' and para.text.startswith('2.'):
        para._element.getparent().remove(para._element)
toc_anchor=next(x for x in doc.paragraphs if x.style.name=='toc 1' and x.text.startswith('3 FORM'))
titles=['Phân tích thực thể','Lược đồ quan hệ','Thiết kế chi tiết các bảng','Mối liên hệ giữa các bảng','Ràng buộc toàn vẹn và chỉ mục','Dữ liệu đã nhập vào các bảng','View hỗ trợ truy vấn','Nhập dữ liệu vào cơ sở dữ liệu','Kết nối ứng dụng Visual Studio với SQL Server','Mã lệnh tạo từng bảng','Các truy vấn mẫu và kết quả','Kiểm thử các ràng buộc toàn vẹn','Sao lưu và phục hồi cơ sở dữ liệu','Bảo mật dữ liệu']
for i,title in enumerate(titles,1):
    para=doc.add_paragraph(style='toc 2'); para.add_run(f'2.{i} {title}\t0')
    para.paragraph_format.tab_stops.add_tab_stop(Cm(15.9826),WD_TAB_ALIGNMENT.RIGHT,WD_TAB_LEADER.DOTS)
    toc_anchor._element.addprevious(para._element)

for para in doc.paragraphs:
    if 'Dự án hiện có một lõi vận hành gồm 20 bảng ứng dụng' in para.text:
        para.text=para.text.replace('Dự án hiện có một lõi vận hành gồm 20 bảng ứng dụng','Báo cáo trình bày 19 bảng phục vụ nghiệp vụ')
doc.save(OUT)
print(OUT)
