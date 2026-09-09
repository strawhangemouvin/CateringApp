using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CateringApp.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "kategori_menu",
                columns: table => new
                {
                    kategori_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nama_kategori = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    deskripsi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_kategori_menu", x => x.kategori_id);
                });

            migrationBuilder.CreateTable(
                name: "peran",
                columns: table => new
                {
                    peran_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nama_peran = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_peran", x => x.peran_id);
                });

            migrationBuilder.CreateTable(
                name: "paket_menu",
                columns: table => new
                {
                    paket_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    kategori_id = table.Column<int>(type: "int", nullable: false),
                    nama_paket = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    harga = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    deskripsi_menu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_paket_menu", x => x.paket_id);
                    table.ForeignKey(
                        name: "FK_paket_menu_kategori_menu_kategori_id",
                        column: x => x.kategori_id,
                        principalTable: "kategori_menu",
                        principalColumn: "kategori_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "pengguna",
                columns: table => new
                {
                    pengguna_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    peran_id = table.Column<int>(type: "int", nullable: false),
                    nama_lengkap = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    username = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    password_hash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    nomor_telepon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    alamat = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pengguna", x => x.pengguna_id);
                    table.ForeignKey(
                        name: "FK_pengguna_peran_peran_id",
                        column: x => x.peran_id,
                        principalTable: "peran",
                        principalColumn: "peran_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "pesanan",
                columns: table => new
                {
                    pesanan_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    pengguna_id = table.Column<int>(type: "int", nullable: false),
                    nomor_pesanan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    tanggal_pesan = table.Column<DateTime>(type: "datetime2", nullable: false),
                    tanggal_pengiriman = table.Column<DateTime>(type: "datetime2", nullable: false),
                    alamat_pengiriman = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    total_bayar = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    status_pesanan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pesanan", x => x.pesanan_id);
                    table.ForeignKey(
                        name: "FK_pesanan_pengguna_pengguna_id",
                        column: x => x.pengguna_id,
                        principalTable: "pengguna",
                        principalColumn: "pengguna_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "detail_pesanan",
                columns: table => new
                {
                    detail_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    pesanan_id = table.Column<int>(type: "int", nullable: false),
                    paket_id = table.Column<int>(type: "int", nullable: false),
                    jumlah = table.Column<int>(type: "int", nullable: false),
                    harga_satuan = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    subtotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    catatan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_detail_pesanan", x => x.detail_id);
                    table.ForeignKey(
                        name: "FK_detail_pesanan_paket_menu_paket_id",
                        column: x => x.paket_id,
                        principalTable: "paket_menu",
                        principalColumn: "paket_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_detail_pesanan_pesanan_pesanan_id",
                        column: x => x.pesanan_id,
                        principalTable: "pesanan",
                        principalColumn: "pesanan_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "pembayaran",
                columns: table => new
                {
                    pembayaran_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    pesanan_id = table.Column<int>(type: "int", nullable: false),
                    metode_pembayaran = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    jumlah_bayar = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    tanggal_bayar = table.Column<DateTime>(type: "datetime2", nullable: false),
                    bukti_transfer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    status_verifikasi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pembayaran", x => x.pembayaran_id);
                    table.ForeignKey(
                        name: "FK_pembayaran_pesanan_pesanan_id",
                        column: x => x.pesanan_id,
                        principalTable: "pesanan",
                        principalColumn: "pesanan_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_detail_pesanan_paket_id",
                table: "detail_pesanan",
                column: "paket_id");

            migrationBuilder.CreateIndex(
                name: "IX_detail_pesanan_pesanan_id",
                table: "detail_pesanan",
                column: "pesanan_id");

            migrationBuilder.CreateIndex(
                name: "IX_paket_menu_kategori_id",
                table: "paket_menu",
                column: "kategori_id");

            migrationBuilder.CreateIndex(
                name: "IX_pembayaran_pesanan_id",
                table: "pembayaran",
                column: "pesanan_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_pengguna_peran_id",
                table: "pengguna",
                column: "peran_id");

            migrationBuilder.CreateIndex(
                name: "IX_pesanan_pengguna_id",
                table: "pesanan",
                column: "pengguna_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "detail_pesanan");

            migrationBuilder.DropTable(
                name: "pembayaran");

            migrationBuilder.DropTable(
                name: "paket_menu");

            migrationBuilder.DropTable(
                name: "pesanan");

            migrationBuilder.DropTable(
                name: "kategori_menu");

            migrationBuilder.DropTable(
                name: "pengguna");

            migrationBuilder.DropTable(
                name: "peran");
        }
    }
}
