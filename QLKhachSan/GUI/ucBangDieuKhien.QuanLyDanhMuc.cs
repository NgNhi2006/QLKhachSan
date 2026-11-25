using QLKhachSan.DTO;

namespace QLKhachSan.GUI;

public partial class ucDashboard
{
    private sealed class ServiceMenuColors : ProfessionalColorTable
    {
        public override Color ToolStripDropDownBackground=>Color.White;
        public override Color ImageMarginGradientBegin=>Color.White;
        public override Color ImageMarginGradientMiddle=>Color.White;
        public override Color ImageMarginGradientEnd=>Color.White;
        public override Color MenuItemSelected=>Color.FromArgb(229,242,233);
        public override Color MenuItemBorder=>AppTheme.Blue;
        public override Color MenuBorder=>AppTheme.Border;
    }

    private void AddTool(string title,int y,Func<Task> action)
    {
        var button=new Button {Text=title,Width=190,Height=38,Location=new Point(10,y)};
        button.Click+=async (_,_)=>await Run(action);pnlLeftTools.Controls.Add(button);
    }
    private Task ShowDeposit() => ShowDeposit(null);
    private Task ShowDeposit(Stay? preselected)
    {
        using var dialog=new InputDialog("Thu cọc bổ sung",760,Math.Min(660,Screen.FromControl(this).WorkingArea.Height-30));
        var choice=Ui.Combo(data.Stays.Where(s=>s.Status==StayStatus.Reserved).Select(s=>new StayChoice(s,$"#{s.Id} • P.{StayRoom(s)?.Number} • {s.Guest} • Đã cọc {s.Deposit:N0} đ")));
        if(choice.Items.Count==0)throw new BusinessException("Không có lượt đặt trước chờ nhận phòng để thu cọc.");
        if(preselected is not null)
        {
            choice.SelectedItem=choice.Items.Cast<StayChoice>().FirstOrDefault(x=>x.Stay.Id==preselected.Id);
            choice.Enabled=false;
        }
        var amount=Ui.Money();var method=Ui.Combo(new[]{"Tiền mặt","Chuyển khoản","Thẻ POS"});var reference=Ui.Text(100);
        var summary=new Label {AutoSize=true,MinimumSize=new Size(0,56),MaximumSize=new Size(680,0),BackColor=AppTheme.Canvas,ForeColor=AppTheme.Ink,Padding=new Padding(12,8,8,6)};
        void UpdateSummary()
        {
            if(choice.SelectedItem is not StayChoice selected)return;
            summary.Text=$"P.{StayRoom(selected.Stay)?.Number}  •  {selected.Stay.Guest}  •  Cọc đã thu: {selected.Stay.Deposit:N0} đ\nSau bổ sung: {selected.Stay.Deposit+amount.Value:N0} đ  •  Hạn nhận: {selected.Stay.HoldUntil:dd/MM/yyyy HH:mm}";
        }
        choice.SelectedIndexChanged+=(_,_)=>UpdateSummary();amount.ValueChanged+=(_,_)=>UpdateSummary();
        var confirm=new CheckBox {Text="Tôi xác nhận đã nhận đủ số tiền bổ sung",AutoSize=true};
        dialog.Add("Lượt đặt trước chưa nhận phòng",choice);dialog.Add("Thu thêm (đồng)",amount);dialog.Add("Hình thức",method);dialog.Add("Mã giao dịch QR/POS",reference);
        dialog.Add("THÔNG TIN THU CỌC",summary);
        PaymentQr.Add(dialog,method,()=>amount.Value,()=>choice.SelectedItem is StayChoice item?$"COC-{item.Stay.Id}":"COC",amount,null,choice,transfer=>
        {
            var area=Screen.FromControl(this).WorkingArea;
            dialog.Height=Math.Min(transfer?1010:660,area.Height-30);
            if(dialog.Visible)dialog.Top=area.Top+(area.Height-dialog.Height)/2;
        });
        dialog.Add("Xác nhận giao dịch",confirm);
        dialog.Note("Cọc đầu tiên chuyển hạn giữ tối đa sang 15 ngày tính từ lúc đặt phòng. Quá hạn chưa nhận: tự hủy và không hoàn cọc.");
        dialog.Action("GHI NHẬN THU",async()=>
        {
            if(!confirm.Checked)throw new BusinessException("Cần xác nhận đã nhận tiền.");
            if(choice.SelectedItem is StayChoice selected)await Changed(()=>service.AddDepositAsync(selected.Stay,amount.Value,(string)method.SelectedItem!,reference.Text));
        });
        UpdateSummary();dialog.ShowDialog(this);return Task.CompletedTask;
    }
    private Task ShowEditBooking(Stay stay)
    {
        using var dialog=new InputDialog("Sửa lịch đặt / thông tin khách",760,720,true);
        var room=Ui.Combo(data.Rooms.Where(r=>r.Status!=RoomStatus.BaoTri));
        room.SelectedItem=data.Rooms.FirstOrDefault(r=>r.Id==stay.RoomId);
        var name=Ui.Text();name.Text=stay.Guest;var phone=Ui.Text(20);phone.Text=stay.Phone;var identity=Ui.Text(20);identity.Text=stay.Identity;
        var arrival=Ui.DatePicker(stay.Arrival);var days=Ui.Number(60,Math.Clamp((int)(stay.Departure-stay.Arrival).TotalDays,1,60));
        var receiveBy=Ui.DatePicker(stay.HoldUntil??stay.Arrival.AddHours(2));var reason=Ui.Text(300);
        dialog.Add("Phòng",room);dialog.Add("Họ tên",name);dialog.Add("SĐT",phone);dialog.Add("CCCD / Hộ chiếu",identity);dialog.Add("Ngày đến",arrival);dialog.Add("Số ngày",days);dialog.Add("Hạn cuối nhận",receiveBy);dialog.Add("Lý do thay đổi",reason);
        dialog.Note($"Cọc đã thu: {stay.Deposit:N0} đ. Hạn tối đa: {QLKhachSan.BLL.HotelService.ReservationHoldLimit(stay.Created,stay.Deposit>0):dd/MM/yyyy HH:mm}. Không sửa được lượt quá hạn.");
        dialog.Action("LƯU THAY ĐỔI",async()=>
        {
            if(room.SelectedItem is not Room target)throw new BusinessException("Chọn phòng.");
            await Changed(()=>service.UpdateBookingAsync(stay,target,new GuestInput(name.Text,phone.Text,identity.Text),arrival.Value,(int)days.Value,receiveBy.Value,reason.Text));
        });
        dialog.ShowDialog(this);return Task.CompletedTask;
    }
    private sealed record OrderManagementRow(Stay Stay,string RoomNumber,ServiceLine Line)
    {
        public string Guest=>Stay.Guest;
        public string Service=>Line.Name;
        public int OrderedQuantity=>Line.Quantity;
        public int DeliveredQuantity=>Line.DeliveredQuantity;
        public string Progress=>$"{Line.DeliveredQuantity} / {Line.Quantity}";
        public int Remaining=>Math.Max(0,Line.Quantity-Line.DeliveredQuantity);
        public decimal Amount=>Line.Total;
        public string Status=>Line.Cancelled is not null?"Đã hủy":Remaining>0?"Chờ giao":"Đã giao";
        public DateTime OrderedAt=>Line.Ordered;
    }

