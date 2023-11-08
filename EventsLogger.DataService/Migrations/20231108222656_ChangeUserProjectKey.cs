using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventsLogger.DataService.Migrations
{
    /// <inheritdoc />
    public partial class ChangeUserProjectKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UsersProjects",
                table: "UsersProjects");

            migrationBuilder.DropIndex(
                name: "IX_UsersProjects_UserId",
                table: "UsersProjects");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UsersProjects",
                table: "UsersProjects",
                columns: new[] { "UserId", "ProjectId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UsersProjects",
                table: "UsersProjects");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UsersProjects",
                table: "UsersProjects",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_UsersProjects_UserId",
                table: "UsersProjects",
                column: "UserId");
        }
    }
}
