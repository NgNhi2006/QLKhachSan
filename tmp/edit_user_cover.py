from io import BytesIO
from pathlib import Path

from docx import Document
from docx.text.paragraph import Paragraph
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.oxml.ns import qn
from docx.shared import Cm, Pt, RGBColor

src = Path(r'D:\Bao_cao_du_an_QLKhachSan_V26.docx')
dst = Path(r'D:\Bao_cao_du_an_QLKhachSan_V26_bia_moi.docx')
doc = Document(src)
body = doc.element.body
old = list(body)
anchor = old[13]  # The original date paragraph carries the cover section break.

logo_para = doc.paragraphs[3]
blip = logo_para._p.xpath('.//a:blip')[0]
logo = BytesIO(doc.part.related_parts[blip.get(qn('r:embed'))].blob)

# Remove the overflowing cover and the unrelated laptop-project assignment page.
for el in old[:13] + old[14:18]:
    body.remove(el)

cover = doc.sections[0]
cover.top_margin = Cm(2.0)
cover.bottom_margin = Cm(2.0)
cover.left_margin = Cm(3.0)
cover.right_margin = Cm(2.0)
for border in cover._sectPr.xpath('./w:pgBorders'):
    cover._sectPr.remove(border)

def style(p, size=13, bold=False, align=WD_ALIGN_PARAGRAPH.CENTER,
          before=0, after=0, left=0):
    p.alignment = align
    p.paragraph_format.space_before = Pt(before)
    p.paragraph_format.space_after = Pt(after)
    p.paragraph_format.left_indent = Cm(left)
    p.paragraph_format.line_spacing = 1.15
    for run in p.runs:
        run.font.name = 'Times New Roman'
        run.font.size = Pt(size)
        run.font.bold = bold
        run.font.color.rgb = RGBColor(0, 0, 0)
    return p

def line(text='', **kwargs):
    p = doc.add_paragraph()
    body.remove(p._p)
    anchor.addprevious(p._p)
    p.add_run(text)
    return style(p, **kwargs)

line('TRƯỜNG CAO ĐẲNG CÔNG NGHỆ VIỆT - HÀN BẮC GIANG', size=13, bold=True, after=4)
line('KHOA CÔNG NGHỆ THÔNG TIN', size=13, bold=True, after=12)
p = line(after=15)
p.add_run().add_picture(logo, width=Cm(3.0))
style(p, after=15)
line('ĐỒ ÁN MÔN HỌC', size=18, bold=True, after=5)
line('LẬP TRÌNH WINDOWS', size=15, bold=True, after=25)
line('ĐỀ TÀI', size=14, bold=True, after=6)
line('XÂY DỰNG ỨNG DỤNG', size=19, bold=True, after=3)
line('QUẢN LÝ KHÁCH SẠN', size=19, bold=True, after=30)
line('Giảng viên hướng dẫn: Trương Thị Nhung', size=13, align=WD_ALIGN_PARAGRAPH.LEFT, left=2.0, after=12)
line('Nhóm sinh viên thực hiện:', size=13, bold=True, align=WD_ALIGN_PARAGRAPH.LEFT, left=2.0, after=4)
for name in ('Nguyễn Ngọc Thuần Nhi', 'Mạc Văn Khoa', 'Nguyễn Thị Thu Hà', 'Nguyễn Đức Anh Tuấn'):
    line('•  ' + name, size=13, align=WD_ALIGN_PARAGRAPH.LEFT, left=2.5, after=2)
line('Lớp: IT1.11A                         Khóa: 11', size=13, align=WD_ALIGN_PARAGRAPH.LEFT, left=2.0, before=10)

p = Paragraph(anchor, doc)
p.text = 'Bắc Giang, tháng 10 năm 2026'
style(p, size=13, before=34)

doc.save(dst)
print(dst)
