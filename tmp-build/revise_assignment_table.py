from docx import Document
from docx.enum.table import WD_CELL_VERTICAL_ALIGNMENT, WD_ROW_HEIGHT_RULE
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.oxml import OxmlElement
from docx.oxml.ns import qn
from docx.shared import Cm, Pt

SOURCE=r'D:\QLKS_bang_phan_cong.docx'
OUT=r'D:\QLKS_phan_cong_truong_nhom.docx'
d=Document(SOURCE)
old=d.tables[1]
assert old.cell(0,0).text=='STT' and len(old.columns)==5

data=[
    ('Nguyễn Thị Thu Hà','Trưởng nhóm',
     '• Tổng hợp viết báo cáo và điều phối công việc của nhóm.\n'
     '• Thiết kế cơ sở dữ liệu, khóa và ràng buộc; kết nối ứng dụng với SQL Server.\n'
     '• Xây dựng xử lý chính: phòng, đặt phòng, lưu trú, bill và thanh toán.\n'
     '• Tích hợp chương trình, kiểm tra bản chạy cuối.'),
    ('Nguyễn Ngọc Thuần Nhi','Thiết kế giao diện và báo cáo',
     '• Thiết kế giao diện chung, màu sắc, biểu tượng và bố cục màn hình.\n'
     '• Trình bày phần cơ sở dữ liệu và định dạng báo cáo Word.'),
    ('Nguyễn Đức Anh Tuấn','Thiết kế màn hình hệ thống',
     '• Thiết kế form đăng nhập, menu chính và điều hướng.\n'
     '• Hoàn thiện giao diện tài khoản, quyền và nhật ký thao tác.'),
    ('Hoàng Anh Ngọc','Thiết kế màn hình nghiệp vụ',
     '• Thiết kế màn hình dịch vụ, yêu cầu phục vụ và kho.\n'
     '• Bổ sung giao diện xem bill và hỗ trợ kiểm thử các thao tác liên quan.'),
    ('Lê Quốc Vượng','Báo cáo và kiểm thử',
     '• Thiết kế màn hình thống kê, hóa đơn, ca trực và xuất dữ liệu.\n'
     '• Kiểm thử giao diện, đối chiếu số liệu và hỗ trợ hoàn thiện báo cáo.')
]
headers=['STT','Họ và tên','Chức vụ trong nhóm','Nhiệm vụ chính được giao (Các hạng mục thiết kế)']
widths=[1.1,3.05,3.35,7.7]
t=d.add_table(rows=1,cols=4)
t.style='Table Grid'; t.autofit=False
for col,w in zip(t.columns,widths): col.width=Cm(w)

def set_cell(c,value,bold=False,center=False):
    c.text=value; c.vertical_alignment=WD_CELL_VERTICAL_ALIGNMENT.CENTER
    for p in c.paragraphs:
        p.alignment=WD_ALIGN_PARAGRAPH.CENTER if center else WD_ALIGN_PARAGRAPH.LEFT
        p.paragraph_format.space_after=Pt(0)
        for run in p.runs:
            run.bold=bold; run.font.name='Times New Roman'; run.font.size=Pt(10)
    pr=c._tc.get_or_add_tcPr(); mar=OxmlElement('w:tcMar')
    for side,n in [('top','100'),('left','80'),('bottom','100'),('right','80')]:
        el=OxmlElement('w:'+side); el.set(qn('w:w'),n); el.set(qn('w:type'),'dxa'); mar.append(el)
    pr.append(mar)

for cell,value,w in zip(t.rows[0].cells,headers,widths):
    cell.width=Cm(w); set_cell(cell,value,True,True)
trpr=t.rows[0]._tr.get_or_add_trPr(); rep=OxmlElement('w:tblHeader'); rep.set(qn('w:val'),'true'); trpr.append(rep)
for i,(name,role,tasks) in enumerate(data,1):
    row=t.add_row(); row.height=Cm(2.0); row.height_rule=WD_ROW_HEIGHT_RULE.AT_LEAST
    no_split=OxmlElement('w:cantSplit'); row._tr.get_or_add_trPr().append(no_split)
    for j,(cell,value,w) in enumerate(zip(row.cells,[str(i),name,role,tasks],widths)):
        cell.width=Cm(w); set_cell(cell,value,j==1,j==0)

old._element.addprevious(t._element)
old._element.getparent().remove(old._element)
d.save(OUT)
print(OUT)
