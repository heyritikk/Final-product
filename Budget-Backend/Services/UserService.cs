
using InternalBudgetTracker.Data;
using InternalBudgetTracker.DTOs;
using InternalBudgetTracker.Enum;
using InternalBudgetTracker.Migrations;
using InternalBudgetTracker.Models;
using InternalBudgetTracker.Services;
using Microsoft.EntityFrameworkCore;
using System.Data;

public class UserService
{
    private readonly AppDbContext _context;
    private readonly HelperService _helperService;


    public UserService(AppDbContext context, HelperService helperService)
    {
        _context = context;
        _helperService = helperService;
    }

    private int EnsureValidDepartmentId(int requestedDepartmentId)
    {
        // If no departments exist yet, create a default one.
        if (!_context.Departments.Any())
        {
            var dept = new Department { DepartmentName = "General" };
            _context.Departments.Add(dept);
            _context.SaveChanges();
            return dept.DepartmentId;
        }

        // If client sent an invalid/empty department, fall back to the first available.
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

    

    public IEnumerable<Department> GetDepartments()
    {
        return _context.Departments.AsNoTracking().ToList();
    }

    public IEnumerable<object> GetManagers()
    {
        return _context.Users
            .AsNoTracking()
            .Where(u => u.Role.RoleName == "Manager" && u.Status == UserStatus.Active)
            .Select(u => new
            {
                userId = u.UserId,
                name = u.Name,
                email = u.Email,
                role = u.Role.RoleName
            })
            .ToList();
    }




    // EMPLOYEE REGISTRATION
    public string RegisterEmployee(UserRegisterDTO dto)
    {
        if (_context.Users.Any(u => u.Email == dto.Email))
            throw new Exception("Email already exists");

        var role = _context.Roles.FirstOrDefault(r => r.RoleName == "Employee");
        if (role == null)
        {
            role = new Role
            {
                RoleName = "Employee"
            };

        }
            var user = new User
        {
            Name = dto.Name,
            Email = dto.Email,
            Password = _helperService.GenerateHashPassword(dto.Password),
            RoleId = role.RoleId,
            Status = UserStatus.Active,
            IsVerified = false,
            DepartmentId = EnsureValidDepartmentId(dto.DepartmentId)
        };

        _context.Users.Add(user);
        _context.SaveChanges();

        string token = _helperService.GenerateToken(
            user.UserId,
            user.Email,
            user.Role.ToString()
        );

        //EmailService.SendVerificationMail(user.Email, token);

        return "Registration Succesfully done!";
    }

    
    // MANAGER REGISTRATION

    public string RegisterManager(UserRegisterDTO dto)
    {
        if (_context.Users.Any(u => u.Email == dto.Email))
            throw new Exception("Email already exists");

        //Ensure role exists
        var role = _context.Roles.FirstOrDefault(r => r.RoleName == "Manager");
        if(role==null)
        {
            role = new Role
            {
                RoleName = "Manager"
            };
            _context.Roles.Add(role);
            _context.SaveChanges();
        }

        var user = new User
        {
            Name = dto.Name,
            Email = dto.Email,
            Password = _helperService.GenerateHashPassword(dto.Password),
            RoleId = role.RoleId,
            Status = UserStatus.Active,
            IsVerified = false,
            DepartmentId = EnsureValidDepartmentId(dto.DepartmentId)
        };

        _context.Users.Add(user);
        _context.SaveChanges();

        string token = _helperService.GenerateToken(
            user.UserId,
            user.Email,
            //role.RoleName
            user.Role.ToString()
        );

        //EmailService.SendVerificationMail(user.Email, token);

        return "Registration Succesfully done!";
    }

    
    // VERIFY USER (EMAIL)
 
    public string VerifyUser(string token)
    {
        dynamic result = _helperService.CheckValidToken(token);

        if (!result.valid)
            throw new Exception(result.error);

        string email = result.data.email;

        if (string.IsNullOrWhiteSpace(email)) throw new Exception("Email not found in token");

        var user = _context.Users.FirstOrDefault(u => u.Email.ToLower() == email.ToLower());
        if (user == null)
            throw new Exception("Invalid User");

        user.IsVerified = true;
        _context.SaveChanges();

        return "Successfully verified";
    }

   
    // LOGIN USER
    
    public Dictionary<string, string> Login(UserLoginDTO dto)
    {
        string hashPassword =
            _helperService.GenerateHashPassword(dto.Password);

    var user = _context.Users.Include(u=>u.Role)
            .FirstOrDefault(
        u => u.Email.ToLower() == dto.Email.ToLower() && u.Password == hashPassword
    );

        if (user == null)
            throw new Exception("Invalid username or password");

        //if (!user.IsVerified)
        //    throw new Exception("Please verify your email first");

    var token = _helperService.GenerateToken(
        user.UserId,
        user.Email,
        
        user.Role.RoleName
        
    );
        
         return new Dictionary<string, string>
         {
             {"token",token.ToString() },
             {"role",user.Role.RoleName},
             {"email",user.Email },
             {"userId",user.UserId.ToString() },
             {"name",user.Name }



         };
    }

    public string ValidateForgotPasswordEmail(ForgotPasswordEmailDTO dto)
    {
        Console.WriteLine("DTO", dto);
        Console.WriteLine(dto?.Email);
        if (string.IsNullOrWhiteSpace(dto.Email))
            throw new Exception("Please enter your email");

        var normalizedEmail = dto.Email.Trim().ToLower();
        var user = _context.Users.FirstOrDefault(u => u.Email.Trim().ToLower() == normalizedEmail);
        if (user == null)
            throw new Exception("Invalid email");

        return "Email is valid";
    }

    public string ResetPassword(ResetPasswordDTO dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email))
            throw new Exception("Please enter your email");

        if (string.IsNullOrWhiteSpace(dto.NewPassword))
            throw new Exception("Please enter a new password");

        var normalizedEmail = dto.Email.ToLower();
        var user = _context.Users.FirstOrDefault(u => u.Email.ToLower() == normalizedEmail);
        if (user == null)
            throw new Exception("Invalid email");

        user.Password = _helperService.GenerateHashPassword(dto.NewPassword);
        _context.SaveChanges();
        return "Password updated successfully";
    }

}

