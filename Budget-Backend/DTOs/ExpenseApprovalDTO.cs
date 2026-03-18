namespace InternalBudgetTracker.DTOs
{
    public class ExpenseApprovalDTO
    {
        public string Action { get; set; } // "Approve" or "Reject"
        public string Comment { get; set; }
    }
}
