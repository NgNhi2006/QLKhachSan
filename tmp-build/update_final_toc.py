import pypdf
from docx import Document
from docx.enum.text import WD_TAB_ALIGNMENT, WD_TAB_LEADER
from docx.shared import Cm

path=r'D:\WorkSpace\QLKhachSan\QLKS_co_so_du_lieu_khong_phien_ban.docx'
pdf=r'D:\WorkSpace\QLKhachSan\tmp-build\QLKS_co_so_du_lieu_khong_phien_ban.pdf'
d=Document(path)
pages=[pg.extract_text().splitlines() for pg in pypdf.PdfReader(pdf).pages]
miss=[]
for p in d.paragraphs:
    if not p.style.name.startswith('toc '): continue
    label=p.text.split('\t')[0].strip()
    if not label or label=='MỤC LỤC': continue
    matches=[i+1 for i,lines in enumerate(pages) if i>=3 and any(line.strip()==label for line in lines)]
    if matches:
        p.text=f'{label}\t{matches[0]}'
        if label.startswith('2.'):
            p.paragraph_format.tab_stops.add_tab_stop(Cm(15.9826),WD_TAB_ALIGNMENT.RIGHT,WD_TAB_LEADER.DOTS)
    else: miss.append(label)
assert not miss,miss
d.save(path)
print('Updated TOC entries')
