import os
import sys
import docx
from docx.shared import Inches, Pt, RGBColor
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.enum.table import WD_TABLE_ALIGNMENT
from docx.oxml import parse_xml
from docx.oxml.ns import nsdecls, qn

from generate_qlks_helpers import (
    add_h1, add_h2, add_h3, add_p, add_bullet, add_styled_table, add_toc_entry,
    COLOR_BLACK, COLOR_DARK, COLOR_MUTED,
    HEX_HEADER_BG, HEX_ROW_ALT, HEX_BORDER, set_cell_background, set_cell_margins
)

def build_cover_page(doc):
    # Khung bìa đen trắng đơn giản trang nhã giống mẫu
    tbl = doc.add_table(rows=1, cols=1)
    tbl.alignment = WD_TABLE_ALIGNMENT.CENTER
    c = tbl.cell(0, 0)
    c.width = Inches(6.8)

    tcPr = c._tc.get_or_add_tcPr()
    borders = parse_xml(
        f'<w:tcBorders {nsdecls("w")}>'
        f'<w:top w:val="double" w:sz="18" w:space="0" w:color="000000"/>'
        f'<w:left w:val="double" w:sz="18" w:space="0" w:color="000000"/>'
        f'<w:bottom w:val="double" w:sz="18" w:space="0" w:color="000000"/>'
        f'<w:right w:val="double" w:sz="18" w:space="0" w:color="000000"/>'
        f'</w:tcBorders>'
    )
    tcPr.append(borders)
    set_cell_margins(c, top=260, bottom=260, left=300, right=300)

    p_inst = c.paragraphs[0]
    p_inst.alignment = WD_ALIGN_PARAGRAPH.CENTER
    p_inst.paragraph_format.line_spacing = 1.15
    p_inst.paragraph_format.space_after = Pt(2)
    r_inst = p_inst.add_run("UBND TỈNH BẮC NINH\nTRƯỜNG CAO ĐẲNG CÔNG NGHỆ VIỆT - HÀN")
    r_inst.font.name = "Times New Roman"
    r_inst.font.size = Pt(13)
    r_inst.font.bold = True
    r_inst.font.color.rgb = COLOR_BLACK

    p_sep = c.add_paragraph()
    p_sep.alignment = WD_ALIGN_PARAGRAPH.CENTER
    p_sep.paragraph_format.space_before = Pt(4)
    p_sep.paragraph_format.space_after = Pt(36)
    r_sep = p_sep.add_run("═════════ ★ ═════════")
    r_sep.font.name = "Times New Roman"
    r_sep.font.size = Pt(11)
    r_sep.font.color.rgb = COLOR_BLACK

    p_sub = c.add_paragraph()
    p_sub.alignment = WD_ALIGN_PARAGRAPH.CENTER
    p_sub.paragraph_format.space_after = Pt(6)
    r_sub = p_sub.add_run("BÁO CÁO ĐỒ ÁN KẾT THÚC MÔN\nMôn học : LẬP TRÌNH WINDOWS")
    r_sub.font.name = "Times New Roman"
    r_sub.font.size = Pt(15)
    r_sub.font.bold = True
    r_sub.font.color.rgb = COLOR_BLACK

    p_title = c.add_paragraph()
    p_title.alignment = WD_ALIGN_PARAGRAPH.CENTER
    p_title.paragraph_format.space_before = Pt(24)
    p_title.paragraph_format.space_after = Pt(36)
    r_tp = p_title.add_run("TÊN ĐỀ TÀI: QUẢN LÝ KHÁCH SẠN")
    r_tp.font.name = "Times New Roman"
    r_tp.font.size = Pt(18)
    r_tp.font.bold = True
    r_tp.font.color.rgb = COLOR_BLACK

    # Thông tin giảng viên & nhóm sinh viên
    meta_tbl = c.add_table(rows=4, cols=2)
    meta_tbl.alignment = WD_TABLE_ALIGNMENT.CENTER
    meta_widths = [Inches(2.5), Inches(3.7)]
    meta_data = [
        ("Giảng viên hướng dẫn:", "TRƯƠNG THỊ NHUNG"),
        ("Nhóm thực hiện:", "NGUYỄN NGỌC THUẦN NHI\nNGUYỄN THỊ THU HÀ\nNGUYỄN ĐỨC ANH TUẤN\nLÊ QUỐC VƯỢNG\nHOÀNG ANH NGỌC"),
        ("Lớp:", "IT1.11.B"),
        ("Thời gian thực hiện:", "Bắc Ninh, năm 2026")
    ]
    for row_idx, (lbl, val) in enumerate(meta_data):
        row = meta_tbl.rows[row_idx]
        c_lbl = row.cells[0]
        c_lbl.width = meta_widths[0]
        set_cell_margins(c_lbl, top=40, bottom=40, left=40, right=40)
        p_l = c_lbl.paragraphs[0]
        p_l.paragraph_format.space_after = Pt(0)
        r_l = p_l.add_run(lbl)
        r_l.font.name = "Times New Roman"
        r_l.font.size = Pt(12)
        r_l.font.bold = True
        r_l.font.color.rgb = COLOR_BLACK

        c_val = row.cells[1]
        c_val.width = meta_widths[1]
        set_cell_margins(c_val, top=40, bottom=40, left=40, right=40)
        p_v = c_val.paragraphs[0]
        p_v.paragraph_format.space_after = Pt(0)
        p_v.paragraph_format.line_spacing = 1.2
        r_v = p_v.add_run(val)
        r_v.font.name = "Times New Roman"
        r_v.font.size = Pt(12)
        r_v.font.color.rgb = COLOR_BLACK

    p_loc = c.add_paragraph()
    p_loc.alignment = WD_ALIGN_PARAGRAPH.CENTER
    p_loc.paragraph_format.space_before = Pt(36)
    p_loc.paragraph_format.space_after = Pt(0)
    r_loc = p_loc.add_run("Bắc Ninh, ngày .....tháng .....năm 2026")
    r_loc.font.name = "Times New Roman"
    r_loc.font.size = Pt(12)
    r_loc.font.italic = True
    r_loc.font.color.rgb = COLOR_BLACK

    doc.add_page_break()

