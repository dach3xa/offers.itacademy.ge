using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace offers.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class AddedPhotoUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PhotoURL",
                table: "Offers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PhotoURL",
                table: "CompanyDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhotoURL",
                table: "Offers");

            migrationBuilder.DropColumn(
                name: "PhotoURL",
                table: "CompanyDetails");
        }
    }
}
