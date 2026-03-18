using InternalBudgetTracker.DTOs;
using InternalBudgetTracker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace InternalBudgetTracker.Controllers
{
   [ApiController]
    [Route("api/expense")]
    [Authorize]// any logged-in user
    public class ExpenseController : ControllerBase
    {
        private readonly ExpenseService _expenseService;

        public ExpenseController(ExpenseService expenseService)
        {
            _expenseService = expenseService;
        }

        [Authorize(Roles ="Employee")]
        [HttpPost("create")]
        public IActionResult CreateExpense([FromBody] ExpenseCreateDTO dto)
        {
            try
            {
                var result = _expenseService.CreateExpense(dto, User);
                return Ok(new { message = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        //GET ALL / GET BY ID – any logged-in user
        [HttpGet]
        public IActionResult GetExpenses([FromQuery] int? expenseId)
        {
            var result = _expenseService.GetExpenses(expenseId, User);
            return Ok(result);
        }

        // ================= UPDATE (PATCH) =================
        // Only creator & Pending
        [Authorize(Roles = "Employee")]
        [HttpPatch("update/{expenseId}")]
        public IActionResult UpdateExpense(
            int expenseId,
            [FromBody] ExpenseUpdateDTO dto)
        {
            var result = _expenseService.UpdateExpense(expenseId, dto, User);
            return Ok(new { message = result });
        }

        // ================= DELETE (PATCH - SOFT DELETE) =================
        // Only creator & Pending
        [Authorize(Roles = "Employee")]
        [HttpPatch("delete/{expenseId}")]
        public IActionResult DeleteExpense(int expenseId)
        {
            var result = _expenseService.DeleteExpense(expenseId, User);
            return Ok(new { message = result });
        }

        //Approval_Reject API
        [Authorize(Roles = "Manager")]
        [HttpPatch("approve-reject/{expenseId}")]
        public IActionResult ApproveRejectExpense(
    int expenseId,
    [FromBody] ExpenseApprovalDTO dto)
        {
            var result = _expenseService
                .ApproveRejectExpense(expenseId, dto, User);

            return Ok(new { message = result });
        }
    }

}

