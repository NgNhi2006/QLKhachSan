using QLKhachSan.DTO;

namespace QLKhachSan.DAL;

public sealed partial class HotelTransaction
{
    public async Task<List<RevenueDay>> RevenueTrendAsync(DateTime from,DateTime until)
    {
        var rows=await Query("SELECT Day,SUM(Rooms),SUM(Services),SUM(Forfeits) FROM (SELECT CAST(i.Issued AS date) Day,i.RoomCharge Rooms,i.ServiceCharge Services,CAST(0 AS decimal(18,2)) Forfeits FROM dbo.Invoices i WHERE i.Issued>=@p0 AND i.Issued<@p1 AND NOT EXISTS(SELECT 1 FROM dbo.InvoiceVoids v WHERE v.InvoiceId=i.Id) UNION ALL SELECT CAST(Created AS date),0,0,Amount FROM dbo.Payments WHERE Kind='Forfeit' AND Created>=@p0 AND Created<@p1 UNION ALL SELECT CAST(a.CreatedAt AS date),0,-a.Amount,0 FROM dbo.InvoiceAdjustments a WHERE a.CreatedAt>=@p0 AND a.CreatedAt<@p1 AND NOT EXISTS(SELECT 1 FROM dbo.InvoiceVoids v WHERE v.InvoiceId=a.InvoiceId)) r GROUP BY Day ORDER BY Day",r=>new RevenueDay(r.GetDateTime(0),r.GetDecimal(1),r.GetDecimal(2),r.GetDecimal(3)),from,until);
        var byDate=rows.ToDictionary(r=>r.Day);
        return Enumerable.Range(0,(until.Date-from.Date).Days).Select(i=>from.Date.AddDays(i)).Select(day=>byDate.GetValueOrDefault(day)??new RevenueDay(day,0,0,0)).ToList();
    }
    public async Task EnsureAvailableAsync(int roomId, DateTime from, DateTime until, long excluding = 0)
    {
        if (until <= from) throw new BusinessException("Ngày trả phải sau ngày nhận.");
        var conflicts = await Scalar("SELECT COUNT_BIG(*) FROM dbo.Stays WHERE RoomId=@p0 AND IsActive=1 AND Id<>@p3 AND COALESCE(CheckIn,Arrival)<@p2 AND Departure>@p1", roomId,from,until,excluding);
        if (conflicts > 0) throw new BusinessException("Phòng đã có lịch trong khoảng ngày này. Hãy chọn phòng hoặc thời gian khác.");
    }
    public Task<List<Stay>> StayHistoryAsync(string search) => Query("SELECT TOP(500) "+StayColumns+" FROM dbo.Stays WHERE @p0=N'' OR CHARINDEX(@p0,GuestName)>0 OR CHARINDEX(@p0,IdentityNumber)>0 OR CHARINDEX(@p0,Phone)>0 ORDER BY Id DESC",MapStay,search);
    public Task<List<TodayScheduleItem>> TodayScheduleAsync(DateTime day) => Query(@"
SELECT s.Id,r.Number,s.GuestName,s.Phone,N'Chờ nhận',s.Arrival
FROM dbo.Stays s JOIN dbo.Rooms r ON r.Id=s.RoomId
WHERE s.Status='Reserved' AND s.Arrival>=@p0 AND s.Arrival<@p1
UNION ALL
SELECT s.Id,r.Number,s.GuestName,s.Phone,N'Đã nhận',s.CheckIn
FROM dbo.Stays s JOIN dbo.Rooms r ON r.Id=s.RoomId
WHERE s.CheckIn>=@p0 AND s.CheckIn<@p1
UNION ALL
SELECT s.Id,r.Number,s.GuestName,s.Phone,N'Chờ trả',s.Departure
FROM dbo.Stays s JOIN dbo.Rooms r ON r.Id=s.RoomId
WHERE s.Status='Occupied' AND s.Departure>=@p0 AND s.Departure<@p1
UNION ALL
SELECT i.StayId,i.RoomNumber,i.GuestName,s.Phone,N'Đã trả',i.Issued
FROM dbo.Invoices i JOIN dbo.Stays s ON s.Id=i.StayId
WHERE i.Issued>=@p0 AND i.Issued<@p1
ORDER BY 6,1",r=>new TodayScheduleItem(r.GetInt64(0),r.GetString(1),r.GetString(2),r.GetString(3),r.GetString(4),r.GetDateTime(5)),day.Date,day.Date.AddDays(1));
    public Task<int> AddDepositAsync(long id,decimal amount,DateTime? holdUntil) => Execute("UPDATE dbo.Stays SET Deposit=Deposit+@p1,HoldUntil=@p2,Version=Version+1 WHERE Id=@p0",id,amount,holdUntil);
    public async Task UpdateBookingAsync(Stay stay,Room room,GuestInput guest,DateTime arrival,DateTime departure,DateTime receiveBy)
    {
        await Execute("IF EXISTS(SELECT 1 FROM dbo.Customers WHERE IdentityNumber=@p2) UPDATE dbo.Customers SET Name=@p0,Phone=@p1 WHERE IdentityNumber=@p2; ELSE INSERT dbo.Customers(Name,Phone,IdentityNumber) VALUES(@p0,@p1,@p2)",guest.Name,guest.Phone,guest.Identity);
        await Execute("UPDATE dbo.Stays SET RoomId=@p1,GuestName=@p2,Phone=@p3,IdentityNumber=@p4,CustomerId=(SELECT Id FROM dbo.Customers WHERE IdentityNumber=@p4),Arrival=@p5,Departure=@p6,HoldUntil=@p7,Version=Version+1 WHERE Id=@p0",stay.Id,room.Id,guest.Name,guest.Phone,guest.Identity,arrival,departure,receiveBy);
    }
    public Task<int> UpdateOrderAsync(long id,int quantity,DateTime now) => Execute("UPDATE dbo.ServiceOrders SET Quantity=@p1,Delivered=CASE WHEN DeliveredQuantity=@p1 THEN @p2 ELSE NULL END WHERE Id=@p0",id,quantity,now);
    public Task<int> CancelOrderAsync(long id,DateTime now,string reason) => Execute("UPDATE dbo.ServiceOrders SET Cancelled=@p1,CancelReason=@p2 WHERE Id=@p0",id,now,reason);
    public Task<int> DeliverOrderAsync(long id,int quantity,DateTime now) => Execute("UPDATE dbo.ServiceOrders SET DeliveredQuantity=DeliveredQuantity+@p1,Delivered=CASE WHEN DeliveredQuantity+@p1=Quantity THEN @p2 ELSE NULL END WHERE Id=@p0",id,quantity,now);
    public Task<List<PaymentEntry>> PaymentsAsync(DateTime from,DateTime until) => Query("SELECT p.Id,p.StayId,s.GuestName,p.Kind,p.Amount,p.Created,p.Method,p.Note,u.Username FROM dbo.Payments p JOIN dbo.Stays s ON s.Id=p.StayId JOIN dbo.Users u ON u.Id=p.CreatedBy WHERE p.Created>=@p0 AND p.Created<@p1 ORDER BY p.Id DESC",r=>new PaymentEntry(r.GetInt64(0),r.GetInt64(1),r.GetString(2),r.GetString(3),r.GetDecimal(4),r.GetDateTime(5),r.GetString(6),r.GetString(7),r.GetString(8)),from,until);
    public Task<List<UserInfo>> UsersAsync() => Query("SELECT Id,Username,Role,Active,LockedUntil,SecurityVersion FROM dbo.Users WHERE Archived=0 ORDER BY Username",r=>new UserInfo(r.GetInt32(0),r.GetString(1),r.GetString(2),r.GetBoolean(3),Date(r,4),r.GetInt64(5)));
    public Task<long> ActiveAdminsAsync() => Scalar("SELECT COUNT_BIG(*) FROM dbo.Users WHERE Active=1 AND Archived=0 AND Role='Admin'");
    public Task<int> ArchiveUsersAsync() => Execute("UPDATE dbo.Users SET Archived=1,Active=0,SecurityVersion=SecurityVersion+1,LockedUntil=NULL WHERE Archived=0");
    public Task<int> UpdateUserAsync(int id,string role,bool active) => Execute("UPDATE dbo.Users SET Role=@p1,Active=@p2,SecurityVersion=SecurityVersion+1,FailedAttempts=0,LockedUntil=NULL WHERE Id=@p0",id,role,active);
    public Task<List<ServiceCatalogItem>> CatalogAsync() => Query("SELECT Id,Category,Name,Price,Unit,Active,Version FROM dbo.Services ORDER BY Category,Name",r=>new ServiceCatalogItem(r.GetInt32(0),r.GetString(1),r.GetString(2),r.GetDecimal(3),r.GetString(4),r.GetBoolean(5),r.GetInt64(6)));
    public Task<int> SaveServiceAsync(ServiceCatalogItem item) => item.Id==0
        ? Execute("INSERT dbo.Services(Category,Name,Price,Unit,Active) VALUES(@p0,@p1,@p2,@p3,@p4)",item.Category,item.Name,item.Price,item.Unit,item.Active)
        : Execute("UPDATE dbo.Services SET Category=@p1,Name=@p2,Price=@p3,Unit=@p4,Active=@p5,Version=Version+1 WHERE Id=@p0 AND Version=@p6",item.Id,item.Category,item.Name,item.Price,item.Unit,item.Active,item.Version);
    public Task<int> SaveRoomAsync(Room room) => room.Id==0
        ? Execute("INSERT dbo.Rooms(Number,Type,Rate,Deposit) VALUES(@p0,@p1,@p2,@p3)",room.Number,room.Type,room.Rate,room.Deposit)
        : Execute("UPDATE dbo.Rooms SET Number=@p1,Type=@p2,Rate=@p3,Deposit=@p4,Version=Version+1 WHERE Id=@p0 AND Version=@p5",room.Id,room.Number,room.Type,room.Rate,room.Deposit,room.Version);
    public Task<List<AuditEntry>> AuditsAsync(DateTime from,DateTime until) => Query("SELECT TOP(1000) a.Id,a.Created,u.Username,a.Action,a.Detail FROM dbo.AuditLog a JOIN dbo.Users u ON u.Id=a.UserId WHERE a.Created>=@p0 AND a.Created<@p1 ORDER BY a.Id DESC",r=>new AuditEntry(r.GetInt64(0),r.GetDateTime(1),r.GetString(2),r.GetString(3),r.GetString(4)),from,until);
}
