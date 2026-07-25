using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TurnovoFarmer.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedInitialData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name", "Slug" },
                values: new object[,]
                {
                    { 1, "Зеленчуци", "zelenchutsi" },
                    { 2, "Плодове", "plodove" },
                    { 3, "Пчелни продукти", "pchelni-produkti" }
                });

            migrationBuilder.InsertData(
                table: "DeliveryZones",
                columns: new[] { "Id", "DeliveryDay", "MinWeightRequired", "PickupLocationDescription", "ZoneName" },
                values: new object[,]
                {
                    { 1, "Сряда", 30.00m, "Доставка до личен адрес в рамките на града.", "Велико Търново (Градска зона)" },
                    { 2, "Събота", 50.00m, "Доставка до адрес за Г. Оряховица и Лясковец.", "Горна Оряховица и Лясковец" }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "CategoryId", "DeletedAt", "Description", "ImageUrl", "IsActive", "Name", "PricePerUnit", "StockQuantity", "UnitType" },
                values: new object[,]
                {
                    { 1, 1, null, "Сладък градински домат, откъснат в деня на доставката.", "/images/products/default.jpg", true, "Розов домат (био)", 4.50m, 100m, "kg" },
                    { 2, 1, null, "Ранни картофи, перфектни за печене.", "/images/products/default.jpg", true, "Пресни картофи", 2.20m, 250m, "kg" },
                    { 3, 3, null, "Буркан натурален пчелен мед от региона на В. Търново.", "/images/products/default.jpg", true, "Домашен мед от букет", 12.00m, 40m, "jar" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "DeliveryZones",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "DeliveryZones",
                keyColumn: "Id",
                keyValue: 2);

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
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3);
        }
    }
}
