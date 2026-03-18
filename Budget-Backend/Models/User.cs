    using InternalBudgetTracker.Enum;
    using Microsoft.AspNetCore.Identity;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    namespace InternalBudgetTracker.Models
            {
                [Table("t_User")]  // Table name convention applied
                public class User
                {
                        [Key]
                      public int UserId { get; set; }

                        [Required]
                      public string Name { get; set; }

                        [Required]
                      public string Email { get; set; }

                        [Required]
                      public string Password { get; set; }
                       public bool IsVerified { get; set; } = false;  // Default false
               
                        //[Column(TypeName = "nvarchar(20)")]
                        //public UserRole Role { get; set; } = UserRole.Employee;

                         public int RoleId { get; set; }
                        public Role Role { get; set; }
                        [Column(TypeName = "nvarchar(20)")]
                        public UserStatus Status { get; set; } = UserStatus.Active;
                        //Foreign Key
                        [ForeignKey(nameof(Department))]
                        public int DepartmentId { get; set; }
                        public  Department Department { get; set; }


                    }
                }

