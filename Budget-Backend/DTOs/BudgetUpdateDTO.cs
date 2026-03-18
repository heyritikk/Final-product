namespace InternalBudgetTracker.DTOs
{
    public class BudgetUpdateDTO
    {
        public string? Title { get; set; }
        public decimal? AmountAllocated { get; set; }
        public int? DepartmentId { get; set; }
    }

}
