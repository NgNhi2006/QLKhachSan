from docx import Document
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.enum.table import WD_TABLE_ALIGNMENT, WD_CELL_VERTICAL_ALIGNMENT
from docx.shared import Pt, Cm, RGBColor
from docx.oxml import OxmlElement
from docx.oxml.ns import qn

SRC = r'D:\QLKS.docx'
OUT = r'D:\WorkSpace\QLKhachSan\QLKS_sua_phan_co_so_du_lieu.docx'
d = Document(SRC)

names = ['PhienBanCSDL','TaiKhoanNhanVien','NhatKyThaoTac','ChucNang','PhanQuyenNhanVien','CauHinhMenu','CauHinhMenuCon','CauHinhChucNang','Phong','KhachHang','LuotLuuTru','ChangLuuTru','DichVu','YeuCauDichVu','HangTonKho','BienDongKho','ChiTietBill','HoaDon','GiaoDichThanhToan','CaTruc']
descs = [d.paragraphs[i].text for i in [154,158,163,167,172,177,182,187,193,198,203,208,214,218,223,227,233,238,243,248]]
fields = [[(r.cells[0].text, r.cells[1].text) for r in d.tables[j].rows[1:]] for j in range(2,22)]

meanings = {
 'PhienBan':'Phiên bản cơ sở dữ liệu','Ma':'Mã bản ghi','TenDangNhap':'Tên đăng nhập','MatKhauBam':'Giá trị băm mật khẩu','MuoiBam':'Muối băm mật khẩu','SoLanBam':'Số lần băm','VaiTro':'Vai trò tài khoản','DangHoatDong':'Trạng thái hoạt động','SoLanDangNhapSai':'Số lần đăng nhập sai','KhoaDen':'Thời điểm hết khóa',
 'MaNhanVien':'Mã nhân viên','HanhDong':'Tên thao tác','ChiTiet':'Nội dung chi tiết','ThoiDiemTao':'Thời điểm tạo','MaChucNang':'Mã chức năng','TieuDe':'Tên hiển thị','MaMenu':'Mã menu chính','TenMenuPhu':'Tên menu phụ','VaiTroDuocPhep':'Vai trò được phép','ThuTuHienThi':'Thứ tự hiển thị','MoTa':'Mô tả','BieuTuong':'Biểu tượng','MauSac':'Màu hiển thị','DangHienThi':'Trạng thái hiển thị','CoSan':'Cấu hình có sẵn','MaMenuCon':'Mã menu con','MaThaoTac':'Mã thao tác',
 'SoPhong':'Số phòng','Loai':'Loại phòng hoặc loại giao dịch','DonGiaPhong':'Đơn giá phòng','TienCoc':'Tiền cọc','TrangThai':'Trạng thái','Ten':'Tên','SoDienThoai':'Số điện thoại','SoGiayTo':'Số giấy tờ','MaPhong':'Mã phòng','MaKhachHang':'Mã khách hàng','TenKhach':'Tên khách','ConHieuLuc':'Còn hiệu lực','ThoiDiemDen':'Thời điểm đến','ThoiDiemDi':'Thời điểm đi','ThoiDiemNhanPhong':'Thời điểm nhận phòng','ThoiDiemTraPhong':'Thời điểm trả phòng','HanGiuPhong':'Hạn giữ phòng','MaNguoiTao':'Mã người tạo','MaLuotLuuTru':'Mã lượt lưu trú','ThoiDiemBatDau':'Thời điểm bắt đầu','ThoiDiemKetThuc':'Thời điểm kết thúc',
 'DanhMuc':'Danh mục','DonGia':'Đơn giá','DonViTinh':'Đơn vị tính','MaDichVu':'Mã dịch vụ','SoLuong':'Số lượng','ThoiDiemGoi':'Thời điểm gọi dịch vụ','ThoiDiemGiao':'Thời điểm giao dịch vụ','GiaVonBinhQuan':'Giá vốn bình quân','NguongDatHang':'Ngưỡng đặt hàng','MaMatHang':'Mã mặt hàng','ThoiDiemBienDong':'Thời điểm biến động','LoaiGiaoDich':'Loại giao dịch','GiaVonDonVi':'Giá vốn đơn vị','MaYeuCauDichVu':'Mã yêu cầu dịch vụ','LyDo':'Lý do','MaHoaDon':'Mã hóa đơn','LyDoSua':'Lý do sửa','SoPhongHoaDon':'Số phòng trên hóa đơn','ThoiDiemLapHoaDon':'Thời điểm lập hóa đơn','TienPhong':'Tiền phòng','TienDichVu':'Tiền dịch vụ','TienDaThu':'Tiền đã thu','TienDaHoan':'Tiền đã hoàn','PhuongThucThanhToan':'Phương thức thanh toán','SoTien':'Số tiền','GhiChu':'Ghi chú','MaThuNgan':'Mã thu ngân','ThoiDiemMoCa':'Thời điểm mở ca','ThoiDiemDongCa':'Thời điểm đóng ca','TienMatDauCa':'Tiền mặt đầu ca','TienMatKiemDem':'Tiền mặt kiểm đếm','TienMatDuKien':'Tiền mặt dự kiến','GiaiTrinh':'Giải trình','ThoiDiemKhoa':'Thời điểm khóa ca','MaNguoiKhoa':'Mã người khóa ca'
}
pk = {'PhienBanCSDL':{'PhienBan'},'ChucNang':{'MaChucNang'},'PhanQuyenNhanVien':{'MaNhanVien','MaChucNang'},'CauHinhMenu':{'MaMenu'}}
fks = {'MaNhanVien':'TaiKhoanNhanVien','MaNguoiTao':'TaiKhoanNhanVien','MaNguoiKhoa':'TaiKhoanNhanVien','MaThuNgan':'TaiKhoanNhanVien','MaPhong':'Phong','MaKhachHang':'KhachHang','MaLuotLuuTru':'LuotLuuTru','MaDichVu':'DichVu','MaMatHang':'HangTonKho','MaYeuCauDichVu':'YeuCauDichVu','MaHoaDon':'HoaDon','MaMenuCon':'CauHinhMenuCon'}

