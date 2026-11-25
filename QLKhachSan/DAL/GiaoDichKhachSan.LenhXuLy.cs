using QLKhachSan.DTO;

namespace QLKhachSan.DAL;

public sealed partial class HotelTransaction
{
    public async Task<long> CreateStayAsync(Room room, GuestInput guest, bool reserve, DateTime now, DateTime arrival, DateTime departure, decimal deposit, DateTime? hold, UserSession user)
    {
        await Execute("IF EXISTS(SELECT 1 FROM dbo.KhachHang WHERE SoGiayTo=@p2) UPDATE dbo.KhachHang SET Ten=@p0,SoDienThoai=@p1 WHERE SoGiayTo=@p2; ELSE INSERT dbo.KhachHang(Ten,SoDienThoai,SoGiayTo) VALUES(@p0,@p1,@p2);",guest.Name,guest.Phone,guest.Identity);
        return await Scalar("INSERT dbo.LuotLuuTru(MaPhong,MaKhachHang,TenKhach,SoDienThoai,SoGiayTo,TrangThai,ConHieuLuc,ThoiDiemTao,ThoiDiemDen,ThoiDiemDi,ThoiDiemNhanPhong,HanGiuPhong,TienCoc,MaNhanVien) OUTPUT INSERTED.Ma SELECT @p0,Ma,@p1,@p2,@p3,@p4,1,@p5,@p6,@p7,@p8,@p9,@p10,@p11 FROM dbo.KhachHang WHERE SoGiayTo=@p3",
            room.Id,guest.Name,guest.Phone,guest.Identity,reserve?"Reserved":"Occupied",now,arrival,departure,reserve?null:now,hold,deposit,user.Id);
    }
    public async Task SetRoomAsync(Room room, RoomStatus status)
    {
        if(await Execute("UPDATE dbo.Phong SET TrangThai=@p1,PhienBan=PhienBan+1 WHERE Ma=@p0 AND PhienBan=@p2",room.Id,status.ToString(),room.Version)!=1)
            throw new BusinessException("Phòng đã thay đổi ở phiên khác. Hãy làm mới và thử lại.");
    }
    public Task<int> StartSegmentAsync(long stayId, Room room, DateTime now) => Task.FromResult(1);
    public Task<int> EndSegmentAsync(long stayId, DateTime now) => Task.FromResult(1);
    public Task<int> CheckInAsync(Stay stay, DateTime now, DateTime departure) => Execute("UPDATE dbo.LuotLuuTru SET TrangThai='Occupied',ThoiDiemNhanPhong=@p1,ThoiDiemDi=@p2,HanGiuPhong=NULL,PhienBan=PhienBan+1 WHERE Ma=@p0",stay.Id,now,departure);
    public Task<int> TransferAsync(Stay stay, int newRoom) => Execute("UPDATE dbo.LuotLuuTru SET MaPhong=@p1,PhienBan=PhienBan+1 WHERE Ma=@p0",stay.Id,newRoom);
    public Task<int> ExtendAsync(Stay stay, DateTime departure) => Execute("UPDATE dbo.LuotLuuTru SET ThoiDiemDi=@p1,PhienBan=PhienBan+1 WHERE Ma=@p0",stay.Id,departure);
    public Task<int> CloseStayAsync(Stay stay, bool paid, DateTime now) => Execute("UPDATE dbo.LuotLuuTru SET TrangThai=@p1,ConHieuLuc=0,ThoiDiemTraPhong=@p2,PhienBan=PhienBan+1 WHERE Ma=@p0",stay.Id,paid?"Paid":"Cancelled",now);
    public Task<int> TouchStayAsync(long id) => Execute("UPDATE dbo.LuotLuuTru SET PhienBan=PhienBan+1 WHERE Ma=@p0",id);
    public Task<long> AddOrderAsync(long stay, ServiceItem item, int quantity, DateTime now, UserSession user) => Task.FromResult(0L);
    public Task<int> DeliverAsync(long stay, DateTime now) => Task.FromResult(0);
    public async Task<int> PaymentAsync(long stay,string kind,decimal amount,DateTime now,string method,string note,UserSession user,string? reference=null)
    {
        if(amount==0)return 0;
        long? shift=kind=="Forfeit"?null:(await Query("SELECT TOP(1) Ma FROM dbo.CaTruc WHERE MaNhanVien=@p0 AND TrangThai='Open'", r=>r.GetInt64(0), user.Id)).SingleOrDefault();
        reference=string.IsNullOrWhiteSpace(reference)?null:reference.Trim();
        return await Execute("INSERT dbo.GiaoDichThanhToan(MaLuotLuuTru,LoaiGiaoDich,SoTien,ThoiDiemTao,PhuongThucThanhToan,GhiChu,MaNhanVien,MaCaTruc,MaThamChieuNgoai) VALUES(@p0,@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8)",stay,kind,amount,now,method,note,user.Id,shift>0?shift:null,reference);
    }
    public Task<long> InvoiceAsync(BillQuote bill,string method,UserSession user,DateTime issued) => Scalar("INSERT dbo.HoaDon(MaLuotLuuTru,SoPhongHoaDon,TenKhach,ThoiDiemLapHoaDon,TienPhong,TienDichVu,TienCoc,TienDaThu,TienDaHoan,PhuongThucThanhToan,MaNhanVien) VALUES(@p0,@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10); SELECT CONVERT(bigint,SCOPE_IDENTITY());",bill.Stay.Id,bill.Room.Number,bill.Stay.Guest,issued,bill.RoomCharge-Math.Min(bill.RoomCharge,bill.DiscountAmount),bill.Services-Math.Max(0,bill.DiscountAmount-bill.RoomCharge),bill.Stay.Deposit,bill.ToCollect,bill.ToRefund,method,user.Id);
    public Task SaveInvoiceDraftLinesAsync(long invoice,BillQuote bill) => Task.CompletedTask;
}
