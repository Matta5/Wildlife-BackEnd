using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wildlife_DAL.Migrations
{
    /// <inheritdoc />
    public partial class newinit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Observations_UserId",
                table: "Observations",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Observations_Users_UserId",
                table: "Observations",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Observations_Users_UserId",
                table: "Observations");

            migrationBuilder.DropIndex(
                name: "IX_Observations_UserId",
                table: "Observations");
        }
    }
}
