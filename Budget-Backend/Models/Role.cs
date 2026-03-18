
using System.ComponentModel.DataAnnotations.Schema;

namespace InternalBudgetTracker.Models
{
    [Table("t_Role")]  // Table name convention applied
    public class Role
    {
       public int RoleId { get; set; }
       public string RoleName { get; set; }
     public ICollection<User> Users { get; set; }

    }
}
