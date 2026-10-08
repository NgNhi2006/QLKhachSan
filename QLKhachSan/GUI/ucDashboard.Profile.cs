using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using QLKhachSan.DTO;

namespace QLKhachSan.GUI;

public partial class ucDashboard
{
    private async Task<bool> EditStaffProfile(UserInfo selected,Form owner)
    {
        using var dialog=new InputDialog($"Hồ sơ nhân viên · {selected.Username}",640,520);
        var name=Ui.Text(100);name.Text=selected.DisplayName;
        dialog.Add("Tên hiển thị",name);
        var avatar=selected.AvatarPng?.ToArray();
        var previewCard=new Panel {Height=158,BackColor=Color.White};
        var preview=new AvatarBadge {Location=new Point(15,13),Size=new Size(112,112)};
        preview.SetProfile(name.Text,avatar);
        previewCard.Controls.Add(preview);
        var choose=new Button {Text="CHỌN ẢNH",Location=new Point(150,27),Size=new Size(160,42)};
        AppTheme.Button(choose,true);previewCard.Controls.Add(choose);
        var remove=new Button {Text="BỎ ẢNH",Location=new Point(150,79),Size=new Size(160,42)};
        AppTheme.Button(remove);previewCard.Controls.Add(remove);
        name.TextChanged+=(_,_)=>preview.SetProfile(name.Text,avatar);
        choose.Click+=(_,_)=>
        {
            using var picker=new OpenFileDialog
            {
                Title="Chọn ảnh đại diện",
                Filter="Ảnh PNG, JPEG, BMP|*.png;*.jpg;*.jpeg;*.bmp",
                CheckFileExists=true
            };
            if(picker.ShowDialog(dialog)!=DialogResult.OK)return;
            try
            {
                avatar=PrepareAvatar(picker.FileName);
                preview.SetProfile(name.Text,avatar);
            }
            catch(Exception ex) when(ex is IOException or ArgumentException or OutOfMemoryException or BusinessException)
            {
                Ui.Error(dialog,ex is BusinessException?ex:new BusinessException("Không đọc được ảnh. Hãy chọn PNG hoặc JPEG dưới 10 MB."));
            }
        };
        remove.Click+=(_,_)=>{avatar=null;preview.SetProfile(name.Text,null);};
        dialog.Add("Ảnh đại diện",previewCard,158);
        dialog.Note("Ảnh được cắt vuông, thu nhỏ và lưu trong hồ sơ nhân viên. Tên đăng nhập vẫn giữ nguyên.");
        dialog.Action("LƯU HỒ SƠ",async()=>await auth.SaveProfileAsync(user,selected,name.Text,avatar));
        return dialog.ShowDialog(owner)==DialogResult.OK;
    }

    private static byte[] PrepareAvatar(string path)
    {
        if(new FileInfo(path).Length>10_000_000)
            throw new BusinessException("Ảnh gốc không được vượt quá 10 MB.");
        using var source=Image.FromFile(path);
        if(source.Width<32 || source.Height<32 || source.Width>8000 || source.Height>8000)
            throw new BusinessException("Ảnh cần có kích thước từ 32 đến 8000 px mỗi chiều.");
        using var square=new Bitmap(160,160,PixelFormat.Format32bppArgb);
        using(var graphics=Graphics.FromImage(square))
        {
            graphics.Clear(Color.White);
            graphics.InterpolationMode=InterpolationMode.HighQualityBicubic;
            graphics.SmoothingMode=SmoothingMode.AntiAlias;
            var side=Math.Min(source.Width,source.Height);
            graphics.DrawImage(source,new Rectangle(0,0,160,160),
                new Rectangle((source.Width-side)/2,(source.Height-side)/2,side,side),GraphicsUnit.Pixel);
        }
        using var output=new MemoryStream();
        square.Save(output,ImageFormat.Png);
        if(output.Length>262144)throw new BusinessException("Ảnh sau xử lý vượt quá 256 KB.");
        return output.ToArray();
    }
}
