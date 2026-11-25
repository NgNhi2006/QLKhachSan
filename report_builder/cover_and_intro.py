import docx
from docx.shared import Inches, Pt, RGBColor
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.enum.table import WD_TABLE_ALIGNMENT
from docx.oxml import parse_xml
from docx.oxml.ns import nsdecls
from .styles import (
    COLOR_PRIMARY, COLOR_SECONDARY, COLOR_TEXT, COLOR_MUTED,
    add_h1, add_h2, add_p, add_bullet, create_styled_table, set_cell_background,
    set_cell_margins
)

def build_cover_page(doc):
    # Bordered frame for cover page
    tbl = doc.add_table(rows=1, cols=1)
    tbl.alignment = WD_TABLE_ALIGNMENT.CENTER
    c = tbl.cell(0, 0)
    c.width = Inches(6.8)
    set_cell_margins(c, top=280, bottom=280, left=320, right=320)

    # Elegant double-line decorative border around cover
    tcPr = c._tc.get_or_add_tcPr()
    borders = parse_xml(
        f'<w:tcBorders {nsdecls("w")}>'
        f'<w:top w:val="double" w:sz="18" w:space="0" w:color="1F4E79"/>'
        f'<w:bottom w:val="double" w:sz="18" w:space="0" w:color="1F4E79"/>'
        f'<w:left w:val="double" w:sz="18" w:space="0" w:color="1F4E79"/>'
        f'<w:right w:val="double" w:sz="18" w:space="0" w:color="1F4E79"/>'
        f'</w:tcBorders>'
    )
    tcPr.append(borders)

    p = c.paragraphs[0]
    p.alignment = WD_ALIGN_PARAGRAPH.CENTER
    p.paragraph_format.line_spacing = 1.15
    p.paragraph_format.space_after = Pt(2)
    r = p.add_run("UBND TỈNH BẮC NINH\nTRƯỜNG CAO ĐẲNG CÔNG NGHỆ VIỆT - HÀN\nKHOA CÔNG NGHỆ THÔNG TIN")
    r.font.name = "Times New Roman"
    r.font.size = Pt(13)
    r.font.bold = True
    r.font.color.rgb = COLOR_PRIMARY

    # Separator line
    p_sep = c.add_paragraph()
    p_sep.alignment = WD_ALIGN_PARAGRAPH.CENTER
    p_sep.paragraph_format.space_before = Pt(4)
    p_sep.paragraph_format.space_after = Pt(36)
    r_sep = p_sep.add_run("═════════ ★ ═════════")
    r_sep.font.name = "Times New Roman"
    r_sep.font.size = Pt(11)
    r_sep.font.color.rgb = COLOR_SECONDARY

    # Subject header
    p_sub = c.add_paragraph()
    p_sub.alignment = WD_ALIGN_PARAGRAPH.CENTER
    p_sub.paragraph_format.space_after = Pt(6)
    r_sub = p_sub.add_run("BÁO CÁO ĐỒ ÁN MÔN HỌC\nMÔN: LẬP TRÌNH WINDOWS")
    r_sub.font.name = "Times New Roman"
    r_sub.font.size = Pt(15)
    r_sub.font.bold = True
    r_sub.font.color.rgb = RGBColor(0x33, 0x33, 0x33)

    # Title
    p_title_pre = c.add_paragraph()
    p_title_pre.alignment = WD_ALIGN_PARAGRAPH.CENTER
    p_title_pre.paragraph_format.space_before = Pt(24)
    p_title_pre.paragraph_format.space_after = Pt(6)
    r_tp = p_title_pre.add_run("ĐỀ TÀI:")
    r_tp.font.name = "Times New Roman"
    r_tp.font.size = Pt(14)
    r_tp.font.bold = True
    r_tp.font.color.rgb = COLOR_MUTED

    p_title = c.add_paragraph()
    p_title.alignment = WD_ALIGN_PARAGRAPH.CENTER
    p_title.paragraph_format.space_after = Pt(8)
    r_t = p_title.add_run("XÂY DỰNG PHẦN MỀM QUẢN LÝ KHÁCH SẠN\n(HOTEL MANAGEMENT SYSTEM)")
    r_t.font.name = "Times New Roman"
    r_t.font.size = Pt(19)
    r_t.font.bold = True
    r_t.font.color.rgb = COLOR_PRIMARY

    p_tech = c.add_paragraph()
    p_tech.alignment = WD_ALIGN_PARAGRAPH.CENTER
    p_tech.paragraph_format.space_after = Pt(40)
    r_tech = p_tech.add_run("Nền tảng: C# Windows Forms (.NET 10) & Microsoft SQL Server")
    r_tech.font.name = "Times New Roman"
    r_tech.font.size = Pt(12)
    r_tech.font.italic = True
    r_tech.font.color.rgb = COLOR_SECONDARY

    # Metadata table inside cover
    meta_tbl = c.add_table(rows=5, cols=2)
    meta_tbl.alignment = WD_TABLE_ALIGNMENT.CENTER
    meta_widths = [Inches(2.4), Inches(3.6)]
    meta_data = [
        ("Giáo viên hướng dẫn:", "Cô Trương Thị Nhung"),
        ("Sinh viên thực hiện:", "Anh Nhi (Nguyễn Nhi) - Trưởng nhóm"),
        ("Thành viên tham gia:", "Nhóm phát triển đề tài Quản lý Khách sạn"),
        ("Lớp học:", "IT1.11.A - Khoa Công Nghệ Thông Tin"),
        ("Thời gian thực hiện:", "Học kỳ 1 - Năm học 2026 - 2027")
    ]
    for row_idx, (lbl, val) in enumerate(meta_data):
        row = meta_tbl.rows[row_idx]

        # Left label cell
        c_lbl = row.cells[0]
        c_lbl.width = meta_widths[0]
        set_cell_margins(c_lbl, top=40, bottom=40, left=60, right=60)
        p_l = c_lbl.paragraphs[0]
        p_l.paragraph_format.space_after = Pt(0)
        r_l = p_l.add_run(lbl)
        r_l.font.name = "Times New Roman"
        r_l.font.size = Pt(12)
        r_l.font.bold = True
        r_l.font.color.rgb = COLOR_TEXT

        # Right value cell
        c_val = row.cells[1]
        c_val.width = meta_widths[1]
        set_cell_margins(c_val, top=40, bottom=40, left=60, right=60)
        p_v = c_val.paragraphs[0]
        p_v.paragraph_format.space_after = Pt(0)
        r_v = p_v.add_run(val)
        r_v.font.name = "Times New Roman"
        r_v.font.size = Pt(12)
        r_v.font.color.rgb = COLOR_TEXT

    # Location & Date Footer inside cover
    p_loc = c.add_paragraph()
    p_loc.alignment = WD_ALIGN_PARAGRAPH.CENTER
    p_loc.paragraph_format.space_before = Pt(50)
    p_loc.paragraph_format.space_after = Pt(10)
    r_loc = p_loc.add_run("BẮC NINH, THÁNG 11 NĂM 2026")
    r_loc.font.name = "Times New Roman"
    r_loc.font.size = Pt(12.5)
    r_loc.font.bold = True
    r_loc.font.color.rgb = COLOR_PRIMARY

    # Page Break after cover
    doc.add_page_break()

