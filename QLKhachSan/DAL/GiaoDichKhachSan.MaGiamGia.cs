using QLKhachSan.DTO;

namespace QLKhachSan.DAL;

public sealed partial class HotelTransaction
{
    public Task<InvoiceDiscount?> InvoiceDiscountAsync(long invoice) => Task.FromResult<InvoiceDiscount?>(null);
    public Task<List<DiscountCode>> DiscountCodesAsync() => Task.FromResult(new List<DiscountCode>());
    public Task<DiscountCode?> DiscountCodeAsync(string code) => Task.FromResult<DiscountCode?>(null);
    public Task<int> SaveDiscountCodeAsync(DiscountCode code,bool create) => Task.FromResult(0);
    public Task<int> RedeemDiscountAsync(long invoice,string code,decimal amount) => Task.FromResult(0);
}
