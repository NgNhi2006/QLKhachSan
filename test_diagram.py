import os
import matplotlib
matplotlib.use('Agg')
import matplotlib.pyplot as plt
import matplotlib.patches as patches

os.makedirs('/sessions/dazzling-gracious-gates/mnt/QLKhachSan/tmp-screenshots', exist_ok=True)

fig, ax = plt.subplots(figsize=(20, 14), dpi=220)
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

    # Shadow & Body
    ax.add_patch(patches.Rectangle((x + 0.3, y - 0.3), w, total_h, facecolor='#E2E8F0', edgecolor='none'))
    ax.add_patch(patches.Rectangle((x, y), w, total_h, facecolor='#FFFFFF', edgecolor='#718096', linewidth=1.2))

    # Title bar
    ax.add_patch(patches.Rectangle((x, y + total_h - header_h), w, header_h, facecolor='#2B579A', edgecolor='#1E395B', linewidth=1))
    ax.text(x + 0.8, y + total_h - header_h / 2, f"dbo.{title}", fontsize=9.5, fontweight='bold', color='#FFFFFF', va='center')

    # Column header
    ax.add_patch(patches.Rectangle((x, y + total_h - header_h - col_hdr_h), w, col_hdr_h, facecolor='#F1F5F9', edgecolor='#CBD5E1', linewidth=0.8))
    ax.text(x + 2.2, y + total_h - header_h - col_hdr_h / 2, "Column Name", fontsize=8, fontweight='bold', color='#475569', va='center')
    ax.text(x + w * 0.62, y + total_h - header_h - col_hdr_h / 2, "Data Type", fontsize=8, fontweight='bold', color='#475569', va='center')
    ax.plot([x + w * 0.58, x + w * 0.58], [y, y + total_h - header_h], color='#E2E8F0', linewidth=0.8)

    curr_y = y + total_h - header_h - col_hdr_h - row_h / 2
    for col_name, data_type, is_pk, is_fk in columns:
        ax.plot([x, x + w], [curr_y - row_h / 2, curr_y - row_h / 2], color='#F1F5F9', linewidth=0.6)
        if is_pk:
            ax.text(x + 0.4, curr_y, "PK", fontsize=7, fontweight='bold', color='#B45309', va='center')
        elif is_fk:
            ax.text(x + 0.4, curr_y, "FK", fontsize=6.5, fontweight='bold', color='#2563EB', va='center')

        weight = 'bold' if is_pk else 'normal'
        color = '#0F172A' if is_pk else '#334155'
        ax.text(x + 2.2, curr_y, col_name, fontsize=8, fontweight=weight, color=color, va='center')
        ax.text(x + w * 0.62, curr_y, data_type, fontsize=7.5, color='#64748B', va='center')
        curr_y -= row_h
    return x, y, w, total_h

# 1. KhachHang
draw_ssms_table(3, 72, 19, "KhachHang", [
    ("MaKhachHang", "int", True, False),
    ("TenKhachHang", "nvarchar(100)", False, False),
    ("SoDienThoai", "nvarchar(20)", False, False),
    ("SoGiayTo", "nvarchar(20)", False, False),
    ("DiaChi", "nvarchar(200)", False, False),
    ("ThoiDiemTao", "datetime2", False, False)
])

# 2. Phong
draw_ssms_table(39, 70, 20, "Phong", [
    ("MaPhong", "int", True, False),
    ("SoPhong", "nvarchar(10)", False, False),
    ("LoaiPhong", "nvarchar(20)", False, False),
    ("DonGiaPhong", "decimal(18,2)", False, False),
    ("TienCoc", "decimal(18,2)", False, False),
    ("TrangThai", "varchar(20)", False, False),
    ("Tang", "int", False, False)
])

