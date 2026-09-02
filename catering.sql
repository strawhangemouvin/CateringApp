-- 1. Tutup koneksi aktif dan Hapus database lama jika sudah ada
USE master;
GO

IF EXISTS (SELECT name FROM sys.databases WHERE name = N'db_catering')
BEGIN
    ALTER DATABASE db_catering SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE db_catering;
END
GO

-- 2. Buat database baru
CREATE DATABASE db_catering;
GO
USE db_catering;
GO

-- 3. Buat Tabel-tabel Utama
-- Tabel Peran (Role)
CREATE TABLE peran (
    peran_id INT IDENTITY(1,1) PRIMARY KEY,
    nama_peran VARCHAR(50) NOT NULL
);

-- Tabel Pengguna (Soft Delete)
CREATE TABLE pengguna (
    pengguna_id INT IDENTITY(1,1) PRIMARY KEY,
    peran_id INT NOT NULL,
    nama_lengkap VARCHAR(100) NOT NULL,
    username VARCHAR(50) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    email VARCHAR(100) UNIQUE NOT NULL,
    nomor_telepon VARCHAR(20) NULL,
    alamat VARCHAR(255) NULL,
    created_at DATETIME DEFAULT GETDATE(),
    updated_at DATETIME DEFAULT GETDATE(),
    deleted_at DATETIME NULL,
    CONSTRAINT fk_pengguna_peran FOREIGN KEY (peran_id) REFERENCES peran(peran_id)
);

-- Tabel Kategori Menu
CREATE TABLE kategori_menu (
    kategori_id INT IDENTITY(1,1) PRIMARY KEY,
    nama_kategori VARCHAR(100) NOT NULL,
    deskripsi VARCHAR(255) NULL,
    created_at DATETIME DEFAULT GETDATE(),
    updated_at DATETIME DEFAULT GETDATE(),
    deleted_at DATETIME NULL
);

-- Tabel Paket Menu (Soft Delete)
CREATE TABLE paket_menu (
    paket_id INT IDENTITY(1,1) PRIMARY KEY,
    kategori_id INT NOT NULL,
    nama_paket VARCHAR(150) NOT NULL,
    harga DECIMAL(18,2) NOT NULL,
    deskripsi_menu VARCHAR(255) NULL,
    created_at DATETIME DEFAULT GETDATE(),
    updated_at DATETIME DEFAULT GETDATE(),
    deleted_at DATETIME NULL,
    CONSTRAINT fk_paket_kategori FOREIGN KEY (kategori_id) REFERENCES kategori_menu(kategori_id)
);

-- Tabel Pesanan
CREATE TABLE pesanan (
    pesanan_id INT IDENTITY(1,1) PRIMARY KEY,
    pengguna_id INT NOT NULL,
    nomor_pesanan VARCHAR(50) UNIQUE NOT NULL,
    tanggal_pesan DATETIME DEFAULT GETDATE(),
    tanggal_pengiriman DATE NOT NULL,
    alamat_pengiriman VARCHAR(255) NOT NULL,
    total_bayar DECIMAL(18,2) NOT NULL,
    status_pesanan VARCHAR(50) DEFAULT 'Pending',
    created_at DATETIME DEFAULT GETDATE(),
    updated_at DATETIME DEFAULT GETDATE(),
    deleted_at DATETIME NULL,
    CONSTRAINT fk_pesanan_pengguna FOREIGN KEY (pengguna_id) REFERENCES pengguna(pengguna_id)
);

-- Tabel Detail Pesanan
CREATE TABLE detail_pesanan (
    detail_id INT IDENTITY(1,1) PRIMARY KEY,
    pesanan_id INT NOT NULL,
    paket_id INT NOT NULL,
    jumlah INT NOT NULL,
    harga_satuan DECIMAL(18,2) NOT NULL,
    subtotal DECIMAL(18,2) NOT NULL,
    catatan VARCHAR(255) NULL,
    created_at DATETIME DEFAULT GETDATE(),
    updated_at DATETIME DEFAULT GETDATE(),
    deleted_at DATETIME NULL,
    CONSTRAINT fk_detail_pesanan FOREIGN KEY (pesanan_id) REFERENCES pesanan(pesanan_id),
    CONSTRAINT fk_detail_paket FOREIGN KEY (paket_id) REFERENCES paket_menu(paket_id)
);

