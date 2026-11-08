using System.Data;
using Microsoft.Data.SqlClient;
using QLKhachSan.DTO;

namespace QLKhachSan.DAL;

// SQL transaction locks coordinate all desktop instances, not just this process.
public sealed class HotelRepository
{
    private readonly string connectionString;
    public HotelRepository(string? connectionString = null) => this.connectionString = connectionString ?? AppSettings.Load().ConnectionString;
    public async Task<T> RunAsync<T>(bool write, Func<HotelTransaction, Task<T>> action, CancellationToken token = default)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(token);
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(IsolationLevel.Serializable, token);
        var store = new HotelTransaction(connection, transaction, token);
        try
        {
            await store.LockAsync(write);
            var result = await action(store);
            await transaction.CommitAsync(token);
            return result;
        }
        catch
        {
            try { await transaction.RollbackAsync(CancellationToken.None); } catch (SqlException) { } catch (InvalidOperationException) { }
            throw;
        }
    }
}
public sealed record Account(int Id, string Username, byte[] Hash, byte[] Salt, int Iterations, string Role, bool Active,
    int Failed, DateTime? LockedUntil, long SecurityVersion, string DisplayName, byte[]? AvatarPng);

public sealed partial class HotelTransaction(SqlConnection connection, SqlTransaction transaction, CancellationToken token)
{
    private static SqlParameter Param(string name, object? value)
    {
        var p = value switch
        {
            int => new SqlParameter(name, SqlDbType.Int), long => new SqlParameter(name, SqlDbType.BigInt),
            bool => new SqlParameter(name, SqlDbType.Bit),
            decimal => new SqlParameter(name, SqlDbType.Decimal) { Precision = 18, Scale = 3 },
            DateTime => new SqlParameter(name, SqlDbType.DateTime2),
            byte[] bytes => new SqlParameter(name, SqlDbType.VarBinary, bytes.Length>8000?-1:bytes.Length),
            string s when s.Length>500 => new SqlParameter(name, SqlDbType.NVarChar, -1),
            _ => new SqlParameter(name, SqlDbType.NVarChar, 500)
        };
        p.Value = value ?? DBNull.Value;
        return p;
    }
    private SqlCommand Command(string sql, object?[] args)
    {
        var cmd = new SqlCommand(sql, connection, transaction) { CommandTimeout = 15 };
        for (var i = 0; i < args.Length; i++) cmd.Parameters.Add(Param("@p" + i, args[i]));
        return cmd;
    }
    private async Task<int> Execute(string sql, params object?[] args)
    {
        await using var cmd = Command(sql, args);
        return await cmd.ExecuteNonQueryAsync(token);
    }
    private async Task<long> Scalar(string sql, params object?[] args)
    {
        await using var cmd = Command(sql, args);
        return Convert.ToInt64(await cmd.ExecuteScalarAsync(token));
    }
    private async Task<List<T>> Query<T>(string sql, Func<SqlDataReader, T> map, params object?[] args)
    {
        await using var cmd = Command(sql, args);
        await using var reader = await cmd.ExecuteReaderAsync(token);
        var rows = new List<T>();
        while (await reader.ReadAsync(token)) rows.Add(map(reader));
        return rows;
    }
    private static DateTime? Date(SqlDataReader r, int i) => r.IsDBNull(i) ? null : r.GetDateTime(i);
    internal Task<int> LockAsync(bool write) => Execute("DECLARE @r int; EXEC @r=sys.sp_getapplock @Resource=N'QLKhachSan.Write',@LockMode=@p0,@LockOwner='Transaction',@LockTimeout=10000; IF @r<0 THROW 51001,N'Hệ thống đang bận. Vui lòng thử lại.',1;", write ? "Exclusive" : "Shared");
    public async Task<DateTime> NowAsync() => (await Query("SELECT SYSDATETIME()", r => r.GetDateTime(0)))[0];
    public async Task RequireUserAsync(UserSession user, bool admin = false)
    {
        var count = await Scalar("SELECT COUNT_BIG(*) FROM dbo.TaiKhoanNhanVien WHERE Ma=@p0 AND TenDangNhap=@p1 AND DangHoatDong=1 AND DaLuuTru=0 AND (@p2=0 OR VaiTro='Admin') AND PhienBanBaoMat=@p3 AND VaiTro=@p4", user.Id, user.Username, admin, user.SecurityVersion, user.Role);
        if (count != 1) throw new BusinessException("Phiên đăng nhập không hợp lệ hoặc bạn không có quyền thực hiện.");
    }
    public Task<long> UserCountAsync() => Scalar("SELECT COUNT_BIG(*) FROM dbo.TaiKhoanNhanVien WHERE DaLuuTru=0");
    public async Task<Account?> AccountAsync(string username) => (await Query(
        "SELECT Ma,TenDangNhap,MatKhauBam,MuoiBam,SoLanBam,VaiTro,DangHoatDong,SoLanDangNhapSai,KhoaDen,PhienBanBaoMat,COALESCE(NULLIF(TenHienThi,N''),TenDangNhap),AnhDaiDien FROM dbo.TaiKhoanNhanVien WHERE TenDangNhap=@p0 AND DaLuuTru=0",
        r => new Account(r.GetInt32(0),r.GetString(1),(byte[])r[2],(byte[])r[3],r.GetInt32(4),r.GetString(5),r.GetBoolean(6),r.GetInt32(7),Date(r,8),r.GetInt64(9),r.GetString(10),r.IsDBNull(11)?null:(byte[])r[11]),username)).SingleOrDefault();
    public async Task<int> CreateUserAsync(string name, byte[] hash, byte[] salt, int iterations, string role)
        => checked((int)await Scalar("INSERT dbo.TaiKhoanNhanVien(TenDangNhap,MatKhauBam,MuoiBam,SoLanBam,VaiTro) OUTPUT INSERTED.Ma VALUES(@p0,@p1,@p2,@p3,@p4)",name,hash,salt,iterations,role));
    public Task<int> LoginResultAsync(int id, bool success) => Execute(success
        ? "UPDATE dbo.TaiKhoanNhanVien SET SoLanDangNhapSai=0,KhoaDen=NULL WHERE Ma=@p0"
        : "UPDATE dbo.TaiKhoanNhanVien SET SoLanDangNhapSai=CASE WHEN KhoaDen<=SYSDATETIME() THEN 1 ELSE SoLanDangNhapSai+1 END,KhoaDen=CASE WHEN KhoaDen<=SYSDATETIME() THEN NULL WHEN SoLanDangNhapSai>=4 THEN DATEADD(minute,5,SYSDATETIME()) ELSE NULL END WHERE Ma=@p0",id);
    public Task<int> ChangePasswordAsync(int id, byte[] hash, byte[] salt, int iterations) => Execute("UPDATE dbo.TaiKhoanNhanVien SET MatKhauBam=@p1,MuoiBam=@p2,SoLanBam=@p3,SoLanDangNhapSai=0,KhoaDen=NULL,PhienBanBaoMat=PhienBanBaoMat+1 WHERE Ma=@p0",id,hash,salt,iterations);
    public Task<int> AuditAsync(UserSession user, string action, string detail) => Execute("INSERT dbo.NhatKyThaoTac(MaNhanVien,HanhDong,ChiTiet) VALUES(@p0,@p1,@p2)",user.Id,action,detail);
    private static Room MapRoom(SqlDataReader r) => new(r.GetInt32(0),r.GetString(1),r.GetString(2),r.GetDecimal(3),r.GetDecimal(4),Enum.Parse<RoomStatus>(r.GetString(5)),r.GetInt64(6));
    public Task<List<Room>> RoomsAsync() => Query("SELECT Ma,SoPhong,Loai,DonGiaPhong,TienCoc,TrangThai,PhienBan FROM dbo.Phong ORDER BY SoPhong",MapRoom);
    public async Task<Room> RoomAsync(int id) => (await Query("SELECT Ma,SoPhong,Loai,DonGiaPhong,TienCoc,TrangThai,PhienBan FROM dbo.Phong WHERE Ma=@p0",MapRoom,id)).SingleOrDefault() ?? throw new BusinessException("Phòng không tồn tại.");
    private const string StayColumns = "Ma,MaPhong,MaKhachHang,TenKhach,SoDienThoai,SoGiayTo,TrangThai,ThoiDiemTao,ThoiDiemDen,ThoiDiemDi,ThoiDiemNhanPhong,HanGiuPhong,TienCoc,PhienBan";
    private static Stay MapStay(SqlDataReader r) => new(r.GetInt64(0),r.GetInt32(1),r.GetInt32(2),r.GetString(3),r.GetString(4),r.GetString(5),Enum.Parse<StayStatus>(r.GetString(6)),r.GetDateTime(7),r.GetDateTime(8),r.GetDateTime(9),Date(r,10),Date(r,11),r.GetDecimal(12),r.GetInt64(13));
    public Task<List<Stay>> ActiveStaysAsync() => Query("SELECT "+StayColumns+" FROM dbo.LuotLuuTru WHERE ConHieuLuc=1",MapStay);
    public async Task<Stay> StayAsync(long id) => (await Query("SELECT "+StayColumns+" FROM dbo.LuotLuuTru WHERE Ma=@p0",MapStay,id)).SingleOrDefault() ?? throw new BusinessException("Lượt lưu trú không tồn tại.");
    public Task<List<Segment>> SegmentsAsync(long stayId) => Query("SELECT Ma,MaLuotLuuTru,MaPhong,ThoiDiemBatDau,ThoiDiemKetThuc,DonGiaPhong FROM dbo.ChangLuuTru WHERE MaLuotLuuTru=@p0 ORDER BY ThoiDiemBatDau,Ma",
        r=>new Segment(r.GetInt64(0),r.GetInt64(1),r.GetInt32(2),r.GetDateTime(3),Date(r,4),r.GetDecimal(5)),stayId);
    public Task<List<ServiceItem>> MenuAsync() => Query("SELECT Ma,DanhMuc,Ten,DonGia,DonViTinh FROM dbo.DichVu WHERE DangHoatDong=1 ORDER BY DanhMuc,Ma",
        r=>new ServiceItem(r.GetInt32(0),r.GetString(1),r.GetString(2),r.GetDecimal(3),r.GetString(4)));
    private static ServiceLine MapLine(SqlDataReader r) => new(r.GetInt64(0),r.GetInt64(1),r.GetString(2),r.GetString(3),r.GetInt32(4),r.GetDecimal(5),r.GetDateTime(6),Date(r,7),r.GetInt32(8),Date(r,9));
    private const string OrderColumns="Ma,MaLuotLuuTru,DanhMuc,Ten,SoLuong,DonGia,ThoiDiemGoi,ThoiDiemGiao,SoLuongDaGiao,ThoiDiemHuy";
    public Task<List<ServiceLine>> OrdersAsync(long stayId) => Query("SELECT "+OrderColumns+" FROM dbo.YeuCauDichVu WHERE MaLuotLuuTru=@p0 AND ThoiDiemHuy IS NULL ORDER BY Ma",MapLine,stayId);
    public Task<List<ServiceLine>> AllOrdersAsync(long stayId) => Query("SELECT "+OrderColumns+" FROM dbo.YeuCauDichVu WHERE MaLuotLuuTru=@p0 ORDER BY Ma",MapLine,stayId);
    public Task<List<ServiceLine>> PendingAsync() => Query("SELECT "+OrderColumns+" FROM dbo.YeuCauDichVu WHERE ThoiDiemGiao IS NULL AND ThoiDiemHuy IS NULL ORDER BY ThoiDiemGoi",MapLine);
    public Task<List<RevenueItem>> RevenueAsync(DateTime day) => RevenueAsync(day.Date,day.Date.AddDays(1));
    public Task<List<RevenueItem>> RevenueAsync(DateTime from,DateTime until) => Query("SELECT DanhMuc,SUM(SoTien) FROM (SELECT N'Tiền phòng' DanhMuc,TienPhong SoTien FROM dbo.HoaDon i WHERE i.ThoiDiemLapHoaDon>=@p0 AND i.ThoiDiemLapHoaDon<@p1 AND NOT EXISTS(SELECT 1 FROM dbo.HoaDonHuy v WHERE v.MaHoaDon=i.Ma) UNION ALL SELECT o.DanhMuc,o.DonGia*o.SoLuong FROM dbo.YeuCauDichVu o JOIN dbo.HoaDon i ON i.MaLuotLuuTru=o.MaLuotLuuTru WHERE i.ThoiDiemLapHoaDon>=@p0 AND i.ThoiDiemLapHoaDon<@p1 AND o.ThoiDiemHuy IS NULL AND NOT EXISTS(SELECT 1 FROM dbo.HoaDonHuy v WHERE v.MaHoaDon=i.Ma) UNION ALL SELECT N'Cọc không hoàn (không đến)',SoTien FROM dbo.GiaoDichThanhToan WHERE LoaiGiaoDich='Forfeit' AND ThoiDiemTao>=@p0 AND ThoiDiemTao<@p1 UNION ALL SELECT N'Giảm trừ',-a.SoTien FROM dbo.DieuChinhHoaDon a JOIN dbo.HoaDon i ON i.Ma=a.MaHoaDon WHERE a.ThoiDiemLap>=@p0 AND a.ThoiDiemLap<@p1 AND NOT EXISTS(SELECT 1 FROM dbo.HoaDonHuy v WHERE v.MaHoaDon=i.Ma)) q GROUP BY DanhMuc",r=>new RevenueItem(r.GetString(0),r.GetDecimal(1)),from,until);
    public Task<List<CustomerSummary>> CustomersAsync(string search) => Query("SELECT TOP(200) c.Ma,c.Ten,c.SoDienThoai,c.SoGiayTo,COUNT(i.Ma),COALESCE(SUM(i.TienPhong+i.TienDichVu),0) FROM dbo.KhachHang c LEFT JOIN dbo.LuotLuuTru s ON s.MaKhachHang=c.Ma LEFT JOIN dbo.HoaDon i ON i.MaLuotLuuTru=s.Ma AND NOT EXISTS(SELECT 1 FROM dbo.HoaDonHuy v WHERE v.MaHoaDon=i.Ma) WHERE @p0=N'' OR CHARINDEX(@p0,c.Ten)>0 OR CHARINDEX(@p0,c.SoDienThoai)>0 OR CHARINDEX(@p0,c.SoGiayTo)>0 GROUP BY c.Ma,c.Ten,c.SoDienThoai,c.SoGiayTo ORDER BY c.Ma DESC",r=>new CustomerSummary(r.GetInt32(0),r.GetString(1),r.GetString(2),r.GetString(3),r.GetInt32(4),r.GetDecimal(5)),search);
    public Task<List<ServiceLine>> CustomerOrdersAsync(int customer) => Query("SELECT TOP(500) o.Ma,o.MaLuotLuuTru,o.DanhMuc,o.Ten,o.SoLuong,o.DonGia,o.ThoiDiemGoi,o.ThoiDiemGiao,o.SoLuongDaGiao,o.ThoiDiemHuy FROM dbo.YeuCauDichVu o JOIN dbo.LuotLuuTru s ON s.Ma=o.MaLuotLuuTru WHERE s.MaKhachHang=@p0 ORDER BY o.Ma DESC",MapLine,customer);
    public Task<List<Invoice>> InvoicesAsync(DateTime day) => InvoicesAsync(day.Date,day.Date.AddDays(1));
    public Task<List<Invoice>> InvoicesAsync(DateTime from,DateTime until) => Query("SELECT Ma,MaLuotLuuTru,SoPhongHoaDon,TenKhach,ThoiDiemLapHoaDon,TienPhong,TienDichVu,TienCoc,TienDaThu,TienDaHoan,PhuongThucThanhToan FROM dbo.HoaDon i WHERE ThoiDiemLapHoaDon>=@p0 AND ThoiDiemLapHoaDon<@p1 AND NOT EXISTS(SELECT 1 FROM dbo.HoaDonHuy v WHERE v.MaHoaDon=i.Ma) ORDER BY Ma DESC",r=>new Invoice(r.GetInt64(0),r.GetInt64(1),r.GetString(2),r.GetString(3),r.GetDateTime(4),r.GetDecimal(5),r.GetDecimal(6),r.GetDecimal(7),r.GetDecimal(8),r.GetDecimal(9),r.GetString(10)),from,until);
    public Task<List<Invoice>> CustomerInvoicesAsync(int customerId) => Query("SELECT TOP(200) i.Ma,i.MaLuotLuuTru,i.SoPhongHoaDon,i.TenKhach,i.ThoiDiemLapHoaDon,i.TienPhong,i.TienDichVu,i.TienCoc,i.TienDaThu,i.TienDaHoan,i.PhuongThucThanhToan FROM dbo.HoaDon i JOIN dbo.LuotLuuTru s ON s.Ma=i.MaLuotLuuTru WHERE s.MaKhachHang=@p0 AND NOT EXISTS(SELECT 1 FROM dbo.HoaDonHuy v WHERE v.MaHoaDon=i.Ma) ORDER BY i.Ma DESC",r=>new Invoice(r.GetInt64(0),r.GetInt64(1),r.GetString(2),r.GetString(3),r.GetDateTime(4),r.GetDecimal(5),r.GetDecimal(6),r.GetDecimal(7),r.GetDecimal(8),r.GetDecimal(9),r.GetString(10)),customerId);
}
