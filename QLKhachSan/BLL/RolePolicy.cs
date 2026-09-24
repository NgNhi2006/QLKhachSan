namespace QLKhachSan.BLL;

public static class RolePolicy
{
    public static readonly string[] Roles=["Admin","Reception","Accountant","Manager"];
    public static bool CanOperate(string role)=>role is "Admin" or "Reception";
    public static bool CanViewFinance(string role)=>role is "Admin" or "Accountant" or "Manager";
    public static bool CanManageUsers(string role)=>role=="Admin";
    public static bool CanViewOperations(string role)=>role is "Admin" or "Reception" or "Manager";
    public static bool CanManageCatalog(string role)=>role is "Admin" or "Manager";
    public static bool CanMaintainRooms(string role)=>role is "Admin" or "Manager";
    public static string Name(string role)=>role switch
    {
        "Admin"=>"Quản trị", "Reception"=>"Lễ tân", "Accountant"=>"Kế toán", "Manager"=>"Quản lý", _=>role
    };
}
