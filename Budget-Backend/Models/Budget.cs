using InternalBudgetTracker.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.Metadata;

namespace InternalBudgetTracker.Models
{
    [Table("t_Budget")]
    public class Budget
    {
        [Key]
        public int BudgetId { get; set; }

        public string Title { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountAllocated { get; set; }

        public DateTime StartDate { get; set; }= DateTime.Now;
        public DateTime? EndDate { get; set; }

        [Column(TypeName = "nvarchar(20)")]
        public BudgetStatus Status { get; set; } = BudgetStatus.Active;

        // Budget is created by a USER (Manager check controller/JWT me hoga)
        public int CreatedByUserId { get; set; }
        public User CreatedByUser { get; set; }
        public int DepartmentId { get;set; }
        public Department Department { get; set; }
        // One Budget → Many Expenses
        public ICollection<Expense> Expenses { get; set; }
    }
}
