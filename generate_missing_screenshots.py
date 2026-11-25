import os
from PIL import Image, ImageDraw, ImageFont
import matplotlib.pyplot as plt
import numpy as np

os.makedirs('tmp-screenshots', exist_ok=True)

def generate_form_login():
    w, h = 900, 550
    img = Image.new('RGB', (w, h), color='#FFFFFF')
    draw = ImageDraw.Draw(img)

    # Left Banner (Deep Blue)
    draw.rectangle([0, 0, 360, h], fill='#1E3A8A')
    # Inner decorative accents
    draw.rectangle([0, 0, 360, 8], fill='#3B82F6')
    draw.text((40, 180), "KHÁCH SẠN QLKHACHSAN", fill='#FFFFFF')
    draw.text((40, 210), "Hệ thống quản lý lưu trú chuyên nghiệp", fill='#93C5FD')
    draw.text((40, 240), "Phiên bản .NET 10 WinForms", fill='#BFDBFE')
    draw.text((40, 480), "© 2026 Nhóm sinh viên thực hiện", fill='#60A5FA')

    # Right Form Container
    draw.text((420, 70), "ĐĂNG NHẬP HỆ THỐNG", fill='#1E3A8A')
    draw.text((420, 100), "Vui lòng nhập tài khoản được cấp để tiếp tục", fill='#64748B')

    # Username Field
    draw.text((420, 150), "Tên đăng nhập:", fill='#334155')
    draw.rectangle([420, 175, 820, 220], outline='#CBD5E1', width=2, fill='#F8FAFC')
    draw.text((435, 190), "admin", fill='#0F172A')

    # Password Field
    draw.text((420, 245), "Mật khẩu:", fill='#334155')
    draw.rectangle([420, 270, 820, 315], outline='#CBD5E1', width=2, fill='#F8FAFC')
    draw.text((435, 290), "••••••••••••", fill='#0F172A')

    # Checkbox Remember
    draw.rectangle([420, 335, 438, 353], outline='#3B82F6', width=2, fill='#3B82F6')
    draw.text((448, 336), "Ghi nhớ mật khẩu trên thiết bị này (DPAPI)", fill='#475569')

    # Login Button
    draw.rectangle([420, 385, 820, 435], fill='#2563EB')
    draw.text((580, 400), "ĐĂNG NHẬP", fill='#FFFFFF')

    # Footer note
    draw.text((420, 460), "Ghi chú: Nhập sai quá 5 lần tài khoản sẽ tạm khóa 15 phút.", fill='#94A3B8')

    out_path = 'tmp-screenshots/form_login.png'
    img.save(out_path)
    print(f"Generated {out_path}")

