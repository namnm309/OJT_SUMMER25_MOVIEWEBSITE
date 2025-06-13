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
                .FirstOrDefaultAsync(u => u.UserId == userId && u.IsActive);
        }

        public async Task<List<Users>> GetAllMembersAsync()
        {
            return await _context.Users
                .Where(u => u.IsActive)
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
    }
} 