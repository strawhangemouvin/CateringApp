using CateringApp.Models.Entity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CateringApp.Services.Context
{
    public static class DbInitializer
    {
        public static void Initialize(CateringDbContext context)
        {
            context.Database.EnsureCreated();

            // 1. Roles / Peran
            if (!context.Perans.Any())
            {
                context.Perans.AddRange(
                    new Peran { NamaPeran = "Pemilik Toko" },
                    new Peran { NamaPeran = "Karyawan" },
                    new Peran { NamaPeran = "User" }
                );
                context.SaveChanges();
            }

            var ownerRole = context.Perans.FirstOrDefault(p => p.NamaPeran == "Pemilik Toko");
            var employeeRole = context.Perans.FirstOrDefault(p => p.NamaPeran == "Karyawan");
            var userRole = context.Perans.FirstOrDefault(p => p.NamaPeran == "User");

            int ownerRoleId = ownerRole?.PeranId ?? 1;
            int employeeRoleId = employeeRole?.PeranId ?? 2;
            int userRoleId = userRole?.PeranId ?? 3;

            var hasher = new PasswordHasher<Pengguna>();

            // 2. Kategori Menu Otentik (Catering Mimi Saripah)
            var defaultCategories = new List<(string Nama, string Deskripsi)>
            {
                ("Nasi Kotak", "Menu paket nasi kotak lezat, higienis, dan praktis dengan aneka lauk tradisional khas Nusantara untuk berbagai acara."),
                ("Prasmanan & Hajatan", "Layanan prasmanan lengkap dengan pemanas makanan dan staf penyaji profesional untuk pernikahan, khitanan, dan resepsi."),
                ("Nasi Tumpeng", "Sajian tumpeng kuning & putih kerucut atau susun bertingkat dengan hiasan sayuran estetik untuk syukuran dan peresmian."),
                ("Snack Box Tradisional", "Kombinasi kue basah manis dan gurih pilihan berkualitas untuk rapat kantor, seminar, arisan, dan coffee break."),
                ("Nasi Kebuli & Liwet", "Sajian nasi beraroma rempah khas Timur Tengah dan nasi liwet wangi daun salam dengan lauk pauk komplit."),
                ("Catering Harian Keluarga", "Paket rantangan menu rumahan sehat bervariasi setiap hari tanpa pengawet untuk keluarga dan karyawan perkantoran."),
                ("Coffee Break & Pastry", "Aneka pastry mini, roti, kue modern, dan minuman teh/kopi premium untuk pertemuan bisnis penting."),
                ("Gubukan Pesta", "Pondokan sajian favorit pesta seperti sate ayam madura, bakso malang, soto betawi, dan siomay bandung."),
                ("Aneka Minuman Segar", "Minuman tradisional dan modern segar seperti es campur buah, es teler, es dawet ayu, dan aneka jus alami."),
                ("Paket Aqiqah Syukuran", "Olahan daging kambing aqiqah empuk bebas bau prengus dalam bentuk sate dan gulai siap saji sesuai syariat.")
            };

            if (!context.KategoriMenus.Any())
            {
                foreach (var cat in defaultCategories)
                {
                    context.KategoriMenus.Add(new KategoriMenu
                    {
                        NamaKategori = cat.Nama,
                        Deskripsi = cat.Deskripsi,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    });
                }
                context.SaveChanges();
            }

            var dbCategories = context.KategoriMenus.ToList();

            // 3. Paket Menu Otentik (Non-Dummy)
            var defaultPackages = new List<(string Nama, decimal Harga, string Deskripsi, string KategoriNama, string Gambar)>
            {
                (
                    "Nasi Kotak Ayam Bakar Spesial",
                    28000,
                    "Nasi putih pulen, ayam bakar kecap bumbu rempah khas Mimi Saripah, tahu & tempe bacem, lalapan segar, sambal terasi matang, dan kerupuk udang.",
                    "Nasi Kotak",
                    "/images/menu_box.svg"
                ),
                (
                    "Nasi Kotak Ayam Goreng Lengkuas",
                    25000,
                    "Nasi putih hangat, ayam goreng tabur serundeng lengkuas renyah gurih, orek tempe manis, sambal bajak, lalapan timun selada, dan kerupuk.",
                    "Nasi Kotak",
                    "/images/menu_box.svg"
                ),
                (
                    "Nasi Kotak Rendang Daging Sapi",
                    35000,
                    "Nasi putih pulen, rendang daging sapi empuk bumbu rempah Minang otentik, tumis buncis jagung manis, sambal ijo, dan kerupuk udang.",
                    "Nasi Kotak",
                    "/images/menu_box.svg"
                ),
                (
                    "Nasi Kotak Ikan Nila Bakar Madu",
                    30000,
                    "Nasi putih pulen, ikan nila bakar lumur bumbu madu gurih manis, tumis kangkung terasi, sambal kecap pedas segar, dan kerupuk aci.",
                    "Nasi Kotak",
                    "/images/menu_box.svg"
                ),
                (
                    "Paket Prasmanan Hajatan Berkah",
                    65000,
                    "Nasi putih pulen, sop kimlo jamur bakso, ayam suwir rica-rica kemangi, rolade daging sapi saus tiram, capcay seafood, kerupuk udang, puding buah, dan es kelapa muda.",
                    "Prasmanan & Hajatan",
                    "/images/menu_buffet.svg"
                ),
                (
                    "Paket Prasmanan Pernikahan Istimewa",
                    85000,
                    "Nasi putih & nasi minyak pandan, soto daging betawi kuah susu, dendeng balado basah, ayam panggang saus mentega, sambal goreng ati krecek, asinan buah, es teler, dan puding lumut.",
                    "Prasmanan & Hajatan",
                    "/images/menu_buffet.svg"
                ),
                (
                    "Nasi Tumpeng Kuning Komplit (10-15 Porsi)",
                    350000,
                    "Tumpeng nasi kuning harum gurih kerucut, ayam bakar suwir balado, perkedel kentang spesial, sambal goreng ati, telur dadar iris, abon sapi serundeng, urap sayur bumbu rempah, hiasan sayur bunga.",
                    "Nasi Tumpeng",
                    "/images/menu_tumpeng.svg"
                ),
                (
                    "Nasi Tumpeng Mini Selamatan",
                    35000,
                    "Nasi kuning porsi personal dalam mika dome eksklusif, ayam suwir serundeng, perkedel kentang, telur balado iris, mie goreng jawa, orek tempe, timun lalap, dan sambal bajak.",
                    "Nasi Tumpeng",
                    "/images/menu_tumpeng.svg"
                ),
                (
                    "Snack Box Rapat Kantor Premium",
                    18000,
                    "Lemper bakar ayam suwir pulen wangi daun, risoles mayo smoked beef keju renyah, bolu gulung red velvet lembut, dan air mineral botol 330ml.",
                    "Snack Box Tradisional",
                    "/images/menu_snack.svg"
                ),
                (
                    "Snack Box Tradisional Manis Gurih",
                    15000,
                    "Pastel renyah isi ragout sayur telur, kue lapis legit pelangi lembut, tahu bakso sapi semarang, cabe rawit hijau, dan air mineral kemasan.",
                    "Snack Box Tradisional",
                    "/images/menu_snack.svg"
                ),
                (
                    "Nasi Kebuli Kambing Rempah Arab",
                    45000,
                    "Nasi basmati aromatik racikan minyak samin dan rempah kapulaga, daging kambing muda oven empuk tidak bau prengus, acar nanas wortel segar, emping melinjo, dan sambal tomat.",
                    "Nasi Kebuli & Liwet",
                    "/images/menu_box.svg"
                ),
                (
                    "Nasi Liwet Bakul Komplit Solo",
                    32000,
                    "Nasi liwet gurih santan daun salam, ayam suwir ungkep opor gurih, sayur labu siam kuah santan pedas gurih, areh santan kental, dan telur pindang cokelat lezat.",
                    "Nasi Kebuli & Liwet",
                    "/images/menu_box.svg"
                ),
                (
                    "Paket Catering Harian Keluarga (3-4 Orang)",
                    85000,
                    "Paket rantangan higienis harian: 1 lauk utama (Ikan nila pesmol/Ayam rica), 1 sayur berkuah (Sayur asem jakarta), 1 pendamping (Bakwan jagung manis renyah), sambal terasi dan lalap segar.",
                    "Catering Harian Keluarga",
                    "/images/menu_buffet.svg"
                ),
                (
                    "Paket Menu Sehat Rendah Kalori (Diet Box)",
                    40000,
                    "Nasi merah organik pulen, dada ayam panggang rosemary garlic olive oil, tumis brokoli wortel jamur kancing, salad selada tomat cherry saus wijen sangrai, dan potongan buah segar.",
                    "Catering Harian Keluarga",
                    "/images/menu_box.svg"
                ),
                (
                    "Coffee Break & Pastry Set",
                    25000,
                    "Croissant mini butter renyah, eclair vanila saus cokelat leleh, quiche lorraine smoked beef jamur, disajikan dengan racikan kopi tubruk robusta & teh melati hangat.",
                    "Coffee Break & Pastry",
                    "/images/menu_snack.svg"
                ),
                (
                    "Gubukan Sate Ayam Madura (50 Porsi)",
                    450000,
                    "250 tusuk sate ayam fillet empuk tanpa lemak, bumbu kacang gurih kental khas Madura, lontong bungkus daun pulen, kecap manis cabai rawit merah, dan taburan bawang merah goreng renyah.",
                    "Gubukan Pesta",
                    "/images/menu_buffet.svg"
                ),
                (
                    "Gubukan Bakso Sapi Malang (Per Porsi)",
                    20000,
                    "Bakso sapi halus kenyal, bakso urat pedas, siomay kukus basah, pangsit goreng segitiga renyah, tahu bakso, bihun kuning, dan kuah kaldu sapi sumsum asli bening harum gurih.",
                    "Gubukan Pesta",
                    "/images/menu_buffet.svg"
                ),
                (
                    "Gubukan Soto Betawi Daging Sapi (Per Porsi)",
                    28000,
                    "Soto kuah santan kaldu susu rempah khas Betawi, potongan daging sapi sengkel empuk, kentang goreng dadu, tomat merah segar, emping melinjo, jeruk limau, dan sambal rawit merah.",
                    "Gubukan Pesta",
                    "/images/menu_buffet.svg"
                ),
                (
                    "Es Campur Buah Segar Nusantara",
                    15000,
                    "Campuran kelapa muda, alpukat mentega, kolang-kaling merah manis, nangka harum, cincau hitam kenyal, serutan es batu, sirup pandan merah asli, dan susu kental manis gurih legit.",
                    "Aneka Minuman Segar",
                    "/images/menu_snack.svg"
                ),
                (
                    "Paket Aqiqah Masak Komplit (1 Ekor Kambing)",
                    1600000,
                    "1 ekor kambing jantan sehat sesuai syariat, diolah higienis menjadi 50 porsi gulai kambing kuah kental tidak prengus dan 250 tusuk sate kambing bakar bumbu kecap kacang siap saji.",
                    "Paket Aqiqah Syukuran",
                    "/images/menu_buffet.svg"
                )
            };

            // Migrasi paket dummy jika sudah ada
            var existingPackages = context.PaketMenus.ToList();
            if (existingPackages.Any())
            {
                // Update item yang namanya dummy (seperti "Paket Prasmanan Pilihan 2")
                for (int i = 0; i < existingPackages.Count && i < defaultPackages.Count; i++)
                {
                    var p = existingPackages[i];
                    if ((p.NamaPaket != null && p.NamaPaket.Contains("Pilihan")) || (p.DeskripsiMenu != null && p.DeskripsiMenu.Contains("Lauk utama")))
                    {
                        var target = defaultPackages[i];
                        var targetCat = dbCategories.FirstOrDefault(c => c.NamaKategori.Contains(target.KategoriNama.Split(' ')[0])) 
                                       ?? dbCategories.FirstOrDefault();

                        p.NamaPaket = target.Nama;
                        p.Harga = target.Harga;
                        p.DeskripsiMenu = target.Deskripsi;
                        p.Gambar = target.Gambar;
                        if (targetCat != null) p.KategoriId = targetCat.KategoriId;
                        p.UpdatedAt = DateTime.Now;
                    }
                }

                // Perbarui gambar menu yang masih berupa .svg, kosong, wikimedia (terkena 403), atau foto yang tidak sesuai
                foreach (var p in existingPackages)
                {
                    if (string.IsNullOrEmpty(p.Gambar) || 
                        p.Gambar.EndsWith(".svg", StringComparison.OrdinalIgnoreCase) || 
                        p.Gambar.Contains("wikimedia.org", StringComparison.OrdinalIgnoreCase) || 
                        (p.NamaPaket != null && p.NamaPaket.Contains("Tumpeng") && !p.Gambar.Contains("Tumpeng")))
                    {
                        p.Gambar = CateringApp.Helpers.MenuImageHelper.GetGambarUrl(p.NamaPaket, null, p.Kategori?.NamaKategori);
                    }
                }
                context.SaveChanges();
            }
            else
            {
                foreach (var item in defaultPackages)
                {
                    var cat = dbCategories.FirstOrDefault(c => c.NamaKategori.Contains(item.KategoriNama.Split(' ')[0]))
                              ?? dbCategories.FirstOrDefault();

                    context.PaketMenus.Add(new PaketMenu
                    {
                        KategoriId = cat?.KategoriId ?? 1,
                        NamaPaket = item.Nama,
                        Harga = item.Harga,
                        DeskripsiMenu = item.Deskripsi,
                        Gambar = item.Gambar,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    });
                }
                context.SaveChanges();
            }

            // 4. Pengguna Otentik (Non-Dummy)
            var realCustomers = new List<(string Username, string Nama, string Email, string Telepon, string Alamat)>
            {
                ("ahmad_fauzi", "Ahmad Fauzi", "ahmad.fauzi@gmail.com", "081298761234", "Jl. Siliwangi No. 18, Kec. Indramayu, Kab. Indramayu"),
                ("dewi_lestari", "Dewi Lestari", "dewi.lestari@yahoo.com", "085712348899", "Perumahan Graha Indah Blok A No. 5, Cirebon"),
                ("rizky_ramadhan", "Rizky Ramadhan", "rizky.ramadhan@gmail.com", "082145673322", "Desa Tulungagung RT 04/RW 02, Kec. Kertasemaya, Indramayu"),
                ("siti_nurhaliza", "Siti Nurhaliza", "siti.nurhaliza@gmail.com", "087812903344", "Jl. Veteran No. 8, Kec. Jatibarang, Kab. Indramayu"),
                ("hendra_setiawan", "Hendra Setiawan", "hendra.setiawan@gmail.com", "081390124567", "Perumahan Griya Asri Blok D No. 9, Indramayu"),
                ("maya_anggraini", "Maya Anggraini", "maya.anggraini@outlook.com", "085698712345", "Jl. Kartini No. 34, Kota Cirebon"),
                ("fajar_pratama", "Fajar Pratama", "fajar.pratama@gmail.com", "081234098712", "Desa Cadangpinggan RT 01/RW 03, Sukagumiwang, Indramayu"),
                ("anisa_rahmawati", "Anisa Rahmawati", "anisa.rahmawati@gmail.com", "083890124455", "Jl. Gatot Subroto No. 50, Jatibarang, Indramayu"),
                ("dimas_wahyudi", "Dimas Wahyudi", "dimas.wahyudi@gmail.com", "081287654390", "Jl. Ahmad Yani No. 72, Kab. Indramayu"),
                ("tri_handayani", "Tri Handayani", "tri.handayani@gmail.com", "085711223344", "Perumahan Telaga Pelangi Blok B2 No. 14, Jatibarang"),
                ("bayu_kurniawan", "Bayu Kurniawan", "bayu.kurniawan@gmail.com", "082233445566", "Jl. Mayor Dasuki No. 102, Jatibarang, Indramayu"),
                ("putri_wulandari", "Putri Wulandari", "putri.wulandari@gmail.com", "081908761234", "Desa Sukagumiwang RT 03/RW 01, Kab. Indramayu"),
                ("eko_prasetyo", "Eko Prasetyo", "eko.prasetyo@gmail.com", "081345678901", "Jl. Pasir Putih No. 21, Karangampel, Indramayu"),
                ("ratna_sari", "Ratna Sari", "ratna.sari@gmail.com", "087712349088", "Komp. Karyawan Pertamina Blok C-15, Balongan, Indramayu"),
                ("arif_hidayat", "Arif Hidayat", "arif.hidayat@gmail.com", "085612348900", "Jl. Raya Cirebon-Indramayu KM 14, Kertasemaya"),
                ("mega_permata", "Mega Permata", "mega.permata@gmail.com", "081267890123", "Jl. Tuparev No. 89, Cirebon"),
                ("bagus_saputra", "Bagus Saputra", "bagus.saputra@gmail.com", "083145678990", "Desa Tenajar Lor RT 01/RW 02, Kec. Kertasemaya, Indramayu")
            };

            var existingUsers = context.Penggunas.ToList();
            if (existingUsers.Any())
            {
                // Update owner dummy
                var existingOwner = existingUsers.FirstOrDefault(u => u.PeranId == ownerRoleId || u.Username == "owner");
                if (existingOwner != null)
                {
                    existingOwner.NamaLengkap = "Hj. Mimi Saripah";
                    existingOwner.Email = "cateringmimisaripah@gmail.com";
                    existingOwner.NomorTelepon = "083148448516";
                    existingOwner.Alamat = "Desa Tenajar Lor RT 03/RW 01, Kec. Kertasemaya, Kab. Indramayu";
                    existingOwner.UpdatedAt = DateTime.Now;
                }

                // Update employee dummy
                var existingEmployee = existingUsers.FirstOrDefault(u => u.PeranId == employeeRoleId || u.Username == "karyawan");
                if (existingEmployee != null)
                {
                    existingEmployee.NamaLengkap = "Siti Aminah";
                    existingEmployee.Email = "sitiaminah@catering.com";
                    existingEmployee.NomorTelepon = "083148448517";
                    existingEmployee.Alamat = "Desa Tenajar Kidul RT 02/RW 01, Kec. Kertasemaya, Kab. Indramayu";
                    existingEmployee.UpdatedAt = DateTime.Now;
                }

                // Update dummy user1..user17
                for (int i = 1; i <= 17; i++)
                {
                    string username = $"user{i}";
                    var dummyUser = existingUsers.FirstOrDefault(u => u.Username == username || u.NamaLengkap == $"Pelanggan Contoh {i}");
                    if (dummyUser != null && i <= realCustomers.Count)
                    {
                        var realData = realCustomers[i - 1];
                        dummyUser.Username = realData.Username;
                        dummyUser.NamaLengkap = realData.Nama;
                        dummyUser.Email = realData.Email;
                        dummyUser.NomorTelepon = realData.Telepon;
                        dummyUser.Alamat = realData.Alamat;
                        dummyUser.UpdatedAt = DateTime.Now;
                    }
                }
                context.SaveChanges();
            }
            else
            {
                // Seed baru
                var owner = new Pengguna
                {
                    PeranId = ownerRoleId,
                    NamaLengkap = "Hj. Mimi Saripah",
                    Username = "owner",
                    Email = "cateringmimisaripah@gmail.com",
                    NomorTelepon = "083148448516",
                    Alamat = "Desa Tenajar Lor RT 03/RW 01, Kec. Kertasemaya, Kab. Indramayu",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                owner.PasswordHash = hasher.HashPassword(owner, "owner123");
                context.Penggunas.Add(owner);

                var employee = new Pengguna
                {
                    PeranId = employeeRoleId,
                    NamaLengkap = "Siti Aminah",
                    Username = "karyawan",
                    Email = "sitiaminah@catering.com",
                    NomorTelepon = "083148448517",
                    Alamat = "Desa Tenajar Kidul RT 02/RW 01, Kec. Kertasemaya, Kab. Indramayu",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                employee.PasswordHash = hasher.HashPassword(employee, "karyawan123");
                context.Penggunas.Add(employee);

                foreach (var cust in realCustomers)
                {
                    var user = new Pengguna
                    {
                        PeranId = userRoleId,
                        NamaLengkap = cust.Nama,
                        Username = cust.Username,
                        Email = cust.Email,
                        NomorTelepon = cust.Telepon,
                        Alamat = cust.Alamat,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    user.PasswordHash = hasher.HashPassword(user, "user123");
                    context.Penggunas.Add(user);
                }
                context.SaveChanges();
            }

            // 5. Pesanan & Catatan Otentik
            var realisticNotes = new[]
            {
                "Mohon dikirim tepat pukul 11:30 WIB untuk makan siang kantor. Makanan tolong tetap hangat.",
                "Sambal tolong dipisahkan di wadah tersendiri agar anak-anak dapat menikmati hidangan.",
                "Tolong kemasan per kotak dilengkapi sendok, garpu higienis, dan tisu basah.",
                "Pengiriman ke aula gedung pertemuan RT, mohon hubungi nomor tertera sebelum tiba di lokasi.",
                "Daging rendang tolong dimasak empuk dan sayur jangan terlalu asin.",
                "Tolong disertakan kantong plastik besar untuk memudahkan pembagian nasi kotak ke para tamu."
            };

            var existingOrders = context.Pesanans.Include(p => p.DetailPesanans).Include(p => p.Pembayaran).ToList();
            if (existingOrders.Any())
            {
                // Update catatan dummy di detail pesanan jika masih "Pedas sedang, sayur dipisah"
                var existingDetails = context.DetailPesanans.Where(d => d.Catatan != null && d.Catatan.Contains("Pedas sedang, sayur dipisah")).ToList();
                for (int i = 0; i < existingDetails.Count; i++)
                {
                    existingDetails[i].Catatan = realisticNotes[i % realisticNotes.Length];
                    existingDetails[i].UpdatedAt = DateTime.Now;
                }

                // Perbaiki data pesanan sebelumnya yang beralamat di luar area gratis agar TotalBayar mencakup ongkir
                foreach (var ord in existingOrders)
                {
                    decimal subtotal = ord.DetailPesanans.Sum(d => d.Subtotal);
                    decimal expectedOngkir = CateringApp.Helpers.OngkirHelper.HitungOngkir(ord.AlamatPengiriman);
                    if (subtotal > 0 && ord.TotalBayar == subtotal && expectedOngkir > 0)
                    {
                        ord.TotalBayar = subtotal + expectedOngkir;
                        ord.UpdatedAt = DateTime.Now;

                        if (ord.Pembayaran != null)
                        {
                            ord.Pembayaran.JumlahBayar = ord.TotalBayar;
                            ord.Pembayaran.UpdatedAt = DateTime.Now;
                        }
                    }
                }
                context.SaveChanges();
            }
            else
            {
                var customers = context.Penggunas.Where(u => u.PeranId == userRoleId).ToList();
                var pakets = context.PaketMenus.ToList();

                for (int i = 1; i <= 15; i++)
                {
                    var customer = customers[i % customers.Count];
                    var paket = pakets[i % pakets.Count];
                    int qty = 15 + (i * 3);
                    decimal subtotal = paket.Harga * qty;
                    decimal ongkir = CateringApp.Helpers.OngkirHelper.HitungOngkir(customer.Alamat);
                    decimal total = subtotal + ongkir;

                    var status = i % 4 == 0 ? "Selesai" : (i % 4 == 1 ? "Dikirim" : (i % 4 == 2 ? "Diproses" : "Pending"));
                    var tanggalPesan = DateTime.Now.AddDays(-i);
                    var tanggalKirim = DateTime.Now.AddDays(-i + 2);

                    var pesanan = new Pesanan
                    {
                        PenggunaId = customer.PenggunaId,
                        NomorPesanan = $"ORD-{tanggalPesan:yyyyMMdd}-{i:00}",
                        TanggalPesan = tanggalPesan,
                        TanggalPengiriman = tanggalKirim,
                        AlamatPengiriman = customer.Alamat ?? "Desa Tenajar Lor, Indramayu",
                        TotalBayar = total,
                        StatusPesanan = status,
                        CreatedAt = tanggalPesan,
                        UpdatedAt = tanggalPesan
                    };
                    context.Pesanans.Add(pesanan);
                    context.SaveChanges();

                    var detail = new DetailPesanan
                    {
                        PesananId = pesanan.PesananId,
                        PaketId = paket.PaketId,
                        Jumlah = qty,
                        HargaSatuan = paket.Harga,
                        Subtotal = total,
                        Catatan = realisticNotes[i % realisticNotes.Length],
                        CreatedAt = tanggalPesan,
                        UpdatedAt = tanggalPesan
                    };
                    context.DetailPesanans.Add(detail);

                    if (status != "Pending")
                    {
                        var pembayaran = new Pembayaran
                        {
                            PesananId = pesanan.PesananId,
                            MetodePembayaran = i % 3 == 0 ? "Transfer Bank Mandiri" : (i % 3 == 1 ? "Transfer Bank BCA" : "QRIS / GoPay"),
                            JumlahBayar = total,
                            TanggalBayar = tanggalPesan,
                            BuktiTransfer = $"/uploads/PAY_{pesanan.PesananId}_proof.jpg",
                            StatusVerifikasi = "Valid",
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
