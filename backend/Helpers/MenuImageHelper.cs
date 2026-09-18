using System;

namespace CateringApp.Helpers
{
    public static class MenuImageHelper
    {
        // Peta gambar otentik hidangan Nusantara berbasis masakan otentik Indonesia
        public static string GetGambarUrl(string? namaMenu, string? gambarDb = null, string? kategori = null)
        {
            // Jika user/admin mengunggah file lokal sendiri via sistem upload, prioritaskan file tersebut
            if (!string.IsNullOrEmpty(gambarDb))
            {
                if (gambarDb.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase))
                {
                    return gambarDb;
                }
                // Jika sudah berupa URL gambar valid yang bukan wikimedia (yang sering 403) dan bukan placeholder svg
                if (gambarDb.StartsWith("http", StringComparison.OrdinalIgnoreCase) && 
                    !gambarDb.Contains("wikimedia.org", StringComparison.OrdinalIgnoreCase) && 
                    !gambarDb.EndsWith(".svg", StringComparison.OrdinalIgnoreCase))
                {
                    return gambarDb;
                }
            }

            string name = (namaMenu ?? string.Empty).ToLowerInvariant();
            string cat = (kategori ?? string.Empty).ToLowerInvariant();

            // 1. Nasi Tumpeng (Wajib foto tumpeng asli bertingkat / kerucut kuning komplit)
            if (name.Contains("tumpeng") || cat.Contains("tumpeng"))
            {
                return "https://images.unsplash.com/photo-1626074353765-517a681e40be?q=80&w=700&auto=format&fit=crop";
            }

            // 2. Rendang Daging Sapi Minang Otentik
            if (name.Contains("rendang"))
            {
                return "https://images.unsplash.com/photo-1604329760661-e71dc83f8f26?q=80&w=700&auto=format&fit=crop";
            }

            // 3. Sate Ayam Madura
            if (name.Contains("sate"))
            {
                return "https://images.unsplash.com/photo-1529563021893-cc83c914d73e?q=80&w=700&auto=format&fit=crop";
            }

            // 4. Bakso Sapi Malang
            if (name.Contains("bakso"))
            {
                return "https://images.unsplash.com/photo-1569718212165-3a8278d5f624?q=80&w=700&auto=format&fit=crop";
            }

            // 5. Soto Betawi Daging Sapi Kuah Santan/Susu
            if (name.Contains("soto"))
            {
                return "https://images.unsplash.com/photo-1572656631137-7935297eff55?q=80&w=700&auto=format&fit=crop";
            }

            // 6. Nasi Liwet Solo Komplit
            if (name.Contains("liwet"))
            {
                return "https://images.unsplash.com/photo-1617093727343-374698b1b08d?q=80&w=700&auto=format&fit=crop";
            }

            // 7. Ayam Bakar Spesial Bumbu Madu/Kecap
            if (name.Contains("bakar") && (name.Contains("ayam") || name.Contains("ikan")))
            {
                if (name.Contains("ikan"))
                    return "https://images.unsplash.com/photo-1519708227418-c8fd9a32b7a2?q=80&w=700&auto=format&fit=crop";
                return "https://images.unsplash.com/photo-1598514983318-2f64f8f4796c?q=80&w=700&auto=format&fit=crop";
            }

            // 8. Ayam Goreng Lengkuas / Serundeng
            if (name.Contains("lengkuas") || name.Contains("serundeng") || (name.Contains("ayam") && name.Contains("goreng")))
            {
                return "https://images.unsplash.com/photo-1626082927389-6cd097cdc6ec?q=80&w=700&auto=format&fit=crop";
            }

            // 9. Prasmanan / Buffet Pernikahan & Hajatan
            if (name.Contains("prasmanan") || cat.Contains("prasmanan") || cat.Contains("hajatan"))
            {
                if (name.Contains("pernikahan") || name.Contains("wedding"))
                    return "https://images.unsplash.com/photo-1519225421980-715cb0215aed?q=80&w=700&auto=format&fit=crop";
                return "https://images.unsplash.com/photo-1555244162-803834f70033?q=80&w=700&auto=format&fit=crop";
            }

            // 10. Snack Box & Aneka Kue Tradisional
            if (name.Contains("snack") || name.Contains("kue") || cat.Contains("snack"))
            {
                return "https://images.unsplash.com/photo-1578985545062-69928b1d9587?q=80&w=700&auto=format&fit=crop";
            }

            // 11. Minuman Segar / Es Teler / Es Campur
            if (name.Contains("es") || name.Contains("teler") || name.Contains("campur") || cat.Contains("minuman"))
            {
                return "https://images.unsplash.com/photo-1551024709-8f23befc6f87?q=80&w=700&auto=format&fit=crop";
            }

            // 12. Nasi Kebuli Kambing
            if (name.Contains("kebuli") || name.Contains("kambing") || name.Contains("aqiqah"))
            {
                return "https://images.unsplash.com/photo-1563379091339-03b21ab4a4f8?q=80&w=700&auto=format&fit=crop";
            }

            // 13. Coffee Break & Pastry
            if (name.Contains("coffee") || name.Contains("pastry"))
            {
                return "https://images.unsplash.com/photo-1517256064527-09c73fc73e38?q=80&w=700&auto=format&fit=crop";
            }

            // 14. Menu Rumahan / Harian / Diet
            if (name.Contains("harian") || name.Contains("keluarga") || name.Contains("diet"))
            {
                return "https://images.unsplash.com/photo-1546069901-ba9599a7e63c?q=80&w=700&auto=format&fit=crop";
            }

            // Fallback: Nasi Kotak Tradisional
            return "https://images.unsplash.com/photo-1598515214211-89d3c73ae83b?q=80&w=700&auto=format&fit=crop";
        }
    }
}
