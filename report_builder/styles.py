import docx
from docx.shared import Inches, Pt, RGBColor
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.enum.table import WD_TABLE_ALIGNMENT
from docx.oxml import OxmlElement, parse_xml
from docx.oxml.ns import qn, nsdecls
import os

# Primary Colors matching sample report & professional corporate theme
COLOR_PRIMARY = RGBColor(0x1F, 0x4E, 0x79)    # #1F4E79 - Deep Blue
COLOR_SECONDARY = RGBColor(0x2E, 0x5B, 0x82)  # #2E5B82 - Slate Blue
COLOR_TEXT = RGBColor(0x22, 0x22, 0x22)       # #222222 - Dark Charcoal
COLOR_MUTED = RGBColor(0x55, 0x55, 0x55)      # #555555 - Muted Grey
COLOR_HEADER_BG = "D9E6F8"                   # Sample table header soft blue
COLOR_ALT_ROW = "F8FAFD"                     # Soft zebra stripe
COLOR_BORDER = "A6B9D0"                      # Clean border color

def setup_document_styles(doc):
    for section in doc.sections:
        section.page_width = Inches(8.27)    # 210mm (A4)
        section.page_height = Inches(11.69)  # 297mm (A4)
        section.left_margin = Inches(1.18)   # 3.0 cm
        section.right_margin = Inches(0.79)  # 2.0 cm
        section.top_margin = Inches(0.79)    # 2.0 cm
        section.bottom_margin = Inches(0.79) # 2.0 cm

        footer = section.footer
        p_ft = footer.paragraphs[0]
        p_ft.alignment = WD_ALIGN_PARAGRAPH.RIGHT
        p_ft.paragraph_format.space_before = Pt(0)
        p_ft.paragraph_format.space_after = Pt(0)
        r_ft = p_ft.add_run("Báo Cáo Đồ Án: Hệ Thống Quản Lý Khách Sạn | Trang ")
        r_ft.font.name = "Times New Roman"
        r_ft.font.size = Pt(9)
        r_ft.font.italic = True
        r_ft.font.color.rgb = RGBColor(0x77, 0x77, 0x77)
        add_page_number_to_run(p_ft.add_run())

    style_normal = doc.styles["Normal"]
    style_normal.font.name = "Times New Roman"
    style_normal.font.size = Pt(12.5)
    style_normal.font.color.rgb = COLOR_TEXT
    style_normal.paragraph_format.line_spacing = 1.25
    style_normal.paragraph_format.space_after = Pt(4)

def add_page_number_to_run(run):
    run.font.name = "Times New Roman"
    run.font.size = Pt(9)
    run.font.color.rgb = RGBColor(0x77, 0x77, 0x77)
    fldSimple = parse_xml(r'<w:fldSimple %s w:instr="PAGE"/>' % nsdecls('w'))
    run._r.append(fldSimple)

def set_cell_background(cell, hex_color):
    tcPr = cell._tc.get_or_add_tcPr()
    shd = parse_xml(f'<w:shd {nsdecls("w")} w:fill="{hex_color}" w:val="clear"/>')
    tcPr.append(shd)

def set_cell_margins(cell, top=100, bottom=100, left=150, right=150):
    tcPr = cell._tc.get_or_add_tcPr()
    tcMar = parse_xml(f'<w:tcMar {nsdecls("w")}><w:top w:w="{top}" w:type="dxa"/><w:bottom w:w="{bottom}" w:type="dxa"/><w:left w:w="{left}" w:type="dxa"/><w:right w:w="{right}" w:type="dxa"/></w:tcMar>')
    tcPr.append(tcMar)

def set_table_borders(table, color=COLOR_BORDER, sz="4", val="single"):
    tblPr = table._tbl.tblPr
    borders = parse_xml(
        f'<w:tblBorders {nsdecls("w")}>'
        f'<w:top w:val="{val}" w:sz="{sz}" w:space="0" w:color="{color}"/>'
        f'<w:bottom w:val="{val}" w:sz="{sz}" w:space="0" w:color="{color}"/>'
        f'<w:left w:val="{val}" w:sz="{sz}" w:space="0" w:color="{color}"/>'
        f'<w:right w:val="{val}" w:sz="{sz}" w:space="0" w:color="{color}"/>'
        f'<w:insideH w:val="{val}" w:sz="{sz}" w:space="0" w:color="{color}"/>'
        f'<w:insideV w:val="{val}" w:sz="{sz}" w:space="0" w:color="{color}"/>'
        f'</w:tblBorders>'
    )
    tblPr.append(borders)

def add_h1(doc, text):
    p = doc.add_paragraph()
    p.paragraph_format.space_before = Pt(16)
    p.paragraph_format.space_after = Pt(8)
    p.paragraph_format.keep_with_next = True
    run = p.add_run(text)
    run.font.name = "Times New Roman"
    run.font.size = Pt(15.5)
    run.font.bold = True
    run.font.color.rgb = COLOR_PRIMARY
    return p

