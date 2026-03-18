using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InternalBudgetTracker.Migrations
{
    /// <inheritdoc />
    public partial class ChangeInNotificatioTablenName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_t_User_ToUserId",
                table: "Notifications");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Notifications",
                table: "Notifications");

            migrationBuilder.RenameTable(
                name: "Notifications",
                newName: "t_Notification");

            migrationBuilder.RenameIndex(
                name: "IX_Notifications_ToUserId",
                table: "t_Notification",
                newName: "IX_t_Notification_ToUserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_t_Notification",
                table: "t_Notification",
                column: "NotificationId");

            migrationBuilder.AddForeignKey(
                name: "FK_t_Notification_t_User_ToUserId",
                table: "t_Notification",
                column: "ToUserId",
                principalTable: "t_User",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_t_Notification_t_User_ToUserId",
                table: "t_Notification");

            migrationBuilder.DropPrimaryKey(
                name: "PK_t_Notification",
                table: "t_Notification");

            migrationBuilder.RenameTable(
                name: "t_Notification",
                newName: "Notifications");

            migrationBuilder.RenameIndex(
                name: "IX_t_Notification_ToUserId",
                table: "Notifications",
                newName: "IX_Notifications_ToUserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Notifications",
                table: "Notifications",
                column: "NotificationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_t_User_ToUserId",
                table: "Notifications",
                column: "ToUserId",
                principalTable: "t_User",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
