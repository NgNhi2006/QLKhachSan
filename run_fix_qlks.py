import docx
from docx.shared import Inches, Pt, RGBColor
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.enum.table import WD_TABLE_ALIGNMENT
from docx.oxml import parse_xml
from docx.oxml.ns import nsdecls, qn
import os
import shutil
import sys

sys.stdout.reconfigure(encoding='utf-8')

def set_cell_margins(cell, top=60, bottom=60, left=100, right=100):
    tcPr = cell._tc.get_or_add_tcPr()
    tcMar = parse_xml(f'<w:tcMar {nsdecls("w")}><w:top w:w="{top}" w:type="dxa"/><w:bottom w:w="{bottom}" w:type="dxa"/><w:left w:w="{left}" w:type="dxa"/><w:right w:w="{right}" w:type="dxa"/></w:tcMar>')
    tcPr.append(tcMar)

def add_page_break_to_p(p):
    r = parse_xml(f'<w:r {nsdecls("w")}><w:br w:type="page"/></w:r>')
    p._p.append(r)

def fix_document():
    src_path = r"D:\qlks.docx"
    print(f"1. Đang đọc tệp: {src_path}")
    doc = docx.Document(src_path)
    body = doc._body._body
    ns = {'w': 'http://schemas.openxmlformats.org/wordprocessingml/2006/main'}

    # =========================================================================
    # A. CHỈNH SỐ TRANG Ở GÓC DƯỚI BÊN PHẢI (CHỈ THUẦN SỐ, KHÔNG CHỮ)
    # =========================================================================
    print("2. Chỉnh sửa chân trang (Footer): góc dưới bên phải, thuần số...")
    for s_idx, section in enumerate(doc.sections):
        if s_idx == 0:
            # Trang bìa: không để số trang
            continue
        ft = section.footer
        p = ft.paragraphs[0]
        p._p.clear()

        # Tạo đoạn footer căn phải
        pPr = parse_xml(f'<w:pPr {nsdecls("w")}><w:pStyle w:val="Footer"/><w:jc w:val="right"/></w:pPr>')
        p._p.append(pPr)

        # Chèn duy nhất trường số trang PAGE
        r1 = parse_xml(f'<w:r {nsdecls("w")}><w:rPr><w:sz w:val="22"/><w:szCs w:val="22"/></w:rPr><w:fldChar w:fldCharType="begin"/></w:r>')
        r2 = parse_xml(f'<w:r {nsdecls("w")}><w:rPr><w:sz w:val="22"/><w:szCs w:val="22"/></w:rPr><w:instrText>PAGE</w:instrText></w:r>')
        r3 = parse_xml(f'<w:r {nsdecls("w")}><w:rPr><w:sz w:val="22"/><w:szCs w:val="22"/></w:rPr><w:fldChar w:fldCharType="separate"/></w:r>')
        r4 = parse_xml(f'<w:r {nsdecls("w")}><w:rPr><w:sz w:val="22"/><w:szCs w:val="22"/></w:rPr><w:t>1</w:t></w:r>')
        r5 = parse_xml(f'<w:r {nsdecls("w")}><w:rPr><w:sz w:val="22"/><w:szCs w:val="22"/></w:rPr><w:fldChar w:fldCharType="end"/></w:r>')

        p._p.append(r1)
        p._p.append(r2)
        p._p.append(r3)
        p._p.append(r4)
        p._p.append(r5)

    # =========================================================================
    # B. BẢNG PHÂN CÔNG NHIỆM VỤ: GÓI GỌN TRỌN VẸN TRONG ĐÚNG 1 TRANG
    # =========================================================================
    print("3. Tối ưu Bảng phân công nhiệm vụ để nằm gọn trong 1 trang...")
    # body[3] là đoạn văn nằm giữa TOC và tiêu đề BẢNG PHÂN CÔNG
    p_before_assign = body[3]
    # Đảm bảo có ngắt trang trước Bảng phân công
    has_page_break_before = any(b.attrib.get(f'{{{ns["w"]}}}type') == 'page' for b in p_before_assign.findall('.//w:br', ns))
    if not has_page_break_before:
        r_pb = parse_xml(f'<w:r {nsdecls("w")}><w:br w:type="page"/></w:r>')
        p_before_assign.append(r_pb)

    # body[4] là tiêu đề: BẢNG PHÂN CÔNG CÔNG VIỆC NHÓM
    p_assign_title = body[4]
    pPr_assign = p_assign_title.find('w:pPr', ns)
    if pPr_assign is None:
        pPr_assign = parse_xml(f'<w:pPr {nsdecls("w")}/>')
        p_assign_title.insert(0, pPr_assign)
    # Căn giữa, khoảng cách trước sau hợp lý
    jc_elem = pPr_assign.find('w:jc', ns)
    if jc_elem is None:
        pPr_assign.append(parse_xml(f'<w:jc {nsdecls("w")} w:val="center"/>'))
    else:
        jc_elem.set(f'{{{ns["w"]}}}val', 'center')
    sp_elem = pPr_assign.find('w:spacing', ns)
    if sp_elem is None:
        pPr_assign.append(parse_xml(f'<w:spacing {nsdecls("w")} w:before="160" w:after="160"/>'))
    else:
        sp_elem.set(f'{{{ns["w"]}}}before', '160')
        sp_elem.set(f'{{{ns["w"]}}}after', '160')

    # Bảng phân công (doc.tables[0])
    tbl_assign = doc.tables[0]
    tbl_assign.alignment = WD_TABLE_ALIGNMENT.CENTER
    # Chỉnh đệm ô và cỡ chữ cho tất cả các ô trong bảng phân công
    for row in tbl_assign.rows:
        for cell in row.cells:
            set_cell_margins(cell, top=50, bottom=50, left=100, right=100)
            for p_cell in cell.paragraphs:
                p_cell.paragraph_format.line_spacing = 1.15
                p_cell.paragraph_format.space_after = Pt(0)
                for r_cell in p_cell.runs:
                    r_cell.font.name = "Times New Roman"
                    r_cell.font.size = Pt(10.5)

    # body[6] là đoạn văn nằm giữa Table 0 và LỜI MỞ ĐẦU
    # Đặt ngắt trang tại đây để Bảng phân công kết thúc trọn vẹn trang đó!
    p_after_assign = body[6]
    p_after_assign.clear()
    r_pb2 = parse_xml(f'<w:r {nsdecls("w")}><w:br w:type="page"/></w:r>')
    p_after_assign.append(r_pb2)

    # =========================================================================
    # C. LỜI MỞ ĐẦU: GÓI GỌN TRỌN VẸN TRONG ĐÚNG 1 TRANG
    # =========================================================================
    print("4. Tối ưu Lời mở đầu để nằm gọn trọn vẹn trong đúng 1 trang...")
    # body[7] là tiêu đề: LỜI MỞ ĐẦU (Heading 1)
    p_intro_title = body[7]
    pPr_intro = p_intro_title.find('w:pPr', ns)
    if pPr_intro is not None:
        sp_intro = pPr_intro.find('w:spacing', ns)
        if sp_intro is None:
            pPr_intro.append(parse_xml(f'<w:spacing {nsdecls("w")} w:before="160" w:after="120"/>'))
        else:
            sp_intro.set(f'{{{ns["w"]}}}before', '160')
            sp_intro.set(f'{{{ns["w"]}}}after', '120')

    # Tìm vị trí bắt đầu CHƯƠNG 1 trong body
    idx_ch1 = -1
    for idx in range(8, len(body)):
        child = body[idx]
        t = ''.join([elem.text for elem in child.findall('.//w:t', ns) if elem.text]).strip()
        if 'CHƯƠNG 1' in t:
            idx_ch1 = idx
            break

    print(f"Vị trí Lời mở đầu từ body[8] đến body[{idx_ch1 - 1}]")

    # Xóa các đoạn cũ của Lời mở đầu (body[8] đến body[idx_ch1 - 1])
    for _ in range(idx_ch1 - 8):
        body.remove(body[8])

    # Tạo 5 đoạn văn tinh tế, súc tích, vừa vặn hoàn hảo trong 1 trang A4:
    intro_paragraphs = [
        "Trong bối cảnh nền kinh tế số và du lịch phát triển mạnh mẽ, việc ứng dụng công nghệ thông tin vào quản lý lưu trú đóng vai trò then chốt nhằm nâng cao năng lực cạnh tranh, tối ưu hóa chi phí vận hành và đảm bảo tính chính xác của dữ liệu. Khảo sát thực tế tại các khách sạn, nhà nghỉ quy mô vừa và nhỏ cho thấy phương pháp quản lý thủ công bằng sổ sách hay bảng tính Excel rời rạc còn bộc lộ nhiều bất cập lớn: nguy cơ xung đột lịch phòng (Overbooking), khó kiểm soát thất thoát tồn kho minibar, dễ nhầm lẫn doanh thu ca trực và tốc độ phục vụ khách hàng còn chậm trễ.",

        "Xuất phát từ thực tiễn cấp thiết đó, nhóm sinh viên chúng em đã nghiên cứu và xây dựng đề tài “Xây dựng phần mềm Quản lý Khách sạn (Hotel Management System)” trên nền tảng Microsoft .NET 10 với ngôn ngữ lập trình C# hiện đại, kết hợp cùng hệ quản trị cơ sở dữ liệu quan hệ mạnh mẽ Microsoft SQL Server. Chương trình tuân thủ nghiêm ngặt mô hình kiến trúc phân tầng 3-Tier (GUI – BLL – DAL – DTO), áp dụng các chuẩn mực kỹ thuật tiên tiến: kiểm soát giao dịch toàn vẹn ACID với khóa ứng dụng độc quyền sys.sp_getapplock, mã hóa mật khẩu PBKDF2 SHA-512, tự động hóa tính tiền lưu trú block 24 giờ và tích hợp thanh toán quét mã VietQR động chuẩn NAPAS 247.",

        "Bản báo cáo đồ án được kết cấu khoa học thành bốn chương chính: Chương 1 trình bày tổng quan mục tiêu, khảo sát hiện trạng, đối tượng sử dụng và các quy tắc nghiệp vụ cốt lõi; Chương 2 đi sâu phân tích đặc tả chức năng, kiến trúc đa tầng và ma trận CRUD; Chương 3 thiết kế chi tiết 20 bảng cơ sở dữ liệu chuẩn hóa 3NF, cơ chế tự động nâng cấp SchemaMigrator và các ràng buộc toàn vẹn; Chương 4 giới thiệu giao diện người dùng, kịch bản kiểm thử toàn diện và hướng dẫn vận hành hệ thống.",

        "Mặc dù nhóm chúng em đã dành trọn tâm huyết và nỗ lực hoàn thiện đề tài với tinh thần trách nhiệm cao nhất, song do giới hạn về kiến thức và kinh nghiệm thực tế, chương trình và báo cáo khó tránh khỏi những điểm còn thiếu sót. Nhóm chúng em rất mong nhận được những nhận xét, góp ý quý báu và sự chỉ bảo tận tình của Cô Trương Thị Nhung cùng các Thầy/Cô bộ môn để sản phẩm ngày càng hoàn thiện hơn.",

        "Chúng em xin chân thành cảm ơn!"
    ]

    # Chèn các đoạn văn mới vào vị trí body[8] trở đi
    insert_pos = 8
    for p_idx, text_content in enumerate(intro_paragraphs):
        p_new = parse_xml(
            f'<w:p {nsdecls("w")}>'
            f'<w:pPr>'
            f'<w:pStyle w:val="Normal"/>'
            f'<w:jc w:val="justify"/>'
            f'<w:spacing w:line="276" w:lineRule="auto" w:after="80"/>'
            f'</w:pPr>'
            f'<w:r>'
            f'<w:rPr>'
            f'<w:rFonts w:ascii="Times New Roman" w:hAnsi="Times New Roman" w:cs="Times New Roman"/>'
            f'<w:sz w:val="25"/>'
            f'<w:szCs w:val="25"/>'
            f'{"<w:i/>" if p_idx == len(intro_paragraphs) - 1 else ""}'
            f'{"<w:b/>" if p_idx == len(intro_paragraphs) - 1 else ""}'
            f'</w:rPr>'
            f'<w:t>{text_content}</w:t>'
            f'</w:r>'
            f'</w:p>'
        )

        # Đoạn cuối cùng ("Chúng em xin chân thành cảm ơn!") kèm ngắt trang để kết thúc trang Lời mở đầu!
        if p_idx == len(intro_paragraphs) - 1:
            r_br = parse_xml(f'<w:r {nsdecls("w")}><w:br w:type="page"/></w:r>')
            p_new.append(r_br)

        body.insert(insert_pos, p_new)
        insert_pos += 1

    # Lưu kết quả ra file
    output_path = r"D:\qlks_fixed.docx"
    doc.save(output_path)
    print(f"5. Đã lưu thành công tệp mới: {output_path}")

    # Thử ghi đè trực tiếp lên D:\qlks.docx nếu Word đã được đóng
    try:
        shutil.copy2(output_path, src_path)
        print(f"-> Đã ghi đè trực tiếp lên {src_path}")
    except Exception as e:
        print(f"-> Chưa ghi đè được D:\\qlks.docx do tệp đang mở trong Word: {e}")

    # Copy một bản sang Downloads
    downloads_path = r"C:\Users\Nguyen Nhi\Downloads\qlks.docx"
    try:
        shutil.copy2(output_path, downloads_path)
        print(f"-> Đã lưu bản sao sang: {downloads_path}")
    except Exception as e:
        print(f"-> Lỗi sao chép Downloads: {e}")

if __name__ == "__main__":
    fix_document()