def add_h2(doc, text):
    p = doc.add_paragraph()
    p.paragraph_format.space_before = Pt(12)
    p.paragraph_format.space_after = Pt(5)
    p.paragraph_format.keep_with_next = True
    run = p.add_run(text)
    run.font.name = "Times New Roman"
    run.font.size = Pt(13.5)
    run.font.bold = True
    run.font.color.rgb = COLOR_SECONDARY
    return p

def add_h3(doc, text):
    p = doc.add_paragraph()
    p.paragraph_format.space_before = Pt(8)
    p.paragraph_format.space_after = Pt(3)
    p.paragraph_format.keep_with_next = True
    run = p.add_run(text)
    run.font.name = "Times New Roman"
    run.font.size = Pt(12.5)
    run.font.bold = True
    run.font.color.rgb = COLOR_TEXT
    return p

def add_p(doc, text, bold=False, italic=False, align=WD_ALIGN_PARAGRAPH.JUSTIFY, space_after=5):
    p = doc.add_paragraph()
    p.alignment = align
    p.paragraph_format.line_spacing = 1.25
    p.paragraph_format.space_after = Pt(space_after)
    run = p.add_run(text)
    run.font.name = "Times New Roman"
    run.font.size = Pt(12.5)
    run.font.bold = bold
    run.font.italic = italic
    run.font.color.rgb = COLOR_TEXT
    return p

def add_bullet(doc, text, bold_prefix="", level=0):
    p = doc.add_paragraph(style="List Bullet")
    p.alignment = WD_ALIGN_PARAGRAPH.JUSTIFY
    p.paragraph_format.line_spacing = 1.2
    p.paragraph_format.space_after = Pt(3)
    p.paragraph_format.left_indent = Inches(0.25 * (level + 1))
    if bold_prefix:
        r_pre = p.add_run(bold_prefix)
        r_pre.font.name = "Times New Roman"
        r_pre.font.size = Pt(12.5)
        r_pre.font.bold = True
        r_pre.font.color.rgb = COLOR_TEXT
    run = p.add_run(text)
    run.font.name = "Times New Roman"
    run.font.size = Pt(12.5)
    run.font.color.rgb = COLOR_TEXT
    return p

def add_callout(doc, text, title="Lưu ý nghiệp vụ:"):
    tbl = doc.add_table(rows=1, cols=1)
    tbl.alignment = WD_TABLE_ALIGNMENT.CENTER
    c = tbl.cell(0, 0)
    set_cell_background(c, "F2F5F9")
    set_cell_margins(c, top=120, bottom=120, left=180, right=180)

    tcPr = c._tc.get_or_add_tcPr()
    borders = parse_xml(
        f'<w:tcBorders {nsdecls("w")}>'
        f'<w:top w:val="none"/>'
        f'<w:left w:val="single" w:sz="24" w:space="0" w:color="1F4E79"/>'
        f'<w:bottom w:val="none"/>'
        f'<w:right w:val="none"/>'
        f'</w:tcBorders>'
    )
    tcPr.append(borders)

    p = c.paragraphs[0]
    p.alignment = WD_ALIGN_PARAGRAPH.JUSTIFY
    p.paragraph_format.line_spacing = 1.2
    p.paragraph_format.space_after = Pt(2)
    r1 = p.add_run(title + " ")
    r1.font.name = "Times New Roman"
    r1.font.size = Pt(11.5)
    r1.font.bold = True
    r1.font.color.rgb = COLOR_PRIMARY

    r2 = p.add_run(text)
    r2.font.name = "Times New Roman"
    r2.font.size = Pt(11.5)
    r2.font.italic = True
    r2.font.color.rgb = COLOR_TEXT

    p_after = doc.add_paragraph()
    p_after.paragraph_format.space_before = Pt(0)
    p_after.paragraph_format.space_after = Pt(4)

def add_code_block(doc, code_text):
    tbl = doc.add_table(rows=1, cols=1)
    tbl.alignment = WD_TABLE_ALIGNMENT.CENTER
    c = tbl.cell(0, 0)
    set_cell_background(c, "F5F7FA")
    set_cell_margins(c, top=100, bottom=100, left=150, right=150)

    tcPr = c._tc.get_or_add_tcPr()
    borders = parse_xml(
        f'<w:tcBorders {nsdecls("w")}>'
        f'<w:top w:val="single" w:sz="6" w:space="0" w:color="D0D7DE"/>'
        f'<w:left w:val="single" w:sz="18" w:space="0" w:color="0969DA"/>'
        f'<w:bottom w:val="single" w:sz="6" w:space="0" w:color="D0D7DE"/>'
        f'<w:right w:val="single" w:sz="6" w:space="0" w:color="D0D7DE"/>'
        f'</w:tcBorders>'
    )
    tcPr.append(borders)

    lines = code_text.strip().split("\n")
    p = c.paragraphs[0]
    p.paragraph_format.line_spacing = 1.15
    p.paragraph_format.space_after = Pt(0)

    for i, line in enumerate(lines):
        if i > 0:
            p = c.add_paragraph()
            p.paragraph_format.line_spacing = 1.15
            p.paragraph_format.space_after = Pt(0)
        r = p.add_run(line)
        r.font.name = "Consolas"
        r.font.size = Pt(9.5)
        r.font.color.rgb = RGBColor(0x24, 0x29, 0x2F)

    p_after = doc.add_paragraph()
    p_after.paragraph_format.space_before = Pt(0)
    p_after.paragraph_format.space_after = Pt(4)