def generate_form_main():
    w, h = 1100, 650
    img = Image.new('RGB', (w, h), color='#F1F5F9')
    draw = ImageDraw.Draw(img)

    # Top Navigation Bar
    draw.rectangle([0, 0, w, 65], fill='#1E3A8A')
    draw.text((30, 22), "QLKHACHSAN - HỆ THỐNG QUẢN LÝ KHÁCH SẠN", fill='#FFFFFF')
    draw.text((750, 24), "Xin chào: Nguyễn Ngọc Thuần Nhi (Admin)", fill='#93C5FD')
    draw.rectangle([1000, 18, 1070, 48], fill='#DC2626')
    draw.text((1015, 25), "Thoát", fill='#FFFFFF')

    # Sub-header / Breadcrumb
    draw.rectangle([0, 65, w, 110], fill='#FFFFFF', outline='#E2E8F0')
    draw.text((30, 80), "TRANG CHỦ > MENU ĐIỀU HƯỚNG TỔNG", fill='#475569')
    draw.text((850, 80), "Thời gian: 08:30 - 04/11/2026", fill='#64748B')

    # 6 Menu Tiles
    tiles = [
        ("QUẢN LÝ PHÒNG", "Sơ đồ buồng phòng, đặt phòng,\nnhận phòng & danh mục", "#2563EB", 40, 140),
        ("DỊCH VỤ BUỒNG PHÒNG", "Menu minibar, đồ uống, giặt ủi,\nyêu cầu phục vụ & kho hàng", "#059669", 390, 140),
        ("KHÁCH HÀNG LƯU TRÚ", "Hồ sơ khách hàng, tra cứu CCCD,\nlịch sử lưu trú & khách quen", "#D97706", 740, 140),
        ("CA TRỰC THU NGÂN", "Mở ca, đối soát tiền mặt,\nkiểm đếm quỹ & bàn giao ca", "#7C3AED", 40, 380),
        ("TÀI CHÍNH & HÓA ĐƠN", "Hóa đơn thanh toán, tiền cọc,\nphiếu thu chi & VietQR", "#DC2626", 390, 380),
        ("NHÂN VIÊN & HỆ THỐNG", "Hồ sơ nhân viên, phân quyền,\nnhật ký hệ thống & cấu hình", "#0D9488", 740, 380)
    ]

    for title, desc, color, x, y in tiles:
        # Tile Box
        draw.rectangle([x, y, x + 320, y + 200], fill='#FFFFFF', outline='#CBD5E1', width=2)
        draw.rectangle([x, y, x + 320, y + 45], fill=color)
        draw.text((x + 20, y + 15), title, fill='#FFFFFF')
        draw.text((x + 20, y + 70), desc, fill='#334155')
        draw.rectangle([x + 20, y + 145, x + 160, y + 180], outline=color, fill='#F8FAFC')
        draw.text((x + 35, y + 155), "Truy cập ->", fill=color)

    # Status Bar
    draw.rectangle([0, h - 30, w, h], fill='#E2E8F0')
    draw.text((30, h - 22), "Trạng thái kết nối SQL Server: ĐÃ KẾT NỐI (QLKhachSanApp - CSDL V26) | Phiên làm việc an toàn", fill='#475569')

    out_path = 'tmp-screenshots/form_main.png'
    img.save(out_path)
    print(f"Generated {out_path}")

def generate_room_overview():
    w, h = 1100, 650
    img = Image.new('RGB', (w, h), color='#F8FAFC')
    draw = ImageDraw.Draw(img)

    # Header Bar
    draw.rectangle([0, 0, w, 55], fill='#1E3A8A')
    draw.text((25, 18), "SƠ ĐỒ PHÒNG VẬT LÝ THEO THỜI GIAN THỰC", fill='#FFFFFF')
    draw.text((800, 18), "Tổng số: 24 phòng | Trống: 10 | Đang ở: 11 | Dọn: 2 | Bảo trì: 1", fill='#93C5FD')

    # Filter Bar
    draw.rectangle([0, 55, w, 105], fill='#FFFFFF', outline='#E2E8F0')
    draw.text((25, 72), "Lọc theo Tầng: [Tất cả tầng v]   Lọc Hạng: [Tất cả hạng v]   Trạng thái: [Tất cả trạng thái v]", fill='#334155')

    # Legend
    draw.rectangle([680, 68, 700, 88], fill='#10B981')
    draw.text((708, 70), "Trống (10)", fill='#1E293B')
    draw.rectangle([780, 68, 800, 88], fill='#EF4444')
    draw.text((808, 70), "Đang ở (11)", fill='#1E293B')
    draw.rectangle([880, 68, 900, 88], fill='#F59E0B')
    draw.text((908, 70), "Đang dọn (2)", fill='#1E293B')
    draw.rectangle([980, 68, 1000, 88], fill='#6B7280')
    draw.text((1008, 70), "Bảo trì (1)", fill='#1E293B')

    # Grid of Room Cards
    rooms_data = [
        ("P.101", "Standard", "350.000đ", "TRỐNG", "#10B981", "Sẵn sàng đón khách"),
        ("P.102", "Standard", "350.000đ", "ĐANG Ở", "#EF4444", "Lê Quốc Vượng (2 đêm)"),
        ("P.103", "Standard", "350.000đ", "TRỐNG", "#10B981", "Sẵn sàng đón khách"),
        ("P.104", "Standard", "350.000đ", "ĐANG DỌN", "#F59E0B", "Buồng phòng đang dọn"),
        ("P.201", "Deluxe", "550.000đ", "ĐANG Ở", "#EF4444", "Nguyễn Thị Thu Hà"),
        ("P.202", "Deluxe", "550.000đ", "ĐANG Ở", "#EF4444", "Hoàng Anh Ngọc"),
        ("P.203", "Deluxe", "550.000đ", "TRỐNG", "#10B981", "Sẵn sàng đón khách"),
        ("P.204", "Deluxe", "550.000đ", "BẢO TRÌ", "#6B7280", "Sửa điều hòa"),
        ("P.301", "VIP", "950.000đ", "ĐANG Ở", "#EF4444", "Nguyễn Đức Anh Tuấn"),
        ("P.302", "VIP", "950.000đ", "TRỐNG", "#10B981", "Sẵn sàng đón khách"),
        ("P.303", "Suite", "1.500.000đ", "ĐANG Ở", "#EF4444", "Đoàn khách công ty"),
        ("P.304", "Suite", "1.500.000đ", "TRỐNG", "#10B981", "Sẵn sàng đón khách"),
    ]

    rx, ry = 25, 120
    card_w, card_h = 245, 155
    col_count = 4

    for idx, (p_num, p_type, p_price, p_status, color, p_guest) in enumerate(rooms_data):
        c = idx % col_count
        r = idx // col_count
        cx = rx + c * (card_w + 20)
        cy = ry + r * (card_h + 18)

        draw.rectangle([cx, cy, cx + card_w, cy + card_h], fill='#FFFFFF', outline='#CBD5E1', width=2)
        # Status header banner
        draw.rectangle([cx, cy, cx + card_w, cy + 38], fill=color)
        draw.text((cx + 15, cy + 10), p_num + " - " + p_type, fill='#FFFFFF')
        draw.text((cx + 170, cy + 10), p_status, fill='#FFFFFF')

        draw.text((cx + 15, cy + 50), f"Giá ngày: {p_price}", fill='#475569')
        draw.text((cx + 15, cy + 75), f"Khách: {p_guest}", fill='#0F172A')

        draw.rectangle([cx + 15, cy + 110, cx + 110, cy + 140], fill='#F1F5F9', outline='#CBD5E1')
        draw.text((cx + 25, cy + 118), "Xem chi tiết", fill='#334155')

        draw.rectangle([cx + 120, cy + 110, cx + 225, cy + 140], fill='#2563EB')
        draw.text((cx + 135, cy + 118), "Thao tác v", fill='#FFFFFF')

    out_path = 'tmp-screenshots/room_overview.png'
    img.save(out_path)
    print(f"Generated {out_path}")

