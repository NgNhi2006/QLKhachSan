using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;
using QLKhachSan.DAL;
using QLKhachSan.DTO;

namespace QLKhachSan.BLL;

public sealed partial class HotelService(HotelRepository repository, UserSession user)
{
    private Task<T> Read<T>(Func<HotelTransaction,Task<T>> action,[CallerMemberName]string caller="") => repository.RunAsync(false,async db=> { await db.RequireUserAsync(user); FunctionPolicy.RequireMethod(user,caller); return await action(db); });
    private Task<T> FinanceRead<T>(Func<HotelTransaction,Task<T>> action,[CallerMemberName]string caller="") => repository.RunAsync(false,async db=> { await db.RequireUserAsync(user); if(!RolePolicy.CanViewFinance(user.Role))throw new BusinessException("Vai trò này không có quyền xem báo cáo tài chính."); FunctionPolicy.RequireMethod(user,caller); return await action(db); });
    private Task<T> OperationsRead<T>(Func<HotelTransaction,Task<T>> action,[CallerMemberName]string caller="") => repository.RunAsync(false,async db=> { await db.RequireUserAsync(user); if(!RolePolicy.CanViewOperations(user.Role))throw new BusinessException("Vai trò này không có quyền xem dữ liệu vận hành."); FunctionPolicy.RequireMethod(user,caller); return await action(db); });
    private Task<T> Write<T>(Func<HotelTransaction,Task<T>> action,[CallerMemberName]string caller="") => repository.RunAsync(true,async db=> { await db.RequireUserAsync(user); if(!RolePolicy.CanOperate(user.Role))throw new BusinessException("Vai trò này chỉ được xem dữ liệu, không được thay đổi nghiệp vụ."); FunctionPolicy.RequireMethod(user,caller); return await action(db); });
    private Task<T> CatalogWrite<T>(Func<HotelTransaction,Task<T>> action,[CallerMemberName]string caller="") => repository.RunAsync(true,async db=> { await db.RequireUserAsync(user); if(!RolePolicy.CanManageCatalog(user.Role))throw new BusinessException("Không có quyền quản lý danh mục."); FunctionPolicy.RequireMethod(user,caller); return await action(db); });
    public Task<DashboardData> DashboardAsync(int revenueDays=7) => Read(async db=>
    {
        if(revenueDays is not (7 or 30))throw new BusinessException("Chọn 7 hoặc 30 ngày cho biểu đồ.");
        var now=await db.NowAsync();
        var operations=RolePolicy.CanViewOperations(user.Role) &&
            (FunctionPolicy.CanAny(user,"rooms") || FunctionPolicy.Can(user,"service.order") || FunctionPolicy.Can(user,"service.manage"));
        var revenue=FunctionPolicy.Can(user,"report.revenue");
        return new DashboardData(operations?await db.RoomsAsync():[],operations?await db.ActiveStaysAsync():[],operations?await db.PendingAsync():[],operations?await db.MenuAsync():[],revenue?await db.RevenueAsync(now.Date):[],now,revenue?await db.RevenueTrendAsync(now.Date.AddDays(1-revenueDays),now.Date.AddDays(1)):[]);
    });
    public Task<List<CustomerSummary>> CustomersAsync(string search) => OperationsRead(db=>db.CustomersAsync(search.Trim()));
    public Task<List<Invoice>> CustomerInvoicesAsync(int customerId) => OperationsRead(db=>db.CustomerInvoicesAsync(customerId));
    public Task<List<ServiceLine>> CustomerOrdersAsync(int customer) => OperationsRead(db=>db.CustomerOrdersAsync(customer));
    public Task<List<Invoice>> InvoicesAsync(DateTime date) => FinanceRead(db=>db.InvoicesAsync(date));
    public Task<List<RevenueItem>> RevenueAsync(DateTime date) => FinanceRead(db=>db.RevenueAsync(date));
    public Task<DailyReport> ReportAsync(DateTime date) => FinanceRead(async db=>new DailyReport(await db.InvoicesAsync(date),await db.RevenueAsync(date)));
    public static GuestInput ValidateGuest(GuestInput guest)
    {
        var g=new GuestInput(guest.Name.Trim(),guest.Phone.Trim(),guest.Identity.Trim().ToUpperInvariant());
        if(g.Name.Length is < 2 or > 100 || g.Name.Any(char.IsControl)) throw new BusinessException("Họ tên phải có 2–100 ký tự hợp lệ.");
        if(!Regex.IsMatch(g.Phone,@"^\+?[0-9]{9,15}$")) throw new BusinessException("Số điện thoại cần 9–15 chữ số; có thể bắt đầu bằng +.");
        if(!Regex.IsMatch(g.Identity,@"^(?:[0-9]{12}|[A-Z][A-Z0-9]{5,19})$")) throw new BusinessException("CCCD cần 12 chữ số; hộ chiếu cần 6–20 ký tự chữ/số và bắt đầu bằng chữ.");
        return g;
    }
    private static void Method(string method)
    {
        if(method is not ("Tiền mặt" or "Chuyển khoản" or "Thẻ POS" or "Công nợ OTA")) throw new BusinessException("Phương thức thanh toán không hợp lệ.");
    }
    private static void RoomUnchanged(Room actual,Room expected,RoomStatus required)
    {
        if(actual.Version!=expected.Version || actual.Status!=required) throw new BusinessException("Phòng đã thay đổi. Vui lòng làm mới trước khi thực hiện.");
    }
    private static void StayUnchanged(Stay actual,Stay expected,StayStatus required)
    {
        if(actual.Version!=expected.Version || actual.Status!=required) throw new BusinessException("Lượt lưu trú đã thay đổi. Vui lòng làm mới.");
    }
    public static DateTime ReservationHoldLimit(DateTime created,bool hasDeposit) => created.AddDays(hasDeposit?15:1);
    public Task<long> CreateStayAsync(Room selected,GuestInput guest,bool reserve,DateTime arrival,int days,bool takeDeposit,string method,decimal? depositAmount=null,DateTime? receiveBy=null,string? reference=null)
    {
        FunctionPolicy.Require(user,reserve?"room.reserve":"room.walkin");
        guest=ValidateGuest(guest); Method(method);
        if(reserve && takeDeposit && method=="Công nợ OTA")throw new BusinessException("Tiền cọc phải thực thu, không thể ghi nhận bằng công nợ OTA.");
        if(!reserve && (takeDeposit || depositAmount.GetValueOrDefault()!=0)) throw new BusinessException("Nhận phòng trực tiếp không thu cọc. Tiền cọc chỉ áp dụng cho đặt phòng trước.");
        if(days is <1 or >60) throw new BusinessException("Số ngày thuê phải từ 1 đến 60.");
        return Write(async db=>
        {
            var room=await db.RoomAsync(selected.Id); RoomUnchanged(room,selected,selected.Status);
            if(room.Status==RoomStatus.BaoTri || (!reserve && room.Status!=RoomStatus.Trong)) throw new BusinessException("Phòng chưa sẵn sàng nhận khách.");
            var now=await db.NowAsync();
            var deposit=takeDeposit?(depositAmount ?? room.Deposit):0;
            ValidateMoney(deposit); if(takeDeposit && deposit==0) throw new BusinessException("Đã chọn thu cọc thì số tiền thực thu phải lớn hơn 0.");
            if(!reserve) arrival=now;
            DateTime? hold=reserve?(receiveBy ?? ReservationHoldLimit(now,deposit>0)):null;
            if(reserve && (arrival<now || arrival>=ReservationHoldLimit(now,deposit>0) || hold<arrival || hold>=arrival.AddDays(days) || hold>ReservationHoldLimit(now,deposit>0))) throw new BusinessException("Ngày đến và hạn nhận phải nằm trong thời gian giữ chỗ: hạn nhận phải trước ngày trả, tối đa 1 ngày khi chưa cọc hoặc 15 ngày khi đã cọc tính từ lúc đặt phòng.");
            await db.EnsureAvailableAsync(room.Id,arrival,arrival.AddDays(days));
            var id=await db.CreateStayAsync(room,guest,reserve,now,arrival,arrival.AddDays(days),deposit,hold,user);
            await db.SetRoomAsync(room,reserve?room.Status:RoomStatus.DangO);
            if(!reserve) await db.StartSegmentAsync(id,room,now);
            await db.PaymentAsync(id,"Deposit",deposit,now,method,"Thu cọc",user,reference);
            await db.AuditAsync(user,reserve?"Reserve":"CheckIn",$"Lượt {id}; phòng {room.Number}");
            return id;
        });
    }
    public Task<List<long>> CreateStaysAsync(IReadOnlyList<Room> selected,GuestInput guest,bool reserve,DateTime arrival,int days,bool takeDeposit,string method,decimal depositPerRoom=0,DateTime? receiveBy=null,string? reference=null)
    {
        FunctionPolicy.Require(user,reserve?"room.reserve":"room.walkin");
        guest=ValidateGuest(guest);Method(method);
        if(selected.Count is <2 or >20 || selected.Select(x=>x.Id).Distinct().Count()!=selected.Count)
            throw new BusinessException("Chọn 2–20 phòng khác nhau.");
        if(days is <1 or >60)throw new BusinessException("Số ngày thuê phải từ 1 đến 60.");
        if(!reserve && (takeDeposit || depositPerRoom!=0))throw new BusinessException("Nhận phòng trực tiếp không thu cọc.");
        if(reserve && takeDeposit && method=="Công nợ OTA")throw new BusinessException("Tiền cọc phải thực thu.");
        ValidateMoney(depositPerRoom);
        if(takeDeposit && depositPerRoom==0)throw new BusinessException("Số tiền cọc mỗi phòng phải lớn hơn 0.");
        return Write(async db=>
        {
            var now=await db.NowAsync();if(!reserve)arrival=now;
            DateTime? hold=reserve?(receiveBy??ReservationHoldLimit(now,takeDeposit)):null;
            if(reserve && (arrival<now || arrival>=ReservationHoldLimit(now,takeDeposit) || hold<arrival || hold>=arrival.AddDays(days) || hold>ReservationHoldLimit(now,takeDeposit)))
                throw new BusinessException("Ngày đến và hạn nhận không hợp lệ với thời gian giữ chỗ.");
            var ids=new List<long>();
            foreach(var expected in selected)
            {
                var room=await db.RoomAsync(expected.Id);RoomUnchanged(room,expected,expected.Status);
                if(room.Status==RoomStatus.BaoTri || (!reserve && room.Status!=RoomStatus.Trong))throw new BusinessException($"Phòng {room.Number} chưa sẵn sàng.");
                await db.EnsureAvailableAsync(room.Id,arrival,arrival.AddDays(days));
                var id=await db.CreateStayAsync(room,guest,reserve,now,arrival,arrival.AddDays(days),takeDeposit?depositPerRoom:0,hold,user);
                await db.SetRoomAsync(room,reserve?room.Status:RoomStatus.DangO);
                if(!reserve)await db.StartSegmentAsync(id,room,now);
                await db.PaymentAsync(id,"Deposit",takeDeposit?depositPerRoom:0,now,method,"Thu cọc",user,reference);
                ids.Add(id);
            }
            await db.AuditAsync(user,reserve?"ReserveMultiple":"CheckInMultiple",$"Khách {guest.Name}; {ids.Count} phòng; lượt {string.Join(',',ids)}");
            return ids;
        });
    }
    public Task CheckInAsync(Stay selected) => Write(async db=>
    {
        var stay=await db.StayAsync(selected.Id); StayUnchanged(stay,selected,StayStatus.Reserved);
        var room=await db.RoomAsync(stay.RoomId); if(room.Status!=RoomStatus.Trong) throw new BusinessException("Phòng chưa trống hoặc chưa dọn xong.");
        var now=await db.NowAsync();
        if(stay.HoldUntil<=now) throw new BusinessException("Đã quá hạn nhận phòng. Khách không đến sẽ không được hoàn cọc. Hãy làm mới để xử lý quá hạn.");
        var duration=stay.Departure-stay.Arrival;
        await db.EnsureAvailableAsync(room.Id,now,now.Add(duration),stay.Id);
        await db.CheckInAsync(stay,now,now.Add(duration));
        await db.StartSegmentAsync(stay.Id,room,now);
        await db.SetRoomAsync(room,RoomStatus.DangO);
        return await db.AuditAsync(user,"CheckIn",$"Lượt {stay.Id}; phòng {room.Number}");
    });
    public Task CancelAsync(Stay selected,string method,decimal? expectedRefund=null,string? reference=null) => Write(async db=>
    {
        Method(method);
        var stay=await db.StayAsync(selected.Id); StayUnchanged(stay,selected,StayStatus.Reserved);
        var room=await db.RoomAsync(stay.RoomId);
        var now=await db.NowAsync();
        var overdue=stay.HoldUntil<=now;
        var refund=overdue?0:stay.Deposit;
        if(expectedRefund is { } expected && expected!=refund) throw new BusinessException("Đã thay đổi số tiền được hoàn do quá hạn nhận phòng. Hãy đóng và mở lại để xác nhận chính sách mới.");
        await db.CloseStayAsync(stay,false,now);
        await db.PaymentAsync(stay.Id,overdue?"Forfeit":"Refund",stay.Deposit,now,overdue?"Không phát sinh tiền":method,overdue?"Không đến nhận phòng trước hạn; không hoàn cọc":"Hoàn cọc khi hủy trước hạn",user,reference);
        await db.SetRoomAsync(room,room.Status);
        return await db.AuditAsync(user,overdue?"NoShow":"Cancel",$"Lượt {stay.Id}; hoàn {refund:N0}; giữ cọc {(overdue?stay.Deposit:0):N0}");
    });
    public Task<int> ExpireReservationsAsync() => Write(async db=>
    {
        var now=await db.NowAsync(); var count=0;
        foreach(var stay in (await db.ActiveStaysAsync()).Where(s=>s.Status==StayStatus.Reserved && s.HoldUntil<=now))
        {
            var room=await db.RoomAsync(stay.RoomId);
            await db.CloseStayAsync(stay,false,now);
            await db.PaymentAsync(stay.Id,"Forfeit",stay.Deposit,now,"Không phát sinh tiền","Không đến nhận phòng trước hạn; không hoàn cọc",user);
            await db.SetRoomAsync(room,room.Status);
            await db.AuditAsync(user,"NoShow",$"Lượt {stay.Id}: quá hạn nhận; cọc không hoàn {stay.Deposit:N0}"); count++;
        }
        return count;
    });
    public Task TransferAsync(Stay selected,Room target) => Write(async db=>
    {
        var stay=await db.StayAsync(selected.Id); StayUnchanged(stay,selected,StayStatus.Occupied);
        var old=await db.RoomAsync(stay.RoomId); var next=await db.RoomAsync(target.Id);
        RoomUnchanged(next,target,RoomStatus.Trong);
        if(old.Status!=RoomStatus.DangO || old.Id==next.Id) throw new BusinessException("Phòng chuyển không phù hợp.");
        var now=await db.NowAsync();
        if(stay.Departure<=now) throw new BusinessException("Hãy gia hạn lượt đã quá ngày trả trước khi chuyển phòng.");
        await db.EnsureAvailableAsync(next.Id,now,stay.Departure,stay.Id);
        if(await db.EndSegmentAsync(stay.Id,now)!=1) throw new BusinessException("Thiếu giai đoạn lưu trú đang mở.");
        await db.StartSegmentAsync(stay.Id,next,now);
        await db.TransferAsync(stay,next.Id);
        await db.SetRoomAsync(old,RoomStatus.DangDon);
        await db.SetRoomAsync(next,RoomStatus.DangO);
        return await db.AuditAsync(user,"Transfer",$"Lượt {stay.Id}: {old.Number} → {next.Number}");
    });
    public Task ExtendAsync(Stay selected,int days) => Write(async db=>
    {
        if(days is <1 or >30) throw new BusinessException("Gia hạn từ 1 đến 30 ngày mỗi lần.");
        var stay=await db.StayAsync(selected.Id); StayUnchanged(stay,selected,StayStatus.Occupied);
        var now=await db.NowAsync();
        var departure=(stay.Departure>now?stay.Departure:now).AddDays(days);
        await db.EnsureAvailableAsync(stay.RoomId,stay.CheckIn ?? now,departure,stay.Id);
        await db.ExtendAsync(stay,departure);
        return await db.AuditAsync(user,"Extend",$"Lượt {stay.Id}; hạn {departure:dd/MM/yyyy HH:mm}");
    });
    public Task AddServicesAsync(Stay selected,IReadOnlyList<OrderInput> items) => Write(async db=>
    {
        if(items.Count is <1 or >100 || items.Any(x=>x.Quantity is <1 or >100)) throw new BusinessException("Giỏ dịch vụ không hợp lệ (1–100 dòng, mỗi dòng 1–100 đơn vị).");
        var stay=await db.StayAsync(selected.Id); StayUnchanged(stay,selected,StayStatus.Occupied);
        var menu=(await db.MenuAsync()).ToDictionary(x=>x.Id); var now=await db.NowAsync();
        foreach(var item in items)
        {
            if(!menu.TryGetValue(item.ServiceId,out var service)) throw new BusinessException("Dịch vụ đã ngừng cung cấp.");
            await db.AddOrderAsync(stay.Id,service,item.Quantity,now,user);
        }
        await db.TouchStayAsync(stay.Id);
        return await db.AuditAsync(user,"Service",$"Lượt {stay.Id}; {items.Count} dòng dịch vụ");
    });
    public Task DeliverAsync(Stay selected) => Write(async db=>
    {
        var stay=await db.StayAsync(selected.Id); StayUnchanged(stay,selected,StayStatus.Occupied);
        foreach(var line in (await db.OrdersAsync(stay.Id)).Where(x=>x.DeliveredQuantity<x.Quantity))
            await db.ConsumeOrderStockAsync(line.Id,line.Quantity-line.DeliveredQuantity,user.Id);
        await db.DeliverAsync(stay.Id,await db.NowAsync()); await db.TouchStayAsync(stay.Id);
        return await db.AuditAsync(user,"Deliver",$"Lượt {stay.Id}: giao tất cả yêu cầu đang chờ");
    });
    public Task SetRoomStatusAsync(Room selected,RoomStatus next) => repository.RunAsync(true,async db=>
    {
        await db.RequireUserAsync(user);
        FunctionPolicy.Require(user,selected.Status==RoomStatus.DangDon?"room.clean":"room.maintain");
        var room=await db.RoomAsync(selected.Id); RoomUnchanged(room,selected,selected.Status);
        var allowed=(room.Status,next) is (RoomStatus.DangDon,RoomStatus.Trong) or (RoomStatus.Trong,RoomStatus.BaoTri) or (RoomStatus.BaoTri,RoomStatus.Trong);
        if(!allowed) throw new BusinessException("Không được chuyển trạng thái phòng theo cách này.");
        if(next==RoomStatus.Trong && room.Status==RoomStatus.DangDon && !RolePolicy.CanOperate(user.Role)) throw new BusinessException("Không có quyền xác nhận dọn phòng.");
        if(room.Status==RoomStatus.BaoTri || next==RoomStatus.BaoTri)
            if(!RolePolicy.CanMaintainRooms(user.Role)) throw new BusinessException("Không có quyền bảo trì phòng.");
        if(next==RoomStatus.BaoTri && (await db.ActiveStaysAsync()).Any(s=>s.RoomId==room.Id)) throw new BusinessException("Phòng còn lịch đặt. Cần xử lý các lượt đặt trước khi bảo trì.");
        await db.SetRoomAsync(room,next);
        return await db.AuditAsync(user,"RoomStatus",$"{room.Number}: {room.Status} → {next}");
    });
    public Task CleanAllAsync(IReadOnlyList<Room> selected) => Write(async db=>
    {
        foreach(var expected in selected)
        {
            var room=await db.RoomAsync(expected.Id); RoomUnchanged(room,expected,RoomStatus.DangDon);
            await db.SetRoomAsync(room,RoomStatus.Trong);
        }
        return await db.AuditAsync(user,"CleanAll",$"Hoàn tất dọn {selected.Count} phòng");
    });
    private static async Task<BillQuote> Quote(HotelTransaction db,Stay stay,DateTime at)
    {
        if(stay.Status!=StayStatus.Occupied || stay.CheckIn is null) throw new BusinessException("Phòng chưa nhận hoặc đã thanh toán.");
        var room=await db.RoomAsync(stay.RoomId);
        if(room.Status!=RoomStatus.DangO) throw new BusinessException("Trạng thái phòng không đồng bộ.");
        var lines=await db.OrdersAsync(stay.Id);
        return new BillQuote(stay,room,at,BillingPolicy.RoomCharge(await db.SegmentsAsync(stay.Id),at),lines.Sum(x=>x.Total),lines);
    }
    public Task<BillQuote> QuoteAsync(Stay selected) => Read(async db=>
    {
        var stay=await db.StayAsync(selected.Id); StayUnchanged(stay,selected,StayStatus.Occupied);
        return await Quote(db,stay,await db.NowAsync());
    });
    public Task<long> CheckoutAsync(BillQuote displayed,string method,string? reference=null) => Write(async db=>
    {
        Method(method);
        var stay=await db.StayAsync(displayed.Stay.Id); StayUnchanged(stay,displayed.Stay,StayStatus.Occupied);
        var now=await db.NowAsync();
        if(displayed.At>now || now-displayed.At>TimeSpan.FromMinutes(10)) throw new BusinessException("Bảng tính tiền đã hết hạn 10 phút. Vui lòng lập lại.");
        var bill=await Quote(db,stay,displayed.At);
        if(bill.Lines.Any(x=>x.Delivered is null)) throw new BusinessException("Còn dịch vụ chưa giao. Hãy xử lý xong trước khi checkout.");
        if(bill.Total!=displayed.Total || bill.ToCollect!=displayed.ToCollect || bill.ToRefund!=displayed.ToRefund) throw new BusinessException("Số tiền đã thay đổi. Hãy lập lại hóa đơn.");
        var id=await db.InvoiceAsync(bill,method,user,now);
        if(await db.EndSegmentAsync(stay.Id,bill.At)!=1) throw new BusinessException("Thiếu giai đoạn lưu trú.");
        await db.CloseStayAsync(stay,true,bill.At);
        await db.PaymentAsync(stay.Id,"Checkout",bill.ToCollect,now,method,$"Hóa đơn {id}",user,reference);
        if(method=="Công nợ OTA" && bill.ToCollect>0)
            await db.AddDebtAsync("AR",$"OTA {reference}",id,now.AddDays(30),bill.ToCollect,$"Cấn trừ OTA theo hóa đơn {id}",user.Id);
        await db.PaymentAsync(stay.Id,"Refund",bill.ToRefund,now,method,$"Hoàn cọc thừa hóa đơn {id}",user);
        await db.SetRoomAsync(bill.Room,RoomStatus.DangDon);
        await db.AuditAsync(user,"Checkout",$"Hóa đơn {id}; lượt {stay.Id}");
        return id;
    });
    public Task<List<BillQuote>> GroupQuoteAsync(IReadOnlyList<Stay> selected) => Read(async db=>
    {
        if(!FunctionPolicy.Can(user,"room.checkout") && !FunctionPolicy.Can(user,"invoice.create"))
            throw new BusinessException("Bạn không được cấp quyền lập hóa đơn.");
        if(selected.Count is <2 or >20 || selected.Select(x=>x.Id).Distinct().Count()!=selected.Count ||
           selected.Select(x=>x.CustomerId).Distinct().Count()!=1)
            throw new BusinessException("Chọn 2–20 lượt ở của cùng một khách để lập phiếu tổng.");
        var now=await db.NowAsync();var quotes=new List<BillQuote>();
        foreach(var expected in selected)
        {
            var stay=await db.StayAsync(expected.Id);StayUnchanged(stay,expected,StayStatus.Occupied);
            quotes.Add(await Quote(db,stay,now));
        }
        return quotes;
    });
    public Task<GroupCheckoutResult> GroupCheckoutAsync(IReadOnlyList<BillQuote> displayed,string method,string? reference=null) => Write(async db=>
    {
        if(!FunctionPolicy.Can(user,"room.checkout") && !FunctionPolicy.Can(user,"invoice.create"))
            throw new BusinessException("Bạn không được cấp quyền lập hóa đơn.");
        Method(method);
        if(method=="Công nợ OTA")throw new BusinessException("Thanh toán gộp chưa hỗ trợ công nợ OTA. Hãy chọn phương thức thu thực tế.");
        if(displayed.Count is <2 or >20 || displayed.Select(x=>x.Stay.Id).Distinct().Count()!=displayed.Count ||
           displayed.Select(x=>x.Stay.CustomerId).Distinct().Count()!=1)
            throw new BusinessException("Các phòng phải thuộc cùng một khách và không được trùng lượt ở.");
        var now=await db.NowAsync();
        if(displayed.Any(x=>x.At>now || now-x.At>TimeSpan.FromMinutes(10)))
            throw new BusinessException("Bảng tính tiền đã hết hạn. Hãy lập lại phiếu tổng.");
        var quotes=new List<BillQuote>();
        foreach(var original in displayed)
        {
            var stay=await db.StayAsync(original.Stay.Id);StayUnchanged(stay,original.Stay,StayStatus.Occupied);
            var bill=await Quote(db,stay,original.At);
            if(bill.Lines.Any(x=>x.Delivered is null))throw new BusinessException($"Phòng {bill.Room.Number} còn dịch vụ chưa giao.");
            if(bill.Total!=original.Total || bill.Stay.Deposit!=original.Stay.Deposit ||
               bill.ToCollect!=original.ToCollect || bill.ToRefund!=original.ToRefund)
                throw new BusinessException($"Số tiền phòng {bill.Room.Number} đã thay đổi. Hãy lập lại phiếu tổng.");
            quotes.Add(bill);
        }
        var guest=quotes[0].Stay.Guest;
        var groupId=await db.NewBillGroupAsync($"Thanh toán khách {guest} · {now:dd/MM/yyyy HH:mm}",user.Id);
        var invoiceIds=new List<long>();
        foreach(var bill in quotes)
        {
            var id=await db.InvoiceAsync(bill,method,user,now);invoiceIds.Add(id);
            if(await db.EndSegmentAsync(bill.Stay.Id,bill.At)!=1)throw new BusinessException("Thiếu giai đoạn lưu trú.");
            await db.CloseStayAsync(bill.Stay,true,bill.At);
            await db.SetRoomAsync(bill.Room,RoomStatus.DangDon);
            await db.AddBillShareAsync(groupId,id,guest,bill.Total);
        }
        var net=quotes.Sum(x=>x.ToCollect-x.ToRefund);
        if(net!=0)await db.PaymentAsync(quotes[0].Stay.Id,net>0?"Checkout":"Refund",Math.Abs(net),now,method,
            $"Thanh toán một lần phiếu tổng #{groupId}; {quotes.Count} phòng",user,reference);
        await db.AuditAsync(user,"GroupCheckout",$"Nhóm {groupId}; khách {guest}; phòng {string.Join(',',quotes.Select(x=>x.Room.Number))}; tổng {quotes.Sum(x=>x.Total):N0}; thu {quotes.Sum(x=>x.ToCollect):N0}; hoàn {quotes.Sum(x=>x.ToRefund):N0}");
        return new GroupCheckoutResult(groupId,invoiceIds,guest,quotes.Sum(x=>x.Total),quotes.Sum(x=>x.Stay.Deposit),
            quotes.Sum(x=>x.ToCollect),quotes.Sum(x=>x.ToRefund));
    });
}
