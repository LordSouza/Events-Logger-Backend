using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventsLogger.DataService.Migrations
{
    /// <inheritdoc />
    public partial class RelationshipUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UsersProjects",
                table: "UsersProjects");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Entrys",
                table: "Entrys");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "UsersProjects");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Entrys");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Addresses");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UsersProjects",
                table: "UsersProjects",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Entrys",
                table: "Entrys",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_UsersProjects_UserId",
                table: "UsersProjects",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Entrys_UserId",
                table: "Entrys",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UsersProjects",
                table: "UsersProjects");

            migrationBuilder.DropIndex(
                name: "IX_UsersProjects_UserId",
                table: "UsersProjects");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Entrys",
                table: "Entrys");

            migrationBuilder.DropIndex(
                name: "IX_Entrys_UserId",
                table: "Entrys");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "UsersProjects",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Projects",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Entrys",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Addresses",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_UsersProjects",
                table: "UsersProjects",
                columns: new[] { "UserId", "ProjectId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Entrys",
                table: "Entrys",
                columns: new[] { "UserId", "ProjectId" });
        }
    }
}