def build_teacher_review(doc):
    p_t = doc.add_paragraph()
    p_t.alignment = WD_ALIGN_PARAGRAPH.CENTER
    p_t.paragraph_format.space_before = Pt(12)
    p_t.paragraph_format.space_after = Pt(18)
    r = p_t.add_run("NHẬN XÉT CỦA GIÁO VIÊN")
    r.font.name = "Times New Roman"
    r.font.size = Pt(16)
    r.font.bold = True
    r.font.color.rgb = COLOR_BLACK

    for _ in range(14):
        add_p(doc, " ................................................................................................................................................................................................. ")

    p_score = doc.add_paragraph()
    p_score.paragraph_format.space_before = Pt(18)
    p_score.paragraph_format.space_after = Pt(20)
    r_s = p_score.add_run("Điểm: .....................................................")
    r_s.font.name = "Times New Roman"
    r_s.font.size = Pt(13)
    r_s.font.bold = True
    r_s.font.color.rgb = COLOR_BLACK

    p_sign = doc.add_paragraph()
    p_sign.alignment = WD_ALIGN_PARAGRAPH.RIGHT
    p_sign.paragraph_format.space_after = Pt(4)
    r_sign1 = p_sign.add_run("Bắc Ninh, ngày ...... tháng ...... năm 2026\n")
    r_sign1.font.name = "Times New Roman"
    r_sign1.font.size = Pt(12)
    r_sign1.font.italic = True
    r_sign1.font.color.rgb = COLOR_BLACK

    r_sign2 = p_sign.add_run("GIẢNG VIÊN HƯỚNG DẪN             \n")
    r_sign2.font.name = "Times New Roman"
    r_sign2.font.size = Pt(13)
    r_sign2.font.bold = True
    r_sign2.font.color.rgb = COLOR_BLACK

    r_sign3 = p_sign.add_run("(Ký và ghi rõ họ tên)                       \n\n\n\n\n")
    r_sign3.font.name = "Times New Roman"
    r_sign3.font.size = Pt(11)
    r_sign3.font.italic = True
    r_sign3.font.color.rgb = COLOR_BLACK

    r_sign4 = p_sign.add_run("Trương Thị Nhung                  ")
    r_sign4.font.name = "Times New Roman"
    r_sign4.font.size = Pt(13)
    r_sign4.font.bold = True
    r_sign4.font.color.rgb = COLOR_BLACK

    doc.add_page_break()

