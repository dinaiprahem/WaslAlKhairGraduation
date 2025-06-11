using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WaslAlkhair.Api.Migrations
{
    /// <inheritdoc />
    public partial class AssistanceAssistanceTypeDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AssistanceTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssistanceTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Assistances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AvailableSpots = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    AssistanceTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ContactInfo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    IsOpen = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assistances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Assistances_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Assistances_AssistanceTypes_AssistanceTypeId",
                        column: x => x.AssistanceTypeId,
                        principalTable: "AssistanceTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_Assistances_AssistanceTypeId",
                table: "Assistances",
                column: "AssistanceTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Assistances_CreatedById",
                table: "Assistances",
                column: "CreatedById");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Assistances");

            migrationBuilder.DropTable(
                name: "AssistanceTypes");
        }
    }
}
