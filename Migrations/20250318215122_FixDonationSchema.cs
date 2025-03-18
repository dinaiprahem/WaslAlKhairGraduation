using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WaslAlkhair.Api.Migrations
{
    /// <inheritdoc />
    public partial class FixDonationSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DonationOpportunities_DonationCategory_CategoryId",
                table: "DonationOpportunities");

            migrationBuilder.DropForeignKey(
                name: "FK_Donations_DonationCategory_CategoryId",
                table: "Donations");

            migrationBuilder.DropForeignKey(
                name: "FK_Donations_DonationCategory_DonationCategoryId",
                table: "Donations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DonationCategory",
                table: "DonationCategory");

            migrationBuilder.RenameTable(
                name: "DonationCategory",
                newName: "DonationCategories");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DonationCategories",
                table: "DonationCategories",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DonationOpportunities_DonationCategories_CategoryId",
                table: "DonationOpportunities",
                column: "CategoryId",
                principalTable: "DonationCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Donations_DonationCategories_CategoryId",
                table: "Donations",
                column: "CategoryId",
                principalTable: "DonationCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Donations_DonationCategories_DonationCategoryId",
                table: "Donations",
                column: "DonationCategoryId",
                principalTable: "DonationCategories",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DonationOpportunities_DonationCategories_CategoryId",
                table: "DonationOpportunities");

            migrationBuilder.DropForeignKey(
                name: "FK_Donations_DonationCategories_CategoryId",
                table: "Donations");

            migrationBuilder.DropForeignKey(
                name: "FK_Donations_DonationCategories_DonationCategoryId",
                table: "Donations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DonationCategories",
                table: "DonationCategories");

            migrationBuilder.RenameTable(
                name: "DonationCategories",
                newName: "DonationCategory");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DonationCategory",
                table: "DonationCategory",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DonationOpportunities_DonationCategory_CategoryId",
                table: "DonationOpportunities",
                column: "CategoryId",
                principalTable: "DonationCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Donations_DonationCategory_CategoryId",
                table: "Donations",
                column: "CategoryId",
                principalTable: "DonationCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Donations_DonationCategory_DonationCategoryId",
                table: "Donations",
                column: "DonationCategoryId",
                principalTable: "DonationCategory",
                principalColumn: "Id");
        }
    }
}
