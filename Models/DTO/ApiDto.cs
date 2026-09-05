using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CateringApp.Models.DTO
{
    #region Menu DTOs
    public class CreateMenuDto
    {
        [Required(ErrorMessage = "Kategori menu wajib dipilih.")]
        [Range(1, int.MaxValue, ErrorMessage = "ID Kategori harus bernilai numerik positif.")]
        public int KategoriId { get; set; }

        [Required(ErrorMessage = "Nama paket menu wajib diisi.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Nama paket harus antara 3 hingga 100 karakter.")]
        public string NamaPaket { get; set; } = string.Empty;

        [Required(ErrorMessage = "Harga wajib diisi.")]
        [Range(1000, 100000000, ErrorMessage = "Harga harus bernilai numerik antara Rp 1.000 sampai Rp 100.000.000.")]
        public decimal Harga { get; set; }

        [MaxLength(500, ErrorMessage = "Deskripsi menu maksimal 500 karakter.")]
        public string? DeskripsiMenu { get; set; }

        [MaxLength(255, ErrorMessage = "Path gambar maksimal 255 karakter.")]
        public string? Gambar { get; set; }
    }

    public class UpdateMenuDto
    {
        [Required(ErrorMessage = "Kategori menu wajib dipilih.")]
        [Range(1, int.MaxValue, ErrorMessage = "ID Kategori harus bernilai numerik positif.")]
        public int KategoriId { get; set; }

        [Required(ErrorMessage = "Nama paket menu wajib diisi.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Nama paket harus antara 3 hingga 100 karakter.")]
        public string NamaPaket { get; set; } = string.Empty;

        [Required(ErrorMessage = "Harga wajib diisi.")]
        [Range(1000, 100000000, ErrorMessage = "Harga harus bernilai numerik antara Rp 1.000 sampai Rp 100.000.000.")]
        public decimal Harga { get; set; }

        [MaxLength(500, ErrorMessage = "Deskripsi menu maksimal 500 karakter.")]
        public string? DeskripsiMenu { get; set; }

        [MaxLength(255, ErrorMessage = "Path gambar maksimal 255 karakter.")]
        public string? Gambar { get; set; }
    }

    public class PatchMenuDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "ID Kategori harus bernilai numerik positif.")]
        public int? KategoriId { get; set; }

        [StringLength(100, MinimumLength = 3, ErrorMessage = "Nama paket harus antara 3 hingga 100 karakter.")]
        public string? NamaPaket { get; set; }

        [Range(1000, 100000000, ErrorMessage = "Harga harus bernilai numerik antara Rp 1.000 sampai Rp 100.000.000.")]
        public decimal? Harga { get; set; }

        [MaxLength(500, ErrorMessage = "Deskripsi menu maksimal 500 karakter.")]
        public string? DeskripsiMenu { get; set; }

        [MaxLength(255, ErrorMessage = "Path gambar maksimal 255 karakter.")]
        public string? Gambar { get; set; }
    }
    #endregion

    #region Kategori DTOs
    public class CreateKategoriDto
    {
        [Required(ErrorMessage = "Nama kategori wajib diisi.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Nama kategori harus antara 2 hingga 50 karakter.")]
        public string NamaKategori { get; set; } = string.Empty;

        [MaxLength(255, ErrorMessage = "Deskripsi kategori maksimal 255 karakter.")]
        public string? Deskripsi { get; set; }
    }

    public class UpdateKategoriDto
    {
        [Required(ErrorMessage = "Nama kategori wajib diisi.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Nama kategori harus antara 2 hingga 50 karakter.")]
        public string NamaKategori { get; set; } = string.Empty;

        [MaxLength(255, ErrorMessage = "Deskripsi kategori maksimal 255 karakter.")]
        public string? Deskripsi { get; set; }
    }

    public class PatchKategoriDto
    {
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Nama kategori harus antara 2 hingga 50 karakter.")]
        public string? NamaKategori { get; set; }

        [MaxLength(255, ErrorMessage = "Deskripsi kategori maksimal 255 karakter.")]
        public string? Deskripsi { get; set; }
    }
    #endregion

    #region Pesanan DTOs
    public class OrderItemDto
    {
        [Required(ErrorMessage = "ID Paket menu wajib diisi.")]
        [Range(1, int.MaxValue, ErrorMessage = "PaketId harus berupa angka positif.")]
        public int PaketId { get; set; }

        [Required(ErrorMessage = "Jumlah pesanan wajib diisi.")]
        [Range(1, 1000, ErrorMessage = "Jumlah pesanan harus berupa angka antara 1 sampai 1000 porsi.")]
        public int Jumlah { get; set; }

        [MaxLength(200, ErrorMessage = "Catatan item maksimal 200 karakter.")]
        public string? Catatan { get; set; }
    }

    public class CreateOrderDto
    {
        [Required(ErrorMessage = "ID Pengguna wajib diisi.")]
        [Range(1, int.MaxValue, ErrorMessage = "PenggunaId harus berupa angka positif.")]
        public int PenggunaId { get; set; }

        [Required(ErrorMessage = "Tanggal pengiriman wajib diisi.")]
        [DataType(DataType.Date, ErrorMessage = "Format tanggal pengiriman tidak valid.")]
        public DateTime TanggalPengiriman { get; set; }

        [Required(ErrorMessage = "Alamat pengiriman wajib diisi.")]
        [StringLength(300, MinimumLength = 5, ErrorMessage = "Alamat pengiriman harus antara 5 sampai 300 karakter.")]
        public string AlamatPengiriman { get; set; } = string.Empty;

        [Required(ErrorMessage = "Daftar item pesanan tidak boleh kosong.")]
        [MinLength(1, ErrorMessage = "Minimal harus memesan 1 menu.")]
        public List<OrderItemDto> Items { get; set; } = new();
    }

    public class UpdateOrderStatusDto
    {
        [Required(ErrorMessage = "Status pesanan wajib diisi.")]
        [RegularExpression("^(Pending|Menunggu Pembayaran|Menunggu Verifikasi|Diproses|Dikirim|Selesai|Dibatalkan)$", 
            ErrorMessage = "Status pesanan harus berupa salah satu dari: Pending, Menunggu Pembayaran, Menunggu Verifikasi, Diproses, Dikirim, Selesai, Dibatalkan.")]
        public string StatusPesanan { get; set; } = string.Empty;
    }
    #endregion

    #region Pembayaran DTOs
    public class CreatePembayaranDto
    {
        [Required(ErrorMessage = "ID Pesanan wajib diisi.")]
        [Range(1, int.MaxValue, ErrorMessage = "PesananId harus berupa angka positif.")]
        public int PesananId { get; set; }

        [Required(ErrorMessage = "Metode pembayaran wajib diisi.")]
        [RegularExpression("^(Transfer Bank|QRIS|E-Wallet|Tunai)$", 
            ErrorMessage = "Metode pembayaran harus berupa salah satu dari enum: Transfer Bank, QRIS, E-Wallet, Tunai.")]
        public string MetodePembayaran { get; set; } = string.Empty;

        [Required(ErrorMessage = "Bukti transfer / struk pembayaran wajib diisi.")]
        [MaxLength(255, ErrorMessage = "Path bukti transfer maksimal 255 karakter.")]
        public string BuktiTransfer { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tanggal bayar wajib diisi.")]
        [DataType(DataType.DateTime, ErrorMessage = "Format tanggal pembayaran tidak valid.")]
        public DateTime TanggalBayar { get; set; } = DateTime.Now;
    }

    public class VerifikasiPembayaranDto
    {
        [Required(ErrorMessage = "Status verifikasi wajib diisi.")]
        [RegularExpression("^(Menunggu Verifikasi|Valid|Ditolak)$", 
            ErrorMessage = "Status verifikasi harus berupa salah satu dari enum: Menunggu Verifikasi, Valid, Ditolak.")]
        public string StatusVerifikasi { get; set; } = string.Empty;
    }
    #endregion

    #region User & Auth DTOs
    public class CreateUserDto
    {
        [Required(ErrorMessage = "Role / Peran wajib dipilih.")]
        [Range(1, 2, ErrorMessage = "Peran ID harus berupa nilai enum 1 (Admin/Pemilik Toko) atau 2 (Pelanggan).")]
        public int PeranId { get; set; }

        [Required(ErrorMessage = "Nama lengkap wajib diisi.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Nama lengkap harus antara 3 sampai 100 karakter.")]
        public string NamaLengkap { get; set; } = string.Empty;

        [Required(ErrorMessage = "Username wajib diisi.")]
        [StringLength(50, MinimumLength = 4, ErrorMessage = "Username harus antara 4 sampai 50 karakter.")]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Username hanya boleh mengandung huruf, angka, dan underscore.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email wajib diisi.")]
        [EmailAddress(ErrorMessage = "Format email tidak valid.")]
        [MaxLength(100, ErrorMessage = "Email maksimal 100 karakter.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password wajib diisi.")]
        [MinLength(6, ErrorMessage = "Password minimal 6 karakter.")]
        [MaxLength(50, ErrorMessage = "Password maksimal 50 karakter.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nomor telepon wajib diisi.")]
        [RegularExpression(@"^(0|62|\+62)[0-9]{8,12}$", ErrorMessage = "Nomor telepon harus diawali 0, 62, atau +62 dan berisi 9-15 digit angka numeric.")]
        [MaxLength(15, ErrorMessage = "Nomor telepon maksimal 15 digit.")]
        public string NomorTelepon { get; set; } = string.Empty;

        [Required(ErrorMessage = "Alamat wajib diisi.")]
        [StringLength(255, MinimumLength = 5, ErrorMessage = "Alamat harus antara 5 sampai 255 karakter.")]
        public string Alamat { get; set; } = string.Empty;
    }

    public class UpdateUserDto
    {
        [Required(ErrorMessage = "Role / Peran wajib dipilih.")]
        [Range(1, 2, ErrorMessage = "Peran ID harus berupa nilai enum 1 (Admin/Pemilik Toko) atau 2 (Pelanggan).")]
        public int PeranId { get; set; }

        [Required(ErrorMessage = "Nama lengkap wajib diisi.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Nama lengkap harus antara 3 sampai 100 karakter.")]
        public string NamaLengkap { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email wajib diisi.")]
        [EmailAddress(ErrorMessage = "Format email tidak valid.")]
        [MaxLength(100, ErrorMessage = "Email maksimal 100 karakter.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nomor telepon wajib diisi.")]
        [RegularExpression(@"^(0|62|\+62)[0-9]{8,12}$", ErrorMessage = "Nomor telepon harus diawali 0, 62, atau +62 dan berisi 9-15 digit angka numeric.")]
        [MaxLength(15, ErrorMessage = "Nomor telepon maksimal 15 digit.")]
        public string NomorTelepon { get; set; } = string.Empty;

        [Required(ErrorMessage = "Alamat wajib diisi.")]
        [StringLength(255, MinimumLength = 5, ErrorMessage = "Alamat harus antara 5 sampai 255 karakter.")]
        public string Alamat { get; set; } = string.Empty;
    }

    public class RegisterApiDto
    {
        [Required(ErrorMessage = "Nama lengkap wajib diisi.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Nama lengkap harus antara 3 sampai 100 karakter.")]
        public string NamaLengkap { get; set; } = string.Empty;

        [Required(ErrorMessage = "Username wajib diisi.")]
        [StringLength(50, MinimumLength = 4, ErrorMessage = "Username harus antara 4 sampai 50 karakter.")]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Username hanya boleh mengandung huruf, angka, dan underscore.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email wajib diisi.")]
        [EmailAddress(ErrorMessage = "Format email tidak valid.")]
        [MaxLength(100, ErrorMessage = "Email maksimal 100 karakter.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password wajib diisi.")]
        [MinLength(6, ErrorMessage = "Password minimal 6 karakter.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Konfirmasi password wajib diisi.")]
        [Compare("Password", ErrorMessage = "Konfirmasi password tidak cocok.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nomor telepon wajib diisi.")]
        [RegularExpression(@"^(0|62|\+62)[0-9]{8,12}$", ErrorMessage = "Nomor telepon harus diawali 0, 62, atau +62 dan berisi 9-15 digit angka numeric.")]
        [MaxLength(15, ErrorMessage = "Nomor telepon maksimal 15 digit.")]
        public string NomorTelepon { get; set; } = string.Empty;

        [Required(ErrorMessage = "Alamat wajib diisi.")]
        [StringLength(255, MinimumLength = 5, ErrorMessage = "Alamat harus antara 5 sampai 255 karakter.")]
        public string Alamat { get; set; } = string.Empty;
    }

    public class RefreshTokenRequestDto
    {
        [Required(ErrorMessage = "Token wajib disertakan.")]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "RefreshToken wajib disertakan.")]
        public string RefreshToken { get; set; } = string.Empty;
    }
    #endregion
}
