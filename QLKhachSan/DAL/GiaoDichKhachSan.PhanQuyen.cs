using QLKhachSan.BLL;
using System.Text.Json;

namespace QLKhachSan.DAL;

public sealed partial class HotelTransaction
{
    public Task<HashSet<string>> MenuFunctionCodesAsync()
    {
        return Task.FromResult(FunctionPolicy.All.Select(x => x.Code).ToHashSet(StringComparer.Ordinal));
    }

    public async Task<HashSet<string>> UserFunctionsAsync(int userId)
    {
        var rows=await Query("SELECT VaiTro,DaTuyChinhQuyen,QuyenChucNang FROM dbo.TaiKhoanNhanVien WHERE Ma=@p0 AND DaLuuTru=0",
            r=>(Role:r.GetString(0),Customized:r.GetBoolean(1),Codes:r.IsDBNull(2)?null:r.GetString(2)),userId);
        var account=rows.SingleOrDefault();
        if(account.Role is null)throw new InvalidOperationException("Tài khoản không tồn tại.");
        if(!account.Customized)return FunctionPolicy.Defaults(account.Role);
        var codes=JsonSerializer.Deserialize<string[]>(account.Codes??"[]")??[];
        return codes.Where(code=>FunctionPolicy.RoleAllows(account.Role,code)).ToHashSet(StringComparer.Ordinal);
    }

    public async Task SaveUserFunctionsAsync(int userId, IEnumerable<string> codes)
    {
        var json=JsonSerializer.Serialize(codes.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal));
        if(await Execute("UPDATE dbo.TaiKhoanNhanVien SET QuyenChucNang=@p1,DaTuyChinhQuyen=1,PhienBanBaoMat=PhienBanBaoMat+1 WHERE Ma=@p0 AND DaLuuTru=0",userId,json)!=1)
            throw new InvalidOperationException("Không lưu được quyền nhân viên.");
    }
}
