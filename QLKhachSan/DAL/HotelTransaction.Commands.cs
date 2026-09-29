using QLKhachSan.DTO;

namespace QLKhachSan.DAL;

public sealed partial class HotelTransaction
{
    public async Task<long> CreateStayAsync(Room room, GuestInput guest, bool reserve, DateTime now, DateTime arrival, DateTime departure, decimal deposit, DateTime? hold, UserSession user)
    {
        await Execute("IF EXISTS(SELECT 1 FROM dbo.Customers WHERE IdentityNumber=@p2) UPDATE dbo.Customers SET Name=@p0,Phone=@p1 WHERE IdentityNumber=@p2; ELSE INSERT dbo.Customers(Name,Phone,IdentityNumber) VALUES(@p0,@p1,@p2);",guest.Name,guest.Phone,guest.Identity);
        return await Scalar("INSERT dbo.Stays(RoomId,CustomerId,GuestName,Phone,IdentityNumber,Status,IsActive,Created,Arrival,Departure,CheckIn,HoldUntil,Deposit,CreatedBy) OUTPUT INSERTED.Id SELECT @p0,Id,@p1,@p2,@p3,@p4,1,@p5,@p6,@p7,@p8,@p9,@p10,@p11 FROM dbo.Customers WHERE IdentityNumber=@p3",
            room.Id,guest.Name,guest.Phone,guest.Identity,reserve?"Reserved":"Occupied",now,arrival,departure,reserve?null:now,hold,deposit,user.Id);
    }
    public async Task SetRoomAsync(Room room, RoomStatus status)
    {
        if(await Execute("UPDATE dbo.Rooms SET Status=@p1,Version=Version+1 WHERE Id=@p0 AND Version=@p2",room.Id,status.ToString(),room.Version)!=1)
            throw new BusinessException("Phòng đã thay đổi ở phiên khác. Hãy làm mới và thử lại.");
    }
    public Task<int> StartSegmentAsync(long stayId, Room room, DateTime now) => Execute("INSERT dbo.StaySegments(StayId,RoomId,Started,Rate) VALUES(@p0,@p1,@p2,@p3)",stayId,room.Id,now,room.Rate);
    public Task<int> EndSegmentAsync(long stayId, DateTime now) => Execute("UPDATE dbo.StaySegments SET Ended=@p1 WHERE StayId=@p0 AND Ended IS NULL",stayId,now);
    public Task<int> CheckInAsync(Stay stay, DateTime now, DateTime departure) => Execute("UPDATE dbo.Stays SET Status='Occupied',CheckIn=@p1,Departure=@p2,HoldUntil=NULL,Version=Version+1 WHERE Id=@p0",stay.Id,now,departure);
    public Task<int> TransferAsync(Stay stay, int newRoom) => Execute("UPDATE dbo.Stays SET RoomId=@p1,Version=Version+1 WHERE Id=@p0",stay.Id,newRoom);
    public Task<int> ExtendAsync(Stay stay, DateTime departure) => Execute("UPDATE dbo.Stays SET Departure=@p1,Version=Version+1 WHERE Id=@p0",stay.Id,departure);
    public Task<int> CloseStayAsync(Stay stay, bool paid, DateTime now) => Execute("UPDATE dbo.Stays SET Status=@p1,IsActive=0,CheckOut=@p2,Version=Version+1 WHERE Id=@p0",stay.Id,paid?"Paid":"Cancelled",now);
    public Task<int> TouchStayAsync(long id) => Execute("UPDATE dbo.Stays SET Version=Version+1 WHERE Id=@p0",id);
    public async Task<int> AddOrderAsync(long stay, ServiceItem item, int quantity, DateTime now, UserSession user)
    {
        var shift=await RequireOpenShiftAsync(user.Id);
        return await Execute("INSERT dbo.ServiceOrders(StayId,ServiceId,Category,Name,Quantity,Price,Ordered,CreatedBy,ShiftId) VALUES(@p0,@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8)",stay,item.Id,item.Category,item.Name,quantity,item.Price,now,user.Id,shift);
    }
    public Task<int> DeliverAsync(long stay, DateTime now) => Execute("UPDATE dbo.ServiceOrders SET Delivered=@p1,DeliveredQuantity=Quantity WHERE StayId=@p0 AND Delivered IS NULL AND Cancelled IS NULL",stay,now);
    public async Task<int> PaymentAsync(long stay,string kind,decimal amount,DateTime now,string method,string note,UserSession user,string? reference=null)
    {
        if(amount==0)return 0;
        long? shift=kind=="Forfeit"?null:await RequireOpenShiftAsync(user.Id);
        reference=string.IsNullOrWhiteSpace(reference)?null:reference.Trim();
        if((method is "Chuyển khoản" or "Thẻ POS" or "Công nợ OTA") && reference is null && (kind is "Deposit" or "Checkout"))
            throw new BusinessException("Giao dịch QR/POS cần mã giao dịch hoặc mã chuẩn chi.");
        if(reference is {Length:>100})throw new BusinessException("Mã giao dịch quá dài.");
        return await Execute("INSERT dbo.Payments(StayId,Kind,Amount,Created,Method,Note,CreatedBy,ShiftId,ExternalReference) VALUES(@p0,@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8)",stay,kind,amount,now,method,note,user.Id,shift,reference);
    }
    public Task<long> InvoiceAsync(BillQuote bill,string method,UserSession user,DateTime issued) => Scalar("INSERT dbo.Invoices(StayId,RoomNumber,GuestName,Issued,RoomCharge,ServiceCharge,Deposit,Collected,Refunded,Method,CreatedBy) OUTPUT INSERTED.Id VALUES(@p0,@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10)",bill.Stay.Id,bill.Room.Number,bill.Stay.Guest,issued,bill.RoomCharge,bill.Services,bill.Stay.Deposit,bill.ToCollect,bill.ToRefund,method,user.Id);
}
