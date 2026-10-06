using QLKhachSan.BLL;
using QLKhachSan.DTO;

namespace QLKhachSan.GUI;

public partial class ucDashboard
{
    private readonly Label lblHeaderSubtitle=new();
    private readonly Label lblDonSummary=new();
    private readonly Label lblDoiSummary=new();
    private readonly Label lblVipSummary=new();
    private void ApplyAppearance()
    {
        SuspendLayout();Font=AppTheme.Body;BackColor=AppTheme.Canvas;
        pnlHeader.Height=88;pnlHeader.BackColor=Color.White;
        pnlHeader.Controls.Add(new DashboardLogo {Location=new Point(22,22)});
        pnlHeader.Paint+=(_,e)=>{using var edge=new Pen(AppTheme.Border);e.Graphics.DrawLine(edge,0,pnlHeader.Height-1,pnlHeader.Width,pnlHeader.Height-1);};
        lblHeaderTitle.Font=AppTheme.Title;lblHeaderTitle.ForeColor=AppTheme.Ink;
        lblHeaderTitle.UseMnemonic=false;
        lblHeaderTitle.AutoSize=false;lblHeaderTitle.AutoEllipsis=true;lblHeaderTitle.Location=new Point(78,12);lblHeaderTitle.Height=40;
        lblHeaderTitle.TextAlign=ContentAlignment.MiddleLeft;
        lblHeaderSubtitle.Text=$"{user.Username}  •  {RolePolicy.Name(user.Role)}";
        lblHeaderSubtitle.Font=AppTheme.Small;lblHeaderSubtitle.ForeColor=AppTheme.Muted;
        lblHeaderSubtitle.AutoSize=false;lblHeaderSubtitle.Location=new Point(80,55);lblHeaderSubtitle.Height=22;
        pnlHeader.Controls.Add(lblHeaderSubtitle);
        void SizeHeaderText(){var width=Math.Max(230,pnlHeader.ClientSize.Width-flpHeaderRight.Width-104);lblHeaderTitle.Width=width;lblHeaderSubtitle.Width=width;}
        pnlHeader.Resize+=(_,_)=>SizeHeaderText();SizeHeaderText();
        lblClock.ForeColor=AppTheme.Muted;lblClock.Font=AppTheme.Bold;lblClock.Margin=new Padding(0,9,18,0);
        flpHeaderRight.Padding=new Padding(0,24,20,0);
        btnRefresh.Text="Làm mới";btnDangXuat.Text="Đăng xuất";AppTheme.Button(btnRefresh,true);AppTheme.Button(btnDangXuat);
        btnRefresh.Size=new Size(125,38);btnDangXuat.Size=new Size(138,38);
        DecorateButton(btnRefresh,"refresh",Color.White,12);
        DecorateButton(btnDangXuat,"logout",Color.FromArgb(166,75,80),12);
        btnDangXuat.ForeColor=Color.FromArgb(166,75,80);
        tlpCards.Height=132;tlpCards.Padding=new Padding(16,14,16,10);
        var panels=new[]{pnlCard1,pnlCard2,pnlCard3,pnlCard4,pnlCard5};
        var titles=new[]{lblCard1Title,lblCard2Title,lblCard3Title,lblCard4Title,lblCard5Title};
        var values=new[]{lblCard1Value,lblCard2Value,lblCard3Value,lblCard4Value,lblCard5Value};
        var subs=new[]{lblCard1Sub,lblCard2Sub,lblCard3Sub,lblCard4Sub,lblCard5Sub};
        var colors=new[]{AppTheme.Blue,AppTheme.Teal,Color.FromArgb(145,94,121),AppTheme.Amber,AppTheme.Blue};
        for(var i=0;i<panels.Length;i++)
        {
            var index=i;
            var panel=panels[i];
            panel.Margin=new Padding(5,0,5,0);panel.BackColor=Color.White;
            if(panel is DashboardMetricCard metric)metric.Accent=colors[i];
            titles[i].Font=AppTheme.Small;titles[i].ForeColor=AppTheme.Muted;titles[i].Location=new Point(16,14);
            titles[i].AutoSize=false;titles[i].Height=23;titles[i].AutoEllipsis=true;
            values[i].Font=i==4?AppTheme.Title:AppTheme.Metric;values[i].ForeColor=colors[i];values[i].Location=new Point(14,38);
            values[i].AutoSize=false;values[i].Height=36;values[i].AutoEllipsis=true;
            subs[i].Font=AppTheme.Small;subs[i].Location=new Point(16,78);subs[i].ForeColor=AppTheme.Muted;
            subs[i].AutoSize=false;subs[i].Height=22;subs[i].AutoEllipsis=true;
            var icon=i switch {0=>"bed",1=>"calendar",2=>"people",3=>"clock",_=>"chart"};
            var badge=new PictureBox {Image=UiIcons.Create(icon,colors[i],22),SizeMode=PictureBoxSizeMode.CenterImage,Size=new Size(32,32),BackColor=Color.White};
            badge.Location=new Point(Math.Max(0,panel.ClientSize.Width-47),12);
            panel.Controls.Add(badge);badge.BringToFront();
            badge.Disposed+=(_,_)=>badge.Image?.Dispose();
            void SizeCard()
            {
                badge.Left=Math.Max(0,panel.ClientSize.Width-47);
                titles[index].Width=Math.Max(30,panel.ClientSize.Width-65);
                values[index].Width=Math.Max(30,panel.ClientSize.Width-30);
                subs[index].Width=Math.Max(30,panel.ClientSize.Width-30);
            }
            panel.Resize+=(_,_)=>SizeCard();SizeCard();
            if(panel is DashboardMetricCard hoverCard)
            {
                panel.MouseEnter+=(_,_)=>hoverCard.SetHovered(true);
                panel.MouseLeave+=(_,_)=>hoverCard.SetHovered(false);
                foreach(Control child in panel.Controls)
                {
                    child.MouseEnter+=(_,_)=>hoverCard.SetHovered(true);
                    child.MouseLeave+=(_,_)=>hoverCard.SetHovered(panel.ClientRectangle.Contains(panel.PointToClient(Cursor.Position)));
                }
            }
        }
        tlpBody.Padding=new Padding(16,4,16,14);tlpBody.ColumnStyles[0].Width=244;tlpBody.ColumnStyles[2].Width=226;
        pnlLeftTools.BackColor=AppTheme.Navy;pnlLeftTools.Padding=new Padding(12);pnlLeftTools.AutoScroll=true;
        pnlLeftTools.Controls.Add(new DashboardLogo {Location=new Point(14,14),Size=new Size(38,38)});
        pnlLeftTools.Controls.Add(new Label {Text="HOTEL DESK",Font=new Font("Segoe UI Semibold",12F,FontStyle.Bold),ForeColor=Color.White,BackColor=AppTheme.Navy,AutoSize=true,Location=new Point(63,20)});
        lblToolsTitle.Text="THAO TÁC NHANH";lblToolsTitle.ForeColor=Color.FromArgb(244,191,151);lblToolsTitle.Font=AppTheme.Small;lblToolsTitle.Location=new Point(15,64);
        txtTimPhong.Location=new Point(14,94);txtTimPhong.Width=154;txtTimPhong.PlaceholderText="Tìm số phòng";txtTimPhong.Font=AppTheme.Body;
        btnTimPhong.Location=new Point(176,92);btnTimPhong.Size=new Size(48,31);AppTheme.Button(btnTimPhong);btnTimPhong.Text="Tìm";
        btnCheckIn.Text="Nhận phòng";btnGoiDichVu.Text="Gọi dịch vụ";btnBaoDonXong.Text="Xong dọn phòng";
        btnQuanLyKhach.Text="Hồ sơ khách hàng";btnDoiPhong.Text="Chuyển phòng";btnGiaHan.Text="Gia hạn lưu trú";btnBaoTri.Text="Bảo trì phòng";
        pnlLeftTools.Resize+=(_,_)=>ReflowSidebar();
        tabMainView.Font=AppTheme.Bold;tabMainView.ItemSize=new Size(173,42);tabMainView.DrawMode=TabDrawMode.OwnerDrawFixed;
        tabMainView.BackColor=AppTheme.Canvas;
        var tabIcons=new Dictionary<TabPage,Image>
        {
            [tabMatrix]=UiIcons.Create("bed",AppTheme.Blue,17),
            [tabLichTrinh]=UiIcons.Create("calendar",AppTheme.Blue,17),
            [tabThongKe]=UiIcons.Create("chart",AppTheme.Blue,17)
        };
        tabMainView.Disposed+=(_,_)=>{foreach(var icon in tabIcons.Values)icon.Dispose();};
        tabMainView.DrawItem+=(_,e)=>
        {
            var active=e.Index==tabMainView.SelectedIndex;
            using var brush=new SolidBrush(active?Color.White:Color.FromArgb(240,237,231));e.Graphics.FillRectangle(brush,e.Bounds);
            var page=tabMainView.TabPages[e.Index];
            if(tabIcons.TryGetValue(page,out var icon))e.Graphics.DrawImage(icon,e.Bounds.Left+13,e.Bounds.Top+11,17,17);
            var textBounds=new Rectangle(e.Bounds.Left+32,e.Bounds.Top,e.Bounds.Width-35,e.Bounds.Height);
            TextRenderer.DrawText(e.Graphics,page.Text,AppTheme.Bold,textBounds,active?AppTheme.Blue:AppTheme.Muted,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis);
            if(active){using var pen=new Pen(AppTheme.Teal,3);e.Graphics.DrawLine(pen,e.Bounds.Left+16,e.Bounds.Bottom-2,e.Bounds.Right-16,e.Bounds.Bottom-2);}
        };
        tabMainView.SelectedIndexChanged+=(_,_)=>
        {
            UpdateRoomSidePanel();
        };
        grpDatCoc.Font=AppTheme.Bold;grpDatCoc.ForeColor=AppTheme.Ink;grpDatCoc.BackColor=Color.White;grpDatCoc.Padding=new Padding(8,12,8,8);
        grpDatCoc.Text="ĐẶT TRƯỚC • KHÁCH CHỜ NHẬN PHÒNG";
        tlpRoomColumns.SuspendLayout();
        tlpRoomColumns.Controls.Clear();tlpRoomColumns.ColumnStyles.Clear();tlpRoomColumns.ColumnCount=3;
        for(var i=0;i<3;i++)tlpRoomColumns.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100f/3));
        AddRoomSection("PHÒNG ĐƠN",flpDon,lblDonSummary,0,AppTheme.Teal);
        AddRoomSection("PHÒNG ĐÔI",flpDoi,lblDoiSummary,1,AppTheme.Blue);
        AddRoomSection("PHÒNG VIP",flpVIP,lblVipSummary,2,Color.FromArgb(145,94,121));
        tlpRoomColumns.ResumeLayout(true);
        scMatrix.BackColor=AppTheme.Canvas;
        scMatrix.RowStyles[1].Height=190;
        AppTheme.Grid(dgvDatCoc);AppTheme.Grid(dgvLichTrinh);
        pnlRight.BackColor=Color.White;pnlRight.Padding=new Padding(14);
        pnlRight.Paint+=(_,e)=>{using var edge=new Pen(AppTheme.Border);e.Graphics.DrawRectangle(edge,0,0,pnlRight.Width-1,pnlRight.Height-1);};
        lblRightTitle1.Font=AppTheme.Bold;lblRightTitle1.ForeColor=AppTheme.Amber;lblRightTitle1.Location=new Point(16,17);
        lblRightTitle2.Text="Dịch vụ đang chờ";lblRightTitle2.Font=AppTheme.Bold;lblRightTitle2.ForeColor=AppTheme.Ink;lblRightTitle2.Location=new Point(16,242);
        flpDonPhong.Location=new Point(14,50);flpDonPhong.Height=178;
        flpYeuCauKhach.Location=new Point(14,273);
        foreach(var flow in new[]{flpDonPhong,flpYeuCauKhach}){flow.BorderStyle=BorderStyle.None;flow.BackColor=AppTheme.Canvas;}
        pnlRight.Resize+=(_,_)=>
        {
            var width=Math.Max(80,pnlRight.ClientSize.Width-28);
            flpDonPhong.Width=width;flpYeuCauKhach.Width=width;
            flpYeuCauKhach.Height=Math.Max(80,pnlRight.ClientSize.Height-flpYeuCauKhach.Top-15);
        };
        var legend=new FlowLayoutPanel {Dock=DockStyle.Top,Height=32,BackColor=Color.White,Padding=new Padding(8,5,0,0),WrapContents=false};
        foreach(var status in new[]{RoomStatus.Trong,RoomStatus.DaDat,RoomStatus.DangO,RoomStatus.DangDon,RoomStatus.BaoTri})
        {
            legend.Controls.Add(new Label {Text=$"● {Ui.Status(status)}",AutoSize=true,Font=AppTheme.Small,ForeColor=AppTheme.RoomColor(status),Margin=new Padding(0,2,12,0)});
        }
        legend.Controls.Add(new Label {Text="• Có lịch đặt: chấm vàng",AutoSize=true,Font=AppTheme.Small,ForeColor=AppTheme.Muted,Margin=new Padding(0,2,0,0)});
        var roomArea=new Panel {Dock=DockStyle.Fill,BackColor=AppTheme.Canvas};
        scMatrix.Controls.Remove(tlpRoomColumns);
        roomArea.Controls.Add(tlpRoomColumns);
        roomArea.Controls.Add(legend);
        scMatrix.Controls.Add(roomArea,0,0);
        tabThongKe.Padding=new Padding(0);pnlChartContainer.BackColor=AppTheme.Canvas;
        ResumeLayout(true);
    }
    private void AddRoomSection(string title,FlowLayoutPanel flow,Label summary,int column,Color accent)
    {
        var card=new TableLayoutPanel {Dock=DockStyle.Fill,BackColor=Color.White,ColumnCount=1,RowCount=2,Margin=new Padding(5),Padding=new Padding(0)};
        card.RowStyles.Add(new RowStyle(SizeType.Absolute,58));card.RowStyles.Add(new RowStyle(SizeType.Percent,100));
        card.Paint+=(_,e)=>{using var pen=new Pen(AppTheme.Border);e.Graphics.DrawRectangle(pen,0,0,card.Width-1,card.Height-1);};
        var header=new Panel {Dock=DockStyle.Fill,BackColor=Color.FromArgb(246,243,238),Padding=new Padding(14,7,12,5)};
        header.Paint+=(_,e)=>{using var brush=new SolidBrush(accent);e.Graphics.FillRectangle(brush,0,0,4,header.Height);};
        var name=new Label {Text=title,Dock=DockStyle.Top,Height=25,Font=AppTheme.Bold,ForeColor=AppTheme.Ink,TextAlign=ContentAlignment.MiddleLeft};
        summary.Dock=DockStyle.Top;summary.Height=19;summary.Font=AppTheme.Small;summary.ForeColor=AppTheme.Muted;
        header.Controls.Add(summary);header.Controls.Add(name);
        flow.Parent?.Controls.Remove(flow);flow.Dock=DockStyle.Fill;flow.AutoScroll=true;flow.BackColor=Color.White;flow.Padding=new Padding(10,12,8,8);flow.WrapContents=true;
        card.Controls.Add(header,0,0);card.Controls.Add(flow,0,1);
        tlpRoomColumns.Controls.Add(card,column,0);
    }
    private static void ResizeRoomTiles(FlowLayoutPanel flow)
    {
        foreach(var tile in flow.Controls.OfType<RoomTile>())if(tile.Size!=new Size(88,74))tile.Size=new Size(88,74);
    }
    private void ReflowSidebar()
    {
        if(pnlLeftTools.IsDisposed || navigationReady)return;
        pnlLeftTools.AutoScrollPosition=Point.Empty;
        txtTimPhong.Width=Math.Max(90,pnlLeftTools.ClientSize.Width-86);
        btnTimPhong.Left=txtTimPhong.Right+8;
        var buttons=pnlLeftTools.Controls.OfType<Button>().Where(b=>b!=btnTimPhong && b.Visible).OrderBy(b=>b.Top).ToArray();
        var width=Math.Max(140,pnlLeftTools.ClientSize.Width-34);
        var y=txtTimPhong.Visible?140:98;
        foreach(var button in buttons)
        {
            var title=button.AccessibleName??button.Text;
            button.AccessibleName=title;
            button.Location=new Point(12,y);button.Size=new Size(width,38);
            AppTheme.Button(button,button==btnCheckIn||button==btnDatLichPhong);
            button.Font=AppTheme.Small;
            DecorateButton(button,UiIcons.Kind(title),Color.White,11);
            if(button!=btnCheckIn && button!=btnDatLichPhong)
            {
                button.BackColor=AppTheme.Navy;button.ForeColor=Color.FromArgb(217,226,238);
                button.FlatAppearance.BorderSize=0;button.TextAlign=ContentAlignment.MiddleLeft;
                button.FlatAppearance.MouseOverBackColor=Color.FromArgb(76,72,105);
                button.FlatAppearance.MouseDownBackColor=Color.FromArgb(58,54,86);
            }
            button.UseVisualStyleBackColor=false;
            button.Text=title;
            y+=44;
        }
        pnlLeftTools.AutoScrollMinSize=new Size(0,y+12);
    }
    private readonly HashSet<Button> decoratedButtons=[];
    private void DecorateButton(Button button,string kind,Color color,int iconLeft)
    {
        button.Image=null;
        button.TextImageRelation=TextImageRelation.Overlay;
        button.TextAlign=ContentAlignment.MiddleLeft;
        button.Padding=new Padding(iconLeft+27,0,4,0);
        button.AutoEllipsis=true;
        if(!decoratedButtons.Add(button))return;
        var icon=UiIcons.Create(kind,color,18);
        button.Paint+=(_,e)=>e.Graphics.DrawImage(icon,iconLeft,(button.Height-18)/2,18,18);
        button.Disposed+=(_,_)=>icon.Dispose();
    }
}
