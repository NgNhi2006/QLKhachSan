namespace QLKhachSan.DAL;

public sealed partial class HotelTransaction
{
    public async Task<HashSet<string>> MenuFunctionCodesAsync()
    {
        var codes = await Query("""
            SELECT MaChucNang FROM dbo.ChucNangQuanLyPhong
            UNION ALL SELECT MaChucNang FROM dbo.ChucNangDichVu
            UNION ALL SELECT MaChucNang FROM dbo.ChucNangKhachHang
            UNION ALL SELECT MaChucNang FROM dbo.ChucNangCaTruc
            UNION ALL SELECT MaChucNang FROM dbo.ChucNangThuChi
            UNION ALL SELECT MaChucNang FROM dbo.ChucNangHoaDon
            UNION ALL SELECT MaChucNang FROM dbo.ChucNangBaoCao
            UNION ALL SELECT MaChucNang FROM dbo.ChucNangNhanVien
            UNION ALL SELECT MaChucNang FROM dbo.ChucNangHeThong
            """, r => r.GetString(0));
        return codes.ToHashSet(StringComparer.Ordinal);
    }

    public async Task<HashSet<string>> UserFunctionsAsync(int userId)
    {
        var available = await MenuFunctionCodesAsync();
        if (await Scalar("SELECT CONVERT(bigint,DaTuyChinhQuyen) FROM dbo.TaiKhoanNhanVien WHERE Ma=@p0", userId) == 0)
            return available;
        var codes = await Query("SELECT MaChucNang FROM dbo.PhanQuyenNhanVien WHERE MaNhanVien=@p0",
            r => r.GetString(0), userId);
        return codes.Where(available.Contains).ToHashSet(StringComparer.Ordinal);
    }

    public async Task SaveUserFunctionsAsync(int userId, IEnumerable<string> codes)
    {
        await Execute("DELETE FROM dbo.PhanQuyenNhanVien WHERE MaNhanVien=@p0", userId);
        foreach (var code in codes)
            await Execute("INSERT dbo.PhanQuyenNhanVien(MaNhanVien,MaChucNang) VALUES(@p0,@p1)", userId, code);
        await Execute("UPDATE dbo.TaiKhoanNhanVien SET DaTuyChinhQuyen=1,PhienBanBaoMat=PhienBanBaoMat+1 WHERE Ma=@p0", userId);
    }
}
