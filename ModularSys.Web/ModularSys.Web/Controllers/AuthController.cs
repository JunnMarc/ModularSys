using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ModularSys.Core.DTOs;
using ModularSys.Core.Interfaces;
using ModularSys.Data.Common.Db;
using ModularSys.Data.Common.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace ModularSys.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ModularSysDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(ModularSysDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login([FromBody] LoginDto request)
        {
            var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Username == request.Username);
            if (user == null) return Unauthorized("Invalid credentials");
            
            bool valid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            if (!valid) return Unauthorized("Invalid credentials");

            var token = GenerateJwtToken(user);
            return Ok(new { token });
        }

        [HttpPost("register")]
        public async Task<ActionResult> Register([FromBody] RegisterDto request)
        {
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
                return BadRequest("Username taken");

            var customerRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Customer");
            if (customerRole == null) 
            {
                 customerRole = new Role { RoleName = "Customer", Description = "Portal User" };
                 _context.Roles.Add(customerRole);
                 await _context.SaveChangesAsync(); 
            }

            var customer = new ModularSys.Data.Common.Entities.CRM.Customer
            {
                CompanyName = request.FirstName + " " + request.LastName,
                ContactName = request.FirstName + " " + request.LastName,
                Email = request.Email,
                City = "Unknown",
                Country = "Philippines",
                Status = "Active",
                CreatedAt = DateTime.UtcNow
            };
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            var user = new User
            {
                Username = request.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 12),
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                RoleId = customerRole.RoleId,
                DepartmentId = 1,
                CustomerId = customer.Id,
                Role = customerRole,
                Department = await _context.Departments.FirstOrDefaultAsync() ?? new Department { DepartmentName = "Default" }
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("User registered successfully");
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role.RoleName)
            };
            
            if (user.CustomerId.HasValue)
            {
                claims.Add(new Claim("CustomerId", user.CustomerId.Value.ToString()));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "superSecretKey12345678901234567890"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
