using System.Drawing.Drawing2D;

namespace QLKhachSan.GUI;

internal static class UiIcons
{
    public static Image Create(string kind,Color color,int size,byte[]? png)
    {
        if(png is null)return Create(kind,color,size);
        using var stream=new MemoryStream(png);
        using var source=Image.FromStream(stream);
        return new Bitmap(source,new Size(size,size));
    }
    public static Image Create(string kind,Color color,int size=20)
    {
        var image=new Bitmap(size,size);
        using var g=Graphics.FromImage(image);
        g.SmoothingMode=SmoothingMode.AntiAlias;
        g.ScaleTransform(size/20f,size/20f);
        using var pen=new Pen(color,1.8f) {StartCap=LineCap.Round,EndCap=LineCap.Round,LineJoin=LineJoin.Round};
        void Line(float x1,float y1,float x2,float y2)=>g.DrawLine(pen,x1,y1,x2,y2);
        void Box(float x,float y,float w,float h)=>g.DrawRectangle(pen,x,y,w,h);
        switch(kind)
        {
            case "bed":
                Box(2,8,16,7);Line(2,15,2,18);Line(18,15,18,18);Line(4,8,4,5);Line(4,5,16,5);Line(16,5,16,8);Line(9,8,9,13);break;
            case "calendar":
                Box(3,4,14,13);Line(3,8,17,8);Line(7,2,7,6);Line(13,2,13,6);
                using(var dot=new SolidBrush(color)){g.FillEllipse(dot,6,11,2,2);g.FillEllipse(dot,11,11,2,2);}break;
            case "people":
                g.DrawEllipse(pen,7,2,6,6);g.DrawArc(pen,3,9,14,9,185,170);break;
            case "people2":
                g.DrawEllipse(pen,3,3,5,5);g.DrawEllipse(pen,12,3,5,5);
                g.DrawArc(pen,1,9,9,9,185,170);g.DrawArc(pen,10,9,9,9,185,170);break;
            case "money":
                Box(2,4,16,12);g.DrawEllipse(pen,7,6,6,8);Line(4,7,5,7);Line(15,13,16,13);break;
            case "chart":
                Line(3,17,17,17);Line(3,17,3,3);Box(6,10,2,7);Box(10,7,2,10);Box(14,4,2,13);break;
            case "receipt":
                Line(5,2,15,2);Line(5,2,5,18);Line(15,2,15,18);Line(5,18,7,16);Line(7,16,10,18);Line(10,18,13,16);Line(13,16,15,18);Line(8,7,12,7);Line(8,11,12,11);break;
            case "clock":
                g.DrawEllipse(pen,3,3,14,14);Line(10,6,10,10);Line(10,10,13,12);break;
            case "calendar-clock":
                Box(2,4,15,13);Line(2,8,17,8);Line(6,2,6,6);Line(13,2,13,6);
                g.DrawEllipse(pen,9,10,7,7);Line(12.5f,11.5f,12.5f,13.5f);Line(12.5f,13.5f,14,14.5f);break;
            case "finance":
                Box(2,3,10,14);Line(4,7,10,7);Line(4,10,10,10);Line(4,13,8,13);
                g.DrawEllipse(pen,10,10,8,8);Line(14,11.5f,14,16.5f);break;
            case "key":
                g.DrawEllipse(pen,2,3,9,9);Line(10,10,17,17);Line(14,14,16,12);break;
            case "service":
                g.DrawArc(pen,3,4,14,11,190,160);Line(2,15,18,15);Line(5,18,15,18);break;
            case "tools":
                Line(4,16,15,5);g.DrawEllipse(pen,2,14,4,4);g.DrawArc(pen,12,2,6,6,10,260);break;
            case "swap":
                Line(3,6,17,6);Line(14,3,17,6);Line(17,6,14,9);Line(17,14,3,14);Line(6,11,3,14);Line(3,14,6,17);break;
            case "search":
                g.DrawEllipse(pen,3,3,10,10);Line(12,12,18,18);break;
            case "building": Box(3,2,14,16);for(var x=6;x<=14;x+=4)for(var y=5;y<=11;y+=4)Box(x,y,1,1);Box(8,14,4,4);break;
            case "door": Box(5,2,10,16);g.DrawEllipse(pen,12,10,1,1);break;
            case "bath": Line(2,11,18,11);g.DrawArc(pen,3,8,14,9,0,180);Line(5,17,4,19);Line(15,17,16,19);Line(4,11,4,5);Line(4,5,8,5);break;
            case "food": g.DrawEllipse(pen,3,8,14,7);Line(2,17,18,17);Line(10,5,10,8);break;
            case "coffee": Box(3,7,11,9);g.DrawArc(pen,12,8,6,6,260,190);Line(4,18,16,18);break;
            case "wifi": g.DrawArc(pen,2,5,16,12,205,130);g.DrawArc(pen,5,9,10,8,205,130);g.FillEllipse(new SolidBrush(color),9,16,2,2);break;
            case "car": Box(3,8,14,7);Line(5,8,7,4);Line(7,4,13,4);Line(13,4,15,8);g.DrawEllipse(pen,5,14,3,3);g.DrawEllipse(pen,12,14,3,3);break;
            case "phone": g.DrawArc(pen,3,3,14,14,120,120);Line(5,14,8,17);Line(12,17,15,14);break;
            case "bell": g.DrawArc(pen,4,4,12,12,180,180);Line(4,10,3,15);Line(3,15,17,15);Line(17,15,16,10);Line(8,18,12,18);break;
            case "bag": Box(3,7,14,11);g.DrawArc(pen,7,2,6,9,180,180);break;
            case "star":
                using(var brush=new SolidBrush(color)){var points=Enumerable.Range(0,10).Select(i=>new PointF(10+(i%2==0?8:4)*(float)Math.Sin(i*Math.PI/5),10-(i%2==0?8:4)*(float)Math.Cos(i*Math.PI/5))).ToArray();g.FillPolygon(brush,points);}break;
            case "heart": g.DrawArc(pen,2,4,8,9,180,180);g.DrawArc(pen,10,4,8,9,180,180);Line(2,9,10,18);Line(18,9,10,18);break;
            case "shield": Line(10,2,17,5);Line(17,5,16,13);Line(16,13,10,18);Line(10,18,4,13);Line(4,13,3,5);Line(3,5,10,2);break;
            case "gear": g.DrawEllipse(pen,4,4,12,12);g.DrawEllipse(pen,8,8,4,4);for(var i=0;i<8;i++){var a=i*Math.PI/4;Line(10+7*(float)Math.Cos(a),10+7*(float)Math.Sin(a),10+9*(float)Math.Cos(a),10+9*(float)Math.Sin(a));}break;
            case "folder": Line(2,6,8,6);Line(8,6,10,8);Line(10,8,18,8);Line(18,8,18,17);Line(18,17,2,17);Line(2,17,2,6);break;
            case "logout":
                Line(10,3,3,3);Line(3,3,3,17);Line(3,17,10,17);Line(9,10,18,10);Line(14,6,18,10);Line(18,10,14,14);break;
            case "refresh":
                g.DrawArc(pen,3,3,14,14,55,290);Line(16,3,17,8);Line(17,8,12,7);break;
            default:
                Box(4,3,12,14);Line(7,7,13,7);Line(7,11,13,11);break;
        }
        return image;
    }
    public static string Kind(string text)
    {
        text=text.ToLowerInvariant();
        if(text.Contains("phòng") && (text.Contains("nhận")||text.Contains("dọn")))return "bed";
        if(text.Contains("đặt")||text.Contains("lịch"))return "calendar";
        if(text.Contains("khách")||text.Contains("nhân viên")||text.Contains("tài khoản"))return "people";
        if(text.Contains("cọc")||text.Contains("thu chi"))return "money";
        if(text.Contains("doanh thu")||text.Contains("thống kê"))return "chart";
        if(text.Contains("hóa đơn"))return "receipt";
        if(text.Contains("mật khẩu"))return "key";
        if(text.Contains("dịch vụ"))return "service";
        if(text.Contains("bảo trì")||text.Contains("xử lý"))return "tools";
        if(text.Contains("chuyển"))return "swap";
        if(text.Contains("gia hạn")||text.Contains("nhật ký"))return "clock";
        return "receipt";
    }
}
