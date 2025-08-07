using DomainLayer.Entities;
using InfrastructureLayer.Data;
using Microsoft.EntityFrameworkCore;

namespace InfrastructureLayer.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly MovieContext _context;

        public UserRepository(MovieContext context)
        {
            _context = context;
        }

        public async Task<Users?> GetByUsernameAsync(string username)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);
        }

        public async Task<Users?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);
        }

        public async Task<Users?> GetByIdAsync(Guid userId)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);
        }

        public async Task<List<Users>> GetAllMembersAsync()
        {
            return await _context.Users
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
        }

        public async Task<Users> CreateAsync(Users user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<Users> UpdateAsync(Users user)
        {
            user.UpdatedAt = DateTime.UtcNow;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> IsUsernameExistsAsync(string username)
        {
            return await _context.Users
                .AnyAsync(u => u.Username == username && u.IsActive);
        }

        public async Task<bool> IsEmailExistsAsync(string email)
        {
            return await _context.Users
                .AnyAsync(u => u.Email == email && u.IsActive);
        }

        public async Task<bool> IsPhoneExistsAsync(string phone)
        {
            return await _context.Users
                .AnyAsync(u => u.Phone == phone && u.IsActive);
        }

        public async Task<bool> IsIdentityCardExistsAsync(string identityCard)
        {
            return await _context.Users
                .AnyAsync(u => u.IdentityCard == identityCard && u.IsActive);
        }

        public async Task<Users?> SearchCustomerAsync(string searchTerm)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => 
                    (u.Phone == searchTerm || u.Email == searchTerm) && 
                    u.IsActive);
        }

        public async Task<int> GetTotalBookingsAsync(Guid userId)
        {
            return await _context.Bookings
                .CountAsync(b => b.UserId == userId);
        }

        public async Task<DateTime?> GetLastBookingDateAsync(Guid userId)
        {
            return await _context.Bookings
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => b.CreatedAt)
                .FirstOrDefaultAsync();
        }

        // Dashboard statistics
        public async Task<int> GetUserCountAsync()
        {
            return await _context.Users
                .Where(u => u.IsActive)
                .CountAsync();
        }

        public async Task<double> GetUserGrowthAsync()
        {
            try
            {
                var currentMonth = DateTime.Now.Month;
                var currentYear = DateTime.Now.Year;
                var lastMonth = currentMonth == 1 ? 12 : currentMonth - 1;
                var lastMonthYear = currentMonth == 1 ? currentYear - 1 : currentYear;

                // Đếm users tạo trong tháng hiện tại
                var currentMonthUsers = await _context.Users
                    .Where(u => u.IsActive && 
                               u.CreatedAt.Month == currentMonth && 
                               u.CreatedAt.Year == currentYear)
                    .CountAsync();

                // Đếm users tạo trong tháng trước
                var lastMonthUsers = await _context.Users
                    .Where(u => u.IsActive && 
                               u.CreatedAt.Month == lastMonth && 
                               u.CreatedAt.Year == lastMonthYear)
                    .CountAsync();

                // Tính phần trăm tăng trưởng
                if (lastMonthUsers == 0)
                {
                    return currentMonthUsers > 0 ? 100.0 : 0.0;
                }

                return Math.Round(((double)(currentMonthUsers - lastMonthUsers) / lastMonthUsers) * 100, 1);
            }
            catch (Exception)
            {
                return 0.0;
            }
        }
    }
}