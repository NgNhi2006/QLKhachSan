using QLKhachSan.DTO;

namespace QLKhachSan.DAL;

public sealed partial class HotelTransaction
{
    public async Task<MenuConfiguration> MenuConfigurationAsync() => new(
        await Query("SELECT MaMenu,TieuDe,MoTa,BieuTuong,MauSac,ThuTuHienThi,DangHienThi,CoSan,IconPng FROM dbo.CauHinhMenu ORDER BY ThuTuHienThi,MaMenu",
            r=>new ConfiguredMenu(r.GetString(0),r.GetString(1),r.GetString(2),r.GetString(3),r.GetString(4),r.GetInt32(5),r.GetBoolean(6),r.GetBoolean(7),r.IsDBNull(8)?null:(byte[])r.GetValue(8))),
        await Query("SELECT Ma,MaMenu,TieuDe,ThuTuHienThi,DangHienThi,CoSan FROM dbo.CauHinhMenuCon ORDER BY ThuTuHienThi,Ma",
            r=>new ConfiguredSubmenu(r.GetInt64(0),r.GetString(1),r.GetString(2),r.GetInt32(3),r.GetBoolean(4),r.GetBoolean(5))),
        await Query("SELECT Ma,MaMenuCon,TieuDe,MaThaoTac,ThuTuHienThi,DangHienThi,CoSan FROM dbo.CauHinhChucNang ORDER BY ThuTuHienThi,Ma",
            r=>new ConfiguredFunction(r.GetInt64(0),r.GetInt64(1),r.GetString(2),r.GetString(3),r.GetInt32(4),r.GetBoolean(5),r.GetBoolean(6))),
        await Query("SELECT Ma,MaMenuCon,TieuDe,ThuTuHienThi,DangHienThi,VaiTro,CauTruc FROM dbo.CauHinhChucNangMoi ORDER BY ThuTuHienThi,Ma",
            r=>new ConfiguredCustomFunction(r.GetInt64(0),r.GetInt64(1),r.GetString(2),r.GetInt32(3),r.GetBoolean(4),r.GetString(5),r.GetString(6))));

