using InternalBudgetTracker.Data;
using InternalBudgetTracker.DTOs;
using InternalBudgetTracker.Enum;
using InternalBudgetTracker.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Security.Claims;

namespace InternalBudgetTracker.Services
{
     public class BudgetService
    {
        private readonly AppDbContext _context;
        private readonly string _connectionString;

        public BudgetService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        private int EnsureValidDepartmentId(int requestedDepartmentId)
        {
            if (!_context.Departments.Any())
            {
                var dept = new Department { DepartmentName = "General" };
                _context.Departments.Add(dept);
                _context.SaveChanges();
                return dept.DepartmentId;
            }

            if (requestedDepartmentId <= 0 || !_context.Departments.Any(d => d.DepartmentId == requestedDepartmentId))
            {
                return _context.Departments
                    .AsNoTracking()
                    .OrderBy(d => d.DepartmentId)
                    .Select(d => d.DepartmentId)
                    .First();
            }

            return requestedDepartmentId;
        }

        public string CreateBudget(BudgetCreateDTO dto, ClaimsPrincipal user)
        {
            //get data from token
            //Console.WriteLine(user);
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
             var roleClaim = user.FindFirst(ClaimTypes.Role);
            var emailClaim = user.FindFirst(ClaimTypes.Email);
           
            if ( userIdClaim==null ||roleClaim == null)
                throw new Exception("Invalid token");

            if (roleClaim.Value != "Manager")
                throw new Exception("Invalid permission");

            var userId = int.Parse(userIdClaim.Value);

            

            //By storedProcedure
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("sp_CreateBudget", conn);

            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@Title", dto.Title);
            cmd.Parameters.AddWithValue("@AmountAllocated", dto.AmountAllocated);
            cmd.Parameters.AddWithValue("@CreatedByUserId", userId);
            cmd.Parameters.AddWithValue("@DepartmentId", EnsureValidDepartmentId(dto.DepartmentId));

            conn.Open();


            // ⭐ BudgetId return hoga
            var budgetId = Convert.ToInt32(cmd.ExecuteScalar());

            conn.Close();

            return $"Budget Created Successfully. Id = {budgetId}";
        }

        //get budget by id or all
        public List<Budget> GetBudgets(int? budgetId)
        {
            

          
            //By StoredProcedure
           var param = new SqlParameter("@BudgetId",
                         budgetId.HasValue ? budgetId.Value : (object)DBNull.Value);

            var budgets = _context.Budgets
                .FromSqlRaw("EXEC sp_GetBudgets @BudgetId", param)
                .ToList();

            return budgets;
        
    }
        

        //Service to Update Budget
        public string UpdateBudget(int budgetId,BudgetUpdateDTO dto,ClaimsPrincipal user )
        {
            // Token se data
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            var roleClaim = user.FindFirst(ClaimTypes.Role);
            var emailClaim = user.FindFirst(ClaimTypes.Email);

            if (userIdClaim == null || roleClaim == null)
                throw new Exception("Invalid token");

            if (roleClaim.Value != "Manager")
                throw new Exception("Invalid permission");

            int userId = int.Parse(userIdClaim.Value);

            //  Budget fetch (same user)
            var budget = _context.Budgets.FirstOrDefault(b =>
                b.BudgetId == budgetId &&
                b.CreatedByUserId == userId &&
                b.EndDate == null &&
                b.Status == BudgetStatus.Active
            );

            if (budget == null)
                throw new Exception("Invalid Budget ID or you did not create this budget");

            //  Partial update
            //if (dto.Title != null)
            //    budget.Title = dto.Title;

            //if (dto.AmountAllocated.HasValue)
            //    budget.AmountAllocated = dto.AmountAllocated.Value;

            //if (dto.DepartmentId.HasValue)
            //    budget.DepartmentId = dto.DepartmentId.Value;

            //_context.SaveChanges();

            //return "success";


         _context.Database.ExecuteSqlInterpolated($@"
        EXEC sp_UpdateBudget
        @BudgetId = {budgetId},
        @Title = {dto.Title},
        @AmountAllocated = {dto.AmountAllocated},
        @DepartmentId = {dto.DepartmentId}
    ");

            return "Budget Updated Successfully";
        }
        


        //Service to delete Budget
        public string DeleteBudget(int budgetId, ClaimsPrincipal user)
        {
            // 1️ Token se data nikalna
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            var roleClaim = user.FindFirst(ClaimTypes.Role);
            var emailClaim = user.FindFirst(ClaimTypes.Email);

            if (userIdClaim == null || roleClaim == null)
                throw new Exception("Invalid token");

            if (roleClaim.Value != "Manager")
                throw new Exception("Invalid permission");

            int userId = int.Parse(userIdClaim.Value);

            // 2️⃣ Budget find karo
            var budget = _context.Budgets
                .FirstOrDefault(b => b.BudgetId == budgetId && b.EndDate == null);
            if (budget == null)
                throw new Exception("Invalid budget id");

            // 3️Check: same user ne create kiya?
            if (budget.CreatedByUserId != userId)
                throw new Exception("You did not create this budget");

            // 4️ Soft delete
            //budget.Status = BudgetStatus.Closed;
            //budget.EndDate = DateTime.UtcNow;

            //_context.SaveChanges();

            //return "Budget deleted successfully";

            //Stored procedure call for soft delete

            _context.Database.ExecuteSqlInterpolated(
        $"EXEC sp_DeleteBudget @BudgetId = {budgetId}, @UserId = {userId}"
    );

            return "Budget deleted successfully";
        }
    }


    }







