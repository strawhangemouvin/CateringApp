using System;
using System.ComponentModel.DataAnnotations;

namespace CateringApp.Models.ViewModel
{
    public class KesanPesanItem
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");

        [Required(ErrorMessage = "Nama wajib diisi")]
        [StringLength(100, ErrorMessage = "Nama maksimal 100 karakter")]
        public string Nama { get; set; } = string.Empty;

        [StringLength(100)]
        public string Acara { get; set; } = "Pelanggan";

        [Range(1, 5, ErrorMessage = "Rating harus antara 1 sampai 5")]
        public int Rating { get; set; } = 5;

        [Required(ErrorMessage = "Pesan wajib diisi")]
        [StringLength(500, ErrorMessage = "Pesan maksimal 500 karakter")]
        public string Pesan { get; set; } = string.Empty;

        public string Tanggal { get; set; } = DateTime.Now.ToString("dd MMM yyyy, HH:mm");
    }
}
