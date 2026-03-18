using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InternalBudgetTracker.Controllers
{
    [ApiController]
    [Route("api/report")]
   
    public class ReportController : ControllerBase
    {
        private readonly ReportService _reportService;

        public ReportController(ReportService reportService)
        {
            // DI: ReportService is injected (registered in Program.cs)
            _reportService = reportService;
        }

        

        [HttpGet("department")]
        [Authorize(Roles = "Admin")]
        public IActionResult DepartmentReport([FromQuery] int? departmentId)
        {
            // API: GET /api/report/department?departmentId=...
            // Used by Admin portal to render department totals and filter by department.
            try
            {
                var data = _reportService.GetDepartmentReport(departmentId);

                return Ok(new
                {
                    success = true,
                    message = "Department report fetched successfully",
                    data = data
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Something went wrong",
                    error = ex.Message
                });
            }
        }

        // All budgets report
        [HttpGet("budget")]
        [Authorize(Roles = "Admin")]

        public IActionResult BudgetReport()
        {
            // API: GET /api/report/budget
            return Ok(_reportService.GetAllBudgetsReport());
        }

        // Overall summary report
        [HttpGet("summary")]
        [Authorize(Roles = "Admin")]
        public IActionResult SummaryReport()
        {
            // API: GET /api/report/summary
            return Ok(_reportService.GetSummaryReport());
        }
    }
}
