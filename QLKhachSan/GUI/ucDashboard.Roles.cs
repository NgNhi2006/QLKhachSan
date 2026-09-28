using QLKhachSan.BLL;

namespace QLKhachSan.GUI;

public partial class ucDashboard
{
    private string DashboardTitle() => user.Role switch
    {
        "Admin" => "Tổng quan quản trị",
        "Manager" => "Tổng quan quản lý",
        "Reception" => "Bàn lễ tân",
        "Accountant" => "Tổng quan kế toán",
        _ => "Tổng quan"
    };

    private void ConfigureRoleDashboard()
    {
        lblHeaderTitle.Text=$"{DashboardTitle()} • {user.Username}";
        lblToolsTitle.Text=user.Role switch
        {
            "Admin" => "QUẢN TRỊ HỆ THỐNG",
            "Manager" => "ĐIỀU HÀNH KHÁCH SẠN",
            "Reception" => "NGHIỆP VỤ LỄ TÂN",
            "Accountant" => "TÀI CHÍNH - KẾ TOÁN",
            _ => "CHỨC NĂNG"
        };
        if(user.Role=="Accountant")
        {
            lblCard1Title.Text="HÓA ĐƠN HÔM NAY";
            lblCard2Title.Text="DOANH THU HÓA ĐƠN";
            lblCard3Title.Text="KHOẢN THU";
            lblCard4Title.Text="KHOẢN HOÀN";
            lblCard1Sub.Text="Đã phát hành";
            lblCard2Sub.Text="Trong ngày";
            lblCard3Sub.Text="Cọc và thanh toán";
            lblCard4Sub.Text="Hoàn cho khách";
            lblCard5Title.Text="DOANH THU HÔM NAY";
            tabThongKe.Text="Doanh thu";
            txtTimPhong.Visible=false;btnTimPhong.Visible=false;
            tlpBody.ColumnStyles[2].Width=0;
            AddTool("Báo cáo tài chính",600,ShowInvoices);
            AddTool("Các khoản thu / hoàn",645,ShowCashFlow);
            AddTool("Thống kê doanh thu",690,ShowRevenueChart);
        }
        else if(user.Role=="Manager")
        {
            lblCard1Title.Text="CÔNG SUẤT PHÒNG";
            lblCard2Title.Text="PHÒNG SẴN SÀNG";
            lblCard3Title.Text="LỊCH ĐẶT SẮP ĐẾN";
            lblCard4Title.Text="KHÁCH SẮP TRẢ";
            tabThongKe.Text="Doanh thu";
        }
        else if(user.Role=="Reception")
        {
            tlpCards.ColumnStyles[4].Width=0;
            btnQuanLyKhach.Text="Khách hàng / Hóa đơn";
            lblCard1Title.Text="PHÒNG ĐANG Ở";
            lblCard2Title.Text="PHÒNG TRỐNG";
            lblCard3Title.Text="CHỜ NHẬN PHÒNG";
            lblCard4Title.Text="CHỜ TRẢ PHÒNG";
            tabMatrix.Text="Tình trạng phòng";
            tabLichTrinh.Text="Lịch nhận / trả";
        }
        else
        {
            lblCard1Title.Text="CÔNG SUẤT PHÒNG";
            tabThongKe.Text="Doanh thu";
        }
    }
}
