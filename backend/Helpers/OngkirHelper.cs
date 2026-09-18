using System;
using System.Collections.Generic;

namespace CateringApp.Helpers
{
    public class WilayahInfo
    {
        public bool IsCovered { get; set; }
        public decimal Ongkir { get; set; }
        public string Keterangan { get; set; } = string.Empty;
        public string NamaZona { get; set; } = string.Empty;
        public string WilayahTerdeteksi { get; set; } = string.Empty;
    }

    public static class OngkirHelper
    {
        // 1. Zona 1: Lokal Kertasemaya & Sekitarnya (Gratis Ongkir - Radius ~0-10 km dari dapur Tenajar Lor)
        private static readonly string[] Zona1Keywords = { 
            "kertasemaya", "tenajar", "sukagumiwang", "jatibarang", 
            "cadangpinggan", "tulungagung", "larangan", "lebak", "kerticala", 
            "sliyeg", "tugu", "bypass tenajar" 
        };

        // 2. Zona 2: Kab. Indramayu Tengah & Pesisir Timur (Rp 15.000 - Radius ~10-25 km)
        private static readonly string[] Zona2Keywords = { 
            "indramayu", "sindang", "balongan", "karangampel", "juntinyuat", 
            "kedokanbunder", "kedokan bunder", "krangkeng", "lohbener", "cantigi", 
            "pasekan", "lelea", "cikedung", "widasari", "bangodua", "tukdana", "arahan" 
        };

        // 3. Zona 3: Kab. Indramayu Barat & Selatan (Rp 25.000 - Radius ~25-50 km)
        private static readonly string[] Zona3Keywords = { 
            "terisi", "kroya", "gabuswetan", "bongas", "kandanghaur", 
            "losarang", "anjatan", "haurgeulis", "gantar", "patrol", "sukra" 
        };

        // 4. Zona 4: Kab. & Kota Cirebon (Rp 35.000 - Antar Kota Perbatasan)
        private static readonly string[] ZonaCirebonKeywords = { 
            "cirebon", "arjawinangun", "panguragan", "gegesik", "susukan", 
            "kaliwedi", "ciwaringin", "gempol", "palimanan", "klangenan", 
            "weru", "kedawung", "plumbon", "plered", "sumber", "mundu", 
            "gunungjati", "suranenggala", "kapetakan", "kota cirebon" 
        };

        // 5. Zona 5: Kab. Majalengka (Rp 40.000 - Antar Kota Perbatasan)
        private static readonly string[] ZonaMajalengkaKeywords = { 
            "majalengka", "kertajati", "jatitujuh", "ligung", "sumberjaya", 
            "dawuan", "jatiwangi", "kadipaten", "kasokandel" 
        };

        // 6. Zona 6: Kab. Subang Timur (Rp 50.000 - Batas Maksimal Jangkauan)
        private static readonly string[] ZonaSubangKeywords = { 
            "subang", "pamanukan", "pusakanagara", "pusakajaya", "legonkulon", 
            "ciasem", "blanakan", "patokbeusi" 
        };

