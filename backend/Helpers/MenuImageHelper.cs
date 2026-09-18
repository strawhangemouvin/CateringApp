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

            // 1. Nasi Tumpeng (Wajib foto kerucut tumpeng kuning komplit asli)
            if (name.Contains("tumpeng") || cat.Contains("tumpeng"))
            {
                if (name.Contains("mini"))
                    return "/uploads/menu/tumpeng_mini_selamatan.jpg";
                return "/uploads/menu/tumpeng_kuning_komplit.jpg";
            }

            // 2. Rendang Daging Sapi Minang Otentik
            if (name.Contains("rendang"))
            {
                return "https://images.unsplash.com/photo-1604329760661-e71dc83f8f26?q=80&w=700&auto=format&fit=crop";
            }

            // 3. Sate Ayam Madura
            if (name.Contains("sate"))
            {
                return "/uploads/menu/sate_ayam_madura.jpg";
            }

            // 4. Bakso Sapi Malang
            if (name.Contains("bakso"))
            {
                return "/uploads/menu/MENU_afddf944-9c88-43f0-b499-63f9afdfea7b.jpg";
            }

            // 5. Soto Betawi Daging Sapi Kuah Santan/Susu
            if (name.Contains("soto"))
            {
                return "/uploads/menu/soto_betawi_daging.jpg";
            }

            // 6. Nasi Liwet Solo Komplit
            if (name.Contains("liwet"))
            {
                return "/uploads/menu/MENU_1f768aa2-8523-48e8-87b1-a3b9f8af1050.jpeg";
            }

            // 7. Ayam Bakar / Ikan Bakar Spesial
            if (name.Contains("bakar") && (name.Contains("ayam") || name.Contains("ikan") || name.Contains("nila")))
            {
                if (name.Contains("ikan") || name.Contains("nila"))
                    return "/uploads/menu/ikan_nila_bakar.jpg";
                return "/uploads/menu/nasi_kotak_ayam_bakar.jpg";
            }

            // 8. Ayam Goreng Lengkuas / Serundeng
            if (name.Contains("lengkuas") || name.Contains("serundeng") || (name.Contains("ayam") && name.Contains("goreng")))
            {
                return "/uploads/menu/nasi_kotak_ayam_lengkuas.jpg";
            }

            // 9. Puding Sutra Mangga
            if (name.Contains("puding") || name.Contains("mangga"))
            {
                return "/uploads/menu/puding_sutra_mangga.jpg";
            }

            // 10. Bolu Gulung Red Velvet & Keju
            if (name.Contains("bolu") || name.Contains("velvet"))
            {
                return "/uploads/menu/bolu_gulung_red_velvet.jpg";
            }

            // 11. Prasmanan / Buffet Pernikahan & Hajatan
            if (name.Contains("prasmanan") || cat.Contains("prasmanan") || cat.Contains("hajatan"))
            {
                if (name.Contains("pernikahan") || name.Contains("wedding"))
                    return "https://images.unsplash.com/photo-1519225421980-715cb0215aed?q=80&w=700&auto=format&fit=crop";
                return "https://images.unsplash.com/photo-1555244162-803834f70033?q=80&w=700&auto=format&fit=crop";
            }

            // 12. Snack Box & Aneka Kue Tradisional
            if (name.Contains("snack") || name.Contains("kue") || cat.Contains("snack"))
            {
                if (name.Contains("tradisional") || name.Contains("manis"))
                    return "/uploads/menu/snack_box_tradisional.jpg";
                return "/uploads/menu/snack_box_rapat.jpg";
            }

            // 13. Minuman Segar / Es Teler / Es Campur
            if (name.Contains("es") || name.Contains("teler") || name.Contains("campur") || cat.Contains("minuman"))
            {
                return "/uploads/menu/es_campur_nusantara.jpg";
            }

            // 14. Nasi Kebuli Kambing / Aqiqah
            if (name.Contains("kebuli") || name.Contains("kambing") || name.Contains("aqiqah"))
            {
                return "/uploads/menu/nasi_kebuli_kambing.jpg";
            }

            // 15. Coffee Break & Pastry
            if (name.Contains("coffee") || name.Contains("pastry"))
            {
                return "https://images.unsplash.com/photo-1517256064527-09c73fc73e38?q=80&w=700&auto=format&fit=crop";
            }

            // 16. Menu Rumahan / Harian / Diet
            if (name.Contains("harian") || name.Contains("keluarga") || name.Contains("diet"))
            {
                return "https://images.unsplash.com/photo-1546069901-ba9599a7e63c?q=80&w=700&auto=format&fit=crop";
            }

            // Fallback: Nasi Kotak Ayam Bakar Tradisional
            return "/uploads/menu/nasi_kotak_ayam_bakar.jpg";
        }
    }
}
