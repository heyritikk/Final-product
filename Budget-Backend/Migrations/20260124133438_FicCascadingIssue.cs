using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InternalBudgetTracker.Migrations
{
    /// <inheritdoc />
    public partial class FicCascadingIssue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_t_User_t_Department_DepartmentId",
                table: "t_User");

            migrationBuilder.CreateTable(
                name: "t_Budget",
                columns: table => new
                {
                    BudgetId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AmountAllocated = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_Budget", x => x.BudgetId);
                    table.ForeignKey(
                        name: "FK_t_Budget_t_Department_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "t_Department",
                        principalColumn: "DepartmentId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_t_Budget_t_User_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "t_User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "t_Expense",
                columns: table => new
                {
                    ExpenseId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExpenseDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    BudgetId = table.Column<int>(type: "int", nullable: false),
                    SubmittedByUserId = table.Column<int>(type: "int", nullable: false),
                    AssignedManagerId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_Expense", x => x.ExpenseId);
                    table.ForeignKey(
                        name: "FK_t_Expense_t_Budget_BudgetId",
                        column: x => x.BudgetId,
                        principalTable: "t_Budget",
                        principalColumn: "BudgetId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_t_Expense_t_User_AssignedManagerId",
                        column: x => x.AssignedManagerId,
                        principalTable: "t_User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_t_Expense_t_User_SubmittedByUserId",
                        column: x => x.SubmittedByUserId,
                        principalTable: "t_User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "t_ExpenseApproval",
                columns: table => new
                {
                    ExpenseApprovalId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExpenseId = table.Column<int>(type: "int", nullable: false),
                    ManagerId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ActionDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_ExpenseApproval", x => x.ExpenseApprovalId);
                    table.ForeignKey(
                        name: "FK_t_ExpenseApproval_t_Expense_ExpenseId",
                        column: x => x.ExpenseId,
                        principalTable: "t_Expense",
                        principalColumn: "ExpenseId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_t_ExpenseApproval_t_User_ManagerId",
                        column: x => x.ManagerId,
                        principalTable: "t_User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_t_Budget_CreatedByUserId",
                table: "t_Budget",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_t_Budget_DepartmentId",
                table: "t_Budget",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_t_Expense_AssignedManagerId",
                table: "t_Expense",
                column: "AssignedManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_t_Expense_BudgetId",
                table: "t_Expense",
                column: "BudgetId");

            migrationBuilder.CreateIndex(
                name: "IX_t_Expense_SubmittedByUserId",
                table: "t_Expense",
                column: "SubmittedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_t_ExpenseApproval_ExpenseId",
                table: "t_ExpenseApproval",
                column: "ExpenseId");

            migrationBuilder.CreateIndex(
                name: "IX_t_ExpenseApproval_ManagerId",
                table: "t_ExpenseApproval",
                column: "ManagerId");

            migrationBuilder.AddForeignKey(
                name: "FK_t_User_t_Department_DepartmentId",
                table: "t_User",
                column: "DepartmentId",
                principalTable: "t_Department",
                principalColumn: "DepartmentId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_t_User_t_Department_DepartmentId",
                table: "t_User");

            migrationBuilder.DropTable(
                name: "t_ExpenseApproval");

            migrationBuilder.DropTable(
                name: "t_Expense");

            migrationBuilder.DropTable(
                name: "t_Budget");

            migrationBuilder.AddForeignKey(
                name: "FK_t_User_t_Department_DepartmentId",
                table: "t_User",
                column: "DepartmentId",
                principalTable: "t_Department",
                principalColumn: "DepartmentId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
