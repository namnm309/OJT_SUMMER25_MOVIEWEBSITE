using DomainLayer.Entities;
using DomainLayer.Enum;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace InfrastructureLayer.Data
{
    public static class DataSeeder
    {
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
                    // Cập nhật mật khẩu admin nếu cần
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

        public static async Task SeedSampleUsers(MovieContext context)
        {
            try
            {
                Console.WriteLine("👥 Đang tạo dữ liệu người dùng mẫu...");

                // Kiểm tra xem đã có user mẫu chưa (ngoài admin)
                var userCount = await context.Users.CountAsync(u => u.Role != UserRole.Admin);
                
                if (userCount == 0)
                {
                    var sampleUsers = new List<Users>
                    {
                        new Users
                        {
                            Id = Guid.NewGuid(),
                            Username = "staff001",
                            Email = "staff001@example.com",
                            Password = BCrypt.Net.BCrypt.HashPassword("password123"),
                            FullName = "Nguyễn Văn Test",
                            Address = "123 Đường Test, Quận 1, TP.HCM",
                            Phone = "0901234567",
                            IdentityCard = "123456789",
                            Role = UserRole.Staff,
                            Score = 0.0,
                            Gender = UserGender.Male,
                            BirthDate = DateTime.Now.AddYears(-25),
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow.AddDays(-10),
                            UpdatedAt = DateTime.UtcNow.AddDays(-10)
                        },
                        new Users
                        {
                            Id = Guid.NewGuid(),
                            Username = "member001",
                            Email = "nguyenminhan306@gmail.com",
                            Password = BCrypt.Net.BCrypt.HashPassword("password123"),
                            FullName = "Nguyễn Văn Test",
                            Address = "456 Đường Example, Quận 2, TP.HCM",
                            Phone = "0907654321",
                            IdentityCard = "987654321",
                            Role = UserRole.Member,
                            Score = 150.0,
                            Gender = UserGender.Female,
                            BirthDate = DateTime.Now.AddYears(-22),
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow.AddDays(-8),
                            UpdatedAt = DateTime.UtcNow.AddDays(-8)
                        },
                        new Users
                        {
                            Id = Guid.NewGuid(),
                            Username = "member002",
                            Email = "nguyenminhan306@gmail.com",
                            Password = BCrypt.Net.BCrypt.HashPassword("password123"),
                            FullName = "Nguyễn Văn Test",
                            Address = "789 Đường Demo, Quận 3, TP.HCM",
                            Phone = "0909876543",
                            IdentityCard = "555666777",
                            Role = UserRole.Member,
                            Score = 75.0,
                            Gender = UserGender.Male,
                            BirthDate = DateTime.Now.AddYears(-30),
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow.AddDays(-5),
                            UpdatedAt = DateTime.UtcNow.AddDays(-5)
                        },
                        new Users
                        {
                            Id = Guid.NewGuid(),
                            Username = "admin_test",
                            Email = "admintest@movieapp.com",
                            Password = BCrypt.Net.BCrypt.HashPassword("password123"),
                            FullName = "Quản trị viên Test",
                            Address = "Hệ thống Test",
                            Phone = "0912345678",
                            IdentityCard = "ADMIN002",
                            Role = UserRole.Admin,
                            Score = 0.0,
                            Gender = UserGender.Male,
                            BirthDate = DateTime.Now.AddYears(-35),
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow.AddDays(-7),
                            UpdatedAt = DateTime.UtcNow.AddDays(-7)
                        }
                    };

                    await context.Users.AddRangeAsync(sampleUsers);
                    await context.SaveChangesAsync();

                    Console.WriteLine("✅ Dữ liệu người dùng mẫu đã được tạo:");
                    Console.WriteLine("   - Staff: staff001 / password123");
                    Console.WriteLine("   - Member: member001 / password123");
                    Console.WriteLine("   - Member: member002 / password123");
                    Console.WriteLine("   - Admin Test: admin_test / password123");
                }
                else
                {
                    Console.WriteLine("  Dữ liệu người dùng mẫu đã tồn tại");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi khi tạo dữ liệu người dùng mẫu: {ex.Message}");
            }
        }
    }
} 