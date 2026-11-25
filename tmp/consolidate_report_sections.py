from pathlib import Path
import re
from docx import Document
from docx.enum.style import WD_STYLE_TYPE
from docx.shared import Pt, RGBColor
from docx.oxml import OxmlElement
from docx.oxml.ns import qn

path=Path(__file__).resolve().parents[1]/'docs'/'Bao_cao_he_thong_quan_ly_khach_san.docx'
doc=Document(path)

groups={
    '1.1': '1.1. Tổng quan và vai trò sử dụng',
    '1.5': '1.2. Quy tắc vận hành, dịch vụ và thanh toán',
    '1.8': '1.3. Yêu cầu và quy trình đặt, nhận phòng',
    '1.11': '1.4. Xử lý dịch vụ, hóa đơn và ca trực',
    '1.16': '1.5. Kiểm soát trạng thái, dữ liệu và rủi ro',
    '2.1': '2.1. Kiến trúc dữ liệu và mô hình quan hệ',
    '2.4': '2.2. Toàn vẹn dữ liệu và nguyên tắc ghi nhận',
    '2.7': '2.3. Danh mục bảng, khóa và giao dịch',
    '2.10': '2.4. Từ điển dữ liệu theo phân hệ',
    '2.14': '2.5. Đối chiếu, nâng cấp và bảo vệ dữ liệu',
    '3.1': '3.1. Điều hướng và quản lý phòng',
    '3.3': '3.2. Giao diện dịch vụ, khách hàng và tài chính',
    '3.10': '3.3. Đặc tả các màn hình nghiệp vụ',
    '3.17': '3.4. Kiểm thử, triển khai và nguồn đối chiếu',
    '3.22': '3.5. Tương tác, nhập liệu và nghiệm thu',
}

style_name='Tiểu mục nội dung'
if style_name not in doc.styles:
    style=doc.styles.add_style(style_name,WD_STYLE_TYPE.PARAGRAPH)
else:
    style=doc.styles[style_name]
style.base_style=doc.styles['Normal']
style.font.name='Times New Roman'
style.font.size=Pt(11.5)
style.font.bold=True
style.font.color.rgb=RGBColor(20,100,76)
style.paragraph_format.space_before=Pt(9)
style.paragraph_format.space_after=Pt(3)
style.paragraph_format.keep_with_next=True

old=[p for p in doc.paragraphs if p.style.name=='Heading 2']
assert len(old)==63, f'Unexpected existing subsection count: {len(old)}'
assert len(groups)==15
for paragraph in old:
    match=re.match(r'^(\d+\.\d+)\.\s*(.+)$',paragraph.text)
    if not match:
        raise ValueError(paragraph.text)
    key,title=match.groups()
    if key in groups:
        new=doc.add_paragraph(style='Heading 2')
        new.add_run(groups[key])
        paragraph._p.addprevious(new._p)
    paragraph.text=title
    paragraph.style=style

for paragraph in doc.paragraphs:
    for node in paragraph._p.iter(qn('w:instrText')):
        if node.text and node.text.strip().startswith('TOC '):
            node.text='TOC \\o "1-2" \\h \\z \\u'

doc.save(path)
print(path)
