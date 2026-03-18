using System.ComponentModel.DataAnnotations;

namespace InternalBudgetTracker.DTOs
{
    public class UserRegisterDTO
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public int DepartmentId { get; set; }
        //public string Role { get; set; } // manager/admin/employee
    }
}
