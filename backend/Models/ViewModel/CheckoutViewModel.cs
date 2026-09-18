using System;
using System.ComponentModel.DataAnnotations;

namespace CateringApp.Models.ViewModel
{
    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "Tentukan tanggal kirim")]
        [DataType(DataType.Date)]
        public DateTime TanggalPengiriman { get; set; } = DateTime.Today.AddDays(2);

        [Required(ErrorMessage = "Pilih slot waktu pengantaran acara")]
        public string JamPengantaran { get; set; } = "10:00 - 12:00";

        [Required(ErrorMessage = "Alamat pengiriman wajib diisi")]
        public string AlamatPengiriman { get; set; } = string.Empty;
    }
}
