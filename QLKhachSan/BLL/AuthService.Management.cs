using System.Security.Cryptography;
using QLKhachSan.DTO;

namespace QLKhachSan.BLL;

public sealed partial class AuthService
{
    public Task<List<UserInfo>> EmployeesAsync(UserSession actor) => repository.RunAsync(false,async db=>
    {
        await db.RequireUserAsync(actor);
        if(actor.Role is not ("Admin" or "Manager"))throw new BusinessException("Không có quyền xem nhân viên.");
        return (await db.UsersAsync()).Where(x=>x.Role!="Admin").ToList();
    });
    public Task<List<UserInfo>> UsersAsync(UserSession actor) => repository.RunAsync(false,async db=>
    {
        await db.RequireUserAsync(actor,true);return await db.UsersAsync();
    });
    public Task UpdateUserAsync(UserSession actor,UserInfo selected,string role,bool active) => repository.RunAsync(true,async db=>
    {
        await db.RequireUserAsync(actor,true);
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
            if(selected.Id==actor.Id) throw new BusinessException("Hãy dùng mục đổi mật khẩu cho tài khoản đang sử dụng.");
            var current=(await db.UsersAsync()).SingleOrDefault(x=>x.Id==selected.Id);
            if(current is null || current.Version!=selected.Version) throw new BusinessException("Tài khoản đã thay đổi. Hãy mở lại.");
            await db.ChangePasswordAsync(selected.Id,hash,salt,Iterations);
            return await db.AuditAsync(actor,"ResetPassword",$"Đặt lại mật khẩu {selected.Username}; thu hồi phiên cũ");
        });
    }
}
