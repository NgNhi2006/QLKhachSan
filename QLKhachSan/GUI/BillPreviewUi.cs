namespace QLKhachSan.GUI;

internal static class BillPreviewUi
{
    public static Control CreateNavigation(PrintPreviewControl preview)
    {
        var bar=new FlowLayoutPanel {Dock=DockStyle.Top,Height=46,BackColor=Color.White,
            Padding=new Padding(10,6,8,3),WrapContents=false};
        Button Add(string title,Action action)
        {
            var button=new Button {Text=title,Width=112,Height=32,Margin=new Padding(2,0,5,0)};
            AppTheme.Button(button);
            button.Click+=(_,_)=>action();
            bar.Controls.Add(button);
            return button;
        }
        Add("◀ Trang trước",()=>{if(preview.StartPage>0)preview.StartPage--;});
        Add("Trang sau ▶",()=>preview.StartPage++);
        Add("Thu nhỏ",()=>preview.Zoom=Math.Max(0.35,preview.Zoom-0.1));
        Add("Phóng to",()=>preview.Zoom=Math.Min(2.0,preview.Zoom+0.1));
        Add("Vừa trang",()=>preview.AutoZoom=true);
        return bar;
    }

    public static void EnableWheel(PrintPreviewControl preview)
    {
        preview.TabStop=true;
        preview.MouseEnter+=(_,_)=>preview.Focus();
        preview.MouseWheel+=(_,e)=>
        {
            var scroll=preview.Controls.OfType<VScrollBar>().FirstOrDefault();
            if(scroll is null)return;
            var maximum=Math.Max(scroll.Minimum,scroll.Maximum-scroll.LargeChange+1);
            var direction=Math.Sign(e.Delta);
            var step=80*Math.Max(1,Math.Abs(e.Delta)/120);
            var next=Math.Clamp(scroll.Value-direction*step,scroll.Minimum,maximum);
            if(next!=scroll.Value)
            {
                scroll.Value=next;
                return;
            }
            var page=preview.StartPage;
            if(direction<0)preview.StartPage=page+1;
            else if(page>0)preview.StartPage=page-1;
            if(preview.StartPage!=page && preview.IsHandleCreated)
                preview.BeginInvoke(()=>
                {
                    if(preview.IsDisposed)return;
                    var bar=preview.Controls.OfType<VScrollBar>().FirstOrDefault();
                    if(bar is null)return;
                    bar.Value=direction<0?bar.Minimum:
                        Math.Max(bar.Minimum,bar.Maximum-bar.LargeChange+1);
                });
        };
    }
}
