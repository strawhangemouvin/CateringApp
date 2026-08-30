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
    public int PeranId { get; set; }

    [Column("nama_lengkap")]
    public string NamaLengkap { get; set; } = string.Empty;

    [Column("username")]
    public string Username { get; set; } = string.Empty;

    [Column("password_hash")]
    public string PasswordHash { get; set; } = string.Empty;

    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [Column("nomor_telepon")]
    [MaxLength(13, ErrorMessage = "Nomor telepon tidak boleh lebih dari 13 digit.")]
    [RegularExpression(@"^[0-9]+$", ErrorMessage = "Nomor telepon hanya boleh berisi angka.")]
    public string? NomorTelepon { get; set; }

    [Column("alamat")]
    public string? Alamat { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [Column("deleted_at")]
    public DateTime? DeletedAt { get; set; }

    public virtual Peran? Peran { get; set; }
}