def build_table_of_contents(doc):
    p_t = doc.add_paragraph()
    p_t.alignment = WD_ALIGN_PARAGRAPH.CENTER
    p_t.paragraph_format.space_before = Pt(12)
    p_t.paragraph_format.space_after = Pt(16)
    r = p_t.add_run("MỤC LỤC")
    r.font.name = "Times New Roman"
    r.font.size = Pt(16)
    r.font.bold = True
    r.font.color.rgb = COLOR_PRIMARY

    # Native TOC Field for Word
    p_toc = doc.add_paragraph()
    p_toc.paragraph_format.space_after = Pt(12)
    r_toc = p_toc.add_run()
    fldChar1 = parse_xml(r'<w:fldChar %s w:fldCharType="begin"/>' % nsdecls('w'))
    instrText = parse_xml(r'<w:instrText %s xml:space="preserve"> TOC \o "1-3" \h \z \u </w:instrText>' % nsdecls('w'))
    fldChar2 = parse_xml(r'<w:fldChar %s w:fldCharType="separate"/>' % nsdecls('w'))
    fldChar3 = parse_xml(r'<w:fldChar %s w:fldCharType="end"/>' % nsdecls('w'))
    r_toc._r.append(fldChar1)
    r_toc._r.append(instrText)
    r_toc._r.append(fldChar2)
    r_toc._r.append(fldChar3)

    # Static High-Fidelity Table of Contents matching the sample exactly
    toc_items = [
        ("LỜI MỞ ĐẦU", "4", 0, True),
        ("BẢNG PHÂN CÔNG NHIỆM VỤ ĐỀ TÀI", "5", 0, True),
        ("CHƯƠNG 1. MÔ TẢ CHỨC NĂNG VÀ MÔ TẢ NGHIỆP VỤ", "6", 0, True),
        ("1.1. Giới thiệu đề tài", "6", 1, True),
        ("1.1.1. Tên đề tài và mục tiêu xây dựng", "6", 2, False),
        ("1.1.2. Phạm vi và công nghệ sử dụng", "7", 2, False),
        ("1.2. Mô tả chức năng", "8", 1, True),
        ("1.2.1. Danh mục các nhóm chức năng chính", "8", 2, False),
        ("1.2.2. Yêu cầu phi chức năng", "10", 2, False),
        ("1.3. Mô tả nghiệp vụ", "11", 1, True),
        ("1.3.1. Đối tượng sử dụng và vai trò", "11", 2, False),
        ("1.3.2. Quy trình nghiệp vụ tổng quát", "12", 2, False),
        ("1.3.3. Mô tả nghiệp vụ từng phân hệ", "13", 2, False),
        ("1.3.4. Các trạng thái của phòng và lượt lưu trú", "15", 2, False),
        ("1.3.5. Quy tắc nghiệp vụ cốt lõi", "16", 2, False),
        ("1.4. Khảo sát hiện trạng và yêu cầu người dùng", "18", 1, True),
        ("1.4.1. Hiện trạng quản lý khách sạn", "18", 2, False),
        ("1.4.2. Giải pháp phần mềm đề xuất", "19", 2, False),
        ("1.4.3. Yêu cầu chi tiết của người dùng", "20", 2, False),
        ("1.4.4. Các giai đoạn thực hiện đề tài", "21", 2, False),
        ("1.5. Mô tả dữ liệu vào/ra của các biểu mẫu", "22", 1, True),
        ("1.6. Danh mục thông báo và cảnh báo của hệ thống", "26", 1, True),
        ("CHƯƠNG 2. PHÂN TÍCH CHỨC NĂNG", "28", 0, True),
        ("2.1. Đặc tả chi tiết các chức năng chính", "28", 1, True),
        ("2.1.1. Chức năng Xác thực & Quản trị tài khoản", "28", 2, False),
        ("2.1.2. Chức năng Sơ đồ phòng & Đặt phòng trực quan", "29", 2, False),
        ("2.1.3. Chức năng Quản lý Lưu trú (Check-in, Đổi phòng, Check-out)", "31", 2, False),
        ("2.1.4. Chức năng Dịch vụ & Kho Minibar", "33", 2, False),
        ("2.1.5. Chức năng Thanh toán hóa đơn & VietQR động", "34", 2, False),
        ("2.1.6. Chức năng Quản lý Ca trực & Sổ quỹ", "35", 2, False),
        ("2.1.7. Chức năng Trình thiết kế màn hình động (Custom Screen)", "37", 2, False),
        ("2.1.8. Chức năng Báo cáo thống kê & Doanh thu", "38", 2, False),
        ("2.2. Kiến trúc hệ thống", "39", 1, True),
        ("2.2.1. Mô hình phân tầng 3-Tier (GUI - BLL - DAL - DTO)", "39", 2, False),
        ("2.2.2. Mẫu thiết kế Repository Pattern & Transaction Script", "41", 2, False),
        ("2.2.3. Luồng dữ liệu tổng quát giữa các tầng", "42", 2, False),
        ("2.3. Các câu lệnh truy vấn nghiệp vụ chính", "43", 1, True),
        ("2.4. Ma trận chức năng – dữ liệu (CRUD Matrix)", "46", 1, True),
        ("CHƯƠNG 3. XÂY DỰNG CƠ SỞ DỮ LIỆU", "48", 0, True),
        ("3.1. Phân tích thực thể", "48", 1, True),
        ("3.2. Lược đồ quan hệ thực thể (ERD)", "50", 1, True),
        ("3.3. Thiết kế chi tiết các bảng", "51", 1, True),
        ("3.4. Mối liên hệ và khóa ngoại giữa các bảng", "58", 1, True),
        ("3.5. Ràng buộc toàn vẹn và chỉ mục", "60", 1, True),
        ("3.6. Dữ liệu mẫu đã nhập vào các bảng", "62", 1, True),
        ("3.7. View và Trigger hỗ trợ truy vấn", "64", 1, True),
        ("3.8. Cơ chế nâng cấp Schema Migration tự động", "66", 1, True),
        ("3.9. Kết nối ứng dụng Visual Studio với SQL Server", "68", 1, True),
        ("3.10. Mã lệnh DDL tạo từng bảng", "70", 1, True),
        ("3.11. Các truy vấn mẫu và kết quả", "74", 1, True),
        ("3.12. Kiểm thử các ràng buộc toàn vẹn", "78", 1, True),
        ("3.13. Sao lưu và phục hồi cơ sở dữ liệu", "80", 1, True),
        ("3.14. Bảo mật dữ liệu", "81", 1, True),
        ("CHƯƠNG 4. CHƯƠNG TRÌNH GIAO DIỆN", "83", 0, True),
        ("4.1. Môi trường và cách chạy chương trình", "83", 1, True),
        ("4.2. Thiết kế giao diện chung", "84", 1, True),
        ("4.3. Các màn hình của chương trình", "86", 1, True),
        ("4.3.1. Bố cục khung chính & Thanh điều hướng", "86", 2, False),
        ("4.3.2. Màn hình Đăng nhập & Xác thực người dùng", "88", 2, False),
        ("4.3.3. Màn hình Sơ đồ phòng trực quan (Room Map)", "90", 2, False),
        ("4.3.4. Màn hình Đặt phòng trước & Tra cứu phòng trống", "93", 2, False),
        ("4.3.5. Màn hình Đặt phòng theo đoàn (Multiple Booking)", "96", 2, False),
        ("4.3.6. Màn hình Quản lý Dịch vụ, Minibar & Kho hàng", "98", 2, False),
        ("4.3.7. Màn hình Trả phòng, Hóa đơn & QR VietQR động", "101", 2, False),
        ("4.3.8. Màn hình Quản lý Ca trực & Bàn giao quỹ", "104", 2, False),
        ("4.3.9. Màn hình Báo cáo Doanh thu & Thống kê", "107", 2, False),
        ("4.3.10. Màn hình Trình thiết kế màn hình động (Custom Screen)", "110", 2, False),
        ("4.3.11. Màn hình Quản trị Tài khoản & Phân quyền chức năng", "113", 2, False),
        ("4.4. Kịch bản kiểm thử", "116", 1, True),
        ("4.5. Hướng dẫn sử dụng chương trình", "122", 1, True),
        ("4.6. Mã nguồn tiêu biểu", "126", 1, True),
        ("4.7. Xử lý sự cố thường gặp", "131", 1, True),
        ("KẾT LUẬN VÀ HƯỚNG PHÁT TRIỂN", "133", 0, True),
        ("1. Kết quả đạt được", "133", 1, True),
        ("2. Hạn chế", "134", 1, True),
        ("3. Hướng phát triển", "135", 1, True),
        ("TÀI LIỆU THAM KHẢO", "136", 0, True),
    ]

    for title, pg, level, is_bold in toc_items:
        p_item = doc.add_paragraph()
        p_item.paragraph_format.line_spacing = 1.15
        p_item.paragraph_format.space_before = Pt(3 if level == 0 else 1)
        p_item.paragraph_format.space_after = Pt(2)
        indent = 0.0 if level == 0 else (0.25 if level == 1 else 0.5)
        p_item.paragraph_format.left_indent = Inches(indent)

        # Title part
        r_t = p_item.add_run(title)
        r_t.font.name = "Times New Roman"
        r_t.font.size = Pt(11.5 if level == 0 else (11.0 if level == 1 else 10.5))
        r_t.font.bold = is_bold
        r_t.font.color.rgb = COLOR_PRIMARY if level == 0 else COLOR_TEXT

        # Dot leader to right margin
        dot_count = max(4, int(58 - len(title) * 0.7 - indent * 18))
        dots = " ." * dot_count
        r_dots = p_item.add_run(dots + " ")
        r_dots.font.name = "Times New Roman"
        r_dots.font.size = Pt(9.5)
        r_dots.font.color.rgb = RGBColor(0x99, 0x99, 0x99)

        # Page number
        r_pg = p_item.add_run(pg)
        r_pg.font.name = "Times New Roman"
        r_pg.font.size = Pt(11.0)
        r_pg.font.bold = is_bold
        r_pg.font.color.rgb = COLOR_PRIMARY if level == 0 else COLOR_TEXT

    doc.add_page_break()

