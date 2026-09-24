using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace QLKhachSan.GUI;

internal static class RememberedLogin
{
    private static string FilePath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"QLKhachSan","remembered-login.dat");

    public static List<(string Username,string Password)> Load()
    {
        try
        {
            if(!File.Exists(FilePath))return [];
            var data=ProtectedData.Unprotect(File.ReadAllBytes(FilePath),null,DataProtectionScope.CurrentUser);
            using var document=JsonDocument.Parse(Encoding.UTF8.GetString(data));
            var root=document.RootElement;
            if(root.ValueKind!=JsonValueKind.Array)return [];
            // Older versions stored one [username,password] pair.
            if(root.GetArrayLength()==2 && root[0].ValueKind==JsonValueKind.String)
                return [(root[0].GetString()??"",root[1].GetString()??"")];
            return root.EnumerateArray().Where(x=>x.ValueKind==JsonValueKind.Array && x.GetArrayLength()==2)
                .Select(x=>(x[0].GetString()??"",x[1].GetString()??""))
                .Where(x=>!string.IsNullOrWhiteSpace(x.Item1)).ToList();
        }
        catch(Exception ex)when(ex is CryptographicException or IOException or JsonException or UnauthorizedAccessException){return [];}
    }

    public static void Save(string username,string password)
    {
        var accounts=Load();
        accounts.RemoveAll(x=>string.Equals(x.Username,username,StringComparison.OrdinalIgnoreCase));
        accounts.Insert(0,(username,password));
        Write(accounts.Take(8));
    }

    public static void ForgetPassword(string username)
    {
        var accounts=Load();
        for(var i=0;i<accounts.Count;i++)
            if(string.Equals(accounts[i].Username,username,StringComparison.OrdinalIgnoreCase))
                accounts[i]=(accounts[i].Username,"");
        Write(accounts);
    }

    private static void Write(IEnumerable<(string Username,string Password)> accounts)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        var data=Encoding.UTF8.GetBytes(JsonSerializer.Serialize(accounts.Select(x=>new[]{x.Username,x.Password}).ToArray()));
        File.WriteAllBytes(FilePath,ProtectedData.Protect(data,null,DataProtectionScope.CurrentUser));
    }
}
