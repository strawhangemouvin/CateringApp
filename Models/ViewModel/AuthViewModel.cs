using System.ComponentModel.DataAnnotations;

namespace CateringApp.Models.ViewModel;

public class LoginViewModel
{
    [Required(ErrorMessage = "Username wajib diisi")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password wajib diisi")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}

public class RegisterViewModel
{
    [Required(ErrorMessage = "Nama lengkap wajib diisi")]
    public string NamaLengkap { get; set; } = string.Empty;

    [Required(ErrorMessage = "Username wajib diisi")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email wajib diisi")]
    [EmailAddress(ErrorMessage = "Format email tidak valid")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nomor Telepon wajib diisi")]
    [MaxLength(15, ErrorMessage = "Nomor telepon tidak boleh lebih dari 15 digit.")]
    [RegularExpression(@"^(0|62|\+62)[0-9]{8,12}$", ErrorMessage = "Nomor telepon harus diawali 0, 62, atau +62 dan terdiri dari 9-15 digit.")]
    public string NomorTelepon { get; set; } = string.Empty;

    [Required(ErrorMessage = "Alamat wajib diisi")]
    public string Alamat { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password wajib diisi")]
    [MinLength(6, ErrorMessage = "Password minimal 6 karakter")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Compare("Password", ErrorMessage = "Konfirmasi password tidak cocok")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "Email wajib diisi")]
    [EmailAddress(ErrorMessage = "Format email tidak valid")]
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordViewModel
{
    [Required(ErrorMessage = "Email wajib diisi")]
    [EmailAddress(ErrorMessage = "Format email tidak valid")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Kode OTP wajib diisi")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "Kode OTP harus 6 digit")]
    [RegularExpression(@"^[0-9]{6}$", ErrorMessage = "Kode OTP hanya boleh berisi 6 angka")]
    public string KodeOtp { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password baru wajib diisi")]
    [MinLength(6, ErrorMessage = "Password minimal 6 karakter")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Konfirmasi password baru wajib diisi")]
    [Compare("Password", ErrorMessage = "Konfirmasi password tidak cocok")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = string.Empty;
}