        public static WilayahInfo CekWilayah(string? alamat)
        {
            if (string.IsNullOrWhiteSpace(alamat))
            {
                return new WilayahInfo
                {
                    IsCovered = false,
                    Ongkir = 0,
                    NamaZona = "Belum Diisi",
                    Keterangan = "Silakan masukkan alamat lengkap pengiriman untuk mendeteksi jangkauan dan biaya kirim.",
                    WilayahTerdeteksi = "-"
                };
            }

            string lower = alamat.ToLowerInvariant();

            // 1. Zona 1: Bebas Ongkir (Lokal Kertasemaya & Sekitarnya)
            foreach (var kw in Zona1Keywords)
            {
                if (lower.Contains(kw))
                {
                    return new WilayahInfo
                    {
                        IsCovered = true,
                        Ongkir = 0,
                        NamaZona = "Zona 1 - Lokal Dapur (Kertasemaya & Sekitar)",
                        Keterangan = "Gratis Ongkir (Area lokal dapur Catering Mimi Saripah)",
                        WilayahTerdeteksi = kw
                    };
                }
            }

            // 2. Zona 2: Kab. Indramayu Tengah & Timur (Rp 15.000)
            foreach (var kw in Zona2Keywords)
            {
                if (lower.Contains(kw))
                {
                    return new WilayahInfo
                    {
                        IsCovered = true,
                        Ongkir = 15000,
                        NamaZona = "Zona 2 - Kab. Indramayu (Tengah & Timur)",
                        Keterangan = "Rp 15.000 (Ongkir Pengantaran Wilayah Indramayu)",
                        WilayahTerdeteksi = kw
                    };
                }
            }

            // 3. Zona 3: Kab. Indramayu Barat & Selatan (Rp 25.000)
            foreach (var kw in Zona3Keywords)
            {
                if (lower.Contains(kw))
                {
                    return new WilayahInfo
                    {
                        IsCovered = true,
                        Ongkir = 25000,
                        NamaZona = "Zona 3 - Kab. Indramayu (Barat & Selatan)",
                        Keterangan = "Rp 25.000 (Ongkir Pengantaran Indramayu Barat / Selatan)",
                        WilayahTerdeteksi = kw
                    };
                }
            }

            // 4. Zona 4: Wilayah Cirebon (Rp 35.000)
            foreach (var kw in ZonaCirebonKeywords)
            {
                if (lower.Contains(kw))
                {
                    return new WilayahInfo
                    {
                        IsCovered = true,
                        Ongkir = 35000,
                        NamaZona = "Zona 4 - Wilayah Kab. / Kota Cirebon",
                        Keterangan = "Rp 35.000 (Ongkir Pengantaran Wilayah Cirebon)",
                        WilayahTerdeteksi = kw
                    };
                }
            }

            // 5. Zona 5: Wilayah Majalengka (Rp 40.000)
            foreach (var kw in ZonaMajalengkaKeywords)
            {
                if (lower.Contains(kw))
                {
                    return new WilayahInfo
                    {
                        IsCovered = true,
                        Ongkir = 40000,
                        NamaZona = "Zona 5 - Wilayah Kab. Majalengka",
                        Keterangan = "Rp 40.000 (Ongkir Pengantaran Wilayah Majalengka)",
                        WilayahTerdeteksi = kw
                    };
                }
            }

            // 6. Zona 6: Wilayah Subang Timur (Rp 50.000)
            foreach (var kw in ZonaSubangKeywords)
            {
                if (lower.Contains(kw))
                {
                    return new WilayahInfo
                    {
                        IsCovered = true,
                        Ongkir = 50000,
                        NamaZona = "Zona 6 - Wilayah Kab. Subang (Timur)",
                        Keterangan = "Rp 50.000 (Ongkir Pengantaran Batas Subang Timur)",
                        WilayahTerdeteksi = kw
                    };
                }
            }

            // 7. Di Luar Jangkauan (Batu Sangkar, Padang, Jakarta, Bandung, Surabaya, Luar Jawa, dll)
            return new WilayahInfo
            {
                IsCovered = false,
                Ongkir = 0,
                NamaZona = "Di Luar Jangkauan",
                Keterangan = "Mohon maaf, lokasi pengiriman di luar jangkauan katering Mimi Saripah (Maksimal pengantaran: Indramayu, Cirebon, Majalengka, dan Subang perbatasan).",
                WilayahTerdeteksi = "Luar Jangkauan"
            };
        }

        public static bool IsWilayahTerjangkau(string? alamat)
        {
            return CekWilayah(alamat).IsCovered;
        }

        public static decimal HitungOngkir(string? alamat)
        {
            var info = CekWilayah(alamat);
            return info.IsCovered ? info.Ongkir : 0;
        }

        public static string GetKeteranganOngkir(string? alamat)
        {
            return CekWilayah(alamat).Keterangan;
        }
    }
}
