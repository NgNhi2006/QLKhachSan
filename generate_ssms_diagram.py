import os
import matplotlib
matplotlib.use('Agg')
import matplotlib.pyplot as plt
import matplotlib.patches as patches

def draw_ssms_diagram():
    out_dir = os.path.join(os.path.dirname(os.path.abspath(__file__)), 'tmp-screenshots')
    os.makedirs(out_dir, exist_ok=True)
    out_path = os.path.join(out_dir, 'ssms_diagram.png')

    fig, ax = plt.subplots(figsize=(19, 13), dpi=220)
    ax.set_xlim(0, 100)
    ax.set_ylim(0, 100)
    ax.axis('off')

    fig.patch.set_facecolor('#FFFFFF')
    ax.set_facecolor('#FFFFFF')

    # Grid dots
    for gx in range(2, 99, 4):
        for gy in range(2, 99, 4):
            ax.plot(gx, gy, marker='.', markersize=1, color='#D1D5DB', alpha=0.5)

    def draw_ssms_table(x, y, w, title, columns):
        row_h = 1.35
        header_h = 2.0
        col_hdr_h = 1.4
        total_h = header_h + col_hdr_h + len(columns) * row_h

        shadow = patches.Rectangle((x + 0.3, y - 0.3), w, total_h, facecolor='#E2E8F0', edgecolor='none')
        ax.add_patch(shadow)

        body = patches.Rectangle((x, y), w, total_h, facecolor='#FFFFFF', edgecolor='#718096', linewidth=1.2)
        ax.add_patch(body)

        title_box = patches.Rectangle((x, y + total_h - header_h), w, header_h,
                                      facecolor='#2B579A', edgecolor='#1E395B', linewidth=1)
        ax.add_patch(title_box)

        ax.text(x + 0.8, y + total_h - header_h / 2, f"dbo.{title}",
                fontsize=9.5, fontweight='bold', color='#FFFFFF', va='center')

        col_bar = patches.Rectangle((x, y + total_h - header_h - col_hdr_h), w, col_hdr_h,
                                    facecolor='#F1F5F9', edgecolor='#CBD5E1', linewidth=0.8)
        ax.add_patch(col_bar)
        ax.text(x + 2.2, y + total_h - header_h - col_hdr_h / 2, "Column Name",
                fontsize=8, fontweight='bold', color='#475569', va='center')
        ax.text(x + w * 0.62, y + total_h - header_h - col_hdr_h / 2, "Data Type",
                fontsize=8, fontweight='bold', color='#475569', va='center')

        ax.plot([x + w * 0.58, x + w * 0.58], [y, y + total_h - header_h], color='#E2E8F0', linewidth=0.8)

        curr_y = y + total_h - header_h - col_hdr_h - row_h / 2
        for col_name, data_type, is_pk, is_fk in columns:
            ax.plot([x, x + w], [curr_y - row_h / 2, curr_y - row_h / 2], color='#F1F5F9', linewidth=0.6)
            if is_pk:
                ax.text(x + 0.4, curr_y, "PK", fontsize=7, fontweight='bold', color='#B45309', va='center')
            elif is_fk:
                ax.text(x + 0.4, curr_y, "FK", fontsize=6.5, fontweight='bold', color='#2563EB', va='center')

            weight = 'bold' if is_pk else 'normal'
            font_color = '#0F172A' if is_pk else '#334155'
            ax.text(x + 2.2, curr_y, col_name, fontsize=8, fontweight=weight, color=font_color, va='center')
            ax.text(x + w * 0.62, curr_y, data_type, fontsize=7.5, color='#64748B', va='center')
            curr_y -= row_h

        return x, y, w, total_h

    # 1. KhachHang (Top Left)
    draw_ssms_table(4, 69, 21, "KhachHang", [
        ("MaKhachHang", "int", True, False),
        ("TenKhachHang", "nvarchar(100)", False, False),
        ("SoDienThoai", "nvarchar(20)", False, False),
        ("SoGiayTo", "nvarchar(20)", False, False),
        ("DiaChi", "nvarchar(200)", False, False),
        ("ThoiDiemTao", "datetime2", False, False)
    ])

    # 2. Phong (Top Center)
    draw_ssms_table(39, 67, 22, "Phong", [
        ("MaPhong", "int", True, False),
        ("SoPhong", "nvarchar(10)", False, False),
        ("LoaiPhong", "nvarchar(20)", False, False),
        ("DonGiaPhong", "decimal(18,2)", False, False),
        ("TienCoc", "decimal(18,2)", False, False),
        ("TrangThai", "varchar(20)", False, False),
        ("Tang", "int", False, False)
    ])

    # 3. TaiKhoanNhanVien (Top Right)
    draw_ssms_table(73, 66, 23, "TaiKhoanNhanVien", [
        ("MaNhanVien", "int", True, False),
        ("TenDangNhap", "nvarchar(50)", False, False),
        ("MatKhauBam", "varbinary(32)", False, False),
        ("MuoiBam", "varbinary(16)", False, False),
        ("HoTen", "nvarchar(100)", False, False),
        ("SoDienThoai", "nvarchar(20)", False, False),
        ("VaiTro", "varchar(20)", False, False),
        ("DangHoatDong", "bit", False, False)
    ])

    # 4. LuotLuuTru (Center)
    draw_ssms_table(37, 24, 25, "LuotLuuTru", [
        ("MaLuotLuuTru", "bigint", True, False),
        ("MaPhong", "int", False, True),
        ("MaKhachHang", "int", False, True),
        ("TenKhach", "nvarchar(100)", False, False),
        ("SoDienThoai", "nvarchar(20)", False, False),
        ("SoGiayTo", "nvarchar(20)", False, False),
        ("TrangThai", "varchar(20)", False, False),
        ("ThoiDiemDen", "datetime2", False, False),
        ("ThoiDiemDi", "datetime2", False, False),
        ("ThoiDiemNhanPhong", "datetime2", False, False),
        ("ThoiDiemTraPhong", "datetime2", False, False),
        ("TienCoc", "decimal(18,2)", False, False),
        ("MaNhanVien", "int", False, True)
    ])

    # 5. HoaDon (Bottom Left)
    draw_ssms_table(4, 15, 23, "HoaDon", [
        ("MaHoaDon", "bigint", True, False),
        ("MaLuotLuuTru", "bigint", False, True),
        ("SoPhongHoaDon", "nvarchar(10)", False, False),
        ("TenKhach", "nvarchar(100)", False, False),
        ("ThoiDiemLapHoaDon", "datetime2", False, False),
        ("TienPhong", "decimal(18,2)", False, False),
        ("TienCoc", "decimal(18,2)", False, False),
        ("TienDaThu", "decimal(18,2)", False, False),
        ("PhuongThucThanhToan", "nvarchar(30)", False, False),
        ("MaNhanVien", "int", False, True)
    ])

    # 6. GiaoDichThanhToan (Bottom Center)
    draw_ssms_table(73, 10, 23, "GiaoDichThanhToan", [
        ("MaGiaoDich", "bigint", True, False),
        ("MaLuotLuuTru", "bigint", False, True),
        ("LoaiGiaoDich", "varchar(20)", False, False),
        ("SoTien", "decimal(18,2)", False, False),
        ("ThoiDiemTao", "datetime2", False, False),
        ("PhuongThucThanhToan", "nvarchar(30)", False, False),
        ("MaNhanVien", "int", False, True),
        ("MaCaTruc", "bigint", False, True)
    ])

    # 7. CaTruc (Bottom Right)
    draw_ssms_table(73, 33, 23, "CaTruc", [
        ("MaCaTruc", "bigint", True, False),
        ("MaNhanVien", "int", False, True),
        ("ThoiDiemMoCa", "datetime2", False, False),
        ("ThoiDiemDongCa", "datetime2", False, False),
        ("TienMatDauCa", "decimal(18,2)", False, False),
        ("TienMatKiemDem", "decimal(18,2)", False, False),
        ("TienMatDuKien", "decimal(18,2)", False, False),
        ("GiaiTrinh", "nvarchar(500)", False, False),
        ("TrangThai", "varchar(12)", False, False)
    ])

    def draw_ssms_rel(x1, y1, x2, y2, l1="1", l2="∞"):
        mid_x = (x1 + x2) / 2
        ax.plot([x1, mid_x, mid_x, x2], [y1, y1, y2, y2], color='#475569', linewidth=1.4)
        ax.text(x1, y1, f" {l1} ", fontsize=8, fontweight='bold', color='#B45309', ha='center', va='center',
                bbox=dict(boxstyle='square,pad=0.1', facecolor='#FEF3C7', edgecolor='none'))
        ax.text(x2, y2, f" {l2} ", fontsize=9, fontweight='bold', color='#1E293B', ha='center', va='center',
                bbox=dict(boxstyle='square,pad=0.1', facecolor='#E2E8F0', edgecolor='none'))

    # KhachHang (1) -> LuotLuuTru (N)
    draw_ssms_rel(25, 75, 37, 45, "1", "∞")

    # Phong (1) -> LuotLuuTru (N)
    draw_ssms_rel(50, 67, 50, 48, "1", "∞")

    # TaiKhoanNhanVien (1) -> LuotLuuTru (N)
    draw_ssms_rel(73, 73, 62, 38, "1", "∞")

    # TaiKhoanNhanVien (1) -> CaTruc (N)
    draw_ssms_rel(84, 66, 84, 50, "1", "∞")

    # LuotLuuTru (1) -> HoaDon (1)
    draw_ssms_rel(37, 30, 27, 30, "1", "1")

    # LuotLuuTru (1) -> GiaoDichThanhToan (N)
    draw_ssms_rel(62, 26, 73, 20, "1", "∞")

    # CaTruc (1) -> GiaoDichThanhToan (N)
    draw_ssms_rel(84, 33, 84, 25, "1", "∞")

    # TaiKhoanNhanVien (1) -> HoaDon (N)
    draw_ssms_rel(73, 70, 27, 24, "1", "∞")

    plt.tight_layout()
    plt.savefig(out_path, dpi=220, bbox_inches='tight', facecolor='#FFFFFF')
    plt.close()
    print(f"Generated authentic SSMS diagram image (no services): {out_path}")

if __name__ == '__main__':
    draw_ssms_diagram()
