using Microsoft.Data.SqlClient;
using QLKhachSan.DTO;

namespace QLKhachSan.DAL;

public sealed partial class HotelTransaction
{
    public async Task<bool> PeriodLockedAsync(DateTime at) => (await Query(
        "SELECT TOP(1) 1 FROM dbo.KyKeToan WHERE NgayDauKy=DATEFROMPARTS(YEAR(@p0),MONTH(@p0),1)",
        r=>true,at)).Count>0;
    public async Task RequireOpenPeriodAsync(DateTime at)
    {
        if(await PeriodLockedAsync(at)) throw new BusinessException("Kỳ kế toán đã khóa.");
    }
    public Task<List<CashShift>> ShiftsAsync(DateTime from, DateTime until,int cashierId=0) => Query(@"
SELECT s.Ma,u.TenDangNhap,s.ThoiDiemMoCa,s.ThoiDiemDongCa,s.TienMatDauCa,s.TienMatKiemDem,s.TienMatDuKien,s.GiaiTrinh,s.TrangThai,
 COALESCE(SUM(CASE WHEN p.PhuongThucThanhToan=N'Tiền mặt' AND p.LoaiGiaoDich IN('Deposit','Checkout') THEN p.SoTien ELSE 0 END),0)
 + COALESCE((SELECT SUM(v.SoTien) FROM dbo.PhieuThuChi v WHERE v.MaCaTruc=s.Ma AND v.KenhThanhToan='Cash' AND v.LoaiPhieu='Receipt' AND v.ThoiDiemDaoButToan IS NULL),0),
 COALESCE(SUM(CASE WHEN p.PhuongThucThanhToan=N'Tiền mặt' AND p.LoaiGiaoDich='Refund' THEN p.SoTien ELSE 0 END),0)
 + COALESCE((SELECT SUM(v.SoTien) FROM dbo.PhieuThuChi v WHERE v.MaCaTruc=s.Ma AND v.KenhThanhToan='Cash' AND v.LoaiPhieu='Payment' AND v.ThoiDiemDaoButToan IS NULL),0),
 COALESCE(SUM(CASE WHEN p.PhuongThucThanhToan=N'Thẻ POS' THEN CASE WHEN p.LoaiGiaoDich='Refund' THEN -p.SoTien WHEN p.LoaiGiaoDich='Forfeit' THEN 0 ELSE p.SoTien END ELSE 0 END),0),
 COALESCE(SUM(CASE WHEN p.PhuongThucThanhToan=N'Chuyển khoản' THEN CASE WHEN p.LoaiGiaoDich='Refund' THEN -p.SoTien WHEN p.LoaiGiaoDich='Forfeit' THEN 0 ELSE p.SoTien END ELSE 0 END),0),
 COALESCE(SUM(CASE WHEN p.PhuongThucThanhToan=N'Công nợ OTA' THEN CASE WHEN p.LoaiGiaoDich='Refund' THEN -p.SoTien WHEN p.LoaiGiaoDich='Forfeit' THEN 0 ELSE p.SoTien END ELSE 0 END),0)
FROM dbo.CaTruc s JOIN dbo.TaiKhoanNhanVien u ON u.Ma=s.MaThuNgan
LEFT JOIN dbo.GiaoDichThanhToan p ON p.MaCaTruc=s.Ma
WHERE s.ThoiDiemMoCa>=@p0 AND s.ThoiDiemMoCa<@p1 AND (@p2=0 OR s.MaThuNgan=@p2)
GROUP BY s.Ma,u.TenDangNhap,s.ThoiDiemMoCa,s.ThoiDiemDongCa,s.TienMatDauCa,s.TienMatKiemDem,s.TienMatDuKien,s.GiaiTrinh,s.TrangThai
ORDER BY s.Ma DESC",r=>new CashShift(r.GetInt64(0),r.GetString(1),r.GetDateTime(2),Date(r,3),r.GetDecimal(4),r.IsDBNull(5)?null:r.GetDecimal(5),r.IsDBNull(6)?null:r.GetDecimal(6),r.IsDBNull(7)?null:r.GetString(7),r.GetString(8),r.GetDecimal(9),r.GetDecimal(10),r.GetDecimal(11),r.GetDecimal(12),r.GetDecimal(13)),from,until,cashierId);
    public async Task<(int CashierId,string Status)> ShiftIdentityAsync(long id) => (await Query(
        "SELECT MaThuNgan,TrangThai FROM dbo.CaTruc WHERE Ma=@p0",r=>(r.GetInt32(0),r.GetString(1)),id)).SingleOrDefault();
    public Task<long> OpenShiftAsync(int cashier,decimal opening) => Scalar(
        "INSERT dbo.CaTruc(MaThuNgan,TienMatDauCa) VALUES(@p0,@p1); SELECT CONVERT(bigint,SCOPE_IDENTITY());",cashier,opening);
    public async Task<long> RequireOpenShiftAsync(int cashier) => (await Query(
        "SELECT Ma FROM dbo.CaTruc WHERE MaThuNgan=@p0 AND TrangThai='Open'",r=>r.GetInt64(0),cashier)).SingleOrDefault() is var id && id>0
        ? id : throw new BusinessException("Hãy mở ca và nhập tiền mặt đầu ca trước khi thu/chi.");
    public async Task<decimal> ExpectedCashAsync(long shiftId)
    {
        var rows=await Query(@"
SELECT s.TienMatDauCa
 + COALESCE((SELECT SUM(CASE WHEN LoaiGiaoDich='Refund' THEN -SoTien WHEN LoaiGiaoDich='Forfeit' THEN 0 ELSE SoTien END) FROM dbo.GiaoDichThanhToan WHERE MaCaTruc=s.Ma AND PhuongThucThanhToan=N'Tiền mặt'),0)
 + COALESCE((SELECT SUM(CASE WHEN LoaiPhieu='Receipt' THEN SoTien ELSE -SoTien END) FROM dbo.PhieuThuChi WHERE MaCaTruc=s.Ma AND KenhThanhToan='Cash' AND ThoiDiemDaoButToan IS NULL),0)
FROM dbo.CaTruc s WHERE s.Ma=@p0 AND s.TrangThai='Open'",r=>r.GetDecimal(0),shiftId);
        return rows.SingleOrDefault();
    }
    public Task<int> SubmitShiftAsync(long id,decimal counted,decimal expected,string explanation) => Execute(
        "UPDATE dbo.CaTruc SET TrangThai='Submitted',ThoiDiemDongCa=SYSDATETIME(),TienMatKiemDem=@p1,TienMatDuKien=@p2,GiaiTrinh=@p3 WHERE Ma=@p0 AND TrangThai='Open'",id,counted,expected,explanation);
    public Task<int> LockShiftAsync(long id,int approver) => Execute(
        "UPDATE dbo.CaTruc SET TrangThai='Locked',ThoiDiemKhoa=SYSDATETIME(),MaNguoiKhoa=@p1 WHERE Ma=@p0 AND TrangThai='Submitted'",id,approver);
    public Task<long> PendingOrdersInShiftAsync(long id) => Scalar(
        "SELECT COUNT_BIG(*) FROM dbo.YeuCauDichVu WHERE MaCaTruc=@p0 AND ThoiDiemHuy IS NULL AND SoLuongDaGiao<SoLuong",id);
    public Task<int> LockPeriodAsync(DateTime month,int approver,string note) => Execute(
        "INSERT dbo.KyKeToan(NgayDauKy,MaNguoiKhoa,GhiChu) VALUES(@p0,@p1,@p2)",new DateTime(month.Year,month.Month,1),approver,note);
    public Task<long> UnlockedShiftsInMonthAsync(DateTime month) => Scalar(@"
SELECT COUNT_BIG(*) FROM dbo.CaTruc WHERE ThoiDiemMoCa>=@p0 AND ThoiDiemMoCa<@p1 AND TrangThai<>'Locked'",
        new DateTime(month.Year,month.Month,1),new DateTime(month.Year,month.Month,1).AddMonths(1));
    public Task<long> AddVoucherAsync(string type,string channel,string category,decimal amount,string party,string? reference,string note,int actor,long? shift) => Scalar(@"
INSERT dbo.PhieuThuChi(LoaiPhieu,KenhThanhToan,DanhMuc,SoTien,DoiTac,MaThamChieu,GhiChu,MaNguoiTao,MaCaTruc)
VALUES(@p0,@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8);
SELECT CONVERT(bigint,SCOPE_IDENTITY())",type,channel,category,amount,party,reference,note,actor,shift);
    public async Task<(DateTime AsOf,decimal Amount)?> OpeningBalanceAsync(string channel)
    {
        var rows=await Query("SELECT TinhDenThoiDiem,SoTien FROM dbo.SoDuDauKy WHERE KenhThanhToan=@p0",
            r=>(r.GetDateTime(0),r.GetDecimal(1)),channel);
        return rows.Count==0?null:rows[0];
    }
    public Task<int> SetOpeningBalanceAsync(string channel,DateTime asOf,decimal amount,string note,int actor) => Execute(
        "INSERT dbo.SoDuDauKy(KenhThanhToan,TinhDenThoiDiem,SoTien,GhiChu,MaNguoiThietLap) VALUES(@p0,@p1,@p2,@p3,@p4)",channel,asOf,amount,note,actor);
    public Task<List<Voucher>> VouchersAsync(DateTime from,DateTime until) => Query(@"
SELECT v.Ma,v.ThoiDiemHachToan,v.LoaiPhieu,v.KenhThanhToan,v.DanhMuc,v.SoTien,v.DoiTac,v.MaThamChieu,v.GhiChu,u.TenDangNhap,v.ThoiDiemDaoButToan
FROM dbo.PhieuThuChi v JOIN dbo.TaiKhoanNhanVien u ON u.Ma=v.MaNguoiTao
WHERE v.ThoiDiemHachToan>=@p0 AND v.ThoiDiemHachToan<@p1 ORDER BY v.Ma DESC",
        r=>new Voucher(r.GetInt64(0),r.GetDateTime(1),r.GetString(2),r.GetString(3),r.GetString(4),r.GetDecimal(5),r.GetString(6),r.IsDBNull(7)?null:r.GetString(7),r.GetString(8),r.GetString(9),!r.IsDBNull(10)),from,until);
    public Task<List<BankStatementLine>> BankLinesAsync(DateTime from,DateTime until) => Query(@"
SELECT Ma,ThoiDiemGiaoDich,KenhThanhToan,MaThamChieu,SoTien,MaGiaoDichDaDoiSoat,MaPhieuDaDoiSoat
FROM dbo.DongSaoKe WHERE ThoiDiemGiaoDich>=@p0 AND ThoiDiemGiaoDich<@p1 ORDER BY Ma DESC",
        r=>new BankStatementLine(r.GetInt64(0),r.GetDateTime(1),r.GetString(2),r.GetString(3),r.GetDecimal(4),r.IsDBNull(5)?null:r.GetInt64(5),r.IsDBNull(6)?null:r.GetInt64(6)),from,until);
    public Task<List<BankStatementLine>> UnmatchedBankLinesAsync() => Query(@"
SELECT TOP(1000) Ma,ThoiDiemGiaoDich,KenhThanhToan,MaThamChieu,SoTien,MaGiaoDichDaDoiSoat,MaPhieuDaDoiSoat
FROM dbo.DongSaoKe WHERE MaGiaoDichDaDoiSoat IS NULL AND MaPhieuDaDoiSoat IS NULL ORDER BY Ma",
        r=>new BankStatementLine(r.GetInt64(0),r.GetDateTime(1),r.GetString(2),r.GetString(3),r.GetDecimal(4),null,null));
    public Task<long> ImportBankLineAsync(DateTime at,string channel,string reference,decimal amount,int actor) => Scalar(
        "INSERT dbo.DongSaoKe(ThoiDiemGiaoDich,KenhThanhToan,MaThamChieu,SoTien,MaNguoiNhap) OUTPUT INSERTED.Ma VALUES(@p0,@p1,@p2,@p3,@p4)",at,channel,reference,amount,actor);
    public async Task<(long? Payment,long? Voucher)> FindBankMatchAsync(string channel,string reference,decimal amount)
    {
        var payments=await Query(@"
SELECT p.Ma FROM dbo.GiaoDichThanhToan p WHERE p.MaThamChieuNgoai=@p1 AND
 ((@p0='Bank' AND p.PhuongThucThanhToan=N'Chuyển khoản') OR (@p0='POS' AND p.PhuongThucThanhToan=N'Thẻ POS'))
 AND CASE WHEN p.LoaiGiaoDich='Refund' THEN -p.SoTien WHEN p.LoaiGiaoDich='Forfeit' THEN 0 ELSE p.SoTien END=@p2",
            r=>r.GetInt64(0),channel,reference,amount);
        var vouchers=await Query(@"
SELECT v.Ma FROM dbo.PhieuThuChi v WHERE v.KenhThanhToan=@p0 AND v.MaThamChieu=@p1
 AND v.ThoiDiemDaoButToan IS NULL AND CASE WHEN v.LoaiPhieu='Receipt' THEN v.SoTien ELSE -v.SoTien END=@p2",
            r=>r.GetInt64(0),channel,reference,amount);
        if(payments.Count+vouchers.Count!=1)return (null,null);
        return (payments.SingleOrDefault() is var p && p>0?p:null,vouchers.SingleOrDefault() is var v && v>0?v:null);
    }
    public Task<int> SetBankMatchAsync(long line,long? payment,long? voucher) => Execute(
        "UPDATE dbo.DongSaoKe SET MaGiaoDichDaDoiSoat=@p1,MaPhieuDaDoiSoat=@p2 WHERE Ma=@p0 AND MaGiaoDichDaDoiSoat IS NULL AND MaPhieuDaDoiSoat IS NULL",line,payment,voucher);
    public async Task<(string Type,decimal Amount,bool Reversed,decimal Allocated)> VoucherAllocationInfoAsync(long id) => (await Query(@"
SELECT v.LoaiPhieu,v.SoTien,CAST(CASE WHEN v.ThoiDiemDaoButToan IS NULL THEN 0 ELSE 1 END AS bit),
 COALESCE((SELECT SUM(a.SoTien) FROM dbo.PhanBoCongNo a WHERE a.MaPhieuThuChi=v.Ma),0)
FROM dbo.PhieuThuChi v WHERE v.Ma=@p0",
        r=>(r.GetString(0),r.GetDecimal(1),r.GetBoolean(2),r.GetDecimal(3)),id)).SingleOrDefault();
    public async Task<(decimal Opening,decimal Receipts,decimal Payments,decimal Closing)> BookAsync(string channel,DateTime from,DateTime until)
    {
        var initial=await OpeningBalanceAsync(channel);
        var lower=initial?.AsOf??new DateTime(1900,1,1);
        if(from<lower)throw new BusinessException("Khoảng báo cáo trước ngày mở sổ của kênh này.");
        var rows=await Query(@"
SELECT
 COALESCE(SUM(CASE WHEN ThoiDiemBienDong<@p1 THEN SoTien ELSE 0 END),0),
 COALESCE(SUM(CASE WHEN ThoiDiemBienDong>=@p1 AND SoTien>0 THEN SoTien ELSE 0 END),0),
 COALESCE(SUM(CASE WHEN ThoiDiemBienDong>=@p1 AND SoTien<0 THEN -SoTien ELSE 0 END),0)
FROM (
 SELECT ThoiDiemHachToan ThoiDiemBienDong,CASE WHEN LoaiPhieu='Receipt' THEN SoTien ELSE -SoTien END SoTien
 FROM dbo.PhieuThuChi WHERE KenhThanhToan=@p0 AND ThoiDiemHachToan>=@p3 AND ThoiDiemHachToan<@p2 AND ThoiDiemDaoButToan IS NULL
 UNION ALL
 SELECT ThoiDiemTao,CASE WHEN LoaiGiaoDich='Refund' THEN -SoTien WHEN LoaiGiaoDich='Forfeit' THEN 0 ELSE SoTien END
 FROM dbo.GiaoDichThanhToan WHERE ThoiDiemTao>=@p3 AND ThoiDiemTao<@p2 AND
  ((@p0='Cash' AND PhuongThucThanhToan=N'Tiền mặt') OR (@p0='Bank' AND PhuongThucThanhToan=N'Chuyển khoản') OR
   (@p0='POS' AND PhuongThucThanhToan=N'Thẻ POS') OR (@p0='OTA' AND PhuongThucThanhToan=N'Công nợ OTA'))
) ledger",
            r=>(r.GetDecimal(0),r.GetDecimal(1),r.GetDecimal(2)),channel,from,until,lower);
        var x=rows[0];var opening=(initial?.Amount??0)+x.Item1;
        return (opening,x.Item2,x.Item3,opening+x.Item2-x.Item3);
    }
    public Task<long> AddDebtAsync(string type,string party,long? invoice,DateTime due,decimal amount,string note,int actor) => Scalar(@"
INSERT dbo.CongNo(LoaiCongNo,DoiTac,MaHoaDon,HanThanhToan,SoTien,GhiChu,MaNguoiTao)
VALUES(@p0,@p1,@p2,@p3,@p4,@p5,@p6);
SELECT CONVERT(bigint,SCOPE_IDENTITY())",type,party,invoice,due.Date,amount,note,actor);
    public Task<List<Debt>> DebtsAsync() => Query(@"
SELECT d.Ma,d.LoaiCongNo,d.DoiTac,d.MaHoaDon,d.ThoiDiemPhatSinhCongNo,d.HanThanhToan,d.SoTien,
 COALESCE(SUM(CASE WHEN v.ThoiDiemDaoButToan IS NULL THEN a.SoTien ELSE 0 END),0),d.GhiChu
FROM dbo.CongNo d LEFT JOIN dbo.PhanBoCongNo a ON a.MaCongNo=d.Ma
LEFT JOIN dbo.PhieuThuChi v ON v.Ma=a.MaPhieuThuChi
WHERE d.ThoiDiemHuyBo IS NULL
GROUP BY d.Ma,d.LoaiCongNo,d.DoiTac,d.MaHoaDon,d.ThoiDiemPhatSinhCongNo,d.HanThanhToan,d.SoTien,d.GhiChu ORDER BY d.HanThanhToan,d.Ma",
        r=>new Debt(r.GetInt64(0),r.GetString(1),r.GetString(2),r.IsDBNull(3)?null:r.GetInt64(3),r.GetDateTime(4),r.GetDateTime(5),r.GetDecimal(6),r.GetDecimal(7),r.GetString(8)));
    public Task<int> CancelDebtAsync(long debt,int actor,string reason) => Execute(
        "UPDATE dbo.CongNo SET ThoiDiemHuyBo=SYSDATETIME(),MaNguoiHuy=@p1,LyDoHuy=@p2 WHERE Ma=@p0 AND ThoiDiemHuyBo IS NULL",debt,actor,reason);
    public Task<int> AllocateDebtAsync(long debtId,long voucherId,decimal amount,int actor) => Execute(
        "INSERT dbo.PhanBoCongNo(MaCongNo,MaPhieuThuChi,SoTien,MaNguoiTao) VALUES(@p0,@p1,@p2,@p3)",debtId,voucherId,amount,actor);
    public Task<List<StockItem>> StockAsync() => Query(
        "SELECT Ma,MaDichVu,Ten,DonViTinh,SoLuong,GiaVonBinhQuan,NguongDatHang,DangHoatDong FROM dbo.HangTonKho ORDER BY Ten",
        r=>new StockItem(r.GetInt32(0),r.IsDBNull(1)?null:r.GetInt32(1),r.GetString(2),r.GetString(3),r.GetDecimal(4),r.GetDecimal(5),r.GetDecimal(6),r.GetBoolean(7)));
    public Task<int> AddStockItemAsync(string name,string unit,int? service,decimal reorder) => Execute(
        "INSERT dbo.HangTonKho(Ten,DonViTinh,MaDichVu,NguongDatHang) VALUES(@p0,@p1,@p2,@p3)",name,unit,service,reorder);
    public Task<int> UpdateStockAsync(int item,decimal quantity,decimal average) => Execute(
        "UPDATE dbo.HangTonKho SET SoLuong=@p1,GiaVonBinhQuan=@p2 WHERE Ma=@p0",item,quantity,average);
    public Task<int> MoveStockAsync(int item,string kind,decimal quantity,decimal cost,string reason,int actor,long? order=null) => Execute(@"
INSERT dbo.BienDongKho(MaMatHang,LoaiGiaoDich,SoLuong,GiaVonDonVi,LyDo,MaNguoiTao,MaYeuCauDichVu)
VALUES(@p0,@p1,@p2,@p3,@p4,@p5,@p6)",item,kind,quantity,cost,reason,actor,order);
    public async Task ConsumeOrderStockAsync(long orderId,int quantity,int actor)
    {
        var rows=await Query(@"SELECT i.Ma,i.SoLuong,i.GiaVonBinhQuan FROM dbo.YeuCauDichVu o
JOIN dbo.HangTonKho i ON i.MaDichVu=o.MaDichVu WHERE o.Ma=@p0 AND i.DangHoatDong=1",
            r=>(Id:r.GetInt32(0),Quantity:r.GetDecimal(1),Cost:r.GetDecimal(2)),orderId);
        if(rows.Count==0)return;
        var stock=rows[0];
        if(stock.Quantity<quantity)throw new BusinessException("Tồn minibar không đủ để giao dịch vụ.");
        await UpdateStockAsync(stock.Id,stock.Quantity-quantity,stock.Cost);
        await MoveStockAsync(stock.Id,"Sale",-quantity,stock.Cost,$"Giao dịch vụ #{orderId}",actor,orderId);
    }
    public Task<int> ReportHousekeepingAsync(int item,long? stay,decimal qty,string note,int actor) => Execute(
        "INSERT dbo.TieuThuBuongPhong(MaMatHang,MaLuotLuuTru,SoLuong,GhiChu,MaNguoiTao) VALUES(@p0,@p1,@p2,@p3,@p4)",item,stay,qty,note,actor);
    public Task<List<StockMovement>> StockMovementsAsync(DateTime from,DateTime until) => Query(@"
SELECT m.Ma,m.ThoiDiemBienDong,i.Ten,m.LoaiGiaoDich,m.SoLuong,m.GiaVonDonVi,m.LyDo
FROM dbo.BienDongKho m JOIN dbo.HangTonKho i ON i.Ma=m.MaMatHang
WHERE m.ThoiDiemBienDong>=@p0 AND m.ThoiDiemBienDong<@p1 ORDER BY m.Ma DESC",
        r=>new StockMovement(r.GetInt64(0),r.GetDateTime(1),r.GetString(2),r.GetString(3),r.GetDecimal(4),r.GetDecimal(5),r.GetString(6)),from,until);
    public Task<List<MinibarReconciliation>> ReconcileMinibarAsync(DateTime from,DateTime until) => Query(@"
SELECT i.Ten,
 COALESCE((SELECT SUM(o.SoLuong) FROM dbo.YeuCauDichVu o WHERE o.MaDichVu=i.MaDichVu AND o.ThoiDiemGoi>=@p0 AND o.ThoiDiemGoi<@p1 AND o.ThoiDiemHuy IS NULL),0),
 COALESCE((SELECT SUM(h.SoLuong) FROM dbo.TieuThuBuongPhong h WHERE h.MaMatHang=i.Ma AND h.ThoiDiemBaoCao>=@p0 AND h.ThoiDiemBaoCao<@p1),0),
 COALESCE((SELECT -SUM(m.SoLuong) FROM dbo.BienDongKho m WHERE m.MaMatHang=i.Ma AND m.LoaiGiaoDich='Sale' AND m.ThoiDiemBienDong>=@p0 AND m.ThoiDiemBienDong<@p1),0),
 i.SoLuong
FROM dbo.HangTonKho i WHERE i.DangHoatDong=1 ORDER BY i.Ten",
        r=>new MinibarReconciliation(r.GetString(0),r.GetDecimal(1),r.GetDecimal(2),r.GetDecimal(3),r.GetDecimal(4),r.GetDecimal(1)-r.GetDecimal(2)),from,until);
    public Task<int> SaveInvoiceFinanceAsync(long invoice,decimal vat,decimal service,int rounding,string? eNumber) => Execute(@"
MERGE dbo.ThongTinTaiChinhHoaDon AS target USING (SELECT @p0 AS MaHoaDon) AS source ON target.MaHoaDon=source.MaHoaDon
WHEN MATCHED THEN UPDATE SET TyLeVAT=@p1,TyLePhiDichVu=@p2,DonViLamTron=@p3,SoHoaDonDienTu=@p4,ThoiDiemPhatHanhHoaDonDienTu=CASE WHEN @p4 IS NULL THEN NULL ELSE SYSDATETIME() END
WHEN NOT MATCHED THEN INSERT(MaHoaDon,TyLeVAT,TyLePhiDichVu,DonViLamTron,SoHoaDonDienTu,ThoiDiemPhatHanhHoaDonDienTu)
VALUES(@p0,@p1,@p2,@p3,@p4,CASE WHEN @p4 IS NULL THEN NULL ELSE SYSDATETIME() END);",invoice,vat,service,rounding,eNumber);
    public async Task<bool> HasEInvoiceAsync(long invoice) => (await Query(
        "SELECT 1 FROM dbo.ThongTinTaiChinhHoaDon WHERE MaHoaDon=@p0 AND SoHoaDonDienTu IS NOT NULL",r=>true,invoice)).Count>0;
    public Task<int> AddAdjustmentAsync(long invoice,string category,decimal amount,string reason,int approver) => Execute(
        "INSERT dbo.DieuChinhHoaDon(MaHoaDon,DanhMuc,SoTien,LyDo,MaNguoiPheDuyet) VALUES(@p0,@p1,@p2,@p3,@p4)",invoice,category,amount,reason,approver);
    public Task<int> VoidInvoiceAsync(long invoice,string reason,string explanation,int actor) => Execute(
        "INSERT dbo.HoaDonHuy(MaHoaDon,MaLyDo,GiaiTrinh,MaNguoiHuyHoaDon) VALUES(@p0,@p1,@p2,@p3)",invoice,reason,explanation,actor);
    public Task<int> AddBillGroupAsync(string name,int actor) => Execute(
        "INSERT dbo.NhomHoaDon(Ten,MaNguoiTao) VALUES(@p0,@p1)",name,actor);
    public Task<long> NewBillGroupAsync(string name,int actor) => Scalar(
        "INSERT dbo.NhomHoaDon(Ten,MaNguoiTao) OUTPUT INSERTED.Ma VALUES(@p0,@p1)",name,actor);
    public Task<int> AddBillShareAsync(long group,long invoice,string payer,decimal amount) => Execute(
        "INSERT dbo.PhanChiaHoaDon(MaNhomHoaDon,MaHoaDon,NguoiThanhToan,SoTien) VALUES(@p0,@p1,@p2,@p3)",group,invoice,payer,amount);
    public Task<List<BillShareView>> BillSharesAsync() => Query(@"
SELECT g.Ma,g.Ten,s.MaHoaDon,s.NguoiThanhToan,s.SoTien,g.ThoiDiemLap
FROM dbo.PhanChiaHoaDon s JOIN dbo.NhomHoaDon g ON g.Ma=s.MaNhomHoaDon ORDER BY g.Ma DESC,s.Ma",
        r=>new BillShareView(r.GetInt64(0),r.GetString(1),r.GetInt64(2),r.GetString(3),r.GetDecimal(4),r.GetDateTime(5)));
    public Task<List<Invoice>> GroupInvoicesAsync(long groupId) => Query("""
        SELECT i.Ma,i.MaLuotLuuTru,i.SoPhongHoaDon,i.TenKhach,i.ThoiDiemLapHoaDon,i.TienPhong,i.TienDichVu,i.TienCoc,i.TienDaThu,i.TienDaHoan,i.PhuongThucThanhToan
        FROM dbo.PhanChiaHoaDon x JOIN dbo.HoaDon i ON i.Ma=x.MaHoaDon
        WHERE x.MaNhomHoaDon=@p0 AND NOT EXISTS(SELECT 1 FROM dbo.HoaDonHuy v WHERE v.MaHoaDon=i.Ma)
        ORDER BY i.SoPhongHoaDon,i.Ma
        """,r=>new Invoice(r.GetInt64(0),r.GetInt64(1),r.GetString(2),r.GetString(3),r.GetDateTime(4),r.GetDecimal(5),r.GetDecimal(6),r.GetDecimal(7),r.GetDecimal(8),r.GetDecimal(9),r.GetString(10)),groupId);
    public Task<List<(long Id,decimal Total,bool Void)>> InvoiceAmountsAsync() => Query(
        "SELECT i.Ma,i.TienPhong+i.TienDichVu-COALESCE((SELECT SUM(a.SoTien) FROM dbo.DieuChinhHoaDon a WHERE a.MaHoaDon=i.Ma),0),CASE WHEN v.MaHoaDon IS NULL THEN CAST(0 AS bit) ELSE CAST(1 AS bit) END FROM dbo.HoaDon i LEFT JOIN dbo.HoaDonHuy v ON v.MaHoaDon=i.Ma",
        r=>(r.GetInt64(0),r.GetDecimal(1),r.GetBoolean(2)));
    public Task<List<InvoiceControl>> InvoiceControlsAsync(DateTime from,DateTime until) => Query(@"
SELECT i.Ma,CAST(CASE WHEN v.MaHoaDon IS NULL THEN 0 ELSE 1 END AS bit),
 COALESCE(f.TyLeVAT,0),COALESCE(f.TyLePhiDichVu,0),f.SoHoaDonDienTu,
 COALESCE((SELECT SUM(a.SoTien) FROM dbo.DieuChinhHoaDon a WHERE a.MaHoaDon=i.Ma),0)
FROM dbo.HoaDon i LEFT JOIN dbo.HoaDonHuy v ON v.MaHoaDon=i.Ma
LEFT JOIN dbo.ThongTinTaiChinhHoaDon f ON f.MaHoaDon=i.Ma
WHERE i.ThoiDiemLapHoaDon>=@p0 AND i.ThoiDiemLapHoaDon<@p1",
        r=>new InvoiceControl(r.GetInt64(0),r.GetBoolean(1),r.GetDecimal(2),r.GetDecimal(3),r.IsDBNull(4)?null:r.GetString(4),r.GetDecimal(5)),from,until);
    public Task<List<Invoice>> AllFinanceInvoicesAsync(DateTime from,DateTime until) => Query(@"
SELECT Ma,MaLuotLuuTru,SoPhongHoaDon,TenKhach,ThoiDiemLapHoaDon,TienPhong,TienDichVu,TienCoc,TienDaThu,TienDaHoan,PhuongThucThanhToan
FROM dbo.HoaDon WHERE ThoiDiemLapHoaDon>=@p0 AND ThoiDiemLapHoaDon<@p1 ORDER BY Ma DESC",
        r=>new Invoice(r.GetInt64(0),r.GetInt64(1),r.GetString(2),r.GetString(3),r.GetDateTime(4),r.GetDecimal(5),r.GetDecimal(6),r.GetDecimal(7),r.GetDecimal(8),r.GetDecimal(9),r.GetString(10)),from,until);
    public async Task<FinanceSummary> FinanceSummaryAsync(DateTime from,DateTime until)
    {
        var row=await Query(@"
SELECT
 COALESCE((SELECT SUM(i.TienPhong) FROM dbo.HoaDon i LEFT JOIN dbo.HoaDonHuy v ON v.MaHoaDon=i.Ma WHERE v.MaHoaDon IS NULL AND i.ThoiDiemLapHoaDon>=@p0 AND i.ThoiDiemLapHoaDon<@p1),0),
 COALESCE((SELECT SUM(o.SoLuong*o.DonGia) FROM dbo.YeuCauDichVu o JOIN dbo.HoaDon i ON i.MaLuotLuuTru=o.MaLuotLuuTru LEFT JOIN dbo.HoaDonHuy v ON v.MaHoaDon=i.Ma WHERE v.MaHoaDon IS NULL AND o.ThoiDiemHuy IS NULL AND o.DanhMuc=N'Minibar' AND i.ThoiDiemLapHoaDon>=@p0 AND i.ThoiDiemLapHoaDon<@p1),0),
 COALESCE((SELECT SUM(i.TienDichVu) FROM dbo.HoaDon i LEFT JOIN dbo.HoaDonHuy v ON v.MaHoaDon=i.Ma WHERE v.MaHoaDon IS NULL AND i.ThoiDiemLapHoaDon>=@p0 AND i.ThoiDiemLapHoaDon<@p1),0),
 COALESCE((SELECT SUM(a.SoTien) FROM dbo.DieuChinhHoaDon a JOIN dbo.HoaDon i ON i.Ma=a.MaHoaDon LEFT JOIN dbo.HoaDonHuy v ON v.MaHoaDon=i.Ma WHERE v.MaHoaDon IS NULL AND a.ThoiDiemLap>=@p0 AND a.ThoiDiemLap<@p1),0),
 COALESCE((SELECT SUM(-m.SoLuong*m.GiaVonDonVi) FROM dbo.BienDongKho m WHERE m.LoaiGiaoDich='Sale' AND m.ThoiDiemBienDong>=@p0 AND m.ThoiDiemBienDong<@p1),0),
 COALESCE((SELECT SUM(v.SoTien) FROM dbo.PhieuThuChi v WHERE v.LoaiPhieu='Payment' AND v.DanhMuc NOT IN(N'Mua hàng tồn kho',N'Trả công nợ',N'Tạm ứng') AND v.ThoiDiemDaoButToan IS NULL AND v.ThoiDiemHachToan>=@p0 AND v.ThoiDiemHachToan<@p1),0)
 + COALESCE((SELECT SUM(-m.SoLuong*m.GiaVonDonVi) FROM dbo.BienDongKho m WHERE m.LoaiGiaoDich IN('Spoilage','Internal') AND m.ThoiDiemBienDong>=@p0 AND m.ThoiDiemBienDong<@p1),0),
 COALESCE((SELECT SUM(CASE WHEN DATEDIFF(day,CONVERT(date,s.ThoiDiemNhanPhong),CONVERT(date,i.ThoiDiemLapHoaDon))<1 THEN 1 ELSE DATEDIFF(day,CONVERT(date,s.ThoiDiemNhanPhong),CONVERT(date,i.ThoiDiemLapHoaDon)) END) FROM dbo.HoaDon i JOIN dbo.LuotLuuTru s ON s.Ma=i.MaLuotLuuTru LEFT JOIN dbo.HoaDonHuy v ON v.MaHoaDon=i.Ma WHERE v.MaHoaDon IS NULL AND i.ThoiDiemLapHoaDon>=@p0 AND i.ThoiDiemLapHoaDon<@p1),0),
 (SELECT COUNT(*) FROM dbo.Phong)*DATEDIFF(day,@p0,@p1)
",r=>new FinanceSummary(r.GetDecimal(0),r.GetDecimal(1),r.GetDecimal(2)-r.GetDecimal(1),r.GetDecimal(3),r.GetDecimal(4),r.GetDecimal(5),r.GetInt32(6),r.GetInt32(7)),from,until);
        return row[0];
    }
}
