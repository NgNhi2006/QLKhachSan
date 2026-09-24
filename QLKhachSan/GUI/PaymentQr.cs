using QLKhachSan.DAL;

namespace QLKhachSan.GUI;

internal static class PaymentQr
{
    public static void Add(InputDialog dialog, ComboBox method, Func<decimal> amount, Func<string> reference, NumericUpDown? amountInput=null, TextBox? referenceInput=null, ComboBox? referenceChoice=null)
    {
        var settings=AppSettings.Load();
        var configured=!string.IsNullOrWhiteSpace(settings.BankCode)
            && !string.IsNullOrWhiteSpace(settings.BankAccount)
            && !string.IsNullOrWhiteSpace(settings.BankAccountName);
        var panel=new TableLayoutPanel {Height=configured?240:68,Dock=DockStyle.Top,ColumnCount=1,RowCount=configured?2:1,Margin=Padding.Empty};
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute,configured?62:60));
        if(configured)panel.RowStyles.Add(new RowStyle(SizeType.Absolute,174));
        var info=new Label {Dock=DockStyle.Fill,AutoEllipsis=true,TextAlign=ContentAlignment.TopLeft};
        panel.Controls.Add(info,0,0);
        PictureBox? picture=null;
        if(configured)
        {
            picture=new PictureBox {Dock=DockStyle.Fill,Margin=new Padding(0,0,0,0),SizeMode=PictureBoxSizeMode.Zoom};
            panel.Controls.Add(picture,0,1);
            picture.LoadCompleted+=(_,e)=>{if(e.Error is not null && !dialog.IsDisposed)info.Text+="\nKhông tải được QR; dùng thông tin tài khoản bên trên.";};
        }
        var caption=dialog.Add("QR chuyển khoản ngân hàng",panel);
        var timer=new System.Windows.Forms.Timer {Interval=450};
        timer.Tick+=(_,_)=>
        {
            timer.Stop();
            if(dialog.IsDisposed || picture is null)return;
            picture.CancelAsync();
            var content=reference().Trim();
            var url=$"https://img.vietqr.io/image/{Uri.EscapeDataString(settings.BankCode)}-{Uri.EscapeDataString(settings.BankAccount)}-qr_only.png?amount={amount():0}&addInfo={Uri.EscapeDataString(content)}&accountName={Uri.EscapeDataString(settings.BankAccountName)}";
            try{picture.LoadAsync(url);}catch(InvalidOperationException){info.Text+="\nKhông tải được ảnh QR; dùng thông tin tài khoản bên trên.";}
        };
        void Refresh()
        {
            var transfer=(string?)method.SelectedItem=="Chuyển khoản";
            var show=transfer && amount()>0;
            panel.Visible=transfer;
            if(caption is not null)caption.Visible=transfer;
            if(!configured)
                info.Text=transfer?"Chưa cấu hình BankCode, BankAccount và BankAccountName trong appsettings.json.":"Chọn Chuyển khoản để hiển thị QR ngân hàng.";
            else
                info.Text=show?$"STK: {settings.BankAccount} • {settings.BankAccountName}\nSố tiền: {amount():N0} đ\nNội dung: {reference()}":"Chọn Chuyển khoản và nhập số tiền lớn hơn 0 để hiển thị QR.";
            if(picture is not null)
            {
                picture.Visible=show;
                if(show){timer.Stop();timer.Start();}
                else{timer.Stop();picture.CancelAsync();}
            }
        }
        method.SelectedIndexChanged+=(_,_)=>Refresh();
        if(amountInput is not null)amountInput.ValueChanged+=(_,_)=>Refresh();
        if(referenceInput is not null)referenceInput.TextChanged+=(_,_)=>Refresh();
        if(referenceChoice is not null)referenceChoice.SelectedIndexChanged+=(_,_)=>Refresh();
        dialog.FormClosed+=(_,_)=>
        {
            timer.Stop();timer.Dispose();
            if(picture is not null){picture.CancelAsync();var image=picture.Image;picture.Image=null;image?.Dispose();}
        };
        Refresh();
    }
}