def build_team_assignments(doc):
    p_t = doc.add_paragraph()
    p_t.alignment = WD_ALIGN_PARAGRAPH.CENTER
    p_t.paragraph_format.space_before = Pt(10)
    p_t.paragraph_format.space_after = Pt(12)
    r = p_t.add_run("BẢNG PHÂN CÔNG NHIỆM VỤ ĐỀ TÀI")
    r.font.name = "Times New Roman"
    r.font.size = Pt(15)
    r.font.bold = True
    r.font.color.rgb = COLOR_PRIMARY

    add_p(doc, "Đề tài được triển khai và hoàn thiện với sự tham gia phối hợp chặt chẽ giữa các thành viên phụ trách từng phân hệ chuyên môn, từ khảo sát nghiệp vụ khách sạn thực tế, thiết kế cấu trúc cơ sở dữ liệu quan hệ, xây dựng tầng truy cập dữ liệu DAL, tầng xử lý nghiệp vụ BLL, đến thiết kế giao diện đồ họa Windows Forms và kiểm thử hệ thống.")

    headers = ["STT", "Thành viên / Vai trò", "Công việc Lập trình & Cơ sở dữ liệu", "Phần báo cáo phụ trách", "Kiểm thử"]
    data = [
        [
            "1",
            "Anh Nhi (Nguyễn Nhi)\n(Trưởng nhóm)",
            "• Thiết kế kiến trúc tổng thể 3-Tier (GUI - BLL - DAL - DTO).\n• Xây dựng CSDL SQL Server: Schema V1-V26, 20 bảng chuẩn, ràng buộc, chỉ mục, khóa ứng dụng sp_getapplock.\n• Viết SchemaMigrator.cs: cơ chế tự động nâng cấp CSDL không mất dữ liệu.\n• Viết HotelRepository.cs, HotelTransaction.Commands.cs, HotelService.cs.\n• Thuật toán tính tiền phòng block 24h, đổi phòng, hủy cọc Forfeit, kiểm tra trùng lịch.",
            "• Chương 3: Xây dựng cơ sở dữ liệu.\n• Chương 2: Kiến trúc hệ thống, CRUD matrix.\n• Tổng hợp, hiệu đính toàn bộ báo cáo Word.",
            "15 kịch bản:\n- Khởi tạo CSDL & Migration\n- Đặt phòng & Trùng lịch\n- Tính tiền & Hủy cọc\n- Giao dịch đồng thời"
        ],
        [
            "2",
            "Nguyễn Thị Khánh Hòa\n(Thành viên)",
            "• Xây dựng Program.cs, AppTheme.cs, WorkspaceSkin.cs, ModernControls.cs.\n• Màn hình đăng nhập FormLogin.cs: xác thực PBKDF2 SHA-512 với Salt, chống Brute-Force, RememberedLogin DPAPI.\n• Khung điều hướng chính FormMain.cs, Navigation.cs, Profile header, Avatar, đồng hồ hệ thống.",
            "• Lời mở đầu.\n• Chương 1: Giới thiệu đề tài, phạm vi, công nghệ, danh mục thông báo.\n• Mục 4.1, 4.2: Giao diện nền tảng.",
            "10 kịch bản:\n- Đăng nhập & Khóa tạm\n- Đổi mật khẩu\n- Phân quyền theo vai trò\n- Ghi nhớ tài khoản"
        ],
        [
            "3",
            "Thân Mạnh Hùng\n(Thành viên)",
            "• Xây dựng Sơ đồ phòng trực quan ucDashboard.RoomOverview.cs (các thẻ phòng đổi màu theo trạng thái thực tế).\n• Xây dựng module đặt phòng ucDashboard.BookingDialog.cs, MultipleBooking.cs.\n• Module lưu trú Check-in, đổi phòng, gia hạn phòng, hủy lượt.",
            "• Chương 1: Nghiệp vụ buồng phòng & lưu trú.\n• Chương 2: Đặc tả chức năng đặt phòng, check-in/out.\n• Mục 4.3.3, 4.3.4, 4.3.5: Giao diện phòng & đặt phòng.",
            "12 kịch bản:\n- Đặt phòng trước\n- Đặt đoàn nhiều phòng\n- Check-in đúng hẹn/trễ\n- Đổi phòng đang ở"
        ],
        [
            "4",
            "Lương Thị Quyền\n(Thành viên)",
            "• Xây dựng module Dịch vụ & Minibar: gọi dịch vụ, giao từng phần, hoàn hủy.\n• Module Kho hàng & Biến động kho: xuất bán, nhập kho, kiểm kê minibar.\n• Module Thanh toán & Hóa đơn ucDashboard.BillPreview.cs, tích hợp PaymentQr.cs (mã VietQR động tạo theo chuẩn NAPAS).",
            "• Chương 1: Nghiệp vụ dịch vụ & thanh toán.\n• Chương 2: Đặc tả chức năng dịch vụ, kho và hóa đơn.\n• Mục 4.3.6, 4.3.7: Giao diện dịch vụ & VietQR.",
            "10 kịch bản:\n- Gọi dịch vụ buồng phòng\n- Xuất bán kho minibar\n- Checkout & xuất hóa đơn\n- Quét mã VietQR"
        ],
        [
            "5",
            "Nguyễn Thị Thư\n(Thành viên)",
            "• Phân hệ Kế toán & Ca trực AccountingForm.cs: mở ca, nộp ca, giải trình lệch quỹ, khóa ca trực.\n• Module Báo cáo doanh thu, công suất phòng, xuất dữ liệu CSV UTF-8.\n• Module Trình thiết kế màn hình động CustomScreenDesigner.cs & cấu hình menu động.\n• Quản trị tài khoản & phân quyền chi tiết FunctionPolicy.cs.",
            "• Chương 1: Nghiệp vụ ca trực & kế toán.\n• Chương 2: Báo cáo & Custom Screen.\n• Mục 4.3.8 - 4.3.12: Giao diện ca trực, báo cáo, phân quyền.\n• Kết luận & Tài liệu tham khảo.",
            "11 kịch bản:\n- Mở/đóng ca & lệch quỹ\n- Báo cáo doanh thu\n- Thiết kế form động\n- Cấp quyền chức năng"
        ]
    ]
    col_widths = [0.5, 1.4, 2.5, 1.7, 1.2]
    create_styled_table(doc, headers, data, col_widths=col_widths, caption="Bảng 0.1. Phân công nhiệm vụ các thành viên trong nhóm đồ án")
    doc.add_page_break()

