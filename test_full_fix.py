import docx
from docx.shared import Pt, Inches
from docx.enum.text import WD_ALIGN_PARAGRAPH, WD_BREAK
from docx.oxml import parse_xml
from docx.oxml.ns import nsdecls
import xml.etree.ElementTree as ET
import os

doc = docx.Document(r"D:\qlks.docx")
body = doc._body._body
ns = {'w': 'http://schemas.openxmlformats.org/wordprocessingml/2006/main'}

# =========================================================================
# 1. FOOTER: Right-aligned, pure number, no words
# =========================================================================
for s_idx, section in enumerate(doc.sections):
    if s_idx == 0:
        continue
    ft = section.footer
    for p in ft.paragraphs:
        pPr = p._p.find('w:pPr', ns)
        if pPr is not None:
            jc = pPr.find('w:jc', ns)
            if jc is not None:
                jc.set(f'{{{ns["w"]}}}val', 'right')
            else:
                pPr.append(parse_xml(f'<w:jc {nsdecls("w")} w:val="right"/>'))
        # Remove any text run like 'Trang '
        for r in list(p._p.findall('w:r', ns)):
            t = r.find('w:t', ns)
            if t is not None and 'Trang' in (t.text or ''):
                p._p.remove(r)

# =========================================================================
# 2. PAGE BREAK BETWEEN TOC AND BẢNG PHÂN CÔNG
# =========================================================================
# In body[3] (paragraph between TOC and BẢNG PHÂN CÔNG):
p3 = body[3]
# clear p3 children and add a run with page break
for child in list(p3):
    p3.remove(child)
r_br = parse_xml(f'<w:r {nsdecls("w")}><w:br w:type="page"/></w:r>')
p3.append(r_br)

# =========================================================================
# 3. BẢNG PHÂN CÔNG NHIỆM VỤ: Fit neatly in 1 page
# =========================================================================
# Format title body[4]
p4 = body[4]
p4_pPr = p4.find('w:pPr', ns)
if p4_pPr is not None:
    jc = p4_pPr.find('w:jc', ns)
    if jc is not None:
        jc.set(f'{{{ns["w"]}}}val', 'center')
    else:
        p4_pPr.append(parse_xml(f'<w:jc {nsdecls("w")} w:val="center"/>'))

# Format table 0
t0 = doc.tables[0]
for row in t0.rows:
    for cell in row.cells:
        for p in cell.paragraphs:
            p.paragraph_format.line_spacing = 1.15
            p.paragraph_format.space_after = Pt(0)
            for r in p.runs:
                r.font.name = "Times New Roman"
                r.font.size = Pt(10.0)

# In body[6] (paragraph between Table 0 and LỜI MỞ ĐẦU):
p6 = body[6]
for child in list(p6):
    p6.remove(child)
r_br2 = parse_xml(f'<w:r {nsdecls("w")}><w:br w:type="page"/></w:r>')
p6.append(r_br2)

# =========================================================================
# 4. LỜI MỞ ĐẦU: Fit neatly in 1 page
# =========================================================================
# Find CHƯƠNG 1
idx_ch1 = -1
for idx in range(8, len(body)):
    child = body[idx]
    t = ''.join([elem.text for elem in child.findall('.//w:t', ns) if elem.text]).strip()
    if 'CHƯƠNG 1' in t:
        idx_ch1 = idx
        break

print(f"LỜI MỞ ĐẦU spans from body[8] to body[{idx_ch1-1}]")

# Remove old paragraphs from body[8] to body[idx_ch1-1]
for _ in range(idx_ch1 - 8):
    body.remove(body[8])