    public Task<int> SaveConfiguredMenuAsync(ConfiguredMenu item) => Execute("""
        MERGE dbo.CauHinhMenu AS target USING (SELECT @p0 AS MaMenu) AS source ON target.MaMenu=source.MaMenu
        WHEN MATCHED THEN UPDATE SET TieuDe=@p1,MoTa=@p2,BieuTuong=@p3,MauSac=@p4,ThuTuHienThi=@p5,DangHienThi=@p6,IconPng=CONVERT(varbinary(max),@p7)
        WHEN NOT MATCHED THEN INSERT(MaMenu,TieuDe,MoTa,BieuTuong,MauSac,ThuTuHienThi,DangHienThi,IconPng) VALUES(@p0,@p1,@p2,@p3,@p4,@p5,@p6,CONVERT(varbinary(max),@p7));
        """,item.Code,item.Title,item.Description,item.Icon,item.Color,item.Order,item.Active,(object?)item.IconPng??DBNull.Value);
    public async Task<int> RenameConfiguredMenuAsync(string oldCode,ConfiguredMenu item) => (int)await Scalar("""
        SET NOCOUNT ON;
        IF NOT EXISTS(SELECT 1 FROM dbo.CauHinhMenu WHERE MaMenu=@p0 AND CoSan=0)
           OR EXISTS(SELECT 1 FROM dbo.CauHinhMenu WHERE MaMenu=@p1)
        BEGIN SELECT CAST(0 AS bigint); RETURN; END;
        INSERT dbo.CauHinhMenu(MaMenu,TieuDe,MoTa,BieuTuong,MauSac,ThuTuHienThi,DangHienThi,IconPng,CoSan)
        VALUES(@p1,@p2,@p3,@p4,@p5,@p6,@p7,CONVERT(varbinary(max),@p8),0);
        UPDATE dbo.CauHinhMenuCon SET MaMenu=@p1 WHERE MaMenu=@p0;
        DELETE FROM dbo.CauHinhMenu WHERE MaMenu=@p0 AND CoSan=0;
        SELECT CAST(@@ROWCOUNT AS bigint);
        """,oldCode,item.Code,item.Title,item.Description,item.Icon,item.Color,item.Order,item.Active,
        (object?)item.IconPng??DBNull.Value);
    public Task<int> SaveConfiguredSubmenuAsync(ConfiguredSubmenu item) => item.Id==0
        ? Execute("INSERT dbo.CauHinhMenuCon(MaMenu,TieuDe,ThuTuHienThi,DangHienThi) VALUES(@p0,@p1,@p2,@p3)",item.MenuCode,item.Title,item.Order,item.Active)
        : Execute("UPDATE dbo.CauHinhMenuCon SET MaMenu=@p1,TieuDe=@p2,ThuTuHienThi=@p3,DangHienThi=@p4 WHERE Ma=@p0",item.Id,item.MenuCode,item.Title,item.Order,item.Active);
    public Task<int> SaveConfiguredFunctionAsync(ConfiguredFunction item) => item.Id==0
        ? Execute("INSERT dbo.CauHinhChucNang(MaMenuCon,TieuDe,MaThaoTac,ThuTuHienThi,DangHienThi) VALUES(@p0,@p1,@p2,@p3,@p4)",item.SubmenuId,item.Title,item.ActionCode,item.Order,item.Active)
        : Execute("UPDATE dbo.CauHinhChucNang SET MaMenuCon=@p1,TieuDe=@p2,MaThaoTac=@p3,ThuTuHienThi=@p4,DangHienThi=@p5 WHERE Ma=@p0",item.Id,item.SubmenuId,item.Title,item.ActionCode,item.Order,item.Active);
    public Task<int> DeleteConfiguredFunctionAsync(long id) => Execute("DELETE FROM dbo.CauHinhChucNang WHERE Ma=@p0 AND CoSan=0",id);
    public Task<int> SaveCustomFunctionAsync(ConfiguredCustomFunction item) => item.Id==0
        ? Execute("INSERT dbo.CauHinhChucNangMoi(MaMenuCon,TieuDe,ThuTuHienThi,DangHienThi,VaiTro,CauTruc) VALUES(@p0,@p1,@p2,@p3,@p4,@p5)",item.SubmenuId,item.Title,item.Order,item.Active,item.AllowedRoles,item.DesignJson)
        : Execute("UPDATE dbo.CauHinhChucNangMoi SET MaMenuCon=@p1,TieuDe=@p2,ThuTuHienThi=@p3,DangHienThi=@p4,VaiTro=@p5,CauTruc=@p6 WHERE Ma=@p0",item.Id,item.SubmenuId,item.Title,item.Order,item.Active,item.AllowedRoles,item.DesignJson);
    public Task<int> DeleteCustomFunctionAsync(long id) => Execute("DELETE FROM dbo.CauHinhChucNangMoi WHERE Ma=@p0 AND NOT EXISTS(SELECT 1 FROM dbo.DuLieuChucNangMoi WHERE MaChucNang=@p0)",id);
    public Task<List<CustomScreenRecord>> CustomRecordsAsync(long functionId) => Query(
        "SELECT Ma,NoiDung,CapNhatLuc FROM dbo.DuLieuChucNangMoi WHERE MaChucNang=@p0 ORDER BY Ma DESC",
        r=>new CustomScreenRecord(r.GetInt64(0),r.GetString(1),r.GetDateTime(2)),functionId);
    public async Task<ConfiguredCustomFunction?> CustomFunctionAsync(long id) => (await Query(
        "SELECT Ma,MaMenuCon,TieuDe,ThuTuHienThi,DangHienThi,VaiTro,CauTruc FROM dbo.CauHinhChucNangMoi WHERE Ma=@p0",
        r=>new ConfiguredCustomFunction(r.GetInt64(0),r.GetInt64(1),r.GetString(2),r.GetInt32(3),r.GetBoolean(4),r.GetString(5),r.GetString(6)),id)).SingleOrDefault();
    public Task<int> SaveCustomRecordAsync(long functionId,long recordId,string valuesJson) => recordId==0
        ? Execute("INSERT dbo.DuLieuChucNangMoi(MaChucNang,NoiDung) VALUES(@p0,@p1)",functionId,valuesJson)
        : Execute("UPDATE dbo.DuLieuChucNangMoi SET NoiDung=@p2,CapNhatLuc=SYSUTCDATETIME() WHERE Ma=@p0 AND MaChucNang=@p1",recordId,functionId,valuesJson);
    public Task<int> DeleteCustomRecordAsync(long functionId,long recordId) => Execute(
        "DELETE FROM dbo.DuLieuChucNangMoi WHERE Ma=@p0 AND MaChucNang=@p1",recordId,functionId);
    public async Task<int> DeleteConfiguredSubmenuAsync(long id) => (int)await Scalar("""
        SET NOCOUNT ON;
        IF NOT EXISTS(SELECT 1 FROM dbo.CauHinhMenuCon WHERE Ma=@p0 AND CoSan=0)
           OR EXISTS(SELECT 1 FROM dbo.CauHinhChucNang WHERE MaMenuCon=@p0 AND CoSan=1)
           OR EXISTS(SELECT 1 FROM dbo.DuLieuChucNangMoi d JOIN dbo.CauHinhChucNangMoi c ON c.Ma=d.MaChucNang WHERE c.MaMenuCon=@p0)
        BEGIN SELECT CAST(0 AS bigint); RETURN; END;
        DELETE FROM dbo.CauHinhChucNang WHERE MaMenuCon=@p0;
        DELETE FROM dbo.CauHinhChucNangMoi WHERE MaMenuCon=@p0;
        DELETE FROM dbo.CauHinhMenuCon WHERE Ma=@p0 AND CoSan=0;
        SELECT CAST(@@ROWCOUNT AS bigint);
        """,id);
    public async Task<int> DeleteConfiguredMenuAsync(string code) => (int)await Scalar("""
        SET NOCOUNT ON;
        IF NOT EXISTS(SELECT 1 FROM dbo.CauHinhMenu WHERE MaMenu=@p0 AND CoSan=0)
           OR EXISTS(SELECT 1 FROM dbo.CauHinhMenuCon WHERE MaMenu=@p0 AND CoSan=1)
           OR EXISTS(SELECT 1 FROM dbo.CauHinhChucNang f JOIN dbo.CauHinhMenuCon s ON s.Ma=f.MaMenuCon WHERE s.MaMenu=@p0 AND f.CoSan=1)
           OR EXISTS(SELECT 1 FROM dbo.DuLieuChucNangMoi d JOIN dbo.CauHinhChucNangMoi c ON c.Ma=d.MaChucNang
                     JOIN dbo.CauHinhMenuCon s ON s.Ma=c.MaMenuCon WHERE s.MaMenu=@p0)
        BEGIN SELECT CAST(0 AS bigint); RETURN; END;
        DELETE f FROM dbo.CauHinhChucNang f JOIN dbo.CauHinhMenuCon s ON s.Ma=f.MaMenuCon WHERE s.MaMenu=@p0;
        DELETE c FROM dbo.CauHinhChucNangMoi c JOIN dbo.CauHinhMenuCon s ON s.Ma=c.MaMenuCon WHERE s.MaMenu=@p0;
        DELETE FROM dbo.CauHinhMenuCon WHERE MaMenu=@p0;
        DELETE FROM dbo.CauHinhMenu WHERE MaMenu=@p0 AND CoSan=0;
        SELECT CAST(@@ROWCOUNT AS bigint);
        """,code);
}
