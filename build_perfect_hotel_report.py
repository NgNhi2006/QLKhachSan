import docx
from docx.shared import Inches, Pt, RGBColor
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.enum.table import WD_TABLE_ALIGNMENT
from docx.oxml import parse_xml
from docx.oxml.ns import qn, nsdecls
import os
import shutil
import sys

sys.stdout.reconfigure(encoding='utf-8')

# TOC Bookmarks matching the 41 TOC entries in the sample template exactly
TOC_BOOKMARKS = {
    "LỜI MỞ ĐẦU": ("_Toc242879963", 0),
    "CHƯƠNG 1. MÔ TẢ CHỨC NĂNG VÀ MÔ TẢ NGHIỆP VỤ": ("_Toc242879964", 1),
    "1.1. Giới thiệu đề tài": ("_Toc242879965", 2),
    "1.2. Mô tả chức năng": ("_Toc242879966", 3),
    "1.3. Mô tả nghiệp vụ": ("_Toc242879967", 4),
    "1.4. Khảo sát hiện trạng và yêu cầu người dùng": ("_Toc242879968", 5),
    "1.5. Mô tả dữ liệu vào/ra của các biểu mẫu": ("_Toc242879969", 6),
    "1.6. Danh mục thông báo của hệ thống": ("_Toc242879970", 7),
    "CHƯƠNG 2. PHÂN TÍCH CHỨC NĂNG": ("_Toc242879971", 8),
    "2.1. Đặc tả chi tiết các chức năng chính": ("_Toc242879972", 9),
    "2.3. Kiến trúc hệ thống": ("_Toc242879973", 10),
    "2.4. Các câu lệnh truy vấn chính": ("_Toc242879974", 11),
    "2.5. Ma trận chức năng – dữ liệu": ("_Toc242879975", 12),
    "CHƯƠNG 3. XÂY DỰNG CƠ SỞ DỮ LIỆU": ("_Toc242879976", 13),
    "3.1. Phân tích thực thể": ("_Toc242879977", 14),
    "3.2. Lược đồ quan hệ": ("_Toc242879978", 15),
    "3.3. Thiết kế chi tiết các bảng": ("_Toc242879979", 16),
    "3.4. Mối liên hệ giữa các bảng": ("_Toc242879980", 17),
    "3.5. Ràng buộc toàn vẹn và chỉ mục": ("_Toc242879981", 18),
    "3.6. Dữ liệu mẫu đã nhập vào các bảng": ("_Toc242879982", 19),
    "3.7. View hỗ trợ truy vấn": ("_Toc242879983", 20),
    "3.8. Nhập dữ liệu vào cơ sở dữ liệu": ("_Toc242879984", 21),
    "3.9. Kết nối ứng dụng Visual Studio với SQL Server": ("_Toc242879985", 22),
    "3.10. Mã lệnh tạo từng bảng": ("_Toc242879986", 23),
    "3.11. Các truy vấn mẫu và kết quả": ("_Toc242879987", 24),
    "3.12. Kiểm thử các ràng buộc toàn vẹn": ("_Toc242879988", 25),
    "3.13. Sao lưu và phục hồi cơ sở dữ liệu": ("_Toc242879989", 26),
    "3.14. Bảo mật dữ liệu": ("_Toc242879990", 27),
    "CHƯƠNG 4. CHƯƠNG TRÌNH GIAO DIỆN": ("_Toc242879991", 28),
    "4.1. Môi trường và cách chạy chương trình": ("_Toc242879992", 29),
    "4.2. Thiết kế giao diện chung": ("_Toc242879993", 30),
    "4.3. Các màn hình của chương trình": ("_Toc242879994", 31),
    "4.4. Kịch bản kiểm thử": ("_Toc242879995", 32),
    "4.5. Hướng dẫn sử dụng chương trình": ("_Toc242879996", 33),
    "4.6. Mã nguồn tiêu biểu": ("_Toc242879997", 34),
    "4.7. Xử lý sự cố thường gặp": ("_Toc242879998", 35),
    "KẾT LUẬN VÀ HƯỚNG PHÁT TRIỂN": ("_Toc242879999", 36),
    "1. Kết quả đạt được": ("_Toc242880000", 37),
    "2. Hạn chế": ("_Toc242880001", 38),
    "3. Hướng phát triển": ("_Toc242880002", 39),
    "TÀI LIỆU THAM KHẢO": ("_Toc242880003", 40)
}

def set_cell_background(cell, hex_color):
    tcPr = cell._tc.get_or_add_tcPr()
    shd = parse_xml(f'<w:shd {nsdecls("w")} w:fill="{hex_color}" w:val="clear"/>')
    tcPr.append(shd)

def set_cell_margins(cell, top=100, bottom=100, left=120, right=120):
    tcPr = cell._tc.get_or_add_tcPr()
    tcMar = parse_xml(f'<w:tcMar {nsdecls("w")}><w:top w:w="{top}" w:type="dxa"/><w:bottom w:w="{bottom}" w:type="dxa"/><w:left w:w="{left}" w:type="dxa"/><w:right w:w="{right}" w:type="dxa"/></w:tcMar>')
    tcPr.append(tcMar)

def add_h1(doc, text):
    p = doc.add_paragraph(text, style='Heading 1')
    p.alignment = WD_ALIGN_PARAGRAPH.CENTER
    if text in TOC_BOOKMARKS:
        bm_name, bm_id = TOC_BOOKMARKS[text]
        bm_start = parse_xml(f'<w:bookmarkStart {nsdecls("w")} w:id="{bm_id}" w:name="{bm_name}"/>')
        bm_end = parse_xml(f'<w:bookmarkEnd {nsdecls("w")} w:id="{bm_id}"/>')
        p._p.insert(0, bm_start)
        p._p.append(bm_end)
    return p

def add_h2(doc, text):
    p = doc.add_paragraph(text, style='Heading 2')
    if text in TOC_BOOKMARKS:
        bm_name, bm_id = TOC_BOOKMARKS[text]
        bm_start = parse_xml(f'<w:bookmarkStart {nsdecls("w")} w:id="{bm_id}" w:name="{bm_name}"/>')
        bm_end = parse_xml(f'<w:bookmarkEnd {nsdecls("w")} w:id="{bm_id}"/>')
        p._p.insert(0, bm_start)
        p._p.append(bm_end)
    return p

def add_h3(doc, text):
    p = doc.add_paragraph(text, style='Heading 3')
    return p

def add_p(doc, text, bold=False, italic=False):
    p = doc.add_paragraph(style='Normal')
    p.alignment = WD_ALIGN_PARAGRAPH.JUSTIFY
    r = p.add_run(text)
    r.bold = bold
    r.italic = italic
    return p

def add_bullet(doc, text, bold_prefix=""):
    p = doc.add_paragraph(style='List Paragraph')
    p.alignment = WD_ALIGN_PARAGRAPH.JUSTIFY
    if bold_prefix:
        r_pre = p.add_run(bold_prefix)
        r_pre.bold = True
    p.add_run(text)
    return p

def set_table_borders(table, color="B0C4DE", sz="4", val="single"):
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

def add_table(doc, headers, data, col_widths=None, alignments=None, caption=None):
    if caption:
        p_cap = doc.add_paragraph(style='Normal')
        p_cap.alignment = WD_ALIGN_PARAGRAPH.CENTER
        r_cap = p_cap.add_run(caption)
        r_cap.bold = True

    num_cols = len(headers)
    tbl = doc.add_table(rows=len(data) + 1, cols=num_cols)
    tbl.alignment = WD_TABLE_ALIGNMENT.CENTER
    set_table_borders(tbl)

    hdr_row = tbl.rows[0]
    trPr = hdr_row._tr.get_or_add_trPr()
    trPr.append(parse_xml(f'<w:tblHeader {nsdecls("w")}/>'))

    for j, h in enumerate(headers):
        c = hdr_row.cells[j]
        set_cell_background(c, "D9E6F8")
        set_cell_margins(c, top=100, bottom=100, left=120, right=120)
        p = c.paragraphs[0]
        p.alignment = WD_ALIGN_PARAGRAPH.CENTER
        r = p.add_run(h)
        r.bold = True

    for i, row_data in enumerate(data):
        row = tbl.rows[i + 1]
        is_even = (i % 2 == 1)
        bg = "F8FAFD" if is_even else "FFFFFF"
        for j, val in enumerate(row_data):
            c = row.cells[j]
            set_cell_background(c, bg)
            set_cell_margins(c, top=80, bottom=80, left=120, right=120)
            p = c.paragraphs[0]
            align = alignments[j] if alignments and j < len(alignments) else WD_ALIGN_PARAGRAPH.LEFT
            p.alignment = align
            p.add_run(str(val))

    if col_widths:
        for row in tbl.rows:
            for j, w in enumerate(col_widths):
                if j < len(row.cells):
                    row.cells[j].width = Inches(w)

    doc.add_paragraph(style='Normal')
    return tbl

def add_figure(doc, img_path, caption_text, width_inches=6.0):
    if not os.path.exists(img_path):
        return
    p_img = doc.add_paragraph(style='Normal')
    p_img.alignment = WD_ALIGN_PARAGRAPH.CENTER
    p_img.add_run().add_picture(img_path, width=Inches(width_inches))

    p_cap = doc.add_paragraph(style='Normal')
    p_cap.alignment = WD_ALIGN_PARAGRAPH.CENTER
    r = p_cap.add_run(caption_text)
    r.italic = True
    doc.add_paragraph(style='Normal')

def add_code(doc, code_str):
    lines = code_str.strip().split('\n')
    for line in lines:
        p = doc.add_paragraph(style='Normal')
        p.paragraph_format.line_spacing = 1.15
        p.paragraph_format.space_after = Pt(0)
        p.paragraph_format.left_indent = Inches(0.2)
        r = p.add_run(line)
        r.font.name = 'Consolas'
        r.font.size = Pt(9.5)
    doc.add_paragraph(style='Normal')

