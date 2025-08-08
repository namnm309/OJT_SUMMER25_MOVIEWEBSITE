using System;
using System.Threading.Tasks;
using System.Linq;
using System.Text.RegularExpressions;
using ApplicationLayer.DTO.UserManagement;
using InfrastructureLayer.Repository;

namespace ApplicationLayer.Services.UserManagement
{
    public class CustomerSearchService : ICustomerSearchService
    {
        private readonly IGenericRepository<DomainLayer.Entities.Users> _userRepository;
        private readonly IGenericRepository<DomainLayer.Entities.Booking> _bookingRepository;

        public CustomerSearchService(
            IGenericRepository<DomainLayer.Entities.Users> userRepository,
            IGenericRepository<DomainLayer.Entities.Booking> bookingRepository)
        {
            _userRepository = userRepository;
            _bookingRepository = bookingRepository;
        }

        public async Task<CustomerSearchDto> SearchCustomerAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                throw new ArgumentException("Từ khóa tìm kiếm không được để trống");
            }

            // Kiểm tra xem searchTerm có phải là email không
            if (IsValidEmail(searchTerm))
            {
                return await SearchCustomerByEmailAsync(searchTerm);
            }

            // Kiểm tra xem searchTerm có phải là số điện thoại không
            if (IsValidPhoneNumber(searchTerm))
            {
                return await SearchCustomerByPhoneAsync(searchTerm);
            }

            throw new ArgumentException("Từ khóa tìm kiếm không hợp lệ. Vui lòng nhập email hoặc số điện thoại.");
        }

        public async Task<CustomerSearchDto> SearchCustomerByPhoneAsync(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                throw new ArgumentException("Số điện thoại không được để trống");
            }

            if (!IsValidPhoneNumber(phoneNumber))
            {
                throw new ArgumentException("Số điện thoại không hợp lệ");
            }

            var user = await _userRepository.FindAsync(u => u.Phone == phoneNumber);

            if (user == null)
            {
                return null;
            }

            var totalBookings = await _bookingRepository.CountAsync(b => b.UserId == user.Id);
            var lastBooking = await _bookingRepository
                .FindAsync(b => b.UserId == user.Id);

            return new CustomerSearchDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.Phone ?? string.Empty,
                Points = (int)Math.Round(user.Score),
                LastBookingDate = lastBooking?.CreatedAt,
                TotalBookings = totalBookings
            };
        }

        public async Task<CustomerSearchDto> SearchCustomerByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Email không được để trống");
            }

            if (!IsValidEmail(email))
            {
                throw new ArgumentException("Email không hợp lệ");
            }

            var user = await _userRepository.FindAsync(u => u.Email == email);

            if (user == null)
            {
                return null;
            }

            var totalBookings = await _bookingRepository.CountAsync(b => b.UserId == user.Id);
            var lastBooking = await _bookingRepository
                .FindAsync(b => b.UserId == user.Id);

            return new CustomerSearchDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.Phone ?? string.Empty,
                Points = (int)Math.Round(user.Score),
                LastBookingDate = lastBooking?.CreatedAt,
                TotalBookings = totalBookings
            };
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private bool IsValidPhoneNumber(string phoneNumber)
        {
            // Regex kiểm tra số điện thoại Việt Nam
            return Regex.IsMatch(phoneNumber, @"^(0[3|5|7|8|9])+([0-9]{8})$");
        }
    }
} 