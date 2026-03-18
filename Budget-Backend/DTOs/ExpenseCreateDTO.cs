namespace InternalBudgetTracker.DTOs
{
    public class ExpenseCreateDTO
    {
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public int BudgetId { get; set; }

        // Employee selects manager
        public int ManagerId { get; set; }
    }
}
