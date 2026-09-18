using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CateringApp.Helpers;

public static class ValidationHelper
{
    public static (bool IsValid, string? ErrorMessage) ValidateNomorTelepon(string? rawPhone, bool isRequired = true)
    {
        if (string.IsNullOrWhiteSpace(rawPhone))
        {
            if (isRequired)
                return (false, "Nomor telepon wajib diisi.");
            return (true, null);
        }

        string phone = rawPhone.Trim();

        if (phone.Contains('-'))
        {
            return (false, "Nomor telepon tidak boleh mengandung tanda minus (-). Masukkan angka saja.");
        }

        if (phone.Contains(' '))
        {
            return (false, "Nomor telepon tidak boleh mengandung spasi.");
        }

        if (!phone.StartsWith("08") && !phone.StartsWith("628") && !phone.StartsWith("+628"))
        {
            return (false, "Nomor telepon kartu seluler harus diawali 08, 628, atau +628.");
        }

        string cleanPhone = phone.Replace("+", "");
        if (!Regex.IsMatch(cleanPhone, @"^\d+$"))
        {
            return (false, "Nomor telepon hanya boleh berisi angka.");
        }

        string localDigits = cleanPhone.StartsWith("62") ? "0" + cleanPhone.Substring(2) : cleanPhone;
        if (localDigits.Length < 10 || localDigits.Length > 13)
        {
            return (false, "Nomor telepon seluler pada umumnya terdiri dari 10 hingga 13 digit angka.");
        }

        string suffix = localDigits.Length > 2 ? localDigits.Substring(2) : "";
        if (Regex.IsMatch(phone, @"(\d)\1{4,}") || (suffix.Length >= 6 && suffix.Distinct().Count() <= 2))
        {
            return (false, "Nomor telepon tidak valid karena angka berulang/pola nomor acak.");
        }

        return (true, null);
    }

