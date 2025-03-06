using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WaslAlkhair.Api.Migrations
{
    /// <inheritdoc />
    public partial class updatedtime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AlterColumn<DateTime>(
                name: "DescriptionUpdatedAt",
                table: "Assistances",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.InsertData(
                table: "AssistanceTypes",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("1d90c5f4-2055-42be-910c-45f6723d7b78"), "بيطرية" },
                    { new Guid("2fc35c9b-96c7-42e8-bfa1-0b30b5ad6bac"), "غذائية" },
                    { new Guid("329281db-bbb6-477c-b6e5-075e3ee9b207"), "مالية" },
                    { new Guid("3937940e-b0e6-4feb-b75e-41a1c21da8e0"), "تعليمية" },
                    { new Guid("58e35eb4-836b-4bb0-b01a-f7bd019f9b5e"), "طبية" },
                    { new Guid("6e2db313-5415-4af0-925a-16d61c4b571e"), "ذوي الاحتياجات الخاصة" },
                    { new Guid("9655751a-a07c-46e0-9871-d63b38b2f8ad"), "طارئة وإغاثية" },
                    { new Guid("f950645f-48f0-4e8c-a315-89165eb4342f"), "بيئية" },
                    { new Guid("fe4d455a-89db-4082-8956-c069917e7c7d"), "سكنية" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AssistanceTypes",
                keyColumn: "Id",
                keyValue: new Guid("1d90c5f4-2055-42be-910c-45f6723d7b78"));

            migrationBuilder.DeleteData(
                table: "AssistanceTypes",
                keyColumn: "Id",
                keyValue: new Guid("2fc35c9b-96c7-42e8-bfa1-0b30b5ad6bac"));

            migrationBuilder.DeleteData(
                table: "AssistanceTypes",
                keyColumn: "Id",
                keyValue: new Guid("329281db-bbb6-477c-b6e5-075e3ee9b207"));

            migrationBuilder.DeleteData(
                table: "AssistanceTypes",
                keyColumn: "Id",
                keyValue: new Guid("3937940e-b0e6-4feb-b75e-41a1c21da8e0"));

            migrationBuilder.DeleteData(
                table: "AssistanceTypes",
                keyColumn: "Id",
                keyValue: new Guid("58e35eb4-836b-4bb0-b01a-f7bd019f9b5e"));

            migrationBuilder.DeleteData(
                table: "AssistanceTypes",
                keyColumn: "Id",
                keyValue: new Guid("6e2db313-5415-4af0-925a-16d61c4b571e"));

            migrationBuilder.DeleteData(
                table: "AssistanceTypes",
                keyColumn: "Id",
                keyValue: new Guid("9655751a-a07c-46e0-9871-d63b38b2f8ad"));

            migrationBuilder.DeleteData(
                table: "AssistanceTypes",
                keyColumn: "Id",
                keyValue: new Guid("f950645f-48f0-4e8c-a315-89165eb4342f"));

            migrationBuilder.DeleteData(
                table: "AssistanceTypes",
                keyColumn: "Id",
                keyValue: new Guid("fe4d455a-89db-4082-8956-c069917e7c7d"));

            migrationBuilder.AlterColumn<DateTime>(
                name: "DescriptionUpdatedAt",
                table: "Assistances",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

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
    }
}
