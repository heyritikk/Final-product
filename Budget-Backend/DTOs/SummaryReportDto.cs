namespace InternalBudgetTracker.DTOs
{
    public class SummaryReportDto
    {
        public decimal TotalBudget { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal RemainingAmount { get; set; }
    }
}
