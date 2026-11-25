import os
import sys
import docx
from docx.shared import Inches, Pt, RGBColor
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.enum.table import WD_TABLE_ALIGNMENT
from docx.oxml import parse_xml, OxmlElement
from docx.oxml.ns import qn, nsdecls

sys.stdout.reconfigure(encoding='utf-8')

# Color Palette - Clean Black & White (No bright colors, simple & academic)
COLOR_BLACK = RGBColor(0x00, 0x00, 0x00)
COLOR_DARK = RGBColor(0x22, 0x22, 0x22)
COLOR_MUTED = RGBColor(0x55, 0x55, 0x55)
COLOR_PRIMARY = COLOR_BLACK
COLOR_SECONDARY = COLOR_BLACK
COLOR_DARK_TEXT = COLOR_BLACK
HEX_HEADER_BG = "EAEAEA"   # Neutral light gray for table header
HEX_ROW_ALT = "FFFFFF"     # Pure white, no colorful zebra tint
HEX_BORDER = "777777"      # Clean, simple dark border

# Content width = Page Width (8.27in = 11909 dxa) - Left (1.0in = 1440 dxa) - Right (0.8in = 1152 dxa) = 9317 dxa
TOC_TAB_POS = "9317"

def setup_document_styles(doc):
    # Set page layout to A4
    for section in doc.sections:
        section.page_width = Inches(8.27)
        section.page_height = Inches(11.69)
        section.top_margin = Inches(0.85)
        section.bottom_margin = Inches(0.85)
        section.left_margin = Inches(1.0)
        section.right_margin = Inches(0.8)

        # Bật trang đầu khác biệt (trang bìa không hiện số trang)
        section.different_first_page_header_footer = True

        # Đánh số trang ở góc dưới bên phải - chỉ có số nguyên (1, 2, 3...)
        footer = section.footer
        p_foot = footer.paragraphs[0]
        p_foot.alignment = WD_ALIGN_PARAGRAPH.RIGHT
        p_foot.paragraph_format.space_after = Pt(0)
        fld_page = parse_xml(r'''
        <w:fldSimple %s w:instr="PAGE">
            <w:r>
                <w:rPr>
                    <w:rFonts w:ascii="Times New Roman" w:hAnsi="Times New Roman"/>
                    <w:sz w:val="22"/>
                    <w:color w:val="000000"/>
                </w:rPr>
                <w:t>1</w:t>
            </w:r>
        </w:fldSimple>
        ''' % nsdecls('w'))
        p_foot._p.append(fld_page)

    # Configure Normal Style - Pure Black
    normal = doc.styles['Normal']
    normal.font.name = 'Times New Roman'
    normal.font.size = Pt(13)
    normal.font.color.rgb = COLOR_BLACK
    normal.paragraph_format.line_spacing = 1.25
    normal.paragraph_format.space_after = Pt(6)

    # Configure Heading 1 Style - Black
    h1_style = doc.styles['Heading 1']
    h1_style.font.name = 'Times New Roman'
    h1_style.font.size = Pt(15)
    h1_style.font.bold = True
    h1_style.font.color.rgb = COLOR_BLACK
    h1_style.paragraph_format.space_before = Pt(14)
    h1_style.paragraph_format.space_after = Pt(6)
    h1_style.paragraph_format.keep_with_next = True

    # Configure Heading 2 Style - Black
    h2_style = doc.styles['Heading 2']
    h2_style.font.name = 'Times New Roman'
    h2_style.font.size = Pt(13.5)
    h2_style.font.bold = True
    h2_style.font.color.rgb = COLOR_BLACK
    h2_style.paragraph_format.space_before = Pt(10)
    h2_style.paragraph_format.space_after = Pt(4)
    h2_style.paragraph_format.keep_with_next = True

    # Configure Heading 3 Style - Black
    h3_style = doc.styles['Heading 3']
    h3_style.font.name = 'Times New Roman'
    h3_style.font.size = Pt(13)
    h3_style.font.bold = True
    h3_style.font.color.rgb = COLOR_BLACK
    h3_style.paragraph_format.space_before = Pt(8)
    h3_style.paragraph_format.space_after = Pt(3)
    h3_style.paragraph_format.keep_with_next = True

    # Ensure settings.xml zoom has required percent attribute for 100% schema validity
    zoom = doc.settings.element.find(qn('w:zoom'))
    if zoom is not None:
        zoom.set(qn('w:percent'), '100')

