using System.Globalization;
using System.Text;
using QLKhachSan.BLL;

namespace QLKhachSan.GUI;

public partial class ucDashboard
{
    private sealed record SearchFunction(string Code,string Title,string Path,string Module);

    private static string SearchText(string value)
    {
        var normalized=value.Normalize(NormalizationForm.FormD);
        var result=new StringBuilder(normalized.Length);
        foreach(var c in normalized)
            if(CharUnicodeInfo.GetUnicodeCategory(c)!=UnicodeCategory.NonSpacingMark)
                result.Append(c is 'đ' or 'Đ'?'d':char.ToLowerInvariant(c));
        return result.ToString().Normalize(NormalizationForm.FormC);
    }

    private IEnumerable<SearchFunction> SearchableFunctions()
    {
        if(menuConfiguration is not null)
        {
            foreach(var function in menuConfiguration.Functions)
            {
                var submenu=menuConfiguration.Submenus.FirstOrDefault(s=>s.Id==function.SubmenuId);
                var menu=menuConfiguration.Menus.FirstOrDefault(m=>m.Code==submenu?.MenuCode);
                if(!function.Active || submenu?.Active!=true || menu?.Active!=true ||
                    !(FunctionPolicy.Can(user,function.ActionCode) ||
                      function.ActionCode=="room.schedule" && CanOpenSchedule()))continue;
                yield return new SearchFunction(function.ActionCode,function.Title,
                    $"{menu.Title} › {submenu.Title}",menu.Code);
            }
            foreach(var function in menuConfiguration.CustomFunctions)
            {
                var submenu=menuConfiguration.Submenus.FirstOrDefault(s=>s.Id==function.SubmenuId);
                var menu=menuConfiguration.Menus.FirstOrDefault(m=>m.Code==submenu?.MenuCode);
                if(!function.Active || submenu?.Active!=true || menu?.Active!=true ||
                    !function.AllowedRoles.Split(',').Contains(user.Role))continue;
                yield return new SearchFunction($"custom:{function.Id}",function.Title,
                    $"{menu.Title} › {submenu.Title}",menu.Code);
            }
            foreach(var code in new[]{"invoice.list","report.revenue","cash.flow","cash.book",
                "cash.debt","report.profit","invoice.create","system.password"})
            {
                if(!FunctionPolicy.Can(user,code))continue;
                var definition=FunctionPolicy.All.First(f=>f.Code==code);
                var module=code=="invoice.create"?"rooms":code=="system.password"?"staff":"finance";
                yield return new SearchFunction(code,definition.Name,
                    $"{(module=="rooms"?"Quản lý phòng":module=="staff"?"Nhân viên & hệ thống":"Tài chính")} › {definition.Submenu}",module);
            }
        }
        else
        {
            foreach(var function in FunctionPolicy.All.Where(f=>FunctionPolicy.Can(user,f.Code)))
            {
                var module=function.Menu switch
                {
                    "cash" or "invoices" or "reports"=>"finance",
                    "system"=>"staff",_=>function.Menu
                };
                yield return new SearchFunction(function.Code,function.Name,
                    $"{module} › {function.Submenu}",module);
            }
        }
        if(FunctionPolicy.Can(user,"staff.manage"))
            yield return new SearchFunction("menu.configure","Tùy chỉnh chức năng",
                "Nhân viên & hệ thống › Cấu hình menu","staff");
    }

