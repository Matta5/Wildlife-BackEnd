using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wildlife_DAL.Migrations
{
    /// <inheritdoc />
    public partial class addedImageToObservation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Observations",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Observations");
        }
    }
}
