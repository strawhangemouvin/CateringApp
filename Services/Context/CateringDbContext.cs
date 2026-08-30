using CateringApp.Models.Entity;
using Microsoft.EntityFrameworkCore;

namespace CateringApp.Services.Context;

public class CateringDbContext : DbContext
{
    public CateringDbContext(DbContextOptions<CateringDbContext> options) : base(options) { }

    public DbSet<Peran> Perans => Set<Peran>();
    public DbSet<Pengguna> Penggunas => Set<Pengguna>();
    public DbSet<KategoriMenu> KategoriMenus => Set<KategoriMenu>();
    public DbSet<PaketMenu> PaketMenus => Set<PaketMenu>();
    public DbSet<Pesanan> Pesanans => Set<Pesanan>();
    public DbSet<DetailPesanan> DetailPesanans => Set<DetailPesanan>();
    public DbSet<Pembayaran> Pembayarans => Set<Pembayaran>();
}