using QLKhachSan.BLL;
using QLKhachSan.DTO;

namespace QLKhachSan.GUI;

public partial class ucDashboard
{
    private async Task ShowMyAccountWorkspace()
    {
        var profile=await auth.ProfileAsync(user,user.Id);
        BuildMyAccountWorkspace(profile);
    }

    private void BuildMyAccountWorkspace(UserInfo profile)
    {
        ClearPage(functionPage);
        activeFunction="system.password";
        var heading=PageHeading("Tài khoản của tôi","Cập nhật hồ sơ và bảo mật tài khoản đang đăng nhập");
        AddBackButton(heading,"← Menu tổng",ShowMainMenu);

        var scroll=new Panel {Dock=DockStyle.Fill,AutoScroll=true,BackColor=AppTheme.Canvas};
        var content=new Panel {BackColor=AppTheme.Canvas};
        scroll.Controls.Add(content);
        var details=new DashboardMetricCard {Accent=AppTheme.Blue};
        var security=new DashboardMetricCard {Accent=AppTheme.Amber};
        content.Controls.Add(details);content.Controls.Add(security);

        details.Controls.Add(new Label {Text="Hồ sơ cá nhân",Font=AppTheme.TextFont(16F,FontStyle.Bold),
            ForeColor=AppTheme.Navy,Location=new Point(22,20),Size=new Size(330,34)});
        var avatar=new AvatarBadge {Location=new Point(22,65),Size=new Size(104,104)};
        details.Controls.Add(avatar);
        var displayName=new Label {Font=AppTheme.TextFont(13F,FontStyle.Bold),ForeColor=AppTheme.Ink,
            Location=new Point(145,73),Height=30,AutoEllipsis=true};details.Controls.Add(displayName);
        var identity=new Label {Font=AppTheme.Body,ForeColor=AppTheme.Muted,
            Location=new Point(145,107),Height=43};details.Controls.Add(identity);
        var choose=new RoundedActionButton {Text="CHỌN ẢNH",Location=new Point(145,158),Size=new Size(128,38)};
        var remove=new Button {Text="Bỏ ảnh",Location=new Point(282,158),Size=new Size(105,38)};
        AppTheme.Button(remove);details.Controls.Add(choose);details.Controls.Add(remove);
        details.Controls.Add(new Label {Text="Tên hiển thị",Font=AppTheme.Bold,ForeColor=AppTheme.Ink,
            Location=new Point(22,215),Size=new Size(250,26)});
        var name=Ui.Text(100);name.Dock=DockStyle.None;name.Location=new Point(22,245);name.Height=38;
        details.Controls.Add(name);
        var save=new RoundedActionButton {Text="LƯU THAY ĐỔI",Location=new Point(22,310),Height=46};
        details.Controls.Add(save);
        var saveHint=new Label {Text="Tên và ảnh sẽ cập nhật trên thanh header sau khi lưu.",
            Font=AppTheme.Small,ForeColor=AppTheme.Muted,Location=new Point(22,369),Height=22};
        details.Controls.Add(saveHint);

        security.Controls.Add(new Label {Text="Đổi mật khẩu",Font=AppTheme.TextFont(16F,FontStyle.Bold),
            ForeColor=AppTheme.Navy,Location=new Point(22,20),Size=new Size(330,34)});
        security.Controls.Add(new Label {Text="Nhập mật khẩu hiện tại để xác nhận.",Font=AppTheme.Small,
            ForeColor=AppTheme.Muted,Location=new Point(23,57),Size=new Size(360,22)});
        var oldPassword=Ui.Text(128,true);var newPassword=Ui.Text(5,true);var confirmPassword=Ui.Text(5,true);
        foreach(var (caption,box,labelTop,boxTop) in new[]
        {
            ("Mật khẩu hiện tại",oldPassword,91,117),
            ("Mật khẩu mới (1–5 ký tự)",newPassword,166,192),
            ("Nhập lại mật khẩu mới",confirmPassword,241,267)
        })
        {
            security.Controls.Add(new Label {Text=caption,Font=AppTheme.Bold,ForeColor=AppTheme.Ink,
                Location=new Point(22,labelTop),Size=new Size(300,24)});
            box.Dock=DockStyle.None;box.Location=new Point(22,boxTop);box.Height=36;
            security.Controls.Add(box);
        }
        var changePassword=new RoundedActionButton {Text="ĐỔI MẬT KHẨU",Location=new Point(22,330),Height=46};
        security.Controls.Add(changePassword);

        byte[]? avatarPng=profile.AvatarPng?.ToArray();
        void RefreshProfile()
        {
            avatar.SetProfile(profile.DisplayName,avatarPng);
            displayName.Text=profile.DisplayName;
            identity.Text=$"@{profile.Username}  •  {profile.Role}\nTên đăng nhập và vai trò không thể sửa ở đây.";
            name.Text=profile.DisplayName;
        }
        RefreshProfile();
        choose.Click+=(_,_)=>
        {
            using var picker=new OpenFileDialog {Title="Chọn ảnh đại diện",
                Filter="Ảnh PNG, JPEG, BMP|*.png;*.jpg;*.jpeg;*.bmp",CheckFileExists=true};
            if(picker.ShowDialog(FindForm())!=DialogResult.OK)return;
            try{avatarPng=PrepareAvatar(picker.FileName);avatar.SetProfile(name.Text,avatarPng);}
            catch(Exception ex) when(ex is IOException or ArgumentException or OutOfMemoryException or BusinessException)
            {Ui.Error(this,ex is BusinessException?ex:new BusinessException("Không đọc được ảnh. Hãy chọn PNG hoặc JPEG dưới 10 MB."));}
        };
        remove.Click+=(_,_)=>{avatarPng=null;avatar.SetProfile(name.Text,null);};
        name.TextChanged+=(_,_)=>avatar.SetProfile(name.Text,avatarPng);
        save.Click+=async (_,_)=>await Run(async()=>
        {
            await auth.SaveProfileAsync(user,profile,name.Text,avatarPng);
            profile=await auth.ProfileAsync(user,user.Id);
            avatarPng=profile.AvatarPng?.ToArray();
            RefreshProfile();UpdateHeaderAccountProfile();
            MessageBox.Show(this,"Đã cập nhật hồ sơ.","Tài khoản của tôi");
        });
        changePassword.Click+=async (_,_)=>await Run(async()=>
        {
            if(newPassword.Text!=confirmPassword.Text)
                throw new BusinessException("Hai mật khẩu mới không khớp.");
            await auth.ChangePasswordAsync(user,oldPassword.Text,newPassword.Text);
            oldPassword.Clear();newPassword.Clear();confirmPassword.Clear();
            MessageBox.Show(this,"Đã đổi mật khẩu.","Tài khoản của tôi");
        });

        void LayoutAccount()
        {
            var width=Math.Max(610,scroll.ClientSize.Width-SystemInformation.VerticalScrollBarWidth-4);
            content.Width=width;
            var innerWidth=width-48;
            var twoColumns=innerWidth>=900;
            var cardWidth=twoColumns?(innerWidth-18)/2:innerWidth;
            details.SetBounds(24,18,cardWidth,410);
            security.SetBounds(twoColumns?42+cardWidth:24,twoColumns?18:448,cardWidth,410);
            displayName.Width=Math.Max(100,cardWidth-168);
            identity.Width=Math.Max(100,cardWidth-168);
            name.Width=cardWidth-44;save.Width=cardWidth-44;
            saveHint.Width=cardWidth-44;
            oldPassword.Width=cardWidth-44;newPassword.Width=cardWidth-44;
            confirmPassword.Width=cardWidth-44;changePassword.Width=cardWidth-44;
            content.Height=twoColumns?448:884;
            scroll.AutoScrollMinSize=new Size(0,content.Height);
        }
        scroll.Resize+=(_,_)=>LayoutAccount();
        functionPage.Controls.Add(scroll);functionPage.Controls.Add(heading);
        LayoutAccount();ActivatePage(functionPage);UpdateNavigationTitle();
    }
}
