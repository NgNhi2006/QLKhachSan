import os
import matplotlib
matplotlib.use('Agg')
import matplotlib.pyplot as plt
import matplotlib.patches as patches

def draw_erd():
    out_dir = os.path.join(os.path.dirname(os.path.abspath(__file__)), 'tmp-screenshots')
    os.makedirs(out_dir, exist_ok=True)
    out_path = os.path.join(out_dir, 'erd_diagram.png')

    fig, ax = plt.subplots(figsize=(18, 12), dpi=220)
    ax.set_xlim(0, 100)
    ax.set_ylim(0, 100)
    ax.axis('off')

    # Title
    ax.text(50, 97.5, "SƠ ĐỒ QUAN HỆ THỰC THỂ (ERD) - HỆ THỐNG QUẢN LÝ KHÁCH SẠN",
            ha='center', va='center', fontsize=15, fontweight='bold', color='#1E3A8A')

    c_blue_hdr = '#1E40AF'
    c_blue_bg = '#F8FAFC'
    c_green_hdr = '#047857'
    c_green_bg = '#F0FDF4'
    c_slate_hdr = '#334155'
    c_slate_bg = '#F1F5F9'

    def draw_entity(x, y, w, h, title, fields, hdr_color, bg_color):
        rect = patches.FancyBboxPatch((x, y), w, h, boxstyle="round,pad=0.2",
                                      facecolor=bg_color, edgecolor='#94A3B8', linewidth=1.4)
        ax.add_patch(rect)
        hdr_h = 2.4
        hdr = patches.FancyBboxPatch((x, y + h - hdr_h), w, hdr_h, boxstyle="round,pad=0.1",
                                     facecolor=hdr_color, edgecolor=hdr_color)
        ax.add_patch(hdr)
        ax.text(x + w / 2, y + h - hdr_h / 2, title, ha='center', va='center',
                fontsize=9.5, fontweight='bold', color='white')

        line_y = y + h - hdr_h - 1.2
        for f in fields:
            is_pk = "[PK]" in f
            is_fk = "[FK]" in f
            weight = 'bold' if (is_pk or is_fk) else 'normal'
            color = '#B91C1C' if is_pk else ('#1D4ED8' if is_fk else '#1E293B')
            ax.text(x + 0.6, line_y, f, fontsize=8, fontweight=weight, color=color, va='center')
            line_y -= 1.35

    # 1. KhachHang (Top Left)
    draw_entity(4, 69, 21, 18, "KhachHang", [
        "[PK] MaKhachHang: INT",
        "TenKhachHang: NVARCHAR(100)",
        "SoDienThoai: NVARCHAR(20)",
        "SoGiayTo: NVARCHAR(20)",
        "DiaChi: NVARCHAR(200)",
        "ThoiDiemTao: DATETIME2"
    ], c_blue_hdr, c_blue_bg)

    # 2. Phong (Top Center)
    draw_entity(39, 67, 22, 20, "Phong", [
        "[PK] MaPhong: INT",
        "SoPhong: NVARCHAR(10)",
        "LoaiPhong: NVARCHAR(20)",
        "DonGiaPhong: DECIMAL(18,2)",
        "TienCoc: DECIMAL(18,2)",
        "TrangThai: VARCHAR(20)",
        "Tang: INT"
    ], c_green_hdr, c_green_bg)

    # 3. TaiKhoanNhanVien (Top Right)
    draw_entity(73, 66, 23, 21, "TaiKhoanNhanVien", [
        "[PK] MaNhanVien: INT",
        "TenDangNhap: NVARCHAR(50)",
        "MatKhauBam: VARBINARY(32)",
        "MuoiBam: VARBINARY(16)",
        "HoTen: NVARCHAR(100)",
        "SoDienThoai: NVARCHAR(20)",
        "VaiTro: VARCHAR(20)",
        "DangHoatDong: BIT"
    ], c_slate_hdr, c_slate_bg)

    # 4. LuotLuuTru (Center)
    draw_entity(37, 22, 25, 29, "LuotLuuTru", [
        "[PK] MaLuotLuuTru: BIGINT",
        "[FK] MaPhong: INT",
        "[FK] MaKhachHang: INT",
        "TenKhach: NVARCHAR(100)",
        "SoDienThoai: NVARCHAR(20)",
        "SoGiayTo: NVARCHAR(20)",
        "TrangThai: VARCHAR(20)",
        "ThoiDiemDen: DATETIME2",
        "ThoiDiemDi: DATETIME2",
        "ThoiDiemNhanPhong: DATETIME2",
        "ThoiDiemTraPhong: DATETIME2",
        "TienCoc: DECIMAL(18,2)",
        "[FK] MaNhanVien: INT"
    ], c_blue_hdr, c_blue_bg)

    # 5. HoaDon (Bottom Left)
    draw_entity(4, 15, 23, 21, "HoaDon", [
        "[PK] MaHoaDon: BIGINT",
        "[FK] MaLuotLuuTru: BIGINT",
        "SoPhongHoaDon: NVARCHAR(10)",
        "TenKhach: NVARCHAR(100)",
        "ThoiDiemLapHoaDon: DATETIME2",
        "TienPhong: DECIMAL(18,2)",
        "TienCoc: DECIMAL(18,2)",
        "TienDaThu: DECIMAL(18,2)",
        "PhuongThucThanhToan: NVARCHAR(30)",
        "[FK] MaNhanVien: INT"
    ], c_green_hdr, c_green_bg)

    # 6. GiaoDichThanhToan (Bottom Center)
    draw_entity(73, 10, 23, 19, "GiaoDichThanhToan", [
        "[PK] MaGiaoDich: BIGINT",
        "[FK] MaLuotLuuTru: BIGINT",
        "LoaiGiaoDich: VARCHAR(20)",
        "SoTien: DECIMAL(18,2)",
        "ThoiDiemTao: DATETIME2",
        "PhuongThucThanhToan: NVARCHAR(30)",
        "[FK] MaNhanVien: INT",
        "[FK] MaCaTruc: BIGINT"
    ], c_blue_hdr, c_blue_bg)

    # 7. CaTruc (Right Center)
    draw_entity(73, 34, 23, 20, "CaTruc", [
        "[PK] MaCaTruc: BIGINT",
        "[FK] MaNhanVien: INT",
        "ThoiDiemMoCa: DATETIME2",
        "ThoiDiemDongCa: DATETIME2",
        "TienMatDauCa: DECIMAL(18,2)",
        "TienMatKiemDem: DECIMAL(18,2)",
        "TienMatDuKien: DECIMAL(18,2)",
        "GiaiTrinh: NVARCHAR(500)",
        "TrangThai: VARCHAR(12)"
    ], c_slate_hdr, c_slate_bg)

    def draw_rel(x1, y1, x2, y2, l1="1", l2="N"):
        mid_x = (x1 + x2) / 2
        ax.plot([x1, mid_x, mid_x, x2], [y1, y1, y2, y2], color='#475569', linewidth=1.4)
        ax.text(x1, y1, f" {l1} ", fontsize=8, fontweight='bold', color='#B45309', ha='center', va='center',
                bbox=dict(boxstyle='round,pad=0.15', facecolor='#FEF3C7', edgecolor='#F59E0B'))
        ax.text(x2, y2, f" {l2} ", fontsize=8, fontweight='bold', color='#1D4ED8', ha='center', va='center',
                bbox=dict(boxstyle='round,pad=0.15', facecolor='#DBEAFE', edgecolor='#3B82F6'))

    draw_rel(25, 75, 37, 45, "1", "N")
    draw_rel(50, 67, 50, 51, "1", "N")
    draw_rel(73, 73, 62, 40, "1", "N")
    draw_rel(84, 66, 84, 54, "1", "N")
    draw_rel(37, 30, 27, 30, "1", "1")
    draw_rel(62, 26, 73, 20, "1", "N")
    draw_rel(84, 34, 84, 29, "1", "N")
    draw_rel(73, 70, 27, 24, "1", "N")

    plt.tight_layout()
    plt.savefig(out_path, dpi=220, bbox_inches='tight', facecolor='#FFFFFF')
    plt.close()
    print(f"Created ERD diagram (no services): {out_path}")

if __name__ == '__main__':
    draw_erd()