    private async Task ShowOrderManagement()
    {
        using var dialog=new Form {Text="Xử lý dịch vụ các phòng",Size=new Size(1080,640),
            MinimumSize=new Size(880,540),StartPosition=FormStartPosition.CenterParent,
            Font=AppTheme.Body,BackColor=AppTheme.Canvas,AutoScaleMode=AutoScaleMode.Dpi};
        var root=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,RowCount=2};
        root.RowStyles.Add(new RowStyle(SizeType.Absolute,80));
        root.RowStyles.Add(new RowStyle(SizeType.Percent,100));dialog.Controls.Add(root);
        var heading=new Panel {Dock=DockStyle.Fill,BackColor=Color.White,Padding=new Padding(24,10,20,8)};
        heading.Controls.Add(new Label {Text="Yêu cầu dịch vụ các phòng",Dock=DockStyle.Top,Height=36,
            Font=AppTheme.Title,ForeColor=AppTheme.Ink});
        heading.Controls.Add(new Label {Text="Xem ngay phòng nào đang chờ dịch vụ, tìm theo phòng, khách hoặc tên dịch vụ.",
            Dock=DockStyle.Bottom,Height=22,ForeColor=AppTheme.Muted});root.Controls.Add(heading,0,0);
        var body=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=2,RowCount=1,Padding=new Padding(18,14,18,8)};
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,70));
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,30));root.Controls.Add(body,0,1);
        var listPanel=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,RowCount=3,
            BackColor=Color.White,Padding=new Padding(14),Margin=new Padding(0,0,7,0)};
        listPanel.RowStyles.Add(new RowStyle(SizeType.Absolute,32));
        listPanel.RowStyles.Add(new RowStyle(SizeType.Absolute,44));
        listPanel.RowStyles.Add(new RowStyle(SizeType.Percent,100));body.Controls.Add(listPanel,0,0);
        var count=new Label {Dock=DockStyle.Fill,Font=AppTheme.Bold,ForeColor=AppTheme.Blue};
        listPanel.Controls.Add(count,0,0);
        var filters=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=2};
        filters.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,70));
        filters.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,30));
        var search=Ui.Text(100);search.Dock=DockStyle.Fill;
        search.PlaceholderText="Tìm số phòng, khách hoặc dịch vụ";
        var status=Ui.Combo(new[]{"Chờ giao","Tất cả","Đã giao","Đã hủy"});status.Dock=DockStyle.Fill;
        filters.Controls.Add(search,0,0);filters.Controls.Add(status,1,0);
        listPanel.Controls.Add(filters,0,1);
        var grid=Ui.Grid();grid.AutoGenerateColumns=false;grid.ReadOnly=true;
        grid.MultiSelect=false;grid.SelectionMode=DataGridViewSelectionMode.FullRowSelect;
        grid.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill;
        void Column(string property,string title,float weight,string? format=null)
        {
            var column=new DataGridViewTextBoxColumn {DataPropertyName=property,HeaderText=title,
                FillWeight=weight,ReadOnly=true};
            if(format is not null)column.DefaultCellStyle.Format=format;
            grid.Columns.Add(column);
        }
        Column(nameof(OrderManagementRow.RoomNumber),"Phòng",65);
        Column(nameof(OrderManagementRow.Guest),"Khách",120);
        Column(nameof(OrderManagementRow.Service),"Dịch vụ",175);
        Column(nameof(OrderManagementRow.Progress),"Giao / Gọi",85);
        Column(nameof(OrderManagementRow.Remaining),"Còn chờ",75);
        Column(nameof(OrderManagementRow.Amount),"Tiền",95,"N0");
        Column(nameof(OrderManagementRow.Status),"Trạng thái",88);
        listPanel.Controls.Add(grid,0,2);
        var editor=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,RowCount=7,
            BackColor=Color.White,Padding=new Padding(16,12,16,10),Margin=new Padding(7,0,0,0)};
        editor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
        foreach(var height in new[]{32,105,29,44,65,48,48})
            editor.RowStyles.Add(new RowStyle(SizeType.Absolute,height));
        body.Controls.Add(editor,1,0);
        editor.Controls.Add(new Label {Text="YÊU CẦU ĐANG CHỌN",Dock=DockStyle.Fill,
            Font=AppTheme.Bold,ForeColor=AppTheme.Blue},0,0);
        var detail=new Label {Dock=DockStyle.Fill,BackColor=AppTheme.Canvas,
            Padding=new Padding(10,9,6,6),ForeColor=AppTheme.Ink};editor.Controls.Add(detail,0,1);
        editor.Controls.Add(new Label {Text="Tổng số lượng khách yêu cầu",Dock=DockStyle.Fill,
            ForeColor=AppTheme.Muted},0,2);
        var totalQuantity=Ui.Number(100);totalQuantity.Dock=DockStyle.Fill;
        editor.Controls.Add(totalQuantity,0,3);
        var guidance=new Label {Dock=DockStyle.Fill,ForeColor=AppTheme.Muted,
            Padding=new Padding(0,9,0,0)};editor.Controls.Add(guidance,0,4);
        var change=new Button {Text="LƯU SỐ LƯỢNG",Dock=DockStyle.Fill,Margin=new Padding(0,2,0,4)};
        AppTheme.Button(change,true);editor.Controls.Add(change,0,5);
        var otherActions=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=2};
        otherActions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,55));
        otherActions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,45));
        var deliver=new Button {Text="GIAO DỊCH VỤ...",Dock=DockStyle.Fill,Margin=new Padding(0,2,4,0)};
        var more=new Button {Text="TÙY CHỌN  ▾",Dock=DockStyle.Fill,Margin=new Padding(4,2,0,0)};
        AppTheme.Button(deliver);AppTheme.Button(more);
        otherActions.Controls.Add(deliver,0,0);otherActions.Controls.Add(more,1,0);
        editor.Controls.Add(otherActions,0,6);
        var menu=new ContextMenuStrip {Font=AppTheme.TextFont(11.5f),BackColor=Color.White,
            ForeColor=AppTheme.Ink,Padding=new Padding(8),ShowImageMargin=true,
            ShowCheckMargin=false,ImageScalingSize=new Size(22,22),
            Renderer=new ToolStripProfessionalRenderer(new ServiceMenuColors())};
        ToolStripMenuItem MenuAction(string label,string icon,bool danger=false)
        {
            var item=new ToolStripMenuItem(label) {AutoSize=false,Size=new Size(354,50),
                Font=AppTheme.TextFont(11.5f),ForeColor=danger?Color.FromArgb(166,49,54):AppTheme.Ink,
                Image=UiIcons.Create(icon,danger?Color.FromArgb(166,49,54):AppTheme.Blue,22),
                ImageScaling=ToolStripItemImageScaling.None,TextAlign=ContentAlignment.MiddleLeft,
                Padding=new Padding(10,5,8,5)};
            return item;
        }
        var move=MenuAction("Chuyển sang phòng khác","swap");
        var deliverAll=MenuAction("Giao hết dịch vụ của phòng","service");
        var cancel=MenuAction("Hủy yêu cầu này","close",true);
        var cancelAll=MenuAction("Hủy dịch vụ chưa giao của phòng","close",true);
        menu.Items.AddRange([move,deliverAll,new ToolStripSeparator(),cancel,cancelAll]);
        dialog.Disposed+=(_,_)=>menu.Dispose();
        more.Click+=(_,_)=>menu.Show(more,new Point(more.Width-menu.Width,more.Height+5));
        var allRows=new List<OrderManagementRow>();
        OrderManagementRow Selected()=>grid.CurrentRow?.DataBoundItem as OrderManagementRow
            ??throw new BusinessException("Chọn một yêu cầu dịch vụ.");
        void ShowSelection()
        {
            if(grid.CurrentRow?.DataBoundItem is not OrderManagementRow row)
            {
                detail.Text="Chưa chọn yêu cầu dịch vụ.";
                deliver.Enabled=change.Enabled=more.Enabled=false;
                move.Enabled=cancel.Enabled=deliverAll.Enabled=cancelAll.Enabled=false;
                return;
            }
            detail.Text=$"P.{row.RoomNumber}  •  {row.Guest}\n{row.Service}\n"+
                $"Đã gọi {row.Line.Quantity}  •  Đã giao {row.Line.DeliveredQuantity}  •  Còn chờ {row.Remaining}\n"+
                $"Đơn giá {row.Line.Price:N0} đ";
            totalQuantity.Minimum=Math.Max(1,row.Line.DeliveredQuantity);
            totalQuantity.Value=Math.Clamp(row.Line.Quantity,1,100);
            guidance.Text=$"Sửa tổng số lượng ở trên rồi lưu. Giao dịch vụ là thao tác riêng; còn chờ {row.Remaining}.";
            var editable=row.Line.Cancelled is null;
            deliver.Enabled=editable && row.Remaining>0;
            change.Enabled=editable && totalQuantity.Value!=row.Line.Quantity;
            more.Enabled=true;
            move.Enabled=editable && row.Line.DeliveredQuantity==0 &&
                data.Stays.Any(x=>x.Status==StayStatus.Occupied && x.CustomerId==row.Stay.CustomerId && x.Id!=row.Stay.Id);
            cancel.Enabled=editable && row.Line.DeliveredQuantity==0;
            var roomWaiting=allRows.Any(x=>x.Stay.Id==row.Stay.Id && x.Line.Cancelled is null && x.Remaining>0);
            deliverAll.Enabled=cancelAll.Enabled=roomWaiting;
        }
        void FilterRows()
        {
            var previous=grid.CurrentRow?.DataBoundItem as OrderManagementRow;
            var query=search.Text.Trim();
            var state=(string?)status.SelectedItem??"Chờ giao";
            var rows=allRows.Where(x=>state=="Tất cả" || x.Status==state)
                .Where(x=>query.Length==0 ||
                    (x.RoomNumber+" "+x.Guest+" "+x.Service).Contains(query,StringComparison.CurrentCultureIgnoreCase))
                .OrderBy(x=>x.Status=="Chờ giao"?0:1).ThenBy(x=>x.OrderedAt).ToList();
            grid.DataSource=rows;
            var keep=rows.FindIndex(x=>x.Line.Id==previous?.Line.Id);
            if(keep<0)keep=rows.FindIndex(x=>x.Stay.Id==previous?.Stay.Id);
            if(keep>=0 && grid.Rows.Count>keep)
            {grid.ClearSelection();grid.Rows[keep].Selected=true;grid.CurrentCell=grid.Rows[keep].Cells[0];}
            count.Text=$"CÒN CHỜ: {allRows.Count(x=>x.Status=="Chờ giao")} yêu cầu / {allRows.Where(x=>x.Status=="Chờ giao").Select(x=>x.Stay.Id).Distinct().Count()} phòng"+
                $"  •  {data.Stays.Count(x=>x.Status==StayStatus.Occupied)} phòng đang ở";
            ShowSelection();
        }
        async Task RefreshOrders()
        {
            await Reload();
            var stays=data.Stays.Where(x=>x.Status==StayStatus.Occupied).ToArray();
            var orders=await Task.WhenAll(stays.Select(x=>service.StayOrdersAsync(x.Id)));
            allRows=stays.SelectMany((stay,index)=>orders[index].Select(line=>
                    new OrderManagementRow(stay,StayRoom(stay)?.Number??"?",line))).ToList();
            FilterRows();
        }
        async Task Execute(Func<OrderManagementRow,Task> work)
        {
            var row=Selected();change.Enabled=deliver.Enabled=more.Enabled=false;
            try{await work(row);await RefreshOrders();}
            catch(Exception ex){Ui.Error(dialog,ex);}
            finally{ShowSelection();}
        }
        grid.SelectionChanged+=(_,_)=>ShowSelection();
        totalQuantity.ValueChanged+=(_,_)=>
        {
            if(grid.CurrentRow?.DataBoundItem is OrderManagementRow row)
                change.Enabled=row.Line.Cancelled is null && totalQuantity.Value!=row.Line.Quantity;
        };
        search.TextChanged+=(_,_)=>FilterRows();
        status.SelectedIndexChanged+=(_,_)=>FilterRows();
        deliver.Click+=(_,_)=>
        {
            var row=Selected();
            using var picker=new InputDialog($"Giao dịch vụ · P.{row.RoomNumber}",500,330);
            var amount=Ui.Number(row.Remaining);amount.Value=1;
            picker.Add($"Giao thêm {row.Service} (còn chờ {row.Remaining})",amount);
            picker.Note($"Phòng {row.RoomNumber}  •  Đã giao {row.Line.DeliveredQuantity}/{row.Line.Quantity}");
            picker.Action("XÁC NHẬN ĐÃ GIAO",async()=>
            {
                await service.DeliverOrderAsync(row.Stay,row.Line.Id,(int)amount.Value);
                await RefreshOrders();
            });
            picker.ShowDialog(dialog);
        };
        change.Click+=async (_,_)=>await Execute(row=>service.ChangeOrderAsync(row.Stay,row.Line.Id,
            (int)totalQuantity.Value,"Điều chỉnh số lượng dịch vụ"));
        move.Click+=(_,_)=>
        {
            var row=Selected();
            var choices=data.Stays.Where(x=>x.Status==StayStatus.Occupied &&
                x.CustomerId==row.Stay.CustomerId && x.Id!=row.Stay.Id)
                .Select(x=>new StayChoice(x,$"P.{StayRoom(x)?.Number}  •  {x.Guest}")).ToArray();
            if(choices.Length==0)return;
            using var picker=new InputDialog($"Chuyển dịch vụ P.{row.RoomNumber}",540,330);
            var target=Ui.Combo(choices);
            picker.Add($"Chuyển {row.Service} sang phòng",target);
            picker.Note("Chỉ chuyển được yêu cầu chưa giao của cùng khách.");
            picker.Action("CHUYỂN YÊU CẦU",async()=>
            {
                if(target.SelectedItem is not StayChoice choice)
                    throw new BusinessException("Chọn phòng nhận yêu cầu.");
                await service.MoveOrderAsync(row.Stay,choice.Stay,row.Line.Id,"Chuyển yêu cầu sang phòng đúng");
                await RefreshOrders();
            });
            picker.ShowDialog(dialog);
        };
        cancel.Click+=async (_,_)=>
        {
            var row=Selected();
            if(Ui.Confirm(dialog,$"Hủy {row.Service} của phòng {row.RoomNumber}? Lịch sử vẫn được lưu."))
                await Execute(x=>service.ChangeOrderAsync(x.Stay,x.Line.Id,0,"Hủy yêu cầu dịch vụ",true));
        };
        deliverAll.Click+=async (_,_)=>
        {
            var row=Selected();
            if(Ui.Confirm(dialog,$"Xác nhận đã giao hết dịch vụ còn chờ của phòng {row.RoomNumber}?"))
                await Execute(x=>service.DeliverAsync(x.Stay));
        };
        cancelAll.Click+=async (_,_)=>
        {
            var row=Selected();
            if(Ui.Confirm(dialog,$"Hủy phần dịch vụ chưa giao của phòng {row.RoomNumber}? Phần đã giao vẫn tính tiền."))
                await Execute(x=>service.CancelPendingOrdersAsync(x.Stay,"Hủy yêu cầu dịch vụ chưa giao"));
        };
        await RefreshOrders();dialog.ShowDialog(this);
    }
    private async Task ShowStayHistory()
    {
        using var dialog=new Form {Text="Lịch đặt và lịch sử lưu trú",Size=new Size(1150,720),MinimumSize=new Size(900,580),StartPosition=FormStartPosition.CenterParent,Font=AppTheme.Body,BackColor=AppTheme.Canvas,AutoScaleMode=AutoScaleMode.Dpi};
        var root=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,RowCount=4};
        root.RowStyles.Add(new RowStyle(SizeType.Absolute,76));root.RowStyles.Add(new RowStyle(SizeType.Absolute,126));
        root.RowStyles.Add(new RowStyle(SizeType.Percent,100));root.RowStyles.Add(new RowStyle(SizeType.Absolute,76));dialog.Controls.Add(root);
        var heading=new Panel {Dock=DockStyle.Fill,BackColor=Color.White,Padding=new Padding(24,10,20,8)};
        heading.Controls.Add(new Label {Text="Lịch đặt và lịch sử lưu trú",Dock=DockStyle.Top,Height=36,Font=AppTheme.Title,ForeColor=AppTheme.Ink});
        heading.Controls.Add(new Label {Text="Tra cứu lượt đặt, lượt đang ở và lịch sử theo khách hoặc khoảng ngày",Dock=DockStyle.Bottom,Height=22,ForeColor=AppTheme.Muted});root.Controls.Add(heading,0,0);
        var filters=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=3,RowCount=3,Padding=new Padding(22,10,22,6)};
        filters.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,50));filters.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,25));filters.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,25));
        filters.RowStyles.Add(new RowStyle(SizeType.Absolute,25));filters.RowStyles.Add(new RowStyle(SizeType.Absolute,42));filters.RowStyles.Add(new RowStyle(SizeType.Percent,100));root.Controls.Add(filters,0,1);
        filters.Controls.Add(new Label {Text="Tên khách / SĐT / CCCD",Dock=DockStyle.Fill,ForeColor=AppTheme.Muted},0,0);
        filters.Controls.Add(new Label {Text="Từ ngày",Dock=DockStyle.Fill,ForeColor=AppTheme.Muted},1,0);
        filters.Controls.Add(new Label {Text="Đến hết ngày",Dock=DockStyle.Fill,ForeColor=AppTheme.Muted},2,0);
        var search=Ui.Text();search.PlaceholderText="Nhập thông tin khách cần tìm...";search.Dock=DockStyle.Fill;search.Margin=new Padding(0,0,12,0);filters.Controls.Add(search,0,1);
        var from=Ui.DatePicker(ServerNow.Date);from.CustomFormat="dd/MM/yyyy";from.Dock=DockStyle.Fill;from.Margin=new Padding(0,0,12,0);filters.Controls.Add(from,1,1);
        var until=Ui.DatePicker(ServerNow.Date.AddDays(30));until.CustomFormat="dd/MM/yyyy";until.Dock=DockStyle.Fill;filters.Controls.Add(until,2,1);
        var all=new CheckBox {Text="Xem cả lịch sử (tối đa 500 lượt gần nhất)",Dock=DockStyle.Fill};filters.Controls.Add(all,0,2);filters.SetColumnSpan(all,3);
        all.CheckedChanged+=(_,_)=>{from.Enabled=!all.Checked;until.Enabled=!all.Checked;};
        var results=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=1,RowCount=3,BackColor=Color.White,Margin=new Padding(22,0,22,8),Padding=new Padding(14,10,14,10)};
        results.RowStyles.Add(new RowStyle(SizeType.Absolute,34));results.RowStyles.Add(new RowStyle(SizeType.Percent,100));results.RowStyles.Add(new RowStyle(SizeType.Absolute,42));root.Controls.Add(results,0,2);
        var count=new Label {Dock=DockStyle.Fill,Font=AppTheme.Bold,ForeColor=AppTheme.Blue};results.Controls.Add(count,0,0);
        var grid=Ui.Grid();grid.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill;results.Controls.Add(grid,0,1);
        results.Controls.Add(new Label {Text="Phòng có dấu * trên sơ đồ đang có lịch đặt. Lượt quá hạn không đến được xử lý theo chính sách cọc.",Dock=DockStyle.Fill,ForeColor=AppTheme.Muted,TextAlign=ContentAlignment.MiddleLeft},0,2);
        var footer=new TableLayoutPanel {Dock=DockStyle.Fill,ColumnCount=2,RowCount=1,Padding=new Padding(22,10,22,14)};
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,50));footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,50));root.Controls.Add(footer,0,3);
        var lookup=new Button {Text="TRA CỨU",Dock=DockStyle.Fill,Margin=new Padding(0,0,6,0)};AppTheme.Button(lookup,true);footer.Controls.Add(lookup,0,0);
        var export=new Button {Text="XUẤT CSV",Dock=DockStyle.Fill,Margin=new Padding(6,0,0,0)};AppTheme.Button(export);footer.Controls.Add(export,1,0);
        async Task LoadRows()
        {
            if(until.Value.Date<from.Value.Date)throw new BusinessException("Khoảng ngày không hợp lệ.");
            var rows=all.Checked?await service.StayHistoryAsync(search.Text):(await service.DashboardAsync()).Stays.Where(s=>s.Arrival<until.Value.Date.AddDays(1) && s.Departure>from.Value.Date && (s.Guest.Contains(search.Text,StringComparison.OrdinalIgnoreCase)||s.Phone.Contains(search.Text)||s.Identity.Contains(search.Text,StringComparison.OrdinalIgnoreCase))).ToList();
            grid.DataSource=rows.Select(s=>new {Mã=s.Id,Phòng=data.Rooms.FirstOrDefault(r=>r.Id==s.RoomId)?.Number,Khách=s.Guest,
                TrạngThái=s.Status switch {StayStatus.Reserved=>"Chờ nhận",StayStatus.Occupied=>"Đang ở",StayStatus.Paid=>"Đã trả",StayStatus.Cancelled=>"Đã hủy",_=>"Khác"},
                NgàyĐến=s.Arrival,NgàyTrả=s.Departure,HạnNhận=s.HoldUntil,Cọc=s.Deposit}).ToList();
            foreach(DataGridViewColumn column in grid.Columns)
            {
                if(column.Name=="TrạngThái")column.HeaderText="Trạng thái";
                if(column.Name=="NgàyĐến")column.HeaderText="Ngày đến";
                if(column.Name=="NgàyTrả")column.HeaderText="Ngày trả";
                if(column.Name=="HạnNhận")column.HeaderText="Hạn nhận";
                if(column.Name is "NgàyĐến" or "NgàyTrả" or "HạnNhận")column.DefaultCellStyle.Format="dd/MM/yyyy HH:mm";
                if(column.Name=="Cọc")column.DefaultCellStyle.Format="N0";
            }
            count.Text=$"KẾT QUẢ: {rows.Count} LƯỢT"+(all.Checked?"  •  Lịch sử gần nhất":"  •  Trong khoảng ngày đã chọn");
        }
        lookup.Click+=async (_,_)=>{lookup.Enabled=false;try{await LoadRows();}catch(Exception ex){Ui.Error(dialog,ex);}finally{lookup.Enabled=true;}};
        search.KeyDown+=(_,e)=>{if(e.KeyCode==Keys.Enter){e.SuppressKeyPress=true;lookup.PerformClick();}};
        export.Click+=(_,_)=>ExportGrid(grid,"lich-luu-tru");
        await LoadRows();dialog.ShowDialog(this);
    }
    private async Task ShowAudit()
    {
        using var dialog=new InputDialog("Nhật ký thao tác",1100,650);
        var day=Ui.DatePicker(ServerNow.Date);var grid=Ui.Grid();grid.Height=380;
        dialog.Add("Ngày (tối đa 1.000 thao tác gần nhất)",day);dialog.Add("Nhật ký",grid);
        async Task LoadRows(){grid.DataSource=await service.AuditsAsync(day.Value);FormatGrid(grid,new() {{"Created","Thời điểm"},{"Username","Nhân viên"},{"Action","Thao tác"},{"Detail","Chi tiết"}});}
        dialog.Action("XEM",LoadRows,false);await LoadRows();dialog.ShowDialog(this);
    }
    private static void HideColumns(DataGridView grid,params string[] names)
    {
        foreach(var name in names)if(grid.Columns[name] is { } column)column.Visible=false;
    }
    private static void FormatGrid(DataGridView grid,Dictionary<string,string> names)
    {
        grid.AutoSizeColumnsMode=names.Count<=6
            ? DataGridViewAutoSizeColumnsMode.Fill
            : DataGridViewAutoSizeColumnsMode.AllCells;
        foreach(DataGridViewColumn column in grid.Columns)
        {
            if(names.TryGetValue(column.Name,out var name))column.HeaderText=name;
            if(column.ValueType==typeof(decimal))column.DefaultCellStyle.Format="N0";
            if(column.ValueType==typeof(DateTime)||column.ValueType==typeof(DateTime?))column.DefaultCellStyle.Format="dd/MM/yyyy HH:mm";
        }
    }
}


