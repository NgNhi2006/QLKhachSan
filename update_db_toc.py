import pypdf
from docx import Document
from docx.enum.text import WD_TAB_ALIGNMENT, WD_TAB_LEADER
from docx.shared import Cm

path=r'D:\WorkSpace\QLKhachSan\QLKS_sua_phan_co_so_du_lieu.docx'
pdf=r'D:\WorkSpace\QLKhachSan\QLKS_sua_phan_co_so_du_lieu.pdf'
d=Document(path)
pages=[p.extract_text().splitlines() for p in pypdf.PdfReader(pdf).pages]
missing=[]
for p in d.paragraphs:
    if not p.style.name.startswith('toc '): continue
    label=p.text.split('\t')[0].strip()
    if not label: continue
    matches=[i+1 for i,lines in enumerate(pages) if i>=3 and any(line.strip()==label for line in lines)]
    if matches:
        p.text=f'{label}\t{matches[0]}'
        if label.startswith('2.'):
            p.paragraph_format.tab_stops.add_tab_stop(Cm(15.9826), WD_TAB_ALIGNMENT.RIGHT, WD_TAB_LEADER.DOTS)
    else: missing.append(label)
d.save(path)
print('TOC unmatched:',missing)
