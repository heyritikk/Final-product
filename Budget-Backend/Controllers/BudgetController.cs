using InternalBudgetTracker.DTOs;
using InternalBudgetTracker.Models;
using InternalBudgetTracker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InternalBudgetTracker.Controllers
{
    
     [ApiController]
    [Route("api/budget")]
    public class BudgetController : ControllerBase
    {
        private readonly BudgetService _budgetService;

        public BudgetController(BudgetService budgetService)
        {
            _budgetService = budgetService;
        }

        // 🔹 CREATE BUDGET (Manager)
        [Authorize(Roles = "Manager")]
        [HttpPost("create")]
        public IActionResult CreateBudget([FromBody] BudgetCreateDTO dto)
        {
            try

            {
                // User object automatically JWT se aata hai
                var result = _budgetService.CreateBudget(dto, User);

                return Ok(new
                {
                    status = 200,
                    message = result
                });
            }
            catch (Exception ex)
            {
                // Don't report every server error as 401; keep auth errors distinct.
                if (ex.Message.Contains("Invalid token", StringComparison.OrdinalIgnoreCase) ||
                    ex.Message.Contains("Invalid permission", StringComparison.OrdinalIgnoreCase))
                {
                    return Unauthorized(new { status = 401, message = ex.Message });
                }

                return StatusCode(500, new { status = 500, message = ex.Message });
            }
        }

        //Get All Active Budget
        //api/budget
        //api/budget?budgetId=5
        [HttpGet]
        public IActionResult GetBudgets([FromQuery]int? budgetId)
        {
            var data = _budgetService.GetBudgets(budgetId);
            return Ok(new { status = 200, data,message="success" });
        }

        [Authorize(Roles = "Manager")]
        [HttpPatch("update/{budgetId}")]
        
        public IActionResult UpdateBudget(int budgetId,[FromBody] BudgetUpdateDTO dto)
        {
            try
            {
                var result = _budgetService.UpdateBudget(
                    budgetId,
                    dto,
                    User
                );

                return Ok(new
                {
                    status = 200,
                    message = result
                });
            }
            catch (Exception ex)
            {
                return Unauthorized(new
                {
                    status = 401,
                    error = ex.Message
                });
            }
        }


        //Delete Budget
        [Authorize(Roles = "Manager")]
        [HttpPatch("delete/{budgetId}")]
        public IActionResult DeleteBudget(int budgetId)
        {
            try
            {
                var result = _budgetService.DeleteBudget(budgetId, User);

                return Ok(new
                {
                    status = 200,
                    message = result
                });
            }
            catch (Exception ex)
            {
                return Unauthorized(new
                {
                    status = 401,
                    error = ex.Message
                });
            }
        }


    }

}