def add_toc_entry(doc, title, page_str, is_bold=False, level=0):
    p = doc.add_paragraph()
    pPr = p._p.get_or_add_pPr()

    # Cấu hình Tab dừng căn phải ở lề phải với dấu chấm dẫn (dot leader)
    tabs = parse_xml(r'''
    <w:tabs %s>
        <w:tab w:val="right" w:leader="dot" w:pos="%s"/>
    </w:tabs>
    ''' % (nsdecls('w'), TOC_TAB_POS))
    pPr.append(tabs)

    p.paragraph_format.space_after = Pt(2.5)
    p.paragraph_format.line_spacing = 1.15

    # Thụt lề theo cấp mục lục
    indent = "    " * level

    r_title = p.add_run(indent + title)
    r_title.font.name = "Times New Roman"
    r_title.font.size = Pt(11)
    r_title.font.bold = is_bold
    r_title.font.color.rgb = COLOR_BLACK

    # Ký tự Tab để kích hoạt dot leader căn thẳng hàng số trang bên phải
    p.add_run("\t")

    r_page = p.add_run(str(page_str))
    r_page.font.name = "Times New Roman"
    r_page.font.size = Pt(11)
    r_page.font.bold = is_bold
    r_page.font.color.rgb = COLOR_BLACK

    return p

def set_cell_background(cell, hex_color):
    tcPr = cell._tc.get_or_add_tcPr()
    shd = parse_xml(f'<w:shd {nsdecls("w")} w:fill="{hex_color}" w:val="clear"/>')
    tcMar = tcPr.find(qn('w:tcMar'))
    if tcMar is not None:
        tcMar.addprevious(shd)
    else:
        tcPr.append(shd)

def set_cell_margins(cell, top=60, bottom=60, left=90, right=90):
    tcPr = cell._tc.get_or_add_tcPr()
    tcMar = parse_xml(f'<w:tcMar {nsdecls("w")}><w:top w:w="{top}" w:type="dxa"/><w:left w:w="{left}" w:type="dxa"/><w:bottom w:w="{bottom}" w:type="dxa"/><w:right w:w="{right}" w:type="dxa"/></w:tcMar>')
    existing = tcPr.find(qn('w:tcMar'))
    if existing is not None:
        tcPr.remove(existing)
    tcPr.append(tcMar)

def set_table_borders(table, color="777777"):
    tblPr = table._tbl.tblPr
    # Strict OpenXML child order: top, left, bottom, right, insideH, insideV
    borders = parse_xml(
        f'<w:tblBorders {nsdecls("w")}>'
        f'<w:top w:val="single" w:sz="6" w:space="0" w:color="{color}"/>'
        f'<w:left w:val="single" w:sz="6" w:space="0" w:color="{color}"/>'
        f'<w:bottom w:val="single" w:sz="6" w:space="0" w:color="{color}"/>'
        f'<w:right w:val="single" w:sz="6" w:space="0" w:color="{color}"/>'
        f'<w:insideH w:val="single" w:sz="4" w:space="0" w:color="{color}"/>'
        f'<w:insideV w:val="single" w:sz="4" w:space="0" w:color="{color}"/>'
        f'</w:tblBorders>'
    )
    existing = tblPr.find(qn('w:tblBorders'))
    if existing is not None:
        tblPr.remove(existing)
    inserted = False
    for child in tblPr:
        if child.tag in [qn('w:tblLayout'), qn('w:tblCellMar'), qn('w:tblLook')]:
            child.addprevious(borders)
            inserted = True
            break
    if not inserted:
        tblPr.append(borders)

def make_row_cant_split(row):
    """Bắt buộc hàng của bảng không bị cắt đôi qua 2 trang"""
    trPr = row._tr.get_or_add_trPr()
    cantSplit = parse_xml(r'<w:cantSplit %s/>' % nsdecls('w'))
    trPr.append(cantSplit)

def add_h1(doc, text):
    p = doc.add_paragraph(text, style='Heading 1')
    p.alignment = WD_ALIGN_PARAGRAPH.LEFT
    return p

def add_h2(doc, text):
    p = doc.add_paragraph(text, style='Heading 2')
    p.alignment = WD_ALIGN_PARAGRAPH.LEFT
    return p

def add_h3(doc, text):
    p = doc.add_paragraph(text, style='Heading 3')
    p.alignment = WD_ALIGN_PARAGRAPH.LEFT
    return p

def add_p(doc, text, bold_prefix="", italic=False):
    p = doc.add_paragraph()
    p.paragraph_format.space_after = Pt(5)
    p.paragraph_format.line_spacing = 1.2
    p.alignment = WD_ALIGN_PARAGRAPH.JUSTIFY
    if bold_prefix:
        r_b = p.add_run(bold_prefix)
        r_b.font.name = 'Times New Roman'
        r_b.font.size = Pt(13)
        r_b.font.bold = True
        r_b.font.color.rgb = COLOR_BLACK
    r = p.add_run(text)
    r.font.name = 'Times New Roman'
    r.font.size = Pt(13)
    r.font.italic = italic
    r.font.color.rgb = COLOR_BLACK
    return p

def add_bullet(doc, text, bold_prefix="", level=0):
    p = doc.add_paragraph()
    p.paragraph_format.space_after = Pt(3)
    p.paragraph_format.line_spacing = 1.2
    p.paragraph_format.left_indent = Inches(0.25 * (level + 1))
    p.alignment = WD_ALIGN_PARAGRAPH.JUSTIFY

    text_clean = str(text).strip()
    if text_clean.startswith("•"):
        text_clean = text_clean[1:].strip()
    elif text_clean.startswith("-"):
        text_clean = text_clean[1:].strip()

    if bold_prefix and not bold_prefix.startswith("•"):
        b_pre = bold_prefix.strip()
        if not b_pre.startswith("-") and not b_pre.endswith("."):
            b_pre = "- " + b_pre + " "
        r_b = p.add_run(b_pre)
        r_b.font.name = 'Times New Roman'
        r_b.font.size = Pt(13)
        r_b.font.bold = True
        r_b.font.color.rgb = COLOR_BLACK
    else:
        r_dash = p.add_run("- ")
        r_dash.font.name = 'Times New Roman'
        r_dash.font.size = Pt(13)
        r_dash.font.color.rgb = COLOR_BLACK

    r = p.add_run(text_clean)
    r.font.name = 'Times New Roman'
    r.font.size = Pt(13)
    r.font.color.rgb = COLOR_BLACK
    return p

def add_styled_table(doc, headers, data, col_widths=None):
    """
    Tạo bảng đen trắng đơn giản, tinh tế, bắt buộc mọi hàng không bị cắt đôi (cantSplit)
    và kích thước nhỏ gọn để bảng nằm trọn vẹn trong 1 trang duy nhất.
    """
    tbl = doc.add_table(rows=len(data) + 1, cols=len(headers))
    tbl.alignment = WD_TABLE_ALIGNMENT.CENTER
    set_table_borders(tbl, HEX_BORDER)

    # Format Header Row - Nền xám nhạt đơn giản, chữ đen đậm
    hdr_row = tbl.rows[0]
    make_row_cant_split(hdr_row)
    for idx, heading in enumerate(headers):
        c = hdr_row.cells[idx]
        if col_widths and idx < len(col_widths):
            c.width = col_widths[idx]
        set_cell_background(c, HEX_HEADER_BG)
        set_cell_margins(c, top=60, bottom=60, left=80, right=80)
        p = c.paragraphs[0]
        p.alignment = WD_ALIGN_PARAGRAPH.CENTER
        p.paragraph_format.space_after = Pt(0)
        r = p.add_run(heading)
        r.font.name = 'Times New Roman'
        r.font.size = Pt(10.5)
        r.font.bold = True
        r.font.color.rgb = COLOR_BLACK

    # Format Data Rows - Nền trắng, chữ đen, cantSplit từng hàng
    for r_idx, row_data in enumerate(data):
        row = tbl.rows[r_idx + 1]
        make_row_cant_split(row)
        for c_idx, val in enumerate(row_data):
            c = row.cells[c_idx]
            if col_widths and c_idx < len(col_widths):
                c.width = col_widths[c_idx]
            set_cell_background(c, "FFFFFF")
            set_cell_margins(c, top=45, bottom=45, left=70, right=70)
            p = c.paragraphs[0]
            p.paragraph_format.space_after = Pt(0)
            p.paragraph_format.line_spacing = 1.1
            if c_idx == 0 or (len(str(val)) <= 8 and not str(val).startswith("-")):
                p.alignment = WD_ALIGN_PARAGRAPH.CENTER
            else:
                p.alignment = WD_ALIGN_PARAGRAPH.LEFT
            r = p.add_run(str(val))
            r.font.name = 'Times New Roman'
            r.font.size = Pt(10)
            r.font.color.rgb = COLOR_BLACK

    p_spacer = doc.add_paragraph()
    p_spacer.paragraph_format.space_after = Pt(4)

def add_image_box(doc, img_path, caption):
    if os.path.exists(img_path):
        p_img = doc.add_paragraph()
        p_img.alignment = WD_ALIGN_PARAGRAPH.CENTER
        p_img.paragraph_format.space_before = Pt(6)
        p_img.paragraph_format.space_after = Pt(3)
        p_img.paragraph_format.keep_with_next = True
        run_img = p_img.add_run()
        run_img.add_picture(img_path, width=Inches(5.6))

        p_cap = doc.add_paragraph()
        p_cap.alignment = WD_ALIGN_PARAGRAPH.CENTER
        p_cap.paragraph_format.space_after = Pt(8)
        r_cap = p_cap.add_run(caption)
        r_cap.font.name = 'Times New Roman'
        r_cap.font.size = Pt(10.5)
        r_cap.font.italic = True
        r_cap.font.color.rgb = COLOR_BLACK

print("Updated generate_qlks_helpers.py with Black & White styling and cantSplit tables.")
