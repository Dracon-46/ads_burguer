using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TotemPWA.Migrations
{
    /// <inheritdoc />
    public partial class mudancas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CanBeAdded",
                table: "Additionals");

            migrationBuilder.DropColumn(
                name: "CanBeRemoved",
                table: "Additionals");

            migrationBuilder.DropColumn(
                name: "IsDefault",
                table: "Additionals");

            migrationBuilder.AddColumn<string>(
                name: "Icon",
                table: "Categories",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Icon",
                table: "Categories");

            migrationBuilder.AddColumn<bool>(
                name: "CanBeAdded",
                table: "Additionals",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanBeRemoved",
                table: "Additionals",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDefault",
                table: "Additionals",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }
    }
}
