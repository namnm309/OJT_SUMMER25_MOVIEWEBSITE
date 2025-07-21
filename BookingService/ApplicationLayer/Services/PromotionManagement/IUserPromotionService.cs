using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace ApplicationLayer.Services.PromotionManagement
{
    public interface IUserPromotionService
    {
        /// <summary>
        /// Get all vouchers of a user
        /// </summary>
        Task<IActionResult> GetUserVouchersAsync(Guid userId);

        /// <summary>
        /// Mark voucher as redeemed
        /// </summary>
        Task<IActionResult> RedeemVoucherAsync(Guid userPromotionId);
    }
} 