def shade(cell, fill):
    tcPr=cell._tc.get_or_add_tcPr(); sh=OxmlElement('w:shd'); sh.set(qn('w:fill'),fill); tcPr.append(sh)
def set_cell(cell, value, bold=False):
    cell.text=str(value); cell.vertical_alignment=WD_CELL_VERTICAL_ALIGNMENT.CENTER
    for p in cell.paragraphs:
        p.paragraph_format.space_after=Pt(0); p.paragraph_format.space_before=Pt(0)
        for r in p.runs:
            r.font.name='Times New Roman'; r.font.size=Pt(10); r.bold=bold
def addp(text, style='Normal', bold=False):
    p=d.add_paragraph(style=style); p.add_run(text).bold=bold
    p.paragraph_format.space_after=Pt(4)
    return p
def heading(num, title):
    p=addp(f'2.{num} {title}','Heading 2'); p.paragraph_format.space_before=Pt(10); return p

# Remove the existing chapter 2 body, retaining chapter 2 title and chapter 3 onward.
start=d.paragraphs[148]._element
end=d.paragraphs[252]._element
e=start.getnext()
while e is not None and e is not end:
    nxt=e.getnext(); e.getparent().remove(e); e=nxt
original_tail=d.element.body[-2]

heading(1,'Phân tích thực thể')
addp('Cơ sở dữ liệu gồm 20 bảng, chia thành các nhóm: tài khoản và phân quyền; menu; phòng và lưu trú; dịch vụ và kho; hóa đơn, thanh toán và ca trực.')
heading(2,'Lược đồ quan hệ')
addp('Các liên kết chính: KhachHang và Phong → LuotLuuTru → ChangLuuTru, YeuCauDichVu, HoaDon và GiaoDichThanhToan. HoaDon → ChiTietBill; DichVu → YeuCauDichVu; HangTonKho → BienDongKho. Các bảng tài khoản liên kết với người tạo và nhật ký thao tác.')
heading(3,'Thiết kế chi tiết các bảng')
for idx,(name,desc,rows) in enumerate(zip(names,descs,fields),1):
    p=addp(f'2.3.{idx} Bảng {name}','Heading 3')
    p.paragraph_format.keep_with_next=True
    addp(desc)
    cap=addp(f'Bảng 2.{idx}. Cấu trúc bảng dbo.{name}')
    cap.alignment=WD_ALIGN_PARAGRAPH.CENTER; cap.paragraph_format.keep_with_next=True
    for run in cap.runs: run.italic=True; run.bold=True
    t=d.add_table(rows=1, cols=5); t.style='Table Grid'; t.alignment=WD_TABLE_ALIGNMENT.CENTER
    t.autofit=False
    trPr=t.rows[0]._tr.get_or_add_trPr(); repeat=OxmlElement('w:tblHeader'); repeat.set(qn('w:val'),'true'); trPr.append(repeat)
    for c,h in zip(t.rows[0].cells,['TT','Tên cột','Kiểu dữ liệu','Khóa','Mô tả']): set_cell(c,h,True); shade(c,'DCE6F1')
    for j,(col,typ) in enumerate(rows,1):
        cells=t.add_row().cells
        key='PK' if col in pk.get(name,set()) or (col=='Ma' and name not in ('PhanQuyenNhanVien','ChucNang','CauHinhMenu')) else ('FK' if col in fks else '—')
        for c,v in zip(cells,[j,col,typ,key,meanings.get(col,col)]): set_cell(c,v)
        if j%2==0:
            for c in cells: shade(c,'F7F9FC')
    widths=[Cm(1.0),Cm(3.6),Cm(2.7),Cm(1.3),Cm(6.2)]
    for column,w in zip(t.columns,widths): column.width=w
    for row in t.rows:
        for cell,w in zip(row.cells,widths): cell.width=w
    addp('')

