import os
import sys
import shutil
import docx

sys.stdout.reconfigure(encoding='utf-8')

from generate_qlks_helpers import setup_document_styles
from report_sections_part1 import build_cover_page, build_teacher_review, build_intro_and_thanks, build_toc_and_team
from report_sections_part2 import build_chapter1, build_chapter2
from report_sections_part3 import build_chapter3
from report_sections_part4 import build_chapter4, build_conclusion_and_references

def main():
    print("=" * 70)
    print("BẮT ĐẦU TẠO BÁO CÁO ĐỒ ÁN QLKHACHSAN (FILE FINAL HOÀN THIỆN ĐEN TRẮNG)")
    print("=" * 70)

    doc = docx.Document()
    setup_document_styles(doc)

    print("1. Đang xây dựng Trang bìa chính đen trắng trang nhã...")
    build_cover_page(doc)

    print("2. Đang xây dựng Trang Nhận xét của Giáo viên hướng dẫn...")
    build_teacher_review(doc)

    print("3. Đang xây dựng Lời mở đầu và Lời cảm ơn...")
    build_intro_and_thanks(doc)

    print("4. Đang xây dựng Mục lục chuẩn Tab Stop căn phải và Bảng phân công...")
    build_toc_and_team(doc)

    print("5. Đang xây dựng CHƯƠNG 1: TỔNG QUAN ĐỀ TÀI...")
    build_chapter1(doc)

    print("6. Đang xây dựng CHƯƠNG 2: KHẢO SÁT VÀ MÔ TẢ NGHIỆP VỤ QUẢN LÝ KHÁCH SẠN...")
    build_chapter2(doc)

    print("7. Đang xây dựng CHƯƠNG 3: XÂY DỰNG CƠ SỞ DỮ LIỆU & SƠ ĐỒ ERD ẢNH...")
    build_chapter3(doc)

    print("8. Đang xây dựng CHƯƠNG 4: THIẾT KẾ GIAO DIỆN KÈM HÌNH ẢNH TOÀN BỘ CÁC FORM...")
    build_chapter4(doc)

    print("9. Đang xây dựng KẾT LUẬN VÀ HƯỚNG PHÁT TRIỂN & TÀI LIỆU THAM KHẢO...")
    build_conclusion_and_references(doc)

    primary_out = "qlks.docx"
    print(f"10. Đang lưu tài liệu ra file: {primary_out}...")
    doc.save(primary_out)

    file_size = os.path.getsize(primary_out)
    print(f"Đã tạo thành công {primary_out} (Kích thước: {file_size:,} bytes).")

    # Danh sách các file cần đồng bộ đè lên
    targets = [
        r"C:\Users\Nguyen Nhi\Downloads\qlks_chuan.docx",
        "qlks.docx",
        "qlks_final.docx",
        "qlks_v2.docx",
        "qlks_new.docx"
    ]
    for target in targets:
        try:
            shutil.copyfile(primary_out, target)
            print(f"-> Đã đồng bộ thành công vào: {target}")
        except Exception as e:
            print(f"-> {target} đang bị khóa hoặc chưa ghi được: {e}")

    print("=" * 70)

if __name__ == "__main__":
    main()
