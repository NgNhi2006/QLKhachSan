using System.Globalization;
using QRCoder;
using QLKhachSan.DAL;

namespace QLKhachSan.GUI;

internal static class BankTransferQr
{
    private static string Field(string id,string value)
    {
        if(value.Length>99)throw new ArgumentException("Dữ liệu QR chuyển khoản quá dài.");
        return id+value.Length.ToString("D2",CultureInfo.InvariantCulture)+value;
    }

    public static Bitmap Create(AppSettings settings,decimal amount,string reference)
    {
        var bank=settings.BankCode.Trim();
        var account=settings.BankAccount.Trim();
        if(bank.Length!=6 || !bank.All(char.IsAsciiDigit) || account.Length is <1 or >19 ||
            !account.All(char.IsAsciiDigit))
            throw new ArgumentException("BankCode phải là BIN 6 số và BankAccount phải là số tài khoản hợp lệ.");
        if(amount<=0 || amount!=decimal.Truncate(amount))
            throw new ArgumentException("Số tiền chuyển khoản phải là số nguyên dương.");
        var content=reference.Trim().ToUpperInvariant();
        if(content.Length is <1 or >25 || !content.All(c=>char.IsAsciiLetterOrDigit(c) || c is ' ' or '-'))
            throw new ArgumentException("Nội dung chuyển khoản chỉ dùng chữ không dấu, số, dấu cách hoặc dấu gạch ngang (tối đa 25 ký tự).");
        var beneficiary=Field("00",bank)+Field("01",account);
        var merchant=Field("00","A000000727")+Field("01",beneficiary)+Field("02","QRIBFTTA");
        var payload=Field("00","01")+Field("01","12")+Field("38",merchant)+Field("53","704")+
            Field("54",amount.ToString("0",CultureInfo.InvariantCulture))+Field("58","VN")+
            Field("62",Field("08",content))+"6304";
        var crc=0xFFFF;
        foreach(var ch in payload)
        {
            crc^=ch<<8;
            for(var i=0;i<8;i++)crc=(crc&0x8000)!=0?(crc<<1)^0x1021:crc<<1;
            crc&=0xFFFF;
        }
        payload+=crc.ToString("X4",CultureInfo.InvariantCulture);
        using var generator=new QRCodeGenerator();
        using var data=generator.CreateQrCode(payload,QRCodeGenerator.ECCLevel.M);
        using var qr=new QRCode(data);
        return qr.GetGraphic(6);
    }
}
