using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CateringApp.Models.Entity;

[Table("paket_menu")]
public class PaketMenu
{
    [Key]
    [Column("paket_id")]
    public int PaketId { get; set; }

    [Column("kategori_id")]
    [Display(Name = "Kategori Menu")]
    [Required(ErrorMessage = "Kategori menu wajib dipilih.")]
    [Range(1, int.MaxValue, ErrorMessage = "Kategori menu wajib dipilih.")]
    public int KategoriId { get; set; }

    [Column("nama_paket")]
    [Display(Name = "Nama Paket")]
    [Required(ErrorMessage = "Nama paket menu wajib diisi.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Nama paket menu harus antara 3 hingga 100 karakter.")]
    public string NamaPaket { get; set; } = string.Empty;

    [Column("harga")]
    [Display(Name = "Harga")]
    [Required(ErrorMessage = "Harga paket menu wajib diisi.")]
    [Range(1000, 100000000, ErrorMessage = "Harga paket menu minimal Rp 1.000 dan tidak boleh 0 atau minus.")]
    public decimal Harga { get; set; }

    [Column("deskripsi_menu")]
    [Display(Name = "Deskripsi Menu")]
    [StringLength(1000, ErrorMessage = "Deskripsi menu maksimal 1000 karakter.")]
    public string? DeskripsiMenu { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [Column("deleted_at")]
    public DateTime? DeletedAt { get; set; }

    [Column("gambar")]
    public string? Gambar { get; set; }

    public virtual KategoriMenu? Kategori { get; set; }
}
