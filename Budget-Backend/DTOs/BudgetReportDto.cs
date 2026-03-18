namespace InternalBudgetTracker.DTOs
{
    public class BudgetReportDto
    {
        public int BudgetId { get; set; }
        public string Title { get; set; }
        public string DepartmentName { get; set; }
        public decimal AmountAllocated { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal RemainingAmount { get; set; }
    }
}
