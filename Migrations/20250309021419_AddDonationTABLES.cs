using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WaslAlkhair.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddDonationTABLES : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
    name: "Donations",
    columns: table => new
    {
        Id = table.Column<int>(type: "int", nullable: false)
            .Annotation("SqlServer:Identity", "1, 1"),
        Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
        DonatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
        DonorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
        Type = table.Column<int>(type: "int", nullable: false),
        CategoryId = table.Column<int>(type: "int", nullable: true) // Keep only this column
    },
    constraints: table =>
    {
        table.PrimaryKey("PK_Donations", x => x.Id);
        table.ForeignKey(
            name: "FK_Donations_AspNetUsers_DonorId",
            column: x => x.DonorId,
            principalTable: "AspNetUsers",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
        table.ForeignKey(
            name: "FK_Donations_DonationCategory_CategoryId",
            column: x => x.CategoryId,
            principalTable: "DonationCategory",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    });

            migrationBuilder.CreateTable(
                name: "Donations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DonatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    DonorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: true),
                    DonationCategoryId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Donations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Donations_AspNetUsers_DonorId",
                        column: x => x.DonorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Donations_DonationCategory_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "DonationCategory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Donations_DonationCategory_DonationCategoryId",
                        column: x => x.DonationCategoryId,
                        principalTable: "DonationCategory",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DonationDistributions",
                columns: table => new
                {
                    DonationId = table.Column<int>(type: "int", nullable: false),
                    DonationOpportunityId = table.Column<int>(type: "int", nullable: false),
                    DistributedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DonationDistributions", x => new { x.DonationId, x.DonationOpportunityId });
                    table.ForeignKey(
                        name: "FK_DonationDistributions_DonationOpportunities_DonationOpportunityId",
                        column: x => x.DonationOpportunityId,
                        principalTable: "DonationOpportunities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DonationDistributions_Donations_DonationId",
                        column: x => x.DonationId,
                        principalTable: "Donations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DonationDistributions_DonationId",
                table: "DonationDistributions",
                column: "DonationId");

            migrationBuilder.CreateIndex(
                name: "IX_DonationDistributions_DonationOpportunityId",
                table: "DonationDistributions",
                column: "DonationOpportunityId");

            migrationBuilder.CreateIndex(
                name: "IX_DonationOpportunities_CategoryId",
                table: "DonationOpportunities",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_DonationOpportunities_CharityId",
                table: "DonationOpportunities",
                column: "CharityId");

            migrationBuilder.CreateIndex(
                name: "IX_Donations_CategoryId",
                table: "Donations",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Donations_DonationCategoryId",
                table: "Donations",
                column: "DonationCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Donations_DonorId",
                table: "Donations",
                column: "DonorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DonationDistributions");

            migrationBuilder.DropTable(
                name: "DonationOpportunities");

            migrationBuilder.DropTable(
                name: "Donations");

            migrationBuilder.DropTable(
                name: "DonationCategory");
        }
    }
}
