using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Xml;
using QLKhachSan.DAL;

namespace QLKhachSan.GUI;

internal static class ExcelExport
{
    private const string MainNs="http://schemas.openxmlformats.org/spreadsheetml/2006/main";
    public static void Attach(DataGridView grid,string title)
    {
        var menu=grid.ContextMenuStrip??new ContextMenuStrip();
        if(!menu.Items.Cast<ToolStripItem>().Any(x=>x.Text=="Xuất Excel (.xlsx)"))
            menu.Items.Add("Xuất Excel (.xlsx)",null,(_,_)=>ExportGrid((IWin32Window?)grid.FindForm()??grid,grid,title,DateTime.Now));
        grid.ContextMenuStrip=menu;
    }
    public static void ExportGrid(IWin32Window owner,DataGridView grid,string title,DateTime created)
    {
        using var save=new SaveFileDialog {Filter="Excel Workbook (*.xlsx)|*.xlsx",FileName=$"{SafeName(title)}-{created:yyyyMMdd-HHmm}.xlsx",OverwritePrompt=true};
        if(save.ShowDialog(owner)!=DialogResult.OK)return;
        Write(save.FileName,grid,title,created);
        MessageBox.Show(owner,"Đã xuất Excel .xlsx.","Xuất báo cáo");
    }
    private static string SafeName(string value) => string.Concat(value.Select(c=>Path.GetInvalidFileNameChars().Contains(c)?'_':c));
    private static bool IsMoneyColumn(DataGridViewColumn column)
    {
        var name=column.Name+" "+column.HeaderText;
        return new[]{"Tiền","DoanhThu","Giá","Vốn","ChiPhí","ThuTiền","ThuThêm","Hoàn","Cọc","Amount","Cost","Revenue","Profit","ADR","RevPAR","LýThuyết","ThựcĐếm","ChênhLệch","GiảmTrừ","DịchVụKhác","Minibar","ĐầuCa","SốGốc","ĐãTrả","CònLại","OpeningCash","ExpectedCash"}
            .Any(key=>name.Contains(key,StringComparison.OrdinalIgnoreCase));
    }
    public static void Write(string path,DataGridView grid,string title,DateTime created)
    {
        using var file=new FileStream(path,FileMode.Create,FileAccess.Write);
        using var zip=new ZipArchive(file,ZipArchiveMode.Create);
        static void Entry(ZipArchive zip,string name,Action<XmlWriter> write)
        {
            var entry=zip.CreateEntry(name,CompressionLevel.Optimal);
            using var stream=entry.Open();using var xml=XmlWriter.Create(stream,new XmlWriterSettings {Encoding=new UTF8Encoding(false),Indent=false});
            write(xml);
        }
        Entry(zip,"[Content_Types].xml",x=>
        {
            x.WriteStartElement("Types","http://schemas.openxmlformats.org/package/2006/content-types");
            x.WriteStartElement("Default");x.WriteAttributeString("Extension","rels");x.WriteAttributeString("ContentType","application/vnd.openxmlformats-package.relationships+xml");x.WriteEndElement();
            x.WriteStartElement("Default");x.WriteAttributeString("Extension","xml");x.WriteAttributeString("ContentType","application/xml");x.WriteEndElement();
            foreach(var (part,type) in new[]{("/xl/workbook.xml","application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"),("/xl/worksheets/sheet1.xml","application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"),("/xl/styles.xml","application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml")})
            {x.WriteStartElement("Override");x.WriteAttributeString("PartName",part);x.WriteAttributeString("ContentType",type);x.WriteEndElement();}
            x.WriteEndElement();
        });
        Entry(zip,"_rels/.rels",x=>
        {
            x.WriteStartElement("Relationships","http://schemas.openxmlformats.org/package/2006/relationships");
            x.WriteStartElement("Relationship");x.WriteAttributeString("Id","rId1");x.WriteAttributeString("Type","http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument");x.WriteAttributeString("Target","xl/workbook.xml");x.WriteEndElement();x.WriteEndElement();
        });
        Entry(zip,"xl/_rels/workbook.xml.rels",x=>
        {
            x.WriteStartElement("Relationships","http://schemas.openxmlformats.org/package/2006/relationships");
            foreach(var (id,type,target) in new[]{("rId1","worksheet","worksheets/sheet1.xml"),("rId2","styles","styles.xml")})
            {x.WriteStartElement("Relationship");x.WriteAttributeString("Id",id);x.WriteAttributeString("Type","http://schemas.openxmlformats.org/officeDocument/2006/relationships/"+type);x.WriteAttributeString("Target",target);x.WriteEndElement();}
            x.WriteEndElement();
        });
        Entry(zip,"xl/workbook.xml",x=>
        {
            x.WriteStartElement("workbook",MainNs);x.WriteStartElement("sheets",MainNs);x.WriteStartElement("sheet",MainNs);
            x.WriteAttributeString("name","Bao cao");x.WriteAttributeString("sheetId","1");x.WriteAttributeString("r","id","http://schemas.openxmlformats.org/officeDocument/2006/relationships","rId1");
            x.WriteEndElement();x.WriteEndElement();x.WriteEndElement();
        });
        Entry(zip,"xl/styles.xml",x=>
        {
            x.WriteStartElement("styleSheet",MainNs);
            x.WriteStartElement("numFmts",MainNs);x.WriteAttributeString("count","1");x.WriteStartElement("numFmt",MainNs);x.WriteAttributeString("numFmtId","164");x.WriteAttributeString("formatCode","#,##0 \"VNĐ\"");x.WriteEndElement();x.WriteEndElement();
            x.WriteStartElement("fonts",MainNs);x.WriteAttributeString("count","2");
            x.WriteStartElement("font",MainNs);x.WriteElementString("sz",MainNs,"11");x.WriteElementString("name",MainNs,"Segoe UI");x.WriteEndElement();
            x.WriteStartElement("font",MainNs);x.WriteElementString("b",MainNs,"");x.WriteElementString("sz",MainNs,"11");x.WriteElementString("name",MainNs,"Segoe UI");x.WriteEndElement();x.WriteEndElement();
            x.WriteStartElement("fills",MainNs);x.WriteAttributeString("count","2");x.WriteStartElement("fill",MainNs);x.WriteStartElement("patternFill",MainNs);x.WriteAttributeString("patternType","none");x.WriteEndElement();x.WriteEndElement();
            x.WriteStartElement("fill",MainNs);x.WriteStartElement("patternFill",MainNs);x.WriteAttributeString("patternType","gray125");x.WriteEndElement();x.WriteEndElement();x.WriteEndElement();
            x.WriteStartElement("borders",MainNs);x.WriteAttributeString("count","1");x.WriteStartElement("border",MainNs);x.WriteEndElement();x.WriteEndElement();
            x.WriteStartElement("cellStyleXfs",MainNs);x.WriteAttributeString("count","1");x.WriteStartElement("xf",MainNs);x.WriteAttributeString("numFmtId","0");x.WriteAttributeString("fontId","0");x.WriteAttributeString("fillId","0");x.WriteAttributeString("borderId","0");x.WriteEndElement();x.WriteEndElement();
            x.WriteStartElement("cellXfs",MainNs);x.WriteAttributeString("count","3");
            foreach(var (num,font) in new[]{("0","0"),("164","0"),("0","1")})
            {x.WriteStartElement("xf",MainNs);x.WriteAttributeString("numFmtId",num);x.WriteAttributeString("fontId",font);x.WriteAttributeString("fillId","0");x.WriteAttributeString("borderId","0");x.WriteAttributeString("xfId","0");if(num=="164")x.WriteAttributeString("applyNumberFormat","1");x.WriteEndElement();}
            x.WriteEndElement();x.WriteEndElement();
        });
        var columns=grid.Columns.Cast<DataGridViewColumn>().Where(c=>c.Visible).OrderBy(c=>c.DisplayIndex).ToArray();
        Entry(zip,"xl/worksheets/sheet1.xml",x=>
        {
            x.WriteStartElement("worksheet",MainNs);x.WriteStartElement("sheetData",MainNs);
            void Cell(object? value,int style=0)
            {
                x.WriteStartElement("c",MainNs);
                if(style!=0)x.WriteAttributeString("s",style.ToString(CultureInfo.InvariantCulture));
                if(value is decimal or int or long or double or float)
                    x.WriteElementString("v",MainNs,Convert.ToString(value,CultureInfo.InvariantCulture));
                else
                {
                    x.WriteAttributeString("t","inlineStr");x.WriteStartElement("is",MainNs);
                    x.WriteElementString("t",MainNs,value is DateTime date?date.ToString("dd/MM/yyyy HH:mm",CultureInfo.GetCultureInfo("vi-VN")):value?.ToString()??"");x.WriteEndElement();
                }
                x.WriteEndElement();
            }
            void Row(int index,Action cells){x.WriteStartElement("row",MainNs);x.WriteAttributeString("r",index.ToString(CultureInfo.InvariantCulture));cells();x.WriteEndElement();}
            Row(1,()=>Cell(title,2));Row(2,()=>Cell("Ngày lập: "+created.ToString("dd/MM/yyyy HH:mm")));
            Row(3,()=>Cell("Đơn vị: "+AppSettings.Load().HotelName));
            Row(5,()=>{foreach(var column in columns)Cell(column.HeaderText,2);});
            var index=6;
            foreach(DataGridViewRow row in grid.Rows)
            {
                if(row.IsNewRow)continue;
                Row(index++,()=>{foreach(var column in columns){var value=row.Cells[column.Index].Value;Cell(value,value is decimal && IsMoneyColumn(column)?1:0);}});
            }
            x.WriteEndElement();x.WriteEndElement();
        });
    }
}
