using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WaslAlkhair.Api.Migrations
{
    /// <inheritdoc />
    public partial class Descriptionupdatedtime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AssistanceTypes",
                keyColumn: "Id",
                keyValue: new Guid("0ffc5342-e850-44a5-a5c1-cf09d95d7291"));

            migrationBuilder.DeleteData(
                table: "AssistanceTypes",
                keyColumn: "Id",
                keyValue: new Guid("169c1e33-8d28-41db-a92b-7a2285c34c74"));

            migrationBuilder.DeleteData(
                table: "AssistanceTypes",
                keyColumn: "Id",
                keyValue: new Guid("3aa682bf-4249-4b1c-81c2-4f5a1023db8d"));

            migrationBuilder.DeleteData(
                table: "AssistanceTypes",
                keyColumn: "Id",
                keyValue: new Guid("725ed29c-5f5d-4029-847a-e138743b23b9"));

            migrationBuilder.DeleteData(
                table: "AssistanceTypes",
                keyColumn: "Id",
                keyValue: new Guid("7acb0cec-05e6-413e-a71a-53dcf72e8177"));

            migrationBuilder.DeleteData(
                table: "AssistanceTypes",
                keyColumn: "Id",
                keyValue: new Guid("9502f090-447a-4ee4-b27e-d51613412b34"));

            migrationBuilder.DeleteData(
                table: "AssistanceTypes",
                keyColumn: "Id",
                keyValue: new Guid("ac94404c-894b-4093-8d64-ebdac3187722"));

            migrationBuilder.DeleteData(
                table: "AssistanceTypes",
                keyColumn: "Id",
                keyValue: new Guid("b4d9e548-4386-43c8-8d14-36adbc44f855"));

            migrationBuilder.DeleteData(
                table: "AssistanceTypes",
                keyColumn: "Id",
                keyValue: new Guid("db990003-196e-48dc-abe1-ac9712cacb52"));

            migrationBuilder.AddColumn<DateTime>(
                name: "DescriptionUpdatedAt",
                table: "Assistances",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.InsertData(
                table: "AssistanceTypes",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("007ff9e2-7730-43c9-89e0-493aabc61974"), "ذوي الاحتياجات الخاصة" },
                    { new Guid("20fe0d71-0c40-4e6d-a009-ccdd096093ad"), "غذائية" },
                    { new Guid("61a37ded-717c-47ab-aff3-287cfbdda6b1"), "مالية" },
                    { new Guid("75462e68-69d2-4675-98c3-e396c49caba6"), "سكنية" },
                    { new Guid("99be6690-2207-45ab-a0df-955c4666d2f1"), "طارئة وإغاثية" },
                    { new Guid("a6af92b6-73dd-4f69-b306-1039ed25cf8e"), "بيطرية" },
                    { new Guid("b1aa4a17-e252-4256-b779-399770574b7c"), "تعليمية" },
                    { new Guid("beee0906-d843-43fb-b917-00d9990a54d7"), "طبية" },
                    { new Guid("ca0df398-27c4-4790-a3d9-b93f2137509a"), "بيئية" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AssistanceTypes",
                keyColumn: "Id",
                keyValue: new Guid("007ff9e2-7730-43c9-89e0-493aabc61974"));

            migrationBuilder.DeleteData(
                table: "AssistanceTypes",
                keyColumn: "Id",
                keyValue: new Guid("20fe0d71-0c40-4e6d-a009-ccdd096093ad"));

            migrationBuilder.DeleteData(
                table: "AssistanceTypes",
                keyColumn: "Id",
                keyValue: new Guid("61a37ded-717c-47ab-aff3-287cfbdda6b1"));

            migrationBuilder.DeleteData(
                table: "AssistanceTypes",
                keyColumn: "Id",
                keyValue: new Guid("75462e68-69d2-4675-98c3-e396c49caba6"));

            migrationBuilder.DeleteData(
                table: "AssistanceTypes",
                keyColumn: "Id",
                keyValue: new Guid("99be6690-2207-45ab-a0df-955c4666d2f1"));

            migrationBuilder.DeleteData(
                table: "AssistanceTypes",
                keyColumn: "Id",
                keyValue: new Guid("a6af92b6-73dd-4f69-b306-1039ed25cf8e"));

            migrationBuilder.DeleteData(
                table: "AssistanceTypes",
                keyColumn: "Id",
                keyValue: new Guid("b1aa4a17-e252-4256-b779-399770574b7c"));

            migrationBuilder.DeleteData(
                table: "AssistanceTypes",
                keyColumn: "Id",
                keyValue: new Guid("beee0906-d843-43fb-b917-00d9990a54d7"));

            migrationBuilder.DeleteData(
                table: "AssistanceTypes",
                keyColumn: "Id",
                keyValue: new Guid("ca0df398-27c4-4790-a3d9-b93f2137509a"));

            migrationBuilder.DropColumn(
                name: "DescriptionUpdatedAt",
                table: "Assistances");

            migrationBuilder.InsertData(
                table: "AssistanceTypes",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("0ffc5342-e850-44a5-a5c1-cf09d95d7291"), "تعليمية" },
                    { new Guid("169c1e33-8d28-41db-a92b-7a2285c34c74"), "مالية" },
                    { new Guid("3aa682bf-4249-4b1c-81c2-4f5a1023db8d"), "طبية" },
                    { new Guid("725ed29c-5f5d-4029-847a-e138743b23b9"), "بيطرية" },
                    { new Guid("7acb0cec-05e6-413e-a71a-53dcf72e8177"), "غذائية" },
                    { new Guid("9502f090-447a-4ee4-b27e-d51613412b34"), "ذوي الاحتياجات الخاصة" },
                    { new Guid("ac94404c-894b-4093-8d64-ebdac3187722"), "بيئية" },
                    { new Guid("b4d9e548-4386-43c8-8d14-36adbc44f855"), "سكنية" },
                    { new Guid("db990003-196e-48dc-abe1-ac9712cacb52"), "طارئة وإغاثية" }
                });
        }
    }
}
