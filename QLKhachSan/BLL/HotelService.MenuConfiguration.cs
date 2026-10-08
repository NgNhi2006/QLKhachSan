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
        item=ValidateConfiguredMenu(item);
        return SaveMenuConfigurationAsync(db=>db.SaveConfiguredMenuAsync(item),$"Menu {item.Code}: {item.Title}");
    }
    public Task RenameConfiguredMenuAsync(string oldCode,ConfiguredMenu item)
    {
        item=ValidateConfiguredMenu(item);
        return SaveMenuConfigurationAsync(db=>db.RenameConfiguredMenuAsync(oldCode,item),$"Đổi mã menu {oldCode} → {item.Code}");
    }
    private static ConfiguredMenu ValidateConfiguredMenu(ConfiguredMenu item)
    {
        item=item with {Code=item.Code.Trim().ToLowerInvariant(),Title=item.Title.Trim(),Description=item.Description.Trim()};
        if(!Regex.IsMatch(item.Code,@"^[a-z0-9][a-z0-9_-]{0,39}$"))
            throw new BusinessException("Mã menu cần 1–40 ký tự: chữ thường, số, _ hoặc -. Không dùng dấu cách.");
        if(item.Title.Length is <1 or >100)throw new BusinessException("Tên menu cần từ 1 đến 100 ký tự.");
        if(item.Description.Length>180)throw new BusinessException("Mô tả menu không được quá 180 ký tự.");
        if(item.Order is <0 or >9999)throw new BusinessException("Thứ tự menu cần từ 0 đến 9999.");
        if(!Regex.IsMatch(item.Color,@"^#[0-9A-Fa-f]{6}$"))
            throw new BusinessException("Màu menu phải có dạng #RRGGBB, ví dụ #3E699D.");
        if(item.Icon is not ("bed" or "service" or "people" or "clock" or "money" or "receipt" or "chart" or "key" or "tools" or "calendar" or "search" or "swap" or "refresh" or "building" or "door" or "bath" or "food" or "coffee" or "wifi" or "car" or "phone" or "bell" or "bag" or "star" or "heart" or "shield" or "gear" or "folder"))
            throw new BusinessException("Biểu tượng menu không được hỗ trợ.");
        if(item.IconPng is {Length: > 1048576} ||
           item.IconPng is { } png && (png.Length<8 || !png.Take(8).SequenceEqual(new byte[]{137,80,78,71,13,10,26,10})))
            throw new BusinessException("Ảnh biểu tượng phải là PNG và không vượt quá 1 MB.");
        return item;
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
    public Task DeleteConfiguredSubmenuAsync(long id) => DeleteMenuTreeAsync(db=>db.DeleteConfiguredSubmenuAsync(id),$"Xóa menu con {id}");
    public Task DeleteConfiguredMenuAsync(string code) => DeleteMenuTreeAsync(db=>db.DeleteConfiguredMenuAsync(code),$"Xóa menu {code}");
    private Task DeleteMenuTreeAsync(Func<QLKhachSan.DAL.HotelTransaction,Task<int>> delete,string audit) =>
        repository.RunAsync(true,async db=>
        {
            await db.RequireUserAsync(user,true);
            FunctionPolicy.Require(user,"staff.manage");
            if(await delete(db)!=1)
                throw new BusinessException("Không thể xóa: menu chứa mục có sẵn hoặc chức năng mới đã có bản ghi. Hãy chuyển mục đó sang menu khác hoặc xóa bản ghi trước.");
            await db.AuditAsync(user,"MenuConfiguration",audit);
            return 0;
        });
}