def build_intro_and_thanks(doc):
    # LỜI MỞ ĐẦU
    p_t = doc.add_paragraph()
    p_t.alignment = WD_ALIGN_PARAGRAPH.CENTER
    p_t.paragraph_format.space_before = Pt(10)
    p_t.paragraph_format.space_after = Pt(14)
    r = p_t.add_run("LỜI MỞ ĐẦU")
    r.font.name = "Times New Roman"
    r.font.size = Pt(16)
    r.font.bold = True
    r.font.color.rgb = COLOR_BLACK

    add_p(doc, "Trong thời đại công nghệ thông tin ngày càng phát triển, việc ứng dụng công nghệ vào công tác quản lý và kinh doanh đã trở thành một nhu cầu thiết yếu đối với các doanh nghiệp và hộ kinh doanh. Đặc biệt, đối với các khách sạn và nhà nghỉ, việc quản lý thông tin buồng phòng, khách hàng, dịch vụ minibar, hóa đơn và doanh thu một cách chính xác, nhanh chóng là yếu tố quan trọng góp phần nâng cao hiệu quả hoạt động.")

    add_p(doc, "Xuất phát từ nhu cầu thực tế đó, nhóm chúng em lựa chọn đề tài “Xây dựng phần mềm quản lý khách sạn” nhằm xây dựng một hệ thống hỗ trợ các công việc quản lý cơ bản của khách sạn. Phần mềm được thiết kế với các chức năng như quản lý phòng, đặt phòng, quản lý dịch vụ, lập hóa đơn, thanh toán và thống kê doanh thu. Qua đó, hệ thống giúp giảm bớt các công việc quản lý thủ công, hạn chế sai sót và tiết kiệm thời gian trong quá trình vận hành khách sạn.")

    add_p(doc, "Trong quá trình thực hiện đề tài, nhóm chúng em đã có cơ hội vận dụng những kiến thức đã học về lập trình C# WinForms, cơ sở dữ liệu SQL Server và phân tích thiết kế hệ thống để xây dựng một sản phẩm có tính ứng dụng thực tế. Tuy nhiên, do kiến thức và kinh nghiệm còn hạn chế nên đề tài khó tránh khỏi những thiếu sót. Nhóm chúng em rất mong nhận được những ý kiến đóng góp từ giảng viên để có thể hoàn thiện sản phẩm tốt hơn.")

    add_p(doc, "Nhóm chúng em xin chân thành cảm ơn!")

    doc.add_page_break()

    # LỜI CẢM ƠN
    p_c = doc.add_paragraph()
    p_c.alignment = WD_ALIGN_PARAGRAPH.CENTER
    p_c.paragraph_format.space_before = Pt(10)
    p_c.paragraph_format.space_after = Pt(14)
    r_c = p_c.add_run("LỜI CẢM ƠN")
    r_c.font.name = "Times New Roman"
    r_c.font.size = Pt(16)
    r_c.font.bold = True
    r_c.font.color.rgb = COLOR_BLACK

    add_p(doc, "Để hoàn thành đề tài “Xây dựng phần mềm quản lý khách sạn”, nhóm chúng em đã nhận được sự hướng dẫn, hỗ trợ và góp ý quý báu từ cô trong suốt quá trình học tập và thực hiện đề tài. Trước hết, nhóm chúng em xin gửi lời cảm ơn chân thành đến quý thầy cô đã tận tình giảng dạy, truyền đạt những kiến thức và kinh nghiệm cần thiết, giúp nhóm có nền tảng để áp dụng vào việc xây dựng và hoàn thiện phần mềm.")

    add_p(doc, "Đặc biệt, nhóm xin gửi lời cảm ơn đến giảng viên hướng dẫn: Cô Trương Thị Nhung đã dành thời gian hướng dẫn, góp ý và định hướng cho nhóm trong quá trình phân tích, thiết kế và xây dựng hệ thống. Những ý kiến đóng góp của cô là nguồn động lực và giúp nhóm khắc phục những thiếu sót trong quá trình thực hiện. Bên cạnh đó, nhóm cũng xin cảm ơn các thành viên trong nhóm đã cùng nhau trao đổi, phân công công việc, hỗ trợ lẫn nhau và cố gắng hoàn thành đề tài đúng tiến độ.")

    add_p(doc, "Mặc dù đã cố gắng hoàn thiện đề tài, nhưng do thời gian thực hiện và kiến thức của nhóm còn hạn chế nên bài làm vẫn không tránh khỏi những thiếu sót. Nhóm chúng em rất mong nhận được sự thông cảm và những ý kiến đóng góp của cô để đề tài được hoàn thiện hơn. Cuối cùng, nhóm chúng em xin kính chúc cô sức khỏe, thành công và luôn nhiệt huyết trong công tác giảng dạy.")

    add_p(doc, "Nhóm chúng em xin chân thành cảm ơn!")

    doc.add_page_break()

