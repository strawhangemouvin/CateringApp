using CateringApp.Models.Entity;
using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;

namespace CateringApp.Services.Context
{
    public static class DbInitializer
    {
        public static void Initialize(CateringDbContext context)
        {
            context.Database.EnsureCreated();

            // 1. Seed Peran
            if (!context.Perans.Any())
            {
                context.Perans.AddRange(
                    new Peran { NamaPeran = "Pemilik Toko" }, // ID 1
                    new Peran { NamaPeran = "Karyawan" },     // ID 2
                    new Peran { NamaPeran = "User" }          // ID 3
                );
                context.SaveChanges();
            }

            // Get role IDs
            var ownerRole = context.Perans.FirstOrDefault(p => p.NamaPeran == "Pemilik Toko");
            var employeeRole = context.Perans.FirstOrDefault(p => p.NamaPeran == "Karyawan");
            var userRole = context.Perans.FirstOrDefault(p => p.NamaPeran == "User");

            int ownerRoleId = ownerRole?.PeranId ?? 1;
            int employeeRoleId = employeeRole?.PeranId ?? 2;
            int userRoleId = userRole?.PeranId ?? 3;

            // 2. Seed Pengguna (min 20 data)
            if (!context.Penggunas.Any())
            {
                var hasher = new PasswordHasher<Pengguna>();

                // 1 Pemilik Toko
                var owner = new Pengguna
                {
                    PeranId = ownerRoleId,
                    NamaLengkap = "Bapak Bos (Owner)",
                    Username = "owner",
                    Email = "owner@catering.com",
                    NomorTelepon = "08111111111",
                    Alamat = "Kantor Direksi",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                owner.PasswordHash = hasher.HashPassword(owner, "owner123");
                context.Penggunas.Add(owner);

                // 1 Karyawan
                var employee = new Pengguna
                {
                    PeranId = employeeRoleId,
                    NamaLengkap = "Staf Dapur & Kasir",
                    Username = "karyawan",
                    Email = "karyawan@catering.com",
                    NomorTelepon = "08222222222",
                    Alamat = "Operasional Toko",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                employee.PasswordHash = hasher.HashPassword(employee, "karyawan123");
                context.Penggunas.Add(employee);

                // 1 Budi Pratama (User)
                var budi = new Pengguna
                {
                    PeranId = userRoleId,
                    NamaLengkap = "Budi Pratama",
                    Username = "budi",
                    Email = "budi@gmail.com",
                    NomorTelepon = "08120000001",
                    Alamat = "Jl. Sukajadi No. 12",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                budi.PasswordHash = hasher.HashPassword(budi, "user123");
                context.Penggunas.Add(budi);

                // 17 Pelanggan Tambahan (untuk memenuhi minimal 20 pengguna)
                for (int i = 1; i <= 17; i++)
                {
                    var userSeed = new Pengguna
                    {
                        PeranId = userRoleId,
                        NamaLengkap = $"Pelanggan Contoh {i}",
                        Username = $"user{i}",
                        Email = $"user{i}@mail.com",
                        NomorTelepon = $"08571234567{i:00}",
                        Alamat = $"Jl. Mawar Merah Block C No. {i}",
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    userSeed.PasswordHash = hasher.HashPassword(userSeed, "password");
                    context.Penggunas.Add(userSeed);
                }
                context.SaveChanges();
            }
            else
            {
                // Migrate any existing plain text passwords (length < 30)
                var plainTextUsers = context.Penggunas.Where(u => u.PasswordHash.Length < 30).ToList();
                if (plainTextUsers.Any())
                {
                    var hasher = new PasswordHasher<Pengguna>();
                    foreach (var user in plainTextUsers)
                    {
                        user.PasswordHash = hasher.HashPassword(user, user.PasswordHash);
                    }
                    context.SaveChanges();
                }
            }

            // 3. Seed KategoriMenu (min 20 data)
            if (!context.KategoriMenus.Any())
            {
                string[] categories = {
                    "Nasi Kotak", "Prasmanan", "Snack Box", "Tumpeng", "Nasi Kebuli",
                    "Catering Harian", "Catering Diet", "Coffee Break", "Gubukan", "Menu Sehat",
                    "Minuman Segar", "Kue Tradisional", "Puding & Dessert", "Roti & Cake", "Sate & Bakaran",
                    "Soto & Sop", "Bakso & Mie", "Menu Vegetarian", "Paket Aqiqah", "Paket Pernikahan"
                };

                foreach (var cat in categories)
                {
                    context.KategoriMenus.Add(new KategoriMenu
                    {
                        NamaKategori = cat,
                        Deskripsi = $"Kategori menu pilihan untuk {cat}",
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    });
                }
                context.SaveChanges();
            }

            // 4. Seed PaketMenu (min 20 data)
            if (!context.PaketMenus.Any())
            {
                var categories = context.KategoriMenus.ToList();
                for (int i = 0; i < 20; i++)
                {
                    var cat = categories[i % categories.Count];
                    string imagePath = cat.NamaKategori.ToLower() switch
                    {
                        var n when n.Contains("kotak") => "/images/menu_box.svg",
                        var n when n.Contains("tumpeng") => "/images/menu_tumpeng.svg",
                        var n when n.Contains("prasmanan") => "/images/menu_buffet.svg",
                        _ => "/images/menu_snack.svg"
                    };

                    context.PaketMenus.Add(new PaketMenu
                    {
                        KategoriId = cat.KategoriId,
                        NamaPaket = $"Paket {cat.NamaKategori} Pilihan {i + 1}",
                        Harga = 25000 + (i * 5000),
                        DeskripsiMenu = $"Nasi, Lauk utama {i + 1}, Sayur pelengkap, Sambal, dan Kerupuk.",
                        Gambar = imagePath,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    });
                }
                context.SaveChanges();
            }

            // 5. Seed Pesanan, DetailPesanan, Pembayaran (min 20 data)
            if (!context.Pesanans.Any())
            {
                var users = context.Penggunas.Where(u => u.PeranId == userRoleId).ToList();
                var pakets = context.PaketMenus.ToList();

                for (int i = 1; i <= 20; i++)
                {
                    var customer = users[i % users.Count];
                    var paket = pakets[i % pakets.Count];
                    int qty = 10 + i;
                    decimal total = paket.Harga * qty;

                    var status = i % 3 == 0 ? "Selesai" : (i % 3 == 1 ? "Diproses" : "Pending");
                    var tanggalPesan = DateTime.Now.AddDays(-i);
                    var tanggalKirim = DateTime.Now.AddDays(-i + 2); // default delivery

                    var pesanan = new Pesanan
                    {
                        PenggunaId = customer.PenggunaId,
                        NomorPesanan = $"ORD-{tanggalPesan:yyyyMMdd}-{i:00}",
                        TanggalPesan = tanggalPesan,
                        TanggalPengiriman = tanggalKirim,
                        AlamatPengiriman = customer.Alamat ?? "Alamat Default",
                        TotalBayar = total,
                        StatusPesanan = status,
                        CreatedAt = tanggalPesan,
                        UpdatedAt = tanggalPesan
                    };
                    context.Pesanans.Add(pesanan);
                    context.SaveChanges(); // Save to generate PesananId

                    var detail = new DetailPesanan
                    {
                        PesananId = pesanan.PesananId,
                        PaketId = paket.PaketId,
                        Jumlah = qty,
                        HargaSatuan = paket.Harga,
                        Subtotal = total,
                        Catatan = "Pedas sedang, sayur dipisah.",
                        CreatedAt = tanggalPesan,
                        UpdatedAt = tanggalPesan
                    };
                    context.DetailPesanans.Add(detail);

                    // payments for all orders except the first 2 (leave those "Belum Bayar")
                    if (i > 2)
                    {
                        var pembayaran = new Pembayaran
                        {
                            PesananId = pesanan.PesananId,
                            MetodePembayaran = i % 2 == 0 ? "Transfer Bank Mandiri" : "GoPay",
                            JumlahBayar = total,
                            TanggalBayar = tanggalPesan,
                            BuktiTransfer = $"/uploads/PAY_{pesanan.PesananId}_proof.jpg",
                            StatusVerifikasi = i % 3 == 0 ? "Valid" : (i % 3 == 1 ? "Menunggu Verifikasi" : "Tidak Valid"),
                            CreatedAt = tanggalPesan,
                            UpdatedAt = tanggalPesan
                        };
                        context.Pembayarans.Add(pembayaran);
                    }
                }
                context.SaveChanges();
            }
        }
    }
}
