using System.Text.RegularExpressions;
using QLKhachSan.DTO;

namespace QLKhachSan.BLL;

public sealed partial class HotelService
{
    public Task<MenuConfiguration> MenuConfigurationAsync() => repository.RunAsync(false,async db=>
    {
        await db.RequireUserAsync(user);
        return await db.MenuConfigurationAsync();
    });

    private Task SaveMenuConfigurationAsync(Func<QLKhachSan.DAL.HotelTransaction,Task<int>> save,string audit) =>
        repository.RunAsync(true,async db=>
        {
            await db.RequireUserAsync(user,true);
            FunctionPolicy.Require(user,"staff.manage");
            if(await save(db)!=1)throw new BusinessException("Không lưu được cấu hình menu. Hãy mở lại danh sách.");
            return await db.AuditAsync(user,"MenuConfiguration",audit);
        });

    public Task SaveConfiguredMenuAsync(ConfiguredMenu item)
    {
        item=item with {Code=item.Code.Trim().ToLowerInvariant(),Title=item.Title.Trim(),Description=item.Description.Trim()};
        if(!Regex.IsMatch(item.Code,@"^[a-z][a-z0-9_-]{2,39}$") || item.Title.Length is <2 or >100 ||
           item.Description.Length>180 || item.Order is <0 or >9999 ||
           !Regex.IsMatch(item.Color,@"^#[0-9A-Fa-f]{6}$") ||
           item.Icon is not ("bed" or "service" or "people" or "clock" or "money" or "receipt" or "chart" or "key" or "tools" or "calendar"))
            throw new BusinessException("Mã, tên, màu hoặc biểu tượng menu không hợp lệ.");
        return SaveMenuConfigurationAsync(db=>db.SaveConfiguredMenuAsync(item),$"Menu {item.Code}: {item.Title}");
    }
    public Task SaveConfiguredSubmenuAsync(ConfiguredSubmenu item)
    {
        item=item with {Title=item.Title.Trim()};
        if(item.Title.Length is <2 or >100 || item.Order is <0 or >9999)throw new BusinessException("Tên hoặc thứ tự menu con không hợp lệ.");
        return SaveMenuConfigurationAsync(db=>db.SaveConfiguredSubmenuAsync(item),$"Menu con {item.MenuCode}: {item.Title}");
    }
    public Task SaveConfiguredFunctionAsync(ConfiguredFunction item)
    {
        item=item with {Title=item.Title.Trim()};
        if(item.Title.Length is <2 or >120 || item.Order is <0 or >9999 || !FunctionPolicy.All.Any(x=>x.Code==item.ActionCode))
            throw new BusinessException("Tên, thứ tự hoặc nghiệp vụ liên kết không hợp lệ.");
        return SaveMenuConfigurationAsync(db=>db.SaveConfiguredFunctionAsync(item),$"Chức năng {item.Title} → {item.ActionCode}");
    }
    public Task DeleteConfiguredFunctionAsync(long id) => SaveMenuConfigurationAsync(db=>db.DeleteConfiguredFunctionAsync(id),$"Xóa chức năng menu {id}");
    public Task DeleteConfiguredSubmenuAsync(long id) => SaveMenuConfigurationAsync(db=>db.DeleteConfiguredSubmenuAsync(id),$"Xóa menu con {id}");
    public Task DeleteConfiguredMenuAsync(string code) => SaveMenuConfigurationAsync(db=>db.DeleteConfiguredMenuAsync(code),$"Xóa menu {code}");
}