heading(4,'Mối liên hệ giữa các bảng')
addp('LuotLuuTru tham chiếu phòng và khách; mỗi lượt có thể có nhiều chặng lưu trú, yêu cầu dịch vụ và giao dịch thanh toán. HoaDon liên kết với lượt và các dòng ChiTietBill. PhanQuyenNhanVien nối tài khoản với chức năng.')
heading(5,'Ràng buộc toàn vẹn và chỉ mục')
addp('Mã bản ghi là khóa chính; các cột mã liên kết dùng để xác định quan hệ giữa bảng. Ràng buộc nghiệp vụ cần kiểm tra trùng lịch phòng, số lượng dịch vụ đã giao và số lượng tồn kho. Các cột dùng để tra cứu theo phòng, khách, thời gian và trạng thái cần có chỉ mục phù hợp.')
heading(6,'Dữ liệu mẫu đã nhập vào các bảng')
addp('Dữ liệu khởi tạo gồm phiên bản cơ sở dữ liệu, menu, chức năng và quyền mặc định. Dữ liệu phòng, khách và giao dịch được nhập qua các màn hình nghiệp vụ.')
heading(7,'View hỗ trợ truy vấn')
addp('Các màn hình có thể dùng truy vấn tổng hợp để xem phòng, lượt lưu trú, hóa đơn và tồn kho. Tên view và định nghĩa cụ thể cần đối chiếu với Database.sql khi triển khai.')
heading(8,'Nhập dữ liệu vào cơ sở dữ liệu')
addp('Dữ liệu được thêm qua form và lớp xử lý nghiệp vụ. Khi lưu lượt, dịch vụ hoặc thanh toán, hệ thống kiểm tra dữ liệu đầu vào và ghi các bảng liên quan trong cùng giao dịch.')
heading(9,'Kết nối ứng dụng Visual Studio với SQL Server')
addp('Ứng dụng dùng chuỗi kết nối SQL Server trong cấu hình. Sau khi kết nối, lớp truy cập dữ liệu thực hiện truy vấn và chuyển kết quả cho lớp nghiệp vụ, sau đó hiển thị trên form.')
heading(10,'Mã lệnh tạo từng bảng')
addp('Tập lệnh tạo và nâng cấp các bảng nằm trong Database.sql của dự án. Các bảng ở trên phản ánh lược đồ đang được mô tả trong báo cáo.')
heading(11,'Các truy vấn mẫu và kết quả')
addp('Các truy vấn thường dùng gồm tìm phòng trống theo khoảng thời gian, xem dịch vụ của một lượt, tổng hợp hóa đơn theo kỳ và đối chiếu số tồn với biến động kho.')
heading(12,'Kiểm thử các ràng buộc toàn vẹn')
addp('Cần thử mã liên kết không tồn tại, lịch đặt phòng chồng nhau, số lượng giao vượt số gọi và xuất kho vượt tồn. Giao dịch lỗi phải hoàn tác toàn bộ thay đổi liên quan.')
heading(13,'Sao lưu và phục hồi cơ sở dữ liệu')
addp('Sao lưu cơ sở dữ liệu trước khi chạy migration hoặc cập nhật phiên bản. Khi phục hồi, kiểm tra phiên bản lược đồ và dữ liệu nghiệp vụ trước khi mở ứng dụng cho người dùng.')
heading(14,'Bảo mật dữ liệu')
addp('Mật khẩu được lưu dưới dạng giá trị băm; quyền chức năng gắn với tài khoản. Tài khoản không còn sử dụng được khóa để giữ lịch sử giao dịch và nhật ký thao tác.')

# Move authored elements immediately before chapter 3.
new_first=original_tail.getnext()
elements=[]
while new_first is not None:
    nxt=new_first.getnext(); elements.append(new_first); new_first=nxt
for el in elements: end.addprevious(el)

# Replace chapter 2 entries in the manual table of contents.
toc_titles=['Phân tích thực thể','Lược đồ quan hệ','Thiết kế chi tiết các bảng','Mối liên hệ giữa các bảng','Ràng buộc toàn vẹn và chỉ mục','Dữ liệu mẫu đã nhập vào các bảng','View hỗ trợ truy vấn','Nhập dữ liệu vào cơ sở dữ liệu','Kết nối ứng dụng Visual Studio với SQL Server','Mã lệnh tạo từng bảng','Các truy vấn mẫu và kết quả','Kiểm thử các ràng buộc toàn vẹn','Sao lưu và phục hồi cơ sở dữ liệu','Bảo mật dữ liệu']
for p in d.paragraphs:
    if p.style.name=='toc 2' and p.text.startswith('2.'):
        p._element.getparent().remove(p._element)
anchor=next(p for p in d.paragraphs if p.style.name=='toc 1' and p.text.startswith('3 FORM'))
for i,title in enumerate(toc_titles,1):
    p=d.add_paragraph(style='toc 2'); p.add_run(f'2.{i} {title}\t')
    anchor._element.addprevious(p._element)

d.save(OUT)
print(OUT)
