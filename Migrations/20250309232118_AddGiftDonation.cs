using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WaslAlkhair.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddGiftDonation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GiftDonation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RecipientName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    RecipientPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ShowAmount = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ShowOpportunity = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DonationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GiftDonation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GiftDonation_Donations_DonationId",
                        column: x => x.DonationId,
                        principalTable: "Donations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GiftDonation_DonationId",
                table: "GiftDonation",
                column: "DonationId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GiftDonation");
        }
    }
}
