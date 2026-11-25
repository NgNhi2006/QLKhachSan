from docx import Document
from docx.shared import Pt, RGBColor
from docx.oxml import OxmlElement
from docx.oxml.ns import qn

path='Bao_cao_du_an_QLKhachSan_V26.docx'
d=Document(path)
used={p.style.name for p in d.paragraphs}
for t in d.tables:
 for c in t._cells:
  used.update(p.style.name for p in c.paragraphs)
for name in used:
 s=d.styles[name]
 if s.font.size is None or s.font.size.pt < 12: s.font.size=Pt(12)
 s.font.color.rgb=RGBColor(0,0,0)
 # Remove the built-in Title rule so the report stays monochrome.
 if name=='Title':
  ppr=s.element.pPr
  if ppr is not None:
   for bd in ppr.findall(qn('w:pBdr')): ppr.remove(bd)
for p in d.paragraphs:
 for r in p.runs:
  if r.font.size is not None and r.font.size.pt<12: r.font.size=Pt(12)
  r.font.color.rgb=RGBColor(0,0,0)
for t in d.tables:
 for c in t._cells:
  for p in c.paragraphs:
   for r in p.runs:
    if r.font.size is not None and r.font.size.pt<12: r.font.size=Pt(12)
    r.font.color.rgb=RGBColor(0,0,0)
# Keep the editable TOC field; Word users can update it manually with F9.
settings=d.settings.element
for e in settings.findall(qn('w:updateFields')): settings.remove(e)
d.save(path)
