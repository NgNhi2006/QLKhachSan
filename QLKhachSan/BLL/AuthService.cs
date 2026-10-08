using System.Security.Cryptography;
using System.Text.RegularExpressions;
using QLKhachSan.DAL;
using QLKhachSan.DTO;

namespace QLKhachSan.BLL;

public sealed partial class AuthService(HotelRepository repository)
{
    public const int Iterations = 210000;
    private static byte[] Hash(string password, byte[] salt, int iterations) => Rfc2898DeriveBytes.Pbkdf2(password,salt,iterations,HashAlgorithmName.SHA512,32);
    private static void Validate(string username, string password)
    {
        if(!Regex.IsMatch(username,@"^[A-Za-z0-9_.-]{3,50}$")) throw new BusinessException("Tài khoản: 3–50 ký tự chữ, số, dấu chấm, gạch dưới hoặc gạch ngang.");
        if(password.Length is < 1 or > 5)
            throw new BusinessException("Mật khẩu cần từ 1 đến 5 ký tự.");
    }
    public Task<bool> NeedsSetupAsync() => repository.RunAsync(false,async db => await db.UserCountAsync()==0);
    public async Task<UserSession> SetupAsync(string username,string password)
    {
        username=username.Trim(); Validate(username,password);
        var salt=RandomNumberGenerator.GetBytes(16);
        var hash=await Task.Run(()=>Hash(password,salt,Iterations));
        return await repository.RunAsync(true,async db =>
        {
            if(await db.UserCountAsync()!=0) throw new BusinessException("Đã có tài khoản quản trị. Vui lòng đăng nhập.");
            var id=await db.CreateUserAsync(username,hash,salt,Iterations,"Admin");
            var session=new UserSession(id,username,"Admin")
            {
                GrantedFunctions=await db.MenuFunctionCodesAsync()
            };
            await db.AuditAsync(session,"Setup","Tạo quản trị ban đầu");
            return session;
        });
    }
    public async Task<UserSession> LoginAsync(string username,string password)
    {
        username=username.Trim();
        if(username.Length is < 1 or > 50 || password.Length is < 1 or > 128) throw new BusinessException("Thông tin đăng nhập không hợp lệ.");
        // Password hashing is deliberately outside the global write lock.
        var candidate=await repository.RunAsync(false,db=>db.AccountAsync(username));
        var calculated=await Task.Run(()=>Hash(password,candidate?.Salt ?? new byte[16],candidate?.Iterations ?? Iterations));
        var session=await repository.RunAsync<UserSession?>(true,async db =>
        {
            var a=await db.AccountAsync(username);
            var now=await db.NowAsync();
            if(a is null || candidate is null || !a.Active || a.LockedUntil>now || a.SecurityVersion!=candidate.SecurityVersion) return null;
            var ok=CryptographicOperations.FixedTimeEquals(calculated,a.Hash);
            await db.LoginResultAsync(a.Id,ok);
            if(!ok) return null; // Commit the failure counter rather than roll it back.
            var user=new UserSession(a.Id,a.Username,a.Role)
            {
                SecurityVersion=a.SecurityVersion,
                GrantedFunctions=await db.UserFunctionsAsync(a.Id),
                DisplayName=a.DisplayName,
                AvatarPng=a.AvatarPng
            };
            await db.AuditAsync(user,"Login","Đăng nhập");
            return user;
        });
        return session ?? throw new BusinessException("Sai tài khoản/mật khẩu, tài khoản bị vô hiệu hóa hoặc đang khóa 5 phút sau nhiều lần nhập sai.");
    }
    public async Task CreateUserAsync(UserSession actor,string username,string password,string role)
    {
        username=username.Trim(); Validate(username,password);
        if(!RolePolicy.Roles.Contains(role)) throw new BusinessException("Vai trò không hợp lệ.");
        var salt=RandomNumberGenerator.GetBytes(16); var hash=await Task.Run(()=>Hash(password,salt,Iterations));
        await repository.RunAsync(true,async db =>
        {
            await db.RequireUserAsync(actor,true);
            FunctionPolicy.Require(actor,"staff.manage");
            if(await db.AccountAsync(username)!=null) throw new BusinessException("Tên đăng nhập đã tồn tại.");
            await db.CreateUserAsync(username,hash,salt,Iterations,role);
            return await db.AuditAsync(actor,"CreateUser",username+" / "+role);
        });
    }
    public async Task ChangePasswordAsync(UserSession user,string oldPassword,string newPassword)
    {
        Validate(user.Username,newPassword);
        var salt=RandomNumberGenerator.GetBytes(16); var hash=await Task.Run(()=>Hash(newPassword,salt,Iterations));
        await repository.RunAsync(true,async db=>
        {
            await db.RequireUserAsync(user);
            FunctionPolicy.Require(user,"system.password");
            var account=await db.AccountAsync(user.Username) ?? throw new BusinessException("Tài khoản không tồn tại.");
            var oldHash=await Task.Run(()=>Hash(oldPassword,account.Salt,account.Iterations));
            if(!CryptographicOperations.FixedTimeEquals(oldHash,account.Hash)) throw new BusinessException("Mật khẩu hiện tại không đúng.");
            await db.ChangePasswordAsync(user.Id,hash,salt,Iterations);
            return await db.AuditAsync(user,"Password","Đổi mật khẩu");
        });
        user.SecurityVersion++;
    }
}
