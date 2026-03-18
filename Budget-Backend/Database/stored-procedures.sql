-- Run this script inside the InternalBudgetDB database.
-- Example:
--   USE InternalBudgetDB;
--   GO

USE InternalBudgetDB;
GO

/* =========================
   Budgets
   ========================= */

-- =============================================
-- dbo.sp_CreateBudget
-- Used by: Backend BudgetService (create budget API)
-- API route: POST /api/budget/create (called from Manager UI)
-- Input: title/amount/department + createdByUserId (manager)
-- What it does: inserts a new row in dbo.t_Budget with Status='Active'
-- Output: returns newly created BudgetId (SCOPE_IDENTITY)
-- After it runs: frontend usually reloads budgets list and shows a notification
-- =============================================
CREATE OR ALTER PROCEDURE dbo.sp_CreateBudget
    @Title NVARCHAR(MAX),
    @AmountAllocated DECIMAL(18,2),
    @CreatedByUserId INT,
    @DepartmentId INT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.t_Budget
        (Title, AmountAllocated, StartDate, EndDate, Status, CreatedByUserId, DepartmentId)
    VALUES
        (@Title, @AmountAllocated, GETDATE(), NULL, N'Active', @CreatedByUserId, @DepartmentId);

    SELECT CAST(SCOPE_IDENTITY() AS INT) AS BudgetId;
END
GO

-- =============================================
-- dbo.sp_GetBudgets
-- Used by: Backend BudgetService (get budgets API)
-- API route: GET /api/budget (called by Manager/Admin UIs to list budgets)
-- Input: optional @BudgetId (null = list all active budgets)
-- What it does: selects Active budgets (EndDate is null)
-- Output: Budget rows (BudgetId, Title, AmountAllocated, ...)
-- After it runs: frontend renders the table and may filter client-side by userId/department
-- =============================================
CREATE OR ALTER PROCEDURE dbo.sp_GetBudgets
    @BudgetId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        b.BudgetId,
        b.Title,
        b.AmountAllocated,
        b.StartDate,
        b.EndDate,
        b.Status,
        b.CreatedByUserId,
        b.DepartmentId
    FROM dbo.t_Budget b
    WHERE
        (@BudgetId IS NULL OR b.BudgetId = @BudgetId)
        AND b.EndDate IS NULL
        AND b.Status = N'Active'
    ORDER BY b.BudgetId DESC;
END
GO

-- =============================================
-- dbo.sp_UpdateBudget
-- Used by: Backend BudgetService (update budget API)
-- API route: PATCH /api/budget/update/{budgetId} (Manager UI "Update")
-- Input: @BudgetId + optional fields (Title/AmountAllocated/DepartmentId)
-- What it does: updates only Active budgets (EndDate is null, Status='Active')
-- Output: no explicit select (rows are updated)
-- After it runs: frontend usually reloads budgets list and closes the edit modal
-- =============================================
CREATE OR ALTER PROCEDURE dbo.sp_UpdateBudget
    @BudgetId INT,
    @Title NVARCHAR(MAX) = NULL,
    @AmountAllocated DECIMAL(18,2) = NULL,
    @DepartmentId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.t_Budget
    SET
        Title = COALESCE(@Title, Title),
        AmountAllocated = COALESCE(@AmountAllocated, AmountAllocated),
        DepartmentId = COALESCE(@DepartmentId, DepartmentId)
    WHERE
        BudgetId = @BudgetId
        AND EndDate IS NULL
        AND Status = N'Active';
END
GO

-- =============================================
-- dbo.sp_DeleteBudget
-- Used by: Backend BudgetService (delete/close budget API)
-- API route: PATCH /api/budget/delete/{budgetId} (Manager UI "Delete")
-- Input: @BudgetId + @UserId (manager/creator)
-- What it does: soft-closes the budget (Status='Closed', sets EndDate)
-- Output: no explicit select
-- After it runs: frontend reloads budgets list; related reports will change
-- =============================================
CREATE OR ALTER PROCEDURE dbo.sp_DeleteBudget
    @BudgetId INT,
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.t_Budget
    SET
        Status = N'Closed',
        EndDate = GETDATE()
    WHERE
        BudgetId = @BudgetId
        AND CreatedByUserId = @UserId
        AND EndDate IS NULL;
END
GO

/* =========================
   Expenses + Approval mapping
   ========================= */

