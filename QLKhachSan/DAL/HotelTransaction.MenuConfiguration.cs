using QLKhachSan.DTO;

namespace QLKhachSan.DAL;

public sealed partial class HotelTransaction
{
    public async Task<MenuConfiguration> MenuConfigurationAsync() => new(
        await Query("SELECT MaMenu,TieuDe,MoTa,BieuTuong,MauSac,ThuTuHienThi,DangHienThi,CoSan FROM dbo.CauHinhMenu ORDER BY ThuTuHienThi,MaMenu",
            r=>new ConfiguredMenu(r.GetString(0),r.GetString(1),r.GetString(2),r.GetString(3),r.GetString(4),r.GetInt32(5),r.GetBoolean(6),r.GetBoolean(7))),
        await Query("SELECT Ma,MaMenu,TieuDe,ThuTuHienThi,DangHienThi,CoSan FROM dbo.CauHinhMenuCon ORDER BY ThuTuHienThi,Ma",
            r=>new ConfiguredSubmenu(r.GetInt64(0),r.GetString(1),r.GetString(2),r.GetInt32(3),r.GetBoolean(4),r.GetBoolean(5))),
        await Query("SELECT Ma,MaMenuCon,TieuDe,MaThaoTac,ThuTuHienThi,DangHienThi,CoSan FROM dbo.CauHinhChucNang ORDER BY ThuTuHienThi,Ma",
            r=>new ConfiguredFunction(r.GetInt64(0),r.GetInt64(1),r.GetString(2),r.GetString(3),r.GetInt32(4),r.GetBoolean(5),r.GetBoolean(6))));

    public Task<int> SaveConfiguredMenuAsync(ConfiguredMenu item) => Execute("""
        MERGE dbo.CauHinhMenu AS target USING (SELECT @p0 AS MaMenu) AS source ON target.MaMenu=source.MaMenu
        WHEN MATCHED THEN UPDATE SET TieuDe=@p1,MoTa=@p2,BieuTuong=@p3,MauSac=@p4,ThuTuHienThi=@p5,DangHienThi=@p6
        WHEN NOT MATCHED THEN INSERT(MaMenu,TieuDe,MoTa,BieuTuong,MauSac,ThuTuHienThi,DangHienThi) VALUES(@p0,@p1,@p2,@p3,@p4,@p5,@p6);
        """,item.Code,item.Title,item.Description,item.Icon,item.Color,item.Order,item.Active);
    public Task<int> SaveConfiguredSubmenuAsync(ConfiguredSubmenu item) => item.Id==0
        ? Execute("INSERT dbo.CauHinhMenuCon(MaMenu,TieuDe,ThuTuHienThi,DangHienThi) VALUES(@p0,@p1,@p2,@p3)",item.MenuCode,item.Title,item.Order,item.Active)
        : Execute("UPDATE dbo.CauHinhMenuCon SET MaMenu=@p1,TieuDe=@p2,ThuTuHienThi=@p3,DangHienThi=@p4 WHERE Ma=@p0",item.Id,item.MenuCode,item.Title,item.Order,item.Active);
    public Task<int> SaveConfiguredFunctionAsync(ConfiguredFunction item) => item.Id==0
        ? Execute("INSERT dbo.CauHinhChucNang(MaMenuCon,TieuDe,MaThaoTac,ThuTuHienThi,DangHienThi) VALUES(@p0,@p1,@p2,@p3,@p4)",item.SubmenuId,item.Title,item.ActionCode,item.Order,item.Active)
        : Execute("UPDATE dbo.CauHinhChucNang SET MaMenuCon=@p1,TieuDe=@p2,MaThaoTac=@p3,ThuTuHienThi=@p4,DangHienThi=@p5 WHERE Ma=@p0",item.Id,item.SubmenuId,item.Title,item.ActionCode,item.Order,item.Active);
    public Task<int> DeleteConfiguredFunctionAsync(long id) => Execute("DELETE FROM dbo.CauHinhChucNang WHERE Ma=@p0 AND CoSan=0",id);
    public Task<int> DeleteConfiguredSubmenuAsync(long id) => Execute("DELETE FROM dbo.CauHinhMenuCon WHERE Ma=@p0 AND CoSan=0 AND NOT EXISTS(SELECT 1 FROM dbo.CauHinhChucNang WHERE MaMenuCon=@p0)",id);
    public Task<int> DeleteConfiguredMenuAsync(string code) => Execute("DELETE FROM dbo.CauHinhMenu WHERE MaMenu=@p0 AND CoSan=0 AND NOT EXISTS(SELECT 1 FROM dbo.CauHinhMenuCon WHERE MaMenu=@p0)",code);
}
