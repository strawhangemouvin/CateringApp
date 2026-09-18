USE db_catering;
GO

BEGIN TRANSACTION;

-- 1. Perbarui Username dan Alamat Pelanggan di Tabel pengguna
UPDATE pengguna SET username = 'ahmad_fauzi' WHERE pengguna_id = 4;
UPDATE pengguna SET username = 'dewi_lestari' WHERE pengguna_id = 5;
UPDATE pengguna SET username = 'rizky_ramadhan' WHERE pengguna_id = 6;
UPDATE pengguna SET username = 'siti_nurhaliza' WHERE pengguna_id = 7;
UPDATE pengguna SET username = 'hendra_setiawan' WHERE pengguna_id = 8;
UPDATE pengguna SET username = 'maya_anggraini' WHERE pengguna_id = 9;
UPDATE pengguna SET username = 'fajar_pratama' WHERE pengguna_id = 10;
UPDATE pengguna SET username = 'anisa_rahmawati' WHERE pengguna_id = 11;
UPDATE pengguna SET username = 'dimas_wahyudi' WHERE pengguna_id = 12;
UPDATE pengguna SET username = 'tri_handayani' WHERE pengguna_id = 13;
UPDATE pengguna SET username = 'bayu_kurniawan' WHERE pengguna_id = 14;
UPDATE pengguna SET username = 'putri_wulandari' WHERE pengguna_id = 15;
UPDATE pengguna SET username = 'eko_prasetyo' WHERE pengguna_id = 16;
UPDATE pengguna SET username = 'ratna_sari' WHERE pengguna_id = 17;
UPDATE pengguna SET username = 'arif_hidayat' WHERE pengguna_id = 18;
UPDATE pengguna SET username = 'mega_permata' WHERE pengguna_id = 19;
UPDATE pengguna SET username = 'bagus_saputra' WHERE pengguna_id = 20;
UPDATE pengguna SET username = 'nabila_monica' WHERE pengguna_id = 23;
UPDATE pengguna SET username = 'nabila_staff' WHERE pengguna_id = 26;

-- Normalisasi alamat pengguna yang sebelumnya luar jangkauan (Bandung / Batu Sangkar)
UPDATE pengguna SET alamat = 'Jl. Siliwangi No. 12, Kec. Indramayu, Kab. Indramayu' WHERE pengguna_id = 3;
UPDATE pengguna SET alamat = 'Desa Tenajar Lor RT 04/RW 02, Kec. Kertasemaya, Kab. Indramayu' WHERE pengguna_id IN (22, 23, 26);
UPDATE pengguna SET alamat = 'Desa Tenajar Lor RT 02/RW 01, Kec. Kertasemaya, Kab. Indramayu' WHERE pengguna_id = 25;

-- 2. Perbarui Gambar dan Nama Paket Menu di Tabel paket_menu
UPDATE paket_menu 
SET nama_paket = 'Nasi Kotak Ayam Bakar Spesial', 
    gambar = '/uploads/menu/nasi_kotak_ayam_bakar.jpg' 
WHERE paket_id = 1;

UPDATE paket_menu 
SET gambar = '/uploads/menu/snack_box_rapat.jpg' 
WHERE paket_id = 3;

UPDATE paket_menu 
SET gambar = '/uploads/menu/tumpeng_kuning_komplit.jpg' 
WHERE paket_id = 4;

UPDATE paket_menu 
SET gambar = '/uploads/menu/nasi_kebuli_kambing.jpg' 
WHERE paket_id = 5;

UPDATE paket_menu 
SET gambar = '/uploads/menu/sate_ayam_madura.jpg' 
WHERE paket_id = 9;

UPDATE paket_menu 
SET gambar = '/uploads/menu/nasi_kotak_ayam_lengkuas.jpg' 
WHERE paket_id = 10;

UPDATE paket_menu 
SET gambar = '/uploads/menu/es_campur_nusantara.jpg' 
WHERE paket_id = 11;

UPDATE paket_menu 
SET gambar = '/uploads/menu/snack_box_tradisional.jpg' 
WHERE paket_id = 12;

UPDATE paket_menu 
SET gambar = '/uploads/menu/puding_sutra_mangga.jpg' 
WHERE paket_id = 13;

UPDATE paket_menu 
SET gambar = '/uploads/menu/bolu_gulung_red_velvet.jpg' 
WHERE paket_id = 14;

UPDATE paket_menu 
SET gambar = '/uploads/menu/ikan_nila_bakar.jpg' 
WHERE paket_id = 15;

UPDATE paket_menu 
SET gambar = '/uploads/menu/soto_betawi_daging.jpg' 
WHERE paket_id = 16;

UPDATE paket_menu 
SET gambar = '/uploads/menu/tumpeng_mini_selamatan.jpg' 
WHERE paket_id = 18;

-- Hapus paket menu sampah/dummy tes yang tidak valid (seperti seblak minus harga, ab76i)
DELETE FROM paket_menu WHERE paket_id >= 22;

-- 3. Perbarui Alamat Pengiriman di Tabel pesanan
-- Sinkronkan alamat pesanan 1-20 (Jl. Mawar Merah Block C) dengan alamat riil pengguna di Indramayu/Cirebon
UPDATE p
SET p.alamat_pengiriman = u.alamat
FROM pesanan p
JOIN pengguna u ON p.pengguna_id = u.pengguna_id
WHERE p.alamat_pengiriman LIKE 'Jl. Mawar Merah%';

-- Perbaiki pesanan tes yang beralamat di luar jangkauan (Aceh, Sabang, Garut, Bandung, Batu Sangkar, Pekalongan, Cikarang)
UPDATE pesanan SET alamat_pengiriman = 'Desa Tenajar Lor RT 01/RW 01, Kec. Kertasemaya, Kab. Indramayu' WHERE pesanan_id = 22;
UPDATE pesanan SET alamat_pengiriman = 'Jl. Raya Kertasemaya No. 15, Kertasemaya, Kab. Indramayu' WHERE pesanan_id = 23;
UPDATE pesanan SET alamat_pengiriman = 'Desa Tulungagung RT 03/RW 02, Kec. Kertasemaya, Kab. Indramayu' WHERE pesanan_id = 24;
UPDATE pesanan SET alamat_pengiriman = 'Jl. Siliwangi No. 18, Kec. Indramayu, Kab. Indramayu' WHERE pesanan_id = 25;
UPDATE pesanan SET alamat_pengiriman = 'Desa Tenajar Kidul RT 02/RW 01, Kec. Kertasemaya, Kab. Indramayu' WHERE pesanan_id = 26;
UPDATE pesanan SET alamat_pengiriman = 'Jl. Gatot Subroto No. 50, Jatibarang, Kab. Indramayu' WHERE pesanan_id = 27;
UPDATE pesanan SET alamat_pengiriman = 'Desa Cadangpinggan RT 01/RW 03, Sukagumiwang, Kab. Indramayu' WHERE pesanan_id = 29;
UPDATE pesanan SET alamat_pengiriman = 'Desa Tenajar Lor Blok D, Kec. Kertasemaya, Kab. Indramayu' WHERE pesanan_id = 30;

COMMIT TRANSACTION;
GO
