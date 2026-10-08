using System.Security.Cryptography;
using QLKhachSan.DTO;

namespace QLKhachSan.BLL;

public sealed partial class AuthService
{
    public Task<UserInfo> ProfileAsync(UserSession actor,int id) => repository.RunAsync(false,async db=>
    {
        await db.RequireUserAsync(actor,id!=actor.Id);
        if(id!=actor.Id)FunctionPolicy.Require(actor,"staff.manage");
        return (await db.UsersAsync()).SingleOrDefault(x=>x.Id==id)
            ?? throw new BusinessException("Tài khoản không tồn tại.");
    });

    public async Task SaveProfileAsync(UserSession actor,UserInfo selected,string displayName,byte[]? avatarPng)
    {
        displayName=displayName.Trim();
        if(displayName.Length is <2 or >100 || displayName.Any(char.IsControl))
            throw new BusinessException("Tên hiển thị cần từ 2 đến 100 ký tự hợp lệ.");
        if(avatarPng is {Length:>262144} || avatarPng is {Length:>0} &&
            !avatarPng.AsSpan().StartsWith(new byte[]{137,80,78,71,13,10,26,10}))
            throw new BusinessException("Ảnh đại diện cần là PNG và không quá 256 KB.");
        await repository.RunAsync(true,async db=>
        {
            await db.RequireUserAsync(actor,selected.Id!=actor.Id);
            if(selected.Id!=actor.Id)FunctionPolicy.Require(actor,"staff.manage");
            var current=(await db.UsersAsync()).SingleOrDefault(x=>x.Id==selected.Id)
                ?? throw new BusinessException("Tài khoản không tồn tại.");
            if(current.Version!=selected.Version)throw new BusinessException("Tài khoản đã thay đổi. Hãy mở lại.");
            if(await db.UpdateProfileAsync(selected.Id,displayName,avatarPng)!=1)
                throw new BusinessException("Không lưu được hồ sơ nhân viên.");
            await db.AuditAsync(actor,"UpdateProfile",$"{selected.Username}; tên hiển thị {displayName}");
            return 0;
        });
        if(selected.Id==actor.Id){actor.DisplayName=displayName;actor.AvatarPng=avatarPng;}
    }

    public Task<HashSet<string>> UserFunctionsAsync(UserSession actor,UserInfo selected) => repository.RunAsync(false,async db=>
    {
        await db.RequireUserAsync(actor,true);
        FunctionPolicy.Require(actor,"staff.manage");
        var current=(await db.UsersAsync()).SingleOrDefault(x=>x.Id==selected.Id)
            ?? throw new BusinessException("Tài khoản không tồn tại.");
        return await db.UserFunctionsAsync(current.Id);
    });

    public Task SaveUserFunctionsAsync(UserSession actor,UserInfo selected,IReadOnlyCollection<string> codes) => repository.RunAsync(true,async db=>
    {
        await db.RequireUserAsync(actor,true);
        FunctionPolicy.Require(actor,"staff.manage");
        if(selected.Id==actor.Id)throw new BusinessException("Không tự thay đổi quyền của tài khoản đang sử dụng.");
        var current=(await db.UsersAsync()).SingleOrDefault(x=>x.Id==selected.Id)
            ?? throw new BusinessException("Tài khoản không tồn tại.");
        if(current.Version!=selected.Version)throw new BusinessException("Tài khoản đã thay đổi. Hãy mở lại.");
        if(codes.Any(code=>!FunctionPolicy.RoleAllows(current.Role,code)))
            throw new BusinessException("Có chức năng không thuộc vai trò nhân viên.");
        var available=await db.MenuFunctionCodesAsync();
        if(codes.Any(code=>!available.Contains(code)))
            throw new BusinessException("Có chức năng không thuộc menu hiện hành.");
        await db.SaveUserFunctionsAsync(current.Id,codes.Distinct(StringComparer.Ordinal));
        return await db.AuditAsync(actor,"UserFunctions",$"Cấp {codes.Count} chức năng cho {current.Username}; thu hồi phiên cũ");
    });
    public Task<List<UserInfo>> EmployeesAsync(UserSession actor) => repository.RunAsync(false,async db=>
    {
        await db.RequireUserAsync(actor);
        FunctionPolicy.Require(actor,"staff.view");
        if(actor.Role is not ("Admin" or "Manager"))throw new BusinessException("Không có quyền xem nhân viên.");
        return (await db.UsersAsync()).Where(x=>x.Role!="Admin").ToList();
    });
    public Task<List<UserInfo>> UsersAsync(UserSession actor) => repository.RunAsync(false,async db=>
    {
        await db.RequireUserAsync(actor,true);
        FunctionPolicy.Require(actor,"staff.manage");
        return await db.UsersAsync();
    });
    public Task UpdateUserAsync(UserSession actor,UserInfo selected,string role,bool active) => repository.RunAsync(true,async db=>
    {
        await db.RequireUserAsync(actor,true);
        FunctionPolicy.Require(actor,"staff.manage");
        if(!RolePolicy.Roles.Contains(role)) throw new BusinessException("Vai trò không hợp lệ.");
        if(selected.Id==actor.Id) throw new BusinessException("Không thay đổi quyền/khóa tài khoản đang sử dụng.");
        var current=(await db.UsersAsync()).SingleOrDefault(x=>x.Id==selected.Id);
        if(current is null || current.Version!=selected.Version) throw new BusinessException("Tài khoản đã thay đổi. Hãy mở lại.");
        if(current.Active && current.Role=="Admin" && (!active || role!="Admin") && await db.ActiveAdminsAsync()<=1) throw new BusinessException("Phải còn ít nhất một quản trị hoạt động.");
        await db.UpdateUserAsync(current.Id,role,active);
        return await db.AuditAsync(actor,"UpdateUser",$"{current.Username}; quyền {role}; hoạt động {active}; thu hồi phiên cũ");
    });
    public async Task ResetPasswordAsync(UserSession actor,UserInfo selected,string password)
    {
        Validate(selected.Username,password);
        var salt=RandomNumberGenerator.GetBytes(16);
        var hash=await Task.Run(()=>Hash(password,salt,Iterations));
        await repository.RunAsync(true,async db=>
        {
            await db.RequireUserAsync(actor,true);
            FunctionPolicy.Require(actor,"staff.manage");
            if(selected.Id==actor.Id) throw new BusinessException("Hãy dùng mục đổi mật khẩu cho tài khoản đang sử dụng.");
            var current=(await db.UsersAsync()).SingleOrDefault(x=>x.Id==selected.Id);
            if(current is null || current.Version!=selected.Version) throw new BusinessException("Tài khoản đã thay đổi. Hãy mở lại.");
            await db.ChangePasswordAsync(selected.Id,hash,salt,Iterations);
            return await db.AuditAsync(actor,"ResetPassword",$"Đặt lại mật khẩu {selected.Username}; thu hồi phiên cũ");
        });
    }
}
