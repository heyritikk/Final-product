namespace InternalBudgetTracker.DTOs
{
    public class DepartmentReportDto
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public decimal TotalBudget { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal RemainingAmount { get; set; }
    }
}
