using System.Data;
using Microsoft.Data.SqlClient;
using QLKhachSan.DTO;

namespace QLKhachSan.DAL;

// SQL transaction locks coordinate all desktop instances, not just this process.
public sealed class HotelRepository
{
    private readonly string connectionString;
    public HotelRepository(string? connectionString = null) => this.connectionString = connectionString ?? AppSettings.Load().ConnectionString;
    public async Task<T> RunAsync<T>(bool write, Func<HotelTransaction, Task<T>> action, CancellationToken token = default)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(token);
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(IsolationLevel.Serializable, token);
        var store = new HotelTransaction(connection, transaction, token);
        try
        {
            await store.LockAsync(write);
            var result = await action(store);
            await transaction.CommitAsync(token);
            return result;
        }
        catch
        {
            try { await transaction.RollbackAsync(CancellationToken.None); } catch (SqlException) { } catch (InvalidOperationException) { }
            throw;
        }
    }
}
public sealed record Account(int Id, string Username, byte[] Hash, byte[] Salt, int Iterations, string Role, bool Active, int Failed, DateTime? LockedUntil, long SecurityVersion);

public sealed partial class HotelTransaction(SqlConnection connection, SqlTransaction transaction, CancellationToken token)
{
    private static SqlParameter Param(string name, object? value)
    {
        var p = value switch
        {
            int => new SqlParameter(name, SqlDbType.Int), long => new SqlParameter(name, SqlDbType.BigInt),
            bool => new SqlParameter(name, SqlDbType.Bit),
            decimal => new SqlParameter(name, SqlDbType.Decimal) { Precision = 18, Scale = 3 },
            DateTime => new SqlParameter(name, SqlDbType.DateTime2),
            byte[] bytes => new SqlParameter(name, SqlDbType.VarBinary, bytes.Length),
            _ => new SqlParameter(name, SqlDbType.NVarChar, 500)
        };
        p.Value = value ?? DBNull.Value;
        return p;
    }
    private SqlCommand Command(string sql, object?[] args)
    {
        var cmd = new SqlCommand(sql, connection, transaction) { CommandTimeout = 15 };
        for (var i = 0; i < args.Length; i++) cmd.Parameters.Add(Param("@p" + i, args[i]));
        return cmd;
    }
    private async Task<int> Execute(string sql, params object?[] args)
    {
        await using var cmd = Command(sql, args);
        return await cmd.ExecuteNonQueryAsync(token);
    }
    private async Task<long> Scalar(string sql, params object?[] args)
    {
        await using var cmd = Command(sql, args);
        return Convert.ToInt64(await cmd.ExecuteScalarAsync(token));
    }
    private async Task<List<T>> Query<T>(string sql, Func<SqlDataReader, T> map, params object?[] args)
    {
        await using var cmd = Command(sql, args);
        await using var reader = await cmd.ExecuteReaderAsync(token);
        var rows = new List<T>();
        while (await reader.ReadAsync(token)) rows.Add(map(reader));
        return rows;
    }
    private static DateTime? Date(SqlDataReader r, int i) => r.IsDBNull(i) ? null : r.GetDateTime(i);
    internal Task<int> LockAsync(bool write) => Execute("DECLARE @r int; EXEC @r=sys.sp_getapplock @Resource=N'QLKhachSan.Write',@LockMode=@p0,@LockOwner='Transaction',@LockTimeout=10000; IF @r<0 THROW 51001,N'Hệ thống đang bận. Vui lòng thử lại.',1;", write ? "Exclusive" : "Shared");
    public async Task<DateTime> NowAsync() => (await Query("SELECT SYSDATETIME()", r => r.GetDateTime(0)))[0];
    public async Task RequireUserAsync(UserSession user, bool admin = false)
    {
        var count = await Scalar("SELECT COUNT_BIG(*) FROM dbo.Users WHERE Id=@p0 AND Username=@p1 AND Active=1 AND Archived=0 AND (@p2=0 OR Role='Admin') AND SecurityVersion=@p3 AND Role=@p4", user.Id, user.Username, admin, user.SecurityVersion, user.Role);
        if (count != 1) throw new BusinessException("Phiên đăng nhập không hợp lệ hoặc bạn không có quyền thực hiện.");
    }
    public Task<long> UserCountAsync() => Scalar("SELECT COUNT_BIG(*) FROM dbo.Users WHERE Archived=0");
    public async Task<Account?> AccountAsync(string username) => (await Query(
        "SELECT Id,Username,PasswordHash,Salt,Iterations,Role,Active,FailedAttempts,LockedUntil,SecurityVersion FROM dbo.Users WHERE Username=@p0 AND Archived=0",
        r => new Account(r.GetInt32(0),r.GetString(1),(byte[])r[2],(byte[])r[3],r.GetInt32(4),r.GetString(5),r.GetBoolean(6),r.GetInt32(7),Date(r,8),r.GetInt64(9)),username)).SingleOrDefault();
    public async Task<int> CreateUserAsync(string name, byte[] hash, byte[] salt, int iterations, string role)
        => checked((int)await Scalar("INSERT dbo.Users(Username,PasswordHash,Salt,Iterations,Role) OUTPUT INSERTED.Id VALUES(@p0,@p1,@p2,@p3,@p4)",name,hash,salt,iterations,role));
    public Task<int> LoginResultAsync(int id, bool success) => Execute(success
        ? "UPDATE dbo.Users SET FailedAttempts=0,LockedUntil=NULL WHERE Id=@p0"
        : "UPDATE dbo.Users SET FailedAttempts=CASE WHEN LockedUntil<=SYSDATETIME() THEN 1 ELSE FailedAttempts+1 END,LockedUntil=CASE WHEN LockedUntil<=SYSDATETIME() THEN NULL WHEN FailedAttempts>=4 THEN DATEADD(minute,5,SYSDATETIME()) ELSE NULL END WHERE Id=@p0",id);
    public Task<int> ChangePasswordAsync(int id, byte[] hash, byte[] salt, int iterations) => Execute("UPDATE dbo.Users SET PasswordHash=@p1,Salt=@p2,Iterations=@p3,FailedAttempts=0,LockedUntil=NULL,SecurityVersion=SecurityVersion+1 WHERE Id=@p0",id,hash,salt,iterations);
    public Task<int> AuditAsync(UserSession user, string action, string detail) => Execute("INSERT dbo.AuditLog(UserId,Action,Detail) VALUES(@p0,@p1,@p2)",user.Id,action,detail);
    private static Room MapRoom(SqlDataReader r) => new(r.GetInt32(0),r.GetString(1),r.GetString(2),r.GetDecimal(3),r.GetDecimal(4),Enum.Parse<RoomStatus>(r.GetString(5)),r.GetInt64(6));
    public Task<List<Room>> RoomsAsync() => Query("SELECT Id,Number,Type,Rate,Deposit,Status,Version FROM dbo.Rooms ORDER BY Number",MapRoom);
    public async Task<Room> RoomAsync(int id) => (await Query("SELECT Id,Number,Type,Rate,Deposit,Status,Version FROM dbo.Rooms WHERE Id=@p0",MapRoom,id)).SingleOrDefault() ?? throw new BusinessException("Phòng không tồn tại.");
    private const string StayColumns = "Id,RoomId,CustomerId,GuestName,Phone,IdentityNumber,Status,Created,Arrival,Departure,CheckIn,HoldUntil,Deposit,Version";
    private static Stay MapStay(SqlDataReader r) => new(r.GetInt64(0),r.GetInt32(1),r.GetInt32(2),r.GetString(3),r.GetString(4),r.GetString(5),Enum.Parse<StayStatus>(r.GetString(6)),r.GetDateTime(7),r.GetDateTime(8),r.GetDateTime(9),Date(r,10),Date(r,11),r.GetDecimal(12),r.GetInt64(13));
    public Task<List<Stay>> ActiveStaysAsync() => Query("SELECT "+StayColumns+" FROM dbo.Stays WHERE IsActive=1",MapStay);
    public async Task<Stay> StayAsync(long id) => (await Query("SELECT "+StayColumns+" FROM dbo.Stays WHERE Id=@p0",MapStay,id)).SingleOrDefault() ?? throw new BusinessException("Lượt lưu trú không tồn tại.");
    public Task<List<Segment>> SegmentsAsync(long stayId) => Query("SELECT Id,StayId,RoomId,Started,Ended,Rate FROM dbo.StaySegments WHERE StayId=@p0 ORDER BY Started,Id",
        r=>new Segment(r.GetInt64(0),r.GetInt64(1),r.GetInt32(2),r.GetDateTime(3),Date(r,4),r.GetDecimal(5)),stayId);
    public Task<List<ServiceItem>> MenuAsync() => Query("SELECT Id,Category,Name,Price,Unit FROM dbo.Services WHERE Active=1 ORDER BY Category,Id",
        r=>new ServiceItem(r.GetInt32(0),r.GetString(1),r.GetString(2),r.GetDecimal(3),r.GetString(4)));
    private static ServiceLine MapLine(SqlDataReader r) => new(r.GetInt64(0),r.GetInt64(1),r.GetString(2),r.GetString(3),r.GetInt32(4),r.GetDecimal(5),r.GetDateTime(6),Date(r,7),r.GetInt32(8),Date(r,9));
    private const string OrderColumns="Id,StayId,Category,Name,Quantity,Price,Ordered,Delivered,DeliveredQuantity,Cancelled";
    public Task<List<ServiceLine>> OrdersAsync(long stayId) => Query("SELECT "+OrderColumns+" FROM dbo.ServiceOrders WHERE StayId=@p0 AND Cancelled IS NULL ORDER BY Id",MapLine,stayId);
    public Task<List<ServiceLine>> AllOrdersAsync(long stayId) => Query("SELECT "+OrderColumns+" FROM dbo.ServiceOrders WHERE StayId=@p0 ORDER BY Id",MapLine,stayId);
    public Task<List<ServiceLine>> PendingAsync() => Query("SELECT "+OrderColumns+" FROM dbo.ServiceOrders WHERE Delivered IS NULL AND Cancelled IS NULL ORDER BY Ordered",MapLine);
    public Task<List<RevenueItem>> RevenueAsync(DateTime day) => RevenueAsync(day.Date,day.Date.AddDays(1));
    public Task<List<RevenueItem>> RevenueAsync(DateTime from,DateTime until) => Query("SELECT Category,SUM(Amount) FROM (SELECT N'Tiền phòng' Category,RoomCharge Amount FROM dbo.Invoices i WHERE i.Issued>=@p0 AND i.Issued<@p1 AND NOT EXISTS(SELECT 1 FROM dbo.InvoiceVoids v WHERE v.InvoiceId=i.Id) UNION ALL SELECT o.Category,o.Price*o.Quantity FROM dbo.ServiceOrders o JOIN dbo.Invoices i ON i.StayId=o.StayId WHERE i.Issued>=@p0 AND i.Issued<@p1 AND o.Cancelled IS NULL AND NOT EXISTS(SELECT 1 FROM dbo.InvoiceVoids v WHERE v.InvoiceId=i.Id) UNION ALL SELECT N'Cọc không hoàn (không đến)',Amount FROM dbo.Payments WHERE Kind='Forfeit' AND Created>=@p0 AND Created<@p1 UNION ALL SELECT N'Giảm trừ',-a.Amount FROM dbo.InvoiceAdjustments a JOIN dbo.Invoices i ON i.Id=a.InvoiceId WHERE a.CreatedAt>=@p0 AND a.CreatedAt<@p1 AND NOT EXISTS(SELECT 1 FROM dbo.InvoiceVoids v WHERE v.InvoiceId=i.Id)) q GROUP BY Category",r=>new RevenueItem(r.GetString(0),r.GetDecimal(1)),from,until);
    public Task<List<CustomerSummary>> CustomersAsync(string search) => Query("SELECT TOP(200) c.Id,c.Name,c.Phone,c.IdentityNumber,COUNT(i.Id),COALESCE(SUM(i.RoomCharge+i.ServiceCharge),0) FROM dbo.Customers c LEFT JOIN dbo.Stays s ON s.CustomerId=c.Id LEFT JOIN dbo.Invoices i ON i.StayId=s.Id AND NOT EXISTS(SELECT 1 FROM dbo.InvoiceVoids v WHERE v.InvoiceId=i.Id) WHERE @p0=N'' OR CHARINDEX(@p0,c.Name)>0 OR CHARINDEX(@p0,c.Phone)>0 OR CHARINDEX(@p0,c.IdentityNumber)>0 GROUP BY c.Id,c.Name,c.Phone,c.IdentityNumber ORDER BY c.Id DESC",r=>new CustomerSummary(r.GetInt32(0),r.GetString(1),r.GetString(2),r.GetString(3),r.GetInt32(4),r.GetDecimal(5)),search);
    public Task<List<ServiceLine>> CustomerOrdersAsync(int customer) => Query("SELECT TOP(500) o.Id,o.StayId,o.Category,o.Name,o.Quantity,o.Price,o.Ordered,o.Delivered,o.DeliveredQuantity,o.Cancelled FROM dbo.ServiceOrders o JOIN dbo.Stays s ON s.Id=o.StayId WHERE s.CustomerId=@p0 ORDER BY o.Id DESC",MapLine,customer);
    public Task<List<Invoice>> InvoicesAsync(DateTime day) => InvoicesAsync(day.Date,day.Date.AddDays(1));
    public Task<List<Invoice>> InvoicesAsync(DateTime from,DateTime until) => Query("SELECT Id,StayId,RoomNumber,GuestName,Issued,RoomCharge,ServiceCharge,Deposit,Collected,Refunded,Method FROM dbo.Invoices i WHERE Issued>=@p0 AND Issued<@p1 AND NOT EXISTS(SELECT 1 FROM dbo.InvoiceVoids v WHERE v.InvoiceId=i.Id) ORDER BY Id DESC",r=>new Invoice(r.GetInt64(0),r.GetInt64(1),r.GetString(2),r.GetString(3),r.GetDateTime(4),r.GetDecimal(5),r.GetDecimal(6),r.GetDecimal(7),r.GetDecimal(8),r.GetDecimal(9),r.GetString(10)),from,until);
    public Task<List<Invoice>> CustomerInvoicesAsync(int customerId) => Query("SELECT TOP(200) i.Id,i.StayId,i.RoomNumber,i.GuestName,i.Issued,i.RoomCharge,i.ServiceCharge,i.Deposit,i.Collected,i.Refunded,i.Method FROM dbo.Invoices i JOIN dbo.Stays s ON s.Id=i.StayId WHERE s.CustomerId=@p0 AND NOT EXISTS(SELECT 1 FROM dbo.InvoiceVoids v WHERE v.InvoiceId=i.Id) ORDER BY i.Id DESC",r=>new Invoice(r.GetInt64(0),r.GetInt64(1),r.GetString(2),r.GetString(3),r.GetDateTime(4),r.GetDecimal(5),r.GetDecimal(6),r.GetDecimal(7),r.GetDecimal(8),r.GetDecimal(9),r.GetString(10)),customerId);
}