-- Tabel Pembayaran (Relasi 1:1)
CREATE TABLE pembayaran (
    pembayaran_id INT IDENTITY(1,1) PRIMARY KEY,
    pesanan_id INT UNIQUE NOT NULL,
    metode_pembayaran VARCHAR(50) NOT NULL,
    jumlah_bayar DECIMAL(18,2) NOT NULL,
    tanggal_bayar DATETIME DEFAULT GETDATE(),
    bukti_transfer VARCHAR(255) NULL,
    status_verifikasi VARCHAR(50) DEFAULT 'Menunggu Verifikasi',
    created_at DATETIME DEFAULT GETDATE(),
    updated_at DATETIME DEFAULT GETDATE(),
    deleted_at DATETIME NULL,
    CONSTRAINT fk_pembayaran_pesanan FOREIGN KEY (pesanan_id) REFERENCES pesanan(pesanan_id)
);
GO

-- 4. Seeding Data
-- Seeding Peran
INSERT INTO peran (nama_peran) VALUES ('Admin'), ('Pelanggan');

-- Seeding 20 Pengguna
INSERT INTO pengguna (peran_id, nama_lengkap, username, password_hash, email, nomor_telepon, alamat) VALUES
(1, 'Administrator Catering', 'admin', 'admin123', 'admin@catering.com', '08110000000', 'Kantor Pusat Catering'),
(2, 'Budi Pratama', 'budi', 'user123', 'budi@gmail.com', '08120000001', 'Jl. Sukajadi No. 12'),
(2, 'Siti Rahmawati', 'siti', 'user123', 'siti@gmail.com', '08120000002', 'Jl. Dago No. 45'),
(2, 'Ahmad Fauzan', 'ahmad', 'user123', 'ahmad@gmail.com', '08120000003', 'Jl. Riau No. 88'),
(2, 'Dewi Lestari', 'dewi', 'user123', 'dewi@gmail.com', '08120000004', 'Jl. Buah Batu No. 102'),
(2, 'Eko Saputra', 'eko', 'user123', 'eko@gmail.com', '08120000005', 'Jl. Setiabudi No. 15'),
(2, 'Rina Marlina', 'rina', 'user123', 'rina@gmail.com', '08120000006', 'Jl. Cihampelas No. 33'),
(2, 'Hendra Wijaya', 'hendra', 'user123', 'hendra@gmail.com', '08120000007', 'Jl. Gatot Subroto No. 60'),
(2, 'Maya Anggraini', 'maya', 'user123', 'maya@gmail.com', '08120000008', 'Jl. Soekarno Hatta No. 200'),
(2, 'Reza Nugraha', 'reza', 'user123', 'reza@gmail.com', '08120000009', 'Jl. Antapani No. 71'),
(2, 'Indah Permata', 'indah', 'user123', 'indah@gmail.com', '08120000010', 'Jl. Kopo Permai No. 9'),
(2, 'Fajar Hidayat', 'fajar', 'user123', 'fajar@gmail.com', '08120000011', 'Jl. Cibaduyut No. 14'),
(2, 'Putri Wulandari', 'putri', 'user123', 'putri@gmail.com', '08120000012', 'Jl. Pasirkaliki No. 55'),
(2, 'Bambang Irawan', 'bambang', 'user123', 'bambang@gmail.com', '08120000013', 'Jl. Merdeka No. 3'),
(2, 'Gita Savitri', 'gita', 'user123', 'gita@gmail.com', '08120000014', 'Jl. Pajajaran No. 27'),
(2, 'Doni Wahyudi', 'doni', 'user123', 'doni@gmail.com', '08120000015', 'Jl. Surapati No. 90'),
(2, 'Tari Handayani', 'tari', 'user123', 'tari@gmail.com', '08120000016', 'Jl. Terusan Jakarta No. 18'),
(2, 'Ari Wibowo', 'ari', 'user123', 'ari@gmail.com', '08120000017', 'Jl. Asia Afrika No. 10'),
(2, 'Nurul Aini', 'nurul', 'user123', 'nurul@gmail.com', '08120000018', 'Jl. Pahlawan No. 41'),
(2, 'Yusuf Maulana', 'yusuf', 'user123', 'yusuf@gmail.com', '08120000019', 'Jl. Cikutra No. 82');

