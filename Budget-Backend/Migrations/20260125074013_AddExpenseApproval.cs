using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InternalBudgetTracker.Migrations
{
    /// <inheritdoc />
    public partial class AddExpenseApproval : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_t_Expense_t_User_AssignedManagerId",
                table: "t_Expense");

            migrationBuilder.DropIndex(
                name: "IX_t_Expense_AssignedManagerId",
                table: "t_Expense");

            migrationBuilder.DropColumn(
                name: "AssignedManagerId",
                table: "t_Expense");

            migrationBuilder.RenameColumn(
                name: "ActionDate",
                table: "t_ExpenseApproval",
                newName: "StartDate");

            migrationBuilder.RenameColumn(
                name: "ExpenseDate",
                table: "t_Expense",
                newName: "UpdatedDate");

            migrationBuilder.AddColumn<string>(
                name: "Comment",
                table: "t_ExpenseApproval",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "t_ExpenseApproval",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "t_Expense",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "t_Expense",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SubmittedDate",
                table: "t_Expense",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Comment",
                table: "t_ExpenseApproval");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "t_ExpenseApproval");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "t_Expense");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "t_Expense");

            migrationBuilder.DropColumn(
                name: "SubmittedDate",
                table: "t_Expense");

            migrationBuilder.RenameColumn(
                name: "StartDate",
                table: "t_ExpenseApproval",
                newName: "ActionDate");

            migrationBuilder.RenameColumn(
                name: "UpdatedDate",
                table: "t_Expense",
                newName: "ExpenseDate");

            migrationBuilder.AddColumn<int>(
                name: "AssignedManagerId",
                table: "t_Expense",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_t_Expense_AssignedManagerId",
                table: "t_Expense",
                column: "AssignedManagerId");

            migrationBuilder.AddForeignKey(
                name: "FK_t_Expense_t_User_AssignedManagerId",
                table: "t_Expense",
                column: "AssignedManagerId",
                principalTable: "t_User",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
