using QLKhachSan.DAL;
using QLKhachSan.DTO;

namespace QLKhachSan.BLL;

public sealed partial class HotelService
{
    private static decimal DiscountAmount(DiscountCode? code,decimal gross,DateTime now)
    {
        if(code is null || !code.Active || now<code.StartsAt || now>code.EndsAt ||
           code.UsageLimit is { } limit && code.UsedCount>=limit)
            throw new BusinessException("Mã giảm giá không tồn tại, hết hạn hoặc đã hết lượt sử dụng.");
        if(gross<=0 || gross>2_000_000_000m)throw new BusinessException("Bill chưa có số tiền hợp lệ để áp mã.");
        var amount=Math.Min(gross,code.Kind=="Percent"
            ?decimal.Round(gross*code.Value/100,0,MidpointRounding.AwayFromZero)
            :code.Value);
        if(amount<=0)throw new BusinessException("Giá trị mã quá nhỏ so với tổng bill.");
        return amount;
    }

    public Task<decimal> PreviewDiscountAsync(string code,decimal gross) =>
        string.IsNullOrWhiteSpace(code) ? Task.FromResult(0m) :
        Task.FromException<decimal>(new BusinessException("Mã giảm giá không có trong bản CSDL gọn."));

    public Task<List<DiscountCode>> DiscountCodesAsync() => Task.FromResult(new List<DiscountCode>());
    public Task<InvoiceDiscount?> InvoiceDiscountAsync(long invoice) => Task.FromResult<InvoiceDiscount?>(null);

    public Task<int> SaveDiscountCodeAsync(DiscountCode code,bool create) =>
        Task.FromException<int>(new BusinessException("Mã giảm giá không có trong bản CSDL gọn."));

    private async Task<BillQuote> ApplyDiscountAsync(HotelTransaction db,BillQuote bill)
    {
        if(string.IsNullOrWhiteSpace(bill.DiscountCode))
        {
            if(bill.DiscountAmount!=0)throw new BusinessException("Bill có số giảm giá nhưng thiếu mã.");
            return bill;
        }
        throw new BusinessException("Mã giảm giá không có trong bản CSDL gọn.");
    }

    private async Task<BillQuote> MaterializeCatalogLinesAsync(HotelTransaction db,BillQuote bill,DateTime now)
    {
        if(bill.DraftLines is not { } draft || draft.All(x=>x.ServiceId is null))return bill;
        var menu=await db.MenuAsync();
        var result=new List<BillDraftLine>(draft.Count);
        foreach(var line in draft)
        {
            if(line.ServiceId is not { } serviceId){result.Add(line);continue;}
            var item=menu.SingleOrDefault(x=>x.Id==serviceId);
            if(item is null || line.OrderId is not null || line.Category!="Service" ||
               line.Name!=item.Name || line.UnitPrice!=item.Price || line.Quantity is <1 or >100)
                throw new BusinessException($"Dịch vụ {line.Name} đã thay đổi trong danh mục. Hãy lập lại bill.");
            var order=await db.AddOrderAsync(bill.Stay.Id,item,line.Quantity,now,user);
            await db.ConsumeOrderStockAsync(order,line.Quantity,user.Id);
            if(await db.DeliverOrderAsync(order,line.Quantity,now)!=1)
                throw new BusinessException($"Không thể ghi nhận dịch vụ {line.Name}.");
            result.Add(line with {OrderId=order});
        }
        return bill with {DraftLines=result};
    }
}
