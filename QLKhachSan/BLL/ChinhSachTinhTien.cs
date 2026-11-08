using QLKhachSan.DTO;

namespace QLKhachSan.BLL;

public static class BillingPolicy
{
    // Round the WHOLE stay to 24-hour blocks; allocate actual time to each room rate.
    // Only the unused remainder of the final block uses the final room's rate.
    public static decimal RoomCharge(IReadOnlyList<Segment> segments, DateTime checkout)
    {
        if(segments.Count==0) throw new BusinessException("Thiếu lịch sử nhận phòng. Không thể lập hóa đơn.");
        var ordered=segments.OrderBy(x=>x.Start).ThenBy(x=>x.Id).ToArray();
        var start=ordered[0].Start;
        if(checkout<start) throw new BusinessException("Giờ trả phòng trước giờ nhận phòng.");
        decimal total=0;
        var previous=start;
        for(var i=0;i<ordered.Length;i++)
        {
            var s=ordered[i]; var end=s.End ?? checkout;
            if(s.Rate<=0 || s.Start!=previous || end<s.Start || end>checkout || (s.End is null && i!=ordered.Length-1))
                throw new BusinessException("Lịch sử đổi phòng không liên tục hoặc không hợp lệ.");
            total += s.Rate * (end.Ticks-s.Start.Ticks) / TimeSpan.TicksPerDay;
            previous=end;
        }
        if(previous!=checkout) throw new BusinessException("Lịch sử phòng chưa đầy đủ đến giờ trả phòng.");
        var elapsed=(decimal)(checkout.Ticks-start.Ticks)/TimeSpan.TicksPerDay;
        var billed=Math.Max(1,decimal.Ceiling(elapsed));
        total += (billed-elapsed)*ordered[^1].Rate;
        return decimal.Round(total,0,MidpointRounding.AwayFromZero);
    }
}
