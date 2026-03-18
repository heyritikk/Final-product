using System.Collections.Generic;
using System.Linq;
using InternalBudgetTracker.Data;
using InternalBudgetTracker.DTOs;
using InternalBudgetTracker.Enum;
using Microsoft.EntityFrameworkCore;

public class ReportService
{
    private readonly AppDbContext _context;

    public ReportService(AppDbContext context)
    {
        // DI: AppDbContext injected (DB connection configured in Program.cs)
        _context = context;
    }
    
    // Department-wise report (consistent: only active budgets and their expenses)
    public List<DepartmentReportDto> GetDepartmentReport(int? departmentId)
    {
        // Called by GET /api/report/department (ReportController)
        var departmentsQuery = _context.Departments.AsQueryable();
        if (departmentId.HasValue)
            departmentsQuery = departmentsQuery.Where(d => d.DepartmentId == departmentId.Value);

        var departments = departmentsQuery.ToList();

        // Aggregate active budgets per department (server-side)
        var budgetsByDept = _context.Budgets
            //.Where(b => b.Status == BudgetStatus.Active)
            .GroupBy(b => b.DepartmentId)
            .Select(g => new
            {
                DepartmentId = g.Key,
                TotalBudget = g.Sum(b => (decimal?)b.AmountAllocated) ?? 0m
            })
            .ToList();

        // Aggregate expenses only for active budgets (join ensures consistency)
        var expensesByDept = _context.Expenses.Where(e =>e.Status == ExpenseStatus.Approved)
            .Join(_context.Budgets,
                  e => e.BudgetId,
                  b => b.BudgetId,
                  (e, b) => new { b.DepartmentId, e.Amount })
            .GroupBy(x => x.DepartmentId)
            .Select(g => new
            {
                DepartmentId = g.Key,
                TotalExpense = g.Sum(x => (decimal?)x.Amount) ?? 0m
            })
            .ToList();

        var result = departments.Select(d =>
        {
            var totalBudget = budgetsByDept.FirstOrDefault(x => x.DepartmentId == d.DepartmentId)?.TotalBudget ?? 0m;
            var totalExpense = expensesByDept.FirstOrDefault(x => x.DepartmentId == d.DepartmentId)?.TotalExpense ?? 0m;

            return new DepartmentReportDto
            {
                DepartmentId = d.DepartmentId,
                DepartmentName = d.DepartmentName,
                TotalBudget = totalBudget,
                TotalExpense = totalExpense,
                RemainingAmount = totalBudget - totalExpense
            };
        }).ToList();

        return result;
    }

    // All budgets report (active budgets only)
    public List<BudgetReportDto> GetAllBudgetsReport()
    {
        // Called by GET /api/report/budget (Admin portal)
        // Pre-aggregate expenses by budget for efficiency
        var expensesByBudget = _context.Expenses
            .Where(e => e.Status == ExpenseStatus.Approved)
            .GroupBy(e => e.BudgetId)
            .Select(g => new { BudgetId = g.Key, TotalExpense = g.Sum(e => (decimal?)e.Amount) ?? 0m })
            .ToList();

        var budgets = _context.Budgets
            .Include(b => b.Department)
            .ToList(); // materialize before projection

        var result = budgets.Select(b =>
        {
            var totalExpense = expensesByBudget.FirstOrDefault(x => x.BudgetId == b.BudgetId)?.TotalExpense ?? 0m;
            return new BudgetReportDto
            {
                BudgetId = b.BudgetId,
                Title = b.Title,
                DepartmentName = b.Department?.DepartmentName ?? "",
                AmountAllocated = b.AmountAllocated,
                TotalExpense = totalExpense,
                RemainingAmount = b.AmountAllocated - totalExpense
            };
        }).ToList();


        return result;
    }

    

    public SummaryReportDto GetSummaryReport()
    {
        // Called by GET /api/report/summary
        var totalBudget = _context.Budgets
            .Sum(b => (decimal?)b.AmountAllocated) ?? 0m;

        var totalExpense = _context.Expenses
            .Join(_context.Budgets,
                e => e.BudgetId,
                b => b.BudgetId,
                (e, b) => e.Amount)
            .Sum();

        return new SummaryReportDto
        {
            TotalBudget = totalBudget,
            TotalExpense = totalExpense,
            RemainingAmount = totalBudget - totalExpense
        };
    }

}