def build_intro(doc):
    add_h1(doc, "LỜI MỞ ĐẦU")

    add_p(doc, "Trong bối cảnh nền kinh tế số và cuộc Cách mạng Công nghiệp 4.0 đang tác động mạnh mẽ đến mọi lĩnh vực đời sống xã hội, ngành du lịch – khách sạn cũng chứng kiến sự chuyển dịch sâu sắc hướng tới chuyển đổi số và tự động hóa quy trình quản trị. Hoạt động quản lý lưu trú tại các khách sạn, nhà nghỉ, resort quy mô vừa và nhỏ đòi hỏi sự chính xác tuyệt đối, tốc độ xử lý nhanh chóng và tính đồng bộ cao giữa các bộ phận: từ lễ tân tiếp đón, buồng phòng dọn dẹp, nhân viên dịch vụ minibar, cho đến kế toán thu ngân và ban quản lý điều hành.")

    add_p(doc, "Tuy nhiên, khảo sát thực tế tại nhiều cơ sở lưu trú hiện nay cho thấy công tác quản lý vẫn còn phụ thuộc rất nhiều vào các phương pháp thủ công truyền thống như ghi chép sổ sách giấy tờ hoặc sử dụng các bảng tính Excel rời rạc. Cách thức vận hành này bộc lộ hàng loạt nhược điểm và rủi ro nghiêm trọng:")

    add_bullet(doc, "Dễ xảy ra tình trạng đặt trùng phòng (Overbooking) vào các mùa cao điểm do thông tin giữa các ca trực không được cập nhật tức thời.", "• Nguy cơ xung đột lịch phòng: ")
    add_bullet(doc, "Lễ tân mất nhiều thời gian kiểm tra sổ sách, tính toán hóa đơn bằng tay dễ sai sót, khách hàng phải chờ đợi lâu khi làm thủ tục check-in hoặc check-out.", "• Tốc độ phục vụ chậm chạp: ")
    add_bullet(doc, "Hàng hóa tiêu thụ tại phòng (nước giải khát, đồ ăn vặt minibar) thường xuyên bị thất thoát, chênh lệch giữa số lượng buồng phòng báo cáo và tồn kho thực tế.", "• Thất thoát dịch vụ và minibar: ")
    add_bullet(doc, "Tiền mặt thu chi giữa các ca trực thường bị nhầm lẫn, thiếu biên bản bàn giao quỹ chặt chẽ, khó khăn trong việc đối soát với sao kê ngân hàng khi khách chuyển khoản.", "• Rủi ro tài chính và bàn giao ca: ")
    add_bullet(doc, "Dữ liệu lưu trữ phân tán, không có cơ chế bảo mật, phân quyền hay sao lưu dự phòng, tiềm ẩn nguy cơ mất mát dữ liệu khi gặp sự cố phần cứng.", "• An toàn thông tin kém: ")

    add_p(doc, "Xuất phát từ những đòi hỏi cấp thiết của thực tế, cùng với mong muốn vận dụng toàn diện các kiến thức nền tảng và nâng cao đã được đào tạo trong chương trình Lập trình Windows, Phát triển phần mềm .NET và Hệ quản trị cơ sở dữ liệu, nhóm sinh viên chúng em đã quyết định lựa chọn đề tài: “Xây dựng phần mềm Quản lý Khách sạn (Hotel Management System)” làm đồ án môn học.")

    add_p(doc, "Hệ thống được thiết kế và hiện thực hóa trên nền tảng Microsoft .NET 10 với ngôn ngữ lập trình C# hiện đại, kết hợp cùng hệ quản trị cơ sở dữ liệu quan hệ mạnh mẽ Microsoft SQL Server. Chương trình tuân thủ nghiêm ngặt mô hình kiến trúc phân tầng 3-Tier (GUI – BLL – DAL – DTO), áp dụng mẫu thiết kế Repository Pattern và Transaction Script nhằm đảm bảo tính toàn vẹn dữ liệu (ACID) tối đa cho mọi giao dịch đặt phòng, thu cọc và thanh toán. Đặc biệt, phần mềm mang phong cách thiết kế giao diện phẳng hiện đại (Modern UI), tích hợp tính năng thanh toán quét mã VietQR động theo chuẩn NAPAS 247, và module độc đáo cho phép người quản trị tự thiết kế các màn hình nhập liệu động (Custom Screen Designer) mà không cần can thiệp vào mã nguồn.")

    add_p(doc, "Bản báo cáo chi tiết này được tổ chức và trình bày một cách khoa học thành bốn chương chính:")
    add_bullet(doc, "Trình bày tổng quan mục tiêu đề tài, phạm vi ứng dụng, các yêu cầu chức năng và phi chức năng, đối tượng sử dụng, quy trình nghiệp vụ tổng thể và quy tắc nghiệp vụ cốt lõi của khách sạn.", "Chương 1. Mô tả chức năng và mô tả nghiệp vụ: ")
    add_bullet(doc, "Đi sâu phân tích đặc tả chi tiết 8 nhóm chức năng chính, kiến trúc hệ thống đa tầng 3-Tier, mẫu thiết kế xử lý giao dịch, các truy vấn nghiệp vụ phức tạp và ma trận chức năng – dữ liệu (CRUD Matrix).", "Chương 2. Phân tích chức năng: ")
    add_bullet(doc, "Trình bày chi tiết phân tích thực thể, lược đồ quan hệ ERD, thiết kế chi tiết 20 bảng cơ sở dữ liệu chuẩn V26, các ràng buộc toàn vẹn, cơ chế tự động nâng cấp CSDL SchemaMigrator, bảo mật mật khẩu PBKDF2 SHA-512 và các kịch bản kiểm thử toàn vẹn.", "Chương 3. Xây dựng cơ sở dữ liệu: ")
    add_bullet(doc, "Giới thiệu môi trường triển khai, triết lý thiết kế giao diện hiện đại, thuyết minh chi tiết 11 màn hình chức năng kèm ảnh chụp thực tế từ hệ thống, bảng kịch bản kiểm thử toàn diện, hướng dẫn vận hành, mã nguồn tiêu biểu và xử lý sự cố thường gặp.", "Chương 4. Chương trình giao diện: ")

    add_p(doc, "Mặc dù nhóm chúng em đã dành trọn tâm huyết, nỗ lực nghiên cứu và hoàn thiện đề tài với tinh thần trách nhiệm cao nhất, song do giới hạn về thời gian và kinh nghiệm thực tế, chương trình và báo cáo khó tránh khỏi những điểm còn thiếu sót. Nhóm chúng em rất mong nhận được những nhận xét, góp ý quý báu và sự chỉ bảo tận tình của Cô Trương Thị Nhung cùng các Thầy/Cô trong Hội đồng chấm đồ án để sản phẩm ngày càng hoàn thiện, có thể ứng dụng rộng rãi và hiệu quả vào thực tế các doanh nghiệp lưu trú.")

    add_p(doc, "Chúng em xin trân trọng cảm ơn!", bold=True, italic=True)
    doc.add_page_break()
