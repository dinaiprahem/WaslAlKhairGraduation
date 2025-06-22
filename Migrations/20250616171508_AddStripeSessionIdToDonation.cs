using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WaslAlkhair.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddStripeSessionIdToDonation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AssistanceTypes",
                keyColumn: "Id",
                keyValue: new Guid("0209baca-30c1-427e-8636-791a92aded76"));

            migrationBuilder.DeleteData(
                table: "AssistanceTypes",
                keyColumn: "Id",
                keyValue: new Guid("33f77767-89d9-4f69-9df6-3f46acdb48ad"));

            migrationBuilder.DeleteData(
                table: "AssistanceTypes",
                keyColumn: "Id",
                keyValue: new Guid("6014379f-cfb6-40ea-9d07-e9735b626acb"));

            migrationBuilder.DeleteData(
                table: "AssistanceTypes",
                keyColumn: "Id",
                keyValue: new Guid("7e1c9876-2419-44fe-b210-9a091dfaa252"));

            migrationBuilder.DeleteData(
                table: "AssistanceTypes",
                keyColumn: "Id",
                keyValue: new Guid("8165d8e4-6a11-4750-b62a-373abab3ce6b"));

            migrationBuilder.DeleteData(
                table: "AssistanceTypes",
                keyColumn: "Id",
                keyValue: new Guid("b293f142-04cc-410f-98f8-62260f82ef24"));

            migrationBuilder.DeleteData(
                table: "AssistanceTypes",
                keyColumn: "Id",
                keyValue: new Guid("b80308fb-f7d4-4b43-b477-b1963d21a7fa"));

            migrationBuilder.DeleteData(
                table: "AssistanceTypes",
                keyColumn: "Id",
                keyValue: new Guid("b8142ce4-d29a-44d3-846e-c419821bdc04"));

            migrationBuilder.DeleteData(
                table: "AssistanceTypes",
                keyColumn: "Id",
                keyValue: new Guid("f3a89476-61c0-40b1-aa99-2b198fb97b9f"));

            migrationBuilder.AddColumn<string>(
                name: "StripeSessionId",
                table: "Donations",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.InsertData(
                table: "AssistanceTypes",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("1dfc92c4-178a-49fb-9a8a-54b8de07a264"), "بيطرية" },
                    { new Guid("1e5422c0-04a8-49d1-9bbc-f0eac0b5dc78"), "بيئية" },
                    { new Guid("832999d0-99d7-445d-b813-069d1e3da593"), "سكنية" },
                    { new Guid("b470c6df-f1d5-453e-94cf-f2faccb6a821"), "طبية" },
                    { new Guid("b4734a6d-0741-4be3-a039-8b01777a8237"), "تعليمية" },
                    { new Guid("b6800d02-6994-48a0-9da0-fcb4bdc8aa9b"), "غذائية" },
                    { new Guid("b72b3271-3ed8-4be9-ac20-30a8e747b304"), "طارئة وإغاثية" },
                    { new Guid("e4f20038-5f10-41c7-a1b9-dc1fcf264ec1"), "مالية" },
                    { new Guid("f05687cf-ea13-45ed-8916-1e577904cda1"), "ذوي الاحتياجات الخاصة" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AssistanceTypes",
                keyColumn: "Id",
                keyValue: new Guid("1dfc92c4-178a-49fb-9a8a-54b8de07a264"));

            migrationBuilder.DeleteData(
                table: "AssistanceTypes",
                keyColumn: "Id",
                keyValue: new Guid("1e5422c0-04a8-49d1-9bbc-f0eac0b5dc78"));

            migrationBuilder.DeleteData(
                table: "AssistanceTypes",
                keyColumn: "Id",
                keyValue: new Guid("832999d0-99d7-445d-b813-069d1e3da593"));

            migrationBuilder.DeleteData(
                table: "AssistanceTypes",
                keyColumn: "Id",
                keyValue: new Guid("b470c6df-f1d5-453e-94cf-f2faccb6a821"));

            migrationBuilder.DeleteData(
                table: "AssistanceTypes",
                keyColumn: "Id",
                keyValue: new Guid("b4734a6d-0741-4be3-a039-8b01777a8237"));

            migrationBuilder.DeleteData(
                table: "AssistanceTypes",
                keyColumn: "Id",
                keyValue: new Guid("b6800d02-6994-48a0-9da0-fcb4bdc8aa9b"));

            migrationBuilder.DeleteData(
                table: "AssistanceTypes",
                keyColumn: "Id",
                keyValue: new Guid("b72b3271-3ed8-4be9-ac20-30a8e747b304"));

            migrationBuilder.DeleteData(
                table: "AssistanceTypes",
                keyColumn: "Id",
                keyValue: new Guid("e4f20038-5f10-41c7-a1b9-dc1fcf264ec1"));

            migrationBuilder.DeleteData(
                table: "AssistanceTypes",
                keyColumn: "Id",
                keyValue: new Guid("f05687cf-ea13-45ed-8916-1e577904cda1"));

            migrationBuilder.DropColumn(
                name: "StripeSessionId",
                table: "Donations");

            migrationBuilder.InsertData(
                table: "AssistanceTypes",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("0209baca-30c1-427e-8636-791a92aded76"), "طبية" },
                    { new Guid("33f77767-89d9-4f69-9df6-3f46acdb48ad"), "بيئية" },
                    { new Guid("6014379f-cfb6-40ea-9d07-e9735b626acb"), "مالية" },
                    { new Guid("7e1c9876-2419-44fe-b210-9a091dfaa252"), "بيطرية" },
                    { new Guid("8165d8e4-6a11-4750-b62a-373abab3ce6b"), "ذوي الاحتياجات الخاصة" },
                    { new Guid("b293f142-04cc-410f-98f8-62260f82ef24"), "غذائية" },
                    { new Guid("b80308fb-f7d4-4b43-b477-b1963d21a7fa"), "سكنية" },
                    { new Guid("b8142ce4-d29a-44d3-846e-c419821bdc04"), "طارئة وإغاثية" },
                    { new Guid("f3a89476-61c0-40b1-aa99-2b198fb97b9f"), "تعليمية" }
                });
        }
    }
}
