from io import BytesIO
from pathlib import Path
from zipfile import ZipFile, ZIP_DEFLATED

from PIL import Image
from docx import Document
from docx.shared import Cm
from docx.oxml.ns import qn
from lxml import etree

SOURCE = Path(r'D:\WorkSpace\qlks.docx')
WORK = Path(r'D:\WorkSpace\QLKhachSan\tmp\qlks_no_bullets.docx')
OUTPUT = Path(r'D:\WorkSpace\QLKhachSan\tmp\qlks_print_friendly.docx')
WORK.parent.mkdir(parents=True, exist_ok=True)

doc = Document(SOURCE)
removed_bullets = 0
for paragraph in doc.paragraphs:
    if paragraph.style.name == 'List Bullet':
        paragraph.style = doc.styles['Normal']
        num_pr = paragraph._p.find('./' + qn('w:pPr') + '/' + qn('w:numPr'))
        if num_pr is not None:
            num_pr.getparent().remove(num_pr)
        paragraph.paragraph_format.left_indent = Cm(0.45)
        paragraph.paragraph_format.first_line_indent = Cm(0)
        removed_bullets += 1
doc.save(WORK)

W = 'http://schemas.openxmlformats.org/wordprocessingml/2006/main'
def local(tag):
    return etree.QName(tag).localname

changed_shading = changed_color = converted_images = 0
with ZipFile(WORK, 'r') as src, ZipFile(OUTPUT, 'w', ZIP_DEFLATED) as dst:
    for member in src.infolist():
        data = src.read(member.filename)
        if member.filename.startswith('word/media/') and member.filename.lower().endswith(('.png','.jpg','.jpeg')):
            im = Image.open(BytesIO(data))
            if im.mode in ('RGBA','LA'):
                base = Image.new('RGBA', im.size, 'white')
                base.alpha_composite(im.convert('RGBA'))
                im = base.convert('RGB')
            gray = im.convert('L').convert('RGB')
            buf = BytesIO()
            fmt = 'PNG' if member.filename.lower().endswith('.png') else 'JPEG'
            gray.save(buf, format=fmt, quality=95)
            data = buf.getvalue()
            converted_images += 1
        elif member.filename.startswith('word/') and member.filename.endswith('.xml'):
            root = etree.fromstring(data)
            for elem in root.iter():
                if not isinstance(elem.tag, str):
                    continue
                name = local(elem.tag)
                if name == 'shd' and elem.tag.startswith('{' + W + '}'):
                    elem.set(qn('w:fill'), 'FFFFFF')
                    elem.set(qn('w:color'), '000000')
                    elem.set(qn('w:val'), 'clear')
                    for key in list(elem.attrib):
                        if local(key) in ('themeFill','themeFillTint','themeFillShade','themeColor','themeTint','themeShade'):
                            del elem.attrib[key]
                    changed_shading += 1
                elif name == 'color' and elem.tag.startswith('{' + W + '}'):
                    elem.set(qn('w:val'), '000000')
                    for key in list(elem.attrib):
                        if local(key) in ('themeColor','themeTint','themeShade'):
                            del elem.attrib[key]
                    changed_color += 1
                elif name == 'highlight' and elem.tag.startswith('{' + W + '}'):
                    elem.set(qn('w:val'), 'none')
                elif name == 'background' and elem.tag.startswith('{' + W + '}'):
                    elem.set(qn('w:color'), 'FFFFFF')
                for key in list(elem.attrib):
                    if key.startswith('{' + W + '}') and local(key) == 'color' and name not in ('shd','background'):
                        elem.set(key, '000000')
            data = etree.tostring(root, xml_declaration=True, encoding='UTF-8', standalone=True)
        dst.writestr(member, data)

print(f'removed_bullets={removed_bullets} white_shading={changed_shading} black_colors={changed_color} grayscale_images={converted_images}')
print(OUTPUT)
