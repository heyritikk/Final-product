//using InternalBudgetTracker.Data;
//using InternalBudgetTracker.Services;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.OpenApi;
//using Microsoft.AspNetCore.Authentication.JwtBearer;
////using Microsoft.OpenApi.Models;
//using Microsoft.IdentityModel.Tokens;
//using System.Text;
//using System.Security.Claims;
//using InternalBudgetTracker.Middleware;



//var builder = WebApplication.CreateBuilder(args);

//// Add services to the container.
////Dependency
//builder.Services.AddControllers();
//builder.Services.AddAuthentication(options =>
//{
//    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//})
//.AddJwtBearer(options =>
//{
//    options.TokenValidationParameters = new TokenValidationParameters
//    {
//        ValidateIssuer = false,
//        ValidateAudience = false,
//        ValidateLifetime = true,
//        ValidateIssuerSigningKey = true,

//        NameClaimType = ClaimTypes.Name,
//        RoleClaimType = ClaimTypes.Role,

//        IssuerSigningKey = new SymmetricSecurityKey(
//            Encoding.UTF8.GetBytes(builder.Configuration["Security:SecretKey"])
//        )
//    };

//});

//builder.Services.AddAuthorization();

//// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen(); // Swashbuckle





//// DB Context
//builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

////Services
//builder.Services.AddScoped<UserService>();
//builder.Services.AddScoped<EmailService>();
//builder.Services.AddScoped<HelperService>();
//builder.Services.AddScoped<BudgetService>();
//builder.Services.AddScoped<ExpenseService>();
//builder.Services.AddScoped<NotificationService>();
//builder.Services.AddScoped<ReportService>();
//var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.MapOpenApi();
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//app.UseHttpsRedirection();
//app.UseAuthentication();

//app.UseAuthorization();
////Register Middleware
//app.UseMiddleware<AuditLoggingMiddleware>();

//app.MapControllers();

//app.Run();

using InternalBudgetTracker.Data;
using InternalBudgetTracker.Middleware;
using InternalBudgetTracker.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// =======================
// Controllers
// =======================
builder.Services.AddControllers();

// =======================
// DB Context
// =======================
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// =======================
// Services (DI / Dependency Injection)
// These classes are injected into Controllers/Services via constructors.
// Example: ExpenseController -> ExpenseService (injected automatically by ASP.NET Core).
// =======================
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<HelperService>();
builder.Services.AddScoped<BudgetService>();
builder.Services.AddScoped<ExpenseService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<ReportService>();


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// =======================
// JWT Authentication
// =======================
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        NameClaimType = System.Security.Claims.ClaimTypes.Name,
        RoleClaimType = System.Security.Claims.ClaimTypes.Role,

        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Security:SecretKey"])
        )
    };
});

// =======================
// Authorization
// =======================
builder.Services.AddAuthorization();

// =======================
// Swagger + JWT Support
// =======================
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Internal Budget Tracker API",
        Version = "v1"
    });

  
    
    // v JWT configuration for Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter token like this: Bearer {your_token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

// =======================
// Middleware Pipeline
// =======================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

// JWT: reads Authorization: Bearer <token> and populates HttpContext.User
app.UseAuthentication();
app.UseAuthorization();
// Custom middleware (audit logs) runs after auth
app.UseMiddleware<AuditLoggingMiddleware>();


app.MapControllers();

app.Run();
