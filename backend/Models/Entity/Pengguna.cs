using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CateringApp.Models.Entity;

[Table("pengguna")]
public class Pengguna
{
    [Key]
    [Column("pengguna_id")]
    public int PenggunaId { get; set; }

    [Column("peran_id")]
    [Required(ErrorMessage = "Peran / role pengguna wajib dipilih.")]
    [Range(1, int.MaxValue, ErrorMessage = "Peran / role pengguna wajib dipilih.")]
    public int PeranId { get; set; }

    [Column("nama_lengkap")]
    [Required(ErrorMessage = "Nama lengkap wajib diisi.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Nama lengkap harus antara 3 hingga 100 karakter.")]
    public string NamaLengkap { get; set; } = string.Empty;

    [Column("username")]
    [Required(ErrorMessage = "Username wajib diisi.")]
    [StringLength(30, MinimumLength = 3, ErrorMessage = "Username harus antara 3 hingga 30 karakter.")]
    [RegularExpression(@"^[a-zA-Z0-9_.]+$", ErrorMessage = "Username hanya boleh huruf, angka, titik, atau garis bawah (_).")]
    public string Username { get; set; } = string.Empty;

    [Column("password_hash")]
    public string? PasswordHash { get; set; }

    [Column("email")]
    [Required(ErrorMessage = "Email wajib diisi.")]
    [EmailAddress(ErrorMessage = "Format alamat email tidak valid.")]
    [StringLength(100, ErrorMessage = "Email tidak boleh lebih dari 100 karakter.")]
    public string Email { get; set; } = string.Empty;

    [Column("nomor_telepon")]
    [Required(ErrorMessage = "Nomor telepon wajib diisi.")]
    [MaxLength(15, ErrorMessage = "Nomor telepon tidak boleh lebih dari 15 digit.")]
    [RegularExpression(@"^(08|628|\+628)[0-9]{7,11}$", ErrorMessage = "Nomor telepon harus diawali 08, 628, atau +628 (10-13 digit).")]
    public string? NomorTelepon { get; set; }

    [Column("alamat")]
    [Required(ErrorMessage = "Alamat lengkap wajib diisi.")]
    [StringLength(300, MinimumLength = 5, ErrorMessage = "Alamat harus antara 5 hingga 300 karakter.")]
    public string? Alamat { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [Column("deleted_at")]
    public DateTime? DeletedAt { get; set; }

    public virtual Peran? Peran { get; set; }
}
