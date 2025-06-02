using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportsCenterReservation.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdToRezervare : Migration
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

            migrationBuilder.AddColumn<int>(
                name: "UserId1",
                table: "Rezervari",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Sali",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nume = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Tip = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sali", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Rezervari_UserId",
                table: "Rezervari",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Rezervari_UserId1",
                table: "Rezervari",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Rezervari_Users_UserId",
                table: "Rezervari",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Rezervari_Users_UserId1",
                table: "Rezervari",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rezervari_Users_UserId",
                table: "Rezervari");

            migrationBuilder.DropForeignKey(
                name: "FK_Rezervari_Users_UserId1",
                table: "Rezervari");

            migrationBuilder.DropTable(
                name: "Sali");

            migrationBuilder.DropIndex(
                name: "IX_Rezervari_UserId",
                table: "Rezervari");

            migrationBuilder.DropIndex(
                name: "IX_Rezervari_UserId1",
                table: "Rezervari");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Rezervari");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "Rezervari");
        }
    }
}
