using QLKhachSan.DAL;
using QLKhachSan.DTO;

namespace QLKhachSan.BLL;

public sealed partial class HotelService
{
    public Task<List<ServiceStockAvailability>> ServiceStockAvailabilityAsync() => Task.FromResult(new List<ServiceStockAvailability>());
    public Task<List<RentalOut>> RentalOutAsync() => Task.FromResult(new List<RentalOut>());
    public Task ReturnRentalAsync(long orderId,int quantity) => Task.CompletedTask;

    private static Task<List<ServiceStockAvailability>> LoadServiceStockAsync(HotelTransaction db) => Task.FromResult(new List<ServiceStockAvailability>());
    private static Task EnsureServiceStockAsync(HotelTransaction db,IEnumerable<(int ServiceId,int Quantity)> additions) => Task.CompletedTask;
}
