using QLKhachSan.DAL;
using QLKhachSan.DTO;

namespace QLKhachSan.BLL;

public sealed partial class HotelService
{
    private static void ValidateMoney(decimal value)
    {
        if(value<0 || value>1_000_000_000 || decimal.Round(value,0)!=value) throw new BusinessException("Số tiền phải là số nguyên đồng từ 0 đến 1 tỷ.");
    }
    private static string Reason(string reason)
    {
        reason=reason.Trim();
        if(reason.Length is <3 or >300) throw new BusinessException("Nhập lý do từ 3 đến 300 ký tự.");
        return reason;
    }
    public Task<DateTime> ServerNowAsync() => Read(db=>db.NowAsync());
    public Task<decimal> RefundQuoteAsync(Stay selected) => OperationsRead(async db=>
    {
        var stay=await db.StayAsync(selected.Id);StayUnchanged(stay,selected,StayStatus.Reserved);
        return stay.HoldUntil<=await db.NowAsync()?0:stay.Deposit;
    });
    public Task<List<Stay>> StayHistoryAsync(string search) => OperationsRead(db=>db.StayHistoryAsync(search.Trim()));
    public Task<List<TodayScheduleItem>> TodayScheduleAsync(DateTime day) => OperationsRead(db=>db.TodayScheduleAsync(day));
    public Task<List<ServiceLine>> StayOrdersAsync(long stay) => OperationsRead(db=>db.AllOrdersAsync(stay));
    public Task UpdateBookingAsync(Stay selected,Room target,GuestInput guest,DateTime arrival,int days,DateTime receiveBy,string reason) => Write(async db=>
    {
        guest=ValidateGuest(guest);reason=Reason(reason);
        var stay=await db.StayAsync(selected.Id);StayUnchanged(stay,selected,StayStatus.Reserved);
        var now=await db.NowAsync();
        if(stay.HoldUntil<=now) throw new BusinessException("Lượt đã quá hạn nhận, không được sửa để tránh chính sách mất cọc.");
        if(days is <1 or >60 || arrival<now || arrival>=ReservationHoldLimit(stay.Created,stay.Deposit>0) || receiveBy<arrival || receiveBy>ReservationHoldLimit(stay.Created,stay.Deposit>0)) throw new BusinessException("Ngày đến và hạn nhận vượt thời gian giữ chỗ: tối đa 1 ngày chưa cọc, 15 ngày đã cọc, tính từ lúc đặt phòng.");
        var room=await db.RoomAsync(target.Id);RoomUnchanged(room,target,target.Status);
        if(room.Status==RoomStatus.BaoTri) throw new BusinessException("Phòng đang bảo trì.");
        await db.EnsureAvailableAsync(room.Id,arrival,arrival.AddDays(days),stay.Id);
        await db.UpdateBookingAsync(stay,room,guest,arrival,arrival.AddDays(days),receiveBy);
        await db.SetRoomAsync(room,room.Status);
        if(stay.RoomId!=room.Id){var old=await db.RoomAsync(stay.RoomId);await db.SetRoomAsync(old,old.Status);}
        return await db.AuditAsync(user,"EditBooking",$"Lượt {stay.Id}; phòng {stay.RoomId} → {room.Id}; đến {stay.Arrival:dd/MM/yy HH:mm} → {arrival:dd/MM/yy HH:mm}; hạn {stay.HoldUntil:dd/MM/yy HH:mm} → {receiveBy:dd/MM/yy HH:mm}; {reason}");
    });
    public Task<PeriodReport> PeriodReportAsync(DateTime from,DateTime through) => FinanceRead(async db=>
    {
        if(through.Date<from.Date || through.Date-from.Date>TimeSpan.FromDays(366)) throw new BusinessException("Chọn khoảng báo cáo tối đa 367 ngày.");
        var until=through.Date.AddDays(1);
        return new PeriodReport(await db.InvoicesAsync(from.Date,until),await db.RevenueAsync(from.Date,until),await db.PaymentsAsync(from.Date,until));
    });
    public Task<Stay> InvoiceStayAsync(long stayId) => FinanceRead(db=>db.StayAsync(stayId));
    public Task<List<ServiceLine>> InvoiceOrdersAsync(long stayId) => FinanceRead(db=>db.AllOrdersAsync(stayId));
    public Task AddDepositAsync(Stay selected,decimal amount,string method) => Write(async db=>
    {
        ValidateMoney(amount);Method(method);
        if(amount==0) throw new BusinessException("Tiền thu bổ sung phải lớn hơn 0.");
        var stay=await db.StayAsync(selected.Id);
        if(stay.Version!=selected.Version || stay.Status!=StayStatus.Reserved) throw new BusinessException("Chỉ thu cọc cho lượt đặt trước chưa nhận phòng. Hãy làm mới danh sách.");
        var now=await db.NowAsync();
        if(stay.Status==StayStatus.Reserved && stay.HoldUntil<=now) throw new BusinessException("Đã quá hạn nhận phòng, không thể thu thêm cọc.");
        ValidateMoney(stay.Deposit+amount);
        var fundedLimit=ReservationHoldLimit(stay.Created,true);
        await db.AddDepositAsync(stay.Id,amount,stay.Deposit==0?fundedLimit:stay.HoldUntil);
        await db.PaymentAsync(stay.Id,"Deposit",amount,now,method,"Thu cọc bổ sung",user);
        return await db.AuditAsync(user,"Deposit",$"Lượt {stay.Id}; thu {amount:N0}");
    });
    public Task ChangeOrderAsync(Stay selected,long orderId,int quantity,string reason,bool cancel=false) => Write(async db=>
    {
        reason=Reason(reason);
        var stay=await db.StayAsync(selected.Id);StayUnchanged(stay,selected,StayStatus.Occupied);
        var line=(await db.OrdersAsync(stay.Id)).SingleOrDefault(x=>x.Id==orderId) ?? throw new BusinessException("Dịch vụ không tồn tại hoặc đã hủy.");
        var now=await db.NowAsync();
        if(cancel)
        {
            if(line.DeliveredQuantity>0) throw new BusinessException("Đã giao một phần: chỉ được giảm số lượng xuống bằng số đã giao.");
            await db.CancelOrderAsync(line.Id,now,reason);
        }
        else
        {
            if(quantity is <1 or >100 || quantity<line.DeliveredQuantity) throw new BusinessException("Số lượng từ 1 đến 100 và không nhỏ hơn số đã giao.");
            await db.UpdateOrderAsync(line.Id,quantity,now);
        }
        await db.TouchStayAsync(stay.Id);
        return await db.AuditAsync(user,cancel?"CancelOrder":"EditOrder",$"Dòng {orderId}; SL {line.Quantity} → {(cancel?0:quantity)}; {reason}");
    });
    public Task DeliverOrderAsync(Stay selected,long orderId,int quantity) => Write(async db=>
    {
        var stay=await db.StayAsync(selected.Id);StayUnchanged(stay,selected,StayStatus.Occupied);
        var line=(await db.OrdersAsync(stay.Id)).SingleOrDefault(x=>x.Id==orderId) ?? throw new BusinessException("Dịch vụ không tồn tại hoặc đã hủy.");
        if(quantity<1 || quantity>line.Quantity-line.DeliveredQuantity) throw new BusinessException("Số lượng giao vượt số còn chờ.");
        await db.DeliverOrderAsync(orderId,quantity,await db.NowAsync());await db.TouchStayAsync(stay.Id);
        return await db.AuditAsync(user,"DeliverOrder",$"Dòng {orderId}; giao {quantity}");
    });
    public Task CancelPendingOrdersAsync(Stay selected,string reason) => Write(async db=>
    {
        reason=Reason(reason);
        var stay=await db.StayAsync(selected.Id);StayUnchanged(stay,selected,StayStatus.Occupied);
        var pending=(await db.OrdersAsync(stay.Id)).Where(x=>x.DeliveredQuantity<x.Quantity).ToList();
        if(pending.Count==0) throw new BusinessException("Không còn dịch vụ đang chờ để hủy.");
        var now=await db.NowAsync();
        foreach(var line in pending)
        {
            if(line.DeliveredQuantity==0) await db.CancelOrderAsync(line.Id,now,reason);
            else await db.UpdateOrderAsync(line.Id,line.DeliveredQuantity,now);
        }
        await db.TouchStayAsync(stay.Id);
        return await db.AuditAsync(user,"CancelPendingOrders",$"Lượt {stay.Id}; {pending.Count} dòng; {reason}");
    });
    public Task<List<ServiceCatalogItem>> CatalogAsync() => Read(async db=> {if(!RolePolicy.CanManageCatalog(user.Role))throw new BusinessException("Không có quyền quản lý dịch vụ.");return await db.CatalogAsync();});
    public Task SaveServiceAsync(ServiceCatalogItem item) => CatalogWrite(async db=>
    {
        ValidateMoney(item.Price);
        item=item with {Name=item.Name.Trim(),Category=item.Category.Trim(),Unit=item.Unit.Trim()};
        if(item.Name.Length is <2 or >150 || item.Category.Length is <2 or >100 || item.Unit.Length is <1 or >20) throw new BusinessException("Tên, danh mục hoặc đơn vị dịch vụ không hợp lệ.");
        if((await db.CatalogAsync()).Any(x=>x.Id!=item.Id && x.Name.Equals(item.Name,StringComparison.OrdinalIgnoreCase))) throw new BusinessException("Tên dịch vụ đã tồn tại.");
        if(await db.SaveServiceAsync(item)!=1) throw new BusinessException("Danh mục đã thay đổi. Hãy mở lại.");
        return await db.AuditAsync(user,"ServiceCatalog",$"Dịch vụ {item.Id}: {item.Name}; giá {item.Price}; hoạt động {item.Active}");
    });
    public Task SaveRoomAsync(Room room) => CatalogWrite(async db=>
    {
        ValidateMoney(room.Rate);ValidateMoney(room.Deposit);
        room=room with {Number=room.Number.Trim()};
        if(room.Number.Length is <1 or >10 || room.Rate==0 || room.Type is not ("Đơn" or "Đôi" or "VIP" or "Tình nhân")) throw new BusinessException("Số phòng, loại hoặc giá phòng không hợp lệ.");
        if((await db.RoomsAsync()).Any(x=>x.Id!=room.Id && x.Number.Equals(room.Number,StringComparison.OrdinalIgnoreCase))) throw new BusinessException("Số phòng đã tồn tại.");
        if(room.Id!=0)
        {
            var old=await db.RoomAsync(room.Id);
            RoomUnchanged(old,room,room.Status);
            if((old.Rate!=room.Rate || old.Type!=room.Type || old.Number!=room.Number) && (await db.ActiveStaysAsync()).Any(s=>s.RoomId==room.Id)) throw new BusinessException("Phòng còn lượt ở/lịch đặt. Chỉ đổi số, loại, giá khi không còn lượt hoạt động.");
        }
        if(await db.SaveRoomAsync(room)!=1) throw new BusinessException("Phòng đã thay đổi. Hãy mở lại.");
        return await db.AuditAsync(user,"RoomCatalog",$"Phòng {room.Number}; giá {room.Rate}; cọc gợi ý {room.Deposit}");
    });
    public Task UpdateRoomsAsync(IReadOnlyList<Room> selected,decimal? rate,decimal? deposit,string? type) => CatalogWrite(async db=>
    {
        if(selected.Count==0 || selected.Count>200 || selected.Select(r=>r.Id).Distinct().Count()!=selected.Count) throw new BusinessException("Chọn các phòng cần cập nhật.");
        if(rate is null && deposit is null && type is null) throw new BusinessException("Chọn ít nhất một thông tin cần thay đổi.");
        if(rate is { } newRate){ValidateMoney(newRate);if(newRate==0)throw new BusinessException("Giá phòng phải lớn hơn 0.");}
        if(deposit is { } newDeposit)ValidateMoney(newDeposit);
        if(type is not null && type is not ("Đơn" or "Đôi" or "VIP")) throw new BusinessException("Loại phòng không hợp lệ.");
        var active=(await db.ActiveStaysAsync()).Select(s=>s.RoomId).ToHashSet();
        foreach(var original in selected)
        {
            var current=await db.RoomAsync(original.Id);
            RoomUnchanged(current,original,original.Status);
            var updated=current with {Rate=rate??current.Rate,Deposit=deposit??current.Deposit,Type=type??current.Type};
            if(active.Contains(current.Id) && (updated.Rate!=current.Rate || updated.Type!=current.Type))
                throw new BusinessException($"Phòng {current.Number} còn lượt ở/lịch đặt; chưa thể đổi giá hoặc loại phòng.");
            if(await db.SaveRoomAsync(updated)!=1) throw new BusinessException($"Phòng {current.Number} đã thay đổi. Hãy mở lại danh sách.");
        }
        return await db.AuditAsync(user,"RoomCatalogBulk",$"{selected.Count} phòng; giá {rate?.ToString()??"giữ nguyên"}; cọc {deposit?.ToString()??"giữ nguyên"}; loại {type??"giữ nguyên"}");
    });
    public Task<List<AuditEntry>> AuditsAsync(DateTime day) => Read(async db=> {if(user.Role is not ("Admin" or "Manager"))throw new BusinessException("Không có quyền xem nhật ký.");return await db.AuditsAsync(day.Date,day.Date.AddDays(1));});
}
