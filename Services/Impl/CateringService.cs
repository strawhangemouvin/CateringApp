using CateringApp.Models.Entity;
using CateringApp.Models.ViewModel;
using CateringApp.Services.Context;
using CateringApp.Services.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CateringApp.Services.Impl;

public class CateringService : ICateringService
{
    private readonly CateringDbContext _context;
    private readonly IPasswordHasher<Pengguna> _passwordHasher;

    public CateringService(CateringDbContext context, IPasswordHasher<Pengguna> passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public DashboardViewModel GetDashboardSummary()
    {
        var last7Days = Enumerable.Range(0, 7)
            .Select(i => DateTime.Today.AddDays(-i))
            .Reverse()
            .ToList();

        var dailyOmset = last7Days.Select(date => new DailyOmsetViewModel
        {
            Tanggal = date.ToString("dd MMM"),
            Omset = _context.Pesanans
                .Where(x => x.DeletedAt == null && x.StatusPesanan == "Selesai" && x.TanggalPesan.Date == date.Date)
                .Sum(x => (decimal?)x.TotalBayar) ?? 0,
            JumlahPesanan = _context.Pesanans
                .Where(x => x.DeletedAt == null && x.TanggalPesan.Date == date.Date)
                .Count()
        }).ToList();

        return new DashboardViewModel
        {
            TotalPesanan = _context.Pesanans.Count(x => x.DeletedAt == null),
            TotalPendapatan = _context.Pesanans.Where(x => x.DeletedAt == null && x.StatusPesanan == "Selesai").Sum(x => (decimal?)x.TotalBayar) ?? 0,
            PesananPending = _context.Pesanans.Count(x => x.DeletedAt == null && x.StatusPesanan == "Pending"),
            PesananDiproses = _context.Pesanans.Count(x => x.DeletedAt == null && x.StatusPesanan == "Diproses"),
            TotalPelanggan = _context.Penggunas.Count(x => x.PeranId == 3 && x.DeletedAt == null), // Role ID 3 = User
            PesananTerbaru = _context.Pesanans.Include(p => p.Pengguna).Where(x => x.DeletedAt == null).OrderByDescending(x => x.TanggalPesan).Take(5).ToList(),
            JadwalPengiriman = _context.Pesanans.Include(p => p.Pengguna).Where(x => x.DeletedAt == null && (x.StatusPesanan == "Diproses" || x.StatusPesanan == "Dikirim")).OrderBy(x => x.TanggalPengiriman).Take(10).ToList(),
            PesananButuhVerifikasi = _context.Pesanans.Include(p => p.Pengguna).Include(p => p.Pembayaran).Where(x => x.DeletedAt == null && x.Pembayaran != null && x.Pembayaran.StatusVerifikasi == "Menunggu Verifikasi").OrderByDescending(x => x.Pembayaran!.TanggalBayar).ToList(),
            DailyOmset = dailyOmset
        };
    }

    public List<PaketMenu> GetAllPaket() =>
        _context.PaketMenus.Include(p => p.Kategori).Where(x => x.DeletedAt == null).ToList();

    public PaketMenu? GetPaketById(int id) =>
        _context.PaketMenus.FirstOrDefault(x => x.PaketId == id && x.DeletedAt == null);

    public void CreatePaket(PaketMenu paket)
    {
        paket.CreatedAt = DateTime.Now;
        paket.UpdatedAt = DateTime.Now;
        _context.PaketMenus.Add(paket);
        _context.SaveChanges();
    }

    public void UpdatePaket(PaketMenu paket)
    {
        var data = _context.PaketMenus.Find(paket.PaketId);
        if (data != null)
        {
            data.NamaPaket = paket.NamaPaket;
            data.KategoriId = paket.KategoriId;
            data.Harga = paket.Harga;
            data.DeskripsiMenu = paket.DeskripsiMenu;
            if (!string.IsNullOrEmpty(paket.Gambar))
            {
                data.Gambar = paket.Gambar;
            }
            data.UpdatedAt = DateTime.Now;
            _context.SaveChanges();
        }
    }

    public void SoftDeletePaket(int id)
    {
        var data = _context.PaketMenus.Find(id);
        if (data != null)
        {
            data.DeletedAt = DateTime.Now;
            _context.SaveChanges();
        }
    }

    public List<Pesanan> GetAllPesanan(int? penggunaId = null)
    {
        var query = _context.Pesanans.Include(p => p.Pengguna).Include(p => p.Pembayaran).Where(p => p.DeletedAt == null);
        if (penggunaId.HasValue)
            query = query.Where(p => p.PenggunaId == penggunaId.Value);
        return query.OrderByDescending(p => p.TanggalPesan).ToList();
    }

    public Pesanan? GetPesananById(int id) =>
        _context.Pesanans
            .Include(p => p.Pengguna)
            .Include(p => p.DetailPesanans).ThenInclude(d => d.Paket)
            .Include(p => p.Pembayaran)
            .FirstOrDefault(p => p.PesananId == id && p.DeletedAt == null);

    public int BuatPesanan(int penggunaId, PemesananViewModel model)
    {
        using var transaction = _context.Database.BeginTransaction();
        try
        {
            var paket = _context.PaketMenus.Find(model.PaketId);
            decimal subtotal = (paket?.Harga ?? 0) * model.JumlahPorsi;

            var pesanan = new Pesanan
            {
                PenggunaId = penggunaId,
                NomorPesanan = "ORD-" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                TanggalPesan = DateTime.Now,
                TanggalPengiriman = model.TanggalPengiriman,
                AlamatPengiriman = model.AlamatPengiriman,
                TotalBayar = subtotal,
                StatusPesanan = "Pending",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            _context.Pesanans.Add(pesanan);
            _context.SaveChanges();

            var detail = new DetailPesanan
            {
                PesananId = pesanan.PesananId,
                PaketId = model.PaketId,
                Jumlah = model.JumlahPorsi,
                HargaSatuan = paket?.Harga ?? 0,
                Subtotal = subtotal,
                Catatan = model.Catatan,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            _context.DetailPesanans.Add(detail);
            _context.SaveChanges();

            transaction.Commit();
            return pesanan.PesananId;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public void UpdateStatusPesanan(int pesananId, string status)
    {
        var pesanan = _context.Pesanans.Find(pesananId);
        if (pesanan != null)
        {
            pesanan.StatusPesanan = status;
            pesanan.UpdatedAt = DateTime.Now;
            _context.SaveChanges();
        }
    }

    public void SoftDeletePesanan(int id)
    {
        var pesanan = _context.Pesanans.Find(id);
        if (pesanan != null)
        {
            pesanan.DeletedAt = DateTime.Now;
            _context.SaveChanges();
        }
    }

    public void UploadBuktiBayar(UploadPembayaranViewModel model, string filePath)
    {
        var pembayaran = _context.Pembayarans.FirstOrDefault(p => p.PesananId == model.PesananId);
        if (pembayaran == null)
        {
            pembayaran = new Pembayaran
            {
                PesananId = model.PesananId,
                MetodePembayaran = model.MetodePembayaran,
                JumlahBayar = model.TotalBayar,
                TanggalBayar = DateTime.Now,
                BuktiTransfer = filePath,
                StatusVerifikasi = "Menunggu Verifikasi",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            _context.Pembayarans.Add(pembayaran);
        }
        else
        {
            pembayaran.MetodePembayaran = model.MetodePembayaran;
            pembayaran.BuktiTransfer = filePath;
            pembayaran.StatusVerifikasi = "Menunggu Verifikasi";
            pembayaran.UpdatedAt = DateTime.Now;
        }
        _context.SaveChanges();
    }

    public void VerifikasiPembayaran(int pesananId, string status)
    {
        var pembayaran = _context.Pembayarans.FirstOrDefault(p => p.PesananId == pesananId);
        if (pembayaran != null)
        {
            pembayaran.StatusVerifikasi = status;
            pembayaran.UpdatedAt = DateTime.Now;

            var pesanan = _context.Pesanans.Find(pesananId);
            if (pesanan != null && status == "Valid")
            {
                pesanan.StatusPesanan = "Diproses";
            }
            _context.SaveChanges();
        }
    }

    // Kategori Menu CRUD
    public List<KategoriMenu> GetAllKategori() =>
        _context.KategoriMenus.Where(x => x.DeletedAt == null).ToList();

    public KategoriMenu? GetKategoriById(int id) =>
        _context.KategoriMenus.FirstOrDefault(x => x.KategoriId == id && x.DeletedAt == null);

    public void CreateKategori(KategoriMenu kategori)
    {
        kategori.CreatedAt = DateTime.Now;
        kategori.UpdatedAt = DateTime.Now;
        _context.KategoriMenus.Add(kategori);
        _context.SaveChanges();
    }

    public void UpdateKategori(KategoriMenu kategori)
    {
        var data = _context.KategoriMenus.Find(kategori.KategoriId);
        if (data != null)
        {
            data.NamaKategori = kategori.NamaKategori;
            data.Deskripsi = kategori.Deskripsi;
            data.UpdatedAt = DateTime.Now;
            _context.SaveChanges();
        }
    }

    public void SoftDeleteKategori(int id)
    {
        var data = _context.KategoriMenus.Find(id);
        if (data != null)
        {
            data.DeletedAt = DateTime.Now;
            _context.SaveChanges();
        }
    }

    // Pengguna CRUD
    public List<Pengguna> GetAllPengguna() =>
        _context.Penggunas.Include(u => u.Peran).Where(x => x.DeletedAt == null).ToList();

    public Pengguna? GetPenggunaById(int id) =>
        _context.Penggunas.Include(u => u.Peran).FirstOrDefault(x => x.PenggunaId == id && x.DeletedAt == null);

    public void CreatePengguna(Pengguna pengguna)
    {
        pengguna.CreatedAt = DateTime.Now;
        pengguna.UpdatedAt = DateTime.Now;
        if (!string.IsNullOrEmpty(pengguna.PasswordHash))
        {
            pengguna.PasswordHash = _passwordHasher.HashPassword(pengguna, pengguna.PasswordHash);
        }
        _context.Penggunas.Add(pengguna);
        _context.SaveChanges();
    }

    public void UpdatePengguna(Pengguna pengguna)
    {
        var data = _context.Penggunas.Find(pengguna.PenggunaId);
        if (data != null)
        {
            data.PeranId = pengguna.PeranId;
            data.NamaLengkap = pengguna.NamaLengkap;
            data.Username = pengguna.Username;
            if (!string.IsNullOrEmpty(pengguna.PasswordHash))
            {
                data.PasswordHash = _passwordHasher.HashPassword(data, pengguna.PasswordHash);
            }
            data.Email = pengguna.Email;
            data.NomorTelepon = pengguna.NomorTelepon;
            data.Alamat = pengguna.Alamat;
            data.UpdatedAt = DateTime.Now;
            _context.SaveChanges();
        }
    }

    public void SoftDeletePengguna(int id)
    {
        var data = _context.Penggunas.Find(id);
        if (data != null)
        {
            data.DeletedAt = DateTime.Now;
            _context.SaveChanges();
        }
    }

    public List<Peran> GetAllPeran() =>
        _context.Perans.ToList();
}