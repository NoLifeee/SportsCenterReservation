using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportsCenterReservation.Migrations
{
    /// <inheritdoc />
    public partial class AddUserToRezervare : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Rezervari",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Rezervari_UserId",
                table: "Rezervari",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Rezervari_Users_UserId",
                table: "Rezervari",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rezervari_Users_UserId",
                table: "Rezervari");

            migrationBuilder.DropIndex(
                name: "IX_Rezervari_UserId",
                table: "Rezervari");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Rezervari");
        }
    }
}
