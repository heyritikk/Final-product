using Microsoft.AspNetCore.Http;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using InternalBudgetTracker.Data;
using InternalBudgetTracker.Models;
using InternalBudgetTracker.Services;

namespace InternalBudgetTracker.Middleware
{
    public class AuditLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public AuditLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, AppDbContext db, HelperService helperService)
        {
            if (!context.Request.Path.StartsWithSegments("/api"))
            {
                await _next(context);
                return;
            }

            var ip = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

            var authHeader = context.Request.Headers["Authorization"].ToString();
            string token = null;

            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                token = authHeader.Split(" ")[1];
            }

            // TOKEN MISSING
            if (string.IsNullOrEmpty(token))
            {
                await SaveLog(db, null, null,"Token Missing", context, ip, 401);

                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";

                await context.Response.WriteAsync(JsonSerializer.Serialize(new
                {
                    status = 401,
                    message = "Token Missing"
                }));

                return;
            }

            dynamic response = helperService.CheckValidToken(token);

            // INVALID TOKEN
            if (!response.valid)
            {
                await SaveLog(db, null, null, "Invalid token",context, ip, 401);

                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";

                await context.Response.WriteAsync(JsonSerializer.Serialize(new
                {
                    status = 401,
                    message = "Invlid Token"
                }));

                return;
            }

            // VALID TOKEN
            var email = response.data.email;
            var userId = Convert.ToInt32(response.data.userId);

            context.Items["email"] = email;
            context.Items["userId"] = userId;

            await _next(context);

            await SaveLog(db, userId, email, GetMessageByStatus(context.Response.StatusCode), context, ip, context.Response.StatusCode);
        }

        private async Task SaveLog(
            AppDbContext db,
            int? userId,
            string ?email,
            string? message,
            HttpContext context,
            string ip,
            int statusCode)
        {
            var log = new AuditLog
            {
                LoginUserId = userId,
                Email = email,
                Path = context.Request.Path,
                Message = message,
                Method = context.Request.Method,
                StatusCode = statusCode,
                IpAddress = ip,
                CreatedAt = DateTime.Now
            };

            db.AuditLogs.Add(log);
            await db.SaveChangesAsync();
        }

        private string GetMessageByStatus(int statusCode)
        {
            return statusCode switch
            {
                200 => "Success",
                201 => "Created Successfully",
                400 => "Bad Request",
                401 => "Unauthorized",
                403 => "Forbidden",
                404 => "Not Found",
                500 => "Internal Server Error",
                _ => "Processed"
            };
        }
    }
}