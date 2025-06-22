using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WaslAlkhair.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentConfirmationToDonation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddColumn<bool>(
                name: "IsPaid",
                table: "Donations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaymentConfirmedAt",
                table: "Donations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.InsertData(
                table: "AssistanceTypes",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("1500f8bc-3911-4444-9457-674ddb2cc192"), "مالية" },
                    { new Guid("1cfe2a3c-9295-45ba-adef-4af164da93e9"), "بيئية" },
                    { new Guid("5412b8e9-fdb9-452c-844b-39a74700cfd2"), "سكنية" },
                    { new Guid("8c6a982a-e357-42dc-bb21-79628e8910fd"), "غذائية" },
                    { new Guid("97c4dae4-168e-4c01-8c66-86f9d9cba50b"), "تعليمية" },
                    { new Guid("a4f25a7e-0151-4ebd-9605-83851b46b691"), "طارئة وإغاثية" },
                    { new Guid("b6b0d603-bcc6-4d5d-9dd0-1610d574cb30"), "طبية" },
                    { new Guid("c2f55428-61e9-45aa-bc98-b7de6b2b5cd2"), "ذوي الاحتياجات الخاصة" },
                    { new Guid("ec1bf3f5-e679-4e46-9ca5-2c99611088e9"), "بيطرية" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AssistanceTypes",
                keyColumn: "Id",
                keyValue: new Guid("1500f8bc-3911-4444-9457-674ddb2cc192"));

            migrationBuilder.DeleteData(
                table: "AssistanceTypes",
                keyColumn: "Id",
                keyValue: new Guid("1cfe2a3c-9295-45ba-adef-4af164da93e9"));

            migrationBuilder.DeleteData(
                table: "AssistanceTypes",
                keyColumn: "Id",
                keyValue: new Guid("5412b8e9-fdb9-452c-844b-39a74700cfd2"));

            migrationBuilder.DeleteData(
                table: "AssistanceTypes",
                keyColumn: "Id",
                keyValue: new Guid("8c6a982a-e357-42dc-bb21-79628e8910fd"));

            migrationBuilder.DeleteData(
                table: "AssistanceTypes",
                keyColumn: "Id",
                keyValue: new Guid("97c4dae4-168e-4c01-8c66-86f9d9cba50b"));

            migrationBuilder.DeleteData(
                table: "AssistanceTypes",
                keyColumn: "Id",
                keyValue: new Guid("a4f25a7e-0151-4ebd-9605-83851b46b691"));

            migrationBuilder.DeleteData(
                table: "AssistanceTypes",
                keyColumn: "Id",
                keyValue: new Guid("b6b0d603-bcc6-4d5d-9dd0-1610d574cb30"));

            migrationBuilder.DeleteData(
                table: "AssistanceTypes",
                keyColumn: "Id",
                keyValue: new Guid("c2f55428-61e9-45aa-bc98-b7de6b2b5cd2"));

            migrationBuilder.DeleteData(
                table: "AssistanceTypes",
                keyColumn: "Id",
                keyValue: new Guid("ec1bf3f5-e679-4e46-9ca5-2c99611088e9"));

            migrationBuilder.DropColumn(
                name: "IsPaid",
                table: "Donations");

            migrationBuilder.DropColumn(
                name: "PaymentConfirmedAt",
                table: "Donations");

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
    }
}
