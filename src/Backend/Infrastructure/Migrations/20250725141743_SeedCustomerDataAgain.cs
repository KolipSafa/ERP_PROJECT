using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedCustomerDataAgain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Customers",
                columns: new[] { "Id", "Address", "Balance", "CompanyName", "CreatedDate", "Email", "FirstName", "IsActive", "LastName", "PhoneNumber", "TaxNumber", "UpdatedDate" },
                values: new object[,]
                {
                    { new Guid("0bf4ad86-626c-4bfc-2424-08ddcb81e062"), null, 25000m, "Aydın Tekstil", new DateTime(2025, 6, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "huseyin.aydin@example.com", "Hüseyin", true, "Aydın", "05547890123", null, null },
                    { new Guid("1cf5be87-737d-4c0d-2525-08ddcb81e063"), null, 0m, "Şahin Market", new DateTime(2025, 5, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), "elif.sahin@example.com", "Elif", false, "Şahin", "05368901234", null, null },
                    { new Guid("2df6cf88-848e-4d1e-2626-08ddcb81e064"), null, 7850.00m, "Koç Otomotiv", new DateTime(2025, 7, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "ibrahim.koc@example.com", "İbrahim", true, "Koç", "05469012345", null, null },
                    { new Guid("3ef7df89-959f-4e2f-2727-08ddcb81e065"), null, 1250.50m, "Yıldız Mobilya", new DateTime(2025, 7, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "meryem.yildiz@example.com", "Meryem", true, "Yıldız", "05370123456", null, null },
                    { new Guid("4f08e08a-a6b0-4f30-2828-08ddcb81e066"), null, 0m, "Can Bilişim", new DateTime(2025, 7, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), "ali.can@example.com", "Ali", true, "Can", "05531234567", null, null },
                    { new Guid("5f19f18b-b7c1-4041-2929-08ddcb81e067"), null, 9900m, "Doğan Medya", new DateTime(2025, 7, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), "sultan.dogan@example.com", "Sultan", true, "Doğan", "05392345678", null, null },
                    { new Guid("6f2a028c-c8d2-4152-3030-08ddcb81e068"), null, 4500m, "Kurt Güvenlik", new DateTime(2025, 4, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), "osman.kurt@example.com", "Osman", false, "Kurt", "05493456789", null, null },
                    { new Guid("7f3b138d-d9e3-4263-3131-08ddcb81e069"), null, 500000m, "Polat Holding", new DateTime(2025, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "hatice.polat@example.com", "Hatice", true, "Polat", "05314567890", null, null },
                    { new Guid("857f3a2c-4ab7-4ffa-9855-08ddcabefce4"), null, 15250.75m, "Yılmaz İnşaat", new DateTime(2025, 6, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "ahmet.yilmaz@example.com", "Ahmet", true, "Yılmaz", "05321234567", null, null },
                    { new Guid("8f4c248e-eaf4-4374-3232-08ddcb81e06a"), null, 0m, "Güneş Enerji", new DateTime(2025, 7, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "yusuf.gunes@example.com", "Yusuf", true, "Güneş", "05515678901", null, null },
                    { new Guid("9f5d358f-fbe5-4485-3333-08ddcb81e06b"), null, 18000m, "Bulut Yazılım", new DateTime(2025, 7, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), "emine.bulut@example.com", "Emine", true, "Bulut", "05346789012", null, null },
                    { new Guid("af6e4690-0cf6-4596-3434-08ddcb81e06c"), null, 75000m, "Özdemir Metal", new DateTime(2025, 7, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "murat.ozdemir@example.com", "Murat", true, "Özdemir", "05437890123", null, null },
                    { new Guid("b6a95892-1d17-46f7-1919-08ddcb81e05d"), null, 0m, "Kaya Gıda Ltd.", new DateTime(2025, 6, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "ayse.kaya@example.com", "Ayşe", true, "Kaya", "05422345678", null, null },
                    { new Guid("bf7f5791-1da7-46a7-3535-08ddcb81e06d"), null, 0m, "Aksoy Kozmetik", new DateTime(2025, 3, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), "sibel.aksoy@example.com", "Sibel", false, "Aksoy", "05368901234", null, null },
                    { new Guid("c7b06982-2e28-47f8-2020-08ddcb81e05e"), null, 8500m, "Demir Teknoloji", new DateTime(2025, 6, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "mehmet.demir@example.com", "Mehmet", false, "Demir", "05553456789", null, null },
                    { new Guid("cf806892-2eb8-47b8-3636-08ddcb81e06e"), null, 2500.50m, "Çetin Emlak", new DateTime(2025, 7, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "ramazan.cetin@example.com", "Ramazan", true, "Çetin", "05529012345", null, null },
                    { new Guid("d8c17a83-3f39-48f9-2121-08ddcb81e05f"), null, 120000.50m, "Çelik Sanayi A.Ş.", new DateTime(2025, 7, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "fatma.celik@example.com", "Fatma", true, "Çelik", "05334567890", null, null },
                    { new Guid("df917993-3fc9-48c9-3737-08ddcb81e06f"), null, 3200m, "Taş Mermer", new DateTime(2025, 7, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "yasemin.tas@example.com", "Yasemin", true, "Taş", "05380123456", null, null },
                    { new Guid("e9d28b84-404a-49fa-2222-08ddcb81e060"), null, 0m, "Arslan Lojistik", new DateTime(2025, 7, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "mustafa.arslan@example.com", "Mustafa", true, "Arslan", "05445678901", null, null },
                    { new Guid("fae39c85-515b-4afb-2323-08ddcb81e061"), null, 500.25m, "Öztürk Danışmanlık", new DateTime(2025, 7, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "zeynep.ozturk@example.com", "Zeynep", true, "Öztürk", "05356789012", null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: new Guid("0bf4ad86-626c-4bfc-2424-08ddcb81e062"));

            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: new Guid("1cf5be87-737d-4c0d-2525-08ddcb81e063"));

            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: new Guid("2df6cf88-848e-4d1e-2626-08ddcb81e064"));

            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: new Guid("3ef7df89-959f-4e2f-2727-08ddcb81e065"));

            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: new Guid("4f08e08a-a6b0-4f30-2828-08ddcb81e066"));

            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: new Guid("5f19f18b-b7c1-4041-2929-08ddcb81e067"));

            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: new Guid("6f2a028c-c8d2-4152-3030-08ddcb81e068"));

            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: new Guid("7f3b138d-d9e3-4263-3131-08ddcb81e069"));

            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: new Guid("857f3a2c-4ab7-4ffa-9855-08ddcabefce4"));

            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: new Guid("8f4c248e-eaf4-4374-3232-08ddcb81e06a"));

            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: new Guid("9f5d358f-fbe5-4485-3333-08ddcb81e06b"));

            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: new Guid("af6e4690-0cf6-4596-3434-08ddcb81e06c"));

            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: new Guid("b6a95892-1d17-46f7-1919-08ddcb81e05d"));

            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: new Guid("bf7f5791-1da7-46a7-3535-08ddcb81e06d"));

            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: new Guid("c7b06982-2e28-47f8-2020-08ddcb81e05e"));

            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: new Guid("cf806892-2eb8-47b8-3636-08ddcb81e06e"));

            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: new Guid("d8c17a83-3f39-48f9-2121-08ddcb81e05f"));

            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: new Guid("df917993-3fc9-48c9-3737-08ddcb81e06f"));

            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: new Guid("e9d28b84-404a-49fa-2222-08ddcb81e060"));

            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: new Guid("fae39c85-515b-4afb-2323-08ddcb81e061"));
        }
    }
}
