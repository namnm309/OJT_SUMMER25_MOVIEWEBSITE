using System;
using System.Threading.Tasks;
using ApplicationLayer.DTO.UserManagement;

namespace ApplicationLayer.Services.UserManagement
{
    public interface ICustomerSearchService
    {
        /// <summary>
        /// Tìm kiếm khách hàng theo số điện thoại hoặc email
        /// </summary>
        /// <param name="searchTerm">Số điện thoại hoặc email để tìm kiếm</param>
        /// <returns>Thông tin khách hàng</returns>
        Task<CustomerSearchDto> SearchCustomerAsync(string searchTerm);

        /// <summary>
        /// Tìm kiếm khách hàng theo số điện thoại
        /// </summary>
        /// <param name="phoneNumber">Số điện thoại để tìm kiếm</param>
        /// <returns>Thông tin khách hàng</returns>
        Task<CustomerSearchDto> SearchCustomerByPhoneAsync(string phoneNumber);

        /// <summary>
        /// Tìm kiếm khách hàng theo email
        /// </summary>
        /// <param name="email">Email để tìm kiếm</param>
        /// <returns>Thông tin khách hàng</returns>
        Task<CustomerSearchDto> SearchCustomerByEmailAsync(string email);
    }
} 