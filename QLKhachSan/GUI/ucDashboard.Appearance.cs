using QLKhachSan.DTO;

namespace QLKhachSan.GUI;

public partial class ucDashboard
{
    private readonly Label lblDonSummary=new();
    private readonly Label lblDoiSummary=new();
    private readonly Label lblVipSummary=new();
    private readonly HashSet<Button> paintedSidebarButtons=[];
    private void ApplyAppearance()
    {
        SuspendLayout();Font=AppTheme.Body;BackColor=AppTheme.Canvas;
        pnlHeader.Height=78;pnlHeader.BackColor=Color.White;
        lblHeaderTitle.Font=AppTheme.Title;lblHeaderTitle.ForeColor=AppTheme.Ink;lblHeaderTitle.Location=new Point(22,12);
        var subtitle=new Label {Text="Không gian làm việc theo vai trò",Font=AppTheme.Small,ForeColor=AppTheme.Muted,AutoSize=true,Location=new Point(24,48)};
        pnlHeader.Controls.Add(subtitle);lblClock.ForeColor=AppTheme.Muted;lblClock.Font=AppTheme.Small;
        flpHeaderRight.Padding=new Padding(0,20,18,0);
        btnRefresh.Text="Làm mới";btnDangXuat.Text="Đăng xuất";AppTheme.Button(btnRefresh);AppTheme.Button(btnDangXuat);
        btnRefresh.Width=128;btnDangXuat.Width=142;
        DecorateButton(btnRefresh,"refresh",AppTheme.Ink,12);
        DecorateButton(btnDangXuat,"logout",Color.FromArgb(180,65,73),12);
        btnDangXuat.ForeColor=Color.FromArgb(180,65,73);
        tlpCards.Height=116;tlpCards.Padding=new Padding(16,10,16,8);
        var panels=new[]{pnlCard1,pnlCard2,pnlCard3,pnlCard4,pnlCard5};
        var titles=new[]{lblCard1Title,lblCard2Title,lblCard3Title,lblCard4Title,lblCard5Title};
        var values=new[]{lblCard1Value,lblCard2Value,lblCard3Value,lblCard4Value,lblCard5Value};
        var subs=new[]{lblCard1Sub,lblCard2Sub,lblCard3Sub,lblCard4Sub,lblCard5Sub};
        var colors=new[]{AppTheme.Blue,AppTheme.Teal,AppTheme.Blue,AppTheme.Amber,AppTheme.Teal};
        for(var i=0;i<panels.Length;i++)
        {
            var panel=panels[i];
            panel.Margin=new Padding(5,0,5,0);panel.BackColor=Color.White;
            titles[i].Font=AppTheme.Small;titles[i].ForeColor=AppTheme.Muted;titles[i].Location=new Point(14,10);
            values[i].Font=i==4?AppTheme.Title:AppTheme.Metric;values[i].ForeColor=colors[i];values[i].Location=new Point(12,30);
            subs[i].Font=AppTheme.Small;subs[i].Location=new Point(14,72);subs[i].ForeColor=AppTheme.Muted;
            var icon=i switch {0=>"bed",1=>"calendar",2=>"people",3=>"clock",_=>"chart"};
            var badge=new PictureBox {Image=UiIcons.Create(icon,colors[i],22),SizeMode=PictureBoxSizeMode.CenterImage,Size=new Size(32,32),BackColor=Color.FromArgb(243,246,251),Anchor=AnchorStyles.Top|AnchorStyles.Right};
            badge.Location=new Point(Math.Max(0,panel.ClientSize.Width-44),10);
            panel.Controls.Add(badge);badge.BringToFront();
            panel.Resize+=(_,_)=>badge.Left=Math.Max(0,panel.ClientSize.Width-44);
        }
        tlpBody.Padding=new Padding(16,4,16,12);tlpBody.ColumnStyles[0].Width=255;tlpBody.ColumnStyles[2].Width=220;
        pnlLeftTools.BackColor=AppTheme.Navy;pnlLeftTools.Padding=new Padding(12);pnlLeftTools.AutoScroll=true;
        lblToolsTitle.Text="ĐIỀU HÀNH";lblToolsTitle.ForeColor=Color.FromArgb(148,168,196);lblToolsTitle.Font=AppTheme.Small;lblToolsTitle.Location=new Point(14,14);
        txtTimPhong.Location=new Point(14,42);txtTimPhong.Width=128;txtTimPhong.PlaceholderText="Số phòng…";txtTimPhong.Font=AppTheme.Body;
        btnTimPhong.Location=new Point(148,40);btnTimPhong.Size=new Size(51,30);AppTheme.Button(btnTimPhong);btnTimPhong.Text="Tìm";
        btnCheckIn.Text="Nhận phòng";btnGoiDichVu.Text="Gọi dịch vụ";btnBaoDonXong.Text="Hoàn tất dọn phòng";
        btnQuanLyKhach.Text="Hồ sơ khách hàng";btnDoiPhong.Text="Chuyển phòng";btnGiaHan.Text="Gia hạn lưu trú";btnBaoTri.Text="Bảo trì phòng";
        pnlLeftTools.Resize+=(_,_)=>ReflowSidebar();
        tabMainView.Font=AppTheme.Bold;tabMainView.ItemSize=new Size(215,40);tabMainView.DrawMode=TabDrawMode.OwnerDrawFixed;
        var tabIcons=new Dictionary<TabPage,Image>
        {
            [tabMatrix]=UiIcons.Create("bed",AppTheme.Blue,17),
            [tabLichTrinh]=UiIcons.Create("calendar",AppTheme.Blue,17),
            [tabThongKe]=UiIcons.Create("chart",AppTheme.Blue,17)
        };
        tabMainView.DrawItem+=(_,e)=>
        {
            var active=e.Index==tabMainView.SelectedIndex;
            using var brush=new SolidBrush(active?Color.White:AppTheme.Canvas);e.Graphics.FillRectangle(brush,e.Bounds);
            var page=tabMainView.TabPages[e.Index];
            if(tabIcons.TryGetValue(page,out var icon))e.Graphics.DrawImage(icon,e.Bounds.Left+13,e.Bounds.Top+11,17,17);
            var textBounds=new Rectangle(e.Bounds.Left+32,e.Bounds.Top,e.Bounds.Width-35,e.Bounds.Height);
            TextRenderer.DrawText(e.Graphics,page.Text,AppTheme.Bold,textBounds,active?AppTheme.Blue:AppTheme.Muted,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis);
            if(active){using var pen=new Pen(AppTheme.Blue,3);e.Graphics.DrawLine(pen,e.Bounds.Left+20,e.Bounds.Bottom-2,e.Bounds.Right-20,e.Bounds.Bottom-2);}
        };
        tabMainView.SelectedIndexChanged+=(_,_)=>
        {
            var chart=tabMainView.SelectedTab==tabThongKe;
            pnlRight.Visible=!chart;tlpBody.ColumnStyles[2].Width=chart?0:220;
        };
        grpDatCoc.Font=AppTheme.Bold;grpDatCoc.ForeColor=AppTheme.Ink;grpDatCoc.BackColor=Color.White;grpDatCoc.Padding=new Padding(8,12,8,8);
        grpDatCoc.Text="ĐẶT TRƯỚC • KHÁCH CHỜ NHẬN PHÒNG";
        tlpRoomColumns.SuspendLayout();
        tlpRoomColumns.Controls.Clear();tlpRoomColumns.ColumnStyles.Clear();tlpRoomColumns.ColumnCount=3;
        for(var i=0;i<3;i++)tlpRoomColumns.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100f/3));
        AddRoomSection("PHÒNG ĐƠN",flpDon,lblDonSummary,0,AppTheme.Teal);
        AddRoomSection("PHÒNG ĐÔI",flpDoi,lblDoiSummary,1,AppTheme.Blue);
        AddRoomSection("PHÒNG VIP",flpVIP,lblVipSummary,2,Color.FromArgb(139,92,246));
        tlpRoomColumns.ResumeLayout(true);
        scMatrix.FixedPanel=FixedPanel.None;scMatrix.SplitterWidth=10;scMatrix.BackColor=AppTheme.Canvas;
        void ResizeRooms(){if(scMatrix.Height>300)scMatrix.SplitterDistance=Math.Max(200,scMatrix.Height-190);}
        scMatrix.Resize+=(_,_)=>ResizeRooms();ResizeRooms();
        AppTheme.Grid(dgvDatCoc);AppTheme.Grid(dgvLichTrinh);
        pnlRight.BackColor=Color.White;lblRightTitle1.Font=AppTheme.Bold;lblRightTitle1.ForeColor=AppTheme.Amber;
        lblRightTitle2.Text="Dịch vụ đang chờ";lblRightTitle2.Font=AppTheme.Bold;lblRightTitle2.ForeColor=AppTheme.Ink;
        foreach(var flow in new[]{flpDonPhong,flpYeuCauKhach}){flow.BorderStyle=BorderStyle.None;flow.BackColor=AppTheme.Canvas;}
        var legend=new FlowLayoutPanel {Dock=DockStyle.Top,Height=32,BackColor=Color.White,Padding=new Padding(8,5,0,0),WrapContents=false};
        foreach(var status in new[]{RoomStatus.Trong,RoomStatus.DaDat,RoomStatus.DangO,RoomStatus.DangDon,RoomStatus.BaoTri})
        {
            legend.Controls.Add(new Label {Text=$"● {Ui.Status(status)}",AutoSize=true,Font=AppTheme.Small,ForeColor=AppTheme.RoomColor(status),Margin=new Padding(0,2,12,0)});
        }
        legend.Controls.Add(new Label {Text="• Có lịch đặt: chấm vàng",AutoSize=true,Font=AppTheme.Small,ForeColor=AppTheme.Muted,Margin=new Padding(0,2,0,0)});
        scMatrix.Panel1.Controls.Add(legend);
        tabThongKe.Padding=new Padding(0);pnlChartContainer.BackColor=AppTheme.Canvas;
        ResumeLayout(true);
    }
    private void AddRoomSection(string title,FlowLayoutPanel flow,Label summary,int column,Color accent)
    {
        var card=new TableLayoutPanel {Dock=DockStyle.Fill,BackColor=Color.White,ColumnCount=1,RowCount=2,Margin=new Padding(6),Padding=new Padding(0)};
        card.RowStyles.Add(new RowStyle(SizeType.Absolute,58));card.RowStyles.Add(new RowStyle(SizeType.Percent,100));
        card.Paint+=(_,e)=>{using var pen=new Pen(Color.FromArgb(222,229,239));e.Graphics.DrawRectangle(pen,0,0,card.Width-1,card.Height-1);};
        var header=new Panel {Dock=DockStyle.Fill,BackColor=Color.FromArgb(248,250,254),Padding=new Padding(14,7,12,5)};
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
        if(pnlLeftTools.IsDisposed)return;
        pnlLeftTools.AutoScrollPosition=Point.Empty;
        var buttons=pnlLeftTools.Controls.OfType<Button>().Where(b=>b!=btnTimPhong && b.Visible).OrderBy(b=>b.Top).ToArray();
        var width=Math.Max(140,pnlLeftTools.ClientSize.Width-34);
        var y=txtTimPhong.Visible?88:52;
        foreach(var button in buttons)
        {
            var title=button.AccessibleName??button.Text;
            button.AccessibleName=title;
            button.Location=new Point(12,y);button.Size=new Size(width,36);
            AppTheme.Button(button,button==btnCheckIn||button==btnDatLichPhong);
            button.Font=AppTheme.Small;
            DecorateButton(button,UiIcons.Kind(title),Color.White,8);
            if(button!=btnCheckIn && button!=btnDatLichPhong)
            {
                button.BackColor=AppTheme.Navy;button.ForeColor=Color.FromArgb(217,226,238);
                button.FlatAppearance.BorderSize=0;button.TextAlign=ContentAlignment.MiddleLeft;
                button.FlatAppearance.MouseOverBackColor=Color.FromArgb(37,54,78);
                button.FlatAppearance.MouseDownBackColor=Color.FromArgb(46,68,98);
            }
            button.UseVisualStyleBackColor=false;
            if(paintedSidebarButtons.Add(button))
            {
                button.Text=string.Empty;
                button.Paint+=(_,e)=>
                {
                    var label=button.AccessibleName??string.Empty;
                    var bounds=new Rectangle(36,0,Math.Max(0,button.Width-40),button.Height);
                    using var fill=new SolidBrush(button.BackColor);e.Graphics.FillRectangle(fill,bounds);
                    TextRenderer.DrawText(e.Graphics,label,AppTheme.Small,bounds,Color.White,TextFormatFlags.Left|TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis);
                };
            }
            y+=41;
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
    }
}
