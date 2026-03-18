using InternalBudgetTracker.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InternalBudgetTracker.Models
{
    [Table("t_ExpenseApproval")]
    public class ExpenseApproval
    {
        [Key]
        public int ExpenseApprovalId { get; set; }

        //  Which expense  
        public int ExpenseId { get; set; }
        public Expense Expense { get; set; }

        // Which manager took action
        // FK- USER(Manager)
        public int ManagerId { get; set; }
        public User Manager { get; set; }

        // Approved / Rejected
        public ExpenseStatus Status { get; set; }=ExpenseStatus.Pending;

        // When manager asseined
        public DateTime StartDate { get; set; } = DateTime.Now;

        //when approve/reject
        public DateTime? EndDate { get; set; } 
        public string? Comment { get; set; }
    }
}