    public static (bool IsValid, string? ErrorMessage) ValidateEmail(string? rawEmail, bool isRequired = true)
    {
        if (string.IsNullOrWhiteSpace(rawEmail))
        {
            if (isRequired)
                return (false, "Alamat email wajib diisi.");
            return (true, null);
        }

        string email = rawEmail.Trim().ToLowerInvariant();

        if (email.Length > 100)
        {
            return (false, "Alamat email tidak boleh lebih dari 100 karakter.");
        }

        if (email.Contains(' '))
        {
            return (false, "Alamat email tidak boleh mengandung spasi.");
        }

        if (!email.Contains('@'))
        {
            return (false, "Format email tidak valid (harus menyertakan simbol '@').");
        }

        var parts = email.Split('@');
        if (parts.Length != 2)
        {
            return (false, "Format email tidak valid (hanya boleh ada satu simbol '@').");
        }

        string local = parts[0];
        string domain = parts[1];

        if (string.IsNullOrWhiteSpace(local) || string.IsNullOrWhiteSpace(domain))
        {
            return (false, "Format email tidak valid (bagian nama atau domain tidak boleh kosong).");
        }

        if (local.StartsWith(".") || local.EndsWith(".") || local.Contains(".."))
        {
            return (false, "Format nama email tidak valid (tidak boleh diawali, diakhiri, atau memuat titik ganda '..').");
        }

        if (domain.StartsWith(".") || domain.EndsWith(".") || domain.Contains("..") || domain.StartsWith("-") || domain.EndsWith("-"))
        {
            return (false, "Format domain email tidak valid (tidak boleh diawali/diakhiri titik/strip, atau memuat '..').");
        }

        if (!domain.Contains('.'))
        {
            return (false, "Domain email tidak lengkap (harus memiliki ekstensi seperti .com atau .id).");
        }

        // Deteksi saltik / typo pada domain email populer
        var typoDomains = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "gmial.com", "gmail.com" },
            { "gamil.com", "gmail.com" },
            { "gmaill.com", "gmail.com" },
            { "gmai.com", "gmail.com" },
            { "gmeil.com", "gmail.com" },
            { "gmal.com", "gmail.com" },
            { "gmaik.com", "gmail.com" },
            { "gmail.con", "gmail.com" },
            { "gmail.cpm", "gmail.com" },
            { "gmail.coom", "gmail.com" },
            { "gmail.cm", "gmail.com" },
            { "gmail.co", "gmail.com" },
            { "yaho.com", "yahoo.com" },
            { "yahooo.com", "yahoo.com" },
            { "yahu.com", "yahoo.com" },
            { "yahoo.con", "yahoo.com" },
            { "yaho.co.id", "yahoo.co.id" },
            { "hotmial.com", "hotmail.com" },
            { "hotmai.com", "hotmail.com" },
            { "outlok.com", "outlook.com" },
            { "otlook.com", "outlook.com" },
            { "outluk.com", "outlook.com" },
            { "icoud.com", "icloud.com" },
            { "iclod.com", "icloud.com" }
        };

        if (typoDomains.TryGetValue(domain, out var suggestedDomain))
        {
            return (false, $"Format domain email tidak valid (terdeteksi kesalahan pengetikan '@{domain}'). Gunakan domain yang benar seperti '@{suggestedDomain}'.");
        }

        // Validasi RFC Regex standar
        var emailRegex = new Regex(@"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*\.[a-zA-Z]{2,}$");
        if (!emailRegex.IsMatch(email))
        {
            return (false, "Format alamat email tidak valid (contoh yang benar: nama@gmail.com).");
        }

        string tld = domain.Substring(domain.LastIndexOf('.') + 1);
        if (tld.Length < 2 || !Regex.IsMatch(tld, @"^[a-zA-Z]+$"))
        {
            return (false, "Ekstensi domain email tidak valid (contoh: .com, .id, .net).");
        }

        return (true, null);
    }

    public static (bool IsValid, string? ErrorMessage) ValidateUsername(string? rawUsername)
    {
        if (string.IsNullOrWhiteSpace(rawUsername))
        {
            return (false, "Username wajib diisi.");
        }

        string username = rawUsername.Trim();

        if (username.Length < 3 || username.Length > 30)
        {
            return (false, "Username harus terdiri dari 3 hingga 30 karakter.");
        }

        if (username.Contains(' '))
        {
            return (false, "Username tidak boleh mengandung spasi.");
        }

        if (!Regex.IsMatch(username, @"^[a-zA-Z0-9_.]+$"))
        {
            return (false, "Username hanya boleh memuat huruf, angka, titik, atau garis bawah (_).");
        }

        return (true, null);
    }

    public static (bool IsValid, string? ErrorMessage) ValidatePassword(string? rawPassword, bool isRequired = true)
    {
        if (string.IsNullOrWhiteSpace(rawPassword))
        {
            if (isRequired)
                return (false, "Password wajib diisi.");
            return (true, null);
        }

        if (rawPassword.Length < 6)
        {
            return (false, "Password minimal 6 karakter.");
        }

        return (true, null);
    }

    public static (bool IsValid, string? ErrorMessage) ValidateHargaMenu(decimal harga)
    {
        if (harga <= 0)
        {
            return (false, "Harga paket menu harus lebih besar dari 0 (tidak boleh 0 atau bernilai minus).");
        }

        if (harga < 1000)
        {
            return (false, "Harga paket menu minimal Rp 1.000 per porsi.");
        }

        if (harga > 100000000)
        {
            return (false, "Harga paket menu tidak wajar (maksimal Rp 100.000.000).");
        }

        return (true, null);
    }

    public static (bool IsValid, string? ErrorMessage) ValidateNamaPaket(string? namaPaket)
    {
        if (string.IsNullOrWhiteSpace(namaPaket))
        {
            return (false, "Nama paket menu wajib diisi.");
        }

        string nama = namaPaket.Trim();
        if (nama.Length < 3 || nama.Length > 100)
        {
            return (false, "Nama paket menu harus terdiri dari 3 hingga 100 karakter.");
        }

        return (true, null);
    }

    public static (bool IsValid, string? ErrorMessage) ValidateDeskripsiMenu(string? deskripsi, bool isRequired = false)
    {
        if (string.IsNullOrWhiteSpace(deskripsi))
        {
            if (isRequired)
            {
                return (false, "Deskripsi / detail sajian menu wajib diisi.");
            }
            return (true, null);
        }

        string desc = deskripsi.Trim();
        if (desc.Length > 1000)
        {
            return (false, "Deskripsi menu maksimal 1000 karakter.");
        }

        return (true, null);
    }
}
