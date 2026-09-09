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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Pesanan>()
            .HasOne(p => p.Pembayaran)
            .WithOne(b => b.Pesanan)
            .HasForeignKey<Pembayaran>(b => b.PesananId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Pengguna>()
            .HasOne(u => u.Peran)
            .WithMany()
            .HasForeignKey(u => u.PeranId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PaketMenu>()
            .HasOne(m => m.Kategori)
            .WithMany()
            .HasForeignKey(m => m.KategoriId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Pesanan>()
            .HasOne(p => p.Pengguna)
            .WithMany()
            .HasForeignKey(p => p.PenggunaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DetailPesanan>()
            .HasOne(d => d.Pesanan)
            .WithMany(p => p.DetailPesanans)
            .HasForeignKey(d => d.PesananId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DetailPesanan>()
            .HasOne(d => d.Paket)
            .WithMany()
            .HasForeignKey(d => d.PaketId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
