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
        var latestOrderDate = _context.Pesanans
            .Where(x => x.DeletedAt == null)
            .Max(x => (DateTime?)(x.TanggalPengiriman > x.TanggalPesan ? x.TanggalPengiriman : x.TanggalPesan))?.Date ?? DateTime.Today;

        var referenceDate = latestOrderDate > DateTime.Today ? latestOrderDate : DateTime.Today;

        var last7Days = Enumerable.Range(0, 7)
            .Select(i => referenceDate.AddDays(-i))
            .Reverse()
            .ToList();

        var dailyOmset = last7Days.Select(date => new DailyOmsetViewModel
        {
            Tanggal = date.ToString("dd MMM"),
            Omset = _context.Pesanans
                .Where(x => x.DeletedAt == null && x.StatusPesanan == "Selesai" && (x.TanggalPengiriman.Date == date.Date || x.TanggalPesan.Date == date.Date))
                .Sum(x => (decimal?)x.TotalBayar) ?? 0,
            JumlahPesanan = _context.Pesanans
                .Where(x => x.DeletedAt == null && (x.TanggalPengiriman.Date == date.Date || x.TanggalPesan.Date == date.Date))
                .Count()
        }).ToList();

        var butuhRefund = _context.Pesanans
            .Include(p => p.Pengguna)
            .Include(p => p.Pembayaran)
            .Where(x => x.DeletedAt == null && 
                       (x.StatusPesanan == "Menunggu Refund" || 
                        ((x.StatusPesanan == "Dibatalkan" || x.StatusPesanan == "Batal") && 
                         x.Pembayaran != null && x.Pembayaran.StatusVerifikasi == "Valid" && 
                         string.IsNullOrEmpty(x.RefundBuktiTf))))
            .OrderByDescending(x => x.TanggalPesan)
            .ToList();

        return new DashboardViewModel
        {
            TotalPesanan = _context.Pesanans.Count(x => x.DeletedAt == null),
            TotalPendapatan = _context.Pesanans.Where(x => x.DeletedAt == null && x.StatusPesanan == "Selesai").Sum(x => (decimal?)x.TotalBayar) ?? 0,
            PesananPending = _context.Pesanans.Count(x => x.DeletedAt == null && x.StatusPesanan == "Pending"),
            PesananDiproses = _context.Pesanans.Count(x => x.DeletedAt == null && x.StatusPesanan == "Diproses"),
            TotalPelanggan = _context.Penggunas.Count(x => x.PeranId == 3 && x.DeletedAt == null), 
            PesananTerbaru = _context.Pesanans.Include(p => p.Pengguna).Where(x => x.DeletedAt == null).OrderByDescending(x => x.TanggalPesan).Take(5).ToList(),
            JadwalPengiriman = _context.Pesanans.Include(p => p.Pengguna).Where(x => x.DeletedAt == null && (x.StatusPesanan == "Diproses" || x.StatusPesanan == "Dikirim")).OrderBy(x => x.TanggalPengiriman).Take(10).ToList(),
            PesananButuhVerifikasi = _context.Pesanans.Include(p => p.Pengguna).Include(p => p.Pembayaran).Where(x => x.DeletedAt == null && x.Pembayaran != null && x.Pembayaran.StatusVerifikasi == "Menunggu Verifikasi").OrderByDescending(x => x.Pembayaran!.TanggalBayar).ToList(),
            PesananButuhRefund = butuhRefund,
            DailyOmset = dailyOmset
        };
    }

    public CustomerDashboardViewModel GetCustomerDashboardSummary(int penggunaId)
    {
        var customerRefund = _context.Pesanans
            .Include(p => p.Pembayaran)
            .Where(x => x.PenggunaId == penggunaId && x.DeletedAt == null && 
                       (x.StatusPesanan == "Menunggu Refund" || 
                        ((x.StatusPesanan == "Dibatalkan" || x.StatusPesanan == "Batal") && 
                         x.Pembayaran != null && x.Pembayaran.StatusVerifikasi == "Valid" && 
                         string.IsNullOrEmpty(x.RefundBuktiTf))))
            .OrderByDescending(x => x.TanggalPesan)
            .ToList();

        return new CustomerDashboardViewModel
        {
            TotalPesanan = _context.Pesanans.Count(x => x.PenggunaId == penggunaId && x.DeletedAt == null),
            TotalBelanja = _context.Pesanans
                .Where(x => x.PenggunaId == penggunaId && x.DeletedAt == null && x.StatusPesanan == "Selesai")
                .Sum(x => (decimal?)x.TotalBayar) ?? 0,
            PesananAktif = _context.Pesanans
                .Count(x => x.PenggunaId == penggunaId && x.DeletedAt == null && (x.StatusPesanan == "Pending" || x.StatusPesanan == "Diproses" || x.StatusPesanan == "Dikirim")),
            PesananTerbaru = _context.Pesanans
                .Include(p => p.Pembayaran)
                .Where(x => x.PenggunaId == penggunaId && x.DeletedAt == null)
                .OrderByDescending(x => x.TanggalPesan)
                .Take(5)
                .ToList(),
            PesananButuhRefund = customerRefund
        };
    }

    public PesananPagedResult GetPagedPesananList(int? penggunaId, string? search, string? status, int? kategoriId, DateTime? tanggal, string? sort, int page, int size)
    {
        var query = _context.Pesanans
            .Include(p => p.Pengguna)
            .Include(p => p.Pembayaran)
            .Include(p => p.DetailPesanans)
                .ThenInclude(dp => dp.Paket)
            .Where(p => p.DeletedAt == null);

        if (penggunaId.HasValue)
        {
            query = query.Where(p => p.PenggunaId == penggunaId.Value);
        }

        if (!string.IsNullOrEmpty(search))
        {
            string s = search.ToLower();
            query = query.Where(p => p.NomorPesanan.ToLower().Contains(s)
                                  || (p.Pengguna != null && p.Pengguna.NamaLengkap.ToLower().Contains(s)));
        }

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(p => p.StatusPesanan == status);
        }

        if (tanggal.HasValue)
        {
            query = query.Where(p => p.TanggalPesan.Date == tanggal.Value.Date);
        }

        if (kategoriId.HasValue)
        {
            query = query.Where(p => p.DetailPesanans.Any(dp => dp.Paket != null && dp.Paket.KategoriId == kategoriId.Value));
        }

        switch (sort)
        {
            case "terlama":
                query = query.OrderBy(p => p.TanggalPesan).ThenBy(p => p.PesananId);
                break;
            case "pelanggan-az":
            case "A-Z":
                query = query.OrderBy(p => p.Pengguna != null ? p.Pengguna.NamaLengkap : "").ThenByDescending(p => p.TanggalPesan);
                break;
            case "pelanggan-za":
            case "Z-A":
                query = query.OrderByDescending(p => p.Pengguna != null ? p.Pengguna.NamaLengkap : "").ThenByDescending(p => p.TanggalPesan);
                break;
            case "total-terbesar":
            case "termahal":
                query = query.OrderByDescending(p => p.TotalBayar);
                break;
            case "total-terkecil":
            case "termurah":
                query = query.OrderBy(p => p.TotalBayar);
                break;
            case "no-pesanan-asc":
                query = query.OrderBy(p => p.NomorPesanan);
                break;
            case "no-pesanan-desc":
                query = query.OrderByDescending(p => p.NomorPesanan);
                break;
            case "terbaru":
            default:
                query = query.OrderByDescending(p => p.TanggalPesan).ThenByDescending(p => p.PesananId);
                break;
        }

        int totalRecords = query.Count();
        int totalPages = (int)Math.Ceiling((double)totalRecords / size);
        int finalPage = Math.Max(1, Math.Min(page, totalPages == 0 ? 1 : totalPages));

        var items = query
            .Skip((finalPage - 1) * size)
            .Take(size)
            .ToList();

        return new PesananPagedResult
        {
            Items = items,
            TotalRecords = totalRecords,
            TotalPages = totalPages
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

    private static DateTime CombineTanggalDanJam(DateTime tanggal, string? jamSlot)
    {
        int hour = 10;
        int minute = 0;
        if (!string.IsNullOrWhiteSpace(jamSlot))
        {
            var match = System.Text.RegularExpressions.Regex.Match(jamSlot, @"^(\d{1,2})[:.](\d{2})");
            if (match.Success)
            {
                int.TryParse(match.Groups[1].Value, out hour);
                int.TryParse(match.Groups[2].Value, out minute);
            }
        }
        return tanggal.Date.AddHours(hour).AddMinutes(minute);
    }

    public int BuatPesanan(int penggunaId, PemesananViewModel model)
    {
        using var transaction = _context.Database.BeginTransaction();
        try
        {
            var paket = _context.PaketMenus.Find(model.PaketId);
            decimal subtotal = (paket?.Harga ?? 0) * model.JumlahPorsi;

            // Jika dipesan oleh staf untuk pelanggan tertentu
            int targetUserId = (model.PenggunaId.HasValue && model.PenggunaId.Value > 0) ? model.PenggunaId.Value : penggunaId;
            DateTime deliveryDateTime = CombineTanggalDanJam(model.TanggalPengiriman, model.JamPengantaran);

            string fullCatatan = model.Catatan ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(model.NamaPemesanManual))
            {
                string walkInInfo = $"[Pesanan Walk-in / WA: {model.NamaPemesanManual} ({model.TeleponPemesanManual ?? "-"}) - Jam Antar: {model.JamPengantaran}]";
                fullCatatan = string.IsNullOrWhiteSpace(fullCatatan) ? walkInInfo : $"{walkInInfo} {fullCatatan}";
            }

            var pesanan = new Pesanan
            {
                PenggunaId = targetUserId,
                NomorPesanan = "ORD-" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                TanggalPesan = DateTime.Now,
                TanggalPengiriman = deliveryDateTime,
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
                Catatan = fullCatatan,
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

    public int BuatPesananDariKeranjang(int penggunaId, List<CartItem> cartItems, CheckoutViewModel model)
    {
        using var transaction = _context.Database.BeginTransaction();
        try
        {
            decimal subtotal = cartItems.Sum(item => item.Subtotal);
            decimal ongkir = CateringApp.Helpers.OngkirHelper.HitungOngkir(model.AlamatPengiriman);
            decimal totalBayar = subtotal + ongkir;
            DateTime deliveryDateTime = CombineTanggalDanJam(model.TanggalPengiriman, model.JamPengantaran);

            var pesanan = new Pesanan
            {
                PenggunaId = penggunaId,
                NomorPesanan = "ORD-" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                TanggalPesan = DateTime.Now,
                TanggalPengiriman = deliveryDateTime,
                AlamatPengiriman = model.AlamatPengiriman,
                TotalBayar = totalBayar,
                StatusPesanan = "Pending",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            _context.Pesanans.Add(pesanan);
            _context.SaveChanges();

            foreach (var item in cartItems)
            {
                var detail = new DetailPesanan
                {
                    PesananId = pesanan.PesananId,
                    PaketId = item.PaketId,
                    Jumlah = item.Jumlah,
                    HargaSatuan = item.Harga,
                    Subtotal = item.Subtotal,
                    Catatan = item.Catatan,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                _context.DetailPesanans.Add(detail);
            }
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

    public void KonfirmasiPembayaranOtomatis(int pesananId, string metodePembayaran)
    {
        var pesanan = _context.Pesanans.Find(pesananId);
        if (pesanan == null) return;

        var pembayaran = _context.Pembayarans.FirstOrDefault(p => p.PesananId == pesananId);
        if (pembayaran == null)
        {
            pembayaran = new Pembayaran
            {
                PesananId = pesananId,
                MetodePembayaran = string.IsNullOrWhiteSpace(metodePembayaran) ? "BCA Virtual Account" : metodePembayaran,
                JumlahBayar = pesanan.TotalBayar,
                TanggalBayar = DateTime.Now,
                BuktiTransfer = "/images/menu_box.svg",
                StatusVerifikasi = "Valid",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            _context.Pembayarans.Add(pembayaran);
        }
        else
        {
            pembayaran.MetodePembayaran = string.IsNullOrWhiteSpace(metodePembayaran) ? pembayaran.MetodePembayaran : metodePembayaran;
            pembayaran.StatusVerifikasi = "Valid";
            pembayaran.TanggalBayar = DateTime.Now;
            pembayaran.UpdatedAt = DateTime.Now;
        }

        pesanan.StatusPesanan = "Diproses";
        pesanan.UpdatedAt = DateTime.Now;
        _context.SaveChanges();
    }

    public (bool IsAllowed, string Reason) CekKelayakanRefund(int pesananId)
    {
        var pesanan = _context.Pesanans.Find(pesananId);
        if (pesanan == null) return (false, "Pesanan tidak ditemukan.");

        if (pesanan.StatusPesanan == "Dibatalkan" || pesanan.StatusPesanan == "Batal")
            return (false, "Pesanan sudah dibatalkan sebelumnya.");

        if (pesanan.StatusPesanan == "Selesai")
            return (false, "Pesanan sudah selesai dan telah diterima pelanggan.");

        if (pesanan.StatusPesanan == "Dikirim")
            return (false, "Pesanan sedang dalam proses pengiriman kurir ke lokasi acara dan tidak dapat dibatalkan.");

        if (pesanan.StatusPesanan == "Menunggu Refund" || pesanan.StatusPesanan == "Refund Selesai")
            return (false, "Pesanan sudah dalam proses pengembalian dana (Refund).");

        // Aturan H-2 Cutoff
        var selisihHari = (pesanan.TanggalPengiriman.Date - DateTime.Today).TotalDays;
        if (selisihHari < 2)
        {
            return (false, $"Batas waktu pembatalan mandiri telah ditutup. Pesanan untuk acara tanggal {pesanan.TanggalPengiriman:dd MMMM yyyy} telah memasuki tahap persiapan bahan baku dan operasional dapur (H-2 Cutoff).");
        }

        return (true, "Pengajuan refund diperbolehkan.");
    }

    public void AjukanRefund(int pesananId, string bank, string noRekening, string atasNama)
    {
        var (isAllowed, reason) = CekKelayakanRefund(pesananId);
        if (!isAllowed)
        {
            throw new InvalidOperationException(reason);
        }

        var pesanan = _context.Pesanans.Find(pesananId);
        if (pesanan != null)
        {
            pesanan.StatusPesanan = "Menunggu Refund";
            pesanan.RefundNamaBank = bank;
            pesanan.RefundNoRekening = noRekening;
            pesanan.RefundAtasNama = atasNama;
            pesanan.UpdatedAt = DateTime.Now;
            _context.SaveChanges();
        }
    }

    public void KonfirmasiRefund(int pesananId, string buktiTfPath)
    {
        var pesanan = _context.Pesanans.Find(pesananId);
        if (pesanan != null)
        {
            pesanan.StatusPesanan = "Refund Selesai";
            pesanan.RefundBuktiTf = buktiTfPath;
            pesanan.UpdatedAt = DateTime.Now;
            _context.SaveChanges();
        }
    }

    public int GetTotalPorsiByTanggal(DateTime tanggal)
    {
        var targetDate = tanggal.Date;
        var total = _context.Pesanans
            .Where(p => p.DeletedAt == null && 
                        p.TanggalPengiriman.Date == targetDate && 
                        p.StatusPesanan != "Dibatalkan" && 
                        p.StatusPesanan != "Batal" &&
                        p.StatusPesanan != "Refund Selesai")
            .SelectMany(p => p.DetailPesanans)
            .Sum(d => (int?)d.Jumlah) ?? 0;

        return total;
    }

    public ManifesDapurViewModel GetManifesDapur(DateTime tanggal)
    {
        var targetDate = tanggal.Date;
        var pesananList = _context.Pesanans
            .Include(p => p.Pengguna)
            .Include(p => p.DetailPesanans)
                .ThenInclude(d => d.Paket)
                    .ThenInclude(pkg => pkg!.Kategori)
            .Where(p => p.DeletedAt == null &&
                        p.TanggalPengiriman.Date == targetDate &&
                        p.StatusPesanan != "Dibatalkan" &&
                        p.StatusPesanan != "Batal" &&
                        p.StatusPesanan != "Refund Selesai")
            .OrderBy(p => p.TanggalPengiriman)
            .ToList();

        var manifes = new ManifesDapurViewModel
        {
            Tanggal = targetDate,
            TotalPesanan = pesananList.Count,
            TotalPorsiKolektif = pesananList.SelectMany(p => p.DetailPesanans).Sum(d => d.Jumlah),
            KapasitasMaksimalDapur = 400
        };

        var details = pesananList.SelectMany(p => p.DetailPesanans).ToList();
        var grouped = details
            .GroupBy(d => d.PaketId)
            .Select(g =>
            {
                var first = g.First();
                var paket = first.Paket;
                var catName = paket?.Kategori?.NamaKategori ?? "Menu Catering";
                var photo = CateringApp.Helpers.MenuImageHelper.GetGambarUrl(paket?.NamaPaket, paket?.Gambar, catName);
                var notes = g.Where(x => !string.IsNullOrWhiteSpace(x.Catatan))
                             .Select(x => x.Catatan!.Trim())
                             .Distinct()
                             .ToList();

                return new ManifesItemViewModel
                {
                    PaketId = g.Key,
                    NamaPaket = paket?.NamaPaket ?? "Paket Menu",
                    Kategori = catName,
                    Gambar = photo,
                    TotalPorsi = g.Sum(x => x.Jumlah),
                    JumlahPesanan = g.Select(x => x.PesananId).Distinct().Count(),
                    CatatanPesanan = notes
                };
            })
            .OrderByDescending(x => x.TotalPorsi)
            .ToList();

        manifes.Items = grouped;

        manifes.DaftarPesananHariIni = pesananList.Select(p => new ManifesPesananRingkasViewModel
        {
            PesananId = p.PesananId,
            NomorPesanan = p.NomorPesanan,
            NamaPelanggan = p.Pengguna?.NamaLengkap ?? "Pelanggan Offline",
            Telepon = p.Pengguna?.NomorTelepon ?? "-",
            JamPengantaran = p.TanggalPengiriman.ToString("HH:mm") + " WIB",
            TotalPorsi = p.DetailPesanans.Sum(d => d.Jumlah),
            AlamatPengiriman = p.AlamatPengiriman,
            StatusPesanan = p.StatusPesanan,
            Catatan = string.Join("; ", p.DetailPesanans.Where(d => !string.IsNullOrWhiteSpace(d.Catatan)).Select(d => d.Catatan))
        }).ToList();

        return manifes;
    }

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

    public void InputRekeningRefund(int pesananId, string bank, string noRekening, string atasNama)
    {
        var pesanan = _context.Pesanans.Find(pesananId);
        if (pesanan != null)
        {
            pesanan.StatusPesanan = "Menunggu Refund";
            pesanan.RefundNamaBank = bank.Trim();
            pesanan.RefundNoRekening = noRekening.Trim();
            pesanan.RefundAtasNama = atasNama.Trim();
            pesanan.UpdatedAt = DateTime.Now;
            _context.SaveChanges();
        }
    }

    public List<NotifikasiItemViewModel> GetNotifikasiList(string role, int? userId)
    {
        var list = new List<NotifikasiItemViewModel>();

        if (role == "Pemilik Toko" || role == "Owner")
        {
            // 1. Pesanan Menunggu Refund (Uang harus ditransfer balik!)
            var refundOrders = _context.Pesanans
                .Include(p => p.Pengguna)
                .Include(p => p.Pembayaran)
                .Where(x => x.DeletedAt == null && 
                           (x.StatusPesanan == "Menunggu Refund" || 
                            ((x.StatusPesanan == "Dibatalkan" || x.StatusPesanan == "Batal") && 
                             x.Pembayaran != null && x.Pembayaran.StatusVerifikasi == "Valid" && 
                             string.IsNullOrEmpty(x.RefundBuktiTf))))
                .OrderByDescending(x => x.TanggalPesan)
                .Take(5)
                .ToList();

            foreach (var r in refundOrders)
            {
                list.Add(new NotifikasiItemViewModel
                {
                    Judul = "Perlu Ditransfer: Refund Dana!",
                    Pesan = $"Pesanan {r.NomorPesanan} ({r.Pengguna?.NamaLengkap ?? "Pelanggan"}) dibatalkan & butuh transfer refund Rp {r.TotalBayar:N0}.",
                    Url = $"/Pesanan/Details/{r.PesananId}",
                    Waktu = r.UpdatedAt?.ToString("dd MMM HH:mm") ?? r.TanggalPesan.ToString("dd MMM"),
                    Tipe = "danger",
                    Icon = "fa-hand-holding-usd"
                });
            }

            // 2. Pesanan Pending Baru Masuk
            var pendingCount = _context.Pesanans.Count(x => x.DeletedAt == null && x.StatusPesanan == "Pending");
            if (pendingCount > 0)
            {
                list.Add(new NotifikasiItemViewModel
                {
                    Judul = $"{pendingCount} Pesanan Baru Masuk",
                    Pesan = "Ada pesanan katering masuk yang menunggu proses transaksi.",
                    Url = "/Pesanan?status=Pending",
                    Waktu = "Saat Ini",
                    Tipe = "warning",
                    Icon = "fa-clock"
                });
            }
        }
        else if (role == "Karyawan")
        {
            // 1. Pembayaran Butuh Verifikasi
            var unverified = _context.Pesanans
                .Include(p => p.Pengguna)
                .Include(p => p.Pembayaran)
                .Where(x => x.DeletedAt == null && x.Pembayaran != null && x.Pembayaran.StatusVerifikasi == "Menunggu Verifikasi")
                .OrderByDescending(x => x.Pembayaran!.TanggalBayar)
                .Take(5)
                .ToList();

            foreach (var u in unverified)
            {
                list.Add(new NotifikasiItemViewModel
                {
                    Judul = "Verifikasi Bukti Transfer!",
                    Pesan = $"Pelanggan {u.Pengguna?.NamaLengkap ?? "Pelanggan"} mengunggah bukti bayar untuk {u.NomorPesanan}.",
                    Url = $"/Pesanan/Details/{u.PesananId}",
                    Waktu = u.Pembayaran != null ? u.Pembayaran.TanggalBayar.ToString("dd MMM HH:mm") : "Hari ini",
                    Tipe = "warning",
                    Icon = "fa-receipt"
                });
            }


            // 3. Jadwal Masak Hari Ini / Besok
            var today = DateTime.Today;
            var masakCount = _context.Pesanans.Count(x => x.DeletedAt == null && x.StatusPesanan == "Diproses" && x.TanggalPengiriman.Date <= today.AddDays(1));
            if (masakCount > 0)
            {
                list.Add(new NotifikasiItemViewModel
                {
                    Judul = $"{masakCount} Antrean Masak Dapur",
                    Pesan = "Pesanan perlu dipersiapkan di dapur untuk jadwal kirim segera.",
                    Url = "/Pesanan/ManifesDapur",
                    Waktu = "Hari ini / Besok",
                    Tipe = "info",
                    Icon = "fa-fire-burner"
                });
            }
        }
        else if (role == "User" && userId.HasValue)
        {
            // 1. Pesanan Dibatalkan Berbayar (Mohon isi rekening)
            var needRek = _context.Pesanans
                .Include(p => p.Pembayaran)
                .Where(x => x.PenggunaId == userId.Value && x.DeletedAt == null && 
                            (x.StatusPesanan == "Dibatalkan" || x.StatusPesanan == "Menunggu Refund") && 
                            x.Pembayaran != null && x.Pembayaran.StatusVerifikasi == "Valid" && 
                            string.IsNullOrEmpty(x.RefundNamaBank))
                .ToList();

            foreach (var n in needRek)
            {
                list.Add(new NotifikasiItemViewModel
                {
                    Judul = "Pesanan Dibatalkan: Masukkan Rekening!",
                    Pesan = $"Pesanan {n.NomorPesanan} dibatalkan. Masukkan rekening Anda agar Owner mentransfer balik dana Rp {n.TotalBayar:N0}.",
                    Url = $"/Pesanan/Details/{n.PesananId}",
                    Waktu = n.UpdatedAt?.ToString("dd MMM HH:mm") ?? "Hari ini",
                    Tipe = "danger",
                    Icon = "fa-hand-holding-usd"
                });
            }

            // 2. Refund Selesai
            var refundDone = _context.Pesanans
                .Where(x => x.PenggunaId == userId.Value && x.DeletedAt == null && x.StatusPesanan == "Refund Selesai")
                .OrderByDescending(x => x.UpdatedAt)
                .Take(2)
                .ToList();

            foreach (var rd in refundDone)
            {
                list.Add(new NotifikasiItemViewModel
                {
                    Judul = "Dana Berhasil Dikembalikan!",
                    Pesan = $"Dana Rp {rd.TotalBayar:N0} untuk {rd.NomorPesanan} telah berhasil ditransfer balik oleh Owner. Klik untuk cek bukti transfer.",
                    Url = $"/Pesanan/Details/{rd.PesananId}",
                    Waktu = rd.UpdatedAt?.ToString("dd MMM HH:mm") ?? "Baru saja",
                    Tipe = "success",
                    Icon = "fa-check-circle"
                });
            }

            // 3. Pesanan Sedang Dikirim
            var shipping = _context.Pesanans
                .Where(x => x.PenggunaId == userId.Value && x.DeletedAt == null && x.StatusPesanan == "Dikirim")
                .Take(3)
                .ToList();

            foreach (var s in shipping)
            {
                list.Add(new NotifikasiItemViewModel
                {
                    Judul = "Pesanan Sedang Dikirim!",
                    Pesan = $"Kurir sedang mengantarkan pesanan {s.NomorPesanan} ke alamat Anda.",
                    Url = $"/Pesanan/Details/{s.PesananId}",
                    Waktu = s.TanggalPengiriman.ToString("dd MMM HH:mm"),
                    Tipe = "info",
                    Icon = "fa-truck"
                });
            }
        }

        return list;
    }

    public List<Peran> GetAllPeran() =>
        _context.Perans.ToList();
}
