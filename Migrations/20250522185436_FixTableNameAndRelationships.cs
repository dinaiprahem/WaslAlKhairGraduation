using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WaslAlkhair.Api.Migrations
{
    /// <inheritdoc />
    public partial class FixTableNameAndRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Donations_DonationCategories_DonationCategoryId",
                table: "Donations");

            migrationBuilder.DropIndex(
                name: "IX_Donations_DonationCategoryId",
                table: "Donations");

            migrationBuilder.DropColumn(
                name: "DonationCategoryId",
                table: "Donations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DonationCategoryId",
                table: "Donations",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Donations_DonationCategoryId",
                table: "Donations",
                column: "DonationCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Donations_DonationCategories_DonationCategoryId",
                table: "Donations",
                column: "DonationCategoryId",
                principalTable: "DonationCategories",
                principalColumn: "Id");
        }
    }
}