def build_toc_and_team(doc):
    # MỤC LỤC TRÌNH BÀY ĐEN TRẮNG CĂN PHẢI BẰNG TAB STOP VỚI DẤU CHẤM DẪN THẲNG HÀNG
    p_m = doc.add_paragraph()
    p_m.alignment = WD_ALIGN_PARAGRAPH.CENTER
    p_m.paragraph_format.space_before = Pt(10)
    p_m.paragraph_format.space_after = Pt(16)
    r_m = p_m.add_run("MỤC LỤC")
    r_m.font.name = "Times New Roman"
    r_m.font.size = Pt(16)
    r_m.font.bold = True
    r_m.font.color.rgb = COLOR_BLACK

    # Danh mục các đề mục chính xác và số trang tương ứng
    toc_entries = [
        ("NHẬN XÉT CỦA GIÁO VIÊN", "3", True, 0),
        ("LỜI MỞ ĐẦU", "4", True, 0),
        ("LỜI CẢM ƠN", "5", True, 0),
        ("MỤC LỤC", "6", True, 0),
        ("PHÂN CÔNG NHIỆM VỤ NHÓM", "9", True, 0),
        ("CHƯƠNG 1. TỔNG QUAN ĐỀ TÀI", "10", True, 0),
        ("1. Lý do chọn đề tài", "10", False, 1),
        ("2. Mục tiêu của đề tài", "10", False, 1),
        ("3. Đối tượng nghiên cứu", "10", False, 1),
        ("4. Phạm vi của đề tài", "11", False, 1),
        ("4.1. Phạm vi chức năng", "11", False, 2),
        ("4.2. Phạm vi dữ liệu", "11", False, 2),
        ("4.3. Phạm vi người sử dụng", "11", False, 2),
        ("5. Phương pháp thực hiện", "12", False, 1),
        ("6. Công nghệ và công cụ sử dụng", "12", False, 1),
        ("CHƯƠNG 2: KHẢO SÁT VÀ MÔ TẢ NGHIỆP VỤ QUẢN LÝ KHÁCH SẠN", "13", True, 0),
        ("1. Khái niệm nghiệp vụ quản lý khách sạn", "13", False, 1),
        ("2. Mô tả hoạt động nghiệp vụ của khách sạn", "13", False, 1),
        ("3. Các đối tượng tham gia nghiệp vụ", "14", False, 1),
        ("3.1. Quản lý", "14", False, 2),
        ("3.2. Nhân viên", "14", False, 2),
        ("3.3. Khách hàng", "14", False, 2),
        ("4. Các nghiệp vụ chính cần quản lý", "15", False, 1),
        ("4.1. Quản lý phòng & sơ đồ phòng", "15", False, 2),
        ("4.2. Quản lý đặt phòng & lưu trú", "15", False, 2),
        ("4.3. Quản lý hóa đơn", "15", False, 2),
        ("4.4. Quản lý dịch vụ & kho minibar", "16", False, 2),
        ("4.5. Quản lý nhân viên", "16", False, 2),
        ("4.6. Quản lý tài khoản", "16", False, 2),
        ("4.7. Thống kê – báo cáo", "17", False, 2),
        ("5. Quy trình nghiệp vụ tổng quát", "17", False, 1),
        ("CHƯƠNG 3: XÂY DỰNG CƠ SỞ DỮ LIỆU QUẢN LÝ KHÁCH SẠN", "18", True, 0),
        ("1. Phân tích yêu cầu", "18", False, 1),
        ("2. Các bảng trong cơ sở dữ liệu", "18", False, 1),
        ("3. Thiết kế các bảng", "19", False, 1),
        ("3.1. Bảng TaiKhoanNhanVien", "19", False, 2),
        ("3.2. Bảng Phong", "20", False, 2),
        ("3.3. Bảng KhachHang", "21", False, 2),
        ("3.4. Bảng LuotLuuTru", "22", False, 2),
        ("3.5. Bảng ChangLuuTru", "23", False, 2),
        ("3.6. Bảng DichVu", "24", False, 2),
        ("3.7. Bảng YeuCauDichVu", "24", False, 2),
        ("3.8. Bảng HangTonKho", "25", False, 2),
        ("3.9. Bảng BienDongKho", "26", False, 2),
        ("3.10. Bảng HoaDon", "26", False, 2),
        ("3.11. Bảng ChiTietBill", "27", False, 2),
        ("3.12. Bảng GiaoDichThanhToan", "28", False, 2),
        ("3.13. Bảng CaTruc", "29", False, 2),
        ("3.14. Bảng NhatKyThaoTac", "29", False, 2),
        ("3.15. Bảng ChucNang", "30", False, 2),
        ("3.16. Bảng PhanQuyenNhanVien", "30", False, 2),
        ("3.17. Bảng CauHinhMenu", "31", False, 2),
        ("3.18. Bảng CauHinhMenuCon", "31", False, 2),
        ("3.19. Bảng CauHinhChucNang", "32", False, 2),
        ("3.20. Bảng PhienBanCSDL", "32", False, 2),
        ("4. Sơ đồ quan hệ", "33", False, 1),
        ("CHƯƠNG 4: MÔ TẢ THIẾT KẾ GIAO DIỆN HỆ THỐNG QUẢN LÝ KHÁCH SẠN", "34", True, 0),
        ("1. Tổng quan thiết kế giao diện", "34", False, 1),
        ("2. Quy chuẩn giao diện chung", "34", False, 1),
        ("3. Thiết kế Form Đăng nhập", "35", False, 1),
        ("4. Thiết kế Form Menu chính", "36", False, 1),
        ("5. Thiết kế Form Sơ đồ phòng", "37", False, 1),
        ("6. Thiết kế Form Đặt phòng & Nhận phòng", "38", False, 1),
        ("7. Thiết kế Form Quản lý dịch vụ & Minibar", "39", False, 1),
        ("8. Thiết kế Form Quản lý hóa đơn", "40", False, 1),
        ("9. Thiết kế Form Thanh toán VietQR", "41", False, 1),
        ("10. Thiết kế Form Quản lý ca trực", "42", False, 1),
        ("11. Thiết kế Form Quản lý nhân viên", "43", False, 1),
        ("12. Thiết kế Form Quản lý tài khoản", "44", False, 1),
        ("13. Thiết kế Form Thống kê doanh thu", "45", False, 1),
        ("14. Luồng sử dụng giao diện", "46", False, 1),
        ("15. Các Control WinForms được sử dụng", "46", False, 1),
        ("16. Đánh giá thiết kế giao diện", "47", False, 1),
        ("17. Ưu điểm và hạn chế của hệ thống", "47", False, 1),
        ("17.1. Ưu điểm", "47", False, 2),
        ("17.2. Hạn chế", "48", False, 2),
        ("KẾT LUẬN", "49", True, 0),
        ("TÀI LIỆU THAM KHẢO", "50", True, 0)
    ]

    for title, page_str, is_bold, level in toc_entries:
        add_toc_entry(doc, title, page_str, is_bold, level)

    doc.add_page_break()

    # BẢNG PHÂN CÔNG NHIỆM VỤ NHÓM
    p_pc = doc.add_paragraph()
    p_pc.alignment = WD_ALIGN_PARAGRAPH.CENTER
    p_pc.paragraph_format.space_before = Pt(10)
    p_pc.paragraph_format.space_after = Pt(14)
    r_pc = p_pc.add_run("PHÂN CÔNG NHIỆM VỤ NHÓM")
    r_pc.font.name = "Times New Roman"
    r_pc.font.size = Pt(16)
    r_pc.font.bold = True
    r_pc.font.color.rgb = COLOR_BLACK

    team_headers = ["Thành viên", "Nhiệm vụ", "Nội dung cần hiểu để thuyết trình"]
    team_data = [
        [
            "Nguyễn Ngọc Thuần Nhi\n(Trưởng nhóm)",
            "Lập trình C# và thiết kế giao diện",
            "Giới thiệu đề tài, công nghệ C# WinForms, giao diện trang chủ, đăng nhập, phân quyền và cách chuyển giữa các chức năng."
        ],
        [
            "Nguyễn Thị Thu Hà",
            "Lập trình các chức năng quản lý",
            "Hiểu cách hoạt động của quản lý phòng, đặt phòng, nhận phòng, gọi dịch vụ minibar, hóa đơn và thanh toán."
        ],
        [
            "Nguyễn Đức Anh Tuấn",
            "Thiết kế và quản lý cơ sở dữ liệu SQL Server",
            "Thiết kế các bảng dữ liệu, khóa chính, khóa ngoại, mối quan hệ giữa các bảng, câu lệnh SQL và kết nối với C#."
        ],
        [
            "Lê Quốc Vượng",
            "Viết và trình bày báo cáo Word",
            "Viết lý do chọn đề tài, mục tiêu, khảo sát nghiệp vụ, mô tả chức năng, hình ảnh giao diện, kết quả, ưu nhược điểm và kết luận."
        ],
        [
            "Hoàng Anh Ngọc",
            "Kiểm tra phần mềm",
            "Kiểm tra các chức năng, phát hiện lỗi, thử nghiệm tính tiền và giới thiệu hướng phát triển phần mềm."
        ]
    ]
    col_widths = [Inches(1.8), Inches(2.3), Inches(2.7)]
    add_styled_table(doc, team_headers, team_data, col_widths)

    doc.add_page_break()

print("report_sections_part1.py updated with Black & White styling.")
