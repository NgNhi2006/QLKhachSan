using QLKhachSan.DTO;

namespace QLKhachSan.DAL;

public sealed partial class HotelTransaction
{
    public Task<List<RevenueEvent>> RevenueEventsAsync(DateTime from, DateTime until) => Query("""
        SELECT ThoiDiem,DanhMuc,HangMuc,SoLuong,SoTien FROM (
            SELECT i.ThoiDiemLapHoaDon ThoiDiem,N'Tiền phòng' DanhMuc,N'Tiền phòng' HangMuc,
                CAST(1 AS decimal(18,2)) SoLuong,
                i.TienPhong SoTien
            FROM dbo.HoaDon i
            WHERE i.ThoiDiemLapHoaDon>=@p0 AND i.ThoiDiemLapHoaDon<@p1
            UNION ALL
            SELECT ThoiDiemTao,N'Cọc không hoàn (không đến)',N'Cọc không hoàn',CAST(1 AS decimal(18,2)),SoTien
            FROM dbo.GiaoDichThanhToan
            WHERE LoaiGiaoDich='Forfeit' AND ThoiDiemTao>=@p0 AND ThoiDiemTao<@p1
        ) q ORDER BY ThoiDiem
        """, r => new RevenueEvent(r.GetDateTime(0),r.GetString(1),r.GetString(2),r.GetDecimal(3),r.GetDecimal(4)),from,until);
}
