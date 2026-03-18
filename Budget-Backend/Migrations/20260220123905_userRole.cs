using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace InternalBudgetTracker.Migrations
{
    /// <inheritdoc />
    public partial class userRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Role",
                table: "t_User");

            migrationBuilder.AddColumn<int>(
                name: "RoleId",
                table: "t_User",
                type: "int",
                nullable: true,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "t_Role",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_Role", x => x.RoleId);
                });

            migrationBuilder.InsertData(
                table: "t_Role",
                columns: new[] { "RoleId", "RoleName" },
                values: new object[,]
                {
                    { 1, "Admin" },
                    { 2, "Manager" },
                    { 3, "Employee" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_t_User_RoleId",
                table: "t_User",
                column: "RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_t_User_t_Role_RoleId",
                table: "t_User",
                column: "RoleId",
                principalTable: "t_Role",
                principalColumn: "RoleId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_t_User_t_Role_RoleId",
                table: "t_User");

            migrationBuilder.DropTable(
                name: "t_Role");

            migrationBuilder.DropIndex(
                name: "IX_t_User_RoleId",
                table: "t_User");

            migrationBuilder.DropColumn(
                name: "RoleId",
                table: "t_User");

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "t_User",
                type: "nvarchar(20)",
                nullable: false,
                defaultValue: "");
        }
    }
}
