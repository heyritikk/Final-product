using InternalBudgetTracker.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace InternalBudgetTracker.Data
{
    public class AppDbContext:DbContext
    {
        
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<Budget> Budgets { get; set; }
        public DbSet<ExpenseApproval> ExpenseApprovals { get; set; }
        public DbSet<Notification> Notifications { get; set; }
         public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Role> Roles { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            foreach (var foreignKey in modelBuilder.Model
                .GetEntityTypes()
                .SelectMany(e => e.GetForeignKeys()))
            {
                foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
            }
            //Configure Role Table name and seed default roles if not present
            modelBuilder.Entity<Role>().ToTable("t_Role");
            modelBuilder.Entity<Notification>().ToTable("t_Notification");
            modelBuilder.Entity<Role>().HasData(
                new Role { RoleId = 1, RoleName = "Admin" },
                new Role { RoleId = 2, RoleName = "Manager" },
                new Role { RoleId = 3, RoleName = "Employee" }
                );
            
            

            base.OnModelCreating(modelBuilder);
        }


    }
}
