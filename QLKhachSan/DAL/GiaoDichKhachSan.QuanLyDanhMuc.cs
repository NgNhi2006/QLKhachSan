using QLKhachSan.DTO;

namespace QLKhachSan.DAL;

public sealed partial class HotelTransaction
{
    public async Task<List<RevenueDay>> RevenueTrendAsync(DateTime from,DateTime until)
    {
        var rows=await Query("SELECT Day,SUM(Rooms),SUM(Services),SUM(Forfeits) FROM (SELECT CAST(i.ThoiDiemLapHoaDon AS date) Day,i.TienPhong Rooms,i.TienDichVu Services,CAST(0 AS decimal(18,2)) Forfeits FROM dbo.HoaDon i WHERE i.ThoiDiemLapHoaDon>=@p0 AND i.ThoiDiemLapHoaDon<@p1 UNION ALL SELECT CAST(ThoiDiemTao AS date),0,0,SoTien FROM dbo.GiaoDichThanhToan WHERE LoaiGiaoDich='Forfeit' AND ThoiDiemTao>=@p0 AND ThoiDiemTao<@p1) r GROUP BY Day ORDER BY Day",r=>new RevenueDay(r.GetDateTime(0),r.GetDecimal(1),r.GetDecimal(2),r.GetDecimal(3)),from,until);
        var byDate=rows.ToDictionary(r=>r.Day);
        return Enumerable.Range(0,(until.Date-from.Date).Days).Select(i=>from.Date.AddDays(i)).Select(day=>byDate.GetValueOrDefault(day)??new RevenueDay(day,0,0,0)).ToList();
    }
    public async Task EnsureAvailableAsync(int roomId, DateTime from, DateTime until, long excluding = 0)
    {
        if (until <= from) throw new BusinessException("Ngày trả phải sau ngày nhận.");
        var conflicts = await Scalar("SELECT COUNT_BIG(*) FROM dbo.LuotLuuTru WHERE MaPhong=@p0 AND ConHieuLuc=1 AND Ma<>@p3 AND COALESCE(ThoiDiemNhanPhong,ThoiDiemDen)<@p2 AND ThoiDiemDi>@p1", roomId,from,until,excluding);
        if (conflicts > 0) throw new BusinessException("Phòng đã có lịch trong khoảng ngày này. Hãy chọn phòng hoặc thời gian khác.");
    }
    public Task<List<Stay>> StayHistoryAsync(string search) => Query("SELECT TOP(500) "+StayColumns+" FROM dbo.LuotLuuTru WHERE @p0=N'' OR CHARINDEX(@p0,TenKhach)>0 OR CHARINDEX(@p0,SoGiayTo)>0 OR CHARINDEX(@p0,SoDienThoai)>0 ORDER BY Ma DESC",MapStay,search);
    public Task<List<TodayScheduleItem>> TodayScheduleAsync(DateTime day) => Query(@"
SELECT s.Ma,r.SoPhong,s.TenKhach,s.SoDienThoai,N'Chờ nhận',s.ThoiDiemDen
FROM dbo.LuotLuuTru s JOIN dbo.Phong r ON r.Ma=s.MaPhong
WHERE s.TrangThai='Reserved' AND s.ThoiDiemDen>=@p0 AND s.ThoiDiemDen<@p1
UNION ALL
SELECT s.Ma,r.SoPhong,s.TenKhach,s.SoDienThoai,N'Đã nhận',s.ThoiDiemNhanPhong
FROM dbo.LuotLuuTru s JOIN dbo.Phong r ON r.Ma=s.MaPhong
WHERE s.ThoiDiemNhanPhong>=@p0 AND s.ThoiDiemNhanPhong<@p1
UNION ALL
SELECT s.Ma,r.SoPhong,s.TenKhach,s.SoDienThoai,N'Chờ trả',s.ThoiDiemDi
FROM dbo.LuotLuuTru s JOIN dbo.Phong r ON r.Ma=s.MaPhong
WHERE s.TrangThai='Occupied' AND s.ThoiDiemDi>=@p0 AND s.ThoiDiemDi<@p1
UNION ALL
SELECT i.MaLuotLuuTru,i.SoPhongHoaDon,i.TenKhach,s.SoDienThoai,N'Đã trả',i.ThoiDiemLapHoaDon
FROM dbo.HoaDon i JOIN dbo.LuotLuuTru s ON s.Ma=i.MaLuotLuuTru
WHERE i.ThoiDiemLapHoaDon>=@p0 AND i.ThoiDiemLapHoaDon<@p1
ORDER BY 6,1",r=>new TodayScheduleItem(r.GetInt64(0),r.GetString(1),r.GetString(2),r.GetString(3),r.GetString(4),r.GetDateTime(5)),day.Date,day.Date.AddDays(1));
    public Task<int> AddDepositAsync(long id,decimal amount,DateTime? holdUntil) => Execute("UPDATE dbo.LuotLuuTru SET TienCoc=TienCoc+@p1,HanGiuPhong=@p2,PhienBan=PhienBan+1 WHERE Ma=@p0",id,amount,holdUntil);
    public async Task UpdateBookingAsync(Stay stay,Room room,GuestInput guest,DateTime arrival,DateTime departure,DateTime receiveBy)
    {
        await Execute("IF EXISTS(SELECT 1 FROM dbo.KhachHang WHERE SoGiayTo=@p2) UPDATE dbo.KhachHang SET Ten=@p0,SoDienThoai=@p1 WHERE SoGiayTo=@p2; ELSE INSERT dbo.KhachHang(Ten,SoDienThoai,SoGiayTo) VALUES(@p0,@p1,@p2)",guest.Name,guest.Phone,guest.Identity);
        await Execute("UPDATE dbo.LuotLuuTru SET MaPhong=@p1,TenKhach=@p2,SoDienThoai=@p3,SoGiayTo=@p4,MaKhachHang=(SELECT Ma FROM dbo.KhachHang WHERE SoGiayTo=@p4),ThoiDiemDen=@p5,ThoiDiemDi=@p6,HanGiuPhong=@p7,PhienBan=PhienBan+1 WHERE Ma=@p0",stay.Id,room.Id,guest.Name,guest.Phone,guest.Identity,arrival,departure,receiveBy);
    }
    public async Task UpdateOccupiedGuestAsync(Stay stay,GuestInput guest)
    {
        await Execute("IF EXISTS(SELECT 1 FROM dbo.KhachHang WHERE SoGiayTo=@p2) UPDATE dbo.KhachHang SET Ten=@p0,SoDienThoai=@p1 WHERE SoGiayTo=@p2; ELSE INSERT dbo.KhachHang(Ten,SoDienThoai,SoGiayTo) VALUES(@p0,@p1,@p2)",guest.Name,guest.Phone,guest.Identity);
        await Execute("UPDATE dbo.LuotLuuTru SET TenKhach=@p1,SoDienThoai=@p2,SoGiayTo=@p3,MaKhachHang=(SELECT Ma FROM dbo.KhachHang WHERE SoGiayTo=@p3),PhienBan=PhienBan+1 WHERE Ma=@p0",stay.Id,guest.Name,guest.Phone,guest.Identity);
    }
    public async Task<bool> HasStayFinancialActivityAsync(long stayId) =>
        await Scalar("SELECT COUNT_BIG(*) FROM dbo.GiaoDichThanhToan WHERE MaLuotLuuTru=@p0",stayId)>0 ||
        await Scalar("SELECT COUNT_BIG(*) FROM dbo.HoaDon WHERE MaLuotLuuTru=@p0",stayId)>0;
    public Task<int> UpdateOrderAsync(long id,int quantity,DateTime now) => Task.FromResult(0);
    public Task<int> MoveOrderAsync(long id,long targetStayId) => Task.FromResult(0);
    public Task<int> CancelOrderAsync(long id,DateTime now,string reason) => Task.FromResult(0);
    public Task<int> DeliverOrderAsync(long id,int quantity,DateTime now) => Task.FromResult(0);
    public Task<List<PaymentEntry>> PaymentsAsync(DateTime from,DateTime until) => Query("SELECT p.Ma,p.MaLuotLuuTru,s.TenKhach,p.LoaiGiaoDich,p.SoTien,p.ThoiDiemTao,p.PhuongThucThanhToan,p.GhiChu,u.TenDangNhap FROM dbo.GiaoDichThanhToan p JOIN dbo.LuotLuuTru s ON s.Ma=p.MaLuotLuuTru JOIN dbo.TaiKhoanNhanVien u ON u.Ma=p.MaNguoiTao WHERE p.ThoiDiemTao>=@p0 AND p.ThoiDiemTao<@p1 ORDER BY p.Ma DESC",r=>new PaymentEntry(r.GetInt64(0),r.GetInt64(1),r.GetString(2),r.GetString(3),r.GetDecimal(4),r.GetDateTime(5),r.GetString(6),r.GetString(7),r.GetString(8)),from,until);
    public async Task<CustomerDepositHistory> DepositHistoryAsync(int customerId)
    {
        var stays=await Query("SELECT "+StayColumns+" FROM dbo.LuotLuuTru WHERE MaKhachHang=@p0 ORDER BY Ma DESC",MapStay,customerId);
        var payments=await Query("SELECT p.Ma,p.MaLuotLuuTru,s.TenKhach,p.LoaiGiaoDich,p.SoTien,p.ThoiDiemTao,p.PhuongThucThanhToan,p.GhiChu,u.TenDangNhap FROM dbo.GiaoDichThanhToan p JOIN dbo.LuotLuuTru s ON s.Ma=p.MaLuotLuuTru JOIN dbo.TaiKhoanNhanVien u ON u.Ma=p.MaNguoiTao WHERE s.MaKhachHang=@p0 ORDER BY p.Ma DESC",r=>new PaymentEntry(r.GetInt64(0),r.GetInt64(1),r.GetString(2),r.GetString(3),r.GetDecimal(4),r.GetDateTime(5),r.GetString(6),r.GetString(7),r.GetString(8)),customerId);
        return new CustomerDepositHistory(stays,payments);
    }
    public Task<List<DepositReceipt>> DepositReceiptsAsync(DateTime from,DateTime until) => Query("""
        SELECT p.Ma,s.Ma,s.TenKhach,s.SoDienThoai,r.SoPhong,p.ThoiDiemTao,p.SoTien,s.TienCoc,
            CONVERT(decimal(18,2),CASE WHEN DATEDIFF(day,s.ThoiDiemDen,s.ThoiDiemDi)<1 THEN 1 ELSE DATEDIFF(day,s.ThoiDiemDen,s.ThoiDiemDi) END)*r.DonGiaPhong,
            i.TienPhong+i.TienDichVu,p.PhuongThucThanhToan,p.GhiChu
        FROM dbo.GiaoDichThanhToan p JOIN dbo.LuotLuuTru s ON s.Ma=p.MaLuotLuuTru
        JOIN dbo.Phong r ON r.Ma=s.MaPhong LEFT JOIN dbo.HoaDon i ON i.MaLuotLuuTru=s.Ma
        WHERE p.LoaiGiaoDich='Deposit' AND p.ThoiDiemTao>=@p0 AND p.ThoiDiemTao<@p1
        ORDER BY p.Ma DESC
        """,r=>new DepositReceipt(r.GetInt64(0),r.GetInt64(1),r.GetString(2),r.GetString(3),r.GetString(4),
            r.GetDateTime(5),r.GetDecimal(6),r.GetDecimal(7),r.GetDecimal(8),r.IsDBNull(9)?null:r.GetDecimal(9),r.GetString(10),r.GetString(11)),from,until);
    public Task<List<UserInfo>> UsersAsync() => Query("SELECT Ma,TenDangNhap,VaiTro,DangHoatDong,KhoaDen,PhienBanBaoMat,COALESCE(NULLIF(TenHienThi,N''),TenDangNhap),AnhDaiDien FROM dbo.TaiKhoanNhanVien WHERE DaLuuTru=0 ORDER BY TenDangNhap",r=>new UserInfo(r.GetInt32(0),r.GetString(1),r.GetString(2),r.GetBoolean(3),Date(r,4),r.GetInt64(5),r.GetString(6),r.IsDBNull(7)?null:(byte[])r[7]));
    public Task<int> UpdateProfileAsync(int id,string displayName,byte[]? avatarPng) =>
        Execute("UPDATE dbo.TaiKhoanNhanVien SET TenHienThi=@p1,AnhDaiDien=CONVERT(varbinary(max),@p2) WHERE Ma=@p0 AND DaLuuTru=0",id,displayName,avatarPng);
    public Task<long> ActiveAdminsAsync() => Scalar("SELECT COUNT_BIG(*) FROM dbo.TaiKhoanNhanVien WHERE DangHoatDong=1 AND DaLuuTru=0 AND VaiTro='Admin'");
    public Task<int> ArchiveUsersAsync() => Execute("UPDATE dbo.TaiKhoanNhanVien SET DaLuuTru=1,DangHoatDong=0,PhienBanBaoMat=PhienBanBaoMat+1,KhoaDen=NULL WHERE DaLuuTru=0");
    public Task<int> UpdateUserAsync(int id,string role,bool active) => Execute("UPDATE dbo.TaiKhoanNhanVien SET VaiTro=@p1,DangHoatDong=@p2,PhienBanBaoMat=PhienBanBaoMat+1,SoLanDangNhapSai=0,KhoaDen=NULL WHERE Ma=@p0",id,role,active);
    public Task<List<ServiceCatalogItem>> CatalogAsync() => Task.FromResult(new List<ServiceCatalogItem>());
    public Task<int> SaveServiceAsync(ServiceCatalogItem item) => Task.FromResult(0);
    public Task<int> SaveRoomAsync(Room room) => room.Id==0
        ? Execute("INSERT dbo.Phong(SoPhong,Loai,DonGiaPhong,TienCoc) VALUES(@p0,@p1,@p2,@p3)",room.Number,room.Type,room.Rate,room.Deposit)
        : Execute("UPDATE dbo.Phong SET SoPhong=@p1,Loai=@p2,DonGiaPhong=@p3,TienCoc=@p4,PhienBan=PhienBan+1 WHERE Ma=@p0 AND PhienBan=@p5",room.Id,room.Number,room.Type,room.Rate,room.Deposit,room.Version);
    public Task<long> RoomHistoryCountAsync(int roomId) => Scalar("SELECT COUNT_BIG(*) FROM dbo.LuotLuuTru WHERE MaPhong=@p0",roomId);
    public Task<int> DeleteRoomAsync(Room room) => Execute("DELETE FROM dbo.Phong WHERE Ma=@p0 AND PhienBan=@p1 AND NOT EXISTS(SELECT 1 FROM dbo.LuotLuuTru WHERE MaPhong=@p0)",room.Id,room.Version);
    public Task<List<AuditEntry>> AuditsAsync(DateTime from,DateTime until) => Task.FromResult(new List<AuditEntry>());
}
