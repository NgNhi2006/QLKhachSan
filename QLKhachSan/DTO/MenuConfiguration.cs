namespace QLKhachSan.DTO;

public sealed record ConfiguredMenu(string Code,string Title,string Description,string Icon,string Color,int Order,bool Active,bool BuiltIn);
public sealed record ConfiguredSubmenu(long Id,string MenuCode,string Title,int Order,bool Active,bool BuiltIn);
public sealed record ConfiguredFunction(long Id,long SubmenuId,string Title,string ActionCode,int Order,bool Active,bool BuiltIn);
public sealed record MenuConfiguration(List<ConfiguredMenu> Menus,List<ConfiguredSubmenu> Submenus,List<ConfiguredFunction> Functions);
