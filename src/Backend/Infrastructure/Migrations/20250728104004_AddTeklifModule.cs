using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTeklifModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Teklifler",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TeklifNumarasi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MusteriId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TeklifTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GecerlilikTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ToplamTutar = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Durum = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teklifler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Teklifler_Customers_MusteriId",
                        column: x => x.MusteriId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeklifSatirlari",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TeklifId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UrunId = table.Column<int>(type: "int", nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Miktar = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BirimFiyat = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Toplam = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeklifSatirlari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeklifSatirlari_Products_UrunId",
                        column: x => x.UrunId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeklifSatirlari_Teklifler_TeklifId",
                        column: x => x.TeklifId,
                        principalTable: "Teklifler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Teklifler_MusteriId",
                table: "Teklifler",
                column: "MusteriId");

            migrationBuilder.CreateIndex(
                name: "IX_TeklifSatirlari_TeklifId",
                table: "TeklifSatirlari",
                column: "TeklifId");

            migrationBuilder.CreateIndex(
                name: "IX_TeklifSatirlari_UrunId",
                table: "TeklifSatirlari",
                column: "UrunId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TeklifSatirlari");

            migrationBuilder.DropTable(
                name: "Teklifler");
        }
    }
}
