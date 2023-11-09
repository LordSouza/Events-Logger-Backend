using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventsLogger.DataService.Migrations
{
    /// <inheritdoc />
    public partial class FixLoopSelfReference_1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_User_Entry",
                table: "Entrys");

            migrationBuilder.AddForeignKey(
                name: "FK_Entrys_AspNetUsers_UserId",
                table: "Entrys",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Entrys_AspNetUsers_UserId",
                table: "Entrys");

            migrationBuilder.AddForeignKey(
                name: "FK_User_Entry",
                table: "Entrys",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