-- =============================================
-- dbo.sp_Expense
-- Used by: Backend ExpenseService (create expense API)
-- API route: POST /api/expense/create (Employee UI "Submit Expense")
-- Input: @Action='INSERT' + description/amount/budgetId/employeeId/managerId
-- What it does:
--   - inserts a new expense row in dbo.t_Expense (Status='Pending')
--   - inserts mapping row in dbo.t_ExpenseApproval (Status=0 => Pending)
-- Output: returns newly created ExpenseId
-- After it runs:
--   - manager gets a notification (created by backend NotificationService)
--   - UI usually reloads expenses list and resets the form
-- =============================================
CREATE OR ALTER PROCEDURE dbo.sp_Expense
    @Action NVARCHAR(20),
    @Description NVARCHAR(MAX) = NULL,
    @Amount DECIMAL(18,2) = NULL,
    @BudgetId INT = NULL,
    @EmployeeId INT = NULL,
    @ManagerId INT = NULL,
    @StartDate DATETIME2 = NULL,
    @EndDate DATETIME2 = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF UPPER(@Action) = 'INSERT'
    BEGIN
        INSERT INTO dbo.t_Expense
            (Amount, Description, SubmittedDate, UpdatedDate, StartDate, EndDate, Status, BudgetId, SubmittedByUserId)
        VALUES
            (@Amount, @Description, SYSUTCDATETIME(), SYSUTCDATETIME(), NULL, NULL, N'Pending', @BudgetId, @EmployeeId);

        DECLARE @NewExpenseId INT = CAST(SCOPE_IDENTITY() AS INT);

        INSERT INTO dbo.t_ExpenseApproval
            (ExpenseId, ManagerId, Status, StartDate, EndDate, Comment)
        VALUES
            (@NewExpenseId, @ManagerId, 0, GETDATE(), NULL, NULL);

        SELECT @NewExpenseId AS ExpenseId;
        RETURN;
    END

    RAISERROR ('Unsupported action for sp_Expense. Only INSERT is implemented.', 16, 1);
    RETURN;
END
GO

-- =============================================
-- dbo.sp_UpdateExpense
-- Used by: Backend ExpenseService (update expense API)
-- API route: PATCH /api/expense/update/{expenseId} (Employee UI "Update")
-- Input: expenseId + employeeId + new description/amount
-- What it does: updates the expense only if it belongs to the employee and is still Pending
-- Output: no explicit select (rows are updated)
-- After it runs: frontend reloads list and closes the edit modal
-- =============================================
CREATE OR ALTER PROCEDURE dbo.sp_UpdateExpense
    @ExpenseId INT,
    @EmployeeId INT,
    @Description NVARCHAR(MAX) = NULL,
    @Amount DECIMAL(18,2)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.t_Expense
    SET
        Description = COALESCE(@Description, Description),
        Amount = @Amount,
        UpdatedDate = SYSUTCDATETIME()
    WHERE
        ExpenseId = @ExpenseId
        AND SubmittedByUserId = @EmployeeId
        AND EndDate IS NULL
        AND Status = N'Pending';
END
GO

-- =============================================
-- dbo.sp_DeleteExpense
-- Used by: Backend ExpenseService (delete expense API - soft delete)
-- API route: PATCH /api/expense/delete/{expenseId} (Employee UI "Delete")
-- Input: expenseId + employeeId
-- What it does: soft-deletes by setting EndDate (only if Pending and owned by employee)
-- Output: no explicit select
-- After it runs: frontend removes row from UI or reloads expenses list
-- =============================================
CREATE OR ALTER PROCEDURE dbo.sp_DeleteExpense
    @ExpenseId INT,
    @EmployeeId INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.t_Expense
    SET
        EndDate = SYSUTCDATETIME(),
        UpdatedDate = SYSUTCDATETIME()
    WHERE
        ExpenseId = @ExpenseId
        AND SubmittedByUserId = @EmployeeId
        AND EndDate IS NULL
        AND Status = N'Pending';
END
GO

/* =========================
   Views
   ========================= */

CREATE OR ALTER VIEW dbo.v_UserWithRole
AS
SELECT
    u.UserId,
    u.Name,
    u.Email,
    u.Password,
    u.IsVerified,
    u.Status,
    u.DepartmentId,
    u.RoleId,
    r.RoleName AS Role
FROM dbo.t_User u
LEFT JOIN dbo.t_Role r
    ON r.RoleId = u.RoleId;
GO

