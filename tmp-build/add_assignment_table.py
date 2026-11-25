from docx import Document
from docx.enum.table import WD_CELL_VERTICAL_ALIGNMENT, WD_ROW_HEIGHT_RULE
from docx.enum.text import WD_ALIGN_PARAGRAPH, WD_BREAK
from docx.oxml import OxmlElement
from docx.oxml.ns import qn
from docx.shared import Cm, Pt, RGBColor

SOURCE=r'D:\QLKS_da_sua.docx'
OUT=r'D:\QLKS_bang_phan_cong.docx'
d=Document(SOURCE)
names=[r.cells[1].text.strip() for r in d.tables[0].rows[1:]]
assert len(names)==5
assignments={
    'Nguyễn Ngọc Thuần Nhi':(
        'Thành viên',
        '• Thiết kế các bảng SQL Server, khóa và ràng buộc.\n• Xử lý lớp truy cập dữ liệu, migration và kết nối CSDL.',
        '• Mục 2: Cơ sở dữ liệu.\n• Đối chiếu tên bảng, cột và câu lệnh SQL.',
        '6 kịch bản\n(CSDL, ràng buộc)'
    ),
    'Nguyễn Đức Anh Tuấn':(
        'Thành viên',
        '• Xây dựng đăng nhập, tài khoản và phân quyền.\n• Thiết kế menu chính, điều hướng và giao diện chung.',
        '• Mục 1.1: Vai trò và phân quyền.\n• Mục 3.1–3.3 và 3.12–3.13.',
        '6 kịch bản\n(đăng nhập, quyền)'
    ),
    'Nguyễn Thị Thu Hà':(
        'Nhóm trưởng',
        '• Phân chia công việc, tích hợp và kiểm tra bản chạy cuối.\n• Xây dựng quy trình phòng, đặt phòng và lưu trú.',
        '• Mở đầu; mục 1.2–1.5.\n• Mục 3.4–3.6; tổng hợp báo cáo.',
        '8 kịch bản\n(phòng, đặt, lưu trú)'
    ),
    'Hoàng Anh Ngọc':(
        'Thành viên',
        '• Xây dựng danh mục dịch vụ, yêu cầu phục vụ và kho.\n• Tích hợp dòng dịch vụ vào bill và checkout.',
        '• Mục 1.6–1.7 và 1.9.\n• Mục 3.7–3.9.',
        '7 kịch bản\n(dịch vụ, kho, bill)'
    ),
    'Lê Quốc Vượng':(
        'Thành viên',
        '• Xây dựng hóa đơn, thanh toán, ca trực và báo cáo.\n• Kiểm tra tổng tiền, giao dịch và dữ liệu xuất.',
        '• Mục 1.8 và 1.10.\n• Mục 3.10–3.11, 3.14; kết luận.',
        '7 kịch bản\n(thanh toán, ca, báo cáo)'
    ),
}
assert set(names)==set(assignments)
opening=next(p for p in d.paragraphs if p.style.name=='Heading 1' and p.text=='MỞ ĐẦU')
anchor=opening._element

title=d.add_paragraph()
title.alignment=WD_ALIGN_PARAGRAPH.CENTER
title.paragraph_format.space_before=Pt(0)
title.paragraph_format.space_after=Pt(12)
r=title.add_run('BẢNG PHÂN CÔNG CÔNG VIỆC')
r.bold=True; r.font.name='Times New Roman'; r.font.size=Pt(14)
title._element.getparent().remove(title._element)
anchor.addprevious(title._element)

headers=['STT','Thành viên / vai trò','Công việc lập trình và cơ sở dữ liệu','Phần báo cáo phụ trách','Kiểm thử']
widths=[1.1,3.1,4.9,3.7,2.4]
t=d.add_table(rows=1,cols=5)
t.style='Table Grid'; t.autofit=False
for col,w in zip(t.columns,widths): col.width=Cm(w)

def fill(cell,color):
    sh=OxmlElement('w:shd'); sh.set(qn('w:fill'),color); cell._tc.get_or_add_tcPr().append(sh)
def setcell(cell,text,bold=False,center=False):
    cell.text=text; cell.vertical_alignment=WD_CELL_VERTICAL_ALIGNMENT.CENTER
    for p in cell.paragraphs:
        p.alignment=WD_ALIGN_PARAGRAPH.CENTER if center else WD_ALIGN_PARAGRAPH.LEFT
        p.paragraph_format.space_after=Pt(0)
        for run in p.runs:
            run.font.name='Times New Roman'; run.font.size=Pt(10); run.bold=bold
def padding(cell):
    tcMar=OxmlElement('w:tcMar')
    for side,val in [('top','90'),('left','90'),('bottom','90'),('right','90')]:
        el=OxmlElement('w:'+side); el.set(qn('w:w'),val); el.set(qn('w:type'),'dxa'); tcMar.append(el)
    cell._tc.get_or_add_tcPr().append(tcMar)

for c,h,w in zip(t.rows[0].cells,headers,widths):
    c.width=Cm(w); setcell(c,h,True,True); fill(c,'DCE8F7'); padding(c)
trPr=t.rows[0]._tr.get_or_add_trPr(); rep=OxmlElement('w:tblHeader'); rep.set(qn('w:val'),'true'); trPr.append(rep)
for i,name in enumerate(names,1):
    row=t.add_row(); row.height=Cm(2.25); row.height_rule=WD_ROW_HEIGHT_RULE.AT_LEAST
    role,work,report,tests=assignments[name]
    values=[str(i),name+'\n'+role,work,report,tests]
    for j,(c,v,w) in enumerate(zip(row.cells,values,widths)):
        c.width=Cm(w); setcell(c,v,j==1,j==0); padding(c)
        if i%2==0: fill(c,'F6F9FD')
    p=row.cells[1].paragraphs[0]
    p.clear()
    a=p.add_run(name); a.bold=True; a.font.name='Times New Roman'; a.font.size=Pt(10); a.font.color.rgb=RGBColor(31,65,112)
    a.add_break()
    b=p.add_run(role); b.italic=True; b.font.name='Times New Roman'; b.font.size=Pt(9)

t._element.getparent().remove(t._element)
anchor.addprevious(t._element)
page=d.add_paragraph()
page.add_run().add_break(WD_BREAK.PAGE)
page._element.getparent().remove(page._element)
anchor.addprevious(page._element)
d.save(OUT)
print(OUT)
