
using InternalBudgetTracker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationController : ControllerBase
{
    private readonly NotificationService _service;

    public NotificationController(NotificationService service)
    {
        // DI: NotificationService injected (registered in Program.cs)
        _service = service;
    }

    // GET LOGIN USER NOTIFICATIONS + COUNT
    [HttpGet]
    public IActionResult GetNotifications()
    {
        // API: GET /api/notifications
        // Reads the current userId from the JWT token and returns unread notifications + count.
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
            return Unauthorized("UserId not found in token");

        int userId = int.Parse(userIdClaim.Value);
        Console.WriteLine("Token User Id=" + userId);

        var notifications= _service.GetNotifications(userId);
        var unreadCount= _service.GetUnreadCount(userId);

        return Ok(new
        {
            status = 200,
            unreadCount = unreadCount,
            data = notifications
        });
    }


    // MARK NOTIFICATION AS READ
    // PATCH: api/notifications/read/5
    [HttpPatch("read/{notificationId}")]
    public IActionResult MarkAsRead(int notificationId)
    {
        // API: PATCH /api/notifications/read/{notificationId}
        // Marks a single notification as read for the current logged-in user.
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
            return Unauthorized("UserId not found in token");

        int userId = int.Parse(userIdClaim.Value);

        bool success = _service.MarkAsRead(notificationId, userId);

        if (!success)
        {
            return NotFound(new
            {
                status = 404,
                message = "Invalid request"
            });
        }

        return Ok(new
        {
            status = 200,
            message = "success"
        });
    }


}