# 3. TaiKhoanNhanVien
draw_ssms_table(74, 69, 23, "TaiKhoanNhanVien", [
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
draw_ssms_table(37, 33, 24, "LuotLuuTru", [
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

# 5. DichVu
draw_ssms_table(3, 8, 19, "DichVu", [
    ("MaDichVu", "int", True, False),
    ("DanhMuc", "nvarchar(100)", False, False),
    ("TenDichVu", "nvarchar(150)", False, False),
    ("DonGia", "decimal(18,2)", False, False),
    ("DonViTinh", "nvarchar(20)", False, False),
    ("DangHoatDong", "bit", False, False)
])

# 6. YeuCauDichVu
draw_ssms_table(27, 7, 21, "YeuCauDichVu", [
    ("MaYeuCau", "bigint", True, False),
    ("MaLuotLuuTru", "bigint", False, True),
    ("MaDichVu", "int", False, True),
    ("TenDichVu", "nvarchar(150)", False, False),
    ("SoLuong", "int", False, False),
    ("DonGia", "decimal(18,2)", False, False),
    ("ThanhTien", "decimal(18,2)", False, False),
    ("ThoiDiemGoi", "datetime2", False, False),
    ("MaNhanVien", "int", False, True)
])

# 7. HoaDon
draw_ssms_table(52, 6, 21, "HoaDon", [
    ("MaHoaDon", "bigint", True, False),
    ("MaLuotLuuTru", "bigint", False, True),
    ("SoPhongHoaDon", "nvarchar(10)", False, False),
    ("TenKhach", "nvarchar(100)", False, False),
    ("ThoiDiemLapHoaDon", "datetime2", False, False),
    ("TienPhong", "decimal(18,2)", False, False),
    ("TienDichVu", "decimal(18,2)", False, False),
    ("TienCoc", "decimal(18,2)", False, False),
    ("TienDaThu", "decimal(18,2)", False, False),
    ("PhuongThucThanhToan", "nvarchar(30)", False, False),
    ("MaNhanVien", "int", False, True)
])

# 8. CaTruc
draw_ssms_table(76, 8, 21, "CaTruc", [
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

def draw_ssms_rel(x1, y1, x2, y2):
    mid_x = (x1 + x2) / 2
    ax.plot([x1, mid_x, mid_x, x2], [y1, y1, y2, y2], color='#475569', linewidth=1.4)
    ax.text(x1, y1, " 1 ", fontsize=8, fontweight='bold', color='#B45309', ha='center', va='center', bbox=dict(boxstyle='square,pad=0.1', facecolor='#FEF3C7', edgecolor='none'))
    ax.text(x2, y2, " ∞ ", fontsize=9, fontweight='bold', color='#1E293B', ha='center', va='center', bbox=dict(boxstyle='square,pad=0.1', facecolor='#E2E8F0', edgecolor='none'))

# KhachHang (1) -> LuotLuuTru (N)
draw_ssms_rel(22, 78, 37, 50)

# Phong (1) -> LuotLuuTru (N)
draw_ssms_rel(49, 70, 49, 57)

# TaiKhoanNhanVien (1) -> LuotLuuTru (N)
draw_ssms_rel(74, 76, 61, 36)

# TaiKhoanNhanVien (1) -> CaTruc (N)
draw_ssms_rel(85, 69, 85, 26)

# LuotLuuTru (1) -> YeuCauDichVu (N)
draw_ssms_rel(43, 33, 43, 25)

# DichVu (1) -> YeuCauDichVu (N)
draw_ssms_rel(22, 18, 27, 18)

# LuotLuuTru (1) -> HoaDon (1)
draw_ssms_rel(55, 33, 55, 24)

# TaiKhoanNhanVien (1) -> HoaDon (N)
draw_ssms_rel(74, 72, 68, 24)

out_path = '/sessions/dazzling-gracious-gates/mnt/QLKhachSan/tmp-screenshots/ssms_diagram.png'
plt.savefig(out_path, dpi=220, bbox_inches='tight', facecolor='#FFFFFF')
plt.close()
print("Success! Generated:", out_path)
