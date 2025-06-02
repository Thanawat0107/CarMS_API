using CarMS_API.Models.Dto;
using CarMS_API.Models.Responsts;
using CarMS_API.Models;
using CarMS_API.Repositorys.IRepositorys;
using CarMS_API.Utility;
using Microsoft.AspNetCore.Identity;
using CarMS_API.Data;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace CarMS_API.Repositorys
{
    public class AuthRepository : IAuthRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly RoleSeeder _roleSeeder;
        private readonly IConfiguration _config;

        public AuthRepository(
            ApplicationDbContext db,
            IConfiguration config,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            RoleSeeder roleSeeder)
        {
            _db = db;
            _config = config;
            _userManager = userManager;
            _roleManager = roleManager;
            _roleSeeder = roleSeeder;
        }

        public async Task<RegisterResponse> RegisterAsync(RegisterDto model)
        {
            var existingUser = await _db.ApplicationUsers
                .FirstOrDefaultAsync(u => u.UserName.ToLower() == model.UserName.ToLower());

            if (existingUser != null)
                throw new Exception("ชื่อผู้ใช้นี้มีอยู่แล้ว");

            var existingEmail = await _db.ApplicationUsers
                .FirstOrDefaultAsync(u => u.Email.ToLower() == model.Email.ToLower());

            if (existingEmail != null)
                throw new Exception("อีเมลนี้ถูกใช้งานแล้ว");

            var newUser = new ApplicationUser
            {
                Email = model.Email,
                UserName = model.UserName,
                FullName = model.FullName,
                PhoneNumber = model.PhoneNumber,
                CreatedAt = model.CreatedAt,
            };

            try
            {
                var result = await _userManager.CreateAsync(newUser, model.Password);
                if (!result.Succeeded)
                    throw new Exception(string.Join("; ", result.Errors.Select(e => e.Description)));

                await _roleSeeder.SeedRolesAsync();

                string userRole = string.IsNullOrEmpty(model.Role) ? SD.Role_Buyer : model.Role.ToLower();
                string selectedRole = SD.Role_Buyer;

                if (userRole == SD.Role_Admin)
                    selectedRole = SD.Role_Admin;
                else if (userRole == SD.Role_Seller)
                    selectedRole = SD.Role_Seller;

                await _userManager.AddToRoleAsync(newUser, selectedRole);

                return new RegisterResponse
                {
                    UserName = newUser.UserName,
                    Email = newUser.Email,
                    FullName = newUser.FullName,
                    PhoneNumber = newUser.PhoneNumber,
                    Role = selectedRole
                };
            }
            catch (Exception ex)
            {
                throw new Exception("เกิดข้อผิดพลาดในการสมัครสมาชิก: " + ex.Message);
            }
        }

        public async Task<LoginResponse> LoginAsync(LoginDto model)
        {
            var user = await _db.ApplicationUsers
                .FirstOrDefaultAsync(u => u.UserName.ToLower() == model.UserName.ToLower());

            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
                throw new Exception("ชื่อผู้ใช้หรือรหัสผ่านไม่ถูกต้อง");

            var roles = await _userManager.GetRolesAsync(user);
            var key = Encoding.ASCII.GetBytes(_config["ApiSettings:Secret"]);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("userId", user.Id),
                    new Claim("userName", user.UserName),
                    new Claim("fullName", user.FullName),
                    new Claim("phoneNumber", user.PhoneNumber),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, roles.FirstOrDefault() ?? SD.Role_Buyer)
            }),
                Expires = DateTime.UtcNow.AddHours(3),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                )
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return new LoginResponse
            {
                Email = user.Email,
                Token = tokenHandler.WriteToken(token)
            };
        }
    }
}
