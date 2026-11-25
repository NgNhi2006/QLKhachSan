import docx
import shutil
import os
import sys

sys.stdout.reconfigure(encoding='utf-8')

from report_builder.styles import setup_document_styles
from report_builder.cover_and_intro import build_cover_page, build_table_of_contents, build_team_assignments, build_intro
from report_builder.chapter1 import build_chapter1
from report_builder.chapter2 import build_chapter2
from report_builder.chapter3 import build_chapter3
from report_builder.chapter4 import build_chapter4
from report_builder.conclusion import build_conclusion

def main():
    print("=" * 60)
    print("BẮT ĐẦU TẠO BÁO CÁO ĐỒ ÁN QUẢN LÝ KHÁCH SẠN (.DOCX)")
    print("=" * 60)

    doc = docx.Document()
    setup_document_styles(doc)

    print("1. Đang tạo Trang bìa...")
    build_cover_page(doc)

    print("2. Đang tạo Mục lục...")
    build_table_of_contents(doc)

    print("3. Đang tạo Bảng phân công nhiệm vụ nhóm...")
    build_team_assignments(doc)

    print("4. Đang tạo Lời mở đầu...")
    build_intro(doc)

    print("5. Đang tạo Chương 1: Mô tả chức năng và mô tả nghiệp vụ...")
    build_chapter1(doc)

    print("6. Đang tạo Chương 2: Phân tích chức năng...")
    build_chapter2(doc)

    print("7. Đang tạo Chương 3: Xây dựng cơ sở dữ liệu...")
    build_chapter3(doc)

    print("8. Đang tạo Chương 4: Chương trình giao diện...")
    build_chapter4(doc)

    print("9. Đang tạo Kết luận và Hướng phát triển, Tài liệu tham khảo...")
    build_conclusion(doc)

    output_project = r"D:\WorkSpace\QLKhachSan\BaoCao_DoAn_QuanLyKhachSan.docx"
    doc.save(output_project)
    print(f"-> Đã lưu báo cáo tại thư mục dự án: {output_project}")

    # Copy to user's Downloads folder for convenience
    downloads_dir = r"C:\Users\Nguyen Nhi\Downloads"
    if os.path.exists(downloads_dir):
        output_downloads = os.path.join(downloads_dir, "BaoCao_DoAn_QuanLyKhachSan.docx")
        try:
            shutil.copy2(output_project, output_downloads)
            print(f"-> Đã sao chép một bản sang thư mục Downloads: {output_downloads}")
        except Exception as e:
            print(f"Không thể sao chép sang Downloads: {e}")

    print("=" * 60)
    print("HOÀN TẤT TẠO BÁO CÁO THÀNH CÔNG!")
    print("=" * 60)

if __name__ == "__main__":
    main()
