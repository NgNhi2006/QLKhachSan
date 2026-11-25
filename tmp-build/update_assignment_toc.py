import pypdf
from docx import Document

file=r'D:\QLKS_bang_phan_cong.docx'
pdf=r'D:\WorkSpace\QLKhachSan\tmp-build\QLKS_bang_phan_cong.pdf'
d=Document(file)
pages=[pg.extract_text().splitlines() for pg in pypdf.PdfReader(pdf).pages]
missing=[]
for para in d.paragraphs:
    if not para.style.name.startswith('toc '): continue
    label=para.text.split('\t')[0].strip()
    if not label or label=='MỤC LỤC': continue
    hits=[i+1 for i,lines in enumerate(pages) if i>=3 and any(x.strip()==label for x in lines)]
    if hits: para.text=f'{label}\t{hits[0]}'
    else: missing.append(label)
assert not missing,missing
d.save(file)
