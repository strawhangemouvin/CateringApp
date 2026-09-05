namespace CateringApp.Models.ViewModel
{
    public class CartItem
    {
        public int PaketId { get; set; }
        public string NamaPaket { get; set; } = string.Empty;
        public decimal Harga { get; set; }
        public string Gambar { get; set; } = string.Empty;
        public int Jumlah { get; set; }
        public string? Catatan { get; set; }
        public decimal Subtotal => Harga * Jumlah;
    }
}
