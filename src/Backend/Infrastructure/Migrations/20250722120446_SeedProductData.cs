using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedProductData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Description", "IsActive", "Name", "Price", "SKU", "StockQuantity" },
                values: new object[,]
                {
                    { 1, "Yüksek performanslı dizüstü bilgisayar", true, "Laptop Pro X1", 32000m, "LP-PRO-X1", 50 },
                    { 2, "Ergonomik ve hassas optik mouse", true, "Kablosuz Mouse", 850m, "MS-WL-001", 200 },
                    { 3, "RGB aydınlatmalı, oyuncular için", true, "Mekanik Klavye", 2500m, "KB-MECH-RGB", 150 },
                    { 4, "Canlı renkler ve net görüntüler", true, "4K Monitör 27\"", 8999m, "MON-4K-27", 80 },
                    { 5, "8-in-1 bağlantı noktası adaptörü", true, "USB-C Hub", 1200m, "HUB-USBC-81", 300 },
                    { 6, "Hızlı veri transferi için taşınabilir SSD", true, "Harici SSD 1TB", 3500m, "SSD-EXT-1TB", 120 },
                    { 7, "Görüntülü görüşmeler için Full HD kamera", false, "Webcam 1080p", 1500m, "WC-FHD-01", 180 },
                    { 8, "7.1 Surround sesli oyuncu kulaklığı", true, "Gaming Headset", 2800m, "HS-GM-71", 90 },
                    { 9, "Fitness takibi ve bildirimler", true, "Akıllı Saat SE", 6500m, "SW-SE-01", 250 },
                    { 10, "Eğlence ve iş için ideal tablet", true, "Tablet 10\"", 7800m, "TAB-10-STD", 110 },
                    { 11, "Modüler ve verimli PSU", false, "Güç Kaynağı 750W", 2100m, "PSU-750W-MD", 70 },
                    { 12, "Yeni nesil oyun ve grafik performansı", true, "Ekran Kartı RTX 4070", 25000m, "GPU-RTX-4070", 40 },
                    { 13, "Yüksek hızlı bellek kiti (2x8GB)", true, "RAM 16GB DDR5", 1800m, "RAM-16-DDR5", 400 },
                    { 14, "Sessiz ve etkili işlemci soğutucusu", true, "Soğutucu Fan", 950m, "FAN-CPU-SLNT", 220 },
                    { 15, "En yeni işlemciler için ATX anakart", true, "Anakart Z790", 9200m, "MB-Z790-ATX", 60 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 15);
        }
    }
}
