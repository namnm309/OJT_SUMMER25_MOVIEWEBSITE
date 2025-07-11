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

        public static async Task SeedSampleData(MovieContext context)
        {
            try
            {
                Console.WriteLine("🎬 Đang tạo dữ liệu mẫu...");

                // 1. Tạo Genres trước
                if (!await context.Genres.AnyAsync())
                {
                    Console.WriteLine("  Tạo thể loại phim...");
                    var genres = new List<Genre>
                    {
                        new Genre { Id = Guid.NewGuid(), GenreName = "Hành động", Description = "Phim hành động" },
                        new Genre { Id = Guid.NewGuid(), GenreName = "Khoa học viễn tưởng", Description = "Phim khoa học viễn tưởng" },
                        new Genre { Id = Guid.NewGuid(), GenreName = "Kinh dị", Description = "Phim kinh dị" },
                        new Genre { Id = Guid.NewGuid(), GenreName = "Hài kịch", Description = "Phim hài kịch" },
                        new Genre { Id = Guid.NewGuid(), GenreName = "Lãng mạn", Description = "Phim lãng mạn" },
                        new Genre { Id = Guid.NewGuid(), GenreName = "Thần thoại", Description = "Phim thần thoại" }
                    };

                    await context.Genres.AddRangeAsync(genres);
                    await context.SaveChangesAsync();
                }

                // 2. Tạo Cinema Rooms
                if (!await context.CinemaRooms.AnyAsync())
                {
                    Console.WriteLine("  Tạo phòng chiếu...");
                    var room1 = new CinemaRoom
                    {
                        Id = Guid.NewGuid(),
                        RoomName = "Phòng A1",
                        TotalSeats = 50,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    var room2 = new CinemaRoom
                    {
                        Id = Guid.NewGuid(),
                        RoomName = "Phòng B1",
                        TotalSeats = 60,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    await context.CinemaRooms.AddRangeAsync(room1, room2);
                    await context.SaveChangesAsync();

                    // 3. Tạo Seats cho từng phòng
                    Console.WriteLine("  Tạo ghế...");
                    var seats = new List<Seat>();

                    // Phòng A1: 5 hàng x 10 ghế
                    for (int row = 1; row <= 5; row++)
                    {
                        string rowLetter = ((char)('A' + row - 1)).ToString();
                        for (int col = 1; col <= 10; col++)
                        {
                            seats.Add(new Seat
                            {
                                Id = Guid.NewGuid(),
                                SeatCode = $"{rowLetter}{col}",
                                RoomId = room1.Id,
                                SeatType = col >= 8 ? SeatType.VIP : SeatType.Normal,
                                RowIndex = row,
                                ColumnIndex = col,
                                PriceSeat = col >= 8 ? 80000 : 50000,
                                IsActive = true,
                                CreatedAt = DateTime.UtcNow
                            });
                        }
                    }

                    // Phòng B1: 6 hàng x 10 ghế
                    for (int row = 1; row <= 6; row++)
                    {
                        string rowLetter = ((char)('A' + row - 1)).ToString();
                        for (int col = 1; col <= 10; col++)
                        {
                            seats.Add(new Seat
                            {
                                Id = Guid.NewGuid(),
                                SeatCode = $"{rowLetter}{col}",
                                RoomId = room2.Id,
                                SeatType = col >= 8 ? SeatType.VIP : SeatType.Normal,
                                RowIndex = row,
                                ColumnIndex = col,
                                PriceSeat = col >= 8 ? 90000 : 60000,
                                IsActive = true,
                                CreatedAt = DateTime.UtcNow
                            });
                        }
                    }

                    await context.Seats.AddRangeAsync(seats);
                    await context.SaveChangesAsync();
                }

                // 4. Tạo Movies
                if (!await context.Movies.AnyAsync())
                {
                    Console.WriteLine("  Tạo phim...");
                    var movie1 = new Movie
                    {
                        Id = Guid.NewGuid(),
                        Title = "Tenet",
                        Content = "Một bộ phim khoa học viễn tưởng hành động về thời gian",
                        RunningTime = 150,
                        Director = "Christopher Nolan",
                        Actors = "John David Washington, Robert Pattinson",
                        ProductionCompany = "Warner Bros",
                        ReleaseDate = DateTime.Now.AddDays(-30),
                        EndDate = DateTime.Now.AddDays(30),
                        Status = MovieStatus.NowShowing,
                        TrailerUrl = "https://youtube.com/watch?v=example",
                        IsRecommended = true,
                        IsFeatured = true,
                        Rating = 8.5,
                        Version = MovieVersion.TwoD,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    await context.Movies.AddAsync(movie1);
                    await context.SaveChangesAsync();

                    // Thêm Genre cho Movie
                    var sciFiGenre = await context.Genres.FirstOrDefaultAsync(g => g.GenreName == "Khoa học viễn tưởng");
                    if (sciFiGenre != null)
                    {
                        var movieGenre = new MovieGenre
                        {
                            MovieId = movie1.Id,
                            GenreId = sciFiGenre.Id
                        };
                        await context.MovieGenres.AddAsync(movieGenre);
                        await context.SaveChangesAsync();
                    }

                    // 5. Tạo ShowTimes
                    Console.WriteLine("  Tạo lịch chiếu...");
                    var room1Id = await context.CinemaRooms.Where(r => r.RoomName == "Phòng A1").Select(r => r.Id).FirstOrDefaultAsync();
                    var room2Id = await context.CinemaRooms.Where(r => r.RoomName == "Phòng B1").Select(r => r.Id).FirstOrDefaultAsync();

                    var showTimes = new List<ShowTime>();
                    
                    // Tạo suất chiếu cho 3 ngày tới
                    for (int day = 0; day < 3; day++)
                    {
                        var showDate = DateTime.Today.AddDays(day);
                        
                        // Mỗi ngày có 4 suất chiếu với thời gian khác nhau
                        var times = new[] { 
                            showDate.AddHours(9),   // 09:00
                            showDate.AddHours(14),  // 14:00  
                            showDate.AddHours(19),  // 19:00
                            showDate.AddHours(22)   // 22:00
                        };
                        
                        foreach (var showDateTime in times)
                        {
                            showTimes.Add(new ShowTime
                            {
                                Id = Guid.NewGuid(),
                                MovieId = movie1.Id,
                                RoomId = day % 2 == 0 ? room1Id : room2Id, // Xen kẽ giữa 2 phòng
                                ShowDate = showDateTime, // ShowDate bây giờ bao gồm cả ngày và giờ
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow
                            });
                        }
                    }

                    await context.ShowTimes.AddRangeAsync(showTimes);
                    await context.SaveChangesAsync();
                }

                Console.WriteLine("✅ Dữ liệu mẫu đã được tạo thành công!");
                Console.WriteLine("   - Phòng chiếu: A1 (50 ghế), B1 (60 ghế)");
                Console.WriteLine("   - Phim: Tenet");
                Console.WriteLine("   - Lịch chiếu: 3 ngày x 4 suất/ngày");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi khi tạo dữ liệu mẫu: {ex.Message}");
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