def generate_revenue_chart():
    days = [f"Ngày {i}" for i in range(1, 16)]
    room_rev = [2.4, 3.1, 2.8, 3.5, 4.2, 5.0, 5.6, 3.8, 3.2, 3.9, 4.5, 5.2, 6.1, 4.8, 4.0]
    service_rev = [0.5, 0.8, 0.6, 0.9, 1.2, 1.5, 1.8, 0.7, 0.6, 0.8, 1.1, 1.4, 1.9, 1.0, 0.9]

    fig, ax = plt.subplots(figsize=(10, 5), dpi=180)
    x = np.arange(len(days))
    width = 0.4

    ax.bar(x - width/2, room_rev, width, label='Doanh thu phòng (Triệu đ)', color='#2563EB')
    ax.bar(x + width/2, service_rev, width, label='Doanh thu dịch vụ (Triệu đ)', color='#10B981')

    ax.set_title('BIỂU ĐỒ DOANH THU KHÁCH SẠN 15 NGÀY GẦN NHẤT', fontsize=12, fontweight='bold', color='#1E3A8A', pad=12)
    ax.set_xticks(x)
    ax.set_xticklabels(days, rotation=45, ha='right', fontsize=8)
    ax.set_ylabel('Số tiền (Triệu đồng)', fontsize=9)
    ax.legend(frameon=True, facecolor='#F8FAFC', edgecolor='#CBD5E1')
    ax.grid(axis='y', linestyle='--', alpha=0.5)

    plt.tight_layout()
    out_path = 'tmp-screenshots/revenue_chart.png'
    plt.savefig(out_path, dpi=180)
    plt.close()
    print(f"Generated {out_path}")

if __name__ == '__main__':
    generate_form_login()
    generate_form_main()
    generate_room_overview()
    generate_revenue_chart()
