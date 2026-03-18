namespace InternalBudgetTracker.DTOs
{
    public class BudgetCreateDTO
    {
        public string Title { get; set; }
        public decimal AmountAllocated { get; set; }
        //public DateTime StartDate { get; set; }
        //public DateTime? EndDate { get; set; }
        public int DepartmentId { get; set; }   
    }
}
