using DomainLayer.Entities;

namespace InfrastructureLayer.Repository
{
    public interface IUserRepository
    {
        Task<Users?> GetByUsernameAsync(string username);
        Task<Users?> GetByEmailAsync(string email);
        Task<Users?> GetByIdAsync(Guid userId);
        Task<List<Users>> GetAllMembersAsync();
        Task<Users> CreateAsync(Users user);
        Task<Users> UpdateAsync(Users user);
        Task<bool> IsUsernameExistsAsync(string username);
        Task<bool> IsEmailExistsAsync(string email);

        Task<bool> IsIdentityCardExistsAsync(string identityCard);
        Task<bool> IsPhoneExistsAsync(string phone);

        Task<Users?> SearchCustomerAsync(string searchTerm);
        Task<int> GetTotalBookingsAsync(Guid userId);
        Task<DateTime?> GetLastBookingDateAsync(Guid userId);
        
        // Dashboard statistics
        Task<int> GetUserCountAsync();
        Task<double> GetUserGrowthAsync();
    }
} 