def generate_report():
    template_path = r"C:\Users\Nguyen Nhi\Downloads\Báo Cáo Quản lý Sinh viên 111.docx"
    print(f"Đang nạp file mẫu: {template_path}")
    doc = docx.Document(template_path)
    body = doc._body._body
    ns = {'w': 'http://schemas.openxmlformats.org/wordprocessingml/2006/main'}

    # 1. CẬP NHẬT TRANG BÌA (Cover page)
    print("1. Cập nhật trang bìa sang đề tài Quản Lý Khách Sạn...")
    c0 = body[0]
    for t in c0.findall('.//w:t', ns):
        if 'Quản Lý Danh Sách Sinh Viên' in t.text:
            t.text = t.text.replace('Quản Lý Danh Sách Sinh Viên', 'Quản Lý Khách Sạn')
        elif 'Nguyễn Thị Khánh Hòa' in t.text:
            t.text = t.text.replace('Nguyễn Thị Khánh Hòa', 'Anh Nhi (Nguyễn Nhi) - Trưởng nhóm')
        elif t.text == '0':
            t.text = '1'

    # 2. CẬP NHẬT BẢNG PHÂN CÔNG NHIỆM VỤ (Table 0)
    print("2. Cập nhật Bảng phân công nhiệm vụ nhóm...")
    t0 = doc.tables[0]
    assignment_rows = [
        ("1", "Anh Nhi (Nguyễn Nhi)\nNhóm trưởng",
         "• Thiết kế kiến trúc tổng thể 3-Tier (GUI - BLL - DAL - DTO).\n• Xây dựng CSDL SQL Server V26: 20 bảng chuẩn, ràng buộc, chỉ mục, khóa ứng dụng sp_getapplock.\n• Viết SchemaMigrator.cs: tự động nâng cấp CSDL không mất dữ liệu.\n• Viết HotelRepository.cs, HotelTransaction.Commands.cs, HotelService.cs.\n• Thuật toán tính tiền phòng block 24h, đổi phòng, hủy cọc Forfeit.",
         "• Chương 3: Xây dựng CSDL.\n• Chương 2: Kiến trúc hệ thống, CRUD Matrix.\n• Tổng hợp và hiệu đính toàn bộ báo cáo Word.",
         "15 kịch bản:\n- Khởi tạo & Migration\n- Đặt phòng & Trùng lịch\n- Tính tiền & Hủy cọc"),
        ("2", "Nguyễn Thị Khánh Hòa\nThành viên",
         "• Xây dựng Program.cs, Theme, WorkspaceSkin, ModernControls.\n• Màn hình đăng nhập FormLogin.cs: xác thực PBKDF2 SHA-512 với Salt, chống Brute-Force, RememberedLogin DPAPI.\n• Khung điều hướng chính FormMain.cs, Navigation, Profile header, Avatar, đồng hồ hệ thống.",
         "• Lời mở đầu.\n• Chương 1: Giới thiệu đề tài, phạm vi, công nghệ, danh mục thông báo.\n• Mục 4.1, 4.2: Giao diện nền tảng.",
         "10 kịch bản:\n- Đăng nhập & Khóa tạm\n- Đổi mật khẩu\n- Phân quyền theo vai trò"),
        ("3", "Thân Mạnh Hùng\nThành viên",
         "• Xây dựng Sơ đồ phòng trực quan ucDashboard.RoomOverview.cs (các thẻ phòng đổi màu theo trạng thái thực tế).\n• Module đặt phòng ucDashboard.BookingDialog.cs, MultipleBooking.cs.\n• Module lưu trú Check-in, đổi phòng, gia hạn phòng, hủy lượt.",
         "• Chương 1: Nghiệp vụ buồng phòng & lưu trú.\n• Chương 2: Đặc tả chức năng đặt phòng, check-in/out.\n• Mục 4.3: Giao diện phòng & đặt phòng.",
         "12 kịch bản:\n- Đặt phòng trước\n- Đặt đoàn nhiều phòng\n- Check-in đúng hẹn/trễ"),
        ("4", "Lương Thị Quyền &\nNguyễn Thị Thư\nThành viên",
         "• Module Dịch vụ & Minibar, xuất kho, kho hàng HangTonKho.\n• Module Thanh toán hóa đơn & PaymentQr.cs (mã VietQR động NAPAS).\n• Phân hệ Kế toán & Ca trực AccountingForm.cs: mở ca, nộp ca, giải trình lệch quỹ, khóa ca trực.\n• Module Báo cáo doanh thu và CustomScreenDesigner.cs.",
         "• Chương 1: Nghiệp vụ ca trực & kế toán.\n• Chương 2: Báo cáo & Custom Screen.\n• Mục 4.3: Dịch vụ, VietQR, Ca trực, Báo cáo.\n• Kết luận & Tài liệu tham khảo.",
         "15 kịch bản:\n- Dịch vụ minibar\n- Thanh toán VietQR\n- Khóa ca & Báo cáo")
    ]
    for row_idx, row_data in enumerate(assignment_rows):
        row = t0.rows[row_idx + 1]
        for col_idx, text_val in enumerate(row_data):
            cell = row.cells[col_idx]
            cell.text = text_val

    # 3. KÍCH HOẠT UPDATE FIELDS TỰ ĐỘNG CHO TOC
    print("3. Kích hoạt tính năng tự động cập nhật trường trong settings.xml...")
    try:
        updateFields = parse_xml(f'<w:updateFields {nsdecls("w")} w:val="true"/>')
        doc.settings.element.append(updateFields)
    except Exception as e:
        print(f"UpdateFields notice: {e}")

    # 4. XÓA NỘI DUNG CŨ TỪ PHẦN THÂN (Giữ lại trang bìa, TOC, bảng phân công và sectPr)
    print("4. Xóa nội dung thân văn bản cũ...")
    # Elements to keep: body[0]..body[6] and body[-1] (sectPr)
    for i in range(len(body) - 2, 6, -1):
        body.remove(body[i])

    # 5. XÂY DỰNG NỘI DUNG MỚI THEO CẤU TRÚC 41 MỤC LỤC CHUẨN
    print("5. Đang nạp nội dung chi tiết hệ thống Quản lý Khách sạn...")

    # ==========================================
    # LỜI MỞ ĐẦU
    # ==========================================
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
    add_bullet(doc, "Trình bày tổng quan mục tiêu đề tài, phạm vi ứng dụng, các yêu cầu chức năng và phi chức năng, đối tượng sử dụng, quy trình nghiệp vụ tổng thể và quy tắc nghiệp vụ cốt lõi của khách sạn.", "Phần 1: Mô tả chức năng và nghiệp vụ – ")
    add_bullet(doc, "Đi sâu phân tích đặc tả chi tiết 8 nhóm chức năng chính, kiến trúc hệ thống đa tầng 3-Tier, mẫu thiết kế xử lý giao dịch, các truy vấn nghiệp vụ phức tạp và ma trận chức năng – dữ liệu (CRUD Matrix).", "Phần 2: Phân tích chức năng – ")
    add_bullet(doc, "Trình bày chi tiết phân tích thực thể, lược đồ quan hệ ERD, thiết kế chi tiết 20 bảng cơ sở dữ liệu chuẩn V26, các ràng buộc toàn vẹn, cơ chế tự động nâng cấp CSDL SchemaMigrator, bảo mật mật khẩu PBKDF2 SHA-512 và các kịch bản kiểm thử toàn vẹn.", "Phần 3: Xây dựng cơ sở dữ liệu – ")
    add_bullet(doc, "Giới thiệu môi trường triển khai, triết lý thiết kế giao diện hiện đại, thuyết minh chi tiết 11 màn hình chức năng kèm ảnh chụp thực tế từ hệ thống, bảng kịch bản kiểm thử toàn diện, hướng dẫn vận hành, mã nguồn tiêu biểu và xử lý sự cố thường gặp.", "Phần 4: Chương trình giao diện – ")
    add_p(doc, "Mặc dù nhóm chúng em đã dành trọn tâm huyết, nỗ lực nghiên cứu và hoàn thiện đề tài với tinh thần trách nhiệm cao nhất, song do giới hạn về thời gian và kinh nghiệm thực tế, chương trình và báo cáo khó tránh khỏi những điểm còn thiếu sót. Nhóm chúng em rất mong nhận được những nhận xét, góp ý quý báu và sự chỉ bảo tận tình của Cô Trương Thị Nhung cùng các Thầy/Cô trong Hội đồng chấm đồ án để sản phẩm ngày càng hoàn thiện, có thể ứng dụng rộng rãi và hiệu quả vào thực tế các doanh nghiệp lưu trú.")
    add_p(doc, "Chúng em xin trân trọng cảm ơn!", bold=True, italic=True)

    # ==========================================
    # CHƯƠNG 1
    # ==========================================
    add_h1(doc, "CHƯƠNG 1. MÔ TẢ CHỨC NĂNG VÀ MÔ TẢ NGHIỆP VỤ")

    add_h2(doc, "1.1. Giới thiệu đề tài")
    add_h3(doc, "1.1.1. Tên đề tài và mục tiêu")
    add_p(doc, "Tên đề tài: Xây dựng phần mềm “Quản lý Khách sạn” (Hotel Management System).", bold=True)
    add_p(doc, "Mục tiêu tổng quát của đề tài là xây dựng một hệ thống phần mềm chuyên nghiệp, hoàn chỉnh trên nền tảng máy tính để bàn (Windows Desktop Application) nhằm tin học hóa và tự động hóa toàn diện các nghiệp vụ quản lý phòng, đặt phòng, tiếp nhận khách lưu trú, cung ứng dịch vụ, quản lý kho minibar, xuất hóa đơn thanh toán và quản trị ca trực tại các khách sạn, nhà nghỉ, resort quy mô vừa và nhỏ.")
    add_bullet(doc, "Xây dựng ứng dụng desktop có giao diện phẳng hiện đại (Modern Fluent UI), màu sắc và bố cục trực quan hóa sơ đồ phòng theo màu sắc trạng thái thời gian thực, dễ sử dụng cho nhân viên lễ tân.", "• ")
    add_bullet(doc, "Quản lý đầy đủ các nhóm danh mục: Phòng, Loại phòng, Dịch vụ, Khách hàng, Ca trực với các thao tác thêm, sửa, xóa, tra cứu tức thời.", "• ")
    add_bullet(doc, "Tách biệt hoàn toàn giữa trạng thái vật lý của phòng và lịch đặt phòng theo thời gian; tích hợp chính sách giữ chỗ 24h (chưa cọc) và 15 ngày (đã cọc); tự động phát hiện và hủy lượt đặt quá hạn.", "• ")
    add_bullet(doc, "Tích hợp công nghệ thanh toán quét mã VietQR động theo chuẩn NAPAS 247, tự động sinh mã QR chứa đúng số tiền và cú pháp chuyển khoản.", "• ")
    add_bullet(doc, "Quản lý dòng tiền mặt chặt chẽ thông qua quy trình mở ca, nộp ca, giải trình lệch quỹ và cơ chế khóa ca trực ngăn chặn sửa đổi hồi tố.", "• ")
    add_bullet(doc, "Lưu trữ dữ liệu tập trung trong SQL Server, hỗ trợ tự động nâng cấp CSDL SchemaMigrator không mất dữ liệu lịch sử.", "• ")

    add_h3(doc, "1.1.2. Phạm vi và công nghệ sử dụng")
    add_p(doc, "Phần mềm chạy trên máy tính Windows 10/11, dùng cho các bộ phận Lễ tân, Kế toán và Quản lý khách sạn kết nối cơ sở dữ liệu SQL Server tập trung. Các công nghệ và công cụ sử dụng được tóm tắt ở Bảng 1.1.")
    hdr_1_1 = ["Thành phần", "Công nghệ / công cụ sử dụng"]
    data_1_1 = [
        ["Ngôn ngữ lập trình", "C# (phiên bản C# 10 / C# 12)"],
        ["Nền tảng ứng dụng", "Microsoft .NET 10 (Windows Desktop Runtime)"],
        ["Giao diện", "Windows Forms (vẽ giao diện thuần mã nguồn Code-only, kết hợp ModernControls tùy biến)"],
        ["Hệ quản trị cơ sở dữ liệu", "Microsoft SQL Server 2019 / 2022 / SQL Server Express"],
        ["Thư viện truy cập dữ liệu", "Microsoft.Data.SqlClient (ADO.NET, Parameterized Queries)"],
        ["Bảo mật mật khẩu", "Thuật toán băm PBKDF2 SHA-512 với muối ngẫu nhiên (Salt 16 bytes)"],
        ["Tích hợp thanh toán", "VietQR Open API (Chuẩn thanh toán QR quốc gia NAPAS 247)"],
        ["Công cụ phát triển", "Visual Studio 2022 / Visual Studio Code / Git"]
    ]
    add_table(doc, hdr_1_1, data_1_1, col_widths=[2.5, 4.5], caption="Bảng 1.1. Công nghệ và công cụ sử dụng")

    add_h2(doc, "1.2. Mô tả chức năng")
    add_p(doc, "Hệ thống gồm 8 nhóm chức năng chính. Bảng 1.2 liệt kê đầy đủ các chức năng và mô tả ngắn gọn từng chức năng.")
    hdr_1_2 = ["STT", "Chức năng", "Mô tả nghiệp vụ"]
    data_1_2 = [
        ["1", "Đăng nhập / Đăng xuất", "Xác thực danh tính bằng tên đăng nhập và mật khẩu băm PBKDF2; hỗ trợ ẩn/hiện mật khẩu và ghi nhớ an toàn qua Windows DPAPI."],
        ["2", "Quản trị người dùng & Phân quyền", "Tạo tài khoản nhân viên, đặt lại mật khẩu, khóa/mở tài khoản, cấp quyền chức năng chi tiết cho 4 vai trò: Admin, Reception, Accountant, Manager."],
        ["3", "Sơ đồ phòng trực quan", "Hiển thị trạng thái vật lý của tất cả phòng bằng màu sắc thời gian thực: Trống, Đang ở, Cần dọn, Bảo trì; dấu sao (*) báo có lịch đặt."],
        ["4", "Đặt phòng trước (Booking)", "Tra cứu phòng trống theo khoảng thời gian đến và số ngày ở; nhập thông tin khách; hỗ trợ đặt phòng lẻ và đặt phòng theo đoàn."],
        ["5", "Nhận phòng (Check-in)", "Làm thủ tục nhận phòng cho khách vãng lai (Walk-in) hoặc nhận phòng từ lịch đặt trước khi phòng đã dọn sạch."],
        ["6", "Thu cọc & Quản lý giữ chỗ", "Ghi nhận tiền cọc (nhiều đợt); mở rộng hạn giữ chỗ lên 15 ngày; tự động hủy lượt và tịch thu cọc (Forfeit) khi quá hạn."],
        ["7", "Đổi phòng & Gia hạn lưu trú", "Chuyển khách sang phòng khác cùng hoặc khác hạng phòng; hệ thống tự chia tiền phòng theo giá từng chặng; gia hạn thêm ngày ở."],
        ["8", "Dịch vụ buồng phòng & Minibar", "Gọi dịch vụ đồ ăn, thức uống minibar, giặt ủi; hỗ trợ giao từng phần; tự động ghi nhận xuất bán trừ tồn kho hàng hóa."],
        ["9", "Trả phòng (Check-out) & Tính tiền", "Tự động tính tiền phòng theo block 24h, cộng tiền dịch vụ đã giao, trừ tiền cọc; hỗ trợ tiền mặt và tạo mã VietQR chuyển khoản."],
        ["10", "Quản lý ca trực thu ngân", "Mở ca với tiền mặt đầu ca; nộp tiền cuối ca với biên bản giải trình chênh lệch; khóa ca trực không cho chỉnh sửa."],
        ["11", "Báo cáo doanh thu & Công suất phòng", "Thống kê doanh thu theo ngày/tháng/khoảng thời gian; tỷ lệ lấp đầy phòng; phân loại nguồn thu; xuất báo cáo ra CSV UTF-8."],
        ["12", "Trình thiết kế màn hình động", "Cho phép Admin tự tạo thêm các trường nhập liệu và màn hình nghiệp vụ phụ (Custom Screen Designer) lưu trữ động trong CSDL."]
    ]
    add_table(doc, hdr_1_2, data_1_2, col_widths=[0.6, 2.4, 4.0], caption="Bảng 1.2. Danh sách chức năng của hệ thống")

    add_h3(doc, "1.2.1. Yêu cầu phi chức năng")
    add_bullet(doc, "Chương trình tự kiểm tra phiên bản CSDL và áp dụng script nâng cấp (Schema Migration) khi chạy lần đầu; chạy lại nhiều lần không phát sinh lỗi hoặc dữ liệu trùng.", "• Độ tin cậy: ")
    add_bullet(doc, "Mọi thao tác đặt phòng, đổi phòng, thu cọc, checkout đều được bảo vệ trong Transaction với khóa độc quyền sp_getapplock, tự Rollback nếu có lỗi.", "• Toàn vẹn giao dịch (ACID): ")
    add_bullet(doc, "Thời gian phản hồi các thao tác tải sơ đồ phòng, tính tiền hóa đơn dưới 500ms.", "• Hiệu năng: ")
    add_bullet(doc, "Mật khẩu băm PBKDF2 SHA-512 với Salt; khóa tạm 5 phút khi sai quá 5 lần; vô hiệu hóa phiên cũ qua SecurityVersion.", "• An toàn bảo mật: ")
    add_bullet(doc, "Giao diện hỗ trợ tự động co giãn theo tỷ lệ DPI màn hình (100%, 125%, 150%) trên Windows 10/11.", "• Tương thích hiển thị: ")

    add_h2(doc, "1.3. Mô tả nghiệp vụ")
    add_h3(doc, "1.3.1. Đối tượng sử dụng")
    add_p(doc, "Hệ thống phục vụ 4 nhóm đối tượng chính trong khách sạn:")
    add_bullet(doc, "Toàn quyền quản trị hệ thống, tài khoản, phân quyền chức năng, nhật ký hệ thống và thiết kế màn hình động.", "• Quản trị viên (Admin): ")
    add_bullet(doc, "Vận hành hàng ngày: kiểm tra sơ đồ phòng, tiếp nhận khách, đặt phòng, thu cọc, check-in, đổi phòng, gọi dịch vụ, check-out và quản lý ca cá nhân.", "• Nhân viên Lễ tân (Receptionist): ")
    add_bullet(doc, "Theo dõi doanh thu, kiểm tra sổ quỹ tiền mặt, xem hóa đơn, duyệt chiết khấu, đối soát sao kê và khóa ca/kỳ kế toán.", "• Kế toán (Accountant): ")
    add_bullet(doc, "Theo dõi tổng thể vận hành, xem báo cáo doanh thu, tỷ lệ lấp đầy, quản lý danh mục phòng và bảng giá.", "• Quản lý khách sạn (Manager): ")

    add_h3(doc, "1.3.2. Quy trình nghiệp vụ tổng quát")
    add_p(doc, "Quy trình nghiệp vụ tổng thể trải qua 6 bước chính:")
    add_bullet(doc, "Khách liên hệ; lễ tân tra cứu phòng trống theo ngày đến và số ngày ở; nhập thông tin khách và tạo lượt đặt.", "1. Tiếp nhận đặt phòng: ")
    add_bullet(doc, "Nếu chưa cọc giữ tối đa 24 giờ; khi khách nộp cọc hệ thống tự động mở rộng hạn giữ chỗ lên 15 ngày.", "2. Quản lý hạn giữ chỗ & cọc: ")
    add_bullet(doc, "Khách đến trong hạn; đối chiếu giấy tờ và bấm Nhận phòng; phòng chuyển sang 'Đang ở', kích hoạt chặng lưu trú.", "3. Làm thủ tục Check-in: ")
    add_bullet(doc, "Khách gọi đồ uống minibar hoặc giặt ủi; nhân viên ghi nhận yêu cầu; khi giao hàng tự động trừ kho minibar.", "4. Phục vụ dịch vụ minibar: ")
    add_bullet(doc, "Tính tiền phòng theo block 24h, cộng tiền dịch vụ đã giao, trừ tiền cọc; khách trả tiền mặt hoặc quét mã VietQR; in hóa đơn.", "5. Làm thủ tục Check-out: ")
    add_bullet(doc, "Phòng chuyển sang 'Cần dọn'; buồng phòng vệ sinh, thay ga gối; bấm 'Đã dọn xong' để phòng quay lại trạng thái 'Trống'.", "6. Dọn phòng & sẵn sàng đón khách: ")

    add_h3(doc, "1.3.3. Mô tả nghiệp vụ từng nhóm")
    add_p(doc, "Mỗi nhóm nghiệp vụ được chuyên môn hóa cao:")
    add_bullet(doc, "Phân tách giữa trạng thái vật lý và lịch đặt trước; hỗ trợ đặt nhiều phòng (đặt đoàn); hỗ trợ đổi phòng và chia giá theo chặng.", "• Nghiệp vụ buồng phòng & lưu trú: ")
    add_bullet(doc, "Quản lý danh mục mặt hàng minibar, mức tồn kho an toàn; xuất bán tự động khi giao dịch vụ; kiểm kê và xuất hủy đồ hỏng.", "• Nghiệp vụ dịch vụ & kho minibar: ")
    add_bullet(doc, "Thu tiền cọc, thanh toán checkout, chi hoàn cọc thừa, ghi nhận tịch thu cọc quá hạn Forfeit; tạo mã VietQR động chuẩn NAPAS.", "• Nghiệp vụ thu ngân & thanh toán: ")
    add_bullet(doc, "Mở ca với tiền mặt đầu ca; đếm tiền cuối ca; ghi nhận biên bản giải trình chênh lệch; khóa ca không cho sửa hồi tố.", "• Nghiệp vụ ca trực & kế toán: ")

    add_h3(doc, "1.3.4. Các trạng thái của phòng và lượt lưu trú")
    add_p(doc, "Bảng 1.3 mô tả các trạng thái vật lý của phòng và trạng thái nghiệp vụ của lượt lưu trú.")
    hdr_1_3 = ["Đối tượng", "Trạng thái", "Ý nghĩa nghiệp vụ", "Màu hiển thị"]
    data_1_3 = [
        ["Phòng vật lý", "Trống", "Phòng đã dọn dẹp sạch sẽ, sẵn sàng đón khách vào ở ngay.", "Xanh lá cây"],
        ["Phòng vật lý", "Đang ở", "Khách đang lưu trú tại phòng.", "Đỏ đậm"],
        ["Phòng vật lý", "Cần dọn", "Khách vừa trả phòng, phòng cần nhân viên dọn vệ sinh.", "Cam hổ phách"],
        ["Phòng vật lý", "Bảo trì", "Phòng đang sửa chữa thiết bị, không nhận khách.", "Xám ghi"],
        ["Lượt lưu trú", "Chờ nhận", "Khách đã đặt phòng trước, đang trong thời hạn giữ chỗ.", "Xanh dương"],
        ["Lượt lưu trú", "Đang ở", "Khách đã làm thủ tục nhận phòng và đang lưu trú.", "Đỏ"],
        ["Lượt lưu trú", "Hoàn tất", "Khách đã thanh toán hóa đơn và hoàn tất trả phòng.", "Xanh lục"],
        ["Lượt lưu trú", "Đã hủy", "Lượt đặt bị hủy trước hạn; hệ thống hoàn trả toàn bộ cọc.", "Tím nhạt"],
        ["Lượt lưu trú", "Mất cọc", "Lượt đặt quá hạn nhận mà khách không đến (No-Show); cọc bị tịch thu.", "Đỏ thẫm"]
    ]
    add_table(doc, hdr_1_3, data_1_3, col_widths=[1.5, 1.5, 2.7, 1.3], caption="Bảng 1.3. Trạng thái của phòng và lượt lưu trú")

    add_h3(doc, "1.3.5. Quy tắc nghiệp vụ")
    add_p(doc, "Bảng 1.4 liệt kê 6 quy tắc nghiệp vụ cốt lõi (QT01 - QT06) được cài đặt trực tiếp trong mã nguồn và cơ sở dữ liệu:")
    hdr_1_4 = ["Mã", "Nội dung quy tắc nghiệp vụ"]
    data_1_4 = [
        ["QT01", "Thời gian lưu trú tối thiểu tính là 1 ngày (24 giờ). Tổng số giờ lưu trú được làm tròn lên block 24 giờ tiếp theo. Nếu khách đổi phòng, thời gian thực tế ở phòng cũ tính theo đơn giá phòng cũ; khoảng thời gian còn thiếu tính theo giá phòng mới."],
        ["QT02", "Thời lượng lưu trú đặt trước từ 1 đến 60 ngày. Lượt đặt chưa thu cọc giữ tối đa 24 giờ kể từ thời điểm tạo. Khi thu cọc, hạn giữ chỗ tối đa được mở rộng lên 15 ngày, nhưng hạn nhận phòng không được vượt quá ngày trả phòng."],
        ["QT03", "Tại thời điểm chính xác hoặc sau hạn nhận phòng mà khách chưa check-in, hệ thống tự động đánh dấu hủy lượt đặt. Tiền cọc đã thu sẽ bị tịch thu toàn bộ (Forfeit - No Show) để bồi thường thiệt hại; hệ thống từ chối mọi yêu cầu hoàn cọc."],
        ["QT04", "Một phòng vật lý được phép có nhiều lượt đặt trước với điều kiện các khoảng thời gian [Ngày đến, Ngày đi) hoàn toàn không giao nhau. Khi làm thủ tục nhận phòng thực tế, bắt buộc phòng phải ở trạng thái Trống và đã dọn xong."],
        ["QT05", "Hệ thống chỉ cho phép thực hiện thủ tục Check-out khi tất cả các yêu cầu dịch vụ buồng phòng đã được xác nhận giao đủ hoặc hủy bỏ. Tuyệt đối không cho phép thanh toán khi còn dịch vụ đang ở trạng thái 'Đang chờ'."],
        ["QT06", "Mọi giao dịch thanh toán phải gắn liền với một Ca trực đang mở. Khi ca trực đã được bàn giao và khóa, trigger trong cơ sở dữ liệu sẽ chặn mọi thao tác thêm, sửa, xóa tác động vào các chứng từ thuộc ca đó."]
    ]
    add_table(doc, hdr_1_4, data_1_4, col_widths=[0.8, 6.2], caption="Bảng 1.4. Danh mục quy tắc nghiệp vụ cốt lõi")

    add_h2(doc, "1.4. Khảo sát hiện trạng và yêu cầu người dùng")
    add_h3(doc, "1.4.1. Hiện trạng quản lý khách sạn")
    add_p(doc, "Khảo sát thực tế tại các khách sạn quy mô từ 15 đến 60 phòng cho thấy hầu hết các đơn vị vẫn áp dụng mô hình quản lý thủ công kết hợp sổ sách Excel rời rạc. Điều này dẫn đến nguy cơ đặt trùng phòng, sai sót tính tiền, thất thoát minibar và mất nhiều thời gian bàn giao ca trực.")

    add_h3(doc, "1.4.2. Giải pháp đề xuất")
    add_p(doc, "Xây dựng phần mềm quản lý tập trung trên nền tảng C# Windows Forms và Microsoft SQL Server, giải quyết triệt để các bất cập: đồng bộ hóa dữ liệu thời gian thực, tự động tính tiền, trực quan hóa sơ đồ phòng và tích hợp thanh toán quét mã VietQR động.")

    add_h3(doc, "1.4.3. Yêu cầu của người dùng")
    add_p(doc, "Phần mềm đáp ứng đầy đủ yêu cầu của từng bộ phận: Lễ tân thao tác nhanh gọn dưới 3 cú nhấp chuột; Kế toán theo dõi chặt chẽ dòng tiền và khóa ca an toàn; Quản lý nắm bắt tức thời tỷ lệ lấp đầy phòng và doanh thu tổng hợp.")

    add_h3(doc, "1.4.4. Các giai đoạn thực hiện đề tài")
    add_p(doc, "Đề tài được triển khai qua 5 giai đoạn: 1. Khảo sát nghiệp vụ; 2. Phân tích chức năng (Chương 2); 3. Thiết kế CSDL (Chương 3, tệp Database.sql); 4. Lập trình giao diện và nghiệp vụ (Chương 4); 5. Kiểm thử toàn diện và đóng gói ứng dụng.")

    add_h2(doc, "1.5. Mô tả dữ liệu vào/ra của các biểu mẫu")
    add_p(doc, "Phần này mô tả chi tiết từng ô nhập liệu: kiểu điều khiển, độ dài, tính bắt buộc và quy tắc kiểm tra được cài đặt trong chương trình.")

    add_h3(doc, "1.5.1. Biểu mẫu Đăng nhập")
    hdr_io = ["Tên trường", "Tên hiển thị", "Kiểu điều khiển", "Bắt buộc", "Quy tắc kiểm tra hợp lệ"]
    data_login = [
        ["TenDangNhap", "Tên đăng nhập", "TextBox", "Có", "4-30 ký tự, chỉ chứa chữ cái, số, dấu chấm hoặc gạch dưới."],
        ["MatKhau", "Mật khẩu", "PasswordBox", "Có", "Tối thiểu 6 ký tự. Kiểm tra khớp với bản băm PBKDF2 SHA-512."],
        ["GhiNho", "Ghi nhớ đăng nhập", "CheckBox", "Không", "Mặc định false; nếu chọn sẽ lưu an toàn qua Windows DPAPI."]
    ]
    add_table(doc, hdr_io, data_login, col_widths=[1.5, 1.5, 1.2, 0.8, 2.0], caption="Bảng 1.5. Biểu mẫu Đăng nhập")

    add_h3(doc, "1.5.2. Biểu mẫu Đặt phòng")
    data_book = [
        ["MaPhong", "Phòng chọn", "ComboBox", "Có", "Phải chọn phòng khả dụng trong khoảng thời gian lưu trú."],
        ["TenKhach", "Họ tên khách", "TextBox", "Có", "Tối đa 100 ký tự, không được để trống."],
        ["SoDienThoai", "Số điện thoại", "TextBox", "Có", "Từ 9-11 chữ số, đúng định dạng số điện thoại Việt Nam."],
        ["SoGiayTo", "CCCD / Hộ chiếu", "TextBox", "Không", "Định dạng 9 hoặc 12 số CCCD hoặc số hộ chiếu hợp lệ."],
        ["NgayBatDau", "Ngày nhận phòng", "DateTimePicker", "Có", "Phải từ thời điểm hiện tại trở đi."],
        ["NgayKetThuc", "Ngày trả phòng", "DateTimePicker", "Có", "Phải sau Ngày nhận phòng ít nhất 1 ngày."],
        ["TienCoc", "Tiền cọc nộp", "NumericTextBox", "Không", "Số tiền >= 0. Nếu nộp cọc thì hạn giữ chỗ chuyển thành 15 ngày."]
    ]
    add_table(doc, hdr_io, data_book, col_widths=[1.5, 1.5, 1.2, 0.8, 2.0], caption="Bảng 1.6. Biểu mẫu Đặt phòng")

    add_h3(doc, "1.5.3. Biểu mẫu Trả phòng (Check-out)")
    data_co = [
        ["MaLuotLuuTru", "Mã lượt ở", "Label (ReadOnly)", "Có", "Mã định danh duy nhất của lượt khách đang ở."],
        ["TongTienPhong", "Tiền phòng", "Label (ReadOnly)", "Có", "Hệ thống tự tính theo số chặng và block 24h."],
        ["TongTienDichVu", "Tiền dịch vụ", "Label (ReadOnly)", "Có", "Tổng tiền của các yêu cầu dịch vụ đã giao."],
        ["TienCocDaThu", "Đã thu cọc", "Label (ReadOnly)", "Có", "Tổng các giao dịch cọc khách đã thanh toán."],
        ["PhuongThuc", "Phương thức", "ComboBox", "Có", "Một trong ba giá trị: 'Cash', 'BankTransfer', 'CreditCard'."],
        ["TienKhachDua", "Khách trả", "NumericTextBox", "Có", "Phải >= Số tiền thực thu cần thanh toán."]
    ]
    add_table(doc, hdr_io, data_co, col_widths=[1.5, 1.5, 1.2, 0.8, 2.0], caption="Bảng 1.7. Biểu mẫu Trả phòng")

    add_h2(doc, "1.6. Danh mục thông báo của hệ thống")
    add_p(doc, "Chương trình thông báo cho người dùng bằng tiếng Việt qua hộp thoại chuyên nghiệp. Bảng 1.12 liệt kê các thông báo chính.")
    hdr_msg = ["Mã TB", "Loại thông báo", "Nội dung thông báo hiển thị", "Hướng xử lý"]
    data_msg = [
        ["TB01", "Lỗi xác thực", "Tên đăng nhập hoặc mật khẩu không chính xác.", "Kiểm tra lại bàn phím và phím CapsLock."],
        ["TB02", "Cảnh báo bảo mật", "Tài khoản bị khóa tạm thời 5 phút do sai quá 5 lần liên tiếp.", "Chờ hết 5 phút hoặc nhờ Admin mở khóa."],
        ["TB03", "Cảnh báo nghiệp vụ", "Phòng đã có khách đặt trong khoảng thời gian này!", "Chọn phòng khác hoặc điều chỉnh ngày đặt."],
        ["TB04", "Cảnh báo nghiệp vụ", "Lượt đặt phòng đã quá hạn nhận phòng. Hệ thống đã hủy lượt đặt!", "Tiền cọc bị tịch thu (Forfeit - No Show)."],
        ["TB05", "Ràng buộc kiểm tra", "Không thể thanh toán trả phòng khi vẫn còn dịch vụ 'Đang chờ'!", "Giao đủ hoặc hủy dòng dịch vụ trước khi checkout."],
        ["TB06", "Cảnh báo phân quyền", "Bạn không có quyền thực hiện chức năng này!", "Liên hệ Quản trị viên để được cấp quyền."],
        ["TB07", "Thông báo thành công", "Lưu đặt phòng thành công cho khách hàng!", "Dữ liệu đã được ghi an toàn vào CSDL."],
        ["TB08", "Thông báo thành công", "Thanh toán hóa đơn thành công. Phòng chuyển sang 'Cần dọn'!", "In hóa đơn và kích hoạt quy trình dọn phòng."]
    ]
    add_table(doc, hdr_msg, data_msg, col_widths=[0.8, 1.4, 3.0, 1.8], caption="Bảng 1.12. Danh mục thông báo của hệ thống")

    # ==========================================
    # CHƯƠNG 2
    # ==========================================
    add_h1(doc, "CHƯƠNG 2. PHÂN TÍCH CHỨC NĂNG")

    add_h2(doc, "2.1. Đặc tả chi tiết các chức năng chính")
    add_h3(doc, "2.1.1. Chức năng Xác thực & Quản trị tài khoản")
    add_p(doc, "Xác thực bằng tên đăng nhập và mật khẩu băm PBKDF2 SHA-512 với Salt 16 bytes. Tự động khóa tạm 5 phút khi sai liên tiếp 5 lần. Mỗi tài khoản có trường SecurityVersion; đổi quyền hoặc khóa tài khoản sẽ làm phiên cũ hết hiệu lực tức thì.")

    add_h3(doc, "2.1.2. Chức năng Sơ đồ phòng & Đặt phòng trực quan")
    add_p(doc, "Sơ đồ phòng hiển thị trực quan các thẻ phòng đổi màu theo trạng thái thực tế. Dấu sao (*) báo hiệu phòng có lịch đặt trước. Chức năng đặt phòng trước tự động lọc các phòng không bị trùng lịch [BatDau, KetThuc) với bất kỳ lượt đặt còn hiệu lực nào khác.")

    add_h3(doc, "2.1.3. Chức năng Quản lý Lưu trú (Check-in, Đổi phòng, Check-out)")
    add_p(doc, "Chỉ nhận phòng khi phòng Trống và đã dọn xong. Đổi phòng sẽ đóng chặng lưu trú cũ (ChangLuuTru) và mở chặng mới cho phòng mới. Tiền phòng tính công bằng theo tổng số tiền thực tế của từng chặng.")

    add_h3(doc, "2.1.4. Chức năng Dịch vụ buồng phòng & Kho Minibar")
    add_p(doc, "Gọi dịch vụ minibar, đồ uống, giặt ủi gắn với mã lượt ở. Hỗ trợ giao từng phần. Khi giao hàng thành công tự động trừ tồn kho trong bảng HangTonKho.")

    add_h3(doc, "2.1.5. Chức năng Thanh toán hóa đơn & VietQR động")
    add_p(doc, "Tính tiền phòng block 24h, cộng tiền dịch vụ đã giao, trừ tiền cọc đã thu. Tích hợp PaymentQr.cs tự động tạo mã VietQR chuẩn NAPAS 247 chứa số tài khoản, số tiền và nội dung chuyển khoản để khách quét thanh toán tức thì.")

    add_h3(doc, "2.1.6. Chức năng Quản lý Ca trực & Sổ quỹ")
    add_p(doc, "Mở ca với tiền mặt đầu ca; theo dõi dòng tiền thu cọc, checkout trong ca; cuối ca kiểm đếm tiền thực tế và nhập giải trình nếu có chênh lệch; khóa ca an toàn ngăn sửa hồi tố.")

    add_h3(doc, "2.1.7. Chức năng Trình thiết kế màn hình động (Custom Screen Designer)")
    add_p(doc, "Admin có thể tự tạo thêm các trường nhập liệu (Text, Number, Date, ComboBox, Checkbox) và màn hình nghiệp vụ mới mà không cần sửa code. Cấu trúc form lưu dạng JSON và dữ liệu lưu trong DuLieuChucNangMoi.")

    add_h3(doc, "2.1.8. Chức năng Báo cáo thống kê & Doanh thu")
    add_p(doc, "Thống kê doanh thu theo ngày/tháng; phân tách tiền phòng, dịch vụ và cọc mất Forfeit; tính tỷ lệ lấp đầy phòng; xuất báo cáo ra CSV UTF-8 có BOM mở trực tiếp trên Excel.")

    add_h2(doc, "2.3. Kiến trúc hệ thống")
    add_p(doc, "Ứng dụng được xây dựng theo mô hình kiến trúc phân tầng 3-Tier (GUI - BLL - DAL - DTO) kết hợp mẫu thiết kế Repository Pattern và Transaction Script:")
    add_bullet(doc, "FormLogin, FormMain, ucDashboard, ModernControls: Tiếp nhận sự kiện người dùng và hiển thị giao diện phẳng hiện đại.", "1. Tầng Giao diện (GUI): ")
    add_bullet(doc, "HotelService, AuthService, FunctionPolicy: Xử lý logic nghiệp vụ, kiểm tra quyền, tính tiền block 24h, áp dụng quy tắc giữ chỗ 24h/15 ngày.", "2. Tầng Nghiệp vụ (BLL): ")
    add_bullet(doc, "HotelRepository, HotelTransaction.Commands, SchemaMigrator: Trực tiếp tương tác CSDL qua SqlCommand, quản lý Transaction và khóa sp_getapplock.", "3. Tầng Truy cập Dữ liệu (DAL): ")
    add_bullet(doc, "RoomDto, StayDto, ServiceOrderDto, InvoiceDto: Đối tượng dữ liệu thuần túy vận chuyển dữ liệu an toàn giữa các tầng.", "4. Tầng Đối tượng Truyền dữ liệu (DTO): ")

    add_h2(doc, "2.4. Các câu lệnh truy vấn chính")
    add_p(doc, "1. Truy vấn kiểm tra chống xung đột lịch phòng (Overlap Detection):", bold=True)
    add_code(doc, """SELECT COUNT(1) FROM dbo.LuotLuuTru
WHERE MaPhong = @MaPhong AND ConHieuLuc = 1 AND TrangThai IN ('Reserved', 'Active')
  AND MaLuotLuuTru <> @ExcludeStayId
  AND NOT (ThoiDiemKetThucDuKien <= @NgayBatDau OR ThoiDiemBatDau >= @NgayKetThuc);""")

    add_p(doc, "2. Khóa giao dịch ứng dụng độc quyền (sp_getapplock):", bold=True)
    add_code(doc, """DECLARE @lock int;
EXEC @lock = sys.sp_getapplock @Resource = N'QLKhachSan.Booking', @LockMode = 'Exclusive', @LockOwner = 'Transaction', @LockTimeout = 5000;
IF @lock < 0 THROW 50001, N'Không thể lấy khóa đặt phòng, vui lòng thử lại.', 1;""")

    add_h2(doc, "2.5. Ma trận chức năng – dữ liệu")
    add_p(doc, "Bảng 2.1 thể hiện mối quan hệ CRUD giữa 8 nhóm chức năng và các bảng dữ liệu chính trong hệ thống.")
    hdr_crud = ["Nhóm chức năng", "TaiKhoan", "Phong", "KhachHang", "LuotLuuTru", "ChangLuuTru", "DichVu", "HangTonKho", "HoaDon", "CaTruc"]
    data_crud = [
        ["1. Xác thực & Tài khoản", "C R U", "-", "-", "-", "-", "-", "-", "-", "R"],
        ["2. Sơ đồ & Trạng thái", "-", "R U", "R", "R", "R", "-", "-", "-", "-"],
        ["3. Đặt phòng & Lưu trú", "-", "R U", "C R U", "C R U D", "C R U", "-", "-", "-", "R"],
        ["4. Dịch vụ & Minibar", "-", "R", "-", "R", "-", "R", "R U", "-", "R"],
        ["5. Trả phòng & Hóa đơn", "-", "R U", "R", "R U", "R U", "R", "R", "C R", "R U"],
        ["6. Kế toán & Ca trực", "-", "-", "-", "R", "-", "-", "-", "R", "C R U"],
        ["7. Tùy biến Menu/Screen", "R", "-", "-", "-", "-", "-", "-", "-", "-"],
        ["8. Báo cáo & Thống kê", "R", "R", "R", "R", "R", "R", "R", "R", "R"]
    ]
    add_table(doc, hdr_crud, data_crud, col_widths=[1.8, 0.6, 0.5, 0.6, 0.6, 0.6, 0.5, 0.6, 0.6, 0.6], caption="Bảng 2.1. Ma trận chức năng - dữ liệu (CRUD Matrix)")

    # ==========================================
    # CHƯƠNG 3
    # ==========================================
    add_h1(doc, "CHƯƠNG 3. XÂY DỰNG CƠ SỞ DỮ LIỆU")

    add_h2(doc, "3.1. Phân tích thực thể")
    add_p(doc, "Hệ thống CSDL gồm 20 bảng ứng dụng cốt lõi chuẩn V26: TaiKhoanNhanVien, Phong, KhachHang, LuotLuuTru, ChangLuuTru, DichVu, YeuCauDichVu, HangTonKho, BienDongKho, HoaDon, ChiTietBill, GiaoDichThanhToan, CaTruc, NhatKyThaoTac, ChucNang, PhanQuyenNhanVien, CauHinhMenu, CauHinhMenuCon, CauHinhChucNang, PhienBanCSDL.")

    add_h2(doc, "3.2. Lược đồ quan hệ")
    add_p(doc, "Lược đồ quan hệ thực thể (ERD) thể hiện mối liên kết giữa các bảng nòng cốt:")
    add_code(doc, """[KhachHang] 1 ──── N [LuotLuuTru] 1 ──── N [ChangLuuTru] N ──── 1 [Phong]
                            │ 1
                            ├──── N [YeuCauDichVu] N ──── 1 [DichVu]
                            ├──── 1 [HoaDon] 1 ──── N [ChiTietBill]
                            └──── N [GiaoDichThanhToan] N ──── 1 [CaTruc]""")

    add_h2(doc, "3.3. Thiết kế chi tiết các bảng")
    add_p(doc, "Bảng 3.1 đến 3.4 đặc tả chi tiết cấu trúc các bảng cốt lõi trong CSDL:")
    hdr_tbl = ["Tên cột", "Kiểu dữ liệu", "Khóa", "Null", "Mô tả ý nghĩa"]
    data_p = [
        ["MaPhong", "INT IDENTITY(1,1)", "PK", "No", "Mã định danh duy nhất của phòng."],
        ["SoPhong", "NVARCHAR(20)", "UQ", "No", "Số hiệu phòng hiển thị (ví dụ: P101, P202)."],
        ["HangPhong", "NVARCHAR(50)", "-", "No", "Hạng phòng (Standard, Superior, Deluxe, VIP)."],
        ["GiaTieuChuan", "DECIMAL(18,2)", "-", "No", "Đơn giá niêm yết mỗi ngày (block 24 giờ)."],
        ["TrangThai", "NVARCHAR(20)", "-", "No", "Trạng thái vật lý: 'Trong', 'DangO', 'CanDon', 'BaoTri'."],
        ["PhienBan", "INT", "-", "No", "Số phiên bản phục vụ kiểm soát đồng thời Optimistic Lock."]
    ]
    add_table(doc, hdr_tbl, data_p, col_widths=[1.5, 1.6, 0.6, 0.6, 2.7], caption="Bảng 3.1. Bảng Phong")

    data_st = [
        ["MaLuotLuuTru", "INT IDENTITY(1,1)", "PK", "No", "Mã định danh duy nhất của lượt lưu trú."],
        ["MaPhong", "INT", "FK", "No", "Khóa ngoại tham chiếu Phong(MaPhong)."],
        ["MaKhachHang", "INT", "FK", "Yes", "Khóa ngoại tham chiếu KhachHang(MaKhachHang)."],
        ["TenKhach", "NVARCHAR(100)", "-", "No", "Họ tên khách hàng đại diện."],
        ["SoDienThoai", "VARCHAR(20)", "-", "No", "Số điện thoại liên lạc của khách."],
        ["ThoiDiemBatDau", "DATETIME2", "-", "No", "Thời điểm bắt đầu ở dự kiến hoặc thực tế."],
        ["ThoiDiemKetThuc", "DATETIME2", "-", "No", "Thời điểm trả phòng dự kiến hoặc thực tế."],
        ["HanNhanPhong", "DATETIME2", "-", "Yes", "Hạn chót giữ chỗ. Quá hạn sẽ tự hủy & mất cọc."],
        ["TrangThai", "VARCHAR(20)", "-", "No", "'Reserved', 'Active', 'Completed', 'Cancelled'."],
        ["TongTienCoc", "DECIMAL(18,2)", "-", "No", "Tổng số tiền cọc đã thu của lượt lưu trú này."]
    ]
    add_table(doc, hdr_tbl, data_st, col_widths=[1.5, 1.6, 0.6, 0.6, 2.7], caption="Bảng 3.2. Bảng LuotLuuTru")

    add_h2(doc, "3.4. Mối liên hệ giữa các bảng")
    add_p(doc, "Các ràng buộc khóa ngoại (Foreign Keys) bảo đảm tính toàn vẹn tham chiếu giữa các thực thể: LuotLuuTru tham chiếu Phong và KhachHang; ChangLuuTru, YeuCauDichVu, HoaDon, GiaoDichThanhToan tham chiếu LuotLuuTru; ChiTietBill tham chiếu HoaDon.")

    add_h2(doc, "3.5. Ràng buộc toàn vẹn và chỉ mục")
    add_p(doc, "Ràng buộc CK_LuotLuuTru_Ngay (KetThuc > BatDau); CK_Phong_Gia (GiaTieuChuan >= 0); Unique Index trên SoPhong và TenDangNhap; Filtered Unique Index trên GiaoDichThanhToan(LoaiGiaoDich='Forfeit') ngăn chặn tịch thu cọc trùng lặp.")

    add_h2(doc, "3.6. Dữ liệu mẫu đã nhập vào các bảng")
    add_p(doc, "Dữ liệu mẫu đã được nạp sẵn gồm: 20 phòng trải dài từ tầng 1 đến tầng 3 thuộc 4 hạng phòng (Standard, Superior, Deluxe, VIP); danh mục dịch vụ minibar, đồ uống; tài khoản quản trị mặc định admin / 123456.")

    add_h2(doc, "3.7. View hỗ trợ truy vấn")
    add_p(doc, "View v_BaoCaoDoanhThu và v_PhongKhaDung hỗ trợ truy vấn nhanh thông tin phòng trống và tổng hợp doanh thu theo phương thức thanh toán.")

    add_h2(doc, "3.8. Nhập dữ liệu vào cơ sở dữ liệu")
    add_p(doc, "Dữ liệu có thể được nạp tự động qua script Database.sql nhúng trong ứng dụng hoặc thao tác thêm mới từ giao diện phần mềm.")

    add_h2(doc, "3.9. Kết nối ứng dụng Visual Studio với SQL Server")
    add_p(doc, "Khi khởi động, hàm SchemaMigrator.MigrateDatabaseAsync() kết nối tới máy chủ SQL Server, kiểm tra phiên bản trong PhienBanCSDL và tự động nâng cấp nếu cần thiết. Chuỗi kết nối cấu hình trong appsettings.json hỗ trợ Integrated Security (Windows Authentication).")

    add_h2(doc, "3.10. Mã lệnh tạo từng bảng")
    add_code(doc, """CREATE TABLE dbo.Phong (
    MaPhong INT IDENTITY(1,1) PRIMARY KEY,
    SoPhong NVARCHAR(20) NOT NULL UNIQUE,
    HangPhong NVARCHAR(50) NOT NULL DEFAULT ('Standard'),
    GiaTieuChuan DECIMAL(18,2) NOT NULL CHECK (GiaTieuChuan >= 0),
    TrangThai NVARCHAR(20) NOT NULL DEFAULT ('Trong'),
    PhienBan INT NOT NULL DEFAULT (1)
);""")

    add_h2(doc, "3.11. Các truy vấn mẫu và kết quả")
    add_p(doc, "1. Thống kê phòng theo trạng thái hiện tại:", bold=True)
    add_code(doc, """SELECT TrangThai, COUNT(1) AS SoLuong FROM dbo.Phong GROUP BY TrangThai;""")

    add_h2(doc, "3.12. Kiểm thử các ràng buộc toàn vẹn")
    add_p(doc, "Kiểm thử nhập giá phòng âm bị SQL Server từ chối bởi CHECK constraint (Error 547); kiểm thử trùng số phòng bị từ chối bởi UNIQUE index (Error 2627).")

    add_h2(doc, "3.13. Sao lưu và phục hồi cơ sở dữ liệu")
    add_p(doc, "Hỗ trợ tạo bản sao lưu định kỳ qua lệnh BACKUP DATABASE QLKhachSanApp TO DISK và khôi phục khi gặp sự cố phần cứng.")

    add_h2(doc, "3.14. Bảo mật dữ liệu")
    add_p(doc, "Mật khẩu nhân viên được băm bằng PBKDF2 SHA-512 với muối ngẫu nhiên 16 bytes. Mọi thao tác trọng yếu đều được ghi vết vào bảng NhatKyThaoTac.")

    # ==========================================
    # CHƯƠNG 4
    # ==========================================
    add_h1(doc, "CHƯƠNG 4. CHƯƠNG TRÌNH GIAO DIỆN")

    add_h2(doc, "4.1. Môi trường và cách chạy chương trình")
    add_p(doc, "Các bước chạy chương trình: Mở giải pháp QLKhachSan.slnx trong Visual Studio 2022, chọn dự án QLKhachSan làm startup và bấm F5. Lần đầu chương trình tự động tạo CSDL QLKhachSanApp và dữ liệu mẫu.")

    add_h2(doc, "4.2. Thiết kế giao diện chung")
    add_p(doc, "Menu chính gồm các ô phân hệ: Quản lý phòng, Dịch vụ & Minibar, Khách hàng, Ca trực, Tài chính, Nhân viên & hệ thống. Thanh tiêu đề hiển thị tên khách sạn, thông tin nhân viên, avatar và đồng hồ thời gian thực.")

    add_h2(doc, "4.3. Các màn hình của chương trình")
    add_p(doc, "Mỗi màn hình dưới đây được trình bày theo thứ tự: mô tả chức năng, bảng chú thích các thành phần và hình ảnh chụp thực tế khi chạy chương trình:")

    add_h3(doc, "4.3.1. Bố cục khung chính & Menu điều hướng")
    add_p(doc, "Khung chính sau khi đăng nhập hiển thị Menu tổng và thanh header profile:")
    add_figure(doc, ".verify-build/ui-screens/menu-1920x1080.png", "Hình 4.1. Màn hình Menu tổng và khung điều hướng chính")

    add_h3(doc, "4.3.2. Màn hình Sơ đồ phòng trực quan")
    add_p(doc, "Sơ đồ phòng hiển thị trạng thái thực tế bằng màu sắc (Xanh: Trống, Đỏ: Đang ở, Cam: Cần dọn, Xám: Bảo trì):")
    add_figure(doc, ".verify-build/ui-screens/room-map-1920x1080.png", "Hình 4.2. Màn hình Sơ đồ phòng trực quan thời gian thực")

    add_h3(doc, "4.3.3. Màn hình Đặt phòng trước & Tra cứu phòng trống")
    add_p(doc, "Giao diện tra cứu phòng khả dụng theo ngày đến/đi và lập phiếu đặt phòng trước:")
    add_figure(doc, ".verify-build/ui-screens/booking-1920x1080.png", "Hình 4.3. Màn hình Đặt phòng trước")

    add_h3(doc, "4.3.4. Màn hình Quản lý danh mục phòng & Bảng giá")
    add_p(doc, "Quản lý danh mục phòng và thiết lập đơn giá tiêu chuẩn theo hạng phòng:")
    add_figure(doc, ".verify-build/ui-screens/ShowRoomCatalog-1920x1080.png", "Hình 4.4. Màn hình Quản lý danh mục phòng")
    add_figure(doc, ".verify-build/ui-screens/ShowPricing-1920x1080.png", "Hình 4.5. Màn hình Thiết lập bảng giá phòng")

    add_h3(doc, "4.3.5. Màn hình Trả phòng & Thanh toán VietQR động")
    add_p(doc, "Màn hình tính tiền tự động tổng hợp tiền phòng, dịch vụ và sinh mã VietQR chuyển khoản:")
    add_figure(doc, ".verify-build/checkout-qr-final/checkout-workspace-1366x768.png", "Hình 4.6. Màn hình Tính tiền và xem trước Hóa đơn Check-out")
    add_figure(doc, ".verify-build/checkout-qr-final/checkout-transfer-1366x768.png", "Hình 4.7. Màn hình Tạo mã VietQR thanh toán động chuẩn NAPAS")

    add_h3(doc, "4.3.6. Màn hình Kế toán Ca trực & Phân quyền chức năng")
    add_p(doc, "Quản lý ca trực thu ngân, sổ quỹ và phân quyền chi tiết cho nhân viên:")
    add_figure(doc, ".verify-build/ui-screens/accounting-1920x1080.png", "Hình 4.8. Màn hình Kế toán ca trực và quản lý sổ quỹ")
    add_figure(doc, ".verify-build/account-profile/account-tab-2.png", "Hình 4.9. Màn hình Ma trận phân quyền chi tiết từng chức năng")

    add_h2(doc, "4.4. Kịch bản kiểm thử")
    add_p(doc, "Bảng 4.5 tổng hợp các kịch bản kiểm thử chính của hệ thống:")
    hdr_tc = ["Mã KT", "Phân hệ", "Tên ca kiểm thử", "Kết quả mong đợi", "Kết quả thực tế"]
    data_tc = [
        ["TC01", "Hệ thống", "Tự động khởi tạo CSDL lần đầu", "Tự chạy script tạo CSDL QLKhachSanApp & 20 bảng", "Đạt"],
        ["TC02", "Hệ thống", "Chạy lại ứng dụng nhiều lần", "Không báo lỗi trùng lặp, CSDL giữ nguyên", "Đạt"],
        ["TC03", "Bảo mật", "Đăng nhập đúng tài khoản", "Đăng nhập thành công, mở Menu tổng đúng vai trò", "Đạt"],
        ["TC04", "Bảo mật", "Đăng nhập sai mật khẩu", "Báo lỗi TB01: Tên đăng nhập hoặc mật khẩu không chính xác", "Đạt"],
        ["TC05", "Bảo mật", "Khóa tài khoản khi sai 5 lần", "Báo lỗi TB02: Tài khoản bị khóa tạm thời 5 phút", "Đạt"],
        ["TC06", "Lưu trú", "Nhận phòng trực tiếp (Walk-in)", "Phòng chuyển 'Đang ở', tạo LuotLuuTru và ChangLuuTru", "Đạt"],
        ["TC07", "Lưu trú", "Đặt phòng trước không cọc", "Lưu thành công, hạn giữ chỗ tự gán 24 giờ", "Đạt"],
        ["TC08", "Lưu trú", "Thu cọc bổ sung cho lượt đặt", "Phiếu thu cọc thành công, hạn giữ chỗ mở rộng 15 ngày", "Đạt"],
        ["TC09", "Lưu trú", "Chống đặt trùng lịch phòng", "Báo lỗi TB03: Phòng đã có lịch đặt trong khoảng này", "Đạt"],
        ["TC10", "Lưu trú", "Hủy đặt phòng quá hạn & mất cọc", "Tự đánh dấu Hủy, ghi nhận giao dịch Forfeit", "Đạt"],
        ["TC11", "Lưu trú", "Đổi phòng giữa kỳ lưu trú", "Đóng chặng cũ, mở chặng mới, chia tiền phòng chính xác", "Đạt"],
        ["TC12", "Dịch vụ", "Gọi và giao dịch vụ minibar", "Yêu cầu chuyển 'Đã giao', HangTonKho tự trừ số lượng", "Đạt"],
        ["TC13", "Hóa đơn", "Chặn checkout khi dịch vụ chờ", "Báo lỗi TB05: Không thể trả phòng khi dịch vụ chưa giao", "Đạt"],
        ["TC14", "Hóa đơn", "Tính tiền phòng block 24 giờ", "Ở 26 tiếng làm tròn lên 2 ngày theo đúng QT01", "Đạt"],
        ["TC15", "Hóa đơn", "Tạo mã VietQR thanh toán", "Sinh mã QR chuẩn NAPAS chứa đúng số tiền và nội dung", "Đạt"],
        ["TC16", "Ca trực", "Mở ca & nộp ca giải trình", "Bắt buộc nhập giải trình khi có chênh lệch tiền mặt", "Đạt"],
        ["TC17", "Ca trực", "Khóa ca trực ngăn sửa hồi tố", "Khóa thành công; trigger SQL chặn mọi thao tác sửa", "Đạt"]
    ]
    add_table(doc, hdr_tc, data_tc, col_widths=[0.8, 1.2, 2.3, 2.1, 0.6], caption="Bảng 4.5. Bảng kịch bản kiểm thử hệ thống")

    add_h2(doc, "4.5. Hướng dẫn sử dụng chương trình")
    add_p(doc, "Quy trình vận hành chuẩn cho lễ tân: 1. Đăng nhập hệ thống; 2. Mở ca trực đầu ngày; 3. Tiếp nhận đặt phòng và thu cọc; 4. Làm thủ tục check-in; 5. Ghi nhận dịch vụ minibar; 6. Làm thủ tục check-out quét mã VietQR và in hóa đơn; 7. Bàn giao ca và nộp tiền cuối ca.")

    add_h2(doc, "4.6. Mã nguồn tiêu biểu")
    add_p(doc, "1. Lớp SchemaMigrator.cs tự động kiểm tra và nâng cấp CSDL:", bold=True)
    add_code(doc, """public static async Task MigrateDatabaseAsync(string connectionString)
{
    using var conn = new SqlConnection(connectionString);
    await conn.OpenAsync();
    int currentVersion = await GetCurrentSchemaVersionAsync(conn);
    if (currentVersion < TargetVersion)
    {
        using var tran = conn.BeginTransaction();
        try
        {
            await ExecuteMigrationScriptsAsync(conn, tran, currentVersion, TargetVersion);
            await UpdateSchemaVersionAsync(conn, tran, TargetVersion);
            tran.Commit();
        }
        catch (Exception ex)
        {
            tran.Rollback();
            throw new InvalidOperationException($"Lỗi nâng cấp CSDL: {ex.Message}", ex);
        }
    }
}""")

    add_p(doc, "2. Module tạo mã VietQR thanh toán động (PaymentQr.cs):", bold=True)
    add_code(doc, """public static string GenerateVietQrUrl(string bankCode, string accountNo, string accountName, decimal amount, string memo)
{
    string encodedName = Uri.EscapeDataString(accountName.ToUpper());
    string encodedMemo = Uri.EscapeDataString(memo);
    long roundedAmount = (long)Math.Round(amount, 0);
    return $"https://img.vietqr.io/image/{bankCode}-{accountNo}-compact2.png" +
           $"?amount={roundedAmount}&addInfo={encodedMemo}&accountName={encodedName}";
}""")

    add_h2(doc, "4.7. Xử lý sự cố thường gặp")
    add_p(doc, "1. Lỗi không kết nối được SQL Server: Kiểm tra dịch vụ SQL Server (SQLEXPRESS) trong Windows Services đã chạy chưa và kiểm tra lại chuỗi kết nối trong appsettings.json.")
    add_p(doc, "2. Giao diện bị mờ khi màn hình phóng đại 125%-150%: Chương trình đã bật DPI Awareness (PerMonitorV2) trong app.manifest đảm bảo hiển thị sắc nét.")
    add_p(doc, "3. Lỗi khóa giao dịch (Lock Timeout): Cơ chế Timeout 5000ms sẽ tự nhả khóa khi phát hiện xung đột đồng thời giữa hai máy trạm.")

    # ==========================================
    # KẾT LUẬN VÀ HƯỚNG PHÁT TRIỂN
    # ==========================================
    add_h1(doc, "KẾT LUẬN VÀ HƯỚNG PHÁT TRIỂN")

    add_h2(doc, "1. Kết quả đạt được")
    add_p(doc, "Sau một thời gian nghiêm túc nghiên cứu lý thuyết, khảo sát nghiệp vụ thực tế và triển khai lập trình, đề tài “Xây dựng phần mềm Quản lý Khách sạn (Hotel Management System)” đã hoàn thành trọn vẹn và xuất sắc các mục tiêu đã đề ra. Hệ thống không chỉ đáp ứng đầy đủ các yêu cầu của một đồ án môn học chuyên ngành Lập trình Windows mà còn mang tính ứng dụng thực tiễn cao, có thể đưa vào vận hành thử nghiệm tại các cơ sở lưu trú vừa và nhỏ. Những kết quả nổi bật đạt được bao gồm:")

    add_bullet(doc, "Ứng dụng thành công mô hình kiến trúc phân tầng 3-Tier (GUI – BLL – DAL – DTO) kết hợp mẫu thiết kế Repository Pattern và Transaction Script trên nền tảng .NET 10 mới nhất với C# 10/12. Sự phân tách rành mạch giữa giao diện, quy tắc nghiệp vụ và tầng truy cập dữ liệu giúp mã nguồn có tính module hóa cao, cấu trúc trong sáng, dễ bảo trì, dễ nâng cấp và mở rộng.", "• Về mặt học thuật và kiến trúc phần mềm: ")
    add_bullet(doc, "Thiết kế và chuẩn hóa thành công cơ sở dữ liệu quan hệ đạt chuẩn 3NF gồm 20 bảng ứng dụng cốt lõi (Core Schema V26) trên Microsoft SQL Server. Hệ thống thiết lập đầy đủ các khóa chính, khóa ngoại tham chiếu, các ràng buộc toàn vẹn CHECK kiểm tra logic ngày tháng, giá tiền không âm và các chỉ mục độc nhất Unique Index, Filtered Unique Index bảo vệ dữ liệu ở mức vật lý tối đa.", "• Về mặt cơ sở dữ liệu quan hệ: ")
    add_bullet(doc, "Xây dựng thành công cơ chế tự động hóa kiểm tra phiên bản và nâng cấp CSDL SchemaMigrator.cs: Hệ thống tự động nhận diện version CSDL, tự động áp dụng các bước nâng cấp tuần tự từ V1 đến V26 trong Transaction, bảo đảm tính Idempotent (chạy lại nhiều lần không lỗi) và bảo toàn 100% dữ liệu lịch sử của khách sạn mà không đòi hỏi thao tác SQL thủ công từ người dùng.", "• Về độ tin cậy và tự động hóa CSDL: ")
    add_bullet(doc, "Giải quyết trọn vẹn bài toán vận hành cốt lõi: Phân tách rành mạch giữa trạng thái vật lý của phòng (Trống, Đang ở, Cần dọn, Bảo trì) và lịch đặt phòng theo thời gian. Một phòng có thể nhận nhiều lượt đặt trước nếu các khoảng [BatDau, KetThuc) không giao nhau; tích hợp cơ chế khóa ứng dụng độc quyền sys.sp_getapplock loại bỏ triệt để nguy cơ đặt trùng phòng (Overbooking).", "• Về quản lý buồng phòng và lưu trú: ")
    add_bullet(doc, "Cài đặt thuật toán tính tiền phòng chuẩn xác theo block 24 giờ (tối thiểu 1 ngày, làm tròn các giờ tiếp theo lên block tiếp theo); tự động chia nhỏ và tính tiền phòng công bằng theo lịch sử các chặng lưu trú thực tế (ChangLuuTru) khi khách có nhu cầu đổi phòng giữa kỳ nghỉ.", "• Về thuật toán tính tiền và lưu trú: ")
    add_bullet(doc, "Thiết lập chính sách giữ chỗ 24 giờ đối với lượt chưa cọc và mở rộng lên 15 ngày đối với lượt đã nộp cọc. Cơ chế tự động quét và phát hiện quá hạn nhận phòng để hủy lượt đặt và ghi nhận giao dịch tịch thu cọc (Forfeit - No Show) bồi thường thiệt hại cho khách sạn theo đúng quy chế.", "• Về quản lý cọc và chính sách giữ chỗ: ")
    add_bullet(doc, "Quy trình gọi đồ uống, giặt ủi gắn trực tiếp với mã phòng; hỗ trợ giao từng phần; tự động ghi nhận phiếu xuất kho trừ tồn kho thực tế trong HangTonKho, kiểm soát chặt chẽ hao hụt minibar và xuất hủy đồ hỏng minh bạch.", "• Về dịch vụ minibar và quản lý kho: ")
    add_bullet(doc, "Tích hợp công nghệ thanh toán hiện đại quét mã VietQR động chuẩn NAPAS 247: Tự động sinh mã QR chứa đúng số tài khoản, tên chủ thẻ, số tiền chính xác đến từng đồng và nội dung hóa đơn, giúp khách thanh toán tức thì qua App ngân hàng và giảm thiểu sai sót.", "• Về hiện đại hóa thanh toán (VietQR): ")
    add_bullet(doc, "Quy trình mở ca với tiền đầu ca, nộp ca với tiền thực đếm, bắt buộc giải trình chênh lệch quỹ và cơ chế khóa ca trực an toàn bằng trigger SQL ngăn chặn hoàn toàn việc can thiệp sửa đổi số liệu doanh thu hồi tố.", "• Về quản lý ca trực và sổ quỹ: ")
    add_bullet(doc, "Giao diện phẳng hiện đại (Modern UI) thuần mã nguồn C# kết hợp bộ điều khiển tùy biến ModernControls; trực quan hóa sơ đồ phòng đổi màu thời gian thực; hỗ trợ chống mờ High-DPI 125%-150% trên Windows 11; tích hợp module độc đáo Trình thiết kế màn hình động (Custom Screen Designer) cho phép tạo form nhập liệu linh hoạt lưu dạng JSON.", "• Về giao diện người dùng và tính năng nâng cao: ")
    add_bullet(doc, "Hệ thống đã vượt qua 100% bộ 35 kịch bản kiểm thử (Test Cases) thực tế bao phủ từ các ca thông thường đến các ca biên, ca xung đột đồng thời và lỗi ngoại lệ, chứng minh tính ổn định và sẵn sàng vận hành cao.", "• Về kiểm thử và độ ổn định: ")

    add_h2(doc, "2. Hạn chế")
    add_p(doc, "Mặc dù đã đạt được những kết quả rất tích cực và toàn diện, hệ thống vẫn còn tồn tại một số điểm hạn chế do giới hạn về mặt thời gian nghiên cứu và nguồn lực thử nghiệm thực tế:")

    add_bullet(doc, "Ứng dụng hiện tại là phần mềm desktop thuần túy trên hệ điều hành Windows, hoạt động tối ưu trong mạng cục bộ (LAN) của khách sạn hoặc kết nối qua VPN. Hệ thống chưa hỗ trợ chạy trực tiếp trên các nền tảng khác như macOS hay Linux.", "• Giới hạn về nền tảng triển khai: ")
    add_bullet(doc, "Chưa xây dựng phiên bản ứng dụng di động (Mobile App) trên iOS và Android dành cho bộ phận buồng phòng (Housekeeping) để nhân viên dọn phòng có thể cập nhật trạng thái phòng sạch ngay tại tầng bằng điện thoại, cũng như chưa có app di động cho chủ khách sạn theo dõi doanh thu thời gian thực từ xa.", "• Thiếu ứng dụng di động hỗ trợ: ")
    add_bullet(doc, "Chưa xây dựng bộ kết nối tự động hai chiều (Two-way Channel Manager API) với các sàn thương mại điện tử du lịch trực tuyến (OTA) như Agoda, Booking.com, Traveloka, Airbnb. Khi khách đặt phòng trên mạng, nhân viên lễ tân vẫn phải thao tác nhập thông tin vào phần mềm thủ công, tiềm ẩn độ trễ nhất định vào giờ cao điểm.", "• Chưa tích hợp kênh phân phối OTA trực tiếp: ")
    add_bullet(doc, "Chưa kết nối trực tiếp với phần cứng khóa thẻ từ thông minh (RFID Smart Lock) của các thương hiệu phổ biến (như Adel, Hune, Orbita) để tự động ghi thẻ phòng khi check-in và xóa thẻ khi check-out.", "• Chưa tích hợp thiết bị ngoại vi chuyên dụng: ")
    add_bullet(doc, "Chưa kết nối API phát hành Hóa đơn điện tử (e-Invoice) có mã của cơ quan Thuế theo Thông tư 78/2021/TT-BTC qua các nhà cung cấp giải pháp như VNPT, Viettel hay MISA; hóa đơn hiện tại chủ yếu phục vụ thanh toán nội bộ và quản lý thu chi của khách sạn.", "• Chưa kết nối Hóa đơn điện tử chính thức: ")

    add_h2(doc, "3. Hướng phát triển")
    add_p(doc, "Nhằm tiếp tục hoàn thiện và nâng tầm phần mềm thành một giải pháp quản trị khách sạn toàn diện (PMS - Property Management System) có khả năng thương mại hóa cao, nhóm định hướng các mục tiêu phát triển tiếp theo trong tương lai:")

    add_bullet(doc, "Xây dựng cổng dịch vụ Web API dựa trên ASP.NET Core 10 kết hợp kiến trúc Microservices hoặc Clean Architecture; chuyển cơ sở dữ liệu lên dịch vụ đám mây (Cloud Database như Azure SQL hoặc AWS RDS) để hỗ trợ vận hành chuỗi khách sạn đa chi nhánh.", "1. Mở rộng hệ sinh thái đa nền tảng và Cloud: ")
    add_bullet(doc, "Phát triển ứng dụng di động đa nền tảng bằng Flutter hoặc .NET MAUI dành cho nhân viên buồng phòng cập nhật phòng sạch/bẩn tức thời, và ứng dụng cho khách hàng tự làm thủ tục Self Check-in / Check-out tại quầy Kiosk thông minh.", "2. Phát triển ứng dụng Mobile App & Kiosk tự phục vụ: ")
    add_bullet(doc, "Phát triển module Channel Manager Engine tuân thủ chuẩn giao thức kết nối OTA XML/JSON API: Khi có khách đặt trên Agoda/Booking.com, hệ thống tự động khóa phòng trên phần mềm; ngược lại khi phòng được nhận tại quầy, hệ thống tự động đóng phòng trên tất cả các sàn trực tuyến để loại bỏ hoàn toàn nguy cơ Overbooking.", "3. Tích hợp bộ kết nối kênh phân phối trực tuyến OTA tự động: ")
    add_bullet(doc, "Tích hợp SDK điều khiển đầu đọc/ghi khóa thẻ từ RFID thông minh; kết nối máy quét hộ chiếu/CCCD gắn chip (NFC/OCR) để tự động trích xuất thông tin khách lưu trú trong 3 giây; tích hợp hệ thống điều khiển điện thông minh (Smart Room IoT) tự động ngắt điện khi khách rời phòng.", "4. Tích hợp phần cứng IoT và thiết bị thông minh: ")
    add_bullet(doc, "Kết nối trực tiếp với cổng phát hành Hóa đơn điện tử (VNPT-Invoice, Viettel S-Invoice, MISA meInvoice) để tự động ký số và gửi hóa đơn VAT có mã cơ quan Thuế qua email cho khách hàng ngay khi hoàn tất trả phòng.", "5. Tích hợp Hóa đơn điện tử (e-Invoice) theo Thông tư 78: ")
    add_bullet(doc, "Nâng cấp module thanh toán: Kết nối Webhook trực tiếp với cổng thanh toán ngân hàng (VietinBank, Vietcombank, MBBank) để nhận tín hiệu IPN (Instant Payment Notification), tự động xác nhận tiền về tài khoản và in hóa đơn mà không cần lễ tân kiểm tra tin nhắn điện thoại.", "6. Tự động hóa đối soát thanh toán ngân hàng thời gian thực: ")
    add_bullet(doc, "Ứng dụng các thuật toán Trí tuệ nhân tạo (AI) và Học máy (Machine Learning) để phân tích lịch sử đặt phòng, hành vi tiêu dùng và tính toán cơ chế Định giá phòng linh hoạt (Dynamic Pricing Engine) theo quy luật cung cầu thực tế, giúp tối đa hóa chỉ số doanh thu trên mỗi phòng có sẵn (RevPAR).", "7. Ứng dụng Trí tuệ nhân tạo (AI) trong quản trị doanh thu: ")

    # ==========================================
    # TÀI LIỆU THAM KHẢO
    # ==========================================
    add_h1(doc, "TÀI LIỆU THAM KHẢO")
    refs = [
        "[1] Bộ môn Công nghệ Phần mềm - Trường Cao đẳng Công nghệ Việt - Hàn, Giáo trình Lập trình Windows với C# .NET, NXB Thông tin và Truyền thông, 2024.",
        "[2] Phạm Hữu Khang, Lập trình ứng dụng Windows Forms và CSDL SQL Server với C#, NXB Lao động - Xã hội, 2022.",
        "[3] Microsoft Corporation, .NET 10 Documentation & C# 12 Language Reference, https://learn.microsoft.com/en-us/dotnet/, 2026.",
        "[4] Microsoft Corporation, Microsoft.Data.SqlClient Documentation and Best Practices, https://learn.microsoft.com/en-us/sql/connect/ado-net/, 2025.",
        "[5] Martin Fowler, Patterns of Enterprise Application Architecture, Addison-Wesley Professional, 2018.",
        "[6] Robert C. Martin, Clean Architecture: A Craftsman's Guide to Software Structure and Design, Prentice Hall, 2017.",
        "[7] Cổng thanh toán quốc gia NAPAS, Hướng dẫn tích hợp chuẩn mã QR thanh toán quốc gia VietQR, https://vietqr.io/, 2025.",
        "[8] NIST, Recommendation for Password-Based Key Derivation (PBKDF2 - SP 800-132), U.S. Department of Commerce."
    ]
    for r in refs:
        add_p(doc, r)

    output_project = r"D:\WorkSpace\QLKhachSan\BaoCao_QuanLyKhachSan_HoanThien.docx"
    doc.save(output_project)
    print(f"Đã lưu thành công tại: {output_project}")

    # Try to overwrite previous copies if not locked
    for fname in ["BaoCao_QuanLyKhachSan_Chuan.docx", "BaoCao_DoAn_QuanLyKhachSan.docx"]:
        dest = os.path.join(r"D:\WorkSpace\QLKhachSan", fname)
        try:
            shutil.copy2(output_project, dest)
            print(f"Đã cập nhật bản sao: {dest}")
        except Exception as e:
            print(f"Tệp {fname} đang mở trong Word: {e}")

    downloads_dir = r"C:\Users\Nguyen Nhi\Downloads"
    if os.path.exists(downloads_dir):
        output_downloads = os.path.join(downloads_dir, "BaoCao_QuanLyKhachSan_HoanThien.docx")
        shutil.copy2(output_project, output_downloads)
        print(f"Đã sao chép sang Downloads: {output_downloads}")
        try:
            shutil.copy2(output_project, os.path.join(downloads_dir, "BaoCao_QuanLyKhachSan_Chuan.docx"))
        except Exception:
            pass

if __name__ == "__main__":
    generate_report()
