from copy import deepcopy
from pathlib import Path
from docx import Document
from docx.shared import Pt
from docx.oxml import OxmlElement
from docx.oxml.ns import qn

SOURCE=Path(r'D:\WorkSpace\qlks.docx')
OUT=Path(r'D:\WorkSpace\QLKhachSan\tmp\qlks_large_text_keep_tables.docx')
doc=Document(SOURCE)

doc.styles['Normal'].font.size=Pt(12)
doc.styles['Tiểu mục nội dung'].font.size=Pt(12)
for name in ('Heading 1','Heading 2'):
    current=doc.styles[name].font.size
    if current:
        doc.styles[name].font.size=Pt(current.pt+0.5)
for table in doc.tables:
    for row in table.rows:
        for cell in row.cells:
            for paragraph in cell.paragraphs:
                for run in paragraph.runs:
                    run.font.size=Pt(10.5)

# The 48-row table is larger than one page. Keep the original order, but
# present it as four complete continuation tables with repeated headers.
large=doc.tables[15]
old=large._tbl
parent=old.getparent()
place=parent.index(old)
rows=old.findall(qn('w:tr'))
assert len(rows)==49
parent.remove(old)
body=rows[1:]
for part,start in enumerate(range(0,len(body),12),1):
    if part>1:
        label=doc.add_paragraph(f'Danh mục bảng (tiếp theo {part}/4)')
        label.style=doc.styles['Normal']
        label.paragraph_format.keep_with_next=True
        for run in label.runs:
            run.bold=True
            run.font.size=Pt(11)
        parent.insert(place,label._p)
        place+=1
    clone=deepcopy(old)
    for child in list(clone.findall(qn('w:tr'))):
        clone.remove(child)
    clone.append(deepcopy(rows[0]))
    for row in body[start:start+12]:
        clone.append(deepcopy(row))
    parent.insert(place,clone)
    place+=1

for table in doc.tables:
    total=len(table.rows)
    for idx,row in enumerate(table.rows):
        trPr=row._tr.get_or_add_trPr()
        if trPr.find(qn('w:cantSplit')) is None:
            trPr.append(OxmlElement('w:cantSplit'))
        if idx==0 and trPr.find(qn('w:tblHeader')) is None:
            header=OxmlElement('w:tblHeader')
            header.set(qn('w:val'),'true')
            trPr.append(header)
        for cell in row.cells:
            for paragraph in cell.paragraphs:
                paragraph.paragraph_format.keep_together=True
                if idx<total-1:
                    paragraph.paragraph_format.keep_with_next=True

OUT.parent.mkdir(parents=True,exist_ok=True)
doc.save(OUT)
print(OUT)
print('tables',len(doc.tables))