-- Seeding 20 Kategori Menu
INSERT INTO kategori_menu (nama_kategori, deskripsi) VALUES
('Nasi Kotak Harian', 'Paket hemat untuk perkantoran dan sekolah'),
('Nasi Kotak Premium', 'Paket lauk komplit premium'),
('Prasmanan Pernikahan', 'Menu prasmanan acara wedding'),
('Prasmanan Syukuran', 'Menu prasmanan acara keluarga'),
('Tumpeng Mini', 'Tumpeng porsi personal'),
('Tumpeng Besar', 'Tumpeng tampah porsi besar'),
('Snack Box Manis', 'Aneka kue manis dan pastry'),
('Snack Box Gurih', 'Aneka gorengan dan roti asin'),
('Coffee Break Standard', 'Paket kopi teh dan kudapan'),
('Coffee Break VIP', 'Paket premium espresso and cookies'),
('Healthy Diet Box', 'Menu rendah lemak dan kalori'),
('Vegetarian Package', 'Menu serba nabati'),
('Kids Bento Box', 'Menu karakter kesukaan anak'),
('Seafood Package', 'Olahan udang cumi dan ikan laut'),
('Western Menu', 'Steak pasta dan salad'),
('Traditional Menu', 'Khas masakan nusantara'),
('Aqiqah Package', 'Paket olahan kambing'),
('Corporate Lunch', 'Paket makan siang instansi'),
('Dessert Box', 'Aneka puding dan cake'),
('Drink Station', 'Aneka jus dan es buah segar');

-- Seeding 20 Paket Menu
INSERT INTO paket_menu (kategori_id, nama_paket, harga, deskripsi_menu) VALUES
(1, 'Nasi Kotak Ayam Bakar', 25000, 'Nasi, ayam bakar madu, sambal, tahu tempe'),
(1, 'Nasi Kotak Ayam Serundeng', 23000, 'Nasi, ayam serundeng, sambal terasi'),
(2, 'Nasi Kotak Rendang Sapi', 35000, 'Nasi, rendang daging sapi empuk, gulai'),
(2, 'Nasi Kotak Empal Daging', 38000, 'Nasi, empal manis, sambal goreng ati'),
(3, 'Prasmanan Gold Wedding', 85000, 'Sop buntut, rollade sapi, ayam mentega'),
(3, 'Prasmanan Silver Wedding', 70000, 'Sup kimlo, kakap asam manis, ayam rica'),
(4, 'Prasmanan Syukuran A', 55000, 'Tongseng sapi, ayam bakar, sambal kentang'),
(4, 'Prasmanan Syukuran B', 50000, 'Nasi kuning, ayam serundeng, orek tempe'),
(5, 'Tumpeng Mini Kuning', 30000, 'Nasi kuning, ayam suwir, perkedel, abon'),
(5, 'Tumpeng Mini Uduk', 30000, 'Nasi uduk gurih, semur telur, bihun'),
(6, 'Tumpeng Jumbo 25 Porsi', 750000, 'Tumpeng tampah 7 macam lauk komplit'),
(7, 'Snack Box Manis A', 15000, 'Kue sus, bolu gulung, pie susu'),
(8, 'Snack Box Gurih B', 15000, 'Risoles mayo, lemper ayam, pastel'),
(9, 'Coffee Break Seminar', 25000, 'Kopi, teh, lumpia semarang, lapis legit'),
(10, 'Coffee Break VIP', 45000, 'Espresso, teh twg, croissant, cheesecake'),
(11, 'Diet Box Dada Ayam', 45000, 'Dada ayam panggang, nasi merah, brokoli'),
(12, 'Vegetarian Tofu Box', 35000, 'Steak tahu jamur, buncis krispi, salad'),
(13, 'Bento Kids Bento Fun', 28000, 'Nasi panda, nugget ayam, sosis goreng'),
(14, 'Seafood Platter Mini', 48000, 'Cumi tepung, udang saus padang, kangkung'),
(15, 'Western Sirloin Box', 55000, 'Sirloin steak, potato wedges, salad');

