using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wildlife_DAL.Migrations
{
    /// <inheritdoc />
    public partial class RemovedUnnesaryPropertiesOfSpecies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Species");

            migrationBuilder.DropColumn(
                name: "IsPreloaded",
                table: "Species");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Species",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsPreloaded",
                table: "Species",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
