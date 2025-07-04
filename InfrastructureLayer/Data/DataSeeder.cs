using DomainLayer.Entities;
using DomainLayer.Enum;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace InfrastructureLayer.Data
{
    public static class DataSeeder
    {
        public static async Task SeedData(MovieContext context)
        {
            await SeedAdminUser(context);
        }
        
        public static async Task SeedAdminUser(MovieContext context)
        {
            try
            {
                Console.WriteLine(" Đang kiểm tra tài khoản admin...");
                // Kiểm tra xem đã có admin chưa
                var existingAdmin = await context.Users
                    .FirstOrDefaultAsync(u => u.Username == "admin");

                if (existingAdmin == null)
                {
                    Console.WriteLine("Đang tạo tài khoản admin mặc định...");
                    
                    var adminUser = new Users
                    {
                        Username = "admin",
                        Email = "admin@movieapp.com",
                        Password = BCrypt.Net.BCrypt.HashPassword("admin123"),
                        FullName = "Quản trị viên",
                        Address = "Hệ thống",
                        Phone = "0123456789",
                        IdentityCard = "ADMIN001",
                        Role = UserRole.Admin,
                        Score = 0.0,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    await context.Users.AddAsync(adminUser);
                    await context.SaveChangesAsync();
                    
                    Console.WriteLine("   Tài khoản admin đã được tạo thành công!");
                    Console.WriteLine("   Username: admin");
                    Console.WriteLine("   Password: admin123");
                }
                else
                {
                    // Update admin password if needed
                    Console.WriteLine("  Tài khoản admin đã tồn tại, đang cập nhật password...");
                    existingAdmin.Password = BCrypt.Net.BCrypt.HashPassword("admin123");
                    existingAdmin.UpdatedAt = DateTime.UtcNow;
                    
                    context.Users.Update(existingAdmin);
                    await context.SaveChangesAsync();
                    
                    Console.WriteLine(" Password admin đã được cập nhật!");
                    Console.WriteLine("   Username: admin");
                    Console.WriteLine("   Password: admin123");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi tạo tài khoản admin: {ex.Message}");
            }
        }
    }
}