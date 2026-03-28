using CarMS_API.Models; // อย่าลืม using Model ของ ApplicationUser
using Microsoft.AspNetCore.Identity;

namespace CarMS_API.Utility
{
    public class RoleSeeder
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        // Inject UserManager เข้ามาเพิ่ม
        public RoleSeeder(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task SeedRolesAndUsersAsync()
        {
            // 1. สร้าง Roles ก่อน
            string[] roles = { SD.Role_Admin, SD.Role_Seller, SD.Role_Buyer };

            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // 2. สร้างบัญชี Admin (ถ้ายูสเซอร์นี้ยังไม่มีในระบบ)
            if (await _userManager.FindByNameAsync("admin") == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = "admin",
                    Email = "admin@gmail.com",
                    FullName = "Administrator",
                    PhoneNumber = "0800000001",
                    CreatedAt = DateTime.UtcNow
                };
                // ใส่รหัสผ่านเริ่มต้น (ต้องตรงตาม Policy ของ Identity เช่น มีพิมพ์ใหญ่ เล็ก ตัวเลข อักขระพิเศษ)
                var result = await _userManager.CreateAsync(adminUser, "aa");
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(adminUser, SD.Role_Admin);
                }
            }

            // 3. สร้างบัญชี Seller
            if (await _userManager.FindByNameAsync("seller") == null)
            {
                var sellerUser = new ApplicationUser
                {
                    UserName = "seller",
                    Email = "seller@gmail.com",
                    FullName = "Test Seller",
                    PhoneNumber = "0800000002",
                    CreatedAt = DateTime.UtcNow
                };
                var result = await _userManager.CreateAsync(sellerUser, "aa");
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(sellerUser, SD.Role_Seller);
                }
            }

            // 4. สร้างบัญชี Buyer
            if (await _userManager.FindByNameAsync("buyer") == null)
            {
                var buyerUser = new ApplicationUser
                {
                    UserName = "buyer",
                    Email = "buyer@gmail.com",
                    FullName = "Test Buyer",
                    PhoneNumber = "0800000003",
                    CreatedAt = DateTime.UtcNow
                };
                var result = await _userManager.CreateAsync(buyerUser, "aa");
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(buyerUser, SD.Role_Buyer);
                }
            }
        }
    }
}