-- Seeding 20 Pesanan
INSERT INTO pesanan (pengguna_id, nomor_pesanan, tanggal_pesan, tanggal_pengiriman, alamat_pengiriman, total_bayar, status_pesanan) VALUES
(2, 'ORD-20260801-001', '2026-08-01 08:00:00', '2026-08-05', 'Jl. Sukajadi No. 12', 1250000, 'Selesai'),
(3, 'ORD-20260802-002', '2026-08-02 09:00:00', '2026-08-06', 'Jl. Dago No. 45', 7000000, 'Selesai'),
(4, 'ORD-20260803-003', '2026-08-03 10:00:00', '2026-08-07', 'Jl. Riau No. 88', 750000, 'Selesai'),
(5, 'ORD-20260804-004', '2026-08-04 11:00:00', '2026-08-08', 'Jl. Buah Batu No. 102', 1750000, 'Selesai'),
(6, 'ORD-20260805-005', '2026-08-05 12:00:00', '2026-08-09', 'Jl. Setiabudi No. 15', 8500000, 'Selesai'),
(7, 'ORD-20260806-006', '2026-08-06 13:00:00', '2026-08-10', 'Jl. Cihampelas No. 33', 450000, 'Selesai'),
(8, 'ORD-20260807-007', '2026-08-07 14:00:00', '2026-08-11', 'Jl. Gatot Subroto No. 60', 1500000, 'Selesai'),
(9, 'ORD-20260808-008', '2026-08-08 15:00:00', '2026-08-12', 'Jl. Soekarno Hatta No. 200', 900000, 'Selesai'),
(10, 'ORD-20260809-009', '2026-08-09 16:00:00', '2026-08-13', 'Jl. Antapani No. 71', 2750000, 'Selesai'),
(11, 'ORD-20260810-010', '2026-08-10 08:30:00', '2026-08-14', 'Jl. Kopo Permai No. 9', 1500000, 'Selesai'),
(12, 'ORD-20260811-011', '2026-08-11 09:30:00', '2026-08-15', 'Jl. Cibaduyut No. 14', 600000, 'Diproses'),
(13, 'ORD-20260812-012', '2026-08-12 10:30:00', '2026-08-16', 'Jl. Pasirkaliki No. 55', 1150000, 'Diproses'),
(14, 'ORD-20260813-013', '2026-08-13 11:30:00', '2026-08-17', 'Jl. Merdeka No. 3', 700000, 'Diproses'),
(15, 'ORD-20260814-014', '2026-08-14 12:30:00', '2026-08-18', 'Jl. Pajajaran No. 27', 560000, 'Diproses'),
(16, 'ORD-20260815-015', '2026-08-15 13:30:00', '2026-08-19', 'Jl. Surapati No. 90', 960000, 'Pending'),
(17, 'ORD-20260816-016', '2026-08-16 14:30:00', '2026-08-20', 'Jl. Terusan Jakarta No. 18', 1100000, 'Pending'),
(18, 'ORD-20260817-017', '2026-08-17 15:30:00', '2026-08-21', 'Jl. Asia Afrika No. 10', 750000, 'Pending'),
(19, 'ORD-20260818-018', '2026-08-18 16:30:00', '2026-08-22', 'Jl. Pahlawan No. 41', 2500000, 'Pending'),
(20, 'ORD-20260819-019', '2026-08-19 17:00:00', '2026-08-23', 'Jl. Cikutra No. 82', 690000, 'Batal'),
(2, 'ORD-20260820-020', '2026-08-20 08:15:00', '2026-08-24', 'Jl. Sukajadi No. 12', 1750000, 'Pending');

