namespace QLKhachSan.DTO;

public sealed record ConfiguredMenu(string Code,string Title,string Description,string Icon,string Color,int Order,bool Active,bool BuiltIn,byte[]? IconPng=null);
public sealed record ConfiguredSubmenu(long Id,string MenuCode,string Title,int Order,bool Active,bool BuiltIn);
public sealed record ConfiguredFunction(long Id,long SubmenuId,string Title,string ActionCode,int Order,bool Active,bool BuiltIn);
public sealed record CustomFieldDefinition(string Key,string Label,string Kind,bool Required,bool Wide,string Options);
public sealed record CustomScreenAction(string Label,string Kind);
public sealed record CustomScreenDesign(List<CustomFieldDefinition> Fields,string SaveLabel,string NewLabel,string DeleteLabel,
    List<CustomScreenAction>? Actions=null);
public sealed record ConfiguredCustomFunction(long Id,long SubmenuId,string Title,int Order,bool Active,string AllowedRoles,string DesignJson);
public sealed record CustomScreenRecord(long Id,string ValuesJson,DateTime UpdatedAt);
public sealed record MenuConfiguration(List<ConfiguredMenu> Menus,List<ConfiguredSubmenu> Submenus,List<ConfiguredFunction> Functions,
    List<ConfiguredCustomFunction> CustomFunctions);
