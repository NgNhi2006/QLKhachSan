using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using QLKhachSan.DTO;

namespace QLKhachSan.BLL;

public sealed partial class HotelService
{
    public static CustomScreenDesign ParseCustomDesign(string json)
    {
        CustomScreenDesign? design;
        try{design=JsonSerializer.Deserialize<CustomScreenDesign>(json);}catch(JsonException){design=null;}
        if(design?.Fields is not {Count: >=1 and <=60})throw new BusinessException("Màn hình cần từ 1 đến 60 thành phần.");
        if(design.Fields.Any(f=>!Regex.IsMatch(f.Key,@"^f[0-9]{1,3}$") || f.Label.Trim().Length is <1 or >80 ||
            f.Kind is not ("text" or "multiline" or "number" or "integer" or "currency" or "percent" or
                "date" or "time" or "check" or "toggle" or "choice" or "radio" or "email" or "phone" or "url" or "heading" or "separator") ||
            f.Options.Length>500 || f.Kind is "choice" or "radio" &&
                f.Options.Split(';',StringSplitOptions.RemoveEmptyEntries|StringSplitOptions.TrimEntries).Length is <1 or >20) ||
            design.Fields.Select(f=>f.Key).Distinct(StringComparer.Ordinal).Count()!=design.Fields.Count)
            throw new BusinessException("Các ô dữ liệu có tên, loại hoặc lựa chọn không hợp lệ.");
        if(new[]{design.SaveLabel,design.NewLabel,design.DeleteLabel}.Any(x=>x.Trim().Length is <1 or >40))
            throw new BusinessException("Tên nút thao tác cần từ 1 đến 40 ký tự.");
        if(design.Actions is {Count: >20} || design.Actions?.Any(x=>x.Label.Trim().Length is <1 or >40 ||
            x.Kind is not ("save" or "new" or "delete" or "duplicate" or "refresh" or "search" or
                "export" or "print" or "clear" or "rental_calc"))==true)
            throw new BusinessException("Danh sách nút hành động không hợp lệ.");
        return design;
    }
    private static bool CustomRoleAllowed(UserSession user,ConfiguredCustomFunction item) =>
        item.AllowedRoles.Split(',',StringSplitOptions.RemoveEmptyEntries|StringSplitOptions.TrimEntries).Contains(user.Role);
    private async Task<ConfiguredCustomFunction> RequireCustomFunctionAsync(QLKhachSan.DAL.HotelTransaction db,long id)
    {
        var item=await db.CustomFunctionAsync(id)??throw new BusinessException("Chức năng mới không còn tồn tại.");
        if(!item.Active || !CustomRoleAllowed(user,item))throw new BusinessException("Bạn không có quyền sử dụng chức năng này.");
        return item;
    }
    public Task SaveCustomFunctionAsync(ConfiguredCustomFunction item)
    {
        item=item with {Title=item.Title.Trim()};
        if(item.Title.Length is <1 or >120 || item.Order is <0 or >9999)
            throw new BusinessException("Tên hoặc thứ tự chức năng không hợp lệ.");
        var roles=item.AllowedRoles.Split(',',StringSplitOptions.RemoveEmptyEntries|StringSplitOptions.TrimEntries);
        if(roles.Length==0 || roles.Distinct().Count()!=roles.Length || roles.Any(x=>x is not ("Admin" or "Manager" or "Reception" or "Accountant")))
            throw new BusinessException("Chọn ít nhất một vai trò được dùng chức năng.");
        ParseCustomDesign(item.DesignJson);
        return SaveMenuConfigurationAsync(db=>db.SaveCustomFunctionAsync(item),$"Chức năng mới {item.Title}");
    }
    public Task DeleteCustomFunctionAsync(long id) => repository.RunAsync(true,async db=>
    {
        await db.RequireUserAsync(user,true);
        FunctionPolicy.Require(user,"staff.manage");
        if(await db.DeleteCustomFunctionAsync(id)!=1)
            throw new BusinessException("Chức năng đã có bản ghi. Hãy xóa các bản ghi trong màn hình chức năng trước.");
        await db.AuditAsync(user,"MenuConfiguration",$"Xóa chức năng mới {id}");
        return 0;
    });
    public Task<List<CustomScreenRecord>> CustomRecordsAsync(long functionId) => repository.RunAsync(false,async db=>
    {
        await db.RequireUserAsync(user);
        await RequireCustomFunctionAsync(db,functionId);
        return await db.CustomRecordsAsync(functionId);
    });
    public Task SaveCustomRecordAsync(long functionId,long recordId,string valuesJson) => repository.RunAsync(true,async db=>
    {
        await db.RequireUserAsync(user,true);
        var item=await RequireCustomFunctionAsync(db,functionId);
        var design=ParseCustomDesign(item.DesignJson);
        Dictionary<string,string>? values;
        try{values=JsonSerializer.Deserialize<Dictionary<string,string>>(valuesJson);}catch(JsonException){values=null;}
        if(values is null || values.Keys.Except(design.Fields.Select(f=>f.Key)).Any())throw new BusinessException("Dữ liệu chức năng không hợp lệ.");
        foreach(var field in design.Fields)
        {
            values.TryGetValue(field.Key,out var value);value??="";
            if(value.Length>2000 || field.Required && string.IsNullOrWhiteSpace(value) &&
                field.Kind is not ("check" or "toggle" or "heading" or "separator"))
                throw new BusinessException($"Ô {field.Label} cần giá trị hợp lệ.");
            if(value.Length==0)continue;
            if(field.Kind is "number" or "currency" or "percent" &&
                !decimal.TryParse(value,NumberStyles.Number,CultureInfo.InvariantCulture,out _))
                throw new BusinessException($"Ô {field.Label} phải là số.");
            if(field.Kind=="integer" && !long.TryParse(value,NumberStyles.Integer,CultureInfo.InvariantCulture,out _))
                throw new BusinessException($"Ô {field.Label} phải là số nguyên.");
            if(field.Kind=="percent" && decimal.TryParse(value,NumberStyles.Number,CultureInfo.InvariantCulture,out var percent) &&
                percent is <0 or >100)throw new BusinessException($"Ô {field.Label} phải từ 0 đến 100.");
            if(field.Kind=="date" && !DateTime.TryParseExact(value,"yyyy-MM-dd",CultureInfo.InvariantCulture,DateTimeStyles.None,out _))
                throw new BusinessException($"Ô {field.Label} phải là ngày hợp lệ.");
            if(field.Kind=="time" && !TimeOnly.TryParseExact(value,"HH:mm",CultureInfo.InvariantCulture,DateTimeStyles.None,out _))
                throw new BusinessException($"Ô {field.Label} phải là giờ hợp lệ.");
            if(field.Kind is "check" or "toggle" && value is not ("true" or "false"))
                throw new BusinessException($"Ô {field.Label} không hợp lệ.");
            if(field.Kind is "choice" or "radio" &&
                !field.Options.Split(';',StringSplitOptions.RemoveEmptyEntries|StringSplitOptions.TrimEntries).Contains(value))
                throw new BusinessException($"Giá trị của ô {field.Label} không có trong lựa chọn.");
            if(field.Kind=="email" && (!System.Net.Mail.MailAddress.TryCreate(value,out var email) ||
                email.Address!=value))throw new BusinessException($"Ô {field.Label} phải là email hợp lệ.");
            if(field.Kind=="url" && (!Uri.TryCreate(value,UriKind.Absolute,out var url) ||
                url.Scheme is not ("http" or "https")))throw new BusinessException($"Ô {field.Label} phải là liên kết http/https.");
        }
        if(await db.SaveCustomRecordAsync(functionId,recordId,JsonSerializer.Serialize(values))!=1)
            throw new BusinessException("Không lưu được bản ghi. Hãy làm mới danh sách.");
        await db.AuditAsync(user,"CustomScreen",$"Lưu chức năng {functionId}, bản ghi {recordId}");
        return 0;
    });
    public Task DeleteCustomRecordAsync(long functionId,long recordId) => repository.RunAsync(true,async db=>
    {
        await db.RequireUserAsync(user,true);
        await RequireCustomFunctionAsync(db,functionId);
        if(await db.DeleteCustomRecordAsync(functionId,recordId)!=1)throw new BusinessException("Bản ghi không còn tồn tại.");
        await db.AuditAsync(user,"CustomScreen",$"Xóa chức năng {functionId}, bản ghi {recordId}");
        return 0;
    });
}