-- Seeding 20 Detail Pesanan
INSERT INTO detail_pesanan (pesanan_id, paket_id, jumlah, harga_satuan, subtotal, catatan) VALUES
(1, 1, 50, 25000, 1250000, 'Sambal dipisah'),
(2, 6, 100, 70000, 7000000, 'Set perlengkapan prasmanan'),
(3, 11, 1, 750000, 750000, 'Tulisan selamat milad'),
(4, 3, 50, 35000, 1750000, 'Rendang pedas sedang'),
(5, 5, 100, 85000, 8500000, 'Acara siang'),
(6, 12, 30, 15000, 450000, 'Kue basah pagi'),
(7, 9, 50, 30000, 1500000, 'Mika kemasan tertutup'),
(8, 14, 60, 15000, 900000, 'Snack meeting'),
(9, 7, 50, 55000, 2750000, 'Tongseng tanpa jeroan'),
(10, 10, 50, 30000, 1500000, 'Sendok garpu higienis'),
(11, 2, 26, 23000, 598000, 'Kirim jam 11.30'),
(12, 4, 30, 38000, 1140000, 'Kuah sop dipisah'),
(13, 16, 20, 35000, 700000, 'Menu vegetarian murni'),
(14, 18, 20, 28000, 560000, 'Paket bento TK'),
(15, 19, 20, 48000, 960000, 'Udang tanpa kulit'),
(16, 20, 20, 55000, 1100000, 'Tingkat matang medium well'),
(17, 11, 1, 750000, 750000, 'Ulang tahun instansi'),
(18, 8, 50, 50000, 2500000, 'Nasi kuning wangi'),
(19, 2, 30, 23000, 690000, 'Pesanan dibatalkan'),
(20, 16, 50, 35000, 1750000, 'Rendah karbo');

-- Seeding 20 Pembayaran
INSERT INTO pembayaran (pesanan_id, metode_pembayaran, jumlah_bayar, tanggal_bayar, bukti_transfer, status_verifikasi) VALUES
(1, 'Transfer BCA', 1250000, '2026-08-01 09:00:00', '/Uploads/bukti1.jpg', 'Valid'),
(2, 'Transfer Mandiri', 7000000, '2026-08-02 10:00:00', '/Uploads/bukti2.jpg', 'Valid'),
(3, 'QRIS', 750000, '2026-08-03 10:30:00', '/Uploads/bukti3.jpg', 'Valid'),
(4, 'Transfer BNI', 1750000, '2026-08-04 12:00:00', '/Uploads/bukti4.jpg', 'Valid'),
(5, 'Transfer BCA', 8500000, '2026-08-05 14:00:00', '/Uploads/bukti5.jpg', 'Valid'),
(6, 'QRIS', 450000, '2026-08-06 14:30:00', '/Uploads/bukti6.jpg', 'Valid'),
(7, 'Transfer BRI', 1500000, '2026-08-07 16:00:00', '/Uploads/bukti7.jpg', 'Valid'),
(8, 'QRIS', 900000, '2026-08-08 08:30:00', '/Uploads/bukti8.jpg', 'Valid'),
(9, 'Transfer BCA', 2750000, '2026-08-09 10:00:00', '/Uploads/bukti9.jpg', 'Valid'),
(10, 'QRIS', 1500000, '2026-08-10 11:15:00', '/Uploads/bukti10.jpg', 'Valid'),
(11, 'Transfer Mandiri', 600000, '2026-08-11 11:30:00', '/Uploads/bukti11.jpg', 'Valid'),
(12, 'QRIS', 1150000, '2026-08-12 13:30:00', '/Uploads/bukti12.jpg', 'Valid'),
(13, 'Transfer BCA', 700000, '2026-08-13 15:00:00', '/Uploads/bukti13.jpg', 'Valid'),
(14, 'Transfer BNI', 560000, '2026-08-14 15:30:00', '/Uploads/bukti14.jpg', 'Valid'),
(15, 'QRIS', 960000, '2026-08-15 16:30:00', '/Uploads/bukti15.jpg', 'Menunggu Verifikasi'),
(16, 'Transfer BRI', 1100000, '2026-08-16 10:00:00', '/Uploads/bukti16.jpg', 'Menunggu Verifikasi'),
(17, 'Transfer BCA', 750000, '2026-08-17 11:00:00', '/Uploads/bukti17.jpg', 'Menunggu Verifikasi'),
(18, 'QRIS', 2500000, '2026-08-18 12:15:00', '/Uploads/bukti18.jpg', 'Menunggu Verifikasi'),
(19, 'Transfer BCA', 690000, '2026-08-19 14:30:00', '/Uploads/bukti19.jpg', 'Ditolak'),
(20, 'COD', 1750000, '2026-08-20 15:45:00', NULL, 'Menunggu Verifikasi');
GO