# Refined 5 paragraphs for Lời mở đầu
intro_texts = [
    "Trong bối cảnh nền kinh tế số và du lịch phát triển mạnh mẽ, việc ứng dụng công nghệ thông tin vào quản lý lưu trú đóng vai trò then chốt nhằm nâng cao năng lực cạnh tranh, tối ưu hóa chi phí vận hành và đảm bảo tính chính xác của dữ liệu. Khảo sát thực tế tại các khách sạn, nhà nghỉ quy mô vừa và nhỏ cho thấy phương pháp quản lý thủ công bằng sổ sách hay bảng tính Excel rời rạc còn bộc lộ nhiều bất cập lớn: nguy cơ xung đột lịch phòng (Overbooking), khó kiểm soát thất thoát tồn kho minibar, dễ nhầm lẫn doanh thu ca trực và tốc độ phục vụ khách hàng còn chậm trễ.",
    "Xuất phát từ thực tiễn cấp thiết đó, nhóm sinh viên chúng em đã nghiên cứu và xây dựng đề tài “Xây dựng phần mềm Quản lý Khách sạn (Hotel Management System)” trên nền tảng Microsoft .NET 10 với ngôn ngữ lập trình C# hiện đại, kết hợp cùng hệ quản trị cơ sở dữ liệu quan hệ mạnh mẽ Microsoft SQL Server. Chương trình tuân thủ nghiêm ngặt mô hình kiến trúc phân tầng 3-Tier (GUI – BLL – DAL – DTO), áp dụng các chuẩn mực kỹ thuật tiên tiến: kiểm soát giao dịch toàn vẹn ACID với khóa ứng dụng độc quyền sys.sp_getapplock, mã hóa mật khẩu PBKDF2 SHA-512, tự động hóa tính tiền lưu trú block 24 giờ và tích hợp thanh toán quét mã VietQR động chuẩn NAPAS 247.",
    "Bản báo cáo đồ án được kết cấu khoa học thành bốn chương chính: Chương 1 trình bày tổng quan mục tiêu, khảo sát hiện trạng, đối tượng sử dụng và các quy tắc nghiệp vụ cốt lõi; Chương 2 đi sâu phân tích đặc tả chức năng, kiến trúc đa tầng và ma trận CRUD; Chương 3 thiết kế chi tiết 20 bảng cơ sở dữ liệu chuẩn hóa 3NF, cơ chế tự động nâng cấp SchemaMigrator và các ràng buộc toàn vẹn; Chương 4 giới thiệu giao diện người dùng, kịch bản kiểm thử toàn diện và hướng dẫn vận hành hệ thống.",
    "Mặc dù nhóm chúng em đã dành trọn tâm huyết và nỗ lực hoàn thiện đề tài với tinh thần trách nhiệm cao nhất, song do giới hạn về kiến thức và kinh nghiệm thực tế, chương trình và báo cáo khó tránh khỏi những điểm còn thiếu sót. Nhóm chúng em rất mong nhận được những nhận xét, góp ý quý báu và sự chỉ bảo tận tình của Cô Trương Thị Nhung cùng các Thầy/Cô bộ môn để sản phẩm ngày càng hoàn thiện hơn.",
    "Chúng em xin chân thành cảm ơn!"
]

# We insert paragraphs before body[8] (which is now CHƯƠNG 1)
# To create clean paragraphs with python-docx, let's build valid XML elements
insert_idx = 8
for p_i, text in enumerate(intro_texts):
    is_last = (p_i == len(intro_texts) - 1)
    
    # Valid Word XML: pPr -> spacing, jc -> r -> rPr -> rFonts, b, i, sz -> t
    rPr_xml = '<w:rPr><w:rFonts w:ascii="Times New Roman" w:hAnsi="Times New Roman" w:cs="Times New Roman"/>'
    if is_last:
        rPr_xml += '<w:b/><w:i/>'
    rPr_xml += '<w:sz w:val="25"/><w:szCs w:val="25"/></w:rPr>'
    
    p_xml = (
        f'<w:p {nsdecls("w")}>'
        f'<w:pPr>'
        f'<w:pStyle w:val="Normal"/>'
        f'<w:spacing w:line="276" w:lineRule="auto" w:after="80"/>'
        f'<w:jc w:val="both"/>'
        f'</w:pPr>'
        f'<w:r>'
        f'{rPr_xml}'
        f'<w:t>{text}</w:t>'
        f'</w:r>'
    )
    if is_last:
        p_xml += f'<w:r><w:br w:type="page"/></w:r>'
    p_xml += '</w:p>'
    
    p_elem = parse_xml(p_xml)
    body.insert(insert_idx, p_elem)
    insert_idx += 1

doc.save("test_full_fixed.docx")
print("Saved test_full_fixed.docx successfully")