    private Control BuildGlobalSearch()
    {
        var surface=new Panel {Height=48,Location=new Point(262,14),BackColor=AppTheme.Canvas};
        var icon=new Label {Text="⌕",Location=new Point(16,7),Size=new Size(28,32),
            Font=AppTheme.TextFont(18),ForeColor=AppTheme.Muted,TextAlign=ContentAlignment.MiddleCenter};
        var input=new TextBox {BorderStyle=BorderStyle.None,PlaceholderText="Tìm chức năng trong ứng dụng...",
            Font=AppTheme.Body,Location=new Point(52,13),Height=30,BackColor=AppTheme.Canvas};
        surface.Controls.Add(icon);surface.Controls.Add(input);
        surface.Resize+=(_,_)=>input.Width=Math.Max(60,surface.Width-66);
        surface.Paint+=(_,e)=>
        {
            using var border=new Pen(input.Focused?AppTheme.Blue:AppTheme.Border,input.Focused?2:1);
            using var path=AppTheme.Rounded(new RectangleF(1,1,surface.Width-3,surface.Height-3),12);
            e.Graphics.DrawPath(border,path);
        };
        input.GotFocus+=(_,_)=>surface.Invalidate();
        input.LostFocus+=(_,_)=>surface.Invalidate();
        icon.Click+=(_,_)=>input.Focus();surface.Click+=(_,_)=>input.Focus();
        var suggestions=new Panel {BackColor=Color.White,BorderStyle=BorderStyle.FixedSingle,
            Visible=false,AutoScroll=true};
        Controls.Add(suggestions);suggestions.BringToFront();
        surface.Disposed+=(_,_)=>suggestions.Dispose();
        List<SearchFunction> matches=[];
        var selectedIndex=0;
        void PositionSuggestions()
        {
            if(!surface.IsHandleCreated)return;
            suggestions.Location=PointToClient(surface.PointToScreen(new Point(0,surface.Height+3)));
            suggestions.Width=Math.Max(320,surface.Width);
        }
        surface.Resize+=(_,_)=>PositionSuggestions();
        pnlHeader.Resize+=(_,_)=>PositionSuggestions();
        Resize+=(_,_)=>PositionSuggestions();
        async Task Open(SearchFunction selected)
        {
            suggestions.Visible=false;input.Clear();
            try
            {
                if(selected.Code=="menu.configure")
                {
                    await Run(ShowMenuConfiguration);return;
                }
                if(selected.Module!="menu" && selected.Module!=activeModule)
                    ShowSubmenu(selected.Module);
                await OpenFunction(selected.Code);
            }
            catch(Exception ex){Ui.Error(this,ex);}
        }
        void Refresh()
        {
            var query=SearchText(input.Text.Trim());
            foreach(Control child in suggestions.Controls.Cast<Control>().ToArray())child.Dispose();
            matches=[];selectedIndex=0;
            if(query.Length==0){suggestions.Visible=false;return;}
            var words=query.Split(' ',StringSplitOptions.RemoveEmptyEntries);
            matches=SearchableFunctions().DistinctBy(f=>f.Code)
                .Select(f=>new {Item=f,Title=SearchText(f.Title),Path=SearchText(f.Path)})
                .Where(x=>words.All(word=>x.Title.Contains(word,StringComparison.Ordinal) ||
                    x.Path.Contains(word,StringComparison.Ordinal)))
                .OrderBy(x=>x.Title==query?0:x.Title.StartsWith(query,StringComparison.Ordinal)?1:
                    x.Title.Contains(query,StringComparison.Ordinal)?2:3)
                .ThenBy(x=>x.Item.Title).Take(8).Select(x=>x.Item).ToList();
            if(matches.Count==0)
                suggestions.Controls.Add(new Label {Text="Không tìm thấy chức năng phù hợp",
                    Location=new Point(12,8),Size=new Size(Math.Max(280,surface.Width-24),38),
                    TextAlign=ContentAlignment.MiddleLeft,ForeColor=AppTheme.Muted});
            for(var i=0;i<matches.Count;i++)
            {
                var match=matches[i];
                var item=new Button {Text=$"{match.Title}    ·    {match.Path}",Tag=match,
                    Location=new Point(4,4+i*44),Size=new Size(Math.Max(300,surface.Width-12),42),
                    TextAlign=ContentAlignment.MiddleLeft,FlatStyle=FlatStyle.Flat,
                    BackColor=i==selectedIndex?Color.FromArgb(239,245,251):Color.White,
                    ForeColor=AppTheme.Ink,Font=AppTheme.Body,Cursor=Cursors.Hand,AutoEllipsis=true};
                item.FlatAppearance.BorderSize=0;
                item.Click+=async (_,_)=>await Open(match);
                suggestions.Controls.Add(item);
            }
            PositionSuggestions();
            suggestions.Height=Math.Min(360,Math.Max(1,matches.Count)*44+10);
            suggestions.Visible=true;suggestions.BringToFront();
        }
        input.TextChanged+=(_,_)=>Refresh();
        input.LostFocus+=(_,_)=>BeginInvoke(() =>
        {
            if(!input.Focused && !suggestions.ContainsFocus)suggestions.Visible=false;
        });
        input.KeyDown+=async (_,e)=>
        {
            if(e.KeyCode==Keys.Escape){suggestions.Visible=false;e.SuppressKeyPress=true;}
            else if(e.KeyCode==Keys.Enter)
            {
                e.SuppressKeyPress=true;
                if(matches.Count>0)await Open(matches[selectedIndex]);
            }
            else if(e.KeyCode==Keys.Down && matches.Count>0)
            {
                selectedIndex=Math.Min(matches.Count-1,selectedIndex+1);
                foreach(var button in suggestions.Controls.OfType<Button>())
                    button.BackColor=button.Tag is SearchFunction match && match.Code==matches[selectedIndex].Code
                        ?Color.FromArgb(239,245,251):Color.White;
                e.SuppressKeyPress=true;
            }
            else if(e.KeyCode==Keys.Up && matches.Count>0)
            {
                selectedIndex=Math.Max(0,selectedIndex-1);
                foreach(var button in suggestions.Controls.OfType<Button>())
                    button.BackColor=button.Tag is SearchFunction match && match.Code==matches[selectedIndex].Code
                        ?Color.FromArgb(239,245,251):Color.White;
                e.SuppressKeyPress=true;
            }
        };
        return surface;
    }
}
