using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FreightSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CustomerSeedAndApiSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Customers",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.InsertData(
                table: "Customers",
                columns: new[] { "Id", "Address", "Balance", "CreatedAt", "CreditLimit", "Email", "Name", "Phone" },
                values: new object[,]
                {
                    { 1, "Cairo", 0m, new DateTime(2026, 4, 1, 13, 22, 48, 861, DateTimeKind.Utc).AddTicks(3325), 50000m, "samah@logistics.com", "Samah Logistics", "+201000000001" },
                    { 2, "Alexandria", 10000m, new DateTime(2026, 4, 1, 13, 22, 48, 861, DateTimeKind.Utc).AddTicks(3731), 100000m, "contact@nile-import.com", "Nile Importers", "+201000000002" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Email",
                table: "Customers",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Customers_Email",
                table: "Customers");

            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
