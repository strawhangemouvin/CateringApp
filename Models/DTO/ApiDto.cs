using System.ComponentModel.DataAnnotations;

namespace CateringApp.Models.DTO
{
    public class CreateMenuDto
    {
        [Required(ErrorMessage = "Kategori menu wajib dipilih")]
        public int KategoriId { get; set; }

        [Required(ErrorMessage = "Nama paket menu wajib diisi")]
        [StringLength(100, ErrorMessage = "Nama paket maksimal 100 karakter")]
        public string NamaPaket { get; set; } = string.Empty;

        [Required(ErrorMessage = "Harga wajib diisi")]
        [Range(1000, 100000000, ErrorMessage = "Harga harus antara Rp 1.000 sampai Rp 100.000.000")]
        public decimal Harga { get; set; }

        public string? DeskripsiMenu { get; set; }
        public string? Gambar { get; set; }
    }

    public class UpdateMenuDto
    {
        [Required(ErrorMessage = "Kategori menu wajib dipilih")]
        public int KategoriId { get; set; }

        [Required(ErrorMessage = "Nama paket menu wajib diisi")]
        [StringLength(100, ErrorMessage = "Nama paket maksimal 100 karakter")]
        public string NamaPaket { get; set; } = string.Empty;

        [Required(ErrorMessage = "Harga wajib diisi")]
        [Range(1000, 100000000, ErrorMessage = "Harga harus antara Rp 1.000 sampai Rp 100.000.000")]
        public decimal Harga { get; set; }

        public string? DeskripsiMenu { get; set; }
        public string? Gambar { get; set; }
    }

    public class PatchMenuDto
    {
        public int? KategoriId { get; set; }

        [StringLength(100, ErrorMessage = "Nama paket maksimal 100 karakter")]
        public string? NamaPaket { get; set; }

        [Range(1000, 100000000, ErrorMessage = "Harga harus antara Rp 1.000 sampai Rp 100.000.000")]
        public decimal? Harga { get; set; }

        public string? DeskripsiMenu { get; set; }
        public string? Gambar { get; set; }
    }

    public class CreateKategoriDto
    {
        [Required(ErrorMessage = "Nama kategori wajib diisi")]
        [StringLength(50, ErrorMessage = "Nama kategori maksimal 50 karakter")]
        public string NamaKategori { get; set; } = string.Empty;

        public string? Deskripsi { get; set; }
    }

    public class UpdateKategoriDto
    {
        [Required(ErrorMessage = "Nama kategori wajib diisi")]
        [StringLength(50, ErrorMessage = "Nama kategori maksimal 50 karakter")]
        public string NamaKategori { get; set; } = string.Empty;

        public string? Deskripsi { get; set; }
    }

    public class PatchKategoriDto
    {
        [StringLength(50, ErrorMessage = "Nama kategori maksimal 50 karakter")]
        public string? NamaKategori { get; set; }

        public string? Deskripsi { get; set; }
    }

    public class UpdateOrderStatusDto
    {
        [Required(ErrorMessage = "Status pesanan wajib diisi")]
        public string StatusPesanan { get; set; } = string.Empty;
    }

    public class RefreshTokenRequestDto
    {
        [Required(ErrorMessage = "Token wajib disertakan")]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "RefreshToken wajib disertakan")]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
