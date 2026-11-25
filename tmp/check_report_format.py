from zipfile import ZipFile
from lxml import etree

z=ZipFile('Bao_cao_du_an_QLKhachSan_V26.docx')
ns={'w':'http://schemas.openxmlformats.org/wordprocessingml/2006/main'}
for file in ['word/document.xml','word/styles.xml','word/footer1.xml']:
 root=etree.fromstring(z.read(file))
 sizes=root.xpath('//w:sz/@w:val',namespaces=ns)
 colors=root.xpath('//w:color/@w:val',namespaces=ns)
 print(file,'sizes under 12pt',sorted(set(x for x in sizes if x.isdigit() and int(x)<24)))
 print(file,'nonblack colors',sorted(set(x for x in colors if x.upper() not in ('000000','AUTO'))))
from docx import Document
d=Document('Bao_cao_du_an_QLKhachSan_V26.docx')
from collections import Counter
used=Counter(p.style.name for p in d.paragraphs)
for t in d.tables:
 for c in t._cells:
  used.update(p.style.name for p in c.paragraphs)
print('used styles')
for name,count in used.items():
 s=d.styles[name]
 print(name,count,s.font.size.pt if s.font.size else None,str(s.font.color.rgb) if s.font.color and s.font.color.rgb else None)
