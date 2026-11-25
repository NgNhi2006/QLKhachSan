using QLKhachSan.BLL;
using QLKhachSan.DAL;
using QLKhachSan.DTO;

namespace QLKhachSan.GUI;

public partial class ucDashboard
{
    private async Task<bool> ShowMultipleBooking(IReadOnlyList<Room> rooms,bool reserve,DateTime arrival,int days)
    {
        using var dialog=new InputDialog(reserve?"Đặt phòng":"Nhận phòng",760,720,true);
        dialog.Note($"{rooms.Count} phòng: {string.Join(", ",rooms.Select(x=>x.Number))}\nMột khách đứng tên các phòng đã chọn.");
        var name=Ui.Text();var phone=Ui.Text(20);var identity=Ui.Text(20);
        dialog.Add("Họ và tên",name);dialog.Add("Số điện thoại",phone);dialog.Add("CCCD (12 số) / Hộ chiếu",identity);
        var date=Ui.DatePicker(arrival);var duration=Ui.Number(60,days);
        if(reserve)dialog.Add("Ngày giờ dự kiến đến",date);
        dialog.Add("Số ngày thuê dự kiến",duration);
        var deposit=new CheckBox {Text="Đã thu cọc cho mỗi phòng",AutoSize=true};
        var amount=Ui.Money();amount.Value=Math.Min(amount.Maximum,rooms.Min(x=>x.Deposit));
        var receiveBy=Ui.DatePicker(ServerNow.AddDays(1));
        void UpdateHold()
        {
            if(!reserve)return;
            var limit=HotelService.ReservationHoldLimit(ServerNow,deposit.Checked && amount.Value>0);
            var departure=date.Value.AddDays((int)duration.Value);
            receiveBy.Value=limit<departure?limit:departure.AddMinutes(-1);
        }
        if(reserve)
        {
            dialog.Add("Tiền cọc",deposit);dialog.Add("Số tiền cọc mỗi phòng (đ)",amount);
            dialog.Add("Hạn cuối nhận phòng",receiveBy);
            deposit.CheckedChanged+=(_,_)=>UpdateHold();amount.ValueChanged+=(_,_)=>UpdateHold();
            date.ValueChanged+=(_,_)=>UpdateHold();duration.ValueChanged+=(_,_)=>UpdateHold();UpdateHold();
        }
        var method=Ui.Combo(new[]{"Tiền mặt","Chuyển khoản","Thẻ POS","Công nợ OTA"});
        var reference=Ui.Text(100);
        var transferContent="COC-"+Guid.NewGuid().ToString("N")[..10].ToUpperInvariant();
        reference.PlaceholderText="Nội dung chuyển khoản hoặc mã giao dịch POS";
        if(reserve)
        {
            dialog.Add("Hình thức thu cọc",method);
            dialog.Add("Nội dung chuyển khoản / mã POS",reference);
            dialog.Note("Khi chuyển khoản, quét QR rồi kiểm tra tiền đã vào tài khoản trước khi đánh dấu đã thu cọc.");
            method.SelectedIndexChanged+=(_,_)=>
            {
                if((string?)method.SelectedItem=="Chuyển khoản" && string.IsNullOrWhiteSpace(reference.Text))
                    reference.Text=transferContent;
            };
            PaymentQr.Add(dialog,method,()=>deposit.Checked?amount.Value*rooms.Count:0,
                ()=>reference.Text,amount,reference,null,null,deposit);
        }
        dialog.Action(reserve?"LƯU ĐẶT PHÒNG":"XÁC NHẬN NHẬN PHÒNG",async()=>
        {
            var guest=new GuestInput(name.Text,phone.Text,identity.Text);
            var paid=reserve && deposit.Checked;
            var paidAmount=paid?amount.Value:0;
            var created=new List<long>();
            if(rooms.Count==1)
                await Changed(async()=>created.Add(await service.CreateStayAsync(rooms[0],guest,reserve,date.Value,(int)duration.Value,
                    paid,(string)method.SelectedItem!,paidAmount,reserve?receiveBy.Value:null,reference.Text)));
            else
                await Changed(async()=>created.AddRange(await service.CreateStaysAsync(rooms,guest,reserve,date.Value,(int)duration.Value,
                    paid,(string)method.SelectedItem!,paidAmount,reserve?receiveBy.Value:null,reference.Text)));
            recentBookingIds.Clear();
            recentBookingIds.UnionWith(created);
        });
        var completed=dialog.ShowDialog(this)==DialogResult.OK;
        if(completed)
        {
            ShowSavedBookingWindow();
        }
        return completed;
    }
}