def create_styled_table(doc, headers, data, col_widths=None, alignments=None, caption=None):
    if caption:
        p_cap = doc.add_paragraph()
        p_cap.alignment = WD_ALIGN_PARAGRAPH.CENTER
        p_cap.paragraph_format.space_before = Pt(8)
        p_cap.paragraph_format.space_after = Pt(4)
        p_cap.paragraph_format.keep_with_next = True
        r_cap = p_cap.add_run(caption)
        r_cap.font.name = "Times New Roman"
        r_cap.font.size = Pt(11.5)
        r_cap.font.bold = True
        r_cap.font.color.rgb = COLOR_TEXT

    num_cols = len(headers)
    tbl = doc.add_table(rows=len(data) + 1, cols=num_cols)
    tbl.alignment = WD_TABLE_ALIGNMENT.CENTER
    set_table_borders(tbl, color="B0C4DE", sz="4", val="single")

    hdr_row = tbl.rows[0]
    trPr = hdr_row._tr.get_or_add_trPr()
    trPr.append(parse_xml(f'<w:tblHeader {nsdecls("w")}/>'))

    for j, h_text in enumerate(headers):
        cell = hdr_row.cells[j]
        set_cell_background(cell, COLOR_HEADER_BG)
        set_cell_margins(cell, top=110, bottom=110, left=110, right=110)
        p = cell.paragraphs[0]
        p.alignment = WD_ALIGN_PARAGRAPH.CENTER
        p.paragraph_format.space_after = Pt(0)
        p.paragraph_format.line_spacing = 1.15
        r = p.add_run(h_text)
        r.font.name = "Times New Roman"
        r.font.size = Pt(11.0)
        r.font.bold = True
        r.font.color.rgb = RGBColor(0x11, 0x2E, 0x51)

    for i, row_data in enumerate(data):
        row = tbl.rows[i + 1]
        is_even = (i % 2 == 1)
        bg = COLOR_ALT_ROW if is_even else "FFFFFF"

        for j, val in enumerate(row_data):
            cell = row.cells[j]
            set_cell_background(cell, bg)
            set_cell_margins(cell, top=80, bottom=80, left=110, right=110)
            p = cell.paragraphs[0]
            align = alignments[j] if alignments and j < len(alignments) else WD_ALIGN_PARAGRAPH.LEFT
            p.alignment = align
            p.paragraph_format.space_after = Pt(0)
            p.paragraph_format.line_spacing = 1.15
            r = p.add_run(str(val))
            r.font.name = "Times New Roman"
            r.font.size = Pt(11.0)
            r.font.color.rgb = COLOR_TEXT

    if col_widths:
        for row in tbl.rows:
            for j, w in enumerate(col_widths):
                if j < len(row.cells):
                    row.cells[j].width = Inches(w)

    p_after = doc.add_paragraph()
    p_after.paragraph_format.space_before = Pt(0)
    p_after.paragraph_format.space_after = Pt(6)
    return tbl

def add_image_box(doc, img_path, caption_text, width_inches=6.0):
    if not os.path.exists(img_path):
        p_err = doc.add_paragraph(f"[Ảnh minh họa: {caption_text}]")
        p_err.alignment = WD_ALIGN_PARAGRAPH.CENTER
        return

    p_img = doc.add_paragraph()
    p_img.alignment = WD_ALIGN_PARAGRAPH.CENTER
    p_img.paragraph_format.space_before = Pt(8)
    p_img.paragraph_format.space_after = Pt(3)
    p_img.paragraph_format.keep_with_next = True

    run_img = p_img.add_run()
    run_img.add_picture(img_path, width=Inches(width_inches))

    p_cap = doc.add_paragraph()
    p_cap.alignment = WD_ALIGN_PARAGRAPH.CENTER
    p_cap.paragraph_format.space_before = Pt(2)
    p_cap.paragraph_format.space_after = Pt(8)
    run_cap = p_cap.add_run(caption_text)
    run_cap.font.name = "Times New Roman"
    run_cap.font.size = Pt(10.5)
    run_cap.font.italic = True
    run_cap.font.color.rgb = COLOR_MUTED
