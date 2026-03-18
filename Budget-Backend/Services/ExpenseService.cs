using InternalBudgetTracker.Data;
using InternalBudgetTracker.DTOs;
using InternalBudgetTracker.Enum;
using InternalBudgetTracker.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace InternalBudgetTracker.Services
{
    public class ExpenseService
    {
        private readonly AppDbContext _context;
        private readonly  NotificationService _notificationService;

        public ExpenseService(AppDbContext context, NotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }
        public string CreateExpense(ExpenseCreateDTO dto, ClaimsPrincipal user)
        {
            // 1️ Token se user nikaalo
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            var roleClaim = user.FindFirst(ClaimTypes.Role);

            if (userIdClaim == null || roleClaim == null)
                throw new Exception("Invalid token");

            // 2️Sirf EMPLOYEE allow
            if (roleClaim.Value != "Employee")
                throw new Exception("Only employee can create expense");

            var employeeId = int.Parse(userIdClaim.Value);

            // Budget limit enforcement: employee cannot submit expenses beyond allocated budget.
            // We count both Approved and Pending expenses to prevent overspending via multiple pending submissions.
            var budget = _context.Budgets.FirstOrDefault(b => b.BudgetId == dto.BudgetId);
            if (budget == null)
                throw new Exception("Invalid budget selected.");

            var usedAmount = _context.Expenses
                .Where(e => e.BudgetId == dto.BudgetId
                            && e.EndDate == null
                            && (e.Status == ExpenseStatus.Pending || e.Status == ExpenseStatus.Approved))
                .Sum(e => (decimal?)e.Amount) ?? 0m;

            var remaining = budget.AmountAllocated - usedAmount;
            if (dto.Amount > remaining)
                throw new Exception($"Expense amount exceeds remaining budget. Remaining: {remaining}");

            var expense= _context.Database.ExecuteSqlRaw(
     "EXEC dbo.sp_Expense " +
     "@Action, @Description, @Amount, @BudgetId, @EmployeeId, @ManagerId, @StartDate, @EndDate",
     new SqlParameter("@Action", "INSERT"),
     new SqlParameter("@Description", (object)dto.Description ?? DBNull.Value),
     new SqlParameter("@Amount", dto.Amount),
     new SqlParameter("@BudgetId", dto.BudgetId),
     new SqlParameter("@EmployeeId", employeeId),
     new SqlParameter("@ManagerId", dto.ManagerId),
     new SqlParameter("@StartDate", DBNull.Value),
     new SqlParameter("@EndDate", DBNull.Value)
     //new SqlParameter("@Status", DBNull.Value)
     
 );


            //Notification to manager when expense create

            _notificationService.CreateNotification(
        dto.ManagerId,
        NotificationType.ExpensePending,
        $"An expense request of ₹{dto.Amount} has been submitted and is awaiting your approval."
    );

            

            return "Expense created successfully";
        }

        //Service to get expenses
        public object GetExpenses(int? expenseId,ClaimsPrincipal user)
        {

            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            var roleClaim = user.FindFirst(ClaimTypes.Role);

            if (userIdClaim == null || roleClaim == null)
                throw new Exception("Invalid token");

            int currentUserId = int.Parse(userIdClaim.Value);
            string role = roleClaim.Value;

            var query = _context.Expenses
                .Include(e => e.Employee)
                .Include(e => e.Budget)
                .AsQueryable();

            if (role == "Employee")
            {
                query = query.Where(e => e.EmployeeId == currentUserId);
            }

            // join with approvals + manager for manager filtering/UI
            var approvals = _context.ExpenseApprovals
                .Include(a => a.Manager)
                .AsQueryable();

            if (role == "Manager")
            {
                approvals = approvals.Where(a => a.ManagerId == currentUserId);
                query = query.Where(e => approvals.Any(a => a.ExpenseId == e.ExpenseId));
            }

            if (expenseId.HasValue)
            {
                var item = query.FirstOrDefault(e => e.ExpenseId == expenseId.Value);
                if (item == null) throw new Exception("Expense not found");

                var approval = approvals.FirstOrDefault(a => a.ExpenseId == item.ExpenseId);

                return new
                {
                    id = item.ExpenseId,
                    title = item.Description,
                    amount = item.Amount,
                    budgetId = item.BudgetId,
                    description = item.Description,
                    employeeName = item.Employee != null ? item.Employee.Name : null,
                    status = item.Status.ToString(),
                    managerId = approval?.ManagerId,
                    managerEmail = approval?.Manager?.Email
                };
            }

            var list = query
                .Join(
                    approvals,
                    e => e.ExpenseId,
                    a => a.ExpenseId,
                    (e, a) => new { e, a }
                )
                .Select(x => new
                {
                    id = x.e.ExpenseId,
                    title = x.e.Description,
                    amount = x.e.Amount,
                    budgetId = x.e.BudgetId,
                    description = x.e.Description,
                    employeeName = x.e.Employee != null ? x.e.Employee.Name : null,
                    status = x.e.Status.ToString(),
                    managerId = x.a.ManagerId,
                    managerEmail = x.a.Manager != null ? x.a.Manager.Email : null
                })
                .ToList();

            return list;
        }
 
        


        public string UpdateExpense(int expenseId, ExpenseUpdateDTO dto, ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            var userId = int.Parse(userIdClaim.Value);

           
            try {
                var rowsAffected = _context.Database.ExecuteSqlRaw(
                "EXEC dbo.sp_UpdateExpense @ExpenseId, @EmployeeId, @Description, @Amount",
                new SqlParameter("@ExpenseId", expenseId),
                new SqlParameter("@EmployeeId", userId),
                new SqlParameter("@Description", dto.Description ?? (object)DBNull.Value),
                new SqlParameter("@Amount", dto.Amount)
            );

                // 3Check if update happened
                if (rowsAffected == 0)
                    throw new Exception("Expense not editable. It may be approved or not yours.");
                return "Expense updated successfully";

            
            }
              catch (Exception ex)
              {
                     throw new Exception(ex.Message);
                 }
     }
        
        public string DeleteExpense(int expenseId,ClaimsPrincipal user)
        {
            try {
                var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
                var userId = int.Parse(userIdClaim.Value);
                var expenseIdParam = new SqlParameter("@ExpenseId", expenseId);
                var employeeIdParam = new SqlParameter("@EmployeeId", userId);

                _context.Database.ExecuteSqlRaw(
                    "EXEC sp_DeleteExpense @ExpenseId, @EmployeeId",
                    expenseIdParam,
                    employeeIdParam
                );

                return "Expense deleted successfully";
            }
            catch (Exception ex)
            {
                return ex.Message;   //  user will get  exact error
            }
        

        }

        //Service for Expense approval or Reject
        public string ApproveRejectExpense(int expenseId,ExpenseApprovalDTO dto, ClaimsPrincipal user)
        {
            //Token se manager id nikalo
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            var roleClaim = user.FindFirst(ClaimTypes.Role);

            if (userIdClaim == null || roleClaim == null)
                throw new Exception("Invalid token");

            if (roleClaim.Value != "Manager")
                throw new Exception("Only manager can approve/reject");

            int managerId = int.Parse(userIdClaim.Value);

            //  Expense approval mapping check
            var approval = _context.ExpenseApprovals
                .FirstOrDefault(a =>
                    a.ExpenseId == expenseId &&
                    a.ManagerId == managerId);

            if (approval == null)
                throw new Exception("Expense not assigned to you ");

            if(approval.Status != ExpenseStatus.Pending ||approval.EndDate != null)
                throw new Exception("Expense already processed ");

            //  Expense fetch
            var expense = _context.Expenses
                .FirstOrDefault(e => e.ExpenseId == expenseId && e.EndDate == null);

            if (expense == null)
                throw new Exception("Invalid expense");

            // Approve / Reject
            //approval.IsApproved = dto.IsApproved;
            if (dto.Action == "Approve")
            {
                approval.Status = ExpenseStatus.Approved;
                expense.Status = ExpenseStatus.Approved;

                //Notification to employee for approval
                _notificationService.CreateNotification(expense.EmployeeId,NotificationType.ExpenseApproval,
        $"Your expense of amount {expense.Amount} has been approved");

            }
            else if (dto.Action == "Reject")
            {

                approval.Status = ExpenseStatus.Rejected;
                expense.Status = ExpenseStatus.Rejected;

                //Notification to employee for rejection
                _notificationService.CreateNotification(expense.EmployeeId,NotificationType.ExpenseRejected,
               $"Your expense was rejected. Reason: {dto.Comment}");

            }
            else
            {
                throw new Exception("Invalid action");
            }
            approval.Comment = dto.Comment;
            approval.EndDate = DateTime.UtcNow;
            expense.EndDate = DateTime.UtcNow;
            _context.SaveChanges();
            return $"Expense {approval.Status} successfully";

        }
    }
}
 


    
