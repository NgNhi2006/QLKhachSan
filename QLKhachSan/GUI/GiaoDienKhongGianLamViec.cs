using System.Drawing.Drawing2D;

namespace QLKhachSan.GUI;

// Applied after each modal window has built its controls. This keeps the many
// business forms visually consistent without changing their data or actions.
internal static class WorkspaceSkin
{
    private static readonly HashSet<Form> Styled=[];

    public static void ApplyOpenForms()
    {
        foreach(Form form in Application.OpenForms.Cast<Form>().ToArray())
        {
            if(form is FormMain or FormLogin or InputDialog || !Styled.Add(form))continue;
            form.FormClosed+=(_,_)=>Styled.Remove(form);
            Apply(form);
        }
    }

    private static void Apply(Form form)
    {
        var working=Screen.FromControl(form).WorkingArea;
        var maxWidth=Math.Max(640,working.Width-24);
        var maxHeight=Math.Max(480,working.Height-24);
        form.MinimumSize=new Size(Math.Min(form.MinimumSize.Width,maxWidth),Math.Min(form.MinimumSize.Height,maxHeight));
        form.MaximumSize=new Size(maxWidth,maxHeight);
        form.Size=new Size(Math.Min(form.Width,maxWidth),Math.Min(form.Height,maxHeight));
        form.Location=new Point(working.Left+(working.Width-form.Width)/2,working.Top+(working.Height-form.Height)/2);
        form.BackColor=AppTheme.Canvas;
        form.ForeColor=AppTheme.Ink;
        form.Font=AppTheme.Body;
        foreach(var grid in Descendants(form).OfType<DataGridView>())AppTheme.Grid(grid);
        foreach(var card in Descendants(form).Where(x=>x.BackColor==Color.White && x.Padding.Left>=10 &&
                     x is Panel && x is not FlowLayoutPanel))
        {
            card.Paint+=(_,e)=>
            {
                using var border=new Pen(AppTheme.Border);
                e.Graphics.DrawRectangle(border,0,0,card.Width-1,card.Height-1);
            };
            card.Invalidate();
        }
        foreach(var tabs in Descendants(form).OfType<TabControl>())StyleTabs(tabs);
        foreach(var button in Descendants(form).OfType<Button>())
        {
            if(button.FlatStyle!=FlatStyle.Standard || button.BackColor!=SystemColors.Control)continue;
            AppTheme.Button(button);
        }
        var root=form.Controls.OfType<TableLayoutPanel>().FirstOrDefault(x=>x.Dock==DockStyle.Fill);
        if(root is null)return;
        var top=root.GetControlFromPosition(0,0);
        if(top is Label directTitle && directTitle.Font.Size>=14)
        {
            directTitle.BackColor=AppTheme.Navy;
            directTitle.ForeColor=Color.White;
            directTitle.Padding=new Padding(Math.Max(18,directTitle.Padding.Left),directTitle.Padding.Top,8,0);
            return;
        }
        if(top is not Panel and not TableLayoutPanel)return;
        var labels=Descendants(top).OfType<Label>().Where(x=>x.Font.Size>=14).ToArray();
        if(labels.Length==0)return;
        if(root.RowStyles.Count>0 && root.RowStyles[0].SizeType==SizeType.Absolute && root.RowStyles[0].Height<92)
            root.RowStyles[0].Height=92;
        top.BackColor=AppTheme.Navy;
        foreach(var label in Descendants(top).OfType<Label>())
        {
            label.ForeColor=label.Font.Size>=14?Color.White:Color.FromArgb(222,231,236);
            if(label.BackColor==Color.White)label.BackColor=AppTheme.Navy;
        }
        top.Paint+=(_,e)=>
        {
            using var accent=new SolidBrush(AppTheme.Focus);
            e.Graphics.FillRectangle(accent,0,0,4,top.Height);
        };
        top.Invalidate();
    }

    private static IEnumerable<Control> Descendants(Control parent)
    {
        foreach(Control child in parent.Controls)
        {
            yield return child;
            foreach(var descendant in Descendants(child))yield return descendant;
        }
    }

    private static void StyleTabs(TabControl tabs)
    {
        if(tabs.DrawMode==TabDrawMode.OwnerDrawFixed)return;
        tabs.DrawMode=TabDrawMode.OwnerDrawFixed;
        tabs.Padding=new Point(15,8);
        tabs.DrawItem+=(_,e)=>
        {
            var selected=e.Index==tabs.SelectedIndex;
            e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;
            using var fill=new SolidBrush(selected?Color.White:Color.FromArgb(238,243,250));
            e.Graphics.FillRectangle(fill,e.Bounds);
            if(selected)
            {
                using var line=new Pen(AppTheme.Blue,3);
                e.Graphics.DrawLine(line,e.Bounds.Left+8,e.Bounds.Bottom-2,e.Bounds.Right-8,e.Bounds.Bottom-2);
            }
            TextRenderer.DrawText(e.Graphics,tabs.TabPages[e.Index].Text,AppTheme.Bold,e.Bounds,
                selected?AppTheme.Navy:AppTheme.Muted,
                TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis);
        };
        tabs.Invalidate();
    }
}
