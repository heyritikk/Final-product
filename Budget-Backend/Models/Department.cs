using System.ComponentModel.DataAnnotations.Schema;

namespace InternalBudgetTracker.Models
{
    [Table("t_Department")]
    public class Department
    {
        public int DepartmentId {  get; set; }
        public string DepartmentName { get; set; }

        //One department multiple user
        public ICollection<User> Users { get; set; }
    }
}
