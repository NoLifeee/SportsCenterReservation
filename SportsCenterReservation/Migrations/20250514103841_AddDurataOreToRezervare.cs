using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportsCenterReservation.Migrations
{
    /// <inheritdoc />
    public partial class AddDurataOreToRezervare : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DurataOre",
                table: "Rezervari",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DurataOre",
                table: "Rezervari");
        